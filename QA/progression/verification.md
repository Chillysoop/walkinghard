# 0.8.2 progression and combat-record verification

## Delivered behavior

The existing results screen now opens with the last damaging hit, leading incoming threats, the final encounter's integrity loss and actual damage dealt in that fight. Victory shows the largest net integrity drop and current-sequence run damage. The scrollable record includes entry/exit integrity, lowest integrity, actual damage after protection, and enemies carried in/out. The fatal unfinished encounter is retained. Existing kills, assists, healing, waits, per-item details and the live statistics sidebar remain available.

Incoming damage excludes blocked damage and overkill. Locker collapses, pressure ruptures and failed rescues have their own sources. A bounded 24-hit buffer labels the last-12-second total as “at least” when it cannot cover the interval. Schema 7 saves the record; earlier compatible checkpoints resume with an explicitly partial history. No new suggestion panel, inventory flow, combat resource or player auto-equip was added.

The automated draft policy now evaluates detached production acquisition, fusion and movement results. It accounts for both weapon mounts, one 2H attack, simultaneous matching 1H attacks, different-family turns, reload overhead, rarity milestones, actual producer/consumer links and stored partners. It can coordinate the two moves needed to leave a 2H loadout, recover ability slots vacated by fusion, and order Drain Brand before damage. Build weights and coverage estimates remain heuristics; this is not an optimal-play solver or a damage predictor.

## Normal-health journey comparison

[Summary and per-run source paths](summary.json), [initial incident](incidents.json), [iteration notes](iteration-notes.json). The runner is `Validate Progression.ps1`; analysis is `Analyze Progression.py`. See `SIMULATIONS.md` for reproduction commands.

Two fixed seeds (1741, 4319), seven draft preferences, the same fully unlocked constellation pool, 21 encounters, Full PhysX and ordinary health/rewards. There are **14 recorded outcomes per draft policy**. Defeat is valid evidence. One additional initial engine exception is retained separately below; a fresh confirmation supplies that case's recorded outcome. Partial runs interrupted by the earlier fail-fast supervisor are excluded and rerun. The supervisor now allows independent cases to finish after another case fails.

**This table describes draft preferences, not seven committed-build win rates.** The policies may pivot when loot makes another loadout better. All recorded outcomes passed sequence, concealed-spawn, body/kill identity and completion checks.

| Draft preference | Old policy completed | Projected policy completed | Projected mean encounters cleared |
| --- | ---: | ---: | ---: |
| pinball | 2/2 | 2/2 | 21 |
| storm | 0/2 | 2/2 | 21 |
| sever | 0/2 | 1/2 | 17 |
| gun | 0/2 | 1/2 | 16 |
| cutcarry | 0/2 | 1/2 | 18.5 |
| close | 1/2 | 1/2 | 16.5 |
| siphon | 2/2 | 2/2 | 21 |

The old policy completed 5/14; the projected policy completed 10/14. This small, adaptive cohort is not a human win-rate estimate or proof of weapon balance. The old Siphon preference uses its historical generic sustain fallback. PhysX runs are not bit-for-bit deterministic.

Important actual loadouts:

- The seed-1741 gun draft retained its Uncommon Rail Rifle and died in encounter 12, versus encounter 5 for the old policy. The seed-4319 gun-preference completion finished with two Epic Conductor Wraps and Arc Bolt. It is **not** evidence that guns completed that journey.
- The seed-1741 close-quarters draft died in encounter 13 with an Epic Scattergun and Common fists. Its seed-4319 completion had pivoted to an Epic 2H Impact Hammer.
- Neither sword-and-hammer preference trial was offered a Breach Hammer. Both ended with an Impact Hammer, so their outcomes do not validate the intended mixed 1H archetype. One-handed swords were offered, but the bot chose armor or the stronger hammer at those checkpoints.
- Successful pinball-preference drafts carried roughly 13, 21 and 23 enemies on average across the three districts. The gun-preference cohort carried roughly 7, 7 and 15. High carry alone did not distinguish defeat; look at damage sources, defense and actual composition.

## Tuning decision

**No combat values, enemy durability, encounter composition, reward counts, rarity odds or fusion rules changed.** The measurements do not justify blanket buffs or shorter cleanup pressure.

Literal weapon-offer gaps were 3–5 completed encounters in the projected cohort. Heuristic weapon-upgrade gaps were much longer (6–21), including successful runs with strong early equipment or later pivots. Defensive-upgrade opportunity gaps were 6–9. These are survival-censored, heuristic opportunity measures, not guaranteed player benefit; shops are not counted as reward offers. The average projected run fused 3.43 times. Old-policy opportunity scores came from an earlier evaluator and should not be compared numerically with final-policy scores.

The next controlled experiment should improve access to missing weapon components without changing rarity odds or guaranteeing a set, then compare actual retained loadouts. Use committed gun and mixed-weapon cases alongside adaptive drafts before assigning weapon-specific buffs. The present evidence supports investigating discovery/offer composition; it does not yet select a safe curation rule.

## Verification and known limitation

Final assembly SHA-256: `38E3ABF56F9C994C694D2ACA7ED206D604A3B7F7EFFD24D0E7CFBB17CA6C578D` (Windows 0.8.2).

- All **15 final regression scenarios** passed, each including **761 production-mechanic assertions** and loadout checks. Reports: `QA/progression-final-*.json`.
- **117 progression/recap assertions** passed in the rendered final build: rarity retention, fusion consuming a paired weapon, coordinated hand swaps, populated utility slots, distinct preferences, legal offers, actual incoming damage, blocked hits, overkill, fatal-wave capture, exact save round-trip and old-save migration. [Checks](acceptance-final/checks.txt).
- **136 journey-flow checks** passed on the final binary. [Report](journey-final/report.json).
- Inspected the final results panel at [720p](acceptance-final/recap-1280x720.png), [1080p](acceptance-final/recap-1920x1080.png) and [ultrawide](acceptance-final/recap-2560x1080.png).

An initial pinball/4319 attempt tripped the existing ragdoll stability guard after the fight-5 heartbeat. That binary did not identify the failing bone or velocity. [Original failure](physics-incident.txt). A checkpoint continuation and a fresh full journey both completed without repeating it. The new diagnostic reports wave, time, actor, bone, position and velocity if it recurs. **No physics fix is claimed; the rare outlier remains unresolved.** Physics simulation, limits and collision budgets are unchanged. The checkpoint continuation is excluded from full-run timing/outcome comparisons; the fresh confirmation is included once.

Cohort manifests preserve provenance. The legacy samples used `CF20115BED71A46ECBF38BB730E35FE78686E843E7D411D5E57B7044EEEA4FBD`. Three projected seed-1741 outcomes used `0EF2628435B2A438BBBB67FE6C168B4AC7A28C21491D1BCD638D6C8C583188BD`, which has the final evaluator; subsequent changes only refined recap presentation and added diagnostic text/replay access. The remaining projected outcomes and final acceptance checks used the final assembly above. Early pilots are excluded from the summary.
