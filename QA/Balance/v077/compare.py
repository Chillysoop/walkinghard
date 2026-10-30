"""Reuse the matching 0.7.6 cases and compare modes separately, without tuning overrides."""
import argparse,json,statistics,collections,csv
from pathlib import Path

HERE=Path(__file__).resolve().parent
OLD=HERE.parent/'v076'

def prepare():
    fast=[]
    for name in ('fast-cases.json','followup-cases.json'):
        fast.extend(c for c in json.loads((OLD/name).read_text(encoding='utf-8'))['cases'] if c['candidate']=='recoil')
    full=[c for c in json.loads((OLD/'full-cases.json').read_text(encoding='utf-8'))['cases'] if c['candidate']=='recoil']
    for name,cases in [('fast',fast),('full',full)]:
        (HERE/(name+'-cases.json')).write_text(json.dumps({'cases':cases},indent=2),encoding='utf-8')
    print(f'{len(fast)} matched Fast cases; {len(full)} matched Full cases')

def load(path):
    return [json.loads(line) for line in path.read_text(encoding='utf-8-sig').splitlines() if line.strip()]

def compare(folders):
    old={}
    for p in (OLD/'evidence').glob('*-samples.jsonl'):
        for r in load(p):old[(r['mode'],r['controlled']['id'])]=r
    groups=collections.defaultdict(list);evidence=HERE/'evidence';evidence.mkdir(exist_ok=True)
    pairs=[];batches=[]
    for folder in folders:
        path=Path(folder);batch=json.loads((path/'batch.json').read_text(encoding='utf-8'));batches.append(batch)
        rows=[r for p in path.glob('worker-*.jsonl') for r in load(p)]
        assert len(rows)==batch['runs'] and all(r['valid'] for r in rows)
        assert len({r['controlled']['id'] for r in rows})==len(rows)
        with (evidence/(batch['mode'].lower()+'-samples.jsonl')).open('w',encoding='utf-8') as f:
            for r in rows:
                c=r['controlled'];before=old[(r['mode'],c['id'])]
                assert before['valid'] and not r['errors'] and not before['errors']
                assert r['seed']==before['seed'] and c['rank']==before['controlled']['rank']
                assert c['maxHealth']==before['controlled']['maxHealth'] and c['ultimateEquipWave'] in (0,4)
                focal=next(i for i in r['items'] if i['item']=='recoil' and i['equipped'])
                prior=next(i for i in before['items'] if i['item']=='recoil' and i['equipped'])
                assert focal['rarity']==prior['rarity']==c['rank']
                pair=dict(mode=r['mode'],id=c['id'],context=c['context'],rank=c['rank'],seed=r['seed'],beforeWin=before['result']=='Complete',afterWin=r['result']=='Complete',beforeHealthPct=100*before['health']/before['controlled']['maxHealth'],afterHealthPct=100*r['health']/c['maxHealth'],beforeDamage=prior['values']['direct'],afterDamage=focal['values']['direct'],beforeBlock=prior['values']['absorption'],afterBlock=focal['values']['absorption'],beforeSeconds=before['elapsed'],afterSeconds=r['elapsed'],beforeWave=before['waveReached'],afterWave=r['waveReached'])
                pairs.append(pair);groups[(r['mode'],c['context'])].append(pair)
                keep={k:r[k] for k in ['controlled','seed','mode','result','health','elapsed','waveReached','valid','errors']};keep['items']=[focal];f.write(json.dumps(keep,separators=(',',':'))+'\n')
    summary=[]
    for (mode,context),ps in sorted(groups.items()):
        row=dict(mode=mode,context=context,n=len(ps))
        for key in ['beforeWin','afterWin','beforeHealthPct','afterHealthPct','beforeDamage','afterDamage','beforeBlock','afterBlock','beforeSeconds','afterSeconds','beforeWave','afterWave']:
            row[key]=sum(p[key] for p in ps) if key.endswith('Win') else round(statistics.mean(p[key] for p in ps),3)
        summary.append(row);print(row)
    for name,data in [('pairs',pairs),('summary',summary)]:
        with (HERE/(name+'.csv')).open('w',newline='',encoding='utf-8') as f:
            writer=csv.DictWriter(f,fieldnames=list(data[0]));writer.writeheader();writer.writerows(data)
    baselines=[json.loads(p.read_text(encoding='utf-8')) for p in sorted((OLD/'evidence').glob('*-batch.json'))]
    (HERE/'results.json').write_text(json.dumps(dict(batches=batches,baselineBatches=baselines,summary=summary),indent=2),encoding='utf-8')

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--prepare',action='store_true');p.add_argument('folders',nargs='*');a=p.parse_args()
    if a.prepare:prepare()
    if a.folders:compare(a.folders)
