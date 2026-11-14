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
        [Serializable] sealed class ForemanCase
        {
            public string build,encounter,result,loadout;public int seed,kills,orderErrors,visibleSpawns,vents,forcedVents,slams,repairs,throws,retrievals;
            public bool crews,valid;public float health,initialHealth,damageTaken,healing,elapsed,reachWait,bracedDamage,ventDamage;
            public int[] slots;public int paired,ultimates;
        }
        [Serializable] sealed class ForemanReport
        {
            public bool passed;public int assertions,productionAssertions;public string scope;
            public List<string> checks=new List<string>();public List<ForemanCase> cases=new List<ForemanCase>();
        }
        static readonly string[] ForemanBuilds={"fists","swords","greatsword","hammer","rifle","pistols","storm","sustain"};
        void EquipForemanKit(string name)
        {
            string right="fists",left="fists",setup="GravityWell",middle="Breaker",last="Counterguard",helmet="surveyor",armor="plate",boots="treads",rig="amplifier",ultimate="singularity";
            switch(name)
            {
                case "swords":right=left="sword";middle="ReapingCut";armor="duelist";ultimate="cyclone";break;
                case "greatsword":right="greatsword";left="";middle="ReapingCut";armor="duelist";ultimate="cyclone";break;
                case "hammer":right="hammer";left="";setup="ConductiveBrand";last="MassDriver";armor="grounding";rig="relay";boots="momentum";ultimate="tempest";break;
                case "rifle":right="rifle";left="";setup="VectorTether";middle="SuppressiveVolley";armor="recoil";boots="loader";ultimate="redline";break;
                case "pistols":right=left="pistol";setup="VectorTether";middle="SuppressiveVolley";armor="recoil";boots="loader";ultimate="redline";break;
                case "storm":right=left="wraps";setup="ConductiveBrand";middle="ArcBolt";last="GravityWell";helmet="capacitor";armor="grounding";rig="relay";ultimate="tempest";break;
                case "sustain":right=left="siphonblade";setup="DrainBrand";middle="ReapingCut";armor="reclaimer";boots="predator";ultimate="harvest";break;
            }
            LoadoutItem Equip(string id,PlaceKind place,int index=0,int rank=2)
            {var item=Deck.Acquire(LoadoutCatalog.Find(id),rank);if(!MoveItem(item,place,index))throw new InvalidOperationException("Foreman kit placement: "+id);return item;}
            Equip(right,PlaceKind.Weapon,0,3);if(left!="")Equip(left,PlaceKind.Weapon,1,3);
            Equip(setup,PlaceKind.Sequence,2);Equip(middle,PlaceKind.Sequence,3);Equip(last,PlaceKind.Sequence,4);
            Equip(helmet,PlaceKind.Helmet);Equip(armor,PlaceKind.Armor);Equip(boots,PlaceKind.Boots);Equip(rig,PlaceKind.Rig);Equip(ultimate,PlaceKind.Ultimate);
            Deck.Move(Deck.Slots[2],PlaceKind.Sequence,0);hero.Equipment(Deck.Has(AbilityId.MassDriver),Deck.Pinball);
            OpeningPicks=2;Health=MaxHealth;UltimateRemaining=UltimateCooldown;UltimateAutomatic=true;
        }
        IEnumerator ReviewForeman()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();bool balance=Array.IndexOf(args,"-foremanBalance")>=0,render=Array.IndexOf(args,"-nographics")<0;
            string Arg(string key,string fallback){int at=Array.IndexOf(args,key);return at<0?fallback:args[at+1];}
            string directory=Path.GetFullPath(Arg("-reviewOutput",Path.Combine(Application.dataPath,"..","ForemanReview")));Directory.CreateDirectory(directory);
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Foreman review requires an isolated save directory");
            var report=new ForemanReport{scope=balance?"Matched Full-physics single-encounter fixtures. Rare weapons, Uncommon supports and ultimates, normal starting health, automatic sequence and ultimates, no augments. Fixed builds are not ordinary draft win rates.":"Controlled rendered boss mechanics and reach fixtures. Elevated fixture health is not balance evidence."};
            void Require(bool condition,string rule){if(!condition)throw new InvalidOperationException("FOREMAN_REVIEW: "+rule);report.assertions++;report.checks.Add(rule);}
            void Write()=>File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));
            IEnumerator Shot(string name){if(!render)yield break;fx.Step(1f/60);yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(directory,name+".png"),image.EncodeToPNG());Destroy(image);yield return null;}
            void Advance(int frames){for(int i=0;i<frames&&State==RunState.Running;i++)AdvanceCombat(1f/60);}
            void BossFrames(Enemy boss,int frames){for(int i=0;i<frames;i++){Elapsed+=1f/60;WaveElapsed+=1f/60;StepForeman(boss,1f/60);Physics.SyncTransforms();Physics.Simulate(1f/60);fx.Step(1f/60);}}
            QualitySettings.vSyncCount=0;Application.targetFrameRate=render?60:-1;if(render)Screen.SetResolution(1920,1080,false);
            for(int i=0;i<8;i++)yield return null;
            if(balance)
            {
                loadingCheckpoint=true;
                int first=int.Parse(Arg("-balanceFirstSeed","91201")),count=int.Parse(Arg("-balanceSeedCount","2"));
                for(int seed=first;seed<first+count;seed++)foreach(string build in ForemanBuilds)foreach(bool boss in new[]{false,true})foreach(bool crews in new[]{false,true})
                {
                    forcedSeed=true;tuning.seed=seed;crewBaseline=!crews;ExpeditionActive=true;Begin();EquipForemanKit(build);Wave=boss?6:7;BeginWave();float initial=Health;var actor=Foreman;float started=Time.realtimeSinceStartup;
                    Require(Seed==seed,"matched seed applied / "+seed);
                    while(State==RunState.Running&&WaveElapsed<120)
                    {Advance(600);yield return null;if(Time.realtimeSinceStartup-started>90)throw new InvalidOperationException("Balance case wall timeout");}
                    var f=actor?.foreman;
                    var c=new ForemanCase{build=build,encounter=boss?"foreman":"mixed-crews",seed=seed,crews=crews,result=State==RunState.Drafting?"Complete":State==RunState.Defeated?"Defeated":"TimedOut",initialHealth=initial,health=Health,damageTaken=initial+ArmorHealing-Health,healing=ArmorHealing,elapsed=WaveElapsed,kills=Kills,vents=f?.vents??0,forcedVents=f?.forcedVents??0,slams=f?.slams??0,bracedDamage=f?.bracedDamage??0,ventDamage=f?.ventDamage??0,repairs=CrewUses(CrewRole.Repair),throws=CrewUses(CrewRole.Freight),retrievals=CrewUses(CrewRole.Reclaimer),paired=PairedCasts,ultimates=UltimateCasts,slots=(int[])SlotCasts.Clone(),orderErrors=OrderErrors,visibleSpawns=VisibleSpawns,loadout=Deck.Loadout,reachWait=contributions.Values.Sum(v=>v.outOfReachWait)};
                    c.valid=OrderErrors==0&&VisibleSpawns==0&&Spawned==Kills+enemies.Count&&corpses.Count==Kills&&(!boss||State!=RunState.Drafting||actor.deathAge>=0);
                    Require(c.valid,"identities and sequencing / "+build+" / "+seed+" / "+c.encounter+" / "+crews);report.cases.Add(c);Write();Debug.Log("FOREMAN_CASE "+report.cases.Count+" / "+build+" / "+c.encounter+" / "+crews+" / "+c.result);yield return null;
                }
                crewBaseline=false;loadingCheckpoint=false;
            }
            else
            {
                CheckCombatMechanics();report.productionAssertions=mechanicAssertions;tuning.heroHealth=10000;BeginExpedition();OpeningPicks=2;Wave=6;BeginWave();spawnTimer=10000;
                var boss=Foreman;Require(boss!=null&&VisibleSpawns==0&&boss.rig.PhysicsBones.Length==11,"Foreman enters concealed on the original skeleton");yield return Shot("01-approach");
                boss.physics.Motor.position=HeroPosition+RoadForward*2.25f;boss.foreman.stage=1;boss.health=boss.maxHealth=10000;Physics.SyncTransforms();BossFrames(boss,8);
                Require(boss.rig.Root.GetComponentsInChildren<Collider>().Count(c=>c.enabled)==1,"Foreman art adds no active colliders");yield return Shot("02-braced");
                currentCredit=new KillCredit{item=Deck.Basics[0],action=++actionSerial};float health=boss.health;Hurt(boss,100,Vector3.zero,0);Require(Mathf.Abs(health-boss.health-65)<.01f,"ordinary damage penetrates bracing at sixty-five percent");
                boss.attack=.5f;boss.foreman.pressure=.1f;currentCredit.action=++actionSerial;Control(boss,.8f,Vector3.zero);float pressure=boss.foreman.pressure;
                Control(boss,.8f,Vector3.zero);Require(boss.attack>=0&&boss.foreman.pressure==pressure&&pressure>.1f,"paired or repeated control shares pressure budget and bracing resists attack cancellation");
                Launch(boss);Require(Mathf.Abs(boss.foreman.pressure-.38f)<.001f,"body launch upgrades the same action to one bounded pressure contribution");flights.Clear();
                int delayedAction=currentCredit.action;currentCredit.action=++actionSerial;Control(boss,.5f,Vector3.zero);pressure=boss.foreman.pressure;
                currentCredit.action=delayedAction;AddForemanPressure(boss,.28f);Require(boss.foreman.pressure==pressure,"delayed contacts retain their pressure budget across intervening actions");
                boss.foreman.pressure=.9f;currentCredit.action=++actionSerial;Hurt(boss,10,Vector3.zero,1);
                Require(boss.foreman.stage==2&&boss.attack<0&&boss.foreman.forcedVents==1,"pressure break opens vent and cancels slam");yield return Shot("03-vent-open");
                boss.stun=0;health=boss.health;Hurt(boss,100,Vector3.zero,0);Require(Mathf.Abs(health-boss.health-135)<.01f,"vent exposes full damage with thirty-five percent bonus");
                float remaining=boss.foreman.ventLeft;TogglePause();AdvanceCombat(.1f);Require(boss.foreman.ventLeft==remaining&&State==RunState.Paused,"pause freezes pressure and vent timers");TogglePause();BossFrames(boss,250);
                Require(boss.foreman.stage==1,"four-second vent returns to bracing");
                boss.foreman.pressure=0;int vents=boss.foreman.vents;BossFrames(boss,550);Require(boss.foreman.vents==vents+1,"ordinary builds get a natural vent without control hits");
                boss.foreman.stage=1;boss.foreman.pressure=.2f;boss.cooldown=0;boss.attack=-1;BossFrames(boss,35);yield return Shot("04-slam-warning");
                var wall=EntryLayout.Solid("Boss review wall",transform,HeroPosition+RoadForward,new Vector3(8,5,.3f),VisualFactory.Stone);wall.rotation=RoadRotation;Physics.SyncTransforms();health=Health;BossFrames(boss,130);Require(Health==health&&boss.attack<0,"Foreman cannot attack through a wall");wall.gameObject.SetActive(false);Destroy(wall.gameObject);
                Deck.Open(DraftKind.Mixed,false);State=RunState.Drafting;boss.foreman.stage=2;boss.foreman.ventLeft=2.2f;
                Require(SaveCheckpoint(),"Foreman checkpoint saves");var graph=JsonUtility.ToJson(CaptureCheckpoint().graph);File.WriteAllText(Path.Combine(directory,"checkpoint-before.json"),graph);
                Require(ResumeExpedition(),"Foreman checkpoint loads");var restoredGraph=JsonUtility.ToJson(CaptureCheckpoint().graph);File.WriteAllText(Path.Combine(directory,"checkpoint-after.json"),restoredGraph);
                Require(graph==restoredGraph,"Foreman phase anchor pressure and credits restore exactly");
                State=RunState.Running;Screen.SetResolution(1280,720,false);ReducedMotion=true;fx.SetReducedMotion(true);for(int i=0;i<8;i++)yield return null;yield return Shot("05-vent-720p");
                foreach(string build in new[]{"fists","swords","greatsword","hammer"})
                {
                    BeginExpedition();EquipForemanKit(build);Wave=6;BeginWave();spawnTimer=10000;boss=Foreman;boss.physics.Motor.position=HeroPosition+RoadForward*2.3f;boss.foreman.stage=1;boss.health=boss.maxHealth=10000;Physics.SyncTransforms();var anchor=HeroPosition;
                    Advance(1800);Require((HeroPosition-anchor).magnitude<.05f&&WeaponHits.Sum()>4&&OrderErrors==0,"stationary melee contact and sequence / "+build);
                    Require(contributions.Values.Sum(v=>v.outOfReachWait)<4,"short melee actions retain contact / "+build);yield return null;
                }
                BeginExpedition();EquipForemanKit("hammer");Wave=6;BeginWave();boss=Foreman;boss.health=1;currentCredit=new KillCredit{item=Deck.Basics[0],action=++actionSerial};Hurt(boss,10,Vector3.zero,0);Advance(1);
                Require(State==RunState.Drafting&&Wave==6&&corpses.Count(e=>e.foreman!=null)==1,"Foreman defeat opens existing wave-six rewards once");
                while(State==RunState.Drafting)SimulationDraft();Require(Wave==7&&!ForemanPresent,"ordinary seventh wave follows the regional boss");
                MainMenu();BeginCinderworksPreview();BeginCombatPreview(7,true);Require(ForemanPractice&&ForemanPresent&&!ExpeditionActive,"Foreman practice is directly available");MainMenu();Require(!ForemanPresent&&!ForemanPractice,"leaving practice clears the boss");
            }
            report.passed=true;Write();Debug.Log("FOREMAN_REVIEW_SUCCESS / "+report.assertions+" assertions / "+report.cases.Count+" balance cases");Application.Quit(0);
        }
    }
}
