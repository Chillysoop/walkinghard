# Walking Hard 0.6.1 — isolated Counterguard change

29 October 2026. The approved change reduces Counterguard's Common barrier from **35 to 25**, before the existing rarity multipliers. Capacities are now 25 / 30 / 37.5 / 47.5 / 62.5. Duration remains 3.5 × effect scale seconds; normal conversion remains 80%, with its existing cap. Discharge, other items and sequence rules are unchanged. Combat and tooltip capacities share one function.

Final release player: `Build/Counterguard25-v061-final/WalkingHard.exe`. Assembly-CSharp.dll SHA-256: `67b5f0172e74eb791affff6c2afef1675227f0e0d47957caa7aafd07a31edf2`.

The 1,000-game candidate screening batch and fifteen regression scenarios used the preceding candidate, `Build/Counterguard25-v061/WalkingHard.exe`, SHA-256 `8846be0f7e912549735bbc980ac5b8e3a361ae61ea2d8614f5f4ffcde7911164`. The final player adds only the optional development `barrierMultiplier` override; with no override its gameplay capacity remains 25. Both sides of the final Full comparison use the final player; its startup production probes also pass.

## Same-seed Fast comparison

Both runs use seeds 12000–12999, mixed policies, four workers, eight waves and a 600-second simulated combat budget. The original screening model is unchanged between batches; the gameplay difference is Counterguard capacity. All 1,000 candidate games are valid with no missing/duplicate seeds.

| Measure | Capacity 35 | Capacity 25 |
| --- | ---: | ---: |
| Wins | 415 | 410 |
| Win rate | 41.5% | 41.0% |
| Timeouts | 7 | 6 |
| Wins among the 331 initial Counterguard builds | 307 (92.75%) | 303 (91.54%) |
| Mean absorption in 329 matched equipped-Counterguard runs | 593.43 | 586.61 |

Seven prior wins became defeats, one defeat became a win and one timeout became a win. Strategy deltas were balanced -1 win, gun -1, random -3, and no net change for Pinball, sever or Storm. These are small changes and do not establish a generally balanced ability.

All **651 controls that never owned Counterguard** match every recorded gameplay field, excluding version and wall-clock duration. This supports the isolation of the gameplay change. Full JSON comparison and batch provenance are in `v061-fast-comparison.json`. Raw results remain locally in `QA/Simulations/counterguard25-fast-1000/`.

The candidate batch took approximately **84 seconds**; the earlier 35-capacity baseline took 88.30 seconds. Throughput is workload/hardware dependent. The Fast mode executes real authored combat/drafting at 60 Hz but approximates geometry, motion and body contacts. Initial Full/Fast calibration agreed in only 23 of 32 cases; these rates are screening results, not predictions of actual player win rates. No confidence claim is made that a 0.5-point aggregate change generalizes beyond these seeds.

## Interpretation and next proposal

Reducing capacity by 28.6% reduced mean matched absorption by only 1.15%. This suggests capacity was often not the limiting factor in this sample. Counterguard builds remain unusually successful in the screening model, including builds ending with a Common copy; acquisition and survival effects still confound item-level win rates. A subsequent isolated **base duration test of 3.5 → 2.5 seconds** is the recommended next experiment, to create clearer gaps between shields. That duration change is **not applied**. The exact value remains a hypothesis requiring approval and matched-seed/full-physics checks.

Duelist Jacket, Razor Cyclone and Recovery Treads changes from the previous screening report are also not applied. Do not tune them simultaneously with this test and attribute the combined outcome to Counterguard.

## Runtime and validation scope

The release also packages the previously tested optional Fast mode. Full remains the default for simulations and regular play always uses PhysX. The comparison script rejects Full/Fast balance comparisons; paired mode comparisons are explicitly model-calibration work. The in-game version label now uses the actual application version.

New production probes verify shield capacity at all five rarities and that a hit exceeding the shield by five applies exactly five health damage while banking the correct absorbed damage. Existing probes continue to cover Discharge replacement, reloads, item ownership, bounded collisions and augment behavior. The shield graphic itself is unchanged; its capacity text is generated from the tested runtime function.

All **15 standalone Full-mode regression scenarios passed**, with **66 production-effect assertions per scenario**: sequence (seeds 1741 and 4319), sword, hammer, rifle, twin, tempest, manual ultimate, reroll, half speed, fast speed, armor, quick equip, armed casting and reload. The regression named `fast` exercises playback speed; it is distinct from the optional Fast simulation model. Individual results are `QA/v061-<scenario>-<seed>.json`.

No new native visual review was performed. The version label now reads the application version and Counterguard tooltip capacity uses the same runtime function as damage absorption; layout and shield art are unchanged.

## Full-physics confirmation

Two six-game batches used the **same final executable**, seeds 12000–12005, two workers per batch, mixed policies, eight waves and a 600-second combat budget. The baseline passed `{"items":[{"id":"Counterguard","barrierMultiplier":1.4}]}`; the candidate used authored values. Startup probes run before overrides and passed all 66 assertions in every worker. All **12 games were valid**, with no missing or duplicate seeds.

| Policy / seed | Capacity 35 | Capacity 25 | Kills before → after |
| --- | --- | --- | ---: |
| random / 12000 | Defeated | Defeated | 132 → 155 |
| balanced / 12001 | Complete | TimedOut | 299 → 298 |
| pinball / 12002 | Complete | Complete | 300 → 300 |
| storm / 12003 | TimedOut | TimedOut | 300 → 300 |
| sever / 12004 | Defeated | Defeated | 204 → 169 |
| gun / 12005 | Complete | Complete | 301 → 301 |

Five of six outcomes agree. However, **all four controls that never owned Counterguard (random, sever, Storm and gun) differ in recorded gameplay**, despite using the same binary and unchanged applicable tuning. This reproduces the variance seen in an earlier historical-binary comparison. The Full sample therefore cannot isolate a capacity effect; in particular, the balanced win becoming a timeout is not established as a tuning regression. These twelve runs verify runtime integrity, not a reliable win-rate delta. Do not conclude that the Fast-model balance effect is confirmed by Full physics.

Per-seed health, absorption, collisions, outcomes, changed fields and full batch provenance are in `v061-full-comparison.json`. Raw paired batches are preserved locally under `QA/Simulations/counterguard-full-matched-35/` and `counterguard-full-matched-25/`. No additional balance changes were made in response to this noisy sample.

The launcher passed checksum rejection, failed-build rollback, archive-containment and offline-fallback checks. All changed PowerShell entry points parse successfully.
