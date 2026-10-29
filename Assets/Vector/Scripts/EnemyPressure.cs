using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        const float ThreatMemory=6f;
        public int RangedSpawned {get;private set;}
        public int RangedHits {get;private set;}
        public float RangedDamage {get;private set;}
        int waveRangedSpawned,waveRangedHits;
        float waveRangedDamage;

        static float ShooterChance(int wave)=>wave<2?0:wave%3==2?.15f:.075f;

        bool InCombatView(Vector3 position)
        {
            // Reserve the bottom strip occupied by the action sequence. This uses
            // the real chase camera in rendered play and in engine-only sims.
            var view=cameraRig.WorldToViewportPoint(position+Vector3.up*1.2f);
            return view.z>cameraRig.nearClipPlane&&view.x>.025f&&view.x<.975f&&view.y>.20f&&view.y<.98f;
        }
        bool ClearEnemyShot(Enemy e)=>FastSimulation||!Physics.Linecast(e.rig.Root.position+Vector3.up*1.2f,HeroPosition+Vector3.up*1.2f,1<<EntryLayout.WorldLayer);
        bool EnemyCanAttack(Enemy e)=>!e.entering&&!e.repositioning&&InCombatView(e.rig.Root.position)&&(!e.shooter||ClearEnemyShot(e));

        void UpdateEnemyRepositioning(Enemy e)
        {
            if(e.entering)return;
            var p=e.cachedPosition;float distance=Vector3.Distance(p,HeroPosition);
            if(!InCombatView(p)&&distance<15||p.z<HeroPosition.z-1.8f)e.repositioning=true;
            else if(e.repositioning&&InCombatView(p)&&p.z>HeroPosition.z+.4f)e.repositioning=false;
        }
        Vector3 RepositionVelocity(Enemy e)
        {
            // Walk back into view through a forward flank; never warp an actor.
            float side=e.cachedPosition.x>=HeroPosition.x?1:-1;
            Vector3 goal=HeroPosition+new Vector3(side*(e.shooter?4.2f:1.8f),0,e.shooter?5:2.4f);
            Vector3 direction=goal-e.cachedPosition;direction.y=0;
            return direction.normalized*tuning.enemySpeed;
        }
        void RecordEnemyPressure(Enemy e,float damage)
        {
            if(e==null||damage<=0)return;
            e.pressure=e.pressure*Mathf.Exp(-(Elapsed-e.lastDamageTime)/ThreatMemory)+damage;
            e.previousDamageTime=e.lastDamageTime;e.lastDamageTime=Elapsed;
            if(e.shooter){RangedHits++;RangedDamage+=damage;}
        }
        float RecentEnemyPressure(Enemy e)=>e.previousDamageTime>=Elapsed-ThreatMemory?e.pressure*Mathf.Exp(-(Elapsed-e.lastDamageTime)/ThreatMemory):0;
    }
}
