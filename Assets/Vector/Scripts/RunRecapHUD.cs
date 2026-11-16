using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        float DrawIntegrityRecap(float y)
        {
            y=DrawJourneyTiming(y);var log=game.RunHistory;
            Label(new Rect(4,y,866,20),game.FinalHitText,12,Accent,true);y+=27;
            bool defeat=game.State==VectorEncounter.RunState.Defeated;
            string[] titles={log.complete?"DAMAGE TAKEN":"DAMAGE SINCE SAVE UPGRADE",defeat?"FINAL ENCOUNTER":"BIGGEST INTEGRITY DROP",defeat?"FINAL FIGHT DAMAGE":"RUN SEQUENCE DAMAGE"};
            for(int i=0;i<3;i++){Box(new Rect(4+i*292,y,282,116),new Color(.045f,.10f,.13f,.95f));Label(new Rect(14+i*292,y+8,262,18),titles[i],11,muted,true);}
            int n=0;foreach(var source in log.sources.OrderByDescending(r=>r.damage).Take(3))
            {Label(new Rect(14,y+31+n*24,260,20),source.name+"  ·  "+source.damage.ToString("0"),13,white);n++;}
            if(n==0)Label(new Rect(14,y+34,260,40),"No recorded integrity loss",12,muted);
            var encounter=game.State==VectorEncounter.RunState.Defeated?log.encounters.LastOrDefault():log.encounters.OrderByDescending(r=>r.start-r.end).FirstOrDefault();
            if(encounter!=null)
            {
                Label(new Rect(306,y+31,262,20),"Encounter "+encounter.wave+" · "+(encounter.finished?"CLEARED":"OVERRUN"),13,white,true);
                Label(new Rect(306,y+56,262,20),encounter.start.ToString("0")+" → "+encounter.end.ToString("0")+" integrity · low "+encounter.lowest.ToString("0"),12,white);
                Label(new Rect(306,y+81,262,20),encounter.arrivedWith+" foes carried into this fight",11,muted);
            }
            n=0;
            if(defeat&&game.LastWaveFeedback?.wave==game.Wave)
            {
                foreach(var item in game.LastWaveFeedback.items.GroupBy(r=>r.name).Select(g=>new{name=g.Key,damage=g.Sum(r=>r.values.direct+r.values.collision)}).OrderByDescending(r=>r.damage).Where(r=>r.damage>0).Take(3))
                {Label(new Rect(598,y+30+n*25,262,18),item.name+" · "+item.damage.ToString("0"),11,white);n++;}
                if(n==0)Label(new Rect(598,y+34,262,40),"No recorded damage in this fight",12,muted);
            }
            else foreach(var item in game.LiveSequenceStats().OrderByDescending(r=>r.damage).Take(3))
            {Label(new Rect(598,y+30+n*25,262,18),item.positions+"  "+item.label+" · "+item.damage.ToString("0"),11,white);n++;}
            y+=125;
            Label(new Rect(4,y,860,20),"ENCOUNTER       INTEGRITY IN → OUT       LOW       DAMAGE TAKEN       FOES IN → OUT",11,Accent,true);y+=25;
            foreach(var row in log.encounters)
            {
                string title=row.wave.ToString("00")+(row.finished?"":" · LOST");
                Label(new Rect(4,y,120,21),title,12,row.finished?white:VisualFactory.Orange,true);
                Label(new Rect(150,y,180,21),row.start.ToString("0")+" → "+row.end.ToString("0"),12,white);
                Label(new Rect(365,y,80,21),row.lowest.ToString("0"),12,white);
                Label(new Rect(460,y,155,21),row.damage.ToString("0"),12,white);
                Label(new Rect(665,y,180,21),row.arrivedWith+" → "+row.survivors,12,muted);y+=23;
            }
            if(!log.complete){Label(new Rect(4,y,860,20),"Earlier encounters predate this integrity log; existing run totals are retained.",11,muted);y+=25;}
            return y+12;
        }
    }
}
