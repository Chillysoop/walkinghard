using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        Color EventMuted=>muted.gamma;
        internal Action eventTextAudit;
        void ReviewEventText(){if(eventTextAudit==null)return;var audit=eventTextAudit;eventTextAudit=null;audit();}
        string EventRules()=>game.Stop.kind==EncounterRoute.PressureWorks?"Up to three taps. Failure loses the entire unbanked haul and ends the event. Banked salvage is safe.":game.Stop.kind==EncounterRoute.Rescue?"Three independent rescues. Rewards are secured immediately. Tools cost salvage even on failure. Forcing a lock risks injury.":game.Stop.kind==EncounterRoute.Auction?"Choose one lot, then place one bid. Your bid is spent on either result. Browsing does not change the odds or prizes.":game.Stop.kind==EncounterRoute.Reclamation?"Trade one stored item plus salvage for one of the two shown items. Same rarity; 50% each. Equipped items and items with augments cannot be traded.":game.Stop.kind==EncounterRoute.DeadLetter?"Take the salvage, or give it up to try opening the cache. The displayed prizes are alternatives. Banked salvage is safe.":"Take the salvage, or work for the displayed copy. Work costs integrity on either result. No salvage is charged from your wallet.";
        string EventExit()=>game.Stop.stash>0?"TAKE "+game.Stop.stash+" SALVAGE & LEAVE":"LEAVE & CHOOSE REWARDS";
        string EventChoiceName(int option)=>game.Stop.kind==EncounterRoute.Auction?(option==0?"SMALL BID":"STRONG BID"):game.Stop.kind==EncounterRoute.Rescue?(option==0?"USE TOOLS":"FORCE LOCK"):game.Stop.kind==EncounterRoute.PressureWorks?(option==0?"CONTROLLED TAP":"OVERRIDE"):game.Stop.kind==EncounterRoute.Reclamation?"EXCHANGE STORED ITEM":game.Stop.kind==EncounterRoute.DeadLetter?"BREAK THE SEAL":"COMPLETE THE ORDER";
        string EventChoiceTerms(int option)
        {
            var s=game.Stop;int cost=game.DistrictEventPrice(option),harm=game.DistrictEventHarm(option);
            if(s.kind==EncounterRoute.Reclamation)return "Spend "+cost+" salvage and the selected stored item.\nReceive one of the two displayed replacements. No takebacks.";
            if(s.kind==EncounterRoute.DeadLetter)return "Give up the "+s.stash+" salvage haul on either result.\nSuccess: one displayed prize. Failure: nothing. Banked salvage is safe.";
            if(s.kind==EncounterRoute.Overtime)return "Spend "+harm+" integrity and give up the "+s.stash+" salvage alternative.\nSuccess: displayed copy. Failure: no item. The integrity is spent either way.";
            string pay=cost>0?"Pay "+cost+" salvage either way. ":"No salvage cost. ";
            return pay+(s.kind==EncounterRoute.Auction?"Success: displayed item.\nFailure: no item.":s.kind==EncounterRoute.Rescue?"Success: +"+s.gain+" salvage; repair up to "+s.repair+".\nFailure: "+harm+" integrity lost. Earlier rewards are safe.":"Success: +"+(s.gain*(option==1?2:1))+" unbanked salvage.\nFailure: lose the "+s.stash+" haul and "+harm+" integrity.");
        }
        string EventBlocked(int option)=>game.Salvage<game.DistrictEventPrice(option)?"NEED MORE SALVAGE":game.Health<=game.DistrictEventHarm(option)?"TOO LITTLE INTEGRITY":"ITEM NO LONGER AVAILABLE";
        internal Rect DistrictEventOptionRect(int option,float w,float h)=>new Rect((w-1120)/2+370,(h-640)/2+326+option*110,724,98);
        internal bool HandleDistrictEventPointer(EventType type,Vector2 point,float w,float h)
        {
            if(type!=EventType.MouseDown||!game.DistrictEventOpen||game.Stop.reveal)return false;
            int count=game.Stop.kind==EncounterRoute.Auction||game.Stop.kind==EncounterRoute.PressureWorks||game.Stop.kind==EncounterRoute.Rescue?2:1;
            for(int i=0;i<count;i++)if(DistrictEventOptionRect(i,w,h).Contains(point))return game.ResolveDistrictEvent(i);
            return false;
        }
        internal string EventRewardDestination(RewardOffer offer,out int rank)
        {
            var p=game.Deck.PreviewChange(offer,null,PlaceKind.Inventory);rank=p.fusionRanks.Count>0?p.fusionRanks.Last():offer.rank;
            var empty=Enumerable.Range(0,2).Where(i=>game.Deck.Weapons[i]!=null&&p.after.Weapons[i]==null).ToArray();
            return (rank>offer.rank?"FUSES → "+ItemRarity.Names[rank].ToUpperInvariant()+" · ":"RECEIVE · ")+(empty.Length>0?"HAND "+string.Join("+",empty.Select(i=>i+1))+" → FISTS":VectorEncounter.DistrictPrizeDestination(p.after,p.incoming));
        }
        void EventItemCard(Rect r,RewardOffer offer,string tag,bool resolved=false)
        {
            int rank=offer.rank;string destination=resolved?"RECEIVED · "+VectorEncounter.DistrictPrizeDestination(game.Deck,game.Stop.districtEvent.reward):EventRewardDestination(offer,out rank);
            Color edge=RarityColor(rank).gamma;Plate(r,new Color(.045f,.085f,.11f,.98f),edge,7,2);
            if(rank>offer.rank||resolved&&game.Stop.districtEvent.receipt.StartsWith("FUSED"))
            {float pulse=game.ReducedMotion?1:.8f+.2f*Mathf.Sin(Time.unscaledTime*3);Outline(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),edge*new Color(1,1,1,pulse),7,2);}
            Label(new Rect(r.x+12,r.y+8,r.width-24,17),tag+" · "+ItemRarity.Names[offer.rank].ToUpperInvariant(),11,RarityColor(offer.rank).gamma,true);
            CategoryArt(new Rect(r.x+13,r.y+35,47,47),offer.def);
            Label(new Rect(r.x+72,r.y+32,r.width-84,42),offer.def.name,17,white,true);
            Label(new Rect(r.x+72,r.y+76,r.width-84,17),ItemRole(offer.def),10,EventMuted,true);
            Label(new Rect(r.x+12,r.y+98,r.width-24,36),(rank>offer.rank?"After fusion: ":"")+ItemSummary(new LoadoutItem(offer.def,rank)),12,white);
            Label(new Rect(r.x+12,r.yMax-23,r.width-24,18),destination,10,edge,true);
        }
        void DistrictEventScreen(float w,float h)
        {
            var s=game.Stop;var e=s.districtEvent;var lot=game.CurrentEventLot;float x=(w-1120)/2,y=(h-640)/2,right=x+370,width=724;
            Box(new Rect(0,0,w,h),new Color(0,0,0,.68f));Panel(new Rect(x,y,1120,640));
            Label(new Rect(x+26,y+24,800,32),DistrictEvents.Title(s.kind),24,Accent,true);
            Label(new Rect(x+26,y+71,800,24),"BANKED SALVAGE  "+game.Salvage+"     ·     INTEGRITY  "+Mathf.CeilToInt(game.Health)+" / "+Mathf.RoundToInt(game.MaxHealth),15,white,true);
            if(Button(new Rect(x+855,y+22,239,36),"SAVE & OFFICE"))game.SuspendExpedition();
            Stroke(new Vector2(x+26,y+108),new Vector2(x+1094,y+108),line,1);
            Label(new Rect(x+26,y+133,305,205),DistrictEvents.Story(s.kind),17,white);
            Label(new Rect(x+26,y+357,305,158),EventRules(),15,EventMuted);
            Label(new Rect(x+26,y+564,305,54),"Travel and combat are paused.\n"+game.SaveMessage,12,EventMuted);
            bool two=s.kind==EncounterRoute.Reclamation||s.kind==EncounterRoute.DeadLetter;
            if(!s.reveal&&(s.kind==EncounterRoute.Auction||s.kind==EncounterRoute.Reclamation)&&e.lots.Count>0)
            {
                if(ExpeditionButton(new Rect(right,y+122,44,34),"←",e.selected>0))game.SelectEventLot(e.selected-1);
                if(ExpeditionButton(new Rect(right+width-44,y+122,44,34),"→",e.selected<e.lots.Count-1))game.SelectEventLot(e.selected+1);
                Label(new Rect(right+54,y+123,width-108,32),(s.kind==EncounterRoute.Auction?"LOT ":"STORED ITEM ")+(e.selected+1)+" / "+e.lots.Count+(s.kind==EncounterRoute.Reclamation?" · SURRENDER "+lot.input.Name:" · "+(e.selected==0?"MATCHING COPY":"DISCOVERY")),12,white,true,TextAnchor.MiddleCenter);
            }
            else if(s.kind==EncounterRoute.DeadLetter&&!s.reveal)
                Label(new Rect(right,y+128,width,25),"IF THE SEAL OPENS: 75% LEFT PRIZE / 25% RIGHT PRIZE",13,Accent,true);
            if(s.reveal&&e.reward!=null)EventItemCard(new Rect(right,y+163,width,150),new RewardOffer(e.reward.def,1,e.awardedRank),e.receipt.StartsWith("FUSED")?"RARITY UPGRADE":"ITEM RECOVERED",true);
            else if(lot!=null)
            {
                EventItemCard(new Rect(right,y+163,two?354:width,150),lot.first,two?(s.kind==EncounterRoute.Reclamation?"50% REPLACEMENT":"75% WHEN OPENED"):"POSSIBLE REWARD");
                if(two)EventItemCard(new Rect(right+370,y+163,354,150),lot.second,s.kind==EncounterRoute.Reclamation?"50% REPLACEMENT":"25% WHEN OPENED");
            }
            else
            {
                Label(new Rect(right,y+147,width,35),s.kind==EncounterRoute.Reclamation?"NO ELIGIBLE STORED ITEMS":s.kind==EncounterRoute.Rescue?"WORKERS FREED  "+s.rescued+" / 3":"UNBANKED HAUL  "+s.stash+" SALVAGE",25,Accent,true);
                Label(new Rect(right,y+204,width,65),s.kind==EncounterRoute.Reclamation?"Only stored items without augments can be exchanged.\nNothing is consumed. You can leave freely.":"ATTEMPTS  "+s.attempt+" / "+game.DistrictEventLimit,16,white);
                if(s.kind!=EncounterRoute.Reclamation)Segments(new Rect(right,y+293,width,8),(float)s.attempt/game.DistrictEventLimit,Accent,game.DistrictEventLimit);
            }
            if(s.reveal)
            {
                Label(new Rect(right,y+330,width,172),s.result,18,s.failed?new Color(1,.66f,.42f):white,true);
                bool more=s.attempt<game.DistrictEventLimit&&(!s.failed||s.kind==EncounterRoute.Rescue);
                if(more&&Button(new Rect(right,y+510,width,42),"INSPECT NEXT CHOICE"))game.ContinueDistrictEvent();
                if(Button(new Rect(right,y+565,width,48),EventExit(),true))game.CashOutStop();return;
            }
            if(lot!=null||s.kind==EncounterRoute.Rescue||s.kind==EncounterRoute.PressureWorks)
            {
                if(Event.current.button==0&&HandleDistrictEventPointer(Event.current.type,Pointer,w,h))Event.current.Use();
                int count=s.kind==EncounterRoute.Auction||s.kind==EncounterRoute.Rescue||s.kind==EncounterRoute.PressureWorks?2:1;
                for(int i=0;i<count;i++)
                {
                    var r=DistrictEventOptionRect(i,w,h);bool available=game.CanResolveDistrictEvent(i);
                    Plate(r,available&&r.Contains(Pointer)?new Color(.07f,.18f,.21f,.98f):ink,available?Accent:line,6);
                    Label(new Rect(r.x+14,r.y+9,r.width-28,25),EventChoiceName(i)+" · "+(s.kind==EncounterRoute.Reclamation?"50 / 50":game.DistrictEventChance(i)+"% SUCCESS"),18,available?white:EventMuted,true);
                    Label(new Rect(r.x+14,r.y+39,r.width-28,44),EventChoiceTerms(i),14,available?white:EventMuted);
                    if(!available)Label(new Rect(r.x+14,r.y+80,r.width-28,15),EventBlocked(i),10,new Color(1,.66f,.42f),true);
                }
                if(s.kind==EncounterRoute.DeadLetter)Label(new Rect(right,y+449,width,58),"OVERALL ODDS  ·  "+(game.DistrictEventChance(0)*.75f).ToString("0.##")+"% left item  /  "+(game.DistrictEventChance(0)*.25f).ToString("0.##")+"% right item  /  "+(100-game.DistrictEventChance(0))+"% nothing",14,EventMuted);
            }
            if(Button(new Rect(right,y+565,width,48),EventExit(),true))game.CashOutStop();
        }
        internal bool DistrictEventTextFits()
        {
            bool fits=Style(17).CalcHeight(new GUIContent(DistrictEvents.Story(game.Stop.kind)),305)<=205&&Style(15).CalcHeight(new GUIContent(EventRules()),305)<=158;
            if(game.Stop.reveal)return fits&&Style(18,true).CalcHeight(new GUIContent(game.Stop.result),724)<=172;
            if(game.CurrentEventLot==null&&game.Stop.kind==EncounterRoute.Reclamation)return fits;
            int count=game.Stop.kind==EncounterRoute.Auction||game.Stop.kind==EncounterRoute.Rescue||game.Stop.kind==EncounterRoute.PressureWorks?2:1;
            for(int i=0;i<count;i++)fits&=Style(14).CalcHeight(new GUIContent(EventChoiceTerms(i)),696)<=44;
            foreach(var lot in game.Stop.districtEvent.lots)foreach(var o in new[]{lot.first,lot.second})if(o!=null)
                {EventRewardDestination(o,out int rank);fits&=Style(17,true).CalcHeight(new GUIContent(o.def.name),270)<=42&&Style(12).CalcHeight(new GUIContent((rank>o.rank?"After fusion: ":"")+ItemSummary(new LoadoutItem(o.def,rank))),330)<=36;}
            return fits;
        }
    }
    public sealed partial class VectorEncounter
    {
        void DistrictStopProp()
        {
            if(Stop.kind==EncounterRoute.Reclamation)
            {
                foreach(int side in new[]{-1,1})VisualFactory.Part("Reassignment piston",stopProp,PrimitiveType.Cylinder,new Vector3(side*1.5f,2,0),new Vector3(.4f,1.6f,.4f),CinderworksWorld.Enamel);
                VisualFactory.Part("Reclamation press",stopProp,PrimitiveType.Cube,new Vector3(0,3.7f,0),new Vector3(3.8f,.6f,2),CinderworksWorld.Ceramic);
                VisualFactory.Part("Exchange signal",stopProp,PrimitiveType.Cube,new Vector3(0,3.7f,-1.05f),new Vector3(1.6f,.15f,.1f),VisualFactory.Glow);
            }
            else
            {
                VisualFactory.Part("Dead letter cabinet",stopProp,PrimitiveType.Cube,new Vector3(0,2,0),new Vector3(3.2f,3,1.4f),CinderworksWorld.Ceramic);
                for(int i=0;i<4;i++)VisualFactory.Part("Sealed dispatch drawer",stopProp,PrimitiveType.Cube,new Vector3(0,1+i*.7f,-.76f),new Vector3(2.8f,.5f,.15f),CinderworksWorld.Iron);
                VisualFactory.Part("Undelivered seal",stopProp,PrimitiveType.Cube,new Vector3(0,2,-.87f),new Vector3(.2f,2.5f,.1f),VisualFactory.GoldGlow);
            }
        }
    }
}
