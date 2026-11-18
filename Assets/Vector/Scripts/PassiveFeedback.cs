using System.Collections.Generic;
using System.Linq;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        // Observation only. These transient counters never choose targets, consume RNG,
        // alter proc budgets or enter the saved combat graph. Completed explanations do.
        readonly Dictionary<LoadoutItem,string> passiveEffects=new Dictionary<LoadoutItem,string>();
        readonly HashSet<string> passiveEvents=new HashSet<string>();
        bool observingPassives;
        static bool PassiveItem(LoadoutItem item)=>item!=null&&(LoadoutCatalog.IsArmor(item.def.kind)||item.def.kind==ItemKind.Rig);
        void BeginPassiveFeedback(){passiveEffects.Clear();passiveEvents.Clear();observingPassives=true;}
        void ResetPassiveFeedback(){passiveEffects.Clear();passiveEvents.Clear();observingPassives=false;}
        void ObservePassiveEvent(string key){if(observingPassives)passiveEvents.Add(key);}
        void ObservePassive(LoadoutItem host,string effect)
        {if(observingPassives&&PassiveItem(host))passiveEffects[host]=effect;}
        string PassiveExplanation(LoadoutItem item,Contribution c)
        {
            if(!PassiveItem(item))return "";
            if(passiveEffects.TryGetValue(item,out var effect))
            {
                if(effect=="barrier")return c.absorption>0?"Barrier absorbed damage.":"Barrier created; no damage absorbed.";
                if(effect=="conductive")return c.assists>0?"Conductive marks discharged.":"Conductive applied; no owned mark discharged.";
                if(effect=="siphon")return c.healing>0?"Siphon marks restored health.":"Siphon applied; no healing recorded.";
                return effect;
            }
            if(c.direct+c.collision+c.control+c.absorption+c.healing+c.assists>.001f)return "Effect contributed this wave.";
            string reason;
            switch(item.def.signature)
            {
                case Signature.Recoil:case Signature.Loader:case Signature.VentGreaves:
                    reason=!passiveEvents.Contains("reload")?"No ammunition replenished.":"Ammo refilled; no effect output recorded.";break;
                case Signature.Switchboard:case Signature.StormCapacitor:
                    reason=!passiveEvents.Contains("discharge")?"No Conductive discharge.":"Discharged; no extra effect output recorded.";break;
                case Signature.Counterweight:case Signature.Momentum:
                    reason=!passiveEvents.Contains("launch")?"No launch this wave.":"Launched; no effect output recorded.";break;
                case Signature.ImpactLens:case Signature.FaultLine:case Signature.Relay:
                    reason=!passiveEvents.Contains("collision")?"No body collision this wave.":"Body collisions; no effect output recorded.";break;
                case Signature.SpurTreads:case Signature.Salvage:
                    reason=!passiveEvents.Contains("sever")?"No sever this wave.":"Severed; no effect output recorded.";break;
                case Signature.Reservoir:case Signature.Predator:
                    reason=!passiveEvents.Contains("healing")?"No actual health restored.":"Healed; no effect output recorded.";break;
                case Signature.WardenVisor:case Signature.ReturnSpring:
                    reason=!passiveEvents.Contains("absorb")?"No damage absorbed by a guard.":"Absorbed hits; no response recorded.";break;
                case Signature.BallisticVest:reason="No qualifying distant gun hit created a barrier.";break;
                case Signature.StaticShroud:reason=!passiveEvents.Contains("loop")?"No sequence loop completed.":"Loop completed; no target marked.";break;
                case Signature.Rangefinder:reason="No bonus hit on a staggered survivor.";break;
                case Signature.MercyLens:reason="No wounded survivor received a Siphon mark.";break;
                case Signature.ExecutionHood:reason="No bonus cut on a wounded survivor.";break;
                default:reason="No passive effect recorded this wave.";break;
            }
            var stats=item.def.stats;
            return reason+(stats.damage!=0||stats.speed!=0||stats.health!=0||stats.resistance!=0?" Stats still apply.":"");
        }
        internal void PreparePassiveFeedbackReview()
        {
            BeginFeedbackWave();
            foreach(var pair in new[]{("switchboard",PlaceKind.Helmet),("ballisticvest",PlaceKind.Armor),("ventgreaves",PlaceKind.Boots),("returnspring",PlaceKind.Rig)})
                MoveItem(Deck.Acquire(LoadoutCatalog.Find(pair.Item1)),pair.Item2);
            Elapsed+=20;CaptureWaveFeedback();
        }
        public string LastPassiveFeedback(LoadoutItem item)
        {
            if(item==null||LastWaveFeedback?.items==null)return "";
            // Reference identity prevents a new same-named item inheriting old feedback.
            var row=LastWaveFeedback.items.FirstOrDefault(r=>ReferenceEquals(r.owner,item));
            return string.IsNullOrEmpty(row?.payoff)?"":"LAST WAVE "+LastWaveFeedback.wave+": "+row.payoff;
        }
    }
}
