# VECTOR — 60-second combat prototype

A Unity 6.3 / URP combat study for a solo martial-arts autobattler. The fighter walks forward automatically, viewed from behind and above. Enemies move continuously, separate from each other, circle, and seek openings. A jab, cross, and wide palm finisher form the automatic three-hit combo. A kinetic pulse pushes the crowd back when enough enemies close in.

## Play

Run `Build/Windows/Vector.exe`, then press **Enter** or click **Begin the walk**.

A desktop shortcut named **VECTOR - Play Prototype** opens the build. `Releases/VECTOR-Prototype-Windows.zip` contains a portable copy: extract the entire ZIP and run `Vector.exe`, keeping its data folders beside it.

| Control | Action |
| --- | --- |
| Enter | Start / replay |
| Space or Escape | Pause / resume |
| R | Restart the encounter |
| F | Trigger force push when charged |
| A | Toggle automatic force push |
| M | Mute / unmute |
| F8 | Save a screenshot |

Everything runs automatically by default. The manual push control exists for testing timing. Pause also provides reduced camera motion, audio, restart, and quit controls. Survive for 60 seconds; zero integrity ends the run. The results panel shows kills, combo finishers, force pushes, and peak nearby crowd.

## Open in Unity

Add this folder in Unity Hub and open it with **6000.3.27f1**. Open `Assets/Vector/Scenes/CombatStudy.unity` and press Play. The encounter builds its environment and articulated character rigs at runtime. Tune the `Assets/Resources/CombatTuning.asset` asset in the Inspector.

Editor menu: **VECTOR → Generate scene and settings** or **VECTOR → Build Windows prototype**.

The project is kept outside OneDrive to avoid syncing Unity's generated Library database. The build is standalone; it does not require Unity to play.

## What this prototype proves

- Elevated chase camera with the hero below center and visibility into the approaching crowd.
- Free continuous crowd steering, separation, circling, and coordinated openings without a visible grid.
- Readable three-step automatic melee and a wider stagger/knockback finisher.
- Automatic radial force push with windup, cooldown, displacement, and interruption.
- Timed escalation, pressure, damage, defeat, victory, pause, and repeatable encounters.

## Deliberate limits

This is a combat-feel prototype. The models, joint animation, effects, and sounds are generated locally; there are no purchased or final character assets. Imported skeletal animation, matched throws, root-motion combat, ragdolls, terrain-aware navigation, and a complete art pass are future work. The six archetypes, card drafting, equipment, banked rerolls, and telegraphed metaprogression unlocks are not implemented in this slice.

The force push is a radial kinetic pulse so enemies circling behind the hero can also be displaced. Heavy enemies resist some displacement. Combat does not use a general-purpose physics simulation for crowd motion; movement and knockback are explicitly controlled for readability.

## Source map

- `Assets/Vector/Scripts/VectorEncounter.cs`: encounter simulation, crowd movement, targeting, combat, camera, and QA report.
- `Assets/Vector/Scripts/FighterRig.cs`: articulated character construction and procedural pose curves.
- `Assets/Vector/Scripts/CombatEffects.cs`: impact trails, field rings, particles, and synthesized audio.
- `Assets/Vector/Scripts/PrototypeHUD.cs`: scaled interface and controls.
- `Assets/Vector/Scripts/VisualFactory.cs`: materials, lighting, and environment.
- `Assets/Vector/Scripts/CombatTuning.cs`: editable combat parameters.
- `Assets/Vector/Editor/PrototypeBuilder.cs`: scene generation and Windows build.

## Repeatable QA

The standalone supports `-batchmode -nographics -vectorQA -seed 1741 -qaOutput <absolute-path>`. It runs the same encounter simulation in fixed 1/60-second steps, verifies completion, melee finishers, force-push effects, crowd buildup, incoming damage, and actor accounting, then writes JSON and exits with code 0 for success or 2 for a failed check. These checks verify logic, not animation quality; visual inspection of the running build is also required.

Screenshot output: `%USERPROFILE%/AppData/LocalLow/Banjo Gaming/VECTOR - Combat Prototype/Captures`.
