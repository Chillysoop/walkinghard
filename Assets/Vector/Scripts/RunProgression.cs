using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        readonly Queue<DraftKind> pendingDrafts=new Queue<DraftKind>();
        public int ItemChoices {get;private set;}
        public int AugmentChoices {get;private set;}
        public int WaveCount=>tuning.waveCount;
        public string PendingDraft=>pendingDrafts.Count>0?"NEXT: "+pendingDrafts.Peek().ToString().ToUpperInvariant()+" PICK":"LET'S FIGHT";
        bool bossSpawned;
        public float BossFraction=>enemies.FirstOrDefault(e=>e.boss) is Enemy boss?Mathf.Clamp01(boss.health/boss.maxHealth):0;
        public bool BossControlOpen=>WaveElapsed%8<2.5f;
        public static DraftKind[] RewardsAfter(int wave)
        {
            var rewards=new List<DraftKind>();if(wave>=8)return rewards.ToArray();
            if(wave==1){rewards.Add(DraftKind.Ability);rewards.Add(DraftKind.Weapon);}
            else {rewards.Add(wave==3?DraftKind.Ultimate:DraftKind.Mixed);if(wave==2||wave==4||wave==6)rewards.Add(DraftKind.Equipment);if(wave==3||wave==5)rewards.Add(DraftKind.Augment);}return rewards.ToArray();
        }
        public static string Formation(int wave)=>new[]{"DENSE PACK · horizon rush with side-door reinforcements","CROSSFIRE · spaced shooters behind a light screen","HEAVY FRONT · enforcers protect a dense pack","FLANKERS · staggered arrivals from both side doors","FUNNEL · heavies advance ahead of ranged support","BREACH · repeated door packs with reinforcements","MIXED PRESSURE · shooters, frontliners and flanking arrivals","FINAL STAND · prepare your full loadout"}[Mathf.Clamp(wave-1,0,7)];
        public string EncounterPreview=>routedWave==Wave+1?RouteName(SelectedRoute):Formation(OpeningDraft?1:Mathf.Min(8,Wave+1));
        void OpenCheckpoint()
        {pendingDrafts.Clear();foreach(var kind in RewardsAfter(Wave))pendingDrafts.Enqueue(kind);State=RunState.Drafting;RoutePending=(Wave==2||Wave==5)&&Controlled==null;if(!RoutePending)Deck.Open(pendingDrafts.Dequeue(),true);}
        void CompleteDraft()
        {
            if(Deck.Draft==DraftKind.Augment)AugmentChoices++;else ItemChoices++;
            if(OpeningPicks<2){OpeningPicks++;if(OpeningPicks<2){Deck.Open(DraftKind.Ability,false);return;}}
            else {if(pendingDrafts.Count>0){Deck.Open(pendingDrafts.Dequeue(),false);return;}Waves[Waves.Count-1].choice=Deck.Loadout;Wave++;}
            hero.Equipment(Deck.Has(AbilityId.MassDriver),Deck.Pinball);BeginWave();Announce(Formation(Wave));
        }
        public bool ClaimAugmentToHost(int offer,LoadoutItem host,bool continueImmediately)
        {
            if(State!=RunState.Drafting||Deck.RewardClaimed||offer<0||offer>=Deck.Offers.Length||!Deck.Items.Contains(host)||Deck.Offers[offer].def.module?.Fits(host.def)!=true)return false;
            if(!ClaimReward(offer,PlaceKind.Inventory,0,false))return false;var module=Deck.Inventory.Last(i=>i.def==Deck.Offers[offer].def);Deck.Attach(module,host);if(continueImmediately)ConfirmDraft();return true;
        }
    }
}
