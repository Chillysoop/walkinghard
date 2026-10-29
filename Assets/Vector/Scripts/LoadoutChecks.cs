using System;
using System.Linq;

namespace VectorGame
{
    public static class LoadoutChecks
    {
        static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException("Loadout contract: "+message);}
        public static void Run()
        {
            BuildVarietyChecks.Run();
            var deck=new AbilityDeck(1741);
            Require(deck.Slots.Count(x=>x?.def.kind==ItemKind.Basic)==2,"two permanent basic actions");
            Require(deck.PairAfter(0)==1,"adjacent empty mounts pair fists");
            Require(!deck.Move(deck.Basics[0],PlaceKind.Inventory),"basic cannot disappear into storage");
            deck.Move(deck.Basics[0],PlaceKind.Sequence,4);
            Require(deck.PairAfter(4)==-1,"a gap is not adjacency");
            deck.Move(deck.Basics[1],PlaceKind.Sequence,0);
            Require(deck.PairAfter(4)==0,"repeat-boundary adjacency");
            var hammer=deck.Acquire(LoadoutCatalog.Weapons[1]);deck.Move(hammer,PlaceKind.Weapon,1);
            Require(deck.TwoHanded&&deck.Weapons[0]==hammer&&deck.Weapons[1]==hammer,"2H consumes both mounts");
            int first=Array.IndexOf(deck.Slots,deck.Basics[0]);
            Require(Array.IndexOf(deck.Slots,deck.Basics[1])==first+1,"2H cards lock in adjacent order");
            deck.Move(deck.Basics[1],PlaceKind.Sequence,4);
            Require(deck.Slots[3]==deck.Basics[0]&&deck.Slots[4]==deck.Basics[1],"dragging either card moves the whole 2H block");
            var skill=deck.Acquire(LoadoutCatalog.Abilities[0]);
            Require(!deck.Move(skill,PlaceKind.Sequence,3),"an ability cannot split a 2H block");
            deck.Move(skill,PlaceKind.Sequence,0);
            var sword=deck.Acquire(LoadoutCatalog.Weapons[0]);deck.Move(sword,PlaceKind.Weapon,1);
            Require(!deck.TwoHanded&&deck.Weapons[0]==null&&deck.Weapons[1]==sword&&deck.Inventory.Contains(hammer),"1H replacement stores 2H and frees other mount");
            var sword2=deck.Acquire(LoadoutCatalog.Weapons[0]);deck.Move(sword2,PlaceKind.Weapon,0);
            Require(deck.PairAfter(3)==4,"matching one-handed weapons pair");
            deck.Acquire(LoadoutCatalog.Weapons[0]);
            Require(deck.Weapons[0]==sword2&&sword2.rank==2&&deck.Weapons[1]==null,"three copies preserve earliest equipped weapon and consume exactly three");
            deck.Move(skill,PlaceKind.Inventory);Require(deck.Inventory.Contains(skill),"abilities store");
            deck.Move(skill,PlaceKind.Sequence,3);
            Require(deck.Slots[3]==skill&&deck.Slots.Count(x=>x?.def.kind==ItemKind.Basic)==2,"stored ability can occupy a basic position without losing basics");
            var fists=new AbilityDeck(2);for(int i=0;i<8;i++)fists.Acquire(LoadoutCatalog.Fists);
            Require(fists.Fists.rank==3&&fists.Copies(LoadoutCatalog.Fists,1)==0&&fists.Copies(LoadoutCatalog.Fists,2)==0,"nine common copies reach Rare");
            for(int i=0;i<72;i++)fists.Acquire(LoadoutCatalog.Fists);
            Require(fists.Fists.rank==5&&fists.Items.Count(x=>x.def==LoadoutCatalog.Fists)==1,"81 commons reach Legendary");
            fists.Acquire(LoadoutCatalog.Fists,5);fists.Acquire(LoadoutCatalog.Fists,5);
            Require(fists.Copies(LoadoutCatalog.Fists,5)==3,"Legendary is the cap");
            var rarityCounts=new int[6];var random=new Random(12345);for(int i=0;i<100000;i++)rarityCounts[ItemRarity.Roll(random)]++;
            for(int i=1;i<=5;i++)Require(Math.Abs(rarityCounts[i]/1000f-ItemRarity.Odds[i-1])<1,"weighted rarity odds");
            var rareFists=fists.Acquire(LoadoutCatalog.Fists,3);Require(fists.Move(rareFists,PlaceKind.Weapon,0)&&fists.WeaponFor(fists.Basics[0])==rareFists,"rolled fist rarities can be equipped");
            var ranks=new AbilityDeck(3);ranks.Acquire(LoadoutCatalog.Weapons[0],1);ranks.Acquire(LoadoutCatalog.Weapons[0],2);ranks.Acquire(LoadoutCatalog.Weapons[0],2);
            Require(ranks.Fusions==0,"different ranks do not fuse");
            var ult=ranks.Acquire(LoadoutCatalog.Ultimates[1]);var old=ranks.Ultimate;ranks.Move(ult,PlaceKind.Ultimate);
            Require(ranks.Ultimate==ult&&ranks.Inventory.Contains(old)&&!ranks.Move(ult,PlaceKind.Sequence,4),"ultimate has a separate compatible slot");
            ranks.Open(DraftKind.Ability,true);Require(ranks.Offers.Length==3&&ranks.Offers.Select(o=>o.def.id).Distinct().Count()==3,"three distinct ability offers");
            ranks.Select(0);Require(!ranks.Claim(),"opening ability requires position");ranks.Assign(2);Require(ranks.Claim()&&!ranks.Claim(),"reward can only be claimed once");
            int count=ranks.Items.Count;ranks.Reroll();Require(ranks.Items.Count==count,"reroll cannot duplicate a claimed reward");
            var quick=new AbilityDeck(43);quick.Open(DraftKind.Ability,true);int before=quick.Items.Count;
            Require(!quick.ClaimTo(0,PlaceKind.Weapon,0)&&quick.Items.Count==before&&!quick.RewardClaimed,"invalid reward drop is atomic");
            Require(quick.ClaimTo(0,PlaceKind.Sequence,4)&&quick.Slots[4]?.def==quick.Offers[0].def,"drop equips ability directly");
            Require(!quick.ClaimTo(1,PlaceKind.Sequence,2),"fast reward cannot be claimed twice");
            quick.Open(DraftKind.Weapon,false);var offered=quick.Offers[0];
            Require(quick.ClaimTo(0,PlaceKind.Weapon,1)&&quick.Weapons[1]?.def==offered.def,"weapon rewards equip directly");
            quick.Open(DraftKind.Mixed,false);
            var bundleDef=LoadoutCatalog.Abilities[0];var equipped=quick.Acquire(bundleDef);quick.Move(equipped,PlaceKind.Sequence,2);quick.Acquire(bundleDef);
            quick.Offers[0]=new RewardOffer(bundleDef,2,1);
            Require(quick.ClaimTo(0,PlaceKind.Sequence,2)&&quick.Slots[2]==equipped&&equipped.rank==2&&quick.Copies(bundleDef,1)==1,"bundle equips the fusion result, not its leftover spare");
            quick.Open(DraftKind.Mixed,false);quick.Acquire(LoadoutCatalog.Fists);quick.Offers[0]=new RewardOffer(LoadoutCatalog.Fists);
            Require(quick.ClaimTo(0,PlaceKind.Weapon,0)&&quick.Weapons[0]==null&&quick.Fists.rank==2,"starter-fist fusion equips without losing the reward");
            quick.SuggestedPlace(bundleDef,out var place,out int destination);
            Require(place==PlaceKind.Sequence&&destination==2,"double-click upgrades the existing action");
            var full=new AbilityDeck(2);for(int i=0;i<3;i++)full.Move(full.Acquire(LoadoutCatalog.Abilities[i]),PlaceKind.Sequence,i+2);
            full.SuggestedPlace(LoadoutCatalog.Abilities[4],out place,out destination);
            Require(place==PlaceKind.Inventory,"auto-equip stores an unrelated ability when the loop is full");
            var armor=new AbilityDeck(8);var helmet=armor.Acquire(LoadoutCatalog.Armor[0]);
            Require(!armor.Move(helmet,PlaceKind.Weapon)&&!armor.Move(helmet,PlaceKind.Boots)&&armor.Move(helmet,PlaceKind.Helmet),"armor has dedicated compatible mounts");
            Require(armor.Damage>1&&armor.StaggerBonus==6&&!armor.Inventory.Contains(helmet),"equipped helmet grants stats and passive");
            armor.Acquire(helmet.def);armor.Acquire(helmet.def);
            Require(armor.Armor[0]==helmet&&helmet.rank==2&&Math.Abs(armor.StaggerBonus-7.2f)<.001f,"armor fusion preserves mount and scales passive");
            armor.Open(DraftKind.Mixed,false);armor.Offers[0]=new RewardOffer(LoadoutCatalog.Armor[1],1,3);
            Require(!armor.ClaimTo(0,PlaceKind.Helmet)&&armor.ClaimTo(0,PlaceKind.Armor)&&armor.MaxHealthBonus==60&&Math.Abs(armor.DamageTaken-.88f)<.001f,"direct armor reward applies rarity-scaled defense");
            armor.Move(armor.Acquire(LoadoutCatalog.Armor[2]),PlaceKind.Boots);
            Require(armor.Speed>1&&armor.Regeneration==0,"boots grant speed; recovery requires real mote pickup");
            armor.Move(helmet,PlaceKind.Inventory);Require(armor.StaggerBonus==0&&armor.Damage==1,"stored armor stops granting effects");
            var score=new ComboScore();score.Kill(100,1);score.Kill(100,1);Require(score.Multiplier==2&&score.Pending==400,"bulk kill multiplier");
            score.Kill(100,2,2);Require(score.Multiplier==4&&score.Pending==1200,"chain depth raises multiplier");
            score.Step(2.9f);Require(score.Banked==0,"combo stays live for window");score.Step(.2f);
            Require(score.Banked==1200&&score.Pending==0&&score.Best==1200,"combo banks once after timeout");
            for(int i=0;i<20;i++)score.Kill(100,10,9);Require(score.Multiplier==10,"multiplier cap");score.Bank();Require(score.Banked==21200,"checkpoint banking");
        }
    }
}
