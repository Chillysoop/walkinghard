"""Paired analyses and compact evidence. Never treat equipment ownership as causal."""
import json,csv,math,statistics as st
from collections import defaultdict,Counter
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]; HERE=Path(__file__).resolve().parent; SIM=ROOT/'QA/Simulations'
def read(name):
 return [json.loads(s) for p in (SIM/name).glob('worker-*.jsonl') for s in p.read_text(encoding='utf-8-sig').splitlines()]
def avg(xs):return round(st.mean(xs),4) if xs else None
def summary(rows):return dict(games=len(rows),wins=sum(r['result']=='Complete' for r in rows),timeouts=sum(r['result']=='TimedOut' for r in rows),meanWave=avg([r['waveReached'] for r in rows]),meanHealth=avg([r['health'] for r in rows]),targetWaitPercent=avg([sum(i['values']['rangeWait'] for i in r['items'])/r['elapsed']*100 for r in rows]))
def paired(a,b):
 assert set(a)==set(b)
 improved=sum(a[k]['result']!='Complete' and b[k]['result']=='Complete' for k in a);worse=sum(a[k]['result']=='Complete' and b[k]['result']!='Complete' for k in a)
 discordant=improved+worse;p=min(1,2*sum(math.comb(discordant,i) for i in range(min(improved,worse)+1))/2**discordant) if discordant else 1
 return dict(pairs=len(a),gainedWins=improved,lostWins=worse,winRateDelta=(improved-worse)/len(a),mcnemarExactP=p)
def groups(rows,key):
 out=defaultdict(list)
 for r in rows:out[key(r)].append(r)
 return out
datasets={n:read(n) for n in ['v072-range','v072-routes','v072-full-range-fixed','v072-full-routes-fresh','v072-reward-standard','v072-reward-shield','v072-reward-volatile','v072-linked-shield','v072-linked-volatile']}
expected=[600,288,12,6,240,240,240,240,240]
for (name,rows),count in zip(datasets.items(),expected):
 assert len(rows)==count,(name,len(rows))
 assert all(r['valid'] for r in rows),name
 ids=[r['controlled']['id'] if r.get('controlled',{}).get('id') else r['seed'] for r in rows];assert len(ids)==len(set(ids)),name
out={'batches':{n:json.loads((SIM/n/'batch.json').read_text(encoding='utf-8-sig')) for n in datasets},'range':{},'routes':{},'rewards':{},'paired':{}}
for mode,name in [('Fast','v072-range'),('Full','v072-full-range-fixed')]:
 out['range'][mode]={k:summary(g) for k,g in sorted(groups(datasets[name],lambda r:r['controlled']['context']).items())}
for mode,name in [('Fast','v072-routes'),('Full','v072-full-routes-fresh')]:
 cases=json.loads((SIM/name/'cases.json').read_text())['cases'];by_id={c['id']:c for c in cases}
 out['routes'][mode]={}
 for (context,route),g in sorted(groups(datasets[name],lambda r:(r['controlled']['context'],by_id[r['controlled']['id']]['route'])).items()):
  stats=summary(g)
  for wave in [3,6]:
   reports=[next((w for w in r['waveReports'] if w['wave']==wave),None) for r in g];reports=[w for w in reports if w]
   stats['wave'+str(wave)]={'completed':len(reports),'meanHealth':avg([w['health'] for w in reports]),'meanHits':avg([w['hitsTaken'] for w in reports]),'meanKills':avg([w['kills'] for w in reports])}
  out['routes'][mode][context+'-'+str(route)]=stats
 # Same fixed loadouts and seeds: all pre-route combat fields must match.
 pairs=groups(datasets[name],lambda r:(r['controlled']['context'],r['seed']))
 for g in pairs.values():
  early=[[w for w in r['waveReports'] if w['wave']<3] for r in g];assert all(w==early[0] for w in early),g[0]['controlled']['id']
for name in ['standard','shield','volatile']:
 rows=datasets['v072-reward-'+name];out['rewards'][name]={'all':summary(rows),'policies':{k:summary(g) for k,g in sorted(groups(rows,lambda r:r['policy']).items())}}
 assert all(r['shieldBearers']==r['volatileCarriers']==0 for r in rows)
for prefix in ['reward','linked']:
 a={r['seed']:r for r in datasets['v072-'+prefix+'-volatile']};b={r['seed']:r for r in datasets['v072-'+prefix+'-shield']}
 out['paired'][prefix+'-shield-vs-volatile']=paired(a,b)
 for seed in a:
  for wave in [1,2]:
   wa=next((w for w in a[seed]['waveReports'] if w['wave']==wave),None);wb=next((w for w in b[seed]['waveReports'] if w['wave']==wave),None)
   clean=lambda w:{k:v for k,v in w.items() if k!='choice'} if w else None
   assert clean(wa)==clean(wb),(prefix,seed,wave)
out['linked']={name:summary(datasets['v072-linked-'+name]) for name in ['shield','volatile']}
out['timeouts']=json.loads((HERE/'timeout-results.json').read_text())
(HERE/'results.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
(ROOT/'QA/v072-balance-results.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
with (HERE/'games.csv').open('w',newline='',encoding='utf-8') as f:
 writer=csv.DictWriter(f,fieldnames=['batch','case','seed','policy','rank','result','wave','health','seconds','waitSeconds']);writer.writeheader()
 for name,rows in datasets.items():
  for r in rows:writer.writerow(dict(batch=name,case=r.get('controlled',{}).get('id',''),seed=r['seed'],policy=r['policy'],rank=r.get('controlled',{}).get('rank',0),result=r['result'],wave=r['waveReached'],health=r['health'],seconds=r['elapsed'],waitSeconds=sum(i['values']['rangeWait'] for i in r['items'])))
print(json.dumps({'range':out['range'],'linked':out['linked'],'paired':out['paired']},indent=2))
