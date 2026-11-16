# Walking Hard item artwork

## Field tablet materials

The ordinary loadout screen uses [PanelMaterials.png](../Assets/Resources/TabletUI/PanelMaterials.png), generated with the **built-in image_gen tool**. The complete prompt is saved in [tablet-panels-prompt.txt](tablet-panels-prompt.txt). This is an unmodified 1254x1254 source atlas: top-left graphite chassis, top-right teal diagnostic glass, bottom-left warm cargo weave, bottom-right blue control metal. `TabletUI.cs` samples each tile with a half-pixel inset and nine-slices its 6% perimeter into fixed-size UI corners. All text, item artwork, controls, highlights and responsive layout are live code. Texture import preserves the original dimensions, uses bilinear clamp sampling, and disables mipmaps and compression.

The material colors identify sections, never rarity or compatibility. Rarity rails and labels retain the existing five colors; cyan indicates interaction. Taller displays reveal additional storage rows instead of stretching twelve item cells. No generated text or rasterized screen layout is used.

The built-in image_gen tool created three original atlases in the existing dark science-fiction style. The complete prompts are saved in [ItemArt-prompts.json](ItemArt-prompts.json).

| Atlas | Layout | Definitions |
| --- | --- | ---: |
| [Equipment](../Assets/Resources/ItemArt/Equipment.png) | 6 columns × 5 rows; final four cells unused | 26 weapons, armor pieces and rigs |
| [Actions](../Assets/Resources/ItemArt/Actions.png) | 4 × 4; final two cells unused | 10 abilities and 4 ultimates |
| [Augments](../Assets/Resources/ItemArt/Augments.png) | 4 × 4 | 16 augment modules |

`ItemArtwork.cs` maps stable item IDs to atlas cells. The same image appears on rewards, stored items, equipment, sequence tiles, tooltips and fusion feedback. Rarity never changes that identity image. Labels and borders carry rarity; category glyphs and labeled synergy symbols remain code-drawn for consistency at small sizes. Augments use a hexagonal module illustration, a neutral border and explicit host-scaling text; they are not Epic items.

The Unity importer preserves atlas dimensions, disables mipmaps and compression, and uses bilinear filtering. Source PNGs remain unmodified. The native presentation fixture checks complete catalog coverage and captures actual game UI with `-vectorQA -qaBuild ui -qaHoldCheckpoint -presentationReview` in a rendered Windows player.


Siphon additions (0.6.7): five individual originals in `Assets/Resources/ItemArt/Siphon-*.png`, generated with the built-in image_gen tool. Full prompts and ID mapping are in [siphon-prompts.json](siphon-prompts.json). No image editing was used; Unity imports the supplied PNGs.


## Deadeye additions (0.7.6)

Two individual images were generated with the built-in imagegen tool. Prompts are in `deadeye-prompts.json`; original generated files remain in the local Codex generated-images directory. Project assets are `Assets/Resources/ItemArt/Deadeye-recoil.png` and `Deadeye-SuppressiveVolley.png`, with stable catalog-ID mappings and individual texture metadata. They use neutral gunmetal/teal identity art; UI rarity remains a separate frame and badge.
