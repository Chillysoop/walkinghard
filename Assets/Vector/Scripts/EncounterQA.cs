using System;
using System.IO;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void QADraft()
        {
            float elapsed=Elapsed,health=Health;Vector3 heroPosition=HeroPosition;int count=Deck.Count;
            ConfirmDraft();Advance();TogglePause();
            qaDraftChecks&=State==RunState.Drafting&&Elapsed==elapsed&&Health==health&&HeroPosition==heroPosition&&Deck.Count==count;
            if(qaBuild=="reroll")
            {int bank=Deck.Rerolls;if(bank>0){SelectOffer(0);AssignSlot(0);for(int i=0;i<bank+1;i++)Reroll();qaDraftChecks&=Deck.Rerolls==0&&Deck.Selected==-1&&Deck.SelectedSlot==-1;}else{Reroll();qaDraftChecks&=Deck.Rerolls==0;}}
            int select=0;
            if(Deck.IsAbilityDraft)
            {
                qaDraftChecks&=Deck.Offers.Length==3;
                for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)qaDraftChecks&=Deck.Offers[i].id!=Deck.Offers[j].id;
                AbilityId wanted=Deck.Count==0?AbilityId.MassDriver:Deck.Count==1?AbilityId.GravityWell:AbilityId.ArcBolt;
                if(qaBuild=="reverse")wanted=Deck.Count==0?AbilityId.Breaker:Deck.Count==1?AbilityId.ArcBolt:AbilityId.ForcePush;
                for(int i=0;i<Deck.Offers.Length;i++)if(Deck.Offers[i].id==wanted)select=i;
                SelectOffer(select);
                ConfirmDraft();qaDraftChecks&=State==RunState.Drafting&&Deck.Count==count;
                int slot=Deck.Count==0?2:Deck.Count==1?0:1;AssignSlot(slot);
            }
            else
            {
                for(int i=0;i<Deck.ModifierOffers.Length;i++)if(Deck.ModifierOffers[i].id==ModifierId.FaultLine)select=i;
                SelectOffer(select);
            }
            ConfirmDraft();
            if(State==RunState.Running){int wave=Wave;ConfirmDraft();qaDraftChecks&=Wave==wave;}
        }
        void QAManual()
        {
            if(Wave!=1)return;
            if(qaManualPhase==0&&Elapsed>5){Automatic=false;qaManualPhase=1;}
            if(qaManualPhase==1&&ActiveSlot<0){qaHeldCasts=AbilityCasts;qaManualPhase=2;}
            if(qaManualPhase==2&&Elapsed>9){qaManualChecks&=AbilityCasts==qaHeldCasts;if(Advance())qaManualPhase=3;}
            if(qaManualPhase==3&&Elapsed>12){qaManualChecks&=AbilityCasts==qaHeldCasts+1;Automatic=true;qaManualPhase=4;}
        }
        [Serializable] sealed class QAReport
        {
            public string result,build,loadout;public int seed,waves,kills,spawned,alive,casts,launches,collisions,ragdolls,physicsContacts,worldContacts,leftDoors,rightDoors,horizon,visibleSpawns,orderErrors,rerolls;
            public float elapsed,health;public bool passed,draftChecks,manualChecks;public int[] slotCasts,castTrace;public WaveReport[] reports;
        }
        void ReportQA()
        {
            reported=true;
            var report=new QAReport{result=State.ToString(),build=qaBuild,seed=Seed,loadout=Deck.Loadout,waves=Waves.Count,kills=Kills,spawned=Spawned,alive=enemies.Count,casts=AbilityCasts,launches=Launches,collisions=CollisionHits,ragdolls=Ragdolls,physicsContacts=PhysicsContacts,worldContacts=WorldContacts,leftDoors=LeftEntries,rightDoors=RightEntries,horizon=HorizonEntries,visibleSpawns=VisibleSpawns,orderErrors=OrderErrors,rerolls=Deck.Rerolls,elapsed=Elapsed,health=Health,draftChecks=qaDraftChecks,manualChecks=qaManualChecks&&(qaBuild!="manual"||qaManualPhase==4),slotCasts=SlotCasts,castTrace=castTrace.ToArray(),reports=Waves.ToArray()};
            report.passed=State==RunState.Complete&&Deck.Count==3&&Deck.Upgrades.Count==1&&OrderErrors==0&&VisibleSpawns==0&&LeftEntries>0&&RightEntries>0&&HorizonEntries>0&&Ragdolls>=10&&PhysicsContacts>0&&WorldContacts>0&&Spawned==Kills+enemies.Count&&qaDraftChecks&&report.manualChecks&&(!Deck.Has(AbilityId.MassDriver)||Launches>0);
            string path=string.IsNullOrEmpty(qaPath)?Path.Combine(Application.persistentDataPath,"qa-report.json"):qaPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(report,true));Debug.Log("WALKING_HARD_QA "+JsonUtility.ToJson(report));Application.Quit(report.passed?0:2);
        }
    }
}
