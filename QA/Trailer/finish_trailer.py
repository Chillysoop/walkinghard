"""Mix original procedural music with event-synced game audio, then verify the MP4."""
from pathlib import Path
import argparse, json, subprocess, wave
import numpy as np
from PIL import Image, ImageDraw

p=argparse.ArgumentParser()
p.add_argument('capture',type=Path)
p.add_argument('--ffmpeg',required=True,type=Path)
p.add_argument('--output',required=True,type=Path)
a=p.parse_args()
root=a.capture.resolve(); out=a.output.resolve();out.parent.mkdir(parents=True,exist_ok=True)
manifest=json.loads((root/'capture.json').read_text(encoding='utf-8'))
assert manifest['frames']==900 and manifest['fps']==30 and manifest['bossFrames']==0
shots={s['name']:s for s in manifest['shots']}
assert shots['greatsword']['severs']>0, 'Sword shot must show real severing'
assert shots['hammer']['kills']>0, 'Hammer shot must connect'
assert shots['paired-guns']['kills']>0, 'Gun shot must connect'
assert shots['pinball-payoff']['collisions']>0, 'Pinball shot must show real collisions'
assert shots['fusion']['fusions']==1, 'Fusion demonstration must fuse'
assert shots['ultimate-payoff']['ultimates']>0, 'Ultimate must actually execute'
assert shots['pick-and-order']['loadout'].split(' > ')[0].endswith('Gravity Well · Rare')
sr=48000; count=sr*30; rng=np.random.default_rng(72030)
music=np.zeros((count,2),dtype=np.float64)
effects=np.zeros_like(music)

def add(bus,sound,time,level=1,pan=0):
    start=int(time*sr)
    if start<0 or start>=count:return
    sound=np.asarray(sound)[:count-start]
    if sound.ndim==1:
        sound=np.column_stack((sound*np.sqrt((1-pan)/2),sound*np.sqrt((1+pan)/2)))
    bus[start:start+len(sound)]+=sound*level

def tone(freq,duration):
    t=np.arange(int(duration*sr))/sr
    # Original restrained industrial bass: harmonics, no external music samples.
    phase=2*np.pi*freq*t
    return np.tanh(1.6*(np.sin(phase)+.22*np.sin(phase*2)+.1*np.sin(phase*3)))*np.minimum(1,t/.008)*np.exp(-t*7)

def kick():
    t=np.arange(int(.32*sr))/sr
    phase=2*np.pi*(44*t+78*.035*(1-np.exp(-t/.035)))
    return np.sin(phase)*np.exp(-t*18)+rng.normal(0,.11,len(t))*np.exp(-t*100)

def snare():
    t=np.arange(int(.15*sr))/sr
    noise=rng.normal(0,1,len(t));noise=np.concatenate(([0],np.diff(noise)))*.4
    return (noise+np.sin(2*np.pi*170*t)*.35)*np.exp(-t*25)

def hat():
    t=np.arange(int(.055*sr))/sr;noise=rng.normal(0,1,len(t))
    return np.concatenate(([0],np.diff(noise)))*np.exp(-t*85)*.18

bass=[73.416,73.416,65.406,87.307] # D2, D2, C2, F2; authored for this edit.
for beat in range(54):
    time=beat*.5
    add(music,kick(),time,.55)
    if beat%2:add(music,snare(),time,.25)
    for sub in range(2):add(music,hat(),time+sub*.25,.32,(-1 if sub==0 else 1)*.35)
    freq=bass[(beat//4)%4]
    add(music,tone(freq,.4),time,.27)
    if beat>=6:add(music,tone(freq,.2),time+.25,.15)
    if 6<=beat<18 or 28<=beat<36 or 47<=beat<54:
        notes=[146.832,220,261.626,293.665]
        add(music,tone(notes[beat%4],.3),time+.125,.07,(beat%3-1)*.55)

# Deliberate transition accents and a resolving title sting.
for time in [0,3,5,7,9,14,18,22,23.5,27]:
    add(music,kick(),time,.45)
    t=np.arange(int(.55*sr))/sr
    sweep=rng.normal(0,1,len(t))*np.exp(-t*9)*.025
    add(music,sweep,time,.7)
for freq in [73.416,146.832,220,293.665]:
    t=np.arange(sr*3)/sr
    add(music,np.sin(2*np.pi*freq*t)*np.exp(-t*1.7),27,.09)
envelope=np.ones(count)
for start,end in [(9,14),(18,23.5)]:
    envelope[int(start*sr):int(end*sr)]=.4
envelope[:int(.04*sr)]*=np.linspace(0,1,int(.04*sr))
envelope[-int(.3*sr):]*=np.linspace(1,0,int(.3*sr))
music*=envelope[:,None]

for event in manifest['sounds']:
    with wave.open(str(root/'audio'/(event['clip']+'.wav')),'rb') as f:
        source_rate=f.getframerate();channels=f.getnchannels()
        samples=np.frombuffer(f.readframes(f.getnframes()),dtype='<i2').astype(float)/32768
        samples=samples.reshape(-1,channels).mean(axis=1)
    target=np.arange(round(len(samples)*sr/source_rate))*source_rate/sr
    samples=np.interp(target,np.arange(len(samples)),samples)
    add(effects,samples,event['time'],event['volume']*1.5)
mix=music+effects
peak=float(np.max(np.abs(mix)))
mix*=min(1,.89/peak)
wav=root/'trailer-mix.wav'
with wave.open(str(wav),'wb') as f:
    f.setnchannels(2);f.setsampwidth(2);f.setframerate(sr)
    f.writeframes((mix*32767).astype('<i2').tobytes())
subprocess.run([str(a.ffmpeg),'-hide_banner','-loglevel','warning','-y','-i',str(root/'gameplay-silent.mp4'),'-i',str(wav),'-map','0:v:0','-map','1:a:0','-c:v','copy','-c:a','aac','-b:a','256k','-af','loudnorm=I=-16:TP=-1:LRA=9','-ar','48000','-t','30','-movflags','+faststart',str(out)],check=True)
probe=a.ffmpeg.with_name('ffprobe.exe')
meta=json.loads(subprocess.check_output([str(probe),'-v','error','-count_frames','-show_streams','-show_format','-of','json',str(out)]))
video=next(s for s in meta['streams'] if s['codec_type']=='video')
audio=next(s for s in meta['streams'] if s['codec_type']=='audio')
assert int(video['nb_read_frames'])==900 and video['width']==1920 and video['height']==1080
assert abs(float(meta['format']['duration'])-30)<.05 and audio['channels']==2
subprocess.run([str(a.ffmpeg),'-v','error','-i',str(out),'-f','null','-'],check=True)

sheet=Image.new('RGB',(1280,6*202),(5,16,22));draw=ImageDraw.Draw(sheet)
proofs=sorted((root/'proofs').glob('*.png'))
for i,file in enumerate(proofs[:30]):
    x=(i%5)*256;y=(i//5)*202
    im=Image.open(file).convert('RGB');im.thumbnail((256,176));sheet.paste(im,(x,y+22))
    draw.text((x+8,y+4),f'{int(file.stem)/30:04.1f}s',fill=(80,240,220))
sheet.save(out.with_name('Walking-Hard-trailer-contact-sheet.jpg'),quality=93)
poster=Image.open(root/'proofs'/'0870.png').convert('RGB');poster.save(out.with_suffix('.jpg'),quality=95)
verification={'duration':meta['format']['duration'],'frames':900,'resolution':'1920x1080','fps':30,'audio':'stereo AAC','bossFrames':0,'sourceVersion':manifest['version'],'mixPeakBeforeNormalization':peak,'outputBytes':out.stat().st_size,'shots':manifest['shots'],'audioEvents':len(manifest['sounds']),'music':'Original procedural score created for this trailer; no third-party music.'}
out.with_suffix('.json').write_text(json.dumps(verification,indent=2),encoding='utf-8')
print(json.dumps(verification,indent=2))
