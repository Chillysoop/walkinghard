# Walking Hard 0.3 verification

Built with Unity 6000.3.27f1, Windows x64, URP 17.3.0. Five standalone simulation runs passed after the sequence, physics, draft, and HUD changes. The final rebuild changes the Steel Jab effect label to “brief stagger”; combat code is unchanged.

| Path | Seed | Kills | Launches | Collision hits | Visible spawns | Order errors |
| --- | --- | --- | --- | --- | --- | --- |
| sequence | 1741 | 94 | 45 | 49 | 0 | 0 |
| sequence | 4319 | 103 | 0 | 0 | 0 | 0 |
| reverse | 1741 | 98 | 0 | 0 | 0 | 0 |
| manual | 1741 | 91 | 44 | 42 | 0 | 0 |
| reroll | 1741 | 99 | 44 | 44 | 0 | 0 |

All runs completed three waves, installed three abilities and one modifier, created real ragdolls and physics contacts, and used both side entrances and the horizon. Assertions cover empty/invalid confirmation, distinct draft offers, exhausted rerolls, manual single-step combat, stable actor accounting, and strict slot order. These results are not a deterministic benchmark: physical contacts and resulting combat outcomes can vary across runs/machines.

Launcher checks passed for valid install, SHA-256 rejection, incomplete-build rejection without changing the current installation, archive path containment, and offline fallback. Release download and real main-branch checks are additionally exercised during publication.

Workshop checks passed for actual pointer drag reordering, click fallback, storing an equipped action, automatic fusion and retained equipment position, weapon compatibility, sequence synergy links, seven-decision progression, local persistence, and an authenticated hosted save/read transaction. Test submissions are labeled `kind=test` and never loaded as user reviews.

Native Unity drag interaction was not fully inspected while the user was active on the desktop. The Unity player compiled and all simulation paths passed; manual feel, animation, and UI playtesting remain useful. The workshop's drag interaction was verified with actual pointer input.
