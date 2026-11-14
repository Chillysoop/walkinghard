using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public enum EncounterRoute { Standard, ShieldLine, VolatileBreach, SalvageStop, Overtime, Auction, PressureWorks, Rescue, RepairLine, FreightYard, Reclaimers, EliteShift }
    public enum RewardEmphasis { None, Equipment, Abilities }
    public sealed partial class VectorEncounter
    {
        public bool RoutePending {get;private set;}
        public EncounterRoute SelectedRoute {get;private set;}
        int routedWave,routeSpawn;
        public int ShieldBearersSpawned {get;private set;}
        public int VolatileCarriersSpawned {get;private set;}
        public int CarrierDetonations {get;private set;}
        public EncounterRoute ActiveRoute=>Wave==routedWave&&!RegionalEncounters.IsStop(SelectedRoute)?(simulation&&!ExpeditionActive&&Controlled==null&&simulationEncounterRoute==0?EncounterRoute.Standard:SelectedRoute):EncounterRoute.Standard;
        public static string RouteName(EncounterRoute route)=>RegionalEncounters.Find(route)?.name??(route==EncounterRoute.SalvageStop?"SALVAGE STOP":route==EncounterRoute.Overtime?"OVERTIME LOCKERS":route==EncounterRoute.ShieldLine?"SHIELD LINE":route==EncounterRoute.VolatileBreach?"VOLATILE BREACH":"STANDARD");
        public static string RouteDescription(EncounterRoute route)=>RegionalEncounters.Find(route)?.description??(route==EncounterRoute.Overtime?"Risk the unbanked haul at overtime lockers, or leave safely with what you have.":route==EncounterRoute.SalvageStop?"Spend salvage on a repair or a matching equipment copy.":route==EncounterRoute.ShieldLine?
            "Shield bearers advance from the horizon. Stagger opens their guard for 2 seconds; attacks from behind bypass it.":
            "Carriers emerge from side doors. Launch one to arm its explosive pack; impact or a 1.3s flight detonates it.");
        public static RewardEmphasis RouteReward(EncounterRoute route)=>RegionalEncounters.Find(route)?.reward??(route==EncounterRoute.ShieldLine?RewardEmphasis.Equipment:route==EncounterRoute.VolatileBreach?RewardEmphasis.Abilities:RewardEmphasis.None);
        void ResetRoutes(){ResetWalkingFork();RoutePending=false;SelectedRoute=EncounterRoute.Standard;routedWave=routeSpawn=0;ShieldBearersSpawned=VolatileCarriersSpawned=CarrierDetonations=0;}
        public bool ChooseRoute(EncounterRoute route)
        {
            if(State!=RunState.Running||!RoutePending||(route!=ForkLeft&&route!=ForkRight))return false;
            ForkChoice=route;forkVisual?.Select(route==ForkLeft?0:1);return true;
        }
        void ConfigureRouteEnemy(Enemy e)
        {
            if((ActiveRoute!=EncounterRoute.ShieldLine&&ActiveRoute!=EncounterRoute.VolatileBreach&&ActiveRoute!=EncounterRoute.EliteShift)||e.boss||e.shooter)return;
            if(routeSpawn++%4!=0)return;
            e.shieldBearer=ActiveRoute==EncounterRoute.ShieldLine||ActiveRoute==EncounterRoute.EliteShift;
            e.volatileCarrier=ActiveRoute==EncounterRoute.VolatileBreach;
            if(e.shieldBearer)ShieldBearersSpawned++;else VolatileCarriersSpawned++;
            e.rig.EncounterEquipment(e.shieldBearer,e.volatileCarrier);
        }
        float ShieldDamage(Enemy e,float damage,Vector3 impulse,Vector3? hitOrigin=null)
        {
            if(!e.shieldBearer||e.shieldOpenUntil>Elapsed)return damage;
            Vector3 source=hitOrigin.HasValue?hitOrigin.Value-e.rig.Root.position:(currentCredit?.hop??0)>0&&impulse.sqrMagnitude>.01f?-impulse:HeroPosition-e.rig.Root.position;
            source.y=0;
            if(Vector3.Dot(e.rig.Root.forward,source.normalized)<.35f)return damage;
            Note("Shield guard reduced frontal damage");return damage*.5f;
        }
        void ExposeShield(Enemy e,float stagger)
        {
            if(!e.shieldBearer||stagger<=0)return;
            if(e.shieldOpenUntil<=Elapsed)Note("Stagger exposed a shield bearer",true);
            e.shieldOpenUntil=Mathf.Max(e.shieldOpenUntil,Elapsed+2);
        }
        void DetonateCarrier(Flight flight)
        {
            var body=flight.body;if(!body.volatileCarrier||body.detonated)return;
            body.detonated=true;CarrierDetonations++;var saved=currentCredit;var credit=flight.chain.credit;
            currentCredit=new KillCredit{item=credit?.item,weapon=credit?.weapon,action=credit?.action??0,hop=flight.hop+1,derived=true};
            try
            {
                Vector3 center=body.physics.Center;
                if(body.deathAge<0){body.health=0;KillEnemy(body,Vector3.up*2,false);}
                Note("Launched carrier detonated",true);
                fx.Ring(center,3.5f,VisualFactory.Orange,.4f);fx.Burst(center,VisualFactory.Orange,18,6);fx.Play(1);
                // Share the original six-hit cascade AND twelve-derived-hit action budgets.
                // No secondary launches: one carrier cannot recursively arm a roomful.
                foreach(var target in enemies.Where(e=>!flight.chain.hit.Contains(e.id)&&Vector3.Distance(e.physics.Center,center)<3.5f).OrderBy(e=>(e.physics.Center-center).sqrMagnitude).Take(4).ToArray())
                {
                    if(flight.chain.remaining<=0||CurrentTurn.extra<=0)break;
                    flight.chain.hit.Add(target.id);flight.chain.remaining--;
                    DerivedHit(target,26*flight.energy,(target.physics.Center-center).normalized*5,.4f);
                    if(target.deathAge>=0)CollisionKills++;
                }
            }
            finally {currentCredit=saved;}
        }
    }
}
