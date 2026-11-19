using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        internal void PrepareBranchBalanceReview()
        {
            ProbeReset();ProbeHost(LoadoutCatalog.Find("BreachSalvo"),null,3);
            var shroud=Deck.Acquire(LoadoutCatalog.Find("staticshroud"),3);Deck.Move(shroud,PlaceKind.Armor);
            BeginFeedbackWave();ProbeEnemy(0,2);Elapsed+=10;ExpansionLoop();CaptureWaveFeedback();
            OpeningPicks=2;Deck.Open(DraftKind.Mixed,false);
            string[] ids={"staticshroud","BreachSalvo","wardenvisor"};for(int i=0;i<3;i++)Deck.Offers[i]=new RewardOffer(LoadoutCatalog.Find(ids[i]),1,3);
        }
        void CheckBranchBalance(Action<bool,string> require)
        {
            for(int rank=1;rank<=5;rank++)
            {
                ProbeReset();var weapon=ProbeHost(LoadoutCatalog.Find("ArcBolt"));var shroud=Deck.Acquire(LoadoutCatalog.Find("staticshroud"),rank);Deck.Move(shroud,PlaceKind.Armor);
                ExpansionLoop();require(BarrierAmount==0,"Shroud: no empty-loop barrier / "+rank);
                ProbeEnemy(30,30);ExpansionLoop();require(BarrierAmount==0,"Shroud: distant targets do not qualify / "+rank);
                var first=ProbeEnemy(0,2);var second=ProbeEnemy(.2f,2.5f);BeginFeedbackWave();ExpansionLoop();float capacity=6*(1+.15f*(rank-1));
                require(marks[first.id].owner==shroud&&marks[second.id].owner==shroud&&Mathf.Abs(BarrierAmount-capacity)<.001f,"Shroud: successful owned marks grant rarity-scaled barrier / "+rank);
                Elapsed+=1;ExpansionLoop();require(barriers.Count(b=>b.host==shroud)==1&&Mathf.Abs(BarrierAmount-capacity)<.001f&&Mathf.Abs(barriers.Single().expires-Elapsed-3)<.001f,"Shroud: one barrier refreshes to three seconds without stacking / "+rank);
                ReceiveDamage(first,1);CaptureWaveFeedback();require(ContributionFor(shroud).absorption==1&&LastPassiveFeedback(shroud).Contains("Barrier absorbed"),"Shroud: absorption belongs to armor and appears in wave feedback / "+rank);
                barriers.Clear();ConsumeMark(first);ConsumeMark(second);ExpansionLoop();require(BarrierAmount==0,"Shroud: same-action rejected marks cannot grant a barrier / "+rank);
                currentCredit.action=++actionSerial;AddBarrier(weapon,88,100);ExpansionLoop();require(Mathf.Abs(BarrierAmount-90)<.001f&&Mathf.Abs(barriers.Single(b=>b.host==shroud).left-2)<.001f,"Shroud: respects shared 90-barrier cap / "+rank);
                Elapsed+=3.01f;StepSignatures(.01f);require(!barriers.Any(b=>b.host==shroud),"Shroud: barrier actually expires / "+rank);
                KillEnemy(first,Vector3.zero,false);require(!MarkEnemy(first,shroud),"Shroud: defeated targets cannot count as successful marks / "+rank);

                ProbeReset();var salvo=ProbeHost(LoadoutCatalog.Find("BreachSalvo"),null,rank);first=ProbeEnemy(0,2);first.stun=1;actionTarget=first;float damage=activeCard.damage*Deck.Damage;Resolve(activeCard);
                require(Mathf.Abs(500-first.health-damage*1.5f)<.01f&&Mathf.Abs(activeCard.damage-28*ItemRarity.Power[rank])<.01f&&Mathf.Abs(activeCard.duration-.8f)<.001f,"Salvo: 28 base damage retains rarity scaling, prior-stagger bonus and 0.8s action / "+rank);
            }
            ProbeReset();var armor=Deck.Acquire(LoadoutCatalog.Find("staticshroud"));Deck.Move(armor,PlaceKind.Armor);
            require(BuildSynergies.Find(Deck,LoadoutCatalog.Find("wardenvisor")).Any(l=>l.partner==armor&&l.key=="guard"),"Shroud: guard payoff is discoverable in equipment comparisons");
            require(LoadoutCatalog.Find("rivetpistol").profile.damage==20&&LoadoutCatalog.Find("rifle").profile.damage==42,"Rivet and rifle remain at approved damage");
            ProbeReset();
        }
    }
}
