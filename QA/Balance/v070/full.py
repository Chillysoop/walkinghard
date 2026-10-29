import json,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]; HERE=Path(__file__).resolve().parent
allcases=json.loads((ROOT/'QA/Balance/v069/rare-support-cases.json').read_text())['cases']
jobs=[c for c in allcases if c['rank']==3 and c['category']=='weapon' and c['context'] in ['gun-rifle','sever-swords','sever-greatsword'] and c['seed']%1000<2]
assert len(jobs)==6,len(jobs)
for name,subset in [('full-baseline',jobs),('full-rifle',[c for c in jobs if 'rifle' in c['weapons']]),('full-duelist',[c for c in jobs if 'rifle' not in c['weapons']])]:
    path=HERE/f'{name}-cases.json';path.write_text(json.dumps({'cases':subset},indent=2))
    args=[sys.executable,str(ROOT/'QA/Balance/controlled-siphon/run.py'),'--player',str(ROOT/'Build/Feedback-v069-final/WalkingHard.exe'),'--out',str(ROOT/f'QA/Simulations/v070-{name}'),'--mode','Full','--workers','2','--cases',str(path)]
    if name!='full-baseline':args+=['--balance',str(HERE/(name.removeprefix('full-')+'-rules.json'))]
    subprocess.run(args,check=True)
