using System.Collections.Generic;
using System.Linq;

namespace VectorGame
{
    // Explicit producer/consumer rules. Sharing a theme tag alone is not a combo.
    public static class BuildSynergies
    {
        public sealed class Link
        {
            public LoadoutItem partner;
            public string key,text;
        }
        static bool Is(ItemDefinition d,params string[] ids)=>ids.Contains(d.id);
        static bool Gun(ItemDefinition d)=>(d.profile?.magazine??0)>0;
        static bool Stagger(ItemDefinition d)=>Is(d,"SteelJab","Breaker","SuppressiveVolley","fists","breachhammer");
        static bool Lightning(ItemDefinition d)=>Is(d,"ArcBolt","tempest");
        static bool Launch(ItemDefinition d)=>Is(d,"MassDriver","hammer","singularity");
        static bool Sever(ItemDefinition d)=>Is(d,"sword","greatsword","siphonblade","ReapingCut","cyclone");
        static bool Mark(ItemDefinition d)=>Is(d,"ConductiveBrand","wraps","capacitor","arcstep","grounding");
        public static bool HasConductiveSetup(AbilityDeck deck)
        {
            var items=Equipped(deck).Select(i=>i.def).ToArray();
            return items.Any(d=>Is(d,"ConductiveBrand","wraps","arcstep"))||
                items.Any(d=>Is(d,"capacitor"))&&items.Any(Stagger)||
                items.Any(d=>Is(d,"grounding"))&&items.Any(d=>Lightning(d)||Launch(d));
        }
        static string ModifiedPair(AbilityDeck deck,ItemDefinition source,LoadoutItem sourceItem,ItemDefinition consumer,LoadoutItem consumerItem,out string key)
        {
            key="chain";
            if(sourceItem?.HasAugment(AugmentId.FollowThrough)==true&&consumer.kind==ItemKind.Weapon)
            {key=source.ability.id==AbilityId.DrainBrand?"siphon":source.ability.id==AbilityId.ConductiveBrand?"mark":"stagger";return "Prepared utility is delivered by the next weapon contact.";}
            if(consumerItem?.HasAugment(AugmentId.Livewire)==true&&Mark(source)&&HasConductiveSetup(deck))
            {key="mark";return "Conductive marks -> one weapon discharge per action.";}
            if(sourceItem?.HasAugment(AugmentId.Livewire)==true&&Is(consumer,"grounding"))
            {key="guard";return HasConductiveSetup(deck)?"Weapon discharges can build the barrier.":"Needs another mark source before the discharge can build a barrier.";}
            if(sourceItem?.HasAugment(AugmentId.Afterfield)==true&&Is(consumer,"surveyor")&&Is(source,"ArcBolt","ReapingCut"))
                return "Later field hits can complete the one multi-target echo.";
            if(Is(source,"GravityWell","VectorTether")&&Is(consumer,"treads"))
            {key="gather";return source.id=="GravityWell"?"Pull repair motes toward the gathering point.":"Pull repair motes onto the walking corridor.";}
            return null;
        }
        static string Pair(ItemDefinition source,ItemDefinition consumer,out string key)
        {
            key="chain";
            if(Gun(source)&&Is(consumer,"recoil")){key="reload";return "Reload → guard → stronger gun hit.";}
            if(Gun(source)&&Is(consumer,"duelist")){key="guard";return "Reload arms the three-hit ward.";}
            if(Gun(source)&&Is(consumer,"loader")){key="reload";return "Faster reloads; refills stagger nearby foes.";}
            if(Gun(source)&&Is(consumer,"redline")){key="reload";return "Refill spent ammo; speed the next six turns.";}
            if(Stagger(source)&&Lightning(consumer)){key="stagger";return "Staggered foes take bonus lightning damage.";}
            if(Stagger(source)&&Is(consumer,"visor","capacitor")){key="stagger";return consumer.id=="visor"?"Hits spread existing stagger to a neighbor.":"Hit staggered foes to apply Conductive.";}
            if(Mark(source)&&Lightning(consumer)){key="mark";return "Conductive marks add discharge damage.";}
            if(Mark(source)&&Is(consumer,"relay")){key="mark";return "Marked foes discharge when a body hits them.";}
            if(Launch(source)&&Is(consumer,"faultline","grounding","relay","momentum","salvager"))
            {key="collision";return consumer.id=="relay"?"Body hits discharge foes that are Conductive.":consumer.id=="grounding"?"Body impacts build a barrier.":consumer.id=="momentum"?"A launch speeds the next ability.":consumer.id=="salvager"?"A launch primes a later corpse pull.":"Body impacts release stagger waves.";}
            if(Lightning(source)&&Is(consumer,"grounding")){key="guard";return "Lightning hits build a barrier.";}
            if(Sever(source)&&Is(consumer,"magnet","salvager","duelist")){key="sever";return consumer.id=="magnet"?"Severed bodies launch into collision chains.":consumer.id=="duelist"?"A sever arms the three-hit ward.":"A sever primes a later corpse pull.";}
            if(Is(source,"GravityWell")&&Is(consumer,"MassDriver","Breaker","ReapingCut","sword","greatsword","hammer","scattergun","SuppressiveVolley","singularity","cyclone")){key="gather";return "Gather foes before the follow-up attack.";}
            if(Is(source,"VectorTether")&&(Is(consumer,"rifle","SuppressiveVolley")||consumer.kind==ItemKind.Weapon&&consumer.weapon==WeaponKind.Rifle)){key="pierce";return "Line foes up for the follow-up volley or shot.";}
            if(Is(source,"siphonblade","DrainBrand","reclaimer","harvest","treads")&&Is(consumer,"predator")){key="siphon";return "Actual healing speeds the next ability.";}
            if(Is(source,"Counterguard","amplifier")&&consumer.kind==ItemKind.Weapon){key="burst";return source.id=="Counterguard"?"Absorbed damage charges the next weapon hit.":"Abilities bank damage for a weapon burst.";}
            return null;
        }
        public static IEnumerable<LoadoutItem> Equipped(AbilityDeck deck)=>deck.Slots.Where(i=>i!=null).Select(i=>i.def.kind==ItemKind.Basic?deck.WeaponFor(i):i).Concat(deck.Armor).Concat(new[]{deck.Rig,deck.Ultimate}).Where(i=>i!=null).Distinct();
        public static List<Link> Find(AbilityDeck deck,ItemDefinition incoming,LoadoutItem displaced=null,LoadoutItem incomingItem=null)
        {
            var result=new List<Link>();if(incoming==null)return result;
            foreach(var item in Equipped(deck))
            {
                var partner=item.def;
                if(item==displaced||partner==incoming)continue;
                // Mutually exclusive equipment cannot be a partner after this choice.
                if((LoadoutCatalog.IsArmor(incoming.kind)||incoming.kind==ItemKind.Rig||incoming.kind==ItemKind.Ultimate)&&partner.kind==incoming.kind)continue;
                if(incoming.kind==ItemKind.Weapon&&partner.kind==ItemKind.Weapon&&(incoming.hands==2||partner.hands==2))continue;
                if(incoming.kind==ItemKind.Augment)
                {if(incoming.module.Fits(partner))result.Add(new Link{partner=item,key=incoming.module.id==AugmentId.Livewire?"mark":"burst",text=incoming.module.id==AugmentId.Livewire?(HasConductiveSetup(deck)?"Weapon hits can discharge your Conductive marks.":"Needs a Conductive source; this weapon alone cannot supply marks."):"Attach to modify this item's effect."});continue;}
                string text=ModifiedPair(deck,incoming,incomingItem,partner,item,out string key)??ModifiedPair(deck,partner,item,incoming,incomingItem,out key)??Pair(incoming,partner,out key)??Pair(partner,incoming,out key);
                if(text==null)continue;
                if(partner.id=="Counterguard"&&item.HasAugment(AugmentId.Discharge)||incoming.id=="Counterguard"&&incomingItem?.HasAugment(AugmentId.Discharge)==true)continue;
                result.Add(new Link{partner=item,key=key,text=text});
            }
            return result;
        }
    }
}
