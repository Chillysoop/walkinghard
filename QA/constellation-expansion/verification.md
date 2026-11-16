# Constellation expansion · 0.9.0

Implementation and verification, November 16, 2026. Feature branch: `feature/constellation-expansion`, based on `feature/combat-feel` at `bfa7e65039bf8423620d7026f5f501967d93f545`. This is a local feature build and draft PR, not a main release.

## Player-visible scope

Six original stars remain unchanged in Foundations. Thirty new identities form six five-star branches: Impact, Storm, Sever, Ballistics, Siphon and Guard. The new content comprises six weapons, six abilities, fourteen passive equipment pieces and four ultimates. Catalog totals are 77 non-module items plus the existing 17 augments. Every new identity has a production combat effect, its own name/icon, short summary, tradeoff and rarity detail.

Each new branch costs 4 + 6 + 6 + 8 + 10 = 34 Sparks. The original six cost 24, so all 36 cost 228. At the unchanged 30-Spark reward for a victorious journey, that represents 7.6 successful journeys' earnings; defeated journeys also contribute. This arithmetic is not human pacing evidence. There are no permanent combat stats, new currencies or reward-count increases.

The Office has seven branch pages, connected prerequisites, short card effects, pinned selection/hover preview, scrollable detail, rarity preview and an explicit purchase button. New purchases affect future journey pools. Old ownership, wallet, seal, workers, victories and frozen checkpoint pools remain intact. Branch-colored etched glyphs use native drawing, without generated raster assets. New weapon variants reuse family geometry with separate identity rails, preserving colliders and held-weapon behavior.

## Provenance

| Assembly | SHA-256 | Scope |
| --- | --- | --- |
| Initial | `9E3FB954E411FA4B82BA7F03E2B39133232E70C85A4ABAB2BC18132CE8AECEA2` | First controlled effect review; retained failing radius fixture |
| r2 | `97ACB83AA2A60E3DA2C2B8E88F728FC2F14CF55DC4C7DFBF580A9BC84D79681D` | 444 headless / 460 rendered assertions; initial UI captures |
| Pre-r4 | `BB797DD4B9EEB9EC27CEEBF00D4E3A0F95BF981836B1944FC332AD9870497EC3` | Seven seed-1741 unlocked-pool exploratory journeys; initial old-core assertion failure |
| r4 | `28D8FBBC9038BA90902D22B6F5EED8FC66C49CD38C00654A5A098553EDF325E5` | Core 15, fixed-layout variety 50, pool comparisons, rendered 468 |
| r5 | `2C12900D74F909581FE8AD52E8829A58AFD142D9DCAB87B2E65B4855F179FC02` | Offer safeguards, lethal Freight Train fix, headless 449 / rendered 501, chapter persistence, matched offer comparisons |
| Final r6 | `0AB12FB987403B8C9BB2B9D4F0D56D2EE273F72E4D0CF45880F6BDEE6FE9336C` | Rendered 501 and full eight-wave regression; clearer rarity/Discharge copy, refreshed capture-fixture meshes and contact timing |

Final r6 preserves r5's production combat, offer, currency and save behavior. Its changes are tooltip wording and review-fixture presentation. The installed binary must match the r6 hash above; the existing install helper records its verified source commit in `%USERPROFILE%/Games/WalkingHard/current.json`.

## Controlled checks

`Validate Constellation.ps1` uses an isolated profile. It checks graph uniqueness, acyclic prerequisites, exact costs, atomic purchase/duplicate rejection, seal gates, schema-1 migration, frozen old-run pools and new-run snapshots. Four hundred seeds across five draft categories expose all 36 unlocks through actual reward generation. Another 240-seed audit checks unique ability choices, rarity bounds, continued new-ability discovery, linked mixed support and locked-pool exclusion.

All thirty items are acquired, placed through their normal slot rules and fused from three matching same-rarity copies. New weapon/ability hosts are compatible with existing augments. Common, Rare and Legendary controlled encounters invoke real production effects: damage, launch, marks and consumption, sever, reload healing, barrier creation/absorption/expiry, push, contribution ownership and normal global budgets. Focused narrowing, Repulsor Discharge, paired independent knuckle hosts, shared two-handed ownership, Grounding from Carbine discharge and single launch of lethal Freight Train targets have explicit checks.

The rendered run captures all seven pages at 1280×720, 1920×1080 and 2560×1080, with reduced motion at 720p. It invokes native item actions and verifies their actual equipped identity, contact delivery and sequence order. These fixtures establish operation and presentation, not complete-build difficulty. The fourteen passive effects also have direct production probes at three rarities.

Chapter compatibility on r5 passed 262 review assertions plus 804 existing production mechanic assertions. A separate process passed six resume assertions. Coverage includes failed disk writes retaining the wallet, corrupt-primary backup recovery, unreadable-profile protection, monotonic Spark receipts, unchanged old-star prices and checkpoint reward gates.

Combat-feel fixtures on r5 passed 71 assertions, including finite, non-silent, unclipped waveforms for all 31 audio cues and effect/audio bounds. This fixture report contains no journey frame samples. No new full-journey frame-rate claim is made.

The 15 core scenarios passed on both r4 and r5. All 50 fixed-loadout variety scenarios passed on r4, with 804 production-effect assertions per scenario. Those unchanged weapon/layout tests were not repeated after the bounded offer safeguard, new-ultimate correction and presentation-copy changes. The r5 full eight-wave regression passed with 13 item choices, two augment choices, 315 kills, 177 collisions and 804 mechanic assertions. These structural suites use 5,000 starting health to exercise complete scenarios and are not difficulty measurements. Ordinary-loot journey trials below use normal health.

Final r6 independently passed all 501 native constellation assertions and the full eight-wave regression with the same 315 kills, 177 collisions, 13 item choices, two augment choices and 804 mechanic assertions. Native inspection confirmed actual rifle/fist models after the fixture correction, readable conductive lanes, Siphon chains, guard rings and Freight Train's launch lane. All Office branches are captured at three resolutions; representative captures are retained in `captures/`.

## Ordinary-loot progression and reward access

Trials use normal health, Full PhysX, current 21-encounter journeys and real draft decisions. Policy names are preferences, not fixed loadouts. Reports retain offered identities, final equipment, weapon history, encounter stats and assembly manifests. Seeds reproduce reward inputs; PhysX outcomes can vary across executions. Defeat is a valid gameplay result.

The first expanded-pool seed (1741, before safeguards) produced seven valid defeats: pinball 6, storm 5, sever 19, gun 5, cutcarry 6, close 5 and siphon 6. These frequently began with Gravity Well, Suppressive Volley, Breach Salvo and a Rare Rail Rifle, then retained duplicate Gravity Wells. That exposed limited access to useful opening attacks; it did not prove each labeled archetype had been tested as a coherent build.

On r4, fresh-pool pinball and gun both completed all 21 encounters on seed 1741. Foundations-only pinball lost at 14 and gun at 6. This also shows that a change in offer composition can materially change a run; fresh/legacy results are contextual controls, not proof that the thirty additions alone caused every defeat.

The bounded response preserves the existing reward structure. If new patterns are available, an ability draft retains one eligible unowned Steel Jab, Breaker, Arc Bolt or Reaping Cut when its first three choices contain none. Two discovery choices remain. Mixed support prefers an explicit equipped producer/consumer relationship, retaining its copy bundle and wildcard. Fresh and Foundations-only pools retain former rules. Rarity odds, number of choices, fusion and unlock filtering are unchanged. `-OriginalPatternOffers` supplies a same-binary baseline.

The complete evidence includes **24 valid normal-health journey trials**, including the initial defeats. A second exploratory full-pool seed (4319, r4, before safeguards) completed for pinball, sever, cutcarry and siphon; storm and close lost at 19, gun at 6. This reinforces the importance of seed and actual equipment.

Same-binary seed-1741 comparison on r5:

| Preference | Original pattern offers | Curated pattern offers |
| --- | --- | --- |
| Pinball | Defeated at 5 | Defeated at 13 |
| Sever | Defeated at 6 | Completed 21 |
| Gun | Defeated at 6 | Defeated at 13 |

The curated opening exposed Steel Jab and Breaker, followed by Reaping Cut alongside two new abilities; the weapon choice still included Rare Rail Rifle, Common Pulse Pistol and Common Dead Letter Pistol. The pinball trial ultimately used three Breakers and a Common two-handed Impact Hammer; the gun trial ended with Steel Jab, Breaker, a Rare two-handed Rail Rifle and Epic Arc Bolt. The completed sever trial ended with Epic Breaker, Rare Steel Jab, Rare Kinetic Fists and Rare Reaping Cut, with **Blood Circuit contributing 72 kills, 8,476.9 damage and 245.7 healing**. Some acquired passives did little for the final equipment, demonstrating the limits of the automated drafter rather than proving those items intrinsically weak.

These three paired samples support retaining the access safeguard, not a statistical win-rate conclusion. Small automated samples cannot establish human win rate or thirty-minute pacing. The expansion still needs player feedback on branch prices, individual item strength and discovery frequency; there is no claim of equal archetype balance.

## Failures retained and corrected

1. Initial compilation rejected an implicitly typed array initializer followed by `Concat`. Explicit `new[]` fixed the declaration. The initial build log is retained.
2. The first controlled Static Shroud test put its second target at `(0.1, 3)`, outside a true three-metre radius. Moving that fixture to 2.7m fixed the assertion; production reach was not increased.
3. Initial native icon lines used rotation against the wrong GUI transform and escaped their tiles. Local matrix composition fixed them. Owned text `UNLOCKED` also matched an overly broad `StartsWith("UNLOCK")`; the availability test now matches `UNLOCK ·`.
4. Old core checks assumed every ability/ultimate damaged an enemy. Repulsor Guard and Last Bastion instead require actual barrier output; they retain the zero-damage tradeoff.
5. Review found a lethal Freight Train target could enter its corpse pass immediately and be launched again. The shared exclusion set now prevents that; a lethal-target regression checks exact launch count.
6. Early native ability captures attempted to replace a locked two-handed action. Revised fixtures select an ordinary ability slot and require successful placement and actual delivery. A further visual check caught stale preview weapon meshes after direct fixture equipment changes; the fixture now refreshes the same family meshes and identity rails as a real wave. These were capture-fixture defects, not changes to live equip rules.
7. The r4 full eight-wave fixed-kit run reached wave eight but exceeded the runner's 180-second wall-time limit under concurrent work. Its original log is retained. The separately recorded r5 and final-r6 confirmations both passed without relaxing the limit. Timing contention is plausible, but this does not retroactively turn the initial timeout into a pass.

Raw working batches remain under ignored `QA/v090-*` directories. Curated reports, manifests, failure evidence and representative native captures accompany this document. Shared failure logs are diagnostic excerpts; unabridged originals remain locally under ignored `Logs/`, without publishing unrelated host, network or licensing details. No player profile was funded or modified by review fixtures.
