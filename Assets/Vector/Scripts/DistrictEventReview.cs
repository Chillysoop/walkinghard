using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        internal bool DistrictForkTextFits(string title,string reward)=>Style(18,true).CalcSize(new GUIContent(title)).x<=150&&Style(12,true).CalcSize(new GUIContent(reward)).x<=150;
        internal void EventReviewRecap(){recapScroll=new Vector2(0,310);}
        internal void EventReviewArchive(bool open){if(open)game.MainMenu();expeditionArchive=open;archivePage=2;}
    }
    public sealed partial class VectorEncounter
    {
        [Serializable] sealed class EventEconomyRow
        {
            public string kind,policy;public int samples,wins,items;public float salvageNet,integrityNet,copyPriceValue;public int[] ranks=new int[6];
        }
        [Serializable] sealed class DistrictEventReport
        {
            public bool passed;public int assertions;public string failure,scope;
            public List<string> checks=new List<string>();public List<EventEconomyRow> economy=new List<EventEconomyRow>();
        }
        IEnumerator ReviewDistrictEvents()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);FastSimulation=false;
            var args=Environment.GetCommandLineArgs();bool render=!args.Contains("-nographics"),economy=args.Contains("-eventEconomy"),resume=args.Contains("-eventResumeOnly");
            string Arg(string key,string fallback){int i=Array.IndexOf(args,key);return i<0?fallback:args[i+1];}
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Event review requires an isolated profile");
            string output=Path.GetFullPath(Arg("-reviewOutput","DistrictEventReview"));Directory.CreateDirectory(output);
            var report=new DistrictEventReport{scope=economy?"Deterministic production-event sample: 1000 seeds per event/option. Common equipped sword; spare Uncommon pistol; full unlocks; 280 max / 180 current integrity; one attempt then cashout. Item value uses shop copy price, not an equivalent currency payout or human preference.":"Production event acquisition, payments, storage consumption, fusion, reward filters, saved randomness, legacy migration and pointer/text-layout fixtures. Controlled outcomes are functional evidence, not difficulty evidence."};
            void Write()=>File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
            void Require(bool ok,string rule){if(!ok){report.failure=rule;Write();throw new InvalidOperationException("DISTRICT_EVENT_REVIEW: "+rule);}report.assertions++;report.checks.Add(rule);}
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception){report.failure=m;Write();Application.Quit(3);}};
            IEnumerator Shot(string name){if(!render)yield break;yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());Destroy(t);yield return null;}
            IEnumerator TextCheck(string name){if(!render)yield break;hud.eventTextAudit=()=>Require(hud.DistrictEventTextFits(),"native text fits / "+name);float end=Time.realtimeSinceStartup+3;while(hud.eventTextAudit!=null&&Time.realtimeSinceStartup<end)yield return null;Require(hud.eventTextAudit==null,"OnGUI text audit completed / "+name);}
            void Setup(EncounterRoute kind,int seed=1741,bool unlocked=true)
            {
                originalEvents=false;office=new OfficeProfile{seal=unlocked,unlocks=unlocked?Constellation.Nodes.Select(n=>n.item).ToList():new List<string>()};
                forcedSeed=true;tuning.seed=seed;tuning.heroHealth=280;StartChapter();OpeningPicks=2;Wave=kind==EncounterRoute.PressureWorks||kind==EncounterRoute.Rescue?4:kind==EncounterRoute.Auction||kind==EncounterRoute.Reclamation?11:18;
                if(Wave>=3)Deck.Open(DraftKind.Ultimate,false);var sword=Deck.Acquire(LoadoutCatalog.Find("sword"));Deck.Move(sword,PlaceKind.Weapon,0);Deck.Acquire(LoadoutCatalog.Find("pistol"),2);
                Salvage=100;Health=180;routedWave=Wave+1;SelectedRoute=kind;State=RunState.Drafting;OpenExpeditionStop();
            }
            void Roll(bool win,int option=0,bool second=false)
            {
                int seed=Enumerable.Range(1,10000).First(s=>{var r=new System.Random(s);bool pass=r.Next(1,101)<=DistrictEventChance(option);int prize=r.Next(1,101);return pass==win&&(Stop.kind!=EncounterRoute.Reclamation&&Stop.kind!=EncounterRoute.DeadLetter||(prize>(Stop.kind==EncounterRoute.Reclamation?50:75))==second);});eventRandom=new CheckpointRandom(seed);
            }
            QualitySettings.vSyncCount=0;Application.targetFrameRate=render?60:-1;hud.TrailerCaptureMode();hud.TrailerClearSelection();
            if(render)Screen.SetResolution(1920,1080,false);for(int i=0;i<8;i++)yield return null;
            if(resume)
            {
                Require(ResumeExpedition()&&DistrictEventOpen&&Stop.reveal&&Stop.kind==EncounterRoute.Reclamation,"independent process restores resolved exchange");
                var input=CurrentEventLot.input;var reward=Stop.districtEvent.reward;Require(!Deck.Items.Contains(input)&&Deck.Items.Contains(reward)&&!ResolveDistrictEvent(0),"independent process cannot replay or resurrect the surrendered item");
                Require(ContributionReports().Any(r=>r.exchanged&&r.values.direct==120&&r.kills==3),"independent process preserves surrendered-item history");
                string graph=JsonUtility.ToJson(CaptureCheckpoint().graph);Require(SaveCheckpoint()&&ResumeExpedition()&&JsonUtility.ToJson(CaptureCheckpoint().graph)==graph,"independent-process graph round trip");
            }
            else if(economy)
            {
                Setup(EncounterRoute.PressureWorks);qa=true;
                foreach(var kind in DistrictEvents.Pools.SelectMany(p=>p))foreach(int option in new[]{0,1})
                {
                    if(option==1&&kind!=EncounterRoute.PressureWorks&&kind!=EncounterRoute.Rescue&&kind!=EncounterRoute.Auction)continue;
                    var row=new EventEconomyRow{kind=kind.ToString(),policy="option "+option+" then bank",samples=1000};report.economy.Add(row);
                    for(int seed=1;seed<=row.samples;seed++)
                    {
                        Deck=new AbilityDeck(seed);Deck.Open(DraftKind.Ultimate,false);BindChapterRewards();var sword=Deck.Acquire(LoadoutCatalog.Find("sword"));Deck.Move(sword,PlaceKind.Weapon,0);Deck.Acquire(LoadoutCatalog.Find("pistol"),2);
                        eventRandom=new CheckpointRandom(seed);Stop=new ExpeditionStop{kind=kind};PrepareDistrictEvent();Salvage=100;Health=180;chapterRun.rescued=0;eventAudits.Clear();
                        Require(ResolveDistrictEvent(option),"economy choice / "+kind+" / "+option+" / "+seed);
                        if(!Stop.failed)row.wins++;if(Stop.districtEvent.reward!=null){row.items++;int rank=Stop.districtEvent.awardedRank;row.ranks[rank]++;row.copyPriceValue+=CopyCost(rank);}
                        CashOutStop();row.salvageNet+=Salvage-100;row.integrityNet+=Health-180;
                        if(seed%100==0)yield return null;
                    }
                    row.salvageNet/=row.samples;row.integrityNet/=row.samples;row.copyPriceValue/=row.samples;Write();
                }
                qa=false;
            }
            else
            {
                var kinds=new HashSet<EncounterRoute>();
                for(int seed=1;seed<=120;seed++)
                {
                    var plan=LongJourney.Plan(seed);var old=LongJourney.Plan(seed,true);
                    Require(plan.Valid()&&plan.waves.SequenceEqual(old.waves)&&JsonUtility.ToJson(plan)==JsonUtility.ToJson(LongJourney.Plan(seed)),"stable plan preserves combat sequence / "+seed);
                    for(int district=0;district<3;district++)
                    {int at=district*3+1;var route=RegionalEncounters.IsStop(plan.left[at])?plan.left[at]:plan.right[at];kinds.Add(route);Require(DistrictEvents.Pools[district].Contains(route),"district owns its event pool / "+seed+" / "+district);}
                }
                Require(kinds.Count==6,"seed pool reaches all six events");
                foreach(var kind in DistrictEvents.Pools.SelectMany(p=>p))
                {
                    Setup(kind);Require(DistrictEventOpen&&stopProp.GetComponentsInChildren<Collider>().All(c=>!c.enabled),"production arrival opens noncolliding event / "+kind);
                    var pos=HeroPosition;float time=Elapsed;AdvanceCombat(1);Require(HeroPosition==pos&&Elapsed==time&&!ClaimReward(0,PlaceKind.Inventory,0,false),"combat frozen and rewards gated / "+kind);
                    Require(!PushOvertime()&&!ContinueOvertime()&&!ResolveRegionalEvent(0)&&!ContinueRegionalEvent(),"old handlers cannot bypass new terms / "+kind);
                    int draws=eventRandom.Draws;foreach(var lot in Stop.districtEvent.lots)Deck.PreviewChange(lot.first,null,PlaceKind.Inventory);if(Stop.districtEvent.lots.Count>1){SelectEventLot(1);SelectEventLot(0);}Require(eventRandom.Draws==draws,"browsing and previews consume no random draws / "+kind);
                    yield return TextCheck(kind.ToString());yield return Shot("choice-"+kind);
                    Require(SaveCheckpoint(),"pre-choice save / "+kind);string saved=File.ReadAllText(SavePath);
                    Require(hud.HandleDistrictEventPointer(EventType.MouseDown,hud.DistrictEventOptionRect(0,1600,900).center,1600,900),"production choice pointer / "+kind);
                    string result=Stop.result;float hp=Health;int wallet=Salvage;
                    Require(!ResolveDistrictEvent(0)&&!ResolveDistrictEvent(1)&&!SelectEventLot(0),"resolved choice and selection cannot replay / "+kind);
                    AtomicWrite(SavePath,saved);Require(ResumeExpedition()&&ResolveDistrictEvent(0)&&Stop.result==result&&Health==hp&&Salvage==wallet,"pre-choice reload gives identical outcome / "+kind);
                    string graph=JsonUtility.ToJson(CaptureCheckpoint().graph);Require(SaveCheckpoint()&&ResumeExpedition()&&JsonUtility.ToJson(CaptureCheckpoint().graph)==graph,"resolved graph exactly restored / "+kind);
                    yield return TextCheck("result "+kind);yield return Shot("result-"+kind);Require(CashOutStop()&&!CashOutStop(),"leave is idempotent / "+kind);
                    int options=kind==EncounterRoute.PressureWorks||kind==EncounterRoute.Rescue||kind==EncounterRoute.Auction?2:1;
                    for(int option=0;option<options;option++)foreach(bool win in kind==EncounterRoute.Reclamation?new[]{true}:new[]{false,true})
                    {
                        Setup(kind,42);Roll(win,option);wallet=Salvage;hp=Health;int price=DistrictEventPrice(option),harm=DistrictEventHarm(option),stash=Stop.stash;var input=CurrentEventLot?.input;
                        Require(ResolveDistrictEvent(option)&&Stop.failed!=win,"controlled outcome / "+kind+" / "+option+" / "+win);
                        Require(Salvage==wallet-price+(win&&kind==EncounterRoute.Rescue?Stop.gain:0),"exact wallet cost / "+kind);
                        float expected=hp-(kind==EncounterRoute.Overtime||!win?harm:0)+(kind==EncounterRoute.Rescue&&win?Stop.repair:0);
                        Require(Mathf.Abs(Health-Mathf.Min(MaxHealth,expected))<.01f,"exact integrity cost / "+kind);
                        if(kind==EncounterRoute.PressureWorks)Require(Stop.stash==(win?stash+Stop.gain*(option==1?2:1):0),"pressure risks unbanked haul only");
                        if(kind==EncounterRoute.Reclamation)Require(!Deck.Items.Contains(input)&&Stop.districtEvent.reward.rank==input.rank,"exchange consumes exactly selected item and keeps rarity");
                        if(kind==EncounterRoute.DeadLetter||kind==EncounterRoute.Overtime)Require(Stop.stash==0,"job/cache surrenders safe alternative on either result");
                    }
                    Setup(kind);Health=DistrictEventHarm(0);Require(!CanResolveDistrictEvent(0)&&CashOutStop(),"lethal attempt rejected; exit works / "+kind);
                    Setup(kind);Salvage=0;if(DistrictEventPrice(0)>0)Require(!ResolveDistrictEvent(0)&&CashOutStop(),"unaffordable attempt rejected / "+kind);
                }
                Setup(EncounterRoute.PressureWorks);Roll(true);ResolveDistrictEvent(0);int chance=Stop.chance,harm0=Stop.harm;Require(ContinueDistrictEvent()&&Stop.chance<chance&&Stop.harm>harm0,"pressure escalation is shown before another commitment");
                Setup(EncounterRoute.Rescue);Roll(true);ResolveDistrictEvent(0);int secured=Salvage,workers=RescuedWorkers;Require(ContinueDistrictEvent(),"rescue advances");Roll(false,1);ResolveDistrictEvent(1);Require(Salvage==secured&&Stop.rescued==1&&RescuedWorkers==workers&&ContinueDistrictEvent(),"failed later rescue keeps prior rewards and Office worker");Roll(true);ResolveDistrictEvent(0);Require(ContinueDistrictEvent()&&!StopOpen,"third rescue ends event");
                foreach(bool second in new[]{false,true})foreach(var kind in new[]{EncounterRoute.Reclamation,EncounterRoute.DeadLetter})
                {Setup(kind);Roll(true,0,second);var lot=CurrentEventLot;ResolveDistrictEvent(0);Require(Stop.districtEvent.reward.def==(second?lot.second:lot.first).def,"both announced alternatives reachable / "+kind+" / "+second);}
                Setup(EncounterRoute.Reclamation);var stored=CurrentEventLot.input;Deck.Move(stored,PlaceKind.Weapon,1);Require(!ResolveDistrictEvent(0),"stale mounted input cannot be consumed");
                Setup(EncounterRoute.Reclamation);stored=CurrentEventLot.input;var mod=Deck.Acquire(AugmentCatalog.Get(AugmentId.BoreRounds));Require(Deck.Attach(mod,stored)&&!ResolveDistrictEvent(0),"augmented stored item cannot be consumed");
                Deck.Items.Remove(stored);Stop=new ExpeditionStop{kind=EncounterRoute.Reclamation};PrepareDistrictEvent();Require(CurrentEventLot==null&&!ResolveDistrictEvent(0),"empty exchange has no charge or invalid roll");yield return TextCheck("empty exchange");yield return Shot("empty-exchange");Require(CashOutStop(),"empty exchange has safe exit");
                foreach(string id in new[]{"sword","hammer"})
                {
                    Setup(EncounterRoute.Overtime);Deck=new AbilityDeck(10);BindChapterRewards();var weapon=Deck.Acquire(LoadoutCatalog.Find(id));Deck.Move(weapon,PlaceKind.Weapon,0);Deck.Acquire(weapon.def);Deck.Acquire(weapon.def,2);Deck.Acquire(weapon.def,2);
                    var module=Deck.Acquire(AugmentCatalog.Get(id=="sword"?AugmentId.Crosscut:AugmentId.Piledriver));Deck.Attach(module,weapon);
                    CurrentEventLot.first=new RewardOffer(weapon.def);var projection=Deck.PreviewChange(CurrentEventLot.first,null,PlaceKind.Inventory);Require(projection.incoming.rank==3&&projection.fusionRanks.SequenceEqual(new[]{2,3}),"preview predicts complete fusion chain / "+id);
                    yield return TextCheck("fusion preview "+id);yield return Shot("fusion-preview-"+id);Roll(true);ResolveDistrictEvent(0);
                    Require(weapon.rank==3&&weapon.augment==module&&Stop.districtEvent.reward==weapon&&Deck.Weapons[0]==weapon&&(id!="hammer"||Deck.Weapons[1]==weapon),"event fusion retains sockets and equipped 2H owner / "+id);
                    yield return TextCheck("fusion result "+id);yield return Shot("fusion-result-"+id);
                }
                Setup(EncounterRoute.Overtime);Deck=new AbilityDeck(10);BindChapterRewards();var paired=Deck.Acquire(LoadoutCatalog.Find("sword"));Deck.Move(paired,PlaceKind.Weapon,0);var partner=Deck.Acquire(paired.def);Deck.Move(partner,PlaceKind.Weapon,1);CurrentEventLot.first=new RewardOffer(paired.def);
                Require(hud.EventRewardDestination(CurrentEventLot.first,out _).Contains("HAND 2 → FISTS"),"fusion preview calls out consuming a second equipped copy");yield return Shot("paired-fusion-preview");Roll(true);ResolveDistrictEvent(0);Require(Deck.Weapons[1]==null&&Stop.result.Contains("Hand 2 returns to fists"),"paired fusion receipt names the affected mount");
                Setup(EncounterRoute.Overtime);Deck.Acquire(LoadoutCatalog.Fists);Deck.Move(Deck.Inventory.First(i=>i.def.id=="pistol"),PlaceKind.Weapon,1);CurrentEventLot.first=new RewardOffer(LoadoutCatalog.Fists);Require(hud.EventRewardDestination(CurrentEventLot.first,out _).Contains("FALLBACK FISTS"),"inactive starter-fist fusion is not claimed as equipped");Roll(true);ResolveDistrictEvent(0);Require(Stop.result.Contains("fallback fists"),"inactive fist receipt preserves mounted weapons");
                foreach(bool unlocked in new[]{false,true})
                {
                    Setup(EncounterRoute.Auction,42,unlocked);
                    foreach(var kind in DistrictEvents.Pools.SelectMany(p=>p))for(int seed=1;seed<=80;seed++)
                    {Stop=new ExpeditionStop{kind=kind};eventRandom=new CheckpointRandom(seed);PrepareDistrictEvent();Require(Stop.districtEvent.lots.SelectMany(l=>new[]{l.first,l.second}).Where(o=>o!=null).All(o=>Deck.EventRewardAllowed(o.def)),"event respects run reward pool / "+kind+" / "+unlocked+" / "+seed);}
                }
                Setup(EncounterRoute.Auction);var bad=CaptureCheckpoint();Stop.districtEvent.selected=99;Require(SaveCheckpoint()&&!ResumeExpedition(),"invalid selection is rejected before replacing world");AtomicWrite(SavePath,JsonUtility.ToJson(bad));Require(ResumeExpedition(),"valid checkpoint remains loadable after invalid fixture");
                originalEvents=true;Setup(EncounterRoute.Overtime);Stop.districtEvent=null;Stop.chance=82;Stop.gain=12;Stop.stash=8;Stop.harm=10;Stop.variant=1;var legacy=CaptureCheckpoint();legacy.schema=11;
                foreach(var node in legacy.graph.objects.Where(n=>n.type==typeof(ExpeditionStop).FullName))node.fields.RemoveAll(f=>f.name=="districtEvent");AtomicWrite(SavePath,JsonUtility.ToJson(legacy));
                Require(ResumeExpedition()&&!DistrictEventOpen&&Stop.chance==82&&Stop.gain==12&&PushOvertime(),"schema 11 active overtime retains its legacy terms");
                bool rejected=false;try{GraphCodec().Decode(legacy.graph);}catch(InvalidOperationException){rejected=true;}Require(rejected,"current graph rejects a missing district-event field");
                RecordOvertime(6);RecordOvertime(7);InitializeExpeditionStorage(args);Require(HasOvertimeRecord(6)&&HasOvertimeRecord(7),"both new lore records persist in the archive");
                if(render){hud.EventReviewArchive(true);Require(State==RunState.Ready&&hud.ExpeditionOverlayOpen,"archive fixture opens the actual menu overlay");yield return Shot("archive-new-records");hud.EventReviewArchive(false);}
                if(render)foreach(var route in new[]{EncounterRoute.Reclamation,EncounterRoute.DeadLetter,EncounterRoute.Overtime})
                {
                    StartChapter();ProbeReset();OpeningPicks=2;Wave=route==EncounterRoute.Reclamation?11:18;BeginWave();spawnTimer=10000;int fork=ChapterEncounter.fork;expeditionPlan.left[fork]=route;expeditionPlan.right[fork]=EncounterRoute.EliteShift;BeginWalkingFork();
                    heroPhysics.Motor.position=forkOrigin-RoadForward*5;Physics.SyncTransforms();cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
                    hud.eventTextAudit=()=>Require(hud.DistrictForkTextFits(ForkThreatLabel(route),ForkRewardLabel(route)),"event fork labels fit / "+route);float deadline=Time.realtimeSinceStartup+3;while(hud.eventTextAudit!=null&&Time.realtimeSinceStartup<deadline)yield return null;Require(hud.eventTextAudit==null,"fork text audited / "+route);
                    Require(hud.HandleForkPointer(EventType.MouseDown,hud.ForkChoiceRect(0,1600,900).center,1600,900)&&ForkChoice==route,"native pointer selects event road / "+route);yield return Shot("fork-"+route);
                    CommitWalkingFork();Require(SelectedRoute==route&&roads.Path.Contains(forkVisual.Left),"event selection commits its own physical road / "+route);
                }
                if(render)foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(2560,1080)})
                {
                    Screen.SetResolution(size.x,size.y,false);ReducedMotion=true;fx.SetReducedMotion(true);for(int i=0;i<10;i++)yield return null;
                    foreach(var kind in new[]{EncounterRoute.Reclamation,EncounterRoute.DeadLetter,EncounterRoute.Rescue}){Setup(kind);yield return TextCheck(kind+" / "+size);yield return Shot(kind+"-"+size.x+"x"+size.y);}
                }
                Setup(EncounterRoute.Reclamation);stored=CurrentEventLot.input;ContributionFor(stored).direct=120;ContributionFor(stored).healing=7;ContributionFor(stored).assists=2;itemKills[stored]=3;
                Roll(true);ResolveDistrictEvent(0);Require(RecordedItems.Contains(stored)&&ContributionReports().Any(r=>r.exchanged&&r.item==stored.def.id&&r.values.direct==120&&r.values.healing==7&&r.kills==3),"exchange preserves named per-item history without attributing it to replacement");
                Require(LiveTotals().damage==120&&LiveTotals().healing==7&&LiveOtherStats(LiveSequenceStats()).damage==120,"live totals include exchanged equipment exactly once");
                Require(SaveCheckpoint()&&ResumeExpedition()&&ContributionReports().Any(r=>r.exchanged&&r.values.direct==120&&r.kills==3),"exchanged-item contribution identity survives checkpoint restore");
                BeginFeedbackWave();stored=CurrentEventLot.input;ContributionFor(stored).collision+=11;CaptureWaveFeedback();Require(LastWaveFeedback.items.Any(r=>r.id==stored.def.id&&r.values.collision==11),"lingering attributed effects remain visible in later encounter feedback");
                if(render){State=RunState.Complete;hud.EventReviewRecap();yield return Shot("exchanged-item-recap");State=RunState.Drafting;}
                Require(SaveCheckpoint(),"resolved exchange left for a fresh-process review");
            }
            report.passed=true;Write();Debug.Log("DISTRICT_EVENT_REVIEW_SUCCESS "+report.assertions);Application.Quit(0);
        }
    }
}
