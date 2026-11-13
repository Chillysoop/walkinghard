# Physical road forks verification

November 13, 2026. This corrects the initial implementation in `84d6abeb3587858fbe304cb7966f1cf571b6c5e7`: the requested feature is two roads going different directions with their own walls, not two lanes that rejoin.

## Delivered behavior

During waves 2 and 5, the incoming corridor ends at a Y-junction. Two roads turn 32 degrees left and right, with separate physical floors, walls and covered enemy entrances. The branches do not reconnect. The second junction extends the selected road, so two same-direction choices accumulate a 64-degree heading change.

The junction is prepared 16 meters ahead at the start of the wave. The six-combat-second decision starts when the Walker approaches within six seconds of maximum walking distance plus one meter. Walking and combat continue. A/D, arrows or a click selects **Shields / More gear** or **Volatiles / More abilities**. The highlighted left road is the no-input default; selections can change until lock. A checkpoint waits for an unresolved fork rather than cutting its decision short. The original encounter bias, two emphasized mixed offers, reward counts, rarity odds and rerolls are retained.

The camera pulls back and widens to reveal both exits, turns with the chosen road and returns to ordinary framing. The original fast-play cap, pause behavior and reduced-motion setting remain supported. Road selection now controls the actual walking centerline, camera heading, entrance positions, enemy navigation, corpse-shove direction, rear boundary and Pursuer approach. Melee and ranged enemies both require an unobstructed world line before attacking through the new walls.

Existing actors and corpses remain in world space; the abandoned roads are retained. A dark service-ground foundation supports bodies outside road surfaces. The original straight corridor is physically and visually ended at the split. Restart removes the branch tree and restores the original corridor geometry. Practice, tutorial and controlled fixed-loadout runs retain their straight layouts.

## Verification

- **45 native assertions** passed, including both directions, a second junction, accumulated road heading, real wall separation, independent physical floors, removal of the straight-ahead corridor, camera following, route rewards, speed restoration, pause, timeout, restart and the Pursuer on a rotated road.
- **761 production-effect assertions** passed in the native review and every core/progression scenario.
- All **15 core regression scenarios** passed, with **zero visible enemy spawns and zero sequence-order errors**.
- The **eight-wave progression scenario** passed with **13 item choices and two augment choices**, zero visible spawns and zero sequence-order errors.
- Native screenshots were inspected at **1920x1080** and **1280x720**, including reduced motion. The first visual pass exposed crossing guide lines and insufficient view of the exits; the final build removes crossing junction markings and widens the camera view.

The rendered fixtures use staged durable targets, elevated health and extended wave time so they can exercise long road traversal. Regression scenarios use 5,000 starting health. These establish functionality, not ordinary difficulty. The pointer checks invoke the production HUD handler; they do not inject operating-system mouse events.

Evidence: [native report](physical-forks/report.json), [regression summary](physical-forks/regressions.json), [source/binary provenance](physical-forks/provenance.json). Ten native captures are retained under `QA/physical-forks`.

Six ordinary-health archetype seeds (83007–83012) were also run in each of Full PhysX and Fast modes. All **12 games were valid**, with zero timeouts, missing games or duplicate seeds. Full completed five of six runs; Human Pinball was defeated in wave eight. Fast completed three of six. These small samples establish functional completion and document outcomes, not comparable difficulty between models or equal archetype strength. No item tuning was changed from these results. [Per-seed results and raw-output hashes](physical-forks/ordinary-health.json) are retained; full local batches are in `Build/Physical-forks-evidence/ordinary-full` and `ordinary-fast`. Reproduce with `Run Simulations.ps1 -Runs 6 -Workers 1 -Seed 83007 -Policy archetypes -Mode Full -MaxCombatSeconds 600`, supplying the player and a fresh output directory; use `-Mode Fast` for the screening batch. The concurrent verification runs were not frame-time measurements.

## Reproduction and limits

Build with Unity 6000.3.27f1 and `Build Windows.ps1 -OutputDirectory Build/Physical-forks`. Play an ordinary run to encounter the junctions. Run the native review with `-walkingForkReview -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -reviewOutput <fresh directory>`.

Regression commands are `Validate Prototype.ps1 -Player <player> -ReportPrefix physical-forks-core -HeroHealth 5000` and `Validate Variety.ps1 -Player <player> -ReportPrefix physical-forks-progression -Builds full -FullRun -HeroHealth 5000`.

Actual walls, entrance placement and camera framing change combat positioning and visibility. This is not an assertion of unchanged difficulty or bit-identical PhysX outcomes. The fork correction does not implement the complete act destination graph, services, events, hub progression or save/resume. No release or launcher update was published.

Tested player: `Build/Physical-forks/WalkingHard.exe`. Assembly-CSharp SHA256: `c0ef3bfa128991565db6b25dbc577d9f65ff88a2d6aa36ba1f553fd8f34ee0bd`. Intermediate captures and individual raw regression reports remain under ignored `Build/Physical-forks-evidence`.
