using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        // Opt-in experimental loadouts. No tuning or ordinary-play behavior changes.
        [Serializable] sealed class ControlledCase
        {
            public string id,context,category,candidate,helmet,armor,boots,rig,ultimate;
            public string[] weapons,abilities;
            public int seed,rank=1,baseRank=1;
            public float healthFraction=1;
        }
        [Serializable] sealed class ControlledCases {public ControlledCase[] cases;}
        [Serializable] sealed class ControlledMetrics
        {
            public string id,context,category,candidate;public int rank,ultimateEquipWave,ultimateCasts;
            public float initialHealth,maxHealth,rawIncoming,damageTaken,prevented,healing,potentialDrain,capLost,overheal,missingHealthSeconds,lowHealthSeconds,preUltimateHealth;
            public int drainActions,cappedDrainActions;public float[] waveHealth;
        }
        ControlledCase[] controlledCases;
        ControlledCase Controlled=>controlledCases==null?null:controlledCases[simulationIndex];
        ControlledMetrics controlledMetrics;
        readonly HashSet<int> controlledDrainActions=new HashSet<int>(),controlledCappedActions=new HashSet<int>();
        readonly List<float> controlledWaveHealth=new List<float>();
        string controlledLoadout;
        void ConfigureControlled(string path)
        {
            if(string.IsNullOrEmpty(path))return;
            controlledCases=JsonUtility.FromJson<ControlledCases>(File.ReadAllText(path))?.cases;
            if(controlledCases==null||controlledCases.Length!=simulationRuns)throw new ArgumentException("Controlled cases must match -simRuns");
            foreach(var c in controlledCases)
            {
                if(c.rank<1||c.rank>5||c.baseRank<1||c.baseRank>5||c.healthFraction<=0||c.healthFraction>1||c.weapons?.Length!=2||c.abilities?.Length!=3)throw new ArgumentException("Invalid controlled case "+c.id);
                foreach(var id in c.weapons.Concat(c.abilities).Concat(new[]{c.helmet,c.armor,c.boots,c.rig,c.ultimate}).Where(s=>!string.IsNullOrEmpty(s)))LoadoutCatalog.Find(id);
                if(c.category!="weapon"&&c.category!="armor"&&c.category!="ultimate")throw new ArgumentException("Invalid controlled category");
            }
            simulationSeed=controlledCases[0].seed;
        }
        void ControlledDraft()
        {
            var c=Controlled;
            if(OpeningPicks==0)
            {
                controlledMetrics=new ControlledMetrics{id=c.id,context=c.context,category=c.category,candidate=c.candidate,rank=c.rank};
                controlledDrainActions.Clear();controlledCappedActions.Clear();controlledWaveHealth.Clear();
                LoadoutItem Equip(string id,PlaceKind place,int index=0,bool focal=false)
                {
                    if(string.IsNullOrEmpty(id))return null;
                    var item=Deck.Acquire(LoadoutCatalog.Find(id),focal?c.rank:c.baseRank);
                    if(!MoveItem(item,place,index))throw new InvalidOperationException("Controlled placement failed: "+id);return item;
                }
                Equip(c.weapons[0],PlaceKind.Weapon,0,c.category=="weapon");Equip(c.weapons[1],PlaceKind.Weapon,1);
                for(int i=0;i<3;i++)Equip(c.abilities[i],PlaceKind.Sequence,i+2);
                Equip(c.helmet,PlaceKind.Helmet);Equip(c.armor,PlaceKind.Armor,0,c.category=="armor");Equip(c.boots,PlaceKind.Boots);Equip(c.rig,PlaceKind.Rig);
                // Fixed setup -> paired basics -> payoff order, identical within each comparison.
                Deck.Move(Deck.Slots[2],PlaceKind.Sequence,0);
                OpeningPicks=2;Health=MaxHealth*c.healthFraction;
                controlledMetrics.initialHealth=Health;controlledMetrics.maxHealth=MaxHealth;
            }
            else
            {
                controlledWaveHealth.Add(Health);Waves[Waves.Count-1].choice=Deck.Loadout;pendingDrafts.Clear();Wave++;
                if(Wave==4&&!string.IsNullOrEmpty(c.ultimate))
                {
                    controlledMetrics.preUltimateHealth=Health;
                    Deck.Open(DraftKind.Ultimate,false);
                    var item=Deck.Acquire(LoadoutCatalog.Find(c.ultimate),c.category=="ultimate"?c.rank:c.baseRank);
                    if(!MoveItem(item,PlaceKind.Ultimate))throw new InvalidOperationException("Controlled ultimate placement failed");
                    controlledMetrics.ultimateEquipWave=Wave;
                }
            }
            controlledLoadout=Deck.Loadout;
            hero.Equipment(Deck.Has(AbilityId.MassDriver),Deck.Pinball);BeginWave();
        }
        void ObserveControlledDrain(Enemy enemy,float dealt)
        {
            if(Controlled==null||controlledMetrics==null||currentCredit==null||dealt<=0||Health<=0)return;
            float rate=DrainSources(enemy).Values.Sum();bool harvest=Owner?.def.id=="harvest";
            if(rate<=0)return;
            int action=currentCredit.action;float used=turns.TryGetValue(action,out var turn)?turn.siphonHealed:0;
            float potential=dealt*rate,remaining=Mathf.Max(0,(harvest?HarvestHealingLimit(Owner):MaxHealth*.01f)-used),capped=Mathf.Min(potential,remaining);
            controlledMetrics.potentialDrain+=potential;controlledMetrics.capLost+=potential-capped;
            controlledMetrics.overheal+=Mathf.Max(0,capped-(MaxHealth-Health));
            if(Health<MaxHealth){controlledDrainActions.Add(action);if(potential>remaining+.0001f)controlledCappedActions.Add(action);}
        }
        ControlledMetrics FinishControlled()
        {
            if(Controlled==null)return null;
            controlledMetrics.healing=ArmorHealing;controlledMetrics.ultimateCasts=UltimateCasts;
            // Several attackers can finish in the lethal frame before HP is clamped.
            // Derive actual HP spent so terminal overkill never creates negative damage.
            controlledMetrics.damageTaken=controlledMetrics.initialHealth+ArmorHealing-Mathf.Max(0,Health);
            controlledMetrics.drainActions=controlledDrainActions.Count;controlledMetrics.cappedDrainActions=controlledCappedActions.Count;
            controlledMetrics.waveHealth=controlledWaveHealth.Concat(new[]{Health}).ToArray();return controlledMetrics;
        }
    }
}
