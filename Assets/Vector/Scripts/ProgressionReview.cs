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
        [Serializable] sealed class DraftOpportunity
        {
            public int wave,pick;public string kind,before,after;public string[] offers;
            public bool weaponOffered,weaponUpgrade,copyOffered,defenseOffered,discovery;public float gain;public string[] offerIds;
        }
        [Serializable] sealed class ProgressionSample
        {
            public string version,policy,drafter,pool,result,loadout,weaponOffers;public int seed,wave,kills,fusions,orderErrors,visibleSpawns;public bool committed;
            public float seconds,health;public bool valid;public string[] errors;
            public WaveReport[] encounters;public DraftOpportunity[] drafts;public RunHistoryData integrity;public ContributionReport[] items;public WeaponJourneyEncounter[] weaponEncounters;
        }
        List<DraftOpportunity> progressionDrafts;
        bool reviewOriginalWeaponOffers,reviewCommittedWeapons;
        string ProgressionPolicy=>reviewCommittedWeapons?"committed:"+CurrentPolicy:CurrentPolicy;
        void RecordDraftOpportunity()
        {
            if(progressionDrafts==null)return;
            var row=new DraftOpportunity{wave=Wave,kind=Deck.Draft.ToString(),before=Deck.Loadout,offers=Deck.Offers.Select(o=>o.Name+" / "+ItemRarity.Names[o.rank]).ToArray(),offerIds=Deck.Offers.Select(o=>o.def.id).ToArray(),discovery=Deck.WeaponDiscovery};
            float before=AutomatedLoadoutPolicy.Score(Deck,ProgressionPolicy);
            foreach(var offer in Deck.Offers)
            {
                var p=AutomatedLoadoutPolicy.Evaluate(Deck,offer,ProgressionPolicy,Deck.Draft==DraftKind.Ability);float gain=p==null?0:p.score-before;row.gain=Math.Max(row.gain,gain);
                if(offer.def.kind==ItemKind.Weapon){row.weaponOffered=true;row.weaponUpgrade|=gain>.5f;}
                row.copyOffered|=BuildSynergies.Equipped(Deck).Any(i=>i.def==offer.def&&i.rank==offer.rank&&i.rank<5);
                row.defenseOffered|=gain>.5f&&(offer.def.kind==ItemKind.Armor||offer.def.id=="Counterguard"||offer.def.id=="DrainBrand"||offer.def.id=="treads"||offer.def.id=="harvest");
            }
            progressionDrafts.Add(row);
        }
        void FinishDraftRecord()
        {if(progressionDrafts?.Count>0){var row=progressionDrafts.Last();row.pick=Deck.Selected;row.after=Deck.Loadout;}}
        void EquipSimulationStorage()
        {
            float max=MaxHealth;var ultimate=Deck.Ultimate;AutomatedLoadoutPolicy.EquipStored(Deck,ProgressionPolicy);
            Health=Mathf.Clamp(Health+MaxHealth-max,0,MaxHealth);
            if(ultimate!=Deck.Ultimate){UltimateRemaining=UltimateCooldown;ultimateDue=Elapsed+UltimateCooldown;ultimateRequested=false;}
        }
        IEnumerator ReviewProgression()
        {
            enabled=false;qa=false;fx.muted=true;SetCombatSpeed(1);FastSimulation=false;
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            var args=Environment.GetCommandLineArgs();string Arg(string key,string fallback){int at=Array.IndexOf(args,key);return at<0?fallback:args[at+1];}
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Progression review requires an isolated profile");
            string output=Path.GetFullPath(Arg("-reviewOutput","ProgressionReview"));Directory.CreateDirectory(output);
            QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;for(int i=0;i<4;i++)yield return null;
            if(args.Contains("-weaponDiscoveryChecks")){yield return CheckWeaponDiscovery(output,!args.Contains("-nographics"));Application.Quit(0);yield break;}
            if(args.Contains("-ragdollStress")){yield return ReviewRagdollStress(output);Application.Quit(0);yield break;}
            if(args.Contains("-progressionChecks")){yield return CheckProgression(output,!args.Contains("-nographics"));Application.Quit(0);yield break;}
            string samples=Path.Combine(output,"samples.jsonl");if(File.Exists(samples))throw new IOException("Use a fresh progression output directory");
            var policies=Arg("-journeyPolicy","all")=="all"?ArchetypePolicies:new[]{Arg("-journeyPolicy","all")};
            if(policies.Any(p=>!ArchetypePolicies.Contains(p)))throw new ArgumentException("Unknown progression style");
            var seeds=Arg("-seeds","1741,4319,6229").Split(',').Select(int.Parse).ToArray();bool unlocked=Arg("-pool","unlocked")=="unlocked";
            legacyDraftPolicy=args.Contains("-legacyDraftPolicy");forcedSeed=true;
            reviewOriginalWeaponOffers=args.Contains("-originalWeaponOffers");reviewCommittedWeapons=args.Contains("-committedWeapons");
            float reviewSeconds=float.Parse(Arg("-progressionSeconds","2100"),System.Globalization.CultureInfo.InvariantCulture);if(!float.IsFinite(reviewSeconds)||reviewSeconds<60||reviewSeconds>2100)throw new ArgumentException("Review seconds must be 60..2100");
            if(reviewCommittedWeapons&&(legacyDraftPolicy||policies.Any(p=>!new[]{"gun","cutcarry","close"}.Contains(p))))throw new ArgumentException("Committed trials require the projected gun, cutcarry or close policy");
            foreach(int seed in seeds)foreach(string policy in policies)
            {
                office=new OfficeProfile{seal=unlocked,unlocks=unlocked?Constellation.Nodes.Select(n=>n.item).ToList():new List<string>()};
                tuning.seed=seed;simulationPolicy=policy;policyRandom=null;progressionDrafts=new List<DraftOpportunity>();weaponJourneyEncounters=new List<WeaponJourneyEncounter>();
                if(args.Contains("-progressionResume")){if(seeds.Length!=1||policies.Length!=1||!ResumeExpedition())throw new InvalidOperationException("Diagnostic replay requires one seed, one policy and a valid checkpoint");}else StartChapter();
                float seconds=0,start=Time.realtimeSinceStartup;bool timeout=false;int heartbeat=0;
                while(State!=RunState.Complete&&State!=RunState.Defeated)
                {
                    if(seconds>reviewSeconds||Time.realtimeSinceStartup-start>1200){timeout=true;break;}
                    if(State==RunState.Drafting){if(StopOpen)SimulationExpeditionStop();else SimulationDraft();}
                    else if(State==RunState.Revealing){StepFinaleReveal(RevealDuration);seconds+=RevealDuration;}
                    else for(int i=0;i<90&&State==RunState.Running;i++){AdvanceCombat(1f/60);seconds+=1f/60;if(RoutePending)ChooseRoute(SimulationExpeditionRoute());}
                    if((int)seconds/120>heartbeat){heartbeat=(int)seconds/120;Debug.Log("PROGRESSION_PROGRESS "+policy+" seed="+seed+" wave="+Wave+" health="+Health);if(args.Contains("-progressionTrace"))WriteProgressionState(Path.Combine(output,"state-"+heartbeat+".txt"));}
                    yield return null;
                }
                var errors=new List<string>();if(timeout)errors.Add("timeout");if(OrderErrors!=0)errors.Add("sequence order");if(VisibleSpawns!=0)errors.Add("visible spawn");if(Spawned!=Kills+enemies.Count||corpses.Count!=Kills||itemKills.Sum(p=>p.Value)+EnvironmentalKills!=Kills)errors.Add("actor/kill identity");if(reviewCommittedWeapons&&!AutomatedLoadoutPolicy.Allowed(Deck,ProgressionPolicy))errors.Add("committed weapon constraint");
                if(timeout)
                {
                    WriteProgressionState(Path.Combine(output,"timeout-state.txt"));
                    if(!args.Contains("-nographics")){yield return new WaitForEndOfFrame();var capture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,"timeout.png"),capture.EncodeToPNG());Destroy(capture);}
                }
                if(State==RunState.Complete&&(Wave!=21||Waves.Count!=21||ItemChoices!=20||AugmentChoices!=5))errors.Add("journey completion contract");
                if(!float.IsFinite(Health)||!float.IsFinite(HeroPosition.z))errors.Add("non-finite state");
                var result=new ProgressionSample{version=Application.version,policy=policy,drafter=legacyDraftPolicy?"legacy":"projected",pool=unlocked?"unlocked":"fresh",seed=seed,result=timeout?"TimedOut":State.ToString(),wave=Wave,kills=Kills,fusions=Deck.Fusions,health=Health,seconds=seconds,loadout=Deck.Loadout,valid=errors.Count==0,errors=errors.ToArray(),orderErrors=OrderErrors,visibleSpawns=VisibleSpawns,encounters=Waves.ToArray(),drafts=progressionDrafts.ToArray(),integrity=RunHistory,items=ContributionReports()};
                result.committed=reviewCommittedWeapons;result.weaponOffers=reviewOriginalWeaponOffers?"original":"discovery";result.weaponEncounters=weaponJourneyEncounters.ToArray();
                File.AppendAllText(samples,JsonUtility.ToJson(result)+Environment.NewLine);Debug.Log("PROGRESSION_RESULT "+policy+" seed="+seed+" "+result.result+" wave="+Wave+" valid="+result.valid);
                if(!result.valid){Application.Quit(4);yield break;}
            }
            progressionDrafts=null;Application.Quit(0);
        }
        void WriteProgressionState(string path)
        {
            var text=new System.Text.StringBuilder();text.AppendLine("state="+State+" wave="+Wave+" waveTime="+WaveElapsed+" elapsed="+Elapsed+" health="+Health+" hero="+HeroPosition+" action="+ActionStatus+" waiting="+TargetWaitDetail);
            text.AppendLine("elite="+EliteEncounter+" blocked="+EliteBlocked+" championDue="+ChampionDue+" forkWaiting="+ForkAwaitingChoice+" pending="+RoutePending+" route="+RegionalRoute(Wave)+" loadout="+Deck.Loadout);
            foreach(var e in enemies.OrderByDescending(e=>e.durability?.champion==true))
            {bool nav=roads.Navigation(e.rig.Root.position,HeroPosition,out var waypoint);text.AppendLine("id="+e.id+" role="+DurabilityRole(e)+" health="+e.health+" p="+e.rig.Root.position+" velocity="+e.physics.LinearVelocity+" entering="+e.entering+" doorway="+e.doorway+" reposition="+e.repositioning+" canAttack="+EnemyCanAttack(e)+" navigation="+nav+" waypoint="+waypoint+" stun="+e.stun);
                if(e.entering){var delta=e.doorway-e.rig.Root.position;delta.y=0;foreach(var hit in Physics.SphereCastAll(e.rig.Root.position+Vector3.up*1.2f,.55f,delta.normalized,delta.magnitude,1<<EntryLayout.WorldLayer))text.AppendLine("  entry block="+hit.collider.name+" parent="+hit.transform.parent.name+" point="+hit.point+" distance="+hit.distance);}}
            foreach(var fork in roads.Forks)text.AppendLine("fork="+fork.Origin+" angle="+fork.Rotation.eulerAngles+" parentEnd="+fork.Parent.End);
            File.WriteAllText(path,text.ToString());
        }
        IEnumerator CheckProgression(string output,bool render)
        {
            int count=0;var checks=new List<string>();void Require(bool ok,string name){if(!ok)throw new InvalidOperationException("PROGRESSION_CHECK: "+name);count++;checks.Add(name);}
            var d=new AbilityDeck(1);var pistol=d.Acquire(LoadoutCatalog.Find("pistol"),3);d.Move(pistol,PlaceKind.Weapon,0);
            var lowRifle=AutomatedLoadoutPolicy.Evaluate(d,new RewardOffer(LoadoutCatalog.Find("rifle")),"gun");
            Require(lowRifle.place==PlaceKind.Inventory,"Rare pistol is retained over a Common two-handed rifle");
            var tether=d.Acquire(LoadoutCatalog.Find("VectorTether"),3);d.Move(tether,PlaceKind.Sequence,2);
            var lower=AutomatedLoadoutPolicy.Evaluate(d,new RewardOffer(tether.def,1,2),"gun");
            Require(lower.projection.after.Slots.Contains(lower.projection.Copy(tether))&&lower.projection.Copy(tether).rank==3,"lower-rarity identical ability cannot replace its stronger equipped copy");
            var second=d.Acquire(pistol.def,3);d.Move(second,PlaceKind.Weapon,1);
            var fusion=AutomatedLoadoutPolicy.Evaluate(d,new RewardOffer(pistol.def,1,3),"gun");
            Require(fusion.projection.fusionRanks.SequenceEqual(new[]{4})&&fusion.projection.after.Weapons.Count(w=>w!=null)==1,"paired-weapon fusion accounts for losing the consumed second weapon");
            Require(pistol.rank==3&&d.Weapons[0]==pistol&&d.Weapons[1]==second,"projection leaves original ranks and mounts unchanged");
            d.Move(d.Acquire(LoadoutCatalog.Find("DrainBrand")),PlaceKind.Sequence,3);AutomatedLoadoutPolicy.Arrange(d,"siphon");
            Require(Array.FindIndex(d.Slots,i=>i?.def.id=="DrainBrand")<Array.FindIndex(d.Slots,i=>i?.def.kind==ItemKind.Basic),"Drain Brand precedes weapon damage");
            foreach(string style in ArchetypePolicies)
            {
                var deck=new AbilityDeck(4319);for(int i=0;i<3;i++){deck.Open(DraftKind.Ability,false);var c=AutomatedLoadoutPolicy.Choose(deck,style);Require(deck.ClaimTo(c.offer,c.place,c.index),style+" legal opening pick "+i);}
                foreach(var kind in new[]{DraftKind.Weapon,DraftKind.Equipment,DraftKind.Mixed,DraftKind.Ultimate,DraftKind.Augment})
                {deck.Open(kind,false);var c=AutomatedLoadoutPolicy.Choose(deck,style);Require(c.host!=null?(deck.ClaimTo(c.offer,PlaceKind.Inventory)&&deck.Attach(deck.Inventory.Last(i=>i.def==deck.Offers[c.offer].def),c.host)):deck.ClaimTo(c.offer,c.place,c.index),style+" legal "+kind);AutomatedLoadoutPolicy.EquipStored(deck,style);Require(float.IsFinite(AutomatedLoadoutPolicy.Score(deck,style))&&deck.Slots.Count(i=>i?.def.kind==ItemKind.Basic)==2,style+" maintains both basic turns / "+kind);}
            }
            var identities=new HashSet<string>();
            foreach(string style in ArchetypePolicies)
            {
                var deck=new AbilityDeck(19);foreach(var definition in LoadoutCatalog.Weapons.Concat(LoadoutCatalog.Abilities))if(definition!=LoadoutCatalog.Fists)deck.Acquire(definition,2);
                AutomatedLoadoutPolicy.EquipStored(deck,style);identities.Add(deck.Loadout);
                Require(deck.Count==3,style+" fills available utility slots");
                if(style=="cutcarry")Require(deck.Weapons.Any(w=>w?.def.weapon==WeaponKind.Sword)&&deck.Weapons.Any(w=>w?.def.weapon==WeaponKind.Hammer),"cut-and-carry retains both weapon families when available");
                if(style=="gun")Require(deck.Weapons.Any(w=>w?.def.profile?.magazine>0),"gun preference actually equips a gun when available");
                if(style=="close")Require(deck.Weapons.Any(w=>w?.def.profile?.magazine>0)&&deck.Weapons.Any(w=>w==null||w.def.weapon==WeaponKind.Fists),"close-quarters preference retains a fist and a gun when available");
                if(style=="pinball")Require(deck.Has(AbilityId.MassDriver)||deck.Weapons.Any(w=>w?.def.weapon==WeaponKind.Hammer),"pinball preference keeps a body launcher when available");
                if(style=="siphon")Require(deck.Has(AbilityId.DrainBrand)||deck.Weapons.Any(w=>w?.def.id=="siphonblade"),"siphon preference keeps a drain source when available");
            }
            Require(identities.Count>=5,"seven preferences produce at least five distinct builds from the same equal-rarity catalog");
            forcedSeed=true;tuning.seed=1741;StartChapter();OpeningPicks=2;ProbeReset();State=RunState.Running;Health=100;RunHistory=new RunHistoryData();BeginIntegrityEncounter();
            var attacker=ProbeEnemy(0,2);float initial=Health;ReceiveDamage(attacker,17);Require(Mathf.Abs(RunHistory.sources.Sum(r=>r.damage)-(initial-Health))<.001f,"incoming damage records actual integrity after mitigation");
            var guard=Deck.Acquire(LoadoutCatalog.Find("Counterguard"));AddBarrier(guard,100,5);int hits=RunHistory.recent.Count;ReceiveDamage(attacker,8);Require(RunHistory.recent.Count==hits,"fully absorbed attacks do not appear as integrity loss");barriers.Clear();
            Health=5;ReceiveDamage(attacker,90);Require(RunHistory.recent.Last().damage==5&&RunHistory.recent.Last().health==0,"fatal damage excludes overkill");Health=0;FinishIntegrityEncounter(false);CaptureWaveFeedback();State=RunState.Defeated;
            Require(!RunHistory.encounters.Last().finished&&RunHistory.encounters.Last().end==0&&LastWaveFeedback.wave==Wave,"defeat includes unfinished encounter and its contributions");
            StartChapter();RecordIncoming("Fixture",4);State=RunState.Drafting;Require(SaveCheckpoint(),"recap saves");string graph=JsonUtility.ToJson(CaptureCheckpoint().graph);Require(ResumeExpedition()&&graph==JsonUtility.ToJson(CaptureCheckpoint().graph),"recap checkpoint graph restores exactly");
            var old=CaptureCheckpoint();old.schema=6;old.graph.roots.RemoveAll(r=>r.name=="RunHistory");AtomicWrite(SavePath,JsonUtility.ToJson(old));Require(ResumeExpedition()&&!RunHistory.complete&&RunHistory.sources.Count==0,"previous save version resumes with explicitly partial recap");
            var broken=CaptureCheckpoint().graph;broken.roots.RemoveAll(r=>r.name=="Health");bool rejected=false;try{GraphCodec().Decode(broken,legacyProgression:true);}catch(InvalidOperationException){rejected=true;}Require(rejected,"migration still rejects missing required combat state");
            if(render)
            {
                StartChapter();ProbeReset();OpeningPicks=2;Wave=9;State=RunState.Running;Health=220;RunHistory=new RunHistoryData();BeginIntegrityEncounter();
                var rifle=Deck.Acquire(LoadoutCatalog.Find("rifle"),3);Deck.Move(rifle,PlaceKind.Weapon,0);ContributionFor(rifle).direct=1280;ContributionFor(rifle).assists=4;
                foreach(var pair in new[]{("Freight Brute",80f),("Ranged Raider",65f),("Linebreaker",75f)}){Elapsed+=3;RecordIncoming(pair.Item1,pair.Item2);Health-=pair.Item2;}
                Health=0;FinishIntegrityEncounter(false);CaptureWaveFeedback();State=RunState.Defeated;
                foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})
                {Screen.SetResolution(size.x,size.y,false);for(int i=0;i<15;i++)yield return null;yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,"recap-"+size.x+"x"+size.y+".png"),t.EncodeToPNG());Destroy(t);}
            }
            File.WriteAllText(Path.Combine(output,"checks.txt"),count+" assertions passed\n"+string.Join("\n",checks));Debug.Log("PROGRESSION_CHECKS "+count+" passed");
        }
    }
}
