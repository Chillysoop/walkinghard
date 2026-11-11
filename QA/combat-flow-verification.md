# Combat-flow and usability pass

November 10, 2026. Development branch after v0.8.0, Unity 6000.3.27f1 / Windows D3D11. The playable candidate is `Build/Combat-flow-acceptance/WalkingHard.exe`. This is not a published release.

## Findings and changes

The previously reported seed 83012 **completed on the published v0.8.0 player** when reproduced, at 337.6 combat seconds. It is not evidence of a current universal lockup.

The isolated full-physics fixture did reproduce a narrower spacing problem. The Pursuer's old desired stop was 2.55m; floor friction settled it around 2.58m, just inside Common Steel Jab's 2.6m reach. Small displacements repeatedly broke contact. A desired stop of 2.4m settles around 2.43m, with clearance between the physical capsules. The hero remains planted. Boss attack timing, movement speed, susceptibility windows, launch forces and authored action ranges are unchanged.

| Controlled observation | v0.8.0 combat | Candidate |
| --- | ---: | ---: |
| Idle Pursuer settled distance | 2.58m | 2.43m |
| Common Steel Jab first contact | 7.02s | 1.72s |
| Common Steel Jab longest target wait | 8.53s | 3.17s |
| Common fists total target wait in 45s | 23.53s | 18.98s |
| Common Impact Hammer longest target wait | 7.18s | 7.18s |

These fixtures start the boss at 3.8m and use a durable target and elevated hero health. They measure contact access, not ordinary build strength. Hammer launch/recovery gaps remain meaningful. The recorder samples `ActionStatus`; a stale status can include up to the initial 0.15s recovery in some totals. The multi-second Steel Jab finding is unaffected. Reports and executable hashes are retained in `combat-flow/`.

A second problem was independent of the boss: with pistol first and scattergun second, an enemy at 8m let the pistol start the pair while the scattergun's 6m contact failed. The sequence consumed both turns. Reversing the hands waited instead. Paired turns now acquire a legal target for **each** hand before starting either. Both may hit one target, or retain independent targets and pressure priority. Tests cover reversed hands, the 5→1 boundary, ammunition, attribution, and clearing the wait. Separating the basics allows independent long-range fire; linking them commits to simultaneous contacts. Two-handed weapons still use one locked attack and one host/socket.

The combat bar now names the limiting action and distinguishes **OUT OF REACH**, **PATH BLOCKED**, and **AWAITING ENEMIES** after 0.35 seconds. A linked partner shows **WAITING FOR PAIR**. A visible out-of-range enemy gets a static distance marker and reach outline. There is no invented countdown, pulse or concealed-enemy marker. Cues clear immediately when the action starts and hide on pause. Reduced motion keeps the static cue.

Native captures: [paired range wait](combat-flow/02-paired-reach.png), [both contacts resume](combat-flow/04-pair-resumed.png), and [classified wave feedback](combat-flow/06-wave-feedback.png).

Recaps and checkpoint warnings distinguish actual range problems from empty/entering-enemy lulls and obstructions. The stored `rangeWait` remains total target-wait time for compatibility; the two new classification fields survive fusion and frozen wave snapshots. Only actual out-of-reach time recommends changing a build. The tooltip explains the paired-start tradeoff.

Final scheduler review also found that queued ultimates checked their base definition's range instead of their actual rarity-scaled range. Tempest repeated the hard-coded base range when selecting its first hit. Both now use the existing authored item range. Focused tests place a target beyond base range but inside Legendary reach for all five ultimates, verify actual damage or Redline activation, preserve the next sequence position, and reject that same extra reach for Common Tempest.

## Verification

- Fifteen core scenarios passed: strict order, pairing, two-handed grouping, delay indicators, held weapons, speed changes, hidden entries, bodies and attribution.
- The final production-mechanic suite passed **755 assertions**, covering physical settling, both mixed-gun orders, independent targets, wrapped pairs, two-handed starts, arrival/obstruction classification, paused/cleared indicators, fusion snapshots and all five ultimate reach boundaries. It had 739 assertions before the ultimate checks. An earlier build counted repeated per-frame wait assertions separately (797); consolidating those counts did not change the exercised behavior.
- The final native flow review passed 26 assertions, including seven spacing fixtures and six 1920×1080 captures. The reach, arrival, blocked-path, reduced-motion, resumed-pair and recap images were inspected.
- Rendered tutorial checks passed 1,375 assertions using production mouse handlers and existing resolution/layout checks.
- The final eight-wave progression fixture passed with 318 kills, 147 collisions, 13 item choices, 2 augment choices, and zero order or visible-spawn errors. Its 5,000 health is coverage, not balance evidence. The pre-ultimate-correction run also passed (318 kills, 166 collisions).
- Twelve matched ordinary Full-physics seeds per build used normal health, ordinary automatic drafts and all six policies (two seeds each). Both builds completed 7/12; every seed retained its win/loss outcome. There were no invalid rows, missing/duplicate seeds or timeouts. This small screen does not establish balance equivalence, and PhysX variation affects individual combat totals.

All 50 variety scenarios passed, including every paired, separated, shared and two-handed family layout. A native ordinary gun run also completed with 307 kills, normal health, 13 item and 2 augment choices, and 55 review assertions. These broader checks precede the final ultimate range correction. The final source repeated and passed the 15 core scenarios, eight-wave progression, and native flow checks. Its six ordinary Full runs (one per policy) retained the same four completions and two defeats as the corresponding baseline seeds, with no invalid games or timeouts. There are **82 recorded regression executions** across the initial suites and final repeats. `combat-flow/analyze.py` collects those acceptance results; `combat-flow/provenance.json` identifies each binary. Rendered tutorial and normal full-run images use the final presentation code.

## Limits and reproduction

This pass does not alter ordinary enemy orbit radii, auto-skip blocked actions, increase attack ranges, remove bodies, or rebalance damage. A target can still move out of reach after an action has committed; normal impact-time validation and retargeting remain in force. Actual player comprehension still needs a fresh-player playtest. These automated checks establish behavior and inspected presentation, not that a new player understands every build.

Run `Build Windows.ps1 -OutputDirectory <absolute-folder>`, then the core, variety and full-progression commands in `CONTRIBUTING.md`. Use `-combatFlowReview -flowOutput <absolute-folder> -force-d3d11 -screen-fullscreen 0` for the native fixtures. The original spacing recorder was run on v0.8.0 combat with only the diagnostic entry point added. Normal comparisons used `Run Simulations.ps1 -Runs 12 -Workers 2 -Seed 83001 -Policy archetypes -Mode Full -MaxCombatSeconds 600` on each binary with separate fresh output folders.

Other test processes ran concurrently. Frame samples from any native full-run review in this evidence are not quiet performance benchmarks; no performance improvement is claimed here.
