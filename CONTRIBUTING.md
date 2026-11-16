# Contributing

Use Unity 6000.3.27f1 and keep `.meta` files with every asset. Create a feature branch from `main`, make the change, and open a pull request describing the player-visible behavior and verification. Coordinate changes to the scene, shared tuning asset, and project settings to avoid unnecessary merge conflicts.

Build with `Build Windows.ps1`, then run `Validate Prototype.ps1`. The fifteen standalone runs check loadouts, five-tier rarity/fusion, ordered pairs, manual ultimates, scoring and kill attribution, hidden entrances, cuts, cascades and ragdolls. Check the game visually when changing the HUD, art, animation, or entry layout. The launcher checks run in GitHub Actions; Unity builds require a licensed editor locally.

For build-variety changes, also run `Validate Variety.ps1` (50 scenarios) and `Validate Variety.ps1 -Builds full -FullRun`. These include production-effect probes, module ownership/fusion checks and matched-rarity weapon layouts. Native `-mechanicReview -reviewCapture` records 46 timed mechanic fixtures with screenshots and motion traces; inspect actual effects as well as the counters. Add `-reviewSharedAugments` for eleven host-specific shared-module and mote fixtures. Headless batches and tuning comparisons are documented in `SIMULATIONS.md`. Headless success does not prove visual readability.

Preserve the main contracts: strict slot order, no visible enemy creation, frozen checkpoint physics, stable body accounting, and bounded collision damage. A weapon or ability should declare what it creates and what it consumes; use the same labeled synergy icon wherever that effect appears.

Follow the current contract in DESIGN.md: five movable actions, two-handed grouping, independent ultimate controls and same-rarity three-copy fusion. Do not silently treat proposed balance values as confirmed design.

## Publish a Windows release

1. Build and validate the final source. Commit source changes and QA reports, then push that commit to main.
2. Run `Publish Release.ps1 -SkipBuild -Tag v0.6.2 -PlayerDirectory <tested player folder>` (choose a new version tag for later releases). Omit `-SkipBuild` if the tree is already clean and you want to rebuild first; commit any generated changes before retrying if needed.
3. The publisher uses the existing Git Credential Manager login, packages only the new player files, creates a draft release, uploads a ZIP and source-commit/checksum manifest, then publishes it. It will not overwrite existing release assets. Never add credentials to source.

The desktop launcher downloads a matching release; if source is ahead, it can compile main in a separate cache when Git and the matching Unity editor are installed. Offline or failed updates preserve the last working build. Merge and release deliberately: main is the version players receive.

## Deliver a local tested build

Keep one permanent playable installation at `%USERPROFILE%/Games/WalkingHard/Current`, and one desktop shortcut named **Walking Hard** that runs the updater. Reuse `Build/Windows` for build/validation staging. After validation, install with `Launcher/Install Build.ps1 -PlayerDirectory <tested player folder> -Commit <verified source commit> -AssemblySha256 <verified Assembly-CSharp.dll SHA-256> -RemoveSourcePlayer`. This verifies the copy, updates the existing shortcut, and removes staged player files while retaining QA captures and reports. Do not point the desktop shortcut directly at a development executable or retain a new named player directory after each iteration. Additional comparison players are temporary and should be removed after preserving their reports.

The updater installs only main commits that descend from the installed commit. Local feature builds therefore remain playable until main catches up; future validated local work must use the same install helper. Run `Launcher/Test Launcher.ps1` for launcher-only changes; no Unity rebuild is needed when the validated game binary is unchanged.

For item presentation changes, run the rendered `-vectorQA -qaBuild ui -qaHoldCheckpoint -presentationReview` fixture and inspect its checkpoint, rarity mismatch, fusion, augment, icon-key and combat captures. Keep the stable ID mapping in `ItemArtwork.cs` complete when adding items; save generation prompts with art sources.

For onboarding or drop-guidance changes, run `Validate Tutorial.ps1` and `Validate Tutorial.ps1 -Rendered` against the built player. The rendered mode briefly opens the game, drives its production mouse handlers, checks text metrics, and captures tutorial/ordinary checkpoints at several resolutions. Inspect the screenshots as well as the assertions; synthetic interaction checks are not a substitute for player feedback.

For checkpoint-workbench changes, also run `-loadoutChecks` (headless) and `-loadoutReview -force-d3d11 -screen-fullscreen 0` (rendered), with an isolated `-saveDirectory`. The review checks detached previews against production acquisition/movement, native drag/click handlers, exact empty-slot and Store destinations, chained fusion, keyboard/primary-action routing, socket ownership, and text fit at 720p, 1080p and ultrawide. Inspect the captures under the player folder’s `LoadoutReview` directory.

For full-journey progression or automated draft changes, use `Validate Progression.ps1` with fixed seeds and reward pools; `-Legacy` selects the previous draft policy. Check actual loadouts and pivots, not just policy labels. Run `-progressionChecks` against an isolated profile and inspect rendered recap captures when changing results presentation. Keep initial engine failures visible in the report even when a later confirmation succeeds. Do not rebuild a staging player while its batch runner can launch new processes.

For weapon-offer changes, use `-weaponDiscoveryChecks` for both-pool shadow audits and save/reroll migration. Compare `Validate Progression.ps1 -CommittedWeapons` with adaptive drafts; `-OriginalWeaponOffers` isolates the old offer rules in the same binary. Inspect per-encounter weapons/ranks and assembly before inferring balance. `-ragdollStress` adds bounded repeated launch/sever fixtures against walls and physical forks; retain any initial failure even if confirmation passes.

For district/boss changes, run `Validate Districts.ps1` in Mechanics, Rendered and Balance modes using separate output directories. Inspect the screenshots, including reduced-motion 720p, and use `Validate Progression.ps1` for ordinary-loot journeys. The fixed kits establish weapon access, not human difficulty or pacing. Preserve initial failures and compare manifests before attributing results to code changes.
