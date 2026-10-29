import csv,json,statistics
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).parent;SIM=ROOT/'QA/Simulations'
def read(name,controlled=True):
    p=SIM/name;meta=json.loads((p/'batch.json').read_text(encoding='utf-8-sig'))
    if 'completedUtc' not in meta and 'valid' not in meta:raise ValueError('Incomplete '+name)
    rs=[json.loads(l) for f in p.glob('worker-*.jsonl') for l in f.read_text(encoding='utf-8-sig').splitlines() if l.strip()]
    assert len(rs)==meta['runs'] and all(r['valid'] for r in rs),name
    ids=[r['controlled']['id'] if controlled else r['seed'] for r in rs];assert len(set(ids))==len(ids),name
    return dict(zip(ids,rs)),meta
def won(r):return r['result']=='Complete'
def totals(rs):
    rs=list(rs);return dict(games=len(rs),wins=sum(map(won,rs)),winPct=100*statistics.mean(map(won,rs)),timeouts=sum(r['result']=='TimedOut' for r in rs),meanWave=statistics.mean(r['waveReached'] for r in rs),meanHealthPct=statistics.mean(100*max(0,r['health'])/r['controlled']['maxHealth'] for r in rs),healingPerSecond=statistics.mean(r['controlled']['healing']/r['elapsed'] for r in rs))
def paired(old,new):return dict(gain=sum(not won(old[k]) and won(v) for k,v in new.items()),loss=sum(won(old[k]) and not won(v) for k,v in new.items()))
base={};provenance={}
for name in ['controlled-siphon-fast-3600','controlled-siphon-rare-support-480']:
    rs,meta=read(name);base.update(rs);provenance[name]=meta
stages={'baseline':base};out={'baselineProvenance':provenance,'stages':{}};summary=[];case_summary=[]
for stage in ['blade','armor','combined','redline']:
    path=SIM/('v068-'+stage)/'batch.json'
    if not path.exists() or 'valid' not in json.loads(path.read_text(encoding='utf-8-sig')):continue
    rerun,meta=read('v068-'+stage)
    prior=stages['combined'] if stage=='redline' else base
    combined=dict(prior);combined.update(rerun);label='final' if stage=='redline' else stage;stages[label]=combined
    # Validate measured healing conservation using actual item attribution.
    for r in rerun.values():
        c=r['controlled'];actual=sum(i['values']['healing'] for i in r['items'])
        assert abs(c['potentialDrain']-c['capLost']-c['overheal']-actual)<1,c['id']
    out['stages'][label]=dict(executedGames=len(rerun),reusedGames=len(base)-len(rerun),provenance=meta,change=paired(prior,rerun),fullMatrix=totals(combined.values()))
    for key,r in sorted(rerun.items()):
        c=r['controlled'];p=prior[key]
        case_summary.append(dict(stage=label,id=key,seed=r['seed'],oldResult=p['result'],newResult=r['result'],oldHealth=p['health'],newHealth=r['health'],oldHealing=p['controlled']['healing'],newHealing=c['healing'],elapsed=r['elapsed'],potentialDrain=c['potentialDrain'],capLost=c['capLost'],overheal=c['overheal']))
for stage,rs in stages.items():
    for support in ['Common','Rare']:
        for category in ['weapon','armor','ultimate']:
            items=sorted({r['controlled']['candidate'] for r in rs.values() if r['controlled']['category']==category})
            for item in items:
                group=[r for r in rs.values() if r['controlled']['category']==category and r['controlled']['candidate']==item and (r['controlled']['id'].endswith('-support-rare'))==(support=='Rare')]
                if not group:continue
                for rank in [0,1,2,3,4,5]:
                    sub=[r for r in group if rank==0 or r['controlled']['rank']==rank]
                    summary.append(dict(stage=stage,support=support,category=category,item=item,rank=rank,**totals(sub)))
with (OUT/'summary.csv').open('w',newline='',encoding='utf-8') as f:w=csv.DictWriter(f,summary[0].keys());w.writeheader();w.writerows(summary)
if case_summary:
    with (OUT/'case-results.csv').open('w',newline='',encoding='utf-8') as f:w=csv.DictWriter(f,case_summary[0].keys());w.writeheader();w.writerows(case_summary)
out['summary']=[r for r in summary if r['rank']==0]
if (SIM/'v068-final-1000/summary.json').exists():
    old,oldMeta=read('v067-final-1000',False);new,newMeta=read('v068-final-1000',False)
    out['normalDraft']=dict(baselineProvenance=oldMeta,candidateProvenance=newMeta,baselineWins=sum(map(won,old.values())),candidateWins=sum(map(won,new.values())),baselineClear3=sum(any(w['wave']==3 for w in r['waveReports']) for r in old.values()),candidateClear3=sum(any(w['wave']==3 for w in r['waveReports']) for r in new.values()),comparison=paired(old,new),policies={p:dict(games=len(g:=[r for r in new.values() if r['policy']==p]),oldWins=sum(won(r) for r in old.values() if r['policy']==p),newWins=sum(map(won,g))) for p in sorted({r['policy'] for r in new.values()})})
fullPath=SIM/'v068-full-6/batch.json'
if fullPath.exists() and 'valid' in json.loads(fullPath.read_text(encoding='utf-8-sig')):
    old,oldMeta=read('controlled-siphon-full-24');new,newMeta=read('v068-full-6')
    out['full']=dict(provenance=newMeta,cases=[dict(id=k,old=old[k]['result'],new=r['result'],oldHealthPct=100*old[k]['health']/old[k]['controlled']['maxHealth'],newHealthPct=100*r['health']/r['controlled']['maxHealth']) for k,r in new.items()])
(ROOT/'QA/v068-balance-results.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf-8')
for stage,x in out['stages'].items():print(stage,'executed',x['executedGames'],'gain/loss',x['change'],'matrix wins',x['fullMatrix']['wins'])
for r in out['summary']:
    if r['stage'] in ('baseline','blade','armor','combined','final') and r['support']=='Common' and r['item'] in ('siphonblade','reclaimer','redline','harvest'):print(r['stage'],r['item'],r['wins'],'/',r['games'],round(r['healingPerSecond'],2))
if 'normalDraft' in out:print('Normal draft',out['normalDraft'])
if 'full' in out:print('Full',out['full']['cases'])
