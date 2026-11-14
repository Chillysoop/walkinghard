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
        [Serializable] sealed class ItineraryReport
        {
            public bool passed;public int assertions,productionAssertions,kills,wave,itemChoices,augmentChoices;public float health;public string scope,result;public ExpeditionPlan itinerary;
            public List<string> checks=new List<string>();
        }
        IEnumerator ReviewExpeditionVariety()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();bool render=Array.IndexOf(args,"-nographics")<0,full=Array.IndexOf(args,"-varietyFullRun")>=0,resume=Array.IndexOf(args,"-varietyResumeOnly")>=0;
            string Arg(string key,string fallback){int at=Array.IndexOf(args,key);return at<0?fallback:args[at+1];}
            string directory=Path.GetFullPath(Arg("-reviewOutput",Path.Combine(Application.dataPath,"..","VarietyReview")));Directory.CreateDirectory(directory);
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Variety review requires an isolated save directory");
            var report=new ItineraryReport{scope=full?"Normal-health ordinary-offer Expedition with automated pinball and route/event choices. One functional run; not a population difficulty estimate.":"Controlled native itinerary, event, ownership and save fixtures; not balance evidence."};
            void Require(bool value,string rule){if(!value)throw new InvalidOperationException("ITINERARY_REVIEW: "+rule);report.assertions++;report.checks.Add(rule);}
            IEnumerator Shot(string name){if(!render)yield break;yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(directory,name+".png"),t.EncodeToPNG());Destroy(t);yield return null;}
            void Advance(int frames){for(int i=0;i<frames&&State==RunState.Running;i++)AdvanceCombat(1f/60);}
            void Until(Func<bool> done,string rule){for(int i=0;i<6000&&!done();i++)Advance(1);Require(done(),rule);}
            void Event(EncounterRoute kind,int seed)
            {tuning.heroHealth=280;forcedSeed=true;tuning.seed=seed;BeginExpedition();Salvage=100;Health=180;Wave=2;routedWave=3;SelectedRoute=kind;OpenExpeditionStop();}
            void Roll(bool win){int seed=Enumerable.Range(1,500).First(s=>win?new System.Random(s).Next(1,101)<=5:new System.Random(s).Next(1,101)>95);eventRandom=new CheckpointRandom(seed);}
            QualitySettings.vSyncCount=0;Application.targetFrameRate=render?60:-1;if(render)Screen.SetResolution(1920,1080,false);for(int i=0;i<8;i++)yield return null;
            if(full)
            {
                simulationPolicy="pinball";BeginExpedition();float start=Time.realtimeSinceStartup;var shots=new HashSet<string>();
                while(State!=RunState.Complete&&State!=RunState.Defeated)
                {
                    if(Time.realtimeSinceStartup-start>300)throw new InvalidOperationException("Variety full run wall timeout");
                    if(State==RunState.Drafting)
                    {
                        if(StopOpen)
                        {if(shots.Add("event-"+Stop.kind+"-"+Stop.reveal))yield return Shot("event-"+Stop.kind+"-"+Stop.reveal);SimulationExpeditionStop();}
                        else SimulationDraft();
                        if(State==RunState.Drafting){var plan=JsonUtility.ToJson(expeditionPlan);Require(SaveCheckpoint()&&ResumeExpedition()&&plan==JsonUtility.ToJson(expeditionPlan),"ordinary checkpoint preserves itinerary and event/reward state");}
                        yield return null;
                    }
                    else if(State==RunState.Revealing){StepFinaleReveal(RevealDuration);yield return null;}
                    else
                    {
                        for(int i=0;i<45&&State==RunState.Running;i++){Advance(1);if(RoutePending)ChooseRoute(SimulationExpeditionRoute());}
                        if(RoutePending&&shots.Add("fork-"+Wave))yield return Shot("fork-"+Wave);
                        if(Foreman is Enemy boss&&boss.foreman.vents>0&&shots.Add("foreman"))yield return Shot("foreman");
                        if(WaveElapsed>18&&shots.Add("wave-"+Wave))yield return Shot("wave-"+Wave);yield return null;
                    }
                }
                Require(OrderErrors==0&&VisibleSpawns==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills,"full-run combat and physical identity contracts");
                if(State==RunState.Complete)Require(ItemChoices==13&&AugmentChoices==2&&bossSpawned&&corpses.Any(e=>e.foreman!=null),"complete ordinary run retains all rewards and both bosses");
                report.result=State.ToString();report.health=Health;report.kills=Kills;report.wave=Wave;report.itemChoices=ItemChoices;report.augmentChoices=AugmentChoices;report.itinerary=expeditionPlan;yield return Shot("run-result");
            }
            else if(resume)
            {
                Require(ResumeExpedition(),"fresh process restores new checkpoint");
                Require(!expeditionPlan.legacy&&StopOpen&&Stop.kind==EncounterRoute.Auction&&Stop.reveal,"new itinerary and resolved auction persist");
                Require(!ResolveRegionalEvent(0)&&!ResolveRegionalEvent(1),"fresh process cannot bid twice");
                var graph=JsonUtility.ToJson(CaptureCheckpoint().graph);Require(SaveCheckpoint()&&ResumeExpedition()&&graph==JsonUtility.ToJson(CaptureCheckpoint().graph),"fresh process preserves complete state graph");yield return Shot("fresh-resume");
            }
            else
            {
                CheckCombatMechanics();report.productionAssertions=mechanicAssertions;
                var distinct=new HashSet<string>();var offered=new HashSet<EncounterRoute>();
                for(int seed=1;seed<=120;seed++)
                {
                    var a=ExpeditionPlan.Create(seed);Require(a.Valid()&&JsonUtility.ToJson(a)==JsonUtility.ToJson(ExpeditionPlan.Create(seed)),"stable valid itinerary / "+seed);
                    Require(a.left[1]==EncounterRoute.SalvageStop||a.right[1]==EncounterRoute.SalvageStop,"service always offered / "+seed);
                    distinct.Add(string.Join(",",a.left.Concat(a.right)));foreach(var route in a.left.Concat(a.right))offered.Add(route);
                }
                Require(distinct.Count>=20&&RegionalEncounters.All.All(e=>offered.Contains(e.id)),"seeds expose all new destinations and at least twenty fork combinations");
                tuning.heroHealth=10000;BeginExpedition();OpeningPicks=2;Wave=2;tuning.waveArrivals=120;BeginWave();spawnTimer=10000;
                Until(()=>RoutePending,"new itinerary produces a walking fork");var left=ForkLeft;var right=ForkRight;
                Require(forkVisual.Left.Root.GetComponentsInChildren<BoxCollider>().Length>20&&forkVisual.Right.Root.GetComponentsInChildren<BoxCollider>().Length>20,"two independent walled roads remain physical");
                Require(hud.HandleForkPointer(EventType.MouseDown,hud.ForkChoiceRect(1,1600,900).center,1600,900)&&ForkChoice==right,"road sign pointer selects generated destination");yield return Shot("01-generated-fork");
                float seconds=ForkSeconds;var position=HeroPosition;TogglePause();Advance(60);Require(ForkSeconds==seconds&&HeroPosition==position,"pause freezes timed fork");TogglePause();
                ChooseRoute(left);Until(()=>!RoutePending,"six-second deadline commits generated destination");Require(SelectedRoute==left&&expeditionPlan.chosen[0]==left&&!ChooseRoute(right),"generated route locks once");
                Until(()=>roads.Progress(HeroPosition)>=forkVisual.Distance+24,"walker takes the selected independent road");FinishWave();
                var planJson=JsonUtility.ToJson(expeditionPlan);Require(SaveCheckpoint()&&ResumeExpedition()&&JsonUtility.ToJson(expeditionPlan)==planJson&&roads.Forks.Count==1,"itinerary, choice and physical road survive resume");
                Wave=3;Require(ForkLeft==left&&ForkRight==right,"locked signs keep generated offers across wave boundary");
                foreach(var kind in new[]{EncounterRoute.Auction,EncounterRoute.PressureWorks,EncounterRoute.Rescue})
                {
                    Event(kind,1801+(int)kind);Require(StopOpen&&RegionalEventLimit==(kind==EncounterRoute.Auction?1:kind==EncounterRoute.Rescue?3:4),"distinct event structure / "+kind);
                    Require(stopProp.GetComponentsInChildren<Collider>().All(c=>!c.enabled),"event prop has no active physical colliders / "+kind);yield return Shot("event-"+kind);
                    int wallet=Salvage;var p=HeroPosition;float elapsed=Elapsed;AdvanceCombat(1);Require(Health==180&&HeroPosition==p&&Elapsed==elapsed,"event freezes combat / "+kind);
                    Require(!ClaimReward(0,PlaceKind.Inventory,0,false),"event cannot bypass ordinary reward gate / "+kind);
                    Require(SaveCheckpoint(),"pre-roll checkpoint / "+kind);string saved=File.ReadAllText(SavePath);
                    Require(hud.HandleRegionalEventPointer(EventType.MouseDown,hud.RegionalEventOptionRect(0,1600,900).center,1600,900),"production event pointer resolves one choice / "+kind);
                    string result=Stop.result;int roll=Stop.roll;float hp=Health;int balance=Salvage;Require(!ResolveRegionalEvent(1),"second input cannot charge twice / "+kind);
                    File.WriteAllText(SavePath,saved);Require(ResumeExpedition()&&ResolveRegionalEvent(0)&&Stop.result==result&&Stop.roll==roll&&Health==hp&&Salvage==balance,"pre-roll reload repeats identical outcome / "+kind);
                    var graph=JsonUtility.ToJson(CaptureCheckpoint().graph);Require(SaveCheckpoint()&&ResumeExpedition()&&graph==JsonUtility.ToJson(CaptureCheckpoint().graph),"post-roll graph exact / "+kind);
                    yield return Shot("result-"+kind);Require(CashOutStop()&&!CashOutStop(),"event exit applies once / "+kind);
                    foreach(int option in new[]{0,1})foreach(bool win in new[]{false,true})
                    {
                        Event(kind,19);Roll(win);wallet=Salvage;hp=Health;int price=EventPrice(option),harm=EventHarm(option),gain=EventGain(option),stash=Stop.stash;
                        var prize=Stop.stock.FirstOrDefault();int copies=prize==null?0:Deck.Copies(prize.def,prize.rank);
                        Require(ResolveRegionalEvent(option)&&Stop.failed!=win,"controlled success/failure / "+kind+" / "+option+" / "+win);
                        Require(Salvage==wallet-price+(kind==EncounterRoute.Rescue&&win?gain:0),"exact paid and secured salvage / "+kind);
                        Require(Mathf.Abs(Health-(win&&kind==EncounterRoute.Rescue?Mathf.Min(MaxHealth,hp+Stop.repair):hp-(!win?harm:0)))<.01f,"exact integrity consequence / "+kind);
                        if(kind==EncounterRoute.PressureWorks)Require(Stop.stash==(win?stash+gain:0),"pressure risks whole haul");
                        if(kind==EncounterRoute.Auction)Require(Deck.Copies(prize.def,prize.rank)==copies+(win?1:0),"auction grants only winning item");
                        wallet=Salvage;stash=Stop.stash;Require(CashOutStop()&&Salvage==wallet+(kind==EncounterRoute.PressureWorks?stash:0)&&!CashOutStop(),"banking and exit cannot duplicate rewards / "+kind);
                    }
                    Event(kind,29);Salvage=0;if(kind!=EncounterRoute.PressureWorks)Require(!ResolveRegionalEvent(0),"unaffordable choice rejected / "+kind);
                    Health=EventHarm(1);if(EventHarm(1)>0)Require(!ResolveRegionalEvent(1),"lethal choice rejected / "+kind);Require(CashOutStop(),"safe exit always available / "+kind);
                }
                Event(EncounterRoute.PressureWorks,42);Roll(true);ResolveRegionalEvent(1);int chance=Stop.chance;int danger=Stop.harm;Require(ContinueRegionalEvent()&&Stop.chance<chance&&Stop.harm>danger,"pressure escalates after success");
                Event(EncounterRoute.Rescue,43);Roll(true);ResolveRegionalEvent(0);int secured=Salvage;Require(ContinueRegionalEvent(),"rescue advances independently");Roll(false);ResolveRegionalEvent(1);Require(Salvage==secured&&Stop.rescued==1&&ContinueRegionalEvent(),"failed rescue retains earlier rewards and permits final worker");Roll(true);ResolveRegionalEvent(1);Require(ContinueRegionalEvent()&&!StopOpen,"three rescues finish exactly once");
                Event(EncounterRoute.Rescue,43);CashOutStop();Wave=6;routedWave=6;SelectedRoute=EncounterRoute.EliteShift;int eliteWallet=Salvage;OpenExpeditionStop();Require(Salvage==eliteWallet+10,"elite clear grants ordinary four plus six salvage");OpenExpeditionStop();Require(Salvage==eliteWallet+10,"elite payout cannot repeat");
                Event(EncounterRoute.Auction,44);Stop.stock=new[]{new RewardOffer(LoadoutCatalog.Find("sword"),1,1)};var sword=Deck.Acquire(LoadoutCatalog.Find("sword"));Deck.Acquire(LoadoutCatalog.Find("sword"));var module=Deck.Acquire(AugmentCatalog.Get(AugmentId.Crosscut));Deck.Attach(module,sword);Roll(true);ResolveRegionalEvent(0);Require(sword.rank==2&&sword.augment==module,"auction copy uses ordinary fusion and preserves socket ownership");
                Event(EncounterRoute.Rescue,45);Screen.SetResolution(1280,720,false);ReducedMotion=true;fx.SetReducedMotion(true);for(int i=0;i<8;i++)yield return null;yield return Shot("rescue-720p");
                foreach(var route in new[]{EncounterRoute.RepairLine,EncounterRoute.FreightYard,EncounterRoute.Reclaimers,EncounterRoute.EliteShift})
                {
                    tuning.heroHealth=10000;BeginExpedition();EquipForemanKit("hammer");Wave=7;expeditionPlan.waves[6]=route;BeginWave();spawnTimer=10000;
                    for(int i=0;i<18;i++)SpawnEntry(NextFormationEntry());
                    Require(enemies.Any(e=>e.crew==RegionalEncounters.Find(route).crew)&&enemies.Count(e=>e.crew!=CrewRole.None)<=3&&VisibleSpawns==0,"authored concealed formation and crew cap / "+route);
                    if(route==EncounterRoute.RepairLine||route==EncounterRoute.EliteShift)Require(enemies.Count(e=>e.heavy)>=3,"heavy screen / "+route);
                    Advance(660);Require(OrderErrors==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills,"physical formation preserves identity / "+route);yield return Shot("formation-"+route);
                }
                Event(EncounterRoute.Auction,1741);Roll(true);ResolveRegionalEvent(0);Require(SaveCheckpoint(),"resolved auction saved for independent process");
                report.itinerary=expeditionPlan;
            }
            report.passed=true;File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));Debug.Log("ITINERARY_REVIEW_SUCCESS / "+report.assertions+" assertions");Application.Quit(0);
        }
    }
}
