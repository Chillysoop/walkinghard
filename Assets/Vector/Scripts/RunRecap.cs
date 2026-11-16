using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] public sealed class IncomingSource {public string name;public float damage;public int hits;}
        [Serializable] public sealed class IncomingHit {public string source;public float time,damage,health;public int wave;}
        [Serializable] public sealed class IntegrityEncounter
        {
            public int wave,arrivedWith,survivors;public string route;public float start,lowest,end,damage;public bool finished;
        }
        [Serializable] public sealed class RunHistoryData
        {
            public bool complete=true;
            public List<IncomingSource> sources=new List<IncomingSource>();
            public List<IncomingHit> recent=new List<IncomingHit>();
            public List<IntegrityEncounter> encounters=new List<IntegrityEncounter>();
        }
        public RunHistoryData RunHistory {get;private set;}=new RunHistoryData();
        void BeginIntegrityEncounter()
        {
            RunHistory.encounters.Add(new IntegrityEncounter{wave=Wave,route=RouteName(ActiveRoute),start=Health,lowest=Health,end=Health,arrivedWith=enemies.Count});
        }
        void FinishIntegrityEncounter(bool finished)
        {
            var row=RunHistory.encounters.LastOrDefault(r=>r.wave==Wave);if(row==null)return;
            row.end=Mathf.Max(0,Health);row.lowest=Mathf.Min(row.lowest,row.end);row.finished=finished;row.survivors=enemies.Count;
        }
        static string IncomingName(Enemy e)=>e==null?"Environment":e.durability?.champion==true?"Freight Champion":e.boss?(e.journeyBoss?.kind==1?"Bell Foreman":e.foreman!=null?"Shift Foreman":"Pursuer"):e.crew!=CrewRole.None?e.crew==CrewRole.Freight?"Freight Brute":e.crew==CrewRole.Repair?"Repair Crew":e.crew.ToString():e.shooter?"Ranged Raider":e.heavy?"Enforcer":e.shieldBearer?"Shield Bearer":e.volatileCarrier?"Volatile Carrier":"Raider";
        void RecordIncoming(string source,float amount)
        {
            float actual=Mathf.Min(Mathf.Max(0,Health),Mathf.Max(0,amount));if(actual<=0)return;
            var total=RunHistory.sources.FirstOrDefault(r=>r.name==source);
            if(total==null){total=new IncomingSource{name=source};RunHistory.sources.Add(total);}total.damage+=actual;total.hits++;
            RunHistory.recent.Add(new IncomingHit{source=source,time=Elapsed,damage=actual,health=Mathf.Max(0,Health-actual),wave=Wave});
            if(RunHistory.recent.Count>24)RunHistory.recent.RemoveAt(0);
            var row=RunHistory.encounters.LastOrDefault(r=>r.wave==Wave&&!r.finished);
            if(row!=null){row.damage+=actual;row.end=Mathf.Max(0,Health-actual);row.lowest=Mathf.Min(row.lowest,row.end);}
        }
        public string FinalHitText
        {
            get
            {
                var last=RunHistory.recent.LastOrDefault();if(State!=RunState.Defeated||last==null)return RunHistory.complete?"INTEGRITY LOG · damage after protection; overkill excluded":"INTEGRITY LOG · recorded since this save was upgraded";
                float burst=RunHistory.recent.Where(r=>r.wave==last.wave&&r.time>=last.time-12).Sum(r=>r.damage);
                bool capped=RunHistory.recent.Count==24&&RunHistory.recent[0].time>=last.time-12;
                return "FINAL HIT · "+last.source+" · "+last.damage.ToString("0.#")+" integrity    /    LAST 12s · "+(capped?"at least ":"")+burst.ToString("0")+" lost";
            }
        }
    }
}
