import json,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3];SIM=ROOT/'QA/Simulations';OUT=Path(__file__).parent
def rows(name):return [json.loads(l) for p in (SIM/name).glob('worker-*.jsonl') for l in p.read_text(encoding='utf-8-sig').splitlines() if l.strip()]
base={r['controlled']['id']:r for name in ['controlled-siphon-fast-3600','controlled-siphon-rare-support-480'] for r in rows(name)}
controls=rows('v068-control');diff=[]
assert len(controls)==420
for r in controls:
    old=base[r['controlled']['id']]
    for k,v in old.items():
        if k not in ('version','wallSeconds','controlled') and v!=r[k]:diff.append([r['controlled']['id'],k])
assert not diff,diff[:10]
(OUT/'control-regression.json').write_text(json.dumps(dict(games=420,compared='All result fields including items and waves; version, wallSeconds and experimental metrics excluded',differences=diff),indent=2),encoding='utf-8')
print('420 matched legacy controls reproduce every gameplay field',flush=True)
for name in ['blade','armor','combined','redline']:
    subprocess.run([sys.executable,str(ROOT/'QA/Balance/controlled-siphon/run.py'),'--player',str(ROOT/'Build/SiphonBalance-v068/WalkingHard.exe'),'--out',str(SIM/('v068-'+name)),'--workers','4','--cases',str(OUT/(name+'-cases.json')),'--balance',str(OUT/(name+'-rules.json'))],check=True)
