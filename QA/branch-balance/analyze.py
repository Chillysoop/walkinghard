"""Summarize matched v0.9.3 engine cases, keeping Fast and Full separate."""
import collections
import gzip
import json
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
groups = []
raw = []
manifests = []
for mode in ('fast', 'full'):
    for variant in ('baseline', 'candidate'):
        source = ROOT / 'QA/Simulations' / f'v093-{mode}-{variant}'
        manifest = json.loads((source / 'batch.json').read_text())
        manifests.append(dict(variant=variant, **manifest))
        rows = [json.loads(line) for path in sorted(source.glob('worker-*.jsonl')) for line in path.read_text().splitlines()]
        assert len(rows) == manifest['runs'] and all(r['valid'] for r in rows)
        grouped = collections.defaultdict(list)
        for row in rows:
            grouped[(row['controlled']['context'], row['controlled']['rank'])].append(row)
            raw.append(dict(comparison=variant, **row))
        for (context, rank), samples in grouped.items():
            groups.append(dict(mode=mode, variant=variant, context=context, rank=rank, n=len(samples),
                wins=sum(r['result'] == 'Complete' for r in samples),
                meanHealth=sum(r['health'] for r in samples)/len(samples),
                meanWaves=sum(r['waves'] for r in samples)/len(samples)))
with gzip.open(HERE / 'engine-results.jsonl.gz', 'wt', encoding='utf-8') as out:
    for row in raw: out.write(json.dumps(row, separators=(',', ':')) + '\n')
(HERE / 'summary.json').write_text(json.dumps(dict(manifests=manifests, groups=groups), indent=2))
text = ['# Matched engine outcomes', '', '| Mode | Kit | Rarity | Baseline wins | Candidate wins | HP before → after | Cleared waves before → after |', '|---|---|---|---|---|---|---|']
for a in groups:
    if a['variant'] != 'baseline': continue
    b = next(g for g in groups if g['variant']=='candidate' and all(g[k]==a[k] for k in ('mode','context','rank')))
    text.append(f"| {a['mode']} | {a['context']} | {a['rank']} | {a['wins']}/{a['n']} | {b['wins']}/{b['n']} | {a['meanHealth']:.1f} → {b['meanHealth']:.1f} | {a['meanWaves']:.2f} → {b['meanWaves']:.2f} |")
(HERE / 'engine-summary.md').write_text('\n'.join(text)+'\n', encoding='utf-8')
print(f'{len(raw)} valid retained outcomes')
