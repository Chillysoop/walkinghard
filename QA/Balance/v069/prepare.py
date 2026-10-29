"""Fixed-loadout gun/sever comparisons; authored 0.6.8 balance, no overrides."""
import copy,json
from pathlib import Path
OUT=Path(__file__).parent
bases={
 'gun':dict(weapons=['pistol','pistol'],abilities=['VectorTether','ArcBolt','Breaker'],helmet='surveyor',armor='duelist',boots='loader',rig='overclock',ultimate='redline'),
 'sever':dict(weapons=['sword','sword'],abilities=['GravityWell','ReapingCut','SteelJab'],helmet='salvager',armor='duelist',boots='momentum',rig='magnet',ultimate='cyclone')}
layouts={'gun':{'pistols':['pistol','pistol'],'scatter-pistol':['scattergun','pistol'],'rifle':['rifle',''],'pistol-sword':['pistol','sword']},
 'sever':{'swords':['sword','sword'],'greatsword':['greatsword',''],'siphon-sword':['siphonblade','sword'],'sword-hammer':['sword','breachhammer']}}
groups={'weapons':[],'ultimates':[],'armor':[]}
def add(group,context,layout,category,candidate,rank,seed_index):
 c=copy.deepcopy(bases[context]);c['weapons']=layouts[context][layout]
 if category!='weapon':c[category]=candidate
 c.update(id=f'{context}-{layout}-{category}-{candidate}-r{rank}-s{seed_index}',context=context+'-'+layout,category=category,candidate=candidate,rank=rank,baseRank=1,matchedWeapons=True,seed=(41000 if context=='gun' else 42000)+seed_index,healthFraction=1)
 groups[group].append(c)
for rank in range(1,6):
 for seed in range(12):
  for context,options in layouts.items():
   for layout,weapons in options.items():add('weapons',context,layout,'weapon',weapons[0],rank,seed)
  for layout in ['pistols','rifle']:
   for ultimate in ['redline','tempest','harvest','singularity','cyclone']:add('ultimates','gun',layout,'ultimate',ultimate,rank,seed)
  for layout in ['swords','greatsword']:
   for armor in ['duelist','reclaimer','plate','grounding','reactive']:add('armor','sever',layout,'armor',armor,rank,seed)
for group,cases in groups.items():
 (OUT/(group+'-cases.json')).write_text(json.dumps({'cases':cases},separators=(',',':'))+'\n',encoding='utf-8');print(group,len(cases))
# Physics coverage selected before seeing Fast results: matched baseline/alternative pairs.
full=[]
for group in groups:
 for c in groups[group]:
  if c['rank']!=3 or not c['id'].endswith('-s0'):continue
  if (group=='weapons' and c['context'] in ['gun-pistols','gun-rifle','sever-swords','sever-greatsword']) or (group=='ultimates' and c['context']=='gun-pistols' and c['candidate']=='tempest') or (group=='armor' and c['context']=='sever-swords' and c['candidate']=='reclaimer'):full.append(c)
(OUT/'full-cases.json').write_text(json.dumps({'cases':full},separators=(',',':'))+'\n',encoding='utf-8');print('Full',len(full))
# Added after all Common-support weapon cases lost: test a smaller, stronger-support slice.
rare=[]
for cases in groups.values():
 for original in cases:
  if original['rank'] not in [1,3,5] or original['seed']%1000>=6:continue
  c=copy.deepcopy(original);c['baseRank']=3;c['id']+='-support-rare';rare.append(c)
(OUT/'rare-support-cases.json').write_text(json.dumps({'cases':rare},separators=(',',':'))+'\n',encoding='utf-8');print('Rare support',len(rare))
