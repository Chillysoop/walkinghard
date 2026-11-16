using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorGame
{
    // Reward-screen history, scoped to this run. Exact identities distinguish
    // fist styles and one-/two-handed variants without promising a fixed set.
    public sealed class WeaponDiscoveryState
    {
        public int roll;
        public bool active;
        public readonly Dictionary<string,int> last=new Dictionary<string,int>();
    }
    public sealed partial class AbilityDeck
    {
        WeaponDiscoveryState weaponDiscovery;
        public bool WeaponDiscovery=>weaponDiscovery?.active==true;
        internal int LastWeaponOffer(string id)=>weaponDiscovery!=null&&weaponDiscovery.last.TryGetValue(id,out int at)?at:0;
        void CurateWeaponDiscovery(List<RewardOffer> choices)
        {
            if(!WeaponDiscovery||choices.Count<3)return;
            var candidates=LoadoutCatalog.Weapons.Where(d=>RewardAvailable(d)&&!choices.Take(2).Any(o=>o.def==d)).ToArray();
            if(candidates.Length==0){weaponDiscovery.active=false;return;}
            // An independent, reproducible tie-break leaves the rarity RNG and
            // the two other offers untouched. Rerolls still cost a normal charge.
            int seed=((CheckpointRandom)random).Seed;
            uint Tie(ItemDefinition d){unchecked{uint h=(uint)seed^(uint)weaponDiscovery.roll*2654435761u;foreach(char c in d.id)h=(h^c)*16777619u;return h;}}
            var selected=candidates.OrderBy(d=>LastWeaponOffer(d.id)).ThenBy(Tie).First();
            choices[2]=new RewardOffer(selected);
        }
        void RememberWeaponOffers()
        {
            foreach(var offer in Offers.Where(o=>o.def.kind==ItemKind.Weapon))weaponDiscovery.last[offer.def.id]=weaponDiscovery.roll;
        }
    }
}
