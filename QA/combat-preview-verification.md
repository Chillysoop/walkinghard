# Playable combat presentation preview

November 2, 2026. Opt-in visual slice on top of v0.8.0; no global art rollout, item tuning, main merge or release publication.

## Playing the slice

Choose **Combat Preview** on the title screen or launch the player with `-combatPreview`. The local **Walking Hard Combat Preview** desktop shortcut opens it directly. Seven fixtures cover fists, paired swords, greatsword, two-handed hammer, fist/scattergun, rifle and the Pursuer. They use production damage, health, targeting, action order, covered-entry spawning and physics after a visible practice formation. Ultimates start ready for manual practice. Each fixture ends after one encounter.

- **V:** restart the same seed with the other look.
- **Left / right arrows:** select a fixture; the top buttons do the same.
- **F:** queue the ultimate; **U:** switch automatic/manual.
- **R:** restart; **Space / Esc:** pause. Exit through the preview controls or pause menu.
- Existing speed and reduced-motion controls remain available.

The original look remains the default for normal runs. Preview health includes the equipped armor bonus; no invulnerability, scripted kills or boosted damage are used. The title screen's new button does not change drafts or reward rules.

## Implemented presentation

Original chamfered armor meshes sit on the existing eleven severable bones. Light teal hero, rust raiders, broad sand-colored enforcers, framed shields, clamped amber canisters and a layered Pursuer mantle retain readable role differences. One- and two-handed weapons use distinct silhouettes; two-handed weapons still render one held weapon and occupy their existing locked group.

A 160-slot pool supplies contact flashes, directional sparks, short hammer dust, inward gathering motes and ultimate anticipation/payoff. Tether motes pull laterally inside its native corridor; Gravity Well motes converge on its gather point. Native footprints own the actual range. Redline uses local vents, Tempest retains its target-to-target lightning, and damage pulses do not invent extra hits or physics. Reduced motion lowers particle count, travel, expansion and opacity. Native impact reactions, cut selection and corpse behavior stay in production code.

The slice uses source-authored meshes and effects. No third-party asset pack, paid asset, new root-motion animation or trailer was added.

## Verification

- Windows build succeeded using Unity 6000.3.27f1.
- **204 native preview assertions**, plus **706 production-effect assertions with each look** (1,412 total). Includes seven fixtures with original/new/repeated-original controls, real ultimates, paired and locked weapon behavior, 11-bone severing, no extra hero physics, 160-effect cap, 0.5x/1x/4x with reduced motion, exit restoration and the next normal run's opening draft.
- **15 core regressions**, **four shared-augment regressions**, and **full eight-wave progression** passed with 706 production assertions in every run. Full progression: 319 kills, 143 collisions, 13 item choices, two augment choices, zero order errors or visible entry violations. Regression fixtures use 5,000 base health to reach all mechanics; this is not a normal-health balance estimate.
- Native 1920×1080 stills were visually inspected with the full interface. The Pursuer controls/health-panel overlap was corrected; the compact boss label preserves the control-resistance information.
- A five-second 1080p, 1x hammer fixture sample measured approximately **16.67 ms median / 16.69 ms p95** wall-frame intervals with a 60 FPS cap. This is a short representative sample, not a maximum-crowd or full-run performance certification. Peak pooled effects in the 21 rollouts: 90 of 160.

Crowd PhysX results are not bit-identical across repeated resets. The initial exact-result assertion failed on dual blades; adding repeated-original controls also showed variation without a style change. Native rollout metrics are retained as observations, while effect rules, damage and physics invariants are checked directly in both styles. Do not claim that every original/new rollout has identical kill or collision counts.

## Evidence and reproduction

Native report: [combat-preview-native.json](combat-preview-native.json). Representative stills: [preview images](combat-preview-ui/). Ordinary regression reports: `combat-preview-core-*`, `combat-preview-variety-*`, `combat-preview-full-full-1741.json`.

Build with `Build Windows.ps1`. Run the visible player with `-combatPreviewCheck -previewOutput <absolute-output-directory> -force-d3d11 -screen-fullscreen 0`. It captures native UI frames, writes `report.json`, and exits. Use the executable with `Validate Prototype.ps1 -HeroHealth 5000`, `Validate Variety.ps1 -Builds shared-conductor,shared-pressure,shared-freight,shared-sustain -HeroHealth 5000`, and `Validate Variety.ps1 -Builds full -FullRun -HeroHealth 5000`.

Regression build: `Build/CombatPreview`, assembly SHA-256 `c455d1cadc558522042988a0c7ab38318fa495c2c13c6f71c48ad87ab0822837`. Subsequent changes only relocate/shorten the preview boss HUD and keep preview controls visible during its reveal. The final reviewed build and native recheck provenance are recorded in `combat-preview-build.json`.

## Next gates

Review the playable style at normal camera distance before extending it across ordinary runs, checkpoint portraits, all gear and the environment. Then profile maximum crowds and persistent corpses over a full run. The separate [32-game rifle study](v080-rifle-review.md) makes Focused Tether coverage the next balance investigation; no tuning is bundled into this visual slice.
