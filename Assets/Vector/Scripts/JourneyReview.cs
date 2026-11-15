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
        [Serializable] sealed class JourneyReport
        {
            public bool passed;public int assertions,wave,kills,spawns,forks,items,augments,sparks,dormantBodies;
            public float health,combatSeconds,estimatedMinutes;public string result,scope;
            public List<string> checks=new List<string>();public List<string> milestones=new List<string>();
        }
        IEnumerator ReviewJourney()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();bool full=args.Contains("-journeyFullRun"),render=!args.Contains("-nographics");
            string Arg(string key,string fallback){int at=Array.IndexOf(args,key);return at<0?fallback:args[at+1];}
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Isolated profile required");
            string output=Path.GetFullPath(Arg("-reviewOutput",Path.Combine(Application.dataPath,"..","JourneyReview")));Directory.CreateDirectory(output);
            var report=new JourneyReport{scope=full?"Normal-health automated run. Combat time includes hit stop at 1x; estimated minutes add 20 seconds per pick and 25 seconds per road stop, not measured human decision time.":"Controlled production input, enemy, progression and checkpoint fixtures."};
            void Require(bool ok,string rule){if(!ok)throw new InvalidOperationException("JOURNEY_REVIEW: "+rule);report.assertions++;report.checks.Add(rule);}
            IEnumerator Shot(string name){if(!render)yield break;yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());Destroy(t);}
            QualitySettings.vSyncCount=0;Application.targetFrameRate=render?60:-1;if(render)Screen.SetResolution(1920,1080,false);for(int i=0;i<8;i++)yield return null;
            if(full)
            {
                forcedSeed=true;tuning.seed=int.Parse(Arg("-seed","1741"));simulationPolicy=Arg("-journeyPolicy","pinball");
                if(args.Contains("-journeyResumeRun")){Require(ResumeExpedition(),"independent process restores late journey");report.combatSeconds=Elapsed;report.scope="Late-checkpoint continuation; timing excludes earlier hit stop and is not a full-run pacing measurement.";}else StartChapter();
                var savedWaves=new HashSet<int>();var shots=new HashSet<int>();float started=Time.realtimeSinceStartup;
                int heartbeat=0;
                while(State!=RunState.Complete&&State!=RunState.Defeated)
                {
                    if(Time.realtimeSinceStartup-started>1500)throw new InvalidOperationException("Journey review timeout");
                    if(State==RunState.Drafting)
                    {
                        if(savedWaves.Add(Wave))
                        {
                            string milestone="Encounter "+Wave+" / HP "+Health.ToString("0")+" / kills "+Kills+" / time "+report.combatSeconds.ToString("0");report.milestones.Add(milestone);Debug.Log(milestone);
                            if(Wave==7||Wave==14||Wave==20)
                            {
                                Require(SaveCheckpoint(),"late checkpoint writes / "+Wave);var json=JsonUtility.ToJson(CaptureCheckpoint().graph);
                                Require(ResumeExpedition()&&json==JsonUtility.ToJson(CaptureCheckpoint().graph),"late checkpoint restores exact graph / "+Wave);
                                yield return Shot("checkpoint-"+Wave);
                            }
                        }
                        if(StopOpen)SimulationExpeditionStop();else SimulationDraft();
                    }
                    else if(State==RunState.Revealing){StepFinaleReveal(RevealDuration);report.combatSeconds+=RevealDuration;}
                    else
                    {
                        for(int i=0;i<90&&State==RunState.Running;i++)
                        {AdvanceCombat(1f/60);report.combatSeconds+=1f/60;if(RoutePending)ChooseRoute(SimulationExpeditionRoute());}
                        if((int)report.combatSeconds/60>heartbeat){heartbeat=(int)report.combatSeconds/60;Debug.Log("JOURNEY_PROGRESS wave="+Wave+" time="+report.combatSeconds+" boss="+BossFraction+" enemies="+enemies.Count+" dormant="+corpses.Count(e=>!e.rig.Root.gameObject.activeSelf));}
                        if((Wave==7||Wave==14||Wave==21)&&WaveElapsed>12&&shots.Add(Wave))yield return Shot("boss-"+Wave);
                    }
                    yield return null;
                }
                report.dormantBodies=corpses.Count(e=>!e.rig.Root.gameObject.activeSelf);report.wave=Wave;report.health=Health;report.kills=Kills;report.spawns=Spawned;report.forks=roads.Forks.Count;report.items=ItemChoices;report.augments=AugmentChoices;report.sparks=Sparks;report.result=State.ToString();report.estimatedMinutes=(report.combatSeconds+(ItemChoices+AugmentChoices)*20+roads.Forks.Count*25)/60;
                Require(OrderErrors==0&&VisibleSpawns==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills,"ordered actions, concealed spawns and persistent body identity throughout run");
                Require(State==RunState.Complete&&Wave==21&&Waves.Count==21,"normal-health run reaches and defeats final boss");
                Require(roads.Forks.Count==9&&roads.Path.All(p=>Vector3.Dot(p.Rotation*Vector3.forward,Vector3.forward)>.8f),"all nine forks keep roads forward without loops");
                Require(ItemChoices==20&&AugmentChoices==5&&Sparks==30&&CompletionSeal,"defined rewards and banked progression across all districts");
                Require(enemies.Concat(corpses).Any(e=>e.crew==CrewRole.Linebreaker)&&enemies.Concat(corpses).Any(e=>e.crew==CrewRole.Cinderlobber)&&enemies.Concat(corpses).Any(e=>e.crew==CrewRole.Bellkeeper),"all three specialist types appear in ordinary encounters");
                yield return Shot("journey-result");FinishExpeditionStorage();OpenOffice();yield return Shot("office-return");
            }
            else
            {
                MainMenu();yield return Shot("title");
                Require(hud.HandlePlayPointer(EventType.MouseDown,hud.PlayButtonRect(1280,720).center,1280,720)&&OfficeOpen&&State==RunState.Ready,"Play pointer opens only the Office");yield return Shot("office");
                CloseOffice();HandleJourneyKey(KeyCode.Return);Require(OfficeOpen,"Enter shares the Play path");
                StartChapter();Require(JourneyActive&&WaveCount==21&&chapterRun.Valid()&&expeditionPlan.Valid(),"new journey has 21 valid encounters and nine forks");
                string run=chapterRun.id;HandleJourneyKey(KeyCode.T);Require(chapterRun.id==run,"old repeat-seed key cannot open legacy play");RestartJourney();Require(JourneyActive&&chapterRun.id!=run,"restart uses the current journey");
                Require(SaveCheckpoint()&&ResumeExpedition()&&JourneyActive,"current journey saves and resumes");
                for(int seed=1;seed<=100;seed++)
                {var plan=LongJourney.Plan(seed);Require(plan.Valid()&&plan.waves.Length==21&&plan.left.Length==9&&Enumerable.Range(0,9).Where(i=>i%3!=1).All(i=>plan.left[i]==EncounterRoute.SalvageStop||plan.right[i]==EncounterRoute.SalvageStop),"valid varied route with guaranteed repair alternatives / "+seed);}
                BeginLegacyExpedition();SaveCheckpoint();string old=File.ReadAllText(SavePath),profile=JsonUtility.ToJson(office);legacyReviewAccess=false;RetireOldCheckpoint();
                Require(!HasCheckpoint&&Directory.GetFiles(Path.Combine(SaveDirectory,"RetiredRuns"),"*.json").Any(p=>File.ReadAllText(p)==old)&&JsonUtility.ToJson(office)==profile,"old run archived byte-for-byte without changing permanent progress");legacyReviewAccess=true;
                StartChapter();OpeningPicks=2;Wave=2;Health=10000;BeginWave();spawnTimer=10000;
                for(int i=0;i<3000&&!RoutePending;i++)AdvanceCombat(1f/60);
                Require(RoutePending,"walker reaches the physical road choice");
                bool ClickRoad(int side,float w=1600,float h=900)
                {
                    var road=side==0?forkVisual.Left:forkVisual.Right;
                    for(float z=9;z<=35;z+=1)for(float x=-6;x<=6;x+=1)
                    {
                        var v=cameraRig.WorldToViewportPoint(road.Point(z,x)+Vector3.up*.01f);var p=new Vector2(v.x*w,(1-v.y)*h);
                        if(v.z<=0||p.x<335||p.x>w-335||p.y<110||p.y>h-230||hud.ForkChoiceRect(0,w,h).Contains(p)||hud.ForkChoiceRect(1,w,h).Contains(p))continue;
                        if(Physics.Raycast(cameraRig.ViewportPointToRay(v),out var hit,180,1<<EntryLayout.WorldLayer)&&hit.transform.IsChildOf(road.Root)&&road.Local(hit.point).z>=8)
                            return hud.HandleForkPointer(EventType.MouseDown,p,w,h);
                    }
                    return false;
                }
                Require(ClickRoad(1)&&ForkChoice==ForkRight&&forkVisual.Right.SelectionMarks.All(r=>r.enabled)&&forkVisual.Left.SelectionMarks.All(r=>!r.enabled),"clicking right road floor changes both route and visible arrows");yield return Shot("right-road");
                Require(ClickRoad(0)&&ForkChoice==ForkLeft,"clicking left road floor changes selection before deadline");yield return Shot("left-road");
                TogglePause();Require(!ChooseForkRoad(new Vector2(.5f,.5f)),"pause rejects road selection");TogglePause();
                Require(!hud.HandleForkPointer(EventType.MouseMove,new Vector2(800,500),1600,900)&&!hud.HandleForkPointer(EventType.MouseDown,new Vector2(100,150),1600,900),"hover and HUD clicks cannot change route");
                if(render){Screen.SetResolution(1280,720,false);for(int i=0;i<8;i++)yield return null;Require(ClickRoad(1),"physical road click at 720p");yield return Shot("fork-720p");}
                var selected=ForkChoice;StepWalkingFork(ForkDecisionSeconds);Require(SelectedRoute==selected&&!RoutePending&&!ClickRoad(0),"deadline follows exactly the highlighted route");
                var taken=roads.Path.Last();hero.Root.position=taken.Point(30)+Vector3.up*.04f;heroPhysics.Motor.position=hero.Root.position;Physics.SyncTransforms();
                Wave=4;BeginWave();spawnTimer=10000;for(int i=0;i<3000&&!RoutePending;i++)AdvanceCombat(1f/60);
                Require(RoutePending&&ClickRoad(0)&&ForkChoice==ForkLeft&&ClickRoad(1)&&ForkChoice==ForkRight,"both physical road clicks work after the camera and road rotate");yield return Shot("rotated-fork");
                foreach(var role in new[]{CrewRole.Linebreaker,CrewRole.Cinderlobber,CrewRole.Bellkeeper})
                {
                    StartChapter();ProbeReset();State=RunState.Running;Wave=8;OpeningPicks=2;Health=MaxHealth;cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
                    var e=ProbeEnemy(0,6,500);e.crew=role;e.crewCooldown=0;e.rig.CrewEquipment(role);e.rig.Root.rotation=Quaternion.LookRotation(HeroPosition-e.rig.Root.position);var ally=ProbeEnemy(2,4,500);ally.cooldown=8;Physics.SyncTransforms();
                    Require(StepCrew(e,.05f)&&e.crewClock>0,"specialist starts a visible wind-up / "+role);
                    fx.Step(.01f);yield return Shot("enemy-"+role);
                    e.stun=.5f;StepCrew(e,.05f);Require(e.crewClock==0&&e.specialist.stage==0&&e.crewNotice>Elapsed,"stagger cancels specialist / "+role);
                    e.stun=0;e.crewCooldown=0;StepCrew(e,.01f);for(int i=0;i<(role==CrewRole.Linebreaker?80:120);i++)StepCrew(e,1f/60);
                    Require(role==CrewRole.Linebreaker?e.specialist.stage==2&&SpecialistVelocity(e,Vector3.zero).magnitude>11:e.crewUses==1,"specialist completes its distinct attack / "+role);
                    if(role==CrewRole.Cinderlobber)Require(Health<MaxHealth,"marked blast damages a hero inside its actual footprint");
                    if(role==CrewRole.Bellkeeper)Require(ally.cooldown<=.1f,"rally readies nearby ally without bypassing attack budget");
                    State=RunState.Drafting;Require(SaveCheckpoint()&&ResumeExpedition()&&enemies.Any(a=>a.crew==role&&a.specialist!=null),"specialist state restores at checkpoint / "+role);
                }
                StartChapter();ProbeReset();var sleeper=ProbeEnemy(0,5,1);int sleeperId=sleeper.id;KillEnemy(sleeper,Vector3.zero,false);sleeper.deathAge=15;
                foreach(var body in sleeper.rig.Root.GetComponentsInChildren<Rigidbody>())if(!body.isKinematic)body.Sleep();
                hero.Root.position=new Vector3(0,.04f,110);heroPhysics.Motor.position=hero.Root.position;cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
                Require(DistantBody(sleeper)&&corpses.Count==1&&byId.ContainsKey(sleeperId),"distant settled body leaves simulation without deleting identity");
                State=RunState.Drafting;Require(SaveCheckpoint()&&ResumeExpedition()&&!byId[sleeperId].rig.Root.gameObject.activeSelf,"dormant body and pose survive a checkpoint");
                sleeper=byId[sleeperId];hero.Root.position=sleeper.cachedPosition+Vector3.forward*3;
                Require(!DistantBody(sleeper)&&sleeper.rig.Root.gameObject.activeSelf&&corpses.Count==1,"approaching wakes the same saved body");
                StartChapter();Wave=7;ClearChapterEncounter();Require(CompletionSeal&&Sparks==10&&!chapterRun.complete&&office.victories==0,"first district opens seal but does not complete journey");
                Health=MaxHealth*.5f;JourneyIntermission();Require(Health==MaxHealth*.75f,"district gate restores one quarter integrity");Wave=21;ClearChapterEncounter();Require(Sparks==30&&chapterRun.complete&&office.victories==1,"only final district completes journey");SyncChapterProgress();Require(Sparks==30&&office.victories==1,"checkpoint replay cannot duplicate rewards");
                StartChapter();OpeningPicks=2;Wave=14;BeginWave();for(int i=0;i<900&&Foreman?.foreman.stage==0;i++)AdvanceCombat(1f/60);yield return Shot("bell-foreman");
                Require(ForemanPresent&&JourneyBossName=="BELL FOREMAN","district two uses the bell boss variant");
                StartChapter();SaveCheckpoint();
            }
            report.passed=true;File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));Debug.Log("JOURNEY_REVIEW_SUCCESS / "+report.assertions);Application.Quit(0);
        }
    }
}
