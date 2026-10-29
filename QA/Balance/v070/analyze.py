import json,statistics
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
def rows(name):
    return [json.loads(line) for f in (ROOT/'QA/Simulations'/name).glob('worker-*.jsonl') for line in f.read_text(encoding='utf-8-sig').splitlines() if line.strip()]
baseline={r['controlled']['id']:r for g in ['weapons','ultimates','armor','rare-support'] for r in rows('v069-'+g)}
for name in ['rifle','duelist']:
    for row in rows('v070-'+name+'-baseline'):baseline[row['controlled']['id']]=row
def summary(rs):
    return dict(n=len(rs),wins=sum(r['result']=='Complete' for r in rs),wave=round(statistics.mean(r['waveReached'] for r in rs),3))
out={}
for name in ['rifle','duelist']:
    new=rows('v070-'+name); old=[baseline[r['controlled']['id']] for r in new]
    details={}
    for context in sorted({r['controlled']['context'] for r in new}):
        for support in [False,True]:
            group=[r for r in new if r['controlled']['context']==context and ('support-rare' in r['controlled']['id'])==support]
            if group:details[context+('/rare-support' if support else '/common-support')]=dict(before=summary([baseline[r['controlled']['id']] for r in group]),after=summary(group))
    out[name]=dict(before=summary(old),after=summary(new),groups=details)
full={r['controlled']['id']:r for r in rows('v070-full-baseline')}
for name in ['rifle','duelist']:
    new=rows('v070-full-'+name)
    out['full-'+name]=[dict(id=r['controlled']['id'],before={k:full[r['controlled']['id']][k] for k in ['result','waveReached','kills']},after={k:r[k] for k in ['result','waveReached','kills']}) for r in new if r['controlled']['id'] in full]
(Path(__file__).parent/'results.json').write_text(json.dumps(out,indent=2),encoding='utf-8')
print(json.dumps(out,indent=2))
