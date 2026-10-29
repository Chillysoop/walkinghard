using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public int RearBoundaryKills {get;private set;}
        public int EnvironmentalKills {get;private set;}
        public float RearBoundaryZ
        {
            get
            {
                var ray=cameraRig.ViewportPointToRay(new Vector3(.5f,0,0));
                var ground=new Plane(Vector3.up,Vector3.zero);
                float bottom=ground.Raycast(ray,out float distance)?ray.GetPoint(distance).z:HeroPosition.z-7;
                return Mathf.Min(HeroPosition.z-8,bottom-3);
            }
        }
        bool BodyInCamera(Enemy e)
        {
            // Airborne heads must leave the image too; a ring-out never pops a
            // visibly flying enemy out of the fight.
            foreach(float height in new[]{.3f,1.2f,2.5f})
            {
                var view=cameraRig.WorldToViewportPoint(e.rig.Root.position+Vector3.up*height);
                if(view.z>0&&view.x>=0&&view.x<=1&&view.y>=0&&view.y<=1)return true;
            }
            return false;
        }
        void RememberDisplacement(Enemy e,Vector3 impulse)
        {
            if(currentCredit==null||impulse.sqrMagnitude<.001f)return;
            e.lastPushCredit=new KillCredit{item=currentCredit.item,weapon=currentCredit.weapon,action=currentCredit.action,hop=currentCredit.hop,derived=currentCredit.derived,augment=currentCredit.augment};
        }
        void ResolveRearBoundary()
        {
            float line=RearBoundaryZ;
            foreach(var e in enemies.ToArray())
            {
                if(e.entering||e.deathAge>=0||e.rig.Root.position.z>=line||BodyInCamera(e))continue;
                var saved=currentCredit;currentCredit=e.lastPushCredit;
                e.health=0;KillEnemy(e,Vector3.zero,false);RearBoundaryKills++;currentCredit=saved;
            }
        }
        void KillEnemy(Enemy e,Vector3 impulse,bool sever)
        {
            if(e.deathAge>=0)return;
            enemies.Remove(e);e.deathAge=0;e.physics.Die(impulse);corpses.Add(e);Kills++;Ragdolls++;
            SetBodyWalkCollision(e,flights.Exists(f=>f.body==e));
            if(sever){e.physics.Sever(e.reaction);Severs++;fx.Burst(e.physics.Center,VisualFactory.Orange,10,3);}
            var owner=currentCredit==null?null:KillOwner(currentCredit.weapon??currentCredit.item);
            if(owner!=null)itemKills[owner]=KillsFor(owner)+1;else EnvironmentalKills++;
            Score.Kill(e.heavy?200:100,currentCredit?.action??-e.id,currentCredit?.hop??0);
            DeathSignatures(e,sever);
        }
    }
}
