using System;
using System.Linq;
using System.Collections.Generic;

namespace VectorGame
{
    public enum AugmentId { FollowThrough,Focused,Echo,Afterfield,SplitPayload,Backwash,Discharge,Anchor,Flurry,PalmDrive,Crosscut,HookedEdge,Groundbreaker,Piledriver,BoreRounds,BreachLoad }
    public sealed class AugmentDefinition
    {
        public readonly AugmentId id; public readonly int family; public readonly AbilityId[] abilities;
        public AugmentDefinition(AugmentId id,int family,params AbilityId[] abilities){this.id=id;this.family=family;this.abilities=abilities;}
        public bool Fits(ItemDefinition host)=>family>=0?host.kind==ItemKind.Weapon&&LoadoutCatalog.Family(host.weapon)==family:host.kind==ItemKind.Ability&&abilities.Contains(host.ability.id);
    }
    public static class AugmentCatalog
    {
        static ItemDefinition A(AugmentId id,string name,string benefit,string cost,int family=-1,params AbilityId[] hosts)=>new ItemDefinition("aug-"+id,name,ItemKind.Augment,benefit){module=new AugmentDefinition(id,family,hosts),tradeoff=cost};
        public static readonly ItemDefinition[] All={
            A(AugmentId.FollowThrough,"Follow-through","Bank one next-basic stagger pulse; replaces any previous bank.","Immediate stagger 100% → 55%.",-1,AbilityId.SteelJab,AbilityId.Breaker),
            A(AugmentId.Focused,"Focused","Concentrate the primary effect on fewer targets.","Narrower coverage or fewer chains/marks.",-1,AbilityId.Breaker,AbilityId.ArcBolt,AbilityId.ConductiveBrand,AbilityId.ReapingCut,AbilityId.DrainBrand),
            A(AugmentId.Echo,"Echo","One weaker primary-target repeat; never repeats the whole chain or cleave.","Opening damage 100% → 80%; +0.22s commitment.",-1,AbilityId.SteelJab,AbilityId.ArcBolt,AbilityId.ReapingCut),
            A(AugmentId.Afterfield,"Afterfield","Leave a bounded field for 2s, ticking every 0.4s at the original point or corridor.","Initial strength 100% → 65%.",-1,AbilityId.GravityWell,AbilityId.VectorTether,AbilityId.ConductiveBrand),
            A(AugmentId.SplitPayload,"Split Payload","One launch → up to two; shared six-hit collision budget.","Each launch impulse 100% → 65%.",-1,AbilityId.MassDriver),
            A(AugmentId.Backwash,"Backwash","Surviving affected enemies return partway toward their original position.","Outward force 100% → 65%; +0.32s return. Corpses never return.",-1,AbilityId.ForcePush),
            A(AugmentId.Discharge,"Discharge","Counterguard releases absorbed damage in a bounded frontal burst on break/expiry.","Conversion 80% banked → 55% burst; no next-basic bank.",-1,AbilityId.Counterguard),
            A(AugmentId.Anchor,"Anchor","Briefly stagger targets that actually move at least 0.25m.","Pull strength 100% → 70%; resistant targets may not qualify.",-1,AbilityId.GravityWell,AbilityId.VectorTether),
            A(AugmentId.Flurry,"Flurry","Two weaker follow-up contacts on the primary target.","One weapon socket; native attack timing preserved.",0),
            A(AugmentId.PalmDrive,"Palm Drive","First contact shoves the primary target forward; no cascade.","One weapon socket; shove can change melee spacing.",0),
            A(AugmentId.Crosscut,"Crosscut","One weaker return cut, including lethal severs.","One weapon socket; native attack timing preserved.",1),
            A(AugmentId.HookedEdge,"Hooked Edge","Pull the first surviving struck target into the forward impact area.","One weapon socket; native reach and cleave preserved.",1),
            A(AugmentId.Groundbreaker,"Groundbreaker","A forward ground wave staggers additional targets; no descendant waves.","One weapon socket; native launch force preserved.",2),
            A(AugmentId.Piledriver,"Piledriver","Strengthen the primary launch while keeping native splash and other launches.","One weapon socket; shared collision limit still applies.",2),
            A(AugmentId.BoreRounds,"Bore Rounds","Add bounded line penetration; keep native ricochets and scatter cone.","One weapon socket; no added focusing delay.",3),
            A(AugmentId.BreachLoad,"Breach Load","Actual ammunition replenishment arms one stagger burst on the first successful shot.","One weapon socket; normal reload time, one armed burst.",3)
        };
        public static ItemDefinition Get(AugmentId id)=>All.First(a=>a.module.id==id);
        public static IEnumerable<ItemDefinition> Offers(AbilityDeck deck,Random random)
        {
            var eligible=All.Where(d=>deck.CompatibleHosts(d).Count()>deck.Items.Count(i=>i.def==d)).OrderBy(_=>random.Next()).ToList();
            var ability=eligible.FirstOrDefault(d=>d.module.family<0);var weapon=eligible.FirstOrDefault(d=>d.module.family>=0);
            var result=new List<ItemDefinition>();if(ability!=null)result.Add(ability);if(weapon!=null)result.Add(weapon);
            result.AddRange(eligible.Where(d=>!result.Contains(d)).Take(3-result.Count));return result;
        }
        public static float Extension(LoadoutItem host)=>host==null||(!BalanceOverrides.LegacyWeaponAugments&&host.def.kind==ItemKind.Weapon)?0:host.HasAugment(AugmentId.Echo)?.22f:host.HasAugment(AugmentId.Flurry)?.28f:host.HasAugment(AugmentId.Crosscut)||host.HasAugment(AugmentId.Backwash)?.32f:0;
        public static float Opening(LoadoutItem host)=>!BalanceOverrides.LegacyWeaponAugments&&host.def.kind==ItemKind.Weapon?1:host.HasAugment(AugmentId.Flurry)?.82f:host.HasAugment(AugmentId.Echo)||host.HasAugment(AugmentId.Crosscut)||host.HasAugment(AugmentId.PalmDrive)?.8f:host.HasAugment(AugmentId.Afterfield)?.65f:1;
        public static string Timing(LoadoutItem host)=>host.HasAugment(AugmentId.Echo)?"ECHO":host.HasAugment(AugmentId.Flurry)?"FLURRY":host.HasAugment(AugmentId.Crosscut)?"RETURN CUT":"BACKWASH";
        public static string Values(LoadoutItem host,float share=1)
        {
            if(host?.augment==null)return "Empty socket · one compatible module";
            float s=LoadoutCatalog.EffectScale(host);bool pair=BalanceOverrides.LegacyWeaponAugments&&share<1;var id=host.augment.def.module.id;
            string value=id==AugmentId.Flurry?$"2 contacts × {(BalanceOverrides.LegacyWeaponAugments?16:15)*s*share:0}% damage":id==AugmentId.Crosscut?$"Return {(BalanceOverrides.LegacyWeaponAugments?45:35)*s*share:0}% damage; cap {(pair?2:4)}":id==AugmentId.PalmDrive?$"Shove {(6+.6f*(host.rank-1))*share:0.0}":id==AugmentId.Groundbreaker?$"Wave {(pair?3.2f:4):0.0}m; cap {(pair?2:3)}; stagger {.45f*s*share:0.00}s":id==AugmentId.Piledriver?$"Primary impulse +{35*s*share:0}%":id==AugmentId.BreachLoad?$"Normal reload; burst {(pair?1.6f:2)+.15f*(host.rank-1):0.0}m, cap {(pair?3:5)}; {.4f*s*share:0.00}s stagger":id==AugmentId.BoreRounds?$"No extra prep; line cap {BoreCap(host,pair)}":id==AugmentId.Echo?$"One repeat {35*s:0}% damage; +0.22s":id==AugmentId.FollowThrough?$"One next-basic pulse: {.4f*s:0.00}s / 2.2m":id==AugmentId.Anchor?$"After 0.25m movement: {.25f*s:0.00}s stagger":id==AugmentId.Afterfield?$"2s field; 5 ticks; 6 distinct targets":id==AugmentId.Discharge?$"55% absorbed; cap {30*s:0} damage / 4m / 5 targets":id==AugmentId.Focused?Focused(host):host.augment.def.description;
            return host.augment.Name+" · "+value+"\n"+(host.def.kind==ItemKind.Weapon?"Socket: ":"Tradeoff: ")+host.augment.def.tradeoff+(pair?"\nPAIRED: half secondary budget; ordinary weapon damage unchanged.":host.def.hands==2?"\nTWO-HANDED: one socket, one full effect.":"");
        }
        static string Focused(LoadoutItem h)=>h.def.ability.id==AbilityId.DrainBrand?$"Drain marks {3+(h.rank-1)/2} → 2; bonus Lifesteal +40%":h.def.ability.id==AbilityId.ArcBolt?$"Chains 3 → {(h.rank>=3?2:1)}; primary +35%":h.def.ability.id==AbilityId.ConductiveBrand?"Marks 5 → 2; potency +40%":h.def.ability.id==AbilityId.Breaker?"Half arc 100° → 40°; stagger +50%":"Half arc 70° → 42°; weakened bonus 40% → 80%";
        public static int BoreCap(LoadoutItem h,bool pair)=>h.def.weapon==WeaponKind.Rifle?Math.Min(6,h.rank+3):Math.Min(pair?3:4,2+(h.rank-1)/2);
    }
}
