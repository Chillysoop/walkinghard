# Walking Hard 0.6.6 — defensive balance retest

29 October 2026. User-approved changes tested separately and together: Counterguard base duration 3.5 → 2.5 seconds; Duelist ward 1 hit / 4 seconds → 2 damaging hits / 6 seconds. Both changes are retained. Global enemy health, damage and spawn cadence remain unchanged.

## Matched Fast screening

Each candidate is 1,000 eight-wave games, normal 280 starting health, six mixed drafting policies, seeds 12000-12999, four workers and a 600-combat-second limit. A 32-game legacy control reproduced all 27 gameplay fields of the prior baseline. The baseline 1,000-game batch is reused from 0.6.4; 0.6.5 impact presentation did not change the Fast model, as separately verified in its regression report.

| Tuning | Reached first ultimate | Eight-wave wins | Wave 1 / 2 / 3 deaths | Timeouts | Wall seconds |
| --- | ---: | ---: | --- | ---: | ---: |
| Baseline | 828 / 1000 | 419 / 1000 | 0 / 10 / 162 | 2 | 104.26 |
| Counterguard only | 827 / 1000 | 360 / 1000 | 0 / 10 / 163 | 1 | 115.78 |
| Duelist only | 829 / 1000 | 423 / 1000 | 0 / 10 / 161 | 2 | 122.02 |
| Both changes | 828 / 1000 | 365 / 1000 | 0 / 10 / 162 | 1 | 113.06 |

All 3,000 candidate games are valid, unique and complete, with no missing games. Fast batches overlapped a separate Full-physics validation job, so these wall times include CPU contention. Timeouts are recorded outcomes, not wins or invalid states.

The combined change preserves first-three-wave survival at **82.8%**, while eight-wave wins decline from **41.9% to 36.5%**. Matched outcomes: four losses become wins, 58 wins become losses. One seed improves early survival and another regresses; the aggregate opening result is unchanged.

Counterguard-only removes 59 net wins while costing one early survival. In the fixed baseline cohort of 331 games with Counterguard by wave two, wins fall 316 → 259; mean Counterguard absorption falls 851.8 → 643.1. This is the intended reduction in sustained late-run defense, without a broad opening-wave nerf. Cohort comparisons include downstream changes in combat and drafts.

Duelist-only raises Common equipped-copy absorption from **10.62 to 24.54** (69 copies in each batch). Full wins move only 419 → 423: five improved outcomes and one regression, too few discordant pairs to claim a conclusive overall win-rate gain. In the fixed 148-game baseline Duelist cohort, absorption rises 10.30 → 24.78 and wins 73 → 75. The combined cohort's lower win rate also includes Counterguard's nerf; it should not be read as Duelist becoming weaker.

## Combined strategy results

| Policy | Games | Early survivors | Full wins before | Full wins after |
| --- | ---: | ---: | ---: | ---: |
| balanced | 167 | 163 | 143 | 125 |
| gun | 166 | 119 | 40 | 37 |
| pinball | 167 | 148 | 82 | 73 |
| random | 167 | 117 | 44 | 32 |
| sever | 166 | 137 | 39 | 34 |
| storm | 167 | 144 | 71 | 64 |

Balanced remains the strongest test policy. This update reduces Counterguard dominance and improves an alternative defense; it does not establish equal build strength. No additional item or global difficulty changes are included.

## Full PhysX comparison

Twelve baseline-control games versus twelve combined games, using the same candidate binary and identical seeds 12654-12665, with two workers and normal health. This block was chosen before Full runs because the old Fast baseline equipped Duelist in six of those games. It includes two games per policy and is deliberately targeted, not representative.

All 12 games in both groups survive the opening three waves. Eight-wave wins are 5 → 4; the changed winner is a Storm policy game. Common equipped Duelist absorption is 10.14 → 32.66 across five copies per group. All 24 games are valid, with no timeouts, missing games or duplicates. Full uses articulated bodies and the 0.6.5 impact changes, unlike Fast's approximate contact model. A sample this small cannot establish a population win-rate change, and PhysX is not bitwise deterministic across processes/run histories.

## Shipped rules

- Counterguard: 2.5-second Common lifetime with existing rarity/effect scaling. Capacity stays 25 at Common; 80% bank conversion and existing caps remain. Action duration is unchanged.
- Duelist: a real reload or lethal sever refreshes two charges for six seconds at Common, preserving rarity scaling and 45% mitigation (65% cap). Only the first mitigated hit staggers. Refresh replaces charges, never stacks. Fully blocked hits preserve charges and first-hit stagger; the old behavior is explicitly restored by the legacy test input.
- Tooltips, one-line reward descriptions and live ward status reflect the new rules.

## Verification and reproducibility

Two Full gameplay QA scenarios (`progression`, `manual`) pass 187 production-effect assertions each, including every rarity's expiry, mitigation, two charges, third-hit behavior, first-only stagger, actual reload/sever triggers, refresh, blocked-hit preservation and run reset. These QA scenarios use an explicit 5,000-health fixture for coverage; all balance simulations use ordinary health.

An initial test-fixture failure was fixed by clearing a Counterguard bank left by an earlier probe: its area damage killed the sever test target before the sword could hit. Only the fixture changed between the 32-game legacy control and the final candidate build; the Fast/gameplay path did not.

Candidate Assembly-CSharp.dll SHA-256: `8ce390368580dfd6bdddd50a45d268a67e86c9766e4eb13930f1b2f3b4dde5a0a`.

Raw batches are local under `QA/Simulations/v066-*`; exact inputs are committed in `QA/Balance/v066/`. `analyze.py` regenerates `QA/v066-balance-results.json`, including batch provenance, fixed cohorts and paired outcomes. The stock comparison runner additionally validates matching metadata, seeds and policy assignments. Accumulated item absorption is not normalized by equipped time. Fast results describe these simple drafting policies, not human win rates.
