using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void RegionalStopProp()
        {
            var ceramic=CinderworksWorld.Ceramic;var iron=CinderworksWorld.Iron;
            if(Stop.kind==EncounterRoute.Auction)
            {
                VisualFactory.Part("Auction counter",stopProp,PrimitiveType.Cube,new Vector3(0,1.9f,0),new Vector3(3.6f,.15f,2),ceramic);
                VisualFactory.Part("Auction gavel head",stopProp,PrimitiveType.Cube,new Vector3(0,3,0),new Vector3(1.6f,.6f,.6f),VisualFactory.GoldGlow);
                VisualFactory.Part("Auction gavel handle",stopProp,PrimitiveType.Cube,new Vector3(0,2.5f,0),new Vector3(.22f,1,.22f),ceramic);
            }
            else if(Stop.kind==EncounterRoute.PressureWorks)
            {
                VisualFactory.Part("Pressure reservoir",stopProp,PrimitiveType.Cylinder,new Vector3(0,1.8f,0),new Vector3(1.6f,1.5f,1.6f),CinderworksWorld.Enamel);
                VisualFactory.Part("Pressure valve",stopProp,PrimitiveType.Cylinder,new Vector3(0,3.4f,0),new Vector3(2,.15f,2),CinderworksWorld.Heat);
                VisualFactory.Part("Pressure gauge",stopProp,PrimitiveType.Cube,new Vector3(0,2.6f,-.87f),new Vector3(.8f,.55f,.12f),ceramic);
            }
            else
            {
                foreach(int side in new[]{-1,1})VisualFactory.Part("Rescue cage upright",stopProp,PrimitiveType.Cube,new Vector3(side*1.5f,1.6f,0),new Vector3(.2f,3.2f,.2f),iron);
                VisualFactory.Part("Rescue crossbeam",stopProp,PrimitiveType.Cube,new Vector3(0,3.1f,0),new Vector3(3.2f,.2f,.2f),ceramic);
                for(int i=0;i<3;i++){VisualFactory.Part("Attendance head",stopProp,PrimitiveType.Sphere,new Vector3(-1+i,2.1f,-.2f),Vector3.one*.3f,VisualFactory.Glow);VisualFactory.Part("Attendance body",stopProp,PrimitiveType.Cube,new Vector3(-1+i,1.55f,-.2f),new Vector3(.35f,.65f,.2f),ceramic);}
            }
        }
    }
    public sealed partial class PrototypeHUD
    {
        bool DrawRegionalRouteGlyph(EncounterRoute route,Vector2 p,Color color)
        {
            if(route<=EncounterRoute.Overtime||RegionalEncounters.Find(route)==null)return false;
            if(route==EncounterRoute.Auction){Stroke(p+new Vector2(-9,10),p+new Vector2(10,-9),color,3);Stroke(p+new Vector2(-2,-12),p+new Vector2(12,2),color,6);Stroke(p+new Vector2(-12,14),p+new Vector2(12,14),color,3);}
            else if(route==EncounterRoute.PressureWorks){Ring(p,13,color);Stroke(p,p+new Vector2(7,-7),color,3);Ring(p,2,color);}
            else if(route==EncounterRoute.Rescue){Ring(p+Vector2.up*-8,5,color);Stroke(p+new Vector2(0,-2),p+new Vector2(0,12),color,3);Stroke(p+new Vector2(-11,2),p+new Vector2(11,2),color,3);}
            else if(route==EncounterRoute.RepairLine){Stroke(p+new Vector2(-12,0),p+new Vector2(12,0),color,4);Stroke(p+new Vector2(0,-12),p+new Vector2(0,12),color,4);Ring(p,15,color);}
            else if(route==EncounterRoute.FreightYard){foreach(int side in new[]{-1,1}){Stroke(p+new Vector2(side*10,-13),p+new Vector2(side*10,11),color,3);Stroke(p+new Vector2(side*10,11),p+new Vector2(side*3,11),color,3);}Stroke(p+new Vector2(-10,-5),p+new Vector2(10,-5),color,3);}
            else if(route==EncounterRoute.Reclaimers){Ring(p,12,color);Stroke(p+new Vector2(9,0),p+new Vector2(-6,0),color,3);Stroke(p+new Vector2(-6,0),p+new Vector2(0,-7),color,3);Stroke(p+new Vector2(-6,0),p+new Vector2(0,7),color,3);}
            else foreach(int y in new[]{-7,5}){Stroke(p+new Vector2(-12,y+6),p+new Vector2(0,y-3),color,3);Stroke(p+new Vector2(0,y-3),p+new Vector2(12,y+6),color,3);}
            return true;
        }
        internal Rect RegionalEventOptionRect(int option,float w,float h)=>new Rect((w-1120)/2+370,(h-640)/2+260+option*116,724,98);
        internal bool HandleRegionalEventPointer(EventType type,Vector2 point,float w,float h)
        {
            if(type!=EventType.MouseDown||!game.StopOpen||!RegionalEncounters.IsNewEvent(game.Stop.kind)||game.Stop.reveal)return false;
            for(int i=0;i<2;i++)if(RegionalEventOptionRect(i,w,h).Contains(point))return game.ResolveRegionalEvent(i);
            return false;
        }
        void RegionalEventScreen(float w,float h)
        {
            var stop=game.Stop;float x=(w-1120)/2,y=(h-640)/2,right=x+370,width=724;bool auction=stop.kind==EncounterRoute.Auction,rescue=stop.kind==EncounterRoute.Rescue;
            Box(new Rect(0,0,w,h),new Color(0,0,0,.68f));Panel(new Rect(x,y,1120,640));
            DrawRegionalRouteGlyph(stop.kind,new Vector2(x+40,y+39),Accent);
            Label(new Rect(x+70,y+24,710,32),VectorEncounter.RouteName(stop.kind),24,Accent,true);
            Label(new Rect(x+26,y+71,800,24),"SALVAGE  "+game.Salvage+"     ·     INTEGRITY  "+Mathf.CeilToInt(game.Health)+" / "+Mathf.RoundToInt(game.MaxHealth),15,white,true);
            if(Button(new Rect(x+855,y+22,239,36),"SAVE & MAIN MENU"))game.SuspendExpedition();
            Stroke(new Vector2(x+26,y+108),new Vector2(x+1094,y+108),line,1);
            Label(new Rect(x+26,y+135,305,205),game.RegionalEventStory,18,white);
            string rule=auction?"One bid. The displayed item is guaranteed only if you win. Your bid is spent on either outcome. Leaving costs nothing.":rescue?"Three rescue attempts. Each success secures salvage and repair immediately. Earlier rewards cannot be lost. Tools cost salvage; forcing a lock risks injury.":"Four taps at most. Each tap risks the entire unbanked haul. A rupture ends the event. Your banked salvage is safe. You may bank the starting haul.";
            Label(new Rect(x+26,y+367,305,158),rule,16,muted);
            Label(new Rect(x+26,y+557,305,56),"Travel and combat are paused.\n"+game.SaveMessage,12,muted);
            if(auction)
            {
                var prize=stop.stock[0];ActionGlyph(new Rect(right,y+134,52,52),prize.def,RarityColor(prize.rank));
                Label(new Rect(right+65,y+133,width-65,30),prize.def.name,22,white,true);
                Label(new Rect(right+65,y+166,width-65,24),ItemRarity.Names[prize.rank]+" · owned "+game.Deck.Copies(prize.def,prize.rank)+" / 3 · matching copies fuse",14,muted);
                Label(new Rect(right,y+207,width,38),ItemSummary(new LoadoutItem(prize.def,prize.rank)),15,white);
            }
            else
            {
                Label(new Rect(right,y+135,width,30),rescue?"WORKERS FREED  "+stop.rescued+" / 3":"UNBANKED HAUL  "+stop.stash+" SALVAGE",23,Accent,true);
                Label(new Rect(right,y+181,width,25),"ATTEMPTS  "+stop.attempt+" / "+game.RegionalEventLimit,15,muted,true);
                Segments(new Rect(right,y+218,width,8),(float)stop.attempt/game.RegionalEventLimit,Accent,game.RegionalEventLimit);
            }
            if(stop.reveal)
            {
                Label(new Rect(right,y+272,width,175),stop.result,21,stop.failed?new Color(1,.65f,.4f):white,true);
                bool done=stop.attempt>=game.RegionalEventLimit||!rescue&&stop.failed;
                if(Button(new Rect(right,y+489,width,48),done?"FINISH & CHOOSE REWARDS":"SEE NEXT CHOICE",true))game.ContinueRegionalEvent();
                if(!done&&Button(new Rect(right,y+549,width,48),rescue?"LEAVE WITH SECURED REWARDS":"BANK "+stop.stash+" SALVAGE & LEAVE"))game.CashOutStop();
                return;
            }
            if(Event.current.button==0&&HandleRegionalEventPointer(Event.current.type,Pointer,w,h))Event.current.Use();
            for(int option=0;option<2;option++)
            {
                var r=RegionalEventOptionRect(option,w,h);bool available=game.CanResolveEvent(option);string name=auction?(option==0?"SMALL BID":"STRONG BID"):rescue?(option==0?"USE TOOLS":"FORCE LOCK"):(option==0?"CONTROLLED TAP":"OVERRIDE");
                Plate(r,available&&r.Contains(Pointer)?new Color(.07f,.18f,.21f,.98f):ink,available?Accent:line,6);Label(new Rect(r.x+14,r.y+10,r.width-28,25),name+" · "+game.EventChance(option)+"% SUCCESS",18,available?white:muted,true);
                string cost=game.EventPrice(option)>0?"Pay "+game.EventPrice(option)+" salvage on either outcome. ":"No salvage cost. ";
                string success=auction?"Win: displayed item.":rescue?"Win: +"+stop.gain+" salvage, repair up to "+stop.repair+".":"Win: +"+game.EventGain(option)+" unbanked salvage.";
                string failure=auction?"Lose: no item.":rescue?"Lose: "+game.EventHarm(option)+" integrity.":"Lose: whole haul + "+game.EventHarm(option)+" integrity.";
                Label(new Rect(r.x+14,r.y+42,r.width-28,47),cost+success+"\n"+failure+(!available?game.Salvage<game.EventPrice(option)?"  NEED MORE SALVAGE":"  TOO LITTLE INTEGRITY":""),15,available?white:new Color(.55f,.6f,.62f));
            }
            if(Button(new Rect(right,y+549,width,48),auction?"PASS · KEEP YOUR SALVAGE":rescue?"LEAVE WITH SECURED REWARDS":"BANK "+stop.stash+" SALVAGE & LEAVE"))game.CashOutStop();
        }
    }
}
