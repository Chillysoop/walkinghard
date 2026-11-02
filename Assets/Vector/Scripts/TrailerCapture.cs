using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorGame
{
    // Explicit capture-only mode. Ordinary play never enters this coroutine.
    public sealed partial class VectorEncounter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartTrailerIfRequested()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-trailerCapture")<0)return;
            var game=UnityEngine.Object.FindFirstObjectByType<VectorEncounter>();
            game.StartCoroutine(game.CaptureTrailer());
        }
        IEnumerator CaptureTrailer()
        {
            enabled=false;hud.enabled=false;QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;tuning.heroHealth=5000;
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","TrailerCapture"));Directory.CreateDirectory(root);
            var target=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);target.Create();
            var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            var clips=new[]{"freight","ballet","funnel","close","shared-pressure","shared-conductor"};
            var args=Environment.GetCommandLineArgs();int only=Array.IndexOf(args,"-trailerOnly");if(only>=0)clips=new[]{args[only+1]};
            const int fps=30,frames=360,rate=44100;
            foreach(string fixture in clips)
            {
                Begin(true);qaBuild=fixture;varietyFixture=false;ConfigureVarietyFixture();
                hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);
                // Stage a late-wave build with production enemies, entry rules and combat.
                // Extra health prevents an interrupted take; no offensive tuning changes.
                Wave=6;OpeningPicks=2;Health=5000;State=RunState.Running;sequencePointer=0;
                expectedSlot=Deck.NextOccupied(0);UltimateAutomatic=true;UltimateRemaining=1.5f;SetCombatSpeed(1);
                cameraRig.fieldOfView=42;
                for(int i=0;i<540;i++)AdvanceCombat(1f/fps);
                string directory=Path.Combine(root,fixture);Directory.CreateDirectory(directory);
                var mix=new float[rate*frames/fps];var samples=new Dictionary<AudioClip,float[]>();int frame=0;
                CombatEffects.CaptureSound=(clip,gain)=>
                {
                    if(!samples.TryGetValue(clip,out var data)){data=new float[clip.samples*clip.channels];clip.GetData(data,0);samples[clip]=data;}
                    int offset=frame*rate/fps;
                    for(int i=0;i<data.Length&&offset+i<mix.Length;i++)mix[offset+i]+=data[i]*gain;
                };
                for(frame=0;frame<frames;frame++)
                {
                    if(State!=RunState.Running)throw new InvalidOperationException("Trailer take ended early: "+fixture);
                    AdvanceCombat(1f/fps);
                    var request=new RenderPipeline.StandardRequest{destination=target};RenderPipeline.SubmitRenderRequest(cameraRig,request);
                    var previous=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();RenderTexture.active=previous;
                    File.WriteAllBytes(Path.Combine(directory,frame.ToString("00000")+".png"),pixels.EncodeToPNG());
                    yield return null;
                }
                CombatEffects.CaptureSound=null;
                using(var writer=new BinaryWriter(File.Create(Path.Combine(directory,"combat.wav"))))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+mix.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(mix.Length*2);
                    foreach(float sample in mix)writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));
                }
                File.WriteAllText(Path.Combine(directory,"take.txt"),"Native 1920x1080, 30 fps, 12 seconds. Build="+fixture+"; wave 6; seed="+Seed+"; 18-second warmup; 5000 capture health; production offense/physics; synchronized native SFX.\n"+Deck.Loadout);
                Debug.Log("TRAILER_TAKE: "+fixture);
            }
            CombatEffects.CaptureSound=null;target.Release();Destroy(target);Destroy(pixels);
            Debug.Log("TRAILER_CAPTURE_SUCCESS");Application.Quit(0);
        }
    }
}
