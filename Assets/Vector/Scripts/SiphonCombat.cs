using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public static readonly Color SiphonColor=new Color(1,.23f,.48f);
        sealed class Drain {public float until,rate;public LoadoutItem owner;}
        readonly Dictionary<int,Drain> drains=new Dictionary<int,Drain>();
        bool predatorReady;
        public float LifestealHealing {get;private set;}
        public float LifestealRate=>Deck.Lifesteal+(Health<MaxHealth*.5f&&Deck.Equipped(Signature.Reclaimer) is LoadoutItem armor?LoadoutCatalog.SiphonRate(armor.rank):0);
        public float HarvestHealingLimit(LoadoutItem item)=>MaxHealth*(.08f+.01f*(item.rank-1));
        void ResetSiphon(){drains.Clear();predatorReady=false;LifestealHealing=0;}
        void ArmPredator(){if(Deck.Equipped(Signature.Predator)!=null)predatorReady=true;}
        void DrainMark(Enemy enemy,LoadoutItem source,float potency=1)
        {
            if(enemy.deathAge>=0)return;
            float rate=LoadoutCatalog.SiphonRate(source.rank)*potency,until=Elapsed+4*Effect(source);
            if(!drains.TryGetValue(enemy.id,out var mark))drains[enemy.id]=mark=new Drain();
            if(mark.until<=Elapsed||rate>=mark.rate){mark.rate=rate;mark.owner=source;}
            mark.until=Mathf.Max(mark.until,until);
            fx.Symbol(enemy.physics.Center+Vector3.up*.8f,SiphonColor,.35f,true);
        }
        void SpreadDrainOnSever(Enemy enemy)
        {
            var blade=currentCredit?.weapon;
            if(blade?.def.id!="siphonblade"||!Once("siphon-spread"))return;
            var next=enemies.Where(e=>e!=enemy&&e.deathAge<0&&Reachable(e,10)&&Vector3.Distance(e.physics.Center,enemy.physics.Center)<3*Effect(blade)).OrderBy(e=>Vector3.Distance(e.physics.Center,enemy.physics.Center)).FirstOrDefault();
            if(next==null)return;
            DrainMark(next,blade);fx.Arc(enemy.physics.Center,next.physics.Center,SiphonColor);Note("Sever spread Siphon");
        }
        void ApplyLifesteal(Enemy enemy,float dealt)
        {
            if(currentCredit==null||dealt<=0||Health<=0||Health>=MaxHealth)return;
            // Credit and the healing budget follow the originating action through every
            // paired hit, augment, chain and delayed body impact. Healing is never a hit.
            if(Deck.Weapons[0]?.def.id!="siphonblade"&&Deck.Weapons[1]?.def.id!="siphonblade"&&Deck.Equipped(Signature.Reclaimer)==null&&!(drains.TryGetValue(enemy.id,out var activeMark)&&activeMark.until>Elapsed)&&Owner?.def.id!="harvest")return;
            var sources=new Dictionary<LoadoutItem,float>();
            foreach(var weapon in Deck.Weapons.Where(w=>w?.def.id=="siphonblade").Distinct())sources[weapon]=LoadoutCatalog.SiphonRate(weapon.rank);
            if(Deck.Equipped(Signature.Reclaimer) is LoadoutItem armor)sources[armor]=LoadoutCatalog.SiphonRate(armor.rank)*(Health<MaxHealth*.5f?2:1);
            if(drains.TryGetValue(enemy.id,out var mark)&&mark.until>Elapsed)sources[mark.owner]=(sources.TryGetValue(mark.owner,out float prior)?prior:0)+mark.rate;
            bool harvest=Owner?.def.id=="harvest";
            if(harvest)sources[Owner]=.2f*Effect(Owner);
            float rate=sources.Values.Sum();if(rate<=0)return;
            var turn=CurrentTurn;float limit=harvest?HarvestHealingLimit(Owner):MaxHealth*.01f;
            float heal=Mathf.Min(dealt*rate,Mathf.Max(0,limit-turn.siphonHealed),MaxHealth-Health);
            if(heal<=0)return;
            turn.siphonHealed+=heal;Health+=heal;LifestealHealing+=heal;ArmorHealing+=heal;
            foreach(var source in sources)ContributionFor(source.Key).healing+=heal*source.Value/rate;
            ArmPredator();fx.Arc(enemy.physics.Center,HeroPosition+Vector3.up,SiphonColor);Note("Siphon restored health");
        }
        void ResolveHarvest(LoadoutItem item)
        {
            var card=LoadoutCatalog.Action(item);
            fx.Ring(HeroPosition,card.range,SiphonColor,.7f);fx.Shape(HeroPosition,Vector3.forward,card.range,180,SiphonColor,.6f);
            foreach(var enemy in enemies.Where(e=>Reachable(e,card.range)).ToArray())
            {
                fx.Arc(enemy.physics.Center,HeroPosition+Vector3.up,SiphonColor);
                Hurt(enemy,card.damage*Deck.Damage,(HeroPosition-enemy.rig.Root.position).normalized*2,.3f);
            }
            fx.Play(2);ImpactPause(.045f);
        }
        public string SiphonDetails(LoadoutItem item)
        {
            string rules="Actual damage only; no overkill/corpses. All hits, pairs and chains share "+(MaxHealth*.01f).ToString("0.#")+" HP (1% max) per action. Marks refresh, never stack.";
            float rate=LoadoutCatalog.SiphonRate(item.rank)*100;
            switch(item.def.id)
            {
                case "siphonblade":return "+"+rate.ToString("0")+"% global Lifesteal. Cuts mark for another "+rate.ToString("0")+"% for "+(4*Effect(item)).ToString("0.#")+"s; one sever spread per action within "+(3*Effect(item)).ToString("0.#")+"m. "+rules;
                case "DrainBrand":return (item.HasAugment(AugmentId.Focused)?2:3+(item.rank-1)/2)+" targets, +"+(rate*(item.HasAugment(AugmentId.Focused)?1.4f:1)).ToString("0.#")+"% Lifesteal against marked foes for "+(4*Effect(item)).ToString("0.#")+"s. "+rules;
                case "reclaimer":return "+"+rate.ToString("0")+"% global Lifesteal; armor contribution doubles to "+(rate*2).ToString("0")+"% below half health. "+rules;
                case "predator":return "Actual healing stores +"+(item.def.effect.Power(item.rank)*100).ToString("0.#")+"% speed for the next ability. One charge, no stacking; weapons and ultimates do not consume it.";
                case "harvest":return "Drain "+(20*Effect(item)).ToString("0")+"% of actual damage; up to "+HarvestHealingLimit(item).ToString("0.#")+" HP ("+(8+item.rank-1)+"% max) per pulse. 16s recharge; no healing from overkill or corpses.";
                default:return "";
            }
        }
    }
}
