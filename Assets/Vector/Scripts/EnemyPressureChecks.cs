using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckEnemyPressureMechanics()
        {
            var savedCombatRandom=combatRandom;combatRandom=new System.Random(17);
            ProbeReset();var nearest=ProbeEnemy(0,1.2f);var repeat=ProbeEnemy(0,4.5f);
            MechanicRequire(Target(8)==nearest,"no recent pressure falls back to nearest");
            ReceiveDamage(repeat,3);MechanicRequire(Target(8)==nearest,"one hit does not override nearest");
            ReceiveDamage(repeat,3);MechanicRequire(Target(8)==repeat,"repeat damage takes target priority");
            MechanicRequire(Target(2)==nearest,"threat priority respects action range");
            MechanicRequire(Target(8,repeat)==nearest,"paired attack can choose an independent target");
            repeat.previousDamageTime=Elapsed-7;MechanicRequire(Target(8)==nearest,"old damage pressure expires");
            repeat.previousDamageTime=Elapsed;repeat.entering=true;MechanicRequire(Target(8)==nearest,"threat cannot target a concealed entrance");

            ProbeReset();cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
            var shooter=ProbeEnemy(0,5);shooter.shooter=true;shooter.radius=6.5f;
            MechanicRequire(EnemyCanAttack(shooter),"visible ranged opponent can begin a shot");
            shooter.rig.Root.position=new Vector3(0,.04f,-7);shooter.physics.Motor.position=shooter.rig.Root.position;Physics.SyncTransforms();
            MechanicRequire(!InCombatView(shooter.rig.Root.position),"ranged test starts below the camera frame");
            shooter.attack=.79f;shooter.engaged=true;shooter.cooldown=0;
            float before=Health;StepEnemies(.02f);
            MechanicRequire(Health==before&&shooter.attack<0&&!shooter.engaged,"offscreen shot cancels before damage");
            MechanicRequire(shooter.repositioning&&shooter.physics.LinearVelocity.z>0,"offscreen enemy walks toward a visible flank");
            for(int frame=0;frame<360;frame++){StepEnemies(1f/60);ProbeFrames(1);}
            MechanicRequire(InCombatView(shooter.rig.Root.position)&&!shooter.repositioning,"enemy physically returns to the combat view");
            ProbeReset();cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
            float line=RearBoundaryZ;var cushion=ProbeEnemy(0,line+1);var outOfBounds=ProbeEnemy(0,line-1);
            MechanicRequire(line<=HeroPosition.z-8&&!BodyInCamera(outOfBounds),"rear boundary sits offscreen with a cushion");
            var pusher=ProbeHost(LoadoutCatalog.Find("ForcePush"));Hurt(outOfBounds,1,Vector3.back,0);
            int beforeKills=Kills,beforeCredit=KillsFor(pusher);float beforeDamage=ContributionFor(pusher).direct;
            ResolveRearBoundary();ResolveRearBoundary();
            MechanicRequire(cushion.deathAge<0&&outOfBounds.deathAge>=0&&Kills==beforeKills+1,"rear cushion survives; crossing dies exactly once");
            MechanicRequire(KillsFor(pusher)==beforeCredit+1&&ContributionFor(pusher).direct==beforeDamage,"ring-out credits the pushing item without fake damage");
            combatRandom=savedCombatRandom;
        }
    }
}
