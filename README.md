# Walking Hard 0.5.2 — Visible Action Delays

A Unity autobattler: one fighter walks forward through a physical crowd, seen from behind and above. Build a five-action loop, discover weapons, and turn bodies into chain reactions.

## Play

Download the Windows ZIP from [Releases](https://github.com/Chillysoop/walkinghard/releases/latest), extract the whole archive, and run WalkingHard.exe. For an updating desktop shortcut, run Launcher/Install.cmd. **Walking Hard (Main)** checks main, verifies the published ZIP checksum, and launches it. Its files and log live in %USERPROFILE%/Games/WalkingHard.

If main has no matching release, the launcher can build it in an isolated cache using Git and Unity 6000.3.27f1. Offline or failed updates retain the last working build. It never resets your development checkout.

## Playable systems

- Five movable sequence positions: two permanent weapon attacks plus three drafted abilities. The sequence always runs automatically.
- Matching adjacent one-handed attacks fire together with independent targets. Two-handed weapons fill both mounts and lock their basic cards into one movable group.
- Fists, swords, hammers, pistols and rifles with distinct attacks, visible preparation/reloads and behavior upgrades. Both equipped weapons stay in their mounted hands during abilities and ultimates; a two-handed weapon remains one model.
- Temporary amber tile bars explain RELOADING, READYING and WAITING FOR PAIR with remaining combat seconds. They disappear when the delay completes.
- Separate ultimates with 12–16 second countdowns, automatic queuing or manual activation, and preserved sequence order.
- Two opening ability drafts; checkpoint one grants a third ability and a weapon choice. Later rewards mix item types and duplicate bundles.
- Drop a reward directly into a compatible slot to equip and advance; double-click auto-equips. Edit after pick keeps the character sheet open for rearranging until Let's fight.
- RPG character sheet with live Walker preview, surrounding gear mounts, an icon storage grid, full hover cards and animated before/after fusion feedback.
- Helmet, armor and boots grant rarity-scaled stats plus stagger damage, resistance or regeneration passives. They join mixed checkpoint rewards.
- Drag-and-drop character sheet, unlimited storage, two weapon mounts, one rig, and a separate ultimate mount. Click-to-select/destination is also supported.
- Common, Uncommon, Rare, Epic and Legendary drops. Three identical items of the same rarity fuse to the next rarity, capped at Legendary.
- Rarity odds: 55% / 27% / 12% / 5% / 1%. Power: 1× / 1.2× / 1.5× / 1.9× / 2.5× Common.
- Distinct category colors and icons, labeled rarity borders, fusion previews, synergy inspection, and item-owned kill counters that follow weapons and abilities between slots.
- Combat speed: 0.5x, 1x, 2x or 4x. The angular combat HUD and checkpoint sheet share colors and icons; combat tiles retain descriptions and live stats.
- Bodies persist for the run. Walkers push resting corpses aside without slowing down; launched bodies still cause collision damage.
- Physical crowd collisions, ragdolls, authored waist cuts, and bounded multi-hop collision cascades. Hidden side-door/horizon creation remains enforced.
- Combo scoring for rapid, bulk and chain kills. Three-second banking window, up to 10× multiplier, checkpoint banking and end-of-run score.

## Controls

| Input | Action |
| --- | --- |
| Enter | Begin / auto-equip selected reward and advance / continue after editing |
| 1 / 2 / 3 | Select draft offer |
| Q / W / E / D / C | Assign draft ability to an empty sequence position |
| Drag, or click then click destination | Reorder, equip, or store an item |
| Double-click reward or stored item | Auto-equip (unrelated abilities are stored if the loop is full) |
| Speed buttons | 0.5x / 1x / 2x / 4x combat simulation |
| Tab | Spend a banked reroll |
| U | Toggle ultimate Auto/Manual |
| F | Queue a ready ultimate |
| Space / Escape | Pause or resume |
| R | New run |
| T | Repeat seed |
| M | Mute |
| F8 | Save screenshot |

There is no toggle for the normal sequence. An ultimate never interrupts an action or a simultaneous pair. Combat, physics, combo expiry and ultimate countdowns stop at checkpoints. Pause includes reduced motion. Physics can differ slightly across machines even with the same draft seed.

## Develop and verify

Use **Unity 6000.3.27f1** with Windows Build Support. Open Assets/Vector/Scenes/CombatStudy.unity. The project uses URP and procedural geometry, animation and audio; no paid assets are required.

Run Build Windows.ps1, then Validate Prototype.ps1. Fifteen standalone scenarios cover normal drafts, distinct weapon families, paired targets, manual ultimates, exhausted rerolls, rarity/fusion contracts, scoring, attribution, hidden entries and physical outcomes, plus temporary reload/readying bars and paired reload waits, persistent held weapons through casts, mounted-hand effects after reordering, persistent bodies, walking shoves, half/quadruple speed, armor passives and one-step reward progression. Named weapon scenarios use explicit weapon fixtures to exercise every family independently of draft luck. Run Launcher/Test Launcher.ps1 for updater safety checks.

| Source | Responsibility |
| --- | --- |
| AbilityDeck, LoadoutCatalog, ItemRarity | Inventory, equipment, offers, grouping, fusion, content and rarity |
| VectorEncounter, EncounterCombat, WeaponCombat | Waves, ordered scheduler, ultimates, weapon attacks, attribution and cascades |
| FighterPhysics, FighterRig, EntryLayout | Physical actors, cuts, procedural weapons and concealed entries |
| PrototypeHUD, CheckpointHUD, ItemPresentation, HUDInteractions | Combat, loadout drag/drop, category/rarity presentation and icons |
| ComboScore | Kill strings, multiplier and banking |
| LoadoutChecks, EncounterQA | Contract assertions and standalone encounter reports |

See [the 0.5.2 verification report](QA/v052-verification.md) and [the 0.5 UI checks](QA/v05-verification.md) for checks and screenshots.

C# scripts are under Assets/Vector/Scripts. See [DESIGN.md](DESIGN.md) for the current design contract and [CONTRIBUTING.md](CONTRIBUTING.md) for collaboration/release instructions. Historical Vector paths remain for asset stability. Art, balance and long-run progression remain prototype work; this release uses authored cuts rather than arbitrary mesh slicing.