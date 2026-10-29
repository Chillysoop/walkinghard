using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VectorGame
{
    // Optional development input. No override file means the authored catalog, unchanged.
    public static class BalanceOverrides
    {
        [Serializable] sealed class Document {public float enemyDamage,enemyHealth,heroHealth,firstSpawnInterval,finalSpawnInterval;public bool legacyWeaponAugments,legacyCounterBurst,excludeSiphon,legacySiphonBlade,legacyReclaimer,legacyRedline;public ItemPatch[] items;}
        public static bool LegacyWeaponAugments {get;private set;}
        public static bool LegacyCounterBurst {get;private set;}
        public static bool ExcludeSiphon {get;private set;}
        public static bool LegacySiphonBlade {get;private set;}
        public static bool LegacyReclaimer {get;private set;}
        public static bool LegacyRedline {get;private set;}
        [Serializable] sealed class ItemPatch {public string id;public float damageMultiplier,durationMultiplier,effectMultiplier,reloadMultiplier,cooldownMultiplier,barrierMultiplier,barrierSeconds,wardSeconds;public int wardHits;public bool consumeBlockedWard;}
        static readonly Dictionary<string,ItemPatch> patches=new Dictionary<string,ItemPatch>();
        static float Multiplier(float value)=>value==0?1:value;
        static void Validate(float value,string label){if(float.IsNaN(value)||float.IsInfinity(value)||value<0||value>10)throw new ArgumentException("Balance multiplier must be 0 (default) or in (0,10]: "+label);}
        public static void Apply(string path,CombatTuning tuning)
        {
            patches.Clear();LegacyWeaponAugments=LegacyCounterBurst=ExcludeSiphon=LegacySiphonBlade=LegacyReclaimer=LegacyRedline=false;if(string.IsNullOrEmpty(path))return;var doc=JsonUtility.FromJson<Document>(File.ReadAllText(path));if(doc==null)throw new ArgumentException("Invalid balance JSON");
            LegacyWeaponAugments=doc.legacyWeaponAugments;LegacyCounterBurst=doc.legacyCounterBurst;ExcludeSiphon=doc.excludeSiphon;LegacySiphonBlade=doc.legacySiphonBlade;LegacyReclaimer=doc.legacyReclaimer;LegacyRedline=doc.legacyRedline;
            foreach(var value in new[]{doc.enemyDamage,doc.enemyHealth,doc.heroHealth,doc.firstSpawnInterval,doc.finalSpawnInterval})if(!float.IsFinite(value)||value<0)throw new ArgumentException("Combat overrides must be finite and non-negative; zero keeps the authored value.");
            if(doc.enemyDamage>0)tuning.enemyDamage=doc.enemyDamage;if(doc.enemyHealth>0)tuning.enemyHealth=doc.enemyHealth;if(doc.heroHealth>0)tuning.heroHealth=doc.heroHealth;
            if(doc.firstSpawnInterval>0)tuning.firstSpawnInterval=doc.firstSpawnInterval;if(doc.finalSpawnInterval>0)tuning.finalSpawnInterval=doc.finalSpawnInterval;
            if(doc.items==null)return;
            foreach(var patch in doc.items)
            {
                var definition=LoadoutCatalog.Find(patch.id);if(definition.kind==ItemKind.Augment||definition.kind==ItemKind.Basic)throw new ArgumentException("Override the host, not module rarity: "+patch.id);
                Validate(patch.damageMultiplier,patch.id+" damage");Validate(patch.durationMultiplier,patch.id+" duration");Validate(patch.effectMultiplier,patch.id+" effect");Validate(patch.reloadMultiplier,patch.id+" reload");Validate(patch.cooldownMultiplier,patch.id+" cooldown");
                Validate(patch.barrierMultiplier,patch.id+" barrier");if(patch.barrierMultiplier>0&&patch.id!="Counterguard")throw new ArgumentException("Barrier override is supported only for Counterguard.");
                Validate(patch.barrierSeconds,patch.id+" barrier seconds");Validate(patch.wardSeconds,patch.id+" ward seconds");
                if(patch.barrierSeconds>0&&patch.id!="Counterguard")throw new ArgumentException("Barrier lifetime is supported only for Counterguard.");
                if(patch.wardHits<0||patch.wardHits>4)throw new ArgumentException("Ward hits must be 0 (default) or 1-4.");
                if((patch.wardSeconds>0||patch.wardHits>0||patch.consumeBlockedWard)&&patch.id!="duelist")throw new ArgumentException("Ward overrides are supported only for Duelist Jacket.");
                if(patches.ContainsKey(patch.id))throw new ArgumentException("Duplicate balance item: "+patch.id);patches.Add(patch.id,patch);
                // Gear profiles own their effect values; active hosts use EffectScale below.
                if(definition.signature!=Signature.None){var e=definition.effect;float m=Multiplier(patch.effectMultiplier);definition.effect=new EffectProfile(e.power*m,e.radius*m,e.seconds*m,e.targets,e.growth);}
            }
        }
        public static float Damage(string id)=>patches.TryGetValue(id,out var p)?Multiplier(p.damageMultiplier):1;
        public static float Duration(string id)=>patches.TryGetValue(id,out var p)?Multiplier(p.durationMultiplier):1;
        public static float Effect(string id)=>patches.TryGetValue(id,out var p)?Multiplier(p.effectMultiplier):1;
        public static float Reload(string id)=>patches.TryGetValue(id,out var p)?Multiplier(p.reloadMultiplier):1;
        public static float Cooldown(string id)=>patches.TryGetValue(id,out var p)?Multiplier(p.cooldownMultiplier):1;
        public static float Barrier(string id)=>patches.TryGetValue(id,out var p)?Multiplier(p.barrierMultiplier):1;
        public static float BarrierSeconds(string id,float fallback)=>patches.TryGetValue(id,out var p)&&p.barrierSeconds>0?p.barrierSeconds:fallback;
        public static float WardSeconds(string id,float fallback)=>patches.TryGetValue(id,out var p)&&p.wardSeconds>0?p.wardSeconds:fallback;
        public static int WardHits(string id,int fallback)=>patches.TryGetValue(id,out var p)&&p.wardHits>0?p.wardHits:fallback;
        public static bool ConsumeBlockedWard(string id)=>patches.TryGetValue(id,out var p)&&p.consumeBlockedWard;
    }
}
