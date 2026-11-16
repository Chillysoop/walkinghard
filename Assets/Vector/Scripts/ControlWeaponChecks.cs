using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckControlWeapons()
        {
            foreach(int rank in new[]{1,2,3,4,5})
            {
                ProbeReset();var hook=ProbeHost(LoadoutCatalog.Find("hookblade"),null,rank);
                var primary=ProbeEnemy(0,3.4f);var second=ProbeEnemy(.7f,3.3f);var before=primary.rig.Root.position;actionTarget=primary;
                ResolveBasic(hook,activeCard,primary,false);ProbeFrames(8);
                MechanicRequire(primary.health<500&&primary.rig.Root.position.z<before.z-.15f,"Hookblade cuts and physically pulls its survivor at rarity "+rank);
                MechanicRequire(second.rig.Root.position.z>3.1f,"Hookblade does not pull secondary cleave victims");
                ProbeReset();hook=ProbeHost(LoadoutCatalog.Find("hookblade"),AugmentId.HookedEdge,rank);
                primary=ProbeEnemy(0,3.6f);ResolveBasic(hook,activeCard,primary,false);ProbeFrames(1);
                float expected=Mathf.Min(10,LoadoutCatalog.HookForce(hook)*1.25f);
                MechanicRequire(primary.physics.Velocity.z < -expected*.85f&&primary.physics.Velocity.z > -expected*1.1f,"Hooked Edge strengthens one pull by 25%, capped by clearance: rarity "+rank+" expected "+expected+" actual "+primary.physics.Velocity.z);
                ProbeReset();hook=ProbeHost(LoadoutCatalog.Find("hookblade"),null,rank);primary=ProbeEnemy(0,3.4f,1);second=ProbeEnemy(.7f,3.3f);ResolveBasic(hook,activeCard,primary,false);
                MechanicRequire(primary.deathAge>=0&&!InteractionNotes.ContainsKey("Hookblade pulled a surviving target"),"killed primary never transfers Hookblade pull to a survivor");
                ProbeReset();var rivet=ProbeHost(LoadoutCatalog.Find("rivetpistol"),null,rank);primary=ProbeEnemy(0,7);primary.attack=.3f;
                ResolveBasic(rivet,activeCard,primary,false);ProbeFrames(1);
                MechanicRequire(primary.health<500&&primary.stun==0&&primary.attack==.3f&&Mathf.Abs(primary.physics.Velocity.z)<.001f&&Ammo(rivet)==5,"Rivet shot spends one round without knockback or interrupt");
                Vector3 inward=Vector3.back;float slow=LoadoutCatalog.RivetSlow(rivet);var input=new Vector3(2,0,-4);var result=RivetVelocity(primary,input,inward,.1f);
                MechanicRequire(Mathf.Abs(result.z+4*(1-slow))<.001f&&result.x==2&&RivetVelocity(primary,Vector3.forward,inward,.1f)==Vector3.forward,"Rivet slows only forward voluntary movement");
                MechanicRequire(Mathf.Abs(primary.rivet.until-Elapsed-LoadoutCatalog.RivetSeconds(rivet)+1f/60)<.001f,"Rivet duration scales by rarity");
                primary.boss=true;MechanicRequire(RivetVelocity(primary,input,inward,.1f)==input,"boss approach ignores Rivet slow");primary.boss=false;
                primary.entering=true;MechanicRequire(RivetVelocity(primary,input,inward,.1f)==input,"Rivet never delays concealed entrance");primary.entering=false;
                Elapsed=primary.rivet.until+.01f;MechanicRequire(RivetVelocity(primary,input,inward,.1f)==input,"Rivet expires without permanent movement penalty");
            }
            ProbeReset();var gun=ProbeHost(LoadoutCatalog.Find("rivetpistol"),AugmentId.BoreRounds,5);var a=ProbeEnemy(0,4);var b=ProbeEnemy(0,6);var off=ProbeEnemy(3,5);ResolveBasic(gun,activeCard,a,false);
            MechanicRequire(a.rivet!=null&&b.rivet!=null&&off.rivet==null&&a.stun==0&&b.stun==0&&Ammo(gun)==5,"Bore applies Rivet to each pierced target using one round");
            var weak=Deck.Acquire(LoadoutCatalog.Find("rivetpistol"));float until=a.rivet.until;ApplyRivet(weak,a);
            MechanicRequire(a.rivet.source==gun&&a.rivet.until==until,"weaker Rivet cannot refresh or replace a stronger slow");
            a.heavy=true;a.rivet=null;ApplyRivet(gun,a);MechanicRequire(Mathf.Abs(a.rivet.slow-LoadoutCatalog.RivetSlow(gun)*.65f)<.001f,"heavy enemies resist Rivet slow");
            var graph=GraphCodec().Capture(this);var codec=GraphCodec();codec.Decode(graph);var savedEnemies=(List<Enemy>)codec.Root("enemies");var savedDeck=(AbilityDeck)codec.Root("Deck");
            MechanicRequire(savedEnemies[0].rivet.source==savedDeck.Weapons[0]&&savedEnemies[0].rivet.until==a.rivet.until,"save roundtrip preserves slow, expiry and actual weapon ownership");
            foreach(var n in graph.objects.Where(n=>n.type==typeof(Enemy).FullName))n.fields.RemoveAll(f=>f.name=="rivet");
            codec=GraphCodec();codec.Decode(graph,legacyControlWeapons:true);MechanicRequire(((List<Enemy>)codec.Root("enemies")).All(e=>e.rivet==null),"schema 12 enemies restore without new slow state");
            ProbeReset();var blade=ProbeHost(LoadoutCatalog.Find("hookblade"));var body=ProbeEnemy(0,2,1);ResolveBasic(blade,activeCard,body,false);var walker=ProbeEnemy(0,4);
            MechanicRequire(body.physics.IgnoresWalker(walker.physics)&&body.physics.IgnoresWalker(heroPhysics),"resting corpse ignores existing walker and hero");
            SetBodyWalkCollision(body,true);MechanicRequire(!body.physics.IgnoresWalker(walker.physics)&&body.physics.IgnoresWalker(heroPhysics),"flying corpse hits walkers but not hero");
            SetBodyWalkCollision(body,false);var newcomer=ProbeEnemy(2,4);MechanicRequire(body.physics.IgnoresWalker(newcomer.physics),"new walkers inherit resting-corpse filter");
        }
    }
}
