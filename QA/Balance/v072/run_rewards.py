"""Isolate reward emphasis on standard encounters using the same 240 seeds/policies."""
import json,subprocess,time,hashlib,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
PLAYER=ROOT/'Build/Reliability-v072-study/WalkingHard.exe'
linked='--linked' in sys.argv
for reward in (['shield','volatile'] if linked else ['standard','shield','volatile']):
 encounter=reward if linked else 'standard'
 prefix='linked' if linked else 'reward'
 out=ROOT/f'QA/Simulations/v072-{prefix}-{reward}';out.mkdir(parents=True,exist_ok=False)
 start=time.monotonic();workers=[]
 meta=dict(player=str(PLAYER),assemblySha256=hashlib.sha256((PLAYER.parent/'WalkingHard_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest(),runs=240,mode='Fast',firstSeed=71000,policy='mixed',encounterRoute=encounter,rewardRoute=reward,maxCombatSeconds=480)
 try:
  for i in range(4):
   args=[str(PLAYER),'-batchmode','-nographics','-simMode','Fast','-simRuns','60','-seed',str(71000+i),'-simStride','4','-simPolicy','mixed','-simWaves','8','-simMaxSeconds','480','-simEncounterRoute',encounter,'-simRewardRoute',reward,'-simOutput',str(out/f'worker-{i}.jsonl'),'-logFile',str(out/f'worker-{i}.log')]
   workers.append(subprocess.Popen(args,creationflags=subprocess.CREATE_NO_WINDOW))
  while any(p.poll() is None for p in workers):
   time.sleep(1)
   if time.monotonic()-start>300:raise TimeoutError(reward)
  rows=[json.loads(s) for p in out.glob('worker-*.jsonl') for s in p.read_text(encoding='utf-8-sig').splitlines()]
  assert all(p.returncode==0 for p in workers) and len(rows)==240 and len({r['seed'] for r in rows})==240 and all(r['valid'] for r in rows)
  if not linked:assert all(r['shieldBearers']==r['volatileCarriers']==0 for r in rows)
  meta.update(wallSeconds=time.monotonic()-start,valid=240,wins=sum(r['result']=='Complete' for r in rows),timeouts=sum(r['result']=='TimedOut' for r in rows))
  (out/'batch.json').write_text(json.dumps(meta,indent=2),encoding='utf-8');print(reward,meta['wins'],round(meta['wallSeconds'],1),flush=True)
 finally:
  for p in workers:
   if p.poll() is None:p.kill();p.wait()
