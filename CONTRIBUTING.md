# Contributing

Use Unity 6000.3.27f1 and keep `.meta` files with every asset. Create a feature branch from `main`, make the change, and open a pull request describing the player-visible behavior and verification. Coordinate changes to the scene, shared tuning asset, and project settings to avoid unnecessary merge conflicts.

Build with `Build Windows.ps1`, then run `Validate Prototype.ps1`. The fifteen standalone runs check loadouts, five-tier rarity/fusion, ordered pairs, manual ultimates, scoring and kill attribution, hidden entrances, cuts, cascades and ragdolls. Check the game visually when changing the HUD, art, animation, or entry layout. The launcher checks run in GitHub Actions; Unity builds require a licensed editor locally.

For build-variety changes, also run `Validate Variety.ps1` (45 scenarios) and `Validate Variety.ps1 -Builds full -FullRun`. These include production-effect probes, module ownership/fusion checks and matched-rarity weapon layouts. Native `-mechanicReview -reviewCapture` records 45 timed mechanic fixtures with screenshots and motion traces; inspect actual effects as well as the counters. Headless batches and tuning comparisons are documented in `SIMULATIONS.md`. Headless success does not prove visual readability.

Preserve the main contracts: strict slot order, no visible enemy creation, frozen checkpoint physics, stable body accounting, and bounded collision damage. A weapon or ability should declare what it creates and what it consumes; use the same labeled synergy icon wherever that effect appears.

Follow the current contract in DESIGN.md: five movable actions, two-handed grouping, independent ultimate controls and same-rarity three-copy fusion. Do not silently treat proposed balance values as confirmed design.

## Publish a Windows release

1. Build and validate the final source. Commit source changes and QA reports, then push that commit to main.
2. Run `Publish Release.ps1 -SkipBuild -Tag v0.6.1 -PlayerDirectory <tested player folder>` (choose a new version tag for later releases). Omit `-SkipBuild` if the tree is already clean and you want to rebuild first; commit any generated changes before retrying if needed.
3. The publisher uses the existing Git Credential Manager login, packages only the new player files, creates a draft release, uploads a ZIP and source-commit/checksum manifest, then publishes it. It will not overwrite existing release assets. Never add credentials to source.

The desktop launcher downloads a matching release; if source is ahead, it can compile main in a separate cache when Git and the matching Unity editor are installed. Offline or failed updates preserve the last working build. Merge and release deliberately: main is the version players receive.
