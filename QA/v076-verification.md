# Walking Hard 0.7.6 verification

October 30, 2026. Unity 6000.3.27f1. Shipping player: `Build/Synergy-v076-shipping/WalkingHard.exe`. Assembly SHA-256: `c01186feb7a19029421f23951d175146170cf137a99faa93581b23e468a48b4c`.

## Changes

- Contextual equipped-partner explanations, white effect badges/underlines, and matching hover detail. Explicit effect relationships exclude storage, displaced partners, mutually exclusive armor and Counterguard's replaced bank when Discharge is attached.
- Bounded actual-event combat cues for Conductive, gathering follow-ups, collision damage, lifesteal, guard absorption and stored haste. Real-time display stays readable at 4x; cues do not queue or affect simulations.
- Suppressive Volley: a ranged, three-target interrupt with no forced knockback, rarity scaling and Focused/Echo compatibility.
- Recoil Harness: actual reload earns one shared, expiring block; absorption stores one later gun-hit bonus. It cannot stack through paired refills or full magazines, and armor owns its damage/kill contribution.
- Dedicated generated card artwork and stable ID mappings. Prompts: `Art/deadeye-prompts.json`.
- Controlled comparisons support a focal ability at the requested rarity while keeping other slots fixed.

Initial values are documented in DESIGN.md. No existing combat values changed. Recommendations in `v076-balance-review.md` remain unapplied.

## Final shipping verification

- **15 core fixtures** passed: `v076-core-*.json`.
- **45 build-variety fixtures** passed: `v076-variety-*.json`, run in three independent groups through the existing validator.
- **One full eight-wave progression fixture** passed: `v076-full-full-1741.json`; 316 kills, 143 collisions, 13 item choices and two augment choices.
- **577 production-mechanic assertions per run**, including new Volley fan/range/entry exclusions, no knockback, Focused behavior, reload eligibility, expiry, paired sharing, actual block/damage attribution, one-shot consumption, armor removal and partner-hint exclusions.
- **1,342 native UI assertions** passed, including ordinary focused/full-sheet drop targets and explicit partner identity for both new items, plus an actual Conductive discharge cue. Existing tutorial, text-fit, storage, fusion and interaction checks remain covered. Native captures span 1280×720, 1920×1080 and 2560×1080; selected screenshots and the original native assertion log are in `v076-ui/`.
- **174 model tutorial assertions** passed separately.
- Item-presentation review passed with **63 mapped definitions**, covering checkpoint, rarity mismatch, fusion, augment detail, icon key and combat.
- New-item timed native captures and motion traces were inspected on the combat-identical candidate under `Build/Synergy-v076-final/NativeReview`. Final shipping UI screenshots were inspected separately.
- No visible spawns or sequence-order errors in the final core, variety or full progression fixtures. Publisher syntax and `git diff --check` passed.

Core/variety/progression fixtures use 5,000 integrity to exercise systems. **Balance comparisons use normal 280 base integrity**, matching seeds/rarities and fixed alternatives. They are not interchangeable evidence.

## Diagnostics and limitations

An early reload fixture failed its unrelated READYING-coverage expectation: initial gun preparation was completely covered by a longer reload, and a same-type pair never needed another type switch. The assertion now requires READYING in the explicit mixed-weapon `armed` fixture. All per-frame delay correctness checks, reload coverage and pair-wait checks remain intact. No game timing changed.

One early Ballet fixture reported `Unstable ragdoll physics`. It did not reproduce in the standalone repeat or final shipping suite. The original log is preserved in `v076-diagnostics/ballet-initial.log`; no unrelated physics or balance adjustment was made to conceal it. This remains a transient PhysX stability observation, not a proven fix.

All **1,572 controlled games** completed without integrity failures or timeouts; the 48 Full cases all won. Strong supporting gear creates ceiling effects, the sample has only two Full seeds per rarity, and Fast/Full rifle outcomes diverge substantially. The balance report separates modes and records this uncertainty. Headless success does not establish player comprehension or visual polish.

## Reproduce

```powershell
& '.\Build Windows.ps1' -OutputDirectory (Join-Path $PWD 'Build\Synergy-v076-shipping')
$player = Join-Path $PWD 'Build\Synergy-v076-shipping\WalkingHard.exe'
& '.\Validate Prototype.ps1' -Player $player -ReportPrefix fresh-core -HeroHealth 5000
& '.\Validate Variety.ps1' -Player $player -ReportPrefix fresh-variety -HeroHealth 5000
& '.\Validate Variety.ps1' -Player $player -ReportPrefix fresh-full -HeroHealth 5000 -Builds full -FullRun
& '.\Validate Tutorial.ps1' -Player $player
& '.\Validate Tutorial.ps1' -Player $player -Rendered
```

The rendered checks briefly show the native game and drive its own mouse handlers. They use no Computer Use. Simulation reproduction commands and per-case evidence are in `v076-balance-review.md` and `Balance/v076/`.
