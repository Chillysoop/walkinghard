import json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).parent;SIM=ROOT/'QA/Simulations'
cases=[]
for name in ['controlled-siphon-fast-3600','controlled-siphon-rare-support-480']:
    cases+=json.loads((SIM/name/'cases.json').read_text(encoding='utf-8'))['cases']
blade=lambda c:'siphonblade' in c['weapons']
armor=lambda c:c['armor']=='reclaimer'
redline=lambda c:c['ultimate']=='redline'
def save(name,j): (OUT/(name+'.json')).write_text(json.dumps(j,separators=(',',':'))+'\n',encoding='utf-8')
flags={'control':dict(legacySiphonBlade=True,legacyReclaimer=True,legacyRedline=True), 'blade':dict(legacyReclaimer=True,legacyRedline=True), 'armor':dict(legacySiphonBlade=True,legacyRedline=True), 'combined':dict(legacyRedline=True), 'redline':{}}
for name,predicate in [('blade',blade),('armor',armor),('combined',lambda c:blade(c) or armor(c)),('redline',redline)]:
    selected=[c for c in cases if predicate(c)];save(name+'-cases',{'cases':selected});save(name+'-rules',flags[name]);print(name,len(selected))
# One seed per family/context/tier on both support levels: covers affected and reused cases.
control=[c for c in cases if c['seed']%1000==0]
save('control-cases',{'cases':control});save('control-rules',flags['control']);print('control',len(control))
# Preselect six existing Full cases; first two weapon/Redline seeds and first armor seed.
full=json.loads((ROOT/'QA/Balance/controlled-siphon/full-cases.json').read_text(encoding='utf-8'))['cases']
selected=[c for c in full if (c['candidate']=='siphonblade' and c['seed'] in (31000,31001)) or (c['candidate']=='reclaimer' and c['seed'] in (31000,32000)) or (c['candidate']=='redline' and c['seed'] in (33000,33001))]
save('full-cases',{'cases':selected});print('Full',len(selected))
