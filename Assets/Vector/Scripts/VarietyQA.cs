using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        bool varietyFixture;
        void ConfigureVarietyFixture()
        {
            if(varietyFixture)return;
            bool moduleCase=qaBuild.StartsWith("aug-");bool comparison=qaBuild.StartsWith("compare-");
            if(!moduleCase&&!comparison&&!new[]{"pinball","pinball-separated","freight","freight-piledriver","ballet","funnel","funnel-breach","cutcarry","close","full"}.Contains(qaBuild))return;
            varietyFixture=true;var rank=moduleCase?1:3;
            LoadoutItem Equip(string id,PlaceKind place,int index=0){var item=Deck.Acquire(LoadoutCatalog.Find(id),rank);MoveItem(item,place,index);return item;}
            void Module(LoadoutItem host,AugmentId id){Deck.Attach(Deck.Acquire(AugmentCatalog.Get(id)),host);}
            void Abilities(params AbilityId[] ids){for(int i=0;i<3;i++)Equip(ids[i].ToString(),PlaceKind.Sequence,i+2);}
            MoveItem(Deck.Basics[0],PlaceKind.Sequence,0);MoveItem(Deck.Basics[1],PlaceKind.Sequence,1);
            MoveItem(Deck.Fists,PlaceKind.Weapon,0);MoveItem(Deck.Fists,PlaceKind.Weapon,1);
            Abilities(AbilityId.GravityWell,AbilityId.MassDriver,AbilityId.ArcBolt);
            if(moduleCase)
            {
                var id=(AugmentId)Enum.Parse(typeof(AugmentId),qaBuild.Substring(4));var def=AugmentCatalog.Get(id);var hostDef=LoadoutCatalog.All.First(def.module.Fits);
                var host=Equip(hostDef.id,hostDef.kind==ItemKind.Weapon?PlaceKind.Weapon:PlaceKind.Sequence,hostDef.kind==ItemKind.Weapon?0:2);Module(host,id);
                if(host.def.kind==ItemKind.Weapon&&host.def.hands!=2)MoveItem(Deck.Basics[1],PlaceKind.Sequence,4);
                if(id==AugmentId.BreachLoad)ammunition[host]=0;
                Equip("loader",PlaceKind.Boots);Equip("reactive",PlaceKind.Armor);
            }
            if(comparison)
            {
                var bits=qaBuild.Split('-');string type=bits[1];bool two=bits.Contains("2h"),separate=bits.Contains("separate"),single=bits.Contains("single");
                string weapon=type=="fists"?"fists":type=="sword"?(two?"greatsword":"sword"):type=="hammer"?(two?"hammer":"breachhammer"):two?"rifle":"pistol";
                AugmentId first=type=="fists"?AugmentId.Flurry:type=="sword"?AugmentId.Crosscut:type=="hammer"?AugmentId.Groundbreaker:AugmentId.BoreRounds;
                AugmentId second=bits.Contains("same")?first:type=="fists"?AugmentId.PalmDrive:type=="sword"?AugmentId.HookedEdge:type=="hammer"?AugmentId.Piledriver:AugmentId.BreachLoad;
                if(bits.Contains("shared")){Deck.Fists.rank=rank;Module(Deck.Fists,first);}
                else {var one=Equip(weapon,PlaceKind.Weapon,0);Module(one,first);if(!two){var other=Equip(weapon,PlaceKind.Weapon,1);if(!single)Module(other,second);}}
                if(separate)MoveItem(Deck.Basics[1],PlaceKind.Sequence,4);
            }
            if(qaBuild.StartsWith("pinball")){Abilities(AbilityId.SteelJab,AbilityId.MassDriver,AbilityId.Breaker);Module(Deck.Slots[2],AugmentId.FollowThrough);Equip("faultline",PlaceKind.Rig);if(qaBuild.EndsWith("separated"))MoveItem(Deck.Basics[1],PlaceKind.Sequence,4);}
            if(qaBuild.StartsWith("freight")){Equip("wraps",PlaceKind.Weapon,0);var hammer=Equip("breachhammer",PlaceKind.Weapon,1);Module(hammer,qaBuild.EndsWith("piledriver")?AugmentId.Piledriver:AugmentId.Groundbreaker);Abilities(AbilityId.ConductiveBrand,AbilityId.MassDriver,AbilityId.ArcBolt);Equip("relay",PlaceKind.Rig);Equip("grounding",PlaceKind.Armor);Equip("arcstep",PlaceKind.Boots);Equip("capacitor",PlaceKind.Helmet);}
            if(qaBuild=="ballet"){var sword=Equip("sword",PlaceKind.Weapon,0);Module(sword,AugmentId.Crosscut);Abilities(AbilityId.GravityWell,AbilityId.ReapingCut,AbilityId.ForcePush);Equip("magnet",PlaceKind.Rig);Equip("salvager",PlaceKind.Helmet);Equip("duelist",PlaceKind.Armor);Equip("cyclone",PlaceKind.Ultimate);}
            if(qaBuild.StartsWith("funnel")){var rifle=Equip("rifle",PlaceKind.Weapon);Module(rifle,qaBuild.EndsWith("breach")?AugmentId.BreachLoad:AugmentId.BoreRounds);Abilities(AbilityId.VectorTether,AbilityId.ConductiveBrand,AbilityId.ArcBolt);Equip("loader",PlaceKind.Boots);Equip("surveyor",PlaceKind.Helmet);Equip("amplifier",PlaceKind.Rig);Equip("redline",PlaceKind.Ultimate);}
            if(qaBuild=="cutcarry"){var sword=Equip("sword",PlaceKind.Weapon);Module(sword,AugmentId.HookedEdge);Equip("breachhammer",PlaceKind.Weapon,1);Abilities(AbilityId.ReapingCut,AbilityId.Counterguard,AbilityId.MassDriver);Equip("momentum",PlaceKind.Boots);}
            if(qaBuild=="close"){Module(Deck.Fists,AugmentId.PalmDrive);var gun=Equip("scattergun",PlaceKind.Weapon,1);Module(gun,AugmentId.BreachLoad);Abilities(AbilityId.Counterguard,AbilityId.Breaker,AbilityId.ForcePush);Equip("loader",PlaceKind.Boots);Equip("duelist",PlaceKind.Armor);}
            if(qaBuild=="full"){var hammer=Equip("hammer",PlaceKind.Weapon);Module(hammer,AugmentId.Piledriver);Abilities(AbilityId.ConductiveBrand,AbilityId.VectorTether,AbilityId.ArcBolt);Equip("relay",PlaceKind.Rig);Equip("grounding",PlaceKind.Armor);Equip("treads",PlaceKind.Boots);Equip("capacitor",PlaceKind.Helmet);Equip("tempest",PlaceKind.Ultimate);}
        }
        [Serializable] public sealed class ContributionReport {public string item,module;public bool equipped;public int rarity,kills;public Contribution values;}
        ContributionReport[] ContributionReports()=>Deck.Items.Where(i=>i.def.kind!=ItemKind.Basic&&i.def.kind!=ItemKind.Augment).Select(i=>new ContributionReport{item=i.def.id,equipped=Deck.Slots.Contains(i)||Deck.Weapons.Contains(i)||Deck.Armor.Contains(i)||Deck.Rig==i||Deck.Ultimate==i||i==Deck.Fists&&Deck.Weapons.Any(w=>w==null),module=i.augment?.def.id??"",rarity=i.rank,kills=KillsFor(i),values=ContributionFor(i)}).ToArray();
    }
}
