# Walking Hard 0.6.0 verification

Verified locally on Windows with Unity 6000.3.27f1 on 29 October 2026. These checks establish implementation behavior, not settled balance or human play quality.

Release player: `Build/Release-v060-final/WalkingHard.exe`. Assembly-CSharp.dll SHA-256: `c34693463eb27d86fcaac98afb45f68b09f8963116a206fdde759ec1dc097722`.

## Coverage and results

- 40 item definitions: 9 weapons, 10 abilities, 12 armor pieces, 5 rigs and 4 ultimates. Sixteen transferable modules cover eight ability and eight weapon augment types.
- 61 integration scenarios passed: 15 baseline checks, 45 variety scenarios and one complete eight-wave run. The individual `v060-*.json` reports contain results and item contributions. Each scenario also passes 56 production-mechanic probes.
- Lifecycle checks cover storage, swapping, module replacement, fusion recovery, one socket on shared starter fists, one socket on two-handed weapons, paired effect shares, distinct owned fist items and compatible drafts over 100 seeds.
- Production probes cover all 16 augment effects, Conductive consumption, barriers, actual reloads/Redline/Breach Load, shared next-basic payoffs, target caps, sever identity and bounded collision descendants.
- The eight-wave fixture completed with 304 credited kills, 95 collisions, no visible spawns or sequence errors, 13 item choices and 2 augment choices. It ended at 280 integrity with a strong predefined Rare build. That is a sustain/boss-cleanup tuning signal, not a representative player success rate.
- The baseline suite includes manual ultimates, rerolls, pause/speed checks, persistent ragdolls, held weapons during abilities, reload indicators, item-owned kills and three-copy rarity fusion.
- Local launcher checks passed valid installation, checksum rejection, failed-build rollback, archive containment and offline fallback. GitHub Actions repeats the launcher checks for the PR.

The 45 variety scenarios and full run used the final gameplay candidate. Subsequent changes addressed review capture, a Tempest test fixture, occupied-socket drop resolution, the tempo label and visual cue clarity. The baseline suite was rerun after the fixture correction. A six-game repeated-run smoke checked the subsequent candidate. Echo, Flurry and Crosscut were rerun after the last visual-only changes; the full 61-case suite was not redundantly rerun after those changes.

## Native visual verification

The game itself rendered 45 controlled mechanic fixtures through URP offscreen render requests. Each uses the normal 60 Hz movement and physics loop. Twelve captures per fixture show affected targets, state and movement; seven brief effects additionally received sixty captures at 0.1-second intervals. Motion traces record positions, velocities, health, stagger, action age and preparation. No computer-use tool was required for this capture pass.

The review covered the 16 augments, all armor and rig signatures, new weapon/ability identities and all ultimates. Examples include separate point/corridor gathering, living-only Backwash returns, a delayed absorption discharge, penetrating shots, reload-triggered bursts, fragment movement and controlled launches. The final readability pass strengthened sword sweep trails and placed distinct Echo/Flurry contact markers above the affected target; these markers appear only for actual follow-up contacts. Three additional dense fixtures checked the revised cues.

`v060-native-contactsheet.jpg` indexes the 45 fixtures. `v060-followup-cues.jpg` shows the revised contact/sweep sequences. `v060-mechanic-motion.jsonl` preserves the timed observations; a single still is not evidence of a complete motion. Earlier native HUD/checkpoint inspection informed the tile and socket layout. Offscreen camera captures exclude IMGUI: the final occupied-socket drop fix was checked against interaction code and ownership tests, but no new end-to-end mouse pass of the final checkpoint UI was performed. Full-speed, dense-combat readability remains a playtest target.

## Engine-only balance batches

`Run Simulations.ps1` launches persistent hidden Unity workers with actual combat and PhysX. Six simple drafting policies can be compared across unique seeds. `Summarize Simulations.ps1` exports per-game, policy and item contribution CSVs; `Compare Simulations.ps1` compares matched seeds and inputs. Development JSON overrides allow numeric tuning without rebuilding. See `SIMULATIONS.md` for commands and limitations.

Measured validation batches:

| Experiment | Result |
| --- | --- |
| 12 complete-run attempts, eight waves, two workers | 429.57 seconds; 1.68 games/minute; 6 wins and 6 defeats; no invalid, duplicate or missing games |
| Repeated three-wave smoke, six policies | Six valid games per batch; most recent pre-visual candidate took 21.99 seconds with two workers |
| Same-seed override check | Baseline won 2/2; deliberately higher enemy damage lost 2/2; confirms configuration affects combat |
| Same-seed replay | Both games matched every recorded gameplay field, excluding wall-clock duration, on this machine |

The full-run benchmark used an earlier candidate before the final rifle-target and visual refinements, and competed with other checks. It projects approximately 10 hours for 1,000 full eight-wave attempts under those conditions; short three-wave experiments are substantially faster. No 1,000-game study was run. The earlier benchmark had a 900-second combat limit; the shipped runner defaults to 480 seconds, configurable from 120 to 600. Timeouts remain valid non-wins and are reported separately.

`v060-simulation-summary.json` preserves input settings, assembly hashes, timing and uncertainty. `v060-balance-comparisons.csv` contains the matched-rarity fixture totals and module investment. These one-seed whole-encounter comparisons include an initial common wave and are not isolated DPS tests. Raw batch logs/results remain locally under ignored `QA/Simulations/`.

## Interpretation limits and next tuning work

- Policy and end-loadout item win rates are observational. Policies use heuristic picks, automatic ultimates, no rerolls and no exhaustive inventory search. They do not represent optimal or human play.
- Two matching item copies count as two equipped observations. Setup assists do not duplicate kills. Control records added stagger duration, not a causal measure of prevented attacks or realized utility.
- Same-seed replay is useful but cross-machine/run-history PhysX determinism is not guaranteed. Keep worker count and input settings fixed for comparisons.
- Physics, persistent bodies and long boss cleanup are significant CPU costs. Native cuts are prototype waist splits, not arbitrary mesh slicing.
- Sustain, time spent cleaning up the boss, discovery odds and paired versus separated reward investment remain tuning hypotheses. Compare larger matched batches before calling an archetype dominant.
- Numeric overrides change supported authored scales; they do not rewrite trigger rules, fixed caps or every gear stat. The greater-than-ten-second ultimate floor remains enforced.
