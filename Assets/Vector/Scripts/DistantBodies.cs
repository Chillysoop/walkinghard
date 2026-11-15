using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        bool DistantBody(Enemy e)
        {
            if(!JourneyActive)return false;
            bool dormant=!e.rig.Root.gameObject.activeSelf;
            if(dormant)
            {
                if((e.cachedPosition-HeroPosition).sqrMagnitude>45*45)return true;
                e.rig.Root.gameObject.SetActive(true);SetBodyWalkCollision(e,false);return false;
            }
            if(e.deathAge<10||e.freightLeft>0||flights.Any(f=>f.body==e))return false;
            var center=e.physics.Center;
            if((center-HeroPosition).sqrMagnitude<55*55||InCombatView(center)||(e.deathAge<30&&!e.physics.Settled))return false;
            // Keep the actor, all bones, cuts, identity and counters. Only an old or settled,
            // unreachable body leaves PhysX; approaching it reactivates the same body.
            e.cachedPosition=center;e.rig.Root.gameObject.SetActive(false);return true;
        }
    }
    public sealed partial class FighterPhysics
    {
        public bool Settled=>Ragdoll&&(fast||bodies.All(b=>b.IsSleeping()));
    }
}
