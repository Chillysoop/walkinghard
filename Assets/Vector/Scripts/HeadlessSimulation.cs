using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public bool FastSimulation {get;private set;}
        readonly long[] simulationProfile=new long[6];
        void ProfileSim(int stage,ref long stamp){long now=System.Diagnostics.Stopwatch.GetTimestamp();simulationProfile[stage]+=now-stamp;stamp=now;}
        bool simulation;int simulationRuns,simulationIndex,simulationSeed,simulationStride=1,simulationWaves=8;
        string simulationPolicy="mixed",simulationOutput;
        readonly System.Diagnostics.Stopwatch simulationClock=new System.Diagnostics.Stopwatch();
        System.Random policyRandom;
        bool simulationTimedOut;float nextSimulationCheck;float simulationLimit=480,nextSimulationTrace=60;
        static readonly string[] SimulationPolicies={"random","balanced","pinball","storm","sever","gun"};
        void ConfigureSimulation(string[] args)
        {
            string Arg(string key,string fallback){int at=Array.IndexOf(args,key);return at>=0&&at+1<args.Length?args[at+1]:fallback;}
            simulationRuns=int.Parse(Arg("-simRuns","0"));simulation=simulationRuns>0;if(!simulation)return;
            simulationSeed=int.Parse(Arg("-seed","1"));simulationStride=Math.Max(1,int.Parse(Arg("-simStride","1")));simulationWaves=int.Parse(Arg("-simWaves","8"));
            if(simulationWaves!=3&&simulationWaves!=8)throw new ArgumentException("-simWaves must be 3 or 8");
            string mode=Arg("-simMode","Full");if(mode!="Full"&&mode!="Fast")throw new ArgumentException("Unknown simulation mode");FastSimulation=mode=="Fast";
            simulationPolicy=Arg("-simPolicy","mixed");if(simulationPolicy!="mixed"&&!SimulationPolicies.Contains(simulationPolicy))throw new ArgumentException("Unknown simulation policy");
            simulationLimit=float.Parse(Arg("-simMaxSeconds","480"),System.Globalization.CultureInfo.InvariantCulture);if(!float.IsFinite(simulationLimit)||simulationLimit<120||simulationLimit>600)throw new ArgumentException("Simulation time limit must be 120–600 combat seconds");
            simulationOutput=Path.GetFullPath(Arg("-simOutput",Path.Combine(Application.dataPath,"..","simulations.jsonl")));
            Directory.CreateDirectory(Path.GetDirectoryName(simulationOutput));
            if(File.Exists(simulationOutput))throw new IOException("Simulation output already exists; use a fresh output path to avoid mixing batches.");
            forcedSeed=true;tuning.seed=simulationSeed;
        }
        string CurrentPolicy=>simulationPolicy=="mixed"?SimulationPolicies[Math.Abs(Seed)%SimulationPolicies.Length]:simulationPolicy;
        void ValidateSimulationStep(float dt)
        {
            if(Elapsed<nextSimulationCheck)return;nextSimulationCheck=Elapsed+1;
            if(!float.IsFinite(HeroPosition.z)||HeroPosition.y< -1||HeroPosition.y>2||HeroPosition.z< -1||HeroPosition.z>500)
                throw new InvalidOperationException("Simulation hero left the playable corridor: "+HeroPosition);
            if(enemies.Any(e=>!float.IsFinite(e.physics.Center.y)||e.physics.Center.y< -4))
                throw new InvalidOperationException("Simulation enemy left the physical floor");
            if(Elapsed>=nextSimulationTrace){nextSimulationTrace+=60;var boss=enemies.FirstOrDefault(e=>e.boss);Debug.Log("SIM_PROGRESS seed="+Seed+" elapsed="+Elapsed+" wave="+Wave+" health="+Health+" enemies="+enemies.Count+" boss="+(boss==null?"none":boss.health+" at "+boss.rig.Root.position)+" action="+ActionStatus);}
            if(Elapsed>=simulationLimit){simulationTimedOut=true;State=RunState.Defeated;}
        }
        string PolicyTags=>CurrentPolicy=="pinball"?"stagger launch collision iron hammer":CurrentPolicy=="storm"?"mark chain storm":CurrentPolicy=="sever"?"sever blade gather":CurrentPolicy=="gun"?"gun reload pierce gather":"guard sustain tempo";
        float PolicyValue(ItemDefinition def,int rank)
        {
            float score=ItemRarity.Power[rank]*2;
            foreach(var tag in def.tags.Split(' '))if(tag.Length>0&&PolicyTags.Split(' ').Contains(tag))score+=1.6f;
            if(def.kind==ItemKind.Augment)return 3+Deck.CompatibleHosts(def).Max(h=>PolicyValue(h.def,h.rank)+(h.augment==null?3:0));
            if(def.kind==ItemKind.Weapon&&Deck.Weapons.Any(w=>w==null))score+=2;
            if(LoadoutCatalog.IsArmor(def.kind)&&Deck.Armor[LoadoutCatalog.ArmorIndex(def.kind)]==null)score+=3;
            if(def.kind==ItemKind.Rig&&Deck.Rig==null)score+=3;
            return score;
        }
        void SimulationDraft()
        {
            if(policyRandom==null)policyRandom=new System.Random(Seed^0x371e);
            if(!Deck.RewardClaimed)
            {
                int pick=CurrentPolicy=="random"?policyRandom.Next(Deck.Offers.Length):Enumerable.Range(0,Deck.Offers.Length).OrderByDescending(i=>PolicyValue(Deck.Offers[i].def,Deck.Offers[i].rank)+(Deck.Copies(Deck.Offers[i].def,Deck.Offers[i].rank)+Deck.Offers[i].copies>=3?2:0)).First();
                var offer=Deck.Offers[pick];Deck.Select(pick);
                if(offer.def.kind==ItemKind.Augment){var compatible=Deck.CompatibleHosts(offer.def).OrderByDescending(h=>(h.augment==null?10:0)+PolicyValue(h.def,h.rank)).ToArray();ClaimAugmentToHost(pick,compatible[CurrentPolicy=="random"?policyRandom.Next(compatible.Length):0],false);}
                else
                {
                    Deck.SuggestedPlace(offer.def,out var place,out int index);
                    if(Deck.Draft==DraftKind.Ability){place=PlaceKind.Sequence;index=Array.FindIndex(Deck.Slots,i=>i==null);}
                    else if(offer.def.kind==ItemKind.Ability&&place==PlaceKind.Inventory)
                    {
                        var weakest=Deck.Slots.Where(i=>i?.def.kind==ItemKind.Ability).OrderBy(i=>PolicyValue(i.def,i.rank)).FirstOrDefault();
                        if(weakest!=null&&PolicyValue(offer.def,offer.rank)>PolicyValue(weakest.def,weakest.rank)){place=PlaceKind.Sequence;index=Array.IndexOf(Deck.Slots,weakest);}
                    }
                    else if(place!=PlaceKind.Inventory)
                    {
                        var existing=place==PlaceKind.Weapon?Deck.Weapons[index]:place==PlaceKind.Rig?Deck.Rig:place==PlaceKind.Ultimate?Deck.Ultimate:LoadoutCatalog.IsArmor(offer.def.kind)?Deck.Armor[LoadoutCatalog.ArmorIndex(offer.def.kind)]:null;
                        if(existing!=null&&existing.def!=offer.def&&CurrentPolicy!="random"&&PolicyValue(existing.def,existing.rank)>PolicyValue(offer.def,offer.rank)){place=PlaceKind.Inventory;index=0;}
                    }
                    if(!ClaimReward(pick,place,index,false))throw new InvalidOperationException("Simulation policy chose an illegal destination");
                }
            }
            if(Deck.RewardClaimed){ArrangeSimulationSequence();ConfirmDraft();}
        }
        void ArrangeSimulationSequence()
        {
            int Priority(LoadoutItem item){if(CurrentPolicy=="random")return policyRandom.Next(100);var id=item.def.ability?.id;return id==AbilityId.ConductiveBrand||id==AbilityId.GravityWell||id==AbilityId.VectorTether?0:id==AbilityId.Counterguard||id==AbilityId.SteelJab||id==AbilityId.Breaker?1:item.def.kind==ItemKind.Basic?2:3;}
            var order=Deck.Slots.Where(i=>i!=null).OrderBy(Priority).ToList();
            if(Deck.TwoHanded)
            {
                var skills=order.Where(i=>i.def.kind!=ItemKind.Basic).ToArray();int before=Math.Min(3,skills.Count(i=>Priority(i)<2));Deck.Move(Deck.Basics[0],PlaceKind.Sequence,before);
                var positions=Enumerable.Range(0,5).Where(i=>Deck.Slots[i]?.def.kind!=ItemKind.Basic).ToArray();for(int i=0;i<skills.Length;i++)Deck.Move(skills[i],PlaceKind.Sequence,positions[i]);
            }
            else for(int i=0;i<order.Count;i++)Deck.Move(order[i],PlaceKind.Sequence,i);
        }
        [Serializable] sealed class SimulationResult
        {
            public int rangedSpawned,rangedHits,rearBoundaryKills,environmentalKills;public float rangedDamage;
            public string schema="walkinghard-sim-1",version,mode,policy,result,loadout;public int seed,waves,waveReached,kills,spawned,collisions,launches,reloads,score,itemChoices,augmentChoices;public float elapsed,health;public double wallSeconds;public bool valid;public string[] errors;public ContributionReport[] items;public WaveReport[] waveReports;
        }
        void ReportSimulation()
        {
            simulationClock.Stop();Debug.Log("SIM_PROFILE signatures,ai,hero,motion,contacts,bodies="+string.Join(",",simulationProfile.Select(t=>(t*1000.0/System.Diagnostics.Stopwatch.Frequency).ToString("F1"))));Array.Clear(simulationProfile,0,simulationProfile.Length);var errors=new List<string>();
            if(OrderErrors!=0)errors.Add("sequence order");if(VisibleSpawns!=0)errors.Add("visible spawn");if(Spawned!=Kills+enemies.Count)errors.Add("enemy accounting");if(corpses.Count!=Kills||itemKills.Sum(p=>p.Value)+EnvironmentalKills!=Kills)errors.Add("kill identity");if(MaxCascadeHop>2)errors.Add("cascade limit");
            var modules=Deck.Hosts.Where(h=>h.augment!=null).Select(h=>h.augment).ToArray();if(modules.Distinct().Count()!=modules.Length)errors.Add("duplicate module ownership");
            if(State==RunState.Complete&&simulationWaves==8&&(ItemChoices!=13||AugmentChoices!=2))errors.Add("reward totals");
            if(State==RunState.Complete&&simulationWaves==8&&!bossSpawned)errors.Add("final boss never spawned");
            if(State==RunState.Complete&&(Kills==0||AbilityCasts==0))errors.Add("completed without combat");
            var result=new SimulationResult{version=Application.version,mode=FastSimulation?"Fast":"Full",policy=CurrentPolicy,result=simulationTimedOut?"TimedOut":State.ToString(),loadout=Deck.Loadout,seed=Seed,waves=Waves.Count,waveReached=Wave,kills=Kills,spawned=Spawned,collisions=CollisionHits,launches=Launches,reloads=Reloads,score=Score.Banked,itemChoices=ItemChoices,augmentChoices=AugmentChoices,elapsed=Elapsed,health=Health,wallSeconds=simulationClock.Elapsed.TotalSeconds,valid=errors.Count==0,errors=errors.ToArray(),items=ContributionReports(),waveReports=Waves.ToArray()};
            result.rangedSpawned=RangedSpawned;result.rangedHits=RangedHits;result.rangedDamage=RangedDamage;
            result.rearBoundaryKills=RearBoundaryKills;result.environmentalKills=EnvironmentalKills;
            File.AppendAllText(simulationOutput,JsonUtility.ToJson(result)+Environment.NewLine);
            Debug.Log("SIMULATION "+(simulationIndex+1)+"/"+simulationRuns+" seed="+Seed+" "+result.result+" valid="+result.valid);
            if(!result.valid){Application.Quit(4);return;}
            simulationIndex++;if(simulationIndex>=simulationRuns){reported=true;Application.Quit(0);return;}
            tuning.seed=simulationSeed+simulationIndex*simulationStride;policyRandom=null;simulationTimedOut=false;nextSimulationCheck=0;nextSimulationTrace=60;Begin();
        }
    }
}
