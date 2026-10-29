using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        LoadoutItem waitingItem;
        float waitingAge;
        bool countedWait;
        void EndTargetWait(){waitingItem=null;waitingAge=0;countedWait=false;}
        void ObserveTargetWait(LoadoutItem item,float dt)
        {
            if(waitingItem!=item){EndTargetWait();waitingItem=item;}
            var c=ContributionFor(item);c.rangeWait+=dt;waitingAge+=dt;
            if(!countedWait&&waitingAge>=.5f){c.waitEpisodes++;countedWait=true;}
        }
        public WaveItemFeedback TargetWaitWarning=>LastWaveFeedback?.items
            .Where(i=>i.values.rangeWait>=3&&i.values.rangeWait>=LastWaveFeedback.seconds*.1f&&
                (i.values.waitEpisodes>=2||i.values.rangeWait>=6))
            .OrderByDescending(i=>i.values.rangeWait).FirstOrDefault();
        internal void PrepareWaitReview()
        {
            var item=Deck.Slots.FirstOrDefault(i=>i?.def.kind==ItemKind.Ability);
            BeginFeedbackWave();ObserveTargetWait(item,2);EndTargetWait();ObserveTargetWait(item,2);
            Elapsed+=20;CaptureWaveFeedback();EndTargetWait();
        }
    }
    public sealed partial class PrototypeHUD
    {
        void TargetWaitCheckpoint(float w,float y)
        {
            var warning=game.TargetWaitWarning;if(warning==null)return;
            string text="LAST WAVE TARGET WAIT: "+warning.name+" · "+warning.values.rangeWait.ToString("0.0")+"s / "+warning.values.waitEpisodes+" pauses · DETAILS";
            if(Button(new Rect(355,y,w-390,20),text)){waveDetails=true;detailItem=null;iconGuide=false;}
        }
    }
}
