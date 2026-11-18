"""Explicit equal-rarity kits through each current journey boss; no tuning overrides."""
import argparse, concurrent.futures, hashlib, json, subprocess, time
from pathlib import Path

p=argparse.ArgumentParser();p.add_argument('--player',required=True);p.add_argument('--out',required=True);p.add_argument('--workers',type=int,default=3);a=p.parse_args()
root=Path(__file__).resolve().parent;out=Path(a.out).resolve();out.mkdir(parents=True,exist_ok=False)
jobs=json.loads((root/'branch-boss-cases.json').read_text())['cases'];player=Path(a.player).resolve()
manifest=dict(player=str(player),assemblySha256=hashlib.sha256((player.parent/'WalkingHard_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest(),kits=len(jobs),encounters=len(jobs)*3,workers=a.workers)
(out/'manifest.json').write_text(json.dumps(manifest,indent=2));start=time.monotonic()
def run(i):
 d=out/f'worker-{i}';d.mkdir();case=d/'cases.json';case.write_text(json.dumps({'cases':jobs[i::a.workers]}))
 command=[str(player),'-batchmode','-nographics','-districtReview','-districtBalance','-districtCases',str(case),'-saveDirectory',str(d/'profile'),'-reviewOutput',str(d),'-logFile',str(d/'player.log')]
 r=subprocess.run(command,creationflags=subprocess.CREATE_NO_WINDOW,timeout=1800)
 report=json.loads((d/'report.json').read_text(encoding='utf-8-sig'))
 if r.returncode or not report['passed']:raise RuntimeError(f'Worker {i}: {r.returncode}, {report.get("failure")}')
 return report['cases']
with concurrent.futures.ThreadPoolExecutor(max_workers=a.workers) as pool:
 futures=[pool.submit(run,i) for i in range(a.workers)]
 while not all(f.done() for f in futures):
  time.sleep(20);count=0
  for f in out.glob('worker-*/report.json'):
   try:count+=len(json.loads(f.read_text(encoding='utf-8-sig'))['cases'])
   except (ValueError,OSError):pass
  print(f'{count}/{len(jobs)*3} boss encounters; {time.monotonic()-start:.0f}s',flush=True)
 rows=[r for f in futures for r in f.result()]
identities={(r['build'],r['wave']) for r in rows}
assert len(rows)==len(jobs)*3==len(identities)
manifest.update(wallSeconds=time.monotonic()-start,wins=sum(r['result']=='Boss defeated' for r in rows),defeats=sum(r['result']=='Defeat' for r in rows),timeouts=sum(r['result']=='Timeout' for r in rows))
(out/'cases.json').write_text(json.dumps(rows,indent=2));(out/'manifest.json').write_text(json.dumps(manifest,indent=2));print(json.dumps(manifest),flush=True)
