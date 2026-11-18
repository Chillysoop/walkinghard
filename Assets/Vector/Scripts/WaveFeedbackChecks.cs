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
            var c=ContributionFor(host);c.direct=50;c.reload=2;c.preparation=1;c.rangeWait=4;c.outOfReachWait=3;c.obstructedWait=.5f;c.healing=3;c.absorption=7;
            Note("Recorded trigger");BeginFeedbackWave();
            var enemy=ProbeEnemy(0,2,500);Hurt(enemy,40,Vector3.zero,0);
            c.reload+=2.5f;c.preparation+=.5f;c.rangeWait+=1.5f;c.outOfReachWait+=.5f;c.obstructedWait+=.25f;c.healing+=6;c.absorption+=8;ArmorPreventedDamage+=2;
            Note("Recorded trigger");Note("Recorded trigger");CaptureWaveFeedback();
            var saved=LastWaveFeedback;var row=saved.items.Single(i=>i.id=="pistol");
            MechanicRequire(Mathf.Abs(saved.damage-40)<.001f&&saved.healing==6&&saved.blocked==10,"feedback uses wave deltas and includes passive resistance once");
            MechanicRequire(row.values.reload==2.5f&&row.values.preparation==.5f&&row.values.rangeWait==1.5f,"reload, readying and target waits stay separate");
            MechanicRequire(row.values.outOfReachWait==.5f&&row.values.obstructedWait==.25f&&row.values.ArrivalWait==.75f,"wave delays distinguish reach, obstructions and arrivals");
            MechanicRequire(saved.triggers.Single().count==2,"synergy counters show this wave only");
            Deck.Move(host,PlaceKind.Inventory);var second=Deck.Acquire(host.def);ContributionFor(second).reload=3;ContributionFor(second).rangeWait=2;ContributionFor(second).outOfReachWait=1;ContributionFor(second).obstructedWait=.25f;ContributionFor(second).preparation=.25f;Deck.Acquire(host.def);
            MechanicRequire(row.rank==1&&row.values.direct==40&&row.values.reload==2.5f,"storage and fusion cannot rewrite a completed wave snapshot");
            var keeper=Deck.Items.Single(i=>i.def.id=="pistol");var total=ContributionFor(keeper);
            MechanicRequire(total.reload==7.5f&&total.rangeWait==7.5f&&total.preparation==1.75f,"fusion preserves every accumulated delay metric");
            MechanicRequire(total.outOfReachWait==4.5f&&total.obstructedWait==1&&total.ArrivalWait==2&&row.values.outOfReachWait==.5f,"fusion preserves reason totals without rewriting the wave snapshot");
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
            ProbeReset();ResetWaveFeedback();
            var quiet=Deck.Acquire(LoadoutCatalog.Find("ventgreaves"));Deck.Move(quiet,PlaceKind.Boots);
            var unused=Deck.Acquire(LoadoutCatalog.Find("switchboard"));
            BeginFeedbackWave();Elapsed+=1;CaptureWaveFeedback();
            var quietRow=LastWaveFeedback.items.Single(r=>r.owner==quiet);
            MechanicRequire(quietRow.values.healing==0&&quietRow.payoff.Contains("No ammunition replenished")&&!LastWaveFeedback.items.Any(r=>r.owner==unused),"zero-output equipped gear stays visible; unused storage does not");
            var passiveOnly=Deck.Acquire(LoadoutCatalog.Find("returnspring"));
            MechanicRequire(!PassiveExplanation(passiveOnly,new Contribution()).Contains("Stats still apply"),"effect-only gear never claims nonexistent stat bonuses");
            var history=LastPassiveFeedback(quiet);var replacement=Deck.Acquire(LoadoutCatalog.Find("ventgreaves"),2);Deck.Move(replacement,PlaceKind.Boots);
            MechanicRequire(history.Length>0&&LastPassiveFeedback(replacement)==""&&LastPassiveFeedback(quiet)==history,"same-name replacements never inherit another item's wave explanation");
            Deck.Move(quiet,PlaceKind.Boots);BeginFeedbackWave();ObservePassiveEvent("reload");CaptureWaveFeedback();
            MechanicRequire(LastPassiveFeedback(quiet).Contains("Ammo refilled"),"actual refill differs from a wave with no refill");
            var shield=Deck.Acquire(LoadoutCatalog.Find("ballisticvest"));Deck.Move(shield,PlaceKind.Armor);BeginFeedbackWave();AddBarrier(shield,5,3);CaptureWaveFeedback();
            MechanicRequire(LastPassiveFeedback(shield).Contains("Barrier created; no damage absorbed"),"an unused created barrier is not mislabeled inactive");
            Deck.Move(unused,PlaceKind.Helmet);BeginFeedbackWave();var marked=ProbeEnemy(0,4);MarkEnemy(marked,unused);CaptureWaveFeedback();
            MechanicRequire(LastPassiveFeedback(unused).Contains("Conductive applied"),"setup marks remain visible even before an owned discharge");
            var graph=GraphCodec().Capture(this);var decoder=GraphCodec();decoder.Decode(graph);
            var feedback=(WaveFeedback)decoder.Root("LastWaveFeedback");var restored=(AbilityDeck)decoder.Root("Deck");
            MechanicRequire(feedback.items.Single(r=>r.id=="switchboard").owner==restored.Armor[0]&&feedback.items.Single(r=>r.id=="switchboard").payoff.Contains("Conductive applied"),"feedback text and exact owned item survive the checkpoint graph");
            foreach(var node in graph.objects.Where(n=>n.type==typeof(WaveItemFeedback).FullName))node.fields.RemoveAll(f=>f.name=="owner"||f.name=="payoff");
            decoder=GraphCodec();decoder.Decode(graph);
            MechanicRequire(((WaveFeedback)decoder.Root("LastWaveFeedback")).items.All(r=>r.owner==null&&r.payoff==null),"older feedback has no invented item identity or explanation");
            ResetWaveFeedback();
        }
    }
}
