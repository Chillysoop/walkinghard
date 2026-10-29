using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public enum EncounterRoute { Standard, ShieldLine, VolatileBreach }
    public enum RewardEmphasis { None, Equipment, Abilities }
    public sealed partial class VectorEncounter
    {
        public bool RoutePending {get;private set;}
        public EncounterRoute SelectedRoute {get;private set;}
        int routedWave,routeSpawn;
        public int ShieldBearersSpawned {get;private set;}
        public int VolatileCarriersSpawned {get;private set;}
        public int CarrierDetonations {get;private set;}
        public EncounterRoute ActiveRoute=>Wave==routedWave?(simulation&&Controlled==null&&simulationEncounterRoute==0?EncounterRoute.Standard:SelectedRoute):EncounterRoute.Standard;
        public static string RouteName(EncounterRoute route)=>route==EncounterRoute.ShieldLine?"SHIELD LINE":route==EncounterRoute.VolatileBreach?"VOLATILE BREACH":"STANDARD";
        public static string RouteDescription(EncounterRoute route)=>route==EncounterRoute.ShieldLine?
            "Shield bearers advance from the horizon. Stagger opens their guard for 2 seconds; attacks from behind bypass it.":
            "Carriers emerge from side doors. Launch one to arm its explosive pack; impact or a 1.3s flight detonates it.";
        public static RewardEmphasis RouteReward(EncounterRoute route)=>route==EncounterRoute.Standard?RewardEmphasis.None:route==EncounterRoute.ShieldLine?RewardEmphasis.Equipment:RewardEmphasis.Abilities;
        void ResetRoutes(){RoutePending=false;SelectedRoute=EncounterRoute.Standard;routedWave=routeSpawn=0;ShieldBearersSpawned=VolatileCarriersSpawned=CarrierDetonations=0;}
        public bool ChooseRoute(EncounterRoute route)
        {
            if(State!=RunState.Drafting||!RoutePending||route==EncounterRoute.Standard)return false;
            SelectedRoute=route;routedWave=Wave+1;RoutePending=false;
            Deck.Open(pendingDrafts.Dequeue(),true,RouteReward(simulation&&simulationRewardRoute>=0?(EncounterRoute)simulationRewardRoute:route));return true;
        }
        void ConfigureRouteEnemy(Enemy e)
        {
            if(ActiveRoute==EncounterRoute.Standard||e.boss||e.shooter)return;
            if(routeSpawn++%4!=0)return;
            e.shieldBearer=ActiveRoute==EncounterRoute.ShieldLine;
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
    public sealed partial class PrototypeHUD
    {
        void RouteChoice(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.006f,.018f,.028f,.86f));
            float x=w*.5f-530,y=h*.5f-200;
            Panel(new Rect(x,y,1060,400));
            Label(new Rect(x+26,y+22,1000,32),"CHECKPOINT "+game.Wave+" / CHOOSE YOUR NEXT ENCOUNTER",24,white,true);
            Label(new Rect(x+26,y+65,1000,42),"Applies to wave "+(game.Wave+1)+" only. Then choose your existing rewards and edit your loadout.",16,muted);
            for(int i=0;i<2;i++)
            {
                var route=i==0?EncounterRoute.ShieldLine:EncounterRoute.VolatileBreach;
                var r=new Rect(x+26+i*512,y+119,496,208);Color color=i==0?Accent:Gold;
                Plate(r,ink,color,9);TypeIcon(new Rect(r.x+16,r.y+17,30,30),i==0?ItemKind.Armor:ItemKind.Ability,color);
                Label(new Rect(r.x+58,r.y+17,r.width-74,29),VectorEncounter.RouteName(route),21,color,true);
                Label(new Rect(r.x+18,r.y+60,r.width-36,65),VectorEncounter.RouteDescription(route),16,white);
                Label(new Rect(r.x+18,r.y+130,r.width-36,28),"MIXED PICK: 2 "+(i==0?"EQUIPMENT":"ABILITY")+" OPTIONS + 1 WILDCARD",13,color,true);
                if(Button(new Rect(r.x+18,r.y+165,r.width-36,29),"CHOOSE "+VectorEncounter.RouteName(route),true))game.ChooseRoute(route);
            }
            Label(new Rect(x+26,y+351,1000,28),"Same reward count and rarity odds. Checkpoint 3 still grants your first ultimate.",14,muted);
        }
    }
}
