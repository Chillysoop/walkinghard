"""One fresh Unity process per route case, preventing previous-case PhysX history."""
import json,subprocess,hashlib,time
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
ROOT=Path(__file__).resolve().parents[3];PLAYER=ROOT/'Build/Reliability-v072-verified/WalkingHard.exe'
OUT=ROOT/'QA/Simulations/v072-full-routes-fresh';OUT.mkdir(parents=True,exist_ok=False)
cases=json.loads((ROOT/'QA/Simulations/v072-inputs/full-routes.json').read_text())['cases']
(OUT/'cases.json').write_text(json.dumps({'cases':cases},indent=2),encoding='utf-8')
start=time.monotonic()
def run(pair):
 i,c=pair;casefile=OUT/f'case-{i}.json';casefile.write_text(json.dumps({'cases':[c]}),encoding='utf-8')
 args=[str(PLAYER),'-batchmode','-nographics','-simMode','Full','-simRuns','1','-simPolicy','balanced','-simWaves','8','-simMaxSeconds','600','-simCases',str(casefile),'-simOutput',str(OUT/f'worker-{i}.jsonl'),'-logFile',str(OUT/f'worker-{i}.log')]
 subprocess.run(args,creationflags=subprocess.CREATE_NO_WINDOW,check=True,timeout=180)
 r=json.loads((OUT/f'worker-{i}.jsonl').read_text(encoding='utf-8-sig').strip());assert r['valid'] and r['controlled']['id']==c['id'];print(c['id'],r['result'],flush=True);return r
with ThreadPoolExecutor(max_workers=3) as pool:rows=list(pool.map(run,enumerate(cases)))
meta=dict(mode='Full',runs=len(cases),workers=3,processPerGame=True,player=str(PLAYER),assemblySha256=hashlib.sha256((PLAYER.parent/'WalkingHard_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest(),maxCombatSeconds=600,baseHealth=280,valid=len(rows),invalid=0,missing=0,duplicateCases=0,wins=sum(r['result']=='Complete' for r in rows),timeouts=sum(r['result']=='TimedOut' for r in rows),wallSeconds=time.monotonic()-start)
(OUT/'batch.json').write_text(json.dumps(meta,indent=2),encoding='utf-8');print(json.dumps(meta),flush=True)
