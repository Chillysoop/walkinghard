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
        [Serializable] sealed class ConstellationReport {public bool passed;public string failure,version,assembly;public int assertions;public List<string> checks=new List<string>();}
        IEnumerator ReviewConstellation()
        {
            enabled=false;qa=false;simulation=false;FastSimulation=false;fx.muted=true;
            var args=Environment.GetCommandLineArgs();string Arg(string key,string fallback){int i=Array.IndexOf(args,key);return i<0?fallback:args[i+1];}
            if(string.IsNullOrEmpty(saveDirectoryOverride))throw new InvalidOperationException("Constellation review requires an isolated profile");
            bool render=!args.Contains("-nographics");string output=Path.GetFullPath(Arg("-reviewOutput","ConstellationReview"));Directory.CreateDirectory(output);
            var report=new ConstellationReport{version=Application.version};using(var hash=System.Security.Cryptography.SHA256.Create())report.assembly=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(typeof(VectorEncounter).Assembly.Location))).Replace("-","");
            void Write()=>File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
            Application.logMessageReceived+=(m,t,type)=>{if(type==LogType.Exception){report.failure=m;Write();Application.Quit(3);}};
            void Require(bool ok,string text){if(!ok){report.failure=text;Write();throw new InvalidOperationException("CONSTELLATION: "+text);}report.assertions++;report.checks.Add(text);}
            IEnumerator Shot(string name){if(!render)yield break;yield return new WaitForEndOfFrame();var shot=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());Destroy(shot);}
            if(render){Screen.SetResolution(1920,1080,false);for(int i=0;i<12;i++)yield return null;}
            Require(Constellation.Nodes.Length==36&&Constellation.Nodes.Sum(n=>n.cost)==228,"36 real item unlocks, 228 Sparks total");Require(ConstellationContent.All.Length==30&&LoadoutCatalog.All.Count()==77,"30 additional identities, 77 non-module identities");
            foreach(var n in Constellation.Nodes){Require(LoadoutCatalog.All.Count(d=>d.id==n.item)==1,"one catalog definition / "+n.item);Require(n.parent==null||Array.FindIndex(Constellation.Nodes,p=>p.item==n.parent)<Array.IndexOf(Constellation.Nodes,n),"acyclic parent precedes child / "+n.item);}
            foreach(var d in ConstellationContent.All){Require(ItemArtwork.Has(d.id)&&ConstellationContent.Short(d.id).Length>0&&d.description.Length>0&&d.tradeoff.Length>0,"art, summary, effect and tradeoff / "+d.id);if(d.kind==ItemKind.Ability||d.kind==ItemKind.Weapon)Require(AugmentCatalog.All.Any(a=>a.module.Fits(d)),"supported augment host / "+d.id);}
            // Schema-1 profile remains valid; no new combat stats or legacy item gates.
            var old=new OfficeProfile{sparks=19,seal=true,rescued=3,victories=1,unlocks=new List<string>{"wraps","greatsword","relay","magnet","harvest","redline"}};AtomicWrite(OfficePath,JsonUtility.ToJson(old));LoadOffice();
            Require(Sparks==19&&ConstellationOwned==6&&RescuedWorkers==3&&JourneyVictories==1,"schema-1 ownership, wallet, workers and victories retained");
            office=new OfficeProfile();SaveOffice(office);OpenOffice();hud.SelectOfficeBranch(0);yield return Shot("office-fresh");
            Require(!BuyConstellation("pileknuckles")&&!BuyConstellation("missing"),"unaffordable and unknown purchases rejected");
            var funded=CopyOffice();funded.sparks=228;SaveOffice(funded);Require(!BuyConstellation("VectorSling"),"child cannot bypass parent or seal");
            StartChapter();var frozen=chapterRun.unlocks.ToArray();Require(!ChapterRewardAllowed(LoadoutCatalog.Find("pileknuckles"))&&ChapterRewardAllowed(LoadoutCatalog.Find("sword")),"fresh pool retains legacy content and gates new identities");Require(SaveCheckpoint(),"save pre-purchase journey");OpenOffice();
            Require(hud.HandleOfficePointer(EventType.MouseDown,hud.OfficeBranchButton(1,1280,720).center,1280,720)&&hud.OfficeBranch==1,"branch click selects Impact without purchase");int wallet=Sparks;
            Require(hud.HandleOfficePointer(EventType.MouseDown,hud.OfficeStar(6,1280,720).center,1280,720)&&Sparks==wallet,"star selection does not spend currency");
            Require(hud.HandleOfficePointer(EventType.MouseDown,hud.OfficeNodeButton(6,1280,720).center,1280,720)&&Sparks==wallet-4&&HasConstellation("pileknuckles"),"explicit purchase spends four Sparks exactly once");Require(!BuyConstellation("pileknuckles")&&!BuyConstellation("VectorSling"),"duplicate and pre-seal child purchases blocked");yield return Shot("office-purchased");
            Require(ResumeExpedition()&&!ChapterRewardAllowed(LoadoutCatalog.Find("pileknuckles"))&&chapterRun.unlocks.SequenceEqual(frozen),"resume preserves frozen reward pool after purchase");OpenOffice();funded=CopyOffice();funded.seal=true;SaveOffice(funded);
            foreach(var n in Constellation.Nodes.Where(n=>!HasConstellation(n.item))){int before=Sparks;Require(BuyConstellation(n.item)&&Sparks==before-n.cost&&!BuyConstellation(n.item),"single atomic purchase / "+n.item);}
            Require(Sparks==0&&ConstellationOwned==36,"all branches cost exactly 228 Sparks");LoadOffice();Require(ConstellationOwned==36&&Sparks==0,"expanded profile survives disk reload");StartChapter();Require(ConstellationContent.All.All(ChapterRewardAllowed),"new journey snapshots all purchased identities");
            var seen=new HashSet<string>();foreach(int seed in Enumerable.Range(1,400))foreach(var kind in new[]{DraftKind.Ability,DraftKind.Weapon,DraftKind.Equipment,DraftKind.Ultimate,DraftKind.Mixed}){var deck=new AbilityDeck(seed){RewardFilter=ChapterRewardAllowed};deck.Open(kind,false);foreach(var offer in deck.Offers)seen.Add(offer.def.id);}
            Require(Constellation.Nodes.All(n=>seen.Contains(n.item)),"all 36 unlocks actually appear through production reward pools");
            bool attackChoice=true,uniqueChoices=true,linkedSupport=true,freshGated=true;var discoveries=new HashSet<string>();
            foreach(int seed in Enumerable.Range(1,240))
            {
                var deck=new AbilityDeck(seed){RewardFilter=ChapterRewardAllowed};deck.Open(DraftKind.Ability,false);
                attackChoice&=deck.Offers.Any(o=>new[]{"SteelJab","Breaker","ArcBolt","ReapingCut"}.Contains(o.def.id));
                uniqueChoices&=deck.Offers.Length==3&&deck.Offers.Select(o=>o.def.id).Distinct().Count()==3&&deck.Offers.All(o=>o.rank>=1&&o.rank<=5);
                foreach(var o in deck.Offers)discoveries.Add(o.def.id);
                var setup=deck.Acquire(LoadoutCatalog.Find("StormLash"));deck.Move(setup,PlaceKind.Sequence,2);deck.Open(DraftKind.Mixed,false);
                linkedSupport&=BuildSynergies.Find(deck,deck.Offers[1].def).Any(l=>!l.text.StartsWith("Needs",StringComparison.Ordinal));
                var fresh=new AbilityDeck(seed){RewardFilter=d=>!Constellation.Nodes.Any(n=>n.item==d.id)};fresh.Open(DraftKind.Ability,false);freshGated&=fresh.Offers.All(o=>!ConstellationContent.IsNew(o.def.id));
            }
            Require(attackChoice&&uniqueChoices,"expanded ability drafts retain an attack, three unique choices and normal rarity bounds");
            Require(ConstellationContent.Abilities.All(d=>discoveries.Contains(d.id)),"all six new abilities remain discoverable beside the attack safeguard");
            Require(linkedSupport&&freshGated,"expanded mixed support has an equipped interaction; fresh pools never leak locked patterns");
            foreach(var d in ConstellationContent.All){var deck=new AbilityDeck(9);var item=deck.Acquire(d,2);deck.SuggestedPlace(d,out var place,out int index);Require(deck.Move(item,place,index),"normal equip destination / "+d.id);deck.Acquire(d,2);deck.Acquire(d,2);Require(item.rank==3&&deck.Items.Contains(item),"same-rarity fusion / "+d.id);}
            // Tests invoke the live effect methods, with controlled target arrangements.
            foreach(int rank in new[]{1,3,5})
            {
                foreach(var d in ConstellationContent.Weapons)
                {
                    ProbeReset();var host=ProbeHost(d,null,rank);var target=ProbeEnemy(0,2);target.health=180;target.stun=1;Health=MaxHealth-50;MarkEnemy(target,host);var second=ProbeEnemy(.2f,3);float hp=target.health;ResolveBasic(host,activeCard,target,false);
                    Require(target.health<hp&&CurrentTurn.extra>=0,"real weapon contact / "+d.id+" / "+rank);
                    if(d.id=="pileknuckles")Require(flights.Count==1,"stagger enables knuckle launch / "+rank);
                    if(d.id=="arccarbine")Require(marks[target.id].until==0&&second.health<500,"carbine pierces and consumes Conductive / "+rank);
                    if(d.id=="rivetsabre")Require(hp-target.health>=activeCard.damage+8*Effect(host)-.01f,"sabre adds wounded cutting hit / "+rank);
                    if(d.id=="letterpistol")Require(Ammo(host)==3&&second.stun>0,"first pistol round releases stagger / "+rank);
                    if(d.id=="leechgrips")Require(Health>MaxHealth-50&&CurrentTurn.siphonHealed<=MaxHealth*.01f+.001f,"grips mark before hit with shared healing cap / "+rank);
                    if(d.id=="bastionmaul")Require(BarrierAmount>0&&Deck.Weapons[0]==Deck.Weapons[1],"maul guards once with shared two-handed host / "+rank);
                }
                foreach(var d in ConstellationContent.Abilities)
                {
                    ProbeReset();var host=ProbeHost(d,null,rank);var target=ProbeEnemy(0,2);var second=ProbeEnemy(.3f,3);var far=ProbeEnemy(30,30);target.stun=1;DrainMark(target,host);Health=MaxHealth-50;actionTarget=target;Resolve(activeCard);
                    Require(far.health==500,"ability respects range / "+d.id+" / "+rank);
                    if(d.id=="RepulsorGuard"){Require(BarrierAmount>0,"repulsor creates actual barrier / "+rank);ReceiveDamage(target,150);ProbeFrames(2);Require(BarrierAmount==0&&target.physics.LinearVelocity.sqrMagnitude>0,"repulsor break physically repels / "+rank);}
                    else Require(target.health<500&&second.health<500,"ability affects actual targets / "+d.id+" / "+rank);
                    if(d.id=="VectorSling")Require(flights.Count==1,"sling needs existing primary stagger / "+rank);
                    if(d.id=="StormLash")Require(marks.ContainsKey(target.id)&&marks.ContainsKey(second.id),"lash creates actual marks / "+rank);
                    if(d.id=="ReclaimPulse")Require(drains[target.id].until==0&&Health>MaxHealth-50,"reclaim heals then spends mark / "+rank);
                    if(d.id!="RepulsorGuard"){ProbeReset();host=ProbeHost(d,AugmentId.Focused,rank);target=ProbeEnemy(0,2);second=ProbeEnemy(.2f,3);actionTarget=target;Resolve(activeCard);Require(target.health<500&&second.health==500,"Focused narrows to one target / "+d.id+" / "+rank);}
                }
                foreach(var d in ConstellationContent.Gear)
                {
                    ProbeReset();var weapon=ProbeHost(LoadoutCatalog.Find("pistol"));var item=Deck.Acquire(d,rank);Deck.SuggestedPlace(d,out var place,out int index);Deck.Move(item,place,index);var target=ProbeEnemy(0,2);var second=ProbeEnemy(.1f,2.7f);Health=MaxHealth-50;int beforeNotes=InteractionNotes.Values.Sum();
                    switch(d.signature)
                    {
                        case Signature.ImpactLens:OnCollision(target,target.physics.Center,10);Require(ContributionFor(item).direct>0,"collision damage belongs to lens / "+rank);break;
                        case Signature.Counterweight:Launch(target);Require(BarrierAmount>0,"launch grants boots barrier / "+rank);break;
                        case Signature.Switchboard:MarkEnemy(target,weapon);ConsumeMark(target);Require(marks.TryGetValue(second.id,out var m)&&m.owner==item,"discharge spreads one owned mark / "+rank);break;
                        case Signature.StaticShroud:ExpansionLoop();Require(marks.Count==2,"loop marks nearby targets / "+rank);break;
                        case Signature.StormCapacitor:MarkEnemy(target,weapon);ConsumeMark(target);Require(second.stun>0,"discharge staggers neighbors / "+rank);break;
                        case Signature.ExecutionHood:target.health=200;Hurt(target,1,Vector3.zero,0);Require(ContributionFor(item).direct>0,"wounded bonus credited to hood / "+rank);break;
                        case Signature.SpurTreads:Hurt(target,999,Vector3.zero,0,true);Require(second.stun>0,"sever staggers neighbors / "+rank);break;
                        case Signature.BallisticVest:target.rig.Root.position=new Vector3(0,0,6);Physics.SyncTransforms();Hurt(target,1,Vector3.zero,0);Require(BarrierAmount>0,"distant gun hit grants vest barrier / "+rank);break;
                        case Signature.VentGreaves:float hp=Health;Replenish(weapon);Require(Health==hp,"full magazine gives no healing / "+rank);ammunition[weapon]=0;Replenish(weapon);Require(Health>hp,"actual reload heals / "+rank);break;
                        case Signature.Rangefinder:target.stun=1;Hurt(target,1,Vector3.zero,0);Require(ContributionFor(item).direct>0,"stagger payoff credited to rig / "+rank);break;
                        case Signature.MercyLens:target.health=200;Hurt(target,1,Vector3.zero,0);Require(drains[target.id].owner==item,"wounded hit creates helmet-owned Siphon / "+rank);break;
                        case Signature.Reservoir:DrainMark(target,weapon);Hurt(target,20,Vector3.zero,0);Require(BarrierAmount>0,"actual healing converts to barrier / "+rank);break;
                        case Signature.WardenVisor:AddBarrier(weapon,10,3);ReceiveDamage(target,5);Require(target.stun>0,"actual absorption interrupts attacker / "+rank);break;
                        case Signature.ReturnSpring:AddBarrier(weapon,10,3);ReceiveDamage(target,5);ProbeFrames(2);Require(target.physics.LinearVelocity.sqrMagnitude>0,"actual absorption shoves attacker / "+rank);break;
                    }
                    Require(BarrierAmount<=90&&CurrentTurn.extra>=0,"gear honors global budgets / "+d.id+" / "+rank);
                }
                foreach(var d in ConstellationContent.Ultimates)
                {
                    ProbeReset();var item=Deck.Acquire(d,rank);Deck.Move(item,PlaceKind.Ultimate);currentCredit=new KillCredit{item=item,action=actionSerial};var target=ProbeEnemy(0,2);ProbeEnemy(.2f,3);var side=ProbeEnemy(7,1);Health=MaxHealth-50;ResolveUltimate();
                    Require(d.id=="lastbastion"?BarrierAmount>0:target.health<500,"real ultimate effect / "+d.id+" / "+rank);Require(BarrierAmount<=90&&CurrentTurn.extra>=0,"ultimate honors global budgets / "+d.id+" / "+rank);
                    if(d.id=="freighttrain")Require(flights.Count==2&&flights[0].chain==flights[1].chain&&side.health==500,"freight lane and shared cascade / "+rank);
                    if(d.id=="bloodcircuit")Require(Health>MaxHealth-50&&CurrentTurn.siphonHealed<=MaxHealth*.01f+.001f,"blood circuit shares action heal limit / "+rank);
                }
            }
            // Per-host weapon payoff; paired items do not accidentally share a proc key.
            ProbeReset();var a=ProbeHost(LoadoutCatalog.Find("pileknuckles"));var b=Deck.Acquire(a.def);Deck.Move(b,PlaceKind.Weapon,1);var left=ProbeEnemy(0,2);var right=ProbeEnemy(.4f,2);left.stun=right.stun=1;ResolveBasic(a,LoadoutCatalog.Action(a),left,false);currentCredit=new KillCredit{item=Deck.Basics[1],weapon=b,action=actionSerial};ResolveBasic(b,LoadoutCatalog.Action(b),right,true);Require(flights.Count==2,"distinct paired knuckles each get one launch");ResolveBasic(b,LoadoutCatalog.Action(b),right,true);Require(flights.Count==2,"same host cannot add another launch in the originating action");
            ProbeReset();var guard=ProbeHost(LoadoutCatalog.Find("RepulsorGuard"),AugmentId.Discharge);var attacker=ProbeEnemy(0,2);actionTarget=attacker;Resolve(activeCard);ReceiveDamage(attacker,30);Require(attacker.health<500&&counterBank==0,"repulsor Discharge replaces repulsion with absorbed-damage burst");
            ProbeReset();var freight=Deck.Acquire(LoadoutCatalog.Find("freighttrain"));Deck.Move(freight,PlaceKind.Ultimate);currentCredit=new KillCredit{item=freight,action=actionSerial};var fragileA=ProbeEnemy(0,2);var fragileB=ProbeEnemy(.2f,3);fragileA.health=fragileB.health=1;int launches=Launches;ResolveUltimate();Require(Launches-launches==2&&flights.Count==2&&flights.All(f=>f.body.deathAge>=0),"Freight Train launches each lethal target once, without corpse relaunch");
            ProbeReset();var carbine=ProbeHost(LoadoutCatalog.Find("arccarbine"));var grounding=Deck.Acquire(LoadoutCatalog.Find("grounding"));Deck.Move(grounding,PlaceKind.Armor);var marked=ProbeEnemy(0,2);MarkEnemy(marked,carbine);ResolveBasic(carbine,activeCard,marked,false);Require(BarrierAmount>0&&barriers.Any(x=>x.host==grounding),"carbine Conductive discharge supports Grounding armor");
            if(render)
            {
                MainMenu();OpenOffice();foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2560,1080)}){Screen.SetResolution(size.x,size.y,false);for(int i=0;i<12;i++)yield return null;ReducedMotion=size.x==1280;foreach(int branch in new[]{0,1,2,3,4,5,6}){hud.SelectOfficeBranch(branch);yield return Shot("office-"+size.x+"-"+branch);}}
                Screen.SetResolution(1920,1080,false);for(int i=0;i<12;i++)yield return null;CloseOffice();
                foreach(var d in ConstellationContent.Weapons.Concat(ConstellationContent.Abilities).Concat(ConstellationContent.Ultimates))
                {
                    BeginCombatPreview(3);fx.muted=true;Health=MaxHealth;UltimateAutomatic=false;var host=Deck.Acquire(d,3);Deck.SuggestedPlace(d,out var place,out int index);Require(Deck.Move(host,place,index),"native fixture places item / "+d.id);
                    if(d.kind==ItemKind.Ultimate){Require(Deck.Ultimate==host,"native fixture equips actual ultimate / "+d.id);UltimateRemaining=0;RequestUltimate();}
                    else if(d.kind==ItemKind.Ability){Require(Deck.Move(host,PlaceKind.Sequence,3)&&Deck.Slots[3]==host,"native fixture equips actual ability outside the locked pair / "+d.id);sequencePointer=expectedSlot=3;ActiveSlot=PairSlot=-1;}
                    else Require(Deck.Weapons.Contains(host),"native fixture equips actual weapon / "+d.id);
                    // Live equipment is normally rebuilt at BeginWave. This fixture changes
                    // the deck after preview setup, so refresh the same meshes and identity rails.
                    hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);
                    hero.WeaponIdentity(Deck.WeaponFor(Deck.Basics[0]),Deck.WeaponFor(Deck.Basics[1]));
                    bool delivered=false;
                    for(int i=0;i<900;i++)
                    {
                        AdvanceCombat(1f/60);if(i%3==0)yield return null;
                        bool current=d.kind==ItemKind.Ultimate?ActiveSlot==5:d.kind==ItemKind.Ability?actionItem==host:actionWeapon==host||pairWeapon==host;
                        if(current&&actionDelivered){delivered=true;for(int frame=0;frame<6;frame++){AdvanceCombat(1f/60);yield return null;}yield return Shot("combat-"+d.id);break;}
                    }
                    Require(delivered&&OrderErrors==0,"native item delivers its action in sequence / "+d.id);if(d.kind==ItemKind.Ultimate)Require(UltimateCasts>0,"native ultimate actually cast / "+d.id);
                }
            }
            report.passed=true;Write();Debug.Log("CONSTELLATION_SUCCESS / "+report.assertions);Application.Quit(0);
        }
    }
}
