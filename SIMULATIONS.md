# Headless balance simulations

## Fast balance screening

Use `-Mode Fast` for high-volume screening. On the Ryzen 5 7600 test machine, 1,000 eight-wave mixed-policy games with four workers completed in **88.30 seconds**, including worker startup, with no invalid, missing or duplicate games. That benchmark used `Build/FastBalance-v2/WalkingHard.exe`; the current player is `Build/EnemyPressure-v064-final/WalkingHard.exe`, with Counterguard capacity reduced from 35 to 25. The 0.6.4 final matched batch completed in 104.26 seconds; see QA/v064-balance-review.md for the new ultimate progression and enemy changes. Release 0.6.1 and later include Fast mode; older v0.6.0 downloads support Full mode only.

```powershell
& '.\Run Simulations.ps1' -Player '.\Build\EnemyPressure-v064-final\WalkingHard.exe' `
  -Mode Fast -Runs 1000 -Workers 4 -Waves 8 -Seed 12000 `
  -MaxCombatSeconds 600
```

Fast mode executes the same authored combat, drafting, rarity, fusion, augment, damage, timing and attribution code at 60 Hz. It uses minimal actor transforms and an approximate movement/contact model instead of animated meshes, articulated ragdolls, fragments and PhysX constraints. Launched enemy identities use swept-circle contacts and the existing collision budgets. Idle body pushing, limb contacts, wall occlusion and body packing are not reproduced faithfully. Regular play and `-Mode Full` continue to use PhysX.

This is a screening model, not a calibrated replacement for the game. In the first 32 matched seeds it agreed on the outcome for 23 games; Full won 19 and Fast won 10. Gun, lightning and collision builds were often understated. Do not use a low Fast win rate alone to buff those builds. Rank hypotheses with contributions and comparable cohorts, then verify a small representative set in Full mode. Mode is recorded per game and per batch; the comparison script rejects mixed-mode balance comparisons.

The original, pre-change screening report is `QA/Simulations/fast-mixed-1000-20261029/balance-review.md` locally. The approved Counterguard capacity reduction and its separate 1,000-game retest are recorded in `QA/v061-verification.md`; other recommendations remain unapplied. The recorded throughput is hardware/workload dependent, not a universal two-minute guarantee.

## Full physics validation

Run actual Walking Hard combat in Unity without a window, clicking, rendering, or computer-use tools. The Windows player uses its normal 60 Hz combat and PhysX loop, including ammunition, action order, collisions, ragdolls, drafting, augments and fusion. Cosmetic effects are skipped. Several persistent worker processes each play many games.

Build once with `Build Windows.ps1`. A simulation-capable build contains `simulation-capabilities.json` beside `WalkingHard.exe`. Older releases fail early instead of opening a menu and hanging. Run the commands below from the repository directory. PowerShell scripts also work beside the executable in an extracted release.

## Run a batch

```powershell
& '.\Run Simulations.ps1' -Player '.\Build\Windows\WalkingHard.exe' `
  -Runs 1000 -Workers 2 -Waves 8 -Seed 100 -Policy mixed `
  -OutputDirectory '.\QA\Simulations\baseline-1000'
```

Use `-Waves 3` for faster early-game experiments. Use eight waves to evaluate the complete reward schedule, both augment drafts and the boss. Start with two workers; additional workers compete for CPU and physics resources, so measure before increasing the count. Each worker reuses the engine process while resetting actors and run state between games.

`mixed` cycles six drafting policies across starting seeds: random, balanced, pinball, storm, sever and gun. You can also select any one of those policies. The strategies weigh rarity, matching synergy tags, missing equipment and fusion opportunities, then arrange setup actions before their payoffs. They are simple test agents, not optimal players or estimates of human skill. They use automatic ultimates and do not spend banked rerolls or exhaustively search stored-item combinations.

Seeds are unique across workers. Keep the same first seed, run count, workers, policies and wave count for comparisons. Seeded randomness helps reproducibility, but PhysX is not guaranteed to be bitwise deterministic across processes, run history or hardware. Record multiple batches when a result is close.

## Inspect results while running

```powershell
& '.\Summarize Simulations.ps1' -Directory '.\QA\Simulations\baseline-1000'
```

Each completed game is immediately appended to a worker JSONL file. Partial batches remain readable after stopping. The summarizer ignores a partially written final line and produces:

| File | Contents |
| --- | --- |
| `policies.csv` | Wins, timeouts, win rate with a 95% Wilson interval, average wave reached, kills, score and run time |
| `games.csv` | Seed, strategy, outcome and final loadout for every valid game |
| `items.csv` | Equipped item-copy/rarity/module observations: damage, kills, control, absorption, reload/preparation/range wait and assists |
| `summary.json` | Completed, invalid, missing and duplicate seeds, throughput and policy summaries |
| `worker-*.jsonl` | Full per-game contributions and wave reports, including stored items |
| `batch.json` | Input settings, executable path, assembly SHA-256 and timestamps |

Item win rates are correlations among end-of-run equipped copies, not isolated item strength. Strong items may be acquired only by surviving longer; paired identical items count as two copies. Use controlled comparisons and sufficient sample sizes before changing balance. Damage/control contributions explain mechanisms better than win rate alone.

Defeats are valid observations. Invalid state, duplicate ownership, broken kill accounting or missing boss encounters are test failures. A run exceeding the configured simulated combat-time budget (480 seconds by default, adjustable with `-MaxCombatSeconds` from 120 to 600) is recorded as `TimedOut`, not a win. A worker producing no completed game for 600 wall-clock seconds is stopped; adjust `-GameTimeoutSeconds` only when intentionally testing unusually expensive runs. Interrupted batches are preserved and never silently mixed or overwritten.

## Change tuning without rebuilding

Copy `balance.example.json`, then change only the values you want to test. Omitted values or zero retain the authored defaults. A multiplier of `1` is unchanged; `0.9` means 90% of the base value. This input is development-only and does not alter ordinary play or save data.

Supported combat values: enemy damage, enemy health, hero health, first spawn interval and final spawn interval. Item IDs are defined in `LoadoutCatalog.cs`. Active items support damage, action-duration, effect, reload and ultimate-cooldown multipliers where applicable. Counterguard additionally supports `barrierMultiplier`, which changes only shield capacity (for example, 1.4 restores 35 from the authored 25 for a controlled baseline). Effect multipliers modify the host's authored effect scale and gear effect-profile values; fixed target caps, trigger rules and gear stat bonuses stay authored. Ultimate recharge retains the greater-than-10-second floor. A module uses its host's scaling, not a separate rarity or tuning entry.

```powershell
& '.\Run Simulations.ps1' -Player '.\Build\Windows\WalkingHard.exe' `
  -Runs 1000 -Workers 2 -Waves 8 -Seed 100 -Policy mixed `
  -BalanceFile '.\balance.candidate.json' `
  -OutputDirectory '.\QA\Simulations\candidate-1000'

& '.\Compare Simulations.ps1' `
  -Baseline '.\QA\Simulations\baseline-1000' `
  -Candidate '.\QA\Simulations\candidate-1000'
```

The runner copies the balance file into the results directory. Comparisons require complete valid batches with matching inputs, join outcomes by seed and policy, and export per-seed and per-policy deltas. They report losses turned into wins, wins turned into losses, wave/kill/score/health changes and the tested assembly hashes. Changes to later offers caused by the evolving build are part of the policy outcome.

Use these runs to find weak or dominant combinations, then inspect representative seeds in the game. Headless results cannot establish visual readability or whether combat feels satisfying.

Version 0.6.4 reports ranged spawn/hit/damage totals, per-wave ranged pressure, rear-boundary kills and unattributed environmental kills. Kill-accounting validity includes those explicit environmental deaths.
