using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public void ReviewActualSynergyCue()
        {
            StartMechanicReview(Array.IndexOf(ReviewCases,"SuppressiveVolley"));
            var source=Deck.Acquire(LoadoutCatalog.Find("ConductiveBrand"));var target=enemies[0];
            ResetSynergyCues();MarkEnemy(target,source);ConsumeMark(target);
        }
    }
    public sealed partial class PrototypeHUD
    {
        IEnumerator RunSynergyReview(string directory)
        {
            Screen.SetResolution(1280,720,false);game.Begin();game.HoldCheckpointAfterPick=true;
            var arc=game.Deck.Acquire(LoadoutCatalog.Find("ArcBolt"));game.Deck.Move(arc,PlaceKind.Sequence,3);
            game.Deck.Move(game.Deck.Acquire(LoadoutCatalog.Find("rifle")),PlaceKind.Weapon,0);
            foreach(string id in new[]{"SuppressiveVolley","recoil"})
            {
                game.Deck.Open(DraftKind.Mixed,false);game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find(id),1,3);game.SelectOffer(0);ClearLoadoutGesture();
                reviewPointer=new Vector2(-200,-200);yield return null;yield return null;
                ReviewCheck(CurrentBuildLink()!=null,"equipped partner shown for "+id);
                var partner=CurrentBuildLink().partner;
                ReviewCheck(partner.def.id==(id=="recoil"?"rifle":"ArcBolt"),"specific valid partner: "+id);
                yield return CapturePresentation(directory,"30-fit-"+id);
                SetManageBuild(true);yield return null;yield return null;
                ReviewCheck(dropRegions.Any(r=>r.offer<0&&Host(r)==partner),"partner highlight maps to equipped item: "+id);
                yield return CapturePresentation(directory,"31-build-"+id);
                SetManageBuild(false);yield return null;
                var destination=dropRegions.First(r=>r.offer<0&&!r.socket&&r.index!=-1&&OfferFitsRegion(0,r));
                yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,destination.rect.center);
                ReviewCheck(game.Deck.RewardClaimed&&BuildSynergies.Equipped(game.Deck).Any(i=>i.def.id==id),"new reward drops directly into its destination: "+id);
            }
            game.ReviewActualSynergyCue();ReviewCheck(game.PayoffVisible&&game.PayoffTitle=="CONDUCTIVE DISCHARGE","actual discharge creates combat cue");
            yield return CapturePresentation(directory,"32-actual-synergy-payoff");
        }
    }
}
