# v0.8.0 full-run balance and pacing

November 2, 2026. The rollout keeps damage, armor, rarity odds, fusion and reward counts unchanged. Focused Tether now trades 30% of its corridor width for two extra metres of reach (previously 50%). Ordinary waves use authored entry patterns; waves 4–7 have three 0.75× intervals followed by one 1.75× interval. That preserves the four-interval mean, not identical spawn counts in every fight. Opening waves keep steady timing and the finale keeps its existing rules.

## Ordinary drafts across six build directions

The new `archetypes` policy set covers pinball, storm, sever, gun, cutcarry and close. These agents receive ordinary offers, acquire real rarity variants, fuse through the normal rules, and unlock their ultimate at checkpoint three. They do not force a complete archetype or spend rerolls. Every completed run had 13 item choices and two augment choices. A policy name describes its preferences, not proof that its final build contains a specific combination.

Three 600-game Fast screens use seeds 82002–82601. Each contains 100 games per policy, no invalid or missing rows, no duplicate seeds and no timeouts. A separate 24-game presentation control exactly matches 13 gameplay fields in the corresponding baseline games.

| Policy | Original completion count / 100 | Formation-only / 100 | Combined / 100 | Original → combined mean wave |
| --- | ---: | ---: | ---: | ---: |
| Pinball | 32 | 35 | 35 | 5.68 → 5.87 |
| Storm | 42 | 41 | 41 | 6.44 → 6.55 |
| Sever | 41 | 41 | 41 | 6.23 → 6.46 |
| Gun | 14 | 15 | 15 | 4.31 → 4.39 |
| Cut & Carry | 20 | 21 | 21 | 5.38 → 5.47 |
| Close Quarters | 25 | 27 | 27 | 5.87 → 5.96 |

The combined screen differs from formation-only in some contributions but not completion counts. Fast uses approximate contacts and historically understates gun and collision builds. These results support a modest pacing change; they do not justify global damage buffs or estimate human win rates.

The Full/PhysX comparison uses 24 ordinary drafted games per version, seeds 83004–83027, two workers, normal base health and a 600-second simulated combat budget. All 48 rows are valid, with no missing or duplicate seeds.

| Policy | Original completions / 4 | Combined / 4 | Original → combined mean wave |
| --- | ---: | ---: | ---: |
| Pinball | 3 | 3 | 8.00 → 8.00 |
| Storm | 1 | 1 | 6.00 → 6.25 |
| Sever | 2 | 3 | 7.50 → 7.75 |
| Gun | 1 | 1 | 6.50 → 6.50 |
| Cut & Carry | 3 | 3 | 7.25 → 7.50 |
| Close Quarters | 2 | 1 | 7.25 → 7.50 |

Both versions completed 12/24; 22 outcomes matched. Sever seed 83012 changed from timeout to completion; Close Quarters seed 83027 changed from completion to defeat in wave eight. The baseline timeout had 61.8 health and one enemy remaining: the boss was still taking damage, falling from 457 to 26.8 health between combat seconds 420 and 600, with repeated Breach Hammer range waits. It was not a frozen encounter. The candidate had no timeouts. Four games per policy and PhysX variation are insufficient to claim superiority or perfect parity.

The candidate also gives shooters a gun pose. That leaves authored attack damage/timing unchanged but can affect subsequent physical corpse poses. Treat the ordinary Full comparison as the complete package, not an isolated estimate of entry-pattern effects.

## Isolated Focused Tether coverage

The earlier [32-game rifle factorial](v080-rifle-review.md) separated Livewire from Focused: native 4/8 completions, Livewire 6/8, Focused 1/8, both 0/8. The new test repeats only the two affected variants on the same eight seeds, fixed Rare equipment, normal 280 health and original encounter rhythm. Reach and damage stay unchanged; width changes from 0.5× to 0.7× native.

| Variant | Previous completions | New completions | Mean survival seconds, previous → new |
| --- | ---: | ---: | ---: |
| Focused Tether | 1/8 | 2/8 | 278.09 → 306.95 |
| Focused Tether + Livewire | 0/8 | 2/8 | 278.34 → 296.24 |

All 16 new rows are valid, with no timeouts. The existing Focused win is preserved. The sample supports softening an overly severe coverage cost in this setup. It does not show that Focused is now stronger than native or that every rifle build needs it. The narrower footprint and written 30% cost remain visible.

## Paired attacks versus two-handed weapons

Twelve additional Full games compare three matched Rare loadouts on seeds 84001 and 84002, with original encounter rhythm, normal health plus equipped armor, no modules, and checkpoint-three ultimates. Each pair shares abilities and passive gear. Both weapon mounts are Rare for the paired cases. Eleven games completed; all twelve are valid and none timed out.

| Matched family | Paired result | Two-handed result | Observed tradeoff |
| --- | --- | --- | --- |
| Swords | 2/2, mean 316s | Greatsword 2/2, mean 307s | Greatsword finished sooner; paired swords produced more collision events in these runs. Both ended at full 310 health. |
| Hammers | Breach pair 2/2, mean 335s | Impact Hammer 2/2, mean 358s | The pair finished sooner here. The heavy hammer retains its stronger single launch and locked group. |
| Guns | Pistols 2/2, mean 132 health | Rifle 1/2 | Pistols were safer in this fixed setup; the rifle generated body launches/collisions and line penetration. This small result does not establish an overall gun ranking. |

The existing production checks cover full activations on distinct paired hosts, one socket on shared fists and two-handed weapons, simultaneous matching attacks, separated actions and attribution. This pass does not halve paired module effects or add another two-handed socket. Balance between acquisition cost, reach, group timing and payoff remains a playtest question beyond these small fixed comparisons.

## Evidence and reproduction

`Balance/full-run/summary.json` contains batch inputs, assembly hashes, integrity counts, grouped outcomes and matched outcome counts. `games.csv` retains every seed, final loadout and compact gameplay metrics. `full-results.json` retains the complete Full rows, contributions and wave reports. `analyze.py` validates the saved batches and regenerates those artifacts. The isolated case inputs and override files are in the same directory.

The original/formation/combined Fast screens contain 1,800 games, plus the 24-game presentation control. The new Full evidence contains 48 ordinary games, 16 Tether cases and 12 paired cases: 76 total. The earlier 32-game rifle matrix is reused as the isolated Tether baseline, not counted as new work.

Run ordinary batches with `Run Simulations.ps1 -Policy archetypes -Waves 8 -MaxCombatSeconds 600`, using the documented seed ranges, two workers for Full and the saved balance input. Fast baseline/formation controls used three workers; the final Fast screen used two. Seeds are identical and independent in Fast mode; Full worker counts match. Controlled cases use `QA/Balance/controlled-siphon/run.py --mode Full --workers 2 --cases <file> --balance <file> --player <exe> --out <new-directory>`.

Early study binaries used `encounterRhythm`; the final implementation uses the inverse `legacyEncounterRhythm`. Source override files carry both for compatibility; the exact input copied into each original batch is retained in its recorded metadata. Final rendered performance and regression gates are documented separately in [verification](v080-fullrun-verification.md).
