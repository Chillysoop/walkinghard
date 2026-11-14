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
        [Serializable] sealed class RegionCase
        {
            public string preset;public bool regional;public int kills,severs,collisions,paired,ultimates,bodies,visibleSpawns;
            public float health;public int[] casts;
        }
        [Serializable] sealed class RegionPerformance
        {
            public bool regional;public float speed,medianMs,p95Ms;public int frames,enemies,bodies;
        }
        [Serializable] sealed class RegionReport
        {
            public bool passed;public int assertions,productionAssertions,worldColliders,decorationRenderers;
            public string measurement="1080p / 60 FPS cap / sequential native samples. Matching practice setup, actual body counts recorded. No full-run or general hardware performance claim.";
            public List<RegionCase> cases=new List<RegionCase>();public List<RegionPerformance> performance=new List<RegionPerformance>();
        }
        IEnumerator ReviewCinderworks()
        {
            enabled=false;fx.muted=true;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;Screen.SetResolution(1920,1080,false);
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-reviewOutput");
            string directory=at>=0?args[at+1]:Path.Combine(Application.dataPath,"..","CinderworksReview");Directory.CreateDirectory(directory);
            var report=new RegionReport();
            void Require(bool condition,string rule){if(!condition)throw new InvalidOperationException("CINDERWORKS_REVIEW: "+rule);report.assertions++;}
            IEnumerator Snapshot(string name)
            {
                yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();
                Require(texture.width==1920&&texture.height==1080,"native 1080p capture");
                File.WriteAllBytes(Path.Combine(directory,name+".png"),texture.EncodeToPNG());Destroy(texture);yield return null;
            }
            for(int i=0;i<12;i++)yield return null;
            // Capture the actual physics objects, not just matching counts.
            var before=regionalWorld.GetComponentsInChildren<Collider>().Select(c=>(c.GetInstanceID(),c.bounds.center,c.bounds.size)).ToArray();
            report.worldColliders=before.Length;
            yield return Snapshot("00-title");
            BeginCinderworksPreview();yield return null;
            var after=regionalWorld.GetComponentsInChildren<Collider>().Select(c=>(c.GetInstanceID(),c.bounds.center,c.bounds.size)).ToArray();
            Require(before.SequenceEqual(after),"original world collider identities, positions and bounds unchanged");
            Require(regionalWorld.Decoration.GetComponentsInChildren<Collider>(true).Length==0,"architecture has no colliders");
            report.decorationRenderers=regionalWorld.Decoration.GetComponentsInChildren<Renderer>().Length;
            Require(report.decorationRenderers<=300,"regional architecture batched by material");
            for(int preset=0;preset<CombatPreviewNames.Length;preset++)
            {
                foreach(bool regional in new[]{false,true})
                {
                    BeginCombatPreview(preset,regional);yield return null;
                    Require(CinderworksActive&&VisualFactory.ArmoredStyle&&VisualFactory.CinderworksStyle==regional,"comparison retains current armor and changes region only");
                    Require(regionalWorld.Visible==regional,"world and actor style agree");
                    Require(hero.Root.GetComponentsInChildren<Collider>().Length==1&&hero.Root.GetComponentsInChildren<Rigidbody>().Length==1,"hero physics unchanged");
                    Require(hero.PhysicsBones.Length==11&&enemies.All(e=>e.rig.PhysicsBones.Length==11),"original severable skeletons");
                    Require(!Deck.TwoHanded||hero.LeftWeapon==null,"two-handed group has one held weapon");
                    Require(enemies.All(e=>e.rig.Root.GetComponentsInChildren<Collider>().Length==1),"armor has no live enemy colliders");
                    if(preset==0)yield return Snapshot(regional?"01-cinderworks-formation":"01-current-formation");
                    if(State==RunState.Revealing)
                    {
                        StepFinaleReveal(RevealDuration*.5f);if(regional)yield return Snapshot("07-pursuer-turn");
                        StepFinaleReveal(RevealDuration*.3f);if(regional)yield return Snapshot("07-pursuer-reveal");StepFinaleReveal(RevealDuration);
                    }
                    bool capturedUltimate=false;
                    for(int frame=0;frame<900;frame++)
                    {
                        if(frame==210)RequestUltimate();AdvanceCombat(1f/60);
                        if(frame%60==0)yield return null;
                        if(regional&&frame>210&&UltimateCasts>0&&!capturedUltimate)
                        {capturedUltimate=true;yield return Snapshot("ultimate-"+preset);}
                        if(regional&&frame==600)yield return Snapshot("combat-"+preset);
                    }
                    Require(Kills>0&&AbilityCasts>0&&UltimateCasts>0,"real attacks and ultimate in "+CombatPreviewNames[preset]);
                    Require(OrderErrors==0&&VisibleSpawns==0,"sequence and concealed entrances");
                    Require(corpses.Count==Kills&&Spawned==Kills+enemies.Count,"body and enemy identity accounting");
                    Require(fx.PreviewActive<=160,"bounded effects");
                    report.cases.Add(new RegionCase{preset=CombatPreviewNames[preset],regional=regional,kills=Kills,severs=Severs,collisions=CollisionHits,paired=PairedCasts,ultimates=UltimateCasts,bodies=corpses.Count,visibleSpawns=VisibleSpawns,health=Health,casts=(int[])SlotCasts.Clone()});
                }
            }
            foreach(bool regional in new[]{false,true})
            {
                BeginCombatPreview(0,regional);yield return null;mechanicAssertions=0;
                // Core probes exercise ordinary route rules. Keep the chosen art, but
                // suspend the practice-only rule that deliberately disables forks.
                CombatPreviewActive=false;try{CheckCombatMechanics();}finally{CombatPreviewActive=true;}
                Require(mechanicAssertions>=755,"production effects and combat-flow checks in both looks");report.productionAssertions+=mechanicAssertions;
                foreach(float speed in new[]{.5f,1f,4f})
                {
                    BeginCombatPreview(3,regional);yield return null;fx.muted=true;ReducedMotion=speed==4;fx.SetReducedMotion(ReducedMotion);
                    for(int i=0;i<1200&&State==RunState.Running;i++)AdvanceCombat(1f/60);
                    SetCombatSpeed(speed);UltimateAutomatic=true;
                    for(int i=0;i<12;i++){yield return null;AdvanceCombat(1f/60);}
                    var values=new List<float>();float previous=Time.realtimeSinceStartup;
                    int peakEnemies=enemies.Count,peakBodies=corpses.Count;
                    for(int frame=0;frame<180&&State==RunState.Running;frame++)
                    {
                        yield return null;float now=Time.realtimeSinceStartup,dt=now-previous;previous=now;values.Add(dt*1000);
                        AdvanceCombat(Mathf.Min(dt,.1f));peakEnemies=Mathf.Max(peakEnemies,enemies.Count);peakBodies=Mathf.Max(peakBodies,corpses.Count);
                    }
                    Require(values.Count>10,"live performance sample");values.Sort();
                    report.performance.Add(new RegionPerformance{regional=regional,speed=speed,frames=values.Count,medianMs=values[values.Count/2],p95Ms=values[(int)(values.Count*.95f)],enemies=peakEnemies,bodies=peakBodies});
                    Require(OrderErrors==0&&fx.PreviewActive<=160,"speed and reduced-motion contracts");
                    if(regional&&speed==4)yield return Snapshot("08-reduced-motion-crowd");
                }
            }
            MainMenu();yield return null;
            Require(!CinderworksActive&&!VisualFactory.CinderworksStyle&&!regionalWorld.Visible,"exit restores original world");
            Require(before.SequenceEqual(regionalWorld.GetComponentsInChildren<Collider>().Select(c=>(c.GetInstanceID(),c.bounds.center,c.bounds.size))),"exit preserves physics objects");
            Begin();yield return null;Require(State==RunState.Drafting&&!CinderworksActive&&VisualFactory.ArmoredStyle==defaultArmoredStyle,"normal run remains available");
            yield return Snapshot("09-normal-run-restored");
            report.passed=true;File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));
            Debug.Log("CINDERWORKS_REVIEW_SUCCESS / "+report.assertions+" assertions");Application.Quit(0);
        }
    }
}
