"""Descriptive matched comparisons; Fast and Full are never pooled."""
import json,statistics,collections,csv,argparse
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('folders',nargs='+');p.add_argument('--output',required=True);a=p.parse_args()
out=Path(a.output);out.mkdir(parents=True,exist_ok=True)
all_rows=[];summary=[];pairs=[]
for folder in a.folders:
    path=Path(folder);rows=[]
    for f in path.glob('worker-*.jsonl'):
        for line in f.read_text(encoding='utf-8-sig').splitlines():
            try:rows.append(json.loads(line))
            except json.JSONDecodeError:pass
    assert rows and all(r['valid'] for r in rows)
    assert len({r['controlled']['id'] for r in rows})==len(rows)
    assert len({r['mode'] for r in rows})==1
    groups=collections.defaultdict(list)
    def val(r,key):
        c=r['controlled'];i=next(x for x in r['items'] if x['item']==c['candidate'] and x['equipped'])
        return i['values'].get(key,0)
    for r in rows:groups[(r['controlled']['context'],r['controlled']['category'],r['controlled']['candidate'],r['controlled']['rank'])].append(r)
    for (context,category,item,rank),rs in groups.items():
        avg=lambda fn:round(statistics.mean(fn(r) for r in rs),3)
        summary.append(dict(batch=path.name,mode=rs[0]['mode'],context=context,category=category,item=item,rank=rank,n=len(rs),wins=sum(r['result']=='Complete' for r in rs),timeouts=sum(r['result']=='TimedOut' for r in rs),wave=avg(lambda r:r['waveReached']),health=avg(lambda r:r['health']),healthPct=avg(lambda r:100*r['health']/r['controlled']['maxHealth']),elapsed=avg(lambda r:r['elapsed']),damageTaken=avg(lambda r:r['controlled']['damageTaken']),direct=avg(lambda r:val(r,'direct')),control=avg(lambda r:val(r,'control')),absorption=avg(lambda r:val(r,'absorption')),healing=avg(lambda r:val(r,'healing')),rangeWait=avg(lambda r:val(r,'rangeWait')),damagePerSecond=avg(lambda r:val(r,'direct')/r['elapsed'])))
        if item not in ('SuppressiveVolley','recoil'):continue
        for (cx,cat,other,rk),others in groups.items():
            if cx!=context or cat!=category or rk!=rank or other==item:continue
            byseed={r['seed']:r for r in others};matched=[(r,byseed[r['seed']]) for r in rs if r['seed'] in byseed]
            pairs.append(dict(batch=path.name,mode=rs[0]['mode'],context=context,category=category,item=item,alternative=other,rank=rank,n=len(matched),onlyNewWins=sum(x['result']=='Complete' and y['result']!='Complete' for x,y in matched),onlyAlternativeWins=sum(y['result']=='Complete' and x['result']!='Complete' for x,y in matched),healthPctDelta=round(statistics.mean(100*x['health']/x['controlled']['maxHealth']-100*y['health']/y['controlled']['maxHealth'] for x,y in matched),3),elapsedDelta=round(statistics.mean(x['elapsed']-y['elapsed'] for x,y in matched),3)))
    all_rows+=rows
for name,data in [('summary',summary),('pairs',pairs)]:
    with (out/(name+'.csv')).open('w',newline='') as f:
        writer=csv.DictWriter(f,fieldnames=list(data[0]));writer.writeheader();writer.writerows(data)
(out/'results.json').write_text(json.dumps(dict(summary=summary,pairs=pairs),indent=2))
print('Analyzed',len(all_rows),'games; modes separated.')
for row in summary:
    if row['rank']==3:print(row)
