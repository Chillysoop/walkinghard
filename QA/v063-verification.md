# Walking Hard 0.6.3 — visible effects, compact combat HUD, ultimate milestone

29 October 2026.

## Changes

- All 56 reward definitions have a short effect summary. Pick-three offers display it separately from stats, synergy tags and fusion progress. Checkpoint and combat sequences use the same summary source; basics resolve the actual weapon, and augmented hosts describe their modified action. Full rules remain in hover details.
- Empty armor, weapon, rig and ultimate mounts have no placeholder item artwork. Empty weapon mounts state that basics use fists. Sequence augment sockets moved to the top corner so they cannot cover the description line.
- Combat HUD scale is 80% of the previous size. Primary panels are 66% opaque and the outer sequence backing is 18% opaque. Checkpoint/pause/recap scale is unchanged.
- New decks contain no ultimate. All early equipment/discovery offers and rerolls exclude ultimates. Checkpoint three replaces its Mixed draft with three distinct Ultimate offers using normal rarity odds, followed by the existing Augment draft. Later reward pools unlock ultimate drops. First equip starts a complete cooldown; empty slots show the milestone with no timer or activation controls.
- No item power, enemy stats, rarity odds or fusion rules changed.

## Binaries and regression scope

The fifteen mechanics regressions used `Build/ReadableHUD-v063-final/WalkingHard.exe`, Assembly-CSharp.dll SHA-256 `8193764bf8b5691cc1a0601250837a91d42a5f0b0f4b659f286adecd4ae06240`. All passed: sequence seeds 1741/4319, sword, hammer, rifle, twin, tempest, manual, reroll, half, fast, armor, quick, armed, reload. Each ran the 66 production-effect probes and expanded loadout contract checks.

The final publish player is `Build/ReadableHUD-v063-publish/WalkingHard.exe`, Assembly-CSharp.dll SHA-256 `50d790ee0a3a09b69e9dc7067983ed0ef06ff1841473dd5b915cd9f699db9457`. Its only differences from the fifteen-test player are QA wave logging, a four-wave progression scenario, and removing stale screenshot files before native capture. Gameplay/HUD logic is identical.

The expanded contracts check 64 seeds across all early reward categories and rerolls, then verify distinct ultimate choices, ultimate-only rerolls, direct equip and rolled rarity. Startup decks must have no owned/equipped ultimate. The schedule retains 13 item choices and two augment choices across eight waves.

Regression scenarios use the explicit QA-only `-qaHealth 5000` option so a loss does not prevent late-system coverage. This option is ignored outside `-vectorQA` and is not present in the release launcher. Manual/armed/Tempest fixtures explicitly supply ultimates to exercise those mechanics in three-wave runs; the progression scenario uses the real unlock schedule.

A normal-health candidate run with seed 1741 lost in wave three, with no ultimate, and passed its runtime ownership/timing/physics checks. Its report is `v063-default-health-sequence-1741.json`. Removing the starting ultimate changes early difficulty; this update does not claim balance validation or silently compensate with buffs. An attempted eight-wave QA run hit the harness's 180-second wall-clock limit without a completion report. The targeted four-wave progression check covers the changed milestone instead.

The four-wave progression scenario **passed on the final publish player** (`v063-progression-1741.json`). Waves 1, 2 and 3 began with no ultimate and zero ultimate casts. Checkpoint three produced exactly one ultimate draft, directly equipped Redline with its full countdown, then retained its augment reward. Wave four cast Redline at 132.878 combat seconds, after its 132.060-second deadline. All 66 production-effect probes passed; the normal sequence retained its order. This is mechanics validation with 5,000 QA health, not a difficulty measurement.

## Native visual review

Rendered checkpoint, gear, augment, ultimate and combat views were inspected at 720p and 1080p. The final publish player repeats the 720p capture with the stale-file fix. The development fixture manually sets representative offers at checkpoint one to inspect categories; its images are not evidence of the real unlock wave. Runtime progression is checked separately.

- `v063-descriptions.png`: plain-language effect lines and empty equipment mounts.
- `v063-equipment.png`: weapon, armor and ultimate summaries.
- `v063-augments.png`: module effects, tradeoffs in existing hover details, host-modified sequence text.
- `v063-ultimate-draft.png`: three distinct ultimate cards.
- `v063-combat.png`: smaller translucent HUD, descriptions, stats and delay bars.

Launcher checks passed valid install, checksum rejection, failed-build rollback, archive containment and offline fallback.
