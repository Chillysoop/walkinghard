# Walking Hard 0.8.0 verification

November 1, 2026 local time. Unity 6000.3.27f1. Feature branch only; no main merge, published release or shortcut-cache replacement.

## Changes

Seventeen modules cover 47 non-module identities (64 total). Livewire lets any weapon discharge Conductive. Follow-through carries stagger or marks to a weapon contact. Focused reshapes Push/Tether. Arc, Reaping and Drain gain fixed Afterfields. Surveyor, Grounding, repair pickups and prepared retrieval connect to those effects.

Distinct paired weapons retain full activations; shared fists and two-handed weapons retain one. Fields keep their original action's attribution, derived-hit cap and healing budget. New fields replace earlier fields from that host. No extra sequence positions, rewards, rarity systems or global stat changes.

Native artwork, host comparisons, visible footprints, prepared-weapon labels and payoff cues explain the behavior. Livewire explicitly uses the consumed mark's strength. Its art was generated with the built-in imagegen tool; the prompt is in `Art/livewire-prompt.md`.

## Verification

- **15 core and 50 build-variety scenarios passed**, each executing **705 production-effect assertions**. Reports: `v080-core-*` and `v080-variety-*`.
- **Full eight-wave progression passed:** 319 kills, 143 collisions, 13 item choices and two augment choices; zero visible spawns or sequence errors (`v080-full-full-1741.json`).
- Model tutorial checks passed (174 assertions). **1,375 native interface assertions passed**, using production mouse handlers and text metrics. Ordinary and shared-augment presentation reviews passed, with 64 artwork mappings.
- Eleven native mechanic fixtures produced **132 timed camera frames** and motion traces. Field shapes, mark/discharge feedback, new art and scrollable comparisons were inspected. Representative captures are in `v080-ui/`. These are review aids, not proof of first-time-player comprehension.
- Final Livewire wording was rechecked with native captures, the 1,375-assertion rendered suite and the shared-conductor scenario. The later capture-only addition was smoke-tested with shared-pressure and all 705 mechanic assertions.
- **16 normal-health Full-physics fixed-build games:** all valid; no invalid, missing, duplicate or timed-out cases. Thirteen completed the run. Exact inputs, results and aggregate CSV are in `Balance/v080/`.

Regression fixtures use 5,000 base health to reach all mechanics. Comparisons use normal 280 base health plus actual armor bonuses. They freeze Rare equipment, use two predetermined seeds, and unlock ultimates at the ordinary checkpoint. Each compares a native build with its two-module version. This is a targeted test, not a population balance estimate or an isolated test of either module.

## Fixed-build comparisons

| Build | Native completions | Augmented completions | Observation |
| --- | --- | --- | --- |
| Pressure / paired fists | 2/2 | 2/2 | Both recovered to full health; mean damage taken 55.8 to 49.0. |
| Conductor / two-handed rifle | 1/2 | 0/2 | Native reached wave 8 in both; Focused Tether + Livewire died in wave 7. Narrower coverage is a material cost; this is not an automatic upgrade. |
| Freight / two-handed hammer | 2/2 | 2/2 | Mean collisions 149 to 162 with Follow-through + Piledriver. |
| Sustain / two-handed sword | 2/2 | 2/2 | Both recovered to full health; fewer collisions with Drain Afterfield + Focused Reaping. |

The rifle combination warrants broader playtesting. Two modules changed together, so this sample cannot identify which caused the difference. No numerical buffs were inferred from this small comparison.

A final retrieval guard preserves preparation when a body is already within 0.25m of the impact point. Ballet and shared-sustain were rerun successfully with **706 assertions** each; all four affected normal-health sustain comparisons also completed successfully. That brings the normal-health work to **20 valid games** (16 initial plus four final rechecks). Final source build: `Build/SharedAugments-v080-ready`, assembly SHA-256 `da522c0c16fb3a8bc2d751e71c90b2bd84dd1ef84f1861f9bde84439735cb60d`. The recheck is retained separately in `Balance/v080/results.json`.

## Corrections during verification

New fixtures initially used exact floating-point equality and moved a rigidbody without synchronizing its visible transform; both were corrected. The controlled runner initially placed an ability into a still-locked two-handed position. It now moves the block first, using production move rules. Hidden-window UI captures were black and their text audit failed; those captures were discarded and the authorized visible-window review passed. These failed attempts are not counted as passing runs.

## Provenance and reproduction

Combat regression: `Build/SharedAugments-v080`, assembly SHA-256 `888ed817ff4dbeb92de00db6744a5b1f433c9f5dfbdf9a640e8ffe3c5aab3af8`.

Full-run/initial UI: `Build/SharedAugments-v080-final`, SHA-256 `5f96ca89c8042ff65a02b4d98ae89e3c3e2f9602bd26a6cd198ed7a42ab27ec8`. Fixed comparisons: `Build/SharedAugments-v080-reviewed`, SHA-256 `5b106fb57a4ea0080da3f371e168d56b7ccde971bc5cd83f5ee01afbdab15316`, adding the controlled fixture placement correction. Final wording: `Build/SharedAugments-v080-accepted`, SHA-256 `9a9e0b930cf1db2bb3e1ae23b7a35a4ad8d130164adabdd299261919344a0ce2`. Combat values remained unchanged. The trailer build additionally includes an opt-in native footage recorder and synchronized sound capture.

Build using `Build Windows.ps1`. Pass the executable with `-Player` to `Validate Prototype.ps1 -HeroHealth 5000`, `Validate Variety.ps1 -HeroHealth 5000`, `Validate Variety.ps1 -Builds full -FullRun -HeroHealth 5000`, and both modes of `Validate Tutorial.ps1`.

Mechanic captures: `-mechanicReview -reviewSharedAugments -reviewCapture`. Shared previews: `-vectorQA -qaBuild ui -qaHealth 5000 -qaHoldCheckpoint -presentationReview -sharedAugmentReview -force-d3d11`. Omit `-sharedAugmentReview` for ordinary previews.

Comparisons: `python QA/Balance/controlled-siphon/run.py --cases QA/Balance/v080/cases.json --mode Full --workers 2 --player <player> --out <new-directory>`.
