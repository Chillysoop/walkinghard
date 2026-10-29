# Walking Hard 0.5.1 verification

Validated October 28, 2026 with Unity 6000.3.27f1. Windows player: `Build/Release-v051/WalkingHard.exe`.

Assembly-CSharp.dll SHA256: `b910bba097a14ab32bfbdba92c4aa4b4163247bc856329391c831e9bafb36959`.

## Gameplay regression checks

All fourteen standalone runs passed. Every simulation step checks that equipped models remain active, attached to the correct hand, correctly typed, and identical to the objects equipped at the start of the wave. A two-handed weapon has one model; fists have no separate weapon mesh. Basic attack effects are checked against the originating basic card's mount, including follow-up hits.

| Scenario | Seed | Weapon-check frames | Armed ability casts | Armed ultimate casts | Left-hand attacks | Result |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| armed | 1741 | 6843 | 24 | 6 | 10 | Pass |
| armor | 1741 | 6770 | 24 | 6 | 8 | Pass |
| fast | 1741 | 6803 | 24 | 6 | 9 | Pass |
| half | 1741 | 6785 | 22 | 6 | 8 | Pass |
| hammer | 1741 | 6835 | 29 | 6 | 0 | Pass |
| manual | 1741 | 6843 | 22 | 6 | 8 | Pass |
| quick | 1741 | 6843 | 24 | 6 | 8 | Pass |
| reroll | 1741 | 6843 | 21 | 6 | 8 | Pass |
| rifle | 1741 | 6843 | 35 | 6 | 0 | Pass |
| sequence | 1741 | 6810 | 24 | 6 | 8 | Pass |
| sequence | 4319 | 6843 | 21 | 6 | 7 | Pass |
| sword | 1741 | 6803 | 22 | 6 | 8 | Pass |
| tempest | 1741 | 6843 | 32 | 4 | 11 | Pass |
| twin | 1741 | 6843 | 35 | 6 | 0 | Pass |

The armed fixture equips a sword in the right hand and pistol in the left, moves the pistol basic before the sword basic, and runs Breaker, Mass Driver and Force Push. Other scenarios cover two-handed hammer/rifle, paired pistols, fists, both ultimates, and alternate draft outcomes. Fixtures deliberately control equipment for coverage; these are not guaranteed player rewards.

All scenarios completed three waves with zero order errors and zero visible spawns. Existing checks for loadouts, fusion, item kill ownership, direct equip, body accounting, walking shoves, scoring, speed clocks, armor effects, manual ultimate timing and hidden entrances passed. Weapon damage, preparation duration, action duration and ultimate scheduling were not changed. Physics outcomes can vary between runs.

## Native Windows visual check

- Inspected the checkpoint preview with sword and pistol equipped and the left-hand basic first in the sequence.
- Used Let's fight, observed Singularity Burst with both weapons still in hand, and selected 0.5x speed.
- Observed and captured Force Push with both weapons retained during its two-arm casting pose. The active Force Push tile is visible in `v051-armed-cast.png`.
- Confirmed both equipment models remain in the character preview at the following checkpoint.
- The screenshot uses the named armed QA fixture. Procedural poses remain prototype animation; this patch does not add authored hand IK.

## Build and distribution validation

Windows build, build/validation/publisher PowerShell syntax, and launcher safety tests passed. Launcher coverage includes valid install, checksum rejection, failed-build rollback, archive containment and offline fallback. Previous checkpoint/fusion UI coverage remains documented in `v05-verification.md`.
