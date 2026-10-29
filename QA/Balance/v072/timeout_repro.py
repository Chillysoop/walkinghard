import json,subprocess,hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'QA/Simulations/v072-timeout-repro';OUT.mkdir(parents=True,exist_ok=False)
results=[]
for version,folder in [('old','Routes-v070-final'),('current','Reliability-v072-study')]:
 player=ROOT/f'Build/{folder}/WalkingHard.exe'
 for seed in [439,685]:
  stem=f'{version}-{seed}';report=OUT/(stem+'.jsonl');log=OUT/(stem+'.log')
  args=[str(player),'-batchmode','-nographics','-simMode','Fast','-simRuns','1','-seed',str(seed),'-simPolicy','balanced','-simWaves','8','-simMaxSeconds','480','-simOutput',str(report),'-logFile',str(log)]
  subprocess.run(args,creationflags=subprocess.CREATE_NO_WINDOW,check=True,timeout=60)
  r=json.loads(report.read_text(encoding='utf-8-sig').strip());assert r['valid']
  results.append(dict(version=version,seed=seed,assemblySha256=hashlib.sha256((player.parent/'WalkingHard_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest(),result=r['result'],health=r['health'],elapsed=r['elapsed'],loadout=r['loadout'],waits={i['item']:i['values']['rangeWait'] for i in r['items'] if i['equipped']},traces=[s for s in log.read_text(encoding='utf-8-sig').splitlines() if 'SIM_PROGRESS' in s]))
(ROOT/'QA/Balance/v072/timeout-results.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')
for r in results:print(r['version'],r['seed'],r['result'],round(r['elapsed'],1),round(r['health'],1))
