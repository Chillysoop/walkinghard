using System;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        static string JourneyDuration(double seconds)=>((int)seconds/60).ToString()+":"+((int)seconds%60).ToString("00");
        float DrawJourneyTiming(float y)
        {
            if(!game.JourneyActive)return y;
            var t=game.JourneyPacing;
            Label(new Rect(4,y,866,20),t.automated?"AUTOMATED RUN · human decision time not measured":t.complete?"JOURNEY TIME · actual play, excluding pauses and time away":"JOURNEY TIME · measured since this save was upgraded",12,Accent,true);y+=25;
            string[] titles={"ACTIVE PLAY","COMBAT + TRAVEL","EQUIPMENT","EVENTS + SERVICES"};
            double[] times={t.Active,t.combat,t.equipment,t.events};
            for(int i=0;i<4;i++)
            {
                Box(new Rect(4+i*219,y,209,53),new Color(.045f,.10f,.13f,.95f));
                Label(new Rect(14+i*219,y+4,189,18),titles[i],10,muted,true);
                Label(new Rect(14+i*219,y+23,189,25),t.automated?"—":JourneyDuration(times[i]),21,white,true);
            }
            y+=58;
            Label(new Rect(4,y,866,18),"Excluded: paused "+JourneyDuration(t.paused)+" · away "+JourneyDuration(t.away)+"     /     Within combat: roads "+JourneyDuration(t.roads)+" · accelerated "+JourneyDuration(t.accelerated),11,muted);y+=20;
            Label(new Rect(4,y,866,18),"Saved journey only. Time between sessions and attempts rolled back to a checkpoint are excluded.",10,muted);
            return y+27;
        }
    }
}
