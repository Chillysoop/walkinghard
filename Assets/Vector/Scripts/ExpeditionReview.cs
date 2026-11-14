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
        [Serializable] sealed class ExpeditionReviewReport
        {
            public bool passed;public int assertions,productionAssertions;
            public string result;public int wave,itemChoices,augmentChoices,kills;public float health;
            public string scope="Native checkpoint, risk, purchase and physical-road fixtures. Elevated fixture health is not a difficulty estimate.";
            public List<string> checks=new List<string>();
        }
        IEnumerator ReviewExpedition()
        {
            enabled=false;qa=false;fx.muted=true;legacyExpeditionReview=true;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;Screen.SetResolution(1920,1080,false);
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-reviewOutput");
            string directory=at>=0?args[at+1]:Path.Combine(Application.dataPath,"..","ExpeditionReview");Directory.CreateDirectory(directory);
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Expedition review requires an isolated -saveDirectory");
            var report=new ExpeditionReviewReport();
            void Require(bool value,string name){if(!value)throw new InvalidOperationException("EXPEDITION_REVIEW: "+name);report.assertions++;report.checks.Add(name);}
            IEnumerator Snapshot(string name)
            {
                yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();
                Require(texture.width==Screen.width&&texture.height==Screen.height,"native capture "+name);
                File.WriteAllBytes(Path.Combine(directory,name+".png"),texture.EncodeToPNG());Destroy(texture);yield return null;
            }
            void Advance(int frames){for(int i=0;i<frames;i++)AdvanceCombat(1f/60);}
            void Until(Func<bool> done,string name){for(int i=0;i<6000&&!done();i++)Advance(1);Require(done(),name);}
            void Overtime(int seed)
            {
                eventRandom=new CheckpointRandom(seed);ExpeditionActive=true;State=RunState.Drafting;Wave=2;routedWave=3;SelectedRoute=EncounterRoute.Overtime;OpenExpeditionStop();
            }
            for(int i=0;i<10;i++)yield return null;
            if(Array.IndexOf(args,"-expeditionFullRun")>=0)
            {
                simulationPolicy="pinball";BeginExpedition();int drafts=0,events=0,services=0;float wallStart=Time.realtimeSinceStartup;
                report.scope="Native eight-wave Expedition with normal health, ordinary offers, automated pinball preferences and real physics. Restores each reward checkpoint. One functional run, not an economy balance estimate.";
                var sampled=new HashSet<int>();
                while(State!=RunState.Complete&&State!=RunState.Defeated)
                {
                    if(Time.realtimeSinceStartup-wallStart>=240)throw new InvalidOperationException("Full expedition exceeded 240 seconds");
                    if(State==RunState.Drafting)
                    {
                        if(StopOpen)
                        {
                            if(Stop.kind==EncounterRoute.Overtime){events++;PushOvertime();yield return Snapshot("run-overtime");if(Stop.reveal)ContinueOvertime();if(StopOpen)CashOutStop();}
                            else {services++;BuyRepair();for(int i=0;i<Stop.stock.Length;i++)if(BuyCopy(i))break;yield return Snapshot("run-service");CashOutStop();}
                        }
                        Require(SaveCheckpoint()&&ResumeExpedition(),"ordinary checkpoint restored before draft "+drafts++);
                        SimulationDraft();yield return null;
                    }
                    else if(State==RunState.Revealing){StepFinaleReveal(RevealDuration);yield return null;}
                    else
                    {
                        for(int i=0;i<60&&State==RunState.Running;i++)
                        {
                            Advance(1);if(RoutePending)ChooseRoute(Wave==2?EncounterRoute.Overtime:EncounterRoute.SalvageStop);
                        }
                        if(WaveElapsed>18&&sampled.Add(Wave))yield return Snapshot("run-wave-"+Wave);
                        yield return null;
                    }
                }
                Require(State==RunState.Complete&&Wave==8&&events==1&&services==1,"eight waves complete through both optional stops");
                Require(ItemChoices==13&&AugmentChoices==2&&bossSpawned,"all ordinary item and augment rewards and finale preserved");
                Require(OrderErrors==0&&VisibleSpawns==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills,"sequence hidden entries and body identities remain valid after repeated resumes");
                FinishExpeditionStorage();Require(!HasCheckpoint,"victory clears checkpoint");yield return Snapshot("run-result");
                report.result=State.ToString();report.wave=Wave;report.itemChoices=ItemChoices;report.augmentChoices=AugmentChoices;report.kills=Kills;report.health=Health;
            }
            else if(Array.IndexOf(args,"-resumeOnly")>=0)
            {
                Require(ResumeExpedition(),"fresh process restores checkpoint from disk");
                Require(StopOpen&&Stop.kind==EncounterRoute.SalvageStop&&Stop.boughtCopy&&Stop.repaired,"fresh process preserves spent service purchases");
                Require(roads.Forks.Count==2&&corpses.Any(e=>e.physics.Severed)&&flights.Count>0,"fresh process restores roads severed bodies and flight");
                Require(!BuyCopy(0)&&!BuyRepair(),"fresh process cannot duplicate purchases");
                yield return Snapshot("12-fresh-process");
            }
            else
            {
                ExpeditionActive=false;CheckCombatMechanics();report.productionAssertions=mechanicAssertions;
                foreach(int seed in new[]{1,7,123456})
                {
                    var plain=new System.Random(seed);var tracked=new CheckpointRandom(seed);
                    for(int i=0;i<200;i++)
                    {
                        Require(plain.Next()==tracked.Next()&&plain.Next(100)==tracked.Next(100)&&plain.Next(-3,12)==tracked.Next(-3,12)&&plain.NextDouble()==tracked.NextDouble(),"tracked RNG preserves existing sequence "+seed+" / "+i);
                    }
                    var restored=new CheckpointRandom(tracked.Seed,tracked.Draws);Require(restored.Next()==tracked.Next(),"RNG resumes at saved draw "+seed);
                }
                ExpeditionActive=false;Begin(true);State=RunState.Ready;yield return Snapshot("01-menu");
                BeginExpedition();Require(SaveCheckpoint(),"opening draft saves");string opening=File.ReadAllText(SavePath);string offers=string.Join("|",Deck.Offers.Select(o=>o.def.id+"/"+o.rank));
                Require(ResumeExpedition()&&offers==string.Join("|",Deck.Offers.Select(o=>o.def.id+"/"+o.rank)),"opening offers preserved");
                Reroll();int rerolls=Deck.Rerolls;offers=string.Join("|",Deck.Offers.Select(o=>o.def.id+"/"+o.rank));
                Require(ResumeExpedition()&&Deck.Rerolls==rerolls&&offers==string.Join("|",Deck.Offers.Select(o=>o.def.id+"/"+o.rank)),"reroll consumption and resulting offers preserved");
                // Traverse the same road geometry and production decision handler as normal play.
                tuning.heroHealth=10000;Health=10000;tuning.waveArrivals=120;OpeningPicks=2;Wave=2;BeginWave();spawnTimer=1000;
                for(int i=0;i<2400&&!RoutePending;i++)Advance(1);
                Require(RoutePending&&ForkRight==EncounterRoute.Overtime,"expedition fork exposes overtime while walking");
                Require(hud.HandleForkPointer(EventType.MouseDown,hud.ForkChoiceRect(1,1600,900).center,1600,900),"click selects overtime road");
                yield return Snapshot("02-overtime-road");Until(()=>!RoutePending,"first choice locks");
                Until(()=>roads.Progress(HeroPosition)>=forkVisual.Distance+24,"walker reaches event road");
                FinishWave();Require(StopOpen&&Stop.kind==EncounterRoute.Overtime&&Salvage==12,"selected road opens event and checkpoint salvage once");
                Require(SaveCheckpoint()&&ResumeExpedition()&&roads.Forks.Count==1,"event and physical right road restore");
                Wave=3;Require(ForkRight==EncounterRoute.Overtime,"locked signs retain their destination across the wave boundary");Wave=2;
                int wallet=Salvage,stash=Stop.stash;Require(!ClaimReward(0,PlaceKind.Inventory,0,false),"event blocks ordinary reward shortcuts");
                Require(CashOutStop()&&Salvage==wallet+stash&&!CashOutStop(),"starting haul may be banked exactly once");
                tuning.heroHealth=280;Health=MaxHealth;
                var variants=new HashSet<string>();for(int i=1;i<=30;i++){Overtime(i);variants.Add(Stop.variant+"/"+Stop.chance+"/"+Stop.gain+"/"+Stop.harm);}
                Require(variants.Count>25,"run rolls vary story odds and stakes");
                Overtime(17);Health=MaxHealth;Require(SaveCheckpoint(),"risk choice saves");string risk=File.ReadAllText(SavePath);wallet=Salvage;stash=Stop.stash;
                yield return Snapshot("03-risk-choice");
                Require(PushOvertime()&&!PushOvertime(),"one click reveals one roll only");int roll=Stop.roll;bool failed=Stop.failed;float health=Health;
                Require(ResumeExpedition()&&Stop.reveal&&Stop.roll==roll&&Stop.failed==failed&&Health==health&&Salvage==wallet,"revealed result and loss survive reload");
                File.WriteAllText(SavePath,risk);Require(ResumeExpedition()&&PushOvertime()&&Stop.roll==roll&&Stop.failed==failed,"restoring pre-roll state produces identical roll");
                // Exercise both outcomes without changing the visible probability contract.
                for(int seed=1;seed<200;seed++)
                {
                    Overtime(seed);Health=MaxHealth;wallet=Salvage;stash=Stop.stash;int damage=Stop.harm;health=Health;PushOvertime();
                    if(!Stop.failed)continue;
                    Require(Stop.stash==0&&Salvage==wallet&&Health==health-damage,"failed attempt loses only stated haul and integrity");
                    yield return Snapshot("04-failed-attempt");Require(ContinueOvertime()&&!StopOpen,"failure leaves event without another roll");break;
                }
                for(int seed=1;seed<200;seed++)
                {
                    Overtime(seed);Health=MaxHealth;stash=Stop.stash;int gain=Stop.gain,chance=Stop.chance,damage=Stop.harm;PushOvertime();
                    if(Stop.failed)continue;
                    Require(Stop.stash==stash+gain,"success adds stated payout to unbanked haul");yield return Snapshot("05-success");
                    Require(ContinueOvertime()&&Stop.chance<chance&&Stop.gain>gain&&Stop.harm>damage,"next attempt shows worse odds and larger stakes");
                    yield return Snapshot("06-next-risk");Health=Stop.harm;Require(!PushOvertime()&&CashOutStop(),"lethal risk blocked while cashout remains available");break;
                }
                Overtime(41);Health=MaxHealth;Stop.attempt=3;Stop.chance=100;PushOvertime();wallet=Salvage;stash=Stop.stash;
                Require(!PushOvertime()&&ContinueOvertime()&&Salvage==wallet+stash&&!StopOpen,"fourth success automatically ends with banked haul");
                // Leave on the second independent road, then restore a mixed physics checkpoint.
                Wave=5;State=RunState.Running;WaveElapsed=0;spawnTimer=1000;Health=MaxHealth;PrepareWalkingFork();
                for(int i=0;i<2400&&!RoutePending;i++)Advance(1);
                Require(RoutePending&&ForkLeft==EncounterRoute.SalvageStop&&ChooseRoute(EncounterRoute.SalvageStop),"second fork offers salvage service");
                Until(()=>!RoutePending,"second choice locks");Until(()=>roads.Progress(HeroPosition)>=forkVisual.Distance+24,"walker reaches service road");
                var sword=Deck.Acquire(LoadoutCatalog.Find("sword"));Deck.Move(sword,PlaceKind.Weapon,0);Deck.Acquire(sword.def);Deck.Attach(Deck.Acquire(AugmentCatalog.Get(AugmentId.Crosscut)),sword);
                var hammer=Deck.Acquire(LoadoutCatalog.Find("hammer"));Deck.Move(hammer,PlaceKind.Weapon,1);
                var live=ProbeEnemy(HeroPosition.x+2,HeroPosition.z+6,500);var corpse=ProbeEnemy(HeroPosition.x-2,HeroPosition.z+5,5);
                currentCredit=new KillCredit{item=Deck.Basics[0],weapon=sword,action=++actionSerial};KillEnemy(corpse,Vector3.forward*5,true);Launch(corpse);
                MarkEnemy(live,sword);AddBarrier(sword,19,5);ammunition[hammer]=2;FinishWave();Salvage=70;Health=MaxHealth-50;
                Require(StopOpen&&Stop.kind==EncounterRoute.SalvageStop&&roads.Forks.Count==2,"service checkpoint on selected second road");
                Stop.stock=new[]{new RewardOffer(sword.def,2,1),new RewardOffer(hammer.def,1,1),new RewardOffer(LoadoutCatalog.Find("GravityWell"),0,1)};
                yield return Snapshot("07-salvage-service");
                int fusions=Deck.Fusions;wallet=Salvage;Require(BuyCopy(0)&&Salvage==wallet-12&&Deck.Fusions==fusions+1&&sword.rank==2&&sword.augment!=null&&!BuyCopy(1),"matching copy uses existing fusion and preserves augment with one purchase");
                wallet=Salvage;Require(BuyRepair()&&Health==MaxHealth&&Salvage==wallet-12&&!BuyRepair(),"repair clamps to missing health and buys only once");
                Require(Stop.result.Contains("FUSED:")&&Stop.result.Contains("Augment retained")&&Stop.result.Contains("integrity"),"fusion and repair receipts stay visible together");
                Require(SaveCheckpoint(),"full physics checkpoint saves");var before=CaptureCheckpoint();string graph=JsonUtility.ToJson(before.graph);Vector3 position=HeroPosition;int bodyId=corpse.id;int remaining=flights.Single(f=>f.body.id==bodyId).chain.remaining;
                Require(ResumeExpedition(),"full physics checkpoint restores");var after=CaptureCheckpoint();
                Require(JsonUtility.ToJson(after.graph)==graph,"entire model graph round trips without changed values or ownership");
                Require(HeroPosition==position&&roads.Forks.Count==2&&roads.Path.Count==3,"walker and two selected roads restored");
                Require(corpses.Any(e=>e.id==bodyId&&e.physics.Severed)&&flights.Any(f=>f.body==byId[bodyId]&&f.chain.remaining==remaining),"severed body and shared flight identity restored");
                foreach(var b in before.actors)
                {
                    var a=after.actors.Single(e=>e.id==b.id);
                    Require(a.bones.Zip(b.bones,(v,z)=>Vector3.Distance(v.position,z.position)<.001f&&Vector3.Distance(v.velocity,z.velocity)<.001f&&Vector3.Distance(v.force,z.force)<.01f&&v.cut==z.cut).All(v=>v),"bone poses velocities pending impulses and severed joints restored for "+b.id);
                }
                Require(!BuyCopy(0)&&!BuyRepair(),"reload cannot duplicate service purchases");yield return Snapshot("08-restored-service");
                Screen.SetResolution(1280,720,false);for(int i=0;i<8;i++)yield return null;yield return Snapshot("09-service-720p");
                string good=File.ReadAllText(SavePath);File.WriteAllText(SavePath,"{broken");Require(!ResumeExpedition()&&File.ReadAllText(SavePath)=="{broken","corrupt save is rejected and retained");File.WriteAllText(SavePath,good);
                Require(ResumeExpedition(),"valid checkpoint remains loadable after rejection");
                CashOutStop();Require(!StopOpen&&pendingDrafts.Count==1&&Deck.Draft==DraftKind.Mixed,"service leaves ordinary mixed and augment rewards intact");
                foreach(int v in new[]{0,1,2})RecordOvertime(v);RecordOvertime(1);Require(ArchiveCount==3,"archive records each story only once");
                SuspendExpedition();hud.ReviewExpeditionArchive();yield return Snapshot("10-records");hud.ReviewExpeditionArchive();
                ExpeditionActive=false;Begin(true);State=RunState.Ready;Require(File.ReadAllText(SavePath)!=opening&&HasCheckpoint,"quick run preserves stored expedition");
                Require(ResumeExpedition(),"stored expedition resumes after quick run");State=RunState.Defeated;FinishExpeditionStorage();Require(!HasCheckpoint,"defeat clears run checkpoint");
                // Retain the full service fixture for an independent fresh-process restore.
                File.WriteAllText(SavePath,good);Require(ResumeExpedition(),"fixture restored for cross-process check");
            }
            report.passed=true;File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));
            Debug.Log("EXPEDITION_REVIEW_SUCCESS / "+report.assertions+" assertions");Application.Quit(0);
        }
    }
    public sealed partial class PrototypeHUD
    {
        internal void ReviewExpeditionArchive(){expeditionArchive=!expeditionArchive;}
    }
}
