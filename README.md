# Walking Hard 0.9.0 - Constellation expansion

The Dead Letter Office now holds **36 permanent item unlocks**, up from six. Six new branches add **30 playable identities**: six weapons, six abilities, fourteen passive equipment pieces and four ultimates. Impact, Storm, Sever, Ballistics, Siphon and Guard each open with a weapon and split into complementary choices. The original six patterns remain in Foundations with their existing prices and ownership.

The Office shows connected prerequisites, affordable stars, short effect descriptions and Common-to-Legendary previews in the existing tablet design. Select a star to inspect it; the explicit unlock button spends Sparks. Each new branch costs 34 Sparks, for 228 across all 36 stars. Purchases expand future loot pools; they do not equip items or change an existing journey's saved reward pool. Existing currency, seals, unlocks and saves are retained.

New combinations include **Pile Knuckles → Counterweight Boots → Freight Train**, **Storm Lash → Arc Carbine → Switchboard Helm**, and **Leech Grips → Reclaim Pulse → Reservoir Coat**. Weapons retain family rarity milestones, one socket per owned host, simultaneous matching one-handed attacks and locked two-handed grouping. Expanded ability drafts retain an ordinary attack option; mixed rewards prefer a concrete interaction with equipped items. [Design](DESIGN.md#constellation-expansion-090) · [Verification](QA/constellation-expansion/verification.md).

Combat has distinct weapon-impact sounds, prioritized boss warnings and ultimate charge/release cues. Actual combo payoffs underline their source in the sequence; ultimates name their charging, release and recovery phases. Boss/champion danger boundaries remain visible through bodies and effects. Reusable effect pools and sound limits keep large chains bounded. Combat rules and persistent bodies are preserved. [Verification](QA/combat-feel/verification.md).

Each district now offers its own event pair. **Cinderworks** has risky pressure taps and independent worker rescues; the **Bell Foundry** has a two-lot auction and a press that exchanges spare gear; the **Last Dispatch** has paid-in-integrity work orders and a sealed cache with weapon/ultimate prizes. Odds, costs, safe exits and possible items are shown before you commit. Event cards preview the final fusion tier and effect, including any second weapon mount consumed by fusion. Exchanged equipment keeps its own combat history, and two new lore records persist in the archive. Existing open events retain their original terms. [Verification and economy](QA/district-events/verification.md).

Elite encounters now have a district identity: the **Freight Champion** in Cinderworks, **Shield Captain** in the Foundry, and **Dispatch Interceptor** in Dispatch. Break the Captain’s gold-linked escort guard or punish its bash recovery. The Interceptor marks a fixed charge lane, then opens after the charge; launch or impact pressure can interrupt it. Distinct armor, countdowns, protection labels and cyan exposed cores show what is happening. Fork signs identify the champion and **+6 salvage**; boss-bound roads identify their escorts instead. [Verification and limits](QA/district-champions/verification.md).

The Bell Foreman's command now needs more focused damage to silence. Clearing his linked escorts and building stagger/impact disruption remain effective counters. Ordinary Foundry and Dispatch reinforcements arrive slightly farther apart, giving existing groups more time to resolve while keeping each district's formation rhythm.

The results screen separates **actual combat and travel time, equipment decisions, and events/services**. Pauses and time away are excluded from active play; road decisions overlap combat. Timing continues from checkpoints, with older saves clearly marked as partial. The latest finished journey's timing and encounter pressure are also saved locally. Automated combat times still do not establish a 30-minute human playthrough. [Verification and measured limits](QA/journey-pacing/verification.md).

The three districts now have different encounter mixes, arrival rhythms and scenery: freight packs among Cinderworks furnaces, coordinated groups beneath the Bell Foundry galleries, and ranged cross traffic through the Last Dispatch's blue registry stacks. Dispatch arrivals can use nearby covered rear entrances. Physical road choices and walls are retained.

Each boss has its own counterplay. Break the **Shift Foreman's** pressure to interrupt its double press. Clear the **Bell Foreman's** gold-linked escorts, or disrupt its command with damage and stagger, to silence the toll. The **Pursuer** commits to a sweep, then opens its chest for a damage and control window; body impacts extend that recovery. Countdown labels, attack shapes and changing equipment show the phases. [Verification and limits](QA/district-bosses/verification.md).

At checkpoints **2, 9 and 16**, the third reward card is **WEAPON DISCOVERY**: one unlocked weapon that has appeared less recently in this run's reward choices. Individual fist styles, one-handed weapons and two-handed weapons each have their own history. The other two cards keep their normal rules; reward counts, rarity odds, fusion and combat values are unchanged. History persists through saves and rerolls. [Verification and measured tradeoffs](QA/weapon-discovery/verification.md).

New forks also keep enemy entrance rooms clear. A reproduced champion encounter stall came from a junction wall sealing an entrance; fork placement now preserves occupied rooms, and arrivals only use passages with full-height clearance. The six-second road choice is unchanged.

The results screen now explains the final hit, leading incoming threats, integrity changes and damage dealt in the fatal fight. It includes the unfinished encounter, keeps existing per-item totals, and carries the record through checkpoints. Older saves retain their progress and clearly label partial history.

Automated drafts now compare complete projected loadouts, both weapon mounts, fusion, stored partners and setup/payoff links. Seven build preferences can be tested across the full journey with the new `Validate Progression.ps1` runner. These changes affect testing; they do not choose equipment for players or add a suggestion panel. [Measurements, limitations and verification](QA/progression/verification.md).

Press **I** or click **LIVE STATS** on the right during combat. Toggle **Kills, Damage, Assists and Heal** independently. Rows follow the actual sequence, with the ultimate below; separate paired weapons keep separate credit, while shared fists and two-handed weapons appear once. Totals survive equipment swaps, fusion and checkpoints. Damage includes body impacts, assists count confirmed setup payoffs, and healing belongs to its source item; other gear/stored-item contributions and environmental kills are identified separately. Sidebar preferences persist between launches.

From fight three, **Linebreakers have 2× ordinary health**, **Enforcers/Freight Brutes 3×**, and **Bellkeepers/repair crews 1.5×**. Fragile raiders and ranged enemies retain their health. The first two fights retain their previous durability while you acquire a weapon and third ability. Damaged durable enemies show health with a recent-hit trail; shield guards, repair protection and exposed states have separate cues.

**Elite Shift** brings one **5.5× health district champion**, a repair escort and plenty of fragile bodies. Light stagger builds brace pressure; launching breaks the brace. The Freight Champion also opens after eight visible seconds, while the Captain and Interceptor expose themselves after their attacks. Open plates reveal a cyan core for a three-second **+25% damage** opportunity. The fight requires its defeat and pays **+6 salvage** once. [Balance measurements and verification](QA/durability/verification.md).

The loadout screen is now a **Dead Letter Office field tablet**: teal equipment bays, textured graphite storage, filled item cells and a blue sequence strip. Live labels and controls sit over generated, resizable panel materials. Taller screens show more storage rows; 720p and ultrawide keep the full build visible. Hover comparisons and fusion callouts remain in place, with no permanent suggestion panel. [Art and generation prompt](Art/README.md) · [UI verification](QA/tablet-loadout/verification.md).

**Play** is the single game mode. It opens the **Dead Letter Office**, then a three-district journey through **21 encounters**, **nine physical road choices**, and **three bosses**. The target is about **30 minutes at 1x including build and event decisions**; faster speed settings shorten combat. Sparks, constellation unlocks and recovered records persist between runs. [Rules](DESIGN.md) · [Verification](QA/long-journey-verification.md).

Click the **road itself** or its sign to select it. Cyan arrows, edge lights and the **FOLLOWING** sign show exactly where the Walker will go. A/D and arrow keys also work. You can change your choice until the six-second countdown locks it. Pause freezes the deadline; the approach caps fast play at 1x.

Linebreakers charge a marked lane; Cinderlobbers target a fixed blast circle; Bellkeepers rally nearby attackers. Silhouettes, colored footprints, tethers and labeled countdowns identify their jobs. Stagger or launch interrupts all three. They join shield bearers, volatile carriers, repair crews, freight brutes and reclaimers. The Shift Foreman, Bell Foreman and Pursuer end the three districts.

Each district offers repair/copy alternatives at two junctions and an event-versus-elite choice at the other. District gates restore 25% maximum integrity and provide a salvage bench. Item rarity odds, fusion, weapon pairing and ordered combat are unchanged. The run contains **20 ordinary item picks and five augment picks**, including the two opening picks; purchases and event prizes are additional.

Start a new journey or resume from the Office. Quit during combat to return to the last reward checkpoint. Earlier short-run saves are preserved under `RetiredRuns`; permanent Office progress and lore records are kept. **How to Play** is an optional tutorial and returns to the Office. Practice/legacy fixtures remain developer command-line tools, with no separate mode buttons.

**Livewire** lets any weapon discharge Conductive. Follow-through now carries brands or suppression into a weapon contact; Focused reshapes Push/Tether; Afterfield adds stationary storm, cut and drain areas. Surveyor, Grounding, Recovery Treads and Salvager connect to those effects. Paired weapons keep full effects; two-handed/shared hosts keep one socket. Visible field footprints, prepared-action labels, native artwork and host-specific previews explain the payoff and cost. See [verification and comparisons](QA/v080-verification.md).

**Recoil Harness** now saves its bonus when the base gun hit already kills the target. The next surviving target can receive it through piercing, a ricochet, the other pistol, or a later attack. The charge still fires once and credits the armor. All item stats are unchanged. See the [matched before/after comparisons](QA/v077-balance-review.md) and [verification](QA/v077-verification.md).

Reward previews identify an equipped combo partner and explain the payoff in one line. The matching item gets a white effect badge; short combat cues confirm actual synergies. **Suppressive Volley** interrupts distant groups without pushing them away. **Recoil Harness** turns a reload into one block, then a stronger gun hit. Both have dedicated card art, rarity scaling and full hover rules. The [initial controlled comparisons](QA/v076-balance-review.md) document the evidence behind the Recoil change.


Ordinary checkpoints now keep the **walker and equipment slots, storage, five-action sequence and ultimate visible together**. The permanent suggestion panel is removed. Hover a reward to light compatible destinations; select it and hover a slot for a compact before/after comparison. Click the glowing slot or drag it there; the preview names what replaces each affected slot and what goes to storage. Two-handed comparisons include both existing weapons. The wider storage grid shows twelve named cells with category filters and full rarity labels. The Store area is an explicit alternative to equipping. Rarity upgrades get a final-tier preview and a merge animation on the taken reward card, including chained fusions. Full effects, socket changes and synergies remain available by scrolling the hover details. See [inline UI verification](QA/inline-loadout/verification.md).

Two-handed groups, same-rarity fusion, independent ultimates and the existing reward schedule remain intact. Edit after pick still controls whether equipping advances immediately. An augment can now auto-attach to its sole compatible item even when that item is stored; multiple compatible hosts still require an explicit choice. See [verification](QA/v074-verification.md).

The main menu offers **Play** and **How to Play**. The tutorial teaches ability selection, weapon equip, same-rarity fusion, augment sockets and sequence order using the actual drag/drop and double-click controls, then gives you a short practice fight. Starting a real run clears practice gear and restores your speed and checkpoint preferences.

Every checkpoint now highlights valid destinations while hovering or dragging rewards and stored items. Ability picks outline the sequence before you hover. Category icons and labels are neutral; rarity colors belong to rarity borders and labels, and cyan marks interaction. Cards and sequence tiles use short effect summaries, with full rules on hover. An augment with no compatible host can be stored directly; multiple hosts prompt you to choose a socket. See [verification](QA/v073-verification.md).

The finale now stops the Walker and turns him around. The camera slowly swings behind him to reveal a giant and its approaching army. Hold your ground as enemies pour in from the distance, flow around the giant, and reach your automatic sequence. The boss is staged just before the scene; no follower is simulated during earlier waves. See [verification](QA/v071-verification.md).

At three junctions in each district, the road ends at a **physical Y-junction with two separate walled roads**. You keep walking and fighting. Use **A/D**, **left/right arrows**, or click either road or its sign before the six-second combat countdown ends. **Shields / More gear** leads to shield bearers and two equipment options; **Volatiles / More abilities** leads to explosive carriers and two ability options. The highlighted road is automatic if you do nothing. You can change your choice until it locks. The Walker and camera turn down that road; it has its own floor, walls and enemy entrances and does not rejoin the other branch. The encounter bias applies to the next wave only. The approach temporarily caps fast play at 1x, then restores your speed; pause freezes the countdown. The third reward option stays a wildcard, and reward counts, rarity odds and checkpoint-three ultimate progression are unchanged. [Physical-fork verification](QA/physical-forks-verification.md).

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

Use the single **Walking Hard** desktop shortcut for update checks and play. Install it with `Launcher/Install.cmd`. The player lives at `%USERPROFILE%/Games/WalkingHard/Current/WalkingHard.exe`; the shortcut runs the updater before opening it. Updates verify the release checksum, replace Current with rollback on installation failure, and remove temporary runtime copies. Offline or failed updates keep the working game. A newer tested development build is preserved until main includes its changes, so an older release cannot replace it. Launcher files and logs live in `%USERPROFILE%/Games/WalkingHard`.

If main has no matching release, the launcher can build it in an isolated cache using Git and Unity 6000.3.27f1. Offline or failed updates retain the last working build. It never resets your development checkout.

## Playable systems

- Five movable sequence positions: two permanent weapon attacks plus three drafted abilities. The sequence always runs automatically.
- Matching adjacent one-handed attacks fire together with independent targets. Two-handed weapons fill both mounts and lock their basic cards into one movable group.
- Ten weapons: two fist styles, one- and two-handed swords and hammers, pistol, rifle and scattergun. Each has an authored signature. Both equipped weapons stay in hand during casts.
- Twelve abilities, fifteen armor pieces, five rigs and five ultimates. Conductive marks, barriers, severing, gathering and collisions support overlapping build directions.
- Seventeen augment modules change particular weapons and abilities. Each owned host has one socket; drag modules between compatible hosts or back to storage. Modules follow items through movement and fusion and have no separate rarity. Weapon modules retain native attacks/timing and receive full paired effects. Before/after previews explain the single-socket choice; ability conversions retain explicit tradeoffs.
- Temporary neutral tile bars explain RELOADING, READYING and WAITING FOR PAIR with remaining combat seconds. They disappear when the delay completes.
- No starting ultimate. Checkpoint three offers three distinct ultimates with normal rarity rolls; early offers and rerolls exclude them. Equipped ultimates have 12–18 second countdowns, automatic queuing or manual activation, and preserved sequence order.
- Eight waves with formation previews and the Pursuer final stand. Thirteen item choices include opening abilities, the third ability and a weapon after wave one, mixed rewards, the checkpoint-three ultimate choice and extra equipment drafts. Two separate augment drafts arrive after waves three and five.
- Select a reward and click a glowing slot, or drag it directly to the destination. Reward double-clicks select without silently equipping. Keep editing / Continue after pick retains your preferred checkpoint pace.
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
| Enter | Play / perform the shown checkpoint action / continue after editing |
| 1 / 2 / 3 | Select draft offer |
| Q / W / E / D / C | Assign draft ability to an empty sequence position |
| Drag, or click then click destination | Reorder, equip, or store an item |
| Click reward | Select; hover a glowing destination to compare, then click it to equip |
| Double-click stored item | Quick equip (unrelated abilities are stored if the loop is full) |
| Speed buttons | 0.5x / 1x / 2x / 4x combat simulation |
| Tab | Spend a banked reroll |
| U | Toggle ultimate Auto/Manual |
| F | Queue a ready ultimate |
| I | Toggle live combat stats; click metric buttons to choose what is shown |
| Space / Escape | Pause or resume |
| R | New run |
| T | How to Play from the title screen |
| M | Mute |
| F8 | Save screenshot |

There is no toggle for the normal sequence. An ultimate never interrupts an action or a simultaneous pair. Combat, physics, combo expiry and ultimate countdowns stop at checkpoints. Pause includes reduced motion. Physics can differ slightly across machines even with the same draft seed.

## Develop and verify

Use **Unity 6000.3.27f1** with Windows Build Support. Open Assets/Vector/Scenes/CombatStudy.unity. The project uses URP and procedural geometry, animation and audio; no paid assets are required.

Run Build Windows.ps1, then `Validate Prototype.ps1 -HeroHealth 5000` for mechanics coverage. The optional QA-only health override lets fixtures reach later systems after removing the starting ultimate; omit it to exercise normal player durability. Fifteen standalone scenarios cover normal drafts, distinct weapon families, paired targets, manual ultimates, exhausted rerolls, rarity/fusion contracts, scoring, attribution, hidden entries and physical outcomes, plus temporary reload/readying bars and paired reload waits, persistent held weapons through casts, mounted-hand effects after reordering, persistent bodies, walking shoves, half/quadruple speed, armor passives and one-step reward progression. Named weapon scenarios use explicit weapon fixtures to exercise every family independently of draft luck. Run Launcher/Test Launcher.ps1 for updater safety checks.

Run `Validate Variety.ps1` for hybrid builds, all seventeen modules, and paired/separated/two-handed comparisons. Each QA process also executes production-effect assertions. Use `-Builds progression -HeroHealth 5000` for a four-wave check of the checkpoint-three ultimate unlock. Use `-Builds full -FullRun -HeroHealth 5000` for the complete eight-wave reward schedule and boss.

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
