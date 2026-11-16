using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorGame
{
    // Test-driver heuristic, never a player recommendation or a combat damage formula.
    // Every candidate uses the same receive/fusion/move rules as the tablet preview.
    internal static class AutomatedLoadoutPolicy
    {
        internal static bool Committed(string style)=>style.StartsWith("committed:",StringComparison.Ordinal);
        static string Preference(string style)=>Committed(style)?style.Substring(10):style;
        internal static bool Allowed(AbilityDeck deck,string style)
        {
            if(!Committed(style))return true;
            style=Preference(style);
            for(int hand=0;hand<2;hand++)
            {
                var d=deck.WeaponFor(deck.Basics[hand]).def;
                // Starter fists remain legal until the requested component drops.
                if(d==LoadoutCatalog.Fists)continue;
                if(style=="gun"&&d.profile?.magazine>0)continue;
                if(style=="cutcarry"&&d.hands==1&&d.weapon==(hand==0?WeaponKind.Sword:WeaponKind.Hammer))continue;
                if(style=="close"&&d.hands==1&&(hand==0?d.weapon==WeaponKind.Fists:d.profile?.magazine>0))continue;
                return false;
            }
            return true;
        }
        internal static int Components(AbilityDeck deck,string style)
        {
            style=Preference(style);
            var a=deck.WeaponFor(deck.Basics[0]).def;var b=deck.WeaponFor(deck.Basics[1]).def;
            if(style=="gun")return a.profile?.magazine>0||b.profile?.magazine>0?1:0;
            if(style=="cutcarry")return (a.hands==1&&a.weapon==WeaponKind.Sword?1:0)+(b.hands==1&&b.weapon==WeaponKind.Hammer?1:0);
            if(style=="close")return a.weapon==WeaponKind.Fists&&b.hands==1&&b.profile?.magazine>0?1:0;
            return 0;
        }
        internal sealed class Choice
        {
            public int offer,index;public PlaceKind place;public LoadoutItem host,partner;
            public float score;public AbilityDeck.Projection projection;
        }
        static string Tags(string style)=>style=="pinball"?"stagger launch collision hammer":style=="storm"?"mark chain storm":style=="sever"?"sever blade gather":style=="gun"?"gun reload pierce":style=="cutcarry"?"sever blade launch hammer":style=="close"?"stagger close gun":style=="siphon"?"siphon sustain":"guard tempo";
        static float Affinity(ItemDefinition d,string style)
        {
            float fit=1+.12f*Math.Min(2,d.tags.Split(' ').Count(t=>t.Length>0&&Tags(style).Split(' ').Contains(t)));
            if(d.kind==ItemKind.Weapon&&(style=="cutcarry"&&(d.id=="sword"||d.id=="breachhammer")||style=="close"&&(d.weapon==WeaponKind.Fists||d.id=="scattergun"||d.id=="pistol")))fit+=.15f;
            return fit;
        }
        static float ContactValue(LoadoutItem item,string style)
        {
            var c=LoadoutCatalog.Action(item);var d=item.def;float power=ItemRarity.Power[item.rank];
            float coverage=d.profile!=null?1+Math.Min(70,d.profile.arc)/100:d.id=="SteelJab"||d.id=="MassDriver"?1:d.id=="ArcBolt"||d.id=="Breaker"||d.id=="ReapingCut"||d.id=="SuppressiveVolley"?2.4f:1.35f;
            if(d.id=="rifle")coverage=1+.5f*Math.Min(3,item.rank);
            if(d.id=="pistol")coverage=1+.5f*Math.Min(2,item.rank-1);
            if(d.id=="hammer"&&item.rank>=3)coverage=2.3f;
            float utility=d.id=="GravityWell"||d.id=="VectorTether"?20:d.id=="Counterguard"?30:d.id=="DrainBrand"?30:d.id=="MassDriver"||d.id=="hammer"||d.id=="breachhammer"?18:d.id=="ConductiveBrand"?3:0;
            if(d.id=="fists"&&item.rank>=2)coverage*=1.4f;
            if(item.augment!=null)utility+=4;
            return (c.damage*coverage+utility*power)*Affinity(d,style);
        }
        public static float Score(AbilityDeck deck,string style)
        {
            bool committed=Committed(style);int components=Components(deck,style);style=Preference(style);
            // Count a locked 2H attack once, paired 1H damage twice with one duration,
            // and mismatched families as two turns. Reload cost is amortized per shot.
            float value=0,seconds=0;var seen=new HashSet<int>();
            for(int slot=0;slot<5;slot++)
            {
                var item=deck.Slots[slot];if(item==null||!seen.Add(slot))continue;
                var action=item.def.kind==ItemKind.Basic?deck.WeaponFor(item):item;
                float duration=LoadoutCatalog.Action(action).duration;
                float Reload(LoadoutItem w)=>w.def.profile?.magazine>0?1.2f/w.def.profile.magazine:0;
                duration+=Reload(action);float contribution=ContactValue(action,style);
                int pair=deck.PairAfter(slot);
                if(pair>=0&&seen.Add(pair))
                {
                    var other=deck.WeaponFor(deck.Slots[pair]);
                    if(!deck.TwoHanded)contribution+=ContactValue(other,style);
                    duration=Math.Max(duration,LoadoutCatalog.Action(other).duration+Reload(other));
                }
                value+=contribution;seconds+=duration;
            }
            float score=value/Math.Max(.4f,seconds)*deck.Damage*deck.Speed;
            // Filling a utility slot matters beyond immediate damage throughput.
            // Duplicate damage cards remain legal but do not replace build identity.
            var abilities=deck.Slots.Where(i=>i?.def.kind==ItemKind.Ability).ToArray();
            score+=abilities.Length*9-(abilities.Length-abilities.Select(i=>i.def.id).Distinct().Count())*3;
            var equipped=BuildSynergies.Equipped(deck).ToArray();
            foreach(var item in equipped)
            {
                if(LoadoutCatalog.IsArmor(item.def.kind)||item.def.kind==ItemKind.Rig)
                    score+=ItemRarity.Power[item.rank]*(3*Affinity(item.def,style)+item.def.stats.health*.09f+item.def.stats.resistance*50);
                if(item.def.id=="reclaimer"||item.def.id=="treads")score+=6*ItemRarity.Power[item.rank];
                if(item.def.kind==ItemKind.Ultimate)score+=ContactValue(item,style)*.12f+(item.def.id=="redline"&&equipped.Any(i=>i.def.profile?.magazine>0)?9:0);
                // Explicit producer/consumer relationships, once per unordered pair.
                foreach(var link in BuildSynergies.Find(deck,item.def,incomingItem:item))
                {
                    if(Array.IndexOf(equipped,item)>=Array.IndexOf(equipped,link.partner)||link.text.StartsWith("Needs"))continue;
                    if((item.def.id=="relay"||link.partner.def.id=="relay")&&!BuildSynergies.HasConductiveSetup(deck))continue;
                    score+=(link.key=="pierce"||link.key=="mark"?5:2.5f)*Math.Min(ItemRarity.Power[item.rank],ItemRarity.Power[link.partner.rank]);
                }
            }
            bool Has(params string[] ids)=>equipped.Any(i=>ids.Contains(i.def.id));
            bool gun=deck.Weapons.Any(w=>w?.def.profile?.magazine>0),fist=deck.Weapons.Any(w=>w==null||w.def.weapon==WeaponKind.Fists);
            // Preserve the intended experiment: seven different build preferences,
            // rather than seven labels for the same generic damage-maximizing bot.
            if(style=="pinball")score+=(Has("MassDriver","hammer")?18:0)+(Has("faultline")&&Has("MassDriver","hammer","breachhammer")?6:0);
            if(style=="storm")score+=(Has("ArcBolt","tempest")?14:0)+(BuildSynergies.HasConductiveSetup(deck)&&Has("ArcBolt","tempest","relay")?12:0);
            if(style=="sever")score+=(Has("ReapingCut","sword","greatsword","siphonblade")?12:0)+(Has("GravityWell")?8:0)+(Has("magnet")?6:0);
            if(style=="gun")score+=(gun?20:0)+(Has("VectorTether")&&Has("rifle","SuppressiveVolley")?12:0)+(Has("SuppressiveVolley")?8:0);
            if(style=="cutcarry")score+=(deck.Weapons.Any(w=>w?.def.weapon==WeaponKind.Sword)?25:0)+(deck.Weapons.Any(w=>w?.def.weapon==WeaponKind.Hammer)?25:0);
            if(style=="close")score+=(gun&&fist?50:0)+(Has("Counterguard")?8:0);
            if(style=="siphon")score+=(Has("DrainBrand")?16:0)+(Has("siphonblade")?12:0)+(Has("reclaimer","harvest")?8:0);
            return score+(committed?components*1000:0);
        }
        static IEnumerable<Choice> Destinations(AbilityDeck deck,ItemDefinition def)
        {
            yield return new Choice{place=PlaceKind.Inventory};
            if(def.kind==ItemKind.Augment){foreach(var host in deck.CompatibleHosts(def))yield return new Choice{place=PlaceKind.Inventory,host=host};yield break;}
            if(def.kind==ItemKind.Ability){for(int i=0;i<5;i++)if(deck.Slots[i]?.def.kind!=ItemKind.Basic)yield return new Choice{place=PlaceKind.Sequence,index=i};}
            if(def.kind==ItemKind.Weapon){for(int i=0;i<2;i++)yield return new Choice{place=PlaceKind.Weapon,index=i};}
            if(LoadoutCatalog.IsArmor(def.kind))yield return new Choice{place=PlaceKind.Helmet+LoadoutCatalog.ArmorIndex(def.kind)};
            if(def.kind==ItemKind.Rig)yield return new Choice{place=PlaceKind.Rig};
            if(def.kind==ItemKind.Ultimate)yield return new Choice{place=PlaceKind.Ultimate};
        }
        internal static Choice Evaluate(AbilityDeck deck,RewardOffer offer,string style,bool opening=false)
        {
            Choice best=null;
            foreach(var c in Destinations(deck,offer.def))
            {
                if(opening&&(c.place!=PlaceKind.Sequence||deck.Slots[c.index]!=null))continue;
                var p=deck.PreviewChange(offer,null,c.place,c.index,c.host);if(!p.valid)continue;
                FillAbilitySlots(p.after,style);Arrange(p.after,style);if(c.place==PlaceKind.Weapon&&!p.after.TwoHanded)ImproveOtherHand(p.after,c.index,style);if(!Allowed(p.after,style))continue;c.projection=p;c.score=Score(p.after,style);
                // A useful spare has small, bounded future value; fusion itself is
                // already reflected in the projected equipped ranks and sockets.
                c.score+=Math.Min(2,p.after.Inventory.Count(i=>BuildSynergies.Equipped(p.after).Any(e=>e.def==i.def&&e.rank==i.rank)))*.15f;
                if(best==null||c.score>best.score+.001f)best=c;
            }
            return best;
        }
        public static Choice Choose(AbilityDeck deck,string style)
        {
            Choice best=null;
            for(int i=0;i<deck.Offers.Length;i++)
            {var c=Evaluate(deck,deck.Offers[i],style,deck.Draft==DraftKind.Ability);if(c==null)continue;c.offer=i;if(best==null||c.score>best.score+.001f)best=c;}
            return best??throw new InvalidOperationException("No legal automated draft choice");
        }
        public static void EquipStored(AbilityDeck deck,string style)
        {
            FillAbilitySlots(deck,style);
            // Revisit fused keepers and displaced weapons, without inventing items.
            for(int pass=0;pass<8;pass++)
            {
                Arrange(deck,style);float score=Score(deck,style);Choice best=null;LoadoutItem source=null;
                foreach(var item in deck.Inventory.Append(deck.Fists).ToArray())foreach(var c in Destinations(deck,item.def))
                {
                    if(c.place==PlaceKind.Inventory&&c.host==null)continue;
                    var p=deck.PreviewChange(null,item,c.place,c.index,c.host);if(!p.valid)continue;Arrange(p.after,style);
                    if(c.place==PlaceKind.Weapon&&!p.after.TwoHanded){var partner=ImproveOtherHand(p.after,c.index,style);c.partner=p.copies.FirstOrDefault(kv=>kv.Value==partner).Key;}
                    if(!Allowed(p.after,style))continue;float candidate=Score(p.after,style);
                    if(candidate<=score+.01f)continue;score=candidate;best=c;source=item;
                }
                if(best==null)break;
                if(best.host!=null)deck.Attach(source,best.host);else deck.Move(source,best.place,best.index);
                if(best.partner!=null)deck.Move(best.partner,PlaceKind.Weapon,1-best.index);
            }
            Arrange(deck,style);
        }
        static void FillAbilitySlots(AbilityDeck deck,string style)
        {
            while(deck.Count<3)
            {
                int slot=Array.FindIndex(deck.Slots,i=>i==null);if(slot<0)return;
                LoadoutItem best=null;float score=float.NegativeInfinity;
                foreach(var item in deck.Inventory.Where(i=>i.def.kind==ItemKind.Ability))
                {
                    var p=deck.PreviewChange(null,item,PlaceKind.Sequence,slot);if(!p.valid)continue;Arrange(p.after,style);float next=Score(p.after,style);
                    if(next>score){best=item;score=next;}
                }
                if(best==null)return;deck.Move(best,PlaceKind.Sequence,slot);
            }
        }
        static LoadoutItem ImproveOtherHand(AbilityDeck deck,int lockedHand,string style)
        {
            // Switching away from 2H may require two coordinated moves. Evaluate
            // the available partner now, rather than rejecting the empty-hand step.
            float score=Score(deck,style);LoadoutItem best=null;
            foreach(var item in deck.Inventory.Append(deck.Fists).Where(i=>i.def.kind==ItemKind.Weapon&&i.def.hands==1).Distinct())
            {
                if(item==deck.Weapons[lockedHand])continue;
                var p=deck.PreviewChange(null,item,PlaceKind.Weapon,1-lockedHand);if(!p.valid||!Allowed(p.after,style))continue;Arrange(p.after,style);float next=Score(p.after,style);
                if(next<=score+.01f)continue;score=next;best=item;
            }
            if(best!=null)deck.Move(best,PlaceKind.Weapon,1-lockedHand);return best;
        }
        public static void Arrange(AbilityDeck deck,string style)
        {
            int Priority(LoadoutItem i)=>i.def.id=="DrainBrand"||i.def.id=="ConductiveBrand"||i.def.id=="GravityWell"||i.def.id=="VectorTether"?0:i.def.id=="Counterguard"||i.def.id=="SteelJab"||i.def.id=="Breaker"||i.def.id=="SuppressiveVolley"?1:i.def.kind==ItemKind.Basic?2:3;
            var order=deck.Slots.Where(i=>i!=null).OrderBy(Priority).ToArray();
            if(deck.TwoHanded)
            {
                var skills=order.Where(i=>i.def.kind!=ItemKind.Basic).ToArray();deck.Move(deck.Basics[0],PlaceKind.Sequence,Math.Min(3,skills.Count(i=>Priority(i)<2)));
                var positions=Enumerable.Range(0,5).Where(i=>deck.Slots[i]?.def.kind!=ItemKind.Basic).ToArray();for(int i=0;i<skills.Length;i++)deck.Move(skills[i],PlaceKind.Sequence,positions[i]);
            }
            else for(int i=0;i<order.Length;i++)deck.Move(order[i],PlaceKind.Sequence,i);
        }
    }
}
