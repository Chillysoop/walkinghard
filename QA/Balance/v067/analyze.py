import json,statistics
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
SIM=ROOT/'QA/Simulations'
def load(name):
 p=SIM/name
 rows=[json.loads(l) for f in sorted(p.glob('worker-*.jsonl')) for l in f.read_text(encoding='utf-8-sig').splitlines() if l.strip()]
 meta=json.loads((p/'batch.json').read_text(encoding='utf-8-sig'))
 assert len(rows)==meta['runs'] and len({r['seed'] for r in rows})==len(rows) and all(r['valid'] for r in rows),name
 return {r['seed']:r for r in rows},meta
def clear(r):return any(w['wave']==3 for w in r['waveReports'])
def won(r):return r['result']=='Complete'
def total(rows):
 rs=list(rows.values())
 return dict(games=len(rs),clear3=sum(map(clear,rs)),wins=sum(map(won,rs)),timeouts=sum(r['result']=='TimedOut' for r in rs),deathsByWave={str(w):sum(r['result']=='Defeated' and r['waveReached']==w for r in rs) for w in range(1,9)},policies={p:dict(games=len(g),clear3=sum(map(clear,g)),wins=sum(map(won,g))) for p in sorted({r['policy'] for r in rs}) for g in [[r for r in rs if r['policy']==p]]})
def compare(a,b):
 assert a.keys()==b.keys()
 return {name:dict(improved=sum(not fn(a[k]) and fn(b[k]) for k in a),regressed=sum(fn(a[k]) and not fn(b[k]) for k in a)) for name,fn in [('wins',won),('clear3',clear)]}
names=['v066-combined-1000','v067-augments-1000','v067-counter-1000','v067-combined-1000','v067-final-1000']
base,meta=load(names[0]);out={'baseline':names[0],'batches':{}}
loaded={}
for name in names:
 rows,meta=load(name);loaded[name]=rows
 summary=json.loads((SIM/name/'summary.json').read_text(encoding='utf-8-sig'))
 out['batches'][name]={'totals':total(rows),'comparison':compare(base,rows),'seconds':summary['batchSeconds'],'provenance':meta}
 out['batches'][name]['fixedEarlyCounterguard']={'games':len(g:=[k for k,r in base.items() if r['waveReports'] and 'Counterguard' in r['waveReports'][0]['choice']]),'wins':sum(won(rows[k]) for k in g)}
final=loaded[names[-1]]
out['siphonAddedToCombined']=compare(loaded[names[-2]],final)
out['siphonExposure']={item:{'games':len(g:=[r for r in final.values() if any(i['item']==item and i['equipped'] for i in r['items'])]),'wins':sum(map(won,g))} for item in ['siphonblade','DrainBrand','reclaimer','predator','harvest']}
reg=[]
for a,b in [('v066-combined-1000','v067-final-control-32'),('v067-combined-1000','v067-final-combined-control-32'),('v067-final-1000','v067-release-control-32')]:
 if not (SIM/b/'summary.json').exists():continue
 left,_=load(a);right,metadata=load(b)
 keys=[k for k,v in next(iter(left.values())).items() if not isinstance(v,(list,dict)) and k not in ['version','wallSeconds']]
 diffs=[(seed,k,left[seed][k],r[k]) for seed,r in right.items() for k in keys if left[seed][k]!=r[k]]
 assert not diffs,(a,b,diffs[:5])
 reg.append({'baseline':a,'candidate':b,'games':len(right),'fields':keys,'differences':diffs,'candidateProvenance':metadata})
out['regressions']=reg
if (SIM/'v067-final-full-6'/'summary.json').exists():
 a,ma=load('v067-full-control-12');b,mb=load('v067-final-full-6');a={k:a[k] for k in b}
 out['fullSample']={'note':'Six matched seeds 12654–12659, one per policy, normal health; two workers. Final gameplay candidate versus legacy control. Tiny coverage sample, not a population estimate; Full is not bitwise deterministic. Final release adds only healing-total fusion bookkeeping.','baseline':total(a),'candidate':total(b),'comparison':compare(a,b),'baselineProvenance':ma,'candidateProvenance':mb}
(ROOT/'QA/v067-balance-results.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
(ROOT/'QA/v067-control-regression.json').write_text(json.dumps(reg,indent=2)+'\n',encoding='utf-8')
for name,b in out['batches'].items():print(name,{k:b['totals'][k] for k in ['games','clear3','wins','timeouts']},round(b['seconds'],2))
if 'fullSample' in out:print('Full',out['fullSample']['baseline']['wins'],'->',out['fullSample']['candidate']['wins'])
