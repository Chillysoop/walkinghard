using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] sealed class WeaponJourneyEncounter
        {
            public int wave,components;public string[] weapons;public int[] ranks;
            public string loadout;public bool allowed,paired,twoHanded;
            public float seconds,damage,weaponDamage,weaponWait,weaponReload,healing,health;
        }
        List<WeaponJourneyEncounter> weaponJourneyEncounters;
        void RecordWeaponJourneyEncounter()
        {
            if(weaponJourneyEncounters==null||LastWaveFeedback==null||weaponJourneyEncounters.Any(e=>e.wave==Wave))return;
            var f=LastWaveFeedback;var weapons=f.items.Where(i=>LoadoutCatalog.Find(i.id).kind==ItemKind.Weapon).ToArray();
            weaponJourneyEncounters.Add(new WeaponJourneyEncounter{wave=Wave,components=AutomatedLoadoutPolicy.Components(Deck,ProgressionPolicy),allowed=AutomatedLoadoutPolicy.Allowed(Deck,ProgressionPolicy),
                weapons=Deck.Basics.Select(b=>Deck.WeaponFor(b).def.id).ToArray(),ranks=Deck.Basics.Select(b=>Deck.WeaponFor(b).rank).ToArray(),loadout=Deck.Loadout,
                paired=Enumerable.Range(0,5).Any(i=>Deck.PairAfter(i)>=0),twoHanded=Deck.TwoHanded,seconds=f.seconds,damage=f.damage,weaponDamage=weapons.Sum(i=>i.values.direct+i.values.collision),
                weaponWait=weapons.Sum(i=>i.values.rangeWait),weaponReload=weapons.Sum(i=>i.values.reload),healing=f.healing,health=Health});
            if(reviewCommittedWeapons&&!weaponJourneyEncounters.Last().allowed)throw new InvalidOperationException("Committed weapon policy escaped its constraints");
        }
        [Serializable] sealed class DiscoveryAudit
        {
            public string pool;public int seeds,originalBreach,discoveryBreach;public float originalIdentities,discoveryIdentities;
        }
        IEnumerator CheckWeaponDiscovery(string output,bool render)
        {
            int count=0;var checks=new List<string>();void Require(bool ok,string name){if(!ok)throw new InvalidOperationException("WEAPON_DISCOVERY: "+name);count++;if(!checks.Contains(name))checks.Add(name);}
            yield return CheckForkEntranceClearance(Require);
            Require(Enumerable.Range(1,21).Where(LongJourney.WeaponDiscoveryAfter).SequenceEqual(new[]{2,9,16}),"one existing mixed checkpoint per district");
            var audits=new List<DiscoveryAudit>();
            foreach(bool unlocked in new[]{false,true})
            {
                var audit=new DiscoveryAudit{pool=unlocked?"unlocked":"fresh",seeds=512};audits.Add(audit);
                for(int seed=1;seed<=audit.seeds;seed++)
                {
                    var original=new AbilityDeck(seed);var discovery=new AbilityDeck(seed);
                    bool Allowed(ItemDefinition d)=>unlocked||!Constellation.Nodes.Any(n=>n.item==d.id);
                    original.RewardFilter=Allowed;discovery.RewardFilter=Allowed;
                    var before=new HashSet<string>();var after=new HashSet<string>();
                    foreach(int wave in Enumerable.Range(0,21))
                    foreach(var kind in wave==0?new[]{DraftKind.Ability,DraftKind.Ability}:LongJourney.Encounters[wave-1].rewards)
                    {
                        var emphasis=(RewardEmphasis)(seed%3);var history=LoadoutCatalog.Weapons.ToDictionary(d=>d.id,d=>discovery.LastWeaponOffer(d.id));
                        original.Open(kind,true,emphasis);discovery.Open(kind,true,emphasis,LongJourney.WeaponDiscoveryAfter(wave));
                        Require(original.Offers.Length==discovery.Offers.Length&&(kind!=DraftKind.Mixed||discovery.Offers.Length==3),"discovery keeps all three mixed reward cards and other draft counts");
                        Require(original.Offers.Select(o=>o.rank).SequenceEqual(discovery.Offers.Select(o=>o.rank)),"identical acquisition stream keeps every rarity roll unchanged");
                        Require(original.Offers.Take(2).Select(o=>o.def.id+o.copies).SequenceEqual(discovery.Offers.Take(2).Select(o=>o.def.id+o.copies)),"owned/support and route-biased choices remain unchanged");
                        Require(discovery.Offers.Select(o=>o.def.id).Distinct().Count()==discovery.Offers.Length&&discovery.Offers.All(o=>Allowed(o.def)),"discovery preserves unique cards and constellation gating");
                        if(discovery.WeaponDiscovery)
                        {
                            var offered=discovery.Offers[2];var eligible=LoadoutCatalog.Weapons.Where(d=>Allowed(d)&&!discovery.Offers.Take(2).Any(o=>o.def==d));
                            Require(offered.def.kind==ItemKind.Weapon&&offered.copies==1&&history[offered.def.id]==eligible.Min(d=>history[d.id]),"discovery picks a least-recently-offered exact weapon identity");
                        }
                        foreach(var o in original.Offers.Where(o=>o.def.kind==ItemKind.Weapon))before.Add(o.def.id);
                        foreach(var o in discovery.Offers.Where(o=>o.def.kind==ItemKind.Weapon))after.Add(o.def.id);
                        // Shadow decks take the unchanged first card. This isolates offer
                        // composition from player/bot pivots; it is not a combat cohort.
                        var first=original.Offers.FirstOrDefault();if(first!=null)for(int copy=0;copy<first.copies;copy++){original.Acquire(first.def,first.rank);discovery.Acquire(first.def,first.rank);}
                    }
                    audit.originalIdentities+=before.Count;audit.discoveryIdentities+=after.Count;
                    if(before.Contains("breachhammer"))audit.originalBreach++;if(after.Contains("breachhammer"))audit.discoveryBreach++;
                    if(seed%64==0)yield return null;
                }
                audit.originalIdentities/=audit.seeds;audit.discoveryIdentities/=audit.seeds;
            }
            var small=new AbilityDeck(3){RewardFilter=d=>d==LoadoutCatalog.Fists||d.id=="sword"||d.id=="SteelJab"};small.Open(DraftKind.Mixed,true,weaponDiscovery:true);
            Require(small.Offers.Select(o=>o.def.id).Distinct().Count()==3,"tiny pool falls back safely when first two consume all weapons");
            foreach(string style in new[]{"gun","cutcarry","close"})
            {
                var deck=new AbilityDeck(9);
                foreach(var def in LoadoutCatalog.Weapons.Where(d=>d!=LoadoutCatalog.Fists))deck.Acquire(def,def.hands==2?5:1);
                AutomatedLoadoutPolicy.EquipStored(deck,"committed:"+style);
                Require(AutomatedLoadoutPolicy.Allowed(deck,"committed:"+style)&&AutomatedLoadoutPolicy.Components(deck,style)==(style=="cutcarry"?2:1),"committed "+style+" assembles its components despite tempting legendary alternatives");
                foreach(var def in LoadoutCatalog.Weapons)
                {var c=AutomatedLoadoutPolicy.Evaluate(deck,new RewardOffer(def,3,3),"committed:"+style);Require(c!=null&&AutomatedLoadoutPolicy.Allowed(c.projection.after,"committed:"+style),"committed "+style+" keeps its weapon contract through acquisition and fusion");}
            }
            forcedSeed=true;tuning.seed=4319;StartChapter();OpeningPicks=2;Wave=9;State=RunState.Drafting;
            Deck.Open(DraftKind.Weapon,false);Deck.Open(DraftKind.Mixed,true,RewardEmphasis.Equipment,true);
            Require(SaveCheckpoint(),"discovery checkpoint saves");string graph=JsonUtility.ToJson(CaptureCheckpoint().graph);
            Require(ResumeExpedition()&&graph==JsonUtility.ToJson(CaptureCheckpoint().graph)&&Deck.WeaponDiscovery,"history and marked card restore exactly");
            int charges=Deck.Rerolls;Require(Deck.Reroll()&&Deck.Rerolls==charges-1&&Deck.WeaponDiscovery,"reroll charges normally and retains the discovery slot");
            string rerolled=JsonUtility.ToJson(CaptureCheckpoint().graph);Require(ResumeExpedition()&&Deck.Reroll()&&rerolled==JsonUtility.ToJson(CaptureCheckpoint().graph),"resume and reroll reproduce the same discovery result");
            var old=CaptureCheckpoint();old.schema=7;foreach(var node in old.graph.objects.Where(o=>o.type==typeof(AbilityDeck).FullName))node.fields.RemoveAll(f=>f.name=="weaponDiscovery");AtomicWrite(SavePath,JsonUtility.ToJson(old));
            Require(ResumeExpedition()&&!Deck.WeaponDiscovery,"schema 7 resumes existing offers without inventing old history");Deck.Open(DraftKind.Mixed,true,weaponDiscovery:true);Require(Deck.WeaponDiscovery,"migrated run starts history on its next reward");
            var broken=CaptureCheckpoint().graph;foreach(var node in broken.objects.Where(o=>o.type==typeof(AbilityDeck).FullName))node.fields.RemoveAll(f=>f.name=="weaponDiscovery");bool rejected=false;try{GraphCodec().Decode(broken);}catch(InvalidOperationException){rejected=true;}Require(rejected,"current schema rejects missing discovery state");
            if(render)
            {
                foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})
                {Screen.SetResolution(size.x,size.y,false);for(int i=0;i<15;i++)yield return null;yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,"discovery-"+size.x+"x"+size.y+".png"),t.EncodeToPNG());Destroy(t);}
            }
            File.WriteAllText(Path.Combine(output,"checks.txt"),count+" assertions passed\n"+string.Join("\n",checks));File.WriteAllText(Path.Combine(output,"offer-audit.jsonl"),string.Join("\n",audits.Select(a=>JsonUtility.ToJson(a))));
            Debug.Log("WEAPON_DISCOVERY_CHECKS "+count+" passed");
        }
    }
}
