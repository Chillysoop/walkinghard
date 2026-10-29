# Walking Hard: 1,000-game balance screening

Historical pre-change screening; see [0.6.1 verification](v061-verification.md) for the subsequently approved capacity-25 test.

29 October 2026. **1,000 / 1,000 valid games in 88.30 seconds** (679.5 games/minute); four workers; seeds 12000–12999; mixed policies; eight waves; 600 simulated seconds per run. Outcomes: **415 wins, 578 defeats, 7 timeouts**. No missing or duplicate seeds. All balance values match release 0.6.0; no recommendations have been applied.

This batch uses the new **Fast screening mode**, not full PhysX. Authored combat/drafting executes at 60 Hz; movement, body geometry and contacts are approximate. The previous full-physics run was stopped at 32 valid games when the user requested a two-minute target. Its evidence is preserved separately.

## Main finding

Defense access dominates these bots more strongly than weapon choice. Counterguard is the clearest outlier. With Counterguard in the first checkpoint's sequence, **307 / 331 runs won (92.75%; 95% interval 89.44–95.08%)**; without it, **108 / 669 won (16.14%; 13.55–19.12%)**. Measuring the first checkpoint reduces the bias from only acquiring items after surviving later waves, though it does not randomize the build or its other choices.

The association remains within every drafting policy. At run end, Common Counterguard alone appears in 97 runs, 89 wins (91.8%), so this is not only a Legendary-stat effect. Among runs reaching wave eight, Common Counterguard absorbed about **493 damage**, versus a base hero's 280 health. The smaller full-physics sample also supports the direction: 11 / 12 equipped-Counterguard runs won, compared with 8 / 20 without it. That full sample is small and not an independently randomized treatment test.

## Strategy outcomes

| Policy | Games | Wins | Win rate | 95% interval | Timeouts |
| --- | ---: | ---: | ---: | --- | ---: |
| balanced | 167 | 145 | 86.83% | 80.86–91.14% | 0 |
| gun | 166 | 34 | 20.48% | 15.04–27.26% | 0 |
| pinball | 167 | 81 | 48.50% | 41.04–56.03% | 5 |
| random | 167 | 48 | 28.74% | 22.42–36.02% | 0 |
| sever | 166 | 39 | 23.49% | 17.69–30.50% | 0 |
| storm | 167 | 68 | 40.72% | 33.56–48.30% | 2 |

These are drafting agents, not archetype power rankings or estimates of player skill. The balanced agent seeks defense/sustain; 133 / 167 of its final builds had Counterguard. The gun agent had it in only 24 / 166. The agents never spend rerolls or exhaustively search inventory, and their sequence heuristics can leave melee actions blocking later ranged attacks.

## Builds that stand out

| Observed equipped core | Games | Wins | Win rate | Interpretation |
| --- | ---: | ---: | ---: | --- |
| Counterguard + Recovery Treads | 89 | 86 | 96.6% | Very forgiving defensive core |
| Counterguard without Recovery Treads | 240 | 224 | 93.3% | Guard is doing most of the work |
| Recovery Treads without Counterguard | 100 | 56 | 56.0% | Strong alternate sustain route |
| Neither Counterguard nor Recovery Treads | 571 | 49 | 8.6% | Offensive builds lack comparable survival |
| Mass Driver + Fault Line | 53 | 29 | 54.7% | Promising Pinball core; 14 also carried Counterguard |
| Sever action + Salvager/Magnet | 151 | 51 | 33.8% | Mixed results; approximation affects fragments |
| Vector Tether + Rail Rifle | 88 | 25 | 28.4% | Needs better controlled validation before weapon buffs |
| Fist style + one-handed gun | 128 | 46 | 35.9% | Mixed close-range core, not a locked class |

Thunder Freight's full core appeared only nine times, and sword/hammer Cut & Carry only ten times. Those samples are too small for confident balance conclusions. These cores may overlap, and end-loadout presence still has survival and acquisition-timing bias.

## Proposed next tuning round — not applied

| Priority | Item | Specific proposed change | Evidence and intended result | Confidence |
| --- | --- | --- | --- | --- |
| 1 | Counterguard | Common barrier capacity **35 → 25** before the existing rarity multiplier; keep its 3.5-second base duration and 80% next-basic conversion for this first test | Reduce continuous protection while preserving the defensive setup/payoff identity. Strong early-choice association, within-policy pattern and supporting full-physics results | Strong signal; exact value is a hypothesis |
| 2 | Duelist Jacket | Ward **1 hit / 4 seconds → 2 hits / 6 seconds**; keep 45% base mitigation and the existing cap. Stagger only on the first absorbed hit; rearming replaces, rather than stacks, charges | Common copies in final-wave runs prevented only about **8.2 damage** on average, versus 57.2 for Reactive Shell and 89.8 for Bulwark. Requiring reloads/severs currently buys too little defense | Medium; exposure time is not recorded |
| 3 | Razor Cyclone | Common damage **45 → 55**, radius **6 → 7**; keep the 15-second cooldown and 1.3-second commitment | Only **3 / 34** equipped runs won. At wave eight a normal enemy has about 49.8 HP, so 45 Common damage misses a useful lethal-sever threshold. This gives the sever ultimate a clearer payoff without automatic execution | Low–medium; only 34 runs, fragment-model bias |
| 4 | Recovery Treads | If still dominant after the guard test, Common repair mote **3 → 2.4 healing**; preserve one mote per originating turn and pickup behavior | Mean realized healing was about 140 across equipped runs. Without Counterguard, Treads builds won 56% versus 8.6% with neither defensive core | Medium signal; defer this nerf until alternate defense is stronger |

**First experiment:** test Counterguard's capacity reduction alone on the same 1,000 Fast seeds. Then verify representative wins, losses and close calls in Full mode. Test the Duelist and Cyclone buffs separately before combining them. Do not simultaneously weaken both major survival routes and infer which change caused the result.

## Items to hold unchanged for now

- **Discharge:** 83 / 83 equipped runs won, but every one carried Counterguard and had already survived to an augment draft. Test its host first; this does not establish that Discharge independently needs a nerf.
- **Guns, Vector Tether and collision/fragment augments:** their low Fast results are not sufficient evidence for broad buffs. Full/Fast calibration particularly diverges for guns, Storm and Pinball. Gun defeats also have lower total contribution exposure because they die earlier; raw damage or total reload time is not normalized equipped-item DPS.
- **Weapon pairing and two-handed budgets:** ordinary strategy choices and rarity are confounded with these layouts. Use matched loadout fixtures for direct comparisons; do not change budgets from this batch alone.
- **Gear with zero direct damage rows:** those zeros do not mean its effect failed. Several shared payoffs are credited to the originating weapon/ability, while gear contributes assists, mitigation or tempo. Some augment contribution counters likewise record damage but not displaced distance or useful control.

## Calibration and verification limits

The 32 preserved full-physics seeds had 19 wins. Their Fast counterparts had 10 wins; 23 / 32 outcomes agreed. All nine disagreements changed a Full win into a Fast loss. By policy, agreement was balanced 5/5, random 7/7, sever 5/6, gun 2/4, Pinball 3/6 and Storm 1/4. That is substantial model error, not a small statistical footnote. Fast screening is useful for obvious numerical outliers and generating hypotheses, but is not yet reliable for quantitative physical-build rankings.

The stopped batch has unequal small policy counts and includes only completed attempts, so it is a diagnostic calibration set, not a representative accuracy benchmark. Same-seed simulations also diverge in later drafting because different combat outcomes alter progress and ownership. No causal treatment effects or human win rates are claimed.

Failures clustered around waves 4–6: 131, 152 and 129 defeats respectively. Wave eight had 83 defeats and seven timeouts. This suggests a mid-run survival bottleneck plus a smaller boss-cleanup problem; do not reduce all enemy damage or boss health based on this result alone.

Each presence count is deduplicated per game. Rarity tables use the highest equipped rarity for that item; contribution tables sum equipped copies. The reports lack equip-time histories, activation exposure and exact mote/ward trigger counts, limiting rate-normalized comparisons. Intervals shown are unadjusted descriptive Wilson intervals, not corrections for testing many items.

Two Full-mode integration scenarios (Close Quarters and Human Pinball) passed on the experimental executable, each including 56 production-mechanic probes. Their kills/collisions matched the saved release fixtures. Regular play and the desktop shortcut still use the released 0.6.0 build; this work did not apply tuning changes or replace the player's installed game.

## Evidence

- `batch.json`, `summary.json`, `worker-*.jsonl`: provenance and complete raw results.
- `analysis-items.csv`: per-run item presence and mean contributions.
- `analysis-item-strata.csv`: policy and Counterguard-present/absent cohorts.
- `analysis-item-rarities.csv`: rarity-separated presence counts and outcomes.
- `analysis-builds.csv`, `analysis-augments.csv`: observed build cores and modules.
- `analysis-calibration.csv`: the 32 paired Full/Fast outcomes.
- `analysis-failures.csv`: failure waves and timeouts.
- `analyze.py`: reproducible analysis generator.

Measured executable: `Build/FastBalance-v2/WalkingHard.exe`; assembly SHA-256 `00C229FC0134662C0D8592C92D5543E7D8E2486F85E4B6B128D15B23412710C6`. Its simulation mode changes geometry/contacts only; the authored item tuning comes from release 0.6.0. The 88.30-second batch timing includes worker startup and excludes the subsequent analysis/report-writing time.
