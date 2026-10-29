# Walking Hard 0.6.4 — early-wave balance screening

29 October 2026. Three matched batches of 1,000 eight-wave Fast simulations, seeds 12000–12999, four workers, six mixed drafting policies, 600-second combat budget, normal player health. All batches have 1,000 valid, unique games with no missing games. The final batch includes the new ultimate progression and every requested enemy change.

## Results

| Measure | Released 0.6.3 | Ranged / targeting fixes | Plus rear death boundary |
| --- | ---: | ---: | ---: |
| Wall seconds | 84.5 | 103.51 | 104.26 |
| Reached checkpoint-three ultimate | 585 | 828 | 828 |
| Won all eight waves | 251 | 418 | 419 |
| Timed out | 2 | 2 | 2 |
| Deaths during wave 1 | 0 | 0 | 0 |
| Deaths during wave 2 | 18 | 10 | 10 |
| Deaths during wave 3 | 397 | 162 | 162 |

Before the fixes, wave three killed 397 of the 982 games reaching it (40.43%). After the fixes, it killed 162 of 990 (16.36%). Survival through all three opening waves rose from 58.5% to 82.8%; the rear boundary did not change this early survival result. Wave three combines the first heavy enemies (28% of eligible spawns), bonus entry packs and a faster spawn cadence before the first ultimate.

The rear boundary produced 199 kills across 145 games, at most five per game. Every observed boundary kill retained item attribution; there were zero unattributed environmental kills. It changed eight-wave wins from 418 to 419 in the matched sample. This supports keeping the generous cushion; it is a cleanup/situational knockback payoff rather than a broad early-wave rescue.

## Strategy spread after the fixes

| Policy | Games | Reached ultimate checkpoint | Eight-wave wins |
| --- | ---: | ---: | ---: |
| balanced | 167 | 163 (97.6%) | 143 (85.6%) |
| gun | 166 | 118 (71.1%) | 40 (24.1%) |
| pinball | 167 | 148 (88.6%) | 82 (49.1%) |
| random | 167 | 118 (70.7%) | 44 (26.3%) |
| sever | 166 | 137 (82.5%) | 39 (23.5%) |
| storm | 167 | 144 (86.2%) | 71 (42.5%) |

Counterguard present in the wave-two sequence is associated with 331/331 reaching the ultimate checkpoint and 316/331 winning the run. Without it in that early sequence, 497/669 reached the checkpoint and 103/669 won. This is a strong follow-up signal, not a randomized item comparison: policy choices, supporting items and later acquisitions differ.

Common equipped Duelist Jacket copies absorbed a mean 10.62 damage (69 copies), compared with 68.67 for Reactive Shell (56) and 91.22 for Bulwark Plate (74). These are accumulated contributions, not rates normalized by equipped time. They support testing alternate defense, not claiming a precise strength ratio.

## Recommendations — not applied

1. **Keep general early-wave health, damage and spawn cadence unchanged for now.** The approved positioning/targeting fixes already brought mixed-policy survival to the first ultimate above 80%. Keep the rear boundary cushion at three metres past the camera floor edge and at least eight metres behind the walker.
2. **Next isolated test: Counterguard base duration 3.5 → 2.5 seconds.** Preserve its 25 Common capacity, rarity multipliers, 80% conversion and existing caps. The prior capacity reduction barely changed absorption; shorter uptime is the clearer hypothesis. Run matched seeds and examine opening-wave losses before deciding to ship.
3. **Separate alternate-defense test: Duelist Jacket ward 1 hit / 4 seconds → 2 hits / 6 seconds.** Keep 45% Common mitigation and its cap. Stagger only on the first mitigated hit; refreshing replaces charges. This gives reload/sever builds a more credible defensive path. Test separately from Counterguard so effects remain attributable.
4. **Hold gun damage and global difficulty changes.** Fast physics previously understated gun, Storm and collision builds. Their ranking here does not justify a numerical buff without full-physics comparisons.

No recommended Counterguard, Duelist, enemy-health, enemy-damage or cadence changes are included in 0.6.4. Only the explicitly requested ranged-frequency, visibility, targeting and rear-boundary changes are implemented.

## Limits and provenance

Fast mode runs production combat, rewards and scheduling but approximates movement/body collisions. Earlier calibration agreed with Full outcomes in 23/32 cases and understated some physical builds. These are screening-policy results, not expected human win rates. The matched seed sets control inputs, but the combined fixes change combat history and later drafts; they do not isolate the contribution of each AI change.

Raw local batches: `QA/Simulations/v064-ultimate-baseline-1000`, `v064-enemy-pressure-1000`, and `v064-rear-boundary-1000`. The committed `v064-early-wave-summary.json` records inputs, executable hashes, wave cohorts and strategy counts.

The final simulation batch used `Build/RearBoundary-v064/WalkingHard.exe`, assembly SHA-256 `782535c5caa78045514e2d3f5db71d23771a0008c7ddfa51607fc51d6d5c1b7d`. The publish candidate `Build/EnemyPressure-v064-final/WalkingHard.exe`, SHA-256 `5bdc31723d600d0db655cd327dce7df482b071554cef44284556749427dccf30`, changes only initialization/restoration of a random generator in the Full-mode QA fixture. Gameplay code and the Fast simulation path are identical.
