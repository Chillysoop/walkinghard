"""20-second revision: full native UI, continuous 1x combat, loot and fusion choices."""
from pathlib import Path
import json, subprocess, wave
import numpy as np
from PIL import Image, ImageDraw
from render import ROOT, SR, make_score, wav_read, imageio_ffmpeg

RAW = ROOT / 'Build/TrailerUI-v080/UITrailerCapture'
OUT = Path.home() / 'OneDrive/Desktop/Walking Hard Gameplay Trailer'
OUT.mkdir(parents=True, exist_ok=True)

def main():
    paths=[RAW/f'{i:05d}.png' for i in range(1200)]
    assert all(p.exists() for p in paths), 'Incomplete native capture'
    native=wav_read(RAW/'combat.wav')
    assert len(native)==SR*20
    score=make_score()
    t=np.arange(len(score))/SR
    # Lower the backing track while comparing rewards; native combat sounds stay aligned.
    duck=np.ones_like(score)
    for start,end in [(4.5,7.5),(12,15.5)]:
        blend=np.clip((t-start)/.18,0,1)*np.clip((end-t)/.18,0,1)
        duck-=.45*blend
    mono=np.tanh(score*.7*duck+native*2.6)*.87
    stereo=np.stack((mono,mono),axis=1)
    stereo[:,1]+=np.roll(score,165)*.025*duck
    stereo[:220]*=np.linspace(0,1,220)[:,None]
    stereo[-int(.15*SR):]*=np.linspace(1,0,int(.15*SR))[:,None]
    stereo=np.clip(stereo,-.97,.97)
    with wave.open(str(OUT/'gameplay-loot-mix.wav'),'wb') as w:
        w.setnchannels(2);w.setsampwidth(2);w.setframerate(SR);w.writeframes((stereo*32767).astype('<i2').tobytes())
    output=OUT/'Walking-Hard-Gameplay-Loot-20s.mp4'
    subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-y','-hide_banner','-loglevel','error','-framerate','60','-i',str(RAW/'%05d.png'),'-i',str(OUT/'gameplay-loot-mix.wav'),'-frames:v','1200','-t','20','-c:v','libx264','-preset','medium','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','320k','-movflags','+faststart',str(output)],check=True,creationflags=subprocess.CREATE_NO_WINDOW)
    contact=Image.new('RGB',(1440,1080))
    moments=[60,180,290,340,380,415,520,640,740,800,850,895,960,1060,1140,1199]
    d=ImageDraw.Draw(contact)
    for i,n in enumerate(moments):
        im=Image.open(paths[n]);assert im.size==(1920,1080)
        contact.paste(im.resize((360,202)),((i%4)*360,(i//4)*270))
        d.text(((i%4)*360+8,(i//4)*270+211),f'{n/60:.2f}s',fill='white')
    contact.save(OUT/'review-contact-sheet.jpg',quality=94)
    Image.open(paths[520]).convert('RGB').save(OUT/'Walking-Hard-Gameplay-Loot-20s-poster.jpg',quality=95)
    metadata={'durationSeconds':20,'width':1920,'height':1080,'fps':60,'frames':1200,'gameplaySpeed':1,'fullNativeUI':True,'cameraCrop':False,'timeline':[{'start':0,'end':4.5,'scene':'Continuous native combat with action bar and normal health'},{'start':4.5,'end':7.5,'scene':'Compare Rare scattergun, sword and hammer; drag Edge Sword to hand 1'},{'start':7.5,'end':12,'scene':'Combat with the equipped sword and hammer'},{'start':12,'end':15.5,'scene':'Compare new gear against a matching Gravity Well copy; third Common copy fuses the equipped ability to Uncommon'},{'start':15.5,'end':20,'scene':'Return to combat with the upgraded ability visible in the action bar'}],'capture':'Native full framebuffer; curated wave-four checkpoint offers and two production drag/drop decisions; normal health, offensive values and physics. No replacement UI, speed-up or camera crop.','audio':'Synchronized native game SFX; original 144 BPM score, quieter during decisions.','peakDbFS':round(20*np.log10(np.max(np.abs(stereo))),2)}
    (OUT/'Walking-Hard-Gameplay-Loot-20s.json').write_text(json.dumps(metadata,indent=2)+'\n',encoding='utf-8')
    print(str(output),flush=True)

if __name__=='__main__':main()
