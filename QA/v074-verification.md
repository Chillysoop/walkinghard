# Walking Hard 0.7.4 verification

October 29, 2026. Unity 6000.3.27f1. Tested shipping player: `Build/Clarity-v074-final/WalkingHard.exe`. Assembly SHA-256: `85a61b973a4a688b948e9109778e81386b59514f68d6505070537bc66aa35cde`.

## Changes

- Ordinary checkpoints start with three reward cards and matching destinations. Manage build opens the full character sheet, storage and five-action sequence on the same screen. The tutorial retains its guided sheet.
- Large Weapon / Ability / Gear / Augment / Ultimate labels and neutral frame shapes identify type. Separate rarity badges retain rarity colors. One-line effects, shorter stats and explicit equip buttons explain the choice without opening details.
- Hover, selection and dragging highlight compatible destinations. Replacement previews explain displaced equipment, linked two-handed attacks and occupied augment sockets. Repeated target-wait warnings and upcoming encounters remain visible; the full wave report is available in Manage build.
- Augments can attach through the primary action or auto-equip to a sole compatible stored item. Multiple hosts require an explicit choice. Auto-equip cannot change a running fight.
- Clicking a stored item takes priority over an unrelated pending reward. Clicking a compatible stored augment host still attaches the chosen augment. Drag/drop, double-click, fusion and Edit after pick retain their behavior.
- Weapon attacks are named consistently in sequence headings and tutorial instructions. Reward counts, rarity odds, balance, sequence order, ultimate progression and two-handed grouping are unchanged.

## Evidence

All results below use the exact shipping assembly above, after the final interaction repair.

- **174 model assertions**, covering nine tutorial paths, task gating, loadout changes, practice, reset and preferences.
- **1,256 native UI assertions**, including the model cases plus actual mouse-handler interactions, all reward-name/effect/stat text metrics, ordinary and tutorial placement, stored augment hosts, pending-reward inventory clicks, linked groups, and checkpoint layouts at 1280×720, 1920×1080 and 2560×1080. `TUTORIAL_REVIEW_SUCCESS`.
- **27 native screenshots** in `v074-ui/`: 20 tutorial/checkpoint captures and seven presentation captures. Inspected focused mixed rewards, augment hosts, the full sheet, tutorial guidance, rarity mismatch, fusion preview and result, augment details, icon key and combat. `PRESENTATION_REVIEW_SUCCESS`, 61 mapped definitions. Captures use Unity's own routine; no OS input automation.
- **15 core fixtures**, all passed in `v074-core-*.json`: 533 production assertions per run, zero visible spawns and zero sequence-order errors. Coverage includes weapons, paired attacks, reloads, armor, manual ultimate, speed controls, fusion, body accounting, scoring and kill attribution.
- **One complete eight-wave fixture**, passed in `v074-full-full-1741.json`: 314 kills, 144 collisions, 13 item choices, two augment choices and 533 assertions.
- **Launcher checks** passed: valid installation, checksum rejection, failed-build rollback, archive containment and offline fallback. All tracked PowerShell scripts parse successfully; `git diff --check` passes.

Combat fixtures use 5,000 starting integrity to exercise systems; these are regression tests, not difficulty estimates. Native scripted checks and visual inspection do not establish first-time-player comprehension. No balance changes were applied or inferred from these results.

## Reproduce

```powershell
& '.\Build Windows.ps1' -OutputDirectory (Join-Path $PWD 'Build\Clarity-v074-final')
$player = Join-Path $PWD 'Build\Clarity-v074-final\WalkingHard.exe'
& '.\Validate Tutorial.ps1' -Player $player
& '.\Validate Tutorial.ps1' -Player $player -Rendered
& '.\Validate Prototype.ps1' -Player $player -ReportPrefix fresh-core -HeroHealth 5000
& '.\Validate Variety.ps1' -Player $player -Builds full -FullRun -ReportPrefix fresh-full -HeroHealth 5000
& '.\Launcher\Test Launcher.ps1'
```

The rendered review briefly opens a native window and saves captures beside the player. For the presentation suite, run the player with `-vectorQA -qaBuild ui -qaHealth 5000 -qaHoldCheckpoint -presentationReview -force-d3d11 -screen-fullscreen 0 -screen-width 1280 -screen-height 720`.
