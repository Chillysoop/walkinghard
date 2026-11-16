# Combat feel — 0.8.8

## Scope

Distinct, original synthesized contact sounds for fists, blades, hammers, guns, electricity and force; separate armor-hit/block cues; five ultimate charge/release pairs; press, bell and sweep warnings; exposure confirmation. Twelve reusable audio voices, per-cue wall-clock rate limits, two slots reserved for important cues, and warning-driven attenuation of ordinary impacts. Pause and mute stop active voices. Audio never advances combat or consumes its random stream.

Combat lines and debris now reuse pools: 128 ordinary lines, 16 independently reserved threat outlines, and 64 debris objects, alongside the existing 160 soft particles. Saturation recycles cosmetic effects. Boss/champion footprints retain their actual authored shape and use a foreground shader so bodies and ultimate particles cannot cover their boundaries. Large primary contacts receive emphasis; later same-frame contacts use smaller flashes. Reduced motion retains danger outlines with less debris movement.

Confirmed combo payoffs identify their owning sequence item with a white symbol and underline. Separate one-handed weapons retain their own source; a shared two-handed weapon highlights its grouped tiles. Counterguard and Amplifier now participate in the existing short payoff notice. No extra advice panel. Ultimates display CHARGING, RELEASE and RECOVERING from the actual action state. Blade/hammer visual extension now reaches the existing 44% contact point; damage timing is unchanged.

No changes to equipment values, rewards, action order, target selection, health, proc budgets, world collision geometry, or save schema. Walking-body scans reuse each walker position and each ragdoll bone center within the pre-simulation shove stage, preserving walker/bone order and every impulse calculation. Stability checks read each position once instead of twice; solver settings and body persistence remain unchanged. The ordinary journey remains 21 encounters. Presentation pose changes can affect the initial pose of a newly created ragdoll; Full PhysX runs are not expected to be bit-identical.

## Reproduction

`Validate Combat Feel.ps1 -Mode Fixtures -OutputDirectory <fresh-directory>` opens native fixtures, verifies pools, sound data, priority handling, actual ultimate identity/phases, source ownership and reduced-motion captures at 720p, 1080p and ultrawide. Profiles are isolated; practice fixtures use the existing fixed seed 1741 (the first runner manifests displayed the unused CLI default 4319). WAV files retain the actual generated sound bank for review.

`-Mode Journey -Seed 4319 -Policy pinball` uses normal health and ordinary automatic drafts through the current 21-encounter journey, sampling 1080p frame intervals at 1x and 4x. Run without competing builds or simulations. `-Unpooled` isolates the former create/destroy allocation policy in the same binary; it does not restore the entire previous game's presentation. Frame reports include active versus retained bodies and separate walking-shove, transform-sync and PhysX CPU time. `-OriginalBodyScan` selects repeated native position queries; `-VerifyWalkingSnapshot` compares cached centers with current native values at every queried bone and reports the count. Verification is separate from performance sampling because its extra queries deliberately restore that cost.

Core contracts use the existing 5,000-health override. Boss/champion rendered fixtures use staged states and elevated health. Neither establishes ordinary difficulty or player comprehension. Automated decisions do not measure the human 30-minute pacing target.

## Initial findings retained

- Initial build: duplicate `LateUpdate` conflicted with the existing journey clock hook. Mixing now shares that hook. Original error retained in `Logs/build-v088-initial.log`.
- Initial presentation fixture: invoking the full mechanics suite from Combat Preview violated the suite's ordinary-route setup. Removed the redundant invocation; the dedicated core runner covers those contracts. The failure and follow-on HUD errors remain in `QA/v088-fixtures-initial/player.log`.
- Second fixture pass: 55 assertions passed, but visual inspection caught mislabeled ultimate images. Equipping through the player UI handler during combat correctly rejected the fixture's swap. The fixture now equips its staged loadout directly and asserts the actual ultimate identity before capturing its phases. Those initial images remain in `QA/v088-fixtures-r2`.
- Core checks: all 15 passed on assembly `153A77E1ADE192A52D065BF6FE8ECC60FD71A508D3A19A6CEF641A9D9301DBA7`. Subsequent changes at that point only corrected/extended the new review harness.

## Performance finding and response

The initial same-binary unpooled/pooled journeys both completed all 21 encounters at normal health, with zero order/visible-spawn errors and valid persistent-body attribution. All sampled 1x medians stayed near 16.67 ms. At the finale's 4x sample, both degraded to roughly 115–117 ms median with 147 nearby active corpses in the unpooled case. Most cost was repeated fixed-step PhysX work plus walking-body scans, not effect allocation. The automatic progression/scheduling and PhysX trajectories vary; these are individual profiles, not universal FPS guarantees.

This prompted the bounded scan optimization described above. No bodies were removed, no solver iterations reduced and no combat time discarded. Initial full-run reports remain separate from the subsequent scan-candidate measurements.

## Final results

Final player assembly: `782908BD1A0B73CDBC3A82A718AB2A7B14C8C637A336ECA6F937C71E6079C9D7`.

- All 15 core scenarios passed again after the scan change. See `core-summary.json` and `core-manifest.json`. Their elevated-health contract scope is retained.
- Final native fixtures: **63 assertions**, plus **63,998 cached-center comparisons** with no mismatch. The six weapon layouts, separate/2H source attribution, all five actual ultimate identities and their phases, voice limits, muted capture, pause, pool saturation/expiry/reuse, and three reduced-motion resolutions passed. See `fixtures-report.json`.
- Boss rendered suite: **713 assertions**; champion rendered suite: **98 assertions**. These were run on the preceding presentation assembly `6BF0E74BE59F0C34A4BEA14EDC1A94B970FB1299CFA8DA89810E3F551A95BB9A`; subsequent changes added the scan optimization, cached stability reads, silent sound-capture compatibility and additional review checks. The phase/footprint code did not change. Both original manifests are retained.
- All **four ordinary-loot native journeys** completed 21 encounters with valid body/kill ownership, no ordering errors, no visible arrivals and unchanged 20-item/5-augment budgets. Initial allocation comparisons used the preceding assembly; the repeated-query versus scan comparison used the final assembly.
- The 23 generated WAV cues are finite, non-silent and unclipped individually. Retained samples and amplitude measurements are in `audio/` and `audio-summary.json`. Automated sample checks do not establish subjective listening quality or hearing accessibility.

### Final same-binary scan comparison

Normal health, full unlock pool, seed 4319, pinball draft preference, Full PhysX, 1080p, 60 FPS cap. Seven three-second samples at each speed. These are one completed journey per policy, with automatic decisions and fast-forward between samples. The original-scan switch restores repeated native position queries; it is an isolation control, not a byte-for-byte recreation of the prior method (both paths cache the bounds' min/max).

| Measure | Repeated-query control | Cached scan |
| --- | ---: | ---: |
| Sampled 1x median frame intervals | 16.66–16.67 ms | 16.66–16.67 ms |
| Highest sampled 1x p95 | 16.70 ms | 16.81 ms |
| Finale 4x median / p95 | 121.17 / 129.04 ms | 78.96 / 109.33 ms |
| Finale peak active / retained corpses | 158 / 942 | 158 / 940 |
| Finale peak living enemies | 42 | 42 |
| Final kills / integrity | 977 / 218.79 | 978 / 228.85 |

This pair improved the crowded 4x finale, but **does not solve its frame-rate limit**. PhysX remains the largest measured cost. Per-frame CPU values also include different numbers of fixed steps as real frame duration changes; they must not be treated as a per-step speedup. PhysX trajectories and encounter state differ, so this is not a universal 35% performance claim. No solver quality, physical body interactions, simulation frequency or collision budgets were reduced to obtain the result. Evidence supports smooth sampled normal-speed play on this machine and a remaining extreme-speed bottleneck.

Selected native images were inspected: weapon payoff ownership, all ultimate identities/states, reduced-motion danger over an ultimate, boss/champion threats, the crowded finale and result. The original incorrectly named ultimate images and all raw logs remain local. Human comprehension, perceived audio quality and the approximately 30-minute human pacing target remain to be playtested.
