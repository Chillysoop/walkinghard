"""Render the approved v0.6.8 experiment report from verified engine results."""
import csv
import json
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).parent
data = json.loads((ROOT / 'QA/v068-balance-results.json').read_text(encoding='utf-8'))
rows = list(csv.DictReader((OUT / 'summary.csv').open(encoding='utf-8')))
normal = data['normalDraft']
assert len(data['stages']) == 4 and normal['candidateProvenance']['runs'] == 1000
timing = normal['candidateProvenance']
seconds = (datetime.fromisoformat(timing['completedUtc']) - datetime.fromisoformat(timing['startedUtc'])).total_seconds()

def find(stage, item, support='Common', rank=0):
    return next(r for r in rows if r['stage'] == stage and r['item'] == item and r['support'] == support and int(r['rank']) == rank)

def pct(row):
    return f"{float(row['winPct']):.1f}%"

names = {'siphonblade':'Siphon Blade','sword':'Edge Sword','breachhammer':'Breach Hammer','fists':'Kinetic Fists','pistol':'Pulse Pistol','reclaimer':'Reclaimer Armor','plate':'Bulwark Plate','reactive':'Reactive Shell','grounding':'Grounding Mantle','duelist':'Duelist Jacket','harvest':'Harvest','tempest':'Tempest Array','singularity':'Singularity Burst','cyclone':'Razor Cyclone','redline':'Redline'}
lines = [
    '# Walking Hard 0.6.8 — approved balance retest',
    '',
    '29 October 2026. Implements the approved recommendations from [the controlled Siphon review](controlled-siphon-review.md). No further balance changes are included.',
    '',
    '## Applied changes',
    '',
    '- Siphon Blade retains 4/5/6/8/10% drain on damage credited to that blade, including augment follow-ups and collisions. It no longer grants global Lifesteal merely by being equipped. Existing marks still let other attacks drain marked targets; mark strength, duration, spread, weapon damage and action time are unchanged.',
    '- Reclaimer grants 3/4/5/6/7% global Lifesteal; only its own contribution becomes 1.5 times stronger below half health. Health bonus and rarity scaling are unchanged.',
    '- Harvest stays at its existing damage, 16-second recharge and 8/9/10/11/12% maximum-health pulse budget.',
    '- After testing healing separately, Redline was tested with six accelerated sequence turns instead of three. It retains 18-second recharge, ammo refill and 20/23/26/29/32% haste. A simultaneous weapon pair consumes one turn; the ultimate activation consumes none.',
    '- Item descriptions distinguish blade/mark healing from global healing. The character sheet labels its global stat GLOBAL LIFESTEAL. The shared 1% maximum-health budget per originating action remains in force.',
    '',
    '## Method and controls',
    '',
    'Same fixed matrix as the earlier review: 3,600 primary cases with Common support plus 480 sensitivity cases with Rare support. Focal item rarity ranges from Common through Legendary. Four contexts (sever, storm, guns and stacked Siphon), eight waves, normal 280 base health plus actual armor, no augments, no later drafting, ultimate unlocked after checkpoint three, 600 simulated-second timeout. These frozen loadouts measure relative differences; their absolute win rates are not ordinary-player difficulty estimates.',
    '',
    '420 controls on the new binary with all three legacy rules reproduce every gameplay field, including item attribution and wave reports, from the old matrix. Version, wall time and experimental diagnostics are excluded. Each stage then reruns all affected loadouts; unaffected results are explicitly reused, not counted as newly simulated games. Healing accounting reconciles potential drain minus cap loss and overheal with actual item-attributed healing.',
    '',
    '| Stage | Games actually rerun | Gains / losses versus prior rules | Wins in reconstructed 4,080-case matrix |',
    '| --- | ---: | ---: | ---: |',
    '| Baseline | Previously recorded | — | 1,559 |',
]
for key, label in [('blade','Blade only'),('armor','Armor only'),('combined','Both healing changes'),('final','Healing changes + six-turn Redline')]:
    d=data['stages'][key]
    lines.append(f"| {label} | {d['executedGames']:,} | {d['change']['gain']} / {d['change']['loss']} | {d['fullMatrix']['wins']:,} |")
lines += [
    '',
    'Blade-only and armor-only compare against baseline; combined also compares against baseline. Redline compares against the combined healing stage. There are 4,680 newly executed affected cases, plus 420 legacy controls. The final matrix has one simulated timeout, counted as a non-win; there are no invalid, missing or duplicate cases.',
    '',
    '## Matching-rarity outcomes',
    '',
    'Final primary matrix, 48 matched context/seed cases per item per rarity; pooled totals give equal weight to every tier. Baseline pooled figures are shown for context. Other equipped items in a fixture also receive the approved changes, so use the isolated stages above when attributing causes.',
    '',
    '| Item | Common | Uncommon | Rare | Epic | Legendary | Final pooled | Old pooled |',
    '| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |',
]
for item,name in names.items():
    cells=[pct(find('final',item,rank=r)) for r in range(1,6)]
    lines.append('| '+name+' | '+' | '.join(cells+[pct(find('final',item)),pct(find('baseline',item))])+' |')
lines += [
    '',
    'Siphon Blade falls from 80.8% to 34.2% pooled wins, compared with Edge Sword at 27.1%. Its average realized whole-build healing falls from 1.79 to 0.95 HP/s. It remains an effective sustain weapon without conferring healing on unrelated attacks to unmarked enemies.',
    '',
    'Reclaimer falls from 52.1% to 43.8% in its isolated test, then 42.5% in the final matrix. It remains ahead of the other armor in these fixtures, particularly at Epic and Legendary. The intended sustain identity survives; this is not evidence that the armor set is now fully balanced.',
    '',
    'Harvest stays at 34.2% versus 25.0% for the other ultimate candidates in the primary matrix. With Rare support it still wins 80/80, versus Tempest at 63/80. Keep it unchanged in this release: do not combine another healing reduction with the persistent-source changes. If later tests show it is still compulsory, the previously proposed 6–10% pulse budget remains an unapproved follow-up experiment.',
    '',
    'Redline yields eight gains and seven losses across its 920 affected cases, a net one additional win. Its focal primary slice remains 60/240. The six-turn burst works mechanically, but this evidence does not show that it solves Redline’s competitive weakness. Sequence timing can change targets and encounter alignment, so a longer buff need not improve every seed. Do not call this a proven balance fix or add another buff without a separate comparison.',
    '',
    'Rare-support sensitivity (80 games per focal item):',
    '',
    '| Item | Old wins | Final wins |',
    '| --- | ---: | ---: |',
]
for item in ['siphonblade','sword','reclaimer','plate','harvest','tempest']:
    lines.append(f"| {names[item]} | {find('baseline',item,'Rare')['wins']}/80 | {find('final',item,'Rare')['wins']}/80 |")
lines += [
    '',
    '## Ordinary drafting retest',
    '',
    f"1,000 mixed-policy Fast games, seeds 12000–12999, eight waves, normal health, normal reward choices and ultimate progression. Eight-wave completions: **{normal['baselineWins']}/1000 → {normal['candidateWins']}/1000**. Cleared wave three: **{normal['baselineClear3']}/1000 → {normal['candidateClear3']}/1000**. Paired outcomes: {normal['comparison']['gain']} gains, {normal['comparison']['loss']} losses. This batch took {seconds:.1f} seconds end to end on this machine. All 1,000 games were valid, with no timeouts.",
    '',
    '| Strategy | Games | Old wins | New wins |',
    '| --- | ---: | ---: | ---: |',
]
for policy,d in normal['policies'].items():
    lines.append(f"| {policy} | {d['games']} | {d['oldWins']} | {d['newWins']} |")
lines += [
    '',
    'Ordinary drafting loses 4.1 percentage points overall and 1.2 points at wave three. Sever takes the largest drop (44.0% to 33.7%); this is consistent with removing free healing from a favored sword, but the strategy-level result alone does not isolate the cause. The balanced policy stays at 77.2%, and the controlled sustain fixtures retain strong outcomes. Preserve this patch for player testing; any follow-up should compare sever alternatives and Redline/gun builds directly rather than undoing the scoped healing rule or raising global difficulty.',
    '',
    '## Full physics checks',
    '',
    'Six preselected matched cases use native body/contact physics with ordinary health. Three clear, versus four of these same six in the prior baseline. These are coverage checks, not a statistically reliable win-rate estimate. Full physics is not bitwise deterministic; the prior calibration agreed with Fast on only 14 of 24 individual outcomes.',
    '',
    '| Case | Previous result / HP | New result / HP |',
    '| --- | --- | --- |',
]
for d in data['full']['cases']:
    lines.append(f"| {d['id']} | {d['old']} / {d['oldHealthPct']:.1f}% | {d['new']} / {d['newHealthPct']:.1f}% |")
lines += [
    '',
    '## Reproduction and limits',
    '',
    'The exact candidate assembly SHA-256 is `b2d257d971d9d7d2a0fee4b9bed837bbad1c3dc63d4d311d12000dfd2c2569d3`. No game code changed after that build. Baseline and candidate provenance are in [the JSON results](v068-balance-results.json); per-case old/new outcomes and healing are in `Balance/v068/case-results.csv`, with rarity summaries in `Balance/v068/summary.csv`. Full raw worker files remain local in `QA/Simulations/`.',
    '',
    '`QA/Balance/v068/prepare.py` writes the exact cases and rule files. `run_experiments.py` verifies the existing 420 legacy controls, then runs blade, armor, combined and Redline sequentially. `analyze.py` validates and aggregates complete batches; `write_report.py` renders this report. Fresh output folders are required when rerunning. The controlled runner accepts `--player`, `--cases`, `--balance`, `--out`, `--mode Full` and `--workers`.',
    '',
    'Fast mode approximates contacts and body motion. These results justify the approved targeted changes, not global enemy tuning or broad buffs to every alternative. [Verification](v068-verification.md) records the 62 passing QA scenarios, launcher checks and the unconfirmed visual check. No Computer Use or scheduled job was used.',
    '',
]
(ROOT/'QA/v068-balance-review.md').write_text('\n'.join(lines),encoding='utf-8')
print('Wrote QA/v068-balance-review.md')
