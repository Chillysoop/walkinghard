# Walking Hard — Design direction

Design revision 2. Confirmed choices from the October 28, 2026 review. The 0.3 executable implements the drafted sequence and physical encounter; weapon equipment, stored inventory, fusion, severing, and multi-hop cascades are next-slice work.

## Confirmed loadout rules

One relentless fighter walks forward, seen from behind and above. Before a run, make two pick-one-of-three ability choices and place them into an ordered sequence. Draft the third ability at checkpoint one. Weapons shape the actions; gear, stored abilities, and three-copy upgrades let the build evolve. Enemies enter through side doors or the horizon. Sliced bodies, knockbacks, and cascading collisions turn a crowded fight into a physical chain reaction.

- **How many weapons can a build carry?** Two equipped weapons — there should be one and two handed weapons
- **Should changing weapons cost time?** A short, visible switch time
- **How much can you store between fights?** No limit in the prototype
- **When should three copies combine?** Automatically on the third copy
- **How should higher ranks work?** Same-rank copies; stop at rank III
- **When do weapons enter the run?** Start unarmed; find weapons at checkpoints
- **What should later checkpoint offers contain?** Mixed abilities, equipment, and copies

Drag abilities and equipment between inventory and compatible slots. Drag between active slots to reorder; drop into inventory to store. Keep a click/keyboard alternative. Use consistent labeled icons for stagger, pull, launch, collision, cut, chain, and reload. Distinguish what an item creates from what it benefits from. Show nearby sequence connections without promising a proc when range or prerequisites fail.

## The new foundation

### The unstoppable walk stays

The elevated camera follows from behind. One fighter advances while the crowd tries to stop him.

Free movement and physical space remain central. Enemies circle, seek openings, and push against each other; no combat grid. Preserve sight of the immediate crowd and the approaching formations.

Tradeoff: Large weapon effects and flying bodies must not hide the fighter or the next threat.

### You design the sequence

Replace the fixed jab-cross-finisher combo with player-selected actions that repeat in slot order.

The loop is occupied slot 1 → slot 2 → slot 3 → repeat. Auto Combat is a prominent main-screen toggle. With it off, a manual advance executes the next action in that same order.

Tradeoff: The interface must show what is next and why an action is waiting.

### Weapons are build-defining

The weapon loadout changes what your sequence can do. Bare fists remain a complete endgame path.

Unarmed offers speed and posture control; swords offer cuts and reach; hammers offer momentum and armor break; guns offer distance and piercing. A weapon is not simply a larger damage number.

Tradeoff: Every weapon family needs strengths, limitations, and compatible techniques worth pursuing.

## Abilities & sequence

### Two opening drafts, then the third slot

Start with two separate pick-one-of-three ability offers. Place each pick into a chosen slot.

The first checkpoint always grants the third ability from another pick-three offer. In the proposed full game, later ability rewards can be equipped or stored. Empty slots are skipped; no hidden default combo fills them.

Tradeoff: Opening offers need viable damage options without guaranteeing a prewritten combo.

### Order is a real tactical decision

Gather → launch → discharge should fight differently from discharge → gather → launch.

The active sequence has three slots for the first full slice. Actions declare their target, reach, execution time, resource cost, and effect. Checkpoints allow free rearrangement, with a before/after sequence preview.

Tradeoff: Do not add simultaneous cooldown priorities that silently override the visible slot order.

### Waiting must be legible

Show exactly why the next action cannot execute, and what will happen next.

Proposal: wait briefly for a target in range; weapon-incompatible actions are blocked at the checkpoint before departure. Empty slots skip. A reload is a visible weapon action rather than an invisible pause. Exact wait and reload rules need playtesting.

Tradeoff: Infinite waiting creates deadlocks; automatic skipping can undermine the chosen order.

### Shared events create emergent builds

Abilities communicate through stagger, cut, launch, collision, conductive, reload, and kill events.

A hammer launch can feed a lightning ability; a sword cut can produce a lighter projectile; a gun can pin a target before a force pulse. Bridges work across compatible weapon families instead of requiring a fixed set bonus.

Tradeoff: Secondary events carry their original action identity and cannot recursively grant unlimited triggers.

## Weapons & character

### Two carried weapons; one or two hands

Carry two weapons, including one-handed and two-handed weapons. Fists remain available.

An ability uses its compatible carried weapon. Changing the active weapon incurs a short visible switch time. A two-handed weapon occupies both hands while active; the other carried weapon is holstered. Dual wielding is not implied by two carried mounts. Armor, rigs, and implant counts remain prototype proposals.

Tradeoff: Too many slots dilute decisions. Start with a small equipment set and clear compatibility checks.

### Fists: speed, posture, precision

Unarmed builds win through rapid execution, reliable stagger, throws, and finishers you choose to equip.

Techniques include a fast jab, posture-breaking palm, elbow rush, and grapple launch. Gauntlets and wraps specialize fists without making them an inferior starter weapon. A finisher is an ability, not an automatic third hit.

Tradeoff: Short reach and limited cutting require different answers to protected or distant enemies.

### Swords: cuts become geometry

Wide arcs, thrusts, and execution cuts can slice defeated enemies into physical pieces.

A lethal sever can produce two pieces with different mass and momentum. A follow-up field can throw the pieces through the pack. Nonlethal cuts apply wounds or posture damage; armor can require exposure before a sever.

Tradeoff: Severing must preserve a single kill reward and a shared effect budget across fragments.

### Hammers: the crowd is a collision problem

Slow, heavy hits launch bodies, break armor, and produce the strongest momentum transfer.

A hammer sweep can turn a frontliner into a projectile. Slams can stagger a cluster; a charged uppercut can throw a heavy body into a backline. Impact direction is readable before the hit lands.

Tradeoff: Long windups and recovery leave dangerous openings; momentum should not erase every boss mechanic.

### Guns: reach with a rhythm

Pistols, shotguns, and rifles solve different distances and formations.

Pistols provide quick targeted shots, shotguns combine spread and knockback, rifles reward aligned targets and penetration. Ammo and a visible reload create sequencing opportunities. A firearm technique can precede a melee follow-up without changing the automatic control model.

Tradeoff: Ammo, range, and aim selection must remain readable from the chase camera. Avoid constant manual targeting.

## Checkpoint & inventory

### A checkpoint is a workshop

Pause combat and physics. Present the pick-three reward beside a complete character sheet.

The sheet shows weapons, equipment slots, the ordered ability bar, stored abilities and gear, ranks and copy counts, health, and the next wave forecast. A clear Continue button resumes only after the player finishes editing.

Tradeoff: The checkpoint must remain quick to read, with advanced statistics tucked behind inspection.

### Drag, equip, store, and rearrange

Drag abilities from inventory into sequence slots, between slots, or back into inventory.

Drag gear to a compatible equipment mount. Moving between occupied sequence slots exchanges their positions; installing a stored item into an occupied slot returns the displaced item to storage. Keep a click-to-select destination fallback. Storage is unlimited for the prototype. Show weapon requirements before continuing.

Tradeoff: Inventory needs clear limits or filtering; proposed starting capacity is 12 stored items, subject to testing.

### Three offers, more than three purposes

After the guaranteed opening ability drafts, later reward pools can offer abilities, weapons, gear, duplicates, and useful stats.

Show item identity, rank, owned copies, compatible slots, and relevant interactions directly on each card. Weight offers toward existing tags while leaving room to pivot. Never disguise a duplicate as a different item.

Tradeoff: Excessive curation makes every run feel scripted. Keep the exact weighting open for testing.

### Bank rerolls and preview threats

Unused rerolls accumulate across checkpoints. Show the next formation before the player spends one.

Each checkpoint adds a reroll in the initial balance proposal. Rerolling replaces offers while preserving owned equipment, stored items, and existing sequence. The first checkpoint keeps its third-ability guarantee.

Tradeoff: Avoid reroll economies that trivialize every draft or guarantee a perfect build.

## Three-copy upgrades

### Automatic three-copy upgrades

On receiving a third matching copy, automatically combine three items of the same identity and rank.

Three rank-I copies become one rank-II; three rank-II become rank-III. Rank III is the cap and requires nine base copies. Preserve an equipped position and show the consumed copies and result immediately. Start runs unarmed; checkpoint one supplies its guaranteed third ability plus a separate weapon offer.

Tradeoff: Duplicate curves and rank behavior need playtesting; the matching rule, automation, and rank cap are confirmed.

### Upgrades change a property you can see

A rank increase should improve an identity, not only add a generic percentage.

Examples: a sword gains a wider sever arc, a hammer retains more launch momentum, a gun gains one penetration, or a pull gains a stronger cluster. Keep simple damage increases when they produce useful kill thresholds.

Tradeoff: New behavior at every rank increases complexity and can make tooltips unreadable.

### Automatic fusion, visible result

Show what combined and where the upgraded item ended up, without requiring a confirmation click.

Prefer retaining an equipped copy. If multiple consumed copies occupied slots, retain the earliest occupied slot and return the other positions to empty. A short fusion receipt names the old rank, three consumed copies, and new rank. Body fragments never count as item copies.

Tradeoff: Automatic upgrades must not obscure why an extra equipped duplicate disappeared.

## Bodies & chain reactions

### Bodies are physical participants

Enemies collide, react to forces, and become jointed ragdolls on defeat.

Floor, walls, living capsules, and ragdoll limbs participate in physical contact. Damage and launch rules remain explicit game systems layered over that contact. Heavy enemies resist displacement but carry more momentum once launched.

Tradeoff: Physics needs solver limits, stable joints, and a budget for persistent bodies.

### Slicing preserves physical consequence

A body cut in half becomes two simulated pieces that can tumble, collide, and be pushed.

Use authored cut points and prepared ragdoll fragments for the first sword slice. Consider arbitrary runtime mesh cutting later. Both pieces retain one source enemy ID, so rewards, collision chains, and kill triggers do not duplicate.

Tradeoff: Performance and readability come first; fragments need sensible lifetimes and lower-cost collision shapes.

### Cascades spend momentum

A launched body can hit another enemy, transfer force, and start a short collision cascade.

Proposed rule: damage derives from relative impact speed and effective mass, with caps. Secondary launches lose energy. All descendants share a per-action hit budget, have a minimum speed threshold, and cannot farm the same target repeatedly.

Tradeoff: Tune a small hop limit and hit budget before lifting caps. The current prototype uses two direct contacts per launch; multi-hop cascades are future work.

### Entrances are part of the scene

Enemies arrive through doors on either side or walk in from the horizon. They never materialize in view.

Side passages hide creation behind physical cover, then enemies walk through the opening. Horizon entries begin outside the camera frustum. Validate the full character bounds and doorway occlusion before creating an actor.

Tradeoff: Door congestion and long approach distances need pacing adjustments without breaking the spatial illusion.

## Runs & unlocks

### A run is a sequence of pivots

Each run combines an opening draft, weapon discoveries, stored options, and duplicate upgrades.

The earlier 12-wave / 15–20 minute run is still a pacing hypothesis. Boss forecasts create reasons to change weapons or slot order. Carry remaining enemies across wave boundaries and preserve health pressure.

Tradeoff: Long checkpoint tinkering can overwhelm a short autobattler. Measure actual decision time.

### Telegraph the next unlock

Permanent progression reveals upcoming weapons, equipment, and fighting styles.

Show the next unlock and a concrete requirement before the run begins and after it ends. Distinguish run inventory from permanent unlocks. Whether permanent progression also adds baseline power remains an open decision; the earlier restriction was not accepted.

Tradeoff: Avoid diluting the reward pool until a new weapon has enough compatible abilities and bridges.

### Explain why the sequence worked or failed

Report damage by ability, useful control, collision chains, and time spent waiting.

Show activations by slot, weapon contributions, peak crowd, enemies carried forward, and the longest cascade. Let players replay a seed to compare orders. Present observed results without claiming that one run proves balance.

Tradeoff: Keep the default report short; expose the detailed character sheet only when requested.

## Playable & next steps

### Playable foundation: three-slot combat

The Unity 0.3 slice is being built around two opening picks, a third ability at checkpoint one, and ordered automatic execution.

Its scope includes six draftable abilities, a main-screen Auto Combat toggle, dynamic enemy capsules, jointed death ragdolls, and concealed side-door / horizon entries. Check the repository release notes for what has passed verification.

Tradeoff: The workshop sandbox is an explanation tool; it is not the Unity combat simulation.

### Next slice: a true checkpoint character sheet

Add weapon and gear equipment, active/stored abilities, free sequence rearrangement, and three-copy fusion.

Build the checkpoint interaction before expanding the item catalog. Verify that swaps preserve ranks, stored abilities survive later waves, incompatible loadouts cannot leave silently, and fusion consumes exactly three matching copies.

Tradeoff: These inventory and weapon systems are proposals for the next playable slice, not features claimed for 0.3.

### Then make one weapon family spectacular

Add a sword with authored body cuts and a hammer with momentum cascades, while preserving viable fists.

Start with a tiny comparison set, one gun, and a few shared-tech abilities. Test whole and severed ragdolls, walls, dense formations, and heavy targets. Expand content only after the same scene makes the weapon differences obvious.

Tradeoff: Do not promise arbitrary mesh slicing, unlimited bodies, or unrestricted recursive physics chains in the first pass.

## Proposed crossover examples

### Human Pinball

Posture break → targeted launch → body impacts spread force

An equipped launch ability supplies the throw; sequence placement controls what sets it up.

Tradeoff: An isolated boss offers less collision fuel. Keep direct damage viable.

### Shrapnel Ballet

Gather → lethal sever → field throws both fragments through the crowd

Fragments inherit the original action ID and share its collision budget.

Tradeoff: Two pieces must not create two kills or duplicate rewards.

### Thunder Freight

Conductive mark → hammer launch → impacts carry a discharge

Collision-triggered lightning uses the existing marks; it cannot refill itself indefinitely.

Tradeoff: The long hammer windup leaves a gap in defense.

### Ballistic Funnel

Pull into a line → penetrating shot → pulse through survivors

The field changes geometry so the weapon can hit more than one target.

Tradeoff: Reload timing can undo the setup if the sequence is poorly ordered.

### Close Quarters

Pistol stagger → unarmed launch → shotgun clears the opening

The loadout supports a sidearm and fists, with visible weapon transitions.

Tradeoff: Swapping and reload rules must not become hidden downtime.

### Cut & Carry

Sword sever → hammer drives a fragment → collisions knock down the next line

Weapon changes preserve shared impact and fragment events.

Tradeoff: Heavy weapon recovery and fragment mass prevent endless free launches.
