"""One changed slot per pair, ten matched seeds, every focal rarity. No tuning overrides."""
from pathlib import Path
import json

OUT=Path(__file__).resolve().parent
CONTEXTS={
    'rail': ['rifle',''],
    'pistols': ['pistol','pistol'],
    'hybrid': ['pistol','siphonblade'],
}
CASES=[]
for context,weapons in CONTEXTS.items():
    for category,candidates in [('ability',['SuppressiveVolley','Breaker','ArcBolt','Counterguard']),('armor',['recoil','duelist','plate','reclaimer'])]:
        for rank in range(1,6):
            for seed in range(76000,76010):
                for candidate in candidates:
                    abilities=[candidate,'VectorTether','DrainBrand'] if category=='ability' else ['ArcBolt','VectorTether','Counterguard']
                    CASES.append(dict(id=f'{context}-{category}-{candidate}-r{rank}-s{seed}',context=context,category=category,candidate=candidate,
                        weapons=weapons,abilities=abilities,sequence=[abilities[0],'basic0','basic1',abilities[1],abilities[2]],
                        helmet='surveyor',armor=candidate if category=='armor' else 'duelist',boots='loader',rig='overclock',ultimate='redline',
                        rank=rank,baseRank=3,healthFraction=1,seed=seed,route=0))
(OUT/'fast-cases.json').write_text(json.dumps({'cases':CASES},indent=2))
# Fresh-process PhysX confirmation: two seeds, three rarities, two gun contexts,
# focal item versus its intended-role alternative. Keep this separate from Fast.
full=[c for c in CASES if c['context'] in ('rail','pistols') and c['rank'] in (1,3,5) and c['seed'] in (76000,76001)
      and c['candidate'] in ('SuppressiveVolley','Breaker','recoil','duelist')]
(OUT/'full-cases.json').write_text(json.dumps({'cases':full},indent=2))
print(f'{len(CASES)} Fast; {len(full)} Full')

# Resolve ceiling effects and test Volley's advertised lightning pairing.
follow=[]
for c in CASES:
    if c['rank'] not in (1,3,5) or c['seed']>=76006: continue
    if c['category']=='armor':
        d=dict(c,baseRank=1,context=c['context']+'-starter',id=c['id']+'-starter');follow.append(d)
    elif c['context'] in ('rail','pistols') and c['candidate']!='ArcBolt':
        skills=[c['candidate'],'ArcBolt','VectorTether']
        d=dict(c,abilities=skills,sequence=[skills[0],skills[1],'basic0','basic1',skills[2]],context=c['context']+'-lightning',id=c['id']+'-lightning');follow.append(d)
(OUT/'followup-cases.json').write_text(json.dumps({'cases':follow},indent=2))
print(f'{len(follow)} ceiling / synergy cases')
