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
        [Serializable] sealed class FlowSample
        {public string fixture;public float seconds,damage,waitSeconds,longestWait,firstHit=-1,minimumDistance=999,finalDistance;public int casts,orderErrors;}
        [Serializable] sealed class FlowReport
        {public List<FlowSample> cases=new List<FlowSample>();public int assertions,productionAssertions;public bool passed;public string scope="Isolated full-physics spacing fixtures; durable targets and high hero health. Not balance or win-rate estimates.";}
        IEnumerator ReviewCombatFlow()
        {
            enabled=false;qa=true;qaHolding=true;fx.muted=true;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-flowOutput");
            string directory=at>=0?args[at+1]:Path.Combine(Application.dataPath,"..","CombatFlowReview");Directory.CreateDirectory(directory);
            var report=new FlowReport();
            void Require(bool condition,string rule){if(!condition)throw new InvalidOperationException("COMBAT_FLOW_REVIEW: "+rule);report.assertions++;}
            CheckCombatMechanics();report.productionAssertions=mechanicAssertions;
            foreach(string id in new[]{"idle","fists","SteelJab","breachhammer","hammer","sword","rifle"})
            {
                Begin(true);OpeningPicks=2;Wave=8;tuning.waveCount=8;Health=10000;
                if(id!="idle"&&id!="fists")
                {
                    var item=Deck.Acquire(LoadoutCatalog.Find(id));
                    if(item.def.kind==ItemKind.Weapon)Deck.Move(item,PlaceKind.Weapon,0);
                    else {Deck.Move(Deck.Basics[0],PlaceKind.Inventory);Deck.Move(Deck.Basics[1],PlaceKind.Inventory);Deck.Move(item,PlaceKind.Sequence,0);}
                }
                BeginWave();StepFinaleReveal(RevealDuration);
                var boss=enemies.Single(e=>e.boss);foreach(var e in enemies.Where(e=>e!=boss).ToArray()){enemies.Remove(e);DisposeEnemy(e);}
                boss.health=boss.maxHealth=100000;boss.rig.Root.position=HeroPosition+Vector3.back*3.8f;boss.physics.Motor.position=boss.rig.Root.position;
                spawnTimer=100000;Physics.SyncTransforms();yield return null;
                var sample=new FlowSample{fixture="pursuer-"+id};float wait=0;
                for(int frame=0;frame<2700;frame++)
                {
                    if(id=="idle")recovery=1;
                    Step(1f/60);sample.seconds+=1f/60;
                    float distance=Vector3.Distance(HeroPosition,boss.rig.Root.position);sample.minimumDistance=Mathf.Min(sample.minimumDistance,distance);sample.finalDistance=distance;
                    if(boss.health<boss.maxHealth&&sample.firstHit<0)sample.firstHit=sample.seconds;
                    if(ActionStatus.StartsWith("WAITING FOR TARGET")){wait+=1f/60;sample.waitSeconds+=1f/60;sample.longestWait=Mathf.Max(sample.longestWait,wait);}else wait=0;
                    if(frame%180==0)yield return null;
                }
                sample.damage=boss.maxHealth-boss.health;sample.casts=AbilityCasts;sample.orderErrors=OrderErrors;report.cases.Add(sample);
                Debug.Log("FLOW_FIXTURE "+JsonUtility.ToJson(sample));
                Require(sample.orderErrors==0,"fixture preserves sequence order");
                Require(id=="idle"?sample.finalDistance<2.5f:sample.damage>0,"fixture reaches contact under full physics");
                if(id=="SteelJab")Require(sample.firstHit<3&&sample.longestWait<4,"short-reach action avoids the friction dead zone");
            }
            if(!Application.isBatchMode)
            {
                QualitySettings.vSyncCount=0;Application.targetFrameRate=60;Screen.SetResolution(1920,1080,false);
                for(int i=0;i<12;i++)yield return null;
                IEnumerator Snapshot(string name)
                {
                    yield return null;yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();
                    Require(texture.width==1920&&texture.height==1080,"native interface capture resolution");File.WriteAllBytes(Path.Combine(directory,name+".png"),texture.EncodeToPNG());Destroy(texture);
                }
                Begin(true);OpeningPicks=2;Wave=1;BeginWave();foreach(var e in enemies.ToArray()){enemies.Remove(e);DisposeEnemy(e);}spawnTimer=100000;recovery=0;
                var pistol=Deck.Acquire(LoadoutCatalog.Find("pistol"));var scatter=Deck.Acquire(LoadoutCatalog.Find("scattergun"));
                Deck.Move(pistol,PlaceKind.Weapon,0);Deck.Move(scatter,PlaceKind.Weapon,1);hero.EquipWeapons(WeaponKind.Pistol,WeaponKind.Shotgun,false);
                for(int i=0;i<30;i++)Advance();Require(ShowTargetWait&&WaitingReason==TargetWaitReason.NoEnemy,"arrival banner uses production scheduler");
                yield return Snapshot("01-awaiting-enemies");
                var target=ProbeEnemy(0,8);for(int i=0;i<30;i++)Advance();
                Require(ShowTargetWait&&WaitingSlot==1&&WaitingPairSlot==0&&TryGetWaitTarget(out _,out _),"mixed pair and visible target are labeled");
                yield return Snapshot("02-paired-reach");
                ReducedMotion=true;fx.SetReducedMotion(true);yield return Snapshot("03-reduced-motion-reach");
                target.rig.Root.position=new Vector3(0,.04f,5);target.physics.Motor.position=target.rig.Root.position;Physics.SyncTransforms();Advance();Preparation=0;StepHero(.34f);
                Require(!ShowTargetWait&&Ammo(pistol)==5&&Ammo(scatter)==2,"target entering reach resumes both guns and clears wait UI");
                yield return Snapshot("04-pair-resumed");
                Begin(true);OpeningPicks=2;BeginWave();foreach(var e in enemies.ToArray()){enemies.Remove(e);DisposeEnemy(e);}recovery=0;
                ProbeEnemy(0,2);var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.GetComponent<Renderer>().sharedMaterial=VisualFactory.Stone;wall.layer=EntryLayout.WorldLayer;wall.transform.position=new Vector3(0,1.2f,1);wall.transform.localScale=new Vector3(1,2,.1f);Physics.SyncTransforms();
                for(int i=0;i<30;i++)Advance();Require(WaitingReason==TargetWaitReason.Obstructed&&ShowTargetWait,"blocked path is distinct from range");
                yield return Snapshot("05-obstructed");DestroyImmediate(wall);
                EndTargetWait();BeginFeedbackWave();ObserveTargetWait(Deck.Fists,8,TargetWaitReason.NoEnemy);EndTargetWait();ObserveTargetWait(Deck.Fists,4);Elapsed+=20;CaptureWaveFeedback();EndTargetWait();
                State=RunState.Drafting;Deck.Open(DraftKind.Weapon,true);hud.FlowFeedbackView(true);yield return Snapshot("06-wave-feedback");hud.FlowFeedbackView(false);
                Require(LastWaveFeedback.items.Single(i=>i.id=="fists").values.ArrivalWait==8,"native recap separates arrival and reach seconds");
            }
            report.passed=true;
            File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));Application.Quit(0);
        }
    }
    public sealed partial class PrototypeHUD
    {
        internal void FlowFeedbackView(bool open){displayedWave=game.LastWaveFeedback;waveDetails=open;}
    }
}
