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
        [Serializable] sealed class RunFrameSample
        {public int wave,enemies,bodies,frames;public float speed,medianMs,p95Ms;public bool reducedMotion,stress;}
        [Serializable] sealed class NativeRunReport
        {
            public int seed,wave,kills,itemChoices,augmentChoices,assertions;public string policy,result;
            public float health;public bool passed;public List<RunFrameSample> performance=new List<RunFrameSample>();
            public string measurement="1080p wall-frame intervals, 60 FPS cap; fast-forward between real-time samples. Normal drafts/health, automatic choices and ultimates. Stress fixture is recorded separately.";
        }
        IEnumerator ReviewFullRun()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;Screen.SetResolution(1920,1080,false);
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-reviewOutput");
            string directory=at>=0?args[at+1]:Path.Combine(Application.dataPath,"..","FullRunReview");Directory.CreateDirectory(directory);
            var report=new NativeRunReport{seed=Seed,policy=CurrentPolicy};var sampled=new HashSet<int>();int draft=0;bool stress=false;
            void Require(bool condition,string rule){if(!condition)throw new InvalidOperationException("FULL_RUN_REVIEW: "+rule);report.assertions++;}
            IEnumerator Snapshot(string name)
            {
                yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();
                Require(texture.width==1920&&texture.height==1080,"1080p native UI capture");
                File.WriteAllBytes(Path.Combine(directory,name+".png"),texture.EncodeToPNG());Destroy(texture);yield return null;
            }
            IEnumerator Sample(float speed,float seconds)
            {
                SetCombatSpeed(speed);for(int i=0;i<4;i++){yield return null;AdvanceCombat(1f/60);}
                var values=new List<float>();float previous=Time.realtimeSinceStartup,end=previous+seconds;
                int peakEnemies=enemies.Count,peakBodies=corpses.Count;
                while(Time.realtimeSinceStartup<end&&State==RunState.Running)
                {
                    yield return null;float now=Time.realtimeSinceStartup;float dt=now-previous;previous=now;
                    values.Add(dt*1000);AdvanceCombat(Mathf.Min(dt,.1f));peakEnemies=Mathf.Max(peakEnemies,enemies.Count);peakBodies=Mathf.Max(peakBodies,corpses.Count);
                }
                if(values.Count>10){values.Sort();report.performance.Add(new RunFrameSample{wave=Wave,enemies=peakEnemies,bodies=peakBodies,frames=values.Count,speed=speed,medianMs=values[values.Count/2],p95Ms=values[(int)(values.Count*.95f)],reducedMotion=ReducedMotion,stress=stress});}
                SetCombatSpeed(1);
            }
            for(int i=0;i<12;i++)yield return null;
            Require(VisualFactory.ArmoredStyle,"normal run uses the new presentation");
            float wallStart=Time.realtimeSinceStartup;
            while(State!=RunState.Complete&&State!=RunState.Defeated)
            {
                if(Time.realtimeSinceStartup-wallStart>=240)throw new InvalidOperationException("Native review exceeded 240 seconds.");
                if(State==RunState.Drafting)
                {
                    yield return null;yield return Snapshot("draft-"+(draft++).ToString("00")+"-wave-"+Wave);
                    if(!RoutePending&&Wave>=1){hud.FullRunBuildView(true);yield return null;yield return Snapshot("gear-"+draft.ToString("00"));hud.FullRunBuildView(false);}
                    SimulationDraft();yield return null;continue;
                }
                if(State==RunState.Revealing)
                {StepFinaleReveal(Time.unscaledDeltaTime);yield return null;continue;}
                if(State==RunState.Running)
                {
                    if(WaveElapsed>=16&&sampled.Add(Wave))
                    {
                        yield return Sample(1,2);yield return Snapshot("combat-wave-"+Wave);
                        if(Wave==6){yield return Sample(.5f,2);yield return Sample(4,2);}
                    }
                    else {for(int i=0;i<10&&State==RunState.Running;i++)AdvanceCombat(1f/60);yield return null;}
                }
            }
            yield return Snapshot("run-result");
            Require(OrderErrors==0&&VisibleSpawns==0,"sequence and concealed entries");
            Require(Spawned==Kills+enemies.Count&&corpses.Count==Kills,"persistent enemy identities");
            if(State==RunState.Complete)Require(ItemChoices==13&&AugmentChoices==2&&bossSpawned,"normal eight-wave reward and finale gates");
            report.result=State.ToString();report.wave=Wave;report.kills=Kills;report.health=Health;report.itemChoices=ItemChoices;report.augmentChoices=AugmentChoices;
            // Separate worst-crowd presentation fixture: retain real physics and default health,
            // stage 42 durable targets and retain the fixture's corpses. It is not a balance result.
            stress=true;simulation=false;BeginCombatPreview(3,true);UltimateAutomatic=true;
            for(int i=0;i<1200&&State==RunState.Running;i++)AdvanceCombat(1f/60);
            for(int i=0;enemies.Count<tuning.maximumEnemies;i++)
            {
                var e=ProbeEnemy(-5+(i%8)*1.4f,HeroPosition.z+5+(i/8)*1.5f);e.radius=2.7f;e.cooldown=1;e.orbit=i%2==0?1:-1;Spawned++;
            }
            Physics.SyncTransforms();yield return Snapshot("stress-crowd");
            yield return Sample(1,3);ReducedMotion=true;fx.SetReducedMotion(true);yield return Sample(4,2);
            Require(fx.PreviewActive<=160&&OrderErrors==0,"bounded effects under crowded speed changes");
            report.passed=true;File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));
            Debug.Log("FULL_RUN_REVIEW_SUCCESS / "+report.result+" / "+report.assertions+" assertions");Application.Quit(0);
        }
    }
    public sealed partial class PrototypeHUD
    {
        public void FullRunBuildView(bool open)=>SetManageBuild(open);
    }
}
