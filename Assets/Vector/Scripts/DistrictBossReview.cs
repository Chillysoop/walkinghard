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
        [Serializable] sealed class JourneyBossObservation
        {public int kind,cycles,strikes,breaks,rallies;public bool defeated;}
        JourneyBossObservation[] ObserveJourneyBosses()=>enemies.Concat(corpses).Where(e=>e.journeyBoss!=null).OrderBy(e=>e.journeyBoss.kind).Select(e=>new JourneyBossObservation{kind=e.journeyBoss.kind,cycles=e.journeyBoss.cycles,strikes=e.journeyBoss.strikes,breaks=e.journeyBoss.kind==0?e.foreman.forcedVents:e.journeyBoss.breaks,rallies=e.journeyBoss.rallies,defeated=e.deathAge>=0}).ToArray();
        [Serializable] sealed class DistrictBossCase
        {
            public string build,boss,result,loadout,context,timeoutReason;public int rank;public ContributionReport[] items;public int wave,seed,strikes,breaks,rallies,cycles,visibleSpawns,orderErrors;
            public float seconds,health,damageTaken,healing,wait;public bool valid;public JourneyPressure pressure;
        }
        [Serializable] sealed class DistrictBossReport
        {
            public bool passed;public int assertions;public string scope,failure;
            public List<string> checks=new List<string>();public List<DistrictBossCase> cases=new List<DistrictBossCase>();
        }
        IEnumerator ReviewDistrictBosses()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);FastSimulation=false;
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();bool balance=args.Contains("-districtBalance"),render=!args.Contains("-nographics");originalBellThreshold=args.Contains("-originalBellThreshold");
            string Arg(string key,string fallback){int at=Array.IndexOf(args,key);return at<0?fallback:args[at+1];}
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("District review requires an isolated profile");
            string output=Path.GetFullPath(Arg("-reviewOutput","DistrictBossReview"));Directory.CreateDirectory(output);
            var report=new DistrictBossReport{scope=balance?"Full PhysX, normal health, fixed Rare weapons / Uncommon supports and ultimates, eight authored kits against each journey boss. Real approaches and reinforcement budgets. No augments. Not natural-loot or human win rates.":"Controlled boss attacks, counterplay, geometry, visibility, pause, checkpoint migration and rendered presentation. Elevated health in mechanics fixtures is not balance evidence."};
            void Write()=>File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
            void Require(bool ok,string rule){if(!ok){report.failure=rule;Write();throw new InvalidOperationException("DISTRICT_BOSS_REVIEW: "+rule);}report.assertions++;report.checks.Add(rule);}
            IEnumerator Shot(string name){if(!render)yield break;fx.Step(.01f);yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());Destroy(t);yield return null;}
            var hud=FindFirstObjectByType<PrototypeHUD>();hud.TrailerCaptureMode();hud.TrailerClearSelection();
            QualitySettings.vSyncCount=0;Application.targetFrameRate=render?60:-1;if(render)Screen.SetResolution(1920,1080,false);for(int i=0;i<8;i++)yield return null;
            if(balance)
            {
                int seed=int.Parse(Arg("-seed","1741"));forcedSeed=true;tuning.seed=seed;
                string casePath=Arg("-districtCases","");var custom=string.IsNullOrEmpty(casePath)?null:JsonUtility.FromJson<ControlledCases>(File.ReadAllText(casePath)).cases;
                if(custom!=null)report.scope="Full PhysX; explicit equal-rarity kits, full normal starting health and full ultimate countdown. Three current journey bosses with approaches and reinforcements. Combat timeouts remain separate from defeats. Not ordinary draft win rates.";
                var matrix=custom??ForemanBuilds.Select(id=>new ControlledCase{id=id,seed=seed}).ToArray();
                foreach(int wave in new[]{7,14,21})foreach(var plan in matrix)
                {
                    string build=plan.id;seed=plan.seed;tuning.seed=seed;StartChapter();if(custom==null)EquipForemanKit(build);else EquipDistrictComparison(plan);Wave=wave;BeginWave();if(State==RunState.Revealing)StepFinaleReveal(RevealDuration);
                    var boss=JourneyBoss;float seconds=0,start=Time.realtimeSinceStartup;
                    while(State==RunState.Running&&seconds<180&&Time.realtimeSinceStartup-start<180)
                    {for(int i=0;i<90&&State==RunState.Running;i++){AdvanceCombat(1f/60);seconds+=1f/60;}yield return null;}
                    var b=boss.journeyBoss;bool defeated=boss.deathAge>=0;
                    var row=new DistrictBossCase{context=plan.context,rank=custom==null?0:plan.rank,items=ContributionReports(),timeoutReason=defeated||State==RunState.Defeated?"":seconds>=180?"combat limit":"wall limit",build=build,boss=LongJourney.Encounters[wave-1].name,wave=wave,seed=seed,result=defeated?"Boss defeated":State==RunState.Defeated?"Defeat":"Timeout",seconds=seconds,health=Health,damageTaken=RunHistory.sources.Sum(s=>s.damage),healing=contributions.Sum(p=>p.Value.healing),wait=contributions.Sum(p=>p.Value.rangeWait),strikes=b.strikes,breaks=b.breaks,rallies=b.rallies,cycles=b.cycles,visibleSpawns=VisibleSpawns,orderErrors=OrderErrors,loadout=Deck.Loadout,pressure=JourneyPacing.encounters.LastOrDefault()};
                    if(b.kind==0)row.breaks=boss.foreman.forcedVents;
                    row.valid=row.result!="Timeout"&&VisibleSpawns==0&&OrderErrors==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills;report.cases.Add(row);Write();
                    Require(custom==null?row.valid:row.timeoutReason!="wall limit"&&VisibleSpawns==0&&OrderErrors==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills,"valid engine state "+row.boss+" / "+build);Debug.Log("DISTRICT_BOSS_CASE "+wave+" "+build+" "+row.result+" seconds="+seconds);
                }
            }
            else
            {
                forcedSeed=true;tuning.seed=int.Parse(Arg("-seed","4319"));tuning.heroHealth=10000;CheckJourneyPacing(Require);
                for(int seed=1;seed<=128;seed++)
                {
                    var plan=LongJourney.Plan(seed);Require(plan.Valid(),"valid district itinerary / "+seed);
                    for(int d=0;d<3;d++)Require(plan.waves.Skip(d*7).Take(6).OrderBy(x=>x).SequenceEqual(DistrictEncounters.Rosters[d].OrderBy(x=>x)),"district roster retained / "+seed+" / "+d);
                    Require(plan.waves.Take(2).All(r=>r!=EncounterRoute.EliteShift),"opening excludes champions / "+seed);
                }
                for(int d=0;d<3;d++)Require(Mathf.Abs(Enumerable.Range(0,4).Sum(i=>DistrictEncounters.Gap(d,i))-4)<.001f,"district cadence preserves mean arrival interval / "+d);
                Require(Enumerable.Range(0,12).Count(i=>DistrictEncounters.Specialist(9,i)==CrewRole.Bellkeeper)==8,"foundry emphasizes Bellkeepers");
                yield return CheckDistrictRearDoors(Require);
                Enemy Setup(int wave)
                {
                    StartChapter();ProbeReset();OpeningPicks=2;Wave=wave;BeginWave();Health=10000;spawnTimer=10000;
                    if(State==RunState.Revealing)StepFinaleReveal(RevealDuration);
                    var e=JourneyBoss;e.rig.Root.position=HeroPosition+EncounterForward*2.4f;e.physics.Motor.position=e.rig.Root.position;e.stun=0;e.cooldown=0;
                    if(e.foreman!=null)e.foreman.stage=1;Physics.SyncTransforms();return e;
                }
                void BossFrames(Enemy e,int frames){for(int i=0;i<frames;i++){float dt=1f/60;Elapsed+=dt;WaveElapsed+=dt;clock+=dt;if(e.foreman!=null)StepForeman(e,dt);else StepPursuer(e,dt);Physics.SyncTransforms();Physics.Simulate(dt);fx.Step(dt);}}
                var boss=Setup(7);var b=boss.journeyBoss;
                b.cycles=1;BossFrames(boss,65);Require(b.phase==1&&boss.attack>0,"Shift Foreman telegraphs a double press");yield return Shot("01-shift-double-warning");
                BossFrames(boss,32);Require(b.beats==1&&!boss.delivered,"first press leaves the second beat pending");yield return Shot("02-shift-second-beat");
                int strikes=b.strikes;boss.foreman.pressure=.98f;currentCredit=new KillCredit{item=Deck.Fists,action=++actionSerial};Control(boss,1,Vector3.zero);BossFrames(boss,70);
                Require(boss.foreman.stage==2&&b.strikes==strikes,"breaking furnace pressure cancels remaining press beats");yield return Shot("03-shift-vent");
                boss=Setup(14);b=boss.journeyBoss;
                foreach(float x in new[]{-3f,0,3f}){var ally=ProbeEnemy(x,5,5000);ally.cooldown=100;Spawned++;}
                BeginBellCommand(boss);Require(b.links.Count==3,"Bell Foreman snapshots three visible linked escorts");BossFrames(boss,60);yield return Shot("04-bell-rally");
                currentCredit=new KillCredit{item=Deck.Fists,action=++actionSerial};AddForemanPressure(boss,.22f);float pressure=b.disruption;AddForemanPressure(boss,.22f);
                Require(Mathf.Abs(b.disruption-pressure)<.0001f,"paired and derived control share one command-disruption budget");
                AddForemanPressure(boss,.28f);Require(Mathf.Abs(b.disruption-.28f)<.0001f,"later impact upgrades the same action contribution");
                var killed=byId[b.links[0]];KillEnemy(killed,Vector3.zero,false);BossFrames(boss,1);Require(b.links.Count==2,"clearing one escort visibly removes one bell");
                foreach(int id in b.links.ToArray())KillEnemy(byId[id],Vector3.zero,false);BossFrames(boss,1);
                Require(b.phase==3&&b.broken&&b.breaks==1,"clearing all linked escorts cancels the command");yield return Shot("05-bell-silenced");
                BeginBellCommand(boss);JourneyBossDamage(boss,boss.maxHealth*BellDamageThreshold*.5f);Require(b.phase==1&&Mathf.Abs(b.disruption-.5f)<.001f,"half the damage threshold fills half the command meter without cancelling");
                BeginBellCommand(boss);float hp=boss.health;Hurt(boss,boss.maxHealth*(BellDamageThreshold+.01f),Vector3.zero,0);
                Require(b.phase==3&&b.broken&&boss.health<hp,"pure damage can break a command without a control ability or escorts");
                BeginBellCommand(boss);BossFrames(boss,235);Require(b.phase==2&&b.links.Count==0,"escort-free command continues into a weaker toll");yield return Shot("06-bell-toll");
                float health=Health;BossFrames(boss,100);Require(b.strikes==1&&Health<health&&health-Health<=tuning.enemyDamage+.01f,"escort-free toll deals one base hit, with no repeated frame damage");
                Require(!boss.rig.Root.GetComponentsInChildren<Transform>().Any(t=>t.name=="Foreman furnace suit"),"conductor uses its own bell silhouette instead of furnace equipment");
                boss=Setup(21);b=boss.journeyBoss;BossFrames(boss,45);
                Require(b.phase==1&&!BossControlOpen,"Pursuer telegraphs a committed sweep");yield return Shot("07-pursuer-execution");
                currentCredit=new KillCredit{item=Deck.Fists,action=++actionSerial};Control(boss,.15f,Vector3.zero);BossFrames(boss,1);Require(b.phase==1,"small basic stagger cannot permanently cancel a committed sweep");
                BossFrames(boss,120);Require(b.phase==3&&BossControlOpen&&b.strikes==1,"Pursuer opens a real recovery window after its sweep");yield return Shot("08-pursuer-open");
                float duration=b.duration;currentCredit=new KillCredit{item=Deck.Fists,action=++actionSerial,hop=1};Hurt(boss,1,Vector3.zero,0);Hurt(boss,1,Vector3.zero,0);
                Require(Mathf.Abs(b.duration-duration-.5f)<.001f,"one originating body-impact action extends recovery only once");
                for(int i=0;i<8;i++){currentCredit=new KillCredit{item=Deck.Fists,action=++actionSerial,hop=1};Hurt(boss,1,Vector3.zero,0);}Require(b.extension==2&&b.duration==5.2f,"impact recovery extension has a two-second ceiling");
                float age=b.age;TogglePause();AdvanceCombat(.5f);Require(b.age==age&&State==RunState.Paused,"pause freezes boss cycles");TogglePause();
                boss.rig.Root.position=HeroPosition+RoadForward*40;boss.physics.Motor.position=boss.rig.Root.position;Physics.SyncTransforms();health=Health;BossFrames(boss,150);Require(Health==health&&b.phase==0,"offscreen Pursuer cancels its attack before it can damage the Walker");
                boss=Setup(14);b=boss.journeyBoss;BeginBellCommand(boss);b.disruption=.42f;b.age=1.2f;Deck.Open(DraftKind.Mixed,false);State=RunState.Drafting;
                Require(SaveCheckpoint(),"boss state checkpoint writes");string graph=JsonUtility.ToJson(CaptureCheckpoint().graph);
                Require(ResumeExpedition()&&graph==JsonUtility.ToJson(CaptureCheckpoint().graph),"boss cycle, contribution budgets and links restore exactly");
                var old=CaptureCheckpoint();old.schema=8;foreach(var node in old.graph.objects.Where(o=>o.type==typeof(Enemy).FullName))node.fields.RemoveAll(f=>f.name=="journeyBoss");AtomicWrite(SavePath,JsonUtility.ToJson(old));
                Require(ResumeExpedition()&&enemies.All(e=>e.journeyBoss==null),"schema 8 retains its actors without inventing mid-fight boss state");
                var invalid=CaptureCheckpoint().graph;foreach(var node in invalid.objects.Where(o=>o.type==typeof(Enemy).FullName))node.fields.RemoveAll(f=>f.name=="journeyBoss");bool rejected=false;try{GraphCodec().Decode(invalid);}catch(InvalidOperationException){rejected=true;}Require(rejected,"current schema still rejects a missing required actor field");
                boss=Setup(14);b=boss.journeyBoss;BeginBellCommand(boss);b.disruption=.98f;KillEnemy(boss,Vector3.zero,false);Launch(boss,new Cascade(),1,1);
                Require(b.disruption==.98f&&b.breaks==0&&b.phase==1,"launching the defeated conductor cannot break a dead command or emit a false opening");
                for(int district=0;district<3;district++)
                {
                    StartChapter();ProbeReset();State=RunState.Running;OpeningPicks=2;Wave=district*7+1;BeginWave();spawnTimer=10000;
                    cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();Physics.SyncTransforms();
                    yield return Shot("district-"+(district+1));
                    var fork=roads.Split(HeroPosition,32);roads.Follow(fork,1);
                    int solids=regionalWorld.GetComponentsInChildren<Collider>().Count(c=>c.enabled);RefreshExpeditionRoads();
                    Require(regionalWorld.GetComponentsInChildren<Collider>().Count(c=>c.enabled)==solids,"district art adds no physical obstacles / "+district);
                    Require(fork.Right.Guides.All(r=>r.sharedMaterial==VisualFactory.Glow)&&fork.Right.SelectionMarks.All(r=>r.enabled)&&fork.Left.SelectionMarks.All(r=>!r.enabled),"cyan route selection survives district styling / "+district);
                }
                if(render)
                {
                    boss=Setup(14);foreach(float x in new[]{-3f,0,3f}){var ally=ProbeEnemy(x,5,5000);ally.cooldown=100;Spawned++;}BeginBellCommand(boss);BossFrames(boss,60);
                    Screen.SetResolution(1280,720,false);ReducedMotion=true;fx.SetReducedMotion(true);for(int i=0;i<15;i++)yield return null;yield return Shot("bell-720p-reduced-motion");
                    boss=Setup(21);BossFrames(boss,170);for(int i=0;i<8;i++)yield return null;yield return Shot("pursuer-720p-reduced-motion");
                }
            }
            report.passed=true;Write();Debug.Log("DISTRICT_BOSS_REVIEW_SUCCESS "+report.assertions);Application.Quit(0);
        }
        void EquipDistrictComparison(ControlledCase c)
        {
            if(c.rank<1||c.rank>5||c.baseRank!=c.rank||c.weapons.Length!=2||c.abilities.Length!=3||c.modules!=null)throw new ArgumentException("Boss matrix needs equal-rarity, unaugmented kits");
            void Equip(string id,PlaceKind place,int index=0)
            {if(string.IsNullOrEmpty(id))return;var item=Deck.Acquire(LoadoutCatalog.Find(id),c.rank);if(!MoveItem(item,place,index))throw new InvalidOperationException("Boss comparison equip: "+id);}
            Equip(c.weapons[0],PlaceKind.Weapon,0);Equip(c.weapons[1],PlaceKind.Weapon,1);
            for(int i=0;i<3;i++)Equip(c.abilities[i],PlaceKind.Sequence,i+2);
            Equip(c.helmet,PlaceKind.Helmet);Equip(c.armor,PlaceKind.Armor);Equip(c.boots,PlaceKind.Boots);Equip(c.rig,PlaceKind.Rig);Equip(c.ultimate,PlaceKind.Ultimate);
            var order=c.sequence.Select(id=>id=="basic0"?Deck.Basics[0]:id=="basic1"?Deck.Basics[1]:Deck.Slots.Single(i=>i?.def.id==id)).ToArray();
            if(Deck.TwoHanded){int first=Array.IndexOf(order,Deck.Basics[0]);if(Array.IndexOf(order,Deck.Basics[1])!=first+1)throw new ArgumentException("Split two-handed boss kit");Deck.Move(Deck.Basics[0],PlaceKind.Sequence,first);}
            for(int i=0;i<order.Length;i++)if(order[i].def.kind!=ItemKind.Basic||!Deck.TwoHanded||order[i]==Deck.Basics[0])Deck.Move(order[i],PlaceKind.Sequence,i);
            if(!Deck.Slots.SequenceEqual(order))throw new InvalidOperationException("Boss comparison order changed");
            hero.Equipment(Deck.Has(AbilityId.MassDriver),Deck.Pinball);OpeningPicks=2;Health=MaxHealth;UltimateRemaining=UltimateCooldown;UltimateAutomatic=true;
        }
        IEnumerator CheckDistrictRearDoors(Action<bool,string> require)
        {
            foreach(int branch in new[]{-1,0,1})foreach(int side in new[]{-1,1})
            {
                StartChapter();ProbeReset();OpeningPicks=2;Wave=15;BeginWave();spawnTimer=10000;
                if(branch>=0){var fork=roads.Split(HeroPosition,32);roads.Follow(fork,branch);}
                yield return null;var road=roads.Path.Last();
                var source=EntryLayout.Doors.First(d=>d.road==road&&d.side==side&&d.localZ>40);
                hero.Root.position=road.Point(source.localZ+2.8f)+Vector3.up*.04f;heroPhysics.Motor.position=hero.Root.position;
                cameraRig.transform.position=HeroPosition+RoadVector(tuning.cameraOffset);AimCamera();Physics.SyncTransforms();
                entryTurn=side<0?0:1;routeSpawn=1;var door=RearDistrictDoor();
                require(door!=null&&EntryLayout.PassageClear(door)&&EntryLayout.Concealed(cameraRig,door.origin),"covered rear entry remains concealed and traversable / "+branch+" / "+side);
                int before=Spawned;SpawnEntry(3,null,CrewRole.None);var e=enemies.Last();
                require(Spawned==before+1&&e.entering&&Vector3.Dot(e.doorway-HeroPosition,RoadForward)<0,"production arrival uses the rear door / "+branch+" / "+side);
                int frame=0;for(;frame<900&&!EnemyCanAttack(e)&&e.deathAge<0;frame++)
                {float dt=1f/60;Elapsed+=dt;clock+=dt;heroPhysics.MovePosition(HeroPosition+RoadForward*tuning.walkSpeed*dt);StepEnemies(dt);Physics.SyncTransforms();Physics.Simulate(dt);UpdateCamera(dt);ResolveRearBoundary();}
                Debug.Log("DISTRICT_REAR_CHECK branch="+branch+" side="+side+" entering="+e.entering+" repositioning="+e.repositioning+" dead="+(e.deathAge>=0)+" hero="+HeroPosition+" enemy="+e.rig.Root.position+" exit="+e.doorway+" visible="+InCombatView(e.rig.Root.position)+" clear="+ClearEnemyShot(e)+" removed="+RearBoundaryKills);
                require(EnemyCanAttack(e)&&e.deathAge<0&&VisibleSpawns==0&&RearBoundaryKills==0,"rear arrival reaches a walking target without disposal or visible creation / "+branch+" / "+side+" / "+(frame/60f).ToString("0.00")+"s");
            }
        }
    }
}
