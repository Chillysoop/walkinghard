# Walking Hard — art and VFX pass

Plan prepared October 29, 2026, against Unity 6000.3.27f1 / URP 17.3.0 and the v0.7.1 Pursuer finale. This is a proposed presentation pass; no assets have been imported or gameplay rules changed.

## Direction

**An armored martial artist walking calmly through industrial sci-fi carnage.** Keep the angular forms and dark teal environment from the approved UI reference, but give bodies weight, equipment distinctive silhouettes, and impacts clear direction. The hero is a person in armor; robot packs supply useful armor/weapon parts and enemy candidates without silently changing that identity.

Use one primary asset family, a shared material palette and consistent lighting. Hero: light teal armor and a bright readable visor. Ordinary enemies: darker rust/red. Shield bearers: a broad shield silhouette. Carriers: obvious amber canisters. The Pursuer: a unique broad, heavy silhouette with deliberate footfalls. Contrast, shape and motion identify roles as well as color.

Preserve the existing rarity colors, labeled synergy icons, empty equipment slots, descriptions, five-action order and separate ultimate. World effect colors communicate the effect; card borders communicate rarity. A legendary lightning spell should still look like lightning.

## Free asset shortlist

Official pages were checked on October 29. File contents and Unity imports still need a small trial before committing to a pack. Asset purchase budget for this plan: **$0**.

| Use | Recommended source | How we would use it |
| --- | --- | --- |
| Corridor and props | [Quaternius Ultimate Modular Sci-Fi Pack](https://quaternius.com/packs/ultimatemodularscifi.html) | CC0, 46 models. Start with a restrained selection of walls, doors and floor modules; keep collision geometry separate. |
| Alternative corridor detail | [Quaternius Modular Sci-Fi Megakit](https://quaternius.com/packs/modularscifimegakit.html) | CC0; use its free Standard subset only. The preconfigured engine projects/custom shaders belong to the Source tier, so budget our own URP materials. Choose one corridor kit after the import trial. |
| Guns, props and enemy candidates | [Quaternius Sci-Fi Essentials Kit](https://quaternius.itch.io/sci-fi-essentials-kit) | Standard download is free and CC0. Inspect the actual free subset; the full catalogue and Unity Source project are not all included in that download. |
| Weapon variety | [Quaternius Sci-Fi Modular Gun Pack](https://quaternius.com/packs/scifimodularguns.html) | CC0. Give pistol, scattergun and rifle clearly different barrels, stocks and lengths. Recolor through our shared palette. |
| Animation | [Quaternius Universal Animation Library](https://quaternius.com/packs/universalanimationlibrary.html) | CC0 with a free Standard subset. Trial walk, run, turn and attack clips on one humanoid first; verify which clips are included free. Keep combat timing owned by our existing simulation. |
| Boss armor reference/parts | [Quaternius Animated Mech Pack](https://quaternius.com/packs/animatedmech.html) | Four CC0 animated mechs. Optional source of heavy mechanical forms; keep the approved humanoid giant rather than replacing him wholesale. |
| Sparks, smoke, flashes | [Kenney Particle Pack](https://kenney.nl/assets/particle-pack) | 80 CC0 textures. Build our own small, pooled effect families so they share timing, scale and color. These are textures, not a ready-made URP combat-effects system. |
| Impact and technology audio | [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds), [Sci-fi Sounds](https://kenney.nl/assets/sci-fi-sounds) | CC0. Layer a short attack transient, material impact and occasional bass/body hit; vary pitch subtly. |
| Supplemental UI components | [Kenney UI Pack — Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi) | CC0. Borrow only suitable small components. Retain our approved checkpoint/combat layout and existing synergy icon vocabulary. |

Prefer these CC0 sources for a repository friends can clone. Save each downloaded license and original URL in `Assets/ThirdParty/<Publisher>/<Pack>/`, and record imported filenames and modifications in `THIRD_PARTY_ASSETS.md`. Import only used assets. Preserve `.meta` files; no API keys, accounts or giant demo projects in Git.

[Mixamo](https://helpx.adobe.com/be_en/creative-cloud/faq/mixamo-faq.html) is a fallback if a needed animation is absent: Adobe lists it as free with an Adobe ID and permits commercial game use. It is not CC0; check source-file sharing terms before putting its files in the shared repository. The base plan does not depend on it.

## Implementation order

### 1. One representative combat scene

Create an art-review scene using the real gameplay camera and lighting: one hero, a raider, an enforcer, shield/carrier variants, a short corridor and the Pursuer at near/far distances. Import a small asset sample, unify scale and material response, and compare silhouettes at actual gameplay size. Establish a neutral key light, cool rim light, warm doorway accents and restrained bloom. Keep the play area clear of foreground clutter.

**Exit:** a 10-second screenshot/video comparison demonstrating that the hero, threat roles and equipped weapons are easy to identify. Confirm the selected models can support the required cuts before expanding the scene.

### 2. Character, weapons and motion

Refine the hero first: readable shoulders, helmet, torso, hands and boots; visible equipment at both hand sockets. Swords and hammers get distinct one/two-handed silhouettes. Retarget locomotion where useful; retain authored upper-body poses for paired attacks and casting with weapons held. Blend turns and recoveries while keeping the planted feet in the final stand.

Separate visual animation from the motor, target selection and damage timing. Imported root motion must not move actors or alter reach, action duration or sequence order. Equip/unequip and two-handed grouping must work before replacing every model.

**Exit:** fists, sword, hammer and gun each read correctly at 0.5×, 1× and 4×; casting never hides equipped weapons.

### 3. Weapon-specific impacts and bodies

| Family | Presentation treatment |
| --- | --- |
| Fists / stagger | Brief contact flash, compressed torso reaction, directional thud and short dust burst. Heavy punches visibly displace the victim. |
| Swords / sever | Tight blade arc, directional slash flash, cut surface and separating body pieces following the actual strike direction. Greatswords get a wider, heavier arc. |
| Hammers / impact | Strong anticipation, dense hit burst, short floor dust ring and a weighty knockdown. Follow collision direction through subsequent bodies. |
| Guns / pierce | Readable muzzle flash, thin shot trail, hit spark and distinct reload motion/sound. Rifle piercing draws one clean line through its victims. |
| Electricity | Branching arcs attached to struck targets, brief jitter and a persistent Conductive marker. Keep chain connections visible through a crowd. |
| Gravity / launch | Inward-flowing lines for pull, a clear launch vector for push, and short trails on dangerous airborne bodies. |
| Lifesteal | Small crimson energy threads returning from damaged targets to the hero; distinguish marked targets from actual healing. |
| Guards / armor | A brief shield response at contact and an unmistakable break/expiry cue. Show successful blocks differently from taking damage. |

Use authored cut variants mapped to hit regions: torso halves and selected limb fragments, with capped cut surfaces. Adapt the chosen enemy mesh to the existing eleven-body ragdoll and sever system. Arbitrary runtime mesh slicing is a separate engineering project and is not required for this pass. Bodies remain persistent until passed, remain pushable and do not block walking, as approved.

**Exit:** sword, hammer and electrical kills are recognizable without reading the HUD; launch/collision chains remain readable with a crowded floor. Effects never apply extra damage or physics impulses themselves.

### 4. Abilities, ultimates and Pursuer

Apply the same effect vocabulary to every ability and augment. Put visual emphasis on the main payoff, with quieter persistent marks and small secondary procs. Reuse pooled materials/textures rather than importing unrelated effect packs per ability.

For the finale, emphasize movement already happening during the camera swing: heavy alternating footfalls, shoulder sway, restrained ground dust and a low mechanical rhythm. The army should be visible in layers behind and around him. Keep the same walking cycle into combat, then clearly telegraph his windup and hit. Use lighting and atmospheric depth to sell scale without obscuring the distant swarm.

**Exit:** the boss reads as catching up to a stopped hero, and the player can still see incoming attacks and the sequence during an ultimate.

### 5. Cards, checkpoint portrait and sound mix

Render weapon/gear thumbnails from the equipped models with one camera angle, crop and lighting setup. Give abilities a consistent illustrated/glyph treatment, retaining the current stable item-ID mapping. Keep type, rarity, synergy and effect description as separate visual signals. Update the checkpoint portrait to show the same gear used in combat. Reuse the same icons for picks, inventory, sequence and tooltips.

Tune the sound mix last against full crowds: limit duplicate hit sounds, prioritize hero attacks and boss warnings, and preserve readable reload/fusion cues. Audio and optional screen shake reinforce hits rather than hiding action with constant noise.

### 6. Performance and regression gate

Profile before and after on the same machine, resolution, seeds and maximum-crowd scenes. Target 60 FPS at 1080p on the playtest machine; verify actual frame times rather than assuming low-poly assets are cheap. Pool particle bursts and decals, cap simultaneous lights and secondary effects, and simplify distant rendering. Do not remove approved persistent bodies to recover performance.

Check normal fights, a dense collision pile, electricity chains, all ultimates, the reversed boss camera, checkpoint portrait, rarity/fusion and 4× speed. Reduced-motion settings must still work. Headless Fast simulations must not instantiate presentation assets. Run the core/build-variety checks plus a full finale and compare gameplay results against the pre-art build.

## First delivery

Deliver a polished **20-second sample** before replacing everything: one corridor section, the equipped hero, two enemy silhouettes, sword sever, hammer impact, electrical jitter and a short Pursuer approach. Review the look, readability and frame time. Then expand the approved treatment across the catalogue.

The substantial custom work is rig/socket integration, cut meshes, ragdoll mapping and effect timing. Free assets reduce modelling effort; they do not make those integrations automatic. No paid packs, new gameplay features or changes to the approved loadout rules are required by this plan.
