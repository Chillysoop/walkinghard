"""Rebuild controlled comparisons from preserved per-game engine output."""
from pathlib import Path
import json,statistics,itertools
ROOT=Path(__file__).resolve().parents[2]
out=Path(__file__).resolve().parent
summary={}
for mode in ['fast','full']:
    folder=ROOT/'QA/Simulations'/('v089-control-'+mode)
    rows=[json.loads(line) for p in folder.glob('worker-*.jsonl') for line in p.read_text(encoding='utf-8-sig').splitlines() if line.strip()]
    groups=[]
    key=lambda r:(r['controlled']['context'],r['controlled']['rank'],r['controlled']['candidate'])
    for (context,rank,candidate),g in itertools.groupby(sorted(rows,key=key),key):
        a=list(g)
        groups.append(dict(context=context,rank=rank,candidate=candidate,runs=len(a),wins=sum(r['result']=='Complete' for r in a),valid=all(r['valid'] for r in a),timeouts=sum(r['result']=='TimedOut' for r in a),meanWave=statistics.mean(r['waveReached'] for r in a),meanHealth=statistics.mean(r['health'] for r in a),meanSeconds=statistics.mean(r['elapsed'] for r in a),meanDamageTaken=statistics.mean(r['controlled']['damageTaken'] for r in a),meanHealing=statistics.mean(r['controlled']['healing'] for r in a)))
    summary[mode]={'batch':json.loads((folder/'batch.json').read_text(encoding='utf-8-sig')),'groups':groups}
    # Keep reviewable per-case evidence without committing large per-action logs.
    trimmed=[{k:r[k] for k in ['controlled','result','seed','mode','valid','errors','loadout','waveReached','health','elapsed','wallSeconds','items','waveReports']} for r in rows]
    (out/(mode+'-games.json')).write_text(json.dumps(trimmed,indent=2),encoding='utf-8')
(out/'comparison.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
for mode,report in summary.items():
    print(mode,len(report['groups']),'groups',sum(g['runs'] for g in report['groups']),'games')
