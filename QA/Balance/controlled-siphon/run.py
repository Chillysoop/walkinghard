"""Fixed-slot experiments against authored v0.6.7. No balance overrides."""
import argparse, copy, hashlib, json, subprocess, time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
CONTEXTS = {
    'sever': dict(weapons=['sword','sword'], abilities=['GravityWell','ReapingCut','SteelJab'], helmet='salvager', armor='duelist', boots='momentum', rig='magnet', ultimate='cyclone'),
    'storm': dict(weapons=['wraps','breachhammer'], abilities=['ConductiveBrand','ArcBolt','MassDriver'], helmet='capacitor', armor='grounding', boots='arcstep', rig='relay', ultimate='tempest'),
    'gun': dict(weapons=['pistol','pistol'], abilities=['VectorTether','ArcBolt','Breaker'], helmet='surveyor', armor='duelist', boots='loader', rig='overclock', ultimate='redline'),
    'siphon': dict(weapons=['sword','siphonblade'], abilities=['GravityWell','DrainBrand','ReapingCut'], helmet='', armor='reclaimer', boots='predator', rig='overclock', ultimate='harvest'),
}
ITEMS = {
    'weapon': ['siphonblade','sword','fists','breachhammer','pistol'],
    'armor': ['reclaimer','plate','reactive','grounding','duelist'],
    'ultimate': ['harvest','singularity','tempest','cyclone','redline'],
}
def cases(seeds):
    out=[]
    for ci,(context,base) in enumerate(CONTEXTS.items()):
        for category,items in ITEMS.items():
            for rank in range(1,6):
                for si in range(seeds):
                    for candidate in items:
                        c=copy.deepcopy(base)
                        if category=='weapon': c['weapons'][0]=candidate
                        else: c[category]=candidate
                        c.update(context=context,category=category,candidate=candidate,rank=rank,baseRank=1,healthFraction=1,seed=31000+ci*1000+si)
                        c['id']=f'{context}-{category}-{candidate}-r{rank}-s{si}'
                        out.append(c)
    return out

def load_rows(out):
    rows=[]
    for path in sorted(out.glob('worker-*.jsonl')):
        for line in path.read_text(encoding='utf-8-sig').splitlines():
            try: rows.append(json.loads(line))
            except json.JSONDecodeError: pass
    return rows

def main():
    p=argparse.ArgumentParser();p.add_argument('--out',required=True);p.add_argument('--mode',default='Fast',choices=['Fast','Full']);p.add_argument('--seeds',type=int,default=12);p.add_argument('--workers',type=int,default=4);p.add_argument('--smoke',action='store_true');p.add_argument('--cases');p.add_argument('--player',default=str(ROOT/'Build/ControlledSiphon-v067-report/WalkingHard.exe'));a=p.parse_args()
    out=Path(a.out).resolve();out.mkdir(parents=True,exist_ok=False)
    jobs=json.loads(Path(a.cases).read_text(encoding='utf-8'))['cases'] if a.cases else cases(a.seeds)
    if a.smoke: jobs=[c for c in jobs if c['context']=='sever' and c['rank'] in (1,5) and c['seed']==31000]
    (out/'cases.json').write_text(json.dumps({'cases':jobs},indent=2),encoding='utf-8')
    dll=Path(a.player).parent/'WalkingHard_Data/Managed/Assembly-CSharp.dll'
    meta=dict(mode=a.mode,runs=len(jobs),workers=a.workers,player=a.player,assemblySha256=hashlib.sha256(dll.read_bytes()).hexdigest(),maxCombatSeconds=600,baseHealth=280,startedUtc=time.strftime('%Y-%m-%dT%H:%M:%SZ',time.gmtime()))
    (out/'batch.json').write_text(json.dumps(meta,indent=2),encoding='utf-8')
    workers=[];start=time.monotonic();last=start
    try:
        for wi in range(a.workers):
            subset=jobs[wi::a.workers]
            if not subset: continue
            cp=out/f'cases-{wi}.json';cp.write_text(json.dumps({'cases':subset}),encoding='utf-8')
            args=[a.player,'-batchmode','-nographics','-simMode',a.mode,'-simRuns',str(len(subset)),'-simPolicy','balanced','-simWaves','8','-simMaxSeconds','600','-simCases',str(cp),'-simOutput',str(out/f'worker-{wi}.jsonl'),'-logFile',str(out/f'worker-{wi}.log')]
            workers.append(subprocess.Popen(args,creationflags=subprocess.CREATE_NO_WINDOW))
        print(f'{len(jobs)} controlled {a.mode} cases, {len(workers)} workers',flush=True)
        while any(w.poll() is None for w in workers):
            time.sleep(1)
            if time.monotonic()-last>=25:
                print(f'{len(load_rows(out))}/{len(jobs)} complete; {time.monotonic()-start:.1f}s',flush=True);last=time.monotonic()
            if time.monotonic()-start>1800: raise TimeoutError('Worker wall-time budget exceeded')
        rows=load_rows(out); ids=[r.get('controlled',{}).get('id') for r in rows]
        errors=[r for r in rows if not r['valid']]
        if any(w.returncode!=0 for w in workers) or len(rows)!=len(jobs) or set(ids)!={c['id'] for c in jobs} or len(set(ids))!=len(ids) or errors:
            raise RuntimeError(f'Batch integrity failure: rows={len(rows)}, codes={[w.returncode for w in workers]}, invalid={errors[:2]}')
        by_id={c['id']:c for c in jobs}
        for r in rows:
            c=by_id[r['controlled']['id']]
            assert r['seed']==c['seed'] and r['controlled']['rank']==c['rank']
            focal=[i for i in r['items'] if i['item']==c['candidate'] and i['rarity']==c['rank'] and i['equipped']]
            if c['category']!='ultimate' or r['waveReached']>=4: assert focal, c['id']
            assert r['controlled']['ultimateEquipWave'] in (0,4)
        meta.update(wallSeconds=time.monotonic()-start,valid=len(rows),invalid=0,missing=0,duplicateCases=0,wins=sum(r['result']=='Complete' for r in rows),timeouts=sum(r['result']=='TimedOut' for r in rows))
        (out/'batch.json').write_text(json.dumps(meta,indent=2),encoding='utf-8');print(json.dumps(meta),flush=True)
    finally:
        for w in workers:
            if w.poll() is None: w.kill();w.wait()
if __name__=='__main__': main()
