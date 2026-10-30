# Walking Hard 0.7.7 verification

October 30, 2026. Unity 6000.3.27f1. Tested shipping player: `Build/Recoil-v077/WalkingHard.exe`. Assembly SHA-256: `4e0e096d60d0363483e091252caebf53ec66a0f2a82635e13768a250ed5ddaf8`.

## Change

Recoil Harness checks whether the base gun hit killed its target before consuming either its stored charge or the action's one-shot proc allowance. A later surviving contact may use it, including piercing, ricochet, the other paired hand or a subsequent action. Bonus damage and kills remain owned by the armor. Hover text and the design contract explain the updated rule. All authored item values are unchanged.

## Verification

- **15 core fixtures passed**: `v077-core-*.json`.
- **45 build-variety fixtures passed**: `v077-variety-*.json`.
- **One full eight-wave progression fixture passed**: `v077-full-full-1741.json`, with 315 kills, 143 collisions, 13 item choices and two augment choices.
- **607 production-mechanic assertions per fixture**, including 30 new checks at all five rarities: lethal-first piercing, no repeated proc, same-action paired-hand carryover, armor-owned bonus kills excluding overkill, ricochet carryover, and carryover into a later action. Existing refill eligibility, guard expiry, barrier ordering, shared guard, removal, attribution and one-shot checks remain intact.
- **1,342 native UI assertions passed** via `Validate Tutorial.ps1 -Rendered`. The native game exercised mouse handlers, drag/drop, text metrics and ordinary/tutorial checkpoint flows. Selected captures and the assertion log are in `v077-ui/`; Recoil and Volley previews and the character sheet were visually inspected.
- **Item presentation review passed**, with 63 mapped definitions. Its seven checkpoint, rarity mismatch, fusion, augment, icon-key and combat captures were inspected and retained in `v077-ui/`.
- **216 controlled games** completed without simulation integrity failures or timeouts: 204 Fast, 12 Full. All use the same shipping assembly. Results and limitations are in `v077-balance-review.md`.
- No visible spawns or sequence-order errors in the final regression suite. No failed or retried fixtures this turn. Publisher PowerShell syntax and Git whitespace checks passed.

The initial Pinball smoke fixture also passed 607 assertions. It is not counted again in the 61-fixture regression total. Regression fixtures use **5,000 integrity** to exercise systems; balance comparisons use **normal 280 base integrity**. The previous version's transient Ballet PhysX instability did not recur here; this is not a claim of an unrelated physics fix.

## Reproduce

```powershell
& '.\Build Windows.ps1' -OutputDirectory (Join-Path $PWD 'Build\Recoil-v077')
$player = Join-Path $PWD 'Build\Recoil-v077\WalkingHard.exe'
& '.\Validate Prototype.ps1' -Player $player -ReportPrefix fresh-core -HeroHealth 5000
& '.\Validate Variety.ps1' -Player $player -ReportPrefix fresh-variety -HeroHealth 5000
& '.\Validate Variety.ps1' -Player $player -ReportPrefix fresh-full -HeroHealth 5000 -Builds full -FullRun
& '.\Validate Tutorial.ps1' -Player $player -Rendered
```

Item presentation uses `-vectorQA -qaBuild ui -qaHealth 5000 -qaHoldCheckpoint -presentationReview -force-d3d11 -screen-fullscreen 0 -screen-width 1280 -screen-height 720`. Native checks briefly show the game and use its own capture/input routines, without Computer Use. Simulation reproduction is documented in the balance report.
