# Walking Hard 0.7.3 verification

October 29, 2026. Unity 6000.3.27f1. Shipping player: `Build/Tutorial-v073-release/WalkingHard.exe`. Assembly SHA-256: `d2cbaa908224a193fddfd48214ee34702197db99417497b2f6b55f9a46a40f27`.

## Changes

- Start Run and Quick Tutorial in the main menu; return to the menu from pause, results or tutorial.
- Six hands-on tutorial tasks use production inventory and combat: ability, weapon, fusion, augment, sequence reorder and a short practice fight. All nine offered ability/weapon combinations work, including the two-handed block. Practice rewards and counters reset; prior speed and checkpoint preferences return.
- Shared destination glows apply to normal checkpoints, selected rewards, hover, dragging and stored items. Ability drafts identify the sequence before hover. Augments identify compatible equipped or stored hosts. Inventory interaction takes priority over a previously selected reward.
- Neutral category/synergy icons; explicit rarity borders/labels; cyan interaction cues. Score, next-action, lock, ultimate and augment styling no longer reuse the Legendary or Epic colors. Artwork and in-world VFX retain their colors.
- Shorter common descriptions and tutorial copy, plus concise fusion counts. Full rules remain in hover details, delayed by half a second so merely crossing a card does not immediately open a large panel. Duelist's summary now reports its actual ward hit count.
- No-host augments offer Store; ambiguous host choices require a socket. Upcoming encounter names remain in the footer. Tutorial instructions clear the rig slot, and weapon sockets no longer overlap item names.

## Evidence

- **787 tutorial/UI assertions**, passing on the shipping player. This includes nine complete tutorial paths, task gating, invalid placement, fusion ownership, augment attachment, two-handed ordering, pause, reset and preference restoration. Native mouse events pass through the actual drag/drop and double-click handler. Font metrics cover all 61 reward names, category labels, all rarity summaries, and every compatible augmented summary at the narrow checkpoint width.
- **21 native screenshots** in `v073-ui/`, reviewed for layout, glows, fusion before/after, mismatched rarity, module details, icon key, combat and tutorial. Checkpoints reviewed at 1280×720, 1920×1080 and 2560×1080. Captures use Unity's own screenshot routine; no OS input automation. `TUTORIAL_REVIEW_SUCCESS` and `PRESENTATION_REVIEW_SUCCESS` both reported success.
- **15 core fixtures**, all passing in `v073-verified-core-*.json`, and **one full eight-wave fixture** in `v073-verified-full-1741.json`. The full fixture completed with 314 kills, 144 collisions, 13 item choices and two augment choices. All include 533 production assertions; concealed spawns and strict order pass. Structural fixtures use 5,000 integrity and do not estimate difficulty.
- **45 build-variety fixtures**, all passing in `v073-variety-*.json`, covering archetypes, all augments, paired/separated weapons and two-handed layouts.
- **64 matched Fast games** against v0.7.2: seeds 81001–81064, mixed policies, eight waves, two workers. All gameplay fields and wave records match exactly after excluding only version and wall runtime; no invalid or missing games. See `v073-gameplay-regression.json`. This is a regression check, not new balance evidence.
- Launcher checks pass for installation, checksum rejection, failed-build rollback, archive containment and offline fallback.

The combat, full-run and 64-game comparison player has assembly hash `3fc78c5beaebab22baaf2da70dd17fa4495f0bf6a7b41b36223d352137a7ea36`. The final rebuild changes only weapon socket coordinates and adds the corresponding UI assertion; native interaction and presentation reviews were rerun on that exact shipping rebuild. The 45 variety fixtures used the earlier UI candidate `8014e85be558bf3f31568fd7c4ba9016592e1da0d32f0600232fd000ce69be79`, before the no-host Store fallback and final highlight/layout repairs. Subsequent changes were checked by the final core/full/regression and native suites. `v073-verification.json` records the build hashes and individual reports.

## Reproduce

```powershell
& '.\Build Windows.ps1'
& '.\Validate Tutorial.ps1'                       # Fast model checks, hidden
& '.\Validate Tutorial.ps1' -Rendered             # Visible native interaction/capture review
& '.\Validate Prototype.ps1' -ReportPrefix fresh-core -HeroHealth 5000
& '.\Validate Variety.ps1' -ReportPrefix fresh-variety -HeroHealth 5000
& '.\Validate Variety.ps1' -Builds full -FullRun -ReportPrefix fresh-full -HeroHealth 5000
& '.\Launcher\Test Launcher.ps1'
```

Pass `-Player` to use a different built executable. The native tutorial review writes `TutorialReview/` beside the player and tests three window sizes. For the standard item presentation suite, launch with `-vectorQA -qaBuild ui -qaHoldCheckpoint -presentationReview -force-d3d11`; it writes `PresentationReview/` beside the player and exits. A visible window is required on this machine. These are scripted production-control tests and visual inspection, not an independent first-time-player usability study.
