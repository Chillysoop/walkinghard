# Walking Hard 0.6.4 verification

29 October 2026. Approved scope: fewer ranged opponents, visible combat/repositioning, repeat-attacker targeting, an offscreen rear death boundary with a cushion, and 1,000-game early-wave screening before further tuning.

## Final player

`Build/EnemyPressure-v064-final/WalkingHard.exe`, Assembly-CSharp.dll SHA-256 `5bdc31723d600d0db655cd327dce7df482b071554cef44284556749427dccf30`.

The three matched 1,000-game Fast batches and exact binary provenance are in `v064-early-wave-summary.json` and `v064-balance-review.md`. Final gameplay screening used the preceding binary, `Build/RearBoundary-v064/WalkingHard.exe`, hash `782535c5caa78045514e2d3f5db71d23771a0008c7ddfa51607fc51d6d5c1b7d`. The only final-binary difference initializes/restores a random generator in the isolated Full QA fixture, which Fast mode does not execute. No gameplay change followed the final 1,000-game batch.

## Full-physics checks

Four standalone scenarios passed on the final player: `sequence`, `twin`, `manual`, and the four-wave `progression` scenario, all seed 1741. They cover normal ordering, independent paired targets, magazine delays, automatic/manual ultimates, the checkpoint-three unlock, collision accounting and persistent corpses. Each runs 81 production-effect assertions. The explicit `-qaHealth 5000` fixture allows mechanics coverage; ordinary play and all 3,000 Fast games use normal authored health.

New production probes verify:

- Nearest target without repeated damage, and after only one hit.
- Repeated actual damage takes priority; action range, pair exclusion and concealed entrances remain constraints.
- Old pressure expires.
- A visible shooter can attack; moving it below the camera cancels an imminent shot before damage.
- The enemy walks toward a forward flank, physically reenters view, then resumes normal movement.
- The rear boundary is offscreen and at least eight metres back.
- An enemy in the rear cushion survives, while one beyond the line dies exactly once.
- A boundary kill is credited to its pushing ability without inventing damage contribution.

Two intermediate development-fixture failures were corrected: teleporting the test Rigidbody without synchronizing its visible Transform, and invoking AI before the fixture's combat random generator was initialized. The final probes pass. These corrections did not change the Fast simulation path.

## Simulation integrity and limits

All three 1,000-game batches have zero invalid games, missing games or duplicate seeds. Final batch: 828 reached the first ultimate, 419 completed eight waves, two timed out, and 199 credited boundary kills occurred in 145 games. The final batch took 104.26 seconds. Matched comparison supports a combined-effect comparison of the approved fixes, not attribution to a single AI component.

Fast mode approximates movement, bodies and world occlusion. These results are not human win-rate forecasts. Full probes establish behavior and accounting, not statistical difficulty calibration. No new rendered UI was required; the hidden boundary and AI were exercised directly through engine geometry and PhysX.

Counterguard duration, Duelist ward, enemy health/damage and general spawn cadence recommendations are **not applied**. See `v064-balance-review.md` for the exact proposed next tests.

Launcher validation passed valid install, checksum rejection, failed-build rollback, archive containment and offline fallback. PowerShell scripts parse successfully.
