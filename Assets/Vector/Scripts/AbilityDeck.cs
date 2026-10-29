using System;
using System.Collections.Generic;

namespace VectorGame
{
    public enum AbilityId { SteelJab, Breaker, ForcePush, MassDriver, GravityWell, ArcBolt }
    public enum ModifierId { FaultLine, Overclock, HeavyHands, Recovery }
    public sealed class AbilityCard
    {
        public readonly AbilityId id;
        public readonly string name, description, interaction;
        public readonly float duration, range, damage;
        public readonly int pose;
        public AbilityCard(AbilityId id,string name,string description,string interaction,float duration,float range,float damage,int pose)
        {this.id=id;this.name=name;this.description=description;this.interaction=interaction;this.duration=duration;this.range=range;this.damage=damage;this.pose=pose;}
    }
    public sealed class ModifierCard
    {
        public readonly ModifierId id;public readonly string name,description;
        public ModifierCard(ModifierId id,string name,string description){this.id=id;this.name=name;this.description=description;}
    }
    public sealed class AbilityDeck
    {
        public static readonly AbilityCard[] Abilities={
            new AbilityCard(AbilityId.SteelJab,"Steel Jab","A fast, precise punch. 24 damage to the nearest threat.","Quick recovery gets you to the next slot sooner.",.40f,2.6f,24,0),
            new AbilityCard(AbilityId.Breaker,"Breaker","A sweeping palm strike. 34 damage and stagger in a wide melee arc.","Staggered targets take extra damage from Arc Bolt.",.68f,3.2f,34,2),
            new AbilityCard(AbilityId.ForcePush,"Force Push","A radial field deals 20 damage and physically throws nearby enemies back.","Follows Gravity Well with a burst through the gathered crowd.",.95f,5.3f,20,-1),
            new AbilityCard(AbilityId.MassDriver,"Mass Driver","Strike for 28 damage, then launch one body. Its first two physical contacts deal 18 damage.","Killing blows still launch ragdolls. Fault Line later amplifies collisions.",.82f,3.5f,28,2),
            new AbilityCard(AbilityId.GravityWell,"Gravity Well","Deal 16 damage and pull nearby enemies into a point ahead of you.","Place before Breaker, Force Push, or Mass Driver to gather their targets.",.80f,6.5f,16,-1),
            new AbilityCard(AbilityId.ArcBolt,"Arc Bolt","Deal 28 damage at range; chain to two nearby enemies for 18 damage each.","Staggered targets take +12 damage. Put Breaker ahead of it.",.65f,9f,28,1)
        };
        public static readonly ModifierCard[] Modifiers={
            new ModifierCard(ModifierId.FaultLine,"Fault Line","Launched bodies deal +14 collision damage and stagger each target for one second."),
            new ModifierCard(ModifierId.Overclock,"Overclock","Execute every occupied slot 20% faster. The order stays exactly as you placed it."),
            new ModifierCard(ModifierId.HeavyHands,"Amplifier","All slotted abilities deal 25% more direct damage."),
            new ModifierCard(ModifierId.Recovery,"Repair Weave","Restore 65 integrity immediately, up to the maximum.")
        };
        readonly Random random;
        public AbilityCard[] Slots {get;}=new AbilityCard[3];
        public AbilityCard[] Offers {get;private set;}=Array.Empty<AbilityCard>();
        public ModifierCard[] ModifierOffers {get;private set;}=Array.Empty<ModifierCard>();
        public readonly List<ModifierId> Upgrades=new List<ModifierId>();
        public bool IsAbilityDraft {get;private set;}
        public int Selected {get;private set;}=-1;
        public int SelectedSlot {get;private set;}=-1;
        public int Rerolls {get;private set;}
        public int Spent {get;private set;}
        public int Count {get {int count=0;foreach(var slot in Slots)if(slot!=null)count++;return count;}}
        public bool Pinball=>Upgrades.Contains(ModifierId.FaultLine);
        public float Speed=>Upgrades.Contains(ModifierId.Overclock)?1.2f:1;
        public float Damage=>Upgrades.Contains(ModifierId.HeavyHands)?1.25f:1;
        public bool CanInstall=>Selected>=0&&(!IsAbilityDraft||(SelectedSlot>=0&&Slots[SelectedSlot]==null));
        public AbilityDeck(int seed){random=new Random(seed^0x5f3759df);}
        public bool Has(AbilityId id){foreach(var slot in Slots)if(slot!=null&&slot.id==id)return true;return false;}
        public int NextOccupied(int from)
        {for(int i=0;i<3;i++){int slot=(from+i)%3;if(Slots[slot]!=null)return slot;}return -1;}
        public void Open(bool ability,bool grantReroll)
        {IsAbilityDraft=ability;if(grantReroll)Rerolls++;Roll();}
        void Shuffle<T>(List<T> list)
        {for(int i=list.Count-1;i>0;i--){int j=random.Next(i+1);T temp=list[i];list[i]=list[j];list[j]=temp;}}
        void Roll()
        {
            Selected=SelectedSlot=-1;
            if(IsAbilityDraft)
            {
                var choices=new List<AbilityCard>();foreach(var ability in Abilities)if(!Has(ability.id))choices.Add(ability);
                Shuffle(choices);Offers=choices.GetRange(0,Math.Min(3,choices.Count)).ToArray();ModifierOffers=Array.Empty<ModifierCard>();
            }
            else
            {
                var choices=new List<ModifierCard>();foreach(var modifier in Modifiers)
                    if(!Upgrades.Contains(modifier.id)&&(modifier.id!=ModifierId.FaultLine||Has(AbilityId.MassDriver)))choices.Add(modifier);
                Shuffle(choices);ModifierOffers=choices.GetRange(0,Math.Min(3,choices.Count)).ToArray();Offers=Array.Empty<AbilityCard>();
            }
        }
        public void Select(int index)
        {if(index>=0&&index<(IsAbilityDraft?Offers.Length:ModifierOffers.Length))Selected=index;}
        public void Assign(int slot)
        {if(IsAbilityDraft&&slot>=0&&slot<3&&Slots[slot]==null)SelectedSlot=slot;}
        public void Swap(int a,int b)
        {if(a<0||a>2||b<0||b>2)return;var temp=Slots[a];Slots[a]=Slots[b];Slots[b]=temp;SelectedSlot=-1;}
        public bool Reroll(){if(Rerolls<=0)return false;Rerolls--;Spent++;Roll();return true;}
        public string Install()
        {
            if(!CanInstall)return null;string name;
            if(IsAbilityDraft){Slots[SelectedSlot]=Offers[Selected];name=Offers[Selected].name;}
            else{var card=ModifierOffers[Selected];Upgrades.Add(card.id);name=card.name;}
            Selected=SelectedSlot=-1;Offers=Array.Empty<AbilityCard>();ModifierOffers=Array.Empty<ModifierCard>();return name;
        }
        public string Loadout
        {get {var names=new string[3];for(int i=0;i<3;i++)names[i]=(i+1)+": "+(Slots[i]?.name??"Empty");return string.Join("  >  ",names);}}
    }
}
