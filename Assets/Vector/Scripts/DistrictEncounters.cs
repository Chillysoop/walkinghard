using System.Linq;
using UnityEngine;

namespace VectorGame
{
    // Existing roles and route rewards, arranged into distinct district rhythms.
    public static class DistrictEncounters
    {
        public static readonly EncounterRoute[][] Rosters={
            new[]{EncounterRoute.RepairLine,EncounterRoute.FreightYard,EncounterRoute.FreightYard,EncounterRoute.ShieldLine,EncounterRoute.VolatileBreach,EncounterRoute.EliteShift},
            new[]{EncounterRoute.ShieldLine,EncounterRoute.Reclaimers,EncounterRoute.Reclaimers,EncounterRoute.RepairLine,EncounterRoute.VolatileBreach,EncounterRoute.EliteShift},
            new[]{EncounterRoute.VolatileBreach,EncounterRoute.Reclaimers,EncounterRoute.VolatileBreach,EncounterRoute.ShieldLine,EncounterRoute.FreightYard,EncounterRoute.EliteShift}};
        static readonly float[][] Gaps={new[]{.8f,.8f,.8f,1.6f},new[]{.6f,.6f,.6f,2.2f},new[]{.65f,1.35f,.65f,1.35f}};
        public static float Gap(int district,int beat)=>Gaps[Mathf.Clamp(district,0,2)][beat%4];
        public static string Cue(int district)=>district==0?"FREIGHT PACKS · repair links + body throws":district==1?"RALLY GROUPS · interrupt the gold links":"CROSS TRAFFIC · ranged fire + rear doors";
        public static CrewRole Specialist(int wave,int ordinal)
        {
            if(wave<=3)return CrewRole.Linebreaker;
            if(wave<=7)return ordinal%3==0?CrewRole.Linebreaker:CrewRole.Cinderlobber;
            if(wave<=14)return ordinal%3==0?CrewRole.Linebreaker:CrewRole.Bellkeeper;
            return ordinal%4==0?CrewRole.Bellkeeper:ordinal%2==0?CrewRole.Linebreaker:CrewRole.Cinderlobber;
        }
    }
    public sealed partial class VectorEncounter
    {
        int DistrictEntry(int entry,int ordinal)=>JourneyActive&&District==2&&!ChapterEncounter.boss&&ordinal%4==3&&!ForkAwaitingChoice?3:entry;
        float CurrentShooterChance=>JourneyActive&&!ChapterEncounter.boss?(enemies.Count(e=>e.shooter)>=4?0:District==0?.06f:District==1?.09f:.18f):ShooterChance(Wave);
        EntryLayout.Door RearDistrictDoor()
        {
            int side=entryTurn%2==0?-1:1;
            // Rear flanks use the closest covered room, not the ordinary forward
            // entrance selector's three-metre lead. Deeper doors take too long to
            // circle back into the visible fight behind a walking target.
            return EntryLayout.Doors.Where(d=>d.side==side&&roads.Path.Contains(d.road)&&d.localZ<d.road.End-3)
                .Where(d=>{float gap=Vector3.Dot(d.exit-HeroPosition,RoadForward);return gap<0&&gap> -3&&Vector3.Distance(d.exit,HeroPosition)<10;})
                .Where(d=>EntryLayout.PassageClear(d)&&EntryLayout.Concealed(cameraRig,d.origin))
                .OrderBy(d=>(d.exit-HeroPosition).sqrMagnitude).FirstOrDefault();
        }
    }
}
