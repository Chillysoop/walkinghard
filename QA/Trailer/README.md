# 20-second combo trailer

The finished local artifact is `Walking-Hard-Combos-20s.mp4`, in the desktop folder `Walking Hard Combo Trailer`. It is 1920x1080, H.264/AAC, 30 fps, exactly 600 frames / 20 seconds. The complete video and stereo audio decoded without errors. Representative title and combat frames were inspected. The folder also contains a poster, contact sheet, mix and edit metadata.

All combat imagery is captured from the Unity player. Six staged Rare builds use wave-six production enemies, entrance rules, attacks and physics. Capture health is 5,000 to prevent an interrupted take; offensive tuning is unchanged. Footage runs at 1.2x in the edit, with a small camera crop and contrast adjustment. Cuts follow the beat. Native impact sounds are synchronized to their source frames. The backing track is an original 144 BPM electronic score synthesized by `render.py`; no stock footage or third-party music is used.

Capture: build the player, then launch it with `-trailerCapture -seed 1741 -force-d3d11`. An optional `-trailerOnly <fixture>` records one take. This explicitly requested mode disables normal input/HUD while recording six 12-second takes after an 18-second warmup. It writes frames, native SFX and take metadata beneath the player's `TrailerCapture` folder, then exits. Ordinary play never enters this path.

Edit: run `render.py` using Python with NumPy, Pillow and imageio-ffmpeg. The script expects `Build/SharedAugments-v080-trailer/TrailerCapture` and writes the desktop output folder. `edit.json` preserves the exact cuts, speed and audio levels. Large intermediate frames and media remain outside source control.
