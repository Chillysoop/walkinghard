using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    // Saved identifiers select reusable encounters; art and rules stay in the catalog.
    [Serializable] public sealed class ExpeditionPlan
    {
        public bool legacy;
        public EncounterRoute[] waves,left,right,chosen=new EncounterRoute[2];
        public static ExpeditionPlan Create(int seed,bool legacy=false)
        {
            var random=new System.Random(seed^0x31b69);var crews=new[]{EncounterRoute.RepairLine,EncounterRoute.FreightYard,EncounterRoute.Reclaimers};
            var events=new[]{EncounterRoute.Auction,EncounterRoute.PressureWorks,EncounterRoute.Rescue,EncounterRoute.Overtime};
            var firstEvent=events[random.Next(events.Length)];
            var plan=new ExpeditionPlan{legacy=legacy,waves=new[]{EncounterRoute.Standard,EncounterRoute.RepairLine,EncounterRoute.FreightYard,EncounterRoute.Reclaimers,crews[random.Next(3)],EncounterRoute.Standard,crews[random.Next(3)],EncounterRoute.Standard},left=new EncounterRoute[2],right=new EncounterRoute[2]};
            plan.left[0]=new[]{EncounterRoute.ShieldLine,EncounterRoute.RepairLine,EncounterRoute.FreightYard}[random.Next(3)];plan.right[0]=firstEvent;
            var otherEvents=events.Where(e=>e!=firstEvent).ToArray();
            plan.left[1]=EncounterRoute.SalvageStop;plan.right[1]=new[]{EncounterRoute.EliteShift,EncounterRoute.Reclaimers,EncounterRoute.VolatileBreach,otherEvents[random.Next(otherEvents.Length)]}[random.Next(4)];
            for(int i=0;i<2;i++)if(random.Next(2)==0){var swap=plan.left[i];plan.left[i]=plan.right[i];plan.right[i]=swap;}
            return plan;
        }
        public bool Valid()=>waves!=null&&waves.Length==8&&left!=null&&right!=null&&left.Length==2&&right.Length==2&&chosen!=null&&chosen.Length==2&&waves.Concat(left).Concat(right).Concat(chosen).All(r=>Enum.IsDefined(typeof(EncounterRoute),r))&&left.Zip(right,(a,b)=>a!=b&&a!=EncounterRoute.Standard&&b!=EncounterRoute.Standard).All(v=>v);
    }
    public sealed class RegionalEncounter
    {
        public readonly EncounterRoute id;public readonly string name,label,benefit,description;public readonly int[] entries;public readonly CrewRole crew;public readonly int heavyEvery;public readonly RewardEmphasis reward;
        public RegionalEncounter(EncounterRoute id,string name,string label,string benefit,string description,int[] entries,CrewRole crew= CrewRole.None,int heavyEvery=0,RewardEmphasis reward=RewardEmphasis.None)
        {this.id=id;this.name=name;this.label=label;this.benefit=benefit;this.description=description;this.entries=entries;this.crew=crew;this.heavyEvery=heavyEvery;this.reward=reward;}
        public bool Combat=>entries!=null;
    }
    public static class RegionalEncounters
    {
        public static readonly RegionalEncounter[] All={
            new RegionalEncounter(EncounterRoute.ShieldLine,"SHIELD LINE","SHIELDS","MORE GEAR","Shield bearers advance from the horizon. Stagger opens their guard; attacks from behind bypass it.",new[]{2,2,2,0,1},CrewRole.None,3,RewardEmphasis.Equipment),
            new RegionalEncounter(EncounterRoute.VolatileBreach,"VOLATILE BREACH","VOLATILES","MORE ABILITIES","Carriers emerge from side doors. Launch their packs into a collision.",new[]{0,0,1,1,2},CrewRole.None,0,RewardEmphasis.Abilities),
            new RegionalEncounter(EncounterRoute.RepairLine,"REPAIR LINE","REPAIR CREW","MORE GEAR","Heavies screen repair workers. Interrupt the green links.",new[]{2,2,0,2,1,2},CrewRole.Repair,3,RewardEmphasis.Equipment),
            new RegionalEncounter(EncounterRoute.FreightYard,"FREIGHT YARD","FREIGHT CREW","MORE ABILITIES","Door packs supply bodies. Interrupt amber throws or launch the payload first.",new[]{0,0,2,1,1,2},CrewRole.Freight,0,RewardEmphasis.Abilities),
            new RegionalEncounter(EncounterRoute.Reclaimers,"RECOVERY DETAIL","RECLAIMERS","MORE GEAR","Reclaimers pull bodies behind the line. Break their purple tethers.",new[]{2,0,2,1,2,2},CrewRole.Reclaimer,0,RewardEmphasis.Equipment),
            new RegionalEncounter(EncounterRoute.EliteShift,"ELITE SHIFT","ELITES","+6 SALVAGE","More heavy guards and shields reinforce the Foreman. Clear the wave for +6 salvage.",new[]{2,2,0,2,1},CrewRole.Repair,2,RewardEmphasis.Equipment),
            new RegionalEncounter(EncounterRoute.Auction,"THE UNCLAIMED LOT","AUCTION","BID FOR GEAR","Pay a visible bid for a chance at the displayed item. Losing bids are spent.",null),
            new RegionalEncounter(EncounterRoute.PressureWorks,"THE PRESSURE DIVIDEND","PRESSURE","RISK & SALVAGE","Choose a safer tap or a dangerous override. Bank the haul before a rupture.",null),
            new RegionalEncounter(EncounterRoute.Rescue,"THE LAST ATTENDANCE","RESCUE","SAVE WORKERS","Spend tools or risk injury to free workers. Each success secures salvage and repair.",null)
        };
        public static RegionalEncounter Find(EncounterRoute id)=>All.FirstOrDefault(e=>e.id==id);
        public static bool IsNewEvent(EncounterRoute id)=>id==EncounterRoute.Auction||id==EncounterRoute.PressureWorks||id==EncounterRoute.Rescue;
        public static bool IsStop(EncounterRoute id)=>id==EncounterRoute.Overtime||id==EncounterRoute.SalvageStop||IsNewEvent(id);
    }
    public sealed partial class VectorEncounter
    {
        ExpeditionPlan expeditionPlan;
        // Retained only for reproducible comparisons and old fixture coverage.
        bool legacyExpeditionReview;
        bool VariedExpedition=>ExpeditionActive&&!CombatPreviewActive&&expeditionPlan!=null&&!expeditionPlan.legacy;
        EncounterRoute RegionalRoute(int wave)=>routedWave==wave&&RegionalEncounters.Find(SelectedRoute)?.Combat==true?SelectedRoute:expeditionPlan.waves[Mathf.Clamp(wave-1,0,7)];
        RegionalEncounter RegionalWave=>VariedExpedition?RegionalEncounters.Find(RegionalRoute(Wave)):null;
        EncounterRoute PlanFork(int side)
        {
            int wave=forkWave>0?forkWave:Wave,index=wave==5?1:0;
            if(VariedExpedition)return side==0?expeditionPlan.left[index]:expeditionPlan.right[index];
            return side==0?(ExpeditionActive&&wave==5?EncounterRoute.SalvageStop:EncounterRoute.ShieldLine):(ExpeditionActive&&wave==2?EncounterRoute.Overtime:EncounterRoute.VolatileBreach);
        }
        void RegionalArrival(CrewRole crew,ref bool heavy,ref bool shooter)
        {
            var encounter=RegionalWave;
            if(crew==CrewRole.None&&encounter?.heavyEvery>0&&Spawned%encounter.heavyEvery==0){heavy=true;shooter=false;}
        }
        string RegionalPreview(int wave)
        {
            if(wave==6)return "SHIFT FOREMAN · "+(routedWave==6&&!RegionalEncounters.IsStop(SelectedRoute)?RouteName(SelectedRoute)+" reinforcements":"stagger and impacts expose his vent");
            var definition=VariedExpedition?RegionalEncounters.Find(RegionalRoute(wave)):null;
            return definition==null?DescribeExpeditionFormation(wave):definition.name+" · "+definition.description;
        }
        EncounterRoute SimulationExpeditionRoute()
        {
            var options=new[]{ForkLeft,ForkRight};
            if((Seed/6)%3==0)return options.FirstOrDefault(r=>!RegionalEncounters.IsStop(r)) is EncounterRoute combat&&combat!=EncounterRoute.Standard?combat:ForkLeft;
            return options.FirstOrDefault(RegionalEncounters.IsStop) is EncounterRoute stop&&stop!=EncounterRoute.Standard?stop:ForkLeft;
        }
        void SimulationExpeditionStop()
        {
            if(!StopOpen)return;
            if(Stop.kind==EncounterRoute.SalvageStop)
            {BuyRepair();foreach(int i in Enumerable.Range(0,Stop.stock.Length).OrderByDescending(i=>Deck.Copies(Stop.stock[i].def,Stop.stock[i].rank)))if(BuyCopy(i))break;CashOutStop();return;}
            if(Stop.kind==EncounterRoute.Overtime){if(Stop.attempt==0&&Health>Stop.harm+MaxHealth*.15f)PushOvertime();CashOutStop();return;}
            if(Stop.reveal){if(Stop.kind==EncounterRoute.Rescue&&Stop.attempt<2)ContinueRegionalEvent();else CashOutStop();return;}
            int choice=(Seed/6)%3==2?1:0;
            if(CanResolveEvent(choice)&&Health>EventHarm(choice)+MaxHealth*.15f)ResolveRegionalEvent(choice);else CashOutStop();
        }
    }
}
