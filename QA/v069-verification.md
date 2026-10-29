# Walking Hard 0.6.9 — verification

29 October 2026. Final player: `Build/Feedback-v069-final/WalkingHard.exe`, Unity 6000.3.27f1.

Assembly-CSharp.dll SHA-256: `fc03cc19029f29f699def21998f52a9554a96cb7e9ff7573a8a13eb11e7e6a29`.

## Behavior

Checkpoint headers show the finished wave's actual damage, healing, prevented damage and collision kills. Wave details shows per-item kills, damage (with collision subtotal), healing, effect blocks, control, setup assists, reload time, target wait and readying time. It identifies the longest target wait and lists recorded synergy payoffs. The opening draft retains fusion instructions until a wave has completed.

Reports snapshot before drafting; equipping, storage and fusion cannot rewrite them. The details panel blocks underlying loadout input and closes with its button or Escape. It does not change the edit-after-pick preference, required drafts, sequence order, or combat timing. New runs clear the report.

Recovery Treads now records repair-mote restoration as healing rather than blocked damage. Fusion preserves reload, readying and target-wait totals along with the existing contribution fields. Additional notes expose Grounding, Duelist, Salvage Magnet, Fault Line, Momentum, Predator, Loader and repair-mote triggers. These are reporting changes, with no effect changes or balance adjustments.

## Checks

- Final player: all fifteen core encounter fixtures, the complete eight-wave fixture and the four-wave ultimate-progression fixture pass, each with 397 production assertions. The fixtures use 5,000 health for coverage and are not balance evidence.
- New assertions verify real repair-mote healing attribution, no opening-wave report, wave-local deltas, separate delays, trigger deltas, immutable snapshots after storage/fusion, preservation of delay totals through fusion, next-wave isolation and run reset.
- 64 ordinary eight-wave Fast games match v0.6.8 gameplay fields and outcomes exactly. Version/wall time are excluded. The three repaired fusion-delay fields are excluded from equality; the Recovery Treads heal/block split is checked as an unchanged sum. All other item values and wave reports match. See `Balance/v069/regression.json`.
- Launcher tests pass valid install, checksum rejection, failed-build rollback, archive containment and offline fallback.

## Native visual inspection

The hidden-window fixture produced black images. The user explicitly approved briefly opening the game for this check. The game's native screenshot routine then captured the final build at 1600 × 900; no Computer Use automation was used.

Inspected [checkpoint header](v069-checkpoint.png) and [wave details](v069-wave-details.png). The summary fits alongside checkpoint controls; the table separates names, rarity, kills, damage, healing, blocks, control and delays. Background content is dimmed behind the details panel. The first-wave capture correctly has no healing/blocks or tracked synergy payoffs; those mechanics are covered by the production probes. Snapshot data: `v069-feedback-snapshot.json`.

## Balance study

2,184 Fast controlled games plus six Full physics cases use normal health, fixed loadouts and matching weapon rarities. Primary batches were followed by a smaller Rare-support slice after the Common-support weapon matrix produced no wins. Every case is valid with no timeouts. These cases use the earlier feedback assembly `4e86c171281a1ce033da5bc3a8c64a1e8bd255030aa7f76f9740e1a88f9c1bb7`; later changes add reporting notes and fix the repair-mote metric. The final 64-game regression verifies unchanged gameplay. The controlled fixtures do not equip Recovery Treads.

Results, proposed rifle/Duelist tests and limitations are in [v069-balance-review.md](v069-balance-review.md). The proposed balance changes are not implemented. Fast contact approximations and six Full examples do not establish population win rates. The scope of this release is checkpoint feedback and the focused comparison; encounter branching and new enemies remain later proposals.
