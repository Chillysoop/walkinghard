using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public struct ThreatReadout
        {
            public string title,state,protection;public Vector3 position;public Color color;
            public float health,maxHealth,ghost,guard,pressure;public bool damaged,champion,exposed,urgent;
        }
        public IEnumerable<ThreatReadout> ThreatReadouts()
        {
            foreach(var e in enemies)
            {
                if(e.boss||!InCombatView(e.rig.Root.position))continue;
                bool champion=e.durability?.champion==true,damaged=e.health<e.maxHealth-.01f;
                bool durable=e.heavy||e.crew==CrewRole.Linebreaker||e.crew==CrewRole.Bellkeeper||e.crew==CrewRole.Repair;
                float guard=e.crewGuardUntil>Elapsed?e.crewGuard:0;
                if(e.crew==CrewRole.None&&!champion&&!(durable&&damaged)&&!e.shieldBearer&&guard<=0&&!(e.durability?.guardBrokenUntil>Elapsed))continue;
                bool exposed=champion?!ChampionBraced(e):e.shieldBearer&&e.shieldOpenUntil>Elapsed;
                string state=e.crewNotice>Elapsed?"INTERRUPTED":e.crewClock>0?(e.crew==CrewRole.Linebreaker?e.specialist?.stage==2?"CHARGING":"CHARGE":e.crew==CrewRole.Cinderlobber?"BLAST":e.crew==CrewRole.Bellkeeper?"RALLY":e.crew==CrewRole.Repair?"REPAIR":e.crew==CrewRole.Freight?"THROW":"RETRIEVE")+" · "+Mathf.Max(0,CrewDuration(e.crew)-e.crewClock).ToString("0.0")+"s":e.durability?.guardBrokenUntil>Elapsed?"PATCH BROKEN":"";
                string protection=champion?exposed?"EXPOSED +25% · "+Mathf.Max(0,e.durability.exposedUntil-Elapsed).ToString("0.0")+"s":"BRACED · LAUNCH TO BREAK":e.shieldBearer?exposed?"GUARD OPEN":"FRONT GUARD · 50%":"";
                yield return new ThreatReadout{title=champion?"SHIFT CHAMPION":e.crew!=CrewRole.None?CrewName(e.crew):e.shieldBearer?"SHIELD BEARER":"ENFORCER",position=e.rig.Root.position+Vector3.up*(champion?4:IsSpecialist(e.crew)?3.5f:2.8f),color=champion?new Color(1,.78f,.25f):e.crew!=CrewRole.None?CrewColor(e.crew):new Color(1,.66f,.35f),health=e.health,maxHealth=e.maxHealth,damaged=damaged,champion=champion,exposed=exposed,state=state,protection=protection,guard=guard,pressure=e.durability?.pressure??0,ghost=e.durability?.hitUntil>Elapsed?e.durability.healthBeforeHit:e.health,urgent=champion||e.crewClock>0||e.crewNotice>Elapsed};
            }
        }
    }
    public sealed partial class PrototypeHUD
    {
        void EnemyThreatLabels(float w,float h)
        {
            var original=GUI.matrix;float scale=Mathf.Max(.9f,original.m00);GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            try {DrawThreatLabels(Screen.width/scale,Screen.height/scale,w,h);}finally{GUI.matrix=original;}
        }
        void DrawThreatLabels(float w,float h,float hudWidth,float hudHeight)
        {
            var occupied=new List<Rect>();
            if(liveStatsOpen){var stats=LiveStatsRect(hudWidth,hudHeight);float factor=w/hudWidth;occupied.Add(new Rect(stats.x*factor,stats.y*factor,stats.width*factor,stats.height*factor));}
            if(game.ForemanPresent||game.JourneyBossPanelVisible)occupied.Add(game.CombatPreviewActive?new Rect(w-330,330,310,132):new Rect(w/2-250,18,500,130));
            if(game.BossSilhouetteViewport is Rect bossArea)occupied.Add(new Rect(bossArea.x*w-8,bossArea.y*h-8,bossArea.width*w+16,bossArea.height*h+16));
            if(game.ShowForkSigns){occupied.Add(new Rect(w/2-215,22,430,66));for(int side=0;side<2;side++){var choice=ForkChoiceRect(side,hudWidth,hudHeight);float factor=w/hudWidth;occupied.Add(new Rect(choice.x*factor,choice.y*factor,choice.width*factor,choice.height*factor));}}
            int count=0;
            foreach(var view in game.ThreatReadouts().OrderByDescending(v=>v.champion).ThenByDescending(v=>v.urgent).ThenByDescending(v=>v.damaged))
            {
                if(count++>=8)break;var p=game.CrewViewport(view.position);if(p.z<=0)continue;
                bool health=view.damaged||view.champion;float height=24+(health?18:0)+(view.state.Length>0?18:0)+(view.protection.Length>0?18:0)+(view.guard>0?17:0)+(view.champion&&!view.exposed?12:0);
                float x=Mathf.Clamp(p.x*w,350,w-350),y=Mathf.Clamp((1-p.y)*h,120,h-245);
                var rect=new Rect(x-96,y-height,192,height);var origin=rect.position;bool fits=false;
                foreach(var offset in new[]{Vector2.zero,new Vector2(0,-80),new Vector2(0,80),new Vector2(-198,0),new Vector2(198,0),new Vector2(-198,-80),new Vector2(198,-80),new Vector2(0,-160),new Vector2(0,160),new Vector2(-396,0),new Vector2(396,0)})
                {rect.position=new Vector2(Mathf.Clamp(origin.x+offset.x,330,w-530),Mathf.Clamp(origin.y+offset.y,110,h-225-height));if(!occupied.Any(r=>r.Overlaps(rect))){fits=true;break;}}
                if(!fits&&!view.champion)continue;
                occupied.Add(new Rect(rect.x-3,rect.y-3,rect.width+6,rect.height+6));
                var color=view.exposed?Accent:view.color;
                if((rect.position-origin).sqrMagnitude>16)Stroke(new Vector2(p.x*w,(1-p.y)*h),new Vector2(rect.center.x,rect.yMax),color,1);
                Plate(rect,new Color(.015f,.035f,.04f,.92f),color,5,view.champion?2:1);
                Label(new Rect(rect.x+5,rect.y+3,182,18),view.title,11,color,true,TextAnchor.MiddleCenter);float row=rect.y+23;
                if(health)
                {
                    var bar=new Rect(rect.x+8,row+4,112,6);Box(bar,new Color(.14f,.08f,.06f));Box(new Rect(bar.x,bar.y,bar.width*Mathf.Clamp01(view.ghost/view.maxHealth),bar.height),white);Box(new Rect(bar.x,bar.y,bar.width*Mathf.Clamp01(view.health/view.maxHealth),bar.height),new Color(.95f,.33f,.25f));
                    Label(new Rect(rect.x+122,row-1,62,15),Mathf.CeilToInt(view.health)+"/"+Mathf.CeilToInt(view.maxHealth),9,white,true,TextAnchor.MiddleRight);row+=18;
                }
                if(view.state.Length>0){Label(new Rect(rect.x+5,row-1,182,17),view.state,10,white,true,TextAnchor.MiddleCenter);row+=18;}
                if(view.protection.Length>0){Label(new Rect(rect.x+5,row-1,182,17),view.protection,9,color,true,TextAnchor.MiddleCenter);row+=18;}
                if(view.champion&&!view.exposed){Label(new Rect(rect.x+8,row-2,59,12),"PRESSURE",8,color,true);Segments(new Rect(rect.x+69,row,115,5),view.pressure,color,12);row+=12;}
                if(view.guard>0){Segments(new Rect(rect.x+8,row+3,70,5),view.guard/18,new Color(.45f,1,.65f),8);Label(new Rect(rect.x+83,row-2,101,15),"PATCH +"+view.guard.ToString("0"),9,new Color(.45f,1,.65f),true);}
            }
        }
    }
}
