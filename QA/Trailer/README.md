# 20-second combat trailer, music-only revision

The current revision is `Walking-Hard-Combat-Music-20s.mp4`, in the Desktop folder `Walking Hard Gameplay Trailer`. It retains the full native UI at 1920x1080 / 60 fps, exactly 1,200 frames / 20 seconds. It focuses on weapon selection, ability order, melee combos, launches, severing and an ultimate. Combat is at 1x with normal health and offensive values. This staged build contains no augments or guns.

| Time | Native footage |
| --- | --- |
| 0–4s | Sword-and-hammer attacks, launches and collisions. |
| 4–7s | Pick Execution Greatsword from three melee weapons and equip its locked two-handed group. |
| 7–10s | Drag Gravity Well ahead of Mass Driver in the sequence editor. |
| 10–15s | Gather enemies, launch them through the crowd, then cleave with the greatsword. |
| 15–20s | Queue and execute Razor Cyclone, followed by severing and collision aftermath. |

The original 144 BPM electronic track has a melodic hook, chords, bass and drums. The export reads only that music as its audio input; there are no game sounds, voice or editorial sound effects. A standalone `Walking-Hard-Music-20s.wav` is included beside the video. Its measured integrated loudness is -14.01 LUFS and true peak -1.61 dBTP before AAC encoding.

Capture: build to `Build/TrailerSystems-v080`, then run with `-trailerSystemsCapture -seed 1741 -force-d3d11 -screen-fullscreen 0 -screen-width 1920 -screen-height 1080`. The opt-in recorder stages a wave-six build and a checkpoint, uses native mouse handlers for the weapon choice and ability reorder, records the full framebuffer, and writes `SystemsTrailerCapture`. It asserts the weapon was equipped, Gravity Well now precedes Mass Driver, the ultimate executed, and there were no augments attached or reloads. Normal play does not enter this path.

Run `render_systems.py` with NumPy, Pillow and imageio-ffmpeg. `edit-systems.json` preserves the timing and provenance. The output folder includes the soundtrack, poster and contact sheet; large media remains outside source control. Verification: build and capture succeeded, all 1,200 frames were present, native selection/reordering and ultimate frames were inspected, and both final MP4 streams decoded without errors through 20.00 seconds. The recorder completed with one ultimate execution and zero reloads. Only capture and trailer authoring tools changed; gameplay regression checks were not repeated.

## Earlier gameplay and loot revision

The previous revision is `Walking-Hard-Gameplay-Loot-20s.mp4`, in the desktop folder `Walking Hard Gameplay Trailer`. It is 1920x1080, H.264/AAC, 60 fps, exactly 1,200 frames / 20 seconds. The full native UI stays visible. Combat runs at normal speed, with three uninterrupted 4.5-second passages and two reward decisions. There are no editorial titles, zooms or crops.

| Time | Native footage |
| --- | --- |
| 0–4.5s | Fists, Breach Hammer and ability sequence in combat. |
| 4.5–7.5s | Compare Compact Scattergun, Edge Sword and Impact Hammer; drag the sword into hand 1. |
| 7.5–12s | Resume with the sword and hammer shown in the action bar and character model. |
| 12–15.5s | Compare Capacitor Crown, Recovery Treads and a Gravity Well copy; take the copy and show the Common-to-Uncommon fusion. |
| 15.5–20s | Resume combat with the upgraded ability visible in the action bar. |

This is a curated wave-four capture, with checkpoints inserted for the edit and predetermined offers. Enemy movement, attacks, damage, health, equipment rules and fusion use production code. Both choices use the native drag-and-drop input handlers, with assertions confirming the equipped sword and upgraded ability. The cursor is drawn at the same coordinates as those input events because framebuffer screenshots exclude the operating-system pointer. The ordinary HUD, reward cards and fusion feedback are captured after IMGUI.

Capture: build the player and launch with `-trailerUICapture -seed 1741 -force-d3d11 -screen-fullscreen 0 -screen-width 1920 -screen-height 1080`. The opt-in mode warms up combat, records frames and synchronized native SFX into `UITrailerCapture`, verifies both decisions and exits. The game window must render visibly. Ordinary play does not enter this path.

Edit: run `render_ui.py` with NumPy, Pillow and imageio-ffmpeg available. It reads `Build/TrailerUI-v080/UITrailerCapture` and writes the Desktop output folder. The original 144 BPM score is quieter during rewards. `edit-ui.json` preserves the timing and provenance; the output folder also contains a poster, contact sheet, mix and metadata. Large frames and media remain outside source control.

Validation: the Unity build and native capture completed successfully. All 1,200 source frames were present. Reward, drag, equipped-loadout, fusion and combat frames were visually inspected; both streams of the completed MP4 decoded without errors through exactly 20 seconds / 1,200 frames. This revision changes opt-in capture tooling only, so the gameplay regression suite was not repeated.

## Earlier combo cut

The finished local artifact is `Walking-Hard-Combos-20s.mp4`, in the desktop folder `Walking Hard Combo Trailer`. It is 1920x1080, H.264/AAC, 30 fps, exactly 600 frames / 20 seconds. The complete video and stereo audio decoded without errors. Representative title and combat frames were inspected. The folder also contains a poster, contact sheet, mix and edit metadata.

All combat imagery is captured from the Unity player. Six staged Rare builds use wave-six production enemies, entrance rules, attacks and physics. Capture health is 5,000 to prevent an interrupted take; offensive tuning is unchanged. Footage runs at 1.2x in the edit, with a small camera crop and contrast adjustment. Cuts follow the beat. Native impact sounds are synchronized to their source frames. The backing track is an original 144 BPM electronic score synthesized by `render.py`; no stock footage or third-party music is used.

Capture: build the player, then launch it with `-trailerCapture -seed 1741 -force-d3d11`. An optional `-trailerOnly <fixture>` records one take. This explicitly requested mode disables normal input/HUD while recording six 12-second takes after an 18-second warmup. It writes frames, native SFX and take metadata beneath the player's `TrailerCapture` folder, then exits. Ordinary play never enters this path.

Edit: run `render.py` using Python with NumPy, Pillow and imageio-ffmpeg. The script expects `Build/SharedAugments-v080-trailer/TrailerCapture` and writes the desktop output folder. `edit.json` preserves the exact cuts, speed and audio levels. Large intermediate frames and media remain outside source control.
