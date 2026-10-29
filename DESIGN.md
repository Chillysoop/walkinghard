# Walking Hard — Current design contract

Revision 6, October 28, 2026. This records the latest approved instructions and supersedes the previous three-slot sequence, manual combat toggle, two-carried-weapon interpretation, and rank-III cap.

## Combat loop

One fighter walks forward, viewed from behind and above. Enemies enter through concealed side doors or the horizon. Combat runs automatically. The prototype remains three waves with frozen checkpoints.

The sequence contains **two persistent basic attacks plus three drafted abilities**. All five actions can be reordered. Basic 1 uses Weapon 1; Basic 2 uses Weapon 2. Empty weapon mounts use the starter fists. Empty ability positions are skipped. The next occupied action waits for a reachable target instead of silently skipping it.

Start unarmed with two separate pick-one-of-three ability drafts. Each pick goes into an empty sequence position. The first checkpoint grants the third ability, then a separate pick-three weapon offer. Later checkpoints offer mixed abilities, weapons, rigs, ultimates, and duplicate bundles. Drop a reward onto a compatible slot to claim, equip and advance immediately. Double-click auto-equips; a new ability goes into an empty position, or an existing matching action is upgraded. When the loop is full, an unrelated ability is stored. Weapons go directly to a mount (two-handed weapons fill both); displaced gear is stored. Enable Edit after pick to stay in the character sheet until Let's fight (or the next required draft pick).

Matching weapon types in adjacent basic positions execute simultaneously and prefer independent targets. Adjacency includes the repeat boundary (position 5 followed by 1); an empty position between them does not count. The pair completes before the next action. Different weapon types execute separately, with the existing preparation delay. Equipped weapons remain in their respective hands during all abilities and ultimates. Preparation changes the attack stance; it never holsters or replaces the models. Reordering a basic changes its sequence position, not its hand. Two-handed weapons use one model and both basic positions. This visual update preserves action durations, preparation timing, damage and ultimate scheduling.

Swords and hammers may each have one-handed and two-handed variants. The current build contains the one-handed Edge Sword and two-handed Impact Hammer; their additional variants are part of the proposed content round below.

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

Timed preparation is visible on the affected combat tile only while it happens: an amber progress bar, a reason and remaining combat seconds. Reloads say RELOADING; the existing stance delay says READYING. A paired weapon with ammunition says WAITING FOR PAIR when its partner reloads. Bars disappear when the delay ends and normal attack progress resumes. Both tiles of a two-handed group show its shared delay. Future charge/channel/preparation effects must expose their real timing through the same presentation contract. An unbounded wait for a target must not show an invented countdown. Ultimate recharge retains its separate countdown. Reloading still delays the next attack turn, then fires in that same turn; this UI change does not alter timing.

Show real damage at the offered rarity, clear 1H/2H occupancy, and what a fusion changes. Contextual effect icons and inspection text explain stagger, gathering, launch, collision, cuts, chains and piercing. A linked pair uses a gold connector; two-handed basics also get one shared border and lock. Do not describe a conditional synergy as a guaranteed proc.

## Remaining work

Models, animations and authored cuts remain procedural prototype art. Arbitrary runtime slicing, a large weapon catalog, long-run pacing, bosses, permanent unlocks and balance across the full rarity curve are future work. The old workshop site's saved choices remain historical review evidence; this file records the latest game rules.

## Recommended next round: replayability and build variety

This is a proposal based on the v0.5.1 code and the v0.5.2 delay-display update. It replaces the earlier broad milestone recommendation with a concrete content target. The newly approved directions here are the delay display, both hand counts for swords/hammers, and a distinctive effect for every item with rarity improving that effect as well as stats. Proposed new items, effects, encounter length and rewards are not implemented or approved balance changes.

### Audit against the current build

The loop, character sheet, equipment storage, five movable actions, matching pairs, two-handed group, independent ultimate, five rarities and automatic fusion are implemented. Weapon persistence, ragdolls, authored cuts, bounded cascades and item-owned kills also exist.

The catalog has **19 item identities**, excluding the two basic cards and rarity copies: five weapons including fists, six abilities, three rigs, two ultimates and one item in each of the three armor slots. Armor currently offers acquisition and rarity choices, but no choice of item identity within a slot. Fists, swords and hammers each have one identity. Storm has Arc Bolt and Tempest but no conductive-mark system. There is no shield/barrier ability or gun/reload support equipment.

A normal three-wave run grants two opening ability choices, then a third ability and weapon at checkpoint one, then just **one mixed choice** at checkpoint two. A mixed offer is already structured: an owned-identity two-copy bundle, a random ability and a random equipment item. The bundle rolls rarity independently and need not match owned copies. Expanding the catalog without improving opportunities to receive it would make armor and fusion less accessible.

| Planned archetype | Present support | Main gap |
| --- | --- | --- |
| Iron Saint | Fists, Steel Jab, Breaker, Mass Driver, stagger bonuses | Fist specialization and a defensive/counter technique |
| Vector Savant | Gravity Well, Force Push, Mass Driver, collision physics | Alternative crowd shapes and equipment that rewards manipulating bodies |
| Storm Conductor | Arc Bolt, Tempest, bonuses against stagger | Conductive marks and physical-to-lightning crossover |
| Sever Artist | Sword cuts; Rare sword launches; corpse pushes | 2H sword, cutting technique and fragment-oriented equipment |
| Wrecking Saint | 2H hammer, launch/cascades, Fault Line | 1H hammer, conditional defense and alternate support effects |
| Deadeye Engine | Pistol, rifle, ricochets, piercing, magazines | Shotgun option, reload payoffs and reliable line setup |

Human Pinball has substantial support. Shrapnel Ballet and Ballistic Funnel have ingredients, but their specialized follow-ups/line setup are incomplete. Thunder Freight needs a conductive bridge. Cut & Carry cannot equip the current sword and hammer together because the hammer occupies both mounts; a 1H hammer resolves this while preserving that rule. Close Quarters should mean pistol + fists or scattergun + fists: its old three-weapon example exceeds two mounts. Existing fragments share one enemy identity; adding independent fragment projectiles must preserve one kill and a shared collision budget.

### Catalog target: 40 identities

| Slot/category | Current | Target | Purpose |
| --- | ---: | ---: | --- |
| Weapons | 5 | 9 | Two fist styles, 1H/2H swords, 1H/2H hammers, three gun patterns |
| Three ability positions | 6 | 10 | Setup, control, payoff and defense compete for the same three positions |
| Helmet | 1 | 4 | Targeting and offensive specialization |
| Armor | 1 | 4 | Different ways to survive |
| Boots | 1 | 4 | Different sources of tempo and sustain |
| Utility rig | 3 | 5 | Strong interaction modifiers |
| Ultimate | 2 | 4 | Physical burst, lightning, cutting and tempo |

This adds 21 identities. Hand count and weapon family are separate properties: matching adjacent one-handed swords pair with swords, hammers with hammers, and fist variants with fists. A two-handed item still occupies both mounts and produces one attack across the locked group. Archetypes are combinations, not class restrictions or mandatory sets.

### Item identity and rarity contract

Every item needs a signature gameplay effect. Stats may accompany it, but a different percentage or color does not constitute a different item. Common must already express its identity; higher rarities strengthen its effect through parameters such as reach, duration, retained energy, targets, charges, coverage or trigger frequency. Keep the same recognizable mechanic rather than adding unrelated powers at every tier. Base stat budgets should leave room for build fit to beat rarity: an off-build Legendary should not automatically win just through its generic stats.

The current stat-focused Overclock, Amplifier, Bulwark and Recovery items need effect passes as part of this round. Duplicate/upgrade cards inherit the item's identity; they are not extra catalog entries. The target is 40 identities across five rarities, not 200 unrelated mechanics.

Every item specification must name: trigger, visible effect, supported archetypes, a tradeoff, rarity-scaled effect parameters, and its recursion/stacking limit. Offer and fusion previews must show the actual effect change, for example **one arc → two arcs**, alongside the stat change.

### Weapons

Retain the current Kinetic Fists, Edge Sword (1H), Impact Hammer (2H), Pulse Pistol (1H) and Rail Rifle (2H), with their existing behavior milestones. Add:

- **Conductor Wraps (fists / one mount):** lower direct impact than Kinetic Fists, but basic hits apply Conductive. Supports Storm + Iron and mixed fist pairs. Kinetic Fists keep the better stagger/direct-impact role.
- **Execution Greatsword (2H sword):** slower and broader than Edge Sword, with strong cleave and lethal severs. It sacrifices the second weapon and independent paired targets for a substantial single swing.
- **Breach Hammer (1H hammer):** faster, lighter impact and reliable stagger; lower launch strength than Impact Hammer. Supports sword/hammer, hammer/pistol and paired light hammers.
- **Compact Scattergun (1H shotgun):** short cone, knockback, a small magazine and a visible reload. Supports fists/shotgun and sword/shotgun; the pistol retains reach and sustained shots, and the rifle retains piercing lines.

New identities must differ at Common. Higher rarity improves their signature effects as well as stats; it should not be necessary merely to make the advertised archetype function.

| Weapon identity | Rarity improves the signature |
| --- | --- |
| Kinetic Fists | Existing second-hit/shockwave milestones, then shockwave coverage and stagger potency |
| Conductor Wraps | Mark duration and discharge potency |
| Edge Sword | Sweep coverage, sever-launch momentum and reach |
| Execution Greatsword | Cleave coverage and follow-through force |
| Breach Hammer | Stagger coverage/duration and controlled knockback |
| Impact Hammer | Existing area/target milestones, then retained launch energy |
| Pulse Pistol | Existing ricochets, then bounce reach and damage retention |
| Rail Rifle | Existing piercing targets, then penetration retention and final-target launch |
| Compact Scattergun | Knockback coverage and pellet penetration/retention under a cap |

 Variant attacks must derive damage, reach, timing and passives from the item definition, rather than assigning identical behavior to every member of a weapon family.

### Abilities and ultimates

Retain the six current abilities. Add four universal tech/martial abilities that remain usable during the unarmed opening:

| New ability | Effect and sequencing decision | Supports |
| --- | --- | --- |
| Conductive Brand | Mark a nearby group. A later lightning hit consumes a mark for a bounded extra discharge. Brand before discharge has a different result from the reverse. | Storm; gun/tech hybrids |
| Vector Tether | Pull enemies toward a narrow forward corridor instead of Gravity Well's gathering point. Choose line setup versus a compact cluster. | Deadeye, Vector, melee follow-ups |
| Reaping Cut | A projected cutting arc with bonus damage against weakened enemies; lethal hits sever. Does not require a carried sword. | Sever; hammer/tech cutting hybrids |
| Counterguard | A short barrier banks a capped amount of absorbed damage for the next basic attack. The payoff is consumed once per attack turn, including a linked pair. | Iron, Wrecking, close-range gun builds |

Use Conductive and capped temporary barriers as the main new shared combat states. Recovery Treads additionally need a small repair-mote pickup; it is an automatic combat pickup, not a new inventory currency. Reuse stagger, target health, launch, sever, ammo and hit geometry for the rest. Defer a separate bleed/wound/status-stack system. Mark discharges cannot recursively grant themselves; derived effects retain their originating item/action for kill credit and obey explicit proc limits. Existing collision hop/hit caps remain.

Existing abilities also need effect scaling beyond their present damage scaling:

| Ability | Signature effect | Rarity-scaled effect |
| --- | --- | --- |
| Steel Jab | Quick single-target interrupt | Longer interrupt/stagger window; stronger payoff against an attacking enemy |
| Breaker | Wide posture-breaking strike | Cone coverage and stagger duration |
| Force Push | Repel living enemies and fallen bodies | Push radius and force |
| Mass Driver | Aim a body into a collision cascade | Launch impulse and retained energy, within existing hop/hit caps |
| Gravity Well | Gather enemies at a point | Gather radius and pull strength |
| Arc Bolt | Jump between targets; exploit stagger | Chain reach/targets and bounce damage retention, with caps |
| Conductive Brand | Set up later discharges | Mark coverage/duration and discharge strength |
| Vector Tether | Form a narrow target corridor | Capture reach/width and pull strength |
| Reaping Cut | Cut weakened enemies and sever on lethal hits | Arc coverage and weakened-target damage threshold/bonus; no automatic boss execution |
| Counterguard | Absorb pressure and bank a basic-attack payoff | Barrier capacity, storage duration and capped conversion strength |

Keep Singularity Burst and Tempest Array. Add **Razor Cyclone**, a radial projected cutting burst that severs on lethal hits, and **Redline**, which reloads equipped guns and accelerates the next three sequence turns; a linked pair counts as one turn. Redline also benefits melee tempo builds. Both retain separate Auto/Manual queuing and proposed recharge periods above ten seconds. They do not insert extra basic attacks or reorder the sequence. Rarity scales their relevant power, with exact values to be tested.

### Gear choices

Retain existing names where appropriate, but replace stat-only identities with signature effects. Small stat contributions accompany these effects. Exact damage and uptime values remain proposed tuning.

| Slot / item | Signature effect | Rarity improves |
| --- | --- | --- |
| Helmet: Tactician Visor | Striking an already-staggered enemy transfers a short stagger to one nearby enemy; retain its direct/stagger damage role. | Transfer reach and stagger duration |
| Helmet: Surveyor Visor | A multi-target attack repeats a weaker echo hit on its last surviving target, once per turn. | Echo strength and small splash coverage |
| Helmet: Salvager Visor | A launch or sever primes the next basic to pull a nearby fallen body toward its impact. | Retrieval radius and pull force |
| Helmet: Capacitor Crown | Hitting a staggered enemy applies Conductive, capped once per turn. | Mark duration and discharge strength |
| Armor: Bulwark Plate | Completing the sequence arms a guard; the next incoming hit is absorbed up to a cap and releases a stagger pulse. | Absorption and pulse radius/duration |
| Armor: Reactive Shell | Close crowd pressure forms a barrier; breaking or expiring it releases a repulsion pulse. Has a cooldown. | Barrier capacity and repulsion coverage |
| Armor: Grounding Mantle | Lightning or body impacts generate a capped barrier; enemies striking it become Conductive. | Barrier conversion and mark duration |
| Armor: Duelist Jacket | A completed reload or sever grants a brief ward that reduces the next melee hit and staggers its attacker. | Mitigation and retaliation duration |
| Boots: Recovery Treads | The first kill of a turn drops a repair mote; the advancing Walker automatically collects nearby motes. | Repair amount and pickup radius |
| Boots: Momentum Soles | Launching an enemy accelerates the next non-weapon ability; one stored boost, no stacking. | Acceleration and the boost's expiry window |
| Boots: Loader Greaves | Faster preparation/reloads; completing a reload releases a short stagger pulse to make room for the shot. | Reload reduction and pulse coverage/duration |
| Boots: Arcstep Boots | A multi-kill sends a pulse that marks nearby survivors Conductive. | Pulse radius and marked-target cap |
| Rig: Fault Line | The first collision in a launch chain creates a local stagger aftershock, retaining its collision-damage role. | Aftershock coverage and impact potency |
| Rig: Overclock | Successive attack turns within a loop ramp up tempo; the ramp resets when the loop completes. Ultimates do not gain recharge speed. | Ramp strength and cap |
| Rig: Amplifier | Abilities bank charge; the next basic spends it on an impact burst. Paired basics spend the bank once. | Stored-charge cap and burst coverage/strength |
| Rig: Relay Coil | Body impacts consume Conductive for a bounded lightning discharge, enabling Thunder Freight. | Arc targets, reach and damage retention |
| Rig: Salvage Magnet | A direct sever propels the resulting body/fragments into a follow-up collision, at most once per originating turn. | Launch impulse and retrieval reach, inside the shared cascade budget |

An ultimate also needs effect scaling: **Singularity Burst** improves launch strength/coverage, **Tempest Array** chain reach/target capacity, **Razor Cyclone** cutting coverage/impulse, and **Redline** the strength/duration of its accelerated turns and reload payoff. Recharge stays above ten seconds. Values must be capped and readable; additional targets are discrete milestones rather than fractional promises.

Example provisional Relay Coil curve: Common arcs to one extra target within 3m; Uncommon keeps one target but reaches 3.5m; Rare reaches two targets within 3.5m; Epic reaches two within 4m; Legendary reaches three within 4m. Ordinary stat scaling applies separately. This demonstrates five tiers of the same effect; these numbers are not approved balance.

Barriers have visible size/duration and cannot accumulate without a cap. Conditional speed bonuses have caps. A secondary proc cannot recursively count as a new direct attack, reload or sever for generating another proc. A simultaneous pair is one triggering turn for gear procs; individual weapons still attack and earn kills independently. The Overclock ramp follows sequence traversal, including wrapping pairs, without adding or skipping a turn. A pair of reloads releases one Loader pulse. Deferred/timed effects use the tile timing presentation introduced in v0.5.2, and equipped gear inspection shows relevant cooldowns.

### Coverage requirements

These are example alternatives, not locked recipes. Every archetype gets at least two plausible options per passive slot and ultimate, and each specialist piece should also support a hybrid.

| Archetype | Helmet alternatives | Armor alternatives | Boots alternatives | Rig alternatives | Ultimate alternatives |
| --- | --- | --- | --- | --- | --- |
| Iron Saint | Tactician / Capacitor | Bulwark / Reactive | Recovery / Momentum | Overclock / Fault Line | Singularity / Redline |
| Vector Savant | Salvager / Surveyor | Bulwark / Grounding | Momentum / Recovery | Fault Line / Salvage Magnet | Singularity / Razor Cyclone |
| Storm Conductor | Capacitor / Surveyor | Grounding / Reactive | Arcstep / Recovery | Relay Coil / Overclock | Tempest / Redline |
| Sever Artist | Salvager / Surveyor | Duelist / Bulwark | Arcstep / Recovery | Salvage Magnet / Amplifier | Razor Cyclone / Singularity |
| Wrecking Saint | Tactician / Salvager | Bulwark / Grounding | Momentum / Recovery | Fault Line / Relay Coil | Singularity / Tempest |
| Deadeye Engine | Surveyor / Capacitor | Duelist / Bulwark | Loader / Arcstep | Amplifier / Overclock | Redline / Tempest |

The playtest should demonstrate Human Pinball, Shrapnel Ballet, Thunder Freight, Ballistic Funnel, Cut & Carry and one legal two-weapon Close Quarters build. Examples must use actual equipment occupancy and sequence rules. No build should require three different weapons at once or two two-handed weapons. A column lists viable candidates, not a promise that every combination is equally strong; verify the proposed alternatives through playtests.

### Replayable run and reward proposal

Use **eight waves**, targeting roughly 5–8 minutes of combat plus checkpoint editing. Keep the two opening drafts and checkpoint one's third-ability-plus-weapon sequence. After waves 2, 4 and 6, add a separate equipment pick-one-of-three in addition to the normal mixed choice. This gives **13 reward choices** per full run: two opening, two at checkpoint one, six later mixed, and three extra equipment choices. It is a proposed payout change, not implemented.

Preserve the owned-identity two-copy offer. For the other mixed cards, propose one build-support choice and one discovery choice, allowed to cross ability/equipment categories. Equipment drafts should offer at least one item for an empty armor slot when one exists, without forcing the player to take it. Avoid three offers that all serve the same purpose. Keep banked rerolls and unlimited storage.

Retain the current rarity probabilities and independent rolls. Do not secretly force a duplicate bundle to the rarity of an owned item. Preserve the choice between an off-build high-rarity item and an on-build Common item. Evaluate fusion frequency before changing odds, matching rules or introducing conversion currencies.

Vary encounter composition from the existing doors/horizon: dense packs, spaced shooters and heavy frontliners. Show the actual next encounter composition at checkpoints; the current code has a general NextThreat string but the combat/character-sheet UI does not yet use it. Add one final boss with supporting enemies and finite control windows so physical builds have material to use, while gun builds have meaningful ranged targets. Avoid complete immunity that invalidates an archetype. An encounter can favor a build without requiring it.

Extend the existing score/kill/wave recap and Repeat Seed feature with per-item kill contribution, useful control, barrier absorption and time spent reloading/preparing/waiting for range. Do not replace the player's kill counters with activation counters. Indicate which synergies were enabled by equipment, with conditions explained; equipping an archetype should not silently grant a set bonus.

### Implementation order and acceptance

1. Make item definitions own their stats and passives. Armor values currently come from slot index and all weapons of a family share one attack definition, so catalog additions alone would be cosmetic. Maintain family-based pairing and definition-based fusion.
2. Add the 1H/2H sword/hammer variants, fist/shotgun options and gear alternatives; then the four abilities, two rigs and two ultimates. Introduce Conductive/barrier hooks, repair motes and the signature-effect passes on current items, with bounded triggering and correct attribution.
3. Integrate the proposed eight-wave/reward structure and encounter previews after approval of the payout changes. Keep a short three-wave test mode for regression checks.
4. Playtest every archetype with alternative gear, at Common and across higher rarities, without granting a mandatory full set. Replay a seed with different orders and verify a meaningful outcome difference. Show at least one ordinary-run fusion and a useful build pivot in a seed sweep, and measure how often they occur rather than assuming they are guaranteed. Profile persistent bodies over the longer run.

Success means a player can make a different worthwhile choice in every equipped category, understand its setup/payoff and cost, and encounter enough rewards to develop it. Exact damage/uptime values, reward support weighting, extra equipment payouts and eight-wave pacing need review before implementation. Permanent unlocks remain planned after this round; whether they add baseline power is still an open decision.
