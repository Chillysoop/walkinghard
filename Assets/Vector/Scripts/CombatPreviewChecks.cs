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
        [Serializable] sealed class PreviewResult
        {
            public string preset;public bool newStyle;public bool exactBaselineMatch;public bool repeatedOriginal;public int kills,severs,launches,collisions,ultimates,paired,peakEffects;
            public float health;public int[] casts;
        }
        [Serializable] sealed class PreviewReport
        {
            public bool passed;public int assertions,productionAssertions;public List<PreviewResult> cases=new List<PreviewResult>();
            public float medianFrameMs,p95FrameMs;public string performance="1080p, 1x, new look, normal wave; wall-frame intervals including presentation";
        }
        IEnumerator CheckCombatPreview()
        {
            enabled=false;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;Screen.SetResolution(1920,1080,false);
            var args=Environment.GetCommandLineArgs();int output=Array.IndexOf(args,"-previewOutput");
            string directory=output>=0?args[output+1]:Path.Combine(Application.dataPath,"..","CombatPreviewQA");Directory.CreateDirectory(directory);
            var report=new PreviewReport();
            void Require(bool condition,string rule){if(!condition)throw new InvalidOperationException("Combat preview: "+rule);report.assertions++;}
            for(int i=0;i<12;i++)yield return null;
            var endFrame=new WaitForEndOfFrame();
            for(int preset=0;preset<CombatPreviewNames.Length;preset++)
            {
                PreviewResult baseline=null;
                for(int style=0;style<3;style++)
                {
                    BeginCombatPreview(preset,style==1);fx.muted=true;
                    yield return null; // Flush deferred destruction before checking the live hierarchy.
                    var colliders=hero.Root.GetComponentsInChildren<Collider>(true).Length;
                    var bodies=hero.Root.GetComponentsInChildren<Rigidbody>(true).Length;
                    Require(colliders==1&&bodies==1,"armor must not add live hero physics");
                    Require(hero.PhysicsBones.Length==11,"eleven original severable bones");
                    Require(hero.Root.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&r.name=="Tapered cuirass")== (style==1),"style selects the armor surface");
                    Require(Deck.TwoHanded?hero.LeftWeapon==null:true,"two-handed weapons never create a second held weapon");
                    if(State==RunState.Revealing)StepFinaleReveal(RevealDuration);
                    int peakEffects=0;
                    for(int frame=0;frame<900;frame++)
                    {
                        if(frame==210)RequestUltimate();
                        AdvanceCombat(1f/60);peakEffects=Mathf.Max(peakEffects,fx.PreviewActive);
                        if(frame%60==0)yield return null;
                        if(frame==300||frame==600)
                        {
                            yield return endFrame;var texture=ScreenCapture.CaptureScreenshotAsTexture();
                            Require(texture.width==1920&&texture.height==1080,"native screenshot includes full 1080p UI");
                            File.WriteAllBytes(Path.Combine(directory,preset+"-"+style+"-"+frame+".png"),texture.EncodeToPNG());Destroy(texture);yield return null;
                        }
                    }
                    var result=new PreviewResult{preset=CombatPreviewNames[preset],newStyle=style==1,repeatedOriginal=style==2,kills=Kills,severs=Severs,launches=Launches,collisions=CollisionHits,ultimates=UltimateCasts,paired=PairedCasts,peakEffects=peakEffects,health=Health,casts=(int[])SlotCasts.Clone()};
                    report.cases.Add(result);
                    Require(Kills>0&&AbilityCasts>0&&UltimateCasts>0,"preset performs real combat and ultimate");
                    Require(OrderErrors==0,"preview preserves action order");Require(peakEffects<=160,"bounded particle pool");
                    if(style==0)baseline=result;
                    else
                    {
                        if(style==1)Require(peakEffects>0,"preview particles rendered during combat");
                        result.exactBaselineMatch=baseline.kills==result.kills&&baseline.severs==result.severs&&baseline.launches==result.launches&&baseline.collisions==result.collisions&&baseline.ultimates==result.ultimates&&baseline.paired==result.paired&&Mathf.Abs(baseline.health-result.health)<.001f&&baseline.casts.SequenceEqual(result.casts);
                        Debug.Log("PREVIEW_COMPARISON: "+JsonUtility.ToJson(baseline)+" / "+JsonUtility.ToJson(result));
                    }
                }
            }
            // Both looks execute the same authored production-effect checks. PhysX crowd
            // rollouts are recorded above alongside repeated-original controls, not treated as bit-identical.
            foreach(bool style in new[]{false,true})
            {
                BeginCombatPreview(0,style);mechanicAssertions=0;CheckCombatMechanics();
                Require(mechanicAssertions>=706,"production combat checks with style "+style);report.productionAssertions+=mechanicAssertions;
                yield return null;
            }
            // Real-time sample excludes screenshot encoding, startup and rig creation.
            BeginCombatPreview(3,true);fx.muted=true;UltimateAutomatic=true;
            for(int frame=0;frame<600;frame++)AdvanceCombat(1f/60);
            var samples=new List<float>();float previous=Time.realtimeSinceStartup;
            for(int i=0;i<300;i++)
            {
                yield return null;float now=Time.realtimeSinceStartup;
                if(i>5)samples.Add((now-previous)*1000);previous=now;AdvanceCombat(1f/60);
            }
            samples.Sort();report.medianFrameMs=samples[samples.Count/2];report.p95FrameMs=samples[(int)(samples.Count*.95f)];
            foreach(float speed in new[]{.5f,1,4})
            {
                BeginCombatPreview(1,true);SetCombatSpeed(speed);ReducedMotion=true;fx.PreviewReducedMotion(true);RequestUltimate();
                for(int i=0;i<480;i++){AdvanceCombat(1f/60);if(i%60==0)yield return null;}
                Require(OrderErrors==0&&Kills>0&&fx.PreviewActive<=160,"reduced motion and speed "+speed);
            }
            MainMenu();yield return null;
            Require(!CombatPreviewActive&&!VisualFactory.PreviewStyle&&State==RunState.Ready,"exit restores ordinary main menu");
            Begin();Require(State==RunState.Drafting&&OpeningPicks==0,"ordinary run still opens its draft");
            Require(!hero.Root.GetComponentsInChildren<Renderer>().Any(r=>r.name=="Tapered cuirass"),"ordinary runs retain original presentation");
            report.passed=true;File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));
            Debug.Log("COMBAT_PREVIEW_SUCCESS / "+report.assertions+" assertions");Application.Quit(0);
        }
    }
}
