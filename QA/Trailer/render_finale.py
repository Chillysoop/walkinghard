"""Keep the native-UI edit; finish on an ultimate and a brief branded sales pitch."""
from pathlib import Path
import csv
import json
import subprocess
import sys
from PIL import Image, ImageDraw, ImageFont
from render_systems import ROOT, RAW, OUT, FFMPEG, music

FINISH = ROOT / 'Build/TrailerFinish-v080/FinishCapture'
CHOICE = sys.argv[1] if len(sys.argv) > 1 else 'singularity'
assert CHOICE in ('singularity', 'cyclone')
CUT, OUTRO, FRAMES = 870, 1050, 1200


def title_layer():
    layer = Image.new('RGBA', (1920, 1080))
    d = ImageDraw.Draw(layer)
    cyan, white = '#19ead9', '#e7f7ff'
    panel = [(432,338),(454,316),(1466,316),(1488,338),(1488,762),(1466,784),(454,784),(432,762)]
    d.polygon(panel, fill=(4,15,23,241))
    d.line(panel+[panel[0]], fill=(70,125,142,220), width=2)
    d.line([(432,366),(432,338),(454,316),(508,316)], fill=cyan, width=4)
    d.line([(1412,784),(1466,784),(1488,762),(1488,734)], fill=cyan, width=4)
    brand = ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf', 78)
    pitch = ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf', 64)
    a, b = 'WALKING ', 'HARD'
    width = d.textlength(a+b, font=brand)
    x = (1920-width)/2
    d.text((x,368), a, font=brand, fill=white)
    d.text((x+d.textlength(a,font=brand),368), b, font=brand, fill=cyan)
    d.line([(874,480),(1046,480)], fill=cyan, width=3)
    for y, text in [(524,'BUILD YOUR COMBO.'),(612,'BREAK THE CROWD.')]:
        width = d.textlength(text, font=pitch)
        assert width < 960
        d.text(((1920-width)/2,y), text, font=pitch, fill=white)
    return layer


def main():
    track = OUT/'Walking-Hard-Music-20s.wav'
    if not track.exists():
        track = music()
    sources = [RAW/f'{f:05d}.png' for f in range(CUT)]
    sources += [FINISH/CHOICE/f'{f:05d}.png' for f in range(FRAMES-CUT)]
    assert len(sources) == FRAMES and all(p.exists() for p in sources)
    overlay = title_layer()
    overlay.save(OUT/'finale-outro-overlay.png')
    output = OUT/'Walking-Hard-Finale-20s.mp4'
    cmd = [FFMPEG,'-y','-hide_banner','-loglevel','error',
           '-f','rawvideo','-pix_fmt','rgb24','-s','1920x1080','-r','60','-i','pipe:0',
           '-i',str(track),'-map','0:v:0','-map','1:a:0','-frames:v','1200','-t','20',
           '-c:v','libx264','-preset','medium','-crf','18','-pix_fmt','yuv420p',
           '-c:a','aac','-b:a','320k','-movflags','+faststart',str(output)]
    proc = subprocess.Popen(cmd,stdin=subprocess.PIPE,creationflags=subprocess.CREATE_NO_WINDOW)
    checkpoints = [60,300,480,655,875,915,950,980,1010,1040,1080,1170]
    contact = Image.new('RGB',(1600,750))
    d = ImageDraw.Draw(contact)
    try:
        for f, path in enumerate(sources):
            with Image.open(path) as source:
                im = source.convert('RGB')
            assert im.size == (1920,1080)
            if f >= OUTRO:
                fade = min(1,(f-OUTRO+1)/12)
                layer = overlay.copy()
                layer.putalpha(overlay.getchannel('A').point(lambda a:round(a*fade)))
                im = Image.alpha_composite(im.convert('RGBA'),layer).convert('RGB')
            if f in checkpoints:
                i = checkpoints.index(f)
                contact.paste(im.resize((400,225)),((i%4)*400,(i//4)*250))
                d.text(((i%4)*400+8,(i//4)*250+230),f'{f/60:.2f}s',fill='white')
            if f == 1170:
                im.save(OUT/'Walking-Hard-Finale-20s-poster.jpg',quality=95)
            proc.stdin.write(im.tobytes())
    finally:
        proc.stdin.close()
    if proc.wait() != 0:
        raise RuntimeError('Finale encoding failed')
    contact.save(OUT/'finale-contact-sheet.jpg',quality=94)
    with (FINISH/CHOICE/'cues.csv').open(newline='',encoding='utf-8') as stream:
        cues = list(csv.DictReader(stream))
    first, last = cues[0], cues[FRAMES-CUT-1]
    metadata = {
        'durationSeconds':20,'fps':60,'frames':1200,'width':1920,'height':1080,
        'gameplaySpeed':1,'fullNativeUI':True,'cameraCrop':False,
        'musicOnly':True,'audioSource':track.name,
        'timeline':[
            {'start':0,'end':14.5,'scene':'Previous native-UI melee, weapon choice and ability-order footage'},
            {'start':14.5,'end':17.5,'scene':f'Gravity Well gathering into {"Singularity Burst" if CHOICE=="singularity" else "Razor Cyclone"}, with production impact physics'},
            {'start':17.5,'end':20,'scene':'Branded pitch over continuing aftermath; edge HUD and action bar stay visible'}],
        'outro':['WALKING HARD','BUILD YOUR COMBO.','BREAK THE CROWD.'],
        'finisher':CHOICE,'takeKills':int(last['kills'])-int(first['kills']),
        'takeImpacts':int(last['impacts'])-int(first['impacts']),
        'capture':'Two curated native captures, normal health and offensive values, no augments or guns. Alternate ultimate ending cuts in at 14.5s; title artwork overlays only the final 2.5s.'}
    (OUT/'Walking-Hard-Finale-20s.json').write_text(json.dumps(metadata,indent=2)+'\n',encoding='utf-8')
    print(output,flush=True)


if __name__ == '__main__':
    main()
