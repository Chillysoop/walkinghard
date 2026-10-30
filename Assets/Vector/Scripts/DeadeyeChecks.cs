using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckDeadeyeMechanics()
        {
            foreach(int rank in new[]{1,3,5})
            {
                ProbeReset();var volley=ProbeHost(LoadoutCatalog.Find("SuppressiveVolley"),null,rank);
                var first=ProbeEnemy(0,9);first.attack=.3f;var second=ProbeEnemy(.4f,10);var third=ProbeEnemy(-.5f,11);var excess=ProbeEnemy(.6f,12);var outside=ProbeEnemy(5,9);var far=ProbeEnemy(0,16);var entering=ProbeEnemy(0,7);entering.entering=true;actionTarget=first;
                Resolve(activeCard);
                MechanicRequire(first.health<500&&second.health<500&&third.health<500&&excess.health==500&&outside.health==500&&far.health==500&&entering.health==500,"Volley range, fan, three-target cap and hidden entrance exclusion");
                MechanicRequire(first.attack<0&&Mathf.Abs(first.stun-LoadoutCatalog.VolleyStagger(volley))<.001f&&first.physics.LinearVelocity.sqrMagnitude<.001f,"Volley interrupts without forced knockback; rarity scales control");
                ProbeReset();volley=ProbeHost(LoadoutCatalog.Find("SuppressiveVolley"),AugmentId.Focused,rank);first=ProbeEnemy(0,9);second=ProbeEnemy(.4f,10);actionTarget=first;Resolve(activeCard);
                MechanicRequire(first.health<500&&second.health==500&&Mathf.Abs(first.stun-1.5f*LoadoutCatalog.VolleyStagger(volley))<.001f,"Focused Volley trades target count for interrupt duration");
                ProbeReset();var gun=ProbeHost(LoadoutCatalog.Find("pistol"));var harness=Deck.Acquire(LoadoutCatalog.Find("recoil"),rank);Deck.Move(harness,PlaceKind.Armor);
                Replenish(gun);MechanicRequire(recoilGuard==0,"full magazine cannot arm Recoil");
                ammunition[gun]=0;Replenish(gun);MechanicRequire(recoilGuard==LoadoutCatalog.RecoilBlock(harness)&&recoilShot==0,"actual refill arms guard, not free damage");
                var attacker=ProbeEnemy(0,2);float before=Health;ReceiveDamage(attacker,50);
                float blocked=LoadoutCatalog.RecoilBlock(harness);
                MechanicRequire(Mathf.Abs(before-Health-(50-blocked))<.001f&&Mathf.Abs(ContributionFor(harness).absorption-blocked)<.001f&&recoilGuard==0&&recoilShot==LoadoutCatalog.RecoilDamage(harness),"Recoil block attribution and one stored shot");
                ReceiveDamage(attacker,10);MechanicRequire(Mathf.Abs(before-Health-(60-blocked))<.001f,"guard protects only one hit");
                var distant=ProbeEnemy(0,8);float hp=distant.health;ResolveBasic(gun,LoadoutCatalog.Action(gun),distant,false);
                MechanicRequire(recoilShot==0&&Mathf.Abs(ContributionFor(harness).direct-LoadoutCatalog.RecoilDamage(harness))<.001f&&hp-distant.health>LoadoutCatalog.Action(gun).damage,"gun hit spends armor-owned damage once");
                float contributed=ContributionFor(harness).direct;ResolveBasic(gun,LoadoutCatalog.Action(gun),distant,false);
                MechanicRequire(ContributionFor(harness).direct==contributed,"second shot cannot repeat stored payoff");
                actionSerial++;currentCredit=new KillCredit{item=Deck.Basics[0],weapon=gun,action=actionSerial};ammunition[gun]=0;Replenish(gun);Elapsed=recoilUntil+.01f;before=Health;ReceiveDamage(attacker,10);
                MechanicRequire(before-Health==10&&recoilShot==0,"expired reload guard gives neither block nor shot");
            }
            ProbeReset();var pistol=ProbeHost(LoadoutCatalog.Find("pistol"));var recoil=Deck.Acquire(LoadoutCatalog.Find("recoil"));Deck.Move(recoil,PlaceKind.Armor);var paired=Deck.Acquire(LoadoutCatalog.Find("pistol"));Deck.Move(paired,PlaceKind.Weapon,1);ammunition[pistol]=ammunition[paired]=0;Replenish(pistol);Replenish(paired);
            MechanicRequire(recoilGuard==12&&InteractionNotes["Reload armed Recoil guard"]==1,"paired reload grants one shared guard");
            var enemy=ProbeEnemy(0,2);var barrier=Deck.Acquire(LoadoutCatalog.Find("Counterguard"));AddBarrier(barrier,20,5);ReceiveDamage(enemy,10);
            MechanicRequire(recoilGuard==12&&recoilShot==0,"fully absorbed hit preserves guard and cannot charge a shot");
            barriers.Clear();ReceiveDamage(enemy,5);MechanicRequire(recoilGuard==0&&recoilShot==12&&ContributionFor(recoil).absorption==5,"small hit discards unused block capacity");
            currentCredit=new KillCredit{item=barrier,action=++actionSerial};Hurt(enemy,1,Vector3.zero,0);MechanicRequire(recoilShot==12,"ability damage cannot consume Recoil shot");
            Deck.Move(recoil,PlaceKind.Inventory);currentCredit=new KillCredit{item=Deck.Basics[0],weapon=pistol,action=++actionSerial};Hurt(enemy,1,Vector3.zero,0);MechanicRequire(ContributionFor(recoil).direct==0,"unequipped armor cannot spend a shot");
            ProbeReset();var arc=Deck.Acquire(LoadoutCatalog.Find("ArcBolt"));Deck.Move(arc,PlaceKind.Sequence,2);var stored=Deck.Acquire(LoadoutCatalog.Find("capacitor"));
            var links=BuildSynergies.Find(Deck,LoadoutCatalog.Find("SuppressiveVolley"));
            MechanicRequire(links.Any(l=>l.partner==arc)&&!links.Any(l=>l.partner==stored),"synergy hints use actual equipped consumers, not storage");
            MechanicRequire(!BuildSynergies.Find(Deck,LoadoutCatalog.Find("SuppressiveVolley"),arc).Any(l=>l.partner==arc),"displaced partner is removed from synergy hint");
            var equippedRecoil=Deck.Acquire(LoadoutCatalog.Find("recoil"));Deck.Move(equippedRecoil,PlaceKind.Armor);
            MechanicRequire(!BuildSynergies.Find(Deck,LoadoutCatalog.Find("duelist")).Any(l=>l.partner==equippedRecoil),"exclusive armor cannot partner with its replacement");
            MechanicRequire(!BuildSynergies.Find(Deck,LoadoutCatalog.Find("recoil")).Any(),"unarmed build does not claim a reload synergy");
            var rifle=Deck.Acquire(LoadoutCatalog.Find("rifle"));Deck.Move(rifle,PlaceKind.Weapon,0);
            MechanicRequire(BuildSynergies.Find(Deck,LoadoutCatalog.Find("recoil")).Count(l=>l.partner==rifle)==1,"two-handed gun appears once as an armor partner");
            var discharge=Deck.Acquire(LoadoutCatalog.Find("Counterguard"));Deck.Attach(Deck.Acquire(AugmentCatalog.Get(AugmentId.Discharge)),discharge);
            MechanicRequire(!BuildSynergies.Find(Deck,discharge.def,null,discharge).Any(l=>l.partner==rifle),"stored Discharge cannot claim the banked-weapon synergy it replaces");
            ResetSynergyCues();SynergyCue("mark","TEST DISCHARGE","actual event");SynergyCue("guard","TEST GUARD","later event");
            MechanicRequire(simulation?PayoffTitle=="":PayoffTitle=="TEST DISCHARGE","simulations suppress cues; simultaneous play payoffs cannot flood or overwrite them");
            ResetSynergyCues();MechanicRequire(!PayoffVisible&&PayoffTitle=="","new runs clear synergy cues");
        }
    }
}
