# VECTOR 0.2 — Human Pinball

A playable Unity autobattler study: one martial artist walks forward, the crowd gathers around him, and your choices between waves change his automatic combat. The camera remains behind and above him.

## Play

Run `Build/Windows/Vector.exe`, or use the **VECTOR - Play Prototype** desktop shortcut. Press **Enter** to start. The portable build is `Releases/VECTOR-Prototype-Windows.zip`; extract the entire ZIP before running its executable.

This version has **three waves and two upgrade checkpoints**. Each wave has 30 seconds of arrivals and up to 8 seconds of cleanup. Clearing the remaining enemies ends cleanup early; otherwise survivors carry into the next wave. Combat freezes while you draft. Surviving the third wave finishes the run, even if some enemies remain. Actual play time is about two minutes plus drafting and brief impact pauses.

For the featured path, choose **Flux Bracers** at the first checkpoint, then **Fault Line** at the second:

1. **Wave 1:** fists, a three-hit combo, and automatic force push.
2. **Wave 2 / Flux Bracers:** the finisher launches one body through the crowd, including bodies killed by the strike. Cyan trails show launches.
3. **Wave 3 / Fault Line:** launched bodies gain gold trails. The first two enemy contacts each deal 26 damage and stagger for one second. Collisions do not create further launches.

You can take alternative upgrades instead. The prototype contains six cards: Flux Bracers, Fault Line, Servo Tuning (+20% combo speed), Dense Knuckles (+25% melee damage), Field Capacitor (20% shorter push cooldown), and Repair Weave (55 immediate healing, capped at maximum integrity). Equipment appears on the hero's wrists. Cards show their effects before installation.

Each checkpoint grants one reroll; unused rerolls remain banked. In this guided slice the first card stays on the featured path, while rerolls refresh supporting choices. Skipping Flux Bracers keeps it available at checkpoint two; Fault Line is only offered after you own the bracers. Select a card, inspect it, then explicitly install it to resume combat.

## Controls

| Control | Action |
| --- | --- |
| Enter | Start / replay; install the selected checkpoint upgrade |
| 1 / 2 / 3 or click a card | Select a checkpoint offer |
| Tab or Reroll button | Spend one banked reroll at a checkpoint |
| Space or Escape | Pause / resume combat |
| R | Restart the whole run, clearing upgrades and counters |
| F | Manually trigger a charged force push (test override) |
| A | Toggle automatic force push |
| M | Mute |
| F8 | Save a screenshot |

Normal movement, targeting, punches, and force push are automatic. Automatic push now waits for the current three-hit combo to finish. Pause offers reduced motion, which also disables impact pauses and camera shake. The checkpoint itself pauses combat, including attacks, projectiles, cooldowns, and the timer.

## Combat and feedback

- Continuous crowd steering, separation, circling, and angle-separated attack openings; no grid.
- Punch contact aligned with extension, target continuity between combo hits, sharper recoil, weight shift, and brief impact pauses.
- Orange ground markers telegraph enemy attack openings; cyan marks indicate stagger.
- One directed launch per finisher; heavy bodies travel more slowly.
- Swept collision detection prevents fast projectiles passing through enemies between simulation steps. Each body can hit a particular enemy only once and at most two enemies total.
- Live launch/collision counters and a per-wave results table make the installed effects visible. Wave pressure escalates, so the table is observational rather than a controlled performance comparison.
- Run integrity increased to 230 for this longer test. Baseline damage and crowd pressure remain tunable.

## Open and build in Unity

Add this folder in Unity Hub and open with **6000.3.27f1**. Open `Assets/Vector/Scenes/CombatStudy.unity` and press Play. The project uses URP 17.3.0 and generates its environment and character rigs at runtime. Edit `Assets/Resources/CombatTuning.asset` for tuning; the supported slice uses three waves.

Editor menus: **VECTOR > Generate scene and settings** and **VECTOR > Build Windows prototype**. `Build Windows.ps1` builds without opening the editor UI. This project is outside OneDrive to avoid syncing Unity's generated Library cache. The standalone player does not require Unity.

## Scope

This is a playable mechanics prototype with procedural placeholder models, animation, and audio. It is not the later six-wave / 18-upgrade slice. Lightning, riflemen, shields, bosses, equipment slots, persistent unlocks, and the other archetypes remain future work. No paid or downloaded art assets are required.

## Source map

- `Assets/Vector/Scripts/VectorEncounter.cs`: fixed-step encounter, waves, steering, strikes, launches, swept collision contacts, and QA reporting.
- `Assets/Vector/Scripts/UpgradeDraft.cs`: six card definitions, prerequisites, modifiers, selection, and banked rerolls. Draft randomness is isolated from combat and spawns.
- `Assets/Vector/Scripts/FighterRig.cs`: articulated characters, procedural poses, telegraphs, and visible bracers.
- `Assets/Vector/Scripts/PrototypeHUD.cs`: combat display, checkpoint cards, pause, and per-wave results.
- `Assets/Vector/Scripts/CombatEffects.cs` and `VisualFactory.cs`: trails, impacts, audio, scene geometry, lighting.
- `Assets/Vector/Editor/PrototypeBuilder.cs`: scene generation and Windows build.

## Verification

Run `Validate Prototype.ps1` after building. It exercises the real standalone executable across five runs: Human Pinball with two seeds, launch without collisions, stat upgrades without launches, and reroll exhaustion. Reports are written to `QA/v02-*.json`; details are in `QA/verification.md`.

The executable supports `-batchmode -nographics -vectorQA -seed 1741 -qaBuild pinball -qaOutput <absolute-path>`. Other test paths are `launch`, `power`, and `reroll`. Tests validate completion, upgrade gating, banked rerolls, duplicate confirmation protection, actor accounting, and actual launch/collision activity. Gameplay uses the same 60 Hz simulation in interactive and headless runs. Visual quality still needs live inspection.

Screenshots: `%USERPROFILE%/AppData/LocalLow/Banjo Gaming/VECTOR - Combat Prototype/Captures`.
