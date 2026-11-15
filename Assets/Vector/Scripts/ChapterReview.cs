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
        [Serializable] sealed class ChapterReport
        {public bool passed;public int assertions,productionAssertions,wave,kills,itemChoices,augmentChoices,sparks,workers;public float health;public string result,scope;public List<string> checks=new List<string>();}
        IEnumerator ReviewChapter()
        {
            legacyChapterReview=true;enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();bool render=!args.Contains("-nographics"),full=args.Contains("-chapterFullRun"),resume=args.Contains("-chapterResumeOnly"),legacy=args.Contains("-chapterLegacyResume");
            string Arg(string key,string fallback){int at=Array.IndexOf(args,key);return at<0?fallback:args[at+1];}
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Chapter review needs an isolated profile");
            string output=Path.GetFullPath(Arg("-reviewOutput",Path.Combine(Application.dataPath,"..","ChapterReview")));Directory.CreateDirectory(output);
            var report=new ChapterReport{scope=full?"One normal-health chapter, ordinary drafts, automated pinball and stop choices. Functional coverage, not balance evidence.":"Controlled chapter/Office/constellation/save fixtures. Not balance evidence."};
            void Require(bool ok,string rule){if(!ok)throw new InvalidOperationException("CHAPTER_REVIEW: "+rule);report.assertions++;report.checks.Add(rule);}
            IEnumerator Shot(string name){if(!render)yield break;yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());Destroy(t);}
            QualitySettings.vSyncCount=0;Application.targetFrameRate=render?60:-1;if(render)Screen.SetResolution(1920,1080,false);for(int i=0;i<8;i++)yield return null;
            if(resume||legacy)
            {
                Require(ResumeExpedition(),"fresh process restores checkpoint");
                Require(ChapterActive!=legacy&&WaveCount==(legacy?8:6),"saved run retains its chapter or legacy format");
                Require(SaveCheckpoint(),"restored checkpoint saves in current schema");var captured=CaptureCheckpoint();Require(ResumeExpedition()&&JsonUtility.ToJson(captured.graph)==JsonUtility.ToJson(CaptureCheckpoint().graph),"exact graph survives another resume");
                if(!legacy){Require(HasConstellation("wraps")&&HasConstellation("greatsword")&&ChapterRewardAllowed(LoadoutCatalog.Find("wraps")),"profile unlock and run reward pool survive process restart");int before=Sparks;Require(SyncChapterProgress()&&Sparks==before,"resume cannot repay banked Sparks");}
            }
            else if(full)
            {
                forcedSeed=true;tuning.seed=int.Parse(Arg("-seed","1741"));simulationPolicy="pinball";StartChapter();float started=Time.realtimeSinceStartup;var shots=new HashSet<string>();
                while(State!=RunState.Complete&&State!=RunState.Defeated)
                {
                    if(Time.realtimeSinceStartup-started>360)throw new InvalidOperationException("Chapter full run timeout");
                    if(State==RunState.Drafting)
                    {
                        if(StopOpen)SimulationExpeditionStop();else SimulationDraft();
                        if(State==RunState.Drafting){var saved=JsonUtility.ToJson(chapterRun);Require(SaveCheckpoint()&&ResumeExpedition()&&JsonUtility.ToJson(chapterRun)==saved,"checkpoint preserves chapter and unlock snapshot");}
                    }
                    else
                    {
                        for(int i=0;i<45&&State==RunState.Running;i++){AdvanceCombat(1f/60);if(RoutePending)ChooseRoute(SimulationExpeditionRoute());}
                        if(ForemanPresent&&shots.Add("foreman"))yield return Shot("foreman");
                        if(WaveElapsed>18&&shots.Add("wave-"+Wave))yield return Shot("wave-"+Wave);
                    }
                    yield return null;
                }
                Require(State==RunState.Complete&&Wave==6&&Waves.Count==6,"normal-health chapter completes at Foreman");
                Require(!FinaleActive&&corpses.Any(e=>e.foreman!=null)&&!corpses.Any(e=>e.boss&&e.foreman==null),"Foreman ends chapter without Pursuer transition");
                Require(ItemChoices==10&&AugmentChoices==2&&roads.Forks.Count==2,"chapter retains defined rewards and both literal forks");
                Require(OrderErrors==0&&VisibleSpawns==0&&corpses.Count==Kills&&Spawned==Kills+enemies.Count,"order, concealed arrivals and physical identity contracts");
                Require(Sparks==9&&CompletionSeal,"ordinary clear banks nine Sparks and first seal");
                FinishExpeditionStorage();Require(!HasCheckpoint,"chapter completion clears resumable run");yield return Shot("chapter-result");
                report.wave=Wave;report.kills=Kills;report.health=Health;report.result=State.ToString();report.itemChoices=ItemChoices;report.augmentChoices=AugmentChoices;
                OpenOffice();Require(OfficeOpen&&State==RunState.Ready,"completion returns to Office");yield return Shot("office-return");
            }
            else
            {
                CheckCombatMechanics();report.productionAssertions=mechanicAssertions;LoadOffice();Require(Sparks==0&&!CompletionSeal,"new Office starts without currency or seal");
                OpenOffice();yield return Shot("office-new");Require(!BuyConstellation("wraps")&&!BuyConstellation("unknown"),"unaffordable and unknown unlocks are blocked");
                StartChapter();Require(ChapterActive&&WaveCount==6&&chapterRun.Valid(),"new chapter uses stable six-encounter definition");
                var locked=Constellation.Nodes.Select(n=>n.item).ToArray();var seen=new HashSet<string>();
                foreach(int seed in Enumerable.Range(1,50))foreach(var kind in new[]{DraftKind.Weapon,DraftKind.Equipment,DraftKind.Ultimate,DraftKind.Mixed})
                {var deck=new AbilityDeck(seed){RewardFilter=ChapterRewardAllowed};deck.Open(kind,false);Require(deck.Offers.Length==3&&deck.Offers.All(o=>!locked.Contains(o.def.id)),"locked items excluded from all draft paths / "+kind);foreach(var o in deck.Offers)seen.Add(o.def.id);}
                Require(LoadoutCatalog.Ultimates.Count(d=>seen.Contains(d.id))==3,"fresh chapter retains three distinct ultimate choices");
                Wave=1;ClearChapterEncounter();Require(Sparks==1&&SaveCheckpoint(),"cleared encounter banks currency and saves checkpoint");
                string old=File.ReadAllText(SavePath);Wave=3;ClearChapterEncounter();Require(Sparks==3,"cleared encounter receipts award only the remaining steps");
                OpenOffice();Require(hud.HandleOfficePointer(EventType.MouseDown,hud.OfficeNodeButton(0,1280,720).center,1280,720)&&Sparks==0&&HasConstellation("wraps"),"production node pointer buys first star for three Sparks");
                Require(!BuyConstellation("wraps")&&!BuyConstellation("greatsword")&&Sparks==0,"duplicate and locked outer star cannot charge");
                AtomicWrite(SavePath,old);Require(ResumeExpedition()&&!ChapterRewardAllowed(LoadoutCatalog.Find("wraps")),"purchase does not change an existing run's reward pool");
                Wave=3;ClearChapterEncounter();Require(Sparks==0,"replaying older checkpoint cannot repay cleared encounters");
                Stop=new ExpeditionStop{kind=EncounterRoute.Rescue};PrepareRegionalEvent();State=RunState.Drafting;Salvage=100;Health=180;
                int winSeed=Enumerable.Range(1,1000).First(s=>new System.Random(s).Next(1,101)<=5);eventRandom=new CheckpointRandom(winSeed);
                Require(ResolveRegionalEvent(0)&&RescuedWorkers==1,"successful rescue persists a worker in Office");Require(!ResolveRegionalEvent(0)&&RescuedWorkers==1,"duplicate rescue click does not duplicate workers");
                Require(SaveCheckpoint()&&ResumeExpedition()&&SyncChapterProgress()&&RescuedWorkers==1,"rescue replay does not duplicate worker count");
                Wave=6;ClearChapterEncounter();Require(Sparks==6&&CompletionSeal,"Foreman completion opens outer tier and banks remaining Sparks");
                Require(SyncChapterProgress()&&Sparks==6&&office.victories==1,"duplicate completion cannot duplicate seal, victory or currency");
                OpenOffice();Require(BuyConstellation("greatsword")&&Sparks==1,"outer equipment star spends five Sparks after parent and seal");
                yield return Shot("office-progress");
                if(render){Screen.SetResolution(1280,720,false);for(int i=0;i<8;i++)yield return null;yield return Shot("office-720p");}
                var before=JsonUtility.ToJson(office);string directory=saveDirectoryOverride;string blocked=Path.Combine(output,"blocked-file");File.WriteAllText(blocked,"fixture");saveDirectoryOverride=blocked;
                Require(!SaveOffice(new OfficeProfile{sparks=999})&&JsonUtility.ToJson(office)==before,"failed disk write preserves in-memory wallet");saveDirectoryOverride=directory;
                AtomicWrite(OfficePath,JsonUtility.ToJson(office));File.WriteAllText(OfficePath,"broken");LoadOffice();Require(officeStorageReady&&HasConstellation("greatsword"),"corrupt primary recovers valid backup");
                AtomicWrite(OfficePath,before);LoadOffice();StartChapter();Require(ChapterRewardAllowed(LoadoutCatalog.Find("wraps"))&&ChapterRewardAllowed(LoadoutCatalog.Find("greatsword"))&&!ChapterRewardAllowed(LoadoutCatalog.Find("redline")),"new chapter snapshots purchased nodes only");
                seen.Clear();for(int seed=0;seed<80;seed++){var deck=new AbilityDeck(seed){RewardFilter=ChapterRewardAllowed};deck.Open(DraftKind.Weapon,false);foreach(var offer in deck.Offers)seen.Add(offer.def.id);}
                Require(seen.Contains("wraps")&&seen.Contains("greatsword"),"purchased equipment actually appears in new offers");
                var snapshot=new List<string>(chapterRun.unlocks);chapterRun.unlocks=Constellation.Nodes.Select(n=>n.item).ToList();seen.Clear();
                foreach(int seed in Enumerable.Range(1,100))foreach(var kind in new[]{DraftKind.Equipment,DraftKind.Ultimate,DraftKind.Mixed}){var d=new AbilityDeck(seed){RewardFilter=ChapterRewardAllowed};d.Open(kind,false);foreach(var o in d.Offers)seen.Add(o.def.id);}
                Require(Constellation.Nodes.All(n=>seen.Contains(n.item)),"all six unlocked identities can enter equipment, ultimate and mixed pools");chapterRun.unlocks=snapshot;
                var profileBefore=JsonUtility.ToJson(office);OpenOffice();var funded=CopyOffice();funded.sparks=30;Require(SaveOffice(funded),"controlled purchase fixture persists funding");
                foreach(var node in Constellation.Nodes.Where(n=>!HasConstellation(n.item))){int wallet=Sparks;Require(BuyConstellation(node.item)&&Sparks==wallet-node.cost&&!BuyConstellation(node.item),"each remaining star charges exactly once / "+node.item);}
                AtomicWrite(OfficePath,profileBefore);LoadOffice();
                string good=JsonUtility.ToJson(office);File.WriteAllText(OfficePath,"broken");File.WriteAllText(OfficePath+".bak","broken");LoadOffice();Require(!officeStorageReady&&!StartChapter()&&!BuyConstellation("relay"),"unreadable profile blocks purchases and new chapters without overwriting files");AtomicWrite(OfficePath,good);LoadOffice();StartChapter();

                State=RunState.Defeated;FinishExpeditionStorage();Require(Sparks==1&&CompletionSeal&&!HasCheckpoint,"defeat retains banked progression and clears run");
                MainMenu();Begin();Require(!ChapterActive&&WaveCount==8&&ChapterRewardAllowed(LoadoutCatalog.Find("redline")),"Quick Run retains eight waves and full catalog");
                BeginLegacyExpedition();Require(!ChapterActive&&WaveCount==8&&SaveCheckpoint(),"legacy Expedition path remains eight waves");Require(ResumeExpedition()&&!ChapterActive,"legacy-format current-schema resume stays legacy");
                StartChapter();Require(SaveCheckpoint(),"chapter checkpoint left for independent fresh process");
            }
            report.sparks=Sparks;report.workers=RescuedWorkers;report.passed=true;File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));Debug.Log("CHAPTER_REVIEW_SUCCESS / "+report.assertions);Application.Quit(0);
        }
    }
}
