using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public enum JourneyTimeKind { None, Combat, Equipment, Event, Paused, Away }
    [Serializable] public sealed class JourneyPressure
    {
        public int wave,carryIn,carryOut,peakAlive,peakNear,arrivals,damageBreaks,controlBreaks,escortBreaks;
        public string formation;
        public float seconds,targetWait,arrivalWait,blockedWait,cleanup,damage,burst5;
    }
    [Serializable] public sealed class JourneyPacingLog
    {
        public bool complete=true,automated;
        public double combat,equipment,events,paused,away,roads,accelerated;
        public List<JourneyPressure> encounters=new List<JourneyPressure>();
        public double Active=>combat+equipment+events;
        // Road selection overlaps combat. Paused/away time never enters Active.
        public void Record(double seconds,JourneyTimeKind kind,bool road,float speed)
        {
            if(automated||!double.IsFinite(seconds)||seconds<=0)return;
            switch(kind)
            {
                case JourneyTimeKind.Combat:combat+=seconds;if(road)roads+=seconds;if(speed>1.01f)accelerated+=seconds;break;
                case JourneyTimeKind.Equipment:equipment+=seconds;break;
                case JourneyTimeKind.Event:events+=seconds;break;
                case JourneyTimeKind.Paused:paused+=seconds;break;
                case JourneyTimeKind.Away:away+=seconds;break;
            }
        }
        public bool Valid()=>encounters!=null&&encounters.Count<=21&&
            new[]{combat,equipment,events,paused,away,roads,accelerated}.All(t=>double.IsFinite(t)&&t>=0)&&roads<=combat&&accelerated<=combat&&
            encounters.All(r=>r!=null&&r.wave>=1&&r.wave<=21&&new[]{r.carryIn,r.carryOut,r.peakAlive,r.peakNear,r.arrivals,r.damageBreaks,r.controlBreaks,r.escortBreaks}.All(n=>n>=0)&&new[]{r.seconds,r.targetWait,r.arrivalWait,r.blockedWait,r.cleanup,r.damage,r.burst5}.All(t=>float.IsFinite(t)&&t>=0));
    }
    public sealed partial class VectorEncounter
    {
        public JourneyPacingLog JourneyPacing {get;private set;}=new JourneyPacingLog();
        JourneyPressure pressureRow;
        readonly Queue<IncomingHit> pressureHits=new Queue<IncomingHit>();
        int pressureSpawnStart;
        double journeyClockAt;
        JourneyTimeKind journeyClockKind;
        bool journeyClockRoad,journeyFocused=true,journeySuspended;
        float journeyClockSpeed=1;
        // Comparison switches are used only by the isolated QA runners.
        bool originalBellThreshold,originalArrivalPacing;
        bool AutomatedJourney=>qa||simulation||!enabled||MechanicReview||CombatPreviewActive;
        JourneyTimeKind CurrentJourneyTime=>!JourneyActive||OfficeOpen||State==RunState.Ready||State==RunState.Complete||State==RunState.Defeated?JourneyTimeKind.None:
            !journeyFocused||journeySuspended?JourneyTimeKind.Away:State==RunState.Paused?JourneyTimeKind.Paused:
            State==RunState.Drafting?(StopOpen?JourneyTimeKind.Event:JourneyTimeKind.Equipment):JourneyTimeKind.Combat;
        void ResetJourneyPacing()
        {
            JourneyPacing=new JourneyPacingLog{automated=AutomatedJourney};pressureRow=null;pressureHits.Clear();
            journeyClockAt=Time.realtimeSinceStartupAsDouble;journeyClockKind=JourneyTimeKind.None;
        }
        void SampleJourneyClock()
        {
            double now=Time.realtimeSinceStartupAsDouble;
            if(!AutomatedJourney)JourneyPacing.Record(now-journeyClockAt,journeyClockKind,journeyClockRoad,journeyClockSpeed);
            journeyClockAt=now;journeyClockKind=CurrentJourneyTime;journeyClockRoad=ForkAwaitingChoice;journeyClockSpeed=EffectiveCombatSpeed;
        }
        void LateUpdate(){SampleJourneyClock();fx?.MixSound();}
        void OnApplicationFocus(bool focused){SampleJourneyClock();journeyFocused=focused;journeyClockKind=CurrentJourneyTime;}
        void OnApplicationPause(bool paused){SampleJourneyClock();journeySuspended=paused;journeyClockKind=CurrentJourneyTime;}
        JourneyPacingLog CaptureJourneyPacing(){SampleJourneyClock();return JsonUtility.FromJson<JourneyPacingLog>(JsonUtility.ToJson(JourneyPacing));}
        void RestoreJourneyPacing(JourneyPacingLog saved)
        {
            JourneyPacing=saved??new JourneyPacingLog{complete=false,automated=AutomatedJourney};
            journeyClockAt=Time.realtimeSinceStartupAsDouble;journeyClockKind=CurrentJourneyTime;
            journeyClockRoad=ForkAwaitingChoice;journeyClockSpeed=EffectiveCombatSpeed;pressureRow=null;pressureHits.Clear();
        }
        [Serializable] sealed class SavedJourneyTiming
        {public string version,result;public int seed,wave;public JourneyPacingLog pacing;}
        void SaveFinishedJourneyTiming()
        {
            if(!JourneyActive||AutomatedJourney)return;
            SampleJourneyClock();
            try{AtomicWrite(Path.Combine(SaveDirectory,"journey-timing-latest.json"),JsonUtility.ToJson(new SavedJourneyTiming{version=Application.version,result=State.ToString(),seed=Seed,wave=Wave,pacing=JourneyPacing},true));}
            catch(Exception e){Debug.LogWarning("JOURNEY_TIMING_SAVE: "+e.Message);}
        }
        string ActualFormation=>JourneyActive&&ChapterEncounter.boss?ChapterEncounter.name:JourneyActive&&RegionalWave?.id==EncounterRoute.EliteShift?ChampionTitle(SpawnChampionKind):RegionalWave?.name??RouteName(ActiveRoute);
        void BeginPressureEncounter()
        {
            if(!JourneyActive)return;
            pressureRow=new JourneyPressure{wave=Wave,formation=ActualFormation,carryIn=enemies.Count,peakAlive=enemies.Count};
            JourneyPacing.encounters.Add(pressureRow);pressureHits.Clear();pressureSpawnStart=Spawned;
        }
        void StepPressureEncounter(float dt)
        {
            if(pressureRow==null)return;
            pressureRow.seconds+=dt;if(CleaningUp)pressureRow.cleanup+=dt;
            pressureRow.peakNear=Math.Max(pressureRow.peakNear,CrowdNear);pressureRow.peakAlive=Math.Max(pressureRow.peakAlive,enemies.Count);
            pressureRow.carryOut=enemies.Count;pressureRow.arrivals=Spawned-pressureSpawnStart;
        }
        void RecordPressureWait(float dt,TargetWaitReason reason)
        {
            if(pressureRow==null)return;
            pressureRow.targetWait+=dt;if(reason==TargetWaitReason.NoEnemy)pressureRow.arrivalWait+=dt;
            else if(reason==TargetWaitReason.Obstructed)pressureRow.blockedWait+=dt;
        }
        void RecordPressureDamage(float amount)
        {
            if(pressureRow==null)return;
            pressureRow.damage+=amount;pressureHits.Enqueue(new IncomingHit{time=Elapsed,damage=amount});
            while(pressureHits.Count>0&&pressureHits.Peek().time<Elapsed-5)pressureHits.Dequeue();
            pressureRow.burst5=Mathf.Max(pressureRow.burst5,pressureHits.Sum(h=>h.damage));
        }
        void RecordBellBreak(string reason)
        {
            if(pressureRow==null)return;
            if(reason=="damage")pressureRow.damageBreaks++;else if(reason=="control")pressureRow.controlBreaks++;else pressureRow.escortBreaks++;
        }
        float BellDamageThreshold=>originalBellThreshold?.09f:.18f;
        float JourneyArrivalScale=>!JourneyActive||originalArrivalPacing||ChapterEncounter.boss?1:District==0?1:District==1?1.08f:1.16f;
    }
}
