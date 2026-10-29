using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckWaveFeedback()
        {
            ProbeReset();var recovery=Deck.Acquire(LoadoutCatalog.Find("treads"));Deck.Move(recovery,PlaceKind.Boots);Health=MaxHealth-10;
            motes.Add(new Mote{owner=recovery,point=HeroPosition,heal=3,age=.31f,left=1});BeginFeedbackWave();StepSignatures(1f/60);CaptureWaveFeedback();
            MechanicRequire(LastWaveFeedback.healing==3&&LastWaveFeedback.blocked==0,"repair motes report healing, never damage blocked");
            ProbeReset();ResetWaveFeedback();MechanicRequire(LastWaveFeedback==null,"opening draft has no previous-wave report");
            var host=ProbeHost(LoadoutCatalog.Find("pistol"));Deck.Fused+=MergeKillCounts;
            var c=ContributionFor(host);c.direct=50;c.reload=2;c.preparation=1;c.rangeWait=4;c.healing=3;c.absorption=7;
            Note("Recorded trigger");BeginFeedbackWave();
            var enemy=ProbeEnemy(0,2,500);Hurt(enemy,40,Vector3.zero,0);
            c.reload+=2.5f;c.preparation+=.5f;c.rangeWait+=1.5f;c.healing+=6;c.absorption+=8;ArmorPreventedDamage+=2;
            Note("Recorded trigger");Note("Recorded trigger");CaptureWaveFeedback();
            var saved=LastWaveFeedback;var row=saved.items.Single(i=>i.id=="pistol");
            MechanicRequire(Mathf.Abs(saved.damage-40)<.001f&&saved.healing==6&&saved.blocked==10,"feedback uses wave deltas and includes passive resistance once");
            MechanicRequire(row.values.reload==2.5f&&row.values.preparation==.5f&&row.values.rangeWait==1.5f,"reload, readying and target waits stay separate");
            MechanicRequire(saved.triggers.Single().count==2,"synergy counters show this wave only");
            Deck.Move(host,PlaceKind.Inventory);var second=Deck.Acquire(host.def);ContributionFor(second).reload=3;ContributionFor(second).rangeWait=2;ContributionFor(second).preparation=.25f;Deck.Acquire(host.def);
            MechanicRequire(row.rank==1&&row.values.direct==40&&row.values.reload==2.5f,"storage and fusion cannot rewrite a completed wave snapshot");
            var keeper=Deck.Items.Single(i=>i.def.id=="pistol");var total=ContributionFor(keeper);
            MechanicRequire(total.reload==7.5f&&total.rangeWait==7.5f&&total.preparation==1.75f,"fusion preserves every accumulated delay metric");
            BeginFeedbackWave();ContributionFor(keeper).direct+=11;Note("Recorded trigger");CaptureWaveFeedback();
            MechanicRequire(LastWaveFeedback.damage==11&&LastWaveFeedback.items.Single().values.reload==0&&LastWaveFeedback.triggers.Single().count==1,"next wave excludes pre-fusion history and previous-wave triggers");
            ResetWaveFeedback();MechanicRequire(LastWaveFeedback==null,"new run clears previous feedback");
            BeginFeedbackWave();ObserveTargetWait(keeper,.2f);EndTargetWait();
            MechanicRequire(ContributionFor(keeper).waitEpisodes==0,"brief target acquisition does not count as a prolonged pause");
            ObserveTargetWait(keeper,2);ObserveTargetWait(keeper,1);EndTargetWait();ObserveTargetWait(keeper,2);Elapsed+=20;CaptureWaveFeedback();
            MechanicRequire(LastWaveFeedback.items.Single().values.waitEpisodes==2,"a continuous wait counts once and a later wait counts separately");
            MechanicRequire(TargetWaitWarning!=null&&TargetWaitWarning.id==keeper.def.id,"repeated long waits appear at the checkpoint");
            var snapshot=TargetWaitWarning;EndTargetWait();Deck.Move(keeper,PlaceKind.Inventory);
            MechanicRequire(TargetWaitWarning==snapshot,"rearranging items keeps the last-wave warning factual");
            BeginFeedbackWave();Elapsed+=20;CaptureWaveFeedback();MechanicRequire(TargetWaitWarning==null,"warnings do not leak into a later wave");
            ResetWaveFeedback();
        }
    }
}
