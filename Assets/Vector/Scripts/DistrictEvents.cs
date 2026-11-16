using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    [Serializable] public sealed class EventLot
    {
        public LoadoutItem input;
        public RewardOffer first,second;
        public int price,boost,chance;
    }
    [Serializable] public sealed class DistrictEventState
    {
        public int selected,prizeRoll,awardedRank;
        public List<EventLot> lots=new List<EventLot>();
        public LoadoutItem reward;
        public string receipt="";
    }
    public static class DistrictEvents
    {
        public static readonly EncounterRoute[][] Pools={
            new[]{EncounterRoute.PressureWorks,EncounterRoute.Rescue},
            new[]{EncounterRoute.Auction,EncounterRoute.Reclamation},
            new[]{EncounterRoute.Overtime,EncounterRoute.DeadLetter}};
        public static string Title(EncounterRoute r)=>r==EncounterRoute.Overtime?"THE UNFINISHED ORDERS":VectorEncounter.RouteName(r);
        public static string Story(EncounterRoute r)=>r==EncounterRoute.Reclamation?
            "The Foundry sorts people by the tools they carry. This press can give a discarded tool a new assignment. Its old owner is still stamped into the metal.":r==EncounterRoute.DeadLetter?
            "The Dispatch holds equipment addressed to workers erased from the register. The seal recognizes neither you nor the dead. You can sell the brass, or try to release its contents.":r==EncounterRoute.Overtime?
            "Three unfinished work orders reached Dispatch after their authors died. Complete one to reclaim the tool named on it, or scrap the paperwork. The machine charges the worker in wear, whether it accepts the work or not.":r==EncounterRoute.Auction?
            "The Foundry auctions confiscated tools to fund another shift. A familiar tool and an unfamiliar design share the counter. Every bidder pays; only one takes the lot.":r==EncounterRoute.Rescue?
            "Three workers remain pinned in the Cinderworks inspection line. Their lamps still say ON SHIFT. Buy the proper release tools, or risk forcing each lock. A freed worker stays free.":
            "The Cinderworks sells heat by the minute, even after evacuation. Bleed the failing valve for salvage, or override its governor for a larger haul. The growing pile is yours only when you bank it.";
    }
    public sealed partial class AbilityDeck
    {
        internal bool EventRewardAllowed(ItemDefinition def)=>UnlockedReward(def)&&def.kind!=ItemKind.Basic&&def.kind!=ItemKind.Augment;
        internal bool CanExchange(LoadoutItem item)=>item!=null&&item!=Fists&&item.augment==null&&item.def.kind!=ItemKind.Augment&&Inventory.Contains(item);
        internal bool ExchangeStored(LoadoutItem item){if(!CanExchange(item))return false;Items.Remove(item);return true;}
    }
    public sealed partial class VectorEncounter
    {
        bool originalEvents; // Isolated QA comparison only.
        internal bool UsesDistrictEvents=>JourneyActive&&!originalEvents;
        static void ValidateDistrictEvent(ExpeditionStop stop,AbilityDeck deck)
        {
            if(stop?.districtEvent==null){if(stop!=null&&(stop.kind==EncounterRoute.Reclamation||stop.kind==EncounterRoute.DeadLetter))throw new InvalidOperationException("Missing district event");return;}var e=stop.districtEvent;
            bool valid=DistrictEvents.Pools.SelectMany(p=>p).Contains(stop.kind)&&e.lots!=null&&e.lots.Count<=2000&&e.selected>=0&&e.selected<Mathf.Max(1,e.lots.Count)&&e.receipt!=null&&stop.attempt>=0&&stop.attempt<=(stop.kind==EncounterRoute.Rescue||stop.kind==EncounterRoute.PressureWorks?3:1)&&stop.stash>=0;
            bool Offer(RewardOffer o)=>o!=null&&o.def!=null&&o.def.kind!=ItemKind.Basic&&o.def.kind!=ItemKind.Augment&&o.rank>=1&&o.rank<=5&&o.copies==1;
            if(valid)foreach(var lot in e.lots)
            {
                valid&=lot!=null&&Offer(lot.first)&&lot.price>=0&&lot.boost>=0&&lot.chance>=0&&lot.chance<=100;
                if(!valid)break;
                if(stop.kind==EncounterRoute.Reclamation||stop.kind==EncounterRoute.DeadLetter)valid&=Offer(lot.second);
                if(stop.kind==EncounterRoute.Reclamation)valid&=lot.input!=null&&(stop.closed||lot.first.rank==lot.input.rank&&lot.second.rank==lot.input.rank)&&lot.first.def!=lot.second.def&&lot.first.def.kind==lot.second.def.kind&&(stop.closed||stop.reveal||deck.CanExchange(lot.input));
            }
            if(stop.kind!=EncounterRoute.PressureWorks&&stop.kind!=EncounterRoute.Rescue&&stop.kind!=EncounterRoute.Reclamation)valid&=e.lots?.Count>0;
            if(e.reward!=null)valid&=stop.reveal&&e.awardedRank>=1&&e.awardedRank<=5&&(stop.closed||deck.Items.Contains(e.reward));
            if(!valid)throw new InvalidOperationException("Invalid district event");
        }
        public bool DistrictEventOpen=>StopOpen&&Stop.districtEvent!=null;
        public EventLot CurrentEventLot=>Stop?.districtEvent is DistrictEventState s&&s.lots.Count>0?s.lots[Mathf.Clamp(s.selected,0,s.lots.Count-1)]:null;
        public int DistrictEventLimit=>Stop.kind==EncounterRoute.PressureWorks||Stop.kind==EncounterRoute.Rescue?3:1;
        ItemDefinition[] EventPool()=>LoadoutCatalog.All.Where(Deck.EventRewardAllowed).OrderBy(d=>d.id,StringComparer.Ordinal).ToArray();
        ItemDefinition EventPick(IEnumerable<ItemDefinition> pool){var a=pool.ToArray();return a[eventRandom.Next(a.Length)];}
        RewardOffer EventCopy()
        {
            var owned=Deck.Items.Where(i=>i!=Deck.Fists&&i.rank<5&&Deck.EventRewardAllowed(i.def)).GroupBy(i=>i.def.id).Select(g=>g.OrderByDescending(i=>i.rank).First()).OrderBy(i=>i.def.id,StringComparer.Ordinal).ToArray();
            if(owned.Length==0)return new RewardOffer(EventPick(EventPool().Where(d=>d.kind==ItemKind.Weapon)));
            var item=owned[eventRandom.Next(owned.Length)];return new RewardOffer(item.def,1,item.rank);
        }
        void PrepareDistrictEvent()
        {
            var state=Stop.districtEvent=new DistrictEventState();Stop.variant=eventRandom.Next(3);
            if(Stop.kind==EncounterRoute.PressureWorks)
            {Stop.stash=eventRandom.Next(2,5);Stop.chance=eventRandom.Next(72,87);Stop.gain=eventRandom.Next(4,8);Stop.harm=Mathf.CeilToInt(MaxHealth*(.025f+(float)eventRandom.NextDouble()*.025f));}
            else if(Stop.kind==EncounterRoute.Rescue)PrepareDistrictRescue();
            else if(Stop.kind==EncounterRoute.Auction)
            {
                var copy=EventCopy();var novel=EventPool().Where(d=>!Deck.Items.Any(i=>i.def==d)).ToArray();
                if(novel.Length==0)novel=EventPool().Where(d=>d!=copy.def).ToArray();
                foreach(var offer in new[]{copy,new RewardOffer(EventPick(novel),1,ItemRarity.Roll(eventRandom))})
                {
                    int cost=CopyCost(offer.rank),price=Mathf.Max(4,Mathf.RoundToInt(cost*.4f));
                    state.lots.Add(new EventLot{first=offer,price=price,boost=Mathf.Max(3,Mathf.RoundToInt(cost*.3f)),chance=eventRandom.Next(40,51)});
                }
            }
            else if(Stop.kind==EncounterRoute.Reclamation)
            {
                foreach(var input in Deck.Inventory.Where(Deck.CanExchange).ToArray())
                {
                    var groups=EventPool().Where(d=>d!=input.def).GroupBy(d=>d.kind).Where(g=>g.Count()>=2).ToArray();
                    if(groups.Length==0)continue;var group=groups[eventRandom.Next(groups.Length)].ToArray();var first=EventPick(group);var second=EventPick(group.Where(d=>d!=first));
                    state.lots.Add(new EventLot{input=input,first=new RewardOffer(first,1,input.rank),second=new RewardOffer(second,1,input.rank),price=1+input.rank,chance=100});
                }
            }
            else if(Stop.kind==EncounterRoute.DeadLetter)
            {
                var weapons=EventPool().Where(d=>d.kind==ItemKind.Weapon).ToArray();var ultimates=EventPool().Where(d=>d.kind==ItemKind.Ultimate).ToArray();
                var first=EventPick(weapons);var second=EventPick(ultimates.Length>0?ultimates:weapons.Where(d=>d!=first));
                state.lots.Add(new EventLot{first=new RewardOffer(first,1,2),second=new RewardOffer(second,1,3),chance=eventRandom.Next(52,69)});Stop.stash=eventRandom.Next(4,7);
            }
            else
            {state.lots.Add(new EventLot{first=EventCopy(),chance=eventRandom.Next(70,86)});Stop.stash=eventRandom.Next(3,6);Stop.harm=Mathf.CeilToInt(MaxHealth*(.03f+(float)eventRandom.NextDouble()*.02f+.02f*(state.lots[0].first.rank-1)));}
        }
        void PrepareDistrictRescue()
        {Stop.price=eventRandom.Next(4,7);Stop.gain=eventRandom.Next(2,5);Stop.repair=Mathf.CeilToInt(MaxHealth*(.05f+(float)eventRandom.NextDouble()*.03f));Stop.chance=eventRandom.Next(82,93);Stop.harm=Mathf.CeilToInt(MaxHealth*(.03f+(float)eventRandom.NextDouble()*.02f));}
        public bool SelectEventLot(int index)
        {if(!DistrictEventOpen||Stop.reveal||index<0||index>=Stop.districtEvent.lots.Count)return false;Stop.districtEvent.selected=index;SaveCheckpoint();return true;}
        public int DistrictEventChance(int option)=>Stop.kind==EncounterRoute.Auction?CurrentEventLot.chance+(option==1?25:0):Stop.kind==EncounterRoute.Reclamation?100:Stop.kind==EncounterRoute.Overtime||Stop.kind==EncounterRoute.DeadLetter?CurrentEventLot.chance:Mathf.Max(5,Stop.chance-(option==1?(Stop.kind==EncounterRoute.Rescue?30:20):0));
        public int DistrictEventPrice(int option)=>Stop.kind==EncounterRoute.Auction?CurrentEventLot.price+(option==1?CurrentEventLot.boost:0):Stop.kind==EncounterRoute.Reclamation?CurrentEventLot.price:Stop.kind==EncounterRoute.Rescue&&option==0?Stop.price:0;
        public int DistrictEventHarm(int option)=>Stop.kind==EncounterRoute.Overtime?Stop.harm:Stop.kind==EncounterRoute.PressureWorks?Stop.harm*(option==1?2:1):Stop.kind==EncounterRoute.Rescue&&option==1?Stop.harm:0;
        public bool CanResolveDistrictEvent(int option)
        {
            if(!DistrictEventOpen||Stop.reveal||Stop.attempt>=DistrictEventLimit||option<0||option>(Stop.kind==EncounterRoute.Auction||Stop.kind==EncounterRoute.PressureWorks||Stop.kind==EncounterRoute.Rescue?1:0))return false;
            if(Stop.kind!=EncounterRoute.Rescue&&Stop.kind!=EncounterRoute.PressureWorks&&CurrentEventLot==null)return false;
            return Salvage>=DistrictEventPrice(option)&&Health>DistrictEventHarm(option)&&(Stop.kind!=EncounterRoute.Reclamation||Deck.CanExchange(CurrentEventLot.input));
        }
        internal static string DistrictPrizeDestination(AbilityDeck deck,LoadoutItem item)=>deck.Inventory.Contains(item)?"STORAGE":item==deck.Fists&&!BuildSynergies.Equipped(deck).Contains(item)?"FALLBACK FISTS":"EQUIPPED ITEM";
        string ReceiveDistrictPrize(RewardOffer prize)
        {
            var state=Stop.districtEvent;var weapons=Deck.Weapons.ToArray();float max=MaxHealth;var item=Deck.Acquire(prize.def,prize.rank);Health=Mathf.Clamp(Health+MaxHealth-max,0,MaxHealth);
            state.reward=item;state.awardedRank=item.rank;
            foreach(var offer in Deck.Offers)offer.owned=Deck.Copies(offer.def,offer.rank);
            bool stored=Deck.Inventory.Contains(item);state.receipt=(item.rank>prize.rank?"FUSED → "+ItemRarity.Names[item.rank]+" ":ItemRarity.Names[item.rank]+" ")+item.def.name+"\n"+(stored?"Added to storage. Equip after leaving this event.":DistrictPrizeDestination(Deck,item)=="FALLBACK FISTS"?"Updated fallback fists; mounted weapons are unchanged.":"Updated the existing equipped item.")+(item.augment!=null?" Augment retained.":"");foreach(int hand in Enumerable.Range(0,2).Where(i=>weapons[i]!=null&&Deck.Weapons[i]==null))state.receipt+=" Hand "+(hand+1)+" returns to fists.";return state.receipt;
        }
        public bool ResolveDistrictEvent(int option)
        {
            if(!CanResolveDistrictEvent(option))return false;var s=Stop.districtEvent;var lot=CurrentEventLot;
            int price=DistrictEventPrice(option),harm=DistrictEventHarm(option),chance=DistrictEventChance(option);int wallet=Salvage;float health=Health;
            if(Stop.kind==EncounterRoute.Reclamation&&!Deck.ExchangeStored(lot.input))return false;
            Salvage-=price;Stop.roll=eventRandom.Next(1,101);Stop.attempt++;Stop.reveal=true;Stop.failed=Stop.roll>chance;
            if(Stop.kind==EncounterRoute.Overtime||Stop.failed&&harm>0){RecordIncoming(DistrictEvents.Title(Stop.kind),harm);Health-=harm;}
            string roll="Rolled "+Stop.roll+" / needed "+chance+" or lower.";
            if(Stop.kind==EncounterRoute.PressureWorks)
            {
                if(Stop.failed){Stop.result="RUPTURE · lost "+Stop.stash+" unbanked salvage and "+harm+" integrity.";Stop.stash=0;}
                else {int gain=Stop.gain*(option==1?2:1);Stop.stash+=gain;Stop.result="+"+gain+" salvage added. "+Stop.stash+" remains at risk.";}
            }
            else if(Stop.kind==EncounterRoute.Rescue)
            {
                if(Stop.failed)Stop.result="RESCUE FAILED · "+price+" salvage spent; "+harm+" integrity lost. Earlier rewards stay secured.";
                else {float repair=Mathf.Min(MaxHealth-Health,Stop.repair);Salvage+=Stop.gain;Health+=repair;Stop.rescued++;RecordChapterRescue();Stop.result="WORKER FREED · +"+Stop.gain+" salvage, +"+Mathf.RoundToInt(repair)+" integrity. Secured immediately.";}
            }
            else if(Stop.kind==EncounterRoute.Reclamation)
            {s.prizeRoll=eventRandom.Next(1,101);Stop.result="EXCHANGED "+lot.input.Name+" · paid "+price+" salvage.\n"+ReceiveDistrictPrize(s.prizeRoll<=50?lot.first:lot.second);roll="Replacement roll "+s.prizeRoll+" / first item on 1–50, second on 51–100.";}
            else if(Stop.kind==EncounterRoute.DeadLetter)
            {
                int risk=Stop.stash;Stop.stash=0;
                if(Stop.failed)Stop.result="SEAL REJECTED · lost the "+risk+" salvage haul. Banked salvage is safe.";
                else {s.prizeRoll=eventRandom.Next(1,101);Stop.result="CACHE OPENED · surrendered the "+risk+" salvage haul.\n"+ReceiveDistrictPrize(s.prizeRoll<=75?lot.first:lot.second);roll+=" Prize roll "+s.prizeRoll+" (1–75 / 76–100).";}
            }
            else {Stop.stash=0;Stop.result=(Stop.kind==EncounterRoute.Auction?price+" salvage spent. ":harm+" integrity spent. ")+(Stop.failed?"No item recovered.":ReceiveDistrictPrize(lot.first));}
            Stop.result+="\n"+roll;RecordEventAudit("resolve-"+option,wallet,health);SaveCheckpoint();return true;
        }
        public bool ContinueDistrictEvent()
        {
            if(!DistrictEventOpen||!Stop.reveal)return false;
            if(Stop.attempt>=DistrictEventLimit||Stop.kind!=EncounterRoute.Rescue&&Stop.failed)return CashOutDistrictEvent();
            Stop.reveal=false;Stop.failed=false;
            if(Stop.kind==EncounterRoute.Rescue)PrepareDistrictRescue();
            else {Stop.chance=Mathf.Max(25,Stop.chance-eventRandom.Next(12,19));Stop.gain+=eventRandom.Next(2,5);Stop.harm+=Mathf.CeilToInt(MaxHealth*.015f);}
            SaveCheckpoint();return true;
        }
        bool CashOutDistrictEvent()
        {
            if(!DistrictEventOpen)return false;int wallet=Salvage;float health=Health;
            Salvage+=Stop.stash;Stop.stash=0;Stop.closed=true;
            RecordOvertime(Stop.kind==EncounterRoute.Auction?3:Stop.kind==EncounterRoute.PressureWorks?4:Stop.kind==EncounterRoute.Rescue?5:Stop.kind==EncounterRoute.Reclamation?6:Stop.kind==EncounterRoute.DeadLetter?7:Stop.variant);
            RecordEventAudit("leave",wallet,health);SaveCheckpoint();return true;
        }
        [Serializable] sealed class EventAudit
        {public int wave,salvageBefore,salvageAfter,roll;public float healthBefore,healthAfter;public string kind,choice,result,prize;public int prizeRank;}
        readonly List<EventAudit> eventAudits=new List<EventAudit>();
        void RecordEventAudit(string choice,int wallet,float health)=>eventAudits.Add(new EventAudit{wave=Wave,kind=Stop.kind.ToString(),choice=choice,result=Stop.result,roll=Stop.roll,salvageBefore=wallet,salvageAfter=Salvage,healthBefore=health,healthAfter=Health,prize=Stop.districtEvent.reward?.def.id??"",prizeRank=Stop.districtEvent.awardedRank});
        void SimulationDistrictEvent()
        {
            if(Stop.reveal){if(Stop.attempt<2&&(Stop.kind==EncounterRoute.Rescue||Stop.kind==EncounterRoute.PressureWorks)&&!Stop.failed)ContinueDistrictEvent();else CashOutStop();return;}
            if(Stop.kind==EncounterRoute.Auction&&Stop.districtEvent.lots.Count>1)
            {int best=Enumerable.Range(0,Stop.districtEvent.lots.Count).OrderByDescending(i=>AutomatedLoadoutPolicy.Evaluate(Deck,Stop.districtEvent.lots[i].first,ProgressionPolicy).score).First();SelectEventLot(best);}
            if(Stop.kind==EncounterRoute.Reclamation)
            {
                var choices=Stop.districtEvent.lots;float baseline=AutomatedLoadoutPolicy.Score(Deck,ProgressionPolicy);int best=-1;float gain=0;
                for(int i=0;i<choices.Count;i++){var lot=choices[i];var removed=Deck.PreviewChange(null,lot.input,PlaceKind.Inventory);removed.after.ExchangeStored(removed.incoming);float value=(AutomatedLoadoutPolicy.Evaluate(removed.after,lot.first,ProgressionPolicy).score+AutomatedLoadoutPolicy.Evaluate(removed.after,lot.second,ProgressionPolicy).score)*.5f-baseline;if(value>gain){gain=value;best=i;}}
                if(best<0){CashOutStop();return;}SelectEventLot(best);
            }
            int option=(Seed/6)%3==2&&(Stop.kind==EncounterRoute.Auction||Stop.kind==EncounterRoute.Rescue||Stop.kind==EncounterRoute.PressureWorks)?1:0;
            if(CanResolveDistrictEvent(option)&&Health>DistrictEventHarm(option)+MaxHealth*.15f)ResolveDistrictEvent(option);else CashOutStop();
        }
    }
}
