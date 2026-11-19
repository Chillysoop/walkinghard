import argparse,concurrent.futures,hashlib,json,subprocess,time
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--player',required=True);p.add_argument('--out',required=True);p.add_argument('--suites',nargs='+',choices=['core','variety','full'],default=['core','variety','full']);a=p.parse_args()
root=Path(__file__).resolve().parents[2];out=Path(a.out).resolve();out.mkdir(parents=True,exist_ok=False);player=Path(a.player).resolve()
prior=json.loads((root/'QA/build-readability/initial-validation.json').read_text());cases=[(suite,c) for suite in a.suites for c in prior[suite]]
manifest={'assemblySha256':hashlib.sha256((player.parent/'WalkingHard_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest(),'workers':3,'cases':[]};start=time.monotonic()
def run(entry):
 suite,c=entry;stem=suite+'-'+c['build']+'-'+str(c['seed']);report=out/(stem+'.json')
 args=[str(player),'-batchmode','-nographics','-vectorQA','-seed',str(c['seed']),'-qaBuild',c['build'],'-qaOutput',str(report),'-saveDirectory',str(out/(stem+'-profile')),'-logFile',str(out/(stem+'.log'))]
 if suite=='full':args+=['-qaFullRun']
 r=subprocess.run(args,creationflags=subprocess.CREATE_NO_WINDOW,timeout=240);v=json.loads(report.read_text(encoding='utf-8-sig'))
 assert r.returncode==0 and v['passed'] and v['mechanicAssertions']>=929,(stem,r.returncode)
 return {'suite':suite,'build':c['build'],'seed':c['seed'],'passed':v['passed'],'assertions':v['mechanicAssertions'],'health':v['health']}
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
 futures=[pool.submit(run,c) for c in cases]
 for f in concurrent.futures.as_completed(futures):
  manifest['cases'].append(f.result());(out/'manifest.json').write_text(json.dumps(manifest,indent=2))
  if len(manifest['cases'])%10==0:print(str(len(manifest['cases']))+'/'+str(len(cases))+' regression cases passed',flush=True)
manifest['wallSeconds']=time.monotonic()-start;(out/'manifest.json').write_text(json.dumps(manifest,indent=2));print('All '+str(len(cases))+' regression cases passed',flush=True)
