# Walking Hard 0.8.5 — journey pacing verification

## Scope and decision

Keep the Bell Foreman's universal damage counter, but require 18% of maximum HP instead of 9% to fill disruption through damage alone. Control still combines with damage; clearing every originally linked escort still breaks the command. There is no mandatory toll, immunity phase, required item, or additional resource. All telegraph/recovery durations and boss health are retained.

Ordinary reinforcement intervals use the existing pattern times 1.00 / 1.08 / 1.16 across the three districts. This reduces nominal later arrival rates by roughly 7.4% / 13.8%, without extending encounters, changing boss reinforcement budgets, removing survivors, or changing rewards. Preserve the new boss identities rather than changing Shift Foreman or Pursuer mechanics without evidence.

Add actual playtime categories to the existing recap and schema-10 checkpoints. Separate equipment and event/service decisions from combat/travel, paused and away time. Road choices and accelerated play are overlapping combat subsets. Save the latest finished journey locally as `journey-timing-latest.json`. Old saves explicitly mark timing partial; offline time and attempts discarded by checkpoint rollback are excluded. Automated reviews leave human timing unmeasured.

## Binary provenance

- Comparison assembly: `04BF53F1A7E3EB4129206BDD321967A87BA358FA7B1972372B3DEF0D0B1CFDA0`. The eight ordinary-loot runs and both 24-case boss cohorts below use this same binary. Baseline flags select the old damage threshold and arrival spacing; manifests and samples record both switches.
- Final assembly: `2B97210AF74DB1A9EEC8304EC3A7A616785F439761343CDF2038B9F3EB66A4F6`. Later changes close the combat pressure row before event costs, validate saved counter values, retain a local finished timing record, and add service/event regression checks. Live balance and timer classification are identical to the comparison build.
- A source-edit helper initially stopped on a Windows text-decoding error; the remaining edits were applied using explicit UTF-8 before the comparison build. No player test failed as a result. All native test results, including intermediate reports, are retained. Initial rendered captures remain local; selected final images and all reports are committed.

## Matched ordinary-loot journeys

Full PhysX, normal health, projected adaptive drafts, actual offers and route decisions. The preference label is not a fixed weapon loadout; inspect each sample's per-encounter equipment. PhysX trajectories can vary even with identical seeds. These are four matched pairs, not a population win-rate estimate.

| Pool / seed / preference | Baseline | Candidate | Combat + travel, baseline → candidate |
|---|---|---|---|
| Unlocked / 4319 / pinball | Defeated in 20 | Complete, 74.8 integrity | 18.25 → 19.59 min; baseline is censored by death |
| Unlocked / 4319 / sever | Complete | Complete, 310 integrity | 19.25 → 19.29 min |
| Unlocked / 4319 / gun | Complete | Complete, 280 integrity | 18.66 → 18.78 min |
| Fresh / 1741 / pinball | Complete | Complete, 263.7 integrity | 19.48 → 19.54 min |

All eight satisfy sequence order, concealed spawning, body/kill identity and progression checks. Each completion retains 21 encounters, 20 ordinary item picks and five augment picks. No comparison sample includes event costs in its combat pressure totals: these were cross-checked against the independent integrity encounter log before using the data.

Largest rolling five-second **ordinary-combat** damage burst (boss fights excluded):

| Pool / preference | Foundry baseline → candidate | Dispatch baseline → candidate |
|---|---|---|
| Unlocked / pinball | 21.1 → 12.2 | 29.4 → 24.3 |
| Unlocked / sever | 23.0 → 13.9 | 28.8 → 19.8 |
| Unlocked / gun | 11.1 → 6.0 | 13.8 → 8.6 |
| Fresh / pinball | 23.0 → 9.6 | 17.5 → 17.1 |

Foundry peak nearby enemies fell from 12/11/13/14 to 11/8/8/13 in the same row order. Dispatch peaks were 16/13/10/15 versus 13/13/10/15. Dispatch mean carryover improved only modestly for several builds: 9.3→9.2, 10.2→9.8, 9.5→6.2, and 11.5→10.7. The fatal baseline pinball encounter limits direct comparisons. Crowd accumulation is reduced, not eliminated.

Candidate ordinary arrival-only waiting is at most 2.9 seconds per later district across these samples, and actual arrivals remain plentiful: roughly 269–271 in Foundry and 356–357 in Dispatch. There is a reach-wait tradeoff: the gun preference's total target wait rises by about 18s in Foundry and 16s in Dispatch. Its Dispatch total damage also increases from 23.3 to 36.5 despite the smaller worst burst. Do not present the change as improving every metric or proving human difficulty is solved. Exact counts are retained in `comparison.json` and the source samples.

## Boss comparison

Eight fixed kits per boss: Rare weapons, Uncommon support/ultimate, no augments, normal health and full PhysX. All 24 cases in each cohort defeated their boss with no ordering, visible-spawn or identity failure. These authored kits demonstrate access, not ordinary-loot win rates.

Across the eight Bell fights, completed rallies increase **2→12**, attempted toll strikes **1→3**, and broken commands change **39→36**. The finishing contribution changes from damage/control/escort counts **22/5/12** to **12/7/17**. This identifies the contribution that crossed the threshold; earlier contributions can combine. Five candidate kits see a completed rally versus one baseline kit. Bell clears range 35.5–74.9 seconds in the candidate, versus 34.4–63.4 in baseline. Damage, physical control and escort clearing remain useful.

Strong ordinary-loot builds can still suppress every command. That is intended counterplay; this pass does not force an attack through a successful counter. Shift Foreman and Pursuer both remain accessible to all fixed kits. Unchanged Pursuer cases vary between cohorts, illustrating why identical seeds do not imply bit-identical PhysX trajectories.

## Timing and presentation

The native timing fixtures verify real-time partitioning without combat-speed scaling, overlapping roads, paused/away exclusion, ignoring invalid deltas, automation exclusion, reset, service classification, detached snapshot ownership, resume without offline time, schema-9 partial migration, invalid-save preservation, actual generated formation labels, five-second bursts, classified sequence waiting, and event/combat separation.

The isolated native UI check starts a real opening draft, saves to the Office, waits, resumes, then quits. Recorded equipment time advances **31.434→47.506 seconds**, matching the two draft intervals and excluding roughly one minute in the Office. The profile is isolated; this is a timer plumbing check, not a human full-run timing sample. See `live-clock` records.

Rendered recap fixtures at 1280×720, 1920×1080 and 2560×1080 use explicitly synthetic 19/7/3-minute values to verify layout. These are not observed player times. Labels and cards fit, the record scrolls, and footer buttons remain separate. Reduced-motion 720p Bell and Pursuer captures preserve readable phases and counter cues. The roughly 30-minute human target remains to be measured with actual build/event decisions; automated combat/travel alone remains about 19 minutes. No forced pauses or extra combat padding were added.

## Final checks

- `final-mechanics`: 713 assertions passed.
- `final-rendered`: 713 assertions passed; selected reduced-motion images retained.
- `final-progression`: 137 policy, recap, timing and save assertions passed.
- `final-journey`: unlocked seed 4319, pinball preference, completed all 21 encounters in 19.53 minutes of automated combat/travel with 67.3 integrity. All progression/identity/order checks passed; all 21 pressure rows match the integrity log, and human timing fields remain zero.
- `core`: all fifteen scenarios passed, including action delays, held weapons, direct equip, fusion, ordered pairs, manual ultimates, speed clocks, persistent bodies, attribution and concealed entries. These use the established 5,000-health QA override to test contracts, not difficulty.

All runners must finish before staging cleanup/installation. The desktop updater remains the sole normal play entry point. This is a tested local feature build, not a main merge or published release.
