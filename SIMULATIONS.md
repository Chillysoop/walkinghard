# Headless balance simulations

## Journey pacing comparisons (0.8.5)

`Validate Progression.ps1 -OriginalBellThreshold -OriginalArrivalPacing` selects the former Bell damage threshold and ordinary reinforcement spacing in the same binary. Omit either switch to isolate its candidate behavior. Use fresh output directories and identical seeds, policies and unlock pools; each manifest and sample records the switches. `Validate Districts.ps1 -Mode Balance -OriginalBellThreshold` isolates the boss threshold with fixed kits. PhysX trajectories can still vary, so a single outcome change is not a win-rate estimate.

Samples include `pacing.encounters`: actual generated formation, simulation seconds, unique sequence target wait (arrival/blocked subsets), cleanup seconds, incoming damage, largest rolling five-second damage burst, peak nearby/alive foes, arrivals and carryover. Bell break counters identify the finishing contribution—damage, control or clearing the linked escorts—not an exclusive cause when contributions combine. Simulation seconds exclude hit stop; the existing review `seconds` includes the 1x combat clock and boss reveal. Automated records explicitly leave human decision time unmeasured.

`-progressionChecks` and the district mechanics suite check timing categories, snapshot ownership, schema 9 migration to partial timing, invalid-save preservation, actual formation labels and event/combat separation. Rendered progression checks show **synthetic** timing values at 720p, 1080p and ultrawide to verify layout. Live saved timing excludes time between sessions and discarded attempts rolled back to a checkpoint. The normal player writes `journey-timing-latest.json` beside its expedition checkpoint after victory or defeat; this stays local.

## Districts and bosses (0.8.4)

`Validate Districts.ps1 -Mode Mechanics -OutputDirectory <fresh-directory>` checks boss phase transitions, per-action control budgets, physical rear arrivals on straight/left/right roads, paused clocks, visibility, schema-8 migration and unchanged scenery collision geometry. `-Mode Rendered` adds 1080p/720p captures with reduced motion. Both mechanics fixtures use elevated health and staged positions; they are not balance samples.

`-Mode Balance -Seed 1741` runs eight fixed kits against each of the three journey bosses with full PhysX, normal health, Rare weapons, Uncommon supports/ultimate and no augments. Real approaches and reinforcement budgets remain active. Defeat is a valid outcome; timeouts and invariant failures fail the review. Manifests retain the tested assembly hash. Boss cycle counts mean attempted strikes, including misses or absorbed hits; incoming health damage is reported separately.

Use ordinary `Validate Progression.ps1` runs for the whole 21-encounter journey and inspect both completed and fatal encounters. Its `bosses` records preserve cycles, strikes, forced pressure/command breaks and completed rallies (kind 0 = Shift Foreman, 1 = Bell Foreman, 2 = Pursuer). Fixed-kit clear times and automated combat time do not establish human difficulty or a 30-minute human playtime. See `QA/district-bosses/verification.md` for initial results and limitations.

## Weapon-discovery comparisons (0.8.3)

`Validate Progression.ps1 -Policies gun,cutcarry,close -Seeds 1741,4319,6229 -CommittedWeapons -Workers 3 -OutputDirectory <fresh-directory>` runs committed weapon policies. Omit `-CommittedWeapons` for adaptive drafts. Add `-OriginalWeaponOffers` for the same binary's original reward rules. Match pools and seeds. Output names distinguish offer rules and commitment; manifests record both and the assembly hash. Direct player flags are `-committedWeapons` / `-originalWeaponOffers` with `-progressionReview`. The old item-tag drafter cannot be combined with committed mode.

Committed guns allow ranged weapons and starter-fist placeholders; sword/hammer requires a 1H sword in hand 1 and a 1H hammer in hand 2; close quarters requires fists in hand 1 and a 1H gun in hand 2. Missing components are prioritized when available. Gear, abilities and ultimates still adapt, so these are natural-loot builds, not controlled equal-rarity comparisons. No items are injected. Inspect `weaponEncounters` for actual equipment, ranks, paired/2H layout, damage, target waits, reload time, healing and health, including the fatal encounter. Encounter seconds use the active simulation clock and exclude hit stop, reveal and decisions; the run-level `seconds` also includes hit stop/reveal. Contribution seconds for two paired weapons can overlap.

`python "Analyze Weapon Discovery.py" <cohort-directory> --output <summary.json>` groups actual outcomes and surfaces attempts without a result. It reports availability, assembly, damage and waiting; it does not infer human win rates. General progression analysis also separates commitment and offer rules when rejecting duplicates.

`-weaponDiscoveryChecks -saveDirectory <isolated-profile> -reviewOutput <fresh-directory>` checks offer invariants across 1,024 shadow reward sequences, both unlock pools, committed mounts, fusion and save migration. Shadow decks take the unchanged first card to isolate reward composition; they are not combat runs. Add `-batchmode -nographics` for assertions only, or `-force-d3d11 -screen-fullscreen 0` for 720p/1080p/ultrawide captures. `-ragdollStress` with the same profile/output arguments runs 56 Full PhysX launch, sever, wall and fork fixtures. Failures preserve the case, step and existing bone diagnostic; passing cannot prove a rare outlier is fixed.

The discovery suite also reproduces the old junction/entrance collision, tests both fork sides at several offsets, retains occupied entry rooms, and walks champion-sized actors out through the retained passages (44 additional assertions). Rendered fixtures hold Running state while their isolated deck has no offers.

For progression stalls, add `-progressionTrace` to save state every 120 review seconds. `-progressionSeconds <60..2100>` can shorten a diagnostic reproduction; the normal limit remains 2100. A timeout writes live actors, entry paths and collider blockers to `timeout-state.txt` and captures the scene when rendered. `-progressionResume` uses an isolated checkpoint with one seed/policy; its reported seconds and encounter samples cover only the continuation, so do not mix it into full-run cohort summaries. Diagnostic time caps are not valid combat defeats.

## Current 21-encounter progression review (0.8.2 onward)

The public Play path is the long journey. Older Quick Run / eight-wave commands below are regression fixtures, not additional player modes. For natural progression use the Windows player with:

```
-batchmode -nographics -progressionReview -seeds 1741,4319 -journeyPolicy all -pool unlocked -saveDirectory <isolated-profile> -reviewOutput <fresh-directory> -logFile <log-path>
```

Policies: `pinball`, `storm`, `sever`, `gun`, `cutcarry`, `close`, `siphon`, or `all`. `-pool fresh` uses the starting reward pool; default `unlocked` enables all six constellation unlocks and resets the Office between runs. These are build preferences, not fixed or guaranteed loadouts. Runs use normal integrity, Full PhysX, actual rewards and nine physical forks. Time includes hit stop at 1x, excluding human decisions. Defeat is a valid result; timeout and broken invariants are failures. Output paths must be fresh.

The projected policy evaluates detached production receive/fusion/move results, including both hands, pair timing, 2H commitment, rarity milestones, explicit producer/consumer links, and stored partners when changing weapon layouts. It fills utility slots vacated by fusion and prepares Drain Brand before weapon damage. Approximate contact coverage and build-preference weights are test-driver heuristics, not damage predictions or proof of optimal play. It does not add a player suggestion panel or automatically equip anything for players. `-legacyDraftPolicy` retains the old item/tag policy for comparison; its mistakes and undefined Siphon preference limit that baseline.

Each JSONL sample records loadout, draft offers/selection, fusions, item contributions, completed-encounter survivors, incoming damage and the fatal encounter. Upgrade/defense opportunity flags use the heuristic; exact-rarity copy and weapon availability are literal facts. Shop purchases are not reward offers. Run `python "Analyze Progression.py" QA/progression --output QA/progression/summary.json` to summarize named `projected-*` / `legacy-*` sample directories. It rejects duplicates and reports survival-censored gaps. Inspect actual loadouts before comparing style labels. Match seeds and reward pools; bot win rates alone do not establish weapon balance.

`-progressionChecks` with an isolated save directory verifies rarity retention, paired fusion, coordinated weapon swaps, policy identity, incoming damage, fatal-wave capture and save migration. Omit `-batchmode -nographics` and add `-force-d3d11 -screen-fullscreen 0` for results-panel captures at 720p, 1080p and ultrawide. Use the same review output/profile arguments.


Expedition stops are opt-in through the title menu. Existing Full/Fast batches remain Quick Run and do not write player saves. Native `-expeditionReview -reviewOutput <directory> -saveDirectory <isolated-profile>` checks real checkpoint restoration, chance outcomes, purchases and physical forks. Add `-resumeOnly` in a fresh player process to verify the retained service fixture, or `-expeditionFullRun -seed 1741` with a different isolated profile for the eight-wave Expedition regression. The staged fixtures elevate combat health; the full Expedition run uses normal health and ordinary offers. These establish functionality, not economy balance. Do not point a review at a real player profile.

Ordinary runs choose roads at physical Y-junctions in waves 2 and 5. Automated policies select at the start of the approach using the existing seed/checkpoint policy (or forced encounter override), then wait for the same deadline as regular play. Full and Fast modes follow the selected road's world-space centerline and direction. Full uses the actual separate floors, walls and entrance colliders; Fast applies its approximate local-road bounds and retains its documented contact limitations. Controlled fixed-loadout cases still omit route choices. Actual road geometry, navigation and camera framing can affect combat outcomes even though item stats and proc budgets are unchanged. Native `-walkingForkReview` separately checks live selection, real separating walls/floors, both turns, two successive forks, rotated Pursuer presentation, pause, timeout, reward continuity and 1080p/720p presentation; simulation success alone does not establish clarity.

## Fast balance screening

Use `-Mode Fast` for high-volume screening. On the Ryzen 5 7600 test machine, 1,000 eight-wave mixed-policy games with four workers completed in **88.30 seconds**, including worker startup, with no invalid, missing or duplicate games. That benchmark used `Build/FastBalance-v2/WalkingHard.exe`; the current player is `Build/DefenseBalance-v066-final/WalkingHard.exe`, with Counterguard capacity reduced from 35 to 25. The 0.6.4 final matched batch completed in 104.26 seconds; see QA/v064-balance-review.md for the new ultimate progression and enemy changes. Release 0.6.1 and later include Fast mode; older v0.6.0 downloads support Full mode only.

```powershell
& '.\Run Simulations.ps1' -Player '.\Build\DefenseBalance-v066-final\WalkingHard.exe' `
  -Mode Fast -Runs 1000 -Workers 4 -Waves 8 -Seed 12000 `
  -MaxCombatSeconds 600
```

Fast mode executes the same authored combat, drafting, rarity, fusion, augment, damage, timing and attribution code at 60 Hz. It uses minimal actor transforms and an approximate movement/contact model instead of animated meshes, articulated ragdolls, fragments and PhysX constraints. Launched enemy identities use swept-circle contacts and the existing collision budgets. Idle body pushing, limb contacts, wall occlusion and body packing are not reproduced faithfully. Regular play and `-Mode Full` continue to use PhysX.

This is a screening model, not a calibrated replacement for the game. In the first 32 matched seeds it agreed on the outcome for 23 games; Full won 19 and Fast won 10. Gun, lightning and collision builds were often understated. Do not use a low Fast win rate alone to buff those builds. Rank hypotheses with contributions and comparable cohorts, then verify a small representative set in Full mode. Mode is recorded per game and per batch; the comparison script rejects mixed-mode balance comparisons.

The original, pre-change screening report is `QA/Simulations/fast-mixed-1000-20261029/balance-review.md` locally. The approved Counterguard capacity reduction and its separate 1,000-game retest are recorded in `QA/v061-verification.md`; subsequent Counterguard-duration and Duelist-ward tuning is recorded in `QA/v066-balance-review.md`. The recorded throughput is hardware/workload dependent, not a universal two-minute guarantee.

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

`archetypes` cycles all six build directions: pinball, storm, sever, gun, cutcarry and close. `cutcarry` favors a one-handed sword and breach hammer; `close` favors fists/wraps and a pistol/scattergun. Each still uses ordinary random offers and progression. The older `mixed` set remains available for historical comparisons.

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


### Isolated defensive tuning (0.6.6)

`QA/Balance/v066/` contains exact Counterguard-only, Duelist-only, combined, and legacy-control inputs. Counterguard accepts `barrierSeconds` (base seconds before rarity/effect scaling), independent of capacity or action time. Duelist accepts `wardSeconds` (base seconds before rarity scaling), `wardHits` (1-4), and the development-only `consumeBlockedWard` flag to reproduce the old behavior of spending the ward on a fully absorbed hit. Zero/omission keeps authored values; only the corresponding item IDs accept these fields. No override input is loaded in normal play.

A 32-game legacy control matches all 27 gameplay fields from the prior screening baseline. Three matched 1,000-game Fast batches test each recommendation separately and together. The Full comparison uses seeds 12654-12665, selected before running because the prior Fast baseline equipped Duelist in six of those twelve games; it is a targeted mechanism check, not a representative win-rate sample. `QA/Balance/v066/analyze.py` rebuilds the aggregate report from local raw batches. See `QA/v066-balance-review.md` for results and limitations.


0.6.7 experiments add opt-in `legacyWeaponAugments`, `legacyCounterBurst` and `excludeSiphon` switches to balance JSON. The default player uses the new rules and full catalog. The legacy combination reproduces 0.6.6 for matched controls. Exact inputs are in `QA/Balance/v067`. Item contribution records and CSVs now include actual `healing`, distinct from absorption.

### Controlled slot comparisons (analysis only)

`-simCases <json>` opts into explicit, fixed per-game loadouts and seeds instead of drafting. The case count must equal `-simRuns`. `QA/Balance/controlled-siphon/run.py` generates a matched matrix covering five focal rarities, four build contexts and one-slot substitutions. Each worker receives its own case list. Fixed gear/abilities start equipped; ultimates still enter after checkpoint three with normal cooldowns. No further rewards or augments are selected. These fixtures measure item substitution under stated builds, not ordinary progression difficulty.

`QA/controlled-siphon-review.md` records the earlier v0.6.7 analysis: 3,600 Common-support cases, 480 Rare-support sensitivity cases and 24 Full physics cases, plus the recommendations awaiting approval at that time. `summary.csv`, `games.csv`, `results.json`, exact sensitivity/Full inputs and reproducible analysis scripts live in `QA/Balance/controlled-siphon/`. Full raw JSONL stays in the ignored `QA/Simulations/` directory. Ordinary simulations without `-simCases` use normal drafting. The controlled runner uses authored rules by default; `--balance <json>` explicitly opts into a development override and records it in batch provenance.


0.6.8 adds development-only `legacySiphonBlade`, `legacyReclaimer` and `legacyRedline` balance switches. Defaults use the new rules. `QA/Balance/v068/` tests each change independently, reconstructing the complete 4,080-case controlled matrix using verified unchanged baseline cases; it records actual rerun counts separately. Source collection for observed drain and production healing is shared to prevent measurement drift.

0.6.9 adds optional `matchedWeapons: true` to fixed cases, giving both carried weapons the focal rarity. The default stays false for previous experiments. `QA/Balance/v069/` compares gun/sever weapon layouts, ultimates and armor at matching rarities, plus a Rare-support sensitivity slice. Read `QA/v069-balance-review.md` before interpreting zero-win frozen Common-support builds as ordinary-run difficulty. Proposed rifle and Duelist changes are not applied. Recovery Treads now reports restoration under healing, and fusion preserves all delay totals; these reporting repairs do not change combat.

0.7.0 ordinary runs choose Shield Line or Volatile Breach after waves 2 and 5 using seed/checkpoint parity. The route biases that checkpoint's existing mixed reward and changes the following wave. Results include route labels in wave reports and counts for shield bearers, carriers and detonations. Fixed `-simCases` experiments retain standard encounters for continuity. `QA/Balance/v070/experiment.py` and `full.py` reproduce separate, unapplied rifle and Duelist tests; `analyze.py` compares results and `verify.py` checks release provenance. Use fresh output directories when rerunning them.

0.7.1 replaces the wave-eight Warden with the stationary-player Pursuer finale, including in controlled loadout experiments. Headless execution skips the real-time camera orbit without advancing combat timers and uses the same end-of-reveal actor positions as rendered play. New balance comparisons must use matching executable versions: wave-eight results are not directly interchangeable with v0.7.0. Earlier waves retain their rules. Full physics checks verify slow giant locomotion against floor friction; Fast alone cannot verify it.

0.7.2 adds optional controlled-case `sequence` (all three ability IDs plus `basic0`/`basic1`; two-handed basics remain adjacent) and `route` (0 standard, 1 Shield Line, 2 Volatile Breach on waves 3/6). Ordinary runs accept `-simEncounterRoute alternating|standard|shield|volatile` and `-simRewardRoute linked|standard|shield|volatile`, independently, for reward-versus-encounter isolation. Defaults retain ordinary play. The Full route self-test temporarily suspends controlled-case inputs so it can check ordinary checkpoint rules. Item contributions now include `waitEpisodes`, counting continuous target waits of at least half a second. `QA/Balance/v072/` reproduces the range, fixed-route, reward-only, linked-route and legacy timeout studies.


Fixed cases may optionally provide `modules`: five strings addressing weapon mounts 0/1, then the three abilities in authored input order. Empty strings leave sockets empty; at most two modules are allowed. A shared or two-handed host cannot receive two modules. `QA/Balance/v080/cases.json` compares native and augmented versions of four Rare builds at normal health across two seeds, using Full physics. These are targeted checks, not population balance estimates.


### Full-run rollout study (0.8.0)

`QA/Balance/full-run/` records six-policy ordinary-draft screens, matched Full-physics runs, an isolated Focused Tether comparison and paired/2H fixed-build checks. `focusedTetherWidth` accepts a fraction between zero and one (zero keeps the authored 0.7); `legacyEncounterRhythm: true` restores the prior round-robin entries and steady intervals. Early study binaries used the inverse `encounterRhythm` field, so saved experimental inputs carry both flags for reproduction across those builds. Assembly hashes identify the actual executable.

For native full-run review, launch a visible player with `-fullRunReview -simRuns 1 -simMode Full -simPolicy gun -seed 83007 -simWaves 8 -simMaxSeconds 600 -simOutput <fresh-absolute-jsonl> -reviewOutput <absolute-directory> -force-d3d11 -screen-fullscreen 0`. It uses normal health, ordinary automatic drafts and ultimates, saves checkpoint/combat images, and samples actual frame intervals at 1080p. It fast-forwards between samples. The final crowded fixture is explicitly separate from the ordinary-run result; it does not estimate build strength. Run without competing simulation workers when measuring performance.

### Combat-flow review

Run `-combatFlowReview -flowOutput <absolute-directory> -force-d3d11 -screen-fullscreen 0` for seven isolated full-physics Pursuer spacing fixtures, production mechanic checks, and native wait/paired-action/recap captures. Adding `-batchmode -nographics` keeps the mechanic and spacing checks but skips screenshots. Fixtures have durable targets and elevated hero health; they measure contact access, not build balance. Normal balance comparisons use ordinary `-Policy archetypes -Mode Full` runs with matching seeds and unchanged health.

Contributions retain `rangeWait` as total target-wait seconds for report compatibility. New `outOfReachWait` and `obstructedWait` values separate reach and path problems; the remainder is time without an eligible enemy. Old report files lack these classifications and must not be retroactively interpreted as zero range waiting. See `QA/combat-flow-verification.md` for measurements and the precise build provenance.

### Foreman and crew comparisons

Run `-foremanReview -seed 1741 -reviewOutput <fresh-absolute-directory> -saveDirectory <isolated-profile> -logFile <log>` for rendered pressure, vent, slam, reach and checkpoint checks. The native mechanics fixture has elevated health. Add `-batchmode -nographics` to omit captures.

Add `-foremanBalance -balanceFirstSeed 91201 -balanceSeedCount 5` for 160 matched Full-physics encounters: eight fixed builds, five seeds, ordinary mixed-crews/Foreman encounters, each with/without crews. The baseline replaces crew arrivals with ordinary enemies; it is a development fixture only. Both variants use identical seeds and arrival cadence, Rare weapons, Uncommon supports/ultimate, normal maximum health, automatic actions/ultimates and no augments. No route or draft decisions are simulated. Reports include survival, health damage including subsequent healing, clear time, pressure windows, crew actions, reach waits and ownership/order invariants. Completion includes the ordinary wave cleanup timeout; boss completion requires the Foreman's death. Do not interpret these equipped fixtures as ordinary-run win rates or evidence that difficulty is final.

`-crewReview -crewFullRun` runs an ordinary-offer, normal-health Expedition with automated pinball preferences and checkpoint restoration through wave six. `-expeditionReview` checks events and saves; `-expeditionReview -resumeOnly` in a fresh process verifies the saved fixture. Use copied isolated profiles when testing old schema-1/2 saves. The actual player profile must never be a test destination. See QA/shift-foreman-verification.md for binary provenance and limits.

### Expedition itinerary and ordinary-draft comparisons

`Run Simulations.ps1 -Expedition -Mode Full -Waves 8 -Policy archetypes -Runs 18 -Seed 91500 -Workers 2 -Player <player> -OutputDirectory <fresh-directory>` exercises the new itinerary with normal health and ordinary offers. Add `-LegacyExpedition` for the same binary's previous route/formation rules, including the Foreman. The runner requires the player's `expeditionVariety` capability and gives each worker an isolated profile. It never writes the live player's save or records. Direct CLI equivalents are `-simExpedition` and `-simLegacyExpedition`, plus the existing simulation options and mandatory `-saveDirectory`.

Six drafting policies rotate by seed. The route heuristic alternates three blocks: combat destinations, first available stop with safer event choices, and first available stop with riskier choices. Service automation buys repair then an affordable matching copy; pressure/Overtime attempts one tap, auction bids once, and rescue attempts at most two workers. These are reproducible heuristics, not optimal or human play. Comparisons include the effects of changed destinations, formation geometry, events, acquisitions and carryover damage. Reports add the itinerary and chosen roads, salvage, crew action counts and Foreman vents. Quick Run simulations retain their earlier behavior.

Native `-varietyReview -reviewOutput <directory> -saveDirectory <isolated-profile>` checks plan diversity, walking selection, events, save/reload, costs, outcomes, fusion and formations. Add `-varietyResumeOnly` in a fresh process with its saved profile. `-varietyFullRun -seed 1741` drives a complete ordinary-offer Expedition, restores checkpoints and captures roads/events/combat. All may be combined with `-batchmode -nographics` to omit captures. The original `-expeditionReview` and `-crewReview` deliberately retain legacy plans to continue testing existing rules and old-save behavior; the new suite covers generated plans. See QA/cinderworks-variety-verification.md.

### Chapter / Office review

`-chapterReview -reviewOutput <absolute-directory> -saveDirectory <isolated-profile>` runs native six-encounter progression, constellation purchases, reward gating, wallet receipts and save checks. It renders the Office at 1080p/720p unless `-nographics` is supplied. `-chapterFullRun` uses normal health and ordinary offers with automatic pinball preferences; `-chapterResumeOnly` reads the final native fixture in another process; `-chapterLegacyResume` accepts copied schema-1/2/3/4 checkpoints. These commands never require the player's profile.

The older `-simExpedition` eight-wave comparison and event/crew/Foreman/itinerary reviews deliberately retain the legacy run format. They do not estimate six-encounter chapter difficulty or the Spark economy. Chapter verification and limitations are in `QA/cinderworks-chapter-verification.md`.


### District champion review (0.8.6)

Use `Validate Champions.ps1 -Mode Mechanics|Rendered|Balance -OutputDirectory <fresh-directory>`. Mechanics tests production spawning, protection/pressure, interrupted and naturally completed attacks, swept single-hit damage, walls, physical junctions, save migration and road selection. Rendered additionally measures sign text in OnGUI and captures normal/reduced-motion views at 1080p and 720p. Elevated health in the mechanics fixtures is not difficulty evidence.

Balance runs eight fixed Rare-weapon / Uncommon-support kits without augments through entire Elite Shifts at encounters 3, 10 and 17, at normal health with full PhysX. `-OriginalChampions` uses the old Freight Champion in all districts in the same binary. It is also available on `Validate Progression.ps1` for matched ordinary-offer journeys. Manifests identify the assembly and comparison switch. New reports count champion cycles, attacks, connected charges/bash, pressure/escort breaks and protected contacts. Freight observations reuse legacy wind-up/crew/interrupt counters; those counters are not identical definitions and should not be compared as if they were. Preserve initial failures and distinguish fixed-kit access from ordinary loot or human difficulty. All runners must finish before rebuilding staging.
