import json, math, statistics
from datetime import datetime
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
SIM=ROOT/'QA/Simulations'

def load(name):
    folder=SIM/name
    rows=[json.loads(line) for path in sorted(folder.glob('worker-*.jsonl')) for line in path.read_text(encoding='utf-8-sig').splitlines() if line.strip()]
    assert len({d['seed'] for d in rows})==len(rows), 'Duplicate seed'
    assert all(d['valid'] for d in rows), 'Invalid game'
    return {d['seed']:d for d in rows}

def clear3(d): return any(w['wave']==3 for w in d['waveReports'])
def win(d): return d['result']=='Complete'
def absorbs(d,item): return sum(i['values']['absorption'] for i in d['items'] if i['item']==item)
def has(d,item): return any(i['item']==item and i['equipped'] for i in d['items'])
def earlyguard(d): return bool(d['waveReports']) and 'Counterguard' in d['waveReports'][0]['choice']
def mean(xs): return round(statistics.mean(xs),3) if xs else None

def totals(data):
    rows=list(data.values())
    return {'games':len(rows),'clear3':sum(map(clear3,rows)),'wins':sum(map(win,rows)),
        'deathsByWave':{str(w):sum(d['result']=='Defeated' and d['waveReached']==w for d in rows) for w in range(1,9)},
        'timeouts':sum(d['result']=='TimedOut' for d in rows),
        'policies':{p:{'games':len(g),'clear3':sum(map(clear3,g)),'wins':sum(map(win,g))} for p in sorted({d['policy'] for d in rows}) for g in [[d for d in rows if d['policy']==p]]},
        'commonEquippedAbsorption':{item:{'copies':len(items),'mean':mean([i['values']['absorption'] for i in items])} for item in ['Counterguard','duelist','reactive','plate'] for items in [[i for d in rows for i in d['items'] if i['item']==item and i['equipped'] and i['rarity']==1]]}}

def delta(a,b,key):
    values=[int(key(b[s]))-int(key(a[s])) for s in a]
    m=statistics.mean(values);se=statistics.stdev(values)/math.sqrt(len(values)) if len(values)>1 else 0
    return {'improved':values.count(1),'regressed':values.count(-1),'deltaPercentagePoints':round(m*100,3),'approxPaired95CI':([round((m-1.96*se)*100,3),round((m+1.96*se)*100,3)] if len(values)>=100 and sum(v!=0 for v in values)>=10 else None)}

def compare(a,b):
    assert a.keys()==b.keys(), 'Seed mismatch'
    return {'wins':delta(a,b,win),'clear3':delta(a,b,clear3),
        'fixedBaselineCohorts':{name:{'games':len(seeds),'beforeClear3':sum(clear3(a[s]) for s in seeds),'afterClear3':sum(clear3(b[s]) for s in seeds),'beforeWins':sum(win(a[s]) for s in seeds),'afterWins':sum(win(b[s]) for s in seeds),'beforeMeanAbsorption':mean([absorbs(a[s],item) for s in seeds]),'afterMeanAbsorption':mean([absorbs(b[s],item) for s in seeds])} for name,item,predicate in [('earlyCounterguard','Counterguard',earlyguard),('equippedDuelist','duelist',lambda d:has(d,'duelist'))] for seeds in [[s for s,d in a.items() if predicate(d)]]}}

names=['v064-rear-boundary-1000','v066-counterguard-only-1000','v066-duelist-only-1000','v066-combined-1000']
base=load(names[0]);out={'baseline':names[0],'batches':{}}
for name in names:
    path=SIM/name
    if not (path/'summary.json').exists(): continue
    data=load(name);assert len(data)==1000
    summary=json.loads((path/'summary.json').read_text(encoding='utf-8-sig'))
    out['batches'][name]={'totals':totals(data),'seconds':(json.loads((ROOT/'QA/v064-early-wave-summary.json').read_text(encoding='utf-8-sig'))['final']['seconds'] if name=='v064-rear-boundary-1000' else (datetime.fromisoformat(json.loads((path/'batch.json').read_text(encoding='utf-8-sig'))['completedUtc'])-datetime.fromisoformat(json.loads((path/'batch.json').read_text(encoding='utf-8-sig'))['startedUtc'])).total_seconds()),'provenance':json.loads((path/'batch.json').read_text(encoding='utf-8-sig')),'comparison':compare(base,data)}
fullnames=['v066-full-legacy-control-12','v066-full-combined-12']
if all((SIM/n/'summary.json').exists() for n in fullnames):
    a,b=map(load,fullnames);out['fullSample']={'selection':'Seeds 12654-12665 chosen before Full runs because baseline Fast had six Duelist-equipped runs; targeted, not representative. Two games per policy.','baseline':totals(a),'candidate':totals(b),'comparison':compare(a,b),'baselineProvenance':json.loads((SIM/fullnames[0]/'batch.json').read_text(encoding='utf-8-sig')),'candidateProvenance':json.loads((SIM/fullnames[1]/'batch.json').read_text(encoding='utf-8-sig'))}
(ROOT/'QA/v066-balance-results.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
for name,b in out['batches'].items(): print(name,{k:b['totals'][k] for k in ['games','clear3','wins','timeouts']},'seconds',round(b['seconds'],2))
if 'fullSample' in out: print('Full sample:',out['fullSample']['baseline']['wins'],'->',out['fullSample']['candidate']['wins'])
