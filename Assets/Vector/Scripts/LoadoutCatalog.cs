namespace VectorGame
{
    public static class LoadoutCatalog
    {
        public static bool IsArmor(ItemKind kind)=>kind==ItemKind.Helmet||kind==ItemKind.Armor||kind==ItemKind.Boots;
        public static int ArmorIndex(ItemKind kind)=>IsArmor(kind)?(int)kind-(int)ItemKind.Helmet:-1;
        public static readonly ItemDefinition[] Armor={
            new ItemDefinition("visor","Tactician Visor",ItemKind.Helmet,"More direct damage. Expose: bonus damage against already-staggered targets.","Higher rarity increases damage and the Expose bonus."),
            new ItemDefinition("plate","Bulwark Plate",ItemKind.Armor,"More maximum integrity. Brace: reduces incoming enemy attack damage.","Higher rarity increases integrity and damage reduction."),
            new ItemDefinition("treads","Recovery Treads",ItemKind.Boots,"Faster actions. Recovery: regenerates integrity while walking in combat.","Higher rarity increases action speed and regeneration.")
        };
        static ItemDefinition Skill(AbilityId id,string name,string description,string interaction,float duration,float range,float damage,int pose)
        {return new ItemDefinition(id.ToString(),name,ItemKind.Ability,description,"Higher rarities increase damage.",ability:new AbilityCard(id,name,description,interaction,duration,range,damage,pose));}
        public static readonly ItemDefinition[] Abilities={
            Skill(AbilityId.SteelJab,"Steel Jab","A fast unarmed strike with brief stagger.","Creates stagger.",.40f,2.6f,24,0),
            Skill(AbilityId.Breaker,"Breaker","A wide palm strike with a full second of stagger.","Stagger feeds Arc Bolt.",.68f,3.2f,34,2),
            Skill(AbilityId.ForcePush,"Force Push","Throw nearby enemies and fallen bodies away.","Uses gathered targets; creates knockback.",.95f,5.3f,20,-1),
            Skill(AbilityId.MassDriver,"Mass Driver","Launch one body into a bounded collision cascade.","Uses nearby bodies; creates launch and impacts.",.82f,3.5f,28,2),
            Skill(AbilityId.GravityWell,"Gravity Well","Pull a crowd into a point ahead of you.","Creates clustered targets for sweeps and pushes.",.80f,6.5f,16,-1),
            Skill(AbilityId.ArcBolt,"Arc Bolt","A ranged discharge chains to two nearby targets.","Uses stagger: bonus damage per affected target.",.65f,9f,28,1)
        };
        public static readonly ItemDefinition Fists=new ItemDefinition("fists","Kinetic Fists",ItemKind.Weapon,"Unarmed upgrade. Equip in a weapon mount; empty mounts use your starter fists.","Uncommon: double strike. Rare+: double strike with a stagger shockwave.");
        public static readonly ItemDefinition[] Weapons={
            new ItemDefinition("sword","Edge Sword",ItemKind.Weapon,"1H · Broad cuts. Lethal hits sever bodies at the waist.","Uncommon: wider arc and reach. Rare+: hurl severed halves into the crowd.",WeaponKind.Sword,1),
            new ItemDefinition("hammer","Impact Hammer",ItemKind.Weapon,"2H · Uses BOTH mounts and locks both basic cards together. One crushing launch per pair.","Uncommon: area stagger. Rare+: launch up to three targets.",WeaponKind.Hammer,2),
            new ItemDefinition("pistol","Pulse Pistol",ItemKind.Weapon,"1H · Fast ranged shots. Six rounds; a visible reload before the next shot.","Uncommon: one ricochet. Rare+: two ricochets with stagger.",WeaponKind.Pistol,1),
            new ItemDefinition("rifle","Rail Rifle",ItemKind.Weapon,"2H · Uses BOTH mounts. Four rounds; shots pierce aligned enemies.","Uncommon: pierce three enemies. Rare+: pierce four and launch the last.",WeaponKind.Rifle,2)
        };
        public static readonly ItemDefinition[] Rigs={
            new ItemDefinition("faultline","Fault Line",ItemKind.Rig,"Body impacts hit harder and stagger targets. Equip one rig.","Higher rarities increase collision damage."),
            new ItemDefinition("overclock","Overclock",ItemKind.Rig,"Execute the sequence faster. Ultimate countdown is unchanged.","Higher rarities increase execution speed."),
            new ItemDefinition("amplifier","Amplifier",ItemKind.Rig,"Increase the damage of direct attacks.","Higher rarities increase direct damage.")
        };
        public static readonly AbilityCard Ultimate=new AbilityCard(AbilityId.Ultimate,"Singularity Burst","Blast the crowd into a collision cascade. Recharges in 12 combat seconds.","Creates launch and collision.",1.1f,8.5f,38,-1);
        public static readonly ItemDefinition[] Ultimates={
            new ItemDefinition("singularity","Singularity Burst",ItemKind.Ultimate,"12s · Blast the crowd into a collision cascade. Separate from the five-slot loop.","Higher rarities increase burst damage.",ability:Ultimate),
            new ItemDefinition("tempest","Tempest Array",ItemKind.Ultimate,"16s · Chain lightning through up to seven enemies; stagger adds damage.","Higher rarities increase discharge damage.",ability:new AbilityCard(AbilityId.Ultimate,"Tempest Array","Chain through seven targets.","Uses stagger.",1.2f,12,38,-1))
        };
        public static AbilityCard Basic(LoadoutItem weapon)
        {
            int r=weapon.rank;string name=weapon.Name;
            switch(weapon.def.weapon)
            {
                case WeaponKind.Sword:return new AbilityCard(AbilityId.BasicAttack,name,weapon.def.description,weapon.def.upgrade,.74f,3.7f+(r-1)*.25f,34*ItemRarity.Power[r],3);
                case WeaponKind.Hammer:return new AbilityCard(AbilityId.BasicAttack,name,weapon.def.description,weapon.def.upgrade,1.12f,3.3f,42*ItemRarity.Power[r],4);
                case WeaponKind.Pistol:return new AbilityCard(AbilityId.BasicAttack,name,weapon.def.description,weapon.def.upgrade,.48f,10,24*ItemRarity.Power[r],5);
                case WeaponKind.Rifle:return new AbilityCard(AbilityId.BasicAttack,name,weapon.def.description,weapon.def.upgrade,.9f,13,35*ItemRarity.Power[r],5);
                default:return new AbilityCard(AbilityId.BasicAttack,name,weapon.def.description,weapon.def.upgrade,r>1?.54f:.36f,2.7f,19*ItemRarity.Power[r],0);
            }
        }
        public static int Magazine(LoadoutItem weapon)=>weapon.def.weapon==WeaponKind.Pistol?6:weapon.def.weapon==WeaponKind.Rifle?4:0;
    }
}
