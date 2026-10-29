# Walking Hard — Current design contract

Revision 5, October 28, 2026. This records the latest approved instructions and supersedes the previous three-slot sequence, manual combat toggle, two-carried-weapon interpretation, and rank-III cap.

## Combat loop

One fighter walks forward, viewed from behind and above. Enemies enter through concealed side doors or the horizon. Combat runs automatically. The prototype remains three waves with frozen checkpoints.

The sequence contains **two persistent basic attacks plus three drafted abilities**. All five actions can be reordered. Basic 1 uses Weapon 1; Basic 2 uses Weapon 2. Empty weapon mounts use the starter fists. Empty ability positions are skipped. The next occupied action waits for a reachable target instead of silently skipping it.

Start unarmed with two separate pick-one-of-three ability drafts. Each pick goes into an empty sequence position. The first checkpoint grants the third ability, then a separate pick-three weapon offer. Later checkpoints offer mixed abilities, weapons, rigs, ultimates, and duplicate bundles. Drop a reward onto a compatible slot to claim, equip and advance immediately. Double-click auto-equips; a new ability goes into an empty position, or an existing matching action is upgraded. When the loop is full, an unrelated ability is stored. Weapons go directly to a mount (two-handed weapons fill both); displaced gear is stored. Enable Edit after pick to stay in the character sheet until Let's fight (or the next required draft pick).

Matching weapon types in adjacent basic positions execute simultaneously and prefer independent targets. Adjacency includes the repeat boundary (position 5 followed by 1); an empty position between them does not count. The pair completes before the next action. Different weapon types execute separately, with the existing preparation delay. Equipped weapons remain in their respective hands during all abilities and ultimates. Preparation changes the attack stance; it never holsters or replaces the models. Reordering a basic changes its sequence position, not its hand. Two-handed weapons use one model and both basic positions. This visual update preserves action durations, preparation timing, damage and ultimate scheduling.

A **two-handed weapon occupies both weapon mounts**. It stores the previously equipped weapons, locks the two basic cards adjacently, and produces one weapon attack across the pair. A shared bounding box and lock identify the group. Dragging either basic moves the group. Two two-handed weapons cannot be equipped together. Replacing a two-handed weapon with a one-handed weapon stores it and frees the other mount.

## Ultimates

Ultimates occupy a separate equipment slot and never replace one of the five actions. Their countdown advances only during combat. When ready, an automatic ultimate queues after the current action or simultaneous pair and its preparation finish. It preserves the next sequence position. It waits for a target if the crowd is out of reach; it never accumulates multiple queued casts.

Only the ultimate has Auto/Manual controls. In Manual, F queues a ready ultimate; the normal sequence continues automatically. U changes ultimate mode. The timer resets when the ultimate finishes. Changing the equipped ultimate at a checkpoint starts its full countdown.

The first catalog has Singularity Burst (12 seconds, launch and collision) and Tempest Array (16 seconds, lightning chain benefiting from stagger). Singularity Burst is supplied at run start. Rarity increases ultimate damage; countdowns remain above ten seconds.

## Equipment, inventory, and fusion

The checkpoint character sheet uses a live 3D Walker preview, surrounding equipment mounts, a stats panel, icon-grid storage and the five-action sequence below. It contains two weapon mounts, a helmet slot, an armor slot, boots, one utility rig, a separate ultimate mount, and unlimited prototype storage. Hover equipped actions, equipment, rewards or storage for full stats and effects. The reference image supplies layout and visual direction; its level, XP, three-reward pick and four-action layout are not new game rules. Drag or click an item and then a compatible destination. Reordering active actions exchanges their positions. Equipping a stored item returns displaced equipment to storage. Both basics remain in the sequence. Two-handed groups cannot be split by dropping an ability into the group; move the group first.

Every acquired weapon, ability, rig and ultimate has a rarity. Basic cards inherit their weapon's rarity. Starter fists and the initial ultimate are Common. Each offered card rolls its rarity independently of its item identity using these initial balance values:

| Rarity | Roll chance | Power versus Common |
| --- | ---: | ---: |
| Common | 55% | 1.00× |
| Uncommon | 27% | 1.20× |
| Rare | 12% | 1.50× |
| Epic | 5% | 1.90× |
| Legendary | 1% | 2.50× |

Three copies of the **same item and same rarity** automatically fuse into one item of the next rarity. Legendary is the cap. Equipment and stored copies both count. Retain the earliest equipped copy, its position, and combined kill credit; consumed positions become empty. The default fist item is preferred when matching starter-fist copies fuse. Higher-rarity fist drops can also be equipped in a weapon mount.

Offer cards show category, rarity, actual power, copies provided, and progress toward fusion. A duplicate bundle remains one reward choice. A bundle's rarity follows the same odds; it is not guaranteed to match the rarity already owned. Rarity does not make an incompatible effect fit a build.

Fusion displays a short real-time animation of three copies merging, then the old and new rarity/stat lines and behavior change. A Last fusion button retains the latest comparison for inspection. The animation does not consume combat time or require another confirmation.

Unused rerolls bank across checkpoints. Rerolling changes offers, never owned items. A claimed reward cannot be rerolled or claimed again.

## Weapons and upgrades

- **Kinetic Fists:** fast short-range stagger. Uncommon adds a second hit; Rare and above add a small stagger shockwave. Higher rarities also increase damage. Always a viable unarmed option.
- **Edge Sword (1H):** sweeping lethal cuts separate a body at its authored waist joint. Higher rarity widens reach and arc; Rare and above launch severed bodies into the crowd.
- **Impact Hammer (2H):** one slow crushing launch. Uncommon adds area stagger; Rare and above launch up to three targets with a shared cascade budget.
- **Pulse Pistol (1H):** six-round magazine and visible reload. Uncommon adds one ricochet; Rare and above add two ricochets and stronger stagger.
- **Rail Rifle (2H):** four rounds, piercing aligned targets, visible reload. Uncommon pierces three enemies; Rare and above pierce four and launch the final target.

Weapon upgrades combine visible behavior with rarity-scaled stats. The catalog is intentionally small. One-handed weapon pairs represent simultaneous attacks, not additional invisible sequence turns.

## Armor and passive equipment

Armor starts unequipped and joins the existing mixed reward pool. Armor has the same five rarities, direct equip, storage and three-copy fusion as other items. A piece only grants effects in its matching slot; utility rigs remain separate. These are initial prototype balance values at Common, scaled by the existing rarity power factor:

- **Tactician Visor / helmet:** +8% direct damage. Expose adds 6 damage against already-staggered targets on direct hits; collision descendants do not gain this bonus.
- **Bulwark Plate / armor:** +40 maximum integrity. Brace reduces incoming enemy attack damage by 12%.
- **Recovery Treads / boots:** +8% action speed. Recovery restores 0.4 integrity per combat second while the Walker advances. It does not tick while paused or at a checkpoint.

The character stats summarize damage, resistance, action speed, regeneration and integrity. Hover explains the exact rarity-scaled values. Equipment rarity/fusion keeps its mount and identity. Armor bonuses do not change the ultimate's base recharge period.

## Physical outcomes and attribution

Living enemies have physical capsules; dead enemies become jointed ragdolls. Sword cuts use prepared anatomy at the waist, not arbitrary mesh cutting. Both halves retain one enemy identity and one kill reward.

Launched bodies can transfer momentum through two additional hops. Descendants share a six-hit budget, a per-target exclusion set, declining energy, and a minimum impact speed. Damage uses relative speed with caps. Bodies and severed fragments persist for the entire run, including behind the camera. Settled ragdolls ignore walking capsules; one-way walking shoves push bones aside without feeding reaction forces back into walkers. Bodies launched by attacks temporarily collide with enemies to preserve damaging cascades, then return to walking-safe collisions. No age or count cleanup runs; a new run clears the old encounter. Force Push and ultimates also move fallen bodies.

Each kill belongs once to its originating weapon, ability or ultimate, including delayed collision kills. Basic cards display their current weapon's count; swapping a weapon does not inherit the former slot count. Moving or storing an item preserves its kills. The HUD says **Kills:** instead of activations. Upgrades preserve credited kills. Telemetry still records activations internally to validate ordering.

## Combo scoring

Each normal kill adds 100 base points; a heavy kill adds 200. Rapid kills maintain a three-combat-second combo. Additional kills from the same action add a bulk bonus. Collision depth increases the multiplier, and every three additional streak kills contributes another step. The multiplier is capped at 10×.

The HUD shows base points × multiplier, pending score, reason, remaining window and banked score. A combo banks after three seconds without a kill, at a checkpoint, or when the run ends. Damage does not erase points. Pausing does not consume the window. These values are prototype tuning, not permanent progression decisions.

## Visual language

Category and rarity are separate channels. Gold weapon icons and upgrade chevrons identify weapons/copies; teal identifies abilities; violet identifies ultimates; blue identifies utility rigs. Rarity uses its own labeled border: gray, green, blue, purple, orange. Text and distinct shapes accompany color.

Use chamfered navy panels, steel-blue edges, distinctive action pictograms, cyan active markers, amber next markers and segmented bars. Place wave status at top right and the ultimate immediately below it. Keep Pause, New Run and 0.5x / 1x / 2x / 4x speed controls at top left. The five-action strip sits at the bottom edge with no auto-sequence footer. Each combat tile includes a short attack description, damage (including equipped rig), range, action time, rarity and item kills. Ultimate also shows its recharge period.

Speed changes the fixed-step simulation budget, so movement, physics, action timing, spawns, score windows and ultimate recharge scale together. Checkpoints and Pause freeze all of those at every speed. Input and double-click timing use real time.

Show real damage at the offered rarity, clear 1H/2H occupancy, and what a fusion changes. Contextual effect icons and inspection text explain stagger, gathering, launch, collision, cuts, chains and piercing. A linked pair uses a gold connector; two-handed basics also get one shared border and lock. Do not describe a conditional synergy as a guaranteed proc.

## Remaining work

Models, animations and authored cuts remain procedural prototype art. Arbitrary runtime slicing, a large weapon catalog, long-run pacing, bosses, permanent unlocks and balance across the full rarity curve are future work. The old workshop site's saved choices remain historical review evidence; this file records the latest game rules.

## Recommended next milestones (proposals, not approved rule changes)

The checkpoint/inventory milestone and the first weapon spectacle pass now have playable implementations. The next question is whether the build economy stays interesting across a complete run.

1. **A complete short run:** trial 6–8 waves targeting roughly 5–8 minutes of combat, with checkpoint tinkering measured separately. Add contrasting enemy roles and a final boss with supporting enemies so launch/collision builds have useful targets. Preview the next encounter at checkpoints. The earlier 12-wave / 15–20-minute idea remains a pacing hypothesis.
2. **Build payoff and reward pacing:** use a small comparison set—Human Pinball, sword/fragment interactions, and pull/piercing-gun interactions. Give armor, useful duplicates, and loadout pivots enough opportunities to be evaluated. Review proposed changes to reward quantity, category guarantees, weighting, or rarity odds before implementing them; preserve the current rules until then. Rarity alone should not determine the best choice.
3. **Feedback for friend playtests:** a compact run recap showing item kills, collision contributions, biggest combo, and a repeatable seed. Use it to compare sequences and locate stalls. Profile persistent-body cost at the longer duration before expanding further.
4. **Then permanent unlocks:** telegraph upcoming weapon, gear and style unlocks and their requirements, as requested in the workshop review. Whether permanent progression grants baseline power is still unresolved. Broader content and production art follow a stable run structure.

These recommendations reflect the current five-action loop, independent ultimate, two-handed groups, armor, five rarities and automatic fusion. They do not change any of those rules.
