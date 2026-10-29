# Current-art trailer verification

October 29, 2026. Final artifact: `Walking-Hard-30s.mp4` in the user's Desktop / Walking Hard Trailer folder. SHA-256: `8625a62d8486236db60d27be09a9c3c30525180b4ed3b492e3f9af2170b2de44`.

- Exactly **900 frames / 30.000 seconds**, 1920×1080 at 30 FPS, H.264 video and stereo AAC at 48 kHz. Fast-start metadata; approximately 15.8 MB.
- Current v0.7.2 art and production combat/item rules, with curated loadouts and staged crowds labeled in the opening montage. Capture-only framing and teaching captions do not change normal play.
- Every rendered frame checks that no boss actor, finale/reveal state or wave eight is present. All 900 frames passed. The edit contains no finale footage or audio.
- Native frame captures, an every-second contact sheet and denser decoded motion strips were inspected. Sword severing, hammer launches, lightning, the reorder drag, fusion before/after and the ultimate payoff are present.
- Real effects recorded: sword shot 13 severs; hammer shot 3 kills and 6 collisions; pinball payoff 24 kills and 12 collisions; one actual three-copy fusion; one actual ultimate execution. These are curated demonstration outcomes, not balance samples.
- The checkpoint's resulting five-action order matches the following pinball shot. Ultimate footage cuts forward to a charged encounter after the checkpoint-three draft; it does not imply an instant recharge.
- Final video decodes without errors. Encoded frame 60 matched its native screenshot with mean RGB error 1.486/255, confirming orientation and content. Audio uses 52 gameplay-timed events, the existing generated game sound effects and an original procedural score; loudness target −16 LUFS / −1 dBTP.
- **15 normal-play core fixtures passed**, including the existing 533 production assertions, with zero visible spawns and zero order errors. Raw reports: `QA/trailer-core-*.json`; compact results: `verification.json`.
- The standard native presentation fixture completed with `PRESENTATION_REVIEW_SUCCESS`. All seven checkpoint, mismatch, preview, augment, icon-key, fusion and combat captures were inspected; see `presentation-review.jpg`.

One existing copy bug found during review was corrected: Duelist's short description now reads its ward count from the same catalogue method as gameplay, reflecting the approved three hits. No balance values or art assets changed. The capture fixture is opt-in through `-trailerCapture`; it does not activate during normal play or simulations.

Source and repeatable edit instructions: `TRAILER.md`, `Assets/Vector/Scripts/TrailerCapture.cs`, and `QA/Trailer/finish_trailer.py`. No external music or downloaded art is included.
