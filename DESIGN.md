# Walking Hard — build-variety contract

Revision 7, October 29, 2026. Implementation target: 0.6.0. This replaces the earlier augment proposal and the three-wave content proposal. Numerical values remain initial balance hypotheses, not a claim of equal build strength. Verification results are recorded separately in QA/v060-verification.md.

## Run and sequence

The Walker advances automatically, viewed from behind and above. A run has eight waves with frozen checkpoints. Entrances remain concealed side doors and the horizon. The level extends beyond the full run so later horizon arrivals remain over physical ground. Three-wave QA remains available with `-vectorQA`; add `-qaFullRun` for eight waves.

The five freely reorderable positions contain two persistent weapon basics and three drafted abilities. Empty positions are skipped; an occupied action waits for a reachable target. There is no sequence Auto toggle, mana system, injury action, or longer sequence.

Basic 1 uses mount 1; Basic 2 uses mount 2. Empty mounts share one owned starter-fist item. Matching adjacent one-handed families (fists, swords, hammers, guns) attack together, including position 5 → 1. Guns includes pistol and scattergun; a rifle occupies both mounts. A linked pair prefers independent targets and completes before progression. Different families execute separately. Ordinary paired weapon damage is not halved.

A two-handed item occupies both mounts, groups the two basics adjacently, and executes once. Drag either basic to move the locked group. Both attacks cannot carry separate sockets on that one item. Equipping a one-handed replacement stores the two-handed item and frees the other mount. Weapons remain visible in their mounted hands throughout ability and ultimate casts.

Runs start with an empty ultimate mount. Ultimates are excluded from all early item offers and rerolls. Checkpoint three (after wave three) replaces its mixed item reward with a choose-one-of-three ultimate draft using the normal rarity odds, then retains its separate augment draft. Opening this dedicated draft unlocks ultimates in later equipment/discovery offers. Equipping the first ultimate starts its full countdown. Ultimate Auto/Manual remains separate. U toggles; F queues a ready manual ultimate. A queued ultimate waits until the current turn and preparation finish, preserves the next sequence position, and resets its timer on completion. Changing the equipped ultimate at a checkpoint starts a full countdown. Recharge periods are 12 / 16 / 15 / 18 seconds for Singularity / Tempest / Cyclone / Redline.

## Rewards and checkpoint editing

| Point | Required drafts |
| --- | --- |
| Opening | Two separate pick-one-of-three abilities |
| After wave 1 | Third ability, then weapon |
| After wave 2 | Mixed, equipment |
| After wave 3 | Mixed, augment |
| After wave 4 | Mixed, equipment |
| After wave 5 | Mixed, augment |
| After wave 6 | Mixed, equipment |
| After wave 7 | Mixed |

Total: **13 item choices plus 2 augment choices**. No random augment drops are inserted elsewhere. Every pick is claimed exactly once. Pending drafts are shown before Let's fight. Edit after pick keeps the character sheet open after a claim; otherwise a valid direct equip/storage drop advances automatically. Both flows preserve all required drafts.

Ordinary mixed rewards provide an owned-item duplicate bundle, a limited tag-based build-support candidate, and a distinct discovery item. These are opportunities, not guarantees. Equipment drafts can support an empty armor slot and never contain modules. Rerolls bank; one is granted at each new checkpoint, not every extra draft. Each offered item's rarity rolls independently: Common 55%, Uncommon 27%, Rare 12%, Epic 5%, Legendary 1%.

The sheet has two weapon mounts, helmet, armor, boots, rig, separate ultimate, five actions, and storage. Drag or click/select a destination. Double-click auto-equips when the destination is unambiguous; a full unrelated ability goes to storage. Hover shows native behavior, actual rarity stats, module changes and cost. Category colors and icons are separate from rarity borders.

Three identical items of the same rarity fuse to the next tier. Power factors are 1 / 1.2 / 1.5 / 1.9 / 2.5. Legendary is capped. The keeper is the earliest equipped copy, with the shared starter fists preferred; kills and contributions merge. Fusion feedback compares actual before/after rarity and stats. Modules are the exception: no module rarity, currency, or fusion.

## Item identities

All identities function at Common. Higher rarity improves relevant stats and the authored effect; it does not remove the tradeoff. Profiles and signature identifiers live in `LoadoutCatalog.cs`; `AuthoredActions.cs` and `CombatSignatures.cs` execute them. Family membership is compatibility, not an identical attack definition.

| Slot | Item | Common identity / trigger | Tradeoff / limit | Rarity direction |
| --- | --- | --- | --- | --- |
| Ability | Arc Bolt | Chain a discharge through nearby targets; stagger adds damage. | Jumps require nearby reachable targets. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Breaker | A broad palm strike that staggers the front crowd. | Limited reach; rear enemies are outside the cone. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Conductive Brand | Mark a group for later lightning discharges. | Low immediate damage; requires a later consumer. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Counterguard | Create a barrier; absorbed damage empowers the next basic turn. | Only absorbed damage charges the payoff; one capped bank. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Force Push | Repel nearby enemies and fallen bodies. | Can push targets beyond the next melee action. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Gravity Well | Pull nearby enemies toward one gathering point. | Gathering does not guarantee a subsequent hit. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Mass Driver | Aim one body through a bounded collision cascade. | Needs a close primary target; limited shared collision budget. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Reaping Cut | A projected cutting arc; weakened targets take extra damage and lethal hits sever. | Slower commitment; bonus is damage, never automatic execution. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Steel Jab | Quick interrupt; bonus impact against an attacking enemy. | Short reach; one target. | Damage and the authored control, area, mark, launch or barrier profile. |
| Ability | Vector Tether | Pull targets into a narrow forward corridor. | Long thin setup instead of a compact cluster. | Damage and the authored control, area, mark, launch or barrier profile. |
| Armor | Bulwark Plate | Completing a sequence arms a guard. The next hit is absorbed and releases stagger. | One guard per completed loop. | More guard capacity and pulse coverage. |
| Armor | Duelist Jacket | A reload or sever arms one brief ward: reduce the next hit and stagger its attacker. | Ward expires after 4s; no stacking. | More mitigation and retaliation duration. |
| Armor | Grounding Mantle | Lightning/body impacts generate a capped barrier; attackers hitting it become Conductive. | Requires lightning or body impacts. | More barrier capacity and mark duration. |
| Armor | Reactive Shell | Three nearby enemies trigger a barrier. Its break/expiry repels the crowd. | 8s cooldown; repulsion can disrupt a gathered crowd. | More barrier capacity and repulsion reach. |
| Boots | Arcstep Boots | Two kills from one turn mark nearby survivors Conductive. | Requires a multi-kill; once per originating turn. | More marked targets and wider coverage. |
| Boots | Loader Greaves | Shorter preparation/reloads. Replenishing ammo sends a nearby stagger pulse. | Pulse is local; a paired reload triggers it once. | Faster preparation and a wider/longer pulse. |
| Boots | Momentum Soles | Launching a body accelerates your next non-weapon ability. | One stored boost; weapons do not consume it. | Stronger acceleration and longer storage. |
| Boots | Recovery Treads | The first kill of a turn drops a repair mote. Walk near it to collect healing. | Distant kills can leave motes out of reach. | More healing and pickup reach. |
| Helmet | Capacitor Crown | A direct hit against stagger applies Conductive, once per turn. | Marks need a later discharge or Relay Coil impact. | Longer marks and stronger discharges. |
| Helmet | Salvager Visor | A launch or sever primes the next basic to pull a fallen body toward its impact. | Needs a fallen body and a later basic. | More retrieval reach and force. |
| Helmet | Surveyor Visor | Multi-target turns echo a weaker hit on the last surviving target. | Requires two different direct-hit targets; once per turn. | Stronger echo and wider splash. |
| Helmet | Tactician Visor | Hits against staggered targets transfer stagger to one neighbor, once per turn. | Requires an already-staggered target. | More stagger transfer reach/duration and direct power. |
| Rig | Amplifier | Abilities bank charge; the next basic releases one impact burst. | Requires setup before a basic; paired basics spend it once. | Larger charge bank and burst. |
| Rig | Fault Line | First body impact in a turn emits a stagger aftershock. | Requires body impacts; one aftershock per originating turn. | More collision power and aftershock coverage. |
| Rig | Overclock | Successive turns ramp tempo within the loop; wrapping resets it. | Early turns have no ramp; no ultimate recharge speed. | Stronger tempo ramp, capped at 40%. |
| Rig | Relay Coil | Body impacts consume Conductive to discharge lightning. | Needs a mark and a later body impact; arcs cannot repeat the trigger. | One extra arc at Common, two at Rare, three at Legendary; more reach. |
| Rig | Salvage Magnet | A direct sever propels the body/fragments into one follow-up collision chain. | Once per turn, with one shared hit budget and enemy identity. | More retrieval reach and launch impulse. |
| Ultimate | Razor Cyclone | 15s recharge; radial cutting burst, lethal hits sever. | Local radius; no automatic executions. | Damage and radius. |
| Ultimate | Redline | 18s recharge; refill equipped guns and accelerate three sequence turns. | No damage; a full magazine is not a reload. | Haste 20% at Common, +3 percentage points per rarity. |
| Ultimate | Singularity Burst | 12s recharge; radial damage and up to six body launches sharing a cascade. | Needs nearby bodies; shared six-hit cascade budget. | Damage, launch force and reach. |
| Ultimate | Tempest Array | 16s recharge; 52 Common damage, seven linked targets; consumes Conductive and benefits from stagger. | Requires targets within 6m jump reach. | Damage, jump reach; eight targets at Rare, nine at Legendary. |
| Weapon / 1H | Breach Hammer | A light hammer staggers and knocks the first target forward. | Less force and splash than Impact Hammer. | Longer stagger; stronger controlled launch. |
| Weapon / 1H | Compact Scattergun | Three rounds; a short cone of knockback pellets. | Short reach; small magazine. | Wider useful coverage and better pellet retention. |
| Weapon / 1H | Conductor Wraps | Fist contacts apply Conductive for later lightning. | Lower direct impact/stagger than Kinetic Fists. | Longer marks and stronger discharges. |
| Weapon / 1H | Edge Sword | Broad cuts; lethal hits sever at the waist. | Less concentrated impact than a two-handed blade. | Wider reach/arc; Rare+ launches severed bodies. |
| Weapon / 1H | Kinetic Fists | Quick close strikes with reliable stagger. | Short reach and limited area. | Uncommon: two contacts. Rare+: stagger shockwave; higher rarity improves coverage. |
| Weapon / 1H | Pulse Pistol | Six rounds; quick ranged shots. | Reload delays the next weapon turn. | Uncommon: one ricochet. Rare+: two, with better retention/reach. |
| Weapon / 2H | Execution Greatsword | A broad heavy cleave; lethal contacts sever and shove bodies. | Slow single swing; both mounts. | Wider cleave, greater reach and follow-through force. |
| Weapon / 2H | Impact Hammer | One crushing launch using both mounts. | Slow commitment and both weapon mounts. | Uncommon: area stagger. Rare+: up to three launches sharing one cascade. |
| Weapon / 2H | Rail Rifle | Four rounds; pierce aligned targets using both mounts. | Needs alignment; four-round reload and both mounts. | More pierced targets and a final-target launch from Rare. |

### Initial numeric profiles

Weapon damage / action seconds / reach / magazine at Common: Kinetic Fists 19 / .36 / 2.7m / none; Wraps 15 / .40 / 2.7m / none; Edge Sword 34 / .74 / 3.7m / none; Greatsword 59 / 1.18 / 4.1m / none; Breach Hammer 29 / .66 / 3.1m / none; Impact Hammer 48 / 1.12 / 3.5m / none; Pistol 24 / .48 / 10m / 6; Rifle 42 / .90 / 13m / 4; Scattergun 31 / .75 / 6m / 3. Weapon range adds .12m per rarity step. Kinetic Fists add a second contact at Uncommon (native duration .54s); Wraps retain their own mark behavior.

Abilities at Common (damage / action seconds / range): Jab 24 / .40 / 2.6m; Breaker 30 / .68 / 3.2m; Push 20 / .95 / 5.3m; Driver 28 / .82 / 3.5m; Gravity 14 / .80 / 6.5m; Arc 28 / .65 / 9m; Brand 7 / .60 / 7m; Tether 10 / .80 / 8m; Reaping 28 / .85 / 4.5m; Counterguard 0 / .55 / 8m. Ability range adds .15m per rarity step.

Unless an explicit milestone overrides it, effect scale S = 1 + .15 × (rarity − 1), separately from damage/stat power. Gear radius adds .15m per rarity step and gear duration adds 10% per step. Do not apply module rarity scaling: there is none. Extra weapon/ability hits use the already-scaled host damage and their authored effect coefficient once.

- Conductive: 4S seconds; discharge 10 × rarity power, modified by Focused/Afterfield as described below. Consumption clears the mark before damage. The same origin cannot reapply and consume it repeatedly.
- Counterguard: 25 × rarity power barrier for 3.5S seconds; normal conversion 80% of absorbed damage, capped at 45S. One stored next-basic payoff, no accumulation. All simultaneous barriers share a 90-point cap. This is the first isolated balance change after the 1,000-game screening run; duration, conversion and other items are unchanged.
- Tactician: +6% direct damage × rarity power; +6 damage × rarity power against stagger. One nearby stagger transfer per turn: .25s, 3m at Common.
- Surveyor: two distinct direct targets permit one 9S echo on the last surviving target, plus a .7m weak splash with at most two neighbors.
- Salvager: launch/sever primes one retrieval within 5m at Common. Retrieved body uses normal collision limits.
- Capacitor: +4% damage × rarity power, and one direct stagger-hit mark per turn.
- Bulwark: +40 HP and 8% resistance × rarity power. Loop completion arms one guard, absorbing up to 14S; .4s stagger / 2.6m pulse at Common.
- Reactive: +25 HP × rarity power. Three targets within 3m trigger an 18S barrier; 8s cooldown, 3s life; expiry/break repels nearby living targets.
- Grounding: 5% resistance × rarity power. The first lightning hit or real body impact per originating turn converts 20% of damage × S into a barrier capped at 18S for 4s. A launch alone does not qualify. Barrier attackers receive Conductive.
- Duelist: +20 HP × rarity power. Reload/sever arms a 4s ward; next hit mitigated by 45% × S, capped at 65%, then attacker staggered.
- Recovery: +4% speed × rarity power. First kill per turn drops a 3S repair mote; 3m pickup, 10s lifetime, at most 24 motes. A brief visible drop precedes pickup; no passive regeneration.
- Momentum: +3% speed × rarity power. Launch arms 18% × S next-ability haste, capped at 40%, expiring after 5s at Common; no stacking.
- Loader: +2% speed × rarity power. Preparation/reload reduction 16% × S, capped at 40%; qualifying reload releases one .35s / 2.4m pulse for a paired turn.
- Arcstep: +3% speed × rarity power. Second kill of an origin marks up to 2 / 2 / 3 / 3 / 4 nearby survivors, once.
- Fault Line: +10S collision damage; one .4s / 2m aftershock per originating turn, never recursive.
- Overclock: +6% × S tempo per completed turn within a loop; reset on traversal wrap. Initial cap grows from 25% by four percentage points per rarity step, capped at 40%. No ultimate recharge bonus.
- Amplifier: one 6S charge per ability turn, up to three charges; next successful basic turn spends the bank on a 2.1m burst at Common, max five targets.
- Relay: consumes an impact target's Conductive, then arcs to 1 / 1 / 2 / 2 / 3 additional targets. 10S arc damage, 3m at Common; no arc-generated impact triggers.
- Salvage Magnet: first sever per turn launches that death identity; fragments do not create extra kills or triggers. Uses a shared six-hit cascade.

## Augment ownership and budgets

A distinct owned ability or weapon (including fists) has one socket. Armor, rigs and ultimates do not. The module stays attached to the host through sequence reordering, hand swaps and storage. Replacing/removing a module returns it to storage. A module can occupy one host only. On fusion, the keeper retains its module; consumed hosts' modules return to storage.

Both empty mounts reference the same starter fists: one host, one socket, one added module activation combining the two paired shares. Separate owned fists have separate sockets. Two-handed weapons receive one full module budget once. Distinct simultaneous one-handed hosts each receive half their added effect budget, even with an unaugmented partner. Ordinary attack damage is not halved. Separating the basics grants full module effects on two sequential turns. Geometry uses explicit caps rather than half-ranges.

Augment drafts offer one compatible ability module, one compatible weapon module and one distinct compatible option from either. Every offer has an owned compatible host. Another copy is eligible only when compatible owned host count exceeds owned copies of that module. Matching module copies still obey pair budgets.

Drag modules onto highlighted compatible equipped or stored hosts. Drag the socket diamond into storage to remove. Double-click attaches only with one unambiguous compatible destination; otherwise it leaves selection pending and highlights destinations. The socket diamond is also shown on the corresponding basic. Two-handed positions share the socket presentation.

| Module | Hosts | Benefit at Common | Persistent cost and limits |
| --- | --- | --- | --- |
| Follow-through | Jab, Breaker | One next-basic 0.4S-second stagger pulse, 2.2m, max 5 | Immediate stagger × .55; one bank, replaced rather than accumulated |
| Focused | Breaker, Arc, Brand, Reaping | Breaker stagger ×1.5; Arc primary ×1.35; Brand discharge ×1.4; Reaping weak-target bonus +80% | Breaker half arc 40°; Arc 1 target (2 Rare+); Brand 2 marks; Reaping half arc 42°; no execution |
| Echo | Jab, Arc, Reaping | One primary-only repeat, .35S of host damage; +.22s | Opening ×.8; never repeats whole cleave/chain |
| Afterfield | Gravity, Tether, Brand | 2s field, .4s ticks, six distinct targets; point vs corridor preserved | Initial strength ×.65; lingering pull/mark only, no damage loop |
| Split Payload | Driver | Up to two launches, one shared six-hit/exclusion budget | Each impulse ×.65; still executes with one eligible target |
| Backwash | Push | Surviving affected living targets return 45% toward original positions over .32s | Outward ×.65; return ends before advancement; no corpse/untouched return |
| Discharge | Counterguard | 55% absorption converted on barrier break/expiry, cap 30S, 4m forward cone, max 5 | Replaces 80% next-basic bank; no simultaneous bank |
| Anchor | Gravity, Tether | .25S-second stagger only after at least .25m actual movement | Pull ×.7; resistant/blocked targets may not qualify |
| Flurry | Fists | Two .16S follow-ups; each gets the host's pair share | Opening ×.82; +.28s; same primary and origin; bounded stagger |
| Palm Drive | Fists | First contact shoves with (6 + .6 per rarity step) × share | Damage ×.8; can force a later melee range wait; no cascade |
| Crosscut | Swords | .45S return cut × share; cap 4 unpaired / 2 paired | Opening ×.8; +.32s; lethal severs share death identity and proc limits |
| Hooked Edge | Swords | First surviving struck enemy pulled toward the forward impact area, force 8S × share | Reach ×.85; arc ×.65; no untouched/corpse retrieval |
| Groundbreaker | Hammers | Forward 4m wave (3.2m paired), width 1.8m, max 3 extra targets (2 paired), .45S × share stagger | Native launch ×.65; descendants cannot emit waves |
| Piledriver | Hammers | One launch with extra .35S × share impulse multiplier | Splash/additional launches removed at every rarity; normal collision caps |
| Bore Rounds | Guns | Pistol bounded straight line; rifle adds bounded penetration; scatter has 20° half cone plus one .35-damage central penetrating shot | +.30s FOCUSING after reload/readying; pistol/scatter line cap 2→4 (paired max3), rifle max6 |
| Breach Load | Guns | Actual refill arms one .4S × share stagger burst; 2m (1.6m paired), +.15m/rarity; max5 (3 paired) | Reload 1.2→1.5s before Loader; one armed state; one successful shot consumes it, never per pellet/bounce |

Shared next-basic payoffs, Amplifier, Overclock and gear triggers treat a pair as one turn. Sequence lead resolves first, then its partner: this stable ordering determines primary shared payoff and mark consumption. Follow-ups remain within their origin; derived effects cannot generate recursive copies of themselves. Additional derived hits share a per-origin budget of twelve. A paired reload triggers Loader once. Redline only arms Breach if it actually restores ammo.

## Encounters, information and physical rules

Wave previews correspond to the actual formation mix: dense packs; ranged crossfire; heavy front; side-door flankers; ranged support behind heavies; repeated breach packs; mixed pressure; Warden plus reinforcements. Shooters use a visible windup and distance check. The Warden has 2.5 seconds of full control susceptibility every eight seconds; otherwise displacement/stagger is reduced to 22%, not immune. Heavy enemies retain 65% control susceptibility. Warden health/control state is visible.

Stance changes take .28s before Loader; native reload 1.2s. Pair delays complete before attack progression. Amber tile bars show real RELOADING, READYING, FOCUSING and WAITING FOR PAIR time. ECHO, FLURRY, RETURN CUT and BACKWASH show their actual extension. Unbounded target/range waits have no invented countdown. Small armed and consumed states explain the next-basic payoffs.

Stagger uses target intent interruption, Conductive uses a persistent lightning shape, barriers use a local outline, and geometry footprints correspond to real cones/corridors. Pull paths and physical movement identify gathering. Outgoing and return cuts use different direction/color. Reload bursts occur once at the real impact. Icons and labels supplement color. Native frame sequences and simulation traces are review aids, not a substitute for player readability testing.

Fixed-step time, speed (.5/1/2/4), pause and checkpoints retain their contracts. Ragdolls and authored waist fragments persist until a new run. Walking capsules ignore settled bodies, with one-way shoves; launched bodies temporarily collide. Cascades have six shared hits, an exclusion set, at most two extra hops and declining energy. One enemy identity produces one kill across all fragments.

Kills and contributions belong to owned items and follow storage/movement/fusion. The recap includes direct/collision damage, control, absorption/repair, reload/preparation/range waiting, augment damage and setup assists. Gather/pull assistance requires a follow-up within three seconds; assists do not add kills. Counted interaction explanations cover Conductive consumption, Counterguard/Amplifier/Follow-through payoffs and reload-armed Breach Load.

Combo scoring is unchanged: 100 base points (200 heavy), three-second banking window, bulk/chain/streak bonuses up to ×10. Checkpoints and run end bank once.

## Archetypes and remaining hypotheses

Iron Saint uses stagger, guards and fists; Vector Savant arranges targets and bodies; Storm Conductor prepares and consumes Conductive; Sever Artist creates and reuses severed bodies; Wrecking Saint concentrates impacts and collision chains; Deadeye Engine uses lines, magazines and reload payoffs. These are overlapping combinations, never classes, set bonuses or equipment restrictions.

Required hybrid fixtures include Human Pinball, Thunder Freight, Shrapnel Ballet, Ballistic Funnel, Cut & Carry and Close Quarters. Matched-rarity comparisons cover distinct modules, identical modules, one augmented partner, shared fists, separated basics and two-handed swords/hammers/guns. Read the measured results before claiming balance. More targets, raw damage and control are different benefits; reward investment and reload exposure matter.

Normal enemy damage is initially 3.2, HP 40 with +3.5% per wave; heavies multiply HP by 1.9. The Warden uses ×24 HP before the wave multiplier. Grounding conversion, paired DPS, two-handed opportunity costs, high-rarity crowd control and eight-wave pacing remain tuning hypotheses. Procedural models, authored cuts and animations remain prototype art. Arbitrary mesh slicing and permanent progression are outside this update.

## Engine-only balance experiments

Headless batches run the production fixed-step combat and PhysX code with cosmetic rendering disabled. Each worker reuses its process but resets all run actors, ownership and combat state. Random, balanced and four synergy-weighted drafting policies exercise different builds; these are test agents, not optimal players. CSV/JSON output separates defeats from invalid runs and timeouts, records per-item contributions, and supports matched-seed before/after comparisons. Optional balance JSON overrides numerical hypotheses without recompiling. See SIMULATIONS.md for commands and limits. Full ragdoll physics remains CPU-intensive; three-wave sweeps are faster, while eight-wave runs exercise both augment choices and the boss. Simulation results cannot establish visual quality or enjoyment.


### Item identity and fusion readability (0.6.2)

Three copies of the exact same definition at the same rarity fuse automatically across equipped gear and storage. Different rarities do not contribute to that set. Each physical item counts once, including a two-handed weapon or the shared fallback Fists. Legendary is the cap. Augments retain the approved host-scaled, non-fusing contract.

Every definition has stable artwork. Rarity changes the border and written tier, not the artwork; category glyphs have one meaning across all surfaces. Stored cards show their tier initial and matching-copy count. Reward previews include incoming copies, show when they will fuse and retain leftover-copy information in the detail panel. Hover details list other-rarity copies separately, and matching owned copies highlight together. Storage sorts by type, item and rarity without altering ownership or fusion priority.

The optional Icon Key explains item types, rarity progression and labeled synergy symbols. A shared symbol means a shared synergy concept, not a promise that every item creates or consumes it in the same way; the effect description states the actual interaction. No balance, rarity odds, fusion thresholds or augment mechanics change in this presentation update.


### Visible effects and combat HUD (0.6.3)

Each pick-three card shows one plain-language effect line below its art/stats and above fusion progress. Both checkpoint and combat sequences show the same concise effect copy; basic cards resolve their equipped weapon or fallback fists, and augmented hosts describe the modified action. Full numerical rules and tradeoffs remain in hover details. Empty equipment mounts show their slot label and empty status without artwork; empty weapon mounts explain that basics use fists.

Combat UI uses 80% of the previous scale with 66%-opaque panels and a lighter sequence backing. Checkpoint, pause and recap scales remain unchanged. An empty ultimate mount shows the checkpoint-three milestone instead of a countdown or usable controls.
