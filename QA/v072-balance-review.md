# v0.7.2 — reliable builds, routes and legacy timeouts

October 29, 2026. **Applied:** the approved Duelist ward increase from two to three damaging hits. **Unchanged:** rifle damage 42, route rewards/enemies, all other item values and strict sequence order. Further proposals below are not applied.

## What to change next

1. Keep rifle damage at 42 and preserve strict order. The rifle's main problems in these comparisons are a short-range action holding the sequence and the build's defensive support. Reordering the same short-range loadout does not fix the underlying reach mismatch.
2. Test a more useful **Volatile Breach ability offering** before changing route enemy numbers: prefer an equipped ability for the existing two-copy offer, and choose the other ability from compatible synergy tags when possible. Retain three distinct offers, the wildcard, current rarity rolls, rerolls and reward count. This is a proposed experiment, not a guaranteed fusion or an applied rule.
3. Make no further boss-stat changes for the two historical Warden timeouts. Both reproduce exactly on the old player and complete against the current Pursuer. Their decisive failure was target access, including a boss stuck at 33 HP, rather than insufficient total damage alone.
4. Expand Deadeye with ranged control/defense alternatives before adding more pure damage. A small proposed first set is listed below; it is not part of this implementation.

## Ranged sequences

600 Fast games: rifle versus paired pistols, five matching weapon rarities, twelve matched seeds, five sequences. Supporting abilities/gear are Rare; both weapon mounts receive the focal rarity. Ultimate remains Redline after checkpoint three. No later upgrades or drafting. All cases use the current three-hit Duelist and standard encounters to isolate the sequence comparison. These are fixed-loadout experiments, not ordinary-run difficulty estimates.

The short-range sequence is Vector Tether → weapon basics → Arc Bolt → Breaker. Other variants replace Breaker with the listed ability. Gun-first uses the same short-range items, moving the basics before Tether. Paired pistols cost two items; the rifle costs one two-handed item, so this matches rarity, not acquisition cost.

| Last ability / ordering | Rifle wins / 60 | Rifle target-wait share | Pistol wins / 60 | Pistol target-wait share |
| --- | ---: | ---: | ---: | ---: |
| Breaker | 0 | 16.7% | 41 | 26.8% |
| Breaker, guns first | 0 | 16.8% | 41 | 26.2% |
| Counterguard | 60 | 5.7% | 60 | 14.1% |
| Conductive Brand | 0 | 10.5% | 45 | 18.3% |
| Drain Brand | 48 | 9.7% | 60 | 17.5% |

Counterguard and Drain Brand change defense/sustain as well as reach; these results do **not** isolate a pure range buff. Conductive Brand reduces waiting without solving the rifle's Fast survival problem. Guns-first raises rifle mean progress from wave 6.52 to 7.17 but still produces no wins. The new checkpoint warning exposes the measured wait without silently skipping an action.

Twelve Full physics games use Rare weapons/support, two preselected seeds per weapon/variant, and Breaker/Counterguard/Conductive Brand only:

| Build | Wins / 2 | Ending health | Target-wait share |
| --- | ---: | --- | --- |
| Rifle + Breaker | 0 | 0, 0 | 23.0%, 18.7% |
| Rifle + Counterguard | 2 | 303.6, 300.2 | 6.6%, 8.8% |
| Rifle + Conductive Brand | 2 | 90.9, 109.8 | 13.5%, 11.7% |
| Pistols + Breaker | 2 | 147.5, 82.8 | 33.4%, 33.2% |
| Pistols + Counterguard | 2 | 308.7, 307.3 | 19.6%, 18.9% |
| Pistols + Conductive Brand | 2 | 243.3, 235.4 | 24.6%, 22.2% |

Full agrees that short-range waits are costly and guard support helps. Its rifle/mark result differs from Fast, so a zero Fast win rate is not proof that the build cannot work. Two Full seeds per variant are corroboration, not a precise ranking of every rarity. Drain Brand was not included in the preselected Full slice.

## Route difficulty versus reward value

**Encounter-only:** 288 Fast games use six fixed Rare builds × sixteen matched seeds × Standard/Shield Line/Volatile Breach. There are no new rewards, and only waves 3 and 6 change encounter type. All combat records before the first affected wave must match within each pair.

| Encounter | Wins / 96 | Mean HP after wave 3 | Mean HP after wave 6 |
| --- | ---: | ---: | ---: |
| Standard | 77 | 266.8 | 200.9 |
| Shield Line | 74 | 267.5 | 199.8 |
| Volatile Breach | 75 | 270.9 | 206.4 |

All 96 cases complete wave 3; 95 Shield Line cases and 96 of the other routes complete wave 6. Later health means condition on those completions. Several fixed builds win every case, while the rifle wins none, so aggregate win rates hide floor/ceiling effects. The clearest differing context is sever: 13/16 Standard, 10/16 Shield, 11/16 Volatile. There is no basis here for a broad Shield enemy nerf.

The small Full route slice uses rifle, sever and pinball, one preselected seed each, matched across Shield/Volatile. It was reduced from two seeds to one before any Full route results were collected, after measuring the Full range runtime. See `v072-balance-results.json` for individual context summaries. This is six coverage cases, not evidence of population equivalence. Two pairs in the original multi-game Full workers diverged before the route began, so those comparisons were excluded and rerun with a fresh Unity process per case to remove preceding-case physics history.

**Reward-only:** 720 normal-drafting Fast games use the same 240 seeds and six policies per condition. All encounters stay Standard; only the two route-checkpoint reward emphases change.

| Reward emphasis | Wins / 240 | Win rate |
| --- | ---: | ---: |
| Ordinary mixed | 95 | 39.6% |
| Equipment | 116 | 48.3% |
| Abilities | 98 | 40.8% |

Equipment beats abilities by 18 wins: 40 seeds improve and 22 worsen, a +7.5 percentage-point paired difference (exploratory exact McNemar p≈0.030, unadjusted). It leads in five policies; pinball slightly favors abilities (19 versus 16 wins out of 40). This is a signal about these policies and offers, not an isolated estimate of an item's strength.

**Linked routes:** 480 additional games use those same 240 seeds with the actual encounter and reward paired. Shield Line wins 117/240 (48.8%); Volatile Breach wins 104/240 (43.3%). Forty-one seeds improve with Shield and 28 worsen; paired p≈0.148. That does not establish a universal default. Combined with the reward-only result, it supports testing ability-offer relevance first while keeping enemy values unchanged.

## Legacy Warden timeouts

Both seeds reproduce with the original v0.7.0 assembly and normal drafting at the 480-second limit:

- **439:** Steel Jab → Counterguard → uncommon Rail Rifle pair → Arc Bolt, with Redline. The Warden falls to 33.17 HP, then its health is unchanged from the 360-second trace to timeout. The next action repeatedly reports `WAITING FOR TARGET / Steel Jab`; total Jab target wait is 168.5 seconds. Redline has no direct damage to finish it independently.
- **685:** Breach Hammer → Conductor Wraps → Mass Driver → Force Push → Arc Bolt, with Redline. Warden health stays at 600.65 through the last 120 seconds while the next Breach Hammer waits; that weapon accumulates 206.7 seconds of target wait.

The old Warden used the ordinary orbit/approach rules, with a randomized desired radius that can sit beyond these short attacks while the Walker keeps moving. The unchanged boss HP and waiting action traces support a target-access stall, not merely a long damage race. No claim is made that lowering HP would fix it.

The same seeds now complete against the planted-player Pursuer: seed 439 at 349.2 seconds with 273.3 HP; seed 685 at 372.2 seconds with 226.9 HP. Both used Reclaimer, so the Duelist change does not explain these two recoveries. The Pursuer approaches within fist reach, and the player holds position. No extra boss nerf or sequence skip was added.

## Proposed Deadeye additions — design only

Develop these after reviewing the comparison and art directions. Each should have its own icon, card art, one-line effect and rarity scaling of its distinctive effect as well as stats.

- **Suppressive Volley — ability:** a 13m narrow fan that interrupts up to three targets; small direct damage, useful ranged stagger, no forced push out of the next action's range. Higher rarity improves stagger and the controlled fan width. Uses existing stagger/line/pierce combinations and gives rifle builds an alternative to Breaker's local control.
- **Recoil Harness — armor:** a real reload primes one limited block; a successful block empowers the next gun hit. Refresh replaces the charge. Rarity scales absorption and the stored payoff. This offers a reload-specific defensive loop rather than another flat damage bonus, with a cap shared by paired guns.
- **Ballistic Screen — ability:** an 11–13m shot creates a brief limited guard only when it hits an enemy. Rarity scales guard strength/duration. This provides an offensive ranged support option; it must be compared directly with Counterguard to avoid becoming a strict upgrade.

Start with one ability and one armor, compare against Counterguard/Duelist at matched rarity in both modes, then expand helmet/boots/rig options if those slots still constrain the archetype. Exact numbers and implementation require the next design/balance decision; none of these items or route-offer changes is included in v0.7.2.

## Reproducibility and limits

`Balance/v072/prepare.py` writes exact fixed cases; use the existing `Balance/controlled-siphon/run.py` with those cases. `run_rewards.py` runs reward-only cases and `--linked` runs real route pairs. `timeout_repro.py` preserves old/current traces. `run_fresh_full.py` isolates each Full route case in a new process. `analyze.py` checks counts, unique cases, pre-route equality and engine invariants, then emits compact JSON and per-game CSV. Raw data stays under ignored `QA/Simulations/v072-*`.

The first controlled Full attempt failed a pre-existing startup test that expected ordinary checkpoint routes while controlled experiments suppress drafting. The test now temporarily suspends experimental inputs, checks ordinary rules, and restores them; no combat rule was changed. All reported Full comparisons use the repaired runner. Fast and Full retain different collision/physics approximations and are reported separately.
