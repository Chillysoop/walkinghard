"""Music-only 20-second cut: melee, weapon choice, ability order and ultimate."""
from pathlib import Path
import json
import subprocess
import sys
import wave
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Build/trailer-tools'))
import imageio_ffmpeg

SR, FPS, SECONDS = 44100, 60, 20
RAW = ROOT / 'Build/TrailerSystems-v080/SystemsTrailerCapture'
OUT = Path.home() / 'OneDrive/Desktop/Walking Hard Gameplay Trailer'
OUT.mkdir(parents=True, exist_ok=True)
FFMPEG = imageio_ffmpeg.get_ffmpeg_exe()


def music():
    """Original D-minor synth track: melody, chords, bass and a 144 BPM backbeat."""
    n = SR * SECONDS
    beat = 60 / 144
    drums = np.zeros(n)
    bass = np.zeros(n)
    harmony = np.zeros((n, 2))
    melody = np.zeros((n, 2))
    rng = np.random.default_rng(8112026)

    def put(bus, at, sound, gain=1, pan=0):
        start = round(at * SR)
        count = min(len(sound), n - start)
        if count <= 0:
            return
        if bus.ndim == 1:
            bus[start:start+count] += sound[:count] * gain
        else:
            bus[start:start+count, 0] += sound[:count] * gain * np.sqrt((1-pan)/2)
            bus[start:start+count, 1] += sound[:count] * gain * np.sqrt((1+pan)/2)

    def tone(midi, duration, kind='lead'):
        t = np.arange(round(duration * SR)) / SR
        hz = 440 * 2 ** ((midi - 69) / 12)
        sound = np.zeros(len(t))
        if kind == 'pad':
            for detune in (-.0025, .0025):
                for h in range(1, 6):
                    sound += np.sin(2*np.pi*hz*(1+detune)*h*t) / (h**1.6 * 2)
            env = np.minimum(t/.06, 1) * np.minimum((duration-t)/.22, 1)
        elif kind == 'bass':
            phase = 2*np.pi*hz*t
            sound = np.sin(phase) + .36*np.sin(phase*2) + .16*np.sin(phase*3)
            env = (1-np.exp(-t*160)) * np.exp(-t*5) * np.minimum((duration-t)/.025, 1)
        else:
            for h, gain in [(1, 1), (2, .28), (3, .12), (4, .05)]:
                sound += gain*np.sin(2*np.pi*hz*h*t + .012*np.sin(2*np.pi*5*t))
            env = (1-np.exp(-t*220)) * np.exp(-t*5) * np.minimum((duration-t)/.04, 1)
        return sound * np.maximum(env, 0)

    # Twelve complete bars. The last two resolve to D minor.
    chords = [[50,53,57], [46,50,53], [53,57,60], [48,52,55]] * 2
    chords += [[46,50,53], [48,52,55], [50,53,57], [50,53,57]]
    phrases = [
        [74,77,81,77,79,77,76,74],
        [74,77,82,81,77,74,72,74],
        [72,77,81,84,81,79,77,76],
        [76,79,84,79,77,76,72,76],
    ]
    for bar, chord in enumerate(chords):
        at = bar * beat * 4
        for j, note in enumerate(chord):
            put(harmony, at, tone(note+12, beat*4+.16, 'pad'), .075, [-.65, 0, .65][j])
        for eighth in range(8):
            put(bass, at+eighth*beat/2, tone(chord[0]-12+(12 if eighth in (3,7) else 0), beat*.46, 'bass'), .23)
            phrase = phrases[bar % 4] if bar < 10 else phrases[0]
            note = phrase[eighth]
            # A definite melodic hook throughout, doubled for the final combat section.
            sound = tone(note, beat*.7)
            put(melody, at+eighth*beat/2, sound, .19, -.15 if eighth%2 else .15)
            if bar >= 8:
                put(melody, at+eighth*beat/2, tone(note-12, beat*.7), .07, 0)

    for b in range(48):
        at = b * beat
        t = np.arange(round(.25*SR))/SR
        kick = np.sin(2*np.pi*(47*t + 12*(1-np.exp(-t*42)))) * np.exp(-t*19)
        put(drums, at, kick, .73)
        if b % 2:
            t = np.arange(round(.16*SR))/SR
            noise = rng.normal(0, 1, len(t))
            noise -= np.convolve(noise, np.ones(14)/14, 'same')
            snare = (noise*.28 + np.sin(2*np.pi*186*t)*.21)*np.exp(-t*27)
            put(drums, at, snare, .7)
        for half in (0, .5):
            t = np.arange(round(.065*SR))/SR
            noise = rng.normal(0, 1, len(t))
            noise -= np.convolve(noise, np.ones(4)/4, 'same')
            put(drums, at+half*beat, noise*np.exp(-t*90), .045 if half == 0 else .065)

    # Pump synth layers gently against the kick; stereo echoes remain musical notes.
    t = np.arange(n)/SR
    pump = .38 + .62*(1-np.exp(-(t % beat)*18))
    delay = round(beat*.75*SR)
    echoed = melody.copy()
    echoed[delay:, 0] += melody[:-delay, 1]*.27
    echoed[delay:, 1] += melody[:-delay, 0]*.27
    mix = drums[:, None] + bass[:, None]*pump[:, None] + harmony*pump[:, None] + echoed
    mix = np.tanh(mix*1.1)
    mix[:220] *= np.linspace(0,1,220)[:, None]
    fade = round(.3*SR)
    mix[-fade:] *= np.linspace(1,0,fade)[:, None]
    mix *= .88/np.max(np.abs(mix))
    draft = OUT/'systems-music-master.wav'
    with wave.open(str(draft), 'wb') as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((mix*32767).astype('<i2').tobytes())
    track = OUT/'Walking-Hard-Music-20s.wav'
    run(['-i', str(draft), '-af', 'loudnorm=I=-14:TP=-1.2:LRA=8', '-ar', str(SR), str(track)])
    return track


def run(args):
    subprocess.run([FFMPEG, '-y', '-hide_banner', '-loglevel', 'error', *args], check=True, creationflags=subprocess.CREATE_NO_WINDOW)


def main():
    track = music()
    paths = [RAW/f'{i:05d}.png' for i in range(FPS*SECONDS)]
    assert all(p.exists() for p in paths), 'Incomplete capture'
    output = OUT/'Walking-Hard-Combat-Music-20s.mp4'
    # The only audio input is the composed music track. No game audio is read or mixed.
    run(['-framerate', str(FPS), '-i', str(RAW/'%05d.png'), '-i', str(track),
         '-map', '0:v:0', '-map', '1:a:0', '-frames:v', '1200', '-t', '20',
         '-c:v', 'libx264', '-preset', 'medium', '-crf', '18', '-pix_fmt', 'yuv420p',
         '-c:a', 'aac', '-b:a', '320k', '-movflags', '+faststart', str(output)])
    moments = [60,150,255,310,360,400,440,480,525,560,655,780,950,1015,1100,1199]
    contact = Image.new('RGB', (1600,1000))
    d = ImageDraw.Draw(contact)
    for i, frame in enumerate(moments):
        im = Image.open(paths[frame]); assert im.size == (1920,1080)
        contact.paste(im.convert('RGB').resize((400,225)), ((i%4)*400,(i//4)*250))
        d.text(((i%4)*400+8,(i//4)*250+230), f'{frame/60:.2f}s', fill='white')
    contact.save(OUT/'systems-contact-sheet.jpg', quality=94)
    Image.open(paths[780]).convert('RGB').save(OUT/'Walking-Hard-Combat-Music-20s-poster.jpg', quality=95)
    metadata = {
        'durationSeconds':20, 'width':1920, 'height':1080, 'fps':60, 'frames':1200,
        'fullNativeUI':True, 'gameplaySpeed':1, 'cameraCrop':False,
        'audio':'Original 144 BPM D-minor electronic music with melody, harmony, bass and drums. Music only; no gameplay SFX, voice or editorial effects.',
        'timeline':[
            {'start':0,'end':4,'scene':'Sword-and-hammer combat, launches and body collisions'},
            {'start':4,'end':7,'scene':'Choose a two-handed Greatsword from three melee weapon offers'},
            {'start':7,'end':10,'scene':'Move Gravity Well ahead of Mass Driver using the native sequence editor'},
            {'start':10,'end':15,'scene':'Gather, launch, blade attacks, severing and collision combos'},
            {'start':15,'end':20,'scene':'Queue Razor Cyclone; ultimate execution and combat aftermath'}],
        'capture':'Native framebuffer and production mouse handlers; curated wave-six checkpoint, no augments or guns, normal health and offensive values. UI remains visible.'}
    (OUT/'Walking-Hard-Combat-Music-20s.json').write_text(json.dumps(metadata,indent=2)+'\n',encoding='utf-8')
    print(output,flush=True)


if __name__ == '__main__':
    main()
