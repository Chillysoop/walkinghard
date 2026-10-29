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
        public bool UltimateAutomatic {get;private set;}=true;
        public bool HoldCheckpointAfterPick {get;set;}
        public float UltimateCooldown=>Deck?.Ultimate==null?0:LoadoutCatalog.Cooldown(Deck.Ultimate);
        public float UltimateRemaining {get;private set;}
        public bool UltimateQueued=>Deck.Ultimate!=null&&UltimateRemaining<=0&&(UltimateAutomatic||ultimateRequested);
        public int UltimateCasts {get;private set;}
        public int TempestHits {get;private set;}
        float ultimateDue;
        readonly List<float> ultimateDeadlines=new List<float>();
        public int PairedCasts {get;private set;}
        public int IndependentTargets {get;private set;}
        public int Severs {get;private set;}
        public int Reloads {get;private set;}
        public int MaxCascadeHop {get;private set;}
        public int PairSlot {get;private set;}=-1;
        public string ActionStatus {get;private set;}="";
        public float Preparation {get;private set;}
        public int[] WeaponHits {get;}=new int[6];
        public ComboScore Score {get;private set;}=new ComboScore();
        readonly Dictionary<LoadoutItem,int> itemKills=new Dictionary<LoadoutItem,int>();
        readonly Dictionary<LoadoutItem,LoadoutItem> fusedOwners=new Dictionary<LoadoutItem,LoadoutItem>();
        LoadoutItem KillOwner(LoadoutItem item){while(item!=null&&fusedOwners.TryGetValue(item,out var next))item=next;return item;}
        public int KillsFor(LoadoutItem item){item=KillOwner(item);return item!=null&&itemKills.TryGetValue(item,out int value)?value:0;}
        public int ActionKills(LoadoutItem item)=>KillsFor(item?.def.kind==ItemKind.Basic?Deck.WeaponFor(item):item);
        public float CombatSpeed {get;private set;}=1;
        public int CorpseShoves {get;private set;}
        public int ArmorBonusHits {get;private set;}
        public float ArmorPreventedDamage {get;private set;}
        public float ArmorHealing {get;private set;}
        public int RetainedBodies=>corpses.Count;
        public void SetCombatSpeed(float speed){if(speed==.5f||speed==1||speed==2||speed==4)CombatSpeed=speed;}
        sealed class KillCredit {public LoadoutItem item,weapon;public int action,hop;public bool derived,augment;}
        KillCredit currentCredit;int actionSerial;
        public bool ReducedMotion {get;private set;}
        public bool Muted=>fx!=null&&fx.muted;
        public float Health {get;private set;}
        public float MaxHealth=>tuning.heroHealth+(Deck?.MaxHealthBonus??0);
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
        public float ActionProgress=>ActiveSlot<0?0:Mathf.Clamp01(actionAge/Mathf.Max(.01f,actionDuration));
        public string EventText {get;private set;}="";
        public float EventAge {get;private set;}=99;
        public int EnemyCount=>enemies.Count;
        public int CrowdNear {get;private set;}
        public Vector3 HeroPosition=>hero.Root.position;
        public float DamageFlash=>flash;
        public readonly int[] SlotCasts=new int[5];
        public readonly List<WaveReport> Waves=new List<WaveReport>();
        public sealed class FusionResult {public ItemDefinition definition;public int before,after;}
        public readonly List<FusionResult> FusionHistory=new List<FusionResult>();
        public string NextThreat=>EncounterPreview;

        sealed class Enemy
        {
            public int id;public FighterRig rig;public FighterPhysics physics;
            public float maxHealth,health,stun,cooldown,attack=-1,deathAge=-1,radius,orbit,phase,react;
            public ImpactReaction reaction;public float reactionLeft,reactionPulse;
            public bool engaged,heavy,delivered,entering,boss,shooter;
            public bool repositioning;
            public float pressure,lastDamageTime=-1000,previousDamageTime=-1000;
            public KillCredit lastPushCredit;
            public Vector3 doorway,cachedPosition;
        }
        sealed class Flight
        {
            public Enemy body;public float age,trail;public Vector3 previous;
            public Cascade chain;public int hop;public float energy=1;
        }
        sealed class Cascade {public KillCredit credit;public int remaining=6;public readonly HashSet<int> hit=new HashSet<int>();}
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
        bool actionDelivered,qa,reported,forcedSeed,qaDraftChecks=true,qaManualChecks=true,ultimateRequested,bonusDelivered;
        AbilityCard activeCard,pairCard;
        LoadoutItem actionItem,pairItem,actionWeapon,pairWeapon;
        Enemy pairTarget;
        float actionDuration;
        WeaponKind heldWeapon=WeaponKind.Fists;
        readonly Dictionary<LoadoutItem,int> ammunition=new Dictionary<LoadoutItem,int>();
        readonly List<float> ultimateTimes=new List<float>();
        Enemy actionTarget;
        string qaPath,qaBuild="sequence";
        bool qaHoldCheckpoint,qaHolding;
        PrototypeHUD hud;
        int qaManualPhase,qaHeldCasts;

        void Awake()
        {
            if(tuning==null)tuning=Resources.Load<CombatTuning>("CombatTuning");
            if(tuning==null)tuning=ScriptableObject.CreateInstance<CombatTuning>();
            var args=Environment.GetCommandLineArgs();ConfigureSimulation(args);qa=simulation||Array.IndexOf(args,"-vectorQA")>=0;
            if(qa)Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            qaHoldCheckpoint=qa&&Array.IndexOf(args,"-qaHoldCheckpoint")>=0;
            for(int i=0;i<args.Length-1;i++)
            {
                if(args[i]=="-qaOutput")qaPath=args[i+1];
                if(args[i]=="-qaBuild")qaBuild=args[i+1];
                if(args[i]=="-seed"&&int.TryParse(args[i+1],out int value)){tuning.seed=value;forcedSeed=true;}
            }
            tuning=Instantiate(tuning);tuning.waveCount=simulation?simulationWaves:qa&&Array.IndexOf(args,"-qaFullRun")<0?(qaBuild=="progression"?4:3):8;
            Seed=tuning.seed;Deck=new AbilityDeck(Seed);
            previousSimulation=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            Physics.defaultSolverIterations=12;Physics.defaultSolverVelocityIterations=8;
            Application.targetFrameRate=qa&&Application.isBatchMode?-1:60;QualitySettings.vSyncCount=qa&&Application.isBatchMode?0:1;
            VisualFactory.Initialize();
            var world=VisualFactory.Node("Concourse and covered entry passages",transform,Vector3.zero);VisualFactory.World(world);
            actors=VisualFactory.Node("Fighters",transform,Vector3.zero);
            hero=new FighterRig("The Walker",actors,true,lightweight:FastSimulation);heroPhysics=new FighterPhysics(hero,0,this,true);
            hero.Root.position=new Vector3(0,.04f,0);
            fx=new CombatEffects(VisualFactory.Node("Impacts",transform,Vector3.zero),qa);
            cameraRig=new GameObject("Elevated chase camera").AddComponent<Camera>();cameraRig.tag="MainCamera";
            cameraRig.transform.SetParent(transform);cameraRig.fieldOfView=tuning.cameraFov;cameraRig.nearClipPlane=.1f;cameraRig.farClipPlane=180;
            cameraRig.backgroundColor=new Color(.10f,.17f,.20f);cameraRig.clearFlags=CameraClearFlags.SolidColor;cameraRig.allowHDR=true;
            cameraRig.gameObject.AddComponent<AudioListener>();cameraRig.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
            Health=tuning.heroHealth;hero.Pose(0,0,-1,0,0,false,0);Physics.SyncTransforms();
            if(!simulation){hud=gameObject.AddComponent<PrototypeHUD>();hud.game=this;}
            if(qa){if(!FastSimulation)CheckCombatMechanics();if(simulation){LoadoutChecks.Run();modelChecks=true;int balanceIndex=Array.IndexOf(args,"-simBalance");if(balanceIndex>=0&&balanceIndex+1<args.Length)BalanceOverrides.Apply(args[balanceIndex+1],tuning);}else{int healthIndex=Array.IndexOf(args,"-qaHealth");if(healthIndex>=0&&healthIndex+1<args.Length&&int.TryParse(args[healthIndex+1],out int qaHealth)&&qaHealth>0)tuning.heroHealth=qaHealth;}Begin();}
            if(Array.IndexOf(args,"-mechanicReview")>=0)StartRequestedMechanicReview();
        }
        void OnDestroy(){Physics.simulationMode=previousSimulation;}
        void DisposeEnemy(Enemy e){e.rig.Root.gameObject.SetActive(false);Destroy(e.rig.Root.gameObject);byId.Remove(e.id);}
        public void Begin(bool repeatSeed=false)
        {
            foreach(var e in enemies)DisposeEnemy(e);foreach(var e in corpses)DisposeEnemy(e);
            enemies.Clear();corpses.Clear();byId.Clear();flights.Clear();contacts.Clear();castTrace.Clear();Waves.Clear();fx.Clear();
            if(!repeatSeed)Seed=qa||forcedSeed?tuning.seed:new System.Random(Environment.TickCount).Next(1,int.MaxValue);
            ResetSignatures();ItemChoices=AugmentChoices=0;pendingDrafts.Clear();bossSpawned=false;Deck=new AbilityDeck(Seed);Deck.Fused+=MergeKillCounts;spawnRandom=new System.Random(Seed);combatRandom=new System.Random(Seed^6179);
            Elapsed=WaveElapsed=clock=accumulator=hitStop=shake=flash=heroReact=0;Health=tuning.heroHealth;Wave=1;OpeningPicks=0;UltimateAutomatic=true;UltimateRemaining=UltimateCooldown;ultimateRequested=false;UltimateCasts=PairedCasts=IndependentTargets=Severs=Reloads=MaxCascadeHop=0;ammunition.Clear();ultimateTimes.Clear();ultimateDeadlines.Clear();ultimateDue=UltimateCooldown;TempestHits=0;heldWeapon=WeaponKind.Fists;Score=new ComboScore();itemKills.Clear();fusedOwners.Clear();actionSerial=0;
            Kills=Spawned=Launches=CollisionHits=CollisionKills=Ragdolls=PhysicsContacts=WorldContacts=LeftEntries=RightEntries=HorizonEntries=VisibleSpawns=PeakCrowd=HitsTaken=AbilityCasts=OrderErrors=0;
            CorpseShoves=ArmorBonusHits=0;ArmorPreventedDamage=ArmorHealing=0;FusionHistory.Clear();
            RangedSpawned=RangedHits=RearBoundaryKills=EnvironmentalKills=0;RangedDamage=0;
            for(int i=0;i<5;i++){SlotCasts[i]=0;WeaponHits[i]=0;}
            nextId=entryTurn=sequencePointer=0;ActiveSlot=-1;recovery=0;reported=false;qaDraftChecks=qaManualChecks=true;qaManualPhase=0;
            // Replace the kinematic motor between runs: a pending MovePosition target must
            // never carry the previous run's world position into the next physics step.
            hero.Root.gameObject.SetActive(false);Destroy(hero.Root.gameObject);
            hero=new FighterRig("The Walker",actors,true,lightweight:FastSimulation);hero.Root.position=new Vector3(0,.04f,0);
            heroPhysics=new FighterPhysics(hero,0,this,true);
            currentCredit=null;actionTarget=pairTarget=null;actionItem=pairItem=actionWeapon=pairWeapon=null;
            pendingReloads.Clear();reloadTimers.Clear();backwash.Clear();returnPoints.Clear();salvageChain=null;
            hero.Equipment(false,false);hero.EquipWeapons(WeaponKind.Fists,WeaponKind.Fists,false);hero.Pose(0,0,-1,0,0,false,0);
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;cameraVelocity=Vector3.zero;AimCamera();Physics.SyncTransforms();
            State=RunState.Drafting;PairSlot=-1;Preparation=0;Deck.Open(DraftKind.Ability,true);if(simulation)simulationClock.Restart();
        }
        void MergeKillCounts(LoadoutItem keep,LoadoutItem[] consumed)
        {
            foreach(var item in consumed){var from=ContributionFor(item);var to=ContributionFor(keep);to.direct+=from.direct;to.collision+=from.collision;to.control+=from.control;to.absorption+=from.absorption;to.augment+=from.augment;to.healing+=from.healing;to.assists+=from.assists;itemKills[keep]=KillsFor(keep)+KillsFor(item);itemKills.Remove(item);fusedOwners[item]=keep;}
            if(Deck.Items.Contains(keep)&&keep.rank>1)FusionHistory.Add(new FusionResult{definition=keep.def,before=keep.rank-1,after=keep.rank});
        }
        public void SelectOffer(int index){if(State==RunState.Drafting)Deck.Select(index);}
        public void AssignSlot(int slot){if(State==RunState.Drafting)Deck.Assign(slot);}
        public void Reroll(){if(State==RunState.Drafting)Deck.Reroll();}
        public void SwapSlots(int a,int b){if(State==RunState.Drafting)Deck.Swap(a,b);}
        public bool MoveItem(LoadoutItem item,PlaceKind place,int index=0)
        {
            if(State!=RunState.Drafting)return false;var previous=Deck.Ultimate;float maxBefore=MaxHealth;
            bool moved=Deck.Move(item,place,index);
            Health=Mathf.Clamp(Health+MaxHealth-maxBefore,0,MaxHealth);
            if(previous!=Deck.Ultimate){UltimateRemaining=UltimateCooldown;ultimateDue=Elapsed+UltimateCooldown;ultimateRequested=false;}
            return moved;
        }
        public bool ClaimReward(int offer,PlaceKind place,int index,bool continueImmediately=true)
        {
            if(State!=RunState.Drafting)return false;var previous=Deck.Ultimate;float maxBefore=MaxHealth;
            if(!Deck.ClaimTo(offer,place,index))return false;
            Health=Mathf.Clamp(Health+MaxHealth-maxBefore,0,MaxHealth);
            if(previous!=Deck.Ultimate){UltimateRemaining=UltimateCooldown;ultimateDue=Elapsed+UltimateCooldown;ultimateRequested=false;}
            if(continueImmediately)ConfirmDraft();return true;
        }
        public bool AutoEquip(LoadoutItem item)
        {
            Deck.SuggestedPlace(item.def,out var place,out int index);return index!=-2&&MoveItem(item,place,index);
        }
        public void AcceptSelectedReward(bool continueImmediately=true)
        {
            if(State!=RunState.Drafting)return;
            if(Deck.RewardClaimed){ConfirmDraft();return;}
            if(Deck.Selected<0)return;
            Deck.SuggestedPlace(Deck.Offers[Deck.Selected].def,out var place,out int index);
            if(index==-2)return;
            if(Deck.SelectedSlot>=0&&Deck.Offers[Deck.Selected].def.kind==ItemKind.Ability){place=PlaceKind.Sequence;index=Deck.SelectedSlot;}
            ClaimReward(Deck.Selected,place,index,continueImmediately);
        }
        public void ConfirmDraft()
        {
            if(State!=RunState.Drafting)return;
            if(!Deck.RewardClaimed){Deck.Claim();return;}
            CompleteDraft();
        }
        void BeginWave()
        {
            hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);
            if(qa)QABeginWeapons();
            WaveElapsed=0;accumulator=0;spawnTimer=.65f;ActiveSlot=PairSlot=-1;Preparation=0;sequencePointer=0;expectedSlot=Deck.NextOccupied(0);recovery=.15f;
            waveKills=Kills;waveLaunches=Launches;waveCollisions=CollisionHits;waveHits=HitsTaken;
            waveRangedSpawned=RangedSpawned;waveRangedHits=RangedHits;waveRangedDamage=RangedDamage;
            State=RunState.Running;
            if(Wave==1){SpawnEntry(0);SpawnEntry(1);SpawnEntry(2);}
        }
        public void ToggleUltimate(){UltimateAutomatic=!UltimateAutomatic;if(!UltimateAutomatic)ultimateRequested=false;}
        public void RequestUltimate(){if(State==RunState.Running&&Deck.Ultimate!=null&&UltimateRemaining<=0)ultimateRequested=true;}
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
            if(MechanicReview){StepMechanicReview(Time.unscaledDeltaTime);return;}
            if(!qa&&hud?.HandleOverlayInput()!=true)
            {
                if(Input.GetKeyDown(KeyCode.Return)&&(State==RunState.Ready||State==RunState.Complete||State==RunState.Defeated))Begin();
                if(Input.GetKeyDown(KeyCode.R))Begin();
                if(Input.GetKeyDown(KeyCode.T))Begin(true);
                if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape))TogglePause();
                if(Input.GetKeyDown(KeyCode.U))ToggleUltimate();
                if(Input.GetKeyDown(KeyCode.F))RequestUltimate();
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
                    if(Input.GetKeyDown(KeyCode.D))AssignSlot(3);
                    if(Input.GetKeyDown(KeyCode.C))AssignSlot(4);
                    if(Input.GetKeyDown(KeyCode.Tab))Reroll();
                    if(Input.GetKeyDown(KeyCode.Return))AcceptSelectedReward(!HoldCheckpointAfterPick);
                }
            }
            if(State==RunState.Running)
            {
                if(qa){for(int i=0;i<(FastSimulation?2048:Application.isBatchMode?8:1)&&State==RunState.Running;i++)AdvanceCombat(1f/60);}
                else AdvanceCombat(Time.unscaledDeltaTime);
            }
            if(qa&&!qaHolding&&State==RunState.Drafting){if(simulation)SimulationDraft();else QADraft();}
            if(qa&&!reported&&(State==RunState.Complete||State==RunState.Defeated))ReportQA();
            if(State==RunState.Ready){clock+=Time.deltaTime;hero.Pose(clock,0,-1,0,0,false,0);}
        }
        void AdvanceCombat(float realDelta)
        {
            if(State!=RunState.Running)return;
            float budget=Mathf.Min(realDelta,.1f)*CombatSpeed;
            if(hitStop>0){float used=Mathf.Min(hitStop,budget);hitStop-=used;budget-=used;}
            accumulator+=budget;
            const float dt=1f/60;
            while(accumulator+0.000001f>=dt&&State==RunState.Running)
            {
                accumulator=Mathf.Max(0,accumulator-dt);Step(dt);
                if(hitStop>0){float used=Mathf.Min(hitStop,accumulator);hitStop-=used;accumulator-=used;if(hitStop>0)break;}
            }
        }
        void Step(float dt)
        {
            float restored=Mathf.Min(MaxHealth-Health,Deck.Regeneration*dt);Health+=restored;ArmorHealing+=restored;
            Score.Step(dt);clock+=dt;Elapsed+=dt;WaveElapsed+=dt;EventAge+=dt;flash=Mathf.Max(0,flash-dt*2);shake=Mathf.Max(0,shake-dt*2);heroReact=Mathf.Max(0,heroReact-dt*4);
            if(qaBuild=="manual"&&qa)QAManual();
            if(Deck.Ultimate!=null&&ActiveSlot!=5)UltimateRemaining=Mathf.Max(0,UltimateRemaining-dt);
            heroPhysics.MovePosition(HeroPosition+Vector3.forward*tuning.walkSpeed*dt*(ActiveSlot>=0?.62f:1));
            spawnTimer-=dt;
            if(spawnTimer<=0&&!CleaningUp&&enemies.Count<tuning.maximumEnemies)
            {
                SpawnEntry(entryTurn++%3);if((Wave==1||Wave==3||Wave==6)&&entryTurn%4==0)SpawnEntry(entryTurn++%3);
                float pressure=((Wave-1)+Mathf.Clamp01(WaveElapsed/tuning.waveArrivals))/tuning.waveCount;
                spawnTimer=Mathf.Lerp(tuning.firstSpawnInterval,tuning.finalSpawnInterval,pressure)*SpawnRange(.85f,1.1f);
            }
            long perf=System.Diagnostics.Stopwatch.GetTimestamp();StepSignatures(dt);ProfileSim(0,ref perf);StepEnemies(dt);ProfileSim(1,ref perf);StepHero(dt);ProfileSim(2,ref perf);if(qa&&!simulation)QACheckWeapons();if(FastSimulation)StepFastPhysics(dt);else{ShoveWalkingBodies(dt);Physics.SyncTransforms();Physics.Simulate(dt);}ProfileSim(3,ref perf);ProcessContacts();ProfileSim(4,ref perf);
            StepBodies(dt);fx.Step(dt);UpdateCamera(dt);ResolveRearBoundary();ProfileSim(5,ref perf);if(simulation)ValidateSimulationStep(dt);
            CrowdNear=0;foreach(var e in enemies)if(Vector3.Distance(e.rig.Root.position,HeroPosition)<6)CrowdNear++;
            PeakCrowd=Mathf.Max(PeakCrowd,CrowdNear);
            if(Health<=0){Health=0;Score.Bank();State=RunState.Defeated;Announce("OVERRUN");}
            else if(CleaningUp&&(enemies.Count==0||WaveElapsed>=tuning.waveArrivals+tuning.cleanupWindow)&&!(Wave==8&&enemies.Exists(e=>e.boss)))FinishWave();
        }
        void SpawnEntry(int kind)
        {
            if(enemies.Count>=tuning.maximumEnemies)return;
            EntryLayout.Door door=kind<2?EntryLayout.Ahead(kind==0?-1:1,HeroPosition.z):null;
            Vector3 p=door!=null?door.origin:EntryLayout.Horizon(cameraRig,HeroPosition,SpawnRange(-6,6));
            Physics.SyncTransforms();
            if(!EntryLayout.Concealed(cameraRig,p)){door=null;p=EntryLayout.Horizon(cameraRig,HeroPosition,SpawnRange(-6,6));}
            if(!EntryLayout.Concealed(cameraRig,p))throw new InvalidOperationException("No concealed entrance was available.");
            bool boss=Wave==8&&!bossSpawned;bool heavy=boss||(Wave>=3&&spawnRandom.NextDouble()<(Wave%3==0?.28:.16));bool shooter=!heavy&&Wave>=2&&spawnRandom.NextDouble()<ShooterChance(Wave);if(boss)bossSpawned=true;
            var e=new Enemy{id=++nextId,heavy=heavy,boss=boss,shooter=shooter,health=tuning.enemyHealth*(boss?24:heavy?1.9f:1)*(1+.035f*(Wave-1)),cooldown=CombatRange(.3f,1.4f),radius=CombatRange(2.4f,3.6f),orbit=combatRandom.NextDouble()<.5?-1:1,phase=CombatRange(0,6.28f),entering=door!=null,doorway=door!=null?door.exit:Vector3.zero};
            if(shooter)e.radius=6.5f;e.maxHealth=e.health;e.rig=new FighterRig(boss?"Warden":shooter?"Shooter":heavy?"Enforcer":"Raider",actors,false,heavy,e.id^Seed,FastSimulation);e.rig.Root.position=p;if(boss)e.rig.Root.localScale*=1.4f;
            e.physics=new FighterPhysics(e.rig,e.id,this,false,heavy);enemies.Add(e);byId.Add(e.id,e);Spawned++;
            if(shooter)RangedSpawned++;
            if(!FastSimulation)foreach(var body in corpses)body.physics.IgnoreWalker(e.physics,!flights.Exists(f=>f.body==body));
            if(door==null)HorizonEntries++;else if(door.side<0)LeftEntries++;else RightEntries++;
            if(!EntryLayout.Concealed(cameraRig,p))VisibleSpawns++;
        }
        void StepEnemies(float dt)
        {
            int committed=0;foreach(var e in enemies){e.cachedPosition=e.rig.Root.position;if(e.engaged)committed++;}
            foreach(var e in enemies.ToArray())
            {
                Vector3 p=e.cachedPosition,delta=HeroPosition-p;delta.y=0;float distance=delta.magnitude;Vector3 inward=distance>.01f?delta/distance:Vector3.back;
                e.cooldown-=dt;e.stun=Mathf.Max(0,e.stun-dt);e.react=Mathf.Max(0,e.react-dt*3);
                if(e.stun>0){e.engaged=false;e.attack=-1;}
                if(e.entering&&Mathf.Abs(p.x)<8.2f)e.entering=false;
                UpdateEnemyRepositioning(e);
                bool canAttack=EnemyCanAttack(e);
                if(!canAttack&&(e.engaged||e.attack>=0)){if(e.engaged)committed--;e.engaged=false;e.attack=-1;e.delivered=false;e.cooldown=Mathf.Max(e.cooldown,.35f);}
                if(canAttack&&!e.engaged&&e.stun<=0&&e.cooldown<=0&&distance<(e.shooter?9:3.3f)&&committed<tuning.simultaneousAttackers)
                {
                    bool opening=true;foreach(var other in enemies)if(other!=e&&other.engaged&&Vector3.Angle(other.cachedPosition-HeroPosition,p-HeroPosition)<34){opening=false;break;}
                    if(opening){e.engaged=true;committed++;}
                }
                Vector3 desired;
                if(e.stun>0)desired=Vector3.zero;
                else if(e.entering){var toward=e.doorway-p;toward.y=0;desired=toward.normalized*4.5f;}
                else if(e.repositioning)desired=RepositionVelocity(e);
                else if(distance>15)desired=inward*9f;
                else if(e.engaged)desired=inward*Mathf.Clamp((distance-(e.shooter?6.5f:1.3f))*4,-1.2f,tuning.enemySpeed);
                else if(distance>e.radius+.7f)desired=inward*tuning.enemySpeed;
                else desired=inward*Mathf.Clamp((distance-e.radius-.15f*Mathf.Sin(clock+e.phase))*2,-1.6f,1.6f)+new Vector3(-inward.z,0,inward.x)*e.orbit*.85f;
                if(e.heavy)desired*=.82f;
                if(!e.entering&&e.stun<=0)
                {
                    foreach(var other in enemies){if(other==e)continue;Vector3 diff=p-other.cachedPosition;diff.y=0;float d=diff.magnitude;if(d>.001f&&d<1.1f)desired+=diff/d*(1.1f-d)*2;}
                }
                Vector3 velocity=e.physics.LinearVelocity;
                Vector3 flat=new Vector3(velocity.x,0,velocity.z);
                flat=Vector3.Lerp(flat,desired,1-Mathf.Exp(-(e.stun>0?1.4f:7)*dt));
                e.physics.LinearVelocity=new Vector3(flat.x,velocity.y,flat.z);
                Vector3 facing=e.entering?desired:inward;
                if(e.stun<=0&&facing.sqrMagnitude>.01f)e.physics.MoveRotation(Quaternion.Slerp(e.rig.Root.rotation,Quaternion.LookRotation(facing),1-Mathf.Exp(-10*dt)));
                if(canAttack&&e.engaged&&distance<(e.shooter?9:tuning.attackRadius)&&e.attack<0){e.attack=0;e.delivered=false;}
                if(e.attack>=0)
                {
                    float duration=e.shooter?1.5f:e.heavy?1.25f:.95f;if(e.shooter&&!e.delivered&&e.attack>duration*.2f&&((int)(e.attack*10)!=(int)((e.attack-dt)*10)))fx.Arc(e.physics.Center,HeroPosition+Vector3.up,new Color(1,.4f,.2f));e.attack+=dt;
                    if(!e.delivered&&e.attack>=duration*.53f)
                    {
                        e.delivered=true;
                        if(e.stun<=0&&EnemyCanAttack(e)&&Vector3.Distance(e.rig.Root.position,HeroPosition)<(e.shooter?9:tuning.attackRadius+.25f))
                        {ReceiveDamage(e,tuning.enemyDamage*(e.heavy?1.8f:1));}
                    }
                    if(e.attack>=duration){e.attack=-1;e.engaged=false;e.cooldown=CombatRange(e.shooter?2:.9f,e.shooter?3:1.8f);}
                }
                e.rig.Pose(clock,flat.magnitude/tuning.enemySpeed,e.attack>=0?1:-1,e.attack/(e.heavy?1.25f:.95f),e.react,false,0);
                StepReaction(e,dt,true);
                e.rig.Telegraph(e.attack>=0&&!e.delivered,e.stun>0);
            }
        }
        [Serializable] public sealed class WaveReport
        {public int wave,kills,launches,collisions,carried,hitsTaken,shooters,rangedHits;public float health,rangedDamage;public string choice="";}
        void FinishWave()
        {
            if(simulation)Debug.Log("SIM_WAVE seed="+Seed+" wave="+Wave+" elapsed="+Elapsed+" kills="+Kills+" enemies="+enemies.Count+" hero="+HeroPosition);
            Score.Bank();
            Waves.Add(new WaveReport{wave=Wave,kills=Kills-waveKills,launches=Launches-waveLaunches,collisions=CollisionHits-waveCollisions,carried=enemies.Count,hitsTaken=HitsTaken-waveHits,health=Health,shooters=RangedSpawned-waveRangedSpawned,rangedHits=RangedHits-waveRangedHits,rangedDamage=RangedDamage-waveRangedDamage});
            accumulator=0;ActiveSlot=PairSlot=-1;Preparation=0;
            if(Wave>=tuning.waveCount){State=RunState.Complete;return;}
            OpenCheckpoint();
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
