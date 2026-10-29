using System;
using System.Linq;

namespace VectorGame
{
    public static class LoadoutChecks
    {
        static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException("Loadout contract: "+message);}
        public static void Run()
        {
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
            var score=new ComboScore();score.Kill(100,1);score.Kill(100,1);Require(score.Multiplier==2&&score.Pending==400,"bulk kill multiplier");
            score.Kill(100,2,2);Require(score.Multiplier==4&&score.Pending==1200,"chain depth raises multiplier");
            score.Step(2.9f);Require(score.Banked==0,"combo stays live for window");score.Step(.2f);
            Require(score.Banked==1200&&score.Pending==0&&score.Best==1200,"combo banks once after timeout");
            for(int i=0;i<20;i++)score.Kill(100,10,9);Require(score.Multiplier==10,"multiplier cap");score.Bank();Require(score.Banked==21200,"checkpoint banking");
        }
    }
}
