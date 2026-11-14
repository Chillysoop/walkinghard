using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void PrepareRegionalEvent()
        {
            Stop.variant=eventRandom.Next(3);
            if(Stop.kind==EncounterRoute.Auction)
            {
                var owned=Deck.Items.Where(i=>i!=Deck.Fists&&i.def.kind!=ItemKind.Basic&&i.def.kind!=ItemKind.Augment&&i.rank<5).GroupBy(i=>i.def.id).Select(g=>g.OrderByDescending(i=>i.rank).First()).OrderBy(i=>i.def.id,StringComparer.Ordinal).ToArray();
                var item=owned.Length>0?owned[eventRandom.Next(owned.Length)]:new LoadoutItem(LoadoutCatalog.Find("sword"),1);
                Stop.stock=new[]{new RewardOffer(item.def,1,item.rank)};Stop.price=eventRandom.Next(6,11);Stop.gain=eventRandom.Next(4,7);Stop.chance=eventRandom.Next(35,51);
            }
            else if(Stop.kind==EncounterRoute.PressureWorks)
            {Stop.stash=eventRandom.Next(4,9);Stop.chance=eventRandom.Next(72,87);Stop.gain=eventRandom.Next(8,15);Stop.harm=Mathf.CeilToInt(MaxHealth*(.025f+(float)eventRandom.NextDouble()*.025f));}
            else PrepareRescueAttempt();
        }
        void PrepareRescueAttempt()
        {Stop.price=eventRandom.Next(5,9);Stop.gain=eventRandom.Next(4,8);Stop.repair=Mathf.CeilToInt(MaxHealth*(.035f+(float)eventRandom.NextDouble()*.025f));Stop.chance=eventRandom.Next(76,92);Stop.harm=Mathf.CeilToInt(MaxHealth*(.04f+(float)eventRandom.NextDouble()*.025f));}
        public int RegionalEventLimit=>Stop?.kind==EncounterRoute.Auction?1:Stop?.kind==EncounterRoute.Rescue?3:4;
        public int EventChance(int option)=>Mathf.Clamp(Stop.chance+(Stop.kind==EncounterRoute.Auction?(option==1?25:0):option==1?(Stop.kind==EncounterRoute.Rescue?-30:-20):0),5,95);
        public int EventPrice(int option)=>Stop.kind==EncounterRoute.Auction?Stop.price+(option==1?Stop.gain:0):Stop.kind==EncounterRoute.Rescue&&option==0?Stop.price:0;
        public int EventHarm(int option)=>Stop.kind==EncounterRoute.Rescue?(option==0?0:Stop.harm):Stop.kind==EncounterRoute.PressureWorks?Stop.harm*(option==1?2:1):0;
        public int EventGain(int option)=>Stop.kind==EncounterRoute.PressureWorks?Stop.gain*(option==1?2:1):Stop.gain;
        public bool CanResolveEvent(int option)=>option>=0&&option<2&&StopOpen&&RegionalEncounters.IsNewEvent(Stop.kind)&&!Stop.reveal&&Stop.attempt<RegionalEventLimit&&Salvage>=EventPrice(option)&&Health>EventHarm(option);
        public bool ResolveRegionalEvent(int option)
        {
            if(!CanResolveEvent(option))return false;
            int chance=EventChance(option),price=EventPrice(option),harm=EventHarm(option),gain=EventGain(option);Salvage-=price;
            Stop.roll=eventRandom.Next(1,101);Stop.attempt++;Stop.reveal=true;Stop.failed=Stop.roll>chance;
            string roll="Rolled "+Stop.roll+" / needed "+chance+" or lower.";
            if(Stop.kind==EncounterRoute.Auction)
            {
                Stop.result=Stop.failed?"OUTBID · "+price+" salvage spent.\nThe item goes to another bidder.":"LOT WON · "+price+" salvage spent.\n"+GrantEventItem(Stop.stock[0]);
            }
            else if(Stop.kind==EncounterRoute.PressureWorks)
            {
                if(Stop.failed){Health-=harm;Stop.result="RUPTURE · lost the "+Stop.stash+" salvage haul\nand "+harm+" integrity. Banked salvage is safe.";Stop.stash=0;}
                else {Stop.stash+=gain;Stop.result="TAP SUCCESSFUL · +"+gain+" salvage.\nThe "+Stop.stash+" haul is still at risk.";}
            }
            else if(Stop.failed)
            {Health-=harm;Stop.result="RESCUE FAILED · "+(price>0?price+" salvage spent.":harm+" integrity lost.")+"\nEarlier rescues and rewards are secure.";}
            else
            {
                float repair=Mathf.Min(MaxHealth-Health,Stop.repair);Health+=repair;Salvage+=gain;Stop.rescued++;
                Stop.result="WORKER FREED · +"+gain+" salvage, +"+Mathf.RoundToInt(repair)+" integrity.\n"+(price>0?price+" salvage spent on tools. ":"")+"Rewards secured immediately.";
            }
            Stop.result+="\n"+roll;SaveCheckpoint();return true;
        }
        string GrantEventItem(RewardOffer offer)
        {
            float before=MaxHealth;var item=Deck.Acquire(offer.def,offer.rank);Health=Mathf.Clamp(Health+MaxHealth-before,0,MaxHealth);
            foreach(var reward in Deck.Offers)reward.owned=Deck.Copies(reward.def,reward.rank);
            return item.rank>offer.rank?"FUSED: "+item.def.name+" → "+ItemRarity.Names[item.rank]+(item.augment!=null?". Installed augment retained.":"."):ItemRarity.Names[item.rank]+" "+item.def.name+" added to storage.";
        }
        public bool ContinueRegionalEvent()
        {
            if(!StopOpen||!RegionalEncounters.IsNewEvent(Stop.kind)||!Stop.reveal)return false;
            if(Stop.attempt>=RegionalEventLimit||Stop.kind==EncounterRoute.PressureWorks&&Stop.failed)return CashOutStop();
            Stop.reveal=false;Stop.failed=false;
            if(Stop.kind==EncounterRoute.Rescue)PrepareRescueAttempt();
            else {Stop.chance=Mathf.Max(25,Stop.chance-eventRandom.Next(8,15));Stop.gain+=eventRandom.Next(3,8);Stop.harm+=Mathf.CeilToInt(MaxHealth*.015f);}
            SaveCheckpoint();return true;
        }
        public string RegionalEventStory
        {
            get
            {
                string[] stories=Stop.kind==EncounterRoute.Auction?new[]{
                    "A payroll clerk sells a tool whose owner is still working downstairs. Every bidder pays the processing fee. The winning receipt calls this voluntary surrender.",
                    "Iven recognizes the mark on a confiscated tool. The Registry calls it unclaimed property. Its previous owner has been forbidden to leave work long enough to claim it.",
                    "Three sealed bids circle an empty desk. The auctioneer died last winter; his recorded voice still raises money for his own replacement."}:
                    Stop.kind==EncounterRoute.PressureWorks?new[]{
                    "The furnace pays out brass maintenance tokens whenever its safety valve opens. Someone has chained the valve to a shift clock. The next release could tear the line apart.",
                    "A neighborhood's heat flows through this failing regulator. Bleeding pressure yields reusable parts. Forcing a release yields more, while the warning needle climbs.",
                    "The gauge has two scales: pressure and unpaid hours. Both are beyond their markings. Each tap releases a little metal and a voice trying to finish its last instruction."}:
                    new[]{"Three workers remain pinned in an inspection press. Their attendance lamps still show ON SHIFT. Each release mechanism needs a different amount of care.","A recovery trolley is taking conscious workers back to the furnace. Cut their restraints with tools, or force the locks before the trolley restarts.","The evacuation order is complete. Three names were copied onto the wrong ledger and left behind. You can free the people even if the factory refuses to correct its records."};
                return stories[Mathf.Clamp(Stop.variant,0,2)];
            }
        }
    }
}
