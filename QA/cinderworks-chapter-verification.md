# Cinderworks chapter and constellation

Development pass, November 14, 2026. Based on destination-variety commit `a3c4ee382b516f4a2fb33ae282c7f28c84e7f08d`.

New Expeditions begin at the Dead Letter Office and end after six encounters with the Shift Foreman. Chapter definitions own encounter identifiers, reward schedules, fork positions, completion and Spark payouts. Both moving physical forks remain; the five pre-boss checkpoints plus two opening drafts provide ten item choices and two augment choices. Quick Run and resumed older Expeditions retain eight waves and the Pursuer finale.

The Office contains Mara's completion-seal account, Iven's rescued-worker count and attendance lamps, and a six-node constellation. Three inner nodes cost three Sparks each; their outer successors cost five and require both their parent and the Foreman's seal. Equipment unlocks Conductor Wraps then Execution Greatsword; Passives unlocks Relay Coil then Salvage Magnet; Ultimates unlocks Harvest then Redline. These are existing item identities made discoverable through new chapter progression, with their original stats, signatures and artwork. Passive nodes unlock equippable gear, not permanent stat increases. The initial chapter pool retains three distinct ultimate choices.

Clearing each ordinary encounter banks one Spark; the Foreman banks four, for nine per completed chapter. Sparks survive defeat and are separate from in-run salvage. Unlocks affect future chapter reward pools and do not auto-equip items. A saved chapter keeps its original unlock snapshot even if the player buys another node before resuming. New unlocks cost 24 Sparks in total. Earning rate and prices are initial tuning, not validated retention or economy targets.

Schema 5 explicitly distinguishes chapters from legacy Expeditions. Office schema 1 uses a separate atomic profile with backup, a run identifier and monotonic per-run clearance/rescue receipts. Replaying an older checkpoint cannot repay currency, duplicate victories or count the same rescue again. Wallet changes commit to disk before the in-memory purchase succeeds. An unreadable primary attempts its backup; if both are unreadable, purchases and new chapters are blocked while files are preserved. These safeguards cover ordinary reload/crash workflows, not external save editing or simultaneous game processes.

## Verification

- [Final native review](cinderworks-chapter/native/report.json): **232 assertions plus 761 production-effect assertions**. Covers six-encounter definitions, all draft paths excluding locked identities, three initial ultimate choices, all six unlocked identities entering offers, production purchase pointer handling, every node charging once, prerequisites, currency/rescue/victory receipts, replaying older checkpoints, fixed unlock snapshots, defeat retention, disk-write rollback, backup recovery and fail-closed unreadable profiles.
- [Normal-health chapter](cinderworks-chapter/full-run/report.json): **18 assertions**, six encounters completed with **191 kills, 239.8 integrity and nine Sparks**. Both literal forks, ten ordinary item choices, two augment choices and the Foreman endpoint passed. Twelve drafting checkpoints restored the chapter snapshot. The result returned to the Office with the seal and wallet intact. One automated pinball run does not establish balance across builds.
- [Fresh-process resume](cinderworks-chapter/fresh-process/report.json): **six assertions**. Actual [schema-1](cinderworks-chapter/legacy-v1/report.json), [schema-2](cinderworks-chapter/legacy-v2/report.json), [schema-3](cinderworks-chapter/legacy-v3/report.json) and [schema-4](cinderworks-chapter/legacy-v4/report.json) copies passed **four assertions each**, including saving in the new format and restoring the exact graph again. This exposed and fixed Unity's empty-object reconstruction for absent legacy itinerary data.
- Legacy [events/saves](cinderworks-chapter/expedition/report.json): **659 assertions**; [destination variety](cinderworks-chapter/variety/report.json): **361**; [crews](cinderworks-chapter/crew/report.json): **37**; [Foreman](cinderworks-chapter/foreman/report.json): **27**. Each also passed 761 production-effect assertions. These suites explicitly keep their eight-wave fixtures.
- [All 66 core/catalog/progression cases](cinderworks-chapter/regressions.json) passed: **15 core scenarios**, **50 build-variety scenarios**, and the **eight-wave Quick Run** with 322 kills, 13 item choices and two augment choices. These are preset mechanical coverage, not ordinary-draft win-rate measurements. Existing item effects, paired attacks, two-handed groups and the three-copy fusion contract remain unchanged.
- Tutorial validation passed **174 model assertions and 1,375 rendered assertions**. Presentation review passed all 64 artwork mappings and captured the checkpoint, rarity/fusion, augment, icon key and combat screens. Office captures were inspected at 1080p and 720p; the main menu and normal-run result/return screens were inspected too.

Verification reports, final captures and binary provenance are retained under `QA/cinderworks-chapter/`. The native fixture uses isolated controlled profiles and deliberately tests an unwritable path and corrupt primary/backup files. Those warnings are expected. Normal-health chapter playthroughs use ordinary offers and automated pinball preferences; they establish functional completion, not comparative build balance or player comprehension.

The [provenance manifest](cinderworks-chapter/provenance.json) maps retained evidence to executable/assembly hashes, changed-source hashes and actual migration inputs. Core/catalog checks used `Chapter-tested`; the normal chapter and tutorial used `Cinderworks-Chapter`; final native, migration, legacy and presentation checks used `Cinderworks-Chapter-final`. Later changes after the core/catalog player affect chapter save handling and chapter UI, not Quick Run combat. The last change after the normal chapter was solely the pre-schema-4 itinerary migration fix. Original migration inputs and the player's profile were untouched. Build-generated scene object-ID churn was restored.

Reproduce with Unity 6000.3.27f1 and `Build Windows.ps1`. Native commands require an isolated profile:

```powershell
& '<player>/WalkingHard.exe' -chapterReview -reviewOutput '<captures>' -saveDirectory '<isolated-profile>' -logFile '<log>'
# Add -chapterFullRun for a normal-health six-encounter playthrough.
# Add -chapterResumeOnly and reuse a completed native fixture profile for a new-process check.
# Add -chapterLegacyResume with a copied schema-1/2/3/4 checkpoint for migration checks.
# -nographics skips screenshots.
```

The playable development build is `Build/Cinderworks-Chapter-final/WalkingHard.exe`. The live launcher is unchanged. The Office is a compact interface with NPC text, item artwork, connecting constellation paths and attendance lamps; a freely traversable hub, later districts and a multi-act journey remain future work.

