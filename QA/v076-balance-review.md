# Deadeye controlled comparisons — 0.7.6

October 30, 2026. **1,572 valid games: 1,524 Fast screenings and 48 fresh-process Full/PhysX confirmations.** No missing/duplicate cases, simulation integrity failures or timeouts. No experimental balance overrides were applied. The two new items retain their initial authored values; existing item values are unchanged.

## Recommendation, not applied

1. **Keep Suppressive Volley at 12 Common damage, 0.75s action and 0.70s stagger.** The fast model understates its rifle performance. Full physics cleared all 12 Volley cases, and it finished faster than Breaker on average. Its narrower interrupt coverage is an actual tradeoff; a blanket damage increase is not supported.
2. **Make Recoil Harness preserve its stored shot when the base gun hit already killed the target.** Spend it on the next gun contact with a surviving target, once across both hands and piercing/ricochet contacts. Keep Common block and bonus at **12 / 12**, guard duration at **6s**, and the existing rarity scaling. This is the next proposed behavior test, not a shipped rule. Presently `SpendRecoil` clears the charge before checking whether the target survived. All six Full rifle runs absorbed damage but dealt **zero armor-owned bonus damage**. Raising the bonus number would not fix that wasted-charge path.
3. **Keep Duelist, Counterguard and Reclaimer unchanged in this update.** Counterguard and Reclaimer look very strong in the weaker-loadout Fast tests, but those cohorts were not confirmed in Full. The large observed Fast/Full discrepancy makes a defensive nerf premature. A follow-up should confirm those specific weaker-loadout pairs in Full before changing them.

## What was held constant

The first batch contains 1,200 eight-wave games: rifle, paired pistols and pistol/Siphon Blade hybrid contexts; all five focal rarities; ten identical seeds per alternative. Supporting items stay Rare. Only the focal ability or armor changes. Normal base integrity is 280. There are no intervening drafts, fusions or gear upgrades. Redline is equipped only at wave four, preserving checkpoint-three ultimate progression. Routes remain Standard, and action order is fixed within each comparison.

- Ability alternatives: Suppressive Volley, Breaker, Arc Bolt, Counterguard. Other abilities: Vector Tether and Drain Brand. The focal action comes first, followed by the paired basics, Tether and Drain.
- Armor alternatives: Recoil Harness, Duelist Jacket, Bulwark Plate, Reclaimer Armor. Abilities: Arc Bolt, Vector Tether, Counterguard, with Arc first and then both basics.
- Shared gear: Surveyor Visor, Loader Greaves, Overclock and Redline. The hybrid has separate, nonmatching weapon attacks. The rifle preserves its linked two-handed pair.

The initial armor cohort mostly saturated survival. A second 324-game Fast batch therefore tested Common supporting gear for armor (three contexts, Common/Rare/Legendary focal items, six seeds, four alternatives). It also tested Volley/Breaker/Counterguard immediately followed by Arc Bolt in rifle and paired-pistol builds (same three focal rarities and six seeds, Rare support). These are separate cohorts, not pooled results.

Full confirmation used two seeds at Common, Rare and Legendary in rifle and pistol contexts, comparing Volley/Breaker and Recoil/Duelist: 48 games, each in a fresh Unity process. Six cases per table row represent three rarities and **only two distinct seeds**; this is a directional check, not a precise population estimate. PhysX is not bitwise reproducible.

## Full physics results

All rows completed 6/6 games. Health is the mean fraction of each run's own maximum, so armor HP bonuses do not inflate the comparison. Time is total simulated combat time, not wall time.

| Context | Item | Ending health | Combat time | Item absorption | Item direct damage |
|---|---|---:|---:|---:|---:|
| Rifle | Suppressive Volley | 74.7% | 339.4s | — | 3,943 |
| Rifle | Breaker | 74.5% | 347.9s | — | 5,018 |
| Paired pistols | Suppressive Volley | 99.9% | 319.1s | — | 4,254 |
| Paired pistols | Breaker | 94.5% | 344.4s | — | 3,849 |
| Rifle | Recoil Harness | 96.2% | 333.9s | 26.2 | **0.0** |
| Rifle | Duelist Jacket | 94.2% | 336.7s | 18.6 | 0.0 |
| Paired pistols | Recoil Harness | 99.5% | 314.0s | 10.1 | **8.9** |
| Paired pistols | Duelist Jacket | 98.1% | 313.1s | 5.7 | 0.0 |

Recoil's defensive result is competitive in these strong builds. Its offensive identity is underexpressed. Counterguard absorbs first, reducing opportunities to charge Recoil; lethal base shots can then discard charges that were earned. This supports fixing reliability before increasing damage or block capacity. It does not establish that Recoil is universally weaker than Duelist.

Volley also reduces waits for short-range targets. At Rare in the Full pistol comparison it spent 15.4 seconds waiting versus Breaker's 77.8, while both won 2/2. At Rare with the rifle, Volley ended at 66.4% health versus Breaker's 84.0%; the three-rarity average hides that variation. No claim of strict dominance is warranted.

## What the Fast model flagged

At Rare, the initial rifle screening produced **0/10 Volley wins versus 10/10 Breaker wins**. The matching Full confirmation was **2/2 for each**. This reverses the apparent survival conclusion and is the main reason not to buff Volley from Fast results alone.

With the immediate Arc Bolt follow-up, paired-pistol Volley won 18/18 Fast cases, ending at 60.1% health on average; Breaker won 14/18 and ended at 19.7%. Counterguard won 18/18 at 96.7%. Rifle/Arc follow-up results were Volley 1/18, Breaker 3/18 and Counterguard 18/18. The supportive damage/tempo role works particularly well for pistols; rifle screening remains model-sensitive.

With Common supporting gear, armor results were:

| Context | Recoil wins | Duelist wins | Bulwark wins | Reclaimer wins |
|---|---:|---:|---:|---:|
| Rifle | 0/18 | 0/18 | 0/18 | 18/18 |
| Paired pistols | 18/18 | 18/18 | 18/18 | 18/18 |
| Hybrid | 0/18 | 13/18 | 6/18 | 18/18 |

Recoil's reload dependence makes it a poor substitute for Duelist's additional sever trigger in the hybrid. At Rare in the Common-support pistol cohort, Recoil ended at 23.1% health versus Duelist's 31.8%, while contributing 162.7 bonus damage and finishing about 3.4 seconds faster. Reclaimer is the strongest screening result here; lack of Full confirmation limits any nerf recommendation.

## Evidence and reproduction

`Balance/v076/results/summary.csv` contains every context/rarity result. `pairs.csv` records seed-matched outcome and health differences. `evidence/*-samples.jsonl` preserves each game's controlled metrics and focal contribution; batch manifests record assembly hashes, validation counts and wall time. Full source traces remain locally under ignored `QA/Simulations/v076-*` folders.

```powershell
python QA/Balance/v076/make_cases.py
python QA/Balance/controlled-siphon/run.py --out QA/Simulations/fresh-fast --cases QA/Balance/v076/fast-cases.json --mode Fast --workers 4 --player Build/Synergy-v076-shipping/WalkingHard.exe
python QA/Balance/controlled-siphon/run.py --out QA/Simulations/fresh-followup --cases QA/Balance/v076/followup-cases.json --mode Fast --workers 4 --player Build/Synergy-v076-shipping/WalkingHard.exe
python QA/Balance/v076/run_full.py --out QA/Simulations/fresh-full --cases QA/Balance/v076/full-cases.json --workers 3 --player Build/Synergy-v076-shipping/WalkingHard.exe
```

Fast batches took 224.9s and 101.4s under concurrent local verification; Full took 1,285.8s. These are eight-wave games through the boss, not shortened encounters. Fast and Full use identical authored combat values but different movement/contact simulation. Fixed loadouts and selected seeds do not estimate ordinary draft-run difficulty.

The original Fast assembly was `3187ddff739e08e05bb009095ecf1ed19586b5e27392d04164ef25fe3c18ccef`. Follow-up/Full used `7f1e2f637dac1cdc10b2d3e3ce6b3f95d6bf42aa9f4e0962b88f1c087a3e2b87`. Shipping is `c01186feb7a19029421f23951d175146170cf137a99faa93581b23e468a48b4c`. Changes between those assemblies were confined to QA expectations, UI partner filtering/copy and a diagnostic item count; damage, targeting, physics, item values and controlled-case behavior are identical.
