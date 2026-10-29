using UnityEngine;

namespace VectorGame
{
    public enum ImpactKind { Strike, Slash, Crush, Electric, Bullet, Pull, Force }
    public readonly struct ImpactReaction
    {
        public readonly ImpactKind kind;
        public readonly Vector3 point,direction,normal;
        public readonly float strength;
        public readonly bool wide;
        public ImpactReaction(ImpactKind kind,Vector3 point,Vector3 direction,float strength=1,bool wide=false,Vector3 normal=default)
        {this.kind=kind;this.point=point;this.direction=direction.sqrMagnitude>.001f?direction.normalized:Vector3.forward;this.strength=Mathf.Clamp(strength,.3f,1.8f);this.wide=wide;this.normal=normal.sqrMagnitude>.001f?normal.normalized:Vector3.up;}
    }

    public sealed partial class VectorEncounter
    {
        ImpactReaction DescribeImpact(Enemy e,Vector3 impulse,bool cut,ImpactKind? overrideKind=null)
        {
            var item=Owner;var def=item?.def;var weapon=def?.weapon;string id=def?.id??"";
            ImpactKind kind=cut?ImpactKind.Slash:ImpactKind.Strike;
            if(!cut)
            {
                if((currentCredit?.hop??0)>0)kind=ImpactKind.Crush;
                else if(weapon==WeaponKind.Hammer||id=="Breaker")kind=ImpactKind.Crush;
                else if(id=="ArcBolt"||id=="ConductiveBrand"||id=="tempest"||id=="wraps"||def?.signature==Signature.Relay)kind=ImpactKind.Electric;
                else if(weapon==WeaponKind.Pistol||weapon==WeaponKind.Rifle||weapon==WeaponKind.Shotgun)kind=ImpactKind.Bullet;
                else if(id=="GravityWell"||id=="VectorTether"||id=="DrainBrand"||id=="harvest")kind=ImpactKind.Pull;
                else if(id=="ForcePush"||id=="MassDriver"||id=="singularity")kind=ImpactKind.Force;
            }
            if(overrideKind.HasValue)kind=overrideKind.Value;
            Vector3 aim=e.physics.Center-(HeroPosition+Vector3.up);aim.y=0;if(aim.sqrMagnitude<.01f)aim=hero.Root.forward;aim.Normalize();
            Vector3 right=Vector3.Cross(Vector3.up,aim),point=e.physics.Center;
            bool wide=def?.hands==2||id=="cyclone";
            float strength=wide?1.5f:1;if(item!=null)strength+=.06f*(item.rank-1);
            Vector3 direction=impulse.sqrMagnitude>.01f?impulse.normalized:aim,normal=Vector3.up;
            if(kind==ImpactKind.Slash)
            {
                // One shared diagonal blade plane, rather than a random sever location.
                // Its intersection with each victim determines which anatomical seams are cut.
                float reverse=currentCredit?.augment==true?-1:1;
                normal=(Vector3.up+right*(wide?.34f:.18f)*reverse-aim*.08f).normalized;
                Vector3 planeOrigin=HeroPosition+Vector3.up*1.2f;
                point.y=planeOrigin.y-(normal.x*(point.x-planeOrigin.x)+normal.z*(point.z-planeOrigin.z))/normal.y;
                direction=right*reverse;point.y=Mathf.Clamp(point.y,e.rig.Root.position.y+.5f,e.rig.Root.position.y+2.1f);
            }
            else if(kind==ImpactKind.Crush)point+=Vector3.up*.4f;
            else if(kind==ImpactKind.Bullet)point-=aim*.25f;
            return new ImpactReaction(kind,point,direction,strength,wide,normal);
        }
        void PresentImpact(Enemy e,ImpactReaction hit,bool lethal)
        {
            if(FastSimulation)return;
            e.reaction=hit;e.reactionLeft=hit.kind==ImpactKind.Electric?.65f:hit.kind==ImpactKind.Pull?.4f:.32f;e.reactionPulse=0;
            if(lethal)e.physics.ReactToImpact(hit);
            fx.Contact(hit,lethal);
            if(Once("impact-sound-"+hit.kind))fx.PlayImpact(hit.kind);
            if(hit.kind==ImpactKind.Slash||hit.kind==ImpactKind.Crush)
            {ImpactPause(hit.wide?.055f:.028f);if(!ReducedMotion)shake=Mathf.Max(shake,hit.wide?.11f:.045f);}
        }
        void StepReaction(Enemy e,float dt,bool alive)
        {
            if(FastSimulation||e.reactionLeft<=0)return;
            e.reactionLeft=Mathf.Max(0,e.reactionLeft-dt);
            var hit=e.reaction;float weight=Mathf.Clamp01(e.reactionLeft/(hit.kind==ImpactKind.Electric?.65f:.32f));
            if(alive)e.rig.ReactToImpact(hit,weight,clock,ReducedMotion);
            else if(hit.kind==ImpactKind.Electric)e.physics.ElectricalSpasm(clock,weight,dt,ReducedMotion);
            if(hit.kind==ImpactKind.Electric)
            {
                e.reactionPulse-=dt;
                if(e.reactionPulse<=0){e.reactionPulse=.085f;fx.Lightning(e.rig.Head.position,e.rig.LeftHand.position,Color.cyan,.10f);fx.Lightning(e.rig.RightHand.position,e.physics.Center,new Color(.65f,.8f,1),.10f);}
            }
        }
    }
}
