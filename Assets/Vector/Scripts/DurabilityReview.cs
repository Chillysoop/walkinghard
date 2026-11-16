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
        [Serializable] sealed class DurabilityReport {public bool passed;public int assertions;public List<string> checks=new List<string>();}
        [Serializable] sealed class DurabilitySample
        {
            public string variant,build,route,result;public int seed,district,rank,spawned,kills,bodies,carried,champions,championKills,orderErrors,visibleSpawns;
            public float seconds,health,maxHealth,healing,damage;public List<DurabilityRoleSample> roles=new List<DurabilityRoleSample>();
        }
        [Serializable] sealed class DurabilityRoleSample
        {
            public string role;public int spawned,hit,firstActionKills,killedAfterHit,actionsOnKilled,livingActions,specialistStarts,specialistCompleted,interrupts,carried;
        }
        Enemy DurabilityProbe(CrewRole role,bool heavy=false,bool champion=false,float x=0,float z=3)
        {
            heavy|=role==CrewRole.Freight;var e=new Enemy{id=++nextId,crew=role,heavy=heavy,health=tuning.enemyHealth*EnemyRoleHealth(heavy,role,champion)*EnemyHealthScale,radius=3,cooldown=2};e.maxHealth=e.health;
            e.rig=new FighterRig("Durability target",actors,false,e.heavy,e.id^Seed);e.rig.Root.position=new Vector3(x,.04f,z);e.rig.Root.rotation=Quaternion.Euler(0,180,0);e.rig.CrewEquipment(role);InitializeDurability(e,champion);
            e.physics=new FighterPhysics(e.rig,e.id,this,false,e.heavy);enemies.Add(e);byId[e.id]=e;Physics.SyncTransforms();return e;
        }
        void DurabilityBuild(string build,int rank)
        {
            LoadoutItem Equip(string id,PlaceKind place,int slot=0){var def=LoadoutCatalog.Find(id);if(def==null)throw new InvalidOperationException("Unknown fixture item "+id);var item=Deck.Acquire(def,rank);if(!Deck.Move(item,place,slot))throw new InvalidOperationException("Fixture equip "+id);return item;}
            string[] abilities;string first,second=null,armor="plate",boots="treads",rig="faultline";
            switch(build)
            {
                case "storm":first="wraps";second="breachhammer";abilities=new[]{"ConductiveBrand","ArcBolt","GravityWell"};armor="grounding";rig="relay";break;
                case "sever":first="greatsword";abilities=new[]{"ReapingCut","GravityWell","MassDriver"};armor="duelist";rig="magnet";break;
                case "gun":first="rifle";abilities=new[]{"VectorTether","SuppressiveVolley","Counterguard"};armor="recoil";boots="loader";rig="overclock";break;
                case "cutcarry":first="sword";second="breachhammer";abilities=new[]{"ReapingCut","Breaker","MassDriver"};armor="duelist";rig="magnet";break;
                case "close":first="fists";second="scattergun";abilities=new[]{"Breaker","GravityWell","Counterguard"};armor="reactive";rig="amplifier";break;
                case "siphon":first="siphonblade";second="sword";abilities=new[]{"ReapingCut","DrainBrand","Counterguard"};armor="reclaimer";boots="predator";rig="overclock";break;
                default:first="hammer";abilities=new[]{"Breaker","MassDriver","GravityWell"};break;
            }
            if(first=="fists")Deck.Fists.rank=rank;else Equip(first,PlaceKind.Weapon);
            if(second!=null)Equip(second,PlaceKind.Weapon,1);
            for(int i=0;i<abilities.Length;i++)Equip(abilities[i],PlaceKind.Sequence,i+2);
            Equip(armor,PlaceKind.Armor);Equip(boots,PlaceKind.Boots);Equip(rig,PlaceKind.Rig);
            if(rank>1){Deck.Open(DraftKind.Ultimate,false);Equip(build=="siphon"?"harvest":build=="gun"?"redline":build=="sever"?"cyclone":"singularity",PlaceKind.Ultimate);UltimateRemaining=UltimateCooldown;}
        }
        IEnumerator ReviewDurability()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();bool render=!args.Contains("-nographics"),batch=args.Contains("-durabilityBatch");
            string Arg(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0?args[i+1]:fallback;}
            if(!IsolatedProfile)throw new InvalidOperationException("Durability review requires -saveDirectory");
            string output=Path.GetFullPath(Arg("-reviewOutput",Path.Combine(Application.dataPath,"..","DurabilityReview")));Directory.CreateDirectory(output);
            var report=new DurabilityReport();
            void Require(bool okay,string rule){if(!okay)throw new InvalidOperationException("DURABILITY_REVIEW: "+rule);report.assertions++;report.checks.Add(rule);}
            IEnumerator Shot(string name){if(!render)yield break;fx.Step(.01f);yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),texture.EncodeToPNG());Destroy(texture);}
            QualitySettings.vSyncCount=0;Application.targetFrameRate=render?60:-1;
            if(render)Screen.SetResolution(1920,1080,false);for(int i=0;i<5;i++)yield return null;
            forcedSeed=true;tuning.seed=1741;StartChapter();ProbeReset();Deck.Fused+=MergeKillCounts;State=RunState.Running;OpeningPicks=2;
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
            foreach(int introWave in new[]{1,2}){Wave=introWave;Require(EnemyRoleHealth(true,CrewRole.None)==1.9f&&EnemyRoleHealth(false,CrewRole.Linebreaker)==1,"opening health preserved until the first weapon and third ability / fight "+introWave);}
            foreach(int wave in new[]{3,10,17})
            {
                Wave=wave;float ordinary=tuning.enemyHealth*EnemyHealthScale;
                foreach(var role in new[]{CrewRole.Linebreaker,CrewRole.Freight,CrewRole.Repair,CrewRole.Bellkeeper,CrewRole.Cinderlobber,CrewRole.None})
                {var target=DurabilityProbe(role);float expected=role==CrewRole.Linebreaker?2:role==CrewRole.Freight?3:role==CrewRole.Repair||role==CrewRole.Bellkeeper?1.5f:1;Require(Mathf.Abs(target.health-ordinary*expected)<.01f,"role health / district "+(District+1)+" / "+role);}
            }
            ProbeReset();Wave=3;State=RunState.Running;var hammer=ProbeHost(LoadoutCatalog.Find("hammer"));var heavy=DurabilityProbe(CrewRole.None,true);
            Hurt(heavy,48,Vector3.zero,.2f);Require(heavy.deathAge<0&&heavy.health<heavy.maxHealth,"ordinary heavy survives a common hammer hit with real damage");
            Require(ThreatReadouts().Any(v=>v.title=="ENFORCER"&&v.damaged&&v.ghost>v.health),"damaged heavy exposes health and recent-hit trail");
            var protectedHeavy=DurabilityProbe(CrewRole.None,true,x:2);protectedHeavy.crewGuard=18;protectedHeavy.crewGuardUntil=Elapsed+10;Hurt(protectedHeavy,18,Vector3.zero,0);
            Require(protectedHeavy.health==protectedHeavy.maxHealth&&ThreatReadouts().Any(v=>v.state=="PATCH BROKEN"),"fully absorbed hit still visibly breaks repair protection");
            currentCredit.action=++actionSerial;Hurt(heavy,300,Vector3.zero,.2f);Require(heavy.deathAge>=0,"strong damage still kills without a hit-count gate");
            ProbeReset();Wave=3;State=RunState.Running;var targetChamp=DurabilityProbe(CrewRole.Freight,true,true);var payload=ProbeEnemy(1,3,1);KillEnemy(payload,Vector3.zero,false);
            Require(StepCrew(targetChamp,.01f)&&targetChamp.crewClock>0,"champion begins production Freight wind-up");
            float beforeClock=targetChamp.crewClock;Hurt(targetChamp,5,Vector3.zero,.1f);StepCrew(targetChamp,.05f);
            Require(targetChamp.crewClock>beforeClock&&targetChamp.stun==0&&targetChamp.durability.pressure>0,"light damage builds pressure without cancelling champion wind-up");
            StepCrew(targetChamp,1.3f);Require(targetChamp.crewUses==1&&payload.freightLeft>0,"braced champion completes a real throw after light damage");payload.freightLeft=0;targetChamp.crewCooldown=0;StepCrew(targetChamp,.01f);
            float pressure=targetChamp.durability.pressure;Control(targetChamp,.1f,Vector3.zero);Require(targetChamp.durability.pressure==pressure,"paired/derived contacts share one pressure contribution");
            currentCredit.action=++actionSerial;Control(targetChamp,1.4f,Vector3.zero);Require(targetChamp.durability.pressure>pressure,"later focused control adds pressure");
            Launch(targetChamp);Require(!ChampionBraced(targetChamp)&&targetChamp.crewClock==0&&flights.Any(f=>f.body==targetChamp)&&targetChamp.stun>0,"launch opens champion, cancels wind-up and retains physical flight");
            float hp=targetChamp.health;Hurt(targetChamp,8,Vector3.zero,0);Require(Mathf.Abs(hp-targetChamp.health-10)<.001f,"exposed champion takes 25 percent more damage");
            Elapsed+=3.1f;StepChampion(targetChamp,.01f);Require(ChampionBraced(targetChamp)&&targetChamp.durability.pressure==0,"champion rebraces with a fresh pressure budget");
            StepChampion(targetChamp,8.1f);Require(!ChampionBraced(targetChamp),"champion opens naturally after eight visible seconds");
            ProbeReset();var specialist=DurabilityProbe(CrewRole.Linebreaker,z:6);StepCrew(specialist,.01f);Control(specialist,.1f,Vector3.zero);Require(specialist.crewClock==0&&specialist.durability.interrupts==1,"ordinary specialists retain light-stagger interrupts");
            ProbeReset();Deck.Fused+=MergeKillCounts;var pistol=Deck.Acquire(LoadoutCatalog.Find("pistol"));var pistol2=Deck.Acquire(LoadoutCatalog.Find("pistol"));Deck.Move(pistol,PlaceKind.Weapon,0);Deck.Move(pistol2,PlaceKind.Weapon,1);
            ContributionFor(pistol).direct=120;ContributionFor(pistol).collision=30;ContributionFor(pistol).assists=2;ContributionFor(pistol2).healing=4;itemKills[pistol]=3;itemKills[pistol2]=1;Kills=4;
            Require(LiveSequenceStats().Count==2&&LiveSequenceStats()[0].damage==150&&LiveSequenceStats()[1].healing==4,"paired weapons have independent real damage and healing");
            Deck.Swap(0,1);Require(LiveSequenceStats()[0].item==pistol2&&LiveSequenceStats()[1].kills==3,"stats follow sequence reordering and item identity");
            var third=Deck.Acquire(LoadoutCatalog.Find("pistol"));Require(LiveTotals().damage==150&&LiveTotals().assists==2&&LiveTotals().healing==4,"fusion retains totals once, excluding consumed historical copies");
            var twoHand=Deck.Acquire(LoadoutCatalog.Find("hammer"));Deck.Move(twoHand,PlaceKind.Weapon);ContributionFor(twoHand).direct=35;
            Require(LiveSequenceStats().Count==1&&LiveSequenceStats()[0].positions.Contains("+")&&LiveTotals().damage==185&&LiveOtherStats(LiveSequenceStats()).damage==150,"two-handed group appears once; stored damage remains in gear/history");
            ProbeReset();Require(LiveSequenceStats().Count==1&&LiveSequenceStats()[0].positions.Contains("+"),"shared starter fists are not double-counted");
            State=RunState.Running;float sw=1600,sh=900;var panel=hud.LiveStatsRect(sw,sh);bool wasOpen=hud.liveStatsOpen;
            Require(hud.HandleStatsPointer(EventType.MouseDown,panel.position+new Vector2(20,12),sw,sh)&&hud.liveStatsOpen!=wasOpen,"production mouse handler toggles live sidebar");
            if(!hud.liveStatsOpen)hud.ToggleLiveStats();float density=Mathf.Max(1,.9f/(Screen.width/sw));var drawn=new Rect(panel.x/density,panel.y/density,panel.width/density,panel.height/density);for(int i=0;i<4;i++){int old=hud.liveStatsMask;Require(hud.HandleStatsPointer(EventType.MouseDown,hud.StatsMetricRect(drawn,i).center*density,sw,sh)&&hud.liveStatsMask==(old^(1<<i)),"independent metric toggle / "+i);}
            hud.liveStatsMask=15;hud.liveStatsOpen=true;
            StartChapter();ProbeReset();OpeningPicks=2;Wave=3;State=RunState.Running;WaveElapsed=ArrivalDuration+tuning.cleanupWindow;expeditionPlan.waves[2]=EncounterRoute.EliteShift;
            Require(ChampionDue&&EliteBlocked,"elite still requires its champion after arrival timeout");
            SpawnEntry(2);var champion=enemies.Single(e=>e.durability?.champion==true);SpawnEntry(2);
            Require(enemies.Count(e=>e.durability?.champion==true)==1&&enemies.Any(e=>e.crew==CrewRole.Repair),"concealed champion gets a repair escort, once per elite encounter");
            FinishWave();Require(State==RunState.Running,"elite cannot pay out while champion lives");
            ContributionFor(Deck.Fists).direct=37;ContributionFor(Deck.Fists).healing=3;State=RunState.Drafting;Require(SaveCheckpoint()&&ResumeExpedition(),"champion state restores from checkpoint");
            Require(LiveTotals().damage==37&&LiveTotals().healing==3,"live item totals survive independent checkpoint decoding");
            champion=enemies.Single(e=>e.durability?.champion==true);Require(champion.rig.Root.Find("Body")!=null||champion.rig.Body!=null,"restored champion has a physical rig");
            var legacy=CaptureCheckpoint();foreach(var o in legacy.graph.objects)if(o.type.EndsWith("+Enemy"))o.fields.RemoveAll(f=>f.name=="durability");var codec=GraphCodec();codec.Decode(legacy.graph);Require(true,"previous checkpoint actors without durability state still decode");
            State=RunState.Running;currentCredit=new KillCredit{weapon=Deck.Fists,item=Deck.Basics[0],action=++actionSerial};Hurt(champion,9999,Vector3.zero,0);int salvage=Salvage;FinishWave();
            Require(State==RunState.Drafting&&Salvage==salvage+10,"defeating champion awards base four plus six salvage at completion");AwardEncounterSalvage();Require(Salvage==salvage+10,"champion payout is idempotent");
            if(render)
            {
                StartChapter();ProbeReset();Wave=10;OpeningPicks=2;State=RunState.Running;DurabilityBuild("cutcarry",2);Health=MaxHealth;hero.EquipWeapons(WeaponKind.Sword,WeaponKind.Hammer,false);hero.WeaponIdentity(Deck.WeaponFor(Deck.Basics[0]),Deck.WeaponFor(Deck.Basics[1]));
                cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();var c=DurabilityProbe(CrewRole.Freight,true,true,x:0,z:5);var repair=DurabilityProbe(CrewRole.Repair,x:2,z:6);var enforcer=DurabilityProbe(CrewRole.None,true,x:-2,z:4);Hurt(enforcer,25,Vector3.zero,.2f);Hurt(c,45,Vector3.zero,.2f);c.crewGuard=18;c.crewGuardUntil=Elapsed+10;StepCrew(repair,.2f);
                foreach(var row in LiveSequenceStats()){var values=ContributionFor(row.item);values.direct=123;values.collision=34;values.assists=2;values.healing=1.5f;itemKills[row.item]=3;}Kills=LiveSequenceStats().Sum(r=>r.kills);
                hud.liveStatsOpen=true;hud.liveStatsMask=15;StepChampion(c,.01f);yield return Shot("champion-braced-stats-1080p");
                ExposeChampion(c);StepChampion(c,.01f);yield return Shot("champion-exposed-stats-1080p");
                foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1024,768),new Vector2Int(2560,1080)})
                {Screen.SetResolution(size.x,size.y,false);for(int i=0;i<8;i++)yield return null;yield return Shot("stats-"+size.x+"x"+size.y);}
                hud.liveStatsMask=6;yield return Shot("stats-damage-assists");hud.liveStatsOpen=false;yield return Shot("stats-collapsed");
                Screen.SetResolution(1280,720,false);for(int i=0;i<8;i++)yield return null;hud.liveStatsOpen=true;Wave=9;BeginWalkingFork();
                float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f)*.8f;float width=Screen.width/scale,height=Screen.height/scale;
                Require(!hud.ForkChoiceRect(0,width,height).Overlaps(hud.LiveStatsRect(width,height))&&!hud.ForkChoiceRect(1,width,height).Overlaps(hud.LiveStatsRect(width,height)),"live stats keep both fork choices unobstructed at 720p");
                Require(!hud.HandleForkPointer(EventType.MouseDown,hud.LiveStatsRect(width,height).center,width,height),"stats clicks cannot select a road behind the sidebar");yield return Shot("stats-fork-720p");
            }
            if(batch)
            {
                string samples=Path.Combine(output,"samples.jsonl");if(File.Exists(samples))throw new InvalidOperationException("Use an empty batch directory");
                int cases=0;
                foreach(bool baseline in new[]{true,false})foreach(string build in new[]{"pinball","storm","sever","gun","cutcarry","close","siphon"})foreach(int district in new[]{0,1,2})foreach(int seed in new[]{1741,4319,6229})
                {
                    durabilityBaseline=baseline;tuning.seed=seed;StartChapter();Wave=3+district*7;OpeningPicks=2;State=RunState.Drafting;DurabilityBuild(build,district+1);Health=MaxHealth*.8f;
                    var route=seed==1741?EncounterRoute.EliteShift:seed==4319?EncounterRoute.FreightYard:EncounterRoute.RepairLine;expeditionPlan.waves[Wave-1]=route;BeginWave();
                    for(int frame=0;frame<60*120&&State==RunState.Running;frame++)AdvanceCombat(1f/60);
                    var sample=new DurabilitySample{variant=baseline?"baseline":"candidate",build=build,route=route.ToString(),result=State.ToString(),seed=seed,district=district+1,rank=district+1,spawned=Spawned,kills=Kills,bodies=RetainedBodies,carried=enemies.Count,seconds=Elapsed,health=Health,maxHealth=MaxHealth,healing=LiveTotals().healing,damage=LiveTotals().damage,champions=enemies.Concat(corpses).Count(e=>e.durability?.champion==true),championKills=corpses.Count(e=>e.durability?.champion==true),orderErrors=OrderErrors,visibleSpawns=VisibleSpawns};
                    foreach(var group in enemies.Concat(corpses).Where(e=>e.durability!=null).GroupBy(DurabilityRole))
                    {var hit=group.Where(e=>e.durability.actions>0).ToArray();var dead=hit.Where(e=>e.deathAge>=0).ToArray();sample.roles.Add(new DurabilityRoleSample{role=group.Key,spawned=group.Count(),hit=hit.Length,firstActionKills=hit.Count(e=>e.durability.firstActionKill),killedAfterHit=dead.Length,actionsOnKilled=dead.Sum(e=>e.durability.actions),livingActions=hit.Where(e=>e.deathAge<0).Sum(e=>e.durability.actions),specialistStarts=group.Sum(e=>e.durability.starts),specialistCompleted=group.Sum(e=>e.crewUses),interrupts=group.Sum(e=>e.durability.interrupts),carried=group.Sum(e=>e.durability.carried)});}
                    File.AppendAllText(samples,JsonUtility.ToJson(sample)+Environment.NewLine);Require(OrderErrors==0&&VisibleSpawns==0,"natural encounter order / hidden entry / "+baseline+" / "+build+" / "+district+" / "+seed);
                    Debug.Log("DURABILITY_SAMPLE "+(++cases)+" / "+sample.variant+" / "+build+" / district "+(district+1)+" / "+route+" / "+State+" / "+Elapsed.ToString("0.0")+"s");yield return null;
                }
            }
            durabilityBaseline=false;report.passed=true;File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));Debug.Log("DURABILITY_REVIEW_SUCCESS / "+report.assertions);Application.Quit(0);
        }
    }
}
