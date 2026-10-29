# Walking Hard 0.6.8 — approved balance retest

29 October 2026. Implements the approved recommendations from [the controlled Siphon review](controlled-siphon-review.md). No further balance changes are included.

## Applied changes

- Siphon Blade retains 4/5/6/8/10% drain on damage credited to that blade, including augment follow-ups and collisions. It no longer grants global Lifesteal merely by being equipped. Existing marks still let other attacks drain marked targets; mark strength, duration, spread, weapon damage and action time are unchanged.
- Reclaimer grants 3/4/5/6/7% global Lifesteal; only its own contribution becomes 1.5 times stronger below half health. Health bonus and rarity scaling are unchanged.
- Harvest stays at its existing damage, 16-second recharge and 8/9/10/11/12% maximum-health pulse budget.
- After testing healing separately, Redline was tested with six accelerated sequence turns instead of three. It retains 18-second recharge, ammo refill and 20/23/26/29/32% haste. A simultaneous weapon pair consumes one turn; the ultimate activation consumes none.
- Item descriptions distinguish blade/mark healing from global healing. The character sheet labels its global stat GLOBAL LIFESTEAL. The shared 1% maximum-health budget per originating action remains in force.

## Method and controls

Same fixed matrix as the earlier review: 3,600 primary cases with Common support plus 480 sensitivity cases with Rare support. Focal item rarity ranges from Common through Legendary. Four contexts (sever, storm, guns and stacked Siphon), eight waves, normal 280 base health plus actual armor, no augments, no later drafting, ultimate unlocked after checkpoint three, 600 simulated-second timeout. These frozen loadouts measure relative differences; their absolute win rates are not ordinary-player difficulty estimates.

420 controls on the new binary with all three legacy rules reproduce every gameplay field, including item attribution and wave reports, from the old matrix. Version, wall time and experimental diagnostics are excluded. Each stage then reruns all affected loadouts; unaffected results are explicitly reused, not counted as newly simulated games. Healing accounting reconciles potential drain minus cap loss and overheal with actual item-attributed healing.

| Stage | Games actually rerun | Gains / losses versus prior rules | Wins in reconstructed 4,080-case matrix |
| --- | ---: | ---: | ---: |
| Baseline | Previously recorded | — | 1,559 |
| Blade only | 1,260 | 1 / 135 | 1,425 |
| Armor only | 1,000 | 1 / 29 | 1,531 |
| Both healing changes | 1,500 | 0 / 191 | 1,368 |
| Healing changes + six-turn Redline | 920 | 8 / 7 | 1,369 |

Blade-only and armor-only compare against baseline; combined also compares against baseline. Redline compares against the combined healing stage. There are 4,680 newly executed affected cases, plus 420 legacy controls. The final matrix has one simulated timeout, counted as a non-win; there are no invalid, missing or duplicate cases.

## Matching-rarity outcomes

Final primary matrix, 48 matched context/seed cases per item per rarity; pooled totals give equal weight to every tier. Baseline pooled figures are shown for context. Other equipped items in a fixture also receive the approved changes, so use the isolated stages above when attributing causes.

| Item | Common | Uncommon | Rare | Epic | Legendary | Final pooled | Old pooled |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Siphon Blade | 25.0% | 25.0% | 27.1% | 45.8% | 47.9% | 34.2% | 80.8% |
| Edge Sword | 25.0% | 25.0% | 25.0% | 27.1% | 33.3% | 27.1% | 27.1% |
| Breach Hammer | 16.7% | 14.6% | 33.3% | 35.4% | 41.7% | 28.3% | 35.4% |
| Kinetic Fists | 0.0% | 10.4% | 22.9% | 25.0% | 25.0% | 16.7% | 24.2% |
| Pulse Pistol | 0.0% | 14.6% | 22.9% | 25.0% | 25.0% | 17.5% | 25.0% |
| Reclaimer Armor | 25.0% | 27.1% | 33.3% | 58.3% | 68.8% | 42.5% | 52.1% |
| Bulwark Plate | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Reactive Shell | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Grounding Mantle | 25.0% | 25.0% | 25.0% | 25.0% | 29.2% | 25.8% | 25.4% |
| Duelist Jacket | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Harvest | 25.0% | 25.0% | 29.2% | 37.5% | 54.2% | 34.2% | 34.2% |
| Tempest Array | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Singularity Burst | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Razor Cyclone | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Redline | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |

Siphon Blade falls from 80.8% to 34.2% pooled wins, compared with Edge Sword at 27.1%. Its average realized whole-build healing falls from 1.79 to 0.95 HP/s. It remains an effective sustain weapon without conferring healing on unrelated attacks to unmarked enemies.

Reclaimer falls from 52.1% to 43.8% in its isolated test, then 42.5% in the final matrix. It remains ahead of the other armor in these fixtures, particularly at Epic and Legendary. The intended sustain identity survives; this is not evidence that the armor set is now fully balanced.

Harvest stays at 34.2% versus 25.0% for the other ultimate candidates in the primary matrix. With Rare support it still wins 80/80, versus Tempest at 63/80. Keep it unchanged in this release: do not combine another healing reduction with the persistent-source changes. If later tests show it is still compulsory, the previously proposed 6–10% pulse budget remains an unapproved follow-up experiment.

Redline yields eight gains and seven losses across its 920 affected cases, a net one additional win. Its focal primary slice remains 60/240. The six-turn burst works mechanically, but this evidence does not show that it solves Redline’s competitive weakness. Sequence timing can change targets and encounter alignment, so a longer buff need not improve every seed. Do not call this a proven balance fix or add another buff without a separate comparison.

Rare-support sensitivity (80 games per focal item):

| Item | Old wins | Final wins |
| --- | ---: | ---: |
| Siphon Blade | 80/80 | 77/80 |
| Edge Sword | 49/80 | 52/80 |
| Reclaimer Armor | 76/80 | 73/80 |
| Bulwark Plate | 61/80 | 61/80 |
| Harvest | 80/80 | 80/80 |
| Tempest Array | 63/80 | 63/80 |

## Ordinary drafting retest

1,000 mixed-policy Fast games, seeds 12000–12999, eight waves, normal health, normal reward choices and ultimate progression. Eight-wave completions: **475/1000 → 434/1000**. Cleared wave three: **861/1000 → 849/1000**. Paired outcomes: 4 gains, 45 losses. This batch took 95.7 seconds end to end on this machine. All 1,000 games were valid, with no timeouts.

| Strategy | Games | Old wins | New wins |
| --- | ---: | ---: | ---: |
| balanced | 167 | 131 | 129 |
| gun | 166 | 50 | 41 |
| pinball | 167 | 86 | 84 |
| random | 167 | 57 | 50 |
| sever | 166 | 73 | 56 |
| storm | 167 | 78 | 74 |

Ordinary drafting loses 4.1 percentage points overall and 1.2 points at wave three. Sever takes the largest drop (44.0% to 33.7%); this is consistent with removing free healing from a favored sword, but the strategy-level result alone does not isolate the cause. The balanced policy stays at 77.2%, and the controlled sustain fixtures retain strong outcomes. Preserve this patch for player testing; any follow-up should compare sever alternatives and Redline/gun builds directly rather than undoing the scoped healing rule or raising global difficulty.

## Full physics checks

Six preselected matched cases use native body/contact physics with ordinary health. Three clear, versus four of these same six in the prior baseline. These are coverage checks, not a statistically reliable win-rate estimate. Full physics is not bitwise deterministic; the prior calibration agreed with Fast on only 14 of 24 individual outcomes.

| Case | Previous result / HP | New result / HP |
| --- | --- | --- |
| sever-weapon-siphonblade-r3-s0 | Complete / 83.1% | Defeated / 0.0% |
| sever-armor-reclaimer-r3-s0 | Complete / 79.2% | Complete / 53.2% |
| gun-ultimate-redline-r3-s0 | Defeated / 0.0% | Defeated / 0.0% |
| sever-weapon-siphonblade-r3-s1 | Complete / 100.0% | Complete / 7.7% |
| storm-armor-reclaimer-r3-s0 | Complete / 48.3% | Complete / 28.9% |
| gun-ultimate-redline-r3-s1 | Defeated / 0.0% | Defeated / 0.0% |

## Reproduction and limits

The exact candidate assembly SHA-256 is `b2d257d971d9d7d2a0fee4b9bed837bbad1c3dc63d4d311d12000dfd2c2569d3`. No game code changed after that build. Baseline and candidate provenance are in [the JSON results](v068-balance-results.json); per-case old/new outcomes and healing are in `Balance/v068/case-results.csv`, with rarity summaries in `Balance/v068/summary.csv`. Full raw worker files remain local in `QA/Simulations/`.

`QA/Balance/v068/prepare.py` writes the exact cases and rule files. `run_experiments.py` verifies the existing 420 legacy controls, then runs blade, armor, combined and Redline sequentially. `analyze.py` validates and aggregates complete batches; `write_report.py` renders this report. Fresh output folders are required when rerunning. The controlled runner accepts `--player`, `--cases`, `--balance`, `--out`, `--mode Full` and `--workers`.

Fast mode approximates contacts and body motion. These results justify the approved targeted changes, not global enemy tuning or broad buffs to every alternative. [Verification](v068-verification.md) records the 62 passing QA scenarios, launcher checks and the unconfirmed visual check. No Computer Use or scheduled job was used.
