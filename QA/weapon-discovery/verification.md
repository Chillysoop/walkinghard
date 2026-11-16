# 0.8.3 weapon-discovery verification

## Player-facing change

Checkpoints **2, 9 and 16** reserve the third card of the existing mixed reward for a less recently offered unlocked weapon. History tracks exact catalog identities, including distinct fist styles and 1H/2H variants, and includes passed reward cards. It excludes shop stock. The first two cards retain their owned-copy, support and route-bias rules. No extra choice, reward, currency, menu or equipment slot is added.

The card is labeled **WEAPON DISCOVERY** in the tablet with a one-line explanation. It uses the existing selection, hover comparison, storage, equipment and fusion flow. Rerolls retain discovery and cost the usual charge. Schema 8 persists history and the offered card. Earlier compatible checkpoints preserve current offers and begin history on their next reward generation.

**Combat values, pair timing, two-handed budgets, rarity odds and fusion rules are unchanged.** The seeded discovery tie-break does not draw from the reward RNG. Reserving a card trades away three wildcard opportunities over a completed journey; it improves weapon breadth without promising a particular set or higher rarity.

## Offer audit

[Machine-readable audit](rendered-acceptance/offer-audit.jsonl), [assertions](rendered-acceptance/checks.txt).

512 seeds per reward pool, 1,024 paired shadow sequences. Each pair takes the unchanged first card to hold acquisition/RNG context equal; these are offer-generation checks, not combat or player-choice simulations. All three mixed cards, first-two definitions/copy counts, rarity sequences, unlock gating, distinct offers, exact-identity recency, tiny-pool fallback, committed mount constraints, fusion, save migration and deterministic rerolls were checked. The final suite adds 44 fork/entrance assertions for 105,558 total repeated assertions.

| Reward pool | Mean distinct weapon identities, original → discovery | Runs offered Breach Hammer, original → discovery |
| --- | ---: | ---: |
| Fresh | 5.65 → 7.39 | 376/512 → 467/512 |
| Fully unlocked | 6.53 → 8.44 | 354/512 → 445/512 |

This supports the access change. It does not establish its effect on survival, defensive reward quality, or human satisfaction. The third card can still be Common, already owned, or less useful to the current build.

## Normal-health journey comparison

[Main cohort and per-run sources](cohort-summary.json), [fresh-account smoke cases](fresh-summary.json).

The initial cohort crosses seeds **1741, 4319 and 6229**, gun / sword-and-hammer / close-quarters preferences, adaptive / committed drafts, and original / discovery offers: **36 ordinary-health Full PhysX attempts: 35 valid outcomes and one encounter stall**. All use the fully unlocked pool, real rewards, normal costs and the full 21-encounter path. These measurements precede the entrance fix described below; they are exploratory evidence, not final-build win rates. Three additional committed cases use the fresh-account pool and seed 1741; these are smoke cases without a matched original-offer fresh cohort.

Committed drafts constrain their weapon mounts, retaining starter fists until a missing component drops and prioritizing the missing component when available. Guns can use 1H or 2H guns; sword-and-hammer uses a 1H sword and 1H hammer; close quarters uses fists and a 1H gun. Abilities, gear and ultimates still adapt. No equipment is injected. Adaptive drafts can pivot completely, so a completion under a preference label is not automatically a completion with that weapon combination.

Reports capture the actual weapons/ranks, pair/2H state, damage, target waiting, reload delays, healing and integrity for every encounter, including the fatal one. Global per-item totals retain assists, control and weapon healing; damage alone does not capture setup value. Encounter contribution rates use active simulation seconds, excluding hit stop/reveal. Paired items' wait/reload seconds can overlap. Offer exposure excludes shops and is censored by death.

This small fixed-seed comparison is **not a human win-rate estimate, an optimal bot, or an equal-rarity weapon benchmark**. PhysX outcomes can differ between attempts, and equipment choices change later reward composition. Missing components are recorded separately from victories, and a loss remains a valid outcome.

| Draft / preference | Original offers: wins / valid runs | Discovery: wins / valid runs | Complete weapon combination assembled, original → discovery |
| --- | ---: | ---: | ---: |
| Adaptive guns | 1/3 | 1/3 | 2/3 → 2/3 |
| Adaptive sword + hammer | 1/3 | 1/3 | 0/3 → 1/3 |
| Adaptive fists + gun | 1/3 | 1/3 | 2/3 → 2/3 |
| Committed guns | 1/3 | 1/3 | 3/3 → 3/3 |
| Committed sword + hammer | 2/3 | 1/3 | 1/3 → 1/3 |
| Committed fists + gun | 1/3 | 0/2 | 3/3 → 2/2 |

The missing discovery/committed close-quarters outcome is seed 4319's stall, retained as an invalid attempt rather than counted as a defeat. "Assembled" means the combination appeared in at least one encounter; it does not mean it remained equipped at victory. None of the adaptive victories finished with their named combination. The original committed sword/hammer victory at seed 1741 finished with a Rare Edge Sword and starter fists: the missing hammer cannot be treated as a successful sword/hammer balance result.

Useful access examples in the initial cohort:

- Seed 4319's committed guns first equipped a gun for fight **10** with original offers, versus fight **3** after a discovery Pistol at checkpoint 2. Both completed. Their later ability/gear choices differ, so this is evidence of earlier access, not isolated gun strength.
- The same seed's committed sword/hammer build received Breach Hammer at checkpoint **15** originally, versus discovery checkpoint **9**; the completed combination first fought in encounters **16** and **10** respectively. Both completed.
- Discovery does not guarantee a required component. Seed 1741 still missed Breach Hammer in the unlocked committed sword/hammer case. Its discovery choices favored Wraps, Siphon Blade and Greatsword as those checkpoints were reached.
- The fresh-account smoke cases ended at encounter 20 for guns, completed 21 with a Common Siphon Blade plus Common Breach Hammer for sword/hammer, and ended at encounter 13 for close quarters. That hammer came from an ordinary checkpoint-13 offer. These cases do not establish a fresh-account improvement without matched controls.

The contribution measurements do not support blanket buffs. Committed guns' weapon share was 31.3% with original offers and 16.4% with discovery; committed sword/hammer was 27.6% and 29.3%. Those totals include incomplete early builds and different ability rarities, and do not measure weapon setup value. Summed weapon target waiting per active fight second was 0.089 → 0.109 for guns and 0.167 → 0.161 for sword/hammer. Survivability did not improve consistently. Per-run healing, incoming damage, assists and district breakdowns remain in the linked summaries. The decision is to improve breadth, retain existing combat values, and avoid interpreting bot labels or small samples as weapon balance.

## Encounter stall found and fixed

[Incident and original attempt](incidents.json), [collider diagnosis](entry-trace/timeout-state.txt), [post-fix natural confirmations](confirmation-summary.json), [restored checkpoint continuation](replay-fixed/samples.jsonl).

The original close-quarters seed 4319 stalled in encounter 4 for more than 2,100 review seconds. The champion and other arrivals remained inside covered rooms with `entering=true`; the hero waited for targets and the champion completion gate correctly stayed closed. A new fork cut the parent road at local Z 57.06, putting its outer wall across both entrance rooms centered at Z 54. The same blockage reproduced from the saved checkpoint and in a fresh diagnostic run.

New fork placement now preserves occupied entry rooms and moves the split at least six metres beyond a nearby entry center. Spawn selection capsule-tests the full route from room to exit using champion-sized clearance. The tests reproduce the old obstruction, check both road sides at several offsets, retain a farther occupied room, physically walk a scaled actor out of each entrance, and reject blocked doors in unchanged older geometry. Saved fork origins remain exact; no teleport, enemy deletion, completion bypass or health nerf is involved.

After the fix, both the saved failing checkpoint and a fresh seed-4319 committed close-quarters run cleared encounter 4 and lost normally in encounter 5. The committed gun and sword/hammer confirmations each completed all 21 encounters. These three fresh runs have valid outcomes. The restored run is continuation-only and is excluded from full-run averages. Road placement can affect later combat, so these confirmations are reported separately from the original cohort.

## Physics investigation

[Stress summary](stress-summary.json), [56 fixture results](stress-final/stress.jsonl).

Eight seeds across open launches, wide severs, both walls, the fork nose and both road branches. Each fixture uses twelve actors and twelve simulated seconds, repeated six-body launches, actual joints, physical contacts and production collision budgets. The production-fix build recorded 2,295 launches, 1,152 detached parts and 279 collision hits, with no invariant failure. Peak observed bone speed was 56.77 m/s and the lowest bone center was approximately -0.10 m; the existing stability thresholds were not relaxed. The final assembly differs only in the rendered test fixture's HUD state.

**The previous rare ragdoll outlier did not reproduce in these fixtures. No physics fix is claimed.** Existing bone-position/velocity diagnostics remain enabled. The stress suite is a repeatable probe, not proof against rare failures in a full journey.

## Test provenance

Windows version **0.8.3**, final assembly SHA-256 `17352C03FB8F3A50B0E0B4D2BFC02FA6C1034EE48D2C186F69469AB4840CA80B`.

The initial shadow-audit assertion assumed every draft kind always had three cards, even after synthetic inventory acquisition. It was narrowed to the changed mixed rewards while comparing unchanged draft kinds against their original shadow deck; this was a harness correction, not a reward-rule change. The new geometry fixture initially retained Drafting state after clearing its offers, causing a rendered HUD exception; it now stays Running while yielding. Both issues and successful reruns are retained in the [iteration notes](iteration-notes.json).

Build identities are explicit in [provenance](provenance.json). The initial combat cohort used `A3E35A...`; the entrance/fork fix used `34ED78...`. The final `17352C...` changes only the isolated test fixture's state, with identical production gameplay. It was built into a temporary comparison folder while the previous binary completed its regression batch; neither runner's executable was replaced underneath it.

- **15 core regression scenarios**, each with **761 mechanic assertions**, passed, recorded in `QA/discovery-acceptance-*.json` on the final assembly. The production-fix build also passed the fifteen scenarios in `QA/discovery-fix-final-*.json`.
- **50 weapon/augment variety scenarios** plus the **eight-encounter full regression** passed, recorded in `QA/discovery-fix-variety-*.json` and `QA/discovery-fix-full-full-1741.json` on the production-fix assembly. These validate production effects and paired/2H contracts, not natural-run win rates. The core suite uses elevated integrity for functional coverage; the variety suite uses its authored fixtures.
- **117 progression/recap checks** passed on the production-fix assembly: [checks](progression-final/checks.txt).
- **136 journey-flow checks** passed on the production-fix assembly: [report](journey-final/report.json).
- **105,558 assertions**, including 1,024 paired shadow reward sequences, migration, reroll, committed-policy and 44 entrance checks, passed headlessly on the production-fix assembly and rendered on the final assembly: [rendered checks](rendered-acceptance/checks.txt).
- Inspected the final discovery tablet at [720p](rendered-acceptance/discovery-1280x720.png), [1080p](rendered-acceptance/discovery-1920x1080.png), and [ultrawide](rendered-acceptance/discovery-2560x1080.png).
- **45 rendered fork assertions and 761 mechanic assertions** passed on the final assembly: [report](fork-rendered-final/report.json). Inspected both road directions, selected-road highlights and reduced-motion 720p presentation. The earlier headless `fork-final` screenshots are black and are not visual evidence.
- **56 Full PhysX stress fixtures** passed on the production-fix assembly. No clamps, hidden corpse deletion, looser thresholds or ragdoll tuning were introduced.
