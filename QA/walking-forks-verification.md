# Moving road forks verification

**Superseded:** this report records the initial rejoining-lane implementation. The correction to literal, separately walled roads is documented in [physical-forks-verification.md](physical-forks-verification.md).

November 13, 2026. Development change on top of Cinderworks commit `ac2fd8401adcf6815c47f0fb8c40837c9080a3d2`.

## Delivered behavior

The route choices in waves 2 and 5 now appear during combat, ten combat seconds before arrivals stop. Two road lanes carry short signs: a shield with **Shields / More gear**, and an explosive symbol with **Volatiles / More abilities**. A/D, arrow keys or a click selects a side. A six-combat-second countdown locks the last selection; the initially highlighted left path is the advertised default. The selected road edge lights up immediately, and the Walker physically follows the chosen lane after locking.

The ten-second approach temporarily caps 2x/4x play at 1x without changing the preferred speed. Pause freezes the decision; 0.5x remains available, and hitstop affects combat time normally. Selection cannot be changed after lock, while paused or after defeat. Restart removes the lane objects and resets route state.

The normal reward screen still follows wave completion. The route biases two of its existing mixed offers and the next wave's special enemies, then expires. Reward counts remain 13 item choices and two augment choices; the first ultimate is still after wave 3. Reroll behavior, rarity odds, enemy arrival cadence, ordered actions, paired and two-handed rules and collision budgets are unchanged.

These are bypass lanes that rejoin the existing physical corridor. Flat road art adds no world colliders. Lateral movement does change combat positioning, so this is not a claim of identical combat outcomes. The first-act graph, event/service destinations and checkpoint save/resume remain future expansion work. Ordinary runs have the forks; tutorial, practice and controlled fixed-loadout experiments do not.

## Verification

- The rendered native review passed **30 assertions**, including combat and walking during selection, the production pointer handler, late selection changes, timeout defaults, pause, defeat, restart, both physical lane directions, preserved colliders, route rewards and speed restoration.
- **761 production-effect assertions** passed in the native review and in each regression run.
- All **15 core regression scenarios** passed with zero visible enemy spawns and zero sequence-order errors.
- The **eight-wave progression scenario** passed with all 13 item and two augment choices, zero visible spawns and zero sequence-order errors.
- Actual captures were inspected at **1920x1080** and **1280x720**, including reduced motion, lane selection, locking, following the lane and the temporary speed notice.

Native fixtures use elevated health and staged durable enemies to exercise the transition. Regression scenarios use 5,000 starting health. They establish functionality and presentation, not ordinary difficulty or fresh-player comprehension. The mouse fixture invokes the same production pointer handler as the HUD; it does not inject operating-system mouse events.

Reports: [native checks](walking-forks/report.json), [regressions](walking-forks/regressions.json), [source and binary provenance](walking-forks/provenance.json). Native screenshots are retained in `QA/walking-forks`.

## Ordinary-health screen

Six matched seeds, 83007 through 83012, were run against the preceding Cinderworks player and this player using Full PhysX, eight waves, archetype policies and a 600-combat-second limit. All **12 games were valid**, with **zero timeouts**, missing games or duplicate seeds. Baseline won four; this build won three. Five of six seed pairs retained the same win/loss result.

| Policy | Baseline | Moving forks |
| --- | --- | --- |
| Gun | Complete | Complete |
| Cut & Carry | Defeated, wave 5 | Defeated, wave 6 |
| Close Quarters | Complete | Defeated, wave 8 |
| Human Pinball | Defeated, wave 8 | Defeated, wave 8 |
| Storm | Complete | Complete |
| Sever | Complete | Complete |

This small sample catches functional failures and records an outcome change; it does not establish equal difficulty or isolate the effect of lateral movement from ordinary PhysX variation. No balance tuning was made from this screen. [Compact results](walking-forks/ordinary-health.json) retain per-seed outcomes, route-enemy counts and raw-output hashes; complete local batches remain under `Build/Walking-forks-evidence/baseline` and `candidate`. Reproduce with `Run Simulations.ps1 -Runs 6 -Workers 1 -Seed 83007 -Policy archetypes -Mode Full -MaxCombatSeconds 600`, supplying the relevant player and a fresh output directory. These concurrent runs were not frame-time measurements.

## Reproduction

Build with Unity 6000.3.27f1 using `Build Windows.ps1 -OutputDirectory Build/Walking-forks`. Play a normal run to see forks during waves 2 and 5. For rendered automated review, launch with `-walkingForkReview -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -reviewOutput <fresh output directory>`; it opens the native game and exits after writing captures and its report.

Run `Validate Prototype.ps1 -Player <player> -ReportPrefix walking-forks-core -HeroHealth 5000` and `Validate Variety.ps1 -Player <player> -ReportPrefix walking-forks-progression -Builds full -FullRun -HeroHealth 5000` for the regression checks.

Player: `Build/Walking-forks/WalkingHard.exe`. Assembly-CSharp SHA256: `5b37916d82333406c4a0c0edfa2e76203147740fbbc2a0290c02a75b966cb41f`. Individual regression reports and the first visual iteration are retained under ignored `Build/Walking-forks-evidence`. This build has not been published to the release channel or desktop launcher.
