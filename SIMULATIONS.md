# Headless balance simulations

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

Supported combat values: enemy damage, enemy health, hero health, first spawn interval and final spawn interval. Item IDs are defined in `LoadoutCatalog.cs`. Active items support damage, action-duration, effect, reload and ultimate-cooldown multipliers where applicable. Effect multipliers modify the host's authored effect scale and gear effect-profile values; fixed target caps, trigger rules and gear stat bonuses stay authored. Ultimate recharge retains the greater-than-10-second floor. A module uses its host's scaling, not a separate rarity or tuning entry.

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
