# Cinderworks roads and crews verification

November 13, 2026. Based on Expedition stops commit `aa726d0330c30fb9e92a1cbb67164a4dda514b79`; prepared as a separate development branch.

## Delivered

Expedition's furnace architecture follows the actual split roads, including the second turn. The original floors, walls and concealed entrances keep their physics; noncolliding bays reuse material-batched meshes. Repair workers, freight brutes and reclaimers enter through normal concealed arrivals, replacing a limited share of ordinary enemies. Waves 2, 3 and 4 introduce their roles separately; waves 5–7 mix them, with at most three living crew members.

Repair grants temporary absorption rather than healing. Brutes visibly aim and throw an existing corpse, with one possible Walker hit and at most two enemy hits. Reclaimers pull physical bodies behind the line. Stagger interrupts channels, and player launches take bodies away from enemy reservations or hostile flight. Silhouettes, names, countdowns, target tethers and throw footprints explain the behavior. Idle role labels are compact; active labels avoid overlap. Iven's canopy and the Overtime press distinguish service props.

Schema 2 preserves roles, reservations, shields, channels and hostile-flight hit budgets. Schema-1 checkpoints remain loadable, with neutral defaults for previously saved enemies. Quick Run and the practice comparison retain their ordinary appearances when leaving Expedition. Exact gameplay values are in [DESIGN.md](../DESIGN.md).

## Evidence

- [Native crew review](cinderworks-crews/native/report.json): **37 assertions**, plus **761 production-effect assertions**. Checks cover interrupting repair/retrieval/throw windups, protection capacity and expiry, offscreen/entering restrictions, walls, exclusive corpse reservation, physical retrieval, stealing hostile flight, one Walker hit and only two of three enemy contacts, exact model-graph checkpoint restoration, hostile-flight persistence, legacy migration, unchanged collider geometry and skeletons, two physical road turns, and restoring Quick Run/practice styles.
- [Normal-health Expedition](cinderworks-crews/full-run/report.json): all eight waves completed with **318 kills and 222.1 integrity**. Normal offers and automated pinball preferences retained **13 item choices and two augment choices**. All roles appeared through concealed spawning; workers completed **8 repairs, 9 throws and 8 retrievals**. Four checkpoints were restored during the run. No sequence-order or visible-spawn errors; living/dead identities remained consistent. This is a functional sample, not comparative balance evidence.
- [Expedition regression](cinderworks-crews/expedition/report.json): **659 assertions** plus **761 production-effect assertions**, covering event probabilities, fixed rolls across reloads, cashout/failure, purchases, fusion, shared ownership, invalid-save rejection and complete state round trips.
- [Fresh-process resume](cinderworks-crews/fresh-process/report.json): **5 assertions** restore purchases, roads, a severed body and flight. [Legacy-process resume](cinderworks-crews/legacy-process/report.json): **5 assertions** load a copy of the actual schema-1 checkpoint produced by the previous build, not just a synthetic migration fixture. The original checkpoint and the live player profile were untouched.
- [Combat regressions](cinderworks-crews/regressions.json): all **15 core scenarios** and the **eight-wave progression scenario** passed, each with 761 production assertions. These use 5,000 starting health for mechanics coverage. Item variety was not expanded in this pass; the 50-case catalog suite was not repeated.
- [Regional preview regression](cinderworks-crews/preview/report.json): **181 assertions** and **1,522 production-effect assertions** passed on the final assembly. All seven practice setups ran in both appearances; 507 original world colliders retained their identities and bounds, and architecture used 258 batched renderers. Sequential 0.5×, 1× and 4× performance samples are recorded for the practice encounter; they are not whole-Expedition or general hardware benchmarks.

Inspected native captures include mixed crew warnings at 1280×720 with reduced motion, 1920×1080 role windups, the physical Y-junction, the second rotated road, service UI, crowded wave-six combat and the Pursuer final stand. The controlled captures use elevated health and deliberately staged actors; the full-run captures use ordinary health, spawning and offers. Accelerated review stepping is not a frame-rate measurement.

The event regression deliberately logs one malformed-JSON rejection. No unexpected save/load warnings or runtime exceptions occurred in the successful reviews. An older Cinderworks preview review needed its setup corrected: ordinary route probes must temporarily leave practice mode, which intentionally suppresses forks. Earlier failed fixture captures remain under the ignored local Build directory; committed reports are from successful runs.

[Source and binary provenance](cinderworks-crews/provenance.json) records the exact assemblies and changed source-file hashes. Core/progression checks precede only the practice material-reset fix and its native assertion. The crew, event, migration and full-Expedition reviews precede only the older preview-review setup correction. Those final changes do not alter their tested combat paths.

## Reproduce and limits

Build with Unity 6000.3.27f1 using `Build Windows.ps1 -OutputDirectory <player-directory>`. Run `Validate Prototype.ps1 -Player <player> -HeroHealth 5000` and `Validate Variety.ps1 -Player <player> -Builds full -FullRun -HeroHealth 5000`.

Native crew fixtures: `-crewReview -seed 1741 -reviewOutput <fresh-output> -saveDirectory <isolated-profile> -logFile <log>`. Add `-crewFullRun` for normal-health progression. Event regression uses `-expeditionReview`; then use `-expeditionReview -resumeOnly` in a fresh player process with the same isolated profile. All rendered reviews close themselves. Never point these fixtures at the player's real profile.

The regional practice regression uses `-cinderworksReview -reviewOutput <fresh-output> -logFile <log>`. Run it sequentially without other tests when interpreting its performance samples.

This is the regional world and enemy-roster slice within the existing eight-wave format. The Foreman, longer act graph, hub and additional items are still proposals. Crew tuning needs broader playtesting across builds. Physics is not promised bit-identical across sessions or hardware. The tested player is a development build; the launcher and published release are unchanged.
