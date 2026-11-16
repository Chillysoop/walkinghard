# Loadout UI without the suggestion column

November 15, 2026. Unity 6000.3.27f1. Player: `Build/InlineLoadout/WalkingHard.exe`. Assembly SHA-256: `dd7a7abb633456d71c6b41a3491929b9bf96181a923547fd8412ba746e51088e`.

The permanent suggestion/comparison column is removed. The walker and storage use the full middle row; storage shows twelve named items per page. Hovering a reward only lights compatible destinations. Select an item, then hover a destination for its exact comparison. The compact hover details close on leaving and remain scrollable while the pointer is inside. Placement checks ensure they never cover rewards, equipment mounts, stored items or sequence destinations; portrait space is the fallback.

Fusion now animates on the reward card that was taken, while the promoted slot pulses. Chained upgrades show the final rarity. Inventory, sequencing, equip rules, rarity odds, quick equip and the after-pick preference are unchanged.

Validation:

- Final native workbench: 1,171 assertions, including 312 detached-preview checks, native mouse input, hover-only visibility, non-overlap with interaction targets, storage capacity per page and all three resolutions.
- Final tutorial review: 1,482 assertions, including its 174 model checks.
- Final item presentation: 64 mapped definitions; native checkpoint, fusion, augment and combat captures.
- Core gameplay: 15/15 scenarios; 761 production-effect assertions per scenario, no visible spawns or order errors. This suite preceded the final source-item text correction in hover details; that presentation correction was rebuilt and rechecked with the native workbench, tutorial and presentation reviews.
- `git diff --check` passed; generated scene identifier churn restored.

Review saves were isolated. These are production-handler and screenshot checks, not a claim of player usability approval.

[Overview](overview.png) · [Hover comparison](hover-comparison.png) · [Fusion on the reward](fusion-on-reward.png) · [Augment details](augment-details.png) · [Ultrawide](ultrawide.png).
