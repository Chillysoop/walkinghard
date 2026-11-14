# Shift Foreman and crew balance verification

November 13, 2026. Based on Cinderworks crews commit `1c9d1a7a9b6e158be6835bc31d569e73658cbb6e`, on `feature/shift-foreman`.

## Delivered behavior

Expedition wave six now introduces the Shift Foreman after the second road/service choice. The Walker holds position while the boss approaches, braces and vents. Stagger and body impacts accelerate pressure; ordinary attacks still damage bracing and a natural vent provides access without control abilities. Paired hands and delayed collision descendants share pressure by originating action, including when another action intervenes. A pressure break cancels the slam. Killing him opens the ordinary wave-six rewards, followed by wave seven and the Pursuer. Quick Run retains its original wave six.

The furnace suit uses the existing skeleton and physics. Closed shutters and planted supports communicate bracing; opening shutters expose a cyan core. Pressure lamps, a labeled bar, damage reduction/bonus text, the slam footprint and countdown make the phases visible. FOREMAN is the eighth practice preset. The encounter adds no catalog entries, equipment sockets, currencies or manual dodge/aim controls. Exact values are in [DESIGN.md](../DESIGN.md).

Schema 3 persists Foreman state and pressure attribution, while accepting actual schema-1 and schema-2 saves. A targeted check exposed an older loader defect: a nested impact-reaction struct was copied into its owner before its fields were restored. The codec now hydrates that value first, preserving the complete graph.

## Verification

- [Native Foreman review](shift-foreman/native/report.json): **27 assertions plus 761 production-effect assertions**. Concealed arrival, original skeleton/collider count, braced and vent damage, shared/late-action pressure budgets, slam cancellation on pressure break, natural venting, pause freeze, wall occlusion, exact graph restoration, four melee contact fixtures, wave-six rewards, wave-seven progression and practice exit all passed. Mechanics fixtures use elevated health; they are not balance evidence.
- [Matched balance study](shift-foreman/balance/report.json): **160 Full-physics encounters and 320 assertions**, five seeds, eight fixed builds, two encounter types, crews enabled/disabled. All completed with zero sequence errors, visible spawns or body-accounting violations. Detailed interpretation follows below.
- [Normal-health Expedition](shift-foreman/full-run/report.json): eight waves completed with **285 kills and 250.9 integrity**, ordinary offers and automated pinball preferences. Retained **13 item choices, two augment choices**, both physical forks, service/event decisions and the Pursuer. Six reward checkpoints restored across waves 3/5/6. Foreman recorded four vents and one slam; his defeated body survived checkpoint restoration and the finale. Crews completed eight repairs, eleven throws and seven retrievals. One functional run is not a win-rate estimate.
- [Crew regression](shift-foreman/crews/report.json): **37 assertions plus 761 production assertions**, including channel interrupts, projectile ownership/budgets, walls, role saves, road geometry and appearance reset.
- [Event/save regression](shift-foreman/expedition/report.json): **659 assertions plus 761 production assertions**. Event odds, repeat-load roll stability, services, fusion, ownership, malformed-save rejection and full graph/physics restoration passed. The malformed-JSON warning is deliberately provoked by this suite.
- [Fresh-process resume](shift-foreman/fresh-process/report.json), [actual schema-1 migration](shift-foreman/legacy-v1/report.json) and [actual schema-2 migration](shift-foreman/legacy-v2/report.json): **five assertions each**. Migration inputs were copies of checkpoints made by the previous builds; original fixtures and the player's profile were untouched.
- [Core/progression regression](shift-foreman/regressions.json): all **15 core scenarios** and the **eight-wave progression case** passed, with 761 production assertions each. These use 5,000 starting health for mechanics coverage. No item identities or signatures changed, so the separate 50-case catalog suite was not repeated.
- [Regional preview](shift-foreman/preview/report.json): **203 assertions and 1,522 production assertions**. All eight presets ran in both appearances. All 507 original world colliders kept their identities/bounds; regional architecture retained 258 batched renderers. The suite's sequential speed/performance samples ran without other players/builds competing; these practice samples are not a full-Expedition hardware benchmark.

Inspected captures include bracing, open vent, slam anticipation, 720p reduced motion, the eight-button practice selector and crowded normal-Expedition combat. Selected images are beside their reports. Accelerated test stepping can retain a fusion popup longer in combat screenshots because it advances combat faster than wall time; these captures do not establish first-time player comprehension. Earlier failed diagnostic runs are retained only under ignored `Build/Foreman-evidence`.

## Cross-build results and tradeoffs

Seeds 91201–91205 use Rare weapons and Uncommon abilities, armor, rig and ultimate, normal maximum health, no augments and automatic sequencing/ultimates. Each kit runs wave-seven mixed arrivals and wave-six Foreman, with a matched ordinary-enemy baseline. This fixes equipment and removes prior-wave attrition, routes and drafting. Ordinary-wave completion includes the existing cleanup timeout; Foreman completion requires his death. Health damage includes health later restored by healing, but excludes absorbed damage.

| Build | Completed fixtures | Foreman seconds with crews, mean | Change vs no crews | Health damage with crews, mean | Early vents across five boss cases |
| --- | ---: | ---: | ---: | ---: | ---: |
| Paired fists | 20/20 | 23.8 | -0.4s | 1.2 | 8 |
| Paired swords | 20/20 | 26.9 | +0.9s | 3.1 | 4 |
| Greatsword | 20/20 | 26.9 | -0.4s | 3.0 | 1 |
| Two-handed hammer | 20/20 | 28.6 | -1.7s | 8.6 | 8 |
| Rifle | 20/20 | 42.1 | +1.4s | 1.3 | 6 |
| Paired pistols | 20/20 | 31.9 | -0.7s | 2.2 | 10 |
| Storm/wraps | 20/20 | 29.7 | -0.1s | 2.5 | 8 |
| Sustain/blades | 20/20 | 26.0 | -0.4s | 1.9 | 3 |

Across ordinary mixed encounters, crews increased mean health damage from 4.03 to 5.25. Across Foreman encounters, it fell from 4.70 to 2.97. Crews completed 107 repairs, 99 throws and 110 retrievals across both encounter groups. Replacement enemy roles can remove a shooter, change density and provide bodies for player combos; enabling crews is therefore not a pure increase in enemy damage. Do not infer causation for the small per-build differences from five seeds or assume Full physics is bit-identical across sessions.

The useful result is coverage: both paired and two-handed melee retain contact, rifles and ordinary damage can finish without relying on frequent pressure breaks, and crew channels actually execute. The equipped fixtures are forgiving. The rifle is the slowest boss clearer, while fists clear fastest; equal win counts do not mean equal strength. No global damage, item or crew-stat adjustment is justified by this sample alone. The next difficulty study should use ordinary drafts across several policies and starting gear strengths, plus player observation of pressure/vent comprehension. The final challenge level remains provisional.

## Reproduction and binary provenance

Build with Unity 6000.3.27f1 using `Build Windows.ps1 -OutputDirectory <directory>`. Commands and fixture limits are documented in [SIMULATIONS.md](../SIMULATIONS.md). Native reviews require an isolated `-saveDirectory`; never use the player's real profile.

[Provenance](shift-foreman/provenance.json) records assembly/executable hashes and all changed source-file hashes. Local player folders correspond to these test stages:

| Player folder | Retained evidence | Changes after these checks |
| --- | --- | --- |
| `Build/Shift-Foreman` | 15 core scenarios | Nested-value checkpoint fix, Foreman pressure-history fix, vent HUD guard; Quick Run combat paths unchanged. |
| `Build/Shift-Foreman-final` | Eight-wave Quick Run progression, event/save regression | Foreman pressure-history fix and vent HUD guard; these fixtures do not run a Foreman encounter. |
| `Build/Shift-Foreman-delivery` | 160 matched cases, normal Expedition, crew regression, fresh/v1/v2 resume | Only the vent HUD property was made phase-aware; no combat/save change. |
| `Build/Shift-Foreman-tested` | 27 native Foreman checks, eight-preset regional review | Final tested source; documentation and QA packaging only. |

The playable delivery is `Build/Shift-Foreman-tested/WalkingHard.exe`. The build-generated scene object-ID churn was restored before commit. This is a development branch and draft PR; the launcher and published release are unchanged. The longer act graph, persistent hub and further catalog expansion remain separate work.
