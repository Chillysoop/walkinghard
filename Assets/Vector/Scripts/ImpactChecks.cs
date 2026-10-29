using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckImpactMechanics()
        {
            foreach(var def in LoadoutCatalog.Weapons)
            {
                ProbeReset();var host=ProbeHost(def);var target=ProbeEnemy(0,2,5);int kills=Kills;ResolveBasic(host,activeCard,target,false);
                MechanicRequire(Kills==kills+1&&target.physics.Ragdoll&&target.physics.BodyCount==11,"weapon reaction retains one credited 11-body ragdoll: "+def.name);
                var kind=def.weapon==WeaponKind.Sword?ImpactKind.Slash:def.weapon==WeaponKind.Hammer?ImpactKind.Crush:def.id=="wraps"?ImpactKind.Electric:def.weapon==WeaponKind.Fists?ImpactKind.Strike:ImpactKind.Bullet;
                MechanicRequire(target.reaction.kind==kind,"weapon impact identity: "+def.name);
                if(def.weapon==WeaponKind.Sword)MechanicRequire(target.physics.DetachedParts>0,"lethal blade physically disconnects joints: "+def.name);
                ProbeFrames(36);MechanicRequire(target.physics.Finite(),"weapon death impulse stays finite: "+def.name);
            }
            foreach(int bone in new[]{1,2,4,8})
            {
                ProbeReset();var e=ProbeEnemy(0,2);e.physics.Die(Vector3.zero);
                var point=e.rig.PhysicsBones[bone].position;
                e.physics.Sever(new ImpactReaction(ImpactKind.Slash,point,Vector3.right));
                MechanicRequire(e.physics.LastCutBone==bone&&e.physics.DetachedParts==1&&e.physics.CutAt(bone),"hit-location sever selects waist/head/arm/leg: "+bone);
                MechanicRequire(e.rig.PhysicsBones[bone].GetComponent<CharacterJoint>()==null,"cut removes the actual constraint before the next physics step");
                ProbeFrames(18);MechanicRequire(e.physics.Finite(),"separated anatomical parts stay finite");
            }
            ProbeReset();var great=ProbeHost(LoadoutCatalog.Find("greatsword"));var victim=ProbeEnemy(0,2,5);int before=Kills;ResolveBasic(great,activeCard,victim,false);
            MechanicRequire(victim.physics.DetachedParts>=2&&victim.physics.DetachedParts<=3,"greatsword plane crosses multiple bounded seams");
            Hurt(victim,100,Vector3.right,1,true);MechanicRequire(Kills==before+1&&KillsFor(great)==1,"fragments remain one enemy and one kill credit");
            ProbeFrames(60);MechanicRequire(victim.physics.Finite()&&victim.physics.IgnoresWalker(heroPhysics),"severed fragments remain stable and cannot block the walker");

            ProbeReset();ProbeHost(LoadoutCatalog.Find("ArcBolt"));var shock=ProbeEnemy(0,2,500);float hp=shock.health;ElectricHit(shock,10,.18f);
            float stun=shock.stun;Vector3 root=shock.rig.Root.position;var arm=shock.rig.PhysicsBones[3].localRotation;
            StepReaction(shock,.02f,true);
            MechanicRequire(shock.health==hp-10&&shock.stun==stun&&shock.rig.Root.position==root,"electrical reaction does not add damage, stun or root displacement");
            MechanicRequire(Quaternion.Angle(arm,shock.rig.PhysicsBones[3].localRotation)>1,"electrical hit articulates the live limbs");
            for(int i=0;i<45;i++)StepReaction(shock,1f/60,true);
            MechanicRequire(shock.reactionLeft==0,"electrical twitch expires instead of becoming permanent");
            ElectricHit(shock,1000,.18f);for(int i=0;i<60;i++){StepReaction(shock,1f/60,false);ProbeFrames(1);}
            MechanicRequire(shock.physics.Finite()&&shock.reactionLeft==0,"electrical death spasm settles with bounded energy");

            foreach(var def in LoadoutCatalog.Abilities.Where(d=>d.id!="Counterguard"))
            {
                ProbeReset();ProbeHost(def);var target=ProbeEnemy(0,2,500);actionTarget=target;float beforeHealth=target.health;Resolve(activeCard);
                MechanicRequire(target.health<beforeHealth&&target.reactionLeft>0,"ability drives a real hit reaction: "+def.name);
            }
            foreach(var def in LoadoutCatalog.Ultimates)
            {
                ProbeReset();var ult=Deck.Acquire(def);Deck.Move(ult,PlaceKind.Ultimate);currentCredit=new KillCredit{item=ult,action=actionSerial};var target=ProbeEnemy(0,2,5);int kills=Kills;
                ResolveUltimate();
                if(def.id=="redline")MechanicRequire(Kills==kills&&redlineTurns==3,"Redline visual vent retains a non-damaging three-turn payoff");
                else
                {
                    var kind=def.id=="tempest"?ImpactKind.Electric:def.id=="cyclone"?ImpactKind.Slash:ImpactKind.Force;
                    MechanicRequire(target.physics.Ragdoll&&target.reaction.kind==kind,"ultimate impact identity: "+def.name);
                    ProbeFrames(30);MechanicRequire(target.physics.Finite(),"ultimate fragments and launch remain finite: "+def.name);
                }
            }
        }
    }
}
