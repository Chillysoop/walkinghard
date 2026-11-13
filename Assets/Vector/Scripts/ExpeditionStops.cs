using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    [Serializable] public sealed class ExpeditionStop
    {
        public EncounterRoute kind;
        public int variant,stash,attempt,chance,gain,harm,roll;
        public bool reveal,failed,closed,repaired,boughtCopy;
        public string result="";
        public RewardOffer[] stock=Array.Empty<RewardOffer>();
    }
    public sealed partial class VectorEncounter
    {
        public bool ExpeditionActive {get;private set;}
        public int Salvage {get;private set;}
        public ExpeditionStop Stop {get;private set;}
        public bool StopOpen=>ExpeditionActive&&State==RunState.Drafting&&Stop!=null&&!Stop.closed;
        CheckpointRandom eventRandom;
        int salvageWave;
        Transform stopProp;
        public EncounterRoute ForkLeft=>ExpeditionActive&&(forkWave>0?forkWave:Wave)==5?EncounterRoute.SalvageStop:EncounterRoute.ShieldLine;
        public EncounterRoute ForkRight=>ExpeditionActive&&(forkWave>0?forkWave:Wave)==2?EncounterRoute.Overtime:EncounterRoute.VolatileBreach;
        public static string ForkLabel(EncounterRoute route)=>route==EncounterRoute.SalvageStop?"SALVAGE":route==EncounterRoute.Overtime?"OVERTIME":route==EncounterRoute.ShieldLine?"SHIELDS":"VOLATILES";
        public static string ForkBenefit(EncounterRoute route)=>route==EncounterRoute.SalvageStop?"HEAL OR COPY":route==EncounterRoute.Overtime?"RISK & SALVAGE":route==EncounterRoute.ShieldLine?"MORE GEAR":"MORE ABILITIES";
        public void BeginExpedition(){ExpeditionActive=true;Begin();}
        void ResetExpeditionRun()
        {
            Stop=null;Salvage=ExpeditionActive?8:0;salvageWave=0;eventRandom=new CheckpointRandom(Seed^0x1247a31);
            if(stopProp!=null){stopProp.gameObject.SetActive(false);Destroy(stopProp.gameObject);stopProp=null;}
        }
        void OpenExpeditionStop()
        {
            if(!ExpeditionActive)return;
            if(Wave>salvageWave){Salvage+=4;salvageWave=Wave;}
            if(routedWave!=Wave+1||SelectedRoute!=EncounterRoute.SalvageStop&&SelectedRoute!=EncounterRoute.Overtime)return;
            Stop=new ExpeditionStop{kind=SelectedRoute};
            if(Stop.kind==EncounterRoute.Overtime)
            {
                Stop.variant=eventRandom.Next(3);Stop.stash=eventRandom.Next(6,13);Stop.chance=eventRandom.Next(74,89);
                Stop.gain=eventRandom.Next(10,19);Stop.harm=Mathf.CeilToInt(MaxHealth*(.03f+(float)eventRandom.NextDouble()*.025f));
            }
            else
            {
                Stop.stock=Deck.Items.Where(i=>i!=Deck.Fists&&i.def.kind!=ItemKind.Basic&&i.def.kind!=ItemKind.Augment&&i.rank<5)
                    .OrderBy(i=>Deck.Inventory.Contains(i)?1:0).ThenBy(i=>i.def.id,StringComparer.Ordinal).GroupBy(i=>i.def.id).Select(g=>g.OrderByDescending(i=>i.rank).First()).Take(3).Select(i=>new RewardOffer(i.def,1,i.rank)).ToArray();
            }
            ShowStopProp();
        }
        void ShowStopProp()
        {
            if(stopProp!=null){stopProp.gameObject.SetActive(false);Destroy(stopProp.gameObject);}
            if(Stop==null)return;
            stopProp=VisualFactory.Node(Stop.kind==EncounterRoute.SalvageStop?"Iven's salvage bench":"Overtime lockers",transform,HeroPosition+RoadVector(new Vector3(4,0,5)));stopProp.rotation=RoadRotation;
            VisualFactory.Part("Station plinth",stopProp,PrimitiveType.Cube,new Vector3(0,.3f,0),new Vector3(3,.6f,2),VisualFactory.Stone);
            for(int i=0;i<3;i++)VisualFactory.Part("Recovery locker",stopProp,PrimitiveType.Cube,new Vector3(-1+i,1.15f,0),new Vector3(.8f,1.1f,1.2f),VisualFactory.Trim);
            VisualFactory.Part("Station signal",stopProp,PrimitiveType.Cube,new Vector3(0,2,0),new Vector3(2.5f,.14f,.2f),Stop.kind==EncounterRoute.SalvageStop?VisualFactory.Glow:VisualFactory.GoldGlow);
        }
        public int RepairAmount=>Mathf.CeilToInt(MaxHealth*.25f);
        public static int CopyCost(int rarity)=>new[]{0,12,18,26,36,99}[Mathf.Clamp(rarity,0,5)];
        public bool BuyRepair()
        {
            if(!StopOpen||Stop.kind!=EncounterRoute.SalvageStop||Stop.repaired||Salvage<12||Health>=MaxHealth)return false;
            float amount=Mathf.Min(RepairAmount,MaxHealth-Health);Salvage-=12;Health+=amount;Stop.repaired=true;Stop.result+=(Stop.result.Length>0?"\n":"")+"Recovered "+Mathf.RoundToInt(amount)+" integrity.";SaveCheckpoint();return true;
        }
        public bool BuyCopy(int index)
        {
            if(!StopOpen||Stop.kind!=EncounterRoute.SalvageStop||Stop.boughtCopy||index<0||index>=Stop.stock.Length)return false;
            var offer=Stop.stock[index];int cost=CopyCost(offer.rank);if(Salvage<cost)return false;
            float before=MaxHealth;Salvage-=cost;var acquired=Deck.Acquire(offer.def,offer.rank);Health=Mathf.Clamp(Health+MaxHealth-before,0,MaxHealth);Stop.boughtCopy=true;
            Stop.result+=(Stop.result.Length>0?"\n":"")+(acquired.rank>offer.rank?"FUSED: "+offer.def.name+" · "+ItemRarity.Names[offer.rank]+" → "+ItemRarity.Names[acquired.rank]+(acquired.augment!=null?". Augment retained.":"."):"Recovered a "+ItemRarity.Names[offer.rank]+" "+offer.def.name+" copy.");
            foreach(var reward in Deck.Offers)reward.owned=Deck.Copies(reward.def,reward.rank);SaveCheckpoint();return true;
        }
        public bool PushOvertime()
        {
            if(!StopOpen||Stop.kind!=EncounterRoute.Overtime||Stop.reveal||Stop.failed||Stop.attempt>=4||Health<=Stop.harm)return false;
            Stop.roll=eventRandom.Next(1,101);Stop.attempt++;Stop.reveal=true;
            if(Stop.roll<=Stop.chance){Stop.stash+=Stop.gain;Stop.result="SUCCESS · rolled "+Stop.roll+" / needed "+Stop.chance+" or lower\nRecovered +"+Stop.gain+" salvage. Your haul is still at risk.";}
            else {Health-=Stop.harm;Stop.failed=true;Stop.result="LOCKERS COLLAPSED · rolled "+Stop.roll+" / needed "+Stop.chance+" or lower\nLost the "+Stop.stash+" salvage haul and "+Stop.harm+" integrity. Banked salvage is safe.";Stop.stash=0;}
            SaveCheckpoint();return true;
        }
        public bool ContinueOvertime()
        {
            if(!StopOpen||Stop.kind!=EncounterRoute.Overtime||!Stop.reveal)return false;
            if(Stop.failed||Stop.attempt>=4)return CashOutStop();
            Stop.reveal=false;Stop.chance=Mathf.Max(25,Stop.chance-eventRandom.Next(9,16));Stop.gain+=eventRandom.Next(4,10);
            Stop.harm+=Mathf.CeilToInt(MaxHealth*(.015f+(float)eventRandom.NextDouble()*.01f));SaveCheckpoint();return true;
        }
        public bool CashOutStop()
        {
            if(!StopOpen)return false;
            if(Stop.kind==EncounterRoute.Overtime){Salvage+=Stop.stash;Stop.stash=0;RecordOvertime(Stop.variant);}
            Stop.closed=true;SaveCheckpoint();return true;
        }
        public string OvertimeTitle=>new[]{"THE UNFINISHED SHIFT","WAGES FOR THE DEAD","THE RESTORATION QUOTA"}[Stop?.variant??0];
        public string OvertimeStory=>new[]{
            "A furnace crew clocked out years ago. The Continuance still assigns their overtime. Their sealed lockers grind open one at a time; each release strains the failing pressure line.",
            "Payroll cylinders keep arriving for workers who no longer have names. A damaged conveyor offers up their wages. Every cleared obstruction makes its next movement less predictable.",
            "A restoration bay has met its quota without receiving a single patient. Recovery tools lie behind jammed shutters. The pressure gauge climbs whenever you force another shutter open."
        }[Stop?.variant??0];
    }
}
