"""Validate and summarize the matched Full/PhysX rifle study without rerunning combat."""
import argparse
import json
import statistics
from pathlib import Path

p = argparse.ArgumentParser()
p.add_argument('--input', type=Path, required=True)
p.add_argument('--out', type=Path, default=Path(__file__).parent)
a = p.parse_args()
jobs = json.loads((a.input / 'cases.json').read_text(encoding='utf-8'))['cases']
rows = [json.loads(line) for path in sorted(a.input.glob('worker-*.jsonl'))
        for line in path.read_text(encoding='utf-8-sig').splitlines()]
by_id = {c['id']: c for c in jobs}
assert len(rows) == len(jobs) == len({r['controlled']['id'] for r in rows}) == 32
assert {r['controlled']['id'] for r in rows} == set(by_id)
for r in rows:
    c = by_id[r['controlled']['id']]
    assert r['valid'] and not r['errors'] and r['result'] in ('Complete', 'Defeated')
    assert r['seed'] == c['seed'] and r['controlled']['rank'] == c['rank'] == 3
    assert r['controlled']['initialHealth'] == 280 and r['controlled']['ultimateEquipWave'] == 4
    equipped = {i['item']: i for i in r['items'] if i['equipped']}
    assert equipped['rifle']['rarity'] == 3
    assert equipped['rifle']['module'] == ('aug-Livewire' if c['modules'][0] else '')
    assert equipped['VectorTether']['module'] == ('aug-Focused' if c['modules'][4] else '')

summary = []
for variant in ('native', 'livewire', 'focused', 'both'):
    subset = [r for r in rows if r['controlled']['candidate'] == variant]
    assert {r['seed'] for r in subset} == set(range(81001, 81009))
    summary.append(dict(variant=variant, n=len(subset), wins=sum(r['result'] == 'Complete' for r in subset),
                        meanWave=statistics.mean(r['waveReached'] for r in subset),
                        meanKills=statistics.mean(r['kills'] for r in subset),
                        meanDamage=statistics.mean(r['controlled']['damageTaken'] for r in subset),
                        meanSurvivalSeconds=statistics.mean(r['elapsed'] for r in subset)))
meta = json.loads((a.input / 'batch.json').read_text(encoding='utf-8'))
meta.update(valid=32, invalid=0, missing=0, duplicateCases=0, timeouts=0,
            wins=sum(r['result'] == 'Complete' for r in rows),
            validation='analyze.py checks IDs, seeds, health, rarity, equipped rifle and both module slots. '
            'Original runner rejected variant labels as item IDs after all simulations completed; focalItem now separates them.')
a.out.mkdir(parents=True, exist_ok=True)
for name, data in [('results', rows), ('summary', summary), ('batch', meta)]:
    (a.out / (name + '.json')).write_text(json.dumps(data, indent=2) + '\n', encoding='utf-8')
print(json.dumps(summary, indent=2))
