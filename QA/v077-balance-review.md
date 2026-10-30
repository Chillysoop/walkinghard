# Recoil charge preservation — 0.7.7

October 30, 2026. **216 new matched games: 204 Fast and 12 Full/PhysX**, compared with the saved 0.7.6 cases using identical seeds, rarities, fixed loadouts, action order and routes. All reports are valid, with no missing/duplicate cases, integrity errors or timeouts. These are full eight-wave runs with normal 280 base integrity, not shortened encounters or the high-health regression fixtures.

## Applied change and conclusion

The user-approved behavior change moves the dead-target check ahead of both charge consumption and the originating action's proc allowance. A lethal base gun hit now saves Recoil for a surviving piercing/ricochet target, the other pistol, or a later action. One charge still produces one armor-owned bonus; kills and damage exclude overkill and remain credited to the armor.

**Keep this behavior and the existing numbers.** The offensive effect now contributes in every Full rifle case, where it previously contributed nothing. Win counts did not change in any tested cohort. This improves the item's reliability without evidence for another numerical buff. It does not establish equal strength against every alternative.

Recoil remains Common **12 block / 12 bonus damage / 6s guard**, with existing rarity scaling. Volley remains **12 damage / 0.75s action / 0.70s stagger** at Common. Duelist, Counterguard, Reclaimer and all other item values are unchanged. The prior recommendation to confirm weaker-loadout defensive comparisons in Full remains outstanding; no further balance changes were applied.

## Full physics comparison

Each row has six matched cases: Common, Rare and Legendary Recoil, with two distinct seeds per rarity. Supporting gear is Rare. Every case won before and after. Health is the mean percentage of each run's own maximum; time is simulated combat time, not wall time.

| Context | Recoil bonus damage before → after | Ending health before → after | Combat time before → after | Recoil absorption before → after |
|---|---:|---:|---:|---:|
| Rifle | **0.0 → 105.1** | 96.2% → 99.1% | 333.9s → 338.3s | 26.2 → 34.2 |
| Paired pistols | **8.9 → 26.2** | 99.5% → 99.3% | 314.0s → 313.3s | 10.1 → 10.1 |

The rifle charge now reliably reaches a surviving contact. The small pistol health/time differences and longer rifle finish time should not be read as precise balance effects: PhysX is not bitwise reproducible, there are only two distinct seeds, and these strong builds already saturate survival. Changing hit outcomes also changes later positioning, damage intake and reload opportunities. The isolated production-mechanic checks verify the intended cause directly at all five rarities.

## Fast comparison

Rare-support groups contain 50 cases each: all five focal rarities and ten seeds. Common-support groups contain 18 each: Common/Rare/Legendary focal rarities and six seeds. These cohorts are kept separate. The hybrid carries a pistol and Siphon Blade.

| Supporting gear | Context | Wins before → after | Mean Recoil bonus damage before → after |
|---|---|---:|---:|
| Rare | Rifle | 50/50 → 50/50 | 12.2 → 100.6 |
| Rare | Paired pistols | 50/50 → 50/50 | 13.5 → 42.9 |
| Rare | Hybrid | 50/50 → 50/50 | 58.1 → 130.9 |
| Common | Rifle | 0/18 → 0/18 | 57.8 → 115.3 |
| Common | Paired pistols | 18/18 → 18/18 | 169.8 → 294.4 |
| Common | Hybrid | 0/18 → 0/18 | 58.7 → 138.6 |

Common-support pistols ended at 27.9% health versus 25.8% previously. The weak rifle and hybrid cohorts still fail, so saving the charge does not by itself solve their survival limitations. Those cohorts have no new Full confirmation. The earlier Fast/Full disagreements still caution against a defensive nerf based solely on this screening model.

## Evidence and reproduction

`Balance/v077/pairs.csv` preserves each matched before/after result. `summary.csv` separates mode and supporting-loadout context. `evidence/*-samples.jsonl` stores per-run outcomes and focal contribution. `results.json` includes current and baseline batch provenance. Inputs are exact Recoil subsets of the committed 0.7.6 inputs; this update reused their saved baselines instead of rerunning unrelated alternatives.

All new runs and regression checks used shipping assembly SHA-256 `4e0e096d60d0363483e091252caebf53ec66a0f2a82635e13768a250ed5ddaf8`. Prior Rare-support Fast baseline: `3187ddff739e08e05bb009095ecf1ed19586b5e27392d04164ef25fe3c18ccef`; prior Common-support Fast and Full baseline: `7f1e2f637dac1cdc10b2d3e3ce6b3f95d6bf42aa9f4e0962b88f1c087a3e2b87`. The 0.7.6 report documents the QA/UI-only differences from its final shipping assembly.

```powershell
python QA/Balance/v077/compare.py --prepare
python QA/Balance/controlled-siphon/run.py --out QA/Simulations/fresh-v077-fast --cases QA/Balance/v077/fast-cases.json --mode Fast --workers 3 --player Build/Recoil-v077/WalkingHard.exe
python QA/Balance/v076/run_full.py --out QA/Simulations/fresh-v077-full --cases QA/Balance/v077/full-cases.json --workers 3 --player Build/Recoil-v077/WalkingHard.exe
python QA/Balance/v077/compare.py QA/Simulations/fresh-v077-fast QA/Simulations/fresh-v077-full
```

Fast took **63.1 seconds**; Full took **419.3 seconds**, using three fresh-process workers and running alongside regression checks. Fixed loadouts do not estimate ordinary draft-run difficulty, and headless success does not prove player comprehension.
