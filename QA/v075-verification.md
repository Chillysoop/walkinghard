# Walking Hard 0.7.5 verification

October 29, 2026. Unity 6000.3.27f1. Shipping player: `Build/Rarity-v075/WalkingHard.exe`. Assembly SHA-256: `c344b90a6547d9fac59ca5ba2370f570afdad91128f54a425c2048de714786f6`.

Reward cards have a full rarity-colored outline plus the existing rarity badge. Selected cards retain that outline, with cyan selection guidance outside it. The shared renderer covers focused checkpoints, Manage build and the tutorial. Host-scaled augments retain a neutral border.

The focused upgrade screen now has an explicit **Add to storage** button beside Equip. It is visible before selection, enabled for a selected reward, and also accepts dragged rewards. It uses the production inventory placement handler, preserving equipped items and the Edit after pick preference. Existing same-rarity automatic fusion still applies; the initial ability drafts still require an empty sequence slot.

The exact shipping player passed **1,295 native UI assertions**, including click-to-storage across weapons, abilities, armor, rigs, ultimates and augments; preservation of the equipped sequence, linked weapons and armor; and continuation with Edit after pick both on and off. Existing tutorial, text-fit and interaction checks also passed. `TUTORIAL_REVIEW_SUCCESS`.

The presentation review passed with 61 mapped definitions. Native screenshots confirm rarity outlines, selected-card borders, storage-button placement, rarity mismatch, fusion preview/result, augment details, icon key and combat. The UI suite captures 1280×720, 1920×1080 and 2560×1080. Selected captures and the assertion log are archived in `v075-ui/`. `PRESENTATION_REVIEW_SUCCESS`.

All **15 core fixtures** passed on the same shipping player (`v075-core-*.json`), with 533 production assertions per run, zero visible spawns and zero sequence-order errors. These use 5,000 starting integrity to exercise systems, not estimate difficulty. Publisher syntax and `git diff --check` also pass.

No balance, item, fusion or combat rules changed. The earlier full checkpoint verification is in `v074-verification.md`.

## Reproduce

```powershell
& '.\Build Windows.ps1' -OutputDirectory (Join-Path $PWD 'Build\Rarity-v075')
$player = Join-Path $PWD 'Build\Rarity-v075\WalkingHard.exe'
& '.\Validate Tutorial.ps1' -Player $player -Rendered
& '.\Validate Prototype.ps1' -Player $player -ReportPrefix fresh-core -HeroHealth 5000
```

Run the player with `-vectorQA -qaBuild ui -qaHealth 5000 -qaHoldCheckpoint -presentationReview -force-d3d11 -screen-fullscreen 0 -screen-width 1280 -screen-height 720` for the item-presentation captures.
