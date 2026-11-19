using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        // Reuse the originating turn's proc/hit/healing budgets. No new saved charge state.
        void ConstellationSource(LoadoutItem host,Action effect)
        {
            var saved=currentCredit;currentCredit=new KillCredit{item=host,action=saved?.action??actionSerial,derived=true};
            try{effect();}finally{currentCredit=saved;}
        }
        void ConstellationCue(LoadoutItem host,string key,string detail)
        {Note(host.def.name+" payoff");SynergyCue(key,host.def.name.ToUpperInvariant(),detail);}
        void PatternBarrier(LoadoutItem host,float capacity,float seconds)
        {
            barriers.RemoveAll(b=>b.host==host);AddBarrier(host,capacity,seconds);
        }
        void ExpansionContact(Enemy e,float damage,bool wasStaggered)
        {
            bool alive=e.deathAge<0&&e.health>0;var weapon=currentCredit.weapon;
            if(weapon!=null)
            {
                string key="pattern-weapon:"+Deck.Items.IndexOf(weapon);float s=Effect(weapon);
                if(weapon.def.id=="pileknuckles"&&alive&&wasStaggered&&Once(key)){Launch(e,null,0,.6f*s);ConstellationCue(weapon,"launch","Prior stagger -> punch launch");}
                if(weapon.def.id=="arccarbine"&&alive&&marks.TryGetValue(e.id,out var mark)&&mark.until>Elapsed&&Once(key)){float before=e.health;if(ConsumeMark(e)&&Deck.Equipped(Signature.Grounding) is LoadoutItem ground&&Once("ground"))AddBarrier(ground,Mathf.Min(ground.def.effect.Power(ground.rank),Mathf.Max(0,before-e.health)*.2f*Effect(ground)),4);fx.Lightning(hero.RightHand.position,e.physics.Center,Color.cyan);ConstellationCue(weapon,"mark","Piercing shot -> Conductive spent");}
                if(weapon.def.id=="rivetsabre"&&alive&&e.health<e.maxHealth*.5f&&Once(key)){DerivedHit(e,8*s,Vector3.zero,.1f,true);fx.Sweep(e.rig.Root.position,hero.Root.forward,.8f,65,Color.white,.3f);ConstellationCue(weapon,"sever","Wounded survivor -> follow-up cut");}
                if(weapon.def.id=="letterpistol"&&Ammo(weapon)==LoadoutCatalog.Magazine(weapon)-1&&Once(key)){Pulse(e.physics.Center,1.8f+.15f*(weapon.rank-1),.3f*s,3);ConstellationCue(weapon,"stagger","First magazine shot -> stagger burst");}
                if(weapon.def.id=="bastionmaul"&&Once(key)){PatternBarrier(weapon,6*s,3);ConstellationCue(weapon,"guard","Maul contact -> barrier");}
                if(LoadoutCatalog.Magazine(weapon)>0)
                {
                    if(Vector3.Distance(e.rig.Root.position,HeroPosition)>=5&&Deck.Equipped(Signature.BallisticVest) is LoadoutItem vest&&Once("ballistic-vest"))ConstellationSource(vest,()=>{PatternBarrier(vest,vest.def.effect.Power(vest.rank),vest.def.effect.Seconds(vest.rank));ConstellationCue(vest,"guard","Distant gun hit -> barrier");});
                    if(alive&&wasStaggered&&Deck.Equipped(Signature.Rangefinder) is LoadoutItem relay&&Once("rangefinder"))ConstellationSource(relay,()=>{DerivedHit(e,relay.def.effect.Power(relay.rank),Vector3.zero,.05f);fx.Arc(hero.RightHand.position,e.physics.Center,ConstellationContent.ColorFor(4));ConstellationCue(relay,"stagger","Staggered target -> focused gun hit");});
                }
            }
            if(e.deathAge<0&&e.health>0&&e.health<e.maxHealth*.5f)
            {
                if(Deck.Equipped(Signature.ExecutionHood) is LoadoutItem hood&&Once("execution-hood"))ConstellationSource(hood,()=>{DerivedHit(e,hood.def.effect.Power(hood.rank),Vector3.zero,.1f,true);fx.Sweep(e.rig.Root.position,hero.Root.forward,1,70,Color.white,.25f);ConstellationCue(hood,"sever","Wounded survivor -> cutting hit");});
                if(Deck.Equipped(Signature.MercyLens) is LoadoutItem mercy&&Once("mercy-lens"))ConstellationSource(mercy,()=>{DrainMark(e,mercy);ConstellationCue(mercy,"siphon","Wounded survivor -> Siphon mark");});
            }
        }
        void ExpansionLaunch()
        {if(Deck.Equipped(Signature.Counterweight) is LoadoutItem host&&Once("counterweight"))ConstellationSource(host,()=>{PatternBarrier(host,host.def.effect.Power(host.rank),host.def.effect.Seconds(host.rank));ConstellationCue(host,"guard","Launch -> barrier");});}
        void ExpansionCollision(Enemy target)
        {if(target.deathAge<0&&Deck.Equipped(Signature.ImpactLens) is LoadoutItem host&&Once("impact-lens"))ConstellationSource(host,()=>{DerivedHit(target,host.def.effect.Power(host.rank),Vector3.zero,.15f);fx.Ring(target.physics.Center,.8f,ConstellationContent.ColorFor(1),.25f);ConstellationCue(host,"collision","Body collision -> focused hit");});}
        void ExpansionDischarge(Enemy target)
        {
            if(Deck.Equipped(Signature.Switchboard) is LoadoutItem host&&Once("switchboard"))
            {
                var other=enemies.Where(e=>e!=target&&e.deathAge<0&&Reachable(e,16)&&Vector3.Distance(e.physics.Center,target.physics.Center)<host.def.effect.Radius(host.rank)).OrderBy(e=>Vector3.Distance(e.physics.Center,target.physics.Center)).FirstOrDefault();
                if(other!=null)ConstellationSource(host,()=>{MarkEnemy(other,host);fx.Lightning(target.physics.Center,other.physics.Center,Color.cyan);ConstellationCue(host,"mark","Discharge -> neighboring mark");});
            }
            if(Deck.Equipped(Signature.StormCapacitor) is LoadoutItem rig&&Once("storm-capacitor"))ConstellationSource(rig,()=>{Pulse(target.physics.Center,rig.def.effect.Radius(rig.rank),rig.def.effect.Seconds(rig.rank),3,target);ConstellationCue(rig,"stagger","Discharge -> stagger pulse");});
        }
        void ExpansionSever(Enemy target)
        {if(Deck.Equipped(Signature.SpurTreads) is LoadoutItem host&&Once("spur-treads"))ConstellationSource(host,()=>{Pulse(target.physics.Center,host.def.effect.Radius(host.rank),host.def.effect.Seconds(host.rank),3,target);ConstellationCue(host,"stagger","Sever -> stagger pulse");});}
        void ExpansionLoop()
        {
            if(!(Deck.Equipped(Signature.StaticShroud) is LoadoutItem host))return;
            ConstellationSource(host,()=>
            {
                bool marked=false;
                foreach(var e in enemies.Where(e=>Reachable(e,host.def.effect.Radius(host.rank))).OrderBy(e=>Vector3.Distance(e.physics.Center,HeroPosition)).Take(2+(host.rank-1)/2).ToArray())marked|=MarkEnemy(e,host);
                if(marked&&ConstellationContent.ShroudBarrier(host)>0)
                {
                    PatternBarrier(host,ConstellationContent.ShroudBarrier(host),3);
                    ConstellationCue(host,"guard","Loop marked a foe -> barrier refreshed");
                }
                fx.Ring(HeroPosition,host.def.effect.Radius(host.rank),Color.cyan,.4f);
            });
        }
        void ExpansionReload()
        {
            if(!(Deck.Equipped(Signature.VentGreaves) is LoadoutItem host)||!Once("vent-greaves"))return;
            ConstellationSource(host,()=>{float heal=Mathf.Min(MaxHealth-Health,host.def.effect.Power(host.rank));if(heal<=0)return;Health+=heal;ArmorHealing+=heal;ContributionFor(host).healing+=heal;ArmPredator();ExpansionHealing(heal);fx.Arc(hero.RightHand.position,HeroPosition+Vector3.up,Color.green);ConstellationCue(host,"siphon","Ammo replenished -> health restored");});
        }
        void ExpansionHealing(float heal)
        {if(heal>0)ObservePassiveEvent("healing");if(heal>0&&Deck.Equipped(Signature.Reservoir) is LoadoutItem host&&Once("reservoir"))ConstellationSource(host,()=>{PatternBarrier(host,Mathf.Min(heal,host.def.effect.Power(host.rank)),host.def.effect.Seconds(host.rank));ConstellationCue(host,"guard","Actual healing -> barrier");});}
        void ExpansionAbsorb(Enemy attacker)
        {
            if(attacker==null||attacker.deathAge>=0)return;
            if(Deck.Equipped(Signature.WardenVisor) is LoadoutItem host&&Once("warden-visor"))ConstellationSource(host,()=>{Control(attacker,host.def.effect.Seconds(host.rank),Vector3.zero);fx.Arc(HeroPosition+Vector3.up,attacker.physics.Center,ConstellationContent.ColorFor(6));ConstellationCue(host,"stagger","Absorption -> attacker interrupted");});
            if(Deck.Equipped(Signature.ReturnSpring) is LoadoutItem rig&&Once("return-spring"))ConstellationSource(rig,()=>{var aim=attacker.rig.Root.position-HeroPosition;aim.y=0;Control(attacker,0,aim.normalized*rig.def.effect.Power(rig.rank));fx.Corridor(HeroPosition,aim.normalized,3,.5f,ConstellationContent.ColorFor(6),.3f);ConstellationCue(rig,"guard","Absorption -> attacker pushed away");});
        }
        bool ResolveConstellationAbility(LoadoutItem host,AbilityCard card,Enemy target,float damage)
        {
            if(!ConstellationContent.IsNew(host.def.id))return false;
            float s=Effect(host);bool focused=host.HasAugment(AugmentId.Focused);var origin=HeroPosition;var aim=hero.Root.forward;var color=ConstellationContent.ColorFor(ConstellationContent.Branch(host.def.id));
            if(card.id==AbilityId.RepulsorGuard){bool discharge=host.HasAugment(AugmentId.Discharge);barriers.RemoveAll(b=>b.host==host);AddBarrier(host,18*ItemRarity.Power[host.rank],3*s,discharge,!discharge);return true;}
            if(target!=null){aim=target.rig.Root.position-origin;aim.y=0;aim.Normalize();}
            int cap=focused?1:card.id==AbilityId.VectorSling||card.id==AbilityId.BreachSalvo?3:card.id==AbilityId.StormLash?4:5;
            var victims=enemies.Where(e=>Reachable(e,card.range)&&(card.id==AbilityId.StormLash||card.id==AbilityId.BreachSalvo?InCorridor(e,origin,aim,card.range,1.25f):card.id==AbilityId.RazorFan?InCone(e,origin,aim,card.range,focused?15:35):true)).OrderBy(e=>e==target?-1:Vector3.Distance(e.physics.Center,origin)).Take(cap).ToArray();
            if(card.id==AbilityId.StormLash||card.id==AbilityId.BreachSalvo)fx.Corridor(origin,aim,card.range,1.25f,color,.5f);
            else if(card.id==AbilityId.RazorFan)fx.Sweep(origin,aim,card.range,focused?15:35,color,.4f);
            else fx.Ring(origin,card.range,color,.45f);
            var chain=new Cascade();foreach(var e in victims)
            {
                bool staggered=e.stun>0;float d=damage*(focused?1.4f:1);Vector3 impulse=Vector3.zero;bool marked=drains.TryGetValue(e.id,out var drain)&&drain.until>Elapsed;
                if(card.id==AbilityId.BreachSalvo&&staggered)d*=1.5f;
                if(card.id==AbilityId.ReclaimPulse&&marked)d*=1.6f;
                if(card.id==AbilityId.VectorSling){var point=origin+aim*2.8f;var pull=point-e.rig.Root.position;pull.y=0;impulse=pull.normalized*Mathf.Min(8,pull.magnitude*3)*s;SetSetup(e,host);fx.Arc(e.physics.Center,point+Vector3.up,color);}
                if(card.id==AbilityId.StormLash)MarkEnemy(e,host,focused?1.4f:1);
                Hurt(e,d,impulse,card.id==AbilityId.BreachSalvo?.15f:.05f,card.id==AbilityId.RazorFan);
                if(card.id==AbilityId.VectorSling&&e==target&&staggered)Launch(e,chain,0,.75f*s*(focused?1.4f:1));
                if(card.id==AbilityId.ReclaimPulse&&marked){drain.until=0;fx.Arc(e.physics.Center,origin+Vector3.up,SiphonColor);}
            }
            fx.Cue(card.id==AbilityId.StormLash?"electric":card.id==AbilityId.RazorFan?"slash":card.id==AbilityId.BreachSalvo?"gun":"force");return true;
        }
        bool ResolveConstellationUltimate(LoadoutItem host)
        {
            if(!ConstellationContent.IsNew(host.def.id))return false;var card=LoadoutCatalog.Action(host);float damage=card.damage*Deck.Damage,s=Effect(host);var aim=hero.Root.forward;var color=ConstellationContent.ColorFor(ConstellationContent.Branch(host.def.id));
            if(host.def.id=="lastbastion"){barriers.RemoveAll(b=>b.host==host);AddBarrier(host,45*ItemRarity.Power[host.rank],5*s,false,true);fx.Ring(HeroPosition,3,color,.7f);return true;}
            if(host.def.id=="bloodcircuit")
            {
                var hit=new System.Collections.Generic.HashSet<int>();var target=Target(card.range);var point=HeroPosition+Vector3.up;
                for(int i=0;i<6&&target!=null;i++){hit.Add(target.id);fx.Lightning(point,target.physics.Center,SiphonColor);DrainMark(target,host,2);Hurt(target,damage,Vector3.zero,.3f);point=target.physics.Center;target=enemies.Where(e=>!hit.Contains(e.id)&&Reachable(e,card.range+3)&&Vector3.Distance(e.physics.Center,point)<4.5f+.15f*(host.rank-1)).OrderBy(e=>Vector3.Distance(e.physics.Center,point)).FirstOrDefault();}return true;
            }
            float width=host.def.id=="guillotine"?1.6f:2;fx.Corridor(HeroPosition,aim,card.range,width,color,.8f);var chain=new Cascade();int n=0;
            foreach(var e in enemies.Where(e=>Reachable(e,card.range)&&InCorridor(e,HeroPosition,aim,card.range,width)).OrderBy(e=>Vector3.Distance(e.physics.Center,HeroPosition)).ToArray())
            {
                bool cut=host.def.id=="guillotine";Hurt(e,damage*(cut&&e.health<e.maxHealth*.5f?1.6f:1),Vector3.zero,.6f,cut);
                if(!cut&&n++<5)Launch(e,chain,0,1.1f*s,aim);
            }
            if(host.def.id=="freighttrain")foreach(var e in corpses.Where(e=>!chain.hit.Contains(e.id)&&InCorridor(e,HeroPosition,aim,card.range,width)).Take(3).ToArray())Launch(e,chain,0,.8f*s,aim);
            return true;
        }
    }
    public sealed partial class PrototypeHUD
    {
        string ConstellationRules(LoadoutItem item)
        {
            var d=item.def;float s=LoadoutCatalog.EffectScale(item);string rules=d.description+"\n"+d.tradeoff;
            string power=d.effect.Power(item.rank).ToString("0.##"),reach=d.effect.Radius(item.rank).ToString("0.##"),seconds=d.effect.Seconds(item.rank).ToString("0.##");
            switch(d.signature)
            {
                case Signature.ImpactLens:case Signature.ExecutionHood:case Signature.Rangefinder:rules+="\nAt this rarity: +"+power+" damage, once per action.";break;
                case Signature.Counterweight:case Signature.BallisticVest:rules+="\nAt this rarity: "+power+" barrier for "+seconds+"s; refreshes this item's barrier.";break;
                case Signature.Switchboard:rules+="\nOne neighbor within "+reach+"m; "+(10*ItemRarity.Power[item.rank]).ToString("0.#")+" discharge damage, "+(4*s).ToString("0.#")+"s mark.";break;
                case Signature.StaticShroud:rules+="\nAt this rarity: "+(2+(item.rank-1)/2)+" targets within "+reach+"m; "+(10*ItemRarity.Power[item.rank]).ToString("0.#")+" discharge damage, "+(4*s).ToString("0.#")+"s mark.\nMark at least one foe: "+ConstellationContent.ShroudBarrier(item).ToString("0.#")+" barrier for 3s. Refreshes; never stacks. Total barriers capped at 90.";break;
                case Signature.StormCapacitor:case Signature.SpurTreads:rules+="\nAt this rarity: stagger three neighbors for "+seconds+"s within "+reach+"m.";break;
                case Signature.VentGreaves:rules+="\nAt this rarity: "+power+" health per actual reload action; paired refills count once.";break;
                case Signature.MercyLens:rules+="\nAt this rarity: "+(100*LoadoutCatalog.SiphonRate(item.rank)).ToString("0")+"% Lifesteal mark for "+(4*s).ToString("0.#")+"s; normal shared healing limit.";break;
                case Signature.Reservoir:rules+="\nAt this rarity: up to "+power+" barrier from the first heal each action, lasting "+seconds+"s.";break;
                case Signature.WardenVisor:rules+="\nAt this rarity: "+seconds+"s attacker stagger; heavy enemies resist control.";break;
                case Signature.ReturnSpring:rules+="\nAt this rarity: "+power+" shove force; heavy enemies resist control.";break;
            }
            if(d.id=="RepulsorGuard")rules+="\n"+(18*ItemRarity.Power[item.rank]).ToString("0.#")+" barrier / "+(3*s).ToString("0.#")+"s. Total barriers capped at 90.";
            if(d.id=="StormLash")rules+="\nConductive: "+(10*ItemRarity.Power[item.rank]*(item.HasAugment(AugmentId.Focused)?1.4f:1)).ToString("0.#")+" discharge damage for "+(4*s).ToString("0.#")+"s. Corridor is 2.5m wide.";
            if(d.id=="VectorSling")rules+="\nPrimary launch impulse "+(.75f*s*(item.HasAugment(AugmentId.Focused)?1.4f:1)).ToString("0.00")+"x; gathering alone does not satisfy prior stagger.";
            if(d.id=="BreachSalvo")rules+="\nCorridor is 2.5m wide; prior stagger multiplies damage by 1.5.";
            if(d.kind==ItemKind.Ability&&item.HasAugment(AugmentId.Focused))rules+="\nFOCUSED: one target, 40% more opening damage; 40% stronger Sling launch or Lash mark where applicable.";
            if(d.id=="lastbastion")rules+="\n"+Mathf.Min(90,45*ItemRarity.Power[item.rank]).ToString("0.#")+" barrier / "+(5*s).ToString("0.#")+"s. Total barriers capped at 90.";
            if(d.id=="pileknuckles")rules+="\nLaunch impulse "+(.6f*s).ToString("0.00")+"x, once per owned host per action.";
            if(d.id=="rivetsabre")rules+="\nFollow-up cut "+(8*s).ToString("0.#")+" damage, once per owned host per action.";
            if(d.id=="bastionmaul")rules+="\nBarrier "+(6*s).ToString("0.#")+" for 3s, once per shared two-handed host.";
            if(d.id=="leechgrips")rules+="\n"+(100*LoadoutCatalog.SiphonRate(item.rank)).ToString("0")+"% Lifesteal mark for "+(4*s).ToString("0.#")+"s; actual damage heals within the shared 1% action cap.";
            if(d.id=="letterpistol")rules+="\nFirst round: up to three foes within "+(1.8f+.15f*(item.rank-1)).ToString("0.#")+"m staggered for "+(.3f*s).ToString("0.##")+"s.";
            if(d.id=="arccarbine")rules+="\nDischarge strength comes from the consumed mark. One discharge per shared two-handed host per action.";
            if(d.kind==ItemKind.Weapon)rules+="\n"+(d.profile.style==WeaponStyle.Kinetic?"Uncommon: two contacts. Rare+: a stagger shockwave.":d.profile.style==WeaponStyle.Rifle?"Pierces up to "+Mathf.Min(4,item.rank+1)+" targets. Rare+: launch the last pierced target.":d.profile.style==WeaponStyle.Impact?"Rare+: launch up to three targets sharing one collision chain.":d.profile.style==WeaponStyle.Pistol?"Ricochets: "+Mathf.Min(2,item.rank-1)+"; later hits deal 65% damage.":"Rare+: one severed target launches each action.");
            if(item.HasAugment(AugmentId.Discharge))rules+="\nDISCHARGE: replaces repulsion. Burst deals 55% of absorption, capped at "+(30*s).ToString("0.#")+", to five targets in a 4m cone.";
            return rules;
        }
    }
}
