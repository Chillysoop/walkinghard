"""Archive compact provenance and check the shipping fixture repair changed no gameplay."""
import hashlib,json,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]; QA=ROOT/'QA'; SIM=QA/'Simulations'
def read(name):
    return [json.loads(x) for p in (SIM/name).glob('worker-*.jsonl') for x in p.read_text(encoding='utf-8-sig').splitlines() if x.strip()]
old={r['seed']:r for r in read('v070-routes-1000')}
new=read('v070-verified-regression');assert len(new)==64
for r in new:
    for key,value in r.items():
        if key!='wallSeconds':assert old[r['seed']][key]==value,(r['seed'],key)
checks=[json.loads(p.read_text(encoding='utf-8-sig')) for p in QA.glob('v070-final-*.json')]
assert len(checks)==62,len(checks)
assert all(c['passed'] and c['mechanicAssertions']==508 for c in checks)
final=json.loads((QA/'v070-verified-full-1741.json').read_text(encoding='utf-8-sig'))
assert final['passed'] and final['itemChoices']==13 and final['augmentChoices']==2
player=ROOT/'Build/Routes-v070-verified'; dll=player/'WalkingHard_Data/Managed/Assembly-CSharp.dll'
out=dict(assemblySha256=hashlib.sha256(dll.read_bytes()).hexdigest(),coreAndVarietyChecks=len(checks),mechanicAssertions=508,finalFullRunPassed=True,matchedGameplayGames=64,batches={})
for name in ['v070-routes-1000','v070-verified-regression','v070-rifle','v070-rifle-baseline','v070-duelist','v070-duelist-baseline','v070-full-baseline','v070-full-rifle','v070-full-duelist']:
    out['batches'][name]=json.loads((SIM/name/'batch.json').read_text(encoding='utf-8-sig'))
(QA/'v070-verification.json').write_text(json.dumps(out,indent=2),encoding='utf-8')
for name in ['21-route-choice','22-route-rewards','23-route-enemies','24-shield-exposed','25-carrier-blast']:
    shutil.copyfile(player/'PresentationReview'/f'{name}.png',QA/f'v070-{name}.png')
print(json.dumps({k:v for k,v in out.items() if k!='batches'},indent=2))
