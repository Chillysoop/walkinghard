# Walking Hard — Current design contract

Revision 3, October 28, 2026. This records the latest approved instructions and supersedes the previous three-slot sequence, manual combat toggle, two-carried-weapon interpretation, and rank-III cap.

## Combat loop

One fighter walks forward, viewed from behind and above. Enemies enter through concealed side doors or the horizon. Combat runs automatically. The prototype remains three waves with frozen checkpoints.

The sequence contains **two persistent basic attacks plus three drafted abilities**. All five actions can be reordered. Basic 1 uses Weapon 1; Basic 2 uses Weapon 2. Empty weapon mounts use the starter fists. Empty ability positions are skipped. The next occupied action waits for a reachable target instead of silently skipping it.

Start unarmed with two separate pick-one-of-three ability drafts. Each pick goes into an empty sequence position. The first checkpoint grants the third ability, then a separate pick-three weapon offer. Later checkpoints offer mixed abilities, weapons, rigs, ultimates, and duplicate bundles. Rewards are claimed once, then the character sheet remains open until Continue.

Matching weapon types in adjacent basic positions execute simultaneously and prefer independent targets. Adjacency includes the repeat boundary (position 5 followed by 1); an empty position between them does not count. The pair completes before the next action. Different weapon types execute separately, with a visible switch delay.

A **two-handed weapon occupies both weapon mounts**. It stores the previously equipped weapons, locks the two basic cards adjacently, and produces one weapon attack across the pair. A shared bounding box and lock identify the group. Dragging either basic moves the group. Two two-handed weapons cannot be equipped together. Replacing a two-handed weapon with a one-handed weapon stores it and frees the other mount.

## Ultimates

Ultimates occupy a separate equipment slot and never replace one of the five actions. Their countdown advances only during combat. When ready, an automatic ultimate queues after the current action or simultaneous pair and its preparation finish. It preserves the next sequence position. It waits for a target if the crowd is out of reach; it never accumulates multiple queued casts.

Only the ultimate has Auto/Manual controls. In Manual, F queues a ready ultimate; the normal sequence continues automatically. U changes ultimate mode. The timer resets when the ultimate finishes. Changing the equipped ultimate at a checkpoint starts its full countdown.

The first catalog has Singularity Burst (12 seconds, launch and collision) and Tempest Array (16 seconds, lightning chain benefiting from stagger). Singularity Burst is supplied at run start. Rarity increases ultimate damage; countdowns remain above ten seconds.

## Equipment, inventory, and fusion

The character sheet contains the five-action sequence, two weapon mounts, one utility rig, a separate ultimate mount, and unlimited prototype storage. Drag or click an item and then a compatible destination. Reordering active actions exchanges their positions. Equipping a stored item returns displaced equipment to storage. Both basics remain in the sequence. Two-handed groups cannot be split by dropping an ability into the group; move the group first.

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

Unused rerolls bank across checkpoints. Rerolling changes offers, never owned items. A claimed reward cannot be rerolled or claimed again.

## Weapons and upgrades

- **Kinetic Fists:** fast short-range stagger. Uncommon adds a second hit; Rare and above add a small stagger shockwave. Higher rarities also increase damage. Always a viable unarmed option.
- **Edge Sword (1H):** sweeping lethal cuts separate a body at its authored waist joint. Higher rarity widens reach and arc; Rare and above launch severed bodies into the crowd.
- **Impact Hammer (2H):** one slow crushing launch. Uncommon adds area stagger; Rare and above launch up to three targets with a shared cascade budget.
- **Pulse Pistol (1H):** six-round magazine and visible reload. Uncommon adds one ricochet; Rare and above add two ricochets and stronger stagger.
- **Rail Rifle (2H):** four rounds, piercing aligned targets, visible reload. Uncommon pierces three enemies; Rare and above pierce four and launch the final target.

Weapon upgrades combine visible behavior with rarity-scaled stats. The catalog is intentionally small. One-handed weapon pairs represent simultaneous attacks, not additional invisible sequence turns.

## Physical outcomes and attribution

Living enemies have physical capsules; dead enemies become jointed ragdolls. Sword cuts use prepared anatomy at the waist, not arbitrary mesh cutting. Both halves retain one enemy identity and one kill reward.

Launched bodies can transfer momentum through two additional hops. Descendants share a six-hit budget, a per-target exclusion set, declining energy, and a minimum impact speed. Damage uses relative speed with caps. Body and fragment lifetimes are bounded. Force Push and ultimates also move fallen bodies.

Kill counters belong to the originating action and weapon, including delayed collision kills. The HUD says **Kills:** instead of activations. Upgrades preserve credited kills. Telemetry still records activations internally to validate ordering.

## Combo scoring

Each normal kill adds 100 base points; a heavy kill adds 200. Rapid kills maintain a three-combat-second combo. Additional kills from the same action add a bulk bonus. Collision depth increases the multiplier, and every three additional streak kills contributes another step. The multiplier is capped at 10×.

The HUD shows base points × multiplier, pending score, reason, remaining window and banked score. A combo banks after three seconds without a kill, at a checkpoint, or when the run ends. Damage does not erase points. Pausing does not consume the window. These values are prototype tuning, not permanent progression decisions.

## Visual language

Category and rarity are separate channels. Gold weapon icons and upgrade chevrons identify weapons/copies; teal identifies abilities; violet identifies ultimates; blue identifies utility rigs. Rarity uses its own labeled border: gray, green, blue, purple, orange. Text and distinct shapes accompany color.

Show real damage at the offered rarity, clear 1H/2H occupancy, and what a fusion changes. Contextual effect icons and inspection text explain stagger, gathering, launch, collision, cuts, chains and piercing. A linked pair uses a gold connector; two-handed basics also get one shared border and lock. Do not describe a conditional synergy as a guaranteed proc.

## Remaining work

Models, animations and authored cuts remain procedural prototype art. Arbitrary runtime slicing, a large weapon catalog, long-run pacing, bosses, permanent unlocks and balance across the full rarity curve are future work. The old workshop site's saved choices remain historical review evidence; this file records the latest game rules.