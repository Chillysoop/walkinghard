"""Collect reviewed 0.8.8 evidence without deleting raw runs or player files."""
import json
import shutil
from pathlib import Path

root = Path(__file__).resolve().parents[2]
out = Path(__file__).resolve().parent

def copy(source, name):
    target = out / name
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(root / source, target)

for source, dest in [('v088-fixtures-scan', 'fixtures'), ('v088-bosses', 'bosses'),
                     ('v088-champions', 'champions'), ('v088-journey-unpooled', 'unpooled'),
                     ('v088-journey-pooled', 'pooled'), ('v088-journey-original-scan', 'original-scan'), ('v088-journey-scan', 'scan')]:
    for name in ['report.json', 'manifest.json']:
        copy(f'QA/{source}/{name}', f'{dest}-{name}')

copy('QA/v088-core-manifest.json', 'initial-core-manifest.json')
copy('QA/v088-final-core-manifest.json', 'core-manifest.json')
core = [json.loads(p.read_text(encoding='utf-8-sig')) for p in sorted((root / 'QA').glob('v088-final-core-*.json')) if p.name != 'v088-final-core-manifest.json']
assert len(core) == 15 and all(r['passed'] for r in core)
keys = ['build', 'seed', 'result', 'kills', 'launches', 'collisions', 'ragdolls', 'health', 'visibleSpawns', 'orderErrors', 'passed']
(out / 'core-summary.json').write_text(json.dumps([{k: r[k] for k in keys} for r in core], indent=2), encoding='utf-8')

shots = {
    'v088-fixtures-scan': ['family-3-270.png', 'singularity-charge.png', 'singularity-release.png',
                          'tempest-release.png', 'cyclone-release.png', 'redline-release.png',
                          'harvest-recovery.png', 'threat-over-ultimate-1280.png', 'threat-over-ultimate-2560.png'],
    'v088-bosses': ['01-shift-double-warning.png', '07-pursuer-execution.png', 'bell-720p-reduced-motion.png'],
    'v088-champions': ['05-interceptor-warning.png'],
    'v088-journey-scan': ['wave-21-speed-4.png', 'result.png'],
}
for folder, names in shots.items():
    for name in names:
        copy(f'QA/{folder}/{name}', f'captures/{name}')
for p in (root / 'QA/v088-fixtures-scan').glob('*.wav'):
    copy(p.relative_to(root), f'audio/{p.name}')
copy('QA/v088-audio-summary.json', 'audio-summary.json')

failures = []
for filename in ['Logs/build-v088-initial.log', 'QA/v088-fixtures-initial/player.log']:
    lines = (root / filename).read_text(encoding='utf-8-sig').splitlines()
    failures.append(filename + '\n' + '\n'.join(line for line in lines if 'error CS' in line or 'Exception:' in line))
(out / 'initial-errors.txt').write_text('\n\n'.join(failures), encoding='utf-8')

comparisons = []
for mode in ['unpooled', 'pooled', 'original-scan', 'scan']:
    r = json.loads((out / f'{mode}-report.json').read_text(encoding='utf-8-sig'))
    assert r['passed']
    comparisons.append({k: r[k] for k in ['assembly', 'result', 'wave', 'kills', 'health', 'unpooled', 'frames']})
(out / 'performance-comparison.json').write_text(json.dumps(comparisons, indent=2), encoding='utf-8')
print('Collected core, native fixtures, audio, boss/champion and full-journey evidence.')
