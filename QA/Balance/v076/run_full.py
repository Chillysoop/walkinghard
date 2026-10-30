"""Fresh Unity/PhysX process per game. Same inputs as the corresponding Fast pairs."""
import json,subprocess,hashlib,time,argparse
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
p=argparse.ArgumentParser();p.add_argument('--player',required=True);p.add_argument('--out',required=True);p.add_argument('--cases',required=True);p.add_argument('--workers',type=int,default=3);a=p.parse_args()
player=Path(a.player).resolve();out=Path(a.out).resolve();out.mkdir(parents=True,exist_ok=False)
cases=json.loads(Path(a.cases).read_text())['cases'];(out/'cases.json').write_text(json.dumps({'cases':cases},indent=2));start=time.monotonic()
def run(pair):
    i,c=pair;casefile=out/f'case-{i}.json';casefile.write_text(json.dumps({'cases':[c]}))
    args=[str(player),'-batchmode','-nographics','-simMode','Full','-simRuns','1','-simPolicy','balanced','-simWaves','8','-simMaxSeconds','600','-simCases',str(casefile),'-simOutput',str(out/f'worker-{i}.jsonl'),'-logFile',str(out/f'worker-{i}.log')]
    subprocess.run(args,creationflags=subprocess.CREATE_NO_WINDOW,check=True,timeout=240)
    r=json.loads((out/f'worker-{i}.jsonl').read_text(encoding='utf-8-sig').strip())
    assert r['valid'] and r['controlled']['id']==c['id'] and r['seed']==c['seed']
    assert any(x['item']==c['candidate'] and x['rarity']==c['rank'] and x['equipped'] for x in r['items'])
    assert r['controlled']['ultimateEquipWave'] in (0,4)
    print(c['id'],r['result'],flush=True);return r
with ThreadPoolExecutor(max_workers=a.workers) as pool: rows=list(pool.map(run,enumerate(cases)))
assert len({r['controlled']['id'] for r in rows})==len(cases)
meta=dict(mode='Full',runs=len(cases),workers=a.workers,processPerGame=True,player=str(player),assemblySha256=hashlib.sha256((player.parent/'WalkingHard_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest(),maxCombatSeconds=600,baseHealth=280,valid=len(rows),invalid=0,missing=0,duplicateCases=0,wins=sum(r['result']=='Complete' for r in rows),timeouts=sum(r['result']=='TimedOut' for r in rows),wallSeconds=time.monotonic()-start)
(out/'batch.json').write_text(json.dumps(meta,indent=2));print(json.dumps(meta),flush=True)
