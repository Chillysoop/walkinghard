# Walking Hard 0.6.7 — verification

29 October 2026. Final player: `Build/Siphon-v067-release/WalkingHard.exe`.

Assembly-CSharp.dll SHA-256: `d5836851c2f09c1d99193aa82fede820fd0d0cd9279a79b5ab1ac2960a4a8639`.

## Behavior

Eight weapon modules preserve native attack damage/timing/coverage; Piledriver preserves multi-launch and splash, Bore preserves ricochets/scatter and adds no focusing delay, and Breach uses normal reload timing. Separate paired weapons retain full module effects. Shared starter fists and two-handed weapons retain one socket/effect. Counterguard's primary burst stays full and four secondaries receive 25%; Amplifier remains full.

Siphon Blade, Drain Brand, Reclaimer Armor, Predator Boots and Harvest use normal five-tier rarity/fusion and checkpoint-three ultimate progression. Lifesteal heals only actual damage to living enemies, with a shared 1% max-HP originating-action cap across simultaneous hits, follow-ups and delayed collisions. Harvest has its own rarity-scaled budget. Focused supports Drain Brand. Healing totals follow fusion ownership.

## Production and encounter checks

- 15 core scenarios passed on the first candidate: sequences, all weapon families, pairing, manual ultimates, rerolls, half/quadruple speed, armor, quick equip, persistent weapons and reload indicators.
- All 45 variety scenarios passed: nine hybrid builds, all sixteen modules, paired/separate/shared/two-handed layouts. Each ran 271 production-effect assertions.
- The final gameplay candidate passed the complete eight-wave fixture and the four-wave ultimate-progression fixture with 272 assertions each.
- The release passed the four-wave progression fixture with 273 assertions, including the added healing-total fusion check.
- QA encounter fixtures use explicit 5,000 health for coverage. Balance runs use ordinary health; QA wins are not balance evidence.
- Controls reproduce 24 scalar gameplay fields across 32 seeds for each comparison, with zero differences. Source changes between tested binaries are recorded in the balance report.
- Launcher checks pass valid install, checksum rejection, failed-build rollback, archive containment and offline fallback.

New probes cover every Lifesteal rarity, actual versus overkill damage, corpse exclusion, per-origin healing cap, delayed collisions, simultaneous pairs, new-action renewal, maximum-health clipping, mark expiry/refresh, low-health armor behavior, real-heal-only haste, weapon/ultimate charge preservation, Harvest limits, fusion attribution, preserved pistol ricochets, no added focusing/reload delay, inline follow-up scheduling and Counterguard/Amplifier separation.

## Native visual inspection

The native renderer produced six checkpoint/combat captures after hidden-window captures failed. Inspected: [character sheet](v067-checkpoint.png), [augment before/after](v067-augment-preview.png), and [combat](v067-combat.png). The sheet exposes Lifesteal, new category/rarity art, a distinct Siphon symbol, clear effect descriptions and installed module icons. The preview states the compatible owned host and opening damage/action-time comparison.

These captures precede the small Focused Drain Brand extension, persistent Siphon mark cue and healing-fusion bookkeeping repair; the pictured layouts and art did not change afterward. The user stopped Computer Use, and no further native UI automation was performed. This does not substitute for subjective player testing of feel or sound.

Five individual PNG assets were generated with the built-in image_gen tool and inspected before copying into `Assets/Resources/ItemArt/`. Complete prompts and mappings: `Art/siphon-prompts.json`. No existing item art was replaced.

## Limits

Fast mode approximates physical contact and body motion. Six matched Full games provide coverage, not statistical certainty. Shared collision/derived-hit limits, ordinary sequence order, two-handed grouping and ultimate gating remain in place. No arbitrary mesh slicing or global difficulty changes are included.
