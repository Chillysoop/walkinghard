using System;
using System.Collections.Generic;
using System.Linq;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] public sealed class WaveItemFeedback
        {
            public string id,name,module;public int rank,kills;public float range;
            public Contribution values;
        }
        [Serializable] public sealed class WaveTriggerFeedback {public string name;public int count;}
        [Serializable] public sealed class WaveFeedback
        {
            public int wave,kills,collisionKills;public float seconds,damage,healing,blocked,resistance;
            public WaveItemFeedback[] items;public WaveTriggerFeedback[] triggers;
        }
        public WaveFeedback LastWaveFeedback {get;private set;}
        readonly Dictionary<LoadoutItem,Contribution> waveContributions=new Dictionary<LoadoutItem,Contribution>();
        readonly Dictionary<LoadoutItem,int> waveItemKills=new Dictionary<LoadoutItem,int>();
        readonly Dictionary<string,int> waveNotes=new Dictionary<string,int>();
        float feedbackStart,feedbackResistance;int feedbackCollisionKills,feedbackKills;
        static Contribution CopyContribution(Contribution c)=>new Contribution{direct=c.direct,collision=c.collision,control=c.control,absorption=c.absorption,reload=c.reload,preparation=c.preparation,rangeWait=c.rangeWait,augment=c.augment,healing=c.healing,assists=c.assists,waitEpisodes=c.waitEpisodes};
        static Contribution ContributionDelta(Contribution a,Contribution b)=>new Contribution{direct=a.direct-b.direct,collision=a.collision-b.collision,control=a.control-b.control,absorption=a.absorption-b.absorption,reload=a.reload-b.reload,preparation=a.preparation-b.preparation,rangeWait=a.rangeWait-b.rangeWait,augment=a.augment-b.augment,healing=a.healing-b.healing,assists=a.assists-b.assists,waitEpisodes=a.waitEpisodes-b.waitEpisodes};
        void ResetWaveFeedback(){LastWaveFeedback=null;EndTargetWait();waveContributions.Clear();waveItemKills.Clear();waveNotes.Clear();}
        void BeginFeedbackWave()
        {
            EndTargetWait();waveContributions.Clear();waveItemKills.Clear();waveNotes.Clear();
            foreach(var entry in contributions)waveContributions[entry.Key]=CopyContribution(entry.Value);
            foreach(var entry in itemKills)waveItemKills[entry.Key]=entry.Value;
            foreach(var entry in InteractionNotes)waveNotes[entry.Key]=entry.Value;
            feedbackStart=Elapsed;feedbackResistance=ArmorPreventedDamage;feedbackCollisionKills=CollisionKills;feedbackKills=Kills;
        }
        void CaptureWaveFeedback()
        {
            // Snapshot before any reward, storage move or fusion mutates the loadout.
            var rows=new List<WaveItemFeedback>();
            foreach(var item in Deck.Items.Where(i=>i.def.kind!=ItemKind.Basic&&i.def.kind!=ItemKind.Augment))
            {
                var delta=ContributionDelta(ContributionFor(item),waveContributions.TryGetValue(item,out var before)?before:new Contribution());
                int kills=KillsFor(item)-(waveItemKills.TryGetValue(item,out int n)?n:0);
                if(kills==0&&delta.direct+delta.collision+delta.control+delta.absorption+delta.reload+delta.preparation+delta.rangeWait+delta.healing+delta.assists<=.001f)continue;
                rows.Add(new WaveItemFeedback{id=item.def.id,name=item.def.name,rank=item.rank,range=item.def.kind==ItemKind.Weapon||item.def.kind==ItemKind.Ability||item.def.kind==ItemKind.Ultimate?LoadoutCatalog.Action(item).range:0,module=item.augment?.def.name??"",kills=kills,values=delta});
            }
            var resistance=ArmorPreventedDamage-feedbackResistance;
            LastWaveFeedback=new WaveFeedback{wave=Wave,kills=Kills-feedbackKills,collisionKills=CollisionKills-feedbackCollisionKills,seconds=Elapsed-feedbackStart,
                damage=rows.Sum(r=>r.values.direct+r.values.collision),healing=rows.Sum(r=>r.values.healing),resistance=resistance,blocked=resistance+rows.Sum(r=>r.values.absorption),
                items=rows.OrderByDescending(r=>r.values.direct+r.values.collision).ThenBy(r=>r.name).ToArray(),
                triggers=InteractionNotes.Select(p=>new WaveTriggerFeedback{name=p.Key,count=p.Value-(waveNotes.TryGetValue(p.Key,out int n)?n:0)}).Where(t=>t.count>0).OrderByDescending(t=>t.count).ThenBy(t=>t.name).ToArray()};
        }
    }
}
