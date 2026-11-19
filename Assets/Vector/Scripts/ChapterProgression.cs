using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed class ChapterEncounter
    {
        public readonly string id,name;public readonly bool boss;public readonly int fork,sparks;public readonly DraftKind[] rewards;
        public ChapterEncounter(string id,string name,int fork,int sparks,bool boss,params DraftKind[] rewards){this.id=id;this.name=name;this.fork=fork;this.sparks=sparks;this.boss=boss;this.rewards=rewards;}
    }
    public static class CinderworksChapter
    {
        public const string Id="cinderworks-1";
        public static readonly ChapterEncounter[] Encounters={
            new ChapterEncounter("gate","Factory gate",-1,1,false,DraftKind.Ability,DraftKind.Weapon),
            new ChapterEncounter("repair","Repair line",0,1,false,DraftKind.Mixed,DraftKind.Equipment),
            new ChapterEncounter("freight","Freight yard",-1,1,false,DraftKind.Ultimate,DraftKind.Augment),
            new ChapterEncounter("recovery","Recovery detail",-1,1,false,DraftKind.Mixed,DraftKind.Equipment),
            new ChapterEncounter("junction","Last junction",1,1,false,DraftKind.Mixed,DraftKind.Augment),
            new ChapterEncounter("foreman","Shift Foreman",-1,4,true)
        };
    }
    [Serializable] public sealed class ChapterRun
    {
        public string definition=LongJourney.Id,id;public List<string> unlocks=new List<string>();public int cleared,rescued;public bool complete;
        public bool Valid()=>(definition==CinderworksChapter.Id||definition==LongJourney.Id)&&Guid.TryParse(id,out _)&&unlocks!=null&&unlocks.All(x=>Constellation.Nodes.Any(n=>n.item==x))&&cleared>=0&&cleared<=(definition==LongJourney.Id?21:6)&&rescued>=0&&rescued<=27&&(!complete||cleared==(definition==LongJourney.Id?21:6));
    }
    [Serializable] public sealed class ChapterReceipt {public string id;public int cleared,rescued;public bool complete,seal;}
    [Serializable] public sealed class OfficeProfile
    {
        public int schema=1,sparks,rescued,victories;public bool seal;public List<string> unlocks=new List<string>();public List<ChapterReceipt> receipts=new List<ChapterReceipt>();
    }
    public sealed class ConstellationNode
    {
        public readonly string item,parent;public readonly int cost,branch;public bool NeedsSeal=>parent!=null;
        public ConstellationNode(string item,int branch,int cost,string parent=null){this.item=item;this.branch=branch;this.cost=cost;this.parent=parent;}
    }
    public static class Constellation
    {
        public static readonly ConstellationNode[] Nodes=new[]{new ConstellationNode("wraps",0,3),new ConstellationNode("greatsword",0,5,"wraps"),new ConstellationNode("relay",1,3),new ConstellationNode("magnet",1,5,"relay"),new ConstellationNode("harvest",2,3),new ConstellationNode("redline",2,5,"harvest")}.Concat(Enumerable.Range(1,6).SelectMany(b=>{var ids=ConstellationContent.BranchItems[b];return new[]{new ConstellationNode(ids[0],b+2,4),new ConstellationNode(ids[1],b+2,6,ids[0]),new ConstellationNode(ids[2],b+2,6,ids[0]),new ConstellationNode(ids[3],b+2,8,ids[1]),new ConstellationNode(ids[4],b+2,10,ids[2])};})).ToArray();
    }
    public sealed partial class VectorEncounter
    {
        ChapterRun chapterRun;bool chapterRequested,officeStorageReady=true;
        OfficeProfile office=new OfficeProfile();
        public bool OfficeOpen {get;private set;}
        public string OfficeMessage {get;private set;}="";
        public bool ChapterActive=>ExpeditionActive&&chapterRun!=null;
        public int Sparks=>office.sparks;
        public int ConstellationOwned=>office.unlocks.Count;
        public int JourneyVictories=>office.victories;
        public int RescuedWorkers=>office.rescued;
        public bool CompletionSeal=>office.seal;
        public int ChapterSparks=>chapterRun==null?0:ChapterEncounters.Take(chapterRun.cleared).Sum(e=>e.sparks);
        ChapterEncounter ChapterEncounter=>ChapterActive?ChapterEncounters[Mathf.Clamp(Wave-1,0,ChapterEncounters.Length-1)]:null;
        string OfficePath=>Path.Combine(SaveDirectory,"office.json");
        void LoadOffice()
        {
            office=new OfficeProfile();officeStorageReady=true;
            if(!File.Exists(OfficePath))return;
            foreach(string path in new[]{OfficePath,OfficePath+".bak"})
            {
                try
                {
                    var p=JsonUtility.FromJson<OfficeProfile>(File.ReadAllText(path));
                    if(p==null||p.schema!=1||p.sparks<0||p.rescued<0||p.victories<0||p.unlocks==null||p.receipts==null||p.unlocks.Distinct().Count()!=p.unlocks.Count||p.unlocks.Any(x=>!Constellation.Nodes.Any(n=>n.item==x))||p.receipts.Any(r=>!Guid.TryParse(r.id,out _)||r.cleared<0||r.cleared>21||r.rescued<0||r.rescued>27)||p.receipts.Select(r=>r.id).Distinct().Count()!=p.receipts.Count)throw new InvalidOperationException("Invalid Office profile");
                    office=p;if(path!=OfficePath)OfficeMessage="Recovered Office progress from backup.";return;
                }
                catch(Exception){ }
            }
            officeStorageReady=false;OfficeMessage="Office save could not be read. Files preserved; purchases and new chapters are disabled.";
        }
        bool SaveOffice(OfficeProfile next)
        {
            if(!officeStorageReady)return false;
            try{AtomicWrite(OfficePath,JsonUtility.ToJson(next));office=next;return true;}
            catch(Exception e){OfficeMessage="Could not save Office progress. Retry before leaving.";Debug.LogWarning("OFFICE_SAVE: "+e.Message);return false;}
        }
        OfficeProfile CopyOffice()=>JsonUtility.FromJson<OfficeProfile>(JsonUtility.ToJson(office));
        public bool HasConstellation(string item)=>office.unlocks.Contains(item);
        public string UnlockRequirement(ConstellationNode node)=>HasConstellation(node.item)?"UNLOCKED":!officeStorageReady?"SAVE UNAVAILABLE":node.parent!=null&&!HasConstellation(node.parent)?"NEEDS "+LoadoutCatalog.Find(node.parent).name.ToUpperInvariant():node.NeedsSeal&&!CompletionSeal?"DEFEAT THE FOREMAN":Sparks<node.cost?"NEED "+(node.cost-Sparks)+" MORE SPARKS":"UNLOCK · "+node.cost+" SPARKS";
        public bool BuyConstellation(string item)
        {
            var node=Constellation.Nodes.FirstOrDefault(n=>n.item==item);
            if(!OfficeOpen||node==null||HasConstellation(item)||!officeStorageReady||Sparks<node.cost||node.NeedsSeal&&(!CompletionSeal||!HasConstellation(node.parent)))return false;
            var next=CopyOffice();next.sparks-=node.cost;next.unlocks.Add(item);if(!SaveOffice(next))return false;
            OfficeMessage=LoadoutCatalog.Find(item).name+" unlocked for future chapter rewards. Your current saved journey keeps its original pool.";return true;
        }
        void ResetChapterRun()
        {
            OfficeOpen=false;if(loadingCheckpoint)return;
            chapterRun=ExpeditionActive&&chapterRequested?new ChapterRun{definition=legacyChapterReview?CinderworksChapter.Id:LongJourney.Id,id=Guid.NewGuid().ToString("N"),unlocks=new List<string>(office.unlocks)}:null;
        }
        bool ChapterRewardAllowed(ItemDefinition item)=>!ChapterActive||!Constellation.Nodes.Any(n=>n.item==item.id)||chapterRun.unlocks.Contains(item.id);
        void BindChapterRewards(){Deck.RewardFilter=ChapterRewardAllowed;}
        bool SyncChapterProgress()
        {
            if(!ChapterActive||qa||simulation)return true;
            var next=CopyOffice();var receipt=next.receipts.FirstOrDefault(r=>r.id==chapterRun.id);
            if(receipt==null){receipt=new ChapterReceipt{id=chapterRun.id};next.receipts.Add(receipt);}
            if(receipt.cleared>=chapterRun.cleared&&receipt.rescued>=chapterRun.rescued&&(!chapterRun.complete||receipt.complete))return true;
            for(int i=receipt.cleared;i<chapterRun.cleared;i++)next.sparks+=ChapterEncounters[i].sparks;
            next.rescued+=Math.Max(0,chapterRun.rescued-receipt.rescued);
            receipt.cleared=Math.Max(receipt.cleared,chapterRun.cleared);receipt.rescued=Math.Max(receipt.rescued,chapterRun.rescued);
            if(chapterRun.cleared>=(JourneyActive?7:6)){receipt.seal=true;next.seal=true;}
            if(chapterRun.complete&&!receipt.complete){receipt.complete=true;next.victories++;}
            return SaveOffice(next);
        }
        void ClearChapterEncounter()
        {
            if(!ChapterActive)return;chapterRun.cleared=Math.Max(chapterRun.cleared,Wave);chapterRun.complete=Wave>=WaveCount;SyncChapterProgress();
        }
        void RecordChapterRescue(){if(ChapterActive){chapterRun.rescued++;SyncChapterProgress();}}
        public void OpenOffice()
        {
            if(ChapterActive&&!SyncChapterProgress())return;
            if(ExpeditionActive&&State==RunState.Drafting&&!SaveCheckpoint())return;
            FinishExpeditionStorage();MainMenu();OfficeOpen=true;hud.FitOfficeChart();
        }
        public void CloseOffice(){OfficeOpen=false;}
        public bool StartChapter()
        {
            if(!officeStorageReady)return false;chapterRequested=true;BeginExpedition();return true;
        }
    }
}
