# Build readability and controlled balance — 0.9.2

This version adds twelve item illustrations and factual passive feedback to the existing checkpoint details and owned-item hover. It does not change catalog values, effects, targeting, action order, fusion, loot, physics, or the user's chosen 4× behavior. The Rivet 22-damage candidate is supplied only through an explicit simulation override.

## What players see

- Six constellation weapons and six abilities now have individual illustrations shared by rewards, equipment, storage and sequence tiles. Category symbols and rarity outlines retain their existing roles. The remaining passive/ultimate schematic identities are cached as textures so scrolling clips cannot displace their strokes.
- Equipped passive gear stays in the last-wave report even at zero output. Explanations distinguish missing prerequisite events from observed effects: no ammunition replenished, no discharge, a created barrier that absorbed nothing, or applied marks without an owned discharge. Numeric contribution columns remain the source of totals.
- Owned-item hover/comparison details show the same last-wave statement. Exact item identity survives save/load; a new same-named item cannot inherit it. Older feedback loads without inventing explanations. Repeated first-line effect descriptions are suppressed in the comparison panel.
- Observers only record events. They do not select targets, draw random values, consume proc budgets or change numerical combat rules. Temporary observers reset per wave; completed feedback is saved.

## Comparison design

808 controlled outcomes are retained in `engine-results.jsonl.gz`; exact inputs, script, per-case contributions, scopes and assembly hashes are beside this document. `analyze.py` regenerates `summary.json` and `engine-summary.md`. Raw runtime logs remain in ignored `QA/Simulations/v092-*` directories.

| Scope | Cases | Design |
|---|---:|---|
| Branch kits / Fast | 288 | Six new and six established kits; Common/Rare/Legendary; eight matched seeds |
| Single substitutions / Fast | 160 | Ten slot substitutions in Storm, Ballistics and Siphon; Common/Rare; eight seeds |
| Rivet / Fast | 216 | 144 Pulse/Rivet baselines and 72 matched Rivet 22 candidates; ranged/close, with/without Bore, three rarities, six seeds |
| Branch kits / Full PhysX | 24 | Both kits per branch, Common/Rare, one matched seed |
| Rivet / Full PhysX | 12 | Eight ranged baselines and four candidates; Common/Rare, with/without Bore, one matched seed |
| Current bosses / Full PhysX | 108 | 36 equal-rarity kits against all three current bosses, one seed per branch |

The eight-wave cases use fixed kits, no further loot, normal base health, matching support rarities and production checkpoint-three ultimate progression. These are the legacy controlled encounter sequence, **not ordinary 21-encounter draft win rates**. The boss matrix uses full normal starting health, a full ultimate countdown, real approaches and reinforcements. Boss success means the boss died, not that an ordinary journey was completed. All 808 completed without engine-invalid outcomes, missing/duplicate identities or timeouts. All 108 boss encounters ended in a boss defeat.

Do not pool Fast and Full. One Full seed per comparison is a confirmation sample, not a population estimate. Whole kits differ in several slots; only the substitution matrix isolates a slot, and that result remains specific to its context. Remaining HP includes zero for deaths and is not normalized for differing gear health bonuses.

## Findings

**Storm has the clearest weakness.** At Rare, the new kit won 0/8 Fast cases versus 8/8 for the established kit; its Full case died in wave six while the established kit finished with 237 HP. Replacing only Static Shroud with Grounding raised the new kit to 6/8 Fast wins. Replacing only Arc Carbine with the rifle stayed at 0/8, as did swapping Storm Lash for Conductive Brand. In the Full new-kit sample, Shroud supplied three discharged marks; it is doing something, but the kit sacrifices substantial defense for more marking. This does not establish that Arc Carbine needs a damage buff.

**Ballistics is the second concern, with mode sensitivity.** Rare new-kit Fast wins were 0/8 versus 7/8 established. Both Full kits won, but new-kit HP was 98.5 versus 193.5 and completion took 34.8 seconds longer. Replacing only Breach Salvo with Arc Bolt gave 2/8 Fast wins; replacing Dead Letter with Pulse gave 1/8. Loader/Recoil swaps gave 0/8. The tested Full kit's Ballistic Vest absorbed 82.3, Vent Greaves healed 68.2, and Rangefinder dealt 265.2 bonus damage: these effects are active, not universally broken. Treat a Salvo buff as a candidate, not a proven fix for the whole branch.

**Rivet 22 is not a reliable improvement.** In Rare ranged Fast tests, Rivet without Bore remained 0/6; with Bore it rose from 0/6 to 1/6. Rare Full without Bore changed 145.5→148.7 HP, while with Bore it fell 184.6→135.9. Full Common + Bore survived to wave six instead of five, but still lost. Common close-range Fast HP improved (83.4→120.3 without Bore), yet higher-rarity changes were mixed. Keep the authored 20 damage pending a more targeted control/timing experiment; this small sample does not establish that 22 is harmful either.

**Sever looks strong; no nerf is justified yet.** Rare Fast HP improved from 46.6 to 165.6, and Full from 145.6 to 227.5. Both kits already win their Rare cases. Common kits still lose the fixed eight-wave gauntlet. The new boss kit is only modestly faster. Isolate Rivet Sabre, Execution Hood and Razor Fan before attributing the gain to one item.

**Impact is healthy with a speed tradeoff.** Both Rare kits won all Fast cases; the new kit retained more HP but took about 18 seconds longer. Full did not repeat the HP advantage (182.1 versus 205.4). Keep current values.

**Siphon is weak at Common, but broad buffs risk overshooting Rare+.** New Common Fast went 0/8 versus established 6/8; Rare both went 8/8. Full Rare finished at essentially equal HP (308.7 versus 307.6), and every isolated boss case won. Swapping Reservoir for Reclaimer did not rescue Common, and reduced Rare Fast remaining HP. Swapping Reclaim Pulse for Breaker helped Rare HP but did not rescue Common. This is a kit-level early-power question, not evidence that Reservoir is simply inferior.

**Guard trades clear speed for safety.** All nine new-kit boss cases won, but average Common boss time was 110.0 seconds versus 82.5 established; its slowest fight took 178.1 of the 180-second budget. Legendary new-kit HP is high, but it also clears more slowly. Keep its defensive values; isolate Maul versus Hammer and Repulsor versus Counterguard before a damage change.

## Recommended next balance experiments — not applied

1. **Static Shroud:** retain its current marks and add **6 barrier for 3 seconds when a loop successfully marks at least one enemy**, once per loop; refresh this item's barrier rather than stack it. Use existing effect scaling (+15% capacity per rarity step) and the global 90-barrier ceiling. This is a proposed new defensive effect, requiring approval; the tested Grounding substitution motivates it but does not validate the value.
2. **Breach Salvo:** test **25→28 base damage (+12%)**, keeping its 0.8-second action time, three-target lane, 50% prior-stagger bonus and rarity scaling unchanged. Change one variable first, then compare the same Ballistics cases and mixed builds. This value has not been tested here.
3. **Keep Rivet at 20**, rifle at 42, and leave other branch values unchanged for this round. No new nerf is supported by this dataset. Check Common Siphon sequence/order alternatives and Guard's slowest boss case before changing their numbers.

Acceptance for the next experiments: improve Common/Rare survival or clear time in the weak context, avoid making Legendary trivial, preserve existing mixed builds, and confirm direction in Full PhysX. Do not install either candidate as production balance without review.

## Validation and initial findings

Final validation and assembly provenance are recorded in `validation.json`. All 15 final core cases passed with 877 mechanic assertions each; final loadout checks passed 472 model and 2,322 rendered assertions; constellation checks passed 449 headless and 501 rendered assertions. The earlier assembly passed all 50 variety cases and the normal-health eight-wave full case (279.5 final HP). The final assembly additionally reproduced 24 baseline Fast outcomes exactly across result, reached wave, health, kills, spawns, collisions, launches, reloads, score and elapsed combat time. No comparative FPS claim is made. Native screenshots are in `captures/`; the illustration contact sheet is `art-contact.png`. These are deterministic engine fixtures, not human usability or FPS measurements.

An initial compilation caught a review-only local-variable shadowing error; it was corrected before testing. The first rendered passive report exposed clipped vector strokes in scrolling rows. Schematic icons now cache the same design into textures, avoiding nested GUI transforms. Initial evidence is retained rather than counted as a clean first pass. The comparison assembly predates that presentation-only fix; final deterministic comparisons check that combat output is unchanged.

Generation records: `Art/constellation-v092-prompts.json` contains initial specifications; `Art/constellation-v092-generation.json` records actual prompts and source paths. The image service rejected two attempts in the batch; a clearly fictional ray-pistol illustration completed the final card. No downloaded third-party art was introduced.
