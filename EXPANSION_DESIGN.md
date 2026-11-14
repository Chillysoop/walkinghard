# Walking Hard expansion proposal

November 13, 2026. Proposed direction for a full roguelite adventure, a deeper combat sandbox, integrated lore, and an art pipeline that can be produced within this project. Names, content counts and pacing targets are design proposals. The implemented combat contract remains in [DESIGN.md](DESIGN.md).

**A condemned martial artist walks through a city that keeps its dead at work. You rewrite his combat routines, turn the city's machinery against its armies, and decide what deserves to keep running.**

The expansion keeps automatic forward movement, the five-position sequence, paired one-handed attacks, locked two-handed groups, a separate ultimate, and physical bodies as useful combat objects. It adds meaningful places to go, enemies that test different builds, and consequences that survive a run. Its visual direction is faceted industrial armor and monumental machinery, built from modular 3D parts on the existing rig.

**Implementation status:** the first regional art milestone is available as Cinderworks Preview, with a comparison toggle, seven real combat setups and the current physics. [Verification and limitations](QA/cinderworks-verification.md) distinguish this delivery from the first act and broader campaign below.

**Cinderworks Expedition pass:** the furnace architecture now follows the actual branching roads, and three work-crew roles use the existing combat systems: temporary ally protection, telegraphed corpse throws, and physical body retrieval. Waves introduce them individually before mixing them. Distinct tools, countdowns, target links and a throw footprint expose their behavior; stagger and launch provide counterplay across existing builds. Stop props distinguish the salvage bench from the Overtime press. Existing checkpoints migrate to the new role data. [Verification](QA/cinderworks-crews-verification.md).

**Shift Foreman pass:** Expedition wave six now includes the bracing/venting regional boss, with a concealed approach, pressure shared by originating action, visible shutters and a slam warning. Natural venting and ordinary damage let builds without launch progress. Eight practice presets and five-seed matched crew comparisons cover existing weapon families and sustain/storm builds. The run still has eight waves and the Pursuer finale. The act graph, hub consequences and catalog expansion below remain future work. [Verification and limits](QA/shift-foreman-verification.md).

**Physical route decisions:** the current eight-wave run contains literal Y-junctions during waves 2 and 5. Each exit is a separate road with physical floors, walls and covered enemy entrances. The roads diverge and do not reconnect; the Walker and camera turn down the selected road. Short icon signs and a six-combat-second countdown lock the highlighted road while combat continues. The next junction extends the selected road. [Verification](QA/physical-forks-verification.md).

**Expedition stops slice:** a separate Expedition entry now adds checkpoint save/resume, one salvage currency, Iven's repair/copy service, and one push-your-luck event with three lore variants. First fork: Shields versus Overtime. Second fork: Salvage versus Volatiles. Events and services open at the following reward checkpoint. Recovered records persist without stat bonuses; quick runs retain both original combat-route choices. This remains the eight-wave format, not the complete act destination graph or hub. Exact implemented rules are in [DESIGN.md](DESIGN.md), with [verification](QA/expedition-stops-verification.md).

## The city and its history

The city is **Veyr**. Its central engine, the **Continuance**, was built during an event called the **Stilling**. Beyond the city, reliable motion began to fail: rivers hung in broken sheets, falling structures stopped halfway to the ground, and machines lost the ability to complete a cycle. The Continuance stabilizes a small territory by drawing on recorded human intention: the impulse to finish an action. It needs both machinery and people to maintain that boundary.

At first, dying volunteers offered unfinished work to the engine. Their bodies completed a final shift; their families received heat, food and protection. The founding promise was specific: every obligation would have an end. Brass plates on the oldest machinery still include a space for a completion date.

Veyr survived. Its emergency offices became permanent institutions. Work orders began renewing automatically. A person could finish every assigned task and still be recalled because the office that owned the assignment had not closed it. New generations inherited the infrastructure, its benefits, and the debts used to justify it. Modern plates omit the completion field.

The first major discovery is that compulsory service is a political choice. The second is harder: the Continuance really does protect living neighborhoods. Simply destroying it would expose people who never consented to the system. The long-term objective becomes finding a workable replacement, or deciding who may control and limit the existing engine.

The story's central question is **who gets to declare a duty finished?** Each faction has an answer, a material stake, and people harmed by its preferred solution.

## The Walker and the Pursuer

The Walker remains a human martial artist in armor. His body has been restored through the Continuance's medical machinery. Restoration preserves a recorded physical pattern and portions of memory; the city cannot prove that the returned consciousness is identical to the original. Characters disagree about that uncertainty without making the player read a philosophy lecture before fighting.

His sentence contains one protected instruction: **walk to the engine and return what you carry**. Five editable operations remain around it. Someone has broken the Registry's authority over those operations, allowing the player to author the sequence. Combat techniques begin as learned routines; rearranging them becomes an act of agency.

The command explains forward movement during encounters. It does not create a real-time punishment for pausing, choosing loot, or visiting the hub. Staging platforms provide legitimate breaks in the instruction. No new movement meter is needed.

The **Pursuer** is the first volunteer recorded by the Continuance, a man now contained within an enormous maintenance suit. His original task was to escort evacuees to safety. The city repeatedly expanded the definition of an evacuee to include escaped workers, unauthorized bodies, and stolen records. He still believes that bringing people home is protection.

He carries the original completion authority, buried beneath generations of amendments. That is why defeating him grants access to the engine's final decision. His size comes from decades of grafted equipment, not from replacing the existing humanoid boss with a new creature rig.

The existing finale remains powerful: after spending the run walking forward, the Walker stops and turns to face the thing following him. This becomes the first deliberate refusal of the sentence. Earlier regions foreshadow it with oversized maintenance tracks, crushed gates, and distant footfalls. His actual combat actor can still be created for the finale; the story does not require simulating a giant throughout every run.

## Why these mechanics belong in this world

| Existing mechanic | Narrative meaning | What the player encounters |
| --- | --- | --- |
| Five-position combat sequence | Five operations escaped the Registry's control. | The same five-slot interface, with a short opening explanation and occasional recovered routines. |
| Weapons and abilities | Civic tools and human techniques repurposed for survival. | Gravity equipment once moved freight; lightning maintained power; hammers opened sealed works; cutting tools dismantled damaged structures. Names remain mechanically descriptive. |
| Augments | Changes to how an operation executes. | A visible socket and an explicit changed behavior. The lore adds no extra socket or resource. |
| Three-copy fusion | Three matching records reconstruct a clearer pattern. | Existing identity, rarity and fusion rules remain. A clearer reconstruction strengthens the signature effect. |
| Bodies, severing and collisions | Residual instructions and stored force survive bodily failure. | Corpses and fragments retain their physical utility. New factions recognize and exploit that fact. |
| Ultimates | Restricted emergency procedures designed to save whole work crews. | Large, readable payoffs with recognizable industrial origins. |
| Repeated runs | Another illegal restoration and another expedition through a changing city. | Equipment resets; recovered knowledge, characters and unlocked options persist. The whole world does not rewind. |

An ordinary item record is a copy of a technique, not a conscious person trapped in every sword. Fusion therefore remains a satisfying build decision. Specific story records can contain testimony, names or a person's continuity; those are clearly distinguished from equipment.

## Regions, enemies and regional bosses

Build four visual districts. A normal expedition crosses two of the three outer districts, then enters the Heartgrid. This supports different combinations without requiring every run to visit every biome.

| District and faction | Belief and visual identity | Combat problems | Boss |
| --- | --- | --- | --- |
| **The Brass Registry** | A completed ledger is a safe city. Tall administrative vaults, numbered gates, black enamel, ivory shields and brass seals. | Shield escorts, standard bearers and firing ranks protect each other. Moving, staggering or piercing the right formation opens a fight. | **The Registrar Marshal** visibly links guards, changes formation, then exposes himself while issuing a new order. |
| **The Cinderworks** | Somebody must keep the heat on. Furnace halls, freight rails, orange enamel, soot and huge pressure vessels. Workers disagree over whether continued service is solidarity or coercion. | Heavies, carriers and braced operators turn launch timing and body collisions into opportunities. Bracing is temporary and shown by planted supports. | **The Shift Foreman** alternates bracing with an exposed pressure vent. Breaking the stance accelerates an opening; ordinary damage can still win. |
| **The Salvage Choir** | No person should disappear without a witness. Ceramic masks, bell frames, archive tags and rows of suspended recovery tools. Remembrance can become another form of captivity. | Retrieval crews pull fallen bodies into defensive positions; bell keepers coordinate short attacks. Existing bodies move rather than disappearing into an abstract corpse counter. | **The Bellkeeper** drags bodies into a moving shield ring, opens gaps to attack, and can be disrupted through damage or control. |
| **The Heartgrid** | The original rescue machinery continues its orders without understanding their cost. Pale stone, dark steel, restrained cyan power and immense rotating assemblies. | Relay-linked enemies, combined formations and earlier roles under greater pressure. Connections are visible and can be broken through several attack families. | **The Pursuer**, followed by the engine decision. His familiar approach, planted finale and humanoid physics remain the foundation. |

An optional fifth boss, **the Toll Warden**, controls a shortcut between districts. The player can pay, take the longer route, or fight for access. Its presence is advertised on the map; it is not an unexplained difficulty spike.

Region mechanics should reward several answers. A relay can be overloaded by a body impact, broken by direct attacks, or interrupted by stagger; lightning offers another efficient route. No district invalidates an entire weapon family through blanket immunity. Fixed-target automatic combat also means boss weak points must be reachable through normal targeting. Do not design encounters around a dodge button, precise manual aim, or movement controls the game does not have.

Ranks and offices can recur after defeat because a new official occupies them. Restored bosses can recur with changed records and dialogue. A freed unique character stays freed; later challenge runs use explicitly identified archived encounters rather than silently undoing the story.

## The roguelite run

Target a three-act expedition lasting approximately **25 to 35 minutes at normal speed**, including decisions. This is a pacing hypothesis to test. Start with roughly six meaningful nodes per act: three ordinary fights, an elite-or-event choice, a service opportunity, and a boss. Branches change the mixture rather than adding endless mandatory stops.

The map shows destination, enemy formation, reward emphasis and any special condition before commitment. Existing Shield Line and Volatile Breach encounters become ingredients in this structure; they already exist and should not be presented as new systems.

The core loop is: select a route, prepare the five-slot sequence, fight, make one focused reward decision, and advance. Keep the existing quick eight-wave format available while the longer mode develops. A player should be able to resume a saved expedition at a safe checkpoint.

Use **one spendable run currency**, salvage, for the shop and forge. Equipment remains pick-one-of-three. Service nodes provide repair, targeted offers, or a matching copy that can contribute to the existing three-copy fusion. Do not add separate currencies for factions, augments and crafting. Story records are discoveries, not another material to grind.

A larger catalog needs better access to useful pieces. Each ordinary draft should aim to offer a viable continuation, a viable pivot and a broadly useful alternative, without guaranteeing the best item. Regional offers bias toward recognizable local equipment while retaining cross-region combinations. Shops and event rewards provide targeted access. Exact odds and reward frequency need simulation once the first act exists; current rarity odds and fusion rules remain the baseline.

Optional contracts show their reward and encounter consequence before selection: for example, recover an archival seal by taking a guarded route in exchange for a chosen gear category. Limit the player to one active contract. Faction relationships are represented by completed story decisions and available services, avoiding three more reputation bars.

## Progression and story through repeated runs

The hub is the **Dead Letter Office**, a closed transit station that still receives orders addressed to people officially declared finished. It contains the restoration equipment, a small cast, a route board, and a growing archive. Most interactions can use short text, portraits rendered from game models, and visible changes to the room.

**Mara Venn**, a former Registry clerk, forged completion notices to release workers. Some recipients survived; others lost access to the systems keeping their bodies alive. She runs the Office and wants enforceable limits rather than an easy promise that nobody needs the engine.

**Iven Rusk**, a Cinderworks mechanic, develops local alternatives to central power. He can make one neighborhood independent; scaling that solution is the difficult part. His story opens new forge services and routes through infrastructure he has repaired.

**Sister Nacre**, a Choir archivist, preserves records the Registry would erase. Her conflict is learning that remembering somebody does not entitle her to keep restoring them. Her story opens archive encounters and alternate starting techniques.

Deaths advance conversations and recovered evidence. Blueprint unlocks, starting loadouts, regional access and optional challenge modifiers persist. Permanent damage and health grinding are not the progression foundation. Stronger play should come primarily from understanding formations, choosing a route and constructing a sequence.

The campaign has three layers of revelation:

1. **Permission to finish.** Learn that work orders can legally end. Defeating the first regional boss delivers a real completion seal and an immediate new route or service.
2. **The cost of continuity.** Discover that the city depends on the engine and that different districts bear unequal costs. Optional expeditions recover practical alternatives, not just exposition.
3. **Who holds authority.** Confront the Pursuer, recover the original compact, and choose an ending supported by the preparations made across runs.

Three ending directions are viable: place strict limits on a reformed Continuance; distribute its functions to independent districts; or organize an evacuation before shutting it down. Each needs visible prerequisites and an honest account of its tradeoff. Reformation risks later abuse, distribution creates uneven safety, and evacuation gives up the city. The ordinary first victory remains a satisfying ending; deeper alternatives add routes and consequences rather than declaring the first win a failure.

## Events with actual build consequences

Events use visible chance, changing stakes and an exit option. Their lore can become familiar, but a memorized dialogue answer must not guarantee the best payout. Generate the situation once per run, show what is certain and what is rolled, and preserve both the situation and RNG state through saves. Different health, salvage and build needs should change the decision. No concealed punishment for choosing the apparently reasonable sentence.

**Overtime — implemented first event.** The Unfinished Shift, Wages for the Dead and The Restoration Quota share a clear risk loop: start with a small free haul; bank it or open another locker. The initial odds, payout and damage vary. Each success grows the haul; further attempts worsen the odds and increase both payout and damage. Failure loses the unbanked haul and stated integrity. Up to four attempts, a separate result reveal, safe cash-out and a recovered record even on failure. It feeds Iven's later service without adding another currency or status system.

**The name inside the helmet — future candidate.** A damaged memory offers a rolled starting technique and a visible recovery chance. Archive the intact fragment or risk extracting another technique while endangering the temporary equipment reward. Clearly separate persistent story discovery from the current-run gamble. Avoid a fixed archive/install answer that becomes automatic once the unlock is owned.

**The broken lift — future candidate.** A freight platform offers a rolled load and repair cost. Pay for a guaranteed safe delivery, take the recovered salvage, or overload it for a larger advertised reward with an explicit chance of damage and a collision-heavy encounter. Generate cargo, prices and odds together; preview the possible encounter before commitment.

**The bell that kept ringing — future candidate.** A Choir machine repeats an evacuation notice in the Pursuer's original voice. Recover a guaranteed fragment, then decide whether to decode another broadcast at an increasing, visible chance of calling a recovery patrol. The possible support effect and patrol formation are shown. Further broadcasts vary across runs; the first story discovery does not require winning a roll.

## Deeper combat across the catalog

The current catalog has **64 identities including augments**: 10 weapons, 12 abilities, 20 gear items including rigs, five ultimates and 17 augments. A full expansion can aim for approximately **120**: 20 weapons, 24 abilities, 40 gear items, eight ultimates and 28 augments. This is a content budget, not a quota. Items enter the pool only when their behavior and tradeoff justify a distinct identity.

Continue to support Human Pinball, Thunder Freight, Shrapnel Ballet, Ballistic Funnel, Cut & Carry, Close Quarters and sustain variants as overlapping build ideas. Regions add encounter questions; loot supplies multiple answers. A faction is an origin for equipment, not a mandatory set bonus or class lock.

The following candidates distribute new behavior across weapons, abilities, gear and ultimates. Numbers require balancing; these rows define the visible action and its cost.

| Candidate | Behavior and existing partners | Tradeoff and visible cue |
| --- | --- | --- |
| **Arrestor Gauntlet**, 1H fist | A direct hit on an already staggered enemy arrests its remaining horizontal movement, setting up a close follow-up. Works after Breaker or Suppressive Volley. | Less direct damage than Kinetic Fists. It can spoil a desired launch; paired order must be deterministic. A contracting jaw-shaped ring shows the stop. |
| **Hookblade**, 1H sword | A surviving primary target is pulled a short distance toward the Walker after the cut. Pairs with another sword or prepares a subsequent hammer turn. | Narrower cleave and weaker damage than Edge Sword; it brings a threat closer. A hooked tip and short inward trail identify the movement. |
| **Pile Driver**, 2H hammer | Drives the primary target downward, sending a short forward stagger wave through grounded enemies. Helps close builds retain a group rather than throwing it beyond reach. | Both mounts, slow commitment, weaker airborne collision creation than Impact Hammer. A vertical piston strike and advancing floor wedges show its footprint. |
| **Rivet Pistol**, 1H gun | Direct hits briefly reduce a target's advance without knocking it back. Supports Tether and piercing combinations by preserving alignment. | Lower damage and no Pulse Pistol ricochets. Visible rivet impacts and a short ground tether indicate the restriction; it expires rather than immobilizing forever. |
| **Crosscurrent**, ability | Draws two reachable enemies toward the centerline before the next action. Builds on Tether alignment and enables cleaves. | Small target count and low direct damage. Two converging paths make the affected targets explicit. |
| **Groundbreak**, ability | Sends a narrow stagger wave forward; a fallen body struck by the wave produces one bounded secondary pulse. | Weaker in an empty fight and narrower than Breaker. Bodies stay physical and are not consumed; subsequent pulses cannot trigger more pulses. |
| **Survey Bell**, helmet | Completing a loop identifies one reachable support enemy; the next eligible action prefers it, then spends the preference. | Gives up another helmet's immediate payoff. Never waits for an unreachable marked enemy; a bell marker and sequence-tile highlight explain the target change. |
| **Transfer Harness**, armor | A damaging hit partially absorbed by a barrier stores one short stagger for the next direct basic hit. Counterguard can feed it. | Less raw protection than dedicated armor; one capped bank, no proc-on-proc rearming. A lit chest segment transfers to the striking hand. |
| **Freight Return**, ultimate | Sweeps nearby fallen bodies into a visible forward train, then launches one shared collision cascade. Extends cut and launch builds without requiring Conductive. | Needs local bodies and sacrifices an ultimate with reliable immediate value in a fresh encounter. Paths appear before launch; the hero and enemy warnings stay visible. |
| **Return Stroke**, augment | Eligible displacement effects bring the surviving primary target partway back after their initial movement. Reshapes selected hammer, push and cutting follow-through hosts. | Lower outgoing force; can undo useful spacing. One explicit host-specific preview, an outward arrow followed by a short inward arrow, and one return per origin. |

These effects reuse a small vocabulary: stagger, displacement, formation, barriers, direct hits and bodies. New status labels should be added only when existing states cannot describe the behavior. Survey Bell is the most consequential targeting change and should be tested after the other first-act additions; it must respect target legality and the combat-flow fixes.

Matching one-handed families still attack simultaneously. Separate equipped weapon hosts retain their own permitted augment activations; global gear banks spend once according to the current ownership rules. A two-handed weapon remains one attack, one locked group and one shared augment socket. Its strength comes from its authored reach, coverage and payoff, not a duplicated socket or an invisible second proc. Compare sustained paired and two-handed output across sparse, dense and boss encounters, including actual waiting and reload delays.

New derived effects inherit bounded origin budgets and cannot recursively create additional copies of themselves. Bodies and fragments retain enemy identity so retrieval never creates new kill, healing or loot credit. Encounter support crews move bodies; they do not repeatedly resurrect enemies for farmable rewards.

## Example expeditions

**Thunder Freight through the Cinderworks.** Conductive Brand marks a freight crew; the current Impact Hammer launches the front worker; Relay Coil turns a collision into lightning. A Crosscurrent offer trades some broad gathering for alignment. Choosing a carrier encounter promises useful body density but greater pressure while the hammer prepares. Later, the Foreman's visible bracing asks the player to put an interrupt before the launch rather than abandon the build.

**Cut & Carry through the Choir.** Edge Sword, Breach Hammer, Reaping Cut and Salvage Magnet already create useful fragments and collisions. Groundbreak adds a controlled way to exploit the floor after the first kills. The Choir tries to retrieve those same bodies; killing or interrupting the retriever changes where the next collision can happen. A repair stop competes with a forge copy needed for the sword's next fusion.

**Ballistic Funnel through the Registry.** Vector Tether lines up targets for Rail Rifle, while Suppressive Volley delays the escort's return fire. A Survey Bell later offers access to a support enemy at the cost of the current helmet's payoff. That swap is optional: piercing the formation or breaking its guard remain legitimate answers. Redline is still useful when the player wants accelerated follow-through and real ammunition replenishment.

## Art direction comparison

The strongest fit is **faceted industrial miniatures**: human figures in heavy enamel armor, ceramic masks, cast-metal tools, and architecture large enough to make a person feel small. Treat the scene like a carefully lit physical model while keeping the current perspective, camera behavior and readable UI. Avoid tilt-shift blur and tiny decorative detail that vanish at gameplay distance.

| Direction | Production burden here | Fit with current combat | Assessment |
| --- | --- | --- | --- |
| Faceted industrial miniatures | Lowest incremental burden: extend the existing modular meshes, shared materials and bone attachments. | Excellent for rigid armor, silhouettes, sever seams, weapon sockets and physical fragments. | Recommended. Its quality depends on deliberate shapes, motion and lighting rather than polygon count alone. |
| Graphic cel or ink rendering | Moderate: simpler surfaces, but outlines and shading need crowded-scene tuning. | Preserves 3D physics; dense corpse piles can turn into unreadable black outlines. | A possible restrained treatment within the recommended style, after a real combat test. |
| Pixelated 3D diorama | Moderate to high: the 3D scene survives, but resolution, effects, UI separation and camera movement need dedicated tuning. | Thin connections, fragment motion and marks can flicker or disappear. Full sprite replacement would require a much larger directional animation pipeline. | Not the easiest route for this game's information density. |
| Realistic or painterly imported characters | Highest integration burden: assets, materials, retargeting, sockets, cuts and consistent quality across a large catalog. | Possible, but a purchased model does not automatically fit the severable rig or authored attack timing. | Reserve for a future art team or a carefully limited external contribution. |

The current native capture shows that the largest opportunity is distinction: most enemies share proportions and the corridor repeats. Improve regional architecture and role silhouettes before adding surface texture. Registry troops can have tall rectangular shields and stamped shoulder plates; Cinderworks heavies can have broad pressure tanks and low helmets; Choir retrievers can have narrow bell frames and hooked tools. Preserve collision truth: decoration must not make a hittable body look several times wider than its actual collider.

Keep the Walker's light teal armor and bright visor. Use human proportions, breathing, weight shifts and visible fabric at protected joints to retain the established human identity. Industrial armor explains rigid construction; it does not require turning the cast into robots. Preserve the current impact and severing intensity.

A practical environment kit uses floor sections, side walls, gates, columns, railings, machinery sockets and distant silhouettes. Give each district one dominant architectural shape, a material palette and several signature props. Put costly-looking machinery beyond the traversable lane, where animation can be simple and collision unnecessary. Floor clutter must not obstruct walking or hide small bodies that can still be launched.

## What can be produced end to end

The core visual pipeline can be owned within this project: design modular meshes; generate and edit their geometry; attach pieces to the existing rig; author materials and lights; build weapon silhouettes; make pooled effects and code-native symbols; render consistent equipment thumbnails; integrate everything into normal runs; capture and compare native scenes. A paid asset library is optional, not a dependency.

Use a small set of coordinated surface families: dark structural metal, painted armor, ceramic, exposed mechanism, and restrained emissive material. Faceting and edge shape do most of the work. Regional variation comes from geometry, wear placement, signage and lighting. Keep effect colors stable across regions so lightning and healing never change meaning with the biome palette.

The existing eleven severable bones remain the attachment boundaries. Combine decoration only within one bone, keep cut surfaces capped, and leave colliders, masses, joints, sockets and damage timing under the current gameplay systems. Unity supports combining meshes with their transforms and a common material; that supplies a practical way to reduce decorative objects without combining separately moving limbs. See [Unity Mesh.CombineMeshes](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/mesh/combinemeshes). Custom materials should remain compatible with the project's URP rendering path; [Unity's SRP Batcher shader requirements](https://docs.unity.com/en-us/engine/6000.0/manual/materials-and-shaders/shaders/writing-custom-shaders-urp/in-universalrp-srp-batcher) describe the relevant constraints. Actual savings still require profiling.

Animation is the main craft risk. Better anticipation, contact poses, recovery and weight shifts can be authored on the current rig while the simulation keeps control of motion and hit timing. Realistic faces, flowing garments, elaborate cinematic acting and a new creature skeleton for every boss would exceed this economical pipeline. Stylized human armor, short text scenes and a few reusable rigs are a credible end-to-end target; finished quality still needs repeated visual review and player testing.

Concept illustrations can guide a region, but they are not playable assets and should never count as the art milestone. Each style decision must survive the actual camera, UI, moving bodies and full combat effects.

## Readability and physics gates

Every new effect needs a source, an affected target or area, a payoff and an end cue. Describe it in one plain sentence on its card. Prefer motion and shape as well as color. Persistent fields remain quieter than new attacks; a major ultimate must not erase enemy windups, the Walker or the sequence.

Rarity stays on item borders and badges. Combat effect color describes behavior. Region colors belong mainly to surfaces and lighting, with warning shapes kept consistent. A support link, a Conductive mark and a target preference must not share the same undifferentiated glow.

Review the new art in the densest pile, at the distant horizon, during an ultimate, in the reversed Pursuer camera and at 0.5x, 1x and 4x. Confirm paired weapon motions and two-handed silhouettes. Reduced-motion settings must preserve information. Native screenshots can establish that an effect renders; new-player tests are needed to establish that it is understood.

The existing [full-run measurements](QA/v080-fullrun-verification.md) identify late-body physics as a major cost at accelerated speed. A lighter art style does not solve that. Extending an eight-wave run to three acts requires independent profiling of the larger body count. Optimize body interaction queries, sleeping and world streaming only where physical availability and observable behavior are preserved; do not quietly delete corpses to make the new mode fit. If distant-body streaming is needed, define and verify how bodies return before relying on it. Do not promise the longer mode's performance from the current short-run numbers.

For an art-only change, fixed-seed combat outcomes, reach, contact timing, paired starts, sever mapping and body interactions should agree with the previous build. Gameplay changes get separate evaluation so a visually successful scene cannot conceal a combat regression.

## Delivery sequence

**First, establish one finished regional look.** Build a short Cinderworks lane, the Walker, three contrasting enemy silhouettes, all four weapon families and a representative ultimate using the real UI and physics. Include a crowded floor and the boss camera. The exit is a playable art comparison with measured performance and unchanged combat behavior, not a mood board.

**Next, deliver one complete act.** Add the route map, safe save/resume, salvage service node, three short events, two hub characters, three enemy roles and the Shift Foreman. Add ten initial catalog entries distributed across two weapons, two abilities, three gear pieces, one ultimate and two augments. Select those entries through the same behavior and clarity gate; the candidates above are a starting pool. Players should be able to discover a story fact, make a route tradeoff, build a new combo, fight a boss, and see a lasting hub consequence.

**Then, complete the three-act expedition.** Expand to the Registry, Choir and Heartgrid; add the remaining regional bosses, recurring characters, combined formations and the Pursuer's narrative role. Use the first act's encounter and reward data to control run length and catalog growth. Add the optional Warden only after normal routes provide sufficient variation.

**Finally, deepen replay.** Deliver alternative endings, further starting options, challenge modifiers, and the rest of the catalog where actual builds reveal useful gaps. Measure repeated-run route variety, viable paired and two-handed builds, useful rewards and first-time comprehension. Avoid padding the pool with stat-only identities to meet the approximate 120-item budget.

The main engineering foundation is replacing hardcoded wave-eight and checkpoint assumptions with explicit run, act, encounter and reward definitions. Save data needs stable identifiers and a versioned format. Existing combat simulation, effect ownership and the quick-run regression fixtures remain useful foundations. This is a substantial expansion; separating a complete first act from the full content target makes it possible to judge the game before multiplying its production cost.
