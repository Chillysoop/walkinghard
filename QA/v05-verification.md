# Walking Hard 0.5 verification

Validated October 28, 2026 with Unity 6000.3.27f1. Final Windows player: `Build/Release-v05-publish/WalkingHard.exe`.

Assembly-CSharp.dll SHA256: `42d23ba588d78e57f3c6a80e4e7e3998d0bb53c576ddd9b49c7eb463cdb3410b`.

## Standalone gameplay checks

All thirteen release-player scenarios passed. Each run completed three waves with exact kill attribution, zero sequence-order errors, zero visible spawns, finite physics, and every killed body still present. Settled bodies ignore walking capsules while accepting one-way shove impulses. The oldest retained bodies exceed the former seven-second cutoff.

| Scenario | Seed | Kills / retained bodies | Body impacts | Oldest body (combat s) | Result |
| --- | ---: | ---: | ---: | ---: | --- |
| armor | 1741 | 100 / 100 | 69 | 107.5 | Pass |
| fast | 1741 | 101 / 101 | 56 | 106.5 | Pass |
| half | 1741 | 101 / 101 | 65 | 106.5 | Pass |
| hammer | 1741 | 100 / 100 | 75 | 106.0 | Pass |
| manual | 1741 | 99 / 99 | 65 | 106.5 | Pass |
| quick | 1741 | 98 / 98 | 58 | 106.5 | Pass |
| reroll | 1741 | 98 / 98 | 65 | 107.5 | Pass |
| rifle | 1741 | 104 / 104 | 56 | 105.8 | Pass |
| sequence | 1741 | 100 / 100 | 65 | 106.5 | Pass |
| sequence | 4319 | 99 / 99 | 75 | 106.5 | Pass |
| sword | 1741 | 99 / 99 | 71 | 106.5 | Pass |
| tempest | 1741 | 102 / 102 | 65 | 106.5 | Pass |
| twin | 1741 | 104 / 104 | 42 | 106.5 | Pass |

Checks include invalid-drop atomicity, one-time claims, direct weapon equip, upgrade bundles choosing the fused result rather than the leftover spare, compatible armor mounts, rarity-scaled armor effects, stored gear losing its bonuses, 2H group moves, delayed kill attribution through fusion, score banking, and all four speed budgets. The quick scenario uses the one-step claim/equip/advance path through the whole run. Half and fast scenarios exercise the common frame-budget function at 0.5x and 4x. Pause and checkpoints reject simulation advancement at every speed.

The armor scenario equips controlled fixtures to exercise all three passives independently of reward luck. It verifies increased max integrity, stagger-bonus hits, prevented incoming damage and actual regeneration. Named weapon scenarios similarly use controlled fixtures for family coverage.

## Native Windows UI checks

- Confirmed the angular combat layout: controls at top left, ultimate below wave status at top right, five actions at the bottom edge, descriptions and live stats retained.
- Dragged a helmet reward directly onto the equipped helmet: it claimed once, fused three Common copies into Uncommon, showed 8% -> 9.6% damage and 6 -> 7.2 stagger bonus, and stayed open with Edit after pick enabled.
- Observed the three-copy convergence animation and retained Last fusion comparison button.
- Double-clicked a stored pistol: it equipped into the free weapon mount and the basic tile showed the pistol's zero kills rather than the fists' prior count.
- Turned Edit after pick off and dragged an Epic Mass Driver reward onto its sequence position: the next wave began immediately and the new item had its own kill count.
- Used Let's fight with editing enabled, then selected 4x during live combat.
- Verified full equipment hover details and dismissal when leaving the item; confirmed that inventory icons stay inside their cells with no stray transformed strokes.
- Captured the final checkpoint, hover, fusion and combat screenshots included here. The checkpoint screenshots use the explicit `-vectorQA -qaBuild ui -qaHoldCheckpoint` presentation fixture (armor and inventory seeded for inspection), not a guaranteed normal reward roll.

## Build and distribution checks

Windows build succeeded; the final build passed the thirteen-scenario validation script. Build, validation and publisher PowerShell syntax checks passed. Launcher checks passed valid install, checksum rejection, failed-build rollback, archive containment and offline fallback.

The UI uses the real procedural character model and code-drawn icons. Models, armor balance and long-run performance beyond this three-wave prototype remain development work. The reference image does not add level/XP progression or change pick-one-of-three into three rewards.
