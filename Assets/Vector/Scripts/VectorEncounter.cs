using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace VectorGame
{
    public sealed class VectorEncounter : MonoBehaviour
    {
        public enum RunState { Ready, Running, Paused, Complete, Defeated }
        public CombatTuning tuning;
        public RunState State {get;private set;}=RunState.Ready;
        public float Elapsed {get;private set;}
        public float Health {get;private set;}
        public int Kills {get;private set;}
        public int PeakCrowd {get;private set;}
        public int ComboFinishers {get;private set;}
        public int Pushes {get;private set;}
        public int HitsTaken {get;private set;}
        public int Spawned {get;private set;}
        public int StrikesLanded {get;private set;}
        public int PushTargets {get;private set;}
        public float PushCharge=>1-Mathf.Clamp01(pushCooldown/tuning.pushCooldown);
        public int EnemyCount=>enemies.Count;
        public int ComboStep=>strike>=0?strike:0;
        public bool IsAttacking=>strike>=0;
        public bool AutoPush {get;private set;}=true;
        public bool ReducedMotion {get;private set;}
        public bool Muted=>fx!=null&&fx.muted;
        public string EventText {get;private set;}="WALK INTO THE STORM";
        public float EventAge {get;private set;}
        public string StageName=>Elapsed<20?"01 / FIRST CONTACT":Elapsed<40?"02 / CLOSING IN":"03 / HOLD THE LINE";
        public Vector3 HeroPosition=>hero.Root.position;
        public Camera GameCamera=>cameraRig;

        sealed class Enemy
        {
            public int id;
            public FighterRig rig;
            public Vector3 velocity,impulse;
            public float health,stun,cooldown,attack=-1,deathAge=-1,orbitDirection,radius,phase,hitReact;
            public bool engaged,heavy,hitDelivered;
        }
        readonly List<Enemy> enemies=new List<Enemy>();
        readonly List<Enemy> corpses=new List<Enemy>();
        FighterRig hero;
        CombatEffects fx;
        Camera cameraRig;
        Transform actors;
        Vector3 cameraVelocity;
        float spawnTimer=.8f,clock,strikeAge,comboWait,hitReact,pushCooldown=2,castAge=-1,shake,flash,tickAccumulator;
        int strike=-1,nextEnemyId;
        bool strikeDelivered,castDelivered,qa,qaReported;
        string qaPath;
        Enemy strikeTarget;
        public float DamageFlash=>flash;
        public bool IsCasting=>castAge>=0;
        public float CrowdNear {get;private set;}
        public float MinSeparation {get;private set;}=99;

        void Awake()
        {
            if(tuning==null)tuning=Resources.Load<CombatTuning>("CombatTuning");
            if(tuning==null)tuning=ScriptableObject.CreateInstance<CombatTuning>();
            var args=Environment.GetCommandLineArgs();qa=Array.IndexOf(args,"-vectorQA")>=0;
            for(int i=0;i<args.Length-1;i++){if(args[i]=="-qaOutput")qaPath=args[i+1];if(args[i]=="-seed"&&int.TryParse(args[i+1],out int seed))tuning.seed=seed;}
            Random.InitState(tuning.seed);
            Application.targetFrameRate=qa?-1:60;QualitySettings.vSyncCount=qa?0:1;
            Time.fixedDeltaTime=1f/60f;
            VisualFactory.Initialize();
            var world=VisualFactory.Node("The approach | environment",transform,Vector3.zero);VisualFactory.World(world);
            actors=VisualFactory.Node("Fighters",transform,Vector3.zero);
            hero=new FighterRig("The Walker",actors,true);
            fx=new CombatEffects(VisualFactory.Node("Combat effects",transform,Vector3.zero),qa);
            cameraRig=new GameObject("Elevated chase camera").AddComponent<Camera>();cameraRig.tag="MainCamera";
            cameraRig.transform.SetParent(transform);cameraRig.fieldOfView=tuning.cameraFov;cameraRig.nearClipPlane=.1f;cameraRig.farClipPlane=160;cameraRig.backgroundColor=new Color(.10f,.17f,.20f);cameraRig.clearFlags=CameraClearFlags.SolidColor;
            cameraRig.allowHDR=true;cameraRig.gameObject.AddComponent<AudioListener>();
            cameraRig.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            cameraRig.transform.position=hero.Root.position+tuning.cameraOffset;AimCamera();
            Health=tuning.heroHealth;hero.Pose(0,0,-1,0,0,false,0);
            gameObject.AddComponent<PrototypeHUD>().game=this;
            if(qa)Begin();
        }
        public void Begin()
        {
            foreach(var e in enemies)Destroy(e.rig.Root.gameObject);foreach(var e in corpses)Destroy(e.rig.Root.gameObject);enemies.Clear();corpses.Clear();fx.Clear();
            Random.InitState(tuning.seed);Elapsed=clock=tickAccumulator=0;Health=tuning.heroHealth;Kills=PeakCrowd=ComboFinishers=Pushes=HitsTaken=Spawned=StrikesLanded=PushTargets=0;
            spawnTimer=.4f;pushCooldown=1.5f;castAge=-1;strike=-1;strikeAge=comboWait=hitReact=flash=shake=0;nextEnemyId=0;strikeTarget=null;
            hero.Root.position=Vector3.zero;hero.Root.rotation=Quaternion.identity;cameraRig.transform.position=tuning.cameraOffset;cameraVelocity=Vector3.zero;
            State=RunState.Running;EventText="FIRST CONTACT";EventAge=0;qaReported=false;
            // Opening group is spaced in depth and width, not in fixed combat cells.
            Spawn(new Vector3(-1.9f,0,8.5f),false);Spawn(new Vector3(2.3f,0,10.5f),false);Spawn(new Vector3(.4f,0,12),false);
        }
        public void TogglePause(){if(State==RunState.Running)State=RunState.Paused;else if(State==RunState.Paused)State=RunState.Running;}
        public void ToggleAutoPush(){AutoPush=!AutoPush;}
        public void ToggleMotion(){ReducedMotion=!ReducedMotion;}
        public void ToggleSound(){fx.muted=!fx.muted;}
        public void Quit(){Application.Quit();}
        void Update()
        {
            if(!qa)
            {
                if(Input.GetKeyDown(KeyCode.Return)&&(State==RunState.Ready||State==RunState.Complete||State==RunState.Defeated))Begin();
                if(Input.GetKeyDown(KeyCode.R))Begin();
                if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape))TogglePause();
                if(Input.GetKeyDown(KeyCode.F))RequestPush();
                if(Input.GetKeyDown(KeyCode.A))ToggleAutoPush();
                if(Input.GetKeyDown(KeyCode.M))ToggleSound();
                if(Input.GetKeyDown(KeyCode.F8))Capture();
            }
            if(State==RunState.Running)
            {
                const float fixedStep=1f/60f;
                if(qa)
                {
                    int steps=Application.isBatchMode?8:1;
                    for(int i=0;i<steps&&State==RunState.Running;i++)Step(fixedStep);
                }
                else
                {
                    tickAccumulator+=Mathf.Min(Time.deltaTime,.1f);
                    while(tickAccumulator>=fixedStep&&State==RunState.Running){Step(fixedStep);tickAccumulator-=fixedStep;}
                }
            }
            if(State==RunState.Ready){clock+=Time.deltaTime;hero.Pose(clock,.05f,-1,0,0,false,0);}
            if(State==RunState.Complete||State==RunState.Defeated)
            {
                fx.Step(Mathf.Min(Time.deltaTime,.04f));
                if(qa&&!qaReported)ReportQA();
            }
        }
        void Step(float dt)
        {
            clock+=dt;Elapsed+=dt;EventAge+=dt;flash=Mathf.Max(0,flash-dt*2);shake=Mathf.Max(0,shake-dt*2);hitReact=Mathf.Max(0,hitReact-dt*4);
            pushCooldown=Mathf.Max(0,pushCooldown-dt);
            // Constant forward intent, with subtle weight shift during strikes.
            hero.Root.position+=Vector3.forward*tuning.walkSpeed*dt*(castAge>=0?.65f:1f);
            spawnTimer-=dt;
            if(spawnTimer<=0&&Elapsed<tuning.duration-5&&enemies.Count<tuning.maximumEnemies)
            {
                float side=Random.Range(-7.1f,7.1f);
                Spawn(hero.Root.position+new Vector3(side,0,Random.Range(11f,17f)),Elapsed>18&&Random.value<.18f);
                spawnTimer=Mathf.Lerp(tuning.firstSpawnInterval,tuning.finalSpawnInterval,Elapsed/tuning.duration)*Random.Range(.8f,1.15f);
                if(Elapsed>32&&Random.value<.28f)Spawn(hero.Root.position+new Vector3(Random.value<.5f?-7.5f:7.5f,0,Random.Range(1f,5f)),false);
            }
            StepEnemies(dt);
            int close=0;foreach(var e in enemies)if((e.rig.Root.position-hero.Root.position).sqrMagnitude<tuning.pushRange*tuning.pushRange)close++;
            CrowdNear=close;PeakCrowd=Mathf.Max(PeakCrowd,close);
            if(AutoPush&&pushCooldown<=0&&castAge<0&&(close>=tuning.pushCrowdThreshold||(Health<tuning.heroHealth*.35f&&close>=2)))RequestPush();
            StepHero(dt);
            fx.Step(dt);StepCorpses(dt);UpdateCamera(dt);
            if(Health<=0){Health=0;State=RunState.Defeated;EventText="OVERRUN";EventAge=0;}
            else if(Elapsed>=tuning.duration){Elapsed=tuning.duration;State=RunState.Complete;EventText="THE WALK CONTINUES";EventAge=0;}
            if(!Finite(hero.Root.position))throw new InvalidOperationException("Non-finite hero position");
        }
        static bool Finite(Vector3 p)=>!(float.IsNaN(p.x)||float.IsInfinity(p.x)||float.IsNaN(p.z)||float.IsInfinity(p.z));
        void Spawn(Vector3 position,bool heavy)
        {
            if(enemies.Count>=tuning.maximumEnemies)return;
            var e=new Enemy{id=++nextEnemyId,rig=new FighterRig(heavy?"Enforcer":"Raider",actors,false,heavy),heavy=heavy,health=tuning.enemyHealth*(heavy?1.9f:1f),cooldown=Random.Range(.2f,1.3f),orbitDirection=Random.value<.5f?-1:1,radius=Random.Range(2.4f,3.6f),phase=Random.value*6.28f};
            e.rig.Root.position=position;e.rig.Root.rotation=Quaternion.Euler(0,180,0);enemies.Add(e);Spawned++;
        }
        void StepEnemies(float dt)
        {
            int committed=0;foreach(var e in enemies)if(e.engaged)committed++;
            foreach(var e in enemies)
            {
                var p=e.rig.Root.position;Vector3 delta=hero.Root.position-p;delta.y=0;float distance=delta.magnitude;Vector3 inward=distance>.01f?delta/distance:Vector3.back;
                e.cooldown-=dt;e.stun=Mathf.Max(0,e.stun-dt);e.hitReact=Mathf.Max(0,e.hitReact-dt*4);
                if(e.stun>0){e.engaged=false;e.attack=-1;}
                if(!e.engaged&&e.stun<=0&&e.cooldown<=0&&distance<3.2f&&committed<tuning.simultaneousAttackers)
                {
                    bool opening=true;
                    foreach(var other in enemies)if(other!=e&&other.engaged&&Vector3.Angle(other.rig.Root.position-hero.Root.position,p-hero.Root.position)<32){opening=false;break;}
                    if(opening){e.engaged=true;committed++;}
                }
                Vector3 desired;
                if(e.stun>0)desired=Vector3.zero;
                else if(e.engaged)desired=inward*Mathf.Clamp((distance-1.32f)*4,-1.2f,tuning.enemySpeed);
                else if(distance>e.radius+.75f)desired=inward*tuning.enemySpeed;
                else
                {
                    Vector3 tangent=new Vector3(-inward.z,0,inward.x)*e.orbitDirection;
                    float breathing=.18f*Mathf.Sin(clock*1.5f+e.phase);
                    desired=inward*Mathf.Clamp((distance-e.radius-breathing)*2,-1.8f,1.8f)+tangent*(.7f+.25f*Mathf.Sin(clock+e.phase));
                }
                if(e.heavy)desired*=.82f;
                Vector3 separation=Vector3.zero;
                foreach(var other in enemies)
                {
                    if(other==e)continue;var diff=p-other.rig.Root.position;diff.y=0;float sq=diff.sqrMagnitude;
                    if(sq<1.35f*1.35f&&sq>.0001f){float d=Mathf.Sqrt(sq);separation+=diff/d*(1.35f-d)*3.6f;}
                }
                desired+=separation;
                if(distance<1.05f)desired-=inward*(1.05f-distance)*9;
                e.velocity=Vector3.Lerp(e.velocity,desired,1-Mathf.Exp(-8*dt));
                e.impulse=Vector3.Lerp(e.impulse,Vector3.zero,1-Mathf.Exp(-3.1f*dt));
                if(e.attack>=0)e.velocity*=.88f;
                e.rig.Root.position+=(e.velocity+e.impulse)*dt;
                var bounded=e.rig.Root.position;bounded.x=Mathf.Clamp(bounded.x,-8.3f,8.3f);bounded.y=0;e.rig.Root.position=bounded;
                Vector3 facing=distance<4.5f?inward:e.velocity.normalized;
                if(facing.sqrMagnitude>.05f)e.rig.Root.rotation=Quaternion.Slerp(e.rig.Root.rotation,Quaternion.LookRotation(facing),1-Mathf.Exp(-9*dt));
                if(e.engaged&&distance<tuning.attackRadius&&e.attack<0){e.attack=0;e.hitDelivered=false;}
                if(e.attack>=0)
                {
                    e.attack+=dt;float duration=e.heavy?1.25f:.95f;
                    if(!e.hitDelivered&&e.attack>=duration*.52f)
                    {
                        e.hitDelivered=true;
                        if(e.stun<=0&&Vector3.Distance(e.rig.Root.position,hero.Root.position)<tuning.attackRadius+.25f)
                        {Health-=tuning.enemyDamage*(e.heavy?1.8f:1f);HitsTaken++;hitReact=.8f;flash=.35f;fx.Play(3);fx.Burst(hero.Root.position+Vector3.up*1.2f,VisualFactory.Orange,3,2);}
                    }
                    if(e.attack>=duration){e.attack=-1;e.engaged=false;e.cooldown=Random.Range(.8f,1.8f);}
                }
                e.rig.Pose(clock,e.velocity.magnitude/tuning.enemySpeed,e.attack>=0?(e.heavy?2:1):-1,e.attack/(e.heavy?1.25f:.95f),e.hitReact,false,0);
                if(!Finite(e.rig.Root.position))throw new InvalidOperationException("Non-finite enemy position");
            }
            // Resolve residual overlap symmetrically, preserving free continuous movement.
            for(int i=0;i<enemies.Count;i++)for(int j=i+1;j<enemies.Count;j++)
            {
                var a=enemies[i];var b=enemies[j];Vector3 delta=a.rig.Root.position-b.rig.Root.position;float d=delta.magnitude;
                if(d>.001f&&d<.64f){var correction=delta/d*(.64f-d)*.5f;a.rig.Root.position+=correction;b.rig.Root.position-=correction;}
            }
        }
        Enemy Target(float range)
        {
            Enemy best=null;float score=float.MaxValue;
            foreach(var e in enemies){float d=Vector3.Distance(e.rig.Root.position,hero.Root.position);if(d>range)continue;float s=d-(e.attack>=0?.6f:0)-(e.stun>0?.1f:0);if(s<score){score=s;best=e;}}
            return best;
        }
        void StepHero(float dt)
        {
            if(castAge>=0)
            {
                castAge+=dt;
                if(!castDelivered&&castAge>=.3f){castDelivered=true;ReleasePush();}
                hero.Pose(clock,.3f,-1,0,hitReact,true,castAge/.75f);
                if(castAge>=.75f)castAge=-1;
                return;
            }
            comboWait-=dt;
            if(strike<0&&comboWait<=0)
            {
                strikeTarget=Target(tuning.meleeReach);
                if(strikeTarget!=null){strike=0;strikeAge=0;strikeDelivered=false;}
            }
            float duration=strike==0?.43f:strike==1?.48f:.67f;
            if(strike>=0)
            {
                if(strikeTarget==null||!enemies.Contains(strikeTarget))strikeTarget=Target(tuning.meleeReach+.3f);
                if(strikeTarget!=null)
                {
                    var aim=strikeTarget.rig.Root.position-hero.Root.position;aim.y=0;
                    if(aim.sqrMagnitude>.01f)hero.Root.rotation=Quaternion.Slerp(hero.Root.rotation,Quaternion.LookRotation(aim),1-Mathf.Exp(-17*dt));
                }
                strikeAge+=dt;
                if(!strikeDelivered&&strikeAge>=duration*.43f){strikeDelivered=true;Strike();}
                if(strikeAge>=duration)
                {
                    if(strike<2){strike++;strikeAge=0;strikeDelivered=false;strikeTarget=Target(tuning.meleeReach+.3f);}
                    else{strike=-1;comboWait=tuning.comboRecovery;}
                }
            }
            else hero.Root.rotation=Quaternion.Slerp(hero.Root.rotation,Quaternion.identity,1-Mathf.Exp(-4*dt));
            hero.Pose(clock,1,strike,strikeAge/duration,hitReact,false,0);
        }
        void Strike()
        {
            if(strike<2)
            {
                if(strikeTarget==null||!enemies.Contains(strikeTarget)||Vector3.Distance(strikeTarget.rig.Root.position,hero.Root.position)>tuning.meleeReach+.3f)return;
                Vector3 point=strikeTarget.rig.Root.position+Vector3.up*1.4f;
                fx.Arc((strike==0?hero.LeftHand:hero.RightHand).position,point,VisualFactory.Cyan);fx.Burst(point,VisualFactory.Cyan,5,2.4f);fx.Play(0);
                Hurt(strikeTarget,strike==0?tuning.jabDamage:tuning.crossDamage,hero.Root.forward*1.5f,.22f);StrikesLanded++;
                EventText=strike==0?"01 / JAB":"02 / CROSS";EventAge=0;shake=.055f;
            }
            else
            {
                int hit=0;
                for(int i=enemies.Count-1;i>=0;i--){var e=enemies[i];var delta=e.rig.Root.position-hero.Root.position;if(delta.magnitude<tuning.meleeReach+.65f&&Vector3.Dot(delta.normalized,hero.Root.forward)>-.1f){fx.Burst(e.rig.Root.position+Vector3.up*1.15f,VisualFactory.Cyan,7,3.5f);Hurt(e,tuning.finisherDamage,delta.normalized*5.6f,.8f);hit++;}}
                if(hit>0){StrikesLanded+=hit;ComboFinishers++;fx.Play(1);fx.Ring(hero.Root.position,2.7f,VisualFactory.Cyan,.3f);shake=.15f;EventText="03 / BREAKER";EventAge=0;}
            }
        }
        public void RequestPush()
        {
            if(State!=RunState.Running||pushCooldown>0||castAge>=0)return;
            castAge=0;castDelivered=false;pushCooldown=tuning.pushCooldown;strike=-1;comboWait=.2f;
            EventText="FORCE PUSH";EventAge=0;
        }
        void ReleasePush()
        {
            Pushes++;fx.Play(2);fx.Ring(hero.Root.position,tuning.pushRange,VisualFactory.Cyan,.6f);fx.Ring(hero.Root.position+Vector3.up*.4f,tuning.pushRange*.92f,new Color(.6f,1,.86f),.45f);fx.Burst(hero.Root.position+Vector3.up,VisualFactory.Cyan,16,5);shake=.27f;
            for(int i=enemies.Count-1;i>=0;i--)
            {
                var e=enemies[i];var delta=e.rig.Root.position-hero.Root.position;float d=delta.magnitude;
                if(d<=tuning.pushRange){PushTargets++;Hurt(e,tuning.pushDamage,delta.normalized*tuning.pushImpulse*(e.heavy?.65f:1),1.15f);}
            }
        }
        void Hurt(Enemy e,float damage,Vector3 impulse,float stun)
        {
            e.health-=damage;e.impulse+=impulse;e.stun=Mathf.Max(e.stun,stun);e.hitReact=1;e.engaged=false;e.attack=-1;
            if(e.health<=0)
            {
                enemies.Remove(e);e.deathAge=0;e.velocity=impulse*.65f;corpses.Add(e);Kills++;Health=Mathf.Min(tuning.heroHealth,Health+.4f);
                if(corpses.Count>22){Destroy(corpses[0].rig.Root.gameObject);corpses.RemoveAt(0);}
            }
        }
        void StepCorpses(float dt)
        {
            for(int i=corpses.Count-1;i>=0;i--)
            {
                var e=corpses[i];e.deathAge+=dt;e.velocity=Vector3.Lerp(e.velocity,Vector3.zero,dt*4);e.rig.Root.position+=e.velocity*dt;
                float fall=Mathf.Clamp01(e.deathAge/.38f);e.rig.Root.rotation=Quaternion.Slerp(e.rig.Root.rotation,Quaternion.Euler(78,e.rig.Root.eulerAngles.y,17*e.orbitDirection),dt*12);
                var p=e.rig.Root.position;p.y=-.05f-fall*.1f;e.rig.Root.position=p;
                if(e.deathAge>2.2f)e.rig.Root.localScale*=Mathf.Max(0,1-dt*3.5f);
                if(e.deathAge>3.2f){Destroy(e.rig.Root.gameObject);corpses.RemoveAt(i);}
            }
        }
        void UpdateCamera(float dt)
        {
            var goal=hero.Root.position+tuning.cameraOffset;
            cameraRig.transform.position=Vector3.SmoothDamp(cameraRig.transform.position,goal,ref cameraVelocity,.22f,100,dt);
            if(!ReducedMotion)cameraRig.transform.position+=new Vector3(Mathf.Sin(clock*59),Mathf.Sin(clock*71),0)*shake*.12f;
            cameraRig.fieldOfView=Mathf.Lerp(cameraRig.fieldOfView,tuning.cameraFov+Mathf.Min(2,CrowdNear*.1f),dt*2);
            AimCamera();
        }
        void AimCamera(){cameraRig.transform.LookAt(hero.Root.position+new Vector3(0,1,tuning.lookAhead));}
        public void Capture()
        {
            string dir=Path.Combine(Application.persistentDataPath,"Captures");Directory.CreateDirectory(dir);
            string path=Path.Combine(dir,"VECTOR-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png");ScreenCapture.CaptureScreenshot(path);Debug.Log("VECTOR screenshot: "+path);
        }
        [Serializable] class QAReport
        { public string result;public int seed;public float elapsed,health;public int spawned,kills,alive,peakCrowd,strikes,finishers,pushes,pushTargets,hitsTaken;public bool passed;public string platform; }
        void ReportQA()
        {
            qaReported=true;
            var r=new QAReport{result=State.ToString(),seed=tuning.seed,elapsed=Elapsed,health=Health,spawned=Spawned,kills=Kills,alive=enemies.Count,peakCrowd=PeakCrowd,strikes=StrikesLanded,finishers=ComboFinishers,pushes=Pushes,pushTargets=PushTargets,hitsTaken=HitsTaken,platform=Application.platform.ToString()};
            r.passed=State==RunState.Complete&&Kills>=15&&ComboFinishers>=5&&Pushes>=3&&PushTargets>=10&&PeakCrowd>=4&&HitsTaken>0&&Spawned==Kills+enemies.Count;
            string path=string.IsNullOrEmpty(qaPath)?Path.Combine(Application.persistentDataPath,"qa-report.json"):qaPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(r,true));Debug.Log("VECTOR_QA "+JsonUtility.ToJson(r));
            Application.Quit(r.passed?0:2);
        }
    }
}
