"""Repeat the core contract matrix against the final player with isolated profiles."""
import argparse, concurrent.futures, hashlib, json, subprocess, time
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--player',required=True);p.add_argument('--out',required=True);a=p.parse_args()
root=Path(__file__).resolve().parents[2];player=Path(a.player).resolve();out=Path(a.out).resolve();out.mkdir(parents=True,exist_ok=False)
# Reuse exactly the cases already run by Validate Prototype.ps1.
cases=[json.loads(f.read_text(encoding='utf-8-sig')) for f in sorted((root/'QA').glob('v092-core-*.json'))];assert len(cases)==15
start=time.monotonic();manifest=dict(assemblySha256=hashlib.sha256((player.parent/'WalkingHard_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest(),workers=3,scope='Same 15 Validate Prototype.ps1 cases; final assembly; normal health; separate profiles.')
def run(c):
 stem=c['build']+'-'+str(c['seed']);report=out/(stem+'.json');log=out/(stem+'.log')
 args=[str(player),'-batchmode','-nographics','-vectorQA','-seed',str(c['seed']),'-qaBuild',c['build'],'-qaOutput',str(report),'-saveDirectory',str(out/(stem+'-profile')),'-logFile',str(log)]
 r=subprocess.run(args,creationflags=subprocess.CREATE_NO_WINDOW,timeout=180)
 result=json.loads(report.read_text(encoding='utf-8-sig'))
 assert r.returncode==0 and result['passed'] and result['mechanicAssertions']>=877,(stem,r.returncode)
 print(stem+' passed',flush=True);return dict(build=c['build'],seed=c['seed'],passed=True,assertions=result['mechanicAssertions'])
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:manifest['cases']=list(pool.map(run,cases))
manifest['wallSeconds']=time.monotonic()-start;(out/'manifest.json').write_text(json.dumps(manifest,indent=2));print('Final core: 15/15 passed',flush=True)
