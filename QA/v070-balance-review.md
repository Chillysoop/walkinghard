# v0.7.0 — separate rifle and Duelist candidates

Neither candidate is applied to normal play. The route/enemy feature release keeps Rail Rifle at 42 damage and Duelist Jacket at two ward hits.

## Recommendation

- **Duelist: recommend three ward hits**, keeping duration, capacity, first-hit stagger and non-stacking refresh unchanged. Fast results improve across dual-sword and greatsword builds, and the small Full physics check preserves wins with more health remaining. This is a recommendation awaiting approval, not shipped tuning.
- **Rifle: hold 42 damage.** Raising it to 52 barely changes Fast outcomes and does not improve the two Full examples. More damage alone is not a demonstrated fix. Next, compare rifle builds with long-range setup/follow-up abilities against the current Vector Tether / Arc Bolt / Breaker sequence at matching rarity; the earlier study identified substantial short-range target waits. Preserve strict sequence order.
- Leave the other weapon, armor and ultimate values unchanged in this release.

## Separate controlled comparisons

The authored v0.6.9 player runs normal health, eight waves, fixed loadouts, no later equipment improvements, and the ultimate only after checkpoint three. Five focal rarities use Common support, followed by Common/Rare/Legendary focal rarities with Rare support. Both carried weapons receive the focal rarity. Cases, overrides and analysis are in `Balance/v070`; raw JSONL is retained locally in `Simulations/v070-*`.

| Candidate | Fast pairs | Wins, baseline → candidate | Mean wave reached |
|---|---:|---:|---:|
| Rifle damage 42 → 52 | 468 | 20 → 23 | 5.150 → 5.182 |
| Duelist ward 2 → 3 hits | 156 | 16 → 23 | 6.372 → 6.891 |

Rifle includes its weapon and ultimate matrices. This is not 468 independent samples of one loadout; results span different rarities and ultimate choices. With Rare support its wins move 19/108 → 22/108, while mean wave slightly decreases (7.528 → 7.519). That is a mixed result, not a reliable buff justification.

Duelist with Rare support improves dual swords from 7/18 → 12/18 wins and greatsword from 9/18 → 11/18. Frozen Common-support builds still win 0/60 in each context, but mean progress improves by about 0.63 waves. These fixed Common-support results do not estimate the difficulty of ordinary drafting runs.

## Full physics checks

Six baseline games and six separate candidate games use two encounter seeds per Rare/Rare-support context. The rifle changes from one win in two to zero in two. This is too small a sample to establish a harmful population effect, but it does not confirm the proposed buff.

All four Duelist comparisons win before and after. Ending health changes:

| Build / seed | Two-hit ward | Three-hit ward |
|---|---:|---:|
| Dual swords / 0 | 91.2 | 120.6 |
| Dual swords / 1 | 84.0 | 116.3 |
| Greatsword / 0 | 135.9 | 173.3 |
| Greatsword / 1 | 135.2 | 157.5 |

The direction agrees with Fast, with an average of about 30 additional health. Four examples are corroboration, not a precise win-rate estimate. No combined rifle-plus-Duelist buff was tested or applied.

## Route smoke batch

1,000 normal-health games use all six draft policies and deterministic alternating routes. All reports satisfy engine invariants: 437 wins, 561 defeats and two 480-second timeouts, both at the Warden. Totals include 8,384 shield bearers, 7,862 volatile carriers and 3,588 detonations. No visible spawns, invalid kill accounting, sequence errors or unbounded cascades were reported.

This batch took about 126 seconds while other QA and editor builds were running. Balanced policy won 77.2%, gun 26.5%, pinball 49.7%, random 25.9%, sever 39.5% and storm 43.1%. These are policy outcomes, not isolated item rankings or route win rates. The new route rewards change later builds; a matched route-difficulty study is still needed before adjusting their spawn rate or payoff. The two Warden timeouts remain a follow-up for long-lived defensive/control builds.
