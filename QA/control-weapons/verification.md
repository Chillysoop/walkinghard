# Control weapons and crowd filters — 0.9.1

Hookblade and Rivet Pistol use the existing equipment mounts, five-slot sequence, family pairing, rarity progression, fusion and sockets. No existing item values or reward counts changed. Initial new-item values remain 28 damage for Hookblade and 20 for Rivet Pistol. Follow-up numerical tuning is proposed below, not applied.

## Functional evidence

- 15 core scenarios passed, preserving strict sequence, kill ownership, persistent bodies, walking shoves, concealed arrivals, fusions, pairs and reload indicators. These contract fixtures use 5,000 base integrity.
- All 50 build-variety cases passed, with 826 production mechanic assertions per case. The normal-health eight-wave `full` case also passed: 320 kills, 131 collisions, 269.34 final integrity. This is the legacy eight-wave test, not the current 21-encounter journey.
- New production probes cover every rarity, physical Hookblade displacement, primary-only targeting, no pull transferred from a killing blow, one augmented pull with a distance cap, Rivet ammo/control/expiry, forward-only velocity, immunity/resistance, strongest-slow replacement, Bore contacts, saved ownership and schema-12 migration. Layer-policy checks cover resting/flying corpses, hero exclusion and newly created walkers. Existing cascade/body checks and stress fixtures exercise actual physics.
- 56 repeated ragdoll stress fixtures passed against open ground, walls and road forks. No body removal, solver reduction or fixed-step skipping was introduced.
- Native combat presentation suite passed 63 assertions. Threat-over-ultimate, card text and hover details were visually inspected. Final new-weapon captures use distant targets to show control before melee contact. `MOVE SLOW` has a distinct symbol; `TEMPO` retains its existing haste meaning. Rarity borders and labels remain unchanged.

Core/variety/comparison provenance is in `manifest.json`. `pre-integration-manifest.json` records the later presentation/review assembly. `final-manifest.json` records the combined 0.9.1 assembly, preserving the separately completed constellation expansion. Detailed initial failures and their fixes are retained in `initial-findings.md`, rather than reporting only the successful reruns.

## Controlled comparison design

150 Fast cases: four fixed contexts, five matching seeds per context, and Common/Rare/Legendary focal weapons. Both weapon mounts have the tested rarity; other support items stay Common. Two close/sever contexts compare Hookblade with Edge Sword and Siphon Blade. Two ranged/mixed contexts compare Rivet Pistol with Pulse Pistol. Only the first mount changes within each matched comparison.

20 Full PhysX confirmations: the same four contexts at Common and Legendary, one seed per context. These are mechanism checks with a small sample, not population win-rate estimates. All 170 base cases were valid with no timeouts, missing results or duplicate case IDs. An additional 30 Fast cases compare both pistols with Bore Rounds on the first mount; all were valid with no timeouts.

These are fixed eight-wave kits, without new rewards after their setup. Ultimates still arrive after checkpoint three. The current 21-encounter ordinary journey is a separate validation scope. Fast and Full results must not be pooled into one win rate. Exact kits, seeds, per-case contributions, summaries and assembly hashes are in this folder; full raw logs remain under ignored `QA/Simulations/v089-control-*`. Run `analyze.py` to rebuild the base comparison.

## Findings and recommendation — not applied

**Hookblade: keep 28 damage and the current pull for now.** Fast runs generally favor Edge Sword, but Full does not show a consistent high-rarity disadvantage. In the Legendary sever kit, both won: Hookblade finished with 21.5 integrity versus Edge Sword's 7.7, while taking about 10 seconds longer. Both close kits lost to the final wave; Legendary Hookblade reached it with 87.5 integrity versus 43.0 for Edge Sword. Common sever Hookblade did worse (wave 6 versus wave 7). One Full seed per context is insufficient to call it overpowered or justify a broad buff.

**Rivet Pistol: propose testing base damage 20 → 22 (+10%), leaving its slow, magazine, reload and action time unchanged.** Its price for control looks high in the firing-line kit. Legendary Full finished with 32.5 integrity versus Pulse's 120.8; Fast reached wave 6.4 on average versus Pulse's 8.0. Bore Rounds improved Rivet's Legendary Fast mean to wave 7.2, but Pulse + Bore won all five matched seeds while Rivet + Bore won none. This supports a small damage experiment rather than adding more crowd effects or changing reload rules.

The mixed close-range kit is a counterexample: both weapons won all Fast cases and both Full rarities. Full Rivet finished at 74.1 / 186.7 integrity (Common / Legendary), versus Pulse's 67.7 / 197.0. Any proposed buff must preserve that viable tradeoff; the +10% proposal has not been applied or tested as a candidate in these results.

Siphon Blade won all four Full sword comparisons; Hookblade and Edge each won one. That is a useful sign of sustain's value in these long fixed kits, not enough evidence for another Siphon nerf. No existing weapon, gear or ability nerf is recommended from this narrow sample.

## Ordinary drafted journeys

Two normal-health, unlocked-pool journeys completed all 21 encounters with valid accounting and zero order/visible-spawn errors: seed 1741 with a gun preference (950 kills, 320 final integrity) and seed 4319 with a sever preference (961 kills, 304 integrity). The gun-preferring policy pivoted to fists, two Breakers and Reaping Cut; the sever policy used paired Edge Swords. The latter stored two Uncommon Hookblades without equipping them. Neither equipped Rivet Pistol. These are progression smoke checks, not evidence that the new weapons are strong or weak; the controlled substitutions address that question. Actual per-encounter drafts and item contributions are preserved in `gun-journey.jsonl` and `sever-journey.jsonl`.

## Performance scope

The new shared collision layers replace per-corpse/per-walker exclusions. Resting bodies ignore living capsules; launched body projectiles still collide with enemies; the hero remains unblocked. Body/body and world contacts, walking shove calculations, solver iterations, CCD, physics step rate and body lifetime remain intact. The old filtering policy is available through `-OriginalBodyFilters` on the native combat-feel runner.

An isolated comparison must run without competing Unity build or simulation workers. A recorded schema-12 checkpoint after wave 20 contains 914 defeated enemies and 926 actor identities. `Validate Combat Feel.ps1 -Mode Journey -CheckpointProfile <profile>` clones that profile into the new output directory and records its SHA-256 before resuming. Both policies must use the same source checkpoint. This is a late-game continuation and save-compatibility check, not a fresh full journey. FPS evidence is recorded separately once the machine is idle; functional success alone does not establish a speedup.


### Initial same-checkpoint measurements

Both filter policies restored the identical recorded checkpoint and completed encounter 21 with valid body/kill accounting. The initial sample had 156–157 active corpses and up to 42 living enemies. Normal speed remained about 16.67ms median. At 4x, the old per-pair filters measured 122.61ms median / 136.61ms p95; shared layers measured 118.63 / 128.74ms. This small single-pair difference does not establish a universal frame-rate improvement. The extreme-speed bottleneck remains.

An Automatic Box Pruning experiment was slower (155.31ms median / 169.28ms p95 at 4x). It was rejected; the original broad-phase setting was restored. The next reverse-order repeat detected another engine process and was automatically invalidated. Its timings are preserved and excluded from performance conclusions. The completed integrated repeat is reported below. The runner now records competing-process observations for future comparisons.


## Combined 0.9.1 verification

The final player integrates constellation commit `9ea1ff6` with the control-weapon work. It retains all 36 unlocks and 30 constellation additions, alongside the two new base weapons: 79 non-module definitions and 17 augments. No weapon damage, pull/slow values or speed-toggle behavior changed during integration.

All 15 core scenarios and all 50 variety scenarios passed again, each including 869 mechanic assertions. Constellation checks passed 449 headless and 501 rendered assertions. Twenty repeated Fast control-weapon cases matched their earlier result, reached wave, final integrity and elapsed combat time exactly (zero invalid cases/timeouts). Native control-card captures at 720p/1080p and the dedicated two-weapon motion review were repeated and inspected. Selected inherited Office/combat captures were also inspected; the earlier constellation report retains its full visual audit. See `integrated/` and `integration-comparison.json` for final-assembly evidence.

The final isolated, reverse-order performance pair resumed the same schema-12 checkpoint, with no competing Unity/player processes observed in 25 and 22 one-second samples. Shared layers measured **115.28ms median / 126.11ms p95** at 4x; original filters measured **115.19 / 129.61ms**. Both remained **16.67ms median at 1x** and completed encounter 21 with valid accounting, with 155–158 active corpses and up to 42 living enemies during samples. Physical runs diverge slightly in contacts and outcomes; these are short workload samples, not deterministic replay or statistical proof of a speedup.

The confirmed conclusion is **no meaningful median improvement in this extreme 4x scene**. Shared filters remove growing pair bookkeeping, but the physics/corpse workload remains expensive. The original broad-phase setting, solver, CCD, fixed steps and body lifetime are retained. Adaptive speed limiting is a separate material behavior proposal and has not been applied.

The initial final-assembly variety runner was deliberately interrupted after 22 completed reports, then its 28 remaining cases were divided across three workers. This was a scheduling change, not a test failure. Performance measurements were already complete before parallel workers started.

Dedicated weapon art and its exact generation prompts are recorded in `Art/control-weapons-prompts.json`; current-build screenshots are in `images/`.

The combined normal-health eight-wave regression also passed: 320 kills, 201 collisions, 274.67 final integrity, 13 item choices and two augment choices, with 869 mechanic assertions. This remains the legacy eight-wave functional fixture; the separate 21-encounter ordinary drafts and saved continuation provide their own progression evidence.
