using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        internal Rect OfficeNodeButton(int index,float w,float h)
        {int col=index/2,row=index%2;return new Rect((w-1140)/2+24+col*370,(h-660)/2+222+row*174+99,352,34);}
        internal bool HandleOfficePointer(EventType type,Vector2 point,float w,float h)
        {
            if(type!=EventType.MouseDown||!game.OfficeOpen)return false;
            for(int i=0;i<Constellation.Nodes.Length;i++)if(OfficeNodeButton(i,w,h).Contains(point))return game.BuyConstellation(Constellation.Nodes[i].item);
            return false;
        }
        void DeadLetterOffice(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.015f,.025f,.04f,.98f));float x=(w-1140)/2,y=(h-660)/2;Panel(new Rect(x,y,1140,660));
            Label(new Rect(x+24,y+17,780,34),"DEAD LETTER OFFICE",28,white,true);
            Label(new Rect(x+800,y+20,315,30),game.Sparks+" SPARK"+(game.Sparks==1?"":"S"),25,Accent,true,TextAnchor.MiddleRight);
            Label(new Rect(x+24,y+57,1080,25),"Spend Sparks to add items to future chapter drafts. Unlocks persist; equipment resets each expedition.",15,white);
            Panel(new Rect(x+24,y+94,540,75));Panel(new Rect(x+576,y+94,540,75));
            Label(new Rect(x+38,y+102,510,21),"MARA VENN  /  "+(game.CompletionSeal?"COMPLETION SEAL RECOVERED":"THE UNFINISHED ORDER"),14,Accent,true);
            Label(new Rect(x+38,y+130,510,33),game.CompletionSeal?"You brought back proof: a duty can end. The outer stars are open.":"The Foreman carries a completion seal. Bring it home to open the outer stars.",13,white);
            Label(new Rect(x+590,y+102,420,21),"IVEN RUSK  /  "+game.RescuedWorkers+" WORKER"+(game.RescuedWorkers==1?"":"S")+" HOME",14,Accent,true);
            Label(new Rect(x+590,y+130,510,33),game.RescuedWorkers>0?"Their lamps stay lit here. We rebuild the patterns you recover.":"There are empty benches waiting. Rescue workers on the road; each one keeps a place here.",13,white);
            for(int worker=0;worker<5;worker++)
            {var lamp=new Vector2(x+1022+worker*18,y+113);Color c=worker<game.RescuedWorkers?Accent:line;Ring(lamp,4,c);Stroke(lamp+new Vector2(0,5),lamp+new Vector2(0,13),c,3);}
            var root=new Vector2(x+570,y+196);Ring(root,9,Accent);Stroke(root+Vector2.left*14,root+Vector2.right*14,Accent,2);Stroke(root+Vector2.up*14,root+Vector2.down*14,Accent,2);
            for(int col=0;col<3;col++)
            {
                float center=x+200+col*370;Stroke(root,new Vector2(center,y+216),line,2);Stroke(new Vector2(center,y+355),new Vector2(center,y+395),game.CompletionSeal?Accent:line,2);
            }
            if(Event.current.button==0&&HandleOfficePointer(Event.current.type,Pointer,w,h))Event.current.Use();
            for(int i=0;i<Constellation.Nodes.Length;i++)
            {
                var node=Constellation.Nodes[i];var item=LoadoutCatalog.Find(node.item);var button=OfficeNodeButton(i,w,h);var r=new Rect(button.x,button.y-99,button.width,141);bool owned=game.HasConstellation(node.item);string requirement=game.UnlockRequirement(node);
                Plate(r,ink,owned?Accent:line,7,owned?2:1);ActionGlyph(new Rect(r.x+10,r.y+10,32,32),item,owned?Accent:white);
                Label(new Rect(r.x+51,r.y+8,r.width-61,17),new[]{"EQUIPMENT","PASSIVES · EQUIPPED GEAR","ULTIMATES"}[node.branch]+" · "+node.cost+" SPARKS",10,Accent,true);
                Label(new Rect(r.x+51,r.y+26,r.width-61,24),item.name,17,white,true);
                Label(new Rect(r.x+12,r.y+53,r.width-24,43),item.description,12,white);
                bool buy=requirement.StartsWith("UNLOCK ·");Plate(button,buy&&button.Contains(Pointer)?new Color(.07f,.18f,.21f,.98f):ink,buy?Accent:line,4);
                Label(button,requirement,12,owned?Accent:buy?white:muted,true,TextAnchor.MiddleCenter);
            }
            Label(new Rect(x+24,y+548,1092,23),game.OfficeMessage.Length>0?game.OfficeMessage:"Earn 1 Spark per encounter cleared; 4 for the Foreman. Banked immediately, kept on defeat. Tier 2 needs its earlier star and the seal.",12,white);
            if(Button(new Rect(x+24,y+581,352,45),"START CINDERWORKS · 6 ENCOUNTERS",true))game.StartChapter();
            if(ExpeditionButton(new Rect(x+394,y+581,352,45),game.HasCheckpoint?game.ResumeLabel:"NO SAVED EXPEDITION",game.HasCheckpoint)){if(game.ResumeExpedition())game.CloseOffice();}
            if(Button(new Rect(x+764,y+581,352,45),"BACK TO MAIN MENU"))game.CloseOffice();
            Label(new Rect(x+24,y+634,1092,18),game.HasCheckpoint?"A new chapter replaces your saved run. Purchases apply to the next new chapter.":"Six encounters · two walking forks · Foreman finale · safe checkpoints. Salvage is spent during the run; Sparks are spent here.",11,muted);
        }
    }
}
