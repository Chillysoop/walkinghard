using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorGame
{
    public enum AbilityId { SteelJab, Breaker, ForcePush, MassDriver, GravityWell, ArcBolt, BasicAttack, Ultimate, ConductiveBrand, VectorTether, ReapingCut, Counterguard, DrainBrand }
    public enum ItemKind { Basic, Ability, Weapon, Rig, Ultimate, Helmet, Armor, Boots, Augment }
    public enum WeaponKind { Fists, Sword, Hammer, Pistol, Rifle, Shotgun }
    public enum DraftKind { Ability, Weapon, Mixed, Equipment, Augment, Ultimate }
    public enum PlaceKind { Sequence, Weapon, Rig, Ultimate, Inventory, Helmet, Armor, Boots }
    public sealed class AbilityCard
    {
        public readonly AbilityId id;
        public readonly string name, description, interaction;
        public readonly float duration, range, damage;
        public readonly int pose;
        public AbilityCard(AbilityId id,string name,string description,string interaction,float duration,float range,float damage,int pose)
        {this.id=id;this.name=name;this.description=description;this.interaction=interaction;this.duration=duration;this.range=range;this.damage=damage;this.pose=pose;}
    }
    public sealed class ItemDefinition
    {
        public readonly string id,name,description,upgrade;
        public readonly ItemKind kind;
        public readonly WeaponKind weapon;
        public readonly int hands;
        public WeaponProfile profile;
        public Signature signature;
        public EffectProfile effect;
        public GearStats stats;
        public AugmentDefinition module;
        public string tags="",tradeoff="";
        public readonly AbilityCard ability;
        public ItemDefinition(string id,string name,ItemKind kind,string description,string upgrade="",WeaponKind weapon=WeaponKind.Fists,int hands=0,AbilityCard ability=null)
        {this.id=id;this.name=name;this.kind=kind;this.description=description;this.upgrade=upgrade;this.weapon=weapon;this.hands=hands;this.ability=ability;}
    }
    public sealed class LoadoutItem
    {
        public readonly ItemDefinition def;
        public int rank=1, mount=-1;
        public LoadoutItem augment;
        public bool Augmentable=>def.kind==ItemKind.Ability||def.kind==ItemKind.Weapon;
        public bool HasAugment(AugmentId id)=>augment?.def.module?.id==id;
        public LoadoutItem(ItemDefinition def,int rank=1){this.def=def;this.rank=rank;}
        public string Name=>def.name+(def.kind==ItemKind.Basic||def.kind==ItemKind.Augment?"":" · "+ItemRarity.Names[rank]);
    }
    public sealed class RewardOffer
    {
        public readonly ItemDefinition def;
        public readonly int copies,rank;
        public int owned;
        public RewardOffer(ItemDefinition def,int copies=1,int rank=1){this.def=def;this.copies=copies;this.rank=rank;}
        public string Name=>def.name+(copies>1?" ×"+copies:"");
    }
    public sealed class AbilityDeck
    {
        public const int SequenceSize=5;
        readonly Random random;
        public LoadoutItem[] Slots {get;}=new LoadoutItem[SequenceSize];
        public LoadoutItem[] Weapons {get;}=new LoadoutItem[2];
        public LoadoutItem[] Armor {get;}=new LoadoutItem[3];
        public LoadoutItem Rig {get;private set;}
        public LoadoutItem Ultimate {get;private set;}
        public bool UltimatesUnlocked {get;private set;}
        public LoadoutItem Fists {get;}
        public readonly LoadoutItem[] Basics=new LoadoutItem[2];
        public readonly List<LoadoutItem> Items=new List<LoadoutItem>();
        public RewardOffer[] Offers {get;private set;}=Array.Empty<RewardOffer>();
        public DraftKind Draft {get;private set;}
        public int Selected {get;private set;}=-1;
        public int SelectedSlot {get;private set;}=-1;
        public int Rerolls {get;private set;}
        public int Spent {get;private set;}
        public int Fusions {get;private set;}
        public event Action<LoadoutItem,LoadoutItem[]> Fused;
        public string Receipt {get;private set;}="";
        public string MoveMessage {get;private set;}="";
        public bool RewardClaimed {get;private set;}
        public int Count=>Slots.Count(s=>s!=null&&s.def.kind==ItemKind.Ability);
        public bool TwoHanded=>Weapons[0]!=null&&Weapons[0].def.hands==2;
        public bool Pinball=>Rig?.def.id=="faultline";
        public IEnumerable<LoadoutItem> Equipment=>Armor.Where(i=>i!=null).Concat(Rig==null?Enumerable.Empty<LoadoutItem>():new[]{Rig});
        public LoadoutItem Equipped(Signature signature)=>Equipment.FirstOrDefault(i=>i.def.signature==signature);
        public float Stat(Func<GearStats,float> pick)=>Equipment.Sum(i=>pick(i.def.stats)*ItemRarity.Power[i.rank]);
        public float Lifesteal=>(BalanceOverrides.LegacySiphonBlade?Weapons.Where(w=>w!=null&&w.def.id=="siphonblade").Distinct().Sum(w=>LoadoutCatalog.SiphonRate(w.rank)):0)+Equipment.Where(i=>i.def.signature==Signature.Reclaimer).Sum(i=>LoadoutCatalog.ReclaimerRate(i.rank));
        public float Speed=>1+Stat(s=>s.speed);
        public float Damage=>1+Stat(s=>s.damage);
        public float MaxHealthBonus=>Stat(s=>s.health);
        public float DamageTaken=>Math.Max(.35f,1-Stat(s=>s.resistance));
        public float StaggerBonus=>Equipped(Signature.Tactician)==null?0:6*ItemRarity.Power[Equipped(Signature.Tactician).rank];
        public float Regeneration=>0;
        public IEnumerable<LoadoutItem> Hosts=>Items.Where(i=>i.Augmentable);
        public LoadoutItem ModuleHost(LoadoutItem module)=>Hosts.FirstOrDefault(i=>i.augment==module);
        public IEnumerable<LoadoutItem> CompatibleHosts(ItemDefinition module)=>Hosts.Where(i=>module.module!=null&&module.module.Fits(i.def));
        public LoadoutItem HostAt(PlaceKind place,int index)=>place==PlaceKind.Sequence&&index>=0&&index<5?(Slots[index]?.def.kind==ItemKind.Basic?WeaponFor(Slots[index]):Slots[index]):place==PlaceKind.Weapon&&index>=0&&index<2?WeaponFor(Basics[index]):null;
        public bool CanClaim=>!RewardClaimed&&Selected>=0&&(Draft!=DraftKind.Ability||(SelectedSlot>=0&&Slots[SelectedSlot]==null));
        public IEnumerable<LoadoutItem> Inventory=>Items.Where(i=>i!=Fists&&Array.IndexOf(Slots,i)<0&&Array.IndexOf(Weapons,i)<0&&Array.IndexOf(Armor,i)<0&&Rig!=i&&Ultimate!=i&&ModuleHost(i)==null);
        public AbilityDeck(int seed)
        {
            random=new Random(seed^0x5f3759df);Fists=new LoadoutItem(LoadoutCatalog.Fists);Items.Add(Fists);
            for(int i=0;i<2;i++){Basics[i]=new LoadoutItem(new ItemDefinition("basic"+i,"Basic "+(i+1),ItemKind.Basic,"Uses Weapon "+(i+1)+". Empty mounts use fists.")){mount=i};Slots[i]=Basics[i];Items.Add(Basics[i]);}
        }
        public bool Has(AbilityId id)=>Slots.Any(s=>s?.def.ability?.id==id);
        public LoadoutItem WeaponFor(LoadoutItem basic)=>Weapons[basic.mount]??Fists;
        public AbilityCard Card(LoadoutItem item)=>LoadoutCatalog.Action(item.def.kind==ItemKind.Basic?WeaponFor(item):item);
        public AbilityCard CardAt(int slot)=>Slots[slot]==null?null:Card(Slots[slot]);
        public string SlotName(int slot)=>Slots[slot]==null?"Empty":Slots[slot].def.kind==ItemKind.Basic?Slots[slot].Name+" · "+WeaponFor(Slots[slot]).Name:Slots[slot].Name;
        public int NextOccupied(int from){for(int i=0;i<SequenceSize;i++){int slot=(from+i)%SequenceSize;if(Slots[slot]!=null)return slot;}return -1;}
        // Adjacency means neighboring sequence positions, including the repeat boundary.
        public int PairAfter(int slot)
        {
            var a=Slots[slot];int next=(slot+1)%SequenceSize;var b=Slots[next];
            return a?.def.kind==ItemKind.Basic&&b?.def.kind==ItemKind.Basic&&LoadoutCatalog.Family(WeaponFor(a).def.weapon)==LoadoutCatalog.Family(WeaponFor(b).def.weapon)?next:-1;
        }
        public int Copies(ItemDefinition def,int rank)=>Items.Count(i=>i.def.id==def.id&&i.rank==rank);
        public void Open(DraftKind kind,bool grantReroll){Draft=kind;if(kind==DraftKind.Ultimate)UltimatesUnlocked=true;RewardClaimed=false;Receipt="";MoveMessage="";if(grantReroll)Rerolls++;Roll();}
        bool UnlockedReward(ItemDefinition def)=>LoadoutCatalog.RewardEnabled(def)&&(def.kind!=ItemKind.Ultimate||UltimatesUnlocked);
        void Shuffle<T>(List<T> list){for(int i=list.Count-1;i>0;i--){int j=random.Next(i+1);T temp=list[i];list[i]=list[j];list[j]=temp;}}
        void Roll()
        {
            Selected=SelectedSlot=-1;var choices=new List<RewardOffer>();
            if(Draft==DraftKind.Ability)
            {foreach(var def in LoadoutCatalog.Abilities.Where(LoadoutCatalog.RewardEnabled))if(!Items.Any(i=>i.def==def))choices.Add(new RewardOffer(def));Shuffle(choices);}
            else if(Draft==DraftKind.Ultimate)
            {choices.AddRange(LoadoutCatalog.Ultimates.Where(LoadoutCatalog.RewardEnabled).Select(d=>new RewardOffer(d)));Shuffle(choices);}
            else if(Draft==DraftKind.Augment)
            {choices.AddRange(AugmentCatalog.Offers(this,random).Select(d=>new RewardOffer(d)));}
            else if(Draft==DraftKind.Equipment)
            {var pool=LoadoutCatalog.Equipment.Where(UnlockedReward).ToList();Shuffle(pool);var empty=Enumerable.Range(0,3).FirstOrDefault(i=>Armor[i]==null);if(Armor[empty]==null){var wanted=pool.First(d=>LoadoutCatalog.ArmorIndex(d.kind)==empty);choices.Add(new RewardOffer(wanted));pool.Remove(wanted);}choices.AddRange(pool.Take(3-choices.Count).Select(d=>new RewardOffer(d)));}
            else if(Draft==DraftKind.Weapon)
            {foreach(var def in LoadoutCatalog.Weapons.Where(LoadoutCatalog.RewardEnabled))choices.Add(new RewardOffer(def));Shuffle(choices);}
            else
            {
                var owned=Items.Where(i=>i.def.kind!=ItemKind.Basic&&i.def.kind!=ItemKind.Augment&&i.rank<5&&UnlockedReward(i.def)).ToList();Shuffle(owned);
                if(owned.Count>0){var item=owned[0];choices.Add(new RewardOffer(item.def,2));}
                else choices.Add(new RewardOffer(LoadoutCatalog.Fists));
                var support=LoadoutCatalog.All.Where(d=>d.kind!=ItemKind.Ultimate&&d!=choices[0].def&&LoadoutCatalog.RewardEnabled(d)).ToList();Shuffle(support);
                var tags=string.Join(" ",Slots.Where(i=>i!=null).Select(i=>(i.def.kind==ItemKind.Basic?WeaponFor(i):i).def.tags));
                var useful=support.FirstOrDefault(d=>d.tags.Split(' ').Any(t=>t.Length>0&&tags.Contains(t)))??support[0];choices.Add(new RewardOffer(useful));
                var discovery=LoadoutCatalog.All.Where(d=>d!=choices[0].def&&d!=useful&&UnlockedReward(d)).ToList();Shuffle(discovery);choices.Add(new RewardOffer(discovery[0]));
            }
            Offers=choices.Take(3).Select(o=>new RewardOffer(o.def,o.copies,o.def.kind==ItemKind.Augment?1:ItemRarity.Roll(random))).ToArray();
            foreach(var offer in Offers)offer.owned=Copies(offer.def,offer.rank);
        }
        public void Select(int index){if(!RewardClaimed&&index>=0&&index<Offers.Length)Selected=index;}
        public void Assign(int slot){if(slot>=0&&slot<SequenceSize&&Slots[slot]==null)SelectedSlot=slot;}
        public bool Reroll(){if(RewardClaimed||Rerolls<=0)return false;Rerolls--;Spent++;Roll();return true;}
        public bool Claim()
        {
            if(!CanClaim)return false;var received=Receive(Offers[Selected]);
            if(Draft==DraftKind.Ability&&received!=null)Move(received,PlaceKind.Sequence,SelectedSlot);
            RewardClaimed=true;return true;
        }
        LoadoutItem Receive(RewardOffer offer)
        {
            Receipt="Received "+offer.Name+" ("+ItemRarity.Names[offer.rank]+").";
            var received=new List<LoadoutItem>();
            for(int i=0;i<offer.copies;i++)received.Add(Acquire(offer.def,offer.rank));
            // A bundle may fuse on its first copy and leave a lower-rarity spare on its last.
            return received.Where(Items.Contains).OrderByDescending(i=>i.rank).ThenBy(Priority).First();
        }
        public bool CanClaimTo(int offer,PlaceKind place,int index=0)
        {
            if(RewardClaimed||offer<0||offer>=Offers.Length)return false;
            var kind=Offers[offer].def.kind;
            if(Draft==DraftKind.Ability&&(place!=PlaceKind.Sequence||index<0||index>=SequenceSize||Slots[index]!=null))return false;
            if(kind==ItemKind.Augment)return place==PlaceKind.Inventory||(HostAt(place,index) is LoadoutItem host&&Offers[offer].def.module.Fits(host.def));
            if(place==PlaceKind.Inventory)return Draft!=DraftKind.Ability;
            if(place==PlaceKind.Sequence)return kind==ItemKind.Ability&&index>=0&&index<SequenceSize&&(!TwoHanded||Slots[index]?.def.kind!=ItemKind.Basic);
            if(place==PlaceKind.Weapon)return kind==ItemKind.Weapon&&index>=0&&index<2;
            if(place==PlaceKind.Rig)return kind==ItemKind.Rig;
            if(place==PlaceKind.Ultimate)return kind==ItemKind.Ultimate;
            return ArmorIndex(place)>=0&&ArmorIndex(place)==LoadoutCatalog.ArmorIndex(kind);
        }
        public bool ClaimTo(int offer,PlaceKind place,int index=0)
        {
            if(!CanClaimTo(offer,place,index))return false;
            Selected=offer;var item=Receive(Offers[offer]);
            if(place!=PlaceKind.Inventory)Move(item,place,index);
            RewardClaimed=true;return true;
        }
        public void SuggestedPlace(ItemDefinition def,out PlaceKind place,out int index)
        {
            index=0;
            if(def.kind==ItemKind.Augment){var hosts=CompatibleHosts(def).ToArray();if(hosts.Length!=1){place=PlaceKind.Inventory;index=-2;MoveMessage="Choose a compatible host; no module has been attached yet.";return;}var host=hosts[0];index=Array.IndexOf(Slots,host);if(index>=0){place=PlaceKind.Sequence;return;}index=Array.FindIndex(Weapons,i=>i==host);if(host==Fists)index=Array.FindIndex(Weapons,i=>i==null);if(index>=0){place=PlaceKind.Weapon;return;}place=PlaceKind.Inventory;index=-2;return;}
            if(def.kind==ItemKind.Weapon)
            {
                place=PlaceKind.Weapon;index=Array.FindIndex(Weapons,w=>w==null);
                if(index<0)index=Array.FindIndex(Weapons,w=>w.def.id==def.id);
                if(index<0)index=0;return;
            }
            if(def.kind==ItemKind.Rig){place=PlaceKind.Rig;return;}
            if(def.kind==ItemKind.Ultimate){place=PlaceKind.Ultimate;return;}
            if(LoadoutCatalog.IsArmor(def.kind)){place=(PlaceKind)((int)PlaceKind.Helmet+LoadoutCatalog.ArmorIndex(def.kind));return;}
            place=PlaceKind.Sequence;index=Array.FindIndex(Slots,s=>s?.def.id==def.id);
            if(index<0)index=Array.FindIndex(Slots,s=>s==null);
            // A full, unrelated build is stored rather than silently replacing a chosen action.
            if(index<0){place=PlaceKind.Inventory;index=0;}
        }
        public LoadoutItem Acquire(ItemDefinition def,int rank=1)
        {
            var added=new LoadoutItem(def,rank);Items.Add(added);LoadoutItem result=added;
            if(def.kind==ItemKind.Augment)return added;
            for(int r=1;r<5;r++)
            {
                var copies=Items.Where(i=>i.def.id==def.id&&i.rank==r).ToList();
                while(copies.Count>=3)
                {
                    copies.Sort((a,b)=>Priority(a).CompareTo(Priority(b)));
                    var keep=copies[0];for(int i=1;i<3;i++){copies[i].augment=null;Unplace(copies[i]);Items.Remove(copies[i]);}
                    keep.rank++;Fusions++;result=keep;Fused?.Invoke(keep,new[]{copies[1],copies[2]});
                    Receipt+="  FUSED: 3 x "+ItemRarity.Names[r]+" "+def.name+" -> "+ItemRarity.Names[keep.rank]+". "+def.upgrade;
                    copies=Items.Where(i=>i.def.id==def.id&&i.rank==r).ToList();
                }
            }
            return Items.Contains(result)?result:Items.First(i=>i.def==def);
        }
        int Priority(LoadoutItem i){if(i==Fists)return -1;int n=Array.IndexOf(Slots,i);if(n>=0)return n;n=Array.IndexOf(Weapons,i);if(n>=0)return 10+n;n=Array.IndexOf(Armor,i);if(n>=0)return 14+n;return i==Rig?12:i==Ultimate?13:100+Items.IndexOf(i);}
        static int ArmorIndex(PlaceKind place)=>(int)place>=(int)PlaceKind.Helmet&&(int)place<=(int)PlaceKind.Boots?(int)place-(int)PlaceKind.Helmet:-1;
        void Unplace(LoadoutItem item){for(int i=0;i<SequenceSize;i++)if(Slots[i]==item)Slots[i]=null;for(int i=0;i<2;i++)if(Weapons[i]==item)Weapons[i]=null;for(int i=0;i<3;i++)if(Armor[i]==item)Armor[i]=null;if(Rig==item)Rig=null;if(Ultimate==item)Ultimate=null;}
        public bool CanMove(LoadoutItem item,PlaceKind place,int index=0)
        {
            if(item==null||!Items.Contains(item))return false;
            if(item.def.kind==ItemKind.Augment)return place==PlaceKind.Inventory||HostAt(place,index)!=null&&item.def.module.Fits(HostAt(place,index).def);
            if(item==Fists)return place==PlaceKind.Weapon&&index>=0&&index<2;
            if(place==PlaceKind.Sequence)return index>=0&&index<SequenceSize&&(item.def.kind==ItemKind.Basic||item.def.kind==ItemKind.Ability)&&(!TwoHanded||item.def.kind==ItemKind.Basic||Slots[index]?.def.kind!=ItemKind.Basic);
            if(place==PlaceKind.Weapon)return index>=0&&index<2&&item.def.kind==ItemKind.Weapon;
            if(place==PlaceKind.Rig)return item.def.kind==ItemKind.Rig;
            if(place==PlaceKind.Ultimate)return item.def.kind==ItemKind.Ultimate;
            if(ArmorIndex(place)>=0)return ArmorIndex(place)==LoadoutCatalog.ArmorIndex(item.def.kind);
            return place==PlaceKind.Inventory&&item.def.kind!=ItemKind.Basic;
        }
        void MoveBlock(int start)
        {
            var rest=Slots.Where(s=>s==null||s.def.kind!=ItemKind.Basic).ToList();
            start=Math.Max(0,Math.Min(3,start));rest.Insert(start,Basics[0]);rest.Insert(start+1,Basics[1]);rest.CopyTo(Slots);
        }
        public bool Move(LoadoutItem item,PlaceKind place,int index=0)
        {
            if(!CanMove(item,place,index)){MoveMessage=TwoHanded?"Move the locked basic pair as one block. Weapons use mounts; abilities use sequence slots.":"Both basics stay in the sequence. Weapons use mounts; abilities use sequence slots.";return false;}
            if(item.def.kind==ItemKind.Augment){var previous=ModuleHost(item);if(previous!=null)previous.augment=null;var host=HostAt(place,index);if(place!=PlaceKind.Inventory)host.augment=item;MoveMessage=place==PlaceKind.Inventory?"Module stored; socket is free.":item.Name+" attached to "+host.Name+". Previous module stored.";return true;}
            if(item==Fists){if(TwoHanded)Weapons[0]=Weapons[1]=null;else Weapons[index]=null;MoveMessage="Starter fists equipped; displaced weapon stored.";return true;}
            if(TwoHanded&&place==PlaceKind.Sequence&&item.def.kind==ItemKind.Basic){MoveBlock(index-item.mount);MoveMessage="Two-handed attack moved as one locked pair.";SelectedSlot=-1;return true;}
            int source=Array.IndexOf(Slots,item),weapon=Array.IndexOf(Weapons,item);
            LoadoutItem displaced=place==PlaceKind.Sequence?Slots[index]:place==PlaceKind.Weapon?Weapons[index]:place==PlaceKind.Rig?Rig:place==PlaceKind.Ultimate?Ultimate:null;
            if(displaced==item)return true;
            if(place==PlaceKind.Sequence&&source<0&&displaced?.def.kind==ItemKind.Basic)
            {
                int spare=Array.FindIndex(Slots,s=>s==null);if(spare<0)spare=Array.FindIndex(Slots,s=>s.def.kind==ItemKind.Ability);
                Slots[spare]=displaced;displaced=null;MoveMessage="Basic moved to slot "+(spare+1)+". Any replaced ability is stored.";
            }
            else MoveMessage=item.Name+" moved.";
            bool wasTwoHanded=TwoHanded;Unplace(item);
            if(place==PlaceKind.Sequence){Slots[index]=item;if(source>=0)Slots[source]=displaced;}
            if(place==PlaceKind.Weapon)
            {
                if(item.def.hands==2){Weapons[0]=Weapons[1]=item;MoveBlock(Math.Min(Array.IndexOf(Slots,Basics[0]),Array.IndexOf(Slots,Basics[1])));}
                else {if(wasTwoHanded)Weapons[0]=Weapons[1]=null;Weapons[index]=item;if(weapon>=0&&!wasTwoHanded)Weapons[weapon]=displaced;}
            }
            if(place==PlaceKind.Rig)Rig=item;
            if(place==PlaceKind.Ultimate)Ultimate=item;
            if(ArmorIndex(place)>=0)Armor[ArmorIndex(place)]=item;
            SelectedSlot=-1;return true;
        }
        public bool Attach(LoadoutItem module,LoadoutItem host)
        {
            if(module==null||host==null||!Items.Contains(module)||!Items.Contains(host)||module.def.module==null||!module.def.module.Fits(host.def))return false;
            var previous=ModuleHost(module);if(previous!=null)previous.augment=null;host.augment=module;MoveMessage=module.Name+" attached to "+host.Name+"; replaced module stored.";return true;
        }
        public void Swap(int a,int b){if(a>=0&&b>=0&&a<SequenceSize&&b<SequenceSize&&Slots[a]!=null)Move(Slots[a],PlaceKind.Sequence,b);}
        public string Loadout=>string.Join(" > ",Enumerable.Range(0,SequenceSize).Select(i=>(i+1)+": "+SlotName(i)));
    }
}
