# Walking Hard 0.7.1 — Pursuer finale verification

The Walker stops after the wave-seven checkpoint, turns, and the camera slowly orbits behind him. A giant is already lumbering toward him with an army behind it. The giant is staged out of sight immediately before the scene, advances throughout the reveal, and preserves its stride phase into combat. The Walker holds position. All finale reinforcements arrive from the distant horizon and flow around the giant; none enter from side doors. Killing the giant ends the run.

## Tested build

- Unity 6000.3.27f1, Windows x64, version 0.7.1.
- Shipping player: `Build/Pursuer-v071-pursuit/WalkingHard.exe`.
- Shipping `Assembly-CSharp.dll` SHA-256: `ac218049204d652ccad915c461e744f68ec2c1f0f601009571ea532c66287c01`.
- The 1,000-game gameplay candidate used SHA-256 `591bc8ee1a4ba65ee9ed6a41a25f780a29eb0e377c018dcb56c31ca862a7a268`. Subsequent changes widened the boss-health label, preserved the reveal stride into combat, and added two assertions. All gameplay fields in 64 matched Fast games are identical, excluding wall-clock runtime.

## Verification

- Fifteen core fixtures pass on the shipping player: sequence seeds 1741/4319, sword, hammer, rifle, twin, tempest, manual ultimate, reroll, half/fast clocks, armor, quick, armed and reload. Reports: `v071-pursuit-core-*.json`.
- Full eight-wave physics runs pass for full, rifle, hammer and pinball loadouts on the gameplay candidate. Reports: `v071-final-{full,rifle,hammer,pinball}-1741.json`. The full loadout also passes on the shipping player: `v071-pursuit-full-full-1741.json`, 314 kills, 144 collisions, 13 item picks, two augment picks and 4,989.34 remaining integrity.
- These structural fixtures use 5,000 integrity to exercise progression; they are not balance estimates. The 1,000 ordinary games below use normal health.
- 528 production assertions pass on the shipping player, including concealed giant creation, the army starting behind it, movement before the reveal, continuous stride handoff, held position, frozen combat/recharge clocks, pause/resume, single boss activation, full-physics movement against floor friction, melee reach, head framing, horizon-only reinforcement, flow around the giant, and reversed rear-boundary behavior.
- Native D3D11 presentation run exits successfully with `FINALE_REVIEW_SUCCESS`. Six screenshots cover stopping, turning, orbiting, reveal, approach and melee contact. The final boss-health percentage fits on one line.
- Launcher checks pass: valid installation, checksum rejection, failed-build rollback, archive containment and offline fallback.

The initial Full physics check caught the slow giant motor sticking against floor friction; its acceleration now overcomes that friction, and a dedicated real-physics movement assertion protects the fix. The camera was raised and moved back to keep the giant's head visible at fist range. No item tuning was changed.

## Ordinary Fast games

1,000 games, seeds 1–1000, six mixed strategies, four workers, eight waves, 480-second combat cap. Runtime: 131.85 seconds while other checks were running. Results: **430 complete, 570 defeated, zero timed out, zero invalid**. Five hundred reached the finale; 430 of those won (86%).

| Strategy | Games | Wins | Win rate |
| --- | ---: | ---: | ---: |
| Balanced | 167 | 131 | 78.44% |
| Gun | 166 | 43 | 25.90% |
| Pinball | 167 | 69 | 41.32% |
| Random | 166 | 43 | 25.90% |
| Sever | 167 | 70 | 41.92% |
| Storm | 167 | 74 | 44.31% |

All **5,161 wave-one-through-seven records** match the same seeds and strategies in `v070-routes-1000`, including health, damage, choices and routes. Wave eight is intentionally a different encounter and should not be treated as a matched boss balance comparison. Fast uses simplified physics; the separate Full and native checks establish locomotion, collisions and presentation. The 45-case item-variety suite was not repeated because this update adds no items or augments.

Raw batches remain locally in ignored `QA/Simulations/v071-final-1000` and `v071-pursuit-regression`. Compact evidence and batch provenance are in `v071-verification.json`.

## Reproduce

```powershell
& '.\Build Windows.ps1' -OutputDirectory '.\Build\Pursuer-v071-pursuit'
& '.\Validate Prototype.ps1' -Player '.\Build\Pursuer-v071-pursuit\WalkingHard.exe' -ReportPrefix v071-pursuit-core -HeroHealth 5000
& '.\Validate Variety.ps1' -Player '.\Build\Pursuer-v071-pursuit\WalkingHard.exe' -ReportPrefix v071-pursuit-full -Builds full -FullRun -HeroHealth 5000
& '.\Run Simulations.ps1' -Player '.\Build\Pursuer-v071-pursuit\WalkingHard.exe' -Runs 1000 -Workers 4 -Mode Fast -Seed 1 -OutputDirectory '.\QA\Simulations\v071-rerun'
```

For native capture, launch the player with `-vectorQA -qaBuild ui -qaHealth 5000 -qaHoldCheckpoint -presentationReview -finaleReview -force-d3d11`. It captures six frames under `PresentationReview/` beside the player and exits. Use a visible window; hidden Unity captures on this machine are black.
