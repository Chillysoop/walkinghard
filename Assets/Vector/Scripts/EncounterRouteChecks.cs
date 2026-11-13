using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckEncounterRoutes()
        {
            // Reward bias survives rerolls without changing draft type, rarity rules or unlocks.
            foreach(var route in new[]{EncounterRoute.ShieldLine,EncounterRoute.VolatileBreach})
            {
                for(int seed=0;seed<12;seed++)
                {
                    var deck=new AbilityDeck(seed);deck.Open(DraftKind.Mixed,true,RouteReward(route));
                    for(int roll=0;roll<2;roll++)
                    {
                        MechanicRequire(deck.Offers.Length==3&&deck.Offers.Select(o=>o.def.id).Distinct().Count()==3&&deck.Offers.All(o=>o.rank>=1&&o.rank<=5&&o.def.kind!=ItemKind.Ultimate),"routed pick is three distinct ranked offers; ultimate stays gated");
                        MechanicRequire(deck.Offers.Take(2).All(o=>route==EncounterRoute.VolatileBreach?o.def.kind==ItemKind.Ability:o.def.kind==ItemKind.Weapon||o.def.kind==ItemKind.Rig||LoadoutCatalog.IsArmor(o.def.kind)),"two offers honor the route emphasis, including rerolls");
                        deck.Reroll();
                    }
                }
            }
            var savedCases=controlledCases;bool savedSimulation=simulation;
            try
            {
            // Probe ordinary checkpoint rules even when the runner uses fixed experimental loadouts.
            controlledCases=null;simulation=false;
            int oldWave=Wave;var oldState=State;float oldWaveElapsed=WaveElapsed,oldSpeed=CombatSpeed;bool oldQa=qa;
            ProbeReset();ResetRoutes();qa=false;Wave=2;WaveElapsed=20;State=RunState.Running;BeginWalkingFork();int rolls=Deck.Rerolls;
            MechanicRequire(RoutePending&&ForkChoice==EncounterRoute.ShieldLine&&ForkSeconds==ForkDecisionSeconds,"moving fork has a visible deterministic default");
            ConfirmDraft();Reroll();MechanicRequire(RoutePending&&Deck.Rerolls==rolls,"confirm and reroll cannot skip the moving fork");
            MechanicRequire(ChooseRoute(EncounterRoute.VolatileBreach)&&ChooseRoute(EncounterRoute.ShieldLine)&&!ChooseRoute(EncounterRoute.Standard),"selection can change before lock and rejects invalid routes");
            SetCombatSpeed(4);MechanicRequire(EffectiveCombatSpeed==1&&CombatSpeed==4,"fork caps actual approach speed without changing saved preference");
            StepWalkingFork(2);float remaining=ForkSeconds;TogglePause();AdvanceCombat(.1f);StepWalkingFork(20);
            MechanicRequire(State==RunState.Paused&&ForkSeconds==remaining,"pause freezes route deadline");TogglePause();
            StepWalkingFork(4);MechanicRequire(!RoutePending&&SelectedRoute==EncounterRoute.ShieldLine&&State==RunState.Running,"deadline commits once while combat continues");
            OpenCheckpoint();MechanicRequire(Deck.Draft==DraftKind.Mixed&&pendingDrafts.Count==1&&Deck.Rerolls==rolls+1,"committed route opens exactly one mixed pick and grants one reroll");
            MechanicRequire(!ChooseRoute(EncounterRoute.VolatileBreach),"route cannot be changed to reroll rewards");
            Wave=3;MechanicRequire(ActiveRoute==EncounterRoute.ShieldLine,"route applies to next wave");Wave=4;MechanicRequire(ActiveRoute==EncounterRoute.Standard,"route expires after one wave");
            Wave=5;State=RunState.Running;BeginWalkingFork();StepWalkingFork(ForkDecisionSeconds);
            MechanicRequire(SelectedRoute==EncounterRoute.ShieldLine&&!RoutePending,"no input follows the advertised path");
            OpenCheckpoint();MechanicRequire(pendingDrafts.Single()==DraftKind.Augment,"checkpoint five retains its augment pick");
            WaveElapsed=forkStarted+10;MechanicRequire(EffectiveCombatSpeed==4,"approach restores the preferred speed");
            ResetRoutes();pendingDrafts.Clear();Wave=oldWave;State=oldState;WaveElapsed=oldWaveElapsed;SetCombatSpeed(oldSpeed);qa=oldQa;
            }
            finally {controlledCases=savedCases;simulation=savedSimulation;}

            ProbeReset();var host=ProbeHost(LoadoutCatalog.Find("MassDriver"));var shield=ProbeEnemy(0,2);shield.shieldBearer=true;shield.rig.Root.rotation=Quaternion.Euler(0,180,0);
            Hurt(shield,40,Vector3.zero,0);MechanicRequire(shield.health==480,"frontal shield halves damage");
            Control(shield,.1f,Vector3.zero);Hurt(shield,40,Vector3.zero,0);MechanicRequire(shield.health==440,"stagger exposes full follow-up damage");
            shield.shieldOpenUntil=Elapsed-1;shield.rig.Root.rotation=Quaternion.identity;Hurt(shield,40,Vector3.zero,0);MechanicRequire(shield.health==400,"rear hits bypass shield");
            shield.rig.Root.rotation=Quaternion.Euler(0,180,0);Hurt(shield,40,Vector3.zero,0);MechanicRequire(shield.health==380,"guard returns after exposure expires");
            ProbeReset();host=ProbeHost(LoadoutCatalog.Find("MassDriver"));var carrier=ProbeEnemy(0,2,10);carrier.volatileCarrier=true;
            var victims=Enumerable.Range(0,6).Select(i=>ProbeEnemy((i-2)*.35f,3,20)).ToArray();Launch(carrier);var flight=flights.Single();int before=Kills,kills=KillsFor(host);
            DetonateCarrier(flight);MechanicRequire(Kills-before==5&&KillsFor(host)-kills==5&&victims.Count(e=>e.deathAge>=0)==4,"carrier plus four blast victims credit the launching item once");
            MechanicRequire(flight.chain.remaining==2&&CurrentTurn.extra==8,"blast shares cascade and derived-hit budgets");
            DetonateCarrier(flight);MechanicRequire(Kills-before==5&&flights.Count==1,"carrier never detonates twice or recursively launches victims");
            ProbeReset();host=ProbeHost(LoadoutCatalog.Find("MassDriver"));carrier=ProbeEnemy(0,2);carrier.volatileCarrier=true;Launch(carrier);flight=flights.Single();flight.chain.remaining=0;var untouched=ProbeEnemy(0,3);DetonateCarrier(flight);
            MechanicRequire(untouched.health==untouched.maxHealth,"exhausted cascade cannot deal more blast damage");
            ProbeReset();ResetRoutes();
        }
        internal void PrepareRouteReview()
        {Wave=2;OpeningPicks=2;State=RunState.Running;BeginWalkingFork();}
        internal void CommitRouteReview(){CommitWalkingFork();OpenCheckpoint();}
        internal void PrepareRouteActors(bool exposed=false,bool explode=false)
        {
            // Recreate the motor: a pending MovePosition from the completed wave
            // would otherwise carry the hero/camera away from these local targets.
            Begin(true);ProbeReset();ProbeHost(LoadoutCatalog.Find("MassDriver"));Wave=3;State=RunState.Running;qaHolding=true;
            actionTarget=pairTarget=null;ActiveSlot=PairSlot=-1;sequencePointer=expectedSlot=0;recovery=.2f;
            WaveElapsed=0;spawnTimer=1000;
            SetCombatSpeed(.5f);cameraRig.transform.position=HeroPosition+tuning.cameraOffset;cameraVelocity=Vector3.zero;AimCamera();
            var shield=ProbeEnemy(-1.4f,5);shield.shieldBearer=true;shield.rig.Root.rotation=Quaternion.Euler(0,180,0);shield.rig.EncounterEquipment(true,false);shield.rig.ShieldGuard(exposed);
            if(exposed)shield.shieldOpenUntil=Elapsed+2;
            var carrier=ProbeEnemy(1.4f,5);carrier.volatileCarrier=true;carrier.rig.Root.rotation=Quaternion.Euler(0,180,0);carrier.rig.EncounterEquipment(false,true);
            if(explode){ProbeEnemy(1,6,20);ProbeEnemy(2,6,20);Launch(carrier);DetonateCarrier(flights.Single());}
            Physics.SyncTransforms();
        }
    }
}
