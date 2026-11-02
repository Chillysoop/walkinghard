"""Compose the requested 20-second trailer from native frames and native SFX.

Requires numpy, Pillow and imageio-ffmpeg (or the local Build/trailer-tools copy).
The music is synthesized here; no external music or stock combat footage is used.
"""
from pathlib import Path
import json, math, subprocess, sys, wave
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageEnhance

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Build/trailer-tools'))
import imageio_ffmpeg

RAW = ROOT / 'Build/SharedAugments-v080-trailer/TrailerCapture'
OUT = Path.home() / 'OneDrive/Desktop/Walking Hard Combo Trailer'
OUT.mkdir(parents=True, exist_ok=True)
FPS, SR, LENGTH = 30, 44100, 20
SHOTS = [
    ('freight', 2.2, 'THUNDER FREIGHT', 'MARK. LAUNCH. DISCHARGE.'),
    ('ballet', 8.7, 'SHRAPNEL BALLET', 'CUT. RETRIEVE. COLLIDE.'),
    ('funnel', 5.4, 'BALLISTIC FUNNEL', 'PULL. PIERCE. REPEAT.'),
    ('close', 8.1, 'CLOSE QUARTERS', 'FISTS MEET FIREPOWER.'),
    ('shared-pressure', 7.6, 'PERSISTENT PRESSURE', 'THE SECOND CUT LINGERS.'),
    ('shared-conductor', 6.1, 'LIVEWIRE', 'TURN MARKS INTO IMPACT.'),
    ('freight', 6.2, 'CHAIN REACTION', 'MAKE THE CROWD YOUR WEAPON.'),
    ('ballet', 1.2, '', ''),
]

def wav_read(path):
    with wave.open(str(path)) as w:
        assert w.getframerate() == SR and w.getnchannels() == 1
        return np.frombuffer(w.readframes(w.getnframes()), '<i2').astype(np.float32) / 32768

def make_audio():
    n = SR * LENGTH
    score = np.zeros(n, np.float64)
    rng = np.random.default_rng(80314)
    beat = 60 / 144
    def add(at, sound, gain=1):
        start = round(at * SR)
        count = min(len(sound), n - start)
        if count > 0: score[start:start+count] += sound[:count] * gain
    # A punchy 144 BPM original rhythm; every edit lands on a six-beat boundary.
    for b in range(48):
        at = b * beat
        t = np.arange(int(.34 * SR)) / SR
        kick = np.sin(2*np.pi*(47*t + 22*(1-np.exp(-t*32)))) * np.exp(-t*16)
        kick += rng.normal(0, .1, len(t)) * np.exp(-t*190)
        add(at, kick, .64 if b % 4 in (0, 2) else .36)
        if b % 2:
            t = np.arange(int(.18*SR)) / SR
            noise = rng.normal(0, 1, len(t))
            noise = noise - np.convolve(noise, np.ones(11)/11, 'same')
            snare = (noise*.34 + np.sin(2*np.pi*175*t)*.18)*np.exp(-t*23)
            add(at, snare, .75)
        for half in (0, .5):
            t = np.arange(int(.055*SR)) / SR
            noise = rng.normal(0, 1, len(t))
            noise -= np.convolve(noise, np.ones(5)/5, 'same')
            add(at+half*beat, noise*np.exp(-t*100), .065 if half else .045)
        # Distorted low synth with harmonic definition and ducking on the kick.
        for half in (0, .5):
            t = np.arange(int(beat*.46*SR)) / SR
            note = [36.708, 36.708, 43.654, 41.203][(b//4) % 4]
            phase = 2*np.pi*note*t
            bass = np.tanh((np.sin(phase)+.4*np.sin(2*phase)+.22*np.sin(3*phase))*2)
            env = (1-np.exp(-t*100))*np.exp(-t*13)
            add(at+half*beat, bass*env, .19)
        if b >= 12 and b % 2 == 0:
            t = np.arange(int(.24*SR)) / SR
            freq = [293.66, 349.23, 440, 311.13][(b//2) % 4]
            lead = np.sin(2*np.pi*freq*t+.35*np.sin(2*np.pi*freq*2*t))
            add(at, lead*np.exp(-t*17)*(1-np.exp(-t*170)), .1)
    for cut in np.arange(2.5, 20, 2.5):
        t = np.arange(int(.3*SR))/SR
        whoosh = rng.normal(0, 1, len(t))*(t/.3)**2*.06
        add(cut-.3, whoosh)
        t = np.arange(int(.5*SR))/SR
        add(cut, np.sin(2*np.pi*(39*t+8*(1-np.exp(-t*22))))*np.exp(-t*9), .25)
    native = np.zeros(n, np.float64)
    for i, (fixture, start, _, _) in enumerate(SHOTS):
        sound = wav_read(RAW/fixture/'combat.wav')
        length = round(2.5*SR)
        pos = start*SR + np.arange(length)*1.2
        excerpt = np.interp(pos, np.arange(len(sound)), sound, left=0, right=0)
        excerpt[:220] *= np.linspace(0,1,220)
        excerpt[-220:] *= np.linspace(1,0,220)
        native[i*length:(i+1)*length] = excerpt
    mono = np.tanh(score*.77 + native*2.5)*.87
    stereo = np.stack((mono, mono), axis=1)
    stereo[:, 1] += np.roll(score, 165)*.035
    stereo[:220] *= np.linspace(0,1,220)[:, None]
    stereo[-int(.18*SR):] *= np.linspace(1,0,int(.18*SR))[:, None]
    stereo = np.clip(stereo, -.97, .97)
    with wave.open(str(OUT/'trailer-mix.wav'), 'wb') as w:
        w.setnchannels(2);w.setsampwidth(2);w.setframerate(SR)
        w.writeframes((stereo*32767).astype('<i2').tobytes())
    return {'peakDbFS':round(20*np.log10(np.max(np.abs(stereo))),2), 'rmsDbFS':round(20*np.log10(np.sqrt(np.mean(stereo**2))),2)}

FONT = 'C:/Windows/Fonts/bahnschrift.ttf'
def font(size): return ImageFont.truetype(FONT, size)
CYAN = (105, 255, 242, 255)
WHITE = (244, 250, 250, 255)
def overlay(index, local):
    layer = Image.new('RGBA', (1920,1080))
    d = ImageDraw.Draw(layer)
    for y in range(370):
        d.line((0,y,1920,y), fill=(2,9,15,round(170*(1-y/370)**1.5)))
    d.text((70,42), 'WALKING HARD', font=font(30), fill=WHITE)
    d.rectangle((70,91,225,96), fill=CYAN)
    if index < 7:
        kicker, title = SHOTS[index][2:]
        # Labels clear quickly so most of each shot is unobstructed combat.
        opacity = round(255*min(1,max(0,(2.15-local)/.35)))
        if opacity:
            d.text((70,131), kicker, font=font(27), fill=(*CYAN[:3],opacity))
            size = 72 if len(title)<30 else 63
            d.text((68,178), title, font=font(size), fill=(*WHITE[:3],opacity), stroke_width=1)
    else:
        d.rectangle((0,100,1920,610), fill=(2,8,13,110))
        def centered(text,y,size,color):
            f=font(size); box=d.textbbox((0,0),text,font=f)
            d.text(((1920-(box[2]-box[0]))/2,y),text,font=f,fill=color,stroke_width=2)
        centered('WALKING HARD',150,160,WHITE)
        d.rectangle((750,361,1170,367),fill=CYAN)
        centered('BUILD THE CHAIN.',406,62,WHITE)
        centered('BREAK THE CROWD.',486,62,CYAN)
    d.text((70,1024),'COMBO SHOWCASE  /  ACTUAL GAMEPLAY',font=font(21),fill=(208,232,235,205))
    return layer

def main():
    for fixture,_,_,_ in SHOTS:
        assert (RAW/fixture/'00359.png').exists() and (RAW/fixture/'combat.wav').exists()
    audio = make_audio()
    output = OUT/'Walking-Hard-Combos-20s.mp4'
    cmd=[imageio_ffmpeg.get_ffmpeg_exe(),'-y','-hide_banner','-loglevel','error','-f','rawvideo','-vcodec','rawvideo','-pix_fmt','rgb24','-s','1920x1080','-r','30','-i','-','-i',str(OUT/'trailer-mix.wav'),'-c:v','libx264','-preset','medium','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','320k','-t','20','-movflags','+faststart',str(output)]
    process=subprocess.Popen(cmd,stdin=subprocess.PIPE,creationflags=subprocess.CREATE_NO_WINDOW)
    contact=Image.new('RGB',(1280,720))
    for n in range(600):
        index=n//75;local=(n%75)/FPS;fixture,start,_,_=SHOTS[index]
        source=min(359,round((start+local*1.2)*FPS))
        frame=Image.open(RAW/fixture/f'{source:05d}.png').convert('RGB')
        zoom=1.08+.025*math.exp(-local*5)
        cw,ch=1920/zoom,1080/zoom;x=(1920-cw)/2;y=min(1080-ch,620-ch/2)
        frame=frame.crop((round(x),round(y),round(x+cw),round(y+ch))).resize((1920,1080),Image.Resampling.LANCZOS)
        frame=ImageEnhance.Contrast(frame).enhance(1.04)
        frame=Image.alpha_composite(frame.convert('RGBA'),overlay(index,local)).convert('RGB')
        process.stdin.write(frame.tobytes())
        if n%75==30:contact.paste(frame.resize((320,180)),((index%4)*320,(index//4)*360))
        if n%75==60:contact.paste(frame.resize((320,180)),((index%4)*320,(index//4)*360+180))
        if n==555:frame.save(OUT/'Walking-Hard-Combos-20s-poster.jpg',quality=95)
        if n%150==0:print(f'{n}/600 frames',flush=True)
    process.stdin.close();assert process.wait()==0
    contact.save(OUT/'review-contact-sheet.jpg',quality=93)
    metadata={'durationSeconds':20,'width':1920,'height':1080,'fps':30,'frames':600,'sourceSpeed':1.2,'shots':SHOTS,'audio':audio,'source':'Native v0.8.0 camera frames and synchronized original game SFX; staged Rare builds in wave six at 5000 capture health; normal offensive values and physics.','music':'Original synthesized 144 BPM score; authored by render.py. No third-party music.'}
    (OUT/'Walking-Hard-Combos-20s.json').write_text(json.dumps(metadata,indent=2)+'\n',encoding='utf-8')
    print(str(output),flush=True)

if __name__=='__main__': main()
