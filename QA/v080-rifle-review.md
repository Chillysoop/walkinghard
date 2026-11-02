# Rifle / Focused Tether follow-up

November 2, 2026. No item values changed. Eight matched seeds, four module variants: **32 valid Full/PhysX games** at normal 280 base health. All cases use the same Rare rifle, Suppressive Volley → Conductive Brand → Vector Tether → locked rifle block, Surveyor / Grounding / Loader / Overclock, and Redline unlocked at checkpoint three. Equipment stays fixed. Inputs and results: [rifle study](Balance/v080-rifle-factorial/).

| Modules | Completed | Mean final wave | Mean kills | Mean combat seconds |
| --- | ---: | ---: | ---: | ---: |
| None | 4 / 8 | 8.00 | 300.8 | 322.3 |
| Livewire on rifle | 6 / 8 | 8.00 | 308.0 | 333.1 |
| Focused on Tether | 1 / 8 | 7.38 | 256.8 | 278.1 |
| Both | 0 / 8 | 7.50 | 260.0 | 278.3 |

The earlier two-seed result was not sufficient to blame either module. This larger comparison isolates the concern to **Focused Tether in this particular build and encounter schedule**. Livewire alone performed better than native in completion count; the Focused variants usually died earlier. This does not establish global module strength or statistical significance.

Tether direct damage per combat second fell from a mean 10.53 to 8.18 with Focused alone. Rifle range-wait time was already below 0.5% of combat time in all variants. That supports investigating corridor coverage and crowd containment before adding rifle damage or shortening waits. Total damage taken is capped by death and total kills reflect survival duration, so neither alone measures efficiency.

**Next balance experiment:** keep Livewire fixed, compare the present Focused corridor with one modest width adjustment, then check other Tether builds and enemy formations. First verify that its extra reach is useful in a player-chosen setup. Preserve a real width-versus-reach choice; do not make the socket an automatic upgrade. No such adjustment is included in the combat presentation preview.

## Reproduction and validation

Source baseline: `b9ca597`, `Build/TrailerFinish-v080/WalkingHard.exe`. Assembly SHA-256: `44ee99e9163582a2eeb74bcca924c4bb88e26c756e9833391d10dbb6de514253`.

Run `python QA/Balance/controlled-siphon/run.py --cases QA/Balance/v080-rifle-factorial/cases.json --mode Full --workers 2 --player <player> --out <new-directory>`, then `python QA/Balance/v080-rifle-factorial/analyze.py --input <new-directory>`.

The original runner completed all 32 simulations, then rejected `native` as an item ID during post-run validation. These study labels describe variants. The runner now accepts an optional `focalItem` (`rifle`) independently of the variant label, preserving its old default for other studies. The saved results were independently validated against exact case IDs, seeds, health, rarity, actual equipped rifle and attached module slots: no invalid, duplicate, missing or timed-out cases. The simulations were not rerun or relabeled to obtain the reported outcome.
