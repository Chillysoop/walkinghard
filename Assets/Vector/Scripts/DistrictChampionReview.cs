using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] sealed class ChampionObservation
        {
            public int kind,wave,cycles,attacks,hits,breaks,guardedHits;public bool defeated;
        }
        ChampionObservation[] ObserveChampions()=>enemies.Concat(corpses).Where(e=>e.durability?.champion==true).Select(e=>new ChampionObservation{
            kind=e.districtChampion?.kind??0,wave=e.durability.wave,defeated=e.deathAge>=0,cycles=e.districtChampion?.cycles??e.durability.starts,
            attacks=e.districtChampion?.attacks??e.crewUses,hits=e.districtChampion?.hits??0,breaks=e.districtChampion?.breaks??e.durability.interrupts,guardedHits=e.districtChampion?.guardedHits??0}).ToArray();
        [Serializable] sealed class ChampionCase
        {
            public string build,result,loadout;public int wave,seed,visibleSpawns,orderErrors;public bool valid;
            public float seconds,health,damageTaken,healing;public ChampionObservation champion;
        }
        [Serializable] sealed class ChampionReviewReport
        {
            public string scope,failure;public bool passed,original;public int assertions;
            public List<string> checks=new List<string>();public List<ChampionCase> cases=new List<ChampionCase>();
        }
        IEnumerator ReviewDistrictChampions()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);FastSimulation=false;
            var args=Environment.GetCommandLineArgs();bool balance=args.Contains("-championBalance"),render=!args.Contains("-nographics");
            string Arg(string key,string fallback){int at=Array.IndexOf(args,key);return at<0?fallback:args[at+1];}
            if(!IsolatedProfile)throw new InvalidOperationException("Champion review requires an isolated profile");
            originalChampions=args.Contains("-originalChampions");forcedSeed=true;tuning.seed=int.Parse(Arg("-seed","1741"));
            string output=Path.GetFullPath(Arg("-reviewOutput","ChampionReview"));Directory.CreateDirectory(output);
            var report=new ChampionReviewReport{original=originalChampions,scope=balance?"Normal health, full PhysX, eight fixed Rare-weapon / Uncommon-support kits through an entire Elite Shift in each district. Actual approaches, ordinary arrivals and rewards. No augments. Not natural loot or human win rates.":"Production champion counters, attack geometry, single-hit delivery, physical launches, concealed spawning, save migration and truthful road previews. Elevated health in mechanics fixtures is not difficulty evidence."};
            void Write()=>File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
            void Require(bool ok,string name){if(!ok){report.failure=name;Write();throw new InvalidOperationException("CHAMPION_REVIEW: "+name);}report.assertions++;report.checks.Add(name);Write();}
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception){report.failure=m;Write();Application.Quit(3);}};
            IEnumerator Shot(string name){if(!render)yield break;fx.Step(.01f);yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());Destroy(t);yield return null;}
            hud.TrailerCaptureMode();hud.TrailerClearSelection();QualitySettings.vSyncCount=0;Application.targetFrameRate=render?60:-1;
            if(render)Screen.SetResolution(1920,1080,false);for(int i=0;i<8;i++)yield return null;
            if(balance)
            {
                foreach(int wave in new[]{3,10,17})foreach(string build in ForemanBuilds)
                {
                    StartChapter();EquipForemanKit(build);Wave=wave;expeditionPlan.waves[wave-1]=EncounterRoute.EliteShift;BeginWave();
                    float seconds=0,start=Time.realtimeSinceStartup;
                    while(State==RunState.Running&&seconds<180&&Time.realtimeSinceStartup-start<240)
                    {for(int i=0;i<90&&State==RunState.Running;i++){AdvanceCombat(1f/60);seconds+=1f/60;}yield return null;}
                    var champion=ObserveChampions().SingleOrDefault();
                    var row=new ChampionCase{build=build,wave=wave,seed=Seed,result=State==RunState.Drafting?"Cleared":State==RunState.Defeated?"Defeat":"Timeout",seconds=seconds,health=Health,damageTaken=RunHistory.sources.Sum(s=>s.damage),healing=contributions.Sum(p=>p.Value.healing),champion=champion,loadout=Deck.Loadout,visibleSpawns=VisibleSpawns,orderErrors=OrderErrors};
                    row.valid=row.result!="Timeout"&&champion!=null&&(row.result!="Cleared"||champion.defeated)&&VisibleSpawns==0&&OrderErrors==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills;
                    report.cases.Add(row);Write();Require(row.valid,"valid elite encounter / "+wave+" / "+build);Debug.Log("CHAMPION_CASE "+wave+" "+build+" "+row.result+" seconds="+seconds);
                }
            }
            else
            {
                originalChampions=false;tuning.heroHealth=10000;
                Enemy Setup(int kind,float z=2.4f)
                {
                    StartChapter();ProbeReset();OpeningPicks=2;Wave=3+kind*7;expeditionPlan.waves[Wave-1]=EncounterRoute.EliteShift;BeginWave();
                    WaveElapsed=5;SpawnEntry(2);spawnTimer=10000;Health=10000;
                    var e=enemies.Single(a=>a.durability?.champion==true);Require(Mathf.Abs(e.maxHealth-tuning.enemyHealth*5.5f*EnemyHealthScale)<.01f,"champion keeps authored 5.5x ordinary health / "+kind);e.health=e.maxHealth=4000;e.entering=e.repositioning=false;e.stun=0;e.cooldown=100;
                    e.rig.Root.position=HeroPosition+RoadForward*z;e.physics.Motor.position=e.rig.Root.position;e.rig.Root.rotation=Quaternion.LookRotation(-RoadForward);
                    cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();Physics.SyncTransforms();
                    currentCredit=new KillCredit{item=Deck.Fists,action=++actionSerial};return e;
                }
                void Frames(int count){for(int f=0;f<count;f++){float dt=1f/60;Elapsed+=dt;WaveElapsed+=dt;clock+=dt;StepEnemies(dt);Physics.SyncTransforms();Physics.Simulate(dt);fx.Step(dt);}}
                Enemy Ally(float x,float z){var e=ProbeEnemy(x,z,500);e.cooldown=100;Spawned++;return e;}
                foreach(int kind in new[]{0,1,2})
                {
                    var e=Setup(kind);Require((e.districtChampion?.kind??0)==kind&&VisibleSpawns==0,"district chooses its champion through concealed production spawn / "+kind);
                    Require(e.heavy&&e.rig.Root.localScale==Vector3.one*1.3f,"champions retain existing heavy physics envelope / "+kind);
                    Require(kind==0?e.crew==CrewRole.Freight:e.crew==CrewRole.None,"only Freight Champion retains body-throw crew behavior / "+kind);
                    Require(ThreatReadouts().Any(v=>v.champion&&v.title==ChampionTitle(kind)),"readout names the actual champion / "+kind);
                }
                var captain=Setup(1);var a=Ally(-2,3);var b=Ally(2,3);BeginChampionAttack(captain);Frames(1);
                Require(captain.districtChampion.links.Count==2&&CaptainGuardFor(a)==captain&&CaptainGuardFor(b)==captain,"Captain snapshots two visible escorts");
                float health=a.health;Hurt(a,100,Vector3.zero,0);Require(Mathf.Abs(health-a.health-65)<.01f,"projected guard reduces escort damage by 35 percent");
                Require(CaptainGuardFor(captain)==null&&ThreatReadouts().Any(v=>v.protection=="CAPTAIN GUARD · 35%"),"guard is separately labeled and never self-protects");
                a.shieldBearer=true;Require(ThreatReadouts().Any(v=>v.protection=="CAPTAIN 35% · FRONT 50%"),"projected and frontal guard are both labeled when protection stacks");
                a.shieldOpenUntil=Elapsed+2;Require(ThreatReadouts().Any(v=>v.protection=="CAPTAIN 35% · GUARD OPEN"),"opened frontal guard does not hide remaining projected protection");a.shieldBearer=false;
                yield return Shot("01-captain-guard");
                currentCredit.action=++actionSerial;Control(captain,.1f,Vector3.zero);float pressure=captain.durability.pressure;Control(captain,.1f,Vector3.zero);
                Require(captain.districtChampion.phase==1&&captain.stun==0&&captain.durability.pressure==pressure,"paired light stagger shares pressure and does not cancel the guard");
                Launch(captain);Require(captain.districtChampion.phase==3&&CaptainGuardFor(a)==null&&flights.Any(f=>f.body==captain),"launch immediately removes guard and preserves physical flight");
                health=a.health;Hurt(a,100,Vector3.zero,0);Require(Mathf.Abs(health-a.health-100)<.01f,"unshielded escort receives full damage immediately");
                health=captain.health;Hurt(captain,8,Vector3.zero,0);Require(Mathf.Abs(health-captain.health-10)<.01f,"exposed Captain keeps the shared 25 percent damage window");yield return Shot("02-captain-launched-open");
                captain=Setup(1);a=Ally(-2,3);b=Ally(2,3);BeginChampionAttack(captain);KillEnemy(a,Vector3.zero,false);KillEnemy(b,Vector3.zero,false);Frames(1);
                Require(captain.districtChampion.phase==3&&captain.districtChampion.breaks==1,"clearing the escort group opens the Captain early");
                captain=Setup(1);BeginChampionAttack(captain);Frames(282);Require(captain.districtChampion.phase==2,"escort-free Captain telegraphs a bash instead of stalling");yield return Shot("03-captain-bash");
                health=Health;Frames(65);Require(captain.districtChampion.phase==3&&captain.districtChampion.attacks==1&&captain.districtChampion.hits==1&&Mathf.Abs(health-Health-tuning.enemyDamage*1.8f)<.01f,"Captain bash hits once then opens naturally");yield return Shot("04-captain-recovery");
                var interceptor=Setup(2,6);BeginChampionAttack(interceptor);Frames(20);var aim=interceptor.districtChampion.aim;yield return Shot("05-interceptor-warning");
                heroPhysics.Motor.position=HeroPosition+Vector3.right*4;Physics.SyncTransforms();health=Health;Frames(135);
                Require(interceptor.districtChampion.aim==aim&&interceptor.districtChampion.phase==3&&interceptor.districtChampion.attacks==1&&interceptor.districtChampion.hits==0&&Health==health,"Interceptor keeps its marked direction and can miss a displaced Walker");yield return Shot("06-interceptor-missed-recovery");
                interceptor=Setup(2,6);BeginChampionAttack(interceptor);health=Health;Frames(155);
                Require(interceptor.districtChampion.phase==3&&interceptor.districtChampion.hits==1&&Mathf.Abs(health-Health-tuning.enemyDamage*2.2f)<.01f,"actual swept charge hits once, then exposes the Interceptor");
                interceptor=Setup(2,6);BeginChampionAttack(interceptor);Launch(interceptor);health=Health;Frames(130);
                Require(interceptor.districtChampion.phase==3&&interceptor.districtChampion.attacks==0&&Health==health,"launch cancels a pending charge without delayed damage");
                interceptor=Setup(2,6);BeginChampionAttack(interceptor);
                for(int i=0;i<3;i++){currentCredit.action=++actionSerial;ChampionDamage(interceptor,1);currentCredit.hop=1;ChampionDamage(interceptor,1);ChampionDamage(interceptor,1);}
                Require(interceptor.districtChampion.phase==3,"three distinct body-impact actions break brace pressure");
                interceptor=Setup(2,6);BeginChampionAttack(interceptor);float age=interceptor.districtChampion.age;TogglePause();AdvanceCombat(.5f);
                Require(interceptor.districtChampion.age==age&&State==RunState.Paused,"pause freezes champion wind-up");TogglePause();
                interceptor.rig.Root.position=HeroPosition+RoadForward*40;interceptor.physics.Motor.position=interceptor.rig.Root.position;Physics.SyncTransforms();health=Health;Frames(1);
                Require(interceptor.districtChampion.phase==0&&Health==health,"offscreen wind-up cancels before damage");
                interceptor=Setup(2,6);BeginChampionAttack(interceptor);Frames(92);
                var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Champion fixture wall";wall.layer=EntryLayout.WorldLayer;wall.transform.position=new Vector3(0,2,3.5f);wall.transform.localScale=new Vector3(10,4,.5f);Physics.SyncTransforms();health=Health;Frames(65);
                Require(interceptor.rig.Root.position.z>3.5f&&Health==health&&interceptor.districtChampion.phase==3,"physical wall stops a committed charge without remote damage");DestroyImmediate(wall);
                interceptor=Setup(2,6);var junction=roads.Split(Vector3.zero,0);roads.Follow(junction,0);
                heroPhysics.Motor.position=junction.Left.Point(5)+Vector3.up*.04f;
                interceptor.rig.Root.position=junction.Right.Point(7.5f)+Vector3.up*.04f;interceptor.physics.Motor.position=interceptor.rig.Root.position;interceptor.physics.LinearVelocity=Vector3.zero;
                Physics.SyncTransforms();cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
                Require(EnemyCanAttack(interceptor)&&roads.Navigation(interceptor.rig.Root.position,HeroPosition,out _),"physical junction fixture has a visible target and a competing navigation waypoint");
                BeginChampionAttack(interceptor);var lockedOrigin=interceptor.rig.Root.position;Frames(20);
                var drift=interceptor.rig.Root.position-lockedOrigin;drift.y=0;
                Require(interceptor.districtChampion.phase==1&&drift.magnitude<.1f,"junction pathfinding cannot move a committed wind-up off its warning lane");
                Frames(74);var chargeDirection=interceptor.districtChampion.aim-interceptor.districtChampion.origin;chargeDirection.y=0;
                var velocity=interceptor.physics.LinearVelocity;velocity.y=0;
                Require(interceptor.districtChampion.phase==2&&Vector3.Dot(velocity.normalized,chargeDirection.normalized)>.98f,"physical junction charge follows its fixed aim instead of the navigation waypoint");
                foreach(int kind in new[]{1,2})
                {
                    var e=Setup(kind,kind==1?2.4f:6);if(kind==1){Ally(-2,3);Ally(2,3);}BeginChampionAttack(e);Frames(20);
                    Deck.Open(DraftKind.Mixed,false);State=RunState.Drafting;Require(SaveCheckpoint(),"champion snapshot writes / "+kind);string graph=JsonUtility.ToJson(CaptureCheckpoint().graph);
                    Require(ResumeExpedition()&&JsonUtility.ToJson(CaptureCheckpoint().graph)==graph,"champion phase, links and aim restore exactly / "+kind);
                    e=enemies.Single(v=>v.districtChampion?.kind==kind);Require(e.rig.Root.GetComponentsInChildren<Transform>().Any(t=>t.name==(kind==1?"Captain shield projector":"Interceptor shoulder ram")),"restored champion retains its distinct silhouette / "+kind);
                    State=RunState.Running;int breaks=e.districtChampion.breaks;KillEnemy(e,Vector3.zero,false);Launch(e);Require(e.districtChampion.breaks==breaks,"corpse launch cannot create a false opening / "+kind);
                }
                var freight=Setup(0);State=RunState.Drafting;var saved=CaptureCheckpoint();saved.schema=10;
                foreach(var node in saved.graph.objects.Where(o=>o.type==typeof(Enemy).FullName))node.fields.RemoveAll(f=>f.name=="districtChampion");AtomicWrite(SavePath,JsonUtility.ToJson(saved));
                Require(ResumeExpedition()&&enemies.Single().districtChampion==null&&enemies.Single().crew==CrewRole.Freight,"schema 10 retains Freight Champion identity without inventing a new phase");
                bool rejected=false;try{GraphCodec().Decode(saved.graph);}catch(InvalidOperationException){rejected=true;}Require(rejected,"current schema rejects a missing required champion field");
                foreach(int wave in new[]{2,9,16,6,13,20})
                {
                    StartChapter();ProbeReset();Wave=wave;OpeningPicks=2;BeginWave();spawnTimer=10000;
                    int fork=ChapterEncounter.fork;expeditionPlan.left[fork]=EncounterRoute.EliteShift;expeditionPlan.right[fork]=EncounterRoute.SalvageStop;BeginWalkingFork();
                    bool boss=(wave+1)%7==0;
                    Require(ForkChampionKind==(wave/7)&&ForkLeadsToBoss==boss,"fork preview uses destination encounter / "+wave);
                    Require(boss?ForkThreatLabel(ForkLeft)=="HEAVY ESCORT"&&ForkRewardLabel(ForkLeft)=="BOSS ESCORT":ForkRewardLabel(ForkLeft)=="+6 SALVAGE","fork promises champion salvage only when a champion will spawn / "+wave);
                    if(render)
                    {
                        hud.championTextAudit=()=>Require(hud.ChampionPreviewFits(ForkThreatLabel(ForkLeft),ForkRewardLabel(ForkLeft)),"champion name and reward fit the existing sign / "+wave);
                        float deadline=Time.realtimeSinceStartup+3;
                        while(hud.championTextAudit!=null&&Time.realtimeSinceStartup<deadline)yield return null;
                        Require(hud.championTextAudit==null,"rendered sign text audit completed / "+wave);
                    }
                    Require(hud.HandleForkPointer(EventType.MouseDown,hud.ForkChoiceRect(1,1600,900).center,1600,900)&&ForkChoice==ForkRight,"sign click still selects and highlights the actual road / "+wave);
                    ChooseRoute(ForkLeft);CommitWalkingFork();Require(SelectedRoute==EncounterRoute.EliteShift&&roads.Path.Contains(forkVisual.Left)&&!ChooseRoute(ForkRight),"deadline commits the highlighted physical branch / "+wave);
                    if(!boss)
                    {
                        // Reopen only the fixture's sign for an inspectable view of both roads.
                        RoutePending=true;forkLockedAge=0;forkAge=1;
                        heroPhysics.Motor.position=forkOrigin-RoadForward*5;Physics.SyncTransforms();cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
                        yield return Shot("fork-"+(wave/7+1)+"-1080p");
                    }
                }
                if(render)
                {
                    Screen.SetResolution(1280,720,false);ReducedMotion=true;fx.SetReducedMotion(true);for(int i=0;i<10;i++)yield return null;
                    captain=Setup(1);Ally(-2,3);Ally(2,3);BeginChampionAttack(captain);Frames(20);yield return Shot("captain-720p-reduced-motion");
                    interceptor=Setup(2,6);BeginChampionAttack(interceptor);Frames(20);yield return Shot("interceptor-720p-reduced-motion");
                    StartChapter();ProbeReset();OpeningPicks=2;Wave=16;BeginWave();expeditionPlan.left[6]=EncounterRoute.EliteShift;expeditionPlan.right[6]=EncounterRoute.SalvageStop;BeginWalkingFork();
                    heroPhysics.Motor.position=forkOrigin-RoadForward*5;Physics.SyncTransforms();cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();hud.liveStatsOpen=true;yield return Shot("fork-720p-live-stats");
                }
            }
            report.passed=true;Write();Debug.Log("CHAMPION_REVIEW_SUCCESS "+report.assertions);Application.Quit(0);
        }
    }
    public sealed partial class PrototypeHUD
    {
        internal Action championTextAudit;
        void ReviewChampionPreviewText(){if(championTextAudit==null)return;var audit=championTextAudit;championTextAudit=null;audit();}
        internal bool ChampionPreviewFits(string title,string reward)=>Style(14,true).CalcHeight(new GUIContent(title.Replace(" ","\n")),150)<=34&&Style(12,true).CalcSize(new GUIContent(reward)).x<=150;
    }
}
