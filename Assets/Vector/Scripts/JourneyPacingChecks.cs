using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckJourneyPacing(Action<bool,string> require)
        {
            var t=new JourneyPacingLog();
            t.Record(60,JourneyTimeKind.Combat,true,1);t.Record(30,JourneyTimeKind.Combat,false,2);
            t.Record(20,JourneyTimeKind.Equipment,false,1);t.Record(10,JourneyTimeKind.Event,false,1);
            t.Record(15,JourneyTimeKind.Paused,true,2);t.Record(25,JourneyTimeKind.Away,true,2);t.Record(100,JourneyTimeKind.None,false,1);
            require(t.Active==120&&t.combat==90&&t.equipment==20&&t.events==10,"actual time partitions combat and decisions without speed scaling");
            require(t.paused==15&&t.away==25&&t.roads==60&&t.accelerated==30,"road time overlaps combat; pauses and away never inflate active play");
            t.Record(double.NaN,JourneyTimeKind.Combat,false,1);t.Record(-1,JourneyTimeKind.Combat,false,1);
            require(t.Valid()&&t.Active==120,"invalid timer deltas are ignored");
            var automated=new JourneyPacingLog{automated=true};automated.Record(100,JourneyTimeKind.Equipment,false,1);
            require(automated.Active==0,"automated decisions cannot become human timing evidence");
            StartChapter();require(JourneyPacing.Active==0&&JourneyPacing.encounters.Count==0,"new journey clears timing and pressure");
            journeyFocused=true;journeySuspended=false;OfficeOpen=false;
            require(CurrentJourneyTime==JourneyTimeKind.Equipment,"opening draft counts as equipment decision time");
            Stop=new ExpeditionStop{kind=EncounterRoute.SalvageStop};require(CurrentJourneyTime==JourneyTimeKind.Event,"service stop counts as event time instead of equipment time");Stop=null;
            State=RunState.Running;require(CurrentJourneyTime==JourneyTimeKind.Combat,"walking and combat share one real-time bucket");
            State=RunState.Paused;require(CurrentJourneyTime==JourneyTimeKind.Paused,"paused game is excluded");
            journeyFocused=false;require(CurrentJourneyTime==JourneyTimeKind.Away,"lost focus takes priority over paused state");journeyFocused=true;
            State=RunState.Complete;require(CurrentJourneyTime==JourneyTimeKind.None,"results screen does not extend run time");
            State=RunState.Drafting;JourneyPacing=t;
            require(SaveCheckpoint(),"schema 10 timing checkpoint writes");var saved=CaptureCheckpoint();
            JourneyPacing.Record(7,JourneyTimeKind.Equipment,false,1);require(saved.pacing.Active==120,"timing snapshot is detached from the live clock");
            require(ResumeExpedition()&&JourneyPacing.Active==120&&JourneyPacing.roads==60,"resume restores saved timing without adding offline time");
            saved.schema=9;saved.pacing=null;AtomicWrite(SavePath,JsonUtility.ToJson(saved));
            require(ResumeExpedition()&&!JourneyPacing.complete&&JourneyPacing.Active==0,"schema 9 migration explicitly marks earlier timing unknown");
            saved=CaptureCheckpoint();saved.pacing.combat=-1;AtomicWrite(SavePath,JsonUtility.ToJson(saved));
            require(!ResumeExpedition()&&File.Exists(SavePath),"invalid timing rejects resume while preserving the save");
            StartChapter();Wave=16;BeginWave();
            require(pressureRow.formation==RegionalWave.name&&RunHistory.encounters.Last().route==RegionalWave.name,"encounters report their generated formation instead of STANDARD");
            Elapsed=0;RecordPressureDamage(10);Elapsed=3;RecordPressureDamage(20);Elapsed=9;RecordPressureDamage(25);
            require(pressureRow.burst5==30&&pressureRow.damage==55,"pressure log measures a rolling five-second damage burst");
            RecordPressureWait(2,TargetWaitReason.NoEnemy);RecordPressureWait(3,TargetWaitReason.OutOfReach);RecordPressureWait(1,TargetWaitReason.Obstructed);
            require(pressureRow.targetWait==6&&pressureRow.arrivalWait==2&&pressureRow.blockedWait==1,"unique sequence waits distinguish arrivals, reach and obstructions");
            var closed=pressureRow;FinishIntegrityEncounter(true);State=RunState.Drafting;RecordIncoming("Event fixture",5);
            require(closed.damage==55&&RunHistory.sources.Any(s=>s.name=="Event fixture"),"event damage remains in run sources without contaminating the completed combat pressure log");
        }
    }
}
