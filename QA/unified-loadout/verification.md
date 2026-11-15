# Unified checkpoint workbench verification

November 15, 2026. Unity 6000.3.27f1. Local player: `Build/UnifiedLoadout/WalkingHard.exe`. Assembly SHA-256: `a106a06cfbbf994e59810dbacd9f81f5e2d63af75a64b6caffaaa040b5045c07`.

The ordinary checkpoint keeps the walker, equipment mounts, named storage, sequence and ultimate visible together. A fixed comparison shows exact destinations and replacements. Cyan indicates interaction; item borders and written rarity labels retain their rarity colors. Rewards select before an explicit equip/store action; owned items retain quick double-click equip. The existing after-pick preference is preserved. Full mechanics and synergies scroll within the comparison, without covering destination hit regions.

The preview executes production acquisition and placement on a detached graph. Tests compare its result with the real graph and verify no live item, socket, receipt, event or random-stream mutation. Cases include every item, three-copy fusion at all tiers, mixed rarities, chained upgrades, bundles, shared fists, both-hand replacement, locked action reorder, displaced abilities and retained/returned socket modules. The result animation celebrates the final tier of a chained fusion. Reduced motion skips the merge motion.

## Validation

- Final native workbench: **1,163 assertions**, including 312 model/projection assertions, production mouse handlers, primary/Enter routing, exact empty-slot and Store previews, invalid-drop rejection, and text bounds for the catalog. Native fixtures are 1280×720, 1920×1080 and 2560×1080.
- Tutorial: **174 model assertions**, **1,482 native assertions**. The guided lesson and ordinary checkpoint flows use the production handlers. Final rebuilt player passed the rendered run.
- Presentation: **64 mapped definitions**, with checkpoint, rarity mismatch, fusion, augment, icon-key and combat captures. Final rebuilt player passed.
- Core gameplay: **15/15 standalone scenarios**, each reporting 761 production-effect assertions. Zero visible spawns and zero sequence-order errors. Compact results are in `regressions.json`. This suite ran before the final read-only fix separating an owned ability's suggested destination from a pending reward's keyboard slot; the final native review explicitly verifies that correction.
- `git diff --check` passed. Generated scene identifier churn was restored; no balance, inventory capacity, rarity odds, fusion requirements, save schema or combat timing changed.

All review profiles used isolated save directories where applicable. Core QA runs use their established non-player QA path. Tests inject events through production handlers rather than OS input. Automated checks and screenshot inspection do not substitute for the player's usability feedback.

## Visual evidence

- [Whole build](01-whole-build.png)
- [Rarity preview](02-hover-fusion.png)
- [Both-hand comparison](03-two-hands.png)
- [Chained rarity result](05b-chain-upgrade-result.png)
- [Explicit ability replacement](06-ability-replacement.png)
- [Store preview](07b-store-preview.png)
- [Ultrawide layout](08-layout-2560.png)
- [Actual journey opening](journey-opening.png)

The tutorial keeps its guided full-sheet coach. Ordinary rewards use the unified workbench. This build is local development work, not a published release.
