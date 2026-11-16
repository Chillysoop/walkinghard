using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    // Stable run definition, independent of the legacy six-fight checkpoint format.
    public static class LongJourney
    {
        public const string Id="continuance-1";
        public const int Count=21,Forks=9;
        public static bool WeaponDiscoveryAfter(int wave)=>wave==2||wave==9||wave==16;
        public static readonly string[] Districts={"CINDERWORKS","THE BELL FOUNDRY","THE LAST DISPATCH"};
        public static readonly string[] Stories={
            "Mara needs the Foreman's seal. The factory still counts the dead as workers.",
            "The seal opens the foundry. Its bells order every shift to begin again. Silence their keeper.",
            "The final dispatch contains an order to stop. The Pursuer was built to prevent its delivery."};
        public static readonly ChapterEncounter[] Encounters=Build();
        static ChapterEncounter[] Build()
        {
            var encounters=new ChapterEncounter[Count];
            for(int i=0;i<Count;i++)
            {
                int act=i/7,step=i%7;bool boss=step==6;
                var rewards=i==Count-1?Array.Empty<DraftKind>():i==0?new[]{DraftKind.Ability,DraftKind.Weapon}:i==2?new[]{DraftKind.Ultimate}:boss?new[]{DraftKind.Equipment,DraftKind.Augment}:step==4?new[]{DraftKind.Augment}:new[]{DraftKind.Mixed};
                encounters[i]=new ChapterEncounter("district-"+act+"-"+step,boss?new[]{"Shift Foreman","Bell Foreman","The Pursuer"}[act]:Districts[act]+" / "+(step+1),step==1||step==3||step==5?act*3+(step-1)/2:-1,boss?4:1,boss,rewards);
            }
            return encounters;
        }
        public static ExpeditionPlan Plan(int seed)
        {
            var random=new System.Random(seed^0x52ac71);
            var combats=new[]{EncounterRoute.ShieldLine,EncounterRoute.VolatileBreach,EncounterRoute.RepairLine,EncounterRoute.FreightYard,EncounterRoute.Reclaimers,EncounterRoute.EliteShift};
            var events=new[]{EncounterRoute.Auction,EncounterRoute.Rescue,EncounterRoute.PressureWorks,EncounterRoute.Overtime};
            var p=new ExpeditionPlan{waves=new EncounterRoute[Count],left=new EncounterRoute[Forks],right=new EncounterRoute[Forks],chosen=new EncounterRoute[Forks]};
            for(int act=0;act<3;act++)
            {
                // Shuffle a complete roster: each district has a legible identity without
                // a memorized combat order. Route choices can replace the next roster.
                var roster=combats.OrderBy(_=>random.Next()).ToArray();
                if(act==0){int elite=Array.IndexOf(roster,EncounterRoute.EliteShift);if(elite<2){roster[elite]=roster[2];roster[2]=EncounterRoute.EliteShift;}}
                for(int step=0;step<6;step++)p.waves[act*7+step]=roster[step];
                p.waves[act*7+6]=EncounterRoute.Standard;
                for(int fork=0;fork<3;fork++)
                {
                    int index=act*3+fork;
                    p.left[index]=fork==1?events[random.Next(events.Length)]:EncounterRoute.SalvageStop;
                    p.right[index]=fork==1?EncounterRoute.EliteShift:combats[random.Next(combats.Length)];
                    if(random.Next(2)==0){var swap=p.left[index];p.left[index]=p.right[index];p.right[index]=swap;}
                }
            }
            return p;
        }
    }
    public sealed partial class VectorEncounter
    {
        bool legacyChapterReview;
        public bool JourneyActive=>ChapterActive&&chapterRun.definition==LongJourney.Id;
        ChapterEncounter[] ChapterEncounters=>chapterRun?.definition==LongJourney.Id?LongJourney.Encounters:CinderworksChapter.Encounters;
        public int District=>JourneyActive?Mathf.Clamp((Wave-1)/7,0,2):0;
        public string JourneyHeading=>JourneyActive?"DISTRICT "+(District+1)+" / 3 · "+LongJourney.Districts[District]:"CINDERWORKS";
        public string JourneyStory=>LongJourney.Stories[Mathf.Clamp(District+(State==RunState.Drafting&&Wave%7==0?1:0),0,2)];
        public string JourneyBossName=>JourneyActive?LongJourney.Encounters[District*7+6].name.ToUpperInvariant():"SHIFT FOREMAN";
        bool PursuerWave=>JourneyActive?Wave==LongJourney.Count:!ChapterActive&&Wave==8;
        float ArrivalDuration=>JourneyActive?40+District*5:tuning.waveArrivals;
        float EnemyHealthScale=>JourneyActive?1+District*.2f+(Wave-1)%7*.025f:1+.035f*(Wave-1);
        string JourneyPreview(int wave)
        {
            int district=Mathf.Clamp((wave-1)/7,0,2);var encounter=LongJourney.Encounters[Mathf.Clamp(wave-1,0,20)];
            if(encounter.boss)return encounter.name.ToUpperInvariant()+" · "+(wave==21?"deliver the final order":wave==14?"rally calls + press slams · break pressure":"stagger + impacts expose the vent");
            return LongJourney.Districts[district]+" · "+(RegionalEncounters.Find(RegionalRoute(wave))?.name??"GATE PATROL")+" · "+SpecialistPreview(wave);
        }
        void JourneyIntermission()
        {
            if(!JourneyActive||Wave%7!=0||Wave>=WaveCount)return;
            Health=Mathf.Min(MaxHealth,Health+MaxHealth*.25f);
            SelectedRoute=EncounterRoute.SalvageStop;routedWave=Wave+1;
            Announce("DISTRICT CLEARED · +25% INTEGRITY · SALVAGE BENCH");
        }
    }
    public sealed partial class PrototypeHUD
    {
        void JourneyProgress(float w,float h)
        {
            if(!game.JourneyActive||game.ShowForkSigns||game.ForemanPresent||game.FinaleActive)return;
            var r=new Rect(w/2-250,20,500,63);Panel(r);
            Label(new Rect(r.x+10,r.y+6,480,20),game.JourneyHeading,13,Accent,true,TextAnchor.MiddleCenter);
            for(int i=0;i<7;i++)
            {
                int step=(game.Wave-1)%7;var dot=new Vector2(r.x+94+i*52,r.y+41);Color color=i<=step?Accent:muted;
                if(i<6)Stroke(dot+Vector2.right*7,dot+Vector2.right*45,line,2);
                if(i==6){Stroke(dot+Vector2.up*7,dot+Vector2.right*7,color,2);Stroke(dot+Vector2.right*7,dot+Vector2.down*7,color,2);Stroke(dot+Vector2.down*7,dot+Vector2.left*7,color,2);Stroke(dot+Vector2.left*7,dot+Vector2.up*7,color,2);}
                else Ring(dot,i==step?6:4,color);
            }
        }
    }
}
