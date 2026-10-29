"""Separate, unapplied candidates, paired to the v069 fixed-loadout evidence."""
import json, subprocess, sys
from pathlib import Path

ROOT=Path(__file__).resolve().parents[3]
HERE=Path(__file__).resolve().parent
cases=[]
for group in ['weapons','ultimates','armor','rare-support']:
    cases += json.loads((ROOT/f'QA/Balance/v069/{group}-cases.json').read_text(encoding='utf-8'))['cases']
experiments={'rifle': {'items':[{'id':'rifle','damageMultiplier':52/42}]},
             'duelist': {'items':[{'id':'duelist','wardHits':3}]}}
for name,rules in experiments.items():
    subset=[c for c in cases if ('rifle' in c['weapons'] if name=='rifle' else c['category']=='armor' and c['candidate']=='duelist')]
    (HERE/f'{name}-cases.json').write_text(json.dumps({'cases':subset},indent=2),encoding='utf-8')
    (HERE/f'{name}-rules.json').write_text(json.dumps(rules,indent=2),encoding='utf-8')
    subprocess.run([sys.executable,str(ROOT/'QA/Balance/controlled-siphon/run.py'),'--player',str(ROOT/'Build/Feedback-v069-final/WalkingHard.exe'),'--out',str(ROOT/f'QA/Simulations/v070-{name}'),'--workers','4','--cases',str(HERE/f'{name}-cases.json'),'--balance',str(HERE/f'{name}-rules.json')],check=True)
