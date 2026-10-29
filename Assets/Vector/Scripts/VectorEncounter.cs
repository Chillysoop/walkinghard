using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VectorGame
{
    public sealed partial class VectorEncounter : MonoBehaviour
    {
        public enum RunState { Ready, Drafting, Running, Paused, Complete, Defeated }
        public CombatTuning tuning;
        public RunState State {get;private set;}=RunState.Ready;
        public AbilityDeck Deck {get;private set;}
        public int Seed {get;private set;}
        public int Wave {get;private set;}=1;
        public int OpeningPicks {get;private set;}
        public bool OpeningDraft=>OpeningPicks<2;
        public bool Automatic {get;private set;}=true;
        public bool ReducedMotion {get;private set;}
        public bool Muted=>fx!=null&&fx.muted;
        public float Health {get;private set;}
        public float Elapsed {get;private set;}
        public float WaveElapsed {get;private set;}
        public bool CleaningUp=>WaveElapsed>=tuning.waveArrivals;
        public float WaveRemaining=>Mathf.Max(0,(CleaningUp?tuning.waveArrivals+tuning.cleanupWindow:tuning.waveArrivals)-WaveElapsed);
        public int Kills {get;private set;}
        public int Spawned {get;private set;}
        public int Launches {get;private set;}
        public int CollisionHits {get;private set;}
        public int CollisionKills {get;private set;}
        public int Ragdolls {get;private set;}
        public int PhysicsContacts {get;private set;}
        public int WorldContacts {get;private set;}
        public int LeftEntries {get;private set;}
        public int RightEntries {get;private set;}
        public int HorizonEntries {get;private set;}
        public int VisibleSpawns {get;private set;}
        public int PeakCrowd {get;private set;}
        public int HitsTaken {get;private set;}
        public int AbilityCasts {get;private set;}
        public int OrderErrors {get;private set;}
        public int ActiveSlot {get;private set;}=-1;
        public int NextSlot=>Deck?.NextOccupied(sequencePointer)??-1;
        public float ActionProgress=>ActiveSlot<0?0:Mathf.Clamp01(actionAge/(Deck.Slots[ActiveSlot].duration/Deck.Speed));
        public string EventText {get;private set;}="";
        public float EventAge {get;private set;}=99;
        public int EnemyCount=>enemies.Count;
        public int CrowdNear {get;private set;}
        public Vector3 HeroPosition=>hero.Root.position;
        public float DamageFlash=>flash;
        public readonly int[] SlotCasts=new int[3];
        public readonly List<WaveReport> Waves=new List<WaveReport>();
        public string NextThreat=>Wave==1?"More attackers from both doors and the horizon. Fill your last empty slot.":"Heavy enforcers join the rush. Reinforce the sequence you built.";

        sealed class Enemy
        {
            public int id;public FighterRig rig;public FighterPhysics physics;
            public float health,stun,cooldown,attack=-1,deathAge=-1,radius,orbit,phase,react;
            public bool engaged,heavy,delivered,entering;
            public Vector3 doorway;
        }
        sealed class Flight
        {
            public Enemy body;public float age,trail;public Vector3 previous;
            public readonly HashSet<int> hit=new HashSet<int>();
        }
        struct Contact {public int source,target;public float speed;public Vector3 point;}
        readonly List<Enemy> enemies=new List<Enemy>();
        readonly List<Enemy> corpses=new List<Enemy>();
        readonly Dictionary<int,Enemy> byId=new Dictionary<int,Enemy>();
        readonly List<Flight> flights=new List<Flight>();
        readonly List<Contact> contacts=new List<Contact>();
        readonly List<int> castTrace=new List<int>();
        FighterRig hero;FighterPhysics heroPhysics;CombatEffects fx;Camera cameraRig;Transform actors;
        System.Random spawnRandom,combatRandom;
        Vector3 cameraVelocity;
        SimulationMode previousSimulation;
        float clock,accumulator,spawnTimer,actionAge,recovery,hitStop,shake,flash,heroReact;
        int nextId,entryTurn,sequencePointer,expectedSlot,waveKills,waveLaunches,waveCollisions,waveHits;
        bool actionDelivered,qa,reported,forcedSeed,qaDraftChecks=true,qaManualChecks=true;
        Enemy actionTarget;
        string qaPath,qaBuild="sequence";
        int qaManualPhase,qaHeldCasts;

        void Awake()
        {
            if(tuning==null)tuning=Resources.Load<CombatTuning>("CombatTuning");
            if(tuning==null)tuning=ScriptableObject.CreateInstance<CombatTuning>();
            var args=Environment.GetCommandLineArgs();qa=Array.IndexOf(args,"-vectorQA")>=0;
            for(int i=0;i<args.Length-1;i++)
            {
                if(args[i]=="-qaOutput")qaPath=args[i+1];
                if(args[i]=="-qaBuild")qaBuild=args[i+1];
                if(args[i]=="-seed"&&int.TryParse(args[i+1],out int value)){tuning.seed=value;forcedSeed=true;}
            }
            Seed=tuning.seed;Deck=new AbilityDeck(Seed);
            previousSimulation=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            Physics.defaultSolverIterations=12;Physics.defaultSolverVelocityIterations=8;
            Application.targetFrameRate=qa?-1:60;QualitySettings.vSyncCount=qa?0:1;
            VisualFactory.Initialize();
            var world=VisualFactory.Node("Concourse and covered entry passages",transform,Vector3.zero);VisualFactory.World(world);
            actors=VisualFactory.Node("Fighters",transform,Vector3.zero);
            hero=new FighterRig("The Walker",actors,true);heroPhysics=new FighterPhysics(hero,0,this,true);
            hero.Root.position=new Vector3(0,.04f,0);
            fx=new CombatEffects(VisualFactory.Node("Impacts",transform,Vector3.zero),qa);
            cameraRig=new GameObject("Elevated chase camera").AddComponent<Camera>();cameraRig.tag="MainCamera";
            cameraRig.transform.SetParent(transform);cameraRig.fieldOfView=tuning.cameraFov;cameraRig.nearClipPlane=.1f;cameraRig.farClipPlane=180;
            cameraRig.backgroundColor=new Color(.10f,.17f,.20f);cameraRig.clearFlags=CameraClearFlags.SolidColor;cameraRig.allowHDR=true;
            cameraRig.gameObject.AddComponent<AudioListener>();cameraRig.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
            Health=tuning.heroHealth;hero.Pose(0,0,-1,0,0,false,0);Physics.SyncTransforms();
            gameObject.AddComponent<PrototypeHUD>().game=this;
            if(qa)Begin();
        }
        void OnDestroy(){Physics.simulationMode=previousSimulation;}
        void DisposeEnemy(Enemy e){e.rig.Root.gameObject.SetActive(false);Destroy(e.rig.Root.gameObject);byId.Remove(e.id);}
        public void Begin(bool repeatSeed=false)
        {
            foreach(var e in enemies)DisposeEnemy(e);foreach(var e in corpses)DisposeEnemy(e);
            enemies.Clear();corpses.Clear();byId.Clear();flights.Clear();contacts.Clear();castTrace.Clear();Waves.Clear();fx.Clear();
            if(!repeatSeed)Seed=qa||forcedSeed?tuning.seed:new System.Random(Environment.TickCount).Next(1,int.MaxValue);
            Deck=new AbilityDeck(Seed);spawnRandom=new System.Random(Seed);combatRandom=new System.Random(Seed^6179);
            Elapsed=WaveElapsed=clock=accumulator=hitStop=shake=flash=heroReact=0;Health=tuning.heroHealth;Wave=1;OpeningPicks=0;Automatic=true;
            Kills=Spawned=Launches=CollisionHits=CollisionKills=Ragdolls=PhysicsContacts=WorldContacts=LeftEntries=RightEntries=HorizonEntries=VisibleSpawns=PeakCrowd=HitsTaken=AbilityCasts=OrderErrors=0;
            for(int i=0;i<3;i++)SlotCasts[i]=0;
            nextId=entryTurn=sequencePointer=0;ActiveSlot=-1;recovery=0;reported=false;qaDraftChecks=qaManualChecks=true;qaManualPhase=0;
            heroPhysics.Motor.position=new Vector3(0,.04f,0);heroPhysics.Motor.rotation=Quaternion.identity;
            hero.Equipment(false,false);hero.Pose(0,0,-1,0,0,false,0);
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;cameraVelocity=Vector3.zero;AimCamera();Physics.SyncTransforms();
            State=RunState.Drafting;Deck.Open(true,true);
        }
        public void SelectOffer(int index){if(State==RunState.Drafting)Deck.Select(index);}
        public void AssignSlot(int slot){if(State==RunState.Drafting)Deck.Assign(slot);}
        public void Reroll(){if(State==RunState.Drafting)Deck.Reroll();}
        public void SwapSlots(int a,int b){if(State==RunState.Drafting)Deck.Swap(a,b);}
        public void ConfirmDraft()
        {
            if(State!=RunState.Drafting||!Deck.CanInstall)return;
            bool recoveryPick=!Deck.IsAbilityDraft&&Deck.ModifierOffers[Deck.Selected].id==ModifierId.Recovery;
            string installed=Deck.Install();if(installed==null)return;
            if(recoveryPick)Health=Mathf.Min(tuning.heroHealth,Health+65);
            hero.Equipment(Deck.Has(AbilityId.MassDriver),Deck.Pinball);
            if(OpeningPicks<2)
            {
                OpeningPicks++;
                if(OpeningPicks<2){Deck.Open(true,false);return;}
            }
            else {Waves[Waves.Count-1].choice=installed;Wave++;}
            BeginWave();Announce(installed.ToUpperInvariant()+" INSTALLED");
        }
        void BeginWave()
        {
            WaveElapsed=0;accumulator=0;spawnTimer=.65f;ActiveSlot=-1;sequencePointer=0;expectedSlot=Deck.NextOccupied(0);recovery=.15f;
            waveKills=Kills;waveLaunches=Launches;waveCollisions=CollisionHits;waveHits=HitsTaken;
            State=RunState.Running;
            if(Wave==1){SpawnEntry(0);SpawnEntry(1);SpawnEntry(2);}
        }
        public void ToggleAutomatic(){Automatic=!Automatic;}
        public void TogglePause(){if(State==RunState.Running)State=RunState.Paused;else if(State==RunState.Paused)State=RunState.Running;}
        public void ToggleMotion(){ReducedMotion=!ReducedMotion;}
        public void ToggleSound(){fx.muted=!fx.muted;}
        public void Quit(){Application.Quit();}
        public void QueueContact(int source,int target,float speed,Vector3 point)
        {contacts.Add(new Contact{source=source,target=target,speed=speed,point=point});}
        void Announce(string text){EventText=text;EventAge=0;}
        float CombatRange(float min,float max)=>Mathf.Lerp(min,max,(float)combatRandom.NextDouble());
        float SpawnRange(float min,float max)=>Mathf.Lerp(min,max,(float)spawnRandom.NextDouble());
        void Update()
        {
            if(!qa)
            {
                if(Input.GetKeyDown(KeyCode.Return)&&(State==RunState.Ready||State==RunState.Complete||State==RunState.Defeated))Begin();
                if(Input.GetKeyDown(KeyCode.R))Begin();
                if(Input.GetKeyDown(KeyCode.T))Begin(true);
                if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape))TogglePause();
                if(Input.GetKeyDown(KeyCode.A))ToggleAutomatic();
                if(Input.GetKeyDown(KeyCode.F))Advance();
                if(Input.GetKeyDown(KeyCode.M))ToggleSound();
                if(Input.GetKeyDown(KeyCode.F8))Capture();
                if(State==RunState.Drafting)
                {
                    if(Input.GetKeyDown(KeyCode.Alpha1))SelectOffer(0);
                    if(Input.GetKeyDown(KeyCode.Alpha2))SelectOffer(1);
                    if(Input.GetKeyDown(KeyCode.Alpha3))SelectOffer(2);
                    if(Input.GetKeyDown(KeyCode.Q))AssignSlot(0);
                    if(Input.GetKeyDown(KeyCode.W))AssignSlot(1);
                    if(Input.GetKeyDown(KeyCode.E))AssignSlot(2);
                    if(Input.GetKeyDown(KeyCode.Tab))Reroll();
                    if(Input.GetKeyDown(KeyCode.Return))ConfirmDraft();
                }
            }
            if(State==RunState.Running)
            {
                const float dt=1f/60;
                if(qa){for(int i=0;i<(Application.isBatchMode?8:1)&&State==RunState.Running;i++)Step(dt);}
                else
                {
                    if(hitStop>0){hitStop=Mathf.Max(0,hitStop-Time.unscaledDeltaTime);return;}
                    accumulator+=Mathf.Min(Time.deltaTime,.1f);
                    while(accumulator>=dt&&State==RunState.Running){accumulator-=dt;Step(dt);if(hitStop>0){accumulator=0;break;}}
                }
            }
            if(qa&&State==RunState.Drafting)QADraft();
            if(qa&&!reported&&(State==RunState.Complete||State==RunState.Defeated))ReportQA();
            if(State==RunState.Ready){clock+=Time.deltaTime;hero.Pose(clock,0,-1,0,0,false,0);}
        }
        void Step(float dt)
        {
            clock+=dt;Elapsed+=dt;WaveElapsed+=dt;EventAge+=dt;flash=Mathf.Max(0,flash-dt*2);shake=Mathf.Max(0,shake-dt*2);heroReact=Mathf.Max(0,heroReact-dt*4);
            if(qaBuild=="manual"&&qa)QAManual();
            heroPhysics.Motor.MovePosition(HeroPosition+Vector3.forward*tuning.walkSpeed*dt*(ActiveSlot>=0?.62f:1));
            spawnTimer-=dt;
            if(spawnTimer<=0&&!CleaningUp&&enemies.Count<tuning.maximumEnemies)
            {
                SpawnEntry(entryTurn++%3);
                float pressure=((Wave-1)+Mathf.Clamp01(WaveElapsed/tuning.waveArrivals))/tuning.waveCount;
                spawnTimer=Mathf.Lerp(tuning.firstSpawnInterval,tuning.finalSpawnInterval,pressure)*SpawnRange(.85f,1.1f);
            }
            StepEnemies(dt);StepHero(dt);Physics.SyncTransforms();Physics.Simulate(dt);ProcessContacts();
            StepBodies(dt);fx.Step(dt);UpdateCamera(dt);
            CrowdNear=0;foreach(var e in enemies)if(Vector3.Distance(e.rig.Root.position,HeroPosition)<6)CrowdNear++;
            PeakCrowd=Mathf.Max(PeakCrowd,CrowdNear);
            if(Health<=0){Health=0;State=RunState.Defeated;Announce("OVERRUN");}
            else if(CleaningUp&&(enemies.Count==0||WaveElapsed>=tuning.waveArrivals+tuning.cleanupWindow))FinishWave();
        }
        void SpawnEntry(int kind)
        {
            if(enemies.Count>=tuning.maximumEnemies)return;
            EntryLayout.Door door=kind<2?EntryLayout.Ahead(kind==0?-1:1,HeroPosition.z):null;
            Vector3 p=door!=null?door.origin:EntryLayout.Horizon(cameraRig,HeroPosition,SpawnRange(-6,6));
            Physics.SyncTransforms();
            if(!EntryLayout.Concealed(cameraRig,p)){door=null;p=EntryLayout.Horizon(cameraRig,HeroPosition,SpawnRange(-6,6));}
            if(!EntryLayout.Concealed(cameraRig,p))throw new InvalidOperationException("No concealed entrance was available.");
            bool heavy=Wave==3&&spawnRandom.NextDouble()<.16;
            var e=new Enemy{id=++nextId,heavy=heavy,health=tuning.enemyHealth*(heavy?1.9f:1),cooldown=CombatRange(.3f,1.4f),radius=CombatRange(2.4f,3.6f),orbit=combatRandom.NextDouble()<.5?-1:1,phase=CombatRange(0,6.28f),entering=door!=null,doorway=door!=null?door.exit:Vector3.zero};
            e.rig=new FighterRig(heavy?"Enforcer":"Raider",actors,false,heavy);e.rig.Root.position=p;
            e.physics=new FighterPhysics(e.rig,e.id,this,false,heavy);enemies.Add(e);byId.Add(e.id,e);Spawned++;
            if(door==null)HorizonEntries++;else if(door.side<0)LeftEntries++;else RightEntries++;
            if(!EntryLayout.Concealed(cameraRig,p))VisibleSpawns++;
        }
        void StepEnemies(float dt)
        {
            int committed=0;foreach(var e in enemies)if(e.engaged)committed++;
            foreach(var e in enemies)
            {
                Vector3 p=e.rig.Root.position,delta=HeroPosition-p;delta.y=0;float distance=delta.magnitude;Vector3 inward=distance>.01f?delta/distance:Vector3.back;
                e.cooldown-=dt;e.stun=Mathf.Max(0,e.stun-dt);e.react=Mathf.Max(0,e.react-dt*3);
                if(e.stun>0){e.engaged=false;e.attack=-1;}
                if(e.entering&&Mathf.Abs(p.x)<8.2f)e.entering=false;
                if(!e.entering&&!e.engaged&&e.stun<=0&&e.cooldown<=0&&distance<3.3f&&committed<tuning.simultaneousAttackers)
                {
                    bool opening=true;foreach(var other in enemies)if(other!=e&&other.engaged&&Vector3.Angle(other.rig.Root.position-HeroPosition,p-HeroPosition)<34){opening=false;break;}
                    if(opening){e.engaged=true;committed++;}
                }
                Vector3 desired;
                if(e.stun>0)desired=Vector3.zero;
                else if(e.entering){var toward=e.doorway-p;toward.y=0;desired=toward.normalized*4.5f;}
                else if(distance>15)desired=inward*9f;
                else if(e.engaged)desired=inward*Mathf.Clamp((distance-1.3f)*4,-1.2f,tuning.enemySpeed);
                else if(distance>e.radius+.7f)desired=inward*tuning.enemySpeed;
                else desired=inward*Mathf.Clamp((distance-e.radius-.15f*Mathf.Sin(clock+e.phase))*2,-1.6f,1.6f)+new Vector3(-inward.z,0,inward.x)*e.orbit*.85f;
                if(e.heavy)desired*=.82f;
                if(!e.entering&&e.stun<=0)
                {
                    foreach(var other in enemies){if(other==e)continue;Vector3 diff=p-other.rig.Root.position;diff.y=0;float d=diff.magnitude;if(d>.001f&&d<1.1f)desired+=diff/d*(1.1f-d)*2;}
                }
                Vector3 velocity=e.physics.Motor.linearVelocity;
                Vector3 flat=new Vector3(velocity.x,0,velocity.z);
                flat=Vector3.Lerp(flat,desired,1-Mathf.Exp(-(e.stun>0?1.4f:7)*dt));
                e.physics.Motor.linearVelocity=new Vector3(flat.x,velocity.y,flat.z);
                Vector3 facing=e.entering?desired:inward;
                if(e.stun<=0&&facing.sqrMagnitude>.01f)e.physics.Motor.MoveRotation(Quaternion.Slerp(e.rig.Root.rotation,Quaternion.LookRotation(facing),1-Mathf.Exp(-10*dt)));
                if(e.engaged&&distance<tuning.attackRadius&&e.attack<0){e.attack=0;e.delivered=false;}
                if(e.attack>=0)
                {
                    float duration=e.heavy?1.25f:.95f;e.attack+=dt;
                    if(!e.delivered&&e.attack>=duration*.53f)
                    {
                        e.delivered=true;
                        if(e.stun<=0&&Vector3.Distance(e.rig.Root.position,HeroPosition)<tuning.attackRadius+.25f)
                        {Health-=tuning.enemyDamage*(e.heavy?1.8f:1);HitsTaken++;heroReact=.8f;flash=.3f;fx.Play(3);}
                    }
                    if(e.attack>=duration){e.attack=-1;e.engaged=false;e.cooldown=CombatRange(.9f,1.8f);}
                }
                e.rig.Pose(clock,flat.magnitude/tuning.enemySpeed,e.attack>=0?1:-1,e.attack/(e.heavy?1.25f:.95f),e.react,false,0);
                e.rig.Telegraph(e.attack>=0&&!e.delivered,e.stun>0);
            }
        }
        [Serializable] public sealed class WaveReport
        {public int wave,kills,launches,collisions,carried,hitsTaken;public float health;public string choice="";}
        void FinishWave()
        {
            Waves.Add(new WaveReport{wave=Wave,kills=Kills-waveKills,launches=Launches-waveLaunches,collisions=CollisionHits-waveCollisions,carried=enemies.Count,hitsTaken=HitsTaken-waveHits,health=Health});
            accumulator=0;ActiveSlot=-1;
            if(Wave>=tuning.waveCount){State=RunState.Complete;return;}
            State=RunState.Drafting;Deck.Open(Wave==1,true);
        }
        void UpdateCamera(float dt)
        {
            cameraRig.transform.position=Vector3.SmoothDamp(cameraRig.transform.position,HeroPosition+tuning.cameraOffset,ref cameraVelocity,.20f,100,dt);
            if(!ReducedMotion)cameraRig.transform.position+=new Vector3(Mathf.Sin(clock*59),Mathf.Sin(clock*71),0)*shake*.12f;
            AimCamera();
        }
        void AimCamera(){cameraRig.transform.LookAt(HeroPosition+new Vector3(0,1,tuning.lookAhead));}
        public void Capture()
        {
            string dir=Path.Combine(Application.persistentDataPath,"Captures");Directory.CreateDirectory(dir);
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,"WalkingHard-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png"));
        }
    }
}
