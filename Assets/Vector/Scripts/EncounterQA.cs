using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        bool modelChecks;
        bool qaUltimateProgression=true;
        int qaUltimateDrafts;
        bool qaDelayChecks=true;
        int qaReloadFrames,qaReadyFrames,qaPairWaitFrames;
        bool qaWeaponChecks=true;
        Transform qaRightWeapon,qaLeftWeapon;
        int qaArmedAbilities,qaArmedUltimates,qaLeftAttacks,qaWeaponFrames,qaWeaponAction=-1;
        void QABeginWeapons()
        {
            qaRightWeapon=hero.RightWeapon;qaLeftWeapon=hero.LeftWeapon;
            if(qaBuild=="progression")qaUltimateProgression&=Wave<=3?Deck.Ultimate==null&&UltimateCasts==0&&!Deck.UltimatesUnlocked:Deck.Ultimate!=null&&Deck.UltimatesUnlocked&&qaUltimateDrafts==1;
            if(!simulation)Debug.Log("QA_WAVE "+Wave+" / ultimate="+(Deck.Ultimate?.def.id??"none")+" / casts="+UltimateCasts);
        }
        void QACheckWeapons()
        {
            for(int slot=0;slot<6;slot++)
            {
                bool shown=TryGetActionDelay(slot,out var delay);
                qaDelayChecks&=!shown||(slot==ActiveSlot||slot==PairSlot);
                if(!shown)continue;
                qaDelayChecks&=delay.remaining>0&&delay.remaining<=delay.duration&&delay.Progress>=0&&delay.Progress<=1&&(Preparation<=0||ActionProgress==0);
                if(delay.label=="RELOADING")qaReloadFrames++;
                else if(delay.label=="READYING")qaReadyFrames++;
                else if(delay.label=="WAITING FOR PAIR")qaPairWaitFrames++;
                else qaDelayChecks&=new[]{"FOCUSING","ECHO","FLURRY","RETURN CUT","BACKWASH"}.Contains(delay.label);
            }
            // Check the actual rendered objects through preparation, attacks, casts and recovery.
            // Identity checks catch destroying/recreating the same-looking weapon between actions.
            qaWeaponFrames++;
            qaWeaponChecks&=hero.RightWeapon==qaRightWeapon&&hero.LeftWeapon==qaLeftWeapon;
            for(int mount=0;mount<2;mount++)
            {
                var model=mount==0?hero.RightWeapon:hero.LeftWeapon;
                var kind=Deck.WeaponFor(Deck.Basics[mount]).def.weapon;
                bool expected=kind!=WeaponKind.Fists&&(mount==0||!Deck.TwoHanded);
                qaWeaponChecks&=expected?model!=null&&model.gameObject.activeInHierarchy&&model.parent==(mount==0?hero.RightHand:hero.LeftHand)&&model.name==kind.ToString():model==null;
            }
            if(ActiveSlot<0||Preparation>0||actionAge<=0||qaWeaponAction==actionSerial)return;
            qaWeaponAction=actionSerial;
            if(hero.RightWeapon!=null||hero.LeftWeapon!=null)
            {
                if(ActiveSlot==5)qaArmedUltimates++;
                else if(actionWeapon==null)qaArmedAbilities++;
                else if(!Deck.TwoHanded&&actionItem.mount==1)qaLeftAttacks++;
            }
        }
        void QADraft()
        {
            if(!modelChecks)
            {
                LoadoutChecks.Run();
                qaUltimateProgression&=Deck.Ultimate==null&&UltimateRemaining==0&&UltimateCasts==0;
                var a=new LoadoutItem(LoadoutCatalog.Abilities[0]);var b=new LoadoutItem(LoadoutCatalog.Abilities[0]);
                itemKills[a]=2;itemKills[b]=3;MergeKillCounts(a,new[]{b});
                var delayed=KillOwner(b);itemKills[delayed]=KillsFor(delayed)+1;
                if(KillsFor(a)!=6||KillsFor(b)!=6)throw new InvalidOperationException("Delayed kill credit must survive fusion");
                itemKills[Deck.Fists]=7;
                if(ActionKills(Deck.Basics[0])!=7)throw new InvalidOperationException("Basic must show its weapon's kills");
                var sword=Deck.Acquire(LoadoutCatalog.Weapons[0]);MoveItem(sword,PlaceKind.Weapon,0);
                if(ActionKills(Deck.Basics[0])!=0)throw new InvalidOperationException("New weapon inherited slot kills");
                itemKills[sword]=3;MoveItem(sword,PlaceKind.Weapon,1);
                if(ActionKills(Deck.Basics[0])!=7||ActionKills(Deck.Basics[1])!=3)throw new InvalidOperationException("Kill counts must follow moved weapons");
                MoveItem(sword,PlaceKind.Inventory);Deck.Items.Remove(sword);
                itemKills.Clear();fusedOwners.Clear();modelChecks=true;
                CheckSpeedClock();SetCombatSpeed(qaBuild=="half"?.5f:qaBuild=="fast"?4:1);
                if(qaBuild=="armor")for(int i=0;i<3;i++)MoveItem(Deck.Acquire(LoadoutCatalog.Armor[i],2),(PlaceKind)((int)PlaceKind.Helmet+i));
            }
            float elapsed=Elapsed,health=Health,ultimate=UltimateRemaining;Vector3 position=HeroPosition;int count=Deck.Items.Count;
            Advance();TogglePause();
            qaDraftChecks&=State==RunState.Drafting&&Elapsed==elapsed&&Health==health&&HeroPosition==position&&Deck.Items.Count==count&&UltimateRemaining==ultimate;
            if(!Deck.RewardClaimed)
            {
                if(qaBuild=="reroll"){int bank=Deck.Rerolls;for(int i=0;i<bank+1;i++)Reroll();qaDraftChecks&=Deck.Rerolls==0&&Deck.Selected==-1;}
                qaDraftChecks&=Deck.Offers.Length==3;
                int selected=0;
                if(Deck.Draft==DraftKind.Ability)
                {
                    var wanted=Deck.Count==0?AbilityId.GravityWell:Deck.Count==1?AbilityId.MassDriver:AbilityId.ArcBolt;
                    for(int i=0;i<3;i++)if(Deck.Offers[i].def.ability.id==wanted)selected=i;
                }
                else if(Deck.Draft==DraftKind.Weapon)
                {
                    string wanted=qaBuild=="hammer"?"hammer":qaBuild=="rifle"?"rifle":qaBuild=="twin"?"pistol":"sword";
                    for(int i=0;i<3;i++)if(Deck.Offers[i].def.id==wanted)selected=i;
                }
                SelectOffer(selected);
                if(Deck.Draft==DraftKind.Augment){var host=Deck.CompatibleHosts(Deck.Offers[selected].def).First();ClaimAugmentToHost(selected,host,false);return;}
                if(Deck.Draft==DraftKind.Ultimate)
                {
                    qaUltimateDrafts++;qaUltimateProgression&=Wave==3&&Deck.Offers.All(o=>o.def.kind==ItemKind.Ultimate);
                    qaUltimateProgression&=ClaimReward(selected,PlaceKind.Ultimate,0,false)&&UltimateRemaining==UltimateCooldown;
                    return;
                }
                if(qaBuild=="quick")
                {
                    Deck.SuggestedPlace(Deck.Offers[selected].def,out var place,out int destination);
                    int picks=OpeningPicks,wave=Wave;var kind=Deck.Draft;
                    if(!ClaimReward(selected,place,destination))throw new InvalidOperationException("One-step reward equip failed");
                    qaDraftChecks&=picks<2?OpeningPicks==picks+1:kind==DraftKind.Ability?Deck.Draft==DraftKind.Weapon:State==RunState.Drafting||State==RunState.Running&&Wave==wave+1;
                    return;
                }
                if(Deck.Draft==DraftKind.Ability)AssignSlot(Array.FindIndex(Deck.Slots,s=>s==null));
                ConfirmDraft();qaDraftChecks&=Deck.RewardClaimed;
                return;
            }
            if(!OpeningDraft&&Deck.Draft==DraftKind.Weapon)
            {
                // These explicit fixtures exercise manual/held-weapon ultimate mechanics
                // in short runs; ordinary runs unlock their first ultimate after wave 3.
                if(qaBuild=="manual"||qaBuild=="armed")MoveItem(Deck.Acquire(LoadoutCatalog.Ultimates[0]),PlaceKind.Ultimate);
                var reward=Deck.Inventory.FirstOrDefault(i=>i.def==Deck.Offers[Deck.Selected].def);
                if(reward!=null)MoveItem(reward,PlaceKind.Weapon,0);
                // Named weapon scenarios add explicit fixtures to exercise each family independently of draft luck.
                if(new[]{"sword","hammer","rifle","twin","reload"}.Contains(qaBuild))
                {
                    string wanted=qaBuild=="twin"||qaBuild=="reload"?"pistol":qaBuild;var def=LoadoutCatalog.Weapons.First(d=>d.id==wanted);
                    var item=Deck.Items.FirstOrDefault(i=>i.def==def)??Deck.Acquire(def);MoveItem(item,PlaceKind.Weapon,0);
                    if(qaBuild=="twin"||qaBuild=="reload")MoveItem(Deck.Acquire(def),PlaceKind.Weapon,1);
                    if(qaBuild=="reload")
                    {
                        // Desynchronize paired magazines: only the empty pistol reloads.
                        ammunition[Deck.Weapons[0]]=0;ammunition[Deck.Weapons[1]]=3;
                        if(!Application.isBatchMode)SetCombatSpeed(.5f);
                    }
                }
                if(qaBuild=="tempest"){
                    MoveItem(Deck.Acquire(LoadoutCatalog.Ultimates[1]),PlaceKind.Ultimate);
                    MoveItem(Deck.Acquire(LoadoutCatalog.Find("wraps"),3),PlaceKind.Weapon,0);
                    // Exercise Common Tempest with legal Storm support, independent of draft luck.
                    MoveItem(Deck.Acquire(LoadoutCatalog.Find("grounding"),3),PlaceKind.Armor);
                    MoveItem(Deck.Acquire(LoadoutCatalog.Find("capacitor"),3),PlaceKind.Helmet);
                    for(int i=0;i<3;i++)MoveItem(Deck.Acquire(LoadoutCatalog.Abilities.First(d=>d.ability.id==new[]{AbilityId.ConductiveBrand,AbilityId.Breaker,AbilityId.ArcBolt}[i]),3),PlaceKind.Sequence,i+2);
                }
                if(qaBuild=="armed")
                {
                    MoveItem(Deck.Acquire(LoadoutCatalog.Weapons.First(d=>d.id=="sword")),PlaceKind.Weapon,0);
                    MoveItem(Deck.Acquire(LoadoutCatalog.Weapons.First(d=>d.id=="pistol")),PlaceKind.Weapon,1);
                    MoveItem(Deck.Basics[1],PlaceKind.Sequence,0);
                    for(int i=0;i<3;i++)MoveItem(Deck.Acquire(LoadoutCatalog.Abilities.First(d=>d.ability.id==new[]{AbilityId.Breaker,AbilityId.MassDriver,AbilityId.ForcePush}[i])),PlaceKind.Sequence,i+2);
                }
                ConfigureVarietyFixture();
                if(qaHoldCheckpoint)
                {
                    if(qaBuild=="ui")
                    {
                        MoveItem(Deck.Acquire(LoadoutCatalog.Weapons[0],3),PlaceKind.Weapon,0);
                        for(int i=0;i<3;i++)MoveItem(Deck.Acquire(LoadoutCatalog.Armor[i]),(PlaceKind)((int)PlaceKind.Helmet+i));
                        Deck.Acquire(LoadoutCatalog.Armor[0]);Deck.Acquire(LoadoutCatalog.Weapons[2]);Deck.Acquire(LoadoutCatalog.Ultimates[1]);Deck.Acquire(LoadoutCatalog.Rigs[0]);
                        Deck.Open(DraftKind.Mixed,false);
                        Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Armor[0]){owned=2};Deck.Offers[1]=new RewardOffer(LoadoutCatalog.Weapons[1],1,3);Deck.Offers[2]=new RewardOffer(LoadoutCatalog.Abilities[5],2,1){owned=Deck.Copies(LoadoutCatalog.Abilities[5],1)};
                        HoldCheckpointAfterPick=true;
                    }
                    qaHolding=true;return;
                }
            }
            if(!OpeningDraft&&Deck.Draft==DraftKind.Mixed&&new[]{"sword","hammer","rifle"}.Contains(qaBuild))
            {
                var item=Deck.Weapons[0];if(item!=null&&item.rank==1){Deck.Acquire(item.def);Deck.Acquire(item.def);}
            }
            ConfirmDraft();
            if(State==RunState.Running){int wave=Wave;ConfirmDraft();qaDraftChecks&=wave==Wave;}
        }
        void CheckSpeedClock()
        {
            // Exercise the same frame-budget path as the player, without an encounter step.
            float[] speeds={.5f,1,2,4};float savedAccumulator=accumulator;
            foreach(float speed in speeds)
            {
                SetCombatSpeed(speed);accumulator=0;State=RunState.Running;AdvanceCombat(.001f);
                if(Mathf.Abs(accumulator-.001f*speed)>.000001f)throw new InvalidOperationException("Combat speed must scale the simulation budget");
                float held=accumulator;State=RunState.Paused;AdvanceCombat(.1f);
                if(accumulator!=held)throw new InvalidOperationException("Pause must freeze the combat clock at every speed");
                State=RunState.Drafting;AdvanceCombat(.1f);
                if(accumulator!=held)throw new InvalidOperationException("Checkpoint must freeze the combat clock at every speed");
            }
            accumulator=savedAccumulator;
        }
        void QAManual()
        {
            if(Wave!=2)return;
            if(qaManualPhase==0){UltimateAutomatic=false;qaHeldCasts=AbilityCasts;qaManualPhase=1;}
            if(qaManualPhase==1&&UltimateRemaining<=0)
            {
                qaManualChecks&=UltimateCasts==0&&UltimateRemaining<=0&&AbilityCasts>qaHeldCasts;
                RequestUltimate();qaManualPhase=2;
            }
            if(qaManualPhase==2&&UltimateCasts==1){qaManualPhase=3;UltimateAutomatic=true;}
        }
        [Serializable] sealed class QAReport
        {
            public bool ultimateProgression;public int ultimateDrafts;
            public int mechanicAssertions;public ContributionReport[] contributions;public int itemChoices,augmentChoices;public bool delayChecks;public int reloadIndicatorFrames,readyIndicatorFrames,pairWaitIndicatorFrames;
            public bool heldWeaponChecks;public int heldWeaponFrames,armedAbilities,armedUltimates,leftHandAttacks;
            public string result,build,loadout;public int tempestHits,seed,waves,kills,spawned,alive,casts,paired,independentTargets,ultimates,severs,reloads,launches,collisions,cascadeHop,ragdolls,physicsContacts,worldContacts,leftDoors,rightDoors,horizon,visibleSpawns,orderErrors,rerolls,fusions,score,bestCombo,bestMultiplier,creditedKills;
            public float elapsed,health,speed,oldestBody,maxHealth,armorHealing,armorPreventedDamage;public int retainedBodies,walkingShoves,armorBonusHits;public bool passed,draftChecks,manualChecks,modelChecks,ultimateTiming,walkingIgnoresBodies;public int[] slotCasts,castTrace,weaponHits;public float[] ultimateTimes,ultimateDeadlines;public WaveReport[] reports;
        }
        void ReportQA()
        {
            if(simulation){ReportSimulation();return;}
            reported=true;bool timing=true;for(int i=0;i<ultimateTimes.Count;i++)timing&=ultimateTimes[i]+.02f>=ultimateDeadlines[i];
            int credited=itemKills.Sum(k=>k.Value);
            var report=new QAReport{mechanicAssertions=mechanicAssertions,contributions=ContributionReports(),itemChoices=ItemChoices,augmentChoices=AugmentChoices,result=State.ToString(),build=qaBuild,tempestHits=TempestHits,seed=Seed,loadout=Deck.Loadout,waves=Waves.Count,kills=Kills,spawned=Spawned,alive=enemies.Count,casts=AbilityCasts,paired=PairedCasts,independentTargets=IndependentTargets,ultimates=UltimateCasts,severs=Severs,reloads=Reloads,launches=Launches,collisions=CollisionHits,cascadeHop=MaxCascadeHop,ragdolls=Ragdolls,physicsContacts=PhysicsContacts,worldContacts=WorldContacts,leftDoors=LeftEntries,rightDoors=RightEntries,horizon=HorizonEntries,visibleSpawns=VisibleSpawns,orderErrors=OrderErrors,rerolls=Deck.Rerolls,fusions=Deck.Fusions,score=Score.Banked,bestCombo=Score.Best,bestMultiplier=Score.BestMultiplier,creditedKills=credited,elapsed=Elapsed,health=Health,draftChecks=qaDraftChecks,manualChecks=qaManualChecks&&(qaBuild!="manual"||qaManualPhase==3),modelChecks=modelChecks,ultimateTiming=timing,slotCasts=SlotCasts,castTrace=castTrace.ToArray(),weaponHits=WeaponHits,ultimateTimes=ultimateTimes.ToArray(),ultimateDeadlines=ultimateDeadlines.ToArray(),reports=Waves.ToArray()};
            report.ultimateProgression=qaUltimateProgression&&(tuning.waveCount<4||qaUltimateDrafts==1);
            report.ultimateDrafts=qaUltimateDrafts;
            bool weapon=qaBuild!="sword"||Severs>0;
            weapon&=qaBuild!="tempest"||TempestHits>0;
            weapon&=qaBuild!="hammer"||WeaponHits[(int)WeaponKind.Hammer]>0;
            weapon&=qaBuild!="rifle"||(WeaponHits[(int)WeaponKind.Rifle]>0&&Reloads>0);
            weapon&=qaBuild!="twin"||(WeaponHits[(int)WeaponKind.Pistol]>0&&IndependentTargets>0&&Reloads>0);
            report.retainedBodies=corpses.Count;report.walkingShoves=CorpseShoves;report.oldestBody=corpses.Count==0?0:corpses.Max(e=>e.deathAge);report.speed=CombatSpeed;
            report.walkingIgnoresBodies=corpses.All(c=>c.physics.IgnoresWalker(heroPhysics)&&(flights.Exists(f=>f.body==c)||enemies.All(e=>c.physics.IgnoresWalker(e.physics))));
            report.maxHealth=MaxHealth;report.armorHealing=ArmorHealing;report.armorPreventedDamage=ArmorPreventedDamage;report.armorBonusHits=ArmorBonusHits;
            weapon&=qaBuild!="armor"||(MaxHealth>tuning.heroHealth&&ArmorBonusHits>0&&ArmorPreventedDamage>0&&ArmorHealing>0);
            report.heldWeaponChecks=qaWeaponChecks;report.heldWeaponFrames=qaWeaponFrames;report.armedAbilities=qaArmedAbilities;report.armedUltimates=qaArmedUltimates;report.leftHandAttacks=qaLeftAttacks;
            weapon&=qaWeaponChecks&&qaWeaponFrames>0;
            report.delayChecks=qaDelayChecks;report.reloadIndicatorFrames=qaReloadFrames;report.readyIndicatorFrames=qaReadyFrames;report.pairWaitIndicatorFrames=qaPairWaitFrames;
            weapon&=qaDelayChecks;
            if(qaArmedAbilities>0||qaArmedUltimates>0)weapon&=qaReadyFrames>0;
            weapon&=qaBuild!="reload"||(qaReloadFrames>0&&qaPairWaitFrames>0&&Reloads>0&&WeaponHits[(int)WeaponKind.Pistol]>0);
            weapon&=qaBuild!="armed"||(qaArmedAbilities>0&&qaArmedUltimates>0&&qaLeftAttacks>0&&WeaponHits[(int)WeaponKind.Pistol]>0&&WeaponHits[(int)WeaponKind.Sword]>0);
            bool ultimateCoverage=Deck.Ultimate==null?UltimateCasts==0:UltimateCasts>0;
            report.passed=State==RunState.Complete&&Waves.Count==tuning.waveCount&&(tuning.waveCount!=8||ItemChoices==13&&AugmentChoices==2)&&Deck.Count==3&&OrderErrors==0&&VisibleSpawns==0&&LeftEntries>0&&RightEntries>0&&HorizonEntries>0&&Ragdolls>=10&&PhysicsContacts>0&&WorldContacts>0&&Spawned==Kills+enemies.Count&&credited==Kills&&qaDraftChecks&&report.manualChecks&&modelChecks&&timing&&ultimateCoverage&&report.ultimateProgression&&Score.Banked>0&&weapon&&corpses.Count==Kills&&report.oldestBody>7&&CorpseShoves>0&&report.walkingIgnoresBodies;
            string path=string.IsNullOrEmpty(qaPath)?Path.Combine(Application.persistentDataPath,"qa-report.json"):qaPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(report,true));Debug.Log("WALKING_HARD_QA "+JsonUtility.ToJson(report));Application.Quit(report.passed?0:2);
        }
    }
}
