"""Predeclared matching-rarity range and route comparisons; no tuning overrides."""
import copy,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
HERE=Path(__file__).resolve().parent
OUT=ROOT/'QA/Simulations/v072-inputs'
OUT.mkdir(parents=True,exist_ok=True)
BASE=dict(helmet='surveyor',armor='duelist',boots='loader',rig='overclock',ultimate='redline',baseRank=3,matchedWeapons=True,healthFraction=1,category='weapon')
VARIANTS={
 'short':(['VectorTether','ArcBolt','Breaker'],False),
 'short-gun-first':(['VectorTether','ArcBolt','Breaker'],True),
 'guard':(['VectorTether','ArcBolt','Counterguard'],False),
 'mark':(['VectorTether','ArcBolt','ConductiveBrand'],False),
 'drain':(['VectorTether','ArcBolt','DrainBrand'],False),
}
range_cases=[]
for weapon in ['rifle','pistol']:
 for rank in range(1,6):
  for seed in range(51000,51012):
   for variant,(abilities,gun_first) in VARIANTS.items():
    c=copy.deepcopy(BASE)
    c.update(weapons=[weapon,'' if weapon=='rifle' else weapon],abilities=abilities,candidate=weapon,rank=rank,seed=seed,context='range-'+weapon+'-'+variant)
    c['sequence']=['basic0','basic1',*abilities] if gun_first else [abilities[0],'basic0','basic1',*abilities[1:]]
    c['id']=f"{c['context']}-r{rank}-s{seed}"
    range_cases.append(c)
contexts={
 'rifle':dict(weapons=['rifle',''],abilities=['VectorTether','ArcBolt','Breaker'],helmet='surveyor',armor='duelist',boots='loader',rig='overclock',ultimate='redline'),
 'pistols':dict(weapons=['pistol','pistol'],abilities=['VectorTether','ArcBolt','Breaker'],helmet='surveyor',armor='duelist',boots='loader',rig='overclock',ultimate='redline'),
 'sever':dict(weapons=['sword','sword'],abilities=['GravityWell','ReapingCut','SteelJab'],helmet='salvager',armor='duelist',boots='momentum',rig='magnet',ultimate='cyclone'),
 'pinball':dict(weapons=['hammer',''],abilities=['GravityWell','MassDriver','ForcePush'],helmet='visor',armor='plate',boots='momentum',rig='amplifier',ultimate='singularity'),
 'storm':dict(weapons=['wraps','breachhammer'],abilities=['ConductiveBrand','ArcBolt','MassDriver'],helmet='capacitor',armor='grounding',boots='arcstep',rig='relay',ultimate='tempest'),
 'siphon':dict(weapons=['siphonblade','sword'],abilities=['GravityWell','DrainBrand','ReapingCut'],helmet='salvager',armor='reclaimer',boots='predator',rig='overclock',ultimate='harvest'),
}
route_cases=[]
for context,base in contexts.items():
 for route in range(3):
  for seed in range(61000,61016):
   c=copy.deepcopy(BASE);c.update(copy.deepcopy(base));c.update(candidate=c['weapons'][0],context='route-'+context,rank=3,seed=seed,route=route,id=f'route-{context}-{route}-s{seed}');route_cases.append(c)
full_range=[c for c in range_cases if c['rank']==3 and c['seed']<51002 and c['context'].split('-')[-1] in ['short','guard','mark']]
full_route=[c for c in route_cases if c['context'] in ['route-rifle','route-pinball','route-sever'] and c['route']>0 and c['seed']<61001]
for name,cases in [('range',range_cases),('routes',route_cases),('full-range',full_range),('full-routes',full_route)]:
 (OUT/(name+'.json')).write_text(json.dumps({'cases':cases},indent=2),encoding='utf-8')
 print(name,len(cases))
