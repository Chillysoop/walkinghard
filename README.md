# Walking Hard 0.3 — Drafted Sequences

A Unity autobattler prototype: one fighter walks forward while enemies enter through side doors and the horizon. Draft the actions, choose their order, and turn bodies into physical projectiles. Camera: behind and above the fighter.

## Play

Download the Windows ZIP from [Releases](https://github.com/Chillysoop/walkinghard/releases/latest), extract the whole archive, and run `WalkingHard.exe`.

For an updating desktop shortcut, run `Launcher/Install.cmd` from the extracted ZIP. **Walking Hard (Main)** checks the main branch each time it opens. It downloads a matching published build and verifies its SHA-256. If main is newer than the published build, it builds an isolated copy using installed Git and Unity 6000.3.27f1. No editor is needed for published builds. If an update fails or you are offline, it keeps the last working installation and reports the fallback. It never resets your development checkout.

The launcher cache and log live under `%LOCALAPPDATA%/WalkingHard`. The first source build can take several minutes. Cloud Unity compilation is not configured; contributors should publish a matching Windows release after merging changes. Without a matching release or an installed editor, the launcher cannot run unreleased source and uses its last working build.

## The current playable slice

- Two separate opening pick-one-of-three ability drafts. Place each in a slot, then explicitly install it.
- Occupied slots execute strictly **1 → 2 → 3 → repeat**. Empty slots skip. Auto Combat is on the main combat screen.
- The first checkpoint offers the third ability. The second offers a modifier; unused rerolls accumulate.
- Drag draft cards to empty slots, or drag existing abilities between slots to reorder at a checkpoint. Keyboard card/slot selection remains available.
- Six abilities: Steel Jab, Breaker, Force Push, Mass Driver, Gravity Well, and Arc Bolt. Labeled effect icons distinguish effects an ability creates from effects it uses.
- Dynamic enemy capsules collide with the floor, walls, and each other. Defeated enemies become jointed ragdolls. Mass Driver turns living or defeated bodies into projectiles; their first two distinct enemy contacts deal damage. Fault Line strengthens those contacts.
- Covered side passages and camera-frustum checks hide spawn locations. Enemies move through door openings or run in from the horizon.

Three waves each have 30 seconds of arrivals and up to 8 seconds of cleanup. Remaining enemies carry into the next wave. Combat and physics stop for drafting. Surviving wave three completes the run even with enemies remaining. Builds and combat outcomes vary; these are balance hypotheses, not final difficulty.

## Controls

| Input | Action |
| --- | --- |
| Enter | Begin / install a selected draft |
| 1 / 2 / 3 | Select draft offer |
| Q / W / E | Assign the selected ability to an empty slot |
| Drag a card or ability | Assign to an empty slot / reorder occupied slots |
| Tab | Spend a banked reroll |
| A | Toggle Auto Combat on the fight screen |
| F | Advance the next eligible action in sequence |
| Space / Escape | Pause or resume |
| R | New run and draft |
| T | Repeat the seed |
| M | Mute |
| F8 | Save a screenshot |

With Auto Combat off, F advances one action in the same order; it does not select a different slot. Pause includes reduced motion. Physics is not guaranteed bit-for-bit deterministic across machines even when the draft seed matches.

## Develop

Clone this repo and add it to Unity Hub. Use **Unity 6000.3.27f1** with Windows Build Support. Open `Assets/Vector/Scenes/CombatStudy.unity`. The project uses URP 17.3.0, procedural geometry, animation, and audio; no paid art packs are required.

Run `Build Windows.ps1` to build `Build/Windows/WalkingHard.exe`. The `-UnityEditor` parameter supports a nonstandard editor location. Run `Validate Prototype.ps1` against the resulting player; it exercises five encounter paths, including manual control and exhausted rerolls. Run `Launcher/Test Launcher.ps1` to check installation and failure handling.

| Source | Responsibility |
| --- | --- |
| `AbilityDeck.cs` | Ability definitions, draft offers, slot order, modifiers |
| `VectorEncounter.cs` | Waves, state, movement, concealed entries |
| `EncounterCombat.cs` | Ability execution, damage, launch/contact rules |
| `FighterPhysics.cs` | Capsules, bodies, joints, contact forwarding |
| `EntryLayout.cs` | Floor, walls, covered doors, visibility tests |
| `PrototypeHUD.cs`, `HUDInteractions.cs` | Drafts, combat HUD, dragging, synergy icons |
| `EncounterQA.cs` | Standalone simulation assertions and reports |
| `Assets/Resources/CombatTuning.asset` | Active encounter tuning |

Scripts are in `Assets/Vector/Scripts`. The historical Vector folder names are retained for asset stability.

## Design versus implementation

[DESIGN.md](DESIGN.md) records the agreed next direction: two carried weapons including one- and two-handed options; visible switching time; unlimited prototype storage; automatic three-copy, same-rank upgrades through rank III; unarmed starts; weapons at checkpoints; mixed later rewards. Drag-and-drop inventory and shared synergy icons are part of that direction.

**Weapon equipment, stored inventory, fusion, body severing, and multi-hop collision cascades are not in the 0.3 executable.** The interactive workshop demonstrates those loadout rules. The current physical projectile system has two direct damaging contacts per launched body, with capped ragdoll lifetime/count. Models and presentation remain placeholders.

See [CONTRIBUTING.md](CONTRIBUTING.md) for collaboration and release instructions. The owner-private design workshop is separate from this public repository; personal review history is not included here.
