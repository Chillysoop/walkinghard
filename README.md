# Walking Hard 0.7.4 — Clearer checkpoints

Ordinary checkpoints now open with three focused reward cards and the relevant equip destinations. **Manage build** opens the RPG character sheet, storage and full sequence on the same screen. Large Weapon / Ability / Gear / Augment / Ultimate labels, distinct art frames and neutral icons identify item type; rarity badges keep the rarity colors. One-line effects explain what the item does, and hover details retain the full mechanics. Preview text names the replacement, while primary buttons say Equip weapon, Equip helmet, Add ability, Attach to an item, or Store.

Two-handed groups, same-rarity fusion, independent ultimates and the existing reward schedule remain intact. Edit after pick still controls whether equipping advances immediately. An augment can now auto-attach to its sole compatible item even when that item is stored; multiple compatible hosts still require an explicit choice. See [verification](QA/v074-verification.md).

The main menu now offers **Start Run** and **Quick Tutorial**. The tutorial teaches ability selection, weapon equip, same-rarity fusion, augment sockets and sequence order using the actual drag/drop and double-click controls, then gives you a short practice fight. Starting a real run clears practice gear and restores your speed and checkpoint preferences.

Every checkpoint now highlights valid destinations while hovering or dragging rewards and stored items. Ability picks outline the sequence before you hover. Category icons and labels are neutral; rarity colors belong to rarity borders and labels, and cyan marks interaction. Cards and sequence tiles use short effect summaries, with full rules on hover. An augment with no compatible host can be stored directly; multiple hosts prompt you to choose a socket. See [verification](QA/v073-verification.md).

The finale now stops the Walker and turns him around. The camera slowly swings behind him to reveal a giant and its approaching army. Hold your ground as enemies pour in from the distance, flow around the giant, and reach your automatic sequence. The boss is staged just before the scene; no follower is simulated during earlier waves. See [verification](QA/v071-verification.md).

After waves 2 and 5, choose **Shield Line** or **Volatile Breach** for the next wave. Shield Line guarantees two equipment options in the existing mixed pick; Volatile Breach guarantees two ability options. The third option stays a wildcard. Reward counts, rarity odds and checkpoint-three ultimate progression are unchanged.

Shield bearers halve frontal damage until stagger opens their guard for two seconds. Flanking bypasses the guard. Volatile carriers detonate when a launched body impacts or finishes its flight; the explosion credits the launching item and shares the existing cascade limits. Both enter through concealed doors or the horizon.

Duelist Jacket now guards three damaging hits after reload/sever; only the first staggers. Rail Rifle remains at 42 damage. Checkpoints flag repeated target waits above the sequence, with reach and pause counts in Wave details. The chosen order stays intact. See [new comparisons](QA/v072-balance-review.md) and [verification](QA/v072-verification.md).

Checkpoints show last-wave damage, healing, blocks and collision kills. Open Wave details for per-item performance, reload and target waits, and recorded synergy payoffs. Results stay fixed while you equip or fuse. [Gun/sever comparison results](QA/v069-balance-review.md) include proposed balance tests; the current balance change is the approved third Duelist ward charge.

Weapon augments now add effects without slowing or weakening the original weapon. Counterguard's stored burst deals full damage to one target and 25% to up to four others; Amplifier is unchanged. Five new Siphon items add Lifesteal, drain marks, low-health sustain, healing-triggered haste and the Harvest ultimate. The character sheet shows Lifesteal; all new items have distinct art, summaries and rarity-scaled effects. [Balance results](QA/v067-balance-review.md).

Counterguard now lasts 2.5 seconds at Common, retaining its 25-point shield and 80% bank conversion. Duelist Jacket now wards three damaging hits for six seconds at Common; only the first staggers, refresh replaces charges, and fully blocked hits preserve them. [Isolated and combined balance tests](QA/v066-balance-review.md).

Ranged spawn odds are halved. Offscreen enemies cancel attacks and walk back to a visible flank; ranged shots also need a clear world line. Actions prioritize reachable repeat damage-dealers from the last six seconds, then nearest. A rear death boundary sits three metres beyond the camera floor edge and at least eight metres behind the walker, retaining knockback kill credit. The first-ultimate checkpoint remains after wave three. [Earlier 1,000-game enemy/progression results](QA/v064-balance-review.md).

Weapons and abilities now drive distinct physical reactions: blade-plane dismemberment, hammer collapse, electrical spasms, directional gun impacts and force/pull reactions. Counterguard flashes a shield and Redline vents energy from the hands. See `QA/v065-verification.md` for coverage and native captures.

Pick-three cards and both action sequences now include a short effect description. Empty equipment mounts have no placeholder artwork. The combat HUD is 20% smaller with translucent panels. Runs start without an ultimate; after wave three, choose one of three ultimates in place of the normal mixed reward, followed by the existing augment choice. See [0.6.3 verification](QA/v063-verification.md).

All 61 item definitions have distinct artwork, shared across rewards, inventory, equipment and combat. Checkpoints explain automatic same-item, same-rarity fusion, show matching-copy counts and separate other-rarity copies. The Icon Key explains categories, rarities and synergy symbols. Augments remain host-scaled and do not fuse. See [art sources and prompts](https://github.com/Chillysoop/walkinghard/tree/main/Art) and [0.6.2 verification](QA/v062-verification.md).

Counterguard now grants 25 barrier at Common (35 previously), with the existing rarity multipliers, duration and damage-conversion rules. The first balance test changes this capacity alone. Optional Fast simulations reuse combat and drafting rules with approximate movement and body collisions; Full remains the default for physics verification. See [the simulation guide](SIMULATIONS.md) and [0.6.1 verification](QA/v061-verification.md).

A Unity autobattler: one fighter walks forward through a physical crowd, seen from behind and above. Build a five-action loop, discover weapons, and turn bodies into chain reactions.

## Play

Download the Windows ZIP from [Releases](https://github.com/Chillysoop/walkinghard/releases/latest), extract the whole archive, and run WalkingHard.exe. For an updating desktop shortcut, run Launcher/Install.cmd. **Walking Hard (Main)** checks main, verifies the published ZIP checksum, and launches it. Its files and log live in %USERPROFILE%/Games/WalkingHard.

If main has no matching release, the launcher can build it in an isolated cache using Git and Unity 6000.3.27f1. Offline or failed updates retain the last working build. It never resets your development checkout.

## Playable systems

- Five movable sequence positions: two permanent weapon attacks plus three drafted abilities. The sequence always runs automatically.
- Matching adjacent one-handed attacks fire together with independent targets. Two-handed weapons fill both mounts and lock their basic cards into one movable group.
- Ten weapons: two fist styles, one- and two-handed swords and hammers, pistol, rifle and scattergun. Each has an authored signature. Both equipped weapons stay in hand during casts.
- Eleven abilities, fourteen armor pieces, five rigs and five ultimates. Conductive marks, barriers, severing, gathering and collisions support overlapping build directions.
- Sixteen augment modules change particular weapons and abilities. Each owned host has one socket; drag modules between compatible hosts or back to storage. Modules follow items through movement and fusion and have no separate rarity. Weapon modules retain native attacks/timing and receive full paired effects. Before/after previews explain the single-socket choice; ability conversions retain explicit tradeoffs.
- Temporary neutral tile bars explain RELOADING, READYING and WAITING FOR PAIR with remaining combat seconds. They disappear when the delay completes.
- No starting ultimate. Checkpoint three offers three distinct ultimates with normal rarity rolls; early offers and rerolls exclude them. Equipped ultimates have 12–18 second countdowns, automatic queuing or manual activation, and preserved sequence order.
- Eight waves with formation previews and the Pursuer final stand. Thirteen item choices include opening abilities, the third ability and a weapon after wave one, mixed rewards, the checkpoint-three ultimate choice and extra equipment drafts. Two separate augment drafts arrive after waves three and five.
- Drop a reward directly into a compatible slot to equip and advance; double-click auto-equips. Edit after pick keeps the character sheet open for rearranging until Let's fight.
- RPG character sheet with live Walker preview, surrounding gear mounts, an icon storage grid, full hover cards and animated before/after fusion feedback.
- Helmets, armor and boots grant rarity-scaled stats plus effects such as repair motes, reload pulses, barriers, mark setup and next-action haste.
- Drag-and-drop character sheet, unlimited storage, two weapon mounts, one rig, and a separate ultimate mount. Click-to-select/destination is also supported.
- Common, Uncommon, Rare, Epic and Legendary drops. Three identical items of the same rarity fuse to the next rarity, capped at Legendary.
- Rarity odds: 55% / 27% / 12% / 5% / 1%. Power: 1× / 1.2× / 1.5× / 1.9× / 2.5× Common.
- Distinct category labels, shapes and neutral icons; rarity badges, fusion previews, synergy inspection, and item-owned kill counters that follow weapons and abilities between slots.
- Combat speed: 0.5x, 1x, 2x or 4x. The angular combat HUD and checkpoint sheet share colors and icons; combat tiles retain descriptions and live stats.
- Bodies persist for the run. Walkers push resting corpses aside without slowing down; launched bodies still cause collision damage.
- Physical crowd collisions, ragdolls, hit-location body/limb cuts, and bounded multi-hop collision cascades. Hidden side-door/horizon creation remains enforced.
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

Run Build Windows.ps1, then `Validate Prototype.ps1 -HeroHealth 5000` for mechanics coverage. The optional QA-only health override lets fixtures reach later systems after removing the starting ultimate; omit it to exercise normal player durability. Fifteen standalone scenarios cover normal drafts, distinct weapon families, paired targets, manual ultimates, exhausted rerolls, rarity/fusion contracts, scoring, attribution, hidden entries and physical outcomes, plus temporary reload/readying bars and paired reload waits, persistent held weapons through casts, mounted-hand effects after reordering, persistent bodies, walking shoves, half/quadruple speed, armor passives and one-step reward progression. Named weapon scenarios use explicit weapon fixtures to exercise every family independently of draft luck. Run Launcher/Test Launcher.ps1 for updater safety checks.

Run `Validate Variety.ps1` for hybrid builds, all sixteen modules, and paired/separated/two-handed comparisons. Each QA process also executes production-effect assertions. Use `-Builds progression -HeroHealth 5000` for a four-wave check of the checkpoint-three ultimate unlock. Use `-Builds full -FullRun -HeroHealth 5000` for the complete eight-wave reward schedule and boss.

For bulk balancing, see [SIMULATIONS.md](SIMULATIONS.md). `Run Simulations.ps1` plays thousands of seeded games in headless Unity workers using real combat and physics. It exports CSV/JSON results, accepts numerical balance files without rebuilding, and supports matched-seed comparisons. No computer-use automation is required.

| Source | Responsibility |
| --- | --- |
| AbilityDeck, LoadoutCatalog, ItemRarity | Inventory, equipment, offers, grouping, fusion, content and rarity |
| VectorEncounter, EncounterCombat, WeaponCombat | Waves, ordered scheduler, ultimates, weapon attacks, attribution and cascades |
| FighterPhysics, FighterRig, EntryLayout | Physical actors, cuts, procedural weapons and concealed entries |
| PrototypeHUD, CheckpointHUD, ItemPresentation, HUDInteractions | Combat, loadout drag/drop, category/rarity presentation and icons |
| ComboScore | Kill strings, multiplier and banking |
| LoadoutChecks, EncounterQA | Contract assertions and standalone encounter reports |

See [the 0.6.0 verification report](QA/v060-verification.md) for checks, balance observations, native evidence and limitations. Earlier UI checks remain in the repository history.

C# scripts are under Assets/Vector/Scripts. See [DESIGN.md](DESIGN.md) for the current design contract and [CONTRIBUTING.md](CONTRIBUTING.md) for collaboration/release instructions. Historical Vector paths remain for asset stability. Art, balance and long-run progression remain prototype work; this release uses authored cuts rather than arbitrary mesh slicing.


0.6.8 narrows Siphon Blade healing to its own damage and marked targets, tunes Reclaimer to 3/4/5/6/7% global Lifesteal (1.5x below half HP), and gives Redline six accelerated turns. Harvest is unchanged. See the [balance results](QA/v068-balance-review.md) and [verification report](QA/v068-verification.md).

0.6.9 adds last-wave checkpoint feedback and a per-item Wave details panel for damage, healing, blocks, control, delays and triggered synergies. The focused gun/sever study recommends further tests without applying balance changes. See [results](QA/v069-balance-review.md) and [verification](QA/v069-verification.md).

The proposed next presentation pass is in [Art and VFX plan](ART_VFX_PLAN.md): a $0 CC0 asset shortlist, a small review scene first, then character/weapon integration, impact families, body cuts, the Pursuer and performance checks. Assets have not been imported yet.
