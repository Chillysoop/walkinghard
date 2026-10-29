import subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[3]
for group in ['weapons','ultimates','armor']:
 subprocess.run([sys.executable,str(ROOT/'QA/Balance/controlled-siphon/run.py'),'--player',str(ROOT/'Build/Feedback-v069/WalkingHard.exe'),'--out',str(ROOT/('QA/Simulations/v069-'+group)),'--workers','4','--cases',str(Path(__file__).parent/(group+'-cases.json'))],check=True)
