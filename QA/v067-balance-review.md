# Walking Hard 0.6.7 — balance review

29 October 2026. Approved weapon-augment upgrades, Counterguard burst adjustment and Siphon content. No global enemy health, damage, spawn cadence, rarity odds or ultimate progression changes.

## Matched Fast screening

Each row is 1,000 games, seeds 12000–12999, eight waves, normal 280 starting health, six drafting policies, four workers, 600 combat-second limit. All batches are complete with zero invalid/missing/duplicate games. The 0.6.6 baseline is reused after a matched legacy control.

| Configuration | Clear first three | Eight-wave wins | Timeouts | Wall seconds |
| --- | ---: | ---: | ---: | ---: |
| 0.6.6 baseline | 828 | 365 | 1 | 113.00 |
| Weapon augments only | 828 | 372 | 0 | 96.26 |
| Counterguard only | 828 | 362 | 1 | 117.91 |
| Both, original catalog | 828 | 370 | 0 | 105.14 |
| Both + Siphon (final gameplay) | 861 | 475 | 2 | 142.85 |

Weapon augments alone add seven net wins; Counterguard alone removes three. Their combination adds five. These small changes do not establish balanced item strength. Adding the Siphon catalog to that combined ruleset adds 105 net wins. Comparing the final gameplay candidate to 0.6.6 gives 266 loss→win and 156 win→loss outcomes; first-three-wave survival improves 82.8%→86.1%.

New catalog entries change seeded offer/shuffle trajectories. The Siphon comparison therefore measures the whole content update, including draft availability and choices, rather than a randomized isolated healing-stat effect. These simple policies are not optimal players. Timeouts are not wins. Batches overlap Full/QA jobs and compilation, so wall times include contention; the final 142.85-second batch is not an isolated throughput benchmark.

| Policy | Baseline wins | Final wins | Baseline % | Final % |
| --- | ---: | ---: | ---: | ---: |
| balanced | 125/167 | 131/167 | 74.9 | 78.4 |
| gun | 37/166 | 50/166 | 22.3 | 30.1 |
| pinball | 73/167 | 86/167 | 43.7 | 51.5 |
| random | 32/167 | 57/167 | 19.2 | 34.1 |
| sever | 34/166 | 73/166 | 20.5 | 44.0 |
| storm | 64/167 | 78/167 | 38.3 | 46.7 |

## Interpretation

Sever and gun strategies improve, with sever gaining the most among focused archetype policies. Defense/sustain still leads. Reclaimer Armor, Siphon Blade and Harvest have high equipped-run win associations (84/100, 130/160 and 115/138 respectively), but survivorship, rarity, drafting policy and acquisition time confound these associations. Do not treat those percentages as causal item power. No additional balance values were applied after the approved pass.

Next evidence to collect: fixed-loadout comparisons of Reclaimer versus other armor, Siphon Blade versus Edge Sword at matching rarity, and Harvest versus the other ultimates. Measure actual healing, overheal and cap saturation alongside damage prevention. Keep gun base damage and global difficulty unchanged until those controlled comparisons.

## Full physics coverage

Six matched seeds 12654–12659, one per policy, normal health; two workers. Final gameplay candidate versus legacy control. Tiny coverage sample, not a population estimate; Full is not bitwise deterministic. Final release adds only healing-total fusion bookkeeping.
First-three-wave survivors: 6/6 → 6/6. Eight-wave wins: 2/6 → 5/6. All six final games are valid; 0 timeouts. This is coverage only, not a reliable win-rate estimate.

## Provenance and reproduction

Inputs are committed in `QA/Balance/v067/*.json`. `analyze.py` regenerates the aggregate JSON from local `QA/Simulations/worker-*.jsonl` batches. Every batch records its DLL hash, exact arguments, seeds and timing. Three 32-game controls match all 24 scalar gameplay fields: old baseline versus legacy rules, no-Siphon rules across the Focused extension, and final gameplay versus release bookkeeping fix.

The first candidate ran 15 core and 45 variety scenarios. A later gameplay candidate added Focused compatibility for Drain Brand and persistent mark feedback, then reran 1,000 Fast games plus Full coverage. The release changes only fusion bookkeeping for the new healing contribution and adds a production assertion; 32 gameplay trajectories match exactly. The final release passes 273 production-effect assertions. See `v067-verification.md`.

An additional pre-Focused 1,000-game Siphon screen and 12-game Full candidate batch are exploratory and excluded from the final tables. Existing Recovery Treads healing remains in its legacy absorption counter; the new `healing` contribution records Siphon sources separately.
