# v0.8.0 normal-run rollout verification

November 2, 2026. Unity 6000.3.27f1, Windows / D3D11, Ryzen 5 7600 and NVIDIA RTX 3080. This is the final rollout of the shared-augment and presentation work documented in the earlier milestone reports.

## Delivered behavior

Normal runs now use armored characters, weapon silhouettes and bounded impact/gather/ultimate effects. The checkpoint portrait uses the same presentation and equipped weapons. Conductor Wraps have knuckle coils; Siphon Blade has a hooked crimson channel; shooters carry a pistol and aim from their weapon hand. Harvest gathering uses its crimson effect color. Armor decorations sharing a material are combined within each existing bone and cached, preserving the original eleven bones, colliders, cuts and independent ragdoll movement.

Focused Tether is 30% narrower than native, with two extra metres of reach. Each wave has an authored entry pattern; waves 4–7 alternate three short gaps and one longer gap at an unchanged average scheduled interval. A short formation banner explains the encounter. Damage, rarity/fusion, simultaneous pairs, two-handed groups and the 13-item/two-augment reward schedule remain unchanged.

Combat Preview remains available with seven fixtures and original/new comparison. Exiting restores the ordinary presentation. `-originalLook` is a development comparison switch; normal play uses the armored presentation.

## Gameplay and interface checks

- **15 core scenarios, 50 build-variety scenarios and one full eight-wave progression passed**, each with **706 production-effect assertions**. The full progression fixture recorded 320 kills, 133 collisions, 13 item choices and two augment choices; no order errors or visible entry violations. These fixtures use 5,000 base health to reach every mechanic and do not estimate normal difficulty. The aggregate is [v080-fullrun-regression.json](v080-fullrun-regression.json); individual reports use `v080-final-core-*`, `v080-final-variety-*` and `v080-final-run-*`.
- **174 tutorial model checks and 1,375 native tutorial interface assertions passed.** Native screenshots include the updated portrait, fusion, equip destinations and sequence. The preview suite passed **204 assertions plus 706 production-effect assertions in each look**: seven fixtures, paired/locked weapons, sever bones, real ultimates, 0.5×/1×/4× reduced motion, bounded effects and restoring the normal run.
- **1,824 Fast games and 76 new Full/PhysX games** support the balance review, including ordinary drafts across six policies, isolated Tether coverage and fixed paired/2H comparisons. All rows are valid; one baseline Full timeout is retained explicitly. See [balance review](v080-fullrun-balance.md).
- Initial visible ordinary drafts covered gun seed 83007, storm seed 83011 and cutcarry seed 83008. Gun and storm completed all eight waves; Cut & Carry was defeated in wave six. Every report passed its accounting/sequence assertions. The final batched-armor native run repeated gun seed 83007 and completed with **318 kills, 13 item choices, two augment choices, 72.64 health and 55 native assertions**.
- Native stills were inspected for the portrait, Wraps/Siphon identities, aimed shooters, formation banner, combat sequence, ultimate and finale. They demonstrate rendered behavior; first-time-player comprehension still requires human playtesting.

The final batched build repeated the 204 native preview assertions, 1,412 production assertions across both looks and 1,375 rendered tutorial assertions successfully. Four affected smoke scenarios (Ballet, shared Freight, shared Sustain and Funnel) also passed 706 production assertions each and exactly matched their pre-batch kills, collisions, health, launches, severs and slot-cast counts. Final portrait and sever/combat captures were inspected.

## Full-run performance

Measurements are wall-frame intervals at 1920×1080 with a 60 FPS cap. The harness uses normal health, real drafts and automatic ultimates, fast-forwarding between real-time two-second samples. The original look, armored look and batched-armor candidate were measured sequentially with no simulation workers running. Short samples and PhysX variation limit precision; these are not a hardware-independent FPS guarantee.

| Scene / speed | Original median / p95 | Armored median / p95 | Final batched armor median / p95 |
| --- | ---: | ---: | ---: |
| Wave 6, 1×, ~174 bodies | 16.67 / 16.70 ms | 16.67 / 16.70 ms | 16.67 / 16.70 ms |
| Wave 6, 0.5× | 16.67 / 16.69 ms | 16.67 / 16.69 ms | 16.67 / 16.70 ms |
| Wave 6, 4×, ~180 bodies | 28.66 / 33.39 ms | 27.53 / 32.44 ms | 25.93 / 33.62 ms |
| Final stand, 1×, 267 bodies | 16.68 / 17.34 ms | 16.68 / 18.99 ms | 16.67 / 17.03 ms |
| Separate 42-enemy fixture, 1× | 16.67 / 16.69 ms | 16.66 / 16.72 ms | 16.66 / 16.70 ms |
| Separate fixture, 4× reduced motion | 16.67 / 16.70 ms | 16.67 / 16.69 ms | 16.66 / 16.69 ms |

All ordinary 1× samples in the final build had medians near 16.67 ms. The 42-enemy fixture retains its own 29–34 corpses; it is **not** a 42-enemy test with the preceding run's 318 corpses. The full-run finale sample separately includes 267 persistent bodies and up to 38 live enemies. No bodies were removed or physics simplified for this pass. The effect pool remained within 160 slots.

**Known limit:** accumulated-body scenes at 4× still fall below 60 FPS. The final sample is about 39 FPS median, with most measured combat CPU time in body shoving/PhysX (17.94 of 20.96 ms per sampled frame). The original look has a similar limit. Batching reduces presentation objects; this small sample does not establish a significant simulation-speed improvement. It does not fix the underlying physics cost.

Earlier samples taken while three background test workers ran were much slower: gun wave-six 4× reached 117.90 ms median and the finale 30.87 ms. Those reports remain in the evidence directory with their concurrency noted. The quiet reruns establish the normal-speed gate without hiding that interference.

## Provenance and reproduction

[v080-fullrun-build.json](v080-fullrun-build.json) records the final assembly SHA-256 and canonical Git hashes for source/assets/settings. The release script checks both, checks the player version against its tag, and requires a clean checkout at the exact remote-main commit before packaging. The generated scene's random file-ID churn was reverted; scene content and object references are unchanged.

`v080-fullrun-ui/` contains native reports, concurrency provenance and representative images. Original/armored quiet controls used `Build/FullRun-profile`; the final batched build is `Build/FullRun-batched`. Compared with the initial regression/balance build, the final build only adds diagnostic capture/timing and combines decoration meshes within individual bones. It does not change combat or physics rules.

Reproduce the ordinary native review using the command in [SIMULATIONS.md](../SIMULATIONS.md); add `-originalLook` for the control. Run `-combatPreviewCheck -previewOutput <absolute-directory> -force-d3d11 -screen-fullscreen 0` for the preview suite. `Validate Tutorial.ps1 -Rendered` exercises actual interface handlers. Regression commands use `Validate Prototype.ps1 -HeroHealth 5000`, `Validate Variety.ps1 -HeroHealth 5000` and `Validate Variety.ps1 -Builds full -FullRun -HeroHealth 5000`.
