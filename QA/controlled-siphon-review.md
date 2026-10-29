# Controlled Siphon comparisons — v0.6.7

29 October 2026. Recommendations only: no item values, enemy settings, release, main branch or installed launcher build changed.

## Conclusion

Siphon Blade is the clearest outlier: its equipped global Lifesteal makes a weapon slot a whole-build sustain engine. Reclaimer is also too efficient, especially as rarity increases, although Common is less decisive. Harvest is a strong sustain choice, with a smaller and more contextual advantage. Reduce the two persistent global Lifesteal sources before tuning Harvest or broadly increasing damage on alternatives.

## Experiment

4,080 fixed-loadout Fast games: 3,600 primary cases and 480 Rare-support sensitivity cases. The primary matrix is four contexts × twelve encounter seeds × five focal rarities × fifteen items. Each candidate changes only one equipment slot. Everything else in each paired comparison stays identical, including gear, sequence order, supporting rarity, encounter seed and ultimate activation policy. The primary support items are Common; the sensitivity matrix repeats six focal items with Rare support, four seeds per context and all five focal rarities.

Contexts: sever (gather/cuts/Salvage Magnet), storm (Conductive/Arc Bolt/body collisions), guns (paired pistols/Loader/Overclock), and stacked Siphon (offhand Siphon Blade/Drain Brand/Predator/Reclaimer). The weapon slot is compared with Edge Sword, Kinetic Fists, Breach Hammer and Pulse Pistol; armor with Bulwark, Reactive, Grounding and Duelist; Harvest with all four other ultimates. Native pairing effects remain enabled, so cross-family weapon substitutions include their real sequence-time consequences. Siphon Blade versus Edge Sword is the cleanest same-family comparison. No augments are equipped.

Eight waves, normal 280 base HP plus actual armor stats, full starting health, authored enemies, 600-second simulated timeout. Fixed gear and three abilities are supplied at the start and never drafted/upgraded later; every ultimate is equipped after checkpoint three with its normal cooldown. These are deliberately frozen build stress tests, not estimates of ordinary player win rate. Every ultimate alternative has exactly identical pre-unlock wave results. Rarity comparisons below use 48 matched context/seed cases per item per tier.

## Matching rarities

| Item | Common | Uncommon | Rare | Epic | Legendary | All tiers |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Siphon Blade | 56.2% | 70.8% | 79.2% | 97.9% | 100.0% | 80.8% |
| Edge Sword | 25.0% | 25.0% | 25.0% | 27.1% | 33.3% | 27.1% |
| Breach Hammer | 29.2% | 29.2% | 37.5% | 39.6% | 41.7% | 35.4% |
| Kinetic Fists | 20.8% | 25.0% | 25.0% | 25.0% | 25.0% | 24.2% |
| Pulse Pistol | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Reclaimer Armor | 27.1% | 35.4% | 56.2% | 68.8% | 72.9% | 52.1% |
| Bulwark Plate | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Reactive Shell | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Grounding Mantle | 25.0% | 25.0% | 25.0% | 25.0% | 27.1% | 25.4% |
| Duelist Jacket | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Harvest | 25.0% | 25.0% | 29.2% | 37.5% | 54.2% | 34.2% |
| Singularity Burst | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Tempest Array | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Razor Cyclone | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |
| Redline | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% | 25.0% |

Main paired differences, using equal weights across contexts and rarities: Siphon Blade over Edge Sword +53.8 percentage points; Reclaimer over Bulwark +27.1 points; Harvest over Tempest +9.2 points. Stratified bootstrap intervals resample encounter-seed blocks within context, keeping repeated rarities together: Reclaimer Armor vs Bulwark Plate 22.9 to 31.2 points; Harvest vs Tempest Array 5.8 to 12.5 points; Siphon Blade vs Edge Sword 50.8 to 56.7 points. These intervals describe these fixture families, not all possible builds or human play.

## Stronger supporting gear

| Focal item | Wins / 80 | Mean finishing HP, deaths = 0 |
| --- | ---: | ---: |
| Bulwark Plate | 61/80 | 49.0% |
| Reclaimer Armor | 76/80 | 77.0% |
| Harvest | 80/80 | 82.1% |
| Tempest Array | 63/80 | 45.6% |
| Siphon Blade | 80/80 | 93.9% |
| Edge Sword | 49/80 | 41.8% |

The advantage persists when support is Rare. In the stacked Siphon context all six sensitivity candidates won every run and finished around 99% HP: additional healing has little marginal value there. This is not evidence that every alternative is weak in its intended build.

## Full physics checks

24 preselected Full games, Rare focal items with Common support, three matched seeds for each of four contrasts. These test direction under real bodies/contact/PhysX; three seeds per contrast are too few for a population win-rate estimate.

| Context | Item | Wins / 3 | Mean finishing HP |
| --- | --- | ---: | ---: |
| gun / ultimate | Harvest | 2/3 | 23.2% |
| gun / ultimate | Redline | 0/3 | 0.0% |
| sever / armor | Bulwark Plate | 3/3 | 28.7% |
| sever / armor | Reclaimer Armor | 3/3 | 85.0% |
| sever / weapon | Siphon Blade | 3/3 | 87.3% |
| sever / weapon | Edge Sword | 0/3 | 0.0% |
| storm / armor | Grounding Mantle | 2/3 | 17.7% |
| storm / armor | Reclaimer Armor | 2/3 | 34.9% |

Fast and Full agree on 14/24 individual outcomes. Interpret their absolute win rates separately; Full is not bitwise deterministic.

## Recommended next patch — not applied

1. **Siphon Blade:** remove unconditional global Lifesteal from unrelated damage. Keep the existing 4 / 5 / 6 / 8 / 10% on damage originating from that blade, including its augment follow-ups and credited collisions. Keep drain marks at their existing strength: subsequent attacks/abilities can still heal against marked enemies. Keep damage, action duration, mark spreading and the shared 1% originating-action limit. This is a material effect-scope change: a blade that enables targeted draining, rather than free global sustain merely for equipping it.

2. **Reclaimer Armor:** test 3 / 4 / 5 / 6 / 7% global Lifesteal instead of 4 / 5 / 6 / 8 / 10%; below-half-health multiplier 1.5× instead of 2×. Keep its +20 base HP and normal rarity scaling. The actual healing ceiling stays 1% max HP per originating action. Reducing advertised rates may have a smaller practical effect when the cap is already binding, so test this separately before combining it with the blade change.

3. **Harvest:** keep current damage, 16-second recharge and 8 / 9 / 10 / 11 / 12% max-HP pulse budget for the first retest. Its benefit is real, but the persistent sources are larger outliers and removing them changes how much emergency healing a build needs. If it still dominates afterwards, test the pulse budget at 6 / 7 / 8 / 9 / 10% without also nerfing its damage or cooldown. That is a conditional follow-up, not a second simultaneous reduction.

4. **Weak alternatives:** do not broadly buff Edge Sword, all armor, or every offensive ultimate from these Fast win rates. Bulwark and Grounding can clear in Full where Fast loses. Redline is the most useful next focused check: three accelerated turns can save less time than its 0.65-second activation costs when magazines are already full. After isolating the healing changes, test six accelerated turns instead of three, retaining its existing haste scaling and 18-second recharge. Do not apply this together with the initial healing changes.

5. **Rarity/stack clarity:** show the shared per-action healing limit in the item comparison. Siphon Blade loadouts lose 38% of potential drain to the cap at Common and 60% at Legendary in this matrix; the number also includes other equipped healing sources. Higher rarity still wins much more often through damage, coverage and consistency. A bigger Lifesteal percentage should not be presented as a proportional increase in actual healing.

Run blade-only, armor-only and combined candidates on these same fixed matrices, then retest normal drafting. Require improvements for alternatives without making Siphon fail as an archetype. Inspect healing and finishing health as well as wins; no global enemy tuning is justified by these fixture results.

## Verification and reproducibility

- All primary and sensitivity cases completed with zero invalid, missing or duplicate case IDs. One primary case timed out; it counts as a non-win.
- Fifteen ordinary QA scenarios passed, each with 273 production-effect assertions. Thirty-two ordinary drafting games match the released v0.6.7 player on all 24 scalar gameplay fields.
- The original measurement build records a per-hit damageTaken diagnostic that can undercount terminal simultaneous hits. It is excluded from analysis. Health conservation instead uses raw incoming minus prevented damage, allowing terminal overkill; healing conservation and pre-ultimate identity checks pass. A reporting-only repair derives actual HP spent at export. Thirty matched controlled cases then reproduce every JSON field except wall time and that corrected diagnostic. Gameplay and balance values are unchanged.
- Primary, sensitivity and Full batches use assembly SHA-256 `ec1f93bbde9f538d84ac734baf480cbe6fee9fdbf2eadf869bb7825f8467ab78`. The reporting-only repaired assembly is `bcb3756a505c31368b0a38b4eeb9898acc12cff359a9fac27fd49b5697ae7476`.
- Full data remain local under `QA/Simulations/controlled-siphon-*`. Committed summaries, case inputs, reproduction scripts, regression checks and compact per-game metrics are in `QA/Balance/controlled-siphon/`.

```powershell
& '.\Build Windows.ps1' -OutputDirectory '.\Build\ControlledSiphon-v067-report'
& C:\Python314\python.exe QA/Balance/controlled-siphon/run.py --out QA/Simulations/controlled-repeat --seeds 12 --workers 4
```

`run.py --cases QA/Balance/controlled-siphon/rare-support-cases.json` repeats the sensitivity fixtures. `--mode Full --workers 2 --cases QA/Balance/controlled-siphon/full-cases.json` repeats the physics fixtures. Use fresh output folders. The analyzer expects the recorded folder names; change them to analyze a new experiment. No computer-use automation was used.
