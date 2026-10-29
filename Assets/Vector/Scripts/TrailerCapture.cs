using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VectorGame
{
    // Opt-in, native frame capture. Normal play never enters this fixture.
    public sealed partial class VectorEncounter
    {
        public bool TrailerMode { get; private set; }
        public float TrailerTime { get; private set; }
        public int TrailerShot { get; private set; }
        public Vector2 TrailerPointer { get; private set; } = new Vector2(-100,-100);
        public ItemDefinition TrailerDrag { get; private set; }
        public bool TrailerHolding { get; private set; }
        readonly int[] trailerCuts = {0,90,150,210,270,420,540,660,705,810,900};
        readonly string[] trailerNames = {"pinball-hook","greatsword","hammer","paired-guns","pick-and-order","pinball-payoff","fusion","ultimate-draft","ultimate-payoff","title"};
        [Serializable] sealed class TrailerSound { public float time; public string clip; public float volume; }
        [Serializable] sealed class TrailerShotRecord { public string name,loadout; public int start,end,kills,collisions,severs,ultimates,fusions; }
        [Serializable] sealed class TrailerManifest { public int width=1920,height=1080,fps=30,frames=900,bossFrames; public string version; public List<TrailerSound> sounds=new List<TrailerSound>(); public List<TrailerShotRecord> shots=new List<TrailerShotRecord>(); }
        TrailerManifest trailerManifest;
        int trailerSoundSerial=-1,trailerCollisions;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartTrailerIfRequested()
        {
            var args=Environment.GetCommandLineArgs();if(Array.IndexOf(args,"-trailerCapture")<0)return;
            var game=UnityEngine.Object.FindFirstObjectByType<VectorEncounter>();
            if(game==null)throw new InvalidOperationException("Trailer needs the encounter scene");
            Application.logMessageReceived+=(m,t,k)=>{if(k==LogType.Exception)Application.Quit(3);};
            game.TrailerMode=true;game.enabled=false;game.StartCoroutine(game.RecordTrailer(args));
        }
        static string TrailerArgument(string[] args,string key)
        {int at=Array.IndexOf(args,key);if(at<0||at+1>=args.Length)throw new ArgumentException("Missing "+key);return args[at+1];}
        IEnumerator RecordTrailer(string[] args)
        {
            string output=Path.GetFullPath(TrailerArgument(args,"-trailerOutput"));
            string encoderPath=Path.GetFullPath(TrailerArgument(args,"-trailerEncoder"));
            if(!File.Exists(encoderPath))throw new FileNotFoundException("FFmpeg missing",encoderPath);
            if(Directory.Exists(output))throw new IOException("Use a fresh trailer output directory");
            Directory.CreateDirectory(output);Directory.CreateDirectory(Path.Combine(output,"proofs"));Directory.CreateDirectory(Path.Combine(output,"audio"));
            Screen.SetResolution(1920,1080,FullScreenMode.Windowed);QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
            Time.captureFramerate=30;Cursor.visible=false;fx.muted=true;
            trailerManifest=new TrailerManifest{version=Application.version};
            tuning.cameraOffset=new Vector3(0,7.5f,-8.5f);tuning.lookAhead=2.4f;cameraRig.fieldOfView=46;
            ExportTrailerSounds(Path.Combine(output,"audio"));
            for(int i=0;i<8;i++)yield return null;
            System.Diagnostics.Process encoder=null;var encoderErrors=new System.Text.StringBuilder();
            int shot=-1;
            for(int frame=0;frame<900;frame++)
            {
                TrailerTime=frame/30f;
                int next=Array.FindLastIndex(trailerCuts,n=>n<=frame);
                if(next!=shot){if(shot>=0)EndTrailerShot(shot);shot=TrailerShot=next;PrepareTrailerShot(shot);yield return null;}
                TrailerActions(frame-trailerCuts[shot]);
                if(State==RunState.Running)
                {
                    for(int n=0;n<2;n++)
                    {
                        AdvanceCombat(1f/60);
                        if(actionDelivered&&actionSerial!=trailerSoundSerial)
                        {trailerSoundSerial=actionSerial;TrailerSoundEvent(ActionSound(),.62f);}
                        if(CollisionHits>trailerCollisions){TrailerSoundEvent("crush",.38f);trailerCollisions=CollisionHits;}
                    }
                }
                if(Wave>=8||finaleActive||State==RunState.Revealing||enemies.Any(e=>e.boss))throw new InvalidOperationException("Boss spoiler guard tripped");
                yield return new WaitForEndOfFrame();
                var pixels=ScreenCapture.CaptureScreenshotAsTexture();
                if(pixels.width!=1920||pixels.height!=1080)throw new InvalidOperationException("Trailer capture resolution changed");
                byte[] bytes=pixels.GetRawTextureData();
                if(encoder==null)
                {
                    string format=bytes.Length==1920*1080*3?"rgb24":bytes.Length==1920*1080*4?"rgba":throw new InvalidOperationException("Unexpected pixel format "+pixels.format);
                    var info=new System.Diagnostics.ProcessStartInfo(encoderPath,"-hide_banner -loglevel warning -y -f rawvideo -pixel_format "+format+" -video_size 1920x1080 -framerate 30 -i pipe:0 -an -vf vflip -c:v libx264 -preset fast -crf 16 -pix_fmt yuv420p -movflags +faststart \""+Path.Combine(output,"gameplay-silent.mp4")+"\"")
                    {UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardError=true};
                    encoder=System.Diagnostics.Process.Start(info);encoder.ErrorDataReceived+=(s,e)=>{if(e.Data!=null)lock(encoderErrors)encoderErrors.AppendLine(e.Data);};encoder.BeginErrorReadLine();
                }
                encoder.StandardInput.BaseStream.Write(bytes,0,bytes.Length);
                if(frame%30==0||frame==899)File.WriteAllBytes(Path.Combine(output,"proofs",frame.ToString("0000")+".png"),pixels.EncodeToPNG());
                Destroy(pixels);
                if(frame%150==0)Debug.Log("TRAILER_FRAME "+frame+" / 900");
            }
            EndTrailerShot(shot);encoder.StandardInput.Close();encoder.WaitForExit();
            File.WriteAllText(Path.Combine(output,"encoder.log"),encoderErrors.ToString());
            if(encoder.ExitCode!=0)throw new InvalidOperationException("Trailer encoder failed: "+encoderErrors);
            File.WriteAllText(Path.Combine(output,"capture.json"),JsonUtility.ToJson(trailerManifest,true));
            Time.captureFramerate=0;Debug.Log("TRAILER_CAPTURE_SUCCESS: 900 frames, zero boss frames");Application.Quit(0);
        }
        void EndTrailerShot(int shot)
        {trailerManifest.shots.Add(new TrailerShotRecord{name=trailerNames[shot],start=trailerCuts[shot],end=trailerCuts[shot+1],loadout=Deck.Loadout,kills=Kills,collisions=CollisionHits,severs=Severs,ultimates=UltimateCasts,fusions=Deck.Fusions});}
        void TrailerSoundEvent(string clip,float volume)
        {trailerManifest.sounds.Add(new TrailerSound{time=TrailerTime,clip=clip,volume=volume});}
        string ActionSound()
        {
            if(ActiveSlot==5)return "push";
            if(actionWeapon!=null)switch(actionWeapon.def.weapon){case WeaponKind.Sword:return "slash";case WeaponKind.Hammer:return "crush";case WeaponKind.Pistol:case WeaponKind.Rifle:case WeaponKind.Shotgun:return "shot";}
            return activeCard?.id==AbilityId.ArcBolt?"electric":activeCard?.id==AbilityId.GravityWell||activeCard?.id==AbilityId.ForcePush||activeCard?.id==AbilityId.MassDriver?"push":"punch";
        }
        void ExportTrailerSounds(string folder)
        {
            foreach(string name in new[]{"punch","finish","push","damage","slash","crush","electric","shot"})
            {
                var clip=(AudioClip)typeof(CombatEffects).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(fx);
                var data=new float[clip.samples*clip.channels];clip.GetData(data,0);
                using(var file=new BinaryWriter(File.Create(Path.Combine(folder,name+".wav"))))
                {int length=data.Length*2;file.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));file.Write(36+length);file.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));file.Write(16);file.Write((short)1);file.Write((short)clip.channels);file.Write(clip.frequency);file.Write(clip.frequency*clip.channels*2);file.Write((short)(clip.channels*2));file.Write((short)16);file.Write(System.Text.Encoding.ASCII.GetBytes("data"));file.Write(length);foreach(float sample in data)file.Write((short)(Mathf.Clamp(sample,-1,1)*32767));}
            }
        }
        LoadoutItem TrailerEquip(string id,PlaceKind place,int slot=0,int rank=3)
        {var item=Deck.Acquire(LoadoutCatalog.Find(id),rank);if(!Deck.Move(item,place,slot))throw new InvalidOperationException("Trailer loadout rejected "+id);return item;}
        void TrailerPinball()
        {
            TrailerEquip("GravityWell",PlaceKind.Sequence,2);TrailerEquip("MassDriver",PlaceKind.Sequence,3);TrailerEquip("ForcePush",PlaceKind.Sequence,4);
            Deck.Move(Deck.Slots[2],PlaceKind.Sequence,0);Deck.Move(Deck.Slots[3],PlaceKind.Sequence,1);
            TrailerEquip("faultline",PlaceKind.Rig);
        }
        void PrepareTrailerShot(int shot)
        {
            Begin(true);Wave=4;OpeningPicks=2;fx.muted=true;trailerSoundSerial=-1;trailerCollisions=0;TrailerDrag=null;TrailerHolding=false;TrailerPointer=new Vector2(-100,-100);
            if(shot==0||shot==5||shot==9)TrailerPinball();
            else if(shot==1){TrailerEquip("greatsword",PlaceKind.Weapon);TrailerEquip("ReapingCut",PlaceKind.Sequence,2);}
            else if(shot==2){TrailerEquip("hammer",PlaceKind.Weapon);TrailerEquip("MassDriver",PlaceKind.Sequence,2);}
            else if(shot==3){TrailerEquip("pistol",PlaceKind.Weapon,0);TrailerEquip("pistol",PlaceKind.Weapon,1);TrailerEquip("ArcBolt",PlaceKind.Sequence,2);}
            else if(shot==4)
            {
                Wave=2;TrailerEquip("faultline",PlaceKind.Rig);TrailerEquip("GravityWell",PlaceKind.Sequence,2);TrailerEquip("ForcePush",PlaceKind.Sequence,4);Deck.Open(DraftKind.Ability,false);
                Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("ArcBolt"),1,2);Deck.Offers[1]=new RewardOffer(LoadoutCatalog.Find("MassDriver"),1,3);Deck.Offers[2]=new RewardOffer(LoadoutCatalog.Find("Breaker"),1,2);
            }
            else if(shot==6)
            {
                Wave=2;TrailerEquip("sword",PlaceKind.Weapon,0,2);TrailerEquip("ArcBolt",PlaceKind.Sequence,2,1);Deck.Acquire(LoadoutCatalog.Find("ArcBolt"),1);Deck.Open(DraftKind.Mixed,false);
                Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("ArcBolt"),1,1);Deck.Offers[1]=new RewardOffer(LoadoutCatalog.Find("hammer"),1,3);Deck.Offers[2]=new RewardOffer(LoadoutCatalog.Find("duelist"),1,2);
            }
            else if(shot==7||shot==8)
            {
                Wave=3;TrailerEquip("sword",PlaceKind.Weapon,0);TrailerEquip("pistol",PlaceKind.Weapon,1);TrailerEquip("GravityWell",PlaceKind.Sequence,2);TrailerEquip("ArcBolt",PlaceKind.Sequence,3);TrailerEquip("MassDriver",PlaceKind.Sequence,4);
                Deck.Open(DraftKind.Ultimate,false);Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("singularity"),1,3);Deck.Offers[1]=new RewardOffer(LoadoutCatalog.Find("tempest"),1,3);Deck.Offers[2]=new RewardOffer(LoadoutCatalog.Find("cyclone"),1,2);
                if(shot==8){TrailerEquip("singularity",PlaceKind.Ultimate);Wave=4;}
            }
            if(shot==4||shot==6||shot==7){State=RunState.Drafting;return;}
            BeginWave();spawnTimer=1000;UltimateRemaining=shot==8?.35f:UltimateCooldown;UltimateAutomatic=true;
            // Curated encounters use real attacks/health/physics. Every actor exists before the cut.
            var staging=new System.Random(720+(shot==5?0:shot));
            int count=shot==0||shot==5||shot==8||shot==9?30:22;
            for(int i=0;i<count;i++)
            {
                float z=2.0f+(i/5)*1.55f+(float)staging.NextDouble()*.85f,x=(i%5-2)*1.6f+((float)staging.NextDouble()-.5f)*1.05f;
                var e=ProbeEnemy(x,z,tuning.enemyHealth*(1+.035f*(Wave-1)));e.radius=2.6f+(i%3)*.3f;e.orbit=i%2==0?-1:1;e.cooldown=.4f+(i%5)*.12f;e.phase=i*.7f;
            }
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();Physics.SyncTransforms();
            // A short unrecorded lead-in makes each edit land on an active fight.
            for(int n=0;n<18;n++)AdvanceCombat(1f/60);
            trailerSoundSerial=actionDelivered?actionSerial:-1;trailerCollisions=CollisionHits;
        }
        void TrailerActions(int frame)
        {
            float t=frame/30f;
            if(TrailerShot==4)
            {
                Vector2 offer=new Vector2(640,175),slot=new Vector2(717,623);
                TrailerPointer=t<.5f?offer:Vector2.Lerp(offer,slot,Mathf.SmoothStep(0,1,(t-.5f)/1.0f));
                TrailerHolding=t>=.5f&&t<1.5f;TrailerDrag=TrailerHolding?LoadoutCatalog.Find("MassDriver"):null;
                if(frame==45){if(!ClaimReward(1,PlaceKind.Sequence,3,false))throw new InvalidOperationException("Trailer reward claim failed");TrailerSoundEvent("electric",.45f);}
                if(t>=2.0f)
                {Vector2 source=new Vector2(519,623),dest=new Vector2(124,623);TrailerPointer=Vector2.Lerp(source,dest,Mathf.SmoothStep(0,1,(t-2.0f)/.8f));TrailerHolding=t<2.8f;TrailerDrag=TrailerHolding?LoadoutCatalog.Find("GravityWell"):null;}
                if(frame==84){if(!MoveItem(Deck.Slots[2],PlaceKind.Sequence,0))throw new InvalidOperationException("Trailer reorder failed");TrailerSoundEvent("finish",.5f);}
                if(t>=3.2f)
                {Vector2 source=new Vector2(717,623),dest=new Vector2(321,623);TrailerPointer=Vector2.Lerp(source,dest,Mathf.SmoothStep(0,1,(t-3.2f)/.8f));TrailerHolding=t<4.0f;TrailerDrag=TrailerHolding?LoadoutCatalog.Find("MassDriver"):null;}
                if(frame==120){if(!MoveItem(Deck.Slots[3],PlaceKind.Sequence,1))throw new InvalidOperationException("Trailer reorder failed");TrailerSoundEvent("finish",.5f);}

            }
            else if(TrailerShot==6)
            {
                TrailerPointer=new Vector2(210,170);
                if(frame==30){if(!ClaimReward(0,PlaceKind.Sequence,2,false)||Deck.Slots[2].rank!=2||FusionHistory.Count!=1)throw new InvalidOperationException("Trailer fusion failed");TrailerSoundEvent("electric",.65f);}
            }
            else if(TrailerShot==7)
            {TrailerPointer=new Vector2(210,170);if(frame==24){if(!ClaimReward(0,PlaceKind.Ultimate,0,false))throw new InvalidOperationException("Trailer ultimate draft failed");TrailerSoundEvent("push",.45f);}}
        }
    }

    public sealed partial class PrototypeHUD
    {
        void TrailerGUI()
        {
            float scale=Screen.width/1920f;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            bool draft=game.State==VectorEncounter.RunState.Drafting;
            if(draft)
            {
                GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1280f,Screen.height/720f,1));Draft(1280,720);
                if(game.TrailerShot==6&&game.TrailerTime>=19){fusionStarted=Time.realtimeSinceStartup-(game.TrailerTime-19);DrawFusionFeedback(1280,720);}
                if(game.TrailerPointer.x>0)
                {
                    var p=game.TrailerPointer;
                    if(game.TrailerDrag!=null){Plate(new Rect(p.x-38,p.y-38,76,76),new Color(.01f,.06f,.08f,.95f),Accent,8,2);ActionGlyph(new Rect(p.x-30,p.y-30,60,60),game.TrailerDrag,white);}
                    Stroke(p,p+new Vector2(0,25),white,3);Stroke(p,p+new Vector2(18,18),white,3);Stroke(p+new Vector2(0,25),p+new Vector2(7,19),white,3);Stroke(p+new Vector2(7,19),p+new Vector2(18,18),white,3);
                }
                GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            }
            else if(game.TrailerShot==5||game.TrailerShot==8)
            {
                GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1600f,Screen.height/900f,1));CombatBar(1600,900);
                if(game.TrailerShot==8)UltimatePanel(new Rect(1270,138,310,180));
                GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            }
            string title="",subtitle="";
            switch(game.TrailerShot)
            {
                case 0:title="KEEP WALKING.";subtitle="BUILD THE ORDER. BREAK THE CROWD.";break;
                case 1:title="CUT.";subtitle="EXECUTION GREATSWORD / TWO-HANDED CLEAVE";break;
                case 2:title="CRUSH.";subtitle="HEAVY HITS. CASCADING COLLISIONS.";break;
                case 3:title="SHOCK.";subtitle="PAIRED PISTOLS + CHAIN LIGHTNING";break;
                case 4:title=game.TrailerTime<11.3f?"CHOOSE 1 OF 3":"ARRANGE YOUR FIVE";subtitle="Two weapon attacks + three abilities. Combat runs automatically.";break;
                case 5:title="MAKE THE ORDER MATTER.";subtitle="PULL  >  LAUNCH  >  COLLIDE";break;
                case 6:title="THREE COPIES. ONE UPGRADE.";subtitle="Identical item. Same rarity. Equipped + stored copies count.";break;
                case 7:title="UNLOCK YOUR ULTIMATE";subtitle="Choose at checkpoint 3. Charges on its own timer.";break;
                case 8:title="UNLOCK YOUR ULTIMATE";subtitle="Choose at checkpoint 3. Charges on its own timer.";break;
            }
            if(game.TrailerShot==9)
            {
                float fade=Mathf.Clamp01((game.TrailerTime-27)/.5f);Box(new Rect(0,0,1920,1080),new Color(.005f,.018f,.025f,.78f*fade));
                Label(new Rect(100,360,1720,140),"WALKING HARD",110,white,true,TextAnchor.MiddleCenter);
                Label(new Rect(100,515,1720,70),"BUILD THE ORDER. BREAK THE CROWD.",36,Accent,true,TextAnchor.MiddleCenter);
                Stroke(new Vector2(680,625),new Vector2(1240,625),Accent,2);
                Label(new Rect(100,665,1720,45),"AN AUTOBATTLER BUILT AROUND YOUR LOADOUT",23,muted,true,TextAnchor.MiddleCenter);
            }
            else if(draft)
            {
                Box(new Rect(0,0,1920,127),new Color(.004f,.02f,.028f,.995f));
                Box(new Rect(48,30,5,68),Accent);Label(new Rect(76,19,1780,56),title,38,white,true);Label(new Rect(78,78,1750,38),subtitle,24,Accent);
            }
            else
            {
                for(int i=0;i<180;i++)Box(new Rect(0,i,1920,1),new Color(.004f,.015f,.022f,.8f*(1-i/180f)));
                Label(new Rect(80,35,1760,80),title,62,white,true,TextAnchor.UpperCenter);
                Label(new Rect(80,120,1760,40),subtitle,24,Accent,true,TextAnchor.UpperCenter);
                if(game.TrailerShot<4)Label(new Rect(70,1008,1780,25),"IN-ENGINE GAMEPLAY  /  CURATED LOADOUTS",17,muted,true);
            }
            GUI.matrix=Matrix4x4.identity;
        }
    }
}
