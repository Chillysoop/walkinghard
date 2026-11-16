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
        [Serializable] sealed class FeelFrame
        {public int wave,enemies,bodies,activeBodies,frames,effectObjects;public float speed,medianMs,p95Ms;public float[] stageMs,bodyMs;}
        [Serializable] sealed class FeelReport
        {
            public string version,assembly,mode,result;public int assertions,wave,kills,orderErrors,visibleSpawns;public bool passed,unpooled,originalBodyScan,originalBodyFilters;public int verifiedWalkingCenters;
            public int startWave;public string checkpointSha256="";public float health;public List<FeelFrame> frames=new List<FeelFrame>();public List<string> checks=new List<string>();
            public string measurement="Native 1080p 60 FPS cap, Full PhysX. Automated ordinary-loot journey; samples at 1x/4x. Decisions are automatic; no human playtime estimate. Fixture shots use staged targets.";
        }
        IEnumerator ReviewCombatFeel()
        {
            enabled=false;qa=false;simulation=false;FastSimulation=false;
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();string Arg(string key,string fallback){int i=Array.IndexOf(args,key);return i<0?fallback:args[i+1];}
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Use an isolated combat feel profile");
            string output=Path.GetFullPath(Arg("-reviewOutput","CombatFeelReview"));Directory.CreateDirectory(output);
            bool render=!args.Contains("-nographics"),journey=args.Contains("-feelJourney");
            var report=new FeelReport{version=Application.version,mode=journey?"journey":"fixtures",unpooled=args.Contains("-unpooledEffects")};
            using(var hash=System.Security.Cryptography.SHA256.Create())report.assembly=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(typeof(VectorEncounter).Assembly.Location))).Replace("-","");
            report.originalBodyFilters=FighterPhysics.OriginalBodyFilters;fx.ReviewUnpooled=report.unpooled;report.originalBodyScan=reviewOriginalBodyScan=args.Contains("-originalBodyScan");FighterPhysics.VerifyWalkingSnapshot=args.Contains("-verifyWalkingSnapshot");
            void Require(bool ok,string rule){if(!ok){report.result="FAILED: "+rule;File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));throw new InvalidOperationException("COMBAT_FEEL: "+rule);}report.assertions++;report.checks.Add(rule);}
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;if(render)Screen.SetResolution(1920,1080,false);
            for(int i=0;i<12;i++)yield return null;
            IEnumerator Shot(string name)
            {if(!render)yield break;fx.Step(.001f);yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());Destroy(t);}
            IEnumerator Sample(float speed)
            {
                SetCombatSpeed(speed);for(int i=0;i<6&&State==RunState.Running;i++){AdvanceCombat(1f/60);yield return null;}
                float start=Time.realtimeSinceStartup,previous=start;var values=new List<float>();var stages=(long[])simulationProfile.Clone();var motion=(long[])bodyMotionProfile.Clone();int alive=0,bodies=0,active=0;
                while(Time.realtimeSinceStartup-start<3&&State==RunState.Running)
                {
                    yield return null;float now=Time.realtimeSinceStartup,dt=now-previous;previous=now;values.Add(dt*1000);AdvanceCombat(Mathf.Min(dt,.1f));fx.MixSound();
                    alive=Math.Max(alive,enemies.Count);bodies=Math.Max(bodies,corpses.Count);active=Math.Max(active,corpses.Count(e=>e.rig.Root.gameObject.activeSelf));
                    if(RoutePending)ChooseRoute(SimulationExpeditionRoute());
                }
                if(values.Count>10){values.Sort();float scale=1000f/System.Diagnostics.Stopwatch.Frequency/values.Count;report.frames.Add(new FeelFrame{wave=Wave,enemies=alive,bodies=bodies,activeBodies=active,frames=values.Count,speed=speed,medianMs=values[values.Count/2],p95Ms=values[(int)(values.Count*.95f)],effectObjects=fx.EffectObjects,stageMs=simulationProfile.Select((v,i)=>(v-stages[i])*scale).ToArray(),bodyMs=bodyMotionProfile.Select((v,i)=>(v-motion[i])*scale).ToArray()});}
                SetCombatSpeed(1);yield return Shot("wave-"+Wave+"-speed-"+speed);
            }
            if(journey)
            {
                Require(render,"performance run must render");forcedSeed=true;tuning.seed=int.Parse(Arg("-seed","4319"));simulationPolicy=Arg("-journeyPolicy","pinball");policyRandom=null;
                office=new OfficeProfile{seal=true,unlocks=Constellation.Nodes.Select(n=>n.item).ToList()};
                if(args.Contains("-feelResume"))
                {
                    using(var hash=System.Security.Cryptography.SHA256.Create())report.checkpointSha256=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(SavePath))).Replace("-","");
                    Require(ResumeExpedition(),"restored actual saved journey");
                    report.measurement="Native 1080p 60 FPS cap, Full PhysX. Continuation from a recorded ordinary-loot checkpoint; identical source save for both filter policies. Not a new full journey or human pacing estimate.";
                }
                else StartChapter();
                report.startWave=Wave;fx.muted=true;SetCombatSpeed(1);
                var sampled=new HashSet<int>();float wall=Time.realtimeSinceStartup;
                while(State!=RunState.Complete&&State!=RunState.Defeated)
                {
                    if(Time.realtimeSinceStartup-wall>900)throw new InvalidOperationException("Combat feel journey timeout");
                    if(State==RunState.Drafting){if(StopOpen)SimulationExpeditionStop();else SimulationDraft();}
                    else if(State==RunState.Revealing)StepFinaleReveal(RevealDuration);
                    else if(WaveElapsed>15&&new[]{1,6,7,13,14,20,21}.Contains(Wave)&&sampled.Add(Wave))
                    {yield return Sample(1);yield return Sample(4);File.WriteAllText(Path.Combine(output,"progress.json"),JsonUtility.ToJson(report,true));}
                    else for(int i=0;i<45&&State==RunState.Running;i++){AdvanceCombat(1f/60);if(RoutePending)ChooseRoute(SimulationExpeditionRoute());}
                    yield return null;
                }
                Require(OrderErrors==0&&VisibleSpawns==0,"strict order and concealed arrivals across journey");
                Require(Spawned==Kills+enemies.Count&&corpses.Count==Kills&&itemKills.Sum(p=>p.Value)+EnvironmentalKills==Kills,"persistent identities and kill attribution");
                if(State==RunState.Complete)Require(Wave==21&&ItemChoices==20&&AugmentChoices==5,"21 encounters / ordinary reward budgets");
                report.result=State.ToString();report.wave=Wave;report.kills=Kills;report.health=Health;report.orderErrors=OrderErrors;report.visibleSpawns=VisibleSpawns;yield return Shot("result");
            }
            else
            {
                Require(render,"native presentation fixture must render");BeginCombatPreview(3);fx.muted=true;
                var rng=UnityEngine.Random.state;var rootPhysics=hero.Root.GetComponentsInChildren<Collider>(true).Length;
                fx.Clear();
                for(int i=0;i<400;i++){fx.Arc(HeroPosition,HeroPosition+Vector3.up,Color.white);fx.ThreatShape(HeroPosition,Vector3.forward,3,65,VisualFactory.Orange,.25f);fx.Contact(new ImpactReaction(ImpactKind.Crush,HeroPosition,Vector3.forward),false);}
                Require(fx.EffectObjects<=208&&fx.ThreatEffects==16,"separate bounded cosmetic and threat pools under saturation");
                Require(JsonUtility.ToJson(rng)==JsonUtility.ToJson(UnityEngine.Random.state),"pooled cue creation preserves gameplay random state");
                Require(rootPhysics==hero.Root.GetComponentsInChildren<Collider>(true).Length,"presentation adds no actor colliders");
                Require(Resources.Load<Shader>("CombatThreat").isSupported,"foreground threat shader supported in player");
                fx.Step(2);Require(fx.ActiveEffects==0,"all temporary geometry expires");int pooled=fx.EffectObjects;
                for(int i=0;i<100;i++){fx.Arc(HeroPosition,HeroPosition+Vector3.up,Color.white);fx.Step(1);}
                Require(fx.EffectObjects==pooled,"subsequent contacts reuse existing objects");
                foreach(var cue in fx.Sound.Cues)
                {
                    var data=new float[cue.clip.samples];cue.clip.GetData(data,0);Require(data.All(float.IsFinite)&&data.Max(Mathf.Abs)<.86f&&data.Any(v=>Mathf.Abs(v)>.01f),"finite non-silent unclipped audio: "+cue.name);
                    WriteFeelWave(Path.Combine(output,cue.name+".wav"),data);
                }
                fx.muted=false;fx.Sound.Clear();foreach(string cue in new[]{"fist","slash","hammer","electric","gun","force","armor","block","collision"})fx.Cue(cue);
                fx.Cue("press-warning");fx.Cue("tempest-release");Require(fx.Sound.Active<=12&&fx.Sound.Played>=11,"warning and ultimate audible over busy impacts");
                int played=fx.Sound.Played;for(int i=0;i<50;i++)fx.Cue("hammer");Require(fx.Sound.Played==played&&fx.Sound.Suppressed>=50,"duplicate collision sound burst suppressed");
                fx.muted=true;Require(fx.Sound.Active==0,"mute immediately stops active voices");int captured=0;fx.Sound.Play("fist",true,(clip,gain)=>captured++);Require(captured==1&&fx.Sound.Active==0,"silent offline sound capture remains available");
                fx.muted=false;fx.Sound.Clear();fx.Cue("bell-warning");TogglePause();Require(State==RunState.Paused&&fx.Sound.Active==0,"pause silences an unfinished warning");TogglePause();fx.muted=true;
                ProbeReset();var weapon=ProbeHost(LoadoutCatalog.Find("hammer"));SynergyCue("collision","COLLISION CHAIN","Launched body -> impact damage");
                Require(PayoffAt(Deck.Basics[0])&&PayoffAt(Deck.Basics[1]),"shared 2H source lights both tiles without duplicate ownership");ResetSynergyCues();Require(!PayoffAt(Deck.Basics[0]),"source glow resets");
                ProbeReset();var gun=ProbeHost(LoadoutCatalog.Find("pistol"));var other=Deck.Acquire(LoadoutCatalog.Find("pistol"),2);Deck.Move(other,PlaceKind.Weapon,1);SynergyCue("burst","RECOIL SHOT","Actual right-hand shot");Require(PayoffAt(Deck.Basics[0])&&!PayoffAt(Deck.Basics[1]),"separate paired weapons retain actual payoff source");
                foreach(int preset in new[]{0,1,2,3,4,5})
                {
                    BeginCombatPreview(preset);fx.muted=true;Health=5000;UltimateAutomatic=false;
                    for(int i=0;i<360;i++){AdvanceCombat(1f/60);if(i%4==0)yield return null;if(i==150||i==270)yield return Shot("family-"+preset+"-"+i);}
                    Require(OrderErrors==0&&Kills>0,"native combat family "+preset);Require(fx.EffectObjects<=208&&fx.PreviewActive<=160,"bounded effects family "+preset);
                }
                foreach(string id in new[]{"singularity","tempest","cyclone","redline","harvest"})
                {
                    BeginCombatPreview(3);fx.muted=true;Health=5000;var item=Deck.Acquire(LoadoutCatalog.Find(id),3);Deck.Move(item,PlaceKind.Ultimate);Require(Deck.Ultimate==item,"fixture equips actual ultimate "+id);UltimateRemaining=0;RequestUltimate();
                    bool charge=false,release=false,recoveryShown=false;
                    for(int i=0;i<540&&(!charge||!release||!recoveryShown);i++)
                    {
                        AdvanceCombat(1f/60);yield return null;
                        if(UltimatePhase=="CHARGING"&&!charge){charge=true;yield return Shot(id+"-charge");}
                        if(UltimatePhase=="RELEASE"&&!release){release=true;yield return Shot(id+"-release");}
                        if(UltimatePhase=="RECOVERING"&&!recoveryShown){recoveryShown=true;yield return Shot(id+"-recovery");}
                    }
                    Require(charge&&release&&recoveryShown,"real ultimate stages: "+id);
                }
                foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})
                {
                    Screen.SetResolution(size.x,size.y,false);for(int i=0;i<15;i++)yield return null;BeginCombatPreview(3);ReducedMotion=true;fx.SetReducedMotion(true);Health=5000;
                    for(int i=0;i<100;i++){AdvanceCombat(1f/60);yield return null;}
                    fx.UltimateCue(HeroPosition,"singularity",6);fx.ThreatShape(HeroPosition+Vector3.forward*3,Vector3.back,4,60,VisualFactory.Orange,.5f);
                    yield return Shot("threat-over-ultimate-"+size.x);Require(fx.ThreatEffects>0,"reduced-motion threat remains in reserved pool "+size.x);
                }
                report.result="Fixtures passed";
            }
            report.verifiedWalkingCenters=FighterPhysics.VerifiedWalkingCenters;report.passed=true;File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));Debug.Log("COMBAT_FEEL_SUCCESS "+report.mode);Application.Quit(0);
        }
        static void WriteFeelWave(string path,float[] data)
        {
            using(var w=new BinaryWriter(File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+data.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(44100);w.Write(88200);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(data.Length*2);foreach(float v in data)w.Write((short)(Mathf.Clamp(v,-1,1)*32767));}
        }
    }
}
