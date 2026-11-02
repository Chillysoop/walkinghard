using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        int waveEntryBeat;
        static readonly int[][] EntryPatterns={
            new[]{2,2,0,2,2,1},new[]{0,2,1,2},new[]{2,2,0,1},new[]{0,1,0,1,2},
            new[]{2,2,2,0,1},new[]{0,0,1,1,2},new[]{2,0,1,2,1,0},new[]{2}};
        int NextFormationEntry()
        {
            int ordinal=entryTurn++;
            if(!BalanceOverrides.EncounterRhythm)return ordinal%3;
            var pattern=EntryPatterns[Mathf.Clamp(Wave-1,0,7)];return pattern[ordinal%pattern.Length];
        }
        float FormationInterval()
        {
            if(!BalanceOverrides.EncounterRhythm||Wave<4||Wave>=8)return 1;
            // Three shorter gaps and one breath: mean interval remains exactly one.
            return ++waveEntryBeat%4==0?1.75f:.75f;
        }
    }
}
