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
        [Serializable] sealed class CrewReport
        {
            public bool passed;public int assertions,productionAssertions;public int repairs,throws,retrievals;
            public int kills,wave,itemChoices,augmentChoices;public float health;public string result,scope;
            public List<string> checks=new List<string>();
        }
        IEnumerator ReviewCrews()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;Screen.SetResolution(1920,1080,false);
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-reviewOutput");string directory=at>=0?args[at+1]:Path.Combine(Application.dataPath,"..","CrewReview");Directory.CreateDirectory(directory);
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Crew review needs an isolated save directory");
            var report=new CrewReport{scope="Controlled native crew and world fixtures with elevated health. Actual movement, bodies and effects; not a difficulty sample."};
            void Require(bool value,string name){if(!value)throw new InvalidOperationException("CREW_REVIEW: "+name);report.assertions++;report.checks.Add(name);}
            IEnumerator Shot(string name){fx.Step(1f/60);yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(directory,name+".png"),t.EncodeToPNG());Destroy(t);yield return null;}
            void Advance(int frames){for(int i=0;i<frames;i++)AdvanceCombat(1f/60);}
            void Until(Func<bool> done,string name){for(int i=0;i<6000&&!done();i++)Advance(1);Require(done(),name);}
            void Reset(){ProbeReset();State=RunState.Running;Wave=2;EventText="";OpeningPicks=2;spawnTimer=10000;tuning.waveArrivals=300;cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();}
            Enemy Crew(CrewRole role,float x,float z){var e=new Enemy{id=++nextId,health=500,maxHealth=500,crew=role,heavy=role==CrewRole.Freight,radius=6};e.rig=new FighterRig(CrewName(role),actors,false,e.heavy,e.id^Seed);e.rig.Root.position=new Vector3(x,.04f,z);e.rig.CrewEquipment(role);e.physics=new FighterPhysics(e.rig,e.id,this,false,e.heavy);enemies.Add(e);byId[e.id]=e;Physics.SyncTransforms();return e;}
            Enemy Body(float x,float z){var e=ProbeEnemy(x,z,1);var credit=currentCredit;currentCredit=null;KillEnemy(e,Vector3.zero,false);currentCredit=credit;for(int i=0;i<45;i++)Physics.Simulate(1f/60);return e;}
            void CrewFrames(Enemy e,int frames){for(int i=0;i<frames;i++){Elapsed+=1f/60;StepCrew(e,1f/60);Physics.SyncTransforms();Physics.Simulate(1f/60);StepFreightBodies(1f/60);fx.Step(1f/60);}}
            for(int i=0;i<10;i++)yield return null;
            if(Array.IndexOf(args,"-crewFullRun")>=0)
            {
                report.scope="Normal-health eight-wave Expedition with ordinary offers and automated pinball choices; one functional run, not balance evidence.";
                simulationPolicy="pinball";BeginExpedition();int restored=0;float start=Time.realtimeSinceStartup;var shot=new HashSet<int>();
                while(State!=RunState.Complete&&State!=RunState.Defeated)
                {
                    if(Time.realtimeSinceStartup-start>300)throw new InvalidOperationException("Crew full run timed out");
                    if(State==RunState.Drafting)
                    {
                        if(StopOpen){if(Stop.kind==EncounterRoute.SalvageStop){BuyRepair();for(int i=0;i<Stop.stock.Length;i++)if(BuyCopy(i))break;}CashOutStop();}
                        if(Wave==3||Wave==5){Require(SaveCheckpoint()&&ResumeExpedition(),"real checkpoint resumes at wave "+Wave);restored++;}
                        SimulationDraft();yield return null;
                    }
                    else if(State==RunState.Revealing){StepFinaleReveal(RevealDuration);yield return null;}
                    else
                    {
                        for(int i=0;i<45&&State==RunState.Running;i++){Advance(1);if(RoutePending)ChooseRoute(Wave==2?EncounterRoute.Overtime:EncounterRoute.SalvageStop);}
                        if(WaveElapsed>18&&shot.Add(Wave))yield return Shot("run-wave-"+Wave);yield return null;
                    }
                }
                Require(OrderErrors==0&&VisibleSpawns==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills,"normal sequence entry and identity invariants");
                Require(Enum.GetValues(typeof(CrewRole)).Cast<CrewRole>().Where(r=>r!=CrewRole.None).All(r=>CrewSpawned(r)>0),"all three roles occur through normal concealed spawning");
                if(State==RunState.Complete)Require(ItemChoices==13&&AugmentChoices==2&&bossSpawned&&restored>=2,"full reward and finale gates survive regional crews");
                report.repairs=CrewUses(CrewRole.Repair);report.throws=CrewUses(CrewRole.Freight);report.retrievals=CrewUses(CrewRole.Reclaimer);report.result=State.ToString();report.wave=Wave;report.kills=Kills;report.health=Health;report.itemChoices=ItemChoices;report.augmentChoices=AugmentChoices;
                FinishExpeditionStorage();yield return Shot("run-result");
            }
            else
            {
                CheckCombatMechanics();report.productionAssertions=mechanicAssertions;ExpeditionActive=false;Begin(true);
                var colliders=regionalWorld.GetComponentsInChildren<Collider>().Select(c=>(c.GetInstanceID(),c.bounds.center,c.bounds.size)).ToArray();
                tuning.heroHealth=10000;BeginExpedition();yield return null;
                Require(colliders.SequenceEqual(regionalWorld.GetComponentsInChildren<Collider>().Select(c=>(c.GetInstanceID(),c.bounds.center,c.bounds.size))),"region changes no original collision geometry");
                Require(VisualFactory.CinderworksStyle&&regionalWorld.RoadDecoration.GetComponentsInChildren<Collider>(true).Length==0,"expedition architecture has no added colliders");
                Reset();var repair=Crew(CrewRole.Repair,-2,5);var target=ProbeEnemy(0,5,500);StepCrew(repair,.25f);
                Require(repair.crewClock>0&&target.crewGuard==0,"repair announces windup before protecting target");yield return Shot("01-repair-windup");
                Control(repair,.6f,Vector3.zero);Require(repair.crewClock==0&&target.crewGuard==0,"stagger interrupts repair immediately");yield return Shot("02-interrupted");
                repair.stun=0;repair.crewCooldown=0;StepCrew(repair,1.5f);Require(target.crewGuard==18&&repair.crewUses==1,"completed repair adds bounded protection");
                Require(CrewProtection(target,30)==12&&target.crewGuard==0,"shield absorbs eighteen then breaks");
                repair.crewCooldown=0;StepCrew(repair,1.5f);Elapsed+=7;Require(CrewProtection(target,5)==5,"unused repair protection expires");
                repair.crewCooldown=0;repair.entering=true;StepCrew(repair,2);Require(repair.crewClock==0,"entering crew cannot begin a channel");repair.entering=false;
                var visiblePosition=repair.rig.Root.position;repair.physics.Motor.position=new Vector3(0,.04f,100);Physics.SyncTransforms();StepCrew(repair,2);Require(repair.crewClock==0,"offscreen crew cannot start a channel");repair.physics.Motor.position=visiblePosition;Physics.SyncTransforms();
                var wall=EntryLayout.Solid("Review occluder",transform,new Vector3(-1,2,5),new Vector3(.3f,4,4),VisualFactory.Stone);Physics.SyncTransforms();repair.crewCooldown=0;StepCrew(repair,2);Require(target.crewGuard==0,"repair cannot cross a wall");wall.gameObject.SetActive(false);Destroy(wall.gameObject);
                Reset();var brute=Crew(CrewRole.Freight,-2,6);var body=Body(0,5);var victim=ProbeEnemy(0,3,500);int kills=Kills;StepCrew(brute,.5f);yield return Shot("03-freight-windup");
                Require(brute.crewClock>0&&body.freightLeft==0,"freight path appears before throw");Launch(body);StepCrew(brute,.1f);Require(brute.crewClock==0&&body.freightLeft==0,"player launch steals reserved body and cancels windup");
                flights.Clear();brute.crewCooldown=0;StepCrew(brute,1.4f);Require(body.freightLeft>0&&brute.crewUses==1,"freight launches existing corpse");var extraA=ProbeEnemy(0,2,500);var extraB=ProbeEnemy(0,4,500);
                // Isolate swept-contact limits from nondeterministic ragdoll contact points.
                body.freightPrevious=HeroPosition+Vector3.forward*7;body.physics.Motor.position=body.rig.Root.position;var before=Health;StepFreightBodies(.02f);
                body.freightPrevious=HeroPosition-Vector3.forward;StepFreightBodies(.02f);float once=Health;body.freightPrevious=HeroPosition-Vector3.forward;StepFreightBodies(.02f);
                Require(Health==once&&before>once&&body.freightHits.Count==2&&new[]{victim,extraA,extraB}.Count(e=>e.health<500)==2&&Kills==kills,"hostile payload damages hero once and only two of three enemies without duplicate identities");
                wall=EntryLayout.Solid("Payload occluder",transform,new Vector3(0,2,2.5f),new Vector3(8,4,.3f),VisualFactory.Stone);Physics.SyncTransforms();body.freightHitHero=false;body.freightHits.Clear();body.freightPrevious=HeroPosition+Vector3.forward;var totalHealth=enemies.Sum(e=>e.health);StepFreightBodies(.02f);
                Require(Health==once&&enemies.Sum(e=>e.health)==totalHealth&&body.freightHits.Count==0,"hostile swept contacts cannot damage through walls");wall.gameObject.SetActive(false);Destroy(wall.gameObject);
                Launch(body);Require(body.freightLeft==0&&flights.Any(f=>f.body==body),"player relaunch defuses hostile ownership");
                Reset();var reclaimer=Crew(CrewRole.Reclaimer,0,7);body=Body(0,3);int bodyKills=Kills;var origin=body.physics.Center;StepCrew(reclaimer,.2f);yield return Shot("04-reclaimer-tether");CrewFrames(reclaimer,70);
                Require(body.physics.Center.z>origin.z+.5f&&corpses.Count==1&&Kills==bodyKills,"retrieval physically moves body without deleting or crediting it again");
                Launch(body);StepCrew(reclaimer,.1f);Require(reclaimer.crewClock==0,"player launch breaks retrieval tether");
                Reset();repair=Crew(CrewRole.Repair,-3,6);target=ProbeEnemy(-1,6);body=Body(3,4);brute=Crew(CrewRole.Freight,3,7);var retriever=Crew(CrewRole.Reclaimer,0,9);var spare=Body(0,6);
                StepCrew(repair,.4f);StepCrew(brute,.4f);StepCrew(retriever,.4f);Require(brute.crewTarget!=retriever.crewTarget,"crews reserve distinct bodies");yield return Shot("04b-mixed-crews");
                Screen.SetResolution(1280,720,false);ReducedMotion=true;fx.SetReducedMotion(true);for(int i=0;i<8;i++)yield return null;yield return Shot("04c-mixed-crews-720p");Screen.SetResolution(1920,1080,false);ReducedMotion=false;fx.SetReducedMotion(false);
                Deck.Open(DraftKind.Mixed,false);State=RunState.Drafting;
                Require(SaveCheckpoint(),"crew windup checkpoint saves");var graph=JsonUtility.ToJson(CaptureCheckpoint().graph);Require(ResumeExpedition()&&graph==JsonUtility.ToJson(CaptureCheckpoint().graph),"all crew reservations and timers round trip exactly");
                Require(enemies.Where(e=>e.crew!=CrewRole.None).All(e=>e.rig.Root.GetComponentsInChildren<Collider>().Count(c=>c.enabled)==1&&e.rig.PhysicsBones.Length==11),"crew art preserves the eleven-bone skeleton and live capsule");
                brute=enemies.Single(e=>e.crew==CrewRole.Freight);StepCrew(brute,1);Require(corpses.Any(e=>e.freightLeft>0),"hostile payload active before save");
                Require(SaveCheckpoint(),"hostile payload checkpoint saves");graph=JsonUtility.ToJson(CaptureCheckpoint().graph);Require(ResumeExpedition()&&graph==JsonUtility.ToJson(CaptureCheckpoint().graph)&&corpses.Any(e=>e.freightLeft>0),"hostile payload ownership hit budgets and remaining flight survive resume");
                // A v1 graph lacks exactly these crew fields; migrate them to neutral defaults.
                var legacy=CaptureCheckpoint();legacy.schema=1;legacy.rules="expedition-stops-1";
                foreach(var node in legacy.graph.objects.Where(n=>n.type=="VectorGame.VectorEncounter+Enemy"))node.fields.RemoveAll(f=>f.name.StartsWith("crew")||f.name.StartsWith("freight"));
                File.WriteAllText(SavePath,JsonUtility.ToJson(legacy));Require(ResumeExpedition()&&enemies.All(e=>e.crew==CrewRole.None),"previous expedition save schema migrates without adding retroactive roles");
                BeginExpedition();OpeningPicks=2;Wave=2;Health=MaxHealth;BeginWave();spawnTimer=10000;tuning.waveArrivals=300;
                Until(()=>RoutePending,"first fork becomes selectable");ChooseRoute(EncounterRoute.Overtime);yield return Shot("05-regional-fork");Until(()=>!RoutePending,"first road selected");Until(()=>roads.Progress(HeroPosition)>forkVisual.Distance+28,"walker enters decorated right road");yield return Shot("06-regional-right-road");
                Require(regionalWorld.RoadDecoration.GetComponentsInChildren<Collider>(true).Length==0&&VisibleSpawns==0,"branch decorations retain concealed-entry physics");
                Wave=5;WaveElapsed=0;PrepareWalkingFork();Until(()=>RoutePending,"second fork becomes selectable");ChooseRoute(EncounterRoute.VolatileBreach);Until(()=>!RoutePending,"second road selected");Until(()=>roads.Progress(HeroPosition)>forkVisual.Distance+28,"walker follows second rotated road");yield return Shot("07-second-road");
                Require(Vector3.Angle(RoadForward,Vector3.forward)>60,"architecture and traversal follow a sixty-four-degree path");
                FinishWave();Require(SaveCheckpoint()&&ResumeExpedition()&&VisualFactory.CinderworksStyle&&roads.Forks.Count==2,"regional roads restore from checkpoint");
                State=RunState.Running;ReducedMotion=true;fx.SetReducedMotion(true);Screen.SetResolution(1280,720,false);for(int i=0;i<8;i++)yield return null;yield return Shot("08-720p-reduced-motion");
                ExpeditionActive=false;Begin(true);State=RunState.Ready;yield return null;Require(!VisualFactory.CinderworksStyle&&regionalWorld.RoadDecoration==null,"quick run restores ordinary world and actor appearance");
                yield return Shot("09-quick-run-restored");
                BeginExpedition();SuspendExpedition();BeginCinderworksPreview();ToggleCombatPreviewStyle();yield return null;
                Require(!ExpeditionActive&&!VisualFactory.CinderworksStyle&&roads.Pieces[0].Root.GetComponentsInChildren<MeshRenderer>().All(r=>r.sharedMaterial!=CinderworksWorld.Floor),"suspended expedition enters preview with a clean original-look comparison");MainMenu();
            }
            report.passed=true;File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));Debug.Log("CREW_REVIEW_SUCCESS / "+report.assertions+" assertions");Application.Quit(0);
        }
    }
}
