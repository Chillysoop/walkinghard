# Walking Hard 0.6.2 — item identity and fusion clarity

29 October 2026. This is a presentation update. Fusion still happens automatically at three copies of the same item definition and rarity, across equipped items and storage. Legendary remains the cap. Augments still scale from their host and do not fuse. Combat values, rarity odds and acquisition rules are unchanged from 0.6.1.

## Player-facing changes

- Original artwork covers all 56 definitions: 26 weapons/armor/rigs, 10 abilities, 4 ultimates and 16 augments. A stable ID-to-atlas mapping shares each image across rewards, storage, equipment, combat, tooltips and fusion animation.
- Reward previews state how incoming copies affect the matching set. Stored cards show the rarity initial and matching-copy count. Storage sorts by type, definition and rarity; ownership and fusion priority are unchanged.
- Hover details show the exact set, the next tier and other-rarity copies that do not count. Matching equipped/stored cards highlight together. A shared Fists item counts once; a two-handed item remains one item.
- A claimed fused reward says **Fusion complete**, rather than showing zero consumed Common copies as if progress had been lost. The existing animation now frames the three source images and explicitly labels **3 Common → 1 Uncommon**, with before/after stats.
- Augments use a neutral border, a hexagonal category symbol, and explicit **host-scaled / no fusion** wording. Common through Legendary retain written rarity and colored borders.
- Category symbols and 15 labeled synergy symbols have a shared Icon Key. The key blocks underlying clicks and keyboard draft actions; Escape closes it. Tooltip panels stay available while moving the pointer into their scrollable details. The upcoming encounter preview is preserved.

Artwork was generated with the built-in image_gen tool. Source PNGs are in `Assets/Resources/ItemArt/`; complete prompts and mapping documentation are in `Art/`. The importer preserves atlas dimensions and disables mipmaps/compression.

## Native visual verification

The rendered Windows fixture `-vectorQA -qaBuild ui -qaHoldCheckpoint -presentationReview` passed catalog coverage for all 56 IDs and loaded all three atlases. It exercised Common and Rare copies of the same helmet, an eligible third Common reward, a two-copy ability offer, a Legendary cap, augment details, the Icon Key, actual automatic fusion and subsequent combat.

The checkpoint and combat layouts were inspected at 1280×720 on the regression candidate, then at 1920×1080 on the final player. Seven final native captures are included:

- [Checkpoint](v062-checkpoint.png)
- [Different rarities do not count](v062-rarity-mismatch.png)
- [Third-copy fusion preview](v062-fusion-preview.png)
- [Augment has no independent rarity/fusion](v062-augment-details.png)
- [Category, rarity and synergy key](v062-icon-key.png)
- [Fusion result and before/after](v062-fused.png)
- [Combat uses the same item artwork](v062-combat.png)

These are fixture-driven captures of the actual Unity UI. The fixture is opt-in and exits automatically; it is not part of regular play or high-volume simulations. Native screen capture requires a rendered game window; hidden-window capture was unsuitable and was not used as evidence.

## Regression results and provenance

All **15 standard standalone regression scenarios passed**, each with **66 production-effect assertions**: sequence (seeds 1741 and 4319), sword, hammer, rifle, twin, tempest, manual ultimate, reroll, half speed, fast speed, armor, quick equip, armed casting and reload. Reports are `v062-<scenario>-<seed>.json`. They cover mixed-rarity non-fusion, legal fusion, direct equip, ordered pairs, kill attribution, delayed actions, held weapons, ultimate controls, speed clocks, persistent bodies and concealed entry.

Those fifteen scenarios used `Build/CardClarity-v062-release/WalkingHard.exe`, Assembly-CSharp.dll SHA-256 `ee4aa33090aee8a016a0956f84e1ed073fc16e4c985e912f9de6dc0c091e308b`.

The final player is `Build/CardClarity-v062-publish/WalkingHard.exe`, Assembly-CSharp.dll SHA-256 `328125054f3763af756b10c9c9cca86bc792f523f75ee72eafb26946c0756c20`. Its final two presentation corrections apply the GUI scale before tooltip hit-testing and pin augment atlas entries to explicit IDs instead of catalog ordering. On this exact player, the 1920×1080 native fixture passed all resource/coverage checks and startup assertions; the additional `v062-final-sequence-1741.json` regression also passed all 66 assertions and completed the encounter.

No balance changes or replacement simulation model were introduced. Fast simulations never instantiate this HUD, so card textures are not loaded for screening batches.
