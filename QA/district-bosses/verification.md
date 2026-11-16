# Walking Hard 0.8.4 — district and boss verification

## Scope

Three district rosters and arrival rhythms, district scenery, Shift Foreman double presses, Bell Foreman linked commands, and the Pursuer's committed sweep/recovery. Item stats, weapon/pair budgets, boss health, reward counts and physical road walls are unchanged. This is a local feature build stacked on `feature/weapon-discovery`; it is not a published release.

## Provenance and initial findings

- Pilot assembly `E2EBB4ED3EA04511898FFFA2911576AA98D1A46890A3777160E02E3F784B02D3`: 673 mechanics assertions, rendered fixtures, 24 fixed-kit boss fights, and six ordinary-loot journeys. Four journeys completed; gun seed 1741 died in encounter 6 and sever seed 1741 in encounter 19. All six passed progression/accounting invariants. The completed pilots took 18.7–19.9 minutes of automated combat and movement, excluding human decisions.
- Some pilot captures were obscured by a real mouse hover tooltip. The final fixture uses the existing synthetic review pointer. An escort label covered the Pursuer's head; threat-label placement now reserves the boss silhouette and tries additional side positions. Final 1080p/720p images were inspected.
- `mechanics-final` failed its new moving-target rear-door check on assembly `10245FF791E12F8E583A4C4FE9A694289E8288C72FFDCF6BACBD8C53598A710A`. `rear-diagnostic` preserved the same failure: after 12 simulated seconds, the actor was alive, visible and unobstructed, but still circling forward from 2.22m behind the Walker. It was not trapped or deleted. The original selector admitted exits up to 7m behind the Walker and prioritized a 3m lead in its reversed direction.
- Rear selection now searches directly for safe covered rooms with exits less than 3m behind the Walker. Six full-physics fixtures cover both sides on straight/left/right roads while the Walker moves at its full unaided speed. They require the arrival to become eligible to attack, with no visible spawn or rear-boundary deletion; extending the test deadline alone is not the production fix.
- Assembly `5169C26FCE65696FCF9BB598818FAEA130DD4A81EC3D710A583B657C5A83FB07` contains that rear-door correction and the final HUD/art. It passed 691 mechanics assertions, the rendered equivalent, 24 fixed-kit boss encounters, the 15 core scenarios, 136 journey checks, 27 legacy Foreman checks, and 105,558 discovery/fork/save assertions. `journeys-final` and `fresh-final` retain ordinary-loot runs from this assembly.
- A final source review found that launching a defeated Bell Foreman could still add disruption to its old command. The final guard rejects dead actors, with a regression fixture that launches a corpse at 98% disruption and verifies no false opening. This guard does not alter live boss attack timings, damage or phase thresholds.
- That new fixture initially failed compilation because its `Launch` arguments were in the wrong order (CS1503). The call was corrected to the existing signature; no player was produced by the failed build. Its full log is retained locally as `corpse-fixture-compile.log`. The first final mechanics invocation also used a stale shell exit-code check after the script had reported success; the JSON passed with 692 assertions. Rendered mode then ran separately and passed all 692.

## Ordinary-loot journeys after the rear-door correction

All four runs below use assembly `5169C26F…83FB07`, normal health, full PhysX and actual draft opportunities. Its only subsequent gameplay change is the defeated-conductor guard described above.

| Seed | Draft preference | Unlock pool | Result | Combat + movement | End integrity |
|---|---|---|---|---|---|
| 4319 | Gun | Unlocked | Complete, encounter 21 | 18.66 min | 280.00 |
| 4319 | Pinball | Unlocked | Defeated, encounter 20 | 18.01 min | 0 |
| 4319 | Sever | Unlocked | Complete, encounter 21 | 19.25 min | 310.00 |
| 1741 | Pinball | Fresh | Complete, encounter 21 | 20.15 min | 242.74 |

All four passed order, concealed-spawn, body/kill identity and progression checks. The three completed runs retained the full 21-encounter completion contract. Every Bell Foreman encountered here was defeated before completing a rally; the three completed Pursuer fights recorded 3, 4 and 11 attempted sweeps. This reinforces the need for human difficulty review, especially whether the conductor is too easy to suppress. The unlocked pinball seed completed in the earlier pilot but died after the arrival correction; one outcome change cannot isolate balance from the changed arrivals and PhysX variation.

## Installed-build validation

Final assembly: `29CD2A8F1510E3E1387820696C6EA567245E2CC64264A9D5F7303AF62010BD38`.

The `release-mechanics` and `release-rendered` runs each passed 692 assertions, including the defeated-boss regression. Final native Bell Foreman and reduced-motion Pursuer images were inspected again; the HUD/art matches the previously reviewed `rendered-final` captures. The directories named `release-*` identify the locally installable test candidate, not a public release.

The same final assembly passed all 24 `release-balance` boss encounters (eight wins per boss) and all fifteen `release-core` scenarios. Final fixed-kit clear ranges were 27.9–39.6s, 34.4–63.4s, and 46.3–127.6s respectively. Core scenarios use the documented 5,000-health QA override to reach every contract and are not difficulty evidence. All workers finished before installation or staging cleanup. Build version is 0.8.4; per-directory manifests identify the exact binaries rather than treating all iterations as one result.

## Boss access and tradeoffs

Fixed kits use full PhysX, normal health, Rare weapons, Uncommon supports/ultimate, automatic actions/ultimates and no augments. They retain actual boss approaches and reinforcement budgets. Eight kits cover fists, paired swords, greatsword, hammer, rifle, paired pistols, storm and sustain. All 24 fights on the corrected-arrival assembly defeated their boss without ordering, visible-spawn or body-accounting failures. Observed clear times were 27.9–39.6s for Shift Foreman, 34.4–63.4s for Bell Foreman, and 46.3–127.7s for the Pursuer. These are equipped fixtures, not natural-loot win rates.

Strong kits frequently disrupted the Bell Foreman: 39 broken commands, two completed rallies and one attempted toll across eight fights. This is intentional counterplay, but its difficulty is provisional. The player can use damage, stagger or escort clearing; no specific item is required. Pursuer strikes completed despite small basic staggers, while real recovery windows remained usable by every tested kit. Attempted strikes include misses and protected hits; health damage is recorded separately.

## Visual review

The native `rendered-final` captures show the first/second press warning and open furnace; three lit command bells, escort foot rings and gold links; extinguished/silenced command lamps; the toll circle; and closed/open Pursuer shutters with distinct strike/recovery countdown bars. The 720p reduced-motion captures retain those state changes and labels. The views also show furnace trim, bronze foundry architecture and blue/ivory Dispatch stacks. Scenery adds no colliders; route selection remains cyan.

The screenshots are controlled mechanics fixtures with elevated health and staged actors, not representative difficulty or marketing footage. They do not replace a human readability/playtime review. No new trailer was made.

## Limits

Automated choices take no reading time. Combat/movement time plus guessed decision time must not be presented as a measured 30-minute run. Small seeded samples can identify broken progression and inaccessible bosses, but cannot establish population win rates or final balance. Long-run deaths remain valid observations. Old compatible checkpoints retain their saved itinerary and existing actor behavior, then create the new bosses at future encounters.
