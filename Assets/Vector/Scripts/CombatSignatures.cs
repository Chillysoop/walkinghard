using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] public sealed class Contribution {public float direct,collision,control,absorption,reload,preparation,rangeWait,augment;public int assists;}
        readonly Dictionary<LoadoutItem,Contribution> contributions=new Dictionary<LoadoutItem,Contribution>();
        public Contribution ContributionFor(LoadoutItem item){item=KillOwner(item);if(item==null)return new Contribution();if(!contributions.TryGetValue(item,out var c))contributions[item]=c=new Contribution();return c;}
        sealed class Turn {public readonly HashSet<string> procs=new HashSet<string>();public readonly HashSet<int> hits=new HashSet<int>();public readonly List<Enemy> orderedHits=new List<Enemy>();public int kills;public Enemy last;public int extra=12;}
        readonly Dictionary<int,Turn> turns=new Dictionary<int,Turn>();
        Turn CurrentTurn {get {int id=currentCredit?.action??actionSerial;if(!turns.TryGetValue(id,out var t))turns[id]=t=new Turn();return t;}}
        bool Once(string key)=>CurrentTurn.procs.Add(key);
        LoadoutItem Owner=>currentCredit?.weapon??currentCredit?.item;
        readonly HashSet<LoadoutItem> breachArmed=new HashSet<LoadoutItem>();
        float followBank,counterBank,amplifierBank,tempo,momentumUntil,reactiveDue;
        LoadoutItem followOwner,counterOwner,salvagerOwner;bool guardArmed;int redlineTurns;
        sealed class Barrier {public LoadoutItem host;public float left,absorbed,expires;public bool discharge,reactive;public KillCredit credit;}
        readonly List<Barrier> barriers=new List<Barrier>();
        sealed class Mark {public float until,power;public LoadoutItem owner;public int consumed=-1;}
        readonly Dictionary<int,Mark> marks=new Dictionary<int,Mark>();
        sealed class Setup {public LoadoutItem host;public float until;}
        readonly Dictionary<int,Setup> setups=new Dictionary<int,Setup>();
        public readonly Dictionary<string,int> InteractionNotes=new Dictionary<string,int>();
        public string RecentPayoff {get;private set;}="";float payoffUntil;
        public string VisiblePayoff=>Elapsed<payoffUntil?RecentPayoff:"";
        void Note(string text,bool flash=false){InteractionNotes[text]=InteractionNotes.TryGetValue(text,out int count)?count+1:1;if(flash){RecentPayoff=text;payoffUntil=Elapsed+.7f;}}
        void SetSetup(Enemy e,LoadoutItem host){setups[e.id]=new Setup{host=host,until=Elapsed+3};}
        sealed class Field {public LoadoutItem host;public KillCredit credit;public Vector3 origin,aim,point;public float left=2,tick;public readonly HashSet<int> hit=new HashSet<int>();}
        readonly List<Field> fields=new List<Field>();
        sealed class AnchorCheck {public Enemy enemy;public Vector3 before;public float left=.3f,stagger;public KillCredit credit;}
        readonly List<AnchorCheck> anchors=new List<AnchorCheck>();
        sealed class Mote {public Vector3 point;public float left=10,age,heal;public LoadoutItem owner;}
        readonly List<Mote> motes=new List<Mote>();
        public float BarrierAmount=>barriers.Sum(b=>b.left);
        public string ArmedState=>string.Join(" · ",new[]{guardArmed?"GUARD READY":"",followBank>0?"FOLLOW-THROUGH":"",counterBank>0?"COUNTER BANK":"",amplifierBank>0?"AMPLIFIED":"",tempo>0?"TEMPO +"+(tempo*100).ToString("0")+"%":"",redlineTurns>0?"REDLINE "+redlineTurns:"",momentumUntil>Elapsed?"QUICK ABILITY":"",salvagerOwner!=null?"RETRIEVAL READY":""}.Where(s=>s.Length>0));
        public bool BreachArmed(LoadoutItem host)=>host!=null&&breachArmed.Contains(host);
        void ResetSignatures(){contributions.Clear();InteractionNotes.Clear();RecentPayoff="";payoffUntil=0;turns.Clear();marks.Clear();setups.Clear();barriers.Clear();fields.Clear();anchors.Clear();motes.Clear();breachArmed.Clear();followBank=counterBank=amplifierBank=tempo=momentumUntil=reactiveDue=duelistUntil=0;followOwner=counterOwner=salvagerOwner=null;guardArmed=false;redlineTurns=0;}
        float Effect(LoadoutItem item)=>LoadoutCatalog.EffectScale(item);
        float ControlResistance(Enemy e)=>e.boss?(WaveElapsed%8<2.5f?1:.22f):e.heavy?.65f:1;
        void Control(Enemy e,float seconds,Vector3 impulse)
        {
            if(e.deathAge>=0)return;float r=ControlResistance(e);float effective=seconds*r;
            ContributionFor(Owner).control+=Mathf.Max(0,effective-e.stun);e.stun=Mathf.Max(e.stun,effective);if(effective>0){e.attack=-1;e.engaged=false;}
            e.physics.Impulse(impulse*r);
        }
        void MarkEnemy(Enemy e,LoadoutItem source,float potency=1)
        {
            if(e.deathAge>=0)return;if(!marks.TryGetValue(e.id,out var m))marks[e.id]=m=new Mark();
            if(m.consumed==(currentCredit?.action??actionSerial))return;
            m.until=Elapsed+4*Effect(source);m.power=10*ItemRarity.Power[source.rank]*potency;m.owner=source;
            fx.Symbol(e.physics.Center+Vector3.up*.9f,VisualFactory.Cyan,.3f,true);
        }
        bool ConsumeMark(Enemy e)
        {
            if(!marks.TryGetValue(e.id,out var m)||m.until<=Elapsed||m.consumed==(currentCredit?.action??actionSerial))return false;
            Note((currentCredit?.hop??0)>0?"Body impact consumed Conductive":"Lightning consumed Conductive");m.until=0;m.consumed=currentCredit?.action??actionSerial;ContributionFor(m.owner).assists++;
            fx.Symbol(e.physics.Center,Color.white,.3f,false);DerivedHit(e,m.power,Vector3.zero,.1f);return true;
        }
        void ElectricHit(Enemy e,float damage,float stagger){ConsumeMark(e);Hurt(e,damage,Vector3.zero,stagger);if(Deck.Equipped(Signature.Grounding) is LoadoutItem g&&Once("ground"))AddBarrier(g,Mathf.Min(g.def.effect.Power(g.rank),damage*.2f*Effect(g)),4);}
        void DerivedHit(Enemy e,float damage,Vector3 impulse,float stagger,bool cut=false,bool aug=false)
        {
            if(CurrentTurn.extra<=0)return;CurrentTurn.extra--;bool previous=currentCredit.derived,wasAug=currentCredit.augment;currentCredit.derived=true;currentCredit.augment=aug;
            Hurt(e,damage,impulse,stagger,cut);currentCredit.derived=previous;currentCredit.augment=wasAug;
        }
        void AddBarrier(LoadoutItem host,float capacity,float duration,bool discharge=false,bool reactive=false)
        {
            capacity=Mathf.Min(capacity,90-BarrierAmount);if(capacity<=0)return;
            barriers.Add(new Barrier{host=host,left=capacity,expires=Elapsed+duration,discharge=discharge,reactive=reactive,credit=currentCredit??new KillCredit{item=host,action=actionSerial}});
            fx.Shape(HeroPosition,Vector3.forward,1.1f,180,Color.cyan,.4f);
        }
        void EndBarrier(Barrier b)
        {
            var saved=currentCredit;currentCredit=b.credit;
            if(b.host.def.ability?.id==AbilityId.Counterguard&&b.absorbed>0)
            {
                if(b.discharge){float damage=Mathf.Min(30*Effect(b.host),b.absorbed*.55f);int n=0;fx.Shape(HeroPosition,hero.Root.forward,4,65,Color.magenta,.35f);foreach(var e in enemies.ToArray())if(n<5&&InCone(e,HeroPosition,hero.Root.forward,4,65)){DerivedHit(e,damage,hero.Root.forward*2,.2f,false,true);n++;}}
                else {counterBank=Mathf.Min(45*Effect(b.host),b.absorbed*.8f);counterOwner=b.host;}
            }
            if(b.reactive){fx.Shape(HeroPosition,Vector3.forward,3,180,Color.cyan,.3f);foreach(var e in enemies.ToArray())if(Reachable(e,3))Control(e,.25f,(e.physics.Center-HeroPosition).normalized*4*Effect(b.host));}
            currentCredit=saved;
        }
        void ReceiveDamage(Enemy attacker,float raw)
        {
            float damage=raw*Deck.DamageTaken;ArmorPreventedDamage+=raw-damage;
            if(guardArmed){guardArmed=false;var g=Deck.Equipped(Signature.Bulwark);if(g!=null){float blocked=Mathf.Min(damage,g.def.effect.Power(g.rank));damage-=blocked;ContributionFor(g).absorption+=blocked;var saved=currentCredit;currentCredit=new KillCredit{item=g,action=actionSerial};Pulse(HeroPosition,g.def.effect.Radius(g.rank),g.def.effect.Seconds(g.rank),5);currentCredit=saved;}}
            foreach(var b in barriers.ToArray())
            {
                float absorbed=Mathf.Min(damage,b.left);b.left-=absorbed;b.absorbed+=absorbed;damage-=absorbed;ContributionFor(b.host).absorption+=absorbed;
                if(absorbed>0&&Deck.Equipped(Signature.Grounding) is LoadoutItem g)MarkEnemy(attacker,g);
                if(b.left<=0){barriers.Remove(b);EndBarrier(b);}if(damage<=0)break;
            }
            if(duelistUntil>Elapsed&&Deck.Equipped(Signature.Duelist) is LoadoutItem d){float prevent=damage*Mathf.Min(.65f,d.def.effect.Power(d.rank));damage-=prevent;ContributionFor(d).absorption+=prevent;duelistUntil=0;var saved=currentCredit;currentCredit=new KillCredit{item=d,action=actionSerial};Control(attacker,.4f*Effect(d),Vector3.zero);currentCredit=saved;}
            Health-=damage;HitsTaken++;heroReact=.8f;flash=damage>0?.3f:.08f;fx.Play(3);
        }
        float duelistUntil;
        void Pulse(Vector3 point,float radius,float stagger,int cap,Enemy exclude=null)
        {
            fx.Shape(point,Vector3.forward,radius,180,VisualFactory.Cyan,.3f);int n=0;
            foreach(var e in enemies.ToArray())if(e!=exclude&&n<cap&&Vector3.Distance(e.physics.Center,point)<radius){Control(e,stagger,Vector3.zero);n++;}
        }
        bool InCone(Enemy e,Vector3 origin,Vector3 direction,float range,float halfArc)
        {var d=e.rig.Root.position-origin;d.y=0;return !e.entering&&d.magnitude<=range&&Vector3.Angle(direction,d)<=halfArc;}
        bool InCorridor(Enemy e,Vector3 origin,Vector3 direction,float length,float width)
        {var d=e.rig.Root.position-origin;d.y=0;float along=Vector3.Dot(d,direction);return !e.entering&&along>=0&&along<=length&&(d-direction*along).magnitude<=width;}
        void DirectSignatures(Enemy e,float damage,bool wasStaggered)
        {
            if(currentCredit==null||currentCredit.derived||currentCredit.hop>0)return;
            var t=CurrentTurn;if(setups.TryGetValue(e.id,out var setup)&&setup.until>Elapsed&&setup.host!=Owner&&Once("setup:"+e.id)){ContributionFor(setup.host).assists++;Note("Gather / pull enabled a follow-up hit");}
            if(t.hits.Add(e.id))t.orderedHits.Add(e);t.last=e;
            if(wasStaggered&&Deck.Equipped(Signature.Tactician) is LoadoutItem tact&&Once("tactician")){var other=enemies.FirstOrDefault(x=>x!=e&&Vector3.Distance(x.physics.Center,e.physics.Center)<tact.def.effect.Radius(tact.rank));if(other!=null){Control(other,tact.def.effect.Seconds(tact.rank),Vector3.zero);fx.Arc(e.physics.Center,other.physics.Center,VisualFactory.Cyan);}}
            if(wasStaggered&&Deck.Equipped(Signature.Capacitor) is LoadoutItem cap&&Once("capacitor"))MarkEnemy(e,cap);
            if(currentCredit.weapon!=null&&Once("basic-payoff"))
            {
                if(counterBank>0)Note("Counterguard empowered a weapon",true);if(amplifierBank>0)Note("Amplifier charge spent",true);float bank=counterBank+amplifierBank;float burstRadius=Deck.Equipped(Signature.Amplifier) is LoadoutItem amplifier?amplifier.def.effect.Radius(amplifier.rank):2.1f;var point=e.physics.Center;counterBank=amplifierBank=0;
                if(bank>0){fx.Shape(point,Vector3.forward,burstRadius,180,VisualFactory.Orange,.3f);foreach(var victim in enemies.ToArray().Where(x=>Vector3.Distance(x.physics.Center,point)<burstRadius).Take(5))DerivedHit(victim,bank,Vector3.zero,.15f);if(counterOwner!=null)ContributionFor(counterOwner).assists++;if(Deck.Equipped(Signature.Amplifier) is LoadoutItem a)ContributionFor(a).assists++;}
                if(followBank>0){Note("Follow-through pulse spent",true);Pulse(point,2.2f,followBank,5);followBank=0;if(followOwner!=null)ContributionFor(followOwner).assists++;}
                if(salvagerOwner!=null){var body=corpses.Where(x=>Vector3.Distance(x.physics.Center,point)<salvagerOwner.def.effect.Radius(salvagerOwner.rank)).OrderBy(x=>Vector3.Distance(x.physics.Center,point)).FirstOrDefault();if(body!=null){var direction=point-body.physics.Center;direction.y=0;Launch(body,null,0,.6f*Effect(salvagerOwner),direction.normalized);fx.Arc(body.physics.Center,point,VisualFactory.Orange);ContributionFor(salvagerOwner).assists++;}salvagerOwner=null;}
            }
        }
        void DeathSignatures(Enemy e,bool sever)
        {
            if(currentCredit==null)return;var t=CurrentTurn;t.kills++;

            if(Deck.Equipped(Signature.Recovery) is LoadoutItem recoveryItem&&Once("recovery")&&motes.Count<24)motes.Add(new Mote{point=e.physics.Center,heal=recoveryItem.def.effect.Power(recoveryItem.rank),owner=recoveryItem});
            if(t.kills>=2&&Deck.Equipped(Signature.Arcstep) is LoadoutItem arc&&Once("arcstep"))foreach(var victim in enemies.Where(x=>Reachable(x,arc.def.effect.Radius(arc.rank))).Take(2+(arc.rank-1)/2))MarkEnemy(victim,arc);
            if(!sever)return;
            if(Deck.Equipped(Signature.Duelist) is LoadoutItem d)duelistUntil=Elapsed+d.def.effect.Seconds(d.rank);
            salvagerOwner=Deck.Equipped(Signature.Salvager)??salvagerOwner;
            if(Deck.Equipped(Signature.Salvage) is LoadoutItem magnet&&Once("salvage")){var chain=salvageChain??(salvageChain=new Cascade());if(chain.remaining>0)Launch(e,chain,0,.7f+.1f*(magnet.rank-1));}
        }
        Cascade salvageChain;
        void OnLaunch(Enemy body)
        {
            if(Deck.Equipped(Signature.Momentum) is LoadoutItem boots)momentumUntil=Elapsed+boots.def.effect.Seconds(boots.rank);
            salvagerOwner=Deck.Equipped(Signature.Salvager)??salvagerOwner;
        }
        void OnCollision(Enemy target,Vector3 point,float damage)
        {
            if(Deck.Equipped(Signature.Grounding) is LoadoutItem g&&Once("ground"))AddBarrier(g,Mathf.Min(g.def.effect.Power(g.rank),damage*.2f*Effect(g)),4);
            if(Deck.Equipped(Signature.FaultLine) is LoadoutItem fault&&Once("faultline"))Pulse(point,fault.def.effect.Radius(fault.rank),fault.def.effect.Seconds(fault.rank),4,target);
            if(Deck.Equipped(Signature.Relay) is LoadoutItem relay&&ConsumeMark(target))
            {
                int cap=1+(relay.rank>=3?1:0)+(relay.rank==5?1:0);
                foreach(var other in enemies.Where(e=>e!=target&&Vector3.Distance(e.physics.Center,point)<relay.def.effect.Radius(relay.rank)).Take(cap).ToArray()){fx.Arc(point,other.physics.Center,Color.cyan);DerivedHit(other,relay.def.effect.Power(relay.rank),Vector3.zero,.15f);}
            }
        }
        float TurnSpeed(bool ultimate,LoadoutItem host)
        {
            if(ultimate)return 1;float haste=tempo+(redlineTurns>0?.2f*Effect(Deck.Ultimate):0);
            if(host.def.kind==ItemKind.Ability&&momentumUntil>Elapsed&&Deck.Equipped(Signature.Momentum) is LoadoutItem m){haste+=Mathf.Min(.4f,m.def.effect.Power(m.rank));momentumUntil=0;}
            return Deck.Speed*(1+Mathf.Min(.85f,haste));
        }
        void FinishTurn()
        {
            if(ActiveSlot==5)return;
            currentCredit=new KillCredit{item=actionItem,weapon=actionWeapon,action=actionSerial};
            if(actionWeapon==null&&Deck.Equipped(Signature.Amplifier) is LoadoutItem amp)amplifierBank=Mathf.Min(amp.def.effect.Power(amp.rank)*3,amplifierBank+amp.def.effect.Power(amp.rank));
            var last=CurrentTurn.orderedHits.LastOrDefault(e=>e.deathAge<0);
            if(CurrentTurn.hits.Count>=2&&last!=null&&Deck.Equipped(Signature.Surveyor) is LoadoutItem survey&&Once("surveyor")){fx.Arc(hero.Root.position+Vector3.up,last.physics.Center,Color.white);var point=last.physics.Center;DerivedHit(last,survey.def.effect.Power(survey.rank),Vector3.zero,.1f);float radius=survey.def.effect.Radius(survey.rank);fx.Shape(point,Vector3.forward,radius,180,Color.white,.25f);foreach(var e in enemies.Where(e=>e!=last&&Vector3.Distance(e.physics.Center,point)<radius).Take(2).ToArray())DerivedHit(e,survey.def.effect.Power(survey.rank)*.35f,Vector3.zero,0);ContributionFor(survey).assists++;}
            bool wrapped=PairSlot>=0?PairSlot<ActiveSlot||PairSlot==4:ActiveSlot==4;
            // Empty trailing positions also complete a loop.
            int next=Deck.NextOccupied(((PairSlot>=0?PairSlot:ActiveSlot)+1)%5);wrapped|=next<=ActiveSlot;
            if(wrapped){tempo=0;guardArmed=Deck.Equipped(Signature.Bulwark)!=null;}
            else if(Deck.Equipped(Signature.Overclock) is LoadoutItem over)tempo=Mathf.Min(Mathf.Min(.4f,.25f+.04f*(over.rank-1)),tempo+over.def.effect.Power(over.rank));
            if(redlineTurns>0)redlineTurns--;
        }
        float ReloadDuration(LoadoutItem host)=>(host.HasAugment(AugmentId.BreachLoad)?1.5f:1.2f)*LoaderFactor*BalanceOverrides.Reload(host.def.id);
        float LoaderFactor=>Deck.Equipped(Signature.Loader) is LoadoutItem l?1-Mathf.Min(.4f,l.def.effect.Power(l.rank)):1;
        void Replenish(LoadoutItem host)
        {
            int max=LoadoutCatalog.Magazine(host);if(max<=0||Ammo(host)>=max)return;ammunition[host]=max;if(host.HasAugment(AugmentId.BreachLoad)){breachArmed.Add(host);Note("Reload armed Breach Load",true);}
            if(Deck.Equipped(Signature.Loader) is LoadoutItem loader&&Once("loader"))Pulse(HeroPosition,loader.def.effect.Radius(loader.rank),loader.def.effect.Seconds(loader.rank),5);
            if(Deck.Equipped(Signature.Duelist) is LoadoutItem d)duelistUntil=Elapsed+d.def.effect.Seconds(d.rank);
        }
        void StepSignatures(float dt)
        {
            var saved=currentCredit;
            foreach(var b in barriers.ToArray())if(b.expires<=Elapsed){barriers.Remove(b);EndBarrier(b);}
            if(Deck.Equipped(Signature.Reactive) is LoadoutItem shell&&Elapsed>=reactiveDue&&enemies.Count(e=>Reachable(e,3))>=3){reactiveDue=Elapsed+8;currentCredit=new KillCredit{item=shell,action=actionSerial};AddBarrier(shell,shell.def.effect.Power(shell.rank),3,false,true);}
            if((int)(Elapsed*5)!=(int)((Elapsed-dt)*5))
            {
                foreach(var e in enemies)if(marks.TryGetValue(e.id,out var mark)&&mark.until>Elapsed)fx.Symbol(e.physics.Center+Vector3.up*.9f,Color.cyan,.24f,true);
                if(BarrierAmount>0)fx.Shape(HeroPosition,Vector3.forward,1.1f,180,Color.cyan,.24f);
                foreach(var m in motes)fx.Symbol(m.point,Color.green,.24f,false);
            }
            foreach(var a in anchors.ToArray()){a.left-=dt;if(a.left>0)continue;anchors.Remove(a);if(a.enemy.deathAge<0&&Vector3.Distance(a.before,a.enemy.rig.Root.position)>=.25f){currentCredit=a.credit;Control(a.enemy,a.stagger,Vector3.zero);fx.Symbol(a.enemy.physics.Center,Color.white,.3f,false);}}
            foreach(var f in fields.ToArray())
            {
                f.left-=dt;f.tick-=dt;if(f.left<=0){fields.Remove(f);continue;}if(f.tick>0)continue;f.tick=.4f;currentCredit=f.credit;
                bool corridor=f.host.def.ability.id==AbilityId.VectorTether;float width=1.8f+.1f*(f.host.rank-1);
                if(corridor)fx.Corridor(f.origin,f.aim,8,width,VisualFactory.Cyan,.4f);else fx.Shape(f.point,Vector3.forward,2.8f,180,VisualFactory.Cyan,.4f);
                foreach(var e in enemies.ToArray())if((f.hit.Contains(e.id)||f.hit.Count<6)&&(corridor?InCorridor(e,f.origin,f.aim,8,width):Vector3.Distance(e.rig.Root.position,f.point)<2.8f))
                {f.hit.Add(e.id);if(f.host.def.ability.id==AbilityId.ConductiveBrand)MarkEnemy(e,f.host,.65f);else {var target=corridor?f.origin+f.aim*Mathf.Clamp(Vector3.Dot(e.rig.Root.position-f.origin,f.aim),1,8):f.point;Control(e,0,(target-e.rig.Root.position).normalized*2*Effect(f.host));SetSetup(e,f.host);}}
            }
            foreach(var m in motes.ToArray()){m.left-=dt;m.age+=dt;if(m.age>.3f&&Vector3.Distance(m.point,HeroPosition)<m.owner.def.effect.Radius(m.owner.rank)){float heal=Mathf.Min(MaxHealth-Health,m.heal);Health+=heal;ArmorHealing+=heal;fx.Arc(m.point,HeroPosition+Vector3.up,Color.green);fx.Symbol(HeroPosition+Vector3.up*1.8f,Color.green,.5f,false);ContributionFor(m.owner).absorption+=heal;motes.Remove(m);}else if(m.left<=0)motes.Remove(m);}
            currentCredit=saved;
        }
    }
}
