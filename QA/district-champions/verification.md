# District champions — 0.8.6 verification

## Change

Cinderworks keeps the Freight Champion. Foundry Elite Shifts use a Shield Captain with two projected escort guards and a telegraphed bash; Dispatch uses an Interceptor with a fixed warning lane, a physical one-hit charge and exposed recovery. Both accept launch and shared brace-pressure counters. Health, ordinary spawn/durability rules, strict sequence order, weapon pairing, rewards and elite completion requirements remain unchanged.

Fork previews identify the actual champion and +6 salvage. Boss-bound combat roads say BOSS ESCORT rather than advertising a champion that cannot spawn. New silhouettes, phase countdowns, protection labels and cyan exposed cores use the existing presentation language. Schema 11 preserves champion phases/links/aim; older saved Freight actors keep their identity.

## Initial findings and retained failures

- Initial compilation failed because a new gold color reference did not exist. It was replaced with an explicit color before player testing; `initial-compile.log` remains local.
- `mechanics-initial` exited with a Unity GUI exception after combat and save checks: the harness attempted to measure sign text outside OnGUI. Its original manifest/log and a failure record are retained. The audit now runs in OnGUI in rendered mode and writes exception reports before quitting.
- `mechanics` then passed 78 assertions.
- `rendered-initial` reached 56 assertions, then correctly found FREIGHT CHAMPION too wide at the proposed 15-point size. The sign text was reduced to 14 points; the failed report and original images remain available.
- `rendered` still found the full title too wide at 14 points (62 assertions before failure). The final sign instead places the full champion name on two lines. `final-rendered` passed 98 assertions; visual inspection then found the existing connector drawn through text when a road anchor was above a sign. Connecting to the facing edge fixes that overlap. The final `final-signs` pass repeats all 98 assertions with clean captures.
- Code review found that general junction navigation could override a committed champion phase. The final source bypasses that steering during champion attacks/recovery and adds a real physical-junction regression. Stacked Captain and frontal-shield protection now names both states.

## Fixed-kit comparison

Both cohorts use assembly `647BA216968EDA3B8D74C01614E9D0121A6783F127034B9BD5FB7136AA68A7DB`. Eight Rare-weapon / Uncommon-support kits, no augments, normal health, full PhysX, entire Elite Shift including concealed approaches, fodder and completion. The `-OriginalChampions` baseline uses Freight Champions in all three districts. All 24 cases in each cohort cleared; all had zero visible spawns/order errors and consistent body accounting. These fixtures use straight roads, so the later junction steering correction does not change their conditions.

| District | Total incoming damage across eight baseline → candidate cases | Candidate encounter duration |
| --- | --- | --- |
| Cinderworks | 66.9 → 66.9 | 48.0–51.0s |
| Foundry | 45.4 → 44.8 | 53.0–56.0s |
| Dispatch | 123.7 → 186.1 | 58.0–62.3s |

The Captain reduced damage on 21 escort contacts. Nine openings came from pressure/escort breaks, with one completed bash. The Interceptor started 12 charges, connected ten times and suffered two forced openings. Its increased Dispatch damage is a real tradeoff, while every kit retained a clear. Freight uses older crew/wind-up counters with different definitions; its zero `hits` field is uninstrumented, not proof that it dealt no damage. Do not compare those counter columns as equivalent. Exact data is in `fixed-kit-comparison.json` and both balance reports.

The Captain can be suppressed by a strong clear/control build; this is intended counterplay. No guaranteed attack or damage gate was added. Fixed kits establish access, not human difficulty or natural-loot win rates.

## Regression coverage on the comparison assembly

- Boss mechanics: 713 assertions; all 24 normal-health boss fixed kits defeated their boss.
- Durability/live-stat checks: 53 assertions.
- Progression, timing and save checks: 137 assertions.
- Core: all fifteen standalone scenarios passed (`QA/v086-core-*.json`). These use the established 5,000-health contract-test override and do not measure difficulty.

## Ordinary-loot comparisons

All eight journeys completed all 21 encounters with zero order errors or visible spawns and valid body/progression accounting. Manifests use the same comparison assembly. These use actual reward offers, route decisions, normal health and full PhysX. Adaptive preferences are not fixed loadouts; per-encounter equipment and decisions are retained in the samples.

| Pool / seed / preference | Final integrity, baseline → candidate | Automated combat/travel, baseline → candidate |
| --- | --- | --- |
| Unlocked / 4319 / pinball | 99.0 → 111.2 | 19.44 → 19.54 min |
| Unlocked / 4319 / sever | 307.6 → 307.6 | 19.34 → 19.33 min |
| Unlocked / 4319 / gun | 280.0 → 280.0 | 18.78 → 18.47 min |
| Fresh / 1741 / pinball | 258.8 → 253.5 | 19.63 → 19.63 min |

Each candidate met and defeated one of each champion. Every Captain command was broken in these four samples; the Interceptor connected once in three of them and missed in the sever sample. The Captain is primarily an escort-protection problem for these builds, and successful counterplay can prevent its bash entirely. These four pairs do not establish a population win rate, causation for every timing difference, or human run length. PhysX outcomes can vary with identical seeds. No human decision time was synthesized.

## Final verification

Final player assembly: `CCE75ACDBC98AD65554441A880EA4AB6A1EAA2BBF4FC9AA04AE2454C2FF24471`. It includes the junction steering correction, explicit stacked guard labels, two-line sign names and the connector fix. Core gameplay/tuning values match the comparison build.

- `final-mechanics-initial`: 84 assertions passed after the junction correction, including a real physical Y-junction with a competing navigation waypoint. Wind-up stays on its marker and the charge follows its fixed aim.
- `final-signs`: 98 assertions passed on the final assembly. Includes all mechanics, snapshot identity, legacy migration, text metrics, native sign-click selection and physical-road commitment. Inspected 1080p guard/bash/recovery/charge/road images plus reduced-motion 720p champion and live-stats/fork images. Initial capture health is intentionally elevated and is not a gameplay balance value.
- `boss-rendered`: 713 assertions passed. Bell and Pursuer reduced-motion screens remain clear. This used `753F1A51BCFEEAD59860FE802C135D7461C4384FEA7F3A5875CB96856FF747D0`, immediately before the sign-connector-only change.

Both final ordinary-loot confirmations passed on that final assembly: unlocked seed 4319 pinball completed all 21 encounters with 111.2 integrity in 19.54 automated minutes; fresh seed 1741 pinball completed with 254.6 integrity in 19.54 minutes. Each defeated all three district champions with valid progression/body accounting and zero visible spawns or order errors. `final-confirmations.json` and the full samples retain the evidence. No source changed after this build; later edits only completed this report.

Selected final screenshots are committed; other captures and original logs are preserved locally. All checks use isolated profiles. The desktop updater remains the single normal play entry point; this work does not merge main or publish a release.
