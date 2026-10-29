# Prototype verification — October 28, 2026

Built successfully with Unity 6000.3.27f1 and URP 17.3.0 for Windows x64. Final player build is approximately 99 MB before compression. The full project, including Unity's generated Library cache, is larger.

The final executable passed two complete fixed-step simulation runs:

| Seed | Duration | Spawned | Kills | Peak nearby crowd | Combo finishers | Force pushes | Health remaining |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1741 | 60 s | 84 | 74 | 18 | 18 | 9 | 66.4 / 180 |
| 4319 | 60 s | 83 | 78 | 17 | 18 | 10 | 37.2 / 180 |

Both runs verified actor accounting, incoming damage, sufficient melee finishers, force-push targets, and encounter completion. Their JSON reports are saved alongside this document. The headless runtime logs contain no matching exception/error warnings from gameplay.

Native Windows inspection verified the start screen, elevated chase camera, visible free-moving crowd, combat, a completed 60-second encounter, replay, pause, and quit. The final build was also inspected after increasing HUD text and hero material brightness. Turning automatic push off, waiting for charge, and pressing F visibly produced the pulse and changed the HUD to Discharging. Automatic push was restored after this check. `force-push.png` records that test.

These are prototype checks, not broad hardware compatibility, formal animation evaluation, or final game-balance validation. One D3D12 diagnostic about querying the info queue appeared on this machine; the game rendered and completed normally.
