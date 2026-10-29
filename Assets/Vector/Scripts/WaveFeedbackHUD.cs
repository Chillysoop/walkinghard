using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        bool waveDetails;Vector2 waveDetailsScroll;VectorEncounter.WaveFeedback displayedWave;
        void WaveFeedbackHeader(float w)
        {
            var report=game.LastWaveFeedback;
            if(displayedWave!=report){displayedWave=report;waveDetails=false;waveDetailsScroll=Vector2.zero;}
            if(report==null){Label(new Rect(251,56,w-285,18),"AUTO-FUSE: 3 identical items + SAME RARITY. Equipped + stored count. Augments do not fuse.",11,muted);return;}
            Label(new Rect(251,56,w-285,18),"LAST WAVE  /  "+report.damage.ToString("N0")+" DAMAGE   ·   "+report.healing.ToString("0")+" HEALED   ·   "+report.blocked.ToString("0")+" BLOCKED   ·   "+report.collisionKills+" COLLISION KILLS",11,Accent,true);
            if(Button(new Rect(763,28,180,25),"WAVE "+report.wave+" DETAILS",waveDetails)){waveDetails=!waveDetails;detailItem=null;iconGuide=false;}
        }
        void DrawWaveDetails(float w,float h)
        {
            var report=game.LastWaveFeedback;if(!waveDetails||report==null)return;
            Box(new Rect(0,0,w,h),new Color(.003f,.012f,.02f,.88f));
            Rect panel=new Rect(60,85,w-120,h-125);Panel(panel);
            Label(new Rect(panel.x+22,panel.y+18,panel.width-200,28),"WAVE "+report.wave+" / WHAT WORKED",23,white,true);
            if(Button(new Rect(panel.xMax-142,panel.y+18,120,27),"CLOSE [ ESC ]")){waveDetails=false;return;}
            Label(new Rect(panel.x+22,panel.y+54,panel.width-44,21),report.kills+" KILLS  ·  "+report.seconds.ToString("0.0")+"s  ·  "+report.damage.ToString("N0")+" DAMAGE  ·  "+report.healing.ToString("0.#")+" HEALED  ·  "+report.blocked.ToString("0.#")+" BLOCKED  ·  "+report.collisionKills+" COLLISION KILLS",13,Accent,true);
            Label(new Rect(panel.x+22,panel.y+82,panel.width-44,32),"Recorded before this checkpoint. Damage includes body impacts. Blocks below are item effects; passive resistance prevented another "+report.resistance.ToString("0.#")+". Paired reload timers overlap.",11,muted);
            var wait=report.items.OrderByDescending(i=>i.values.rangeWait).FirstOrDefault();
            string insight=wait!=null&&wait.values.rangeWait>=.5f?"LONGEST TARGET WAIT: "+wait.name+" / "+wait.values.rangeWait.ToString("0.0")+"s. Check its reach and the actions before it.":"TARGET ACCESS: No item spent half a second waiting for a reachable target.";
            Label(new Rect(panel.x+22,panel.y+118,panel.width-44,22),insight,12,Gold,true);
            Rect view=new Rect(panel.x+20,panel.y+153,panel.width-40,panel.height-197);
            float width=view.width-24,contentHeight=34+report.items.Length*58+64+report.triggers.Length*23;
            waveDetailsScroll=GUI.BeginScrollView(view,waveDetailsScroll,new Rect(0,0,width,contentHeight));
            float nameWidth=width*.30f,col=(width-nameWidth)/6;
            string[] headings={"KILLS","DAMAGE","HEAL","BLOCK","CONTROL","DELAYS"};
            Label(new Rect(3,0,nameWidth-6,20),"ITEM / RARITY / AUGMENT",11,muted,true);
            for(int i=0;i<headings.Length;i++)Label(new Rect(nameWidth+i*col,0,col-6,20),headings[i],11,muted,true);
            float y=27;
            foreach(var row in report.items)
            {
                var def=LoadoutCatalog.Find(row.id);var c=row.values;ActionGlyph(new Rect(3,y+2,30,30),def,TypeColor(def.kind));
                Label(new Rect(41,y,nameWidth-48,20),row.name,13,white,true);
                Label(new Rect(41,y+20,nameWidth-48,28),ItemRarity.Names[row.rank]+(row.module.Length>0?" / "+row.module:""),10,RarityColor(row.rank));
                string[] values={row.kills.ToString(),(c.direct+c.collision).ToString("0"),c.healing.ToString("0.#"),c.absorption.ToString("0.#"),c.control.ToString("0.0")+"s",c.reload.ToString("0.0")+"s reload"};
                for(int i=0;i<values.Length;i++)Label(new Rect(nameWidth+i*col,y,col-6,20),values[i],12,i==5?Gold:white);
                Label(new Rect(nameWidth+col,y+21,col-6,30),c.collision.ToString("0")+" collision",9,muted);
                Label(new Rect(nameWidth+4*col,y+21,col-6,30),c.assists+" assists",10,muted);
                Label(new Rect(nameWidth+5*col,y+19,col-6,34),c.rangeWait.ToString("0.0")+"s target\n"+c.preparation.ToString("0.0")+"s ready",9,muted);
                Stroke(new Vector2(0,y+53),new Vector2(width,y+53),line,1);y+=58;
            }
            Label(new Rect(3,y+8,width-6,22),"SYNERGIES THAT TRIGGERED",13,Accent,true);y+=36;
            if(report.triggers.Length==0)Label(new Rect(3,y,width-6,22),"No tracked synergy triggered this wave.",12,muted);
            foreach(var trigger in report.triggers){Label(new Rect(3,y,width-6,22),trigger.name+"  × "+trigger.count,12,white);y+=23;}
            GUI.EndScrollView();
            Label(new Rect(panel.x+22,panel.yMax-32,panel.width-44,20),"One-wave totals, not estimates. Setup assists do not add kills. Drag and equip after closing this panel.",11,muted);
        }
    }
}
