using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckSharedAugments()
        {
            foreach(int rank in new[]{1,2,3,4,5})
            {
                ProbeReset();var push=ProbeHost(LoadoutCatalog.Find("ForcePush"),AugmentId.Focused,rank);
                var front=ProbeEnemy(0,3);var flank=ProbeEnemy(3,1);actionTarget=front;Resolve(activeCard);
                MechanicRequire(front.health<500&&flank.health==500,"Focused push keeps side threats outside the actual cone");
                ProbeReset();var tether=ProbeHost(LoadoutCatalog.Find("VectorTether"),AugmentId.Focused,rank);
                front=ProbeEnemy(0,9);flank=ProbeEnemy(AugmentCatalog.TetherWidth(tether)+.2f,4);actionTarget=front;Resolve(activeCard);
                MechanicRequire(front.health<500&&flank.health==500&&activeCard.range>=10,"Focused Tether uses longer reach and a narrower corridor");

                foreach(string id in new[]{"ConductiveBrand","DrainBrand"})
                {
                    ProbeReset();var brand=ProbeHost(LoadoutCatalog.Find(id),AugmentId.FollowThrough,rank);
                    for(int i=0;i<5;i++)ProbeEnemy((i-2)*.25f,2.5f+i*.1f,1000);
                    actionTarget=enemies[0];Resolve(activeCard);
                    int opening=id=="ConductiveBrand"?marks.Count:drains.Count;
                    MechanicRequire(opening==(id=="ConductiveBrand"?2:1)&&FollowThroughReady!="","Follow-through preserves the declared immediate mark count");
                    actionSerial++;var gun=ProbeHost(LoadoutCatalog.Find("pistol"));ResolveBasic(gun,activeCard,enemies[0],false);
                    int total=id=="ConductiveBrand"?marks.Count:drains.Count;
                    MechanicRequire(total==(id=="ConductiveBrand"?5:3+(rank-1)/2)&&FollowThroughReady=="","weapon delivery prioritizes new targets, spends one preparation and uses source rarity");
                    MechanicRequire((id=="ConductiveBrand"?marks.Values.All(m=>m.owner==brand):drains.Values.All(m=>m.owner==brand)),"carried marks keep ability ownership");
                }
                ProbeReset();var volley=ProbeHost(LoadoutCatalog.Find("SuppressiveVolley"),AugmentId.FollowThrough,rank);
                front=ProbeEnemy(0,5);actionTarget=front;Resolve(activeCard);
                MechanicRequire(Mathf.Abs(front.stun-LoadoutCatalog.VolleyStagger(volley)*.55f)<.001f&&followBank>0,"Volley shifts stagger into one visible weapon preparation");

                ProbeReset();var wraps=ProbeHost(LoadoutCatalog.Find("wraps"),AugmentId.Livewire,rank);
                var ground=Deck.Acquire(LoadoutCatalog.Find("grounding"));Deck.Move(ground,PlaceKind.Armor);
                front=ProbeEnemy(0,2,10000);float baseDamage=activeCard.damage;
                ResolveBasic(wraps,activeCard,front,false);
                MechanicRequire(Mathf.Abs(10000-front.health-baseDamage-10*ItemRarity.Power[rank])<.01f&&BarrierAmount>0,"Conductor Wraps can self-start one Livewire discharge and Grounding barrier");
                float after=front.health,barrier=BarrierAmount;ResolveBasic(wraps,activeCard,front,false);
                MechanicRequire(Mathf.Abs(after-front.health-baseDamage)<.01f&&BarrierAmount==barrier,"same host cannot repeat discharge, refresh consumed marks, or farm barriers within one action");

                ProbeReset();var first=ProbeHost(LoadoutCatalog.Find("wraps"),AugmentId.Livewire,rank);
                var second=Deck.Acquire(LoadoutCatalog.Find("wraps"),rank);Deck.Move(second,PlaceKind.Weapon,1);
                Deck.Attach(Deck.Acquire(AugmentCatalog.Get(AugmentId.Livewire)),second);
                var left=ProbeEnemy(-.6f,2,10000);var right=ProbeEnemy(.6f,2,10000);
                ResolveBasic(first,activeCard,left,false);
                currentCredit=new KillCredit{item=Deck.Basics[1],weapon=second,action=actionSerial};ResolveBasic(second,LoadoutCatalog.Action(second),right,true);
                MechanicRequire(Mathf.Abs(left.health-right.health)<.01f&&CurrentTurn.livewire.Count==2,"distinct paired Livewire hosts each get one full discharge");

                ProbeReset();var rifle=ProbeHost(LoadoutCatalog.Find("rifle"),AugmentId.Livewire,3);
                var source=Deck.Acquire(LoadoutCatalog.Find("ConductiveBrand"),rank);
                var dead=ProbeEnemy(0,3,1);var survivor=ProbeEnemy(0,5,10000);var later=ProbeEnemy(0,7,10000);
                MarkEnemy(dead,source);MarkEnemy(survivor,source);MarkEnemy(later,source);
                ResolveBasic(rifle,activeCard,dead,false);
                MechanicRequire(dead.deathAge>=0&&CurrentTurn.livewire.Count==1&&marks[later.id].until>Elapsed,"lethal primary contact does not waste the two-handed discharge or repeat it on other penetration targets");
                MechanicRequire(Mathf.Abs(10000-survivor.health-activeCard.damage-10*ItemRarity.Power[rank])<.01f,"two-handed Livewire has its full marked-target payoff");
            }
            foreach(string id in new[]{"ArcBolt","ReapingCut","DrainBrand"})
            {
                ProbeReset();var host=ProbeHost(LoadoutCatalog.Find(id),AugmentId.Afterfield);
                var target=ProbeEnemy(0,2,10000);actionTarget=target;Resolve(activeCard);
                MechanicRequire(fields.Count==1&&fields[0].host==host,"new Afterfield host leaves a fixed field");
                var point=fields[0].point;hero.Root.position=new Vector3(10,.04f,0);Physics.SyncTransforms();
                float health=target.health;Elapsed+=.7f;StepSignatures(.7f);
                MechanicRequire(fields[0].point==point&&(id=="DrainBrand"?Mathf.Abs(drains[target.id].rate-LoadoutCatalog.SiphonRate(1)*.65f)<.00001f:target.health<health),"field remains in original space with weaker drain or delayed damage: "+id);
                if(id!="DrainBrand")MechanicRequire(fields[0].pulses==1,"damage fields have one discrete first pulse");
                Elapsed+=.7f;StepSignatures(.7f);if(id!="DrainBrand")MechanicRequire(fields[0].pulses==2,"damage fields have exactly two bounded pulses");
                Elapsed+=.7f;StepSignatures(.7f);MechanicRequire(fields.Count==0,"field expires on combat time");
                hero.Root.position=new Vector3(0,.04f,0);Physics.SyncTransforms();actionTarget=target;
                actionSerial++;currentCredit=new KillCredit{item=host,action=actionSerial};Resolve(activeCard);
                actionSerial++;currentCredit=new KillCredit{item=host,action=actionSerial};Resolve(activeCard);
                MechanicRequire(fields.Count==1,"recasting replaces the same owned host's previous field");
            }
            ProbeReset();var cut=ProbeHost(LoadoutCatalog.Find("ReapingCut"),AugmentId.Afterfield);
            var survey=Deck.Acquire(LoadoutCatalog.Find("surveyor"));Deck.Move(survey,PlaceKind.Helmet);
            var a=ProbeEnemy(0,2,10000);var b=ProbeEnemy(6,2,10000);actionTarget=a;Resolve(activeCard);FinishTurn();
            MechanicRequire(ContributionFor(survey).assists==0,"one opening target cannot trigger Surveyor");
            b.rig.Root.position=new Vector3(1,.04f,2.5f);Physics.SyncTransforms();Elapsed+=.7f;StepSignatures(.7f);
            MechanicRequire(ContributionFor(survey).assists==1,"a later distinct field target can complete Surveyor's one echo");
            Elapsed+=.7f;StepSignatures(.7f);MechanicRequire(ContributionFor(survey).assists==1,"later pulses cannot repeat Surveyor");

            ProbeReset();var gravity=ProbeHost(LoadoutCatalog.Find("GravityWell"));
            var treads=Deck.Acquire(LoadoutCatalog.Find("treads"));Deck.Move(treads,PlaceKind.Boots);Health=MaxHealth-30;
            var victim=ProbeEnemy(0,6,1);Hurt(victim,2,Vector3.zero,0);float beforeHeal=Health;
            var live=ProbeEnemy(0,5);actionTarget=live;Resolve(activeCard);
            MechanicRequire(motes.Count==1&&motes[0].pullOwner==gravity&&motes[0].point.z>5,"gathering starts visible mote movement without immediate remote healing");
            ProbeFrames(60);
            MechanicRequire(Health>beforeHeal&&motes.Count==0&&ContributionFor(gravity).assists>0,"gathered mote heals only upon actual pickup and credits setup");

            ProbeReset();var pistol=ProbeHost(LoadoutCatalog.Find("pistol"));
            var helmet=Deck.Acquire(LoadoutCatalog.Find("salvager"));Deck.Move(helmet,PlaceKind.Helmet);salvagerOwner=helmet;
            live=ProbeEnemy(0,4,1000);ResolveBasic(pistol,activeCard,live,false);
            MechanicRequire(salvagerOwner==helmet,"empty contact preserves the prepared body retrieval");
            var corpse=ProbeEnemy(.5f,3,1);KillEnemy(corpse,Vector3.zero,false);
            TryRetrieveBody(corpse.physics.Center);MechanicRequire(salvagerOwner==helmet,"a body already at the impact point cannot waste retrieval on a zero-distance pull");
            currentCredit=new KillCredit{item=Deck.Basics[0],weapon=pistol,action=++actionSerial};ResolveBasic(pistol,activeCard,live,false);
            MechanicRequire(salvagerOwner==null&&flights.Any(f=>f.body==corpse),"retrieval spends only on a real body and cannot rearm itself");

            var deck=new AbilityDeck(31);
            MechanicRequire(!BuildSynergies.HasConductiveSetup(deck),"unmarked build does not advertise a self-starting Livewire setup");
            var mantle=deck.Acquire(LoadoutCatalog.Find("grounding"));deck.Move(mantle,PlaceKind.Armor);
            MechanicRequire(!BuildSynergies.HasConductiveSetup(deck),"Grounding alone cannot bootstrap Livewire");
            var conductor=deck.Acquire(LoadoutCatalog.Find("wraps"));deck.Move(conductor,PlaceKind.Weapon,0);
            MechanicRequire(BuildSynergies.HasConductiveSetup(deck),"Conductor Wraps provide an independent Conductive setup");
        }
    }
}
