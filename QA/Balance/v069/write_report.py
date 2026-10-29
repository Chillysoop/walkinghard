import json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3];d=json.loads((ROOT/'QA/v069-balance-results.json').read_text(encoding='utf-8'));rs=d['summary']
def row(support,category,context,candidate):return next(r for r in rs if r['mode']=='Fast' and r['support']==support and r['category']==category and r['context']==context and r['candidate']==candidate and r['rank']==0)
lines=['# Walking Hard 0.6.9 — gun and sever comparisons','','29 October 2026. Recommendations only: this release changes feedback and reporting, not balance.','',
'## Method','',
'1,680 fixed-loadout Fast games: 480 weapon-layout cases, 600 gun-ultimate substitutions, and 600 sever-armor substitutions. Every primary comparison uses twelve identical encounter seeds and five matching rarities. Both carried weapons use the focal rarity, including when comparing them to one two-handed item. Other support items are Common; the armor or ultimate under test also receives the focal rarity. A pair costs two owned items and two sockets, while a two-handed weapon costs one item and one socket: this matches rarity, not acquisition cost.', '',
'All runs use normal 280 base health plus armor, eight waves, no later rewards or augments, and a 600 simulated-second timeout. Sequence: setup ability, weapon basics, then two payoff abilities. Ultimate selection remains after checkpoint three. Gun setup uses Vector Tether / Arc Bolt / Breaker with Surveyor, Duelist, Loader and Overclock. Sever uses Gravity Well / Reaping Cut / Steel Jab with Salvager, Duelist, Momentum and Salvage Magnet.', '',
'All 480 primary weapon cases lost before completing eight waves. To avoid ranking equal zero-win outcomes, a follow-up sensitivity slice raises supporting gear/abilities to Rare: 504 games across Common/Rare/Legendary focal tiers and six matched seeds. This slice was added after seeing the primary floor. Six Full physics cases were selected before Fast results; they cover one Rare seed for six representative loadouts. There are no invalid, missing, duplicate or timed-out cases.', '',
'## Weapon layouts','',
'| Layout | Common support: mean wave reached | Target-wait share | Reload weapon-seconds / minute | Rare support wins |',
'| --- | ---: | ---: | ---: | ---: |']
for context,candidate,label in [('gun-pistols','pistol','Paired pistols'),('gun-scatter-pistol','scattergun','Scattergun + pistol'),('gun-rifle','rifle','Rail Rifle'),('gun-pistol-sword','pistol','Pistol + sword'),('sever-swords','sword','Paired Edge Swords'),('sever-greatsword','greatsword','Execution Greatsword'),('sever-siphon-sword','siphonblade','Siphon + Edge Sword'),('sever-sword-hammer','sword','Sword + Breach Hammer')]:
 a=row('Common','weapon',context,candidate);b=row('Rare','weapon',context,candidate)
 lines.append(f"| {label} | {a['meanWave']:.2f} | {a['targetWaitPct']:.1f}% | {a['reloadWeaponSecondsPerMinute']:.1f} | {b['wins']}/{b['games']} |")
lines += ['',
'Reload timers are summed across weapons; simultaneous reloads overlap, so this is not whole-sequence downtime. Target waits occur serially at the next occupied action. Survivorship and encounter length also affect these averages; read them alongside the matched outcomes.', '',
'The rifle trails paired pistols in both support conditions. It reaches wave 4.12 versus 6.38 with Common support, then wins 0/18 versus 12/18 with Rare support. The Full sample reaches wave six with the rifle and wave eight with pistols (both lose). This is a consistent direction, not a precise population effect size.', '',
'Greatsword reverses its relative position with stronger support: 9/18 wins versus 6/18 for paired swords. Do not broadly buff it based on the Common-support floor. Siphon + sword wins 18/18 in the stronger slice; its sustain remains effective after the previous scope change.', '',
'The gun setup itself also stalls. At Rare weapons and Rare support, the paired-pistol build spends an average 44.2 seconds waiting on Breaker, 27.4 on Vector Tether, and only 0.3 per pistol. A short-range action can hold up the whole sequence despite long-range guns. This is the existing strict-order contract, not a reload bug. These are descriptive delays, not a tested counterfactual for replacing an ability.', '',
'## Ultimate and armor substitutions','',
'Rare-support slice, six seeds at each of three focal rarities (18 games per candidate):','',
'| Ultimate | Paired pistols | Rail Rifle |','| --- | ---: | ---: |']
for item in ['redline','tempest','singularity','cyclone','harvest']:
 a=row('Rare','ultimate','gun-pistols',item);b=row('Rare','ultimate','gun-rifle',item)
 lines.append(f"| {item.title()} | {a['wins']}/18 | {b['wins']}/18 |")
lines += ['',
'Redline is not uniformly weak: it ties Tempest and Singularity at 12/18 with supported pistols. It reduces summed reload time from about 4.0 to 1.6 weapon-seconds per minute there. With the rifle, the difference is only about 3.0 to 2.6, and Redline still wins 0/18. Fixing a rifle shortfall with a universal Redline buff would also strengthen pistol builds that already benefit.', '',
'| Armor | Paired swords | Greatsword |','| --- | ---: | ---: |']
for item in ['duelist','plate','reactive','grounding','reclaimer']:
 a=row('Rare','armor','sever-swords',item);b=row('Rare','armor','sever-greatsword',item)
 lines.append(f"| {item.title()} | {a['wins']}/18 | {b['wins']}/18 |")
lines += ['',
'Reclaimer remains the strongest sustain option in this slice. Bulwark Plate is a credible alternative (17/18 with paired swords); Duelist is substantially less consistent (7/18). Those are contextual comparisons, not proof that Grounding is weak in its intended lightning/impact build.', '',
'## Recommended next tests — not applied','',
'1. **Rail Rifle: test Common damage 42 → 52 (+23.8%)**, retaining its 0.90-second action, four-round magazine, piercing and rarity effects. This is a one-variable test of its damage return for occupying both mounts. Repeat against paired pistols at all five matching rarities and include additional Full seeds before accepting it.', '',
'2. **Duelist Jacket: test three guarded hits instead of two**, preserving mitigation, duration, first-hit stagger and refresh/no-stacking behavior. This gives the reload/sever defense more coverage between triggers. Test separately from the rifle change against Bulwark and Reclaimer in both sword and gun setups.', '',
'3. **Keep Greatsword, Redline, Reclaimer and Harvest unchanged in that first comparison.** Their results vary with supporting equipment, and the current data do not justify another simultaneous global sustain reduction. Add a gun-drafting comparison that values reachable ranges and defensive support before attributing all gun-policy losses to weapon numbers. Preserve strict sequence order; do not silently skip waiting actions.', '',
'## Full physics and reproducibility','',
'| Case | Result | Wave reached | Ending HP |','| --- | --- | ---: | ---: |']
for r in rs:
 if r['mode']=='Full' and r['rank']==0:lines.append(f"| {r['context']} / {r['category']} / {r['candidate']} | {'Complete' if r['wins'] else 'Defeated'} | {r['meanWave']:.0f} | {r['meanHealth']:.1f} |")
lines += ['',
'Only the Reclaimer sword case clears in this six-game physics sample. Six single-seed examples are coverage, not statistical certainty; Fast approximates contact/body motion and has previously disagreed with Full.', '',
'Case inputs, per-game metrics, rarity summaries and scripts are in `QA/Balance/v069/`. `prepare.py` writes the primary, Rare-support and Full inputs; `run.py` executes the three primary batches using the controlled runner. Sensitivity and Full commands use their saved `--cases` files and fresh `--out` directories. `analyze.py` verifies the 64-seed ordinary-gameplay regression, matching weapon rarities, complete batches, and identical pre-ultimate wave reports; `write_report.py` produces this report.', '',
'Balance experiments used assembly `4e86c171281a1ce033da5bc3a8c64a1e8bd255030aa7f76f9740e1a88f9c1bb7`. Later changes add more recorded synergy notes and correct Recovery Treads from blocked damage to healing; neither changes combat. The final player regression and binary hash are recorded in [verification](v069-verification.md). Data and batch provenance: [v069-balance-results.json](v069-balance-results.json). No balance changes from this report are applied.', '']
(ROOT/'QA/v069-balance-review.md').write_text('\n'.join(lines),encoding='utf-8')
