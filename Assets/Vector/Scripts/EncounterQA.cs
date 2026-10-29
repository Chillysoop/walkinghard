using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        bool modelChecks;
        void QADraft()
        {
            if(!modelChecks)
            {
                LoadoutChecks.Run();
                var a=new LoadoutItem(LoadoutCatalog.Abilities[0]);var b=new LoadoutItem(LoadoutCatalog.Abilities[0]);
                itemKills[a]=2;itemKills[b]=3;MergeKillCounts(a,new[]{b});
                var delayed=KillOwner(b);itemKills[delayed]=KillsFor(delayed)+1;
                if(KillsFor(a)!=6||KillsFor(b)!=6)throw new InvalidOperationException("Delayed kill credit must survive fusion");
                itemKills.Clear();fusedOwners.Clear();modelChecks=true;
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
                if(Deck.Draft==DraftKind.Ability)AssignSlot(Array.FindIndex(Deck.Slots,s=>s==null));
                ConfirmDraft();qaDraftChecks&=Deck.RewardClaimed;
                return;
            }
            if(!OpeningDraft&&Deck.Draft==DraftKind.Weapon)
            {
                var reward=Deck.Inventory.FirstOrDefault(i=>i.def==Deck.Offers[Deck.Selected].def);
                if(reward!=null)MoveItem(reward,PlaceKind.Weapon,0);
                // Named weapon scenarios add explicit fixtures to exercise each family independently of draft luck.
                if(new[]{"sword","hammer","rifle","twin"}.Contains(qaBuild))
                {
                    string wanted=qaBuild=="twin"?"pistol":qaBuild;var def=LoadoutCatalog.Weapons.First(d=>d.id==wanted);
                    var item=Deck.Items.FirstOrDefault(i=>i.def==def)??Deck.Acquire(def);MoveItem(item,PlaceKind.Weapon,0);
                    if(qaBuild=="twin")MoveItem(Deck.Acquire(def),PlaceKind.Weapon,1);
                }
                if(qaBuild=="tempest")MoveItem(Deck.Acquire(LoadoutCatalog.Ultimates[1]),PlaceKind.Ultimate);
                if(qaHoldCheckpoint){qaHolding=true;return;}
            }
            if(!OpeningDraft&&Deck.Draft==DraftKind.Mixed&&new[]{"sword","hammer","rifle"}.Contains(qaBuild))
            {
                var item=Deck.Weapons[0];if(item!=null&&item.rank==1){Deck.Acquire(item.def);Deck.Acquire(item.def);}
            }
            ConfirmDraft();
            if(State==RunState.Running){int wave=Wave;ConfirmDraft();qaDraftChecks&=wave==Wave;}
        }
        void QAManual()
        {
            if(Wave!=1)return;
            if(qaManualPhase==0){UltimateAutomatic=false;qaHeldCasts=AbilityCasts;qaManualPhase=1;}
            if(qaManualPhase==1&&Elapsed>16)
            {
                qaManualChecks&=UltimateCasts==0&&UltimateRemaining<=0&&AbilityCasts>qaHeldCasts;
                RequestUltimate();qaManualPhase=2;
            }
            if(qaManualPhase==2&&UltimateCasts==1){qaManualPhase=3;UltimateAutomatic=true;}
        }
        [Serializable] sealed class QAReport
        {
            public string result,build,loadout;public int tempestHits,seed,waves,kills,spawned,alive,casts,paired,independentTargets,ultimates,severs,reloads,launches,collisions,cascadeHop,ragdolls,physicsContacts,worldContacts,leftDoors,rightDoors,horizon,visibleSpawns,orderErrors,rerolls,fusions,score,bestCombo,bestMultiplier,creditedKills;
            public float elapsed,health;public bool passed,draftChecks,manualChecks,modelChecks,ultimateTiming;public int[] slotCasts,castTrace,weaponHits;public float[] ultimateTimes,ultimateDeadlines;public WaveReport[] reports;
        }
        void ReportQA()
        {
            reported=true;bool timing=true;for(int i=0;i<ultimateTimes.Count;i++)timing&=ultimateTimes[i]+.02f>=ultimateDeadlines[i];
            int credited=itemKills.Where(k=>k.Key.def.kind!=ItemKind.Weapon).Sum(k=>k.Value);
            var report=new QAReport{result=State.ToString(),build=qaBuild,tempestHits=TempestHits,seed=Seed,loadout=Deck.Loadout,waves=Waves.Count,kills=Kills,spawned=Spawned,alive=enemies.Count,casts=AbilityCasts,paired=PairedCasts,independentTargets=IndependentTargets,ultimates=UltimateCasts,severs=Severs,reloads=Reloads,launches=Launches,collisions=CollisionHits,cascadeHop=MaxCascadeHop,ragdolls=Ragdolls,physicsContacts=PhysicsContacts,worldContacts=WorldContacts,leftDoors=LeftEntries,rightDoors=RightEntries,horizon=HorizonEntries,visibleSpawns=VisibleSpawns,orderErrors=OrderErrors,rerolls=Deck.Rerolls,fusions=Deck.Fusions,score=Score.Banked,bestCombo=Score.Best,bestMultiplier=Score.BestMultiplier,creditedKills=credited,elapsed=Elapsed,health=Health,draftChecks=qaDraftChecks,manualChecks=qaManualChecks&&(qaBuild!="manual"||qaManualPhase==3),modelChecks=modelChecks,ultimateTiming=timing,slotCasts=SlotCasts,castTrace=castTrace.ToArray(),weaponHits=WeaponHits,ultimateTimes=ultimateTimes.ToArray(),ultimateDeadlines=ultimateDeadlines.ToArray(),reports=Waves.ToArray()};
            bool weapon=qaBuild!="sword"||Severs>0;
            weapon&=qaBuild!="tempest"||TempestHits>0;
            weapon&=qaBuild!="hammer"||WeaponHits[(int)WeaponKind.Hammer]>0;
            weapon&=qaBuild!="rifle"||(WeaponHits[(int)WeaponKind.Rifle]>0&&Reloads>0);
            weapon&=qaBuild!="twin"||(WeaponHits[(int)WeaponKind.Pistol]>0&&IndependentTargets>0&&Reloads>0);
            report.passed=State==RunState.Complete&&Deck.Count==3&&OrderErrors==0&&VisibleSpawns==0&&LeftEntries>0&&RightEntries>0&&HorizonEntries>0&&Ragdolls>=10&&PhysicsContacts>0&&WorldContacts>0&&Spawned==Kills+enemies.Count&&credited==Kills&&qaDraftChecks&&report.manualChecks&&modelChecks&&timing&&UltimateCasts>0&&Score.Banked>0&&weapon;
            string path=string.IsNullOrEmpty(qaPath)?Path.Combine(Application.persistentDataPath,"qa-report.json"):qaPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(report,true));Debug.Log("WALKING_HARD_QA "+JsonUtility.ToJson(report));Application.Quit(report.passed?0:2);
        }
    }
}
