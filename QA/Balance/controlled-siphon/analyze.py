import csv, json, math, random, statistics
from collections import defaultdict
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'QA/Balance/controlled-siphon'
def read(name):
    d=ROOT/'QA/Simulations'/name
    rows=[json.loads(l) for p in sorted(d.glob('worker-*.jsonl')) for l in p.read_text(encoding='utf-8-sig').splitlines() if l.strip()]
    meta=json.loads((d/'batch.json').read_text(encoding='utf-8-sig'))
    assert len(rows)==meta['runs'] and all(r['valid'] for r in rows)
    assert len({r['controlled']['id'] for r in rows})==len(rows)
    return rows,meta
def mean(xs):return statistics.mean(xs)
def win(r):return int(r['result']=='Complete')
def key(r):
    c=r['controlled'];return c['context'],c['category'],c['rank'],r['seed']
def aggregate(rows):
    return dict(n=len(rows),wins=sum(map(win,rows)),winPct=100*mean(map(win,rows)),meanWave=mean(r['waveReached'] for r in rows),meanHealthPct=mean(max(0,r['health'])/r['controlled']['maxHealth']*100 for r in rows),meanSeconds=mean(r['elapsed'] for r in rows),healingPerSecond=mean(r['controlled']['healing']/r['elapsed'] for r in rows),preventionPerSecond=mean(r['controlled']['prevented']/r['elapsed'] for r in rows),damagePerSecond=mean(sum(i['values']['direct']+i['values']['collision'] for i in r['items'])/r['elapsed'] for r in rows),meanHealing=mean(r['controlled']['healing'] for r in rows),meanPrevention=mean(r['controlled']['prevented'] for r in rows),capLossPct=100*sum(r['controlled']['capLost'] for r in rows)/max(1,sum(r['controlled']['potentialDrain'] for r in rows)),overhealPct=100*sum(r['controlled']['overheal'] for r in rows)/max(1,sum(r['controlled']['potentialDrain'] for r in rows)),cappedActionPct=100*sum(r['controlled']['cappedDrainActions'] for r in rows)/max(1,sum(r['controlled']['drainActions'] for r in rows)))
def paired(a,b):
    aa={key(r):r for r in a};bb={key(r):r for r in b};assert aa.keys()==bb.keys()
    deltas=[win(bb[k])-win(aa[k]) for k in aa]
    # Cluster the five rarity observations together for a context/seed; resample
    # seed blocks within each context, never treating repeated rarities as independent.
    clusters=defaultdict(lambda:defaultdict(list))
    for k in aa:clusters[k[0]][k[3]].append(win(bb[k])-win(aa[k]))
    rng=random.Random(8103);boots=[]
    for _ in range(4000):
        ds=[]
        for ctx,seeds in clusters.items():
            groups=list(seeds.values())
            for j in range(len(groups)):ds.extend(rng.choice(groups))
        boots.append(mean(ds)*100)
    boots.sort()
    return dict(n=len(aa),gain=sum(d==1 for d in deltas),loss=sum(d==-1 for d in deltas),winDeltaPP=100*mean(deltas),bootstrap95=[boots[100],boots[3899]],healthDeltaPP=mean(100*(max(0,bb[k]['health'])/bb[k]['controlled']['maxHealth']-max(0,aa[k]['health'])/aa[k]['controlled']['maxHealth']) for k in aa),waveDelta=mean(bb[k]['waveReached']-aa[k]['waveReached'] for k in aa))

rows,meta=read('controlled-siphon-fast-3600')
integrity=[]
for r in rows:
    c=r['controlled'];drain=sum(i['values']['healing'] for i in r['items'])
    assert abs(c['potentialDrain']-c['capLost']-c['overheal']-drain)<1,('drain conservation',c['id'])
    # Multiple final-frame attackers may see negative HP before the end-step death clamp.
    # Use raw minus prevented for conservation, not the per-hit clamped diagnostic.
    net=c['rawIncoming']-c['prevented']; spent=c['initialHealth']+c['healing']-max(0,r['health'])
    assert (abs(net-spent)<1 if r['result']!='Defeated' else net+1>=spent),('health conservation',c['id'])
groups=defaultdict(list)
for r in rows:groups[(r['controlled']['category'],r['controlled']['candidate'])].append(r)
records=[]
for (cat,item),rs in sorted(groups.items()):
    for ctx in ['ALL','sever','storm','gun','siphon']:
        for rarity in [0,1,2,3,4,5]:
            sub=[r for r in rs if (ctx=='ALL' or r['controlled']['context']==ctx) and (rarity==0 or r['controlled']['rank']==rarity)]
            records.append(dict(category=cat,item=item,context=ctx,rarity=rarity,**aggregate(sub)))
with (OUT/'summary.csv').open('w',newline='',encoding='utf-8') as f:
    w=csv.DictWriter(f,records[0].keys());w.writeheader();w.writerows(records)
comparisons=[]
for cat,focus in [('weapon','siphonblade'),('armor','reclaimer'),('ultimate','harvest')]:
    for (otherCat,item),rs in groups.items():
        if otherCat!=cat or item==focus:continue
        comparisons.append(dict(category=cat,baseline=item,candidate=focus,**paired(rs,groups[(cat,focus)])))
# All ultimate alternatives must have exactly identical pre-unlock trajectories.
pre=defaultdict(list)
for r in rows:
    if r['controlled']['category']=='ultimate':pre[key(r)].append(r)
for k,rs in pre.items():
    first=[(w['wave'],w['kills'],w['health'],w['hitsTaken']) for w in rs[0]['waveReports'] if w['wave']<=3]
    assert all([(w['wave'],w['kills'],w['health'],w['hitsTaken']) for w in r['waveReports'] if w['wave']<=3]==first for r in rs),('pre-ultimate mismatch',k)
out=dict(provenance=meta,integrity=dict(valid=len(rows),missing=0,duplicates=0,drainConservation=True,healthConservation=True,identicalUltimatePreUnlock=True),overall=[r for r in records if r['context']=='ALL' and r['rarity']==0],paired=sorted(comparisons,key=lambda c:(c['category'],c['baseline'])))
sensitivity,smeta=read('controlled-siphon-rare-support-480')
sg=defaultdict(list)
for r in sensitivity:sg[(r['controlled']['category'],r['controlled']['candidate'])].append(r)
out['rareSupport']=dict(provenance=smeta,groups=[dict(category=k[0],item=k[1],**aggregate(v)) for k,v in sorted(sg.items())],paired=[dict(category=cat,baseline=alt,candidate=focus,**paired(sg[(cat,alt)],sg[(cat,focus)])) for cat,focus,alt in [('weapon','siphonblade','sword'),('armor','reclaimer','plate'),('ultimate','harvest','tempest')]])
out['rareSupport']['byContext']=[dict(context=ctx,category=k[0],item=k[1],**aggregate([r for r in v if r['controlled']['context']==ctx])) for k,v in sorted(sg.items()) for ctx in ['sever','storm','gun','siphon']]
fullPath=ROOT/'QA/Simulations/controlled-siphon-full-24/batch.json'
if fullPath.exists() and 'valid' in json.loads(fullPath.read_text(encoding='utf-8-sig')):
    full,fmeta=read('controlled-siphon-full-24');fg=defaultdict(list)
    for r in full:fg[(r['controlled']['context'],r['controlled']['category'],r['controlled']['candidate'])].append(r)
    out['full']=dict(provenance=fmeta,groups=[dict(context=k[0],category=k[1],item=k[2],**aggregate(v)) for k,v in fg.items()])
    fastById={r['controlled']['id']:r for r in rows}
    out['full']['outcomeAgreement']=sum(win(r)==win(fastById[r['controlled']['id']]) for r in full)
(OUT/'results.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
for r in out['overall']:print(r['category'],r['item'],f"{r['wins']}/{r['n']} {r['winPct']:.1f}%",f"heal/s {r['healingPerSecond']:.2f}; prevent/s {r['preventionPerSecond']:.2f}; cap {r['capLossPct']:.1f}%")
for c in out['paired']:print('paired',c['baseline'],'->',c['candidate'],round(c['winDeltaPP'],1),[round(x,1) for x in c['bootstrap95']])
if 'full' in out:
    for g in out['full']['groups']:print('Full',g['context'],g['category'],g['item'],g['wins'],'/',g['n'],round(g['meanHealthPct'],1))
