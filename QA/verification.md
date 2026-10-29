# VECTOR 0.2 verification — October 28, 2026

The Windows x64 release built successfully with Unity 6000.3.27f1 / URP 17.3.0. `Validate Prototype.ps1` ran five complete three-wave simulations against the final executable; all exited with code 0 and passed.

| Path | Seed | Kills | Launches | Collision hits | Collision kills | Health remaining | Rerolls banked |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Flux Bracers > Fault Line | 1741 | 124 | 28 | 26 | 7 | 106.8 / 230 | 2 |
| Flux Bracers > Fault Line | 4319 | 118 | 26 | 23 | 12 | 191.2 / 230 | 2 |
| Flux Bracers > Servo Tuning | 1741 | 123 | 30 | 0 | 0 | 157.6 / 230 | 2 |
| Servo Tuning > Servo Tuning | 1741 | 124 | 0 | 0 | 0 | 178.0 / 230 | 2 |
| Pinball, spending every reroll | 1741 | 124 | 28 | 26 | 7 | 106.8 / 230 | 0 |

The default Pinball run produced zero launches/collisions in wave one, 13 launches and zero collisions in wave two, then 15 launches and 26 collisions in wave three. Fault Line dealt 574 actual collision damage. The stat path is also viable; these seeds do not establish general balance or imply that Pinball is always the strongest choice.

## Checks

- Three waves finish, and exactly two cards are installed.
- Three distinct offers; Fault Line requires Flux Bracers.
- Invalid/empty installation and repeated confirmation cannot skip a checkpoint or install extra cards.
- Push/pause controls do not advance a checkpoint's health, timer, or position.
- Rerolls add at checkpoints, carry forward, stop at zero, and clear a pending selection.
- Spending rerolls changes no tested combat metric when the same cards are installed: elapsed time, health, spawns, kills, launches, collisions, and damage match exactly.
- Launch-only builds cause zero collision damage; builds without bracers cause zero launches.
- Actor accounting balances; collision contacts are limited to two per launched body.
- No gameplay exceptions or errors were found in the five headless runtime logs.

## Native playtest

A complete interactive three-wave run matched the default headless report: 124 takedowns, 28 launches, 26 collision hits, and 7 collision kills. Mouse selection, keyboard selection, keyboard installation, mouse installation, and the Tab reroll were exercised. The first unspent reroll appeared as two banked at checkpoint two, then decreased to one on use. Selecting a card left combat paused until installation. The bracers changed from cyan to gold after Fault Line; launch trails, collision bursts, stagger, and live counters were visible.

The results screen shows separate per-wave launch and collision counts. Its column headings were aligned in a final UI pass, and disabled install/reroll buttons were visually dimmed. `v02-checkpoint.png` and `v02-human-pinball.png` capture the interactive playtest.

This verifies the small prototype on this Windows machine, not final animation quality, broad hardware compatibility, or long-term balance. Art and audio remain procedural placeholders. The earlier 60-second build's record is preserved in `verification-v01.md`.

Final UI follow-up: the release start screen and disabled installation state were inspected after the final build. The later results-layout recheck was interrupted when Computer Use reported `foreground window did not report a process id`; recovery found no VECTOR window. The player process was no longer running, and Player.log showed normal shutdown without a gameplay exception. No further UI input was attempted. The full interactive run described above preceded only the final UI alignment/dimming pass; all five automated runs used the final executable.
