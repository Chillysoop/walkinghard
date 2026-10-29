using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public enum Signature { None,Tactician,Surveyor,Salvager,Capacitor,Bulwark,Reactive,Grounding,Duelist,Recovery,Momentum,Loader,Arcstep,FaultLine,Overclock,Amplifier,Relay,Salvage }
    public enum WeaponStyle { Kinetic,Conductor,Edge,Greatsword,Breach,Impact,Pistol,Rifle,Scatter }
    public readonly struct GearStats
    {
        public readonly float damage,speed,health,resistance;
        public GearStats(float damage=0,float speed=0,float health=0,float resistance=0){this.damage=damage;this.speed=speed;this.health=health;this.resistance=resistance;}
    }
    public readonly struct EffectProfile
    {
        public readonly float power,radius,seconds,growth;
        public readonly int targets;
        public EffectProfile(float power,float radius,float seconds,int targets=1,float growth=.15f){this.power=power;this.radius=radius;this.seconds=seconds;this.targets=targets;this.growth=growth;}
        public float Scale(int rank)=>1+growth*(rank-1);
        public float Power(int rank)=>power*Scale(rank);
        public float Radius(int rank)=>radius+.15f*(rank-1);
        public float Seconds(int rank)=>seconds*(1+.1f*(rank-1));
    }
    public sealed class WeaponProfile
    {
        public readonly WeaponStyle style;
        public readonly float damage,duration,range,arc,force;
        public readonly int magazine,pose;
        public WeaponProfile(WeaponStyle style,float damage,float duration,float range,float arc,float force,int magazine,int pose)
        {this.style=style;this.damage=damage;this.duration=duration;this.range=range;this.arc=arc;this.force=force;this.magazine=magazine;this.pose=pose;}
    }
    public static class LoadoutCatalog
    {
        public const float CounterguardBarrier=25f;
        public static float CounterguardCapacity(LoadoutItem item)=>CounterguardBarrier*ItemRarity.Power[item.rank]*BalanceOverrides.Barrier(item.def.id);
        public static bool IsArmor(ItemKind kind)=>kind==ItemKind.Helmet||kind==ItemKind.Armor||kind==ItemKind.Boots;
        public static int ArmorIndex(ItemKind kind)=>IsArmor(kind)?(int)kind-(int)ItemKind.Helmet:-1;
        public static int Family(WeaponKind kind)=>kind==WeaponKind.Pistol||kind==WeaponKind.Rifle||kind==WeaponKind.Shotgun?3:(int)kind;
        static ItemDefinition Gear(string id,string name,ItemKind kind,Signature sig,string description,string scaling,string tags,string cost,GearStats stats,EffectProfile effect)
        {return new ItemDefinition(id,name,kind,description,scaling){signature=sig,stats=stats,effect=effect,tags=tags,tradeoff=cost};}
        // First three preserve the original definitions/indices used by saved QA fixtures.
        public static readonly ItemDefinition[] Armor={
            Gear("visor","Tactician Visor",ItemKind.Helmet,Signature.Tactician,"Hits against staggered targets transfer stagger to one neighbor, once per turn.","More stagger transfer reach/duration and direct power.","stagger iron storm","Requires an already-staggered target.",new GearStats(damage:.06f),new EffectProfile(6,3,.25f)),
            Gear("plate","Bulwark Plate",ItemKind.Armor,Signature.Bulwark,"Completing a sequence arms a guard. The next hit is absorbed and releases stagger.","More guard capacity and pulse coverage.","guard iron hammer","One guard per completed loop.",new GearStats(health:40,resistance:.08f),new EffectProfile(14,2.6f,.4f)),
            Gear("treads","Recovery Treads",ItemKind.Boots,Signature.Recovery,"The first kill of a turn drops a repair mote. Walk near it to collect healing.","More healing and pickup reach.","sustain iron blade","Distant kills can leave motes out of reach.",new GearStats(speed:.04f),new EffectProfile(3,3,10)),
            Gear("surveyor","Surveyor Visor",ItemKind.Helmet,Signature.Surveyor,"Multi-target turns echo a weaker hit on the last surviving target.","Stronger echo and wider splash.","chain pierce blade gun storm","Requires two different direct-hit targets; once per turn.",new GearStats(damage:.04f),new EffectProfile(9,.7f,0)),
            Gear("salvager","Salvager Visor",ItemKind.Helmet,Signature.Salvager,"A launch or sever primes the next basic to pull a fallen body toward its impact.","More retrieval reach and force.","launch sever vector blade","Needs a fallen body and a later basic.",new GearStats(damage:.03f),new EffectProfile(7,5,7)),
            Gear("capacitor","Capacitor Crown",ItemKind.Helmet,Signature.Capacitor,"A direct hit against stagger applies Conductive, once per turn.","Longer marks and stronger discharges.","stagger mark storm iron","Marks need a later discharge or Relay Coil impact.",new GearStats(damage:.04f),new EffectProfile(10,3,6)),
            Gear("reactive","Reactive Shell",ItemKind.Armor,Signature.Reactive,"Three nearby enemies trigger a barrier. Its break/expiry repels the crowd.","More barrier capacity and repulsion reach.","guard close iron","8s cooldown; repulsion can disrupt a gathered crowd.",new GearStats(health:25),new EffectProfile(18,3,3)),
            Gear("grounding","Grounding Mantle",ItemKind.Armor,Signature.Grounding,"Lightning/body impacts generate a capped barrier; attackers hitting it become Conductive.","More barrier capacity and mark duration.","mark collision storm vector","Requires lightning or body impacts.",new GearStats(resistance:.05f),new EffectProfile(18,3,4)),
            Gear("duelist","Duelist Jacket",ItemKind.Armor,Signature.Duelist,"A reload or sever arms one brief ward: reduce the next hit and stagger its attacker.","More mitigation and retaliation duration.","reload sever gun blade","Ward expires after 4s; no stacking.",new GearStats(health:20),new EffectProfile(.45f,0,4)),
            Gear("momentum","Momentum Soles",ItemKind.Boots,Signature.Momentum,"Launching a body accelerates your next non-weapon ability.","Stronger acceleration and longer storage.","launch vector hammer iron","One stored boost; weapons do not consume it.",new GearStats(speed:.03f),new EffectProfile(.18f,0,5)),
            Gear("loader","Loader Greaves",ItemKind.Boots,Signature.Loader,"Shorter preparation/reloads. Replenishing ammo sends a nearby stagger pulse.","Faster preparation and a wider/longer pulse.","reload gun close","Pulse is local; a paired reload triggers it once.",new GearStats(speed:.02f),new EffectProfile(.16f,2.4f,.35f)),
            Gear("arcstep","Arcstep Boots",ItemKind.Boots,Signature.Arcstep,"Two kills from one turn mark nearby survivors Conductive.","More marked targets and wider coverage.","mark storm blade","Requires a multi-kill; once per originating turn.",new GearStats(speed:.03f),new EffectProfile(10,3,6,2))
        };
        static ItemDefinition Skill(AbilityId id,string name,string desc,string interaction,float duration,float range,float damage,int pose,string tags,string cost)
        {return new ItemDefinition(id.ToString(),name,ItemKind.Ability,desc,"Rarity improves damage and the signature's coverage, duration or force.",ability:new AbilityCard(id,name,desc,interaction,duration,range,damage,pose)){tags=tags,tradeoff=cost,effect=new EffectProfile(1,range,.4f)};}
        public static readonly ItemDefinition[] Abilities={
            Skill(AbilityId.SteelJab,"Steel Jab","Quick interrupt; bonus impact against an attacking enemy.","Creates stagger.",.4f,2.6f,24,0,"stagger iron","Short reach; one target."),
            Skill(AbilityId.Breaker,"Breaker","A broad palm strike that staggers the front crowd.","Stagger feeds lightning and later basics.",.68f,3.2f,30,2,"stagger iron hammer","Limited reach; rear enemies are outside the cone."),
            Skill(AbilityId.ForcePush,"Force Push","Repel nearby enemies and fallen bodies.","Uses gathered targets; creates knockback.",.95f,5.3f,20,-1,"push vector","Can push targets beyond the next melee action."),
            Skill(AbilityId.MassDriver,"Mass Driver","Aim one body through a bounded collision cascade.","Creates launch and body impacts.",.82f,3.5f,28,2,"launch collision vector iron","Needs a close primary target; limited shared collision budget."),
            Skill(AbilityId.GravityWell,"Gravity Well","Pull nearby enemies toward one gathering point.","Creates a cluster for cleaves and bursts.",.8f,6.5f,14,-1,"gather vector blade","Gathering does not guarantee a subsequent hit."),
            Skill(AbilityId.ArcBolt,"Arc Bolt","Chain a discharge through nearby targets; stagger adds damage.","Consumes Conductive; uses stagger.",.65f,9,28,1,"chain storm gun","Jumps require nearby reachable targets."),
            Skill(AbilityId.ConductiveBrand,"Conductive Brand","Mark a group for later lightning discharges.","Creates Conductive; Relay Coil lets impacts consume it.",.6f,7,7,-1,"mark storm","Low immediate damage; requires a later consumer."),
            Skill(AbilityId.VectorTether,"Vector Tether","Pull targets into a narrow forward corridor.","Creates a line for piercing shots.",.8f,8,10,-1,"gather pierce vector gun","Long thin setup instead of a compact cluster."),
            Skill(AbilityId.ReapingCut,"Reaping Cut","A projected cutting arc; weakened targets take extra damage and lethal hits sever.","Creates severed bodies for retrieval and collision.",.85f,4.5f,28,3,"sever blade","Slower commitment; bonus is damage, never automatic execution."),
            Skill(AbilityId.Counterguard,"Counterguard","Create a barrier; absorbed damage empowers the next basic turn.","Defense creates a stored offensive payoff.",.55f,8,0,-1,"guard iron hammer","Only absorbed damage charges the payoff; one capped bank.")
        };
        static ItemDefinition Weapon(string id,string name,WeaponKind family,int hands,WeaponStyle style,string desc,string upgrade,string tags,string cost,float damage,float duration,float range,float arc,float force,int mag,int pose)
        {return new ItemDefinition(id,name,ItemKind.Weapon,desc,upgrade,family,hands){profile=new WeaponProfile(style,damage,duration,range,arc,force,mag,pose),tags=tags,tradeoff=cost};}
        public static readonly ItemDefinition Fists=Weapon("fists","Kinetic Fists",WeaponKind.Fists,1,WeaponStyle.Kinetic,"Quick close strikes with reliable stagger.","Uncommon: two contacts. Rare+: stagger shockwave; higher rarity improves coverage.","stagger iron","Short reach and limited area.",19,.36f,2.7f,0,1.2f,0,0);
        public static readonly ItemDefinition[] Weapons={
            Weapon("sword","Edge Sword",WeaponKind.Sword,1,WeaponStyle.Edge,"Broad cuts; lethal hits sever at the waist.","Wider reach/arc; Rare+ launches severed bodies.","sever blade","Less concentrated impact than a two-handed blade.",34,.74f,3.7f,45,3,0,3),
            Weapon("hammer","Impact Hammer",WeaponKind.Hammer,2,WeaponStyle.Impact,"One crushing launch using both mounts.","Uncommon: area stagger. Rare+: up to three launches sharing one cascade.","launch collision hammer","Slow commitment and both weapon mounts.",48,1.12f,3.5f,55,15,0,4),
            Weapon("pistol","Pulse Pistol",WeaponKind.Pistol,1,WeaponStyle.Pistol,"Six rounds; quick ranged shots.","Uncommon: one ricochet. Rare+: two, with better retention/reach.","gun reload","Reload delays the next weapon turn.",24,.48f,10,0,.7f,6,5),
            Weapon("rifle","Rail Rifle",WeaponKind.Rifle,2,WeaponStyle.Rifle,"Four rounds; pierce aligned targets using both mounts.","More pierced targets and a final-target launch from Rare.","gun pierce reload","Needs alignment; four-round reload and both mounts.",42,.9f,13,0,2,4,5),
            Fists,
            Weapon("wraps","Conductor Wraps",WeaponKind.Fists,1,WeaponStyle.Conductor,"Fist contacts apply Conductive for later lightning.","Longer marks and stronger discharges.","mark storm iron","Lower direct impact/stagger than Kinetic Fists.",15,.4f,2.7f,0,1,0,0),
            Weapon("greatsword","Execution Greatsword",WeaponKind.Sword,2,WeaponStyle.Greatsword,"A broad heavy cleave; lethal contacts sever and shove bodies.","Wider cleave, greater reach and follow-through force.","sever blade","Slow single swing; both mounts.",59,1.18f,4.1f,70,6,0,3),
            Weapon("breachhammer","Breach Hammer",WeaponKind.Hammer,1,WeaponStyle.Breach,"A light hammer staggers and knocks the first target forward.","Longer stagger; stronger controlled launch.","stagger launch hammer","Less force and splash than Impact Hammer.",29,.66f,3.1f,25,8,0,4),
            Weapon("scattergun","Compact Scattergun",WeaponKind.Shotgun,1,WeaponStyle.Scatter,"Three rounds; a short cone of knockback pellets.","Wider useful coverage and better pellet retention.","gun push close","Short reach; small magazine.",31,.75f,6,40,5,3,5)
        };
        public static readonly ItemDefinition[] Rigs={
            Gear("faultline","Fault Line",ItemKind.Rig,Signature.FaultLine,"First body impact in a turn emits a stagger aftershock.","More collision power and aftershock coverage.","collision hammer vector","Requires body impacts; one aftershock per originating turn.",new GearStats(),new EffectProfile(10,2,.4f)),
            Gear("overclock","Overclock",ItemKind.Rig,Signature.Overclock,"Successive turns ramp tempo within the loop; wrapping resets it.","Stronger tempo ramp, capped at 40%.","tempo iron gun","Early turns have no ramp; no ultimate recharge speed.",new GearStats(),new EffectProfile(.06f,0,0)),
            Gear("amplifier","Amplifier",ItemKind.Rig,Signature.Amplifier,"Abilities bank charge; the next basic releases one impact burst.","Larger charge bank and burst.","burst gun iron","Requires setup before a basic; paired basics spend it once.",new GearStats(),new EffectProfile(6,2.1f,0,3)),
            Gear("relay","Relay Coil",ItemKind.Rig,Signature.Relay,"Body impacts consume Conductive to discharge lightning.","One extra arc at Common, two at Rare, three at Legendary; more reach.","mark collision storm hammer","Needs a mark and a later body impact; arcs cannot repeat the trigger.",new GearStats(),new EffectProfile(10,3,0)),
            Gear("magnet","Salvage Magnet",ItemKind.Rig,Signature.Salvage,"A direct sever propels the body/fragments into one follow-up collision chain.","More retrieval reach and launch impulse.","sever collision blade vector","Once per turn, with one shared hit budget and enemy identity.",new GearStats(),new EffectProfile(1,5,0))
        };
        public static readonly AbilityCard Ultimate=new AbilityCard(AbilityId.Ultimate,"Singularity Burst","Blast bodies into collision chains.","Creates launch and collision.",1.1f,8.5f,38,-1);
        public static readonly ItemDefinition[] Ultimates={
            new ItemDefinition("singularity","Singularity Burst",ItemKind.Ultimate,"Launch nearby bodies into a shared collision cascade.","More launch force and coverage.",ability:Ultimate){tags="launch vector hammer",tradeoff="Shared six-hit cascade budget."},
            new ItemDefinition("tempest","Tempest Array",ItemKind.Ultimate,"Chain lightning through a crowd; exploit stagger and Conductive.","More chain reach and target capacity.",ability:new AbilityCard(AbilityId.Ultimate,"Tempest Array","Chain discharge.","Uses marks and stagger.",1.2f,12,52,-1)){tags="chain storm",tradeoff="Targets must connect through jump reach."},
            new ItemDefinition("cyclone","Razor Cyclone",ItemKind.Ultimate,"Radial cutting burst; lethal hits sever and shove.","More cutting coverage and follow-through force.",ability:new AbilityCard(AbilityId.Ultimate,"Razor Cyclone","Radial severing burst.","Creates fragments.",1.3f,6,45,3)){tags="sever blade",tradeoff="Local area; no automatic executions."},
            new ItemDefinition("redline","Redline",ItemKind.Ultimate,"Refill equipped guns and accelerate the next three sequence turns.","Stronger acceleration; capped at 45%.",ability:new AbilityCard(AbilityId.Ultimate,"Redline","Reload and accelerate three turns.","Uses reload and tempo.",.65f,14,0,-1)){tags="reload tempo gun iron",tradeoff="No direct damage; full magazines do not count as reloads."}
        };
        public static IEnumerable<ItemDefinition> Equipment=>Weapons.Concat(Armor).Concat(Rigs).Concat(Ultimates);
        public static IEnumerable<ItemDefinition> All=>Equipment.Concat(Abilities);
        public static ItemDefinition Find(string id)=>All.Concat(AugmentCatalog.All).First(d=>d.id==id);
        public static float EffectScale(LoadoutItem item)=>(1+.15f*(item.rank-1))*BalanceOverrides.Effect(item.def.id);
        public static int Magazine(LoadoutItem item)=>item?.def.profile?.magazine??0;
        public static float Cooldown(LoadoutItem item)=>Mathf.Max(10.1f,(item?.def.id=="tempest"?16:item?.def.id=="cyclone"?15:item?.def.id=="redline"?18:12)*BalanceOverrides.Cooldown(item?.def.id??""));
        public static AbilityCard Basic(LoadoutItem weapon)=>Action(weapon);
        public static AbilityCard Action(LoadoutItem item)
        {
            var d=item.def;int r=item.rank;float power=ItemRarity.Power[r];
            if(d.profile!=null){var p=d.profile;float time=p.duration;if(p.style==WeaponStyle.Kinetic&&r>1)time=.54f;return new AbilityCard(AbilityId.BasicAttack,item.Name,d.description,d.upgrade,time*BalanceOverrides.Duration(d.id),p.range+.12f*(r-1),p.damage*power*BalanceOverrides.Damage(d.id),p.pose);}
            var a=d.ability;if(a==null)return null;return new AbilityCard(a.id,a.name,a.description,a.interaction,a.duration*BalanceOverrides.Duration(d.id),a.range+.15f*(r-1),a.damage*power*BalanceOverrides.Damage(d.id),a.pose);
        }
    }
}
