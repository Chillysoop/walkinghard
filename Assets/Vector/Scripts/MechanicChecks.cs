using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        int mechanicAssertions;
        void MechanicRequire(bool okay,string rule){if(!okay)throw new InvalidOperationException("Combat mechanic: "+rule);mechanicAssertions++;}
        Enemy ProbeEnemy(float x,float z,float hp=500)
        {
            var e=new Enemy{id=++nextId,health=hp,maxHealth=hp};e.rig=new FighterRig("QA target",actors,false);e.rig.Root.position=new Vector3(x,.04f,z);e.physics=new FighterPhysics(e.rig,e.id,this,false);enemies.Add(e);byId[e.id]=e;Physics.SyncTransforms();return e;
        }
        void ProbeReset()
        {
            foreach(var e in enemies)DisposeEnemy(e);foreach(var e in corpses)DisposeEnemy(e);enemies.Clear();corpses.Clear();byId.Clear();flights.Clear();contacts.Clear();ResetSignatures();fx.Clear();
            Deck=new AbilityDeck(7);Health=tuning.heroHealth;heroPhysics.Motor.position=new Vector3(0,.04f,0);heroPhysics.Motor.rotation=Quaternion.identity;Physics.SyncTransforms();
            actionSerial++;actionItem=Deck.Basics[0];pairItem=Deck.Basics[1];actionWeapon=pairWeapon=null;PairSlot=-1;ActiveSlot=0;actionAge=0;salvageChain=null;
            currentCredit=new KillCredit{item=actionItem,action=actionSerial};
        }
        LoadoutItem ProbeHost(ItemDefinition def,AugmentId? augment=null,int rank=1)
        {
            var host=def==LoadoutCatalog.Fists?Deck.Fists:Deck.Acquire(def,rank);host.rank=rank;
            if(augment.HasValue)Deck.Attach(Deck.Acquire(AugmentCatalog.Get(augment.Value)),host);
            if(def.kind==ItemKind.Weapon){Deck.Move(host,PlaceKind.Weapon,0);actionWeapon=host;actionItem=Deck.Basics[0];}else {actionItem=host;Deck.Move(host,PlaceKind.Sequence,2);ActiveSlot=2;}
            activeCard=LoadoutCatalog.Action(host);currentCredit=new KillCredit{item=actionItem,weapon=actionWeapon,action=actionSerial};return host;
        }
        void ProbeFrames(int frames){for(int n=0;n<frames;n++){Elapsed+=1f/60;StepSignatures(1f/60);Physics.SyncTransforms();Physics.Simulate(1f/60);fx.Step(1f/60);}}
        void CheckCombatMechanics()
        {
            // These probes execute production effects and PhysX, then discard the isolated fixture.
            // They complement (not replace) native visual acceptance and natural encounter runs.
            foreach(var module in AugmentCatalog.All)
            {
                ProbeReset();var def=LoadoutCatalog.All.First(module.module.Fits);var host=ProbeHost(def,module.module.id);var target=ProbeEnemy(0,2);var other=ProbeEnemy(.6f,3);var untouched=ProbeEnemy(7,2);actionTarget=target;
                if(def.kind==ItemKind.Weapon)
                {
                    if(host.HasAugment(AugmentId.BreachLoad)){MechanicRequire(!breachArmed.Contains(host),"Breach starts unarmed");Replenish(host);MechanicRequire(!breachArmed.Contains(host),"full magazine is not a qualifying reload");ammunition[host]=0;Replenish(host);MechanicRequire(breachArmed.Contains(host)&&Ammo(host)==LoadoutCatalog.Magazine(host),"actual refill arms once");}
                    float before=target.health;ResolveBasic(host,activeCard,target,false);float opening=before-target.health;MechanicRequire(opening>0,"weapon module retains real direct attack: "+module.name);
                    if(host.HasAugment(AugmentId.Flurry)||host.HasAugment(AugmentId.Crosscut)){WeaponExtension(host,actionItem,activeCard,target,1);WeaponExtension(host,actionItem,activeCard,target,2);MechanicRequire(target.health<before-opening&&AugmentCatalog.Extension(host)>0,"follow-up has added impact and commitment: "+module.name);}
                    if(host.HasAugment(AugmentId.PalmDrive)||host.HasAugment(AugmentId.HookedEdge)){Vector3 beforePosition=target.rig.Root.position;ProbeFrames(12);MechanicRequire(Vector3.Distance(target.rig.Root.position,beforePosition)>.05f,"actual displacement: "+module.name);}
                    if(host.HasAugment(AugmentId.Groundbreaker))MechanicRequire(other.stun>0,"ground wave affects additional target");
                    if(host.HasAugment(AugmentId.Piledriver))MechanicRequire(flights.Count==1,"Piledriver one launch");
                    if(host.HasAugment(AugmentId.BoreRounds))MechanicRequire(other.health<500,"Bore penetrates aligned target");
                    if(host.HasAugment(AugmentId.BreachLoad))MechanicRequire(!breachArmed.Contains(host)&&Ammo(host)==LoadoutCatalog.Magazine(host)-1&&other.stun>0,"Breach consumes one payoff and one round");
                }
                else
                {
                    Resolve(activeCard);switch(module.module.id)
                    {
                        case AugmentId.FollowThrough:MechanicRequire(followBank>0&&target.stun<.5f,"follow-through moves control into one bank");break;
                        case AugmentId.Focused:MechanicRequire(target.stun>1,"focused Breaker increases primary stagger");break;
                        case AugmentId.Echo:float hp=target.health;nativeDuration=.4f;actionDuration=.62f;actionAge=.5f;extensionStep=0;AdvanceExtension();MechanicRequire(target.health<hp,"Echo repeats one real hit");break;
                        case AugmentId.Afterfield:MechanicRequire(fields.Count==1,"Afterfield leaves one bounded field");ProbeFrames(30);MechanicRequire(fields[0].hit.Count<=6&&fields[0].left<2,"field advances lifetime and target cap");break;
                        case AugmentId.SplitPayload:MechanicRequire(flights.Count==2&&flights[0].chain==flights[1].chain&&flights[0].energy<1,"Split Payload shares collision budget and reduced impulse");break;
                        case AugmentId.Backwash:MechanicRequire(backwash.ContainsKey(target)&&!backwash.ContainsKey(untouched),"Backwash only records struck living targets");ProbeFrames(10);nativeDuration=.95f;actionDuration=1.27f;actionAge=1.1f;extensionStep=0;AdvanceExtension();MechanicRequire(untouched.stun==0,"Backwash cannot gather untouched targets");break;
                        case AugmentId.Discharge:MechanicRequire(barriers.Count==1,"Counterguard barrier exists");ReceiveDamage(target,50);MechanicRequire(counterBank==0&&target.health<500,"Discharge replaces bank with actual burst");break;
                        case AugmentId.Anchor:MechanicRequire(anchors.Count==2,"Anchor checks affected living targets");ProbeFrames(22);MechanicRequire(target.stun>0||other.stun>0,"Anchor staggers after physical movement");break;
                    }
                }
                MechanicRequire(untouched.health==500,"no damage outside the actual effect: "+module.name);
            }
            ProbeReset();var gun=ProbeHost(LoadoutCatalog.Find("pistol"),AugmentId.BreachLoad);var red=Deck.Acquire(LoadoutCatalog.Find("redline"));Deck.Move(red,PlaceKind.Ultimate);ResolveUltimate();MechanicRequire(!breachArmed.Contains(gun),"Redline full mag cannot arm Breach");ammunition[gun]=2;ResolveUltimate();MechanicRequire(breachArmed.Contains(gun)&&redlineTurns==3,"Redline real refill arms Breach and three turns");
            ProbeReset();var wrap=ProbeHost(LoadoutCatalog.Find("wraps"));var marked=ProbeEnemy(0,2);MarkEnemy(marked,wrap);float beforeMark=marked.health;MechanicRequire(ConsumeMark(marked)&&!ConsumeMark(marked)&&beforeMark>marked.health,"Conductive consumes exactly once");MarkEnemy(marked,wrap);MechanicRequire(!ConsumeMark(marked),"same-origin mark refresh cannot discharge loop");
            ProbeReset();var sword=ProbeHost(LoadoutCatalog.Find("sword"),AugmentId.Crosscut);var low=ProbeEnemy(0,2,5);int beforeKills=Kills;ResolveBasic(sword,activeCard,low,false);WeaponExtension(sword,actionItem,activeCard,low,1);MechanicRequire(Kills==beforeKills+1&&corpses.Count==1,"return cut cannot kill fragments twice");
            ProbeReset();var hammer=ProbeHost(LoadoutCatalog.Find("hammer"),AugmentId.Piledriver,5);var first=ProbeEnemy(0,2,1000);ProbeEnemy(.5f,2.2f,1000);ResolveBasic(hammer,activeCard,first,false);MechanicRequire(flights.Count==1,"Legendary Piledriver still trades away multi-launch");
            ProbeReset();var shared=ProbeHost(LoadoutCatalog.Fists,AugmentId.Flurry);actionWeapon=pairWeapon=shared;PairSlot=1;MechanicRequire(Share(shared)==1,"shared fists combine pair shares once");var otherFists=Deck.Acquire(LoadoutCatalog.Fists);pairWeapon=otherFists;MechanicRequire(Share(shared)==.5f,"distinct paired host gets half even if partner unaugmented");PairSlot=-1;MechanicRequire(Share(shared)==1,"separation restores full budget");
            ProbeReset();Debug.Log("BUILD_VARIETY_MECHANICS: "+mechanicAssertions+" production-effect assertions passed.");
        }
    }
}
