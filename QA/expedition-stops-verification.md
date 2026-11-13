# Expedition stops verification

November 13, 2026. Stacked on the physical-fork implementation in `0b09ff3f3a5a1f8bd0d4fe9fc122774a1a426e0d`.

## Delivered

New Expedition and Resume sit alongside Quick Run. The first physical fork can lead to Overtime; the second can lead to Iven's salvage bench. Stops happen at the next reward checkpoint and retain the normal thirteen item choices, two augment choices and Pursuer finale.

Overtime has three lore variants with per-run odds, payout, starting haul and damage. Every attempt shows its exact probability and stakes, then pauses on the actual roll. Players can bank immediately, inspect the next risk without rolling, or push for up to four attempts. Failure loses only the stated unbanked haul and integrity. The recovered story persists even after failure. Iven sells one repair and one matching copy; the latter uses ordinary fusion and retains the equipped host's augment. The service shows a fusion prediction and keeps purchase/repair receipts visible together.

Checkpoint files preserve offers, spent rerolls, event rolls, purchases, shared item ownership and credits, enemy identities, body and bone poses, severed joints, velocities and queued forces, plus both physical road choices. Saving uses an atomic replacement and keeps a previous-file backup. Unsupported/corrupt files are rejected; failed load attempts do not consume them. Quick Run preserves the stored expedition. Victory/defeat clears the run checkpoint; records remain. Quitting in combat resumes the last reward checkpoint rather than the exact combat frame.

## Verification

- The final native review passed **659 assertions**, including **600 comparisons with the existing random generator**, plus **761 production-effect assertions**. Checks cover varying situations, pre-roll/reveal reloads, success/failure/cashout, the attempt cap, lethal-risk prevention, one-time purchases, fusion/module ownership, corrupt-file rejection, safe-mode separation and complete model-graph round trips.
- A fresh player process passed **five additional assertions**, restoring the service checkpoint, spent purchases, two selected roads, a severed body and a shared flight/collision budget from disk.
- The normal-health eight-wave Expedition passed **30 assertions**, completed with **318 kills and 218.3 integrity**, visited both stops and restored every reward checkpoint. Ordinary offers and automated pinball preferences retained all thirteen item picks, two augment picks, hidden entries, strict sequence order and enemy/body accounting. [Final report](expedition-stops/full-run/report.json). This is one functional run, not a balance sample.
- All **15 core scenarios, 50 variety scenarios and the eight-wave progression scenario** passed; see [regressions](expedition-stops/regressions.json). Those fixtures use 5,000 starting health and are not normal-difficulty results. The variety suite was run before the final receipt and locked-sign clarity changes; its combat code is identical. The final player has separate native, core and full-progression verification.
- Tutorial model checks passed **174 assertions**. The rendered tutorial passed **1,375 assertions**, driving production mouse handlers, text-fit checks and captures at 1280, 1920 and 2560 widths. These invoke native game handlers rather than operating-system mouse injection.
- Event odds, result, next-risk, salvage/fusion, archive and menu captures were inspected at 1920×1080; the service was also inspected at 1280×720. Inactive service labels remain readable. Existing tutorial/checkpoint captures were inspected after the menu change.

Evidence: [native report](expedition-stops/native/report.json), [fresh-process report](expedition-stops/fresh-process/report.json), [source/binary provenance](expedition-stops/provenance.json). Native event captures are in `QA/expedition-stops/native`; tutorial captures remain alongside the local tested player in `Build/Expedition-stops-final/TutorialReview`.

The deliberate malformed-JSON test emits one `CHECKPOINT_LOAD` warning. There are no unexpected save/load failures in the reviewed runs. Tests use isolated profiles under `Build/Expedition-evidence`; they do not modify the player's real save or archive.

## Reproduce

Build with Unity 6000.3.27f1 using `Build Windows.ps1 -OutputDirectory <player-directory>`. Run `Validate Prototype.ps1`, `Validate Variety.ps1`, `Validate Variety.ps1 -Builds full -FullRun`, and both modes of `Validate Tutorial.ps1` against the selected player. The core/variety functional runs above use `-HeroHealth 5000`.

For rendered Expedition fixtures, launch the player with `-expeditionReview -seed 1741 -reviewOutput <fresh-output> -saveDirectory <isolated-profile> -logFile <log>`. Use a fresh profile, never the live player profile. After that process exits successfully, launch again with `-expeditionReview -resumeOnly` and the same isolated profile but a different review output. For the ordinary-health progression check use `-expeditionReview -expeditionFullRun -seed 1741` with a separate profile and output. Reviews require a rendered Windows player and terminate themselves after writing their reports.

## Limits

This delivers one event loop with three story variants, one service stop and safe-checkpoint persistence within the existing eight waves. It does not deliver the longer act graph, hub, Foreman or the other proposed events. Event probabilities and prices are initial tuning. Familiar rules can still be learned; varying situations remove a guaranteed memorized payout, not the value of understanding probabilities.

Persistence has been verified in the Windows Mono player. PhysX's internal contact/solver history is not serialized, so cross-session motion is not promised bit-identical. Save format `expedition-stops-1` is explicitly versioned; future schema changes will require migration or a clear unsupported-save message. The build is prepared for review, not published as a release.
