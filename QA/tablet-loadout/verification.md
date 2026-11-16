# Field tablet loadout screen

November 15, 2026. Unity 6000.3.27f1. Player: `Build/TabletLoadoutFinal/WalkingHard.exe`.
Assembly SHA-256: `658b0259b5b4540879dbf62cb9b1ba429a9507ddc902a3fe0b2ff6c56fe8d2d4`.

The ordinary checkpoint now reads as a Dead Letter Office field tablet: a graphite chassis, teal diagnostic glass behind equipment, woven warm-gray storage, and a brushed blue sequence panel. Filled item cells, recessed icons, brighter small text, rarity rails and a filled primary action separate content and interactions. The built-in image_gen tool supplied the unmodified four-material atlas; the [full prompt](../../Art/tablet-panels-prompt.txt) and [production asset](../../Assets/Resources/TabletUI/PanelMaterials.png) are committed. No text or controls are baked into the image.

Nine-slicing preserves panel corners while the live layout scales. Native captures cover 1280x720, 1440x900, 1600x1200, 1920x1080 and 2560x1080. Taller displays add storage rows, up to 24 cells per page; the 4:3 fixture exposes all nineteen stored items. Equipment mounts spread vertically. Resizing an open hover comparison clears its old geometry, retains the selected item and preserves the complete loadout.

There is no permanent suggestion panel. The existing character, equipment, storage, sequence and ultimate remain together. Compatible targets glow, exact replacements appear on destination hover, and fusion animates on the claimed reward card. Normal combat and guided tutorial styling remain unchanged.

Final native verification:

- Loadout model: 312 assertions; rendered review: 1,586 assertions, including those model checks, real placement handlers, text metrics, five resolutions, live resizing and non-overlap with every interactive target.
- Tutorial model: 174 assertions; rendered review: 1,482 assertions, including model checks and the actual journey opening checkpoint.
- Presentation fixture: 64 mapped item definitions, with rarity mismatch, fusion, augment, icon-key and combat captures inspected.
- Review saves used isolated directories. Generated scene-ID churn was restored; `git diff --check` passed.

These checks establish rendering and interaction correctness; player feedback is still needed to judge usability.

[Overview](overview.png) · [Actual journey opening](journey-opening.png) · [Replacement comparison](hover-comparison.png) · [Fusion result](fusion-on-reward.png) · [Augment](augment-details.png) · [16:10](16x10.png) · [4:3](4x3.png) · [Ultrawide](ultrawide.png).
