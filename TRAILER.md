# Walking Hard — 30-second gameplay trailer

Use the **current v0.7.2 art**, per the user's October 29 direction. The art/VFX pass remains a separate plan. Capture native game rendering with production attacks, physics, item claims and fusion. Curated encounters/loadouts are labeled in the footage. No boss, reveal, final-stand camera, boss health bar, silhouette or finale audio.

| Time | Shot | Teaching point |
| --- | --- | --- |
| 0–3 | Gravity and collision opening | Keep walking; builds cause crowd-clearing payoffs. |
| 3–5 | Greatsword | Sword kills sever bodies. |
| 5–7 | Hammer | Heavy hits create cascading impacts. |
| 7–9 | Paired pistols and lightning | Mix weapons and technology. |
| 9–14 | Actual checkpoint claim and two reorders | Choose one of three; arrange two basics and three abilities. |
| 14–18 | Gravity → Mass Driver | Sequence order produces combinations. |
| 18–22 | Claim the third matching Arc Bolt | Three identical items of the same rarity automatically fuse. |
| 22–23.5 | Checkpoint-three ultimate draft | Ultimates enter the run after wave three. |
| 23.5–27 | Cut forward to a charged Singularity | Ultimate recharge is separate; execution joins the sequence. |
| 27–30 | Title over continuing gameplay | Build the order. Break the crowd. |

## Reproduction

Build with `Build Windows.ps1 -OutputDirectory <new capture player directory>`. Launch the resulting player normally, with a visible rendering window, at 1920×1080:

```powershell
WalkingHard.exe -trailerCapture -trailerOutput <fresh directory> -trailerEncoder <absolute ffmpeg.exe path> -force-d3d11 -screen-width 1920 -screen-height 1080 -screen-fullscreen 0
```

The capture renders 900 frames at 30 FPS, writes `gameplay-silent.mp4`, saves one proof frame per second, exports the game's generated sound effects and records gameplay audio events. Combat advances through the existing 60 Hz simulation. A per-frame guard aborts if any boss, wave eight or reveal state appears. The fixture does not change normal-play art or balance.

Run `QA/Trailer/finish_trailer.py <capture directory> --ffmpeg <path> --output <final.mp4>` using Python with NumPy and Pillow. It adds an original procedural score, synchronizes the game's sound effects, checks real sever/collision/fusion/ultimate events, and verifies the final frame count, duration, resolution, audio and decode. FFmpeg, NumPy and Pillow are build tools, not bundled game assets.

Review the contact sheet and motion clips for readable captions, working drags, actual impacts and absence of spoilers before delivering. The final export is an H.264/AAC MP4 with fast-start metadata for easy sharing. Large renders stay outside Git under `Build/` or the user's chosen export folder.
