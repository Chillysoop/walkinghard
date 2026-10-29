# Walking Hard 0.6.5 verification

29 October 2026. Approved scope: satisfying, source-specific reactions across the weapon/ability/ultimate roster, with hit-location sword severing, hammer impacts and electric spasms.

## Player and coverage

Final player: `Build/WeaponReactions-v065-final/WalkingHard.exe`. Assembly-CSharp.dll SHA-256: `66967fffc020a60d3f0c89d317516d9c994236b67974fcbd3cc2da74b9ff6391`.

All six Full PhysX scenarios pass 145 production-effect assertions apiece. New probes execute every weapon and damaging ability, all four ultimates, waist/head/elbow/knee sever selection, immediate joint removal, bounded multi-part cuts, fragment kill credit, corpse walking collision rules, electric expiry and physical stability. Live electric poses preserve health, stun duration and root position.

| Scenario | Waves | Kills | Collisions | Assertions | Passed |
| --- | ---: | ---: | ---: | ---: | --- |
| sequence | 3 | 116 | 14 | 145 | True |
| twin | 3 | 118 | 0 | 145 | True |
| manual | 3 | 118 | 33 | 145 | True |
| progression | 4 | 155 | 20 | 145 | True |
| compare-sword-2h | 3 | 122 | 25 | 145 | True |
| compare-hammer-2h | 3 | 120 | 60 | 145 | True |

The scenario fixture uses explicit `-qaHealth 5000` to finish mechanics coverage. Ordinary play and simulation regression use normal health. These are functionality/stability tests, not balance estimates.

## Native visual verification

The native renderer captured all 23 roster entries (9 weapons, 10 abilities, 4 ultimates), twelve frames each, with actual combat/PhysX. Review traces include reaction type, lifetime, position, velocity and detached-part count. The final build then captured Counterguard and all four ultimates again after adding shield/Redline feedback. The earlier roster binary has identical impact physics; its only final differences are shield/vent presentation and additional QA assertions. Both runs exited successfully without exceptions.

`v065-native-contactsheet.jpg` contains labeled crops of unaltered native frames. `v065-native-motion.jsonl` contains 336 traces with each capture pass identified. Reviewed native images show free torso/arm pieces, hammer tumble, electrical arcs, shield formation and Redline venting. This is visual verification; generated impact sounds are implemented but were not auditioned through an audio playback tool.

## Numerical regression and limits

32 matched Fast games, seeds 12000-12031, six mixed policies, eight waves: all 27 gameplay output fields match the same seeds from the v0.6.4 final 1,000-game batch. Zero invalid games. Version and wall time are excluded. See `v065-fast-regression.json`.

Damage, health, cooldowns, ammunition, stun duration, knockback values for living enemies and cascade budgets have not been retuned. Full PhysX corpse impulses and articulation intentionally change and can alter secondary collisions. The Fast regression therefore verifies the unchanged numerical/scheduling path; it does not establish unchanged Full-mode balance. The previous 1,000-game report remains evidence for v0.6.4 enemy/progression changes.

Severing disconnects anatomical joints on the existing mannequin (waist, neck, shoulders, elbows, hips, knees), with caps and persistent fragments. It does not implement arbitrary mesh slicing or deformable flesh. Broad cuts are capped at three disconnected joints per lethal hit. Every fragment shares the original enemy's kill/cascade identity and passes the existing corpse cleanup/walking rules. No additional living-enemy stun is implied by the visual twitch.

Counterguard duration and Duelist ward recommendations in `v064-balance-review.md` remain unapplied.
