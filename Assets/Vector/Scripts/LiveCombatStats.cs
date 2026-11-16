using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public sealed class LiveStatRow
        {
            public LoadoutItem item;public string label,positions;public int kills,assists;public float damage,healing;
        }
        public bool IsolatedProfile=>saveDirectoryOverride!=null;
        // Exchanged items keep their own history; fusion predecessors are already included in their keeper.
        public LoadoutItem[] RecordedItems=>Deck.Items.Concat(contributions.Keys).Concat(itemKills.Keys).Distinct().Where(i=>i.def.kind!=ItemKind.Basic&&i.def.kind!=ItemKind.Augment&&KillOwner(i)==i).ToArray();
        LiveStatRow ItemStat(LoadoutItem item,string positions)
        {
            var c=ContributionFor(item);
            return new LiveStatRow{item=item,label=item.def.name,positions=positions,kills=KillsFor(item),damage=c.direct+c.collision,healing=c.healing,assists=c.assists};
        }
        public List<LiveStatRow> LiveSequenceStats()
        {
            var rows=new List<LiveStatRow>();
            for(int slot=0;slot<5;slot++)
            {
                var action=Deck.Slots[slot];var item=action?.def.kind==ItemKind.Basic?Deck.WeaponFor(action):action;if(item==null)continue;
                var existing=rows.FirstOrDefault(r=>r.item==item);
                if(existing!=null){existing.positions+="+"+(slot+1);continue;}
                rows.Add(ItemStat(item,(slot+1).ToString()));
            }
            if(Deck.Ultimate!=null)rows.Add(ItemStat(Deck.Ultimate,"ULT"));
            return rows;
        }
        public LiveStatRow LiveTotals()
        {
            var total=new LiveStatRow{label="RUN TOTAL",kills=Kills};
            foreach(var entry in contributions)
            {
                // Consumed copies remain in the historical map; the surviving item already owns their sums.
                if(KillOwner(entry.Key)!=entry.Key)continue;var c=entry.Value;
                total.damage+=c.direct+c.collision;total.healing+=c.healing;total.assists+=c.assists;
            }
            return total;
        }
        public LiveStatRow LiveOtherStats(List<LiveStatRow> sequence)
        {
            var total=LiveTotals();return new LiveStatRow{label="GEAR + OTHER",positions="+",kills=Mathf.Max(0,total.kills-EnvironmentalKills-sequence.Sum(r=>r.kills)),damage=Mathf.Max(0,total.damage-sequence.Sum(r=>r.damage)),healing=Mathf.Max(0,total.healing-sequence.Sum(r=>r.healing)),assists=Mathf.Max(0,total.assists-sequence.Sum(r=>r.assists))};
        }
    }
    public sealed partial class PrototypeHUD
    {
        internal bool liveStatsOpen;internal int liveStatsMask=15;bool statsPrefsLoaded;Vector2 liveStatsScroll;
        void LoadStatsPrefs()
        {if(statsPrefsLoaded)return;statsPrefsLoaded=true;if(game.IsolatedProfile)return;liveStatsOpen=PlayerPrefs.GetInt("LiveStats.Open",0)!=0;liveStatsMask=PlayerPrefs.GetInt("LiveStats.Metrics",15)&15;}
        void SaveStatsPrefs()
        {if(game.IsolatedProfile)return;PlayerPrefs.SetInt("LiveStats.Open",liveStatsOpen?1:0);PlayerPrefs.SetInt("LiveStats.Metrics",liveStatsMask);PlayerPrefs.Save();}
        public void ToggleLiveStats(){LoadStatsPrefs();liveStatsOpen=!liveStatsOpen;SaveStatsPrefs();}
        float StatsDensity(float w)=>Mathf.Max(1,.9f/(Screen.width/w));
        internal Rect LiveStatsRect(float w,float h)=>new Rect(w-20-310*StatsDensity(w),game.Deck.Ultimate==null?226:330,310*StatsDensity(w),Mathf.Max(160,h-175-(game.Deck.Ultimate==null?226:330)));
        internal Rect StatsMetricRect(Rect r,int index)=>new Rect(r.x+8+index*74,r.y+57,70,23);
        internal bool HandleStatsPointer(EventType type,Vector2 pointer,float w,float h)
        {
            if(type!=EventType.MouseDown||game.State!=VectorEncounter.RunState.Running)return false;
            var r=LiveStatsRect(w,h);
            float density=StatsDensity(w);var drawn=new Rect(r.x/density,r.y/density,r.width/density,r.height/density);
            if(new Rect(r.x,r.y,r.width,28*density).Contains(pointer)){ToggleLiveStats();return true;}
            if(liveStatsOpen)for(int i=0;i<4;i++)if(StatsMetricRect(drawn,i).Contains(pointer/density)){liveStatsMask^=1<<i;SaveStatsPrefs();return true;}
            return false;
        }
        void LiveCombatSidebar(float w,float h)
        {
            LoadStatsPrefs();
            if(Event.current.button==0&&HandleStatsPointer(Event.current.type,Pointer,w,h))Event.current.Use();
            var original=GUI.matrix;float density=StatsDensity(w);GUI.matrix=original*Matrix4x4.Scale(new Vector3(density,density,1));
            try { DrawLiveStats(w,h,density); } finally {GUI.matrix=original;}
        }
        void DrawLiveStats(float w,float h,float density)
        {
            var layout=LiveStatsRect(w,h);var r=new Rect(layout.x/density,layout.y/density,layout.width/density,layout.height/density);var header=new Rect(r.x,r.y,r.width,28);
            Plate(header,ink,liveStatsOpen?Accent:muted,6);
            Label(header,(liveStatsOpen?"−  ":"+  ")+"LIVE STATS  [ I ]",12,liveStatsOpen?Accent:white,true,TextAnchor.MiddleCenter);
            if(!liveStatsOpen)return;
            Panel(new Rect(r.x,r.y+32,r.width,r.height-32));
            Label(new Rect(r.x+12,r.y+36,r.width-24,18),"THIS RUN / SEQUENCE ORDER",10,muted,true);
            string[] names={"KILLS","DAMAGE","ASSISTS","HEAL"};
            for(int i=0;i<4;i++)
            {
                var box=StatsMetricRect(r,i);bool selected=(liveStatsMask&(1<<i))!=0;
                Plate(box,selected?new Color(.04f,.23f,.25f,.94f):ink,selected?Accent:line,4);
                Label(box,(selected?"✓ ":"□ ")+names[i],10,selected?Accent:muted,true,TextAnchor.MiddleCenter);
            }
            var rows=game.LiveSequenceStats();var other=game.LiveOtherStats(rows);if(other.damage+other.kills+other.healing+other.assists>0)rows.Add(other);
            var view=new Rect(r.x+8,r.y+87,r.width-16,Mathf.Max(35,r.height-153));
            liveStatsScroll=GUI.BeginScrollView(view,liveStatsScroll,new Rect(0,0,view.width-19,rows.Count*40));
            float rowWidth=view.width-21,y=0;
            foreach(var row in rows)
            {
                var tint=row.item==null?muted:RarityColor(row.item.rank);
                Label(new Rect(3,y,34,18),row.positions,11,Accent,true);
                if(row.item!=null)ActionGlyph(new Rect(36,y,20,20),row.item.def,tint);
                Label(new Rect(row.item==null?38:62,y,rowWidth-(row.item==null?38:62),19),row.label,12,white,true);
                DrawStatValues(new Rect(4,y+20,rowWidth-8,17),row,10);
                Stroke(new Vector2(4,y+38),new Vector2(rowWidth,y+38),line,1);y+=40;
            }
            GUI.EndScrollView();
            Label(new Rect(r.x+12,r.yMax-61,286,16),"RUN TOTAL  ·  "+game.EnvironmentalKills+" environment kills",10,Accent,true);
            DrawStatValues(new Rect(r.x+12,r.yMax-42,286,17),game.LiveTotals(),12);
            Label(new Rect(r.x+12,r.yMax-21,286,16),liveStatsMask==0?"Choose metrics above to show values.":"Damage includes impacts · assists count setups",9,muted);
        }
        void DrawStatValues(Rect r,VectorEncounter.LiveStatRow row,int size)
        {
            string[] values={row.kills.ToString("N0")+" kills",row.damage.ToString("N0")+" dmg",row.assists.ToString("N0")+" ast",row.healing.ToString("0.#")+" heal"};
            int count=Enumerable.Range(0,4).Count(i=>(liveStatsMask&(1<<i))!=0);if(count==0)return;
            int column=0;for(int i=0;i<4;i++)if((liveStatsMask&(1<<i))!=0){Label(new Rect(r.x+column*r.width/count,r.y,r.width/count-3,r.height),values[i],size,i==3?new Color(.55f,1,.65f):white,true,TextAnchor.MiddleRight);column++;}
        }
    }
}
