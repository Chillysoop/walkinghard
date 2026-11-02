using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        IEnumerator CaptureUITrailer()
        {
            // Record the normal game framebuffer after IMGUI, including its full HUD.
            enabled=false;QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception){Debug.LogError("UI_TRAILER_FAILED: "+message);Application.Quit(3);}};
            Screen.SetResolution(1920,1080,false);
            for(int i=0;i<12;i++)yield return null;
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","UITrailerCapture"));Directory.CreateDirectory(root);
            bool previousHold=HoldCheckpointAfterPick;float previousSpeed=CombatSpeed;
            Begin(true);qaBuild="shared-pressure";varietyFixture=false;ConfigureVarietyFixture();
            var gravity=Deck.Slots.First(i=>i?.def.id=="GravityWell");gravity.rank=1;
            Deck.Acquire(gravity.def,1); // The next matching copy can upgrade the equipped ability.
            Deck.Move(Deck.Acquire(LoadoutCatalog.Find("breachhammer"),2),PlaceKind.Weapon,1);
            var ordered=new[]{gravity,Deck.Slots.First(i=>i?.def.id=="ReapingCut"),Deck.Basics[0],Deck.Basics[1],Deck.Slots.First(i=>i?.def.id=="Counterguard")};
            for(int i=0;i<ordered.Length;i++)Deck.Move(ordered[i],PlaceKind.Sequence,i);
            OpeningPicks=2;Wave=4;Health=MaxHealth;State=RunState.Running;sequencePointer=0;expectedSlot=Deck.NextOccupied(0);
            SetCombatSpeed(1);HoldCheckpointAfterPick=true;hud.TrailerCaptureMode();
            hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);
            for(int i=0;i<840;i++)AdvanceCombat(1f/60);
            const int fps=60,frames=1200,rate=44100;
            var audio=new float[rate*20];var samples=new Dictionary<AudioClip,float[]>();int frame=0;
            CombatEffects.CaptureSound=(clip,gain)=>
            {
                if(!samples.TryGetValue(clip,out var data)){data=new float[clip.samples*clip.channels];clip.GetData(data,0);samples[clip]=data;}
                int offset=frame*rate/fps;
                for(int i=0;i<data.Length&&offset+i<audio.Length;i++)audio[offset+i]+=data[i]*gain;
            };
            var cursor=gameObject.AddComponent<TrailerCapturePointer>();cursor.game=this;
            var endFrame=new WaitForEndOfFrame();Vector2 from=Vector2.zero,to=Vector2.zero;
            for(frame=0;frame<frames;frame++)
            {
                if(frame==270||frame==720)
                {
                    State=RunState.Drafting;pendingDrafts.Clear();Deck.Open(frame==270?DraftKind.Weapon:DraftKind.Mixed,false);
                    var ids=frame==270?new[]{"scattergun","sword","hammer"}:new[]{"capacitor","GravityWell","treads"};
                    var ranks=frame==270?new[]{3,3,3}:new[]{3,1,3};
                    for(int i=0;i<3;i++)Deck.Offers[i]=new RewardOffer(LoadoutCatalog.Find(ids[i]),1,ranks[i]);
                    hud.TrailerClearSelection();hud.TrailerPoint(new Vector2(640,120));cursor.visible=true;
                }
                if(frame==450||frame==930)
                {
                    if(!Deck.RewardClaimed)throw new InvalidOperationException("Trailer choice was not claimed through the UI");
                    if(frame==450&&Deck.Weapons[0]?.def.id!="sword")throw new InvalidOperationException("Trailer sword was not equipped");
                    if(frame==930&&gravity.rank!=2)throw new InvalidOperationException("Trailer fusion did not upgrade Gravity Well");
                    hud.TrailerClearSelection();cursor.visible=false;State=RunState.Running;
                    hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);
                }
                if(State==RunState.Running)
                {
                    hud.TrailerPoint(new Vector2(-200,-200));AdvanceCombat(1f/fps);
                    if(Health<=0)throw new InvalidOperationException("Trailer fighter was defeated");
                }
                else
                {
                    int local=frame-(frame<720?270:720);
                    if(local==45)from=hud.TrailerOfferPoint(1);
                    if(local>=45&&local<60)hud.TrailerPoint(Vector2.Lerp(new Vector2(640,120),from,(local-45)/15f));
                    if(local==60)hud.TrailerPoint(from,EventType.MouseDown);
                    if(local==62)to=hud.TrailerDestinationPoint(frame<720?PlaceKind.Weapon:PlaceKind.Sequence,0);
                    if(local>=63&&local<112)
                    {float t=Mathf.SmoothStep(0,1,(local-63)/48f);hud.TrailerPoint(Vector2.Lerp(from,to,t),EventType.MouseDrag);}
                    if(local==112)hud.TrailerPoint(to,EventType.MouseUp);
                    if(local==115)hud.TrailerPinFusion(frame>=720);
                    if(local>=116)hud.TrailerPoint(Vector2.Lerp(to,new Vector2(1080,641),Mathf.Clamp01((local-116)/25f)));
                    cursor.point=hud.TrailerPointer;
                }
                yield return endFrame;
                var image=ScreenCapture.CaptureScreenshotAsTexture();
                if(image.width!=1920||image.height!=1080)throw new InvalidOperationException("Unexpected UI capture size: "+image.width+"x"+image.height);
                File.WriteAllBytes(Path.Combine(root,frame.ToString("00000")+".png"),image.EncodeToPNG());Destroy(image);
                if(frame%300==0)Debug.Log("UI_TRAILER_FRAME: "+frame);
                yield return null;
            }
            CombatEffects.CaptureSound=null;HoldCheckpointAfterPick=previousHold;SetCombatSpeed(previousSpeed);
            using(var writer=new BinaryWriter(File.Create(Path.Combine(root,"combat.wav"))))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+audio.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(audio.Length*2);
                foreach(float sample in audio)writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));
            }
            File.WriteAllText(Path.Combine(root,"take.txt"),"Full native UI, 1920x1080, 60fps, 20 seconds, normal health and 1x combat. Curated wave-four capture: 4.5s combat; 3s weapon choice; 4.5s combat; 3.5s fusion-versus-gear choice; 4.5s combat. Production mouse handlers equip Edge Sword and fuse Gravity Well Common to Uncommon. No UI replacement, speed-up or camera crop.\n"+Deck.Loadout);
            Debug.Log("UI_TRAILER_CAPTURE_SUCCESS");Application.Quit(0);
        }
    }
    public sealed partial class PrototypeHUD
    {
        internal Vector2 TrailerPointer=>reviewPointer;
        internal void TrailerCaptureMode(){tutorialReviewActive=true;editAfterPick=true;}
        internal void TrailerPoint(Vector2 point,EventType? type=null)
        {reviewPointer=point;if(type.HasValue)reviewMouse=new Event{type=type.Value,button=0,mousePosition=point};}
        internal Vector2 TrailerOfferPoint(int index)=>dropRegions.First(r=>r.offer==index).rect.center;
        internal Vector2 TrailerDestinationPoint(PlaceKind place,int index)=>ReviewRegion(place,index).rect.center;
        internal void TrailerPinFusion(bool pinned){fusionPinned=pinned;detailItem=null;detailOffer=null;}
        internal void TrailerClearSelection()
        {ClearLoadoutGesture();presentationHover=null;detailItem=null;detailOffer=null;detailHovered=false;fusionPinned=false;shownFusion=null;seenFusions=game.FusionHistory.Count;reviewPointer=new Vector2(-200,-200);}
    }
    // ScreenCapture excludes the OS cursor. Draw the same pointer used by the native UI harness.
    public sealed class TrailerCapturePointer:MonoBehaviour
    {
        internal VectorEncounter game;internal Vector2 point;internal bool visible;
        Texture2D cursor;
        void Awake()
        {
            cursor=new Texture2D(22,30,TextureFormat.RGBA32,false);cursor.filterMode=FilterMode.Point;
            for(int y=0;y<30;y++)for(int x=0;x<22;x++)
            {int row=29-y;bool inside=row<24&&x<=row*.62f;bool edge=x<2||x>=row*.62f-2||row>=22;cursor.SetPixel(x,y,!inside?Color.clear:edge?Color.black:Color.white);}
            cursor.Apply();
        }
        void OnGUI()
        {if(!visible)return;GUI.depth=-1000;float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.identity;GUI.DrawTexture(new Rect(point.x*scale,point.y*scale,22*scale,30*scale),cursor);}
        void OnDestroy(){if(cursor!=null)Destroy(cursor);}
    }
}
