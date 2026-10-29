# Walking Hard 0.5.2 verification

Validated October 28, 2026 with Unity 6000.3.27f1. Windows player: `Build/Release-v052/WalkingHard.exe`.

Assembly-CSharp.dll SHA256: `25e43c4e13b742a53cdca36236df504e557a27670e3884b941886135822aed55`.

## Delay presentation checks

All fifteen standalone scenarios passed. During preparation the actual active tile(s) expose a delay reason, remaining simulation seconds and bounded progress. Inactive tiles and completed delays expose no delay bar. Attack progress remains at zero during preparation. The normal attack progress resumes after preparation. The existing gameplay, rarity/fusion, weapon persistence, physics and attribution assertions also passed.

| Scenario | Reload indicator frames | Readying frames | Paired-wait frames | Result |
| --- | ---: | ---: | ---: | --- |
| armed / 1741 | 72 | 384 | 0 | Pass |
| armor / 1741 | 0 | 224 | 0 | Pass |
| fast / 1741 | 0 | 273 | 0 | Pass |
| half / 1741 | 0 | 320 | 0 | Pass |
| hammer / 1741 | 0 | 480 | 0 | Pass |
| manual / 1741 | 0 | 256 | 0 | Pass |
| quick / 1741 | 0 | 272 | 0 | Pass |
| reload / 1741 | 216 | 384 | 144 | Pass |
| reroll / 1741 | 0 | 272 | 0 | Pass |
| rifle / 1741 | 432 | 464 | 0 | Pass |
| sequence / 1741 | 0 | 304 | 0 | Pass |
| sequence / 4319 | 0 | 288 | 0 | Pass |
| sword / 1741 | 0 | 256 | 0 | Pass |
| tempest / 1741 | 0 | 368 | 0 | Pass |
| twin / 1741 | 288 | 544 | 0 | Pass |

The new reload scenario equips two pistols and seeds their magazines to 0 and 3 rounds. This tests the loaded pistol displaying WAITING FOR PAIR while only the empty pistol displays RELOADING. The rifle scenario checks the two-handed group. Other scenarios cover normal resets, later turns, one-handed weapons, speed, checkpoints and ultimate scheduling. Rarity, ammo, action duration, reload duration and preparation duration are unchanged.

## Native Windows UI inspection

At checkpoint one, resumed the paired-pistol fixture at 0.5x speed. Observed both amber tile bars with RELOADING 0.7s on the empty pistol and WAITING FOR PAIR 0.7s on the loaded pistol. The screenshot in `v052-reloading.png` preserves this observation. Descriptions, stats, rarity and kill counters remain visible; normal attack progress is suppressed during the preparation bar. An unrelated Arc Bolt hover card is visible on the right of the screenshot.

Computer Use was subsequently stopped by the user with Escape. No further Computer Use calls were made; the previously observed screenshot had already been saved. The game window was left alone.

## Scope

The release changes delay presentation only. DESIGN.md also contains an audit and proposed 40-item content round, both hand counts for swords/hammers, signature effects and rarity scaling, archetype coverage and reward pacing. Those proposed content and reward changes are not implemented in this player.
