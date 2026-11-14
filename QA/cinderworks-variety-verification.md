# Cinderworks destination and event variety

Verified November 13, 2026 (Pacific time), on `feature/cinderworks-variety`, based on Shift Foreman commit `2c0918de4876273642fb659d6e3395286dcdf762`.

New Expeditions save a generated itinerary for the two existing physical road forks. The first offers a combat destination against one of four events, including Overtime. The second always offers salvage service against a combat or different event destination. New repair, freight, reclaimer and elite definitions vary arrivals, crew roles, previews and reward emphasis. The elite destination reinforces the Foreman and pays six additional salvage once.

Three new events make different decisions: pay for a chance at a displayed auction item; bank or risk an escalating pressure haul; or rescue workers with secure rewards using paid tools or risky force. Each choice exposes its current odds, payment, payoff and failure cost. Ordinary acquisition/fusion applies to auction items. Three records join the original three Overtime records. Schema 4 preserves itinerary, stakes, rolls and payments; schemas 1–3 retain legacy destinations when resumed. Exact rules are in [DESIGN.md](../DESIGN.md).

## Verification

- [Final native review](cinderworks-variety/native/report.json): **361 assertions plus 761 production-effect assertions** passed. Coverage includes 120 generated plans; deterministic itinerary restoration; walking-fork pointer, pause and deadline behavior; every event option's success/failure and costs; production event pointer handling; repeat-load roll stability; no duplicate charges; free exits; blocked unaffordable/lethal attempts; pressure escalation; rescue reward retention; auction fusion preserving Crosscut; elite payout exactly once; and actual crew formations with concealed arrivals, disabled prop colliders and stable physics identities.
- [Normal-health Expedition](cinderworks-variety/full-run/report.json): **14 assertions**, all eight waves completed, **292 kills and 161.3 integrity**, ordinary offered choices with automated pinball preferences. Both physical forks, Overtime, rescue, the Foreman and the Pursuer ran. Twelve stopped checkpoints restored the exact itinerary and event/reward state. The run retained **13 ordinary item choices and two augment choices**. This is functional coverage, not a population difficulty estimate.
- [Fresh-process schema-4 resume](cinderworks-variety/fresh-process/report.json): **four assertions** passed for the resolved auction, preserved itinerary/graph and blocked second bid. Actual [schema-1](cinderworks-variety/legacy-v1/report.json), [schema-2](cinderworks-variety/legacy-v2/report.json) and [schema-3](cinderworks-variety/legacy-v3/report.json) checkpoint copies passed **five assertions each**. Original inputs and the player's profile were untouched.
- [Event/save regression](cinderworks-variety/events/report.json): **659 assertions plus 761 production assertions** passed. Its malformed-JSON warning is intentional. [Crew regression](cinderworks-variety/crews/report.json): **37 plus 761**. [Foreman regression](cinderworks-variety/foreman/report.json): **27 plus 761**. Legacy fixtures explicitly retain their original itinerary; generated itineraries have the separate native review.
- [Core/progression regression](cinderworks-variety/regressions.json): all **15 core scenarios** and the **eight-wave Quick Run progression** passed, with 761 production assertions each. These mechanics fixtures use 5,000 starting health. No item identities, signatures or weapon rules changed, so the separate 50-case catalog suite was not repeated; auction ownership and fusion are checked directly.
- [Ordinary-draft comparison](cinderworks-variety/balance/report.json): **36 Full-physics Expeditions**, with no invalid runs or timeouts. Results and limitations follow below.

Inspected final captures include the [physical fork](cinderworks-variety/native/01-generated-fork.png), [auction](cinderworks-variety/native/event-Auction.png), [pressure choices](cinderworks-variety/native/event-PressureWorks.png), [pressure result](cinderworks-variety/native/result-PressureWorks.png), [rescue at 720p](cinderworks-variety/native/rescue-720p.png) and four combat formations. Event costs and consequences use bright text; choice cards have hover feedback. Combat destinations say REPAIR CREW, FREIGHT CREW or RECLAIMERS to distinguish them from services. Selected ordinary-run captures are retained beside its report. Synthetic interaction and screenshots do not establish first-time player comprehension.

## Ordinary-draft comparison

Both batches use the same candidate player assembly, seeds 91500–91517, two workers per batch, Full physics, eight waves, normal health and ordinary offered drafts. One enables the legacy itinerary/formation switch; the other uses generated plans. Six draft preferences run three times each. Both retain production equipment, event costs, health and automatic ultimates. These are whole-Expedition comparisons, including changed routes, enemies, rewards and later offers, rather than isolated enemy-stat tests.

| Draft preference | Legacy completed | Generated completed |
| --- | ---: | ---: |
| Pinball | 3/3 | 3/3 |
| Storm | 3/3 | 3/3 |
| Sever | 3/3 | 2/3 |
| Guns | 2/3 | 3/3 |
| Cut & Carry | 3/3 | 2/3 |
| Close Quarters | 1/3 | 3/3 |
| **Total** | **15/18** | **16/18** |

Thirteen matched cases completed under both rulesets. Three losses became wins; two wins became losses. The generated-plan defeats were Cut & Carry seed 91504 and Sever seed 91508, both in wave eight. Close Quarters seed 91505 survived with only 1.5 integrity. The sample therefore contains meaningful attrition despite most runs completing.

Across all cases, mean final integrity was 164.17 legacy versus 166.90 generated, and mean simulated combat time was 358.40 versus 379.05 seconds. These include defeats; integrity is not normalized for equipment-dependent maximum health, and duration is not a hardware performance measurement. New runs recorded 104 crew repairs, 124 throws, 147 retrievals and 43 Foreman vents. All new event types and combat route types were exercised across native fixtures and the comparison; the native suite forces both outcomes independently.

Route policy rotates by seed group: prefer combat, prefer a stop with safer choices, or prefer a stop with bolder choices. It repairs/buys an affordable copy at services, takes at most one pressure/auction/Overtime risk, and attempts at most two rescues. It preserves a 15%-maximum-health margin before injury risks. It does not optimize event strategy or explore all escalating gambles. Salvage service is always offered at the second fork, but a stop-preferring policy can choose the event on the other side.

Three cases per draft policy cannot establish relative strength or a reliable win rate. Changed formation and event RNG, later loot choices and PhysX variation prevent attributing individual outcome changes to a single system. No global balance adjustment was made from this sample. The next useful balance work is a larger ordinary-draft sample plus human observation of fork comprehension and event spending. Longer acts and the persistent hub remain separate work.

Raw worker rows and batch manifests are retained under [balance](cinderworks-variety/balance/report.json). Rebuild the descriptive comparison with `python QA/cinderworks-variety/analyze.py`.

## Reproduction and binary provenance

Build with Unity 6000.3.27f1 using `Build Windows.ps1 -OutputDirectory <directory>`. Native reviews require an isolated `-saveDirectory`; never use the player's real profile. [SIMULATIONS.md](../SIMULATIONS.md) documents the commands, route heuristics and older review switches.

```powershell
& '<player>/WalkingHard.exe' -varietyReview -reviewOutput '<captures>' -saveDirectory '<isolated-profile>' -logFile '<log>'
# Add -varietyFullRun for the ordinary Expedition; -varietyResumeOnly for its saved auction fixture.
# -nographics skips captures. To test fresh resume, reuse a completed native fixture's isolated profile.

& '.\Run Simulations.ps1' -Player '<player>/WalkingHard.exe' -Expedition -LegacyExpedition -Runs 18 -Workers 2 -Seed 91500 -Policy archetypes -Waves 8 -Mode Full -MaxCombatSeconds 600 -OutputDirectory '<baseline>'
& '.\Run Simulations.ps1' -Player '<player>/WalkingHard.exe' -Expedition -Runs 18 -Workers 2 -Seed 91500 -Policy archetypes -Waves 8 -Mode Full -MaxCombatSeconds 600 -OutputDirectory '<candidate>'
```

[Provenance](cinderworks-variety/provenance.json) records executable/assembly hashes and changed source-file hashes. The retained evidence spans these player folders:

| Player folder | Evidence | Later source changes |
| --- | --- | --- |
| `Build/Cinderworks-Variety-candidate` | 36 ordinary-draft comparison runs | Corrected native fixture's Crosscut catalog lookup; event text contrast/hover; clearer combat-route labels. No combat/save change. |
| `Build/Cinderworks-Variety-tested` | 15 core scenarios; normal-health Expedition; fresh/v1/v2/v3 resume | Event text contrast/hover and route labels only. |
| `Build/Cinderworks-Variety-delivery` | Eight-wave Quick Run; event/save, crew and Foreman regressions | Route labels only. |
| `Build/Cinderworks-Variety-release-candidate` | Final 361 native checks, 761 production checks and rendered captures | Documentation and QA packaging only. |

The playable delivery is `Build/Cinderworks-Variety-release-candidate/WalkingHard.exe`. Build-generated scene object IDs were restored before commit. This is a development branch and draft PR; the published launcher remains unchanged.
