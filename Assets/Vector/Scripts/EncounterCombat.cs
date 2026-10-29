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
        Enemy Target(float range)
        {
            Enemy best=null;float score=float.MaxValue;
            foreach(var e in enemies){if(!Reachable(e,range))continue;float value=Vector3.Distance(e.rig.Root.position,HeroPosition)-(e.attack>=0?.4f:0);if(value<score){best=e;score=value;}}
            return best;
        }
        public bool Advance()
        {
            if(State!=RunState.Running||ActiveSlot>=0||recovery>0)return false;
            int slot=NextSlot;if(slot<0)return false;
            var card=Deck.Slots[slot];var target=Target(card.range);if(target==null)return false;
            if(slot!=expectedSlot)OrderErrors++;
            ActiveSlot=slot;actionTarget=target;actionAge=0;actionDelivered=false;AbilityCasts++;SlotCasts[slot]++;castTrace.Add(slot);
            expectedSlot=Deck.NextOccupied((slot+1)%3);Announce("SLOT "+(slot+1)+" / "+card.name.ToUpperInvariant());return true;
        }
        void StepHero(float dt)
        {
            recovery=Mathf.Max(0,recovery-dt);
            if(Automatic&&ActiveSlot<0)Advance();
            if(ActiveSlot>=0)
            {
                var card=Deck.Slots[ActiveSlot];float duration=card.duration/Deck.Speed;
                if(actionTarget==null||actionTarget.deathAge>=0)actionTarget=Target(card.range);
                if(actionTarget!=null)
                {
                    Vector3 aim=actionTarget.rig.Root.position-HeroPosition;aim.y=0;
                    if(aim.sqrMagnitude>.01f)heroPhysics.Motor.MoveRotation(Quaternion.Slerp(hero.Root.rotation,Quaternion.LookRotation(aim),1-Mathf.Exp(-17*dt)));
                }
                actionAge+=dt;
                if(!actionDelivered&&actionAge>=duration*.44f){actionDelivered=true;Resolve(card);}
                hero.Pose(clock,.7f,card.pose,actionAge/duration,heroReact,card.pose<0,actionAge/duration);
                if(actionAge>=duration){sequencePointer=(ActiveSlot+1)%3;ActiveSlot=-1;recovery=.11f/Deck.Speed;}
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
                Hurt(body,card.damage*Deck.Damage,card.id==AbilityId.SteelJab?hero.Root.forward*.9f:Vector3.zero,.45f);
                if(card.id==AbilityId.MassDriver)Launch(body);
                ImpactPause(card.id==AbilityId.SteelJab?.025f:.06f);return;
            }
            if(card.id==AbilityId.ArcBolt)
            {
                if(actionTarget==null)return;var hit=new HashSet<int>();Enemy target=actionTarget;Vector3 from=hero.RightHand.position;
                for(int i=0;i<3&&target!=null;i++)
                {
                    Vector3 point=target.physics.Center;hit.Add(target.id);fx.Arc(from,point,color);fx.Burst(point,color,5,2);
                    Hurt(target,((i==0?card.damage:18)+(target.stun>0?12:0))*Deck.Damage,Vector3.zero,.18f);
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
                Hurt(e,card.damage*Deck.Damage,impulse+Vector3.up*(card.id==AbilityId.ForcePush?1:0),card.id==AbilityId.Breaker?1f:.65f);
                fx.Burst(e.physics.Center,color,4,2.5f);hits++;
            }
            if(hits>0){fx.Play(card.id==AbilityId.Breaker?1:2);fx.Ring(HeroPosition,card.range,color,.45f);shake=.18f;ImpactPause(.05f);}
        }
        void Hurt(Enemy e,float damage,Vector3 impulse,float stun)
        {
            if(e.deathAge>=0)return;
            e.health-=damage;e.stun=Mathf.Max(e.stun,stun);e.react=1;e.engaged=false;e.attack=-1;
            if(e.health<=0)
            {
                enemies.Remove(e);e.deathAge=0;e.physics.Die(impulse);corpses.Add(e);Kills++;Ragdolls++;
                Health=Mathf.Min(tuning.heroHealth,Health+.3f);
            }
            else e.physics.Impulse(impulse);
        }
        void Launch(Enemy body)
        {
            for(int i=flights.Count-1;i>=0;i--)if(flights[i].body==body)flights.RemoveAt(i);
            Vector3 origin=body.physics.Center;Vector3 direction=origin-(HeroPosition+Vector3.up);direction.y=0;direction.Normalize();float nearest=7;
            foreach(var e in enemies)
            {
                if(e==body)continue;Vector3 delta=e.physics.Center-origin;delta.y=0;
                if(delta.magnitude<nearest&&Vector3.Dot(delta.normalized,direction)>.4f){direction=delta.normalized;nearest=delta.magnitude;}
            }
            if(direction.sqrMagnitude<.1f)direction=hero.Root.forward;
            body.physics.Impulse(direction*(body.heavy?11:15)+Vector3.up*1.5f-body.physics.Velocity);
            body.stun=1.2f;body.engaged=false;body.attack=-1;
            flights.Add(new Flight{body=body,previous=origin});Launches++;Announce("MASS DRIVER / BODY LAUNCHED");
        }
        void ProcessContacts()
        {
            var batch=contacts.ToArray();contacts.Clear();
            foreach(var contact in batch)
            {
                if(contact.target<0){if(contact.speed>1)WorldContacts++;continue;}
                PhysicsContacts++;
                if(contact.target<=0||!byId.TryGetValue(contact.target,out var target)||target.deathAge>=0)continue;
                foreach(var flight in flights)
                {
                    if(flight.body.id!=contact.source||flight.hit.Count>=2||contact.speed<1.2f||flight.hit.Contains(target.id))continue;
                    flight.hit.Add(target.id);CollisionHits++;Color color=new Color(1,.74f,.28f);
                    fx.Ring(contact.point,1.1f,color,.32f);fx.Burst(contact.point,color,7,3.5f);fx.Play(1);
                    Hurt(target,Deck.Pinball?32:18,Vector3.zero,Deck.Pinball?1:.3f);if(target.deathAge>=0)CollisionKills++;
                    Announce((Deck.Pinball?"FAULT LINE":"BODY IMPACT")+" / COLLISION "+flight.hit.Count+" OF 2");ImpactPause(.04f);
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
                if(e.deathAge>7||(corpses.Count>22&&e.deathAge>2)){DisposeEnemy(e);corpses.RemoveAt(i);}
            }
            for(int i=flights.Count-1;i>=0;i--)
            {
                var f=flights[i];if(!byId.ContainsKey(f.body.id)){flights.RemoveAt(i);continue;}
                f.age+=dt;f.trail-=dt;Vector3 p=f.body.physics.Center;
                if(f.trail<=0){fx.Arc(f.previous,p,Deck.Pinball?new Color(1,.74f,.28f):VisualFactory.Cyan);f.trail=.04f;}
                f.previous=p;if(f.age>1.3f)flights.RemoveAt(i);
            }
        }
        void ImpactPause(float seconds){if(!qa&&!ReducedMotion)hitStop=Mathf.Max(hitStop,seconds);}
    }
}
