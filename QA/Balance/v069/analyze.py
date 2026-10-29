import csv,json,statistics
from collections import defaultdict
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).parent;SIM=ROOT/'QA/Simulations'
def read(name):
 p=SIM/name;meta=json.loads((p/'batch.json').read_text(encoding='utf-8-sig'))
 assert 'valid' in meta or 'completedUtc' in meta,name
 rows=[json.loads(l) for f in p.glob('worker-*.jsonl') for l in f.read_text(encoding='utf-8-sig').splitlines() if l.strip()]
 assert len(rows)==meta['runs'] and all(r['valid'] for r in rows),name
 return rows,meta
old,_=read('v068-final-1000');old={r['seed']:r for r in old};new,meta=read('v069-final-regression-64' if (SIM/'v069-final-regression-64/batch.json').exists() else 'v069-regression-64')
differences=[];excluded={'version','wallSeconds','items'}
for r in new:
 p=old[r['seed']]
 for k in p:
  if k not in excluded and p[k]!=r[k]:differences.append([r['seed'],k])
 assert len(r['items'])==len(p['items'])
 for a,b in zip(p['items'],r['items']):
  for k in a:
   if k!='values':assert a[k]==b[k],(r['seed'],k)
  for k in a['values']:
   if k in ('reload','preparation','rangeWait'):continue
   if a['item']=='treads' and k in ('healing','absorption'):
    assert abs(a['values']['healing']+a['values']['absorption']-b['values']['healing']-b['values']['absorption'])<.001
   else:assert a['values'][k]==b['values'][k],(r['seed'],k)
assert not differences,differences
regression=dict(games=len(new),differences=differences,excluded=['version','wallSeconds','item reload/preparation/rangeWait (fusion accounting repair)','Recovery Treads heal/block split (sum checked)'],provenance=meta)
(OUT/'regression.json').write_text(json.dumps(regression,indent=2)+'\n',encoding='utf-8')
groups=defaultdict(list);cases=[];provenance={};by_batch={}
for batch in ['weapons','ultimates','armor','rare-support','full']:
 p=SIM/('v069-'+batch)/'batch.json'
 if not p.exists() or 'valid' not in json.loads(p.read_text(encoding='utf-8-sig')):continue
 rows,metadata=read('v069-'+batch);provenance[batch]=metadata;by_batch[batch]=rows
 assert len({r['controlled']['id'] for r in rows})==len(rows)
 for r in rows:
  c=r['controlled'];values=[i['values'] for i in r['items']]
  category=c['category'];context=c['context'];support='Rare' if batch=='rare-support' else 'Common';mode=metadata['mode']
  if category=='ultimate':assert c['ultimateEquipWave'] in (0,4)
  weapons=[i for i in r['items'] if i['equipped'] and i['item'] in ['pistol','rifle','scattergun','sword','greatsword','siphonblade','breachhammer']]
  assert all(i['rarity']==c['rank'] for i in weapons)
  rec=dict(mode=mode,support=support,category=category,context=context,candidate=c['candidate'],rank=c['rank'],seed=r['seed'],id=c['id'],result=r['result'],wave=r['waveReached'],elapsed=r['elapsed'],health=r['health'],damage=sum(v['direct']+v['collision'] for v in values),healing=sum(v['healing'] for v in values),blocked=c['prevented'],reload=sum(v['reload'] for v in values),ready=sum(v['preparation'] for v in values),targetWait=sum(v['rangeWait'] for v in values),ultCasts=c['ultimateCasts'])
  cases.append(rec)
  for rank in [0,c['rank']]:groups[(mode,support,category,context,c['candidate'],rank)].append(rec)
summary=[]
for key,g in sorted(groups.items()):
 d=dict(zip(['mode','support','category','context','candidate','rank'],key));d.update(games=len(g),wins=sum(r['result']=='Complete' for r in g),meanWave=statistics.mean(r['wave'] for r in g),meanSeconds=statistics.mean(r['elapsed'] for r in g),meanHealth=statistics.mean(r['health'] for r in g),damagePerSecond=statistics.mean(r['damage']/r['elapsed'] for r in g),healingPerSecond=statistics.mean(r['healing']/r['elapsed'] for r in g),blockedPerSecond=statistics.mean(r['blocked']/r['elapsed'] for r in g),reloadWeaponSecondsPerMinute=statistics.mean(60*r['reload']/r['elapsed'] for r in g),readySecondsPerMinute=statistics.mean(60*r['ready']/r['elapsed'] for r in g),targetWaitPct=statistics.mean(100*r['targetWait']/r['elapsed'] for r in g),meanUltimateCasts=statistics.mean(r['ultCasts'] for r in g));summary.append(d)
for name,rows in [('case-results',cases),('summary',summary)]:
 with (OUT/(name+'.csv')).open('w',newline='',encoding='utf-8') as f:w=csv.DictWriter(f,rows[0].keys());w.writeheader();w.writerows(rows)
# Ultimate candidates must see identical encounters before their checkpoint-three unlock.
gates=defaultdict(list)
for r in by_batch.get('ultimates',[]):
 c=r['controlled'];gates[(c['context'],c['rank'],r['seed'])].append(r['waveReports'][:3])
for key,group in gates.items():assert all(x==group[0] for x in group),key
out=dict(provenance=provenance,regression=regression,summary=summary)
(ROOT/'QA/v069-balance-results.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
for d in summary:
 if d['rank']==0:print(d['mode'],d['support'],d['category'],d['context'],d['candidate'],f"{d['wins']}/{d['games']}",f"wave {d['meanWave']:.2f}",f"DPS {d['damagePerSecond']:.1f}",f"target {d['targetWaitPct']:.1f}%",f"reload {d['reloadWeaponSecondsPerMinute']:.1f}s/min")
print('64-seed gameplay regression passed;',len(cases),'controlled cases summarized')
