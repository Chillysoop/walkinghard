using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        const float RevealDuration=6.5f;
        FighterRig pursuer;
        readonly System.Collections.Generic.List<(Enemy enemy,Vector3 origin)> revealFollowers=new System.Collections.Generic.List<(Enemy,Vector3)>();
        bool finaleActive,pausedReveal;
        float revealAge,pursuerGaitClock,pursuerStride;
        Vector3 finaleAnchor,pursuerRevealStart;
        public bool FinaleActive=>finaleActive;
        public float RevealAge=>revealAge;
        Vector3 EncounterForward=>finaleActive?Vector3.back:Vector3.forward;
        int EncounterDirection=>finaleActive?-1:1;
        Vector3 FinaleCameraOffset=>new Vector3(0,13.5f,20);
        Vector3 FinaleCameraLook=>new Vector3(0,3.8f,-7);
        void ResetFinale()
        {
            if(pursuer!=null){pursuer.Root.gameObject.SetActive(false);Destroy(pursuer.Root.gameObject);}
            pursuer=null;finaleActive=pausedReveal=false;revealAge=pursuerGaitClock=pursuerStride=0;
            revealFollowers.Clear();
        }
        void CreatePursuer()
        {
            pursuer=new FighterRig("The Pursuer",actors,false,true,Seed^891,FastSimulation);
            pursuer.Root.localScale=Vector3.one*4.5f;pursuer.Root.position=HeroPosition+Vector3.back*26;
            pursuer.PursuerArmor();pursuer.Pose(0,0,-1,0,0,false,0);
        }
        void StartFinaleReveal()
        {
            finaleActive=true;revealAge=0;finaleAnchor=HeroPosition;State=RunState.Revealing;
            if(pursuer==null)CreatePursuer();pursuerRevealStart=pursuer.Root.position;
            for(int i=0;i<12&&enemies.Count<tuning.maximumEnemies-1;i++)
            {
                SpawnEntry(2,HeroPosition+new Vector3(-5+(i%4)*3.3f,0,-34-(i/4)*4));
                var ally=enemies[enemies.Count-1];revealFollowers.Add((ally,ally.rig.Root.position));
            }
            // Finish the checkpoint with every actor and every combat timer frozen.
            heroPhysics.MovePosition(finaleAnchor);heroPhysics.MoveRotation(Quaternion.identity);
            SetRevealFrame(0);
        }
        void SetRevealFrame(float age)
        {
            if(pursuer!=null)
            {
                // The pursuit is already in motion when the camera first catches him.
                pursuerGaitClock=age*.3f;pursuerStride=.6f;
                pursuer.Root.position=pursuerRevealStart+Vector3.forward*(age*.35f);
                pursuer.Pose(pursuerGaitClock,pursuerStride,-1,0,0,false,0);
            }
            foreach(var follower in revealFollowers)
            {
                Vector3 p=follower.origin+Vector3.forward*(age*.75f);follower.enemy.rig.Root.position=p;
                if(!FastSimulation)follower.enemy.physics.Motor.position=p;
                follower.enemy.rig.Pose(age,1,-1,0,0,false,0);
            }
            float turn=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.45f)/1.1f));
            hero.Root.rotation=Quaternion.Euler(0,180*turn,0);hero.Pose(clock,0,-1,0,0,false,0);
            float orbit=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-1.4f)/4.8f));
            float radius=Mathf.Lerp(-tuning.cameraOffset.z,FinaleCameraOffset.z,orbit);
            Vector3 offset=Quaternion.Euler(0,180*orbit,0)*new Vector3(0,Mathf.Lerp(tuning.cameraOffset.y,FinaleCameraOffset.y,orbit),-radius);
            cameraRig.transform.position=finaleAnchor+offset;
            cameraRig.transform.LookAt(finaleAnchor+Vector3.Lerp(new Vector3(0,1,tuning.lookAhead),FinaleCameraLook,orbit));
        }
        void StepFinaleReveal(float dt)
        {
            if(State!=RunState.Revealing)return;
            revealAge=Mathf.Min(RevealDuration,revealAge+dt);SetRevealFrame(revealAge);
            if(revealAge<RevealDuration)return;
            heroPhysics.MoveRotation(Quaternion.Euler(0,180,0));cameraVelocity=Vector3.zero;
            if(pursuer==null)CreatePursuer();
            var e=new Enemy{id=++nextId,boss=true,heavy=true,health=tuning.enemyHealth*24*(1+.035f*(Wave-1)),cooldown=2.2f,radius=2.55f,rig=pursuer};
            e.maxHealth=e.health;e.physics=new FighterPhysics(e.rig,e.id,this,false,true);
            enemies.Add(e);byId.Add(e.id,e);Spawned++;bossSpawned=true;pursuer=null;
            revealFollowers.Clear();Physics.SyncTransforms();State=RunState.Running;Announce("THE PURSUER");
        }
        void StepPursuer(Enemy e,float dt)
        {
            Vector3 delta=HeroPosition-e.rig.Root.position;delta.y=0;float distance=delta.magnitude;
            e.stun=Mathf.Max(0,e.stun-dt);e.cooldown-=dt;e.react=Mathf.Max(0,e.react-dt*3);
            if(e.stun>0){e.attack=-1;e.delivered=false;}
            Vector3 desired=e.stun>0||e.attack>=0?Vector3.zero:delta.normalized*Mathf.Clamp((distance-2.55f)*2,0,1.05f);
            var velocity=e.physics.LinearVelocity;
            // A slow desired speed still needs enough acceleration to overcome floor friction.
            var planar=new Vector3(velocity.x,0,velocity.z);
            var flat=e.stun>0?Vector3.Lerp(planar,Vector3.zero,1-Mathf.Exp(-1.4f*dt)):Vector3.MoveTowards(planar,desired,16*dt);
            e.physics.LinearVelocity=new Vector3(flat.x,velocity.y,flat.z);
            if(delta.sqrMagnitude>.01f)e.physics.MoveRotation(Quaternion.LookRotation(delta));
            if(e.stun<=0&&e.attack<0&&e.cooldown<=0&&distance<4.5f){e.attack=0;e.delivered=false;}
            if(e.attack>=0)
            {
                e.attack+=dt;
                if(!e.delivered&&e.attack>=1.5f)
                {
                    e.delivered=true;
                    if(distance<4.7f){ReceiveDamage(e,tuning.enemyDamage*1.8f);fx.Ring(HeroPosition,4.5f,VisualFactory.Orange,.6f);}
                }
                if(e.attack>=2.4f){e.attack=-1;e.cooldown=2.2f;}
            }
            // Keep the same stride phase through the reveal-to-combat handoff.
            pursuerGaitClock+=dt*.3f;pursuerStride=Mathf.MoveTowards(pursuerStride,flat.magnitude/1.05f,dt*2);
            e.rig.Pose(pursuerGaitClock,pursuerStride,e.attack>=0?2:-1,e.attack/2.4f,e.react,false,0);
            e.rig.Telegraph(e.attack>=0&&!e.delivered,e.stun>0);StepReaction(e,dt,true);
        }
        Vector3 FlowAroundPursuer(Enemy ally,Vector3 desired)
        {
            var boss=enemies.FirstOrDefault(e=>e.boss);if(boss==null)return desired;
            Vector3 toBoss=boss.rig.Root.position-ally.rig.Root.position;toBoss.y=0;
            if(toBoss.z< -1||toBoss.z>9||Mathf.Abs(toBoss.x)>3.5f)return desired;
            float side=Mathf.Abs(toBoss.x)>.3f?-Mathf.Sign(toBoss.x):ally.id%2==0?1:-1;
            Vector3 goal=boss.rig.Root.position+new Vector3(side*3.4f,0,1.5f);
            Vector3 around=goal-ally.rig.Root.position;around.y=0;
            return around.normalized*Mathf.Max(tuning.enemySpeed,desired.magnitude);
        }
        internal void PrepareFinaleReview()
        {
            Begin(true);OpeningPicks=2;Wave=8;qaHolding=true;tuning.waveCount=8;
            hero.Root.position=new Vector3(0,.04f,200);heroPhysics.Motor.position=hero.Root.position;
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
            Deck.Move(Deck.Acquire(LoadoutCatalog.Find("sword")),PlaceKind.Weapon,0);
            foreach(string id in new[]{"MassDriver","ArcBolt","Breaker"})Deck.Move(Deck.Acquire(LoadoutCatalog.Find(id)),PlaceKind.Sequence,Deck.Count+2);
            BeginWave();
        }
    }
    public sealed partial class PrototypeHUD
    {
        void FinaleReveal(float w,float h)
        {
            Box(new Rect(0,0,w,56),Color.black);Box(new Rect(0,h-70,w,70),Color.black);
            if(game.RevealAge>4.2f)Label(new Rect(0,h-59,w,34),"THE PURSUER",27,white,true,TextAnchor.MiddleCenter);
            if(Button(new Rect(w-180,14,154,28),"PAUSE [ SPACE ]"))game.TogglePause();
        }
    }
}
