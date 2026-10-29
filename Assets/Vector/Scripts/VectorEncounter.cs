using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VectorGame
{
    public sealed class VectorEncounter : MonoBehaviour
    {
        public enum RunState { Ready, Running, Paused, Checkpoint, Complete, Defeated }
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
        public float PushCharge=>1-Mathf.Clamp01(pushCooldown/(tuning.pushCooldown*Draft.FieldCooldown));
        public UpgradeDraft Draft {get;private set;}
        public int Wave {get;private set;}=1;
        public float WaveElapsed {get;private set;}
        public bool CleaningUp=>WaveElapsed>=tuning.waveArrivals;
        public float WaveRemaining=>Mathf.Max(0,(CleaningUp?tuning.waveArrivals+tuning.cleanupWindow:tuning.waveArrivals)-WaveElapsed);
        public int Launches {get;private set;}
        public int CollisionHits {get;private set;}
        public int CollisionKills {get;private set;}
        public float CollisionDamage {get;private set;}
        public string Highlight {get;private set;}="";
        public float HighlightAge {get;private set;}=99;
        public readonly List<WaveReport> Waves=new List<WaveReport>();
        public int EnemyCount=>enemies.Count;
        public int ComboStep=>strike>=0?strike:0;
        public bool IsAttacking=>strike>=0;
        public bool AutoPush {get;private set;}=true;
        public bool ReducedMotion {get;private set;}
        public bool Muted=>fx!=null&&fx.muted;
        public string EventText {get;private set;}="WALK INTO THE STORM";
        public float EventAge {get;private set;}
        public string StageName=>Wave==1?"FIRST CONTACT":Wave==2?"MAKE SPACE":"THE CRUSH";
        public string NextThreat=>Wave==1?"More rushers, tighter groups. A launch opens a path through them.":"Dense groups and heavy enforcers. Heavy bodies resist displacement.";
        public Vector3 HeroPosition=>hero.Root.position;
        public Camera GameCamera=>cameraRig;

        sealed class Enemy
        {
            public int id;
            public FighterRig rig;
            public Vector3 velocity,impulse;
            public float health,stun,cooldown,attack=-1,deathAge=-1,orbitDirection,radius,phase,hitReact,flightTime;
            public bool engaged,heavy,hitDelivered;
        }
        readonly List<Enemy> enemies=new List<Enemy>();
        readonly List<Enemy> corpses=new List<Enemy>();
        sealed class Flight
        {
            public Enemy body;public Vector3 previous;public float age,trailTimer;public bool armed;
            public readonly HashSet<int> hitIds=new HashSet<int>();
        }
        readonly List<Flight> flights=new List<Flight>();
        System.Random spawnRandom,combatRandom;
        FighterRig hero;
        CombatEffects fx;
        Camera cameraRig;
        Transform actors;
        Vector3 cameraVelocity;
        float spawnTimer=.8f,clock,strikeAge,comboWait,hitReact,pushCooldown=2,castAge=-1,shake,flash,tickAccumulator,hitStop;
        int strike=-1,nextEnemyId;
        bool strikeDelivered,castDelivered,qa,qaReported;
        string qaPath,qaBuild="pinball";
        int waveStartKills,waveStartLaunches,waveStartCollisions,waveStartHits,waveStartPushes,waveStartRerolls;
        float waveStartHealth;
        bool qaCheckpointChecks=true;
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
            for(int i=0;i<args.Length-1;i++){if(args[i]=="-qaOutput")qaPath=args[i+1];if(args[i]=="-qaBuild")qaBuild=args[i+1];if(args[i]=="-seed"&&int.TryParse(args[i+1],out int seed))tuning.seed=seed;}
            Draft=new UpgradeDraft(tuning.seed);
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
            spawnRandom=new System.Random(tuning.seed);combatRandom=new System.Random(tuning.seed^6179);Draft=new UpgradeDraft(tuning.seed);
            Elapsed=clock=tickAccumulator=hitStop=0;Health=tuning.heroHealth;Kills=PeakCrowd=ComboFinishers=Pushes=HitsTaken=Spawned=StrikesLanded=PushTargets=0;
            Wave=1;WaveElapsed=0;Launches=CollisionHits=CollisionKills=0;CollisionDamage=0;Waves.Clear();flights.Clear();HighlightAge=99;AutoPush=true;
            waveStartKills=waveStartLaunches=waveStartCollisions=waveStartHits=waveStartPushes=waveStartRerolls=0;waveStartHealth=Health;qaCheckpointChecks=true;
            spawnTimer=.4f;pushCooldown=1.5f;castAge=-1;strike=-1;strikeAge=comboWait=hitReact=flash=shake=0;nextEnemyId=0;strikeTarget=null;
            hero.Root.position=Vector3.zero;hero.Root.rotation=Quaternion.identity;hero.Equipment(false,false);cameraRig.transform.position=tuning.cameraOffset;cameraVelocity=Vector3.zero;
            State=RunState.Running;EventText="FIRST CONTACT";EventAge=0;qaReported=false;
            // Opening group is spaced in depth and width, not in fixed combat cells.
            Spawn(new Vector3(-1.9f,0,8.5f),false);Spawn(new Vector3(2.3f,0,10.5f),false);Spawn(new Vector3(.4f,0,12),false);
        }
        public void TogglePause(){if(State==RunState.Running)State=RunState.Paused;else if(State==RunState.Paused)State=RunState.Running;}
        public void ToggleAutoPush(){AutoPush=!AutoPush;}
        public void ToggleMotion(){ReducedMotion=!ReducedMotion;}
        public void ToggleSound(){fx.muted=!fx.muted;}
        public void Quit(){Application.Quit();}
        public void SelectUpgrade(int index){if(State==RunState.Checkpoint)Draft.Select(index);}
        public void Reroll(){if(State==RunState.Checkpoint)Draft.Reroll();}
        public void ConfirmUpgrade()
        {
            if(State!=RunState.Checkpoint)return;
            var card=Draft.Install();if(card==null)return;
            if(card.id==UpgradeId.RepairWeave)Health=Mathf.Min(tuning.heroHealth,Health+55);
            hero.Equipment(Draft.Launch,Draft.Pinball);
            Waves[Waves.Count-1].choice=card.title;
            Wave++;WaveElapsed=0;spawnTimer=.7f;tickAccumulator=hitStop=0;
            waveStartKills=Kills;waveStartLaunches=Launches;waveStartCollisions=CollisionHits;waveStartHits=HitsTaken;waveStartPushes=Pushes;waveStartHealth=Health;waveStartRerolls=Draft.Rerolls;
            State=RunState.Running;Announce(card.title.ToUpperInvariant()+" INSTALLED");
        }
        void Announce(string text){Highlight=text;HighlightAge=0;}
        float CombatRange(float min,float max)=>Mathf.Lerp(min,max,(float)combatRandom.NextDouble());
        float SpawnRange(float min,float max)=>Mathf.Lerp(min,max,(float)spawnRandom.NextDouble());
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
                if(State==RunState.Checkpoint)
                {
                    if(Input.GetKeyDown(KeyCode.Alpha1))SelectUpgrade(0);
                    if(Input.GetKeyDown(KeyCode.Alpha2))SelectUpgrade(1);
                    if(Input.GetKeyDown(KeyCode.Alpha3))SelectUpgrade(2);
                    if(Input.GetKeyDown(KeyCode.Tab))Reroll();
                    if(Input.GetKeyDown(KeyCode.Return))ConfirmUpgrade();
                }
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
                    // Presentation hit-stop consumes real time without changing the deterministic simulation.
                    if(hitStop>0){hitStop=Mathf.Max(0,hitStop-Time.unscaledDeltaTime);return;}
                    tickAccumulator+=Mathf.Min(Time.deltaTime,.1f);
                    while(tickAccumulator>=fixedStep&&State==RunState.Running)
                    {tickAccumulator-=fixedStep;Step(fixedStep);if(hitStop>0){tickAccumulator=0;break;}}
                }
            }
            if(qa&&State==RunState.Checkpoint)QADraft();
            if(State==RunState.Ready){clock+=Time.deltaTime;hero.Pose(clock,.05f,-1,0,0,false,0);}
            if(State==RunState.Complete||State==RunState.Defeated)
            {
                fx.Step(Mathf.Min(Time.deltaTime,.04f));
                if(qa&&!qaReported)ReportQA();
            }
        }
        void Step(float dt)
        {
            clock+=dt;Elapsed+=dt;WaveElapsed+=dt;EventAge+=dt;HighlightAge+=dt;flash=Mathf.Max(0,flash-dt*2);shake=Mathf.Max(0,shake-dt*2);hitReact=Mathf.Max(0,hitReact-dt*4);
            pushCooldown=Mathf.Max(0,pushCooldown-dt);
            // Constant forward intent, with subtle weight shift during strikes.
            hero.Root.position+=Vector3.forward*tuning.walkSpeed*dt*(castAge>=0?.45f:strike>=0?.65f:1f);
            spawnTimer-=dt;
            if(spawnTimer<=0&&!CleaningUp&&enemies.Count<tuning.maximumEnemies)
            {
                float side=SpawnRange(-5.5f,5.5f);
                Spawn(hero.Root.position+new Vector3(side,0,SpawnRange(8.5f,13f)),Wave==3&&spawnRandom.NextDouble()<.16);
                float pressure=((Wave-1)+Mathf.Clamp01(WaveElapsed/tuning.waveArrivals))/tuning.waveCount;
                spawnTimer=Mathf.Lerp(tuning.firstSpawnInterval,tuning.finalSpawnInterval,pressure)*SpawnRange(.85f,1.12f);
                if(Wave>=2&&spawnRandom.NextDouble()<.22)Spawn(hero.Root.position+new Vector3(SpawnRange(-3f,3f),0,SpawnRange(10f,13f)),false);
            }
            StepEnemies(dt);
            int close=0;foreach(var e in enemies)if((e.rig.Root.position-hero.Root.position).sqrMagnitude<tuning.pushRange*tuning.pushRange)close++;
            CrowdNear=close;PeakCrowd=Mathf.Max(PeakCrowd,close);
            // Let the current combo finish; a field discharge no longer eats the third hit.
            if(AutoPush&&strike<0&&pushCooldown<=0&&castAge<0&&(close>=tuning.pushCrowdThreshold||(Health<tuning.heroHealth*.35f&&close>=2)))RequestPush();
            StepHero(dt);
            fx.Step(dt);StepCorpses(dt);StepFlights(dt);UpdateCamera(dt);
            if(Health<=0){Health=0;State=RunState.Defeated;EventText="OVERRUN";EventAge=0;}
            else if(CleaningUp&&(enemies.Count==0||WaveElapsed>=tuning.waveArrivals+tuning.cleanupWindow))FinishWave();
            if(!Finite(hero.Root.position))throw new InvalidOperationException("Non-finite hero position");
        }
        static bool Finite(Vector3 p)=>!(float.IsNaN(p.x)||float.IsInfinity(p.x)||float.IsNaN(p.z)||float.IsInfinity(p.z));
        void Spawn(Vector3 position,bool heavy)
        {
            if(enemies.Count>=tuning.maximumEnemies)return;
            var e=new Enemy{id=++nextEnemyId,rig=new FighterRig(heavy?"Enforcer":"Raider",actors,false,heavy),heavy=heavy,health=tuning.enemyHealth*(heavy?1.9f:1f),cooldown=CombatRange(.2f,1.3f),orbitDirection=combatRandom.NextDouble()<.5?-1:1,radius=CombatRange(2.4f,3.6f),phase=CombatRange(0,6.28f)};
            e.rig.Root.position=position;e.rig.Root.rotation=Quaternion.Euler(0,180,0);enemies.Add(e);Spawned++;
        }
        void StepEnemies(float dt)
        {
            int committed=0;foreach(var e in enemies)if(e.engaged)committed++;
            foreach(var e in enemies)
            {
                var p=e.rig.Root.position;Vector3 delta=hero.Root.position-p;delta.y=0;float distance=delta.magnitude;Vector3 inward=distance>.01f?delta/distance:Vector3.back;
                e.cooldown-=dt;e.stun=Mathf.Max(0,e.stun-dt);e.hitReact=Mathf.Max(0,e.hitReact-dt*3);
                e.flightTime=Mathf.Max(0,e.flightTime-dt);
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
                if(e.flightTime<=0)desired+=separation;
                if(distance<1.05f)desired-=inward*(1.05f-distance)*9;
                e.velocity=Vector3.Lerp(e.velocity,desired,1-Mathf.Exp(-8*dt));
                e.impulse=Vector3.Lerp(e.impulse,Vector3.zero,1-Mathf.Exp(-(e.flightTime>0?.65f:3.6f)*dt));
                if(e.attack>=0)e.velocity*=.88f;
                e.rig.Root.position+=(e.velocity+e.impulse)*dt;
                var bounded=e.rig.Root.position;bounded.x=Mathf.Clamp(bounded.x,-8.3f,8.3f);bounded.y=0;e.rig.Root.position=bounded;
                Vector3 facing=distance<4.5f?inward:e.velocity.normalized;
                if(facing.sqrMagnitude>.05f&&e.flightTime<=0)e.rig.Root.rotation=Quaternion.Slerp(e.rig.Root.rotation,Quaternion.LookRotation(facing),1-Mathf.Exp(-9*dt));
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
                    if(e.attack>=duration){e.attack=-1;e.engaged=false;e.cooldown=CombatRange(.8f,1.8f);}
                }
                e.rig.Pose(clock,e.velocity.magnitude/tuning.enemySpeed,e.attack>=0?(e.heavy?2:1):-1,e.attack/(e.heavy?1.25f:.95f),e.hitReact,false,0);
                e.rig.Telegraph(e.attack>=0&&!e.hitDelivered,e.stun>0);
                if(e.flightTime>0)e.rig.Airborne(e.flightTime);
                if(!Finite(e.rig.Root.position))throw new InvalidOperationException("Non-finite enemy position");
            }
            // Resolve residual overlap symmetrically, preserving free continuous movement.
            for(int i=0;i<enemies.Count;i++)for(int j=i+1;j<enemies.Count;j++)
            {
                var a=enemies[i];var b=enemies[j];Vector3 delta=a.rig.Root.position-b.rig.Root.position;float d=delta.magnitude;
                if(a.flightTime<=0&&b.flightTime<=0&&d>.001f&&d<.64f){var correction=delta/d*(.64f-d)*.5f;a.rig.Root.position+=correction;b.rig.Root.position-=correction;}
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
            comboWait-=dt*Draft.Speed;
            if(strike<0&&comboWait<=0)
            {
                strikeTarget=Target(tuning.meleeReach);
                if(strikeTarget!=null){strike=0;strikeAge=0;strikeDelivered=false;}
            }
            float duration=(strike==0?.38f:strike==1?.44f:.64f)/Draft.Speed;
            if(strike>=0)
            {
                if(strikeTarget==null||!enemies.Contains(strikeTarget))strikeTarget=Target(tuning.meleeReach+.3f);
                if(strikeTarget!=null)
                {
                    var aim=strikeTarget.rig.Root.position-hero.Root.position;aim.y=0;
                    if(aim.sqrMagnitude>.01f)hero.Root.rotation=Quaternion.Slerp(hero.Root.rotation,Quaternion.LookRotation(aim),1-Mathf.Exp(-17*dt));
                }
                strikeAge+=dt;
                if(!strikeDelivered&&strikeAge>=duration*.44f){strikeDelivered=true;Strike();}
                if(strikeAge>=duration)
                {
                    if(strike<2){strike++;strikeAge=0;strikeDelivered=false;if(strikeTarget==null||!enemies.Contains(strikeTarget)||Vector3.Distance(strikeTarget.rig.Root.position,hero.Root.position)>tuning.meleeReach+.3f)strikeTarget=Target(tuning.meleeReach+.3f);}
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
                Hurt(strikeTarget,(strike==0?tuning.jabDamage:tuning.crossDamage)*Draft.Damage,hero.Root.forward*.9f,.26f);StrikesLanded++;ImpactPause(.025f);
                EventText=strike==0?"01 / JAB":"02 / CROSS";EventAge=0;shake=.055f;
            }
            else
            {
                int hit=0;Enemy launchTarget=Draft.Launch?FindLaunchTarget():null;
                for(int i=enemies.Count-1;i>=0;i--)
                {
                    var e=enemies[i];var delta=e.rig.Root.position-hero.Root.position;
                    if(delta.magnitude<tuning.meleeReach+.65f&&Vector3.Dot(delta.normalized,hero.Root.forward)>-.1f)
                    {
                        fx.Burst(e.rig.Root.position+Vector3.up*1.15f,VisualFactory.Cyan,7,3.5f);
                        Vector3 impulse=delta.normalized*(e==launchTarget?0:4.2f);
                        Hurt(e,tuning.finisherDamage*Draft.Damage,impulse,.85f);
                        if(e==launchTarget)Launch(e,LaunchDirection(e));hit++;
                    }
                }
                if(hit>0){StrikesLanded+=hit;ComboFinishers++;fx.Play(1);fx.Ring(hero.Root.position,2.7f,VisualFactory.Cyan,.3f);shake=.15f;EventText="03 / BREAKER";EventAge=0;ImpactPause(.065f);}
            }
        }
        public void RequestPush()
        {
            if(State!=RunState.Running||pushCooldown>0||castAge>=0)return;
            castAge=0;castDelivered=false;pushCooldown=tuning.pushCooldown*Draft.FieldCooldown;strike=-1;comboWait=.2f;
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
                e.rig.Telegraph(false,false);
                if(corpses.Count>32&&corpses[0].flightTime<=0){Destroy(corpses[0].rig.Root.gameObject);corpses.RemoveAt(0);}
            }
        }
        void StepCorpses(float dt)
        {
            for(int i=corpses.Count-1;i>=0;i--)
            {
                var e=corpses[i];e.deathAge+=dt;e.flightTime=Mathf.Max(0,e.flightTime-dt);e.velocity=Vector3.Lerp(e.velocity,Vector3.zero,dt*(e.flightTime>0?.65f:4));e.rig.Root.position+=e.velocity*dt;
                if(e.flightTime>0){e.rig.Airborne(e.flightTime);continue;}
                float fall=Mathf.Clamp01(e.deathAge/.38f);e.rig.Root.rotation=Quaternion.Slerp(e.rig.Root.rotation,Quaternion.Euler(78,e.rig.Root.eulerAngles.y,17*e.orbitDirection),dt*12);
                var p=e.rig.Root.position;p.y=-.05f-fall*.1f;e.rig.Root.position=p;
                if(e.deathAge>2.2f)e.rig.Root.localScale*=Mathf.Max(0,1-dt*3.5f);
                if(e.deathAge>3.2f){Destroy(e.rig.Root.gameObject);corpses.RemoveAt(i);}
            }
        }
        void ImpactPause(float seconds){if(!qa&&!ReducedMotion)hitStop=Mathf.Max(hitStop,seconds);}
        Enemy FindLaunchTarget()
        {
            Enemy best=null;float bestScore=-1000;
            foreach(var e in enemies)
            {
                Vector3 delta=e.rig.Root.position-hero.Root.position;
                if(delta.magnitude>=tuning.meleeReach+.65f||Vector3.Dot(delta.normalized,hero.Root.forward)<=-.1f)continue;
                float score=-delta.magnitude;
                foreach(var other in enemies)
                {
                    if(other==e)continue;Vector3 behind=other.rig.Root.position-e.rig.Root.position;
                    if(behind.magnitude<6&&Vector3.Dot(behind.normalized,delta.normalized)>.45f)score+=2;
                }
                if(score>bestScore){best=e;bestScore=score;}
            }
            return best;
        }
        Vector3 LaunchDirection(Enemy body)
        {
            var forward=(body.rig.Root.position-hero.Root.position).normalized;
            Vector3 direction=forward;float nearest=7;
            foreach(var other in enemies)
            {
                if(other==body)continue;var delta=other.rig.Root.position-body.rig.Root.position;
                if(delta.magnitude<nearest&&Vector3.Dot(delta.normalized,forward)>.45f){direction=delta.normalized;nearest=delta.magnitude;}
            }
            return direction.sqrMagnitude>.1f?direction:hero.Root.forward;
        }
        void Launch(Enemy body,Vector3 direction)
        {
            // One primary body per finisher. Even a killing strike leaves a useful projectile.
            for(int i=flights.Count-1;i>=0;i--)if(flights[i].body==body)flights.RemoveAt(i);
            body.flightTime=1.05f;body.stun=Mathf.Max(body.stun,1.3f);body.attack=-1;body.engaged=false;
            Vector3 velocity=direction*(body.heavy?10f:15f);
            body.impulse=velocity;body.velocity=body.deathAge>=0?velocity:Vector3.zero;
            body.rig.Root.rotation=Quaternion.LookRotation(-direction);
            flights.Add(new Flight{body=body,previous=body.rig.Root.position,armed=Draft.Pinball});Launches++;
            Color color=Draft.Pinball?new Color(1,.73f,.25f):VisualFactory.Cyan;
            fx.Arc(hero.RightHand.position,body.rig.Root.position+Vector3.up*1.3f,color);
            Announce(Draft.Pinball?"HUMAN PINBALL / BODY ARMED":"FLUX BRACERS / LAUNCH");
        }
        static float SegmentDistance(Vector3 point,Vector3 start,Vector3 end,out float along)
        {
            var segment=end-start;along=segment.sqrMagnitude>.000001f?Mathf.Clamp01(Vector3.Dot(point-start,segment)/segment.sqrMagnitude):0;
            return (point-(start+segment*along)).magnitude;
        }
        void StepFlights(float dt)
        {
            for(int i=flights.Count-1;i>=0;i--)
            {
                var f=flights[i];if(f.body.rig.Root==null){flights.RemoveAt(i);continue;}
                f.age+=dt;var current=f.body.rig.Root.position;current.y=0;f.trailTimer-=dt;
                Color color=f.armed?new Color(1,.73f,.25f):VisualFactory.Cyan;
                if(f.trailTimer<=0){fx.Arc(f.previous+Vector3.up*1.3f,current+Vector3.up*1.3f,color);f.trailTimer=.035f;}
                if(f.armed&&f.hitIds.Count<2)
                {
                    // Swept contacts cannot tunnel through targets at fast launch speeds.
                    var contacts=new List<KeyValuePair<Enemy,float>>();
                    foreach(var e in enemies)
                    {
                        if(e==f.body||f.hitIds.Contains(e.id))continue;
                        var p=e.rig.Root.position;p.y=0;
                        if(SegmentDistance(p,f.previous,current,out float along)<(e.heavy?1.05f:.9f))contacts.Add(new KeyValuePair<Enemy,float>(e,along));
                    }
                    contacts.Sort((a,b)=>a.Value.CompareTo(b.Value));
                    foreach(var contact in contacts)
                    {
                        if(f.hitIds.Count>=2)break;var target=contact.Key;
                        if(!enemies.Contains(target))continue;
                        f.hitIds.Add(target.id);CollisionHits++;CollisionDamage+=Mathf.Min(26,target.health);
                        fx.Ring(target.rig.Root.position,1.3f,color,.34f);fx.Burst(target.rig.Root.position+Vector3.up, color,9,4);fx.Play(1);
                        Vector3 direction=(current-f.previous).normalized;
                        Hurt(target,26,direction*3.2f,1f);if(target.health<=0)CollisionKills++;
                        Announce("FAULT LINE / COLLISION "+f.hitIds.Count+" OF 2");ImpactPause(.045f);shake=.17f;
                    }
                }
                f.previous=current;
                if(f.age>=1.05f){f.body.flightTime=0;flights.RemoveAt(i);}
            }
        }
        [Serializable] public sealed class WaveReport
        {
            public int wave,kills,launches,collisions,hitsTaken,pushes,carried,bankedRerolls;
            public float healthStart,healthEnd,duration;
            public string choice="";
        }
        void FinishWave()
        {
            Waves.Add(new WaveReport{wave=Wave,kills=Kills-waveStartKills,launches=Launches-waveStartLaunches,collisions=CollisionHits-waveStartCollisions,hitsTaken=HitsTaken-waveStartHits,pushes=Pushes-waveStartPushes,carried=enemies.Count,bankedRerolls=Draft.Rerolls,healthStart=waveStartHealth,healthEnd=Health,duration=WaveElapsed});
            tickAccumulator=0;
            if(Wave>=tuning.waveCount){State=RunState.Complete;EventText="THE WALK CONTINUES";EventAge=0;return;}
            State=RunState.Checkpoint;Draft.Open();
        }
        void QADraft()
        {
            // Validate the real checkpoint API: no damage/time progress, no invalid installation,
            // banked rerolls survive, three unique cards, and duplicate confirmation is harmless.
            float elapsed=Elapsed,health=Health;int wave=Wave;Vector3 position=HeroPosition;
            ConfirmUpgrade();RequestPush();TogglePause();
            qaCheckpointChecks&=State==RunState.Checkpoint&&Elapsed==elapsed&&Health==health&&HeroPosition==position&&Wave==wave;
            qaCheckpointChecks&=Draft.Rerolls==waveStartRerolls+1;
            if(qaBuild=="reroll")
            {
                int available=Draft.Rerolls;
                SelectUpgrade(1);
                for(int i=0;i<available+1;i++)Reroll();
                qaCheckpointChecks&=Draft.Rerolls==0&&Draft.Selected==-1;
            }
            qaCheckpointChecks&=Draft.Offers.Length==3&&Draft.Offers[0].id!=Draft.Offers[1].id&&Draft.Offers[1].id!=Draft.Offers[2].id&&Draft.Offers[0].id!=Draft.Offers[2].id;
            int select=0;
            if(qaBuild=="power"||(qaBuild=="launch"&&Wave==2))
            {
                select=1;
                for(int i=1;i<3;i++)if(Draft.Offers[i].id==UpgradeId.DenseKnuckles||Draft.Offers[i].id==UpgradeId.ServoTuning){select=i;break;}
            }
            SelectUpgrade(select);ConfirmUpgrade();int count=Draft.History.Count;ConfirmUpgrade();
            qaCheckpointChecks&=State==RunState.Running&&Wave==wave+1&&Draft.History.Count==count&&count==wave;
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
        { public string result,build;public int seed;public float elapsed,health,collisionDamage;public int spawned,kills,alive,peakCrowd,strikes,finishers,pushes,pushTargets,hitsTaken,launches,collisions,collisionKills,rerolls;public bool passed,checkpointChecks;public string platform;public string[] upgrades;public WaveReport[] waves; }
        void ReportQA()
        {
            qaReported=true;
            var r=new QAReport{result=State.ToString(),build=qaBuild,seed=tuning.seed,elapsed=Elapsed,health=Health,spawned=Spawned,kills=Kills,alive=enemies.Count,peakCrowd=PeakCrowd,strikes=StrikesLanded,finishers=ComboFinishers,pushes=Pushes,pushTargets=PushTargets,hitsTaken=HitsTaken,platform=Application.platform.ToString(),launches=Launches,collisions=CollisionHits,collisionKills=CollisionKills,collisionDamage=CollisionDamage,checkpointChecks=qaCheckpointChecks,rerolls=Draft.Rerolls,upgrades=Draft.History.ConvertAll(id=>id.ToString()).ToArray(),waves=Waves.ToArray()};
            bool pathValid=qaBuild=="power"?Launches==0&&CollisionHits==0:qaBuild=="launch"?Launches>0&&CollisionHits==0:Launches>=4&&CollisionHits>=3;
            r.passed=State==RunState.Complete&&Waves.Count==tuning.waveCount&&Draft.History.Count==tuning.waveCount-1&&Kills>=25&&ComboFinishers>=8&&Pushes>=3&&PeakCrowd>=4&&Spawned==Kills+enemies.Count&&qaCheckpointChecks&&pathValid&&CollisionHits<=Launches*2;
            string path=string.IsNullOrEmpty(qaPath)?Path.Combine(Application.persistentDataPath,"qa-report.json"):qaPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(r,true));Debug.Log("VECTOR_QA "+JsonUtility.ToJson(r));
            Application.Quit(r.passed?0:2);
        }
    }
}
