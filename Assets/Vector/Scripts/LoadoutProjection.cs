using System.Collections.Generic;
using System.Linq;

namespace VectorGame
{
    public sealed partial class AbilityDeck
    {
        // Run the production receive / fusion / move rules on a detached graph. A hover
        // must not consume RNG, fire live events, change sockets, or write a save.
        internal sealed class Projection
        {
            public AbilityDeck after;
            public readonly Dictionary<LoadoutItem,LoadoutItem> copies=new Dictionary<LoadoutItem,LoadoutItem>();
            public readonly List<int> fusionRanks=new List<int>();
            public LoadoutItem incoming;
            public bool valid;
            public LoadoutItem Copy(LoadoutItem item)=>item!=null&&copies.TryGetValue(item,out var copy)?copy:null;
        }
        internal Projection PreviewChange(RewardOffer offer,LoadoutItem source,PlaceKind place,int index=0,LoadoutItem host=null)
        {
            var p=new Projection();var d=p.after=new AbilityDeck(0);
            p.copies[Fists]=d.Fists;
            for(int i=0;i<2;i++)p.copies[Basics[i]]=d.Basics[i];
            foreach(var item in Items)if(!p.copies.ContainsKey(item))p.copies[item]=new LoadoutItem(item.def,item.rank){mount=item.mount};
            d.Items.Clear();
            foreach(var item in Items){var copy=p.Copy(item);copy.rank=item.rank;copy.augment=p.Copy(item.augment);d.Items.Add(copy);}
            for(int i=0;i<5;i++)d.Slots[i]=p.Copy(Slots[i]);
            for(int i=0;i<2;i++)d.Weapons[i]=p.Copy(Weapons[i]);
            for(int i=0;i<3;i++)d.Armor[i]=p.Copy(Armor[i]);
            d.Rig=p.Copy(Rig);d.Ultimate=p.Copy(Ultimate);d.UltimatesUnlocked=UltimatesUnlocked;
            d.Fused+=(keep,consumed)=>p.fusionRanks.Add(keep.rank);
            p.incoming=offer!=null?d.Receive(offer):p.Copy(source);
            if(p.incoming==null)return p;
            p.valid=host!=null?d.Attach(p.incoming,p.Copy(host)):offer!=null&&place==PlaceKind.Inventory||d.Move(p.incoming,place,index);
            return p;
        }
    }
}
