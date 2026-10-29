using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        float nativeDuration,turnSpeed;int extensionStep;bool borePending;
        readonly Dictionary<Enemy,Vector3> returnPoints=new Dictionary<Enemy,Vector3>();
        readonly Dictionary<Enemy,Vector3> backwash=new Dictionary<Enemy,Vector3>();
        readonly Dictionary<LoadoutItem,float> reloadTimers=new Dictionary<LoadoutItem,float>();
        float Share(LoadoutItem host)=>PairSlot<0||Deck.TwoHanded||actionWeapon==pairWeapon?1:.5f;
        bool SharedSecond(LoadoutItem host)=>PairSlot>=0&&!Deck.TwoHanded&&actionWeapon==pairWeapon&&currentCredit.item==pairItem;
        void BeginAuthoredTurn(bool ultimate)
        {
            currentCredit=new KillCredit{item=ultimate?Deck.Ultimate:actionItem,weapon=actionWeapon,action=actionSerial};salvageChain=null;
            turnSpeed=TurnSpeed(ultimate,actionWeapon??(ultimate?Deck.Ultimate:actionItem));nativeDuration=Mathf.Max(activeCard.duration,pairCard?.duration??0)/turnSpeed;
            float extension=Mathf.Max(AugmentCatalog.Extension(actionWeapon??actionItem),Deck.TwoHanded?0:AugmentCatalog.Extension(pairWeapon));
            actionDuration=nativeDuration+extension/turnSpeed;extensionStep=0;backwash.Clear();returnPoints.Clear();reloadTimers.Clear();
            borePending=actionWeapon?.HasAugment(AugmentId.BoreRounds)==true||pairWeapon?.HasAugment(AugmentId.BoreRounds)==true;
        }
        void AdvanceExtension()
        {
            if(actionAge<nativeDuration||actionDuration<=nativeDuration)return;
            float progress=(actionAge-nativeDuration)/(actionDuration-nativeDuration);
            int step=progress>=.7f?2:progress>=.2f?1:0;if(step<=extensionStep)return;extensionStep=step;
            if(actionWeapon==null)
            {
                currentCredit=new KillCredit{item=actionItem,action=actionSerial,augment=true};
                if(actionItem.HasAugment(AugmentId.Echo)&&step==1&&actionTarget?.deathAge<0&&Reachable(actionTarget,activeCard.range)){fx.Arc(hero.RightHand.position,actionTarget.physics.Center,Color.magenta);fx.FollowupContact(actionTarget.physics.Center,true);fx.Burst(actionTarget.physics.Center,Color.magenta,3,1);DerivedHit(actionTarget,activeCard.damage*.35f*Effect(actionItem)*Deck.Damage,Vector3.zero,.1f,activeCard.id==AbilityId.ReapingCut,true);}

                return;
            }
            WeaponExtension(actionWeapon,actionItem,activeCard,actionTarget,step);
            if(PairSlot>=0&&!Deck.TwoHanded&&pairWeapon!=actionWeapon)WeaponExtension(pairWeapon,pairItem,pairCard,pairTarget,step);
        }
        void StepBackwash(float dt)
        {
            if(actionWeapon!=null||actionItem?.HasAugment(AugmentId.Backwash)!=true||actionAge<nativeDuration)return;
            foreach(var pair in backwash){var e=pair.Key;if(e.deathAge>=0)continue;if(!returnPoints.TryGetValue(e,out var to)){to=Vector3.Lerp(e.rig.Root.position,pair.Value,.45f);returnPoints[e]=to;fx.Arc(e.physics.Center,to+Vector3.up,Color.magenta);}
                Vector3 delta=to-e.rig.Root.position;delta.y=0;float remaining=Mathf.Max(dt,actionDuration-actionAge);Vector3 velocity=Vector3.ClampMagnitude(delta/remaining,7*ControlResistance(e));
                e.physics.LinearVelocity=new Vector3(actionAge>=actionDuration?0:velocity.x,e.physics.LinearVelocity.y,actionAge>=actionDuration?0:velocity.z);e.stun=Mathf.Max(e.stun,remaining);}
        }
        void WeaponExtension(LoadoutItem host,LoadoutItem basic,AbilityCard card,Enemy target,int step)
        {
            currentCredit=new KillCredit{item=basic,weapon=host,action=actionSerial,augment=true};float share=Share(host);
            if(host.HasAugment(AugmentId.Flurry)&&target?.deathAge<0&&Reachable(target,card.range))
            {fx.Arc(step==1?hero.LeftHand.position:hero.RightHand.position,target.physics.Center,Color.magenta);fx.FollowupContact(target.physics.Center,false);fx.Burst(target.physics.Center,Color.magenta,3,1);DerivedHit(target,card.damage*.16f*Effect(host)*share*Deck.Damage,Vector3.zero,.08f*share,false,true);if(host.def.profile.style==WeaponStyle.Conductor)MarkEnemy(target,host);}
            if(host.HasAugment(AugmentId.Crosscut)&&step==1)
            {
                Vector3 aim=hero.Root.forward;float arc=host.def.profile.arc+(host.rank-1)*10;fx.Sweep(HeroPosition,aim,card.range,arc,new Color(1,.48f,1),.3f,true);
                foreach(var e in enemies.Where(e=>InCone(e,HeroPosition,aim,card.range,arc)).OrderBy(e=>e.id).Take(share<1?2:4).ToArray())DerivedHit(e,card.damage*.45f*Effect(host)*share*Deck.Damage,aim*2,.2f,true,true);
            }
        }
        void ResolveAuthoredAbility(AbilityCard card)
        {
            var host=actionItem;float s=Effect(host),damage=card.damage*Deck.Damage*AugmentCatalog.Opening(host);bool focused=host.HasAugment(AugmentId.Focused);
            Vector3 origin=HeroPosition,aim=hero.Root.forward,point=origin+aim*2.8f;
            var target=actionTarget?.deathAge<0&&Reachable(actionTarget,card.range)?actionTarget:Target(card.range);actionTarget=target;
            float stagger=host.HasAugment(AugmentId.FollowThrough)?.55f:1;
            if(host.HasAugment(AugmentId.FollowThrough)){followBank=.4f*s;followOwner=host;}
            if(card.id==AbilityId.Counterguard){AddBarrier(host,35*ItemRarity.Power[host.rank],3.5f*s,host.HasAugment(AugmentId.Discharge));return;}
            if(card.id==AbilityId.SteelJab||card.id==AbilityId.MassDriver)
            {
                if(target==null)return;var victims=new List<Enemy>{target};bool split=host.HasAugment(AugmentId.SplitPayload);
                if(split){var other=Target(card.range,target);if(other!=null)victims.Add(other);}var chain=new Cascade();
                foreach(var e in victims){fx.Arc(hero.RightHand.position,e.physics.Center,VisualFactory.Cyan);Hurt(e,damage*(card.id==AbilityId.SteelJab&&e.attack>=0?1.25f+.05f*(host.rank-1):1),card.id==AbilityId.SteelJab?aim*.9f:Vector3.zero,(card.id==AbilityId.SteelJab?.5f:.35f)*s*stagger);if(card.id==AbilityId.MassDriver)Launch(e,chain,0,(split?.65f:1)*s);}
                fx.Play(1);return;
            }
            if(card.id==AbilityId.ArcBolt)
            {
                var hit=new HashSet<int>();Vector3 from=hero.RightHand.position;int cap=focused?(host.rank>=3?2:1):3+(host.rank>=4?1:0);
                for(int i=0;i<cap&&target!=null;i++){Vector3 p=target.physics.Center;hit.Add(target.id);fx.Arc(from,p,Color.cyan);ElectricHit(target,damage*(i==0?(focused?1.35f:1):.65f)+(target.stun>0?8*s:0),.18f);from=p;target=enemies.Where(e=>!hit.Contains(e.id)&&Reachable(e,card.range+3)&&Vector3.Distance(e.physics.Center,p)<4.2f).OrderBy(e=>Vector3.Distance(e.physics.Center,p)).FirstOrDefault();}return;
            }
            if(card.id==AbilityId.ConductiveBrand)
            {
                int cap=focused?2:5;foreach(var e in enemies.Where(e=>Reachable(e,card.range)).OrderBy(e=>Vector3.Distance(e.physics.Center,point)).Take(cap).ToArray()){Hurt(e,damage,Vector3.zero,.05f);MarkEnemy(e,host,focused?1.4f:1);fx.Arc(hero.RightHand.position,e.physics.Center,Color.cyan);}
            }
            else
            {
                float arc=card.id==AbilityId.Breaker?(focused?40:100+5*(host.rank-1)):card.id==AbilityId.ReapingCut?(focused?42:70+5*(host.rank-1)):180;
                float width=1.8f+.1f*(host.rank-1);bool corridor=card.id==AbilityId.VectorTether;
                if(corridor)fx.Corridor(origin,aim,card.range,width,Color.cyan,.55f);else if(card.id==AbilityId.GravityWell){fx.Shape(origin,Vector3.forward,card.range,180,Color.cyan,.4f);fx.Symbol(point,Color.cyan,.4f,false);}else fx.Shape(origin,aim,card.range,arc,card.id==AbilityId.ReapingCut?Color.white:VisualFactory.Cyan,.4f);
                foreach(var e in enemies.ToArray())
                {
                    if(corridor?!InCorridor(e,origin,aim,card.range,width):!InCone(e,origin,aim,card.range,arc)||!Reachable(e,card.range))continue;
                    Vector3 delta=e.rig.Root.position-origin;delta.y=0;Vector3 impulse=Vector3.zero;float stun=.2f*s,hitDamage=damage;
                    if(card.id==AbilityId.Breaker){stun=1*s*stagger*(focused?1.5f:1);impulse=delta.normalized*3.5f;}
                    if(card.id==AbilityId.ForcePush){bool wash=host.HasAugment(AugmentId.Backwash);if(wash)backwash[e]=e.rig.Root.position;impulse=delta.normalized*11*s*(wash?.65f:1);stun=.45f*s;}
                    if(card.id==AbilityId.GravityWell||corridor)
                    {
                        Vector3 gather=corridor?origin+aim*Mathf.Clamp(Vector3.Dot(delta,aim),1,card.range):point;var pull=gather-e.rig.Root.position;pull.y=0;impulse=pull.normalized*Mathf.Min(10,pull.magnitude*3)*s*AugmentCatalog.Opening(host)*(host.HasAugment(AugmentId.Anchor)?.7f:1);stun=0;SetSetup(e,host);
                        if(host.HasAugment(AugmentId.Anchor))anchors.Add(new AnchorCheck{enemy=e,before=e.rig.Root.position,stagger=.25f*s,credit=currentCredit});fx.Arc(e.physics.Center,gather+Vector3.up,Color.cyan);
                    }
                    if(card.id==AbilityId.ReapingCut&&e.health<e.maxHealth*(.5f+.025f*(host.rank-1)))hitDamage*=focused?1.8f+.1f*(host.rank-1):1.4f+.05f*(host.rank-1);
                    Hurt(e,hitDamage,impulse,stun,card.id==AbilityId.ReapingCut);
                }
                if(card.id==AbilityId.ForcePush)PushCorpses(card.range);
            }
            if(host.HasAugment(AugmentId.Afterfield))fields.Add(new Field{host=host,credit=currentCredit,origin=origin,aim=aim,point=point});fx.Play(2);
        }
        void ResolveAuthoredBasic(LoadoutItem host,AbilityCard card,Enemy target,bool left,bool nativeFollowup=false)
        {
            if(target==null||target.deathAge>=0||!Reachable(target,card.range))target=Target(card.range);if(target==null)return;
            var profile=host.def.profile;float share=Share(host),s=Effect(host),damage=card.damage*Deck.Damage*AugmentCatalog.Opening(host);
            Vector3 origin=left?hero.LeftHand.position:hero.RightHand.position,aim=target.physics.Center-(HeroPosition+Vector3.up);aim.y=0;aim.Normalize();
            bool secondary=SharedSecond(host);float range=card.range*(host.HasAugment(AugmentId.HookedEdge)?.85f:1);
            if(nativeFollowup&&profile.style!=WeaponStyle.Kinetic)return;
            if(host.def.weapon==WeaponKind.Fists)
            {
                fx.Arc(origin,target.physics.Center,VisualFactory.Cyan);fx.Burst(target.physics.Center,VisualFactory.Cyan,4,2);
                Hurt(target,damage,aim*profile.force,(profile.style==WeaponStyle.Kinetic?.45f:.18f)*s);WeaponHits[0]++;
                if(profile.style==WeaponStyle.Conductor)MarkEnemy(target,host);
                if(nativeFollowup&&host.rank>=3&&Once("kinetic-wave"+Deck.Items.IndexOf(host)))Pulse(target.physics.Center,2.4f,.5f*s,3,target);
                if(!nativeFollowup&&!secondary&&host.HasAugment(AugmentId.PalmDrive)&&target.deathAge<0){Control(target,.12f,aim*(6+.6f*(host.rank-1))*share);fx.Corridor(target.rig.Root.position,aim,1,.35f,Color.magenta,.3f);}
                SpendBreach(host,target,share);return;
            }
            if(host.def.weapon==WeaponKind.Sword)
            {
                float arc=(profile.arc+10*(host.rank-1))*(host.HasAugment(AugmentId.HookedEdge)?.65f:1);bool hooked=false;
                fx.Sweep(HeroPosition,aim,range,arc,Color.white,.3f);
                foreach(var e in enemies.Where(e=>InCone(e,HeroPosition,aim,range,arc)&&Reachable(e,range)).OrderBy(e=>Vector3.Distance(e.physics.Center,HeroPosition)).ToArray())
                {
                    float d=damage*(profile.style==WeaponStyle.Greatsword&&e.health<e.maxHealth*.5f?1.4f:1);Hurt(e,d,aim*profile.force*s,.4f,true);WeaponHits[1]++;if(e.deathAge>=0&&host.rank>=3&&profile.style==WeaponStyle.Edge&&Once("edge-launch")){Launch(e,null,0,.6f*s);}
                    if(host.HasAugment(AugmentId.HookedEdge)&&!hooked&&e.deathAge<0){hooked=true;var to=HeroPosition+aim*1.9f;fx.Arc(e.physics.Center,to+Vector3.up,Color.magenta);Control(e,.12f,(to-e.rig.Root.position).normalized*8*s*share);SetSetup(e,host);}
                }
                return;
            }
            if(host.def.weapon==WeaponKind.Hammer)
            {
                bool pile=host.HasAugment(AugmentId.Piledriver),ground=host.HasAugment(AugmentId.Groundbreaker);var victims=new List<Enemy>{target};
                int count=profile.style==WeaponStyle.Impact&&host.rank>=3?3:1;
                if(!pile)victims.AddRange(enemies.Where(e=>e!=target&&Reachable(e,range)&&Vector3.Distance(e.physics.Center,target.physics.Center)<2.5f).Take(count-1));
                var chain=new Cascade();foreach(var e in victims){Hurt(e,damage,Vector3.zero,.6f*s);Launch(e,chain,0,profile.force/15*s*(ground?.65f:1)*(pile?1+.35f*s*share:1));WeaponHits[2]++;fx.Burst(e.physics.Center,pile?Color.magenta:VisualFactory.Orange,8,3);}
                if(!pile&&profile.style==WeaponStyle.Impact)Pulse(target.physics.Center,2.2f+.15f*(host.rank-1),.25f*s,4,target);
                if(ground&&!secondary){float length=share<1?3.2f:4;fx.GroundWave(HeroPosition,aim,length,.9f,Color.magenta,.3f);foreach(var e in enemies.Where(e=>!victims.Contains(e)&&InCorridor(e,HeroPosition,aim,length,.9f)).Take(share<1?2:3))Control(e,.45f*s*share,Vector3.zero);}
                return;
            }
            if(Ammo(host)<=0)return;ammunition[host]--;bool bore=host.HasAugment(AugmentId.BoreRounds);var hit=new HashSet<int>();
            if(profile.style==WeaponStyle.Scatter)
            {
                float arc=bore?20:40;fx.Shape(HeroPosition,aim,range,arc,VisualFactory.Cyan,.22f);
                foreach(var e in enemies.Where(e=>InCone(e,HeroPosition,aim,range,arc)&&Reachable(e,range)).OrderBy(e=>e.id).Take(4+(host.rank-1)/2).ToArray()){hit.Add(e.id);Hurt(e,damage,aim*profile.force*s,.2f*s);WeaponHits[5]++;}
            }
            if(bore||profile.style==WeaponStyle.Rifle)
            {
                int cap=bore?AugmentCatalog.BoreCap(host,share<1):Mathf.Min(4,host.rank+1);fx.Arc(origin,HeroPosition+Vector3.up+aim*range,bore?Color.magenta:VisualFactory.Cyan);
                var pierced=enemies.Where(e=>(profile.style==WeaponStyle.Scatter||!hit.Contains(e.id))&&InCorridor(e,HeroPosition,aim,range,.7f)&&Reachable(e,range)).OrderBy(e=>Vector3.Distance(e.physics.Center,HeroPosition)).Take(cap).ToArray();
                foreach(var e in pierced){Hurt(e,damage*(profile.style==WeaponStyle.Scatter?.35f:1),aim*profile.force,.2f);WeaponHits[(int)host.def.weapon]++;}
                if(profile.style==WeaponStyle.Rifle&&host.rank>=3&&pierced.Length>0&&Once("rail-launch"))Launch(pierced[pierced.Length-1],null,0,.65f*s);
            }
            else if(profile.style==WeaponStyle.Pistol)
            {
                var e=target;Vector3 from=origin;for(int i=0;i<Mathf.Min(3,host.rank)&&e!=null;i++){hit.Add(e.id);Vector3 p=e.physics.Center;fx.Arc(from,p,VisualFactory.Cyan);Hurt(e,damage*(i==0?1:.65f),aim*.7f,.15f*s);WeaponHits[3]++;from=p;e=enemies.Where(x=>!hit.Contains(x.id)&&Reachable(x,range)&&Vector3.Distance(x.physics.Center,p)<4.2f).OrderBy(x=>Vector3.Distance(x.physics.Center,p)).FirstOrDefault();}
            }
            SpendBreach(host,target,share);fx.Play(0);
        }
        void SpendBreach(LoadoutItem host,Enemy primary,float share)
        {if(!host.HasAugment(AugmentId.BreachLoad)||!breachArmed.Remove(host))return;Note("Breach Load impact burst",true);Pulse(primary.physics.Center,(share<1?1.6f:2)+.15f*(host.rank-1),.4f*Effect(host)*share,share<1?3:5);}
    }
}
