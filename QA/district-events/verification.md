# District events — 0.8.7 verification

## Player-facing change

Six events are grouped by district: pressure/rescue in Cinderworks, auction/exchange in the Foundry, and work orders/sealed equipment in Dispatch. Two events are new; the existing four are reworked for distinct costs and rewards. There is still one event-versus-elite offer per district. Existing combat layout RNG, regular draft counts, catalog effects, rarity roll weights, weapon pairing and fusion rules are unchanged.

Actual item art and effect summaries show each possible prize. Cards forecast complete fusion chains and the resulting effect, storage/equipped/fallback destination, and a second equipped weapon consumed by fusion. Success and failure costs are explicit; the cache shows both conditional and overall odds. All rewards respect the run's unlock snapshot. Events remain paused, and an open legacy event keeps its original terms. New lore fragments persist in the eight-record archive.

Reclamation consumes only the selected stored, unaugmented item. The surrendered item's contribution history remains separate from its replacement in the recap and live totals, survives saves, and can receive later lingering-effect credit. Fusion predecessors remain counted only through their surviving keeper. The historical recap screenshot uses injected counters, deliberately including three item kills while its synthetic run total is zero; it is a display/attribution fixture, not an actual playthrough.

## Initial findings

The first compile failed because the new review harness tried to assign the private ultimate-unlock setter. The harness now opens the normal ultimate draft. The original compile log remains local in `Logs/build-v087-initial.log`; relevant errors are retained in `initial-build-errors.txt`.

The initial event mechanics/render checks passed (1,592 / 1,638 assertions), including complete save graph round trips, identical rolls after restoring a pre-choice save, old handler gating and chained 2H fusion. Visual review led to brighter secondary text, rarity labels that distinguish the incoming item from its projected tier, and effect text for the actual post-fusion tier.

Code review found that removing an item from inventory would hide its old contributions from the recap. Historical enumeration now includes unmerged attributed items outside inventory; 1,596 mechanics / 1,642 rendered assertions passed with exchange-history tests. Final preview refinements add paired-mount and fallback-fist outcomes. A final menu check found the archive's old `/ 6` count; it is corrected to `/ 8`. The added archive capture initially stayed on the legacy event screen because that overlay is drawn from the main menu. Visual inspection caught this despite the functional checks passing; the harness now enters the actual menu and asserts the overlay state. The initial mislabeled capture is retained as `archive-capture-initial.png`. No event, combat or economy rule changed after the comparison assembly described below.

## Economy sample

Comparison assembly: `E5950772517455568918BAC571C777C4CFD142C477E84DD3F4AD31313AA13DFE`. The production engine ran 1,000 deterministic seeds for each event/option, with one attempt then banking: Common equipped sword, spare Uncommon pistol, full unlocks, ultimate gate open, 280 maximum / 180 current integrity. All 9,000 attempts passed their execution checks.

| Event / option | Successes / 1,000 | Mean wallet change | Mean integrity change | Mean awarded copy-price value |
| --- | ---: | ---: | ---: | ---: |
| PressureWorks / option 0 | 792 | +6.74 | -2.28 | 0.00 |
| PressureWorks / option 1 | 594 | +8.32 | -8.94 | 0.00 |
| Rescue / option 0 | 867 | -2.39 | +16.23 | 0.00 |
| Rescue / option 1 | 567 | +1.71 | +5.57 | 0.00 |
| Auction / option 0 | 452 | -6.00 | +0.00 | 6.77 |
| Auction / option 1 | 697 | -10.50 | +0.00 | 10.46 |
| Reclamation / option 0 | 1000 | -3.00 | +0.00 | 18.00 |
| Overtime / option 0 | 775 | +0.00 | -14.49 | 11.63 |
| DeadLetter / option 0 | 600 | +0.00 | +0.00 | 12.01 |

Copy-price value is not spendable salvage or net value. Reclamation also surrenders its Uncommon input (shop-copy reference value 18), so its guaranteed replacement is an identity change, not 18 free currency. Work orders surrender a 3–5 salvage alternative and spend integrity on both outcomes; the cache surrenders 4–6 salvage. Those alternatives do not appear as wallet debits. Pressure returns include the starting unbanked haul. Repair samples have missing integrity; full-health rescues cannot bank unused healing.

For comparison, the shop guarantees 25% repair for 12 salvage, or a chosen stocked copy for 12 / 18 / 26 / 36 salvage across Common–Epic. Strong auction bids average close to that copy price per success in this sample, while still risking failure. Small bids trade lower success for less committed currency. Pressure rewards average 6.74 / 8.32 salvage after one normal/override attempt, with different injury exposure; the guaranteed champion bonus remains +6 after defeating the champion, on top of the ordinary encounter payout. These comparisons support distinct choices, not a claim that every option is equally attractive for every build.

The auction discovery lot uses the existing 55/27/12/5/1 rarity weights. The cache deliberately advertises fixed Uncommon/Rare alternatives. Reclamation preserves the input rarity. Higher-rarity work orders cost more integrity. Raw sample rows, rank distributions and manifest are in `economy-report.json`; the full 9,000-check working report remains local under `QA/v087-events-release-economy`.

## Ordinary-loot journey comparisons

All eight cases used the same comparison assembly above, normal health, actual reward offers, automatic projected drafts and full PhysX. `-OriginalEvents` restores the previous shared four-event pool and mechanics in the same binary; both plans keep identical initial combat layout RNG. Automated route/event decisions are reproducible heuristics, not human or optimal choices. They exercise all six new event kinds across the two seeds. The comparisons include changed choices, equipment, injury and later combat consequences.

| Pool / seed / preference | Final integrity, baseline → candidate | Final salvage, baseline → candidate | Fusions, baseline → candidate | Automated combat/travel minutes, baseline → candidate |
| --- | ---: | ---: | ---: | ---: |
| unlocked / 4319 / pinball | 97.2 → 294.6 | 8 → 10 | 5 → 5 | 19.77 → 19.03 |
| unlocked / 4319 / sever | 307.6 → 356.0 | 28 → 12 | 2 → 4 | 19.33 → 19.46 |
| unlocked / 4319 / gun | 280.0 → 275.2 | 40 → 11 | 4 → 2 | 18.40 → 18.73 |
| fresh / 1741 / pinball | 253.5 → 178.9 | 11 → 5 | 6 → 7 | 19.63 → 19.73 |

All eight completed all 21 encounters with valid body/reward accounting and zero order errors or visible spawns. The new 4319 pinball route exchanged spare gear for a Rare Fault Line rig. Fresh 1741 acquired Rare Reaping Cut at auction and fused to Epic through a later ordinary draft. Its late work order then awarded an additional Epic copy to storage, not another upgrade. That run finished with less integrity and salvage than its baseline. Events can change the build without guaranteeing a stronger run. Per-case manifests, equipment/draft histories and transaction receipts are committed beside `journey-comparison.json`.

This is four matched pairs, not a population difficulty estimate. PhysX can vary with the same seed. Automated combat/travel time excludes human drafting, reading and decisions; no 30-minute human run has been established.

## Regression and visual coverage

- Initial core: all 15 standalone contract scenarios passed with the existing 5,000-health override. These establish action/body/delay contracts, not difficulty.
- Progression/timing/save checks: 137 assertions.
- Legacy event/itinerary checks: 361 assertions plus 761 production mechanic assertions.
- Office/constellation/chapter checks: 232 assertions plus 761 production mechanic assertions.
- Loadout projection checks: 312 assertions.
- Durability/live stats: 53 assertions.
- Comparison event rendered suite: 1,658 assertions, including production mouse choices, both replacement outcomes, exact spending, safe exits, complete graph round trips, schema-11 migration, current-schema rejection, unlock filters, historical contribution identity, paired/2H/fallback fusion and native road choice.
- Independent-process resume: four assertions, including the surrendered item's persistent contribution identity and rejection of duplicate exchange input.

Rendered captures were inspected at 1080p, reduced-motion 720p and 2560×1080. Selection terms, two-prize cards, normal/paired/2H fusion, empty storage, outcomes and new road symbols fit the existing interface. Each sign commits the selected separate physical road. Earlier regression manifests are retained so checks are not falsely attributed to a later binary.

## Final verified build

Final 0.8.7 assembly: `C5F7C388EEED9E80D2EA2C521E4DD54A51E7E677AF472C9D78F75D0C076C3B42`.

The final rendered suite passed 1,659 assertions, including the corrected archive overlay fixture; the two new archive entries were visually inspected. Independent-process resume passed all four assertions on the same binary. Selected final images, `final-report.json`, `final-manifest.json` and `final-resume-*` retain that evidence. No gameplay source changed after the ordinary-journey/economy comparison: later differences are the eight-record menu count and archive-capture fixture.

The final 15 core scenarios passed on `8FEB2C1A875EB825EF2E9ED9E5C934D1B79B48D7DDC2B8FC4A7D98D876994794`, immediately before the archive-review-only correction. `final-core-summary.json` and its manifest retain the results and the explicit 5,000-health contract-test override. Repeating that entire batch for a fixture-only change would not add gameplay evidence.

Use the existing desktop launcher and the validated installation helper with the final assembly hash. Profiles, full raw working reports and original logs remain local; curated reports and selected screenshots are committed. Main is not merged and no release is published by this change.
