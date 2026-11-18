"""Rebuild summaries from retained engine output, never combine Fast/Full win rates."""
import collections, gzip, json, statistics
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
NAMES=['branches-fast','branches-full','substitutions-fast','rivet-baseline','rivet-candidate','rivet-full-baseline','rivet-full-candidate']
def mean(rows,key):return round(statistics.mean(r[key] for r in rows),3)
def summarize(rows):
 return dict(n=len(rows),wins=sum(r['result']=='Complete' for r in rows),health=mean(rows,'health'),wave=mean(rows,'waveReached'),seconds=mean(rows,'elapsed'))
summary={};archive=[];manifests={}
for name in NAMES:
 folder=ROOT/'QA/Simulations'/('v092-'+name)
 rows=[json.loads(line) for f in sorted(folder.glob('worker-*.jsonl')) for line in f.read_text(encoding='utf-8-sig').splitlines() if line.strip()]
 if not rows:raise RuntimeError('Missing '+name)
 manifest=json.loads((folder/'batch.json').read_text());manifests[name]=manifest
 assert len(rows)==manifest['runs'] and all(r['valid'] for r in rows)
 groups=collections.defaultdict(list)
 for r in rows:
  c=r['controlled'];group=c['id'].rsplit('-r',1)[0]+'-r'+str(c['rank']);groups[group].append(r)
  archive.append(dict(batch=name,row=r))
 summary[name]={key:summarize(v) for key,v in sorted(groups.items())}
folder=ROOT/'QA/Simulations/v092-branch-bosses';bosses=json.loads((folder/'cases.json').read_text());manifests['branch-bosses']=json.loads((folder/'manifest.json').read_text())
groups=collections.defaultdict(list)
for r in bosses:
 groups[r['build'].rsplit('-s',1)[0]].append(r);archive.append(dict(batch='branch-bosses',row=r))
summary['branch-bosses']={k:dict(n=len(v),wins=sum(r['result']=='Boss defeated' for r in v),health=mean(v,'health'),seconds=mean(v,'seconds'),longest=max(r['seconds'] for r in v)) for k,v in sorted(groups.items())}
(HERE/'summary.json').write_text(json.dumps(summary,indent=2));(HERE/'comparison-manifests.json').write_text(json.dumps(manifests,indent=2))
with gzip.open(HERE/'engine-results.jsonl.gz','wt',encoding='utf-8') as f:
 for r in archive:f.write(json.dumps(r,separators=(',',':'))+'\n')
lines=['# Engine summary','', 'Fast and Full are separate. HP is mean remaining integrity, including zero for defeats. Fixed eight-wave kits receive no later loot; current boss encounters are a separate scope.','']
for name,groups in summary.items():
 lines+=['## '+name,'','| Kit / rarity | Complete | Mean HP | Mean seconds |','|---|---:|---:|---:|']
 for key,s in groups.items():lines.append(f'| {key} | {s["wins"]}/{s["n"]} | {s["health"]:.1f} | {s["seconds"]:.1f} |')
 lines.append('')
(HERE/'engine-summary.md').write_text('\n'.join(lines),encoding='utf-8')
print('Retained',len(archive),'comparisons')
for name in ['rivet-full-baseline','rivet-full-candidate']:print(name,json.dumps(summary[name],indent=2))
