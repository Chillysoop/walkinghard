using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckPursuerFinale()
        {
            ProbeReset();ResetFinale();int savedWave=Wave;var savedState=State;bool savedBoss=bossSpawned;
            Wave=8;bossSpawned=false;WaveElapsed=0;spawnTimer=1000;
            spawnRandom=new System.Random(Seed);combatRandom=new System.Random(Seed^6179);
            hero.Root.position=new Vector3(0,.04f,160);heroPhysics.Motor.position=HeroPosition;heroPhysics.MovePosition(HeroPosition);
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
            MechanicRequire(pursuer==null,"no pursuer actor is needed during ordinary waves");
            var anchor=HeroPosition;float time=Elapsed,health=Health;UltimateRemaining=3.2f;
            StartFinaleReveal();var model=pursuer;
            MechanicRequire(State==RunState.Revealing&&model!=null&&!bossSpawned,"finale stages a giant before activating boss combat");
            MechanicRequire(revealFollowers.Count==12&&revealFollowers.All(f=>f.origin.z<model.Root.position.z)&&enemies.All(e=>!e.boss),"the staged army starts behind the giant without spawning a second boss");
            MechanicRequire(!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cameraRig),new Bounds(model.Root.position+Vector3.up*5,new Vector3(6,11,6))),"giant creation is hidden behind the original camera");
            StepFinaleReveal(1.65f);
            MechanicRequire(model.Root.position.z>pursuerRevealStart.z+.5f&&pursuerGaitClock>0,"giant is already walking before the camera reveals him");
            MechanicRequire(HeroPosition==anchor&&hero.Root.forward.z<-.99f&&Elapsed==time&&Health==health&&UltimateRemaining==3.2f,"turning holds position and freezes combat, health and ultimate timers");
            TogglePause();float age=revealAge;StepFinaleReveal(2);MechanicRequire(State==RunState.Paused&&revealAge==age,"pause freezes reveal");TogglePause();
            StepFinaleReveal(2);MechanicRequire(State==RunState.Revealing&&cameraRig.transform.position.x<anchor.x-5,"camera orbits around the stationary hero");
            StepFinaleReveal(10);var boss=enemies.Single(e=>e.boss);
            MechanicRequire(State==RunState.Running&&boss.rig==model&&pursuer==null&&bossSpawned,"reveal activates the same giant without a replacement pop");
            MechanicRequire(Mathf.Abs(pursuerGaitClock-RevealDuration*.3f)<.001f&&pursuerStride==.6f,"combat inherits the moving reveal stride without restarting the animation");
            MechanicRequire(cameraRig.transform.position.z>HeroPosition.z&&cameraRig.transform.forward.z<0&&boss.rig.Root.localScale.x==4.5f,"camera settles behind reversed hero with giant scale");
            int count=Spawned;StepFinaleReveal(10);MechanicRequire(Spawned==count,"reveal cannot activate two bosses");
            float start=boss.rig.Root.position.z;
            for(int i=0;i<120;i++){StepPursuer(boss,1f/60);Physics.SyncTransforms();Physics.Simulate(1f/60);}
            MechanicRequire(boss.rig.Root.position.z>start+1.2f,"giant really walks toward the hero under full floor friction");
            boss.rig.Root.position=HeroPosition+Vector3.back*2.55f;boss.physics.Motor.position=boss.rig.Root.position;Physics.SyncTransforms();
            MechanicRequire(Reachable(boss,Deck.Card(Deck.Basics[0]).range),"fists can reach the giant at its stopping distance");
            var head=cameraRig.WorldToViewportPoint(boss.rig.Root.position+Vector3.up*10.5f);MechanicRequire(head.y>.15f&&head.y<.85f,"giant head stays clear of the top HUD at melee range");
            int left=LeftEntries,right=RightEntries,horizon=HorizonEntries;
            SpawnEntry(0);SpawnEntry(1);SpawnEntry(2);
            MechanicRequire(LeftEntries==left&&RightEntries==right&&HorizonEntries==horizon+3,"all finale reinforcements enter through the concealed horizon");
            var ally=enemies.First(e=>!e.boss);ally.rig.Root.position=boss.rig.Root.position+Vector3.back*5;
            var steering=FlowAroundPursuer(ally,Vector3.forward*3);
            MechanicRequire(Mathf.Abs(steering.x)>.1f&&steering.z>0,"incoming allies flow around the giant toward the hero");
            MechanicRequire(RearBoundaryZ>HeroPosition.z+7.9f,"rear death boundary reverses with the camera");
            boss.rig.Root.position=HeroPosition+Vector3.forward*40;boss.physics.Motor.position=boss.rig.Root.position;ResolveRearBoundary();
            MechanicRequire(boss.deathAge<0,"boss cannot be killed by the ordinary rear boundary");
            boss.rig.Root.position=HeroPosition+Vector3.back*2.55f;boss.physics.Motor.position=boss.rig.Root.position;
            ActiveSlot=PairSlot=-1;actionTarget=pairTarget=null;recovery=1;Physics.SyncTransforms();
            bool wasQa=qa;qa=false;try{for(int i=0;i<12;i++)AdvanceCombat(1f/60);}finally{qa=wasQa;}
            MechanicRequire(Vector3.Distance(HeroPosition,anchor)<.001f,"hero remains planted during live finale combat");
            ResetFinale();ProbeReset();Wave=savedWave;State=savedState;bossSpawned=savedBoss;cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
        }
    }
}
