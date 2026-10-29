using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        bool Reachable(Enemy e,float range)
        {
            return !e.entering&&Vector3.Distance(e.rig.Root.position,HeroPosition)<=range&&
                !Physics.Linecast(HeroPosition+Vector3.up*1.2f,e.rig.Root.position+Vector3.up*1.2f,1<<EntryLayout.WorldLayer);
        }
        Enemy Target(float range,Enemy exclude=null)
        {
            Enemy best=null;float score=float.MaxValue;
            foreach(var e in enemies){if(e==exclude||!Reachable(e,range))continue;float value=Vector3.Distance(e.rig.Root.position,HeroPosition)-(e.attack>=0?.4f:0);if(value<score){best=e;score=value;}}
            return best;
        }
        public int Ammo(LoadoutItem weapon)
        {
            if(weapon==null)return 0;
            if(!ammunition.TryGetValue(weapon,out int ammo)){ammo=LoadoutCatalog.Magazine(weapon);ammunition[weapon]=ammo;}return ammo;
        }
        readonly List<LoadoutItem> pendingReloads=new List<LoadoutItem>();
        void PrepareWeapon(LoadoutItem weapon)
        {
            if(weapon==null||LoadoutCatalog.Magazine(weapon)==0||Ammo(weapon)>0||pendingReloads.Contains(weapon))return;
            pendingReloads.Add(weapon);Preparation=Mathf.Max(Preparation,1.2f);Reloads++;ActionStatus="RELOADING / "+weapon.def.name;
        }
        bool Advance()
        {
            if(State!=RunState.Running||ActiveSlot>=0||recovery>0)return false;
            bool ultimate=UltimateQueued&&Target(Deck.Ultimate.def.ability.range)!=null;
            int slot=ultimate?5:NextSlot;if(slot<0)return false;
            var item=ultimate?null:Deck.Slots[slot];var card=ultimate?Deck.Ultimate.def.ability:Deck.Card(item);
            var target=Target(card.range);if(target==null){ActionStatus="WAITING FOR TARGET / "+card.name;return false;}
            ActiveSlot=slot;PairSlot=ultimate?-1:Deck.PairAfter(slot);actionItem=item;actionWeapon=item?.def.kind==ItemKind.Basic?Deck.WeaponFor(item):null;
            activeCard=card;actionTarget=target;pairItem=PairSlot>=0?Deck.Slots[PairSlot]:null;pairWeapon=pairItem!=null?Deck.WeaponFor(pairItem):null;
            pairCard=pairItem==null?null:Deck.Card(pairItem);pairTarget=pairCard==null?null:Target(pairCard.range,target)??target;
            actionSerial++;actionAge=0;actionDelivered=bonusDelivered=false;Preparation=0;pendingReloads.Clear();
            actionDuration=Mathf.Max(card.duration,pairCard?.duration??0)/(ultimate?1:Deck.Speed);
            if(ultimate){UltimateCasts++;ultimateRequested=false;ultimateTimes.Add(Elapsed);ultimateDeadlines.Add(ultimateDue);castTrace.Add(-1);}
            else
            {
                if(slot!=expectedSlot)OrderErrors++;
                AbilityCasts++;SlotCasts[slot]++;castTrace.Add(slot);
                if(PairSlot>=0){PairedCasts++;SlotCasts[PairSlot]++;castTrace.Add(PairSlot);if(!Deck.TwoHanded&&pairTarget!=target)IndependentTargets++;}
                expectedSlot=Deck.NextOccupied(((PairSlot>=0?PairSlot:slot)+1)%5);
            }
            WeaponKind desired=actionWeapon?.def.weapon??(card.id==AbilityId.SteelJab||card.id==AbilityId.Breaker||card.id==AbilityId.MassDriver?WeaponKind.Fists:heldWeapon);
            ActionStatus=ultimate?"ULTIMATE":PairSlot>=0?(Deck.TwoHanded?"TWO-HANDED ATTACK":"SIMULTANEOUS ATTACKS"):card.name.ToUpperInvariant();
            // Keep the approved preparation cadence; changing stance never unequips a weapon.
            if(desired!=heldWeapon){Preparation=.28f;ActionStatus="READYING / "+desired;heldWeapon=desired;}
            PrepareWeapon(actionWeapon);if(PairSlot>=0&&!Deck.TwoHanded)PrepareWeapon(pairWeapon);
            Announce(ultimate?"ULTIMATE / SINGULARITY BURST":PairSlot>=0?ActionStatus:"SLOT "+(slot+1)+" / "+card.name.ToUpperInvariant());return true;
        }
        void StepHero(float dt)
        {
            recovery=Mathf.Max(0,recovery-dt);if(ActiveSlot<0)Advance();
            if(ActiveSlot>=0)
            {
                if(Preparation>0)
                {
                    Preparation=Mathf.Max(0,Preparation-dt);hero.Pose(clock,.6f,-1,0,heroReact,false,0);
                    if(Preparation==0){foreach(var weapon in pendingReloads)ammunition[weapon]=LoadoutCatalog.Magazine(weapon);pendingReloads.Clear();}
                    return;
                }
                if(actionTarget==null||actionTarget.deathAge>=0)actionTarget=Target(activeCard.range);
                if(actionTarget!=null)
                {
                    Vector3 aim=actionTarget.rig.Root.position-HeroPosition;aim.y=0;
                    if(aim.sqrMagnitude>.01f)heroPhysics.Motor.MoveRotation(Quaternion.Slerp(hero.Root.rotation,Quaternion.LookRotation(aim),1-Mathf.Exp(-17*dt)));
                }
                actionAge+=dt;ActionStatus=ActiveSlot==5?"ULTIMATE EXECUTING":PairSlot>=0?(Deck.TwoHanded?"TWO-HANDED SWING":"PAIRED / INDEPENDENT TARGETS"):activeCard.name.ToUpperInvariant();
                if(!actionDelivered&&actionAge>=actionDuration*.44f)
                {
                    actionDelivered=true;
                    currentCredit=new KillCredit{item=ActiveSlot==5?Deck.Ultimate:actionItem,weapon=actionWeapon,action=actionSerial};
                    if(ActiveSlot==5)ResolveUltimate();
                    else if(actionWeapon!=null)
                    {
                        ResolveBasic(actionWeapon,activeCard,actionTarget,!Deck.TwoHanded&&actionItem.mount==1);
                        if(PairSlot>=0&&!Deck.TwoHanded){currentCredit=new KillCredit{item=pairItem,weapon=pairWeapon,action=actionSerial};ResolveBasic(pairWeapon,pairCard,pairTarget,pairItem.mount==1);}
                    }
                    else Resolve(activeCard);
                }
                if(!bonusDelivered&&actionAge>=actionDuration*.70f)
                {
                    bonusDelivered=true;currentCredit=new KillCredit{item=actionItem,weapon=actionWeapon,action=actionSerial};
                    if(actionWeapon?.def.weapon==WeaponKind.Fists&&actionWeapon.rank>=2)ResolveBasic(actionWeapon,activeCard,actionTarget,actionItem.mount==1,true);
                    if(PairSlot>=0&&!Deck.TwoHanded&&pairWeapon?.def.weapon==WeaponKind.Fists&&pairWeapon.rank>=2){currentCredit=new KillCredit{item=pairItem,weapon=pairWeapon,action=actionSerial};ResolveBasic(pairWeapon,pairCard,pairTarget,pairItem.mount==1,true);}
                }
                hero.Pose(clock,.7f,activeCard.pose,actionAge/actionDuration,heroReact,activeCard.pose<0,actionAge/actionDuration,
                    actionWeapon==null?-1:Deck.TwoHanded?0:actionItem.mount,PairSlot>=0&&!Deck.TwoHanded,actionWeapon!=null&&Deck.TwoHanded);
                if(actionAge>=actionDuration)
                {
                    if(ActiveSlot==5){UltimateRemaining=UltimateCooldown;ultimateDue=Elapsed+UltimateCooldown;}
                    else sequencePointer=((PairSlot>=0?PairSlot:ActiveSlot)+1)%5;
                    ActiveSlot=PairSlot=-1;recovery=.11f/Deck.Speed;
                }
            }
            else {hero.Pose(clock,1,-1,0,heroReact,false,0);heroPhysics.Motor.MoveRotation(Quaternion.Slerp(hero.Root.rotation,Quaternion.identity,dt*4));}
        }
        void Resolve(AbilityCard card)
        {
            if(actionTarget==null||actionTarget.deathAge>=0||!Reachable(actionTarget,card.range+.3f))actionTarget=Target(card.range);
            Color color=card.id==AbilityId.ArcBolt?new Color(.55f,.7f,1):VisualFactory.Cyan;
            if(card.id==AbilityId.SteelJab||card.id==AbilityId.MassDriver)
            {
                if(actionTarget==null)return;
                var body=actionTarget;fx.Arc(hero.RightHand.position,body.physics.Center,color);fx.Burst(body.physics.Center,color,7,3);fx.Play(card.id==AbilityId.MassDriver?1:0);
                Hurt(body,card.damage*Deck.Damage*ItemRarity.Power[actionItem?.rank??1],card.id==AbilityId.SteelJab?hero.Root.forward*.9f:Vector3.zero,.45f);
                if(card.id==AbilityId.MassDriver)Launch(body);
                ImpactPause(card.id==AbilityId.SteelJab?.025f:.06f);return;
            }
            if(card.id==AbilityId.ArcBolt)
            {
                if(actionTarget==null)return;var hit=new HashSet<int>();Enemy target=actionTarget;Vector3 from=hero.RightHand.position;
                for(int i=0;i<3&&target!=null;i++)
                {
                    Vector3 point=target.physics.Center;hit.Add(target.id);fx.Arc(from,point,color);fx.Burst(point,color,5,2);
                    Hurt(target,((i==0?card.damage:18)+(target.stun>0?12:0))*Deck.Damage*ItemRarity.Power[actionItem?.rank??1],Vector3.zero,.18f);
                    from=point;target=null;float nearest=4.2f;
                    foreach(var e in enemies)if(!hit.Contains(e.id)&&Reachable(e,card.range+4))
                    {float d=Vector3.Distance(e.physics.Center,from);if(d<nearest){nearest=d;target=e;}}
                }
                fx.Play(0);ImpactPause(.03f);return;
            }
            int hits=0;
            foreach(var e in enemies.ToArray())
            {
                if(!Reachable(e,card.range))continue;
                Vector3 delta=e.rig.Root.position-HeroPosition;delta.y=0;
                if(card.id==AbilityId.Breaker&&Vector3.Dot(delta.normalized,hero.Root.forward)<-.15f)continue;
                Vector3 impulse=delta.normalized*(card.id==AbilityId.ForcePush?11:3.5f);
                if(card.id==AbilityId.GravityWell){var gather=HeroPosition+hero.Root.forward*2.8f-e.rig.Root.position;gather.y=0;impulse=gather.normalized*Mathf.Min(10,gather.magnitude*3);}
                if(e.heavy)impulse*=.65f;
                Hurt(e,card.damage*Deck.Damage*ItemRarity.Power[actionItem?.rank??1],impulse+Vector3.up*(card.id==AbilityId.ForcePush?1:0),card.id==AbilityId.Breaker?1f:.65f);
                fx.Burst(e.physics.Center,color,4,2.5f);hits++;
            }
            if(card.id==AbilityId.ForcePush)PushCorpses(card.range);
            if(hits>0){fx.Play(card.id==AbilityId.Breaker?1:2);fx.Ring(HeroPosition,card.range,color,.45f);shake=.18f;ImpactPause(.05f);}
        }
        void Hurt(Enemy e,float damage,Vector3 impulse,float stun,bool sever=false)
        {
            if(e.deathAge>=0)return;
            if(e.stun>0&&(currentCredit?.hop??0)==0&&Deck.StaggerBonus>0){damage+=Deck.StaggerBonus;ArmorBonusHits++;}
            e.health-=damage;e.stun=Mathf.Max(e.stun,stun);e.react=1;e.engaged=false;e.attack=-1;
            if(e.health<=0)
            {
                enemies.Remove(e);e.deathAge=0;e.physics.Die(impulse);corpses.Add(e);Kills++;Ragdolls++;
                SetBodyWalkCollision(e,flights.Exists(f=>f.body==e));
                if(sever){e.physics.Sever();Severs++;fx.Burst(e.physics.Center,VisualFactory.Orange,10,3);}
                if(currentCredit!=null)
                {
                    var owner=KillOwner(currentCredit.weapon??currentCredit.item);
                    if(owner!=null)itemKills[owner]=KillsFor(owner)+1;
                    Score.Kill(e.heavy?200:100,currentCredit.action,currentCredit.hop);
                }
                Health=Mathf.Min(MaxHealth,Health+.3f);
            }
            else e.physics.Impulse(impulse);
        }
        void Launch(Enemy body,Cascade chain=null,int hop=0,float energy=1,Vector3? forcedDirection=null)
        {
            for(int i=flights.Count-1;i>=0;i--)if(flights[i].body==body)flights.RemoveAt(i);
            Vector3 origin=body.physics.Center;Vector3 direction=origin-(HeroPosition+Vector3.up);direction.y=0;direction.Normalize();float nearest=7;
            foreach(var e in enemies)
            {
                if(e==body)continue;Vector3 delta=e.physics.Center-origin;delta.y=0;
                if(delta.magnitude<nearest&&Vector3.Dot(delta.normalized,direction)>.2f){direction=delta.normalized;nearest=delta.magnitude;}
            }
            if(forcedDirection.HasValue)direction=forcedDirection.Value;
            if(direction.sqrMagnitude<.1f)direction=hero.Root.forward;
            body.physics.Impulse(direction*(body.heavy?11:15)*energy+Vector3.up*1.5f-body.physics.Velocity);
            body.stun=1.2f;body.engaged=false;body.attack=-1;
            if(chain==null)chain=new Cascade();if(chain.credit==null)chain.credit=currentCredit;chain.hit.Add(body.id);
            flights.Add(new Flight{body=body,previous=origin,chain=chain,hop=hop,energy=energy});Launches++;MaxCascadeHop=Mathf.Max(MaxCascadeHop,hop);
            if(body.deathAge>=0)SetBodyWalkCollision(body,true);
        }
        void ProcessContacts()
        {
            var batch=contacts.ToArray();contacts.Clear();
            foreach(var contact in batch)
            {
                if(contact.target<0){if(contact.speed>1)WorldContacts++;continue;}
                PhysicsContacts++;
                if(contact.target<=0||!byId.TryGetValue(contact.target,out var target)||target.deathAge>=0)continue;
                foreach(var flight in flights.ToArray())
                {
                    var chain=flight.chain;
                    if(flight.body.id!=contact.source||chain.remaining<=0||contact.speed<2||chain.hit.Contains(target.id))continue;
                    chain.hit.Add(target.id);chain.remaining--;CollisionHits++;Color color=new Color(1,.74f,.28f);
                    fx.Ring(contact.point,1.1f,color,.32f);fx.Burst(contact.point,color,7,3.5f);fx.Play(1);
                    float damage=Mathf.Clamp(contact.speed*(flight.body.heavy?2.4f:1.8f),6,26)*flight.energy;
                    if(Deck.Pinball)damage+=14*ItemRarity.Power[Deck.Rig.rank];
                    Vector3 direction=target.physics.Center-flight.body.physics.Center;direction.y=0;direction.Normalize();
                    currentCredit=new KillCredit{item=chain.credit?.item,weapon=chain.credit?.weapon,action=chain.credit?.action??0,hop=flight.hop+1};
                    Hurt(target,damage,Vector3.zero,Deck.Pinball?1:.4f);if(target.deathAge>=0)CollisionKills++;
                    if(flight.hop<2&&chain.remaining>0)Launch(target,chain,flight.hop+1,flight.energy*.64f,direction);
                    Announce("COLLISION CASCADE / "+(6-chain.remaining)+" OF 6");ImpactPause(.04f);
                }
            }
        }
        void StepBodies(float dt)
        {
            foreach(var e in enemies)if(!e.physics.Finite())throw new InvalidOperationException("Non-finite living physics body");
            for(int i=corpses.Count-1;i>=0;i--)
            {
                var e=corpses[i];e.deathAge+=dt;
                if(!e.physics.Finite())throw new InvalidOperationException("Unstable ragdoll physics");
            }
            for(int i=flights.Count-1;i>=0;i--)
            {
                var f=flights[i];if(!byId.ContainsKey(f.body.id)){flights.RemoveAt(i);continue;}
                f.age+=dt;f.trail-=dt;Vector3 p=f.body.physics.Center;
                if(f.trail<=0){fx.Arc(f.previous,p,Deck.Pinball?new Color(1,.74f,.28f):VisualFactory.Cyan);f.trail=.04f;}
                f.previous=p;if(f.age>1.3f){flights.RemoveAt(i);if(f.body.deathAge>=0)SetBodyWalkCollision(f.body,false);}
            }
        }
        void SetBodyWalkCollision(Enemy body,bool projectile)
        {
            body.physics.IgnoreWalker(heroPhysics,true);
            foreach(var enemy in enemies)body.physics.IgnoreWalker(enemy.physics,!projectile);
        }
        void ShoveWalkingBodies(float dt)
        {
            foreach(var body in corpses)
            {
                if(flights.Exists(f=>f.body==body))continue;
                if(body.physics.ShoveAside(HeroPosition,Vector3.forward*tuning.walkSpeed,dt))CorpseShoves++;
                foreach(var enemy in enemies)
                    if(enemy.stun<=0&&body.physics.ShoveAside(enemy.rig.Root.position,enemy.physics.Velocity,dt))CorpseShoves++;
            }
        }
        void ImpactPause(float seconds){if(!qa&&!ReducedMotion)hitStop=Mathf.Max(hitStop,seconds);}
    }
}
