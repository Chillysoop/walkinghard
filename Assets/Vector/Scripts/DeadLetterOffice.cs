using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        int officeBranch,officeSelection,officeRank=1;Vector2 officeDetailScroll;
        string officeFlashItem="";float officeFlashAt;
        internal int OfficeBranch=>officeBranch;
        internal int OfficeSelection=>officeSelection;
        Rect OfficePage(float w,float h)=>new Rect((w-Mathf.Min(w-32,1480))/2,16,Mathf.Min(w-32,1480),h-32);
        internal Rect OfficeBranchButton(int branch,float w,float h){var p=OfficePage(w,h);return new Rect(p.x+18,p.y+104+branch*55,180,47);}
        Rect OfficeMap(float w,float h){var p=OfficePage(w,h);return new Rect(p.x+214,p.y+104,p.width-614,p.height-224);}
        Rect OfficeDetail(float w,float h){var p=OfficePage(w,h);return new Rect(p.xMax-382,p.y+104,364,p.height-224);}
        internal Rect OfficeStar(int index,float w,float h)
        {
            int i=Array.IndexOf(ConstellationContent.BranchItems[officeBranch],Constellation.Nodes[index].item);if(i<0)return new Rect(-2000,-2000,1,1);var m=OfficeMap(w,h);
            if(officeBranch==0){float width=(m.width-32)/3;return new Rect(m.x+(i/2)*(width+16),m.y+48+(i%2)*(m.height-70)/2,width,(m.height-102)/2);}
            float cw=(m.width-30)/2,ch=Mathf.Min(132,(m.height-68)/3),step=(m.height-ch-36)/2;float x=i==0?m.center.x-cw/2:i==1||i==3?m.x:m.xMax-cw;return new Rect(x,m.y+30+(i==0?0:i<3?step:step*2),cw,ch);
        }
        internal Rect OfficeNodeButton(int index,float w,float h){var d=OfficeDetail(w,h);return index==officeSelection?new Rect(d.x+16,d.yMax-57,d.width-32,42):new Rect(-2000,-2000,1,1);}
        internal void SelectOfficeBranch(int branch)
        {officeBranch=Mathf.Clamp(branch,0,6);officeSelection=Array.FindIndex(Constellation.Nodes,n=>n.item==ConstellationContent.BranchItems[officeBranch][0]);officeDetailScroll=Vector2.zero;}
        internal bool HandleOfficePointer(EventType type,Vector2 point,float w,float h)
        {
            if(type!=EventType.MouseDown||!game.OfficeOpen)return false;
            for(int b=0;b<7;b++)if(OfficeBranchButton(b,w,h).Contains(point)){SelectOfficeBranch(b);return true;}
            for(int i=0;i<Constellation.Nodes.Length;i++)if(OfficeStar(i,w,h).Contains(point)){officeSelection=i;officeDetailScroll=Vector2.zero;return true;}
            if(OfficeNodeButton(officeSelection,w,h).Contains(point)){var item=Constellation.Nodes[officeSelection].item;if(game.BuyConstellation(item)){officeFlashItem=item;officeFlashAt=Time.unscaledTime;}return true;}
            return false;
        }
        void DeadLetterOffice(float w,float h)
        {
            TabletFrame(w,h);var p=OfficePage(w,h);var map=OfficeMap(w,h);var detail=OfficeDetail(w,h);int owned=game.ConstellationOwned;
            Label(new Rect(p.x+18,p.y+12,780,34),"DEAD LETTER OFFICE / CONSTELLATION",26,white,true);
            Label(new Rect(p.xMax-240,p.y+12,222,30),game.Sparks+" SPARKS",25,Accent,true,TextAnchor.MiddleRight);
            Label(new Rect(p.x+18,p.y+55,p.width-36,23),owned+" / "+Constellation.Nodes.Length+" PATTERNS RECOVERED   ·   Unlock equipment for future journeys. Build it from loot on the road.",14,white);
            Box(new Rect(p.x+18,p.y+87,p.width-36,3),line);Box(new Rect(p.x+18,p.y+87,(p.width-36)*owned/Constellation.Nodes.Length,3),Accent);
            if(Event.current.button==0&&HandleOfficePointer(Event.current.type,Pointer,w,h))Event.current.Use();
            for(int b=0;b<7;b++)
            {
                var r=OfficeBranchButton(b,w,h);var ids=ConstellationContent.BranchItems[b];int count=ids.Count(game.HasConstellation);int ready=ids.Count(id=>game.UnlockRequirement(Constellation.Nodes.First(n=>n.item==id)).StartsWith("UNLOCK ·"));var color=ConstellationContent.ColorFor(b);
                Plate(r,b==officeBranch?new Color(.08f,.17f,.19f):ink,b==officeBranch?color:line,7,b==officeBranch?2:1);Label(new Rect(r.x+12,r.y+7,r.width-24,18),ConstellationContent.BranchNames[b],13,color,true);Label(new Rect(r.x+12,r.y+28,r.width-24,15),count+" / "+ids.Length+(ready>0?"  ·  "+ready+" AFFORDABLE":"  ·  VIEW BRANCH"),10,white);
            }
            Label(new Rect(p.x+18,p.y+504,180,55),game.CompletionSeal?"MARA / SEAL RECOVERED\nIVEN / "+game.RescuedWorkers+" WORKERS HOME":"MARA / FIND THE SEAL\nDefeat the Foreman to open deeper stars.",12,game.CompletionSeal?Accent:muted,true);
            Label(new Rect(map.x,map.y-2,map.width,24),ConstellationContent.BranchNames[officeBranch]+" / "+(officeBranch==0?"ORIGINAL PATTERNS":"RECONSTRUCTION PATH"),14,ConstellationContent.ColorFor(officeBranch),true);
            foreach(string id in ConstellationContent.BranchItems[officeBranch])
            {int index=Array.FindIndex(Constellation.Nodes,n=>n.item==id);var node=Constellation.Nodes[index];if(node.parent==null)continue;int parent=Array.FindIndex(Constellation.Nodes,n=>n.item==node.parent);var a=OfficeStar(parent,w,h);var b=OfficeStar(index,w,h);Stroke(new Vector2(a.center.x,a.yMax),new Vector2(b.center.x,b.y),game.HasConstellation(node.parent)?ConstellationContent.ColorFor(officeBranch):line,2);}
            int hovered=-1;
            foreach(string id in ConstellationContent.BranchItems[officeBranch])
            {
                int index=Array.FindIndex(Constellation.Nodes,n=>n.item==id);var node=Constellation.Nodes[index];var def=LoadoutCatalog.Find(id);var r=OfficeStar(index,w,h);bool unlocked=game.HasConstellation(id),chosen=index==officeSelection;bool ready=game.UnlockRequirement(node).StartsWith("UNLOCK ·");var color=ConstellationContent.ColorFor(officeBranch);if(r.Contains(Pointer))hovered=index;
                TabletMaterial(r,unlocked?1:3);Plate(r,new Color(.015f,.035f,.05f,.35f),chosen?white:unlocked?color:line,7,chosen?2:1);float icon=Mathf.Min(42,r.width*.22f);ActionGlyph(new Rect(r.x+9,r.y+10,icon,icon),def,white);
                Label(new Rect(r.x+icon+18,r.y+10,r.width-icon-26,34),def.name,officeBranch==0?12:15,white,true);string kind=def.kind==ItemKind.Weapon?(def.hands==2?"2H WEAPON":"1H WEAPON"):Category(def).ToUpperInvariant();Label(new Rect(r.x+10,r.y+58,r.width-20,16),kind,10,color,true);Label(new Rect(r.x+10,r.y+78,r.width-20,officeBranch==0?42:27),ItemSummary(new LoadoutItem(def)),11,white);Label(new Rect(r.x+10,r.yMax-25,r.width-20,18),unlocked?"UNLOCKED":ready?node.cost+" SPARKS · AVAILABLE":node.cost+" SPARKS · LOCKED",11,unlocked?Accent:ready?white:muted,true);
                if(!unlocked&&!ready)LockIcon(new Vector2(r.xMax-13,r.y+12),muted);if(id==officeFlashItem&&Time.unscaledTime-officeFlashAt<1.5f)Border(r,new Color(.5f,1,1,game.ReducedMotion?1:1-(Time.unscaledTime-officeFlashAt)/1.5f),3);
            }
            int detailIndex=hovered>=0?hovered:officeSelection;var selected=Constellation.Nodes[detailIndex];var item=LoadoutCatalog.Find(selected.item);bool preview=detailIndex!=officeSelection;
            TabletMaterial(detail,2);ActionGlyph(new Rect(detail.x+16,detail.y+16,64,64),item,white);Label(new Rect(detail.x+94,detail.y+17,detail.width-108,42),item.name,21,white,true);Label(new Rect(detail.x+94,detail.y+61,detail.width-108,24),Category(item).ToUpperInvariant()+" · "+selected.cost+" SPARKS",11,ConstellationContent.ColorFor(ConstellationContent.Branch(item.id)),true);
            Label(new Rect(detail.x+16,detail.y+91,detail.width-32,22),preview?"HOVER PREVIEW · CLICK STAR TO SELECT":"PATTERN DETAILS / "+(game.HasConstellation(item.id)?"OWNED":"FUTURE LOOT"),10,Accent,true);
            if(Button(new Rect(detail.x+16,detail.y+119,detail.width-32,27),"RARITY PREVIEW: "+ItemRarity.Names[officeRank].ToUpperInvariant()+"   >"))officeRank=officeRank==5?1:officeRank+1;
            var shown=new LoadoutItem(item,officeRank);string desc=ConstellationContent.IsNew(item.id)?ConstellationRules(shown):item.description+"\n\n"+EffectRules(shown)+"\n\n"+item.tradeoff;
            string body=ItemStats(item,officeRank)+"\n\n"+desc+"\n\n"+(selected.parent==null?"ENTRY STAR · no item prerequisite":"REQUIRES: "+LoadoutCatalog.Find(selected.parent).name+" + Completion Seal")+"\n\nUnlocks enter future loot pools. Buying does not equip gear or change a saved journey.";
            var viewport=new Rect(detail.x+16,detail.y+160,detail.width-32,detail.height-229);float bodyHeight=Style(13).CalcHeight(new GUIContent(body),viewport.width-18)+12;officeDetailScroll=GUI.BeginScrollView(viewport,officeDetailScroll,new Rect(0,0,viewport.width-18,bodyHeight));Label(new Rect(0,0,viewport.width-18,bodyHeight),body,13,white);GUI.EndScrollView();
            var buy=OfficeNodeButton(officeSelection,w,h);string state=game.UnlockRequirement(Constellation.Nodes[officeSelection]);bool available=state.StartsWith("UNLOCK ·");Plate(buy,available?new Color(.07f,.2f,.21f):ink,available?Accent:line,5,available?2:1);Label(new Rect(buy.x+6,buy.y+3,buy.width-12,buy.height-6),preview?"CLICK A STAR TO SELECT IT":state,12,available&&!preview?Accent:muted,true,TextAnchor.MiddleCenter);
            Label(new Rect(p.x+18,p.yMax-13,p.width-36,13),game.HasCheckpoint?"Starting a new journey replaces the saved run. Unlock purchases apply to the next new journey.":"3 districts / 21 encounters / 9 road choices. Banked Sparks and unlocked patterns persist.",10,muted);
            float foot=p.yMax-100;string message=game.OfficeMessage.Length>0?game.OfficeMessage:game.Sparks==0?"Earn 1 Spark per ordinary fight and 4 per boss. Keep banked Sparks on defeat.":ConstellationContent.BranchLines[officeBranch];Label(new Rect(p.x+18,foot,p.width-36,36),message,13,white);float bw=(p.width-68)/3;
            if(Button(new Rect(p.x+18,foot+41,bw,42),"START JOURNEY · ~30 MIN",true))game.StartChapter();if(ExpeditionButton(new Rect(p.x+34+bw,foot+41,bw,42),game.HasCheckpoint?game.ResumeLabel:"NO SAVED JOURNEY",game.HasCheckpoint)){if(game.ResumeExpedition())game.CloseOffice();}if(Button(new Rect(p.x+50+2*bw,foot+41,bw,42),"BACK TO MAIN MENU"))game.CloseOffice();
        }
    }
}
