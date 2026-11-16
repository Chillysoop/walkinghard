using System;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] sealed class RivetControl
        {public float until,slow;public LoadoutItem source;}
        void HookPrimary(LoadoutItem host,Enemy target)
        {
            if(target==null||target.deathAge>=0||!Reachable(target,LoadoutCatalog.Action(host).range))return;
            Vector3 delta=HeroPosition-target.rig.Root.position;delta.y=0;
            float distance=delta.magnitude;
            if(distance<=1.8f)return;
            float force=Mathf.Min((distance-1.8f)/.18f,LoadoutCatalog.HookForce(host)*(host.HasAugment(AugmentId.HookedEdge)?1.25f:1));
            Control(target,.18f,delta.normalized*force);SetSetup(target,host);
            fx.Arc(target.physics.Center,target.physics.Center+delta.normalized*Mathf.Min(1.2f,distance-1.8f),Color.white);
            Note("Hookblade pulled a surviving target");
        }
        void ApplyRivet(LoadoutItem host,Enemy target)
        {
            if(target.deathAge>=0||target.boss||target.foreman!=null)return;
            float slow=LoadoutCatalog.RivetSlow(host)*ControlResistance(target);
            if(target.rivet!=null&&target.rivet.until>Elapsed&&target.rivet.slow>slow)return;
            target.rivet=new RivetControl{source=host,slow=slow,until=Elapsed+LoadoutCatalog.RivetSeconds(host)};
            SetSetup(target,host);fx.Arc(target.physics.Center,target.rig.Root.position+Vector3.up*.08f,Color.white);
            fx.Ring(target.rig.Root.position,.45f,Color.white,.25f);Note("Rivet Pistol slowed an advance");
        }
        Vector3 RivetVelocity(Enemy e,Vector3 desired,Vector3 inward,float dt)
        {
            // Voluntary advance only: leave impulses, retreat and committed charges intact.
            if(e.rivet==null||e.rivet.until<=Elapsed||e.entering||e.stun>0||e.boss||e.foreman!=null||DistrictChampionBusy(e)||e.specialist?.stage>0)return desired;
            float advance=Mathf.Max(0,Vector3.Dot(desired,inward));
            if(advance>0)ContributionFor(e.rivet.source).control+=dt;
            return desired-inward*(advance*e.rivet.slow);
        }
    }
}
