# Cinderworks preview verification

November 13, 2026. First playable regional art milestone on top of combat-flow commit `d114e5f09a97116920c2960e2a3b57c4dd69f072`. This is an unreleased development preview, not the three-act expansion.

## Player visible changes

The title screen exposes **Cinderworks Preview**. Seven existing practice encounters demonstrate fists, paired swords, a greatsword, a hammer, mixed close-range weapons, a rifle and the Pursuer. V compares Cinderworks against the current armored presentation using the same practice setup. R restarts and F queues the real ultimate. Exit Preview restores the original environment and normal run flow.

Furnace bays, freight rails, pressure chambers, ceramic trim and low service ducts establish the regional setting. Workers wear respirators and heat aprons. Heavy enemies have broad brows and solid counterweights; volatile carriers retain distinct illuminated cylindrical tanks. These are presentations of existing enemy roles, with unchanged statistics and attack rules.

Decorative geometry is combined and cached by material within each repeated bay or existing severable bone. The native review records 258 regional environment renderers. No new live physics bodies, world colliders, enemy colliders, lights or continuously updating scenery behaviors are introduced. New primitives have their temporary colliders disabled immediately and removed before the review checks the hierarchy.

Visual review caught cross-lane beams blocking the boss and high side rails intersecting the camera orbit. The final design removes overhead crossbeams and lowers service structures. During the reveal, preview controls occupy the letterbox, leaving the Walker and Pursuer visible.

## Verification

- The native regional review passed **181 assertions**, exercising all seven presets in both looks, actual ultimates, persistent-body accounting, hidden entrances, paired/two-handed weapons, comparison and restoring normal play.
- Production combat checks passed **755 assertions in each look**, for **1,510** in the regional review. They include the existing combat-flow, augment, weapon and effect contracts.
- All **507 existing world colliders** retained the same object identities, positions and bounds after entering and exiting the region. The additional scenery has no colliders; hero and living enemy physics counts remain unchanged; skeletons retain eleven severable bones.
- The full eight-wave regression completed with **318 kills, 147 collisions, 13 item choices, two augment choices, zero visible spawns and zero sequence errors**. This uses 5,000 health for coverage and is not a difficulty estimate.
- All **fifteen standard core runs passed**, with results recorded in [regressions.json](cinderworks/regressions.json). Native cases and samples are in [report.json](cinderworks/report.json); source and binary hashes are in [provenance.json](cinderworks/provenance.json).

Native captures were inspected for the title entry, original/regional formation, hammer and rifle combat, ultimate preparation, crowded reduced-motion combat, the middle and end of the Pursuer camera turn, and the restored ordinary draft. The shipped screenshot set is under [cinderworks](cinderworks). These checks establish rendered behavior; fresh-player comprehension remains a playtest question.

## Performance and limits

Sequential 1920x1080 samples used a 60 FPS cap and three-second windows. Both current and regional looks measured approximately **16.67 ms median and 16.69 ms p95** at 0.5x, 1x and 4x. The samples contained 15–19 live enemies and 29–43 retained bodies. The 4x samples used reduced motion. No regression workers ran during these measurements.

These are short practice samples, not a maximum-crowd or full three-act performance claim. The existing late-run body-physics limitation remains. Full-physics trajectories also vary between runs: the report retains actual kill, sever and collision counts rather than claiming bit-identical crowd outcomes. The art changes do not edit combat rules, tuning, physical shapes or timing.

The preview uses the current hero, weapon and effect library. It does not add new enemy behaviors, loot identities, branching acts, save/resume, a hub or campaign progression. Those remain the next stages in [the expansion design](../EXPANSION_DESIGN.md). No release or launcher update was published.

## Reproduction

Build with `Build Windows.ps1 -OutputDirectory <preview directory>` using Unity 6000.3.27f1. Launch the resulting player with `-cinderworksPreview` for manual play, or `-cinderworksReview -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -reviewOutput <output directory>` for the rendered review. The latter briefly opens a native window and exits after writing its report and captures.

Run `Validate Prototype.ps1 -Player <player> -ReportPrefix cinderworks-core -HeroHealth 5000` and `Validate Variety.ps1 -Player <player> -ReportPrefix cinderworks-progression -Builds full -FullRun -HeroHealth 5000` for regressions. Run these separately from frame-time measurement. Local intermediate art captures and individual regression reports are retained under ignored `Build/Cinderworks-evidence`.

Final player: `Build/Cinderworks-preview/WalkingHard.exe`. Assembly-CSharp SHA256: `d59521ee029b2548784e46f4a3d57de5bbae03984d89ad41503ebf8be4b61d43`.
