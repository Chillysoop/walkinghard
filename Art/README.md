# Walking Hard item artwork

The built-in image_gen tool created three original atlases in the existing dark science-fiction style. The complete prompts are saved in [ItemArt-prompts.json](ItemArt-prompts.json).

| Atlas | Layout | Definitions |
| --- | --- | ---: |
| [Equipment](../Assets/Resources/ItemArt/Equipment.png) | 6 columns × 5 rows; final four cells unused | 26 weapons, armor pieces and rigs |
| [Actions](../Assets/Resources/ItemArt/Actions.png) | 4 × 4; final two cells unused | 10 abilities and 4 ultimates |
| [Augments](../Assets/Resources/ItemArt/Augments.png) | 4 × 4 | 16 augment modules |

`ItemArtwork.cs` maps stable item IDs to atlas cells. The same image appears on rewards, stored items, equipment, sequence tiles, tooltips and fusion feedback. Rarity never changes that identity image. Labels and borders carry rarity; category glyphs and labeled synergy symbols remain code-drawn for consistency at small sizes. Augments use a hexagonal module illustration, a neutral border and explicit host-scaling text; they are not Epic items.

The Unity importer preserves atlas dimensions, disables mipmaps and compression, and uses bilinear filtering. Source PNGs remain unmodified. The native presentation fixture checks complete catalog coverage and captures actual game UI with `-vectorQA -qaBuild ui -qaHoldCheckpoint -presentationReview` in a rendered Windows player.
