using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckSiphonBalance()
        {
            // Normal production damage/healing paths, including attribution and delayed credits.
            ProbeReset();var focusedBrand=ProbeHost(LoadoutCatalog.Find("DrainBrand"),AugmentId.Focused);ProbeEnemy(0,2);ProbeEnemy(.5f,2);ProbeEnemy(-.5f,2);Resolve(activeCard);
            MechanicRequire(drains.Count==2&&drains.Values.All(m=>Mathf.Abs(m.rate-.056f)<.001f),"Focused Drain Brand trades coverage for stronger real healing marks");
            foreach(int rank in new[]{1,2,3,4,5})
            {
                ProbeReset();var blade=ProbeHost(LoadoutCatalog.Find("siphonblade"),null,rank);Health=MaxHealth*.6f;
                MechanicRequire(Deck.Lifesteal==0&&LifestealRate==0,"equipped blade is not global Lifesteal on the character sheet");
                var otherBlade=Deck.Acquire(blade.def,5);Deck.Move(otherBlade,PlaceKind.Weapon,1);
                var isolated=ProbeEnemy(0,2,500);float initial=Health;Hurt(isolated,10,Vector3.zero,0);
                MechanicRequire(Mathf.Abs(Health-initial-10*LoadoutCatalog.SiphonRate(rank))<.002f,"blade damage uses its own rarity, never sums the offhand blade");
                var unrelated=Deck.Acquire(LoadoutCatalog.Find("ArcBolt"));currentCredit=new KillCredit{item=unrelated,action=++actionSerial};initial=Health;Hurt(isolated,10,Vector3.zero,0);
                MechanicRequire(Health==initial,"unrelated ability damage against unmarked enemies cannot borrow equipped blade Lifesteal");
                currentCredit=new KillCredit{item=Deck.Basics[1],weapon=Deck.Fists,action=++actionSerial};Hurt(isolated,10,Vector3.zero,0);
                MechanicRequire(Health==initial,"an unrelated weapon cannot borrow blade Lifesteal");
                DrainMark(isolated,blade);currentCredit=new KillCredit{item=unrelated,action=++actionSerial};Hurt(isolated,10,Vector3.zero,0);
                MechanicRequire(Mathf.Abs(Health-initial-10*LoadoutCatalog.SiphonRate(rank))<.002f,"blade marks still let unrelated abilities drain the marked target");
                drains.Clear();currentCredit=new KillCredit{item=Deck.Basics[0],weapon=blade,action=++actionSerial,hop=1};initial=Health;Hurt(isolated,10,Vector3.zero,0);
                MechanicRequire(Mathf.Abs(Health-initial-10*LoadoutCatalog.SiphonRate(rank))<.002f,"delayed collision damage retains its originating blade Lifesteal");
                currentCredit=new KillCredit{item=Deck.Basics[0],weapon=blade,action=++actionSerial,augment=true};initial=Health;Hurt(isolated,10,Vector3.zero,0);
                MechanicRequire(Mathf.Abs(Health-initial-10*LoadoutCatalog.SiphonRate(rank))<.002f,"blade augment followups retain their own Lifesteal");
                currentCredit=new KillCredit{item=Deck.Basics[0],weapon=blade,action=++actionSerial};
                var victim=ProbeEnemy(0,2,1);float before=Health;Hurt(victim,1000,Vector3.zero,0,true);
                MechanicRequire(Mathf.Abs(Health-before-LoadoutCatalog.SiphonRate(rank))<.002f,"Lifesteal uses actual health damage, not overkill, at every rarity");
                before=Health;Hurt(victim,1000,Vector3.zero,0);MechanicRequire(Health==before,"corpses cannot heal");
                var live=ProbeEnemy(0,2,100000);Hurt(live,10000,Vector3.zero,0);
                MechanicRequire(Mathf.Abs(CurrentTurn.siphonHealed-MaxHealth*.01f)<.002f,"all damage in an action shares the healing cap");
                before=Health;currentCredit.hop=1;Hurt(live,10000,Vector3.zero,0);MechanicRequire(Health==before,"late collision credit cannot reset the action healing budget");
                currentCredit=new KillCredit{item=Deck.Basics[1],weapon=blade,action=currentCredit.action};Hurt(live,10000,Vector3.zero,0);MechanicRequire(Health==before,"paired basic shares the originating action budget");
                currentCredit.action=++actionSerial;Hurt(live,10000,Vector3.zero,0);MechanicRequire(Health>before,"a new action gets its own healing budget");
                Health=MaxHealth-.1f;currentCredit.action=++actionSerial;Hurt(live,100,Vector3.zero,0);MechanicRequire(Mathf.Abs(Health-MaxHealth)<.002f,"healing cannot exceed maximum health");

                ProbeReset();var brand=ProbeHost(LoadoutCatalog.Find("DrainBrand"),null,rank);var marked=ProbeEnemy(0,2);DrainMark(marked,brand);DrainMark(marked,brand);
                Health=MaxHealth*.6f;before=Health;Hurt(marked,10,Vector3.zero,0);
                MechanicRequire(Mathf.Abs(Health-before-10*LoadoutCatalog.SiphonRate(rank))<.002f,"mark works without another Lifesteal source and refreshes without stacking");
                Elapsed=drains[marked.id].until+.01f;currentCredit.action=++actionSerial;before=Health;Hurt(marked,10,Vector3.zero,0);MechanicRequire(Health==before,"expired mark stops healing");

                ProbeReset();var armor=Deck.Acquire(LoadoutCatalog.Find("reclaimer"),rank);Deck.Move(armor,PlaceKind.Armor);var enemy=ProbeEnemy(0,2);
                Health=MaxHealth*.6f;before=Health;Hurt(enemy,10,Vector3.zero,0);float normal=Health-before;
                MechanicRequire(Mathf.Abs(normal-10*new[]{.03f,.04f,.05f,.06f,.07f}[rank-1])<.002f,"Reclaimer uses its approved independent rarity rates");
                Health=MaxHealth*.4f;currentCredit.action=++actionSerial;before=Health;Hurt(enemy,10,Vector3.zero,0);
                MechanicRequire(Mathf.Abs(Health-before-normal*1.5f)<.002f,"Reclaimer multiplies only its own Lifesteal by 1.5 below half health");
            }
            ProbeReset();var sword=ProbeHost(LoadoutCatalog.Find("siphonblade"));var dying=ProbeEnemy(0,2,1);var neighbor=ProbeEnemy(.8f,2.2f);ResolveBasic(sword,activeCard,dying,false);
            MechanicRequire(drains.ContainsKey(neighbor.id)&&InteractionNotes.ContainsKey("Sever spread Siphon"),"Siphon Blade sever spreads a real mark");
            ProbeReset();var boots=Deck.Acquire(LoadoutCatalog.Find("predator"));Deck.Move(boots,PlaceKind.Boots);var brandHost=ProbeHost(LoadoutCatalog.Find("DrainBrand"));var target=ProbeEnemy(0,2);
            Health=MaxHealth;DrainMark(target,brandHost);Hurt(target,10,Vector3.zero,0);MechanicRequire(!predatorReady,"overhealing cannot arm Predator");
            Health=MaxHealth*.6f;Hurt(target,10,Vector3.zero,0);Hurt(target,10,Vector3.zero,0);MechanicRequire(predatorReady,"actual healing arms one Predator charge");
            float speed=TurnSpeed(false,Deck.Fists);MechanicRequire(predatorReady&&Mathf.Abs(speed-Deck.Speed)<.001f,"weapons preserve Predator charge");
            var ult=Deck.Acquire(LoadoutCatalog.Find("harvest"));Deck.Move(ult,PlaceKind.Ultimate);TurnSpeed(true,ult);MechanicRequire(predatorReady,"ultimate preserves Predator charge");
            speed=TurnSpeed(false,brandHost);MechanicRequire(!predatorReady&&Mathf.Abs(speed-Deck.Speed*1.12f)<.001f,"next ability consumes exactly one haste boost");
            foreach(int rank in new[]{1,2,3,4,5})
            {
                ProbeReset();var harvest=Deck.Acquire(LoadoutCatalog.Find("harvest"),rank);Deck.Move(harvest,PlaceKind.Ultimate);Health=MaxHealth*.5f;
                currentCredit=new KillCredit{item=harvest,action=++actionSerial};var enemy=ProbeEnemy(0,2,100000);float before=Health;Hurt(enemy,10000,Vector3.zero,0);
                MechanicRequire(Mathf.Abs(Health-before-HarvestHealingLimit(harvest))<.002f,"Harvest uses its own rarity-scaled limit");
                before=Health;Hurt(enemy,10000,Vector3.zero,0);MechanicRequire(Health==before,"Harvest cannot heal beyond its pulse budget");
            }
            ProbeReset();MechanicRequire(drains.Count==0&&!predatorReady&&LifestealHealing==0,"new run clears all Siphon state");

            foreach(int rank in new[]{1,2,3,4,5})
            {
                ProbeReset();var gun=ProbeHost(LoadoutCatalog.Find("pistol"));var red=Deck.Acquire(LoadoutCatalog.Find("redline"),rank);Deck.Move(red,PlaceKind.Ultimate);ammunition[gun]=1;ResolveUltimate();
                MechanicRequire(Ammo(gun)==6&&redlineTurns==6&&LoadoutCatalog.Cooldown(red)==18,"Redline refills and grants six turns without changing recharge");
                ActiveSlot=5;FinishTurn();MechanicRequire(redlineTurns==6,"the ultimate activation does not consume an accelerated turn");
                ActiveSlot=0;PairSlot=1;
                for(int i=6;i>0;i--){float expected=Deck.Speed*(1+.2f*(1+.15f*(rank-1)));MechanicRequire(Mathf.Abs(TurnSpeed(false,gun)-expected)<.001f,"Redline preserves rarity haste on every accelerated turn");FinishTurn();MechanicRequire(redlineTurns==i-1,"a paired sequence action consumes only one Redline charge");}
                MechanicRequire(Mathf.Abs(TurnSpeed(false,gun)-Deck.Speed)<.001f,"Redline haste ends after exactly six sequence turns");
            }

            ProbeHost(LoadoutCatalog.Fists);var primary=ProbeEnemy(0,2,1000);var secondary=ProbeEnemy(.5f,2,1000);counterBank=20;amplifierBank=18;
            DirectSignatures(primary,1,false);
            MechanicRequire(Mathf.Abs(primary.health-962)<.001f&&Mathf.Abs(secondary.health-977)<.001f,"Counterguard secondaries get 25 percent while Amplifier remains full");
            foreach(var id in new[]{AugmentId.Flurry,AugmentId.Crosscut})
            {
                ProbeReset();var host=ProbeHost(LoadoutCatalog.Find(id==AugmentId.Flurry?"fists":"sword"),id);var enemy=ProbeEnemy(0,2,1000);actionTarget=enemy;
                BeginAuthoredTurn(false);MechanicRequire(Mathf.Abs(actionDuration-nativeDuration)<.001f,"weapon augment adds no action duration");
                actionAge=nativeDuration*.95f;float before=enemy.health;AdvanceExtension();float expected=activeCard.damage*(id==AugmentId.Flurry?.30f:.35f);
                MechanicRequire(Mathf.Abs(before-enemy.health-expected)<.002f,"inline contacts both resolve even across one large tick");before=enemy.health;AdvanceExtension();MechanicRequire(enemy.health==before,"inline contacts cannot repeat on the next tick");
            }
            ProbeReset();var pistol=ProbeHost(LoadoutCatalog.Find("pistol"),AugmentId.BoreRounds,3);var lineTarget=ProbeEnemy(0,2);var ricochet=ProbeEnemy(2,2.5f);ResolveBasic(pistol,activeCard,lineTarget,false);
            MechanicRequire(lineTarget.health<500&&ricochet.health<500,"Bore adds piercing without removing native ricochet");BeginAuthoredTurn(false);MechanicRequire(!borePending,"Bore does not add a focusing delay");
            Deck.Attach(Deck.Acquire(AugmentCatalog.Get(AugmentId.BreachLoad)),pistol);MechanicRequire(Mathf.Abs(ReloadDuration(pistol)-1.2f)<.001f,"Breach Load preserves normal reload duration");
            ProbeReset();Deck.Fused+=MergeKillCounts;var firstCopy=Deck.Acquire(LoadoutCatalog.Find("siphonblade"));var secondCopy=Deck.Acquire(firstCopy.def);ContributionFor(firstCopy).healing=3;ContributionFor(secondCopy).healing=7;Deck.Acquire(firstCopy.def);
            var fused=Deck.Items.Single(i=>i.def.id=="siphonblade");MechanicRequire(fused.rank==2&&ContributionFor(fused).healing==10,"fusion preserves healing on the surviving item");
        }
    }
}
