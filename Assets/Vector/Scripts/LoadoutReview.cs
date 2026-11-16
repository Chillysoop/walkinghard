using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        bool workbenchTextAudit;
        void AuditWorkbenchText(float mountW,float cellW,float cellH,float cardW)
        {
            if(!workbenchTextAudit||Event.current.type!=EventType.Repaint)return;workbenchTextAudit=false;
            foreach(var def in LoadoutCatalog.All.Concat(AugmentCatalog.All))
            {
                ReviewCheck(Style(10,true).CalcHeight(new GUIContent(def.name),cellW-10)<=cellH-32+.5f,"storage name fits without clipping: "+def.id);
                ReviewCheck(Style(11,true).CalcHeight(new GUIContent(def.name),mountW-46)<=32,"mount name fits: "+def.id);
                ReviewCheck(Style(17,true).CalcHeight(new GUIContent(def.name),cardW-103)<=36,"reward name fits: "+def.id);
            }
            foreach(string name in ItemRarity.Names.Skip(1))ReviewCheck(Style(10).CalcSize(new GUIContent(name)).x<=cellW-36,"full rarity label fits storage: "+name);
        }
        static string LoadoutShape(AbilityDeck d)
        {
            string Ref(LoadoutItem i)=>i==null?"-":d.Items.IndexOf(i).ToString();
            return string.Join(";",d.Items.Select(i=>i.def.id+":"+i.rank+":"+i.mount+":"+Ref(i.augment)))+"/"+string.Join(",",d.Slots.Select(Ref))+"/"+string.Join(",",d.Weapons.Select(Ref))+"/"+string.Join(",",d.Armor.Select(Ref))+"/"+Ref(d.Rig)+"/"+Ref(d.Ultimate);
        }
        AbilityDeck.Projection CheckProjection(AbilityDeck d,RewardOffer offer,LoadoutItem source,PlaceKind place,int index=0,LoadoutItem host=null)
        {
            if(offer!=null){d.Open(DraftKind.Mixed,false);d.Offers[0]=offer;}
            string before=LoadoutShape(d),receipt=d.Receipt;int events=0;d.Fused+=(a,b)=>events++;
            var random=(CheckpointRandom)typeof(AbilityDeck).GetField("random",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(d);int draws=random.Draws;
            var p=d.PreviewChange(offer,source,place,index,host);
            ReviewCheck(LoadoutShape(d)==before&&receipt==d.Receipt&&events==0&&random.Draws==draws,"hover leaves live graph, receipt, events and random stream unchanged");
            ReviewCheck(p.valid,"projection accepts legal destination");
            if(offer!=null){ReviewCheck(d.ClaimTo(0,host==null?place:PlaceKind.Inventory,index),"real reward claim succeeds");if(host!=null)d.Attach(d.Inventory.Last(i=>i.def==offer.def),host);}
            else if(host!=null)d.Attach(source,host);else d.Move(source,place,index);
            ReviewCheck(LoadoutShape(d)==LoadoutShape(p.after),"projected graph equals actual acquisition, fusion, placement and socket ownership");
            return p;
        }
        void CheckLoadoutProjections()
        {
            foreach(var def in LoadoutCatalog.All.Where(x=>x.kind!=ItemKind.Augment))
            {
                var d=new AbilityDeck(11);var owned=def==LoadoutCatalog.Fists?d.Fists:d.Acquire(def);d.SuggestedPlace(def,out var place,out int index);d.Move(owned,place,index);d.Acquire(def);
                var p=CheckProjection(d,new RewardOffer(def),null,place,index);
                ReviewCheck(p.fusionRanks.Contains(2),"same-rarity third copy upgrades: "+def.id);
            }
            var visor=LoadoutCatalog.Find("visor");
            foreach(int rank in new[]{1,2,3,4,5})
            {
                var d=new AbilityDeck(rank);d.Move(d.Acquire(visor,rank),PlaceKind.Helmet);d.Acquire(visor,rank);
                var p=CheckProjection(d,new RewardOffer(visor,1,rank),null,PlaceKind.Inventory);
                ReviewCheck(p.fusionRanks.Count==(rank==5?0:1),"storage fusion respects maximum rarity "+rank);
            }
            var chain=new AbilityDeck(1);chain.Move(chain.Acquire(visor),PlaceKind.Helmet);chain.Acquire(visor);chain.Acquire(visor,2);chain.Acquire(visor,2);
            var chained=CheckProjection(chain,new RewardOffer(visor),null,PlaceKind.Helmet);
            ReviewCheck(chained.incoming.rank==3&&chained.fusionRanks.SequenceEqual(new[]{2,3}),"chained fusion forecasts final Rare keeper");
            var mismatch=new AbilityDeck(2);mismatch.Move(mismatch.Acquire(visor,3),PlaceKind.Helmet);mismatch.Acquire(visor);
            var mixed=CheckProjection(mismatch,new RewardOffer(visor),null,PlaceKind.Helmet);
            ReviewCheck(mixed.fusionRanks.Count==0&&mismatch.Inventory.Any(i=>i.def==visor&&i.rank==3),"rarity mismatch is a replacement, not an upgrade");
            var bundle=new AbilityDeck(3);bundle.Move(bundle.Acquire(visor),PlaceKind.Helmet);bundle.Acquire(visor);
            CheckProjection(bundle,new RewardOffer(visor,2),null,PlaceKind.Helmet);
            ReviewCheck(bundle.Armor[0].rank==2&&bundle.Inventory.Any(i=>i.def==visor&&i.rank==1),"bundle keeps promoted copy equipped and extra Common stored");
            var fists=new AbilityDeck(4);var shared=CheckProjection(fists,new RewardOffer(LoadoutCatalog.Fists),null,PlaceKind.Inventory);
            ReviewCheck(shared.fusionRanks.Count==0&&fists.Fists.rank==1,"two empty hands count as only one owned Fists copy");
            var two=new AbilityDeck(5);two.Move(two.Acquire(LoadoutCatalog.Find("sword")),PlaceKind.Weapon,0);two.Move(two.Acquire(LoadoutCatalog.Find("pistol")),PlaceKind.Weapon,1);
            CheckProjection(two,new RewardOffer(LoadoutCatalog.Find("hammer")),null,PlaceKind.Weapon,1);
            ReviewCheck(two.TwoHanded&&two.Inventory.Count()==2,"two-handed weapon stores both different weapons");
            CheckProjection(two,new RewardOffer(LoadoutCatalog.Find("sword")),null,PlaceKind.Weapon,1);
            ReviewCheck(!two.TwoHanded&&two.Weapons[0]==null&&two.Weapons[1].def.id=="sword","one-handed weapon replaces 2H group with other hand empty");
            var actions=new AbilityDeck(6);foreach(string id in new[]{"ArcBolt","GravityWell","ReapingCut"})actions.Move(actions.Acquire(LoadoutCatalog.Find(id)),PlaceKind.Sequence,Array.FindIndex(actions.Slots,i=>i==null));
            CheckProjection(actions,new RewardOffer(LoadoutCatalog.Find("MassDriver")),null,PlaceKind.Sequence,0);
            ReviewCheck(actions.Slots.Count(i=>i.def.kind==ItemKind.Basic)==2&&actions.Inventory.Any(i=>i.def.id=="ArcBolt"),"ability over basic preserves basics and stores displaced ability");
            var sockets=new AbilityDeck(7);var sword=sockets.Acquire(LoadoutCatalog.Find("sword"));sockets.Move(sword,PlaceKind.Weapon,0);var spare=sockets.Acquire(sword.def);
            sockets.Attach(sockets.Acquire(AugmentCatalog.Get(AugmentId.Crosscut)),sword);var oldModule=sockets.Acquire(AugmentCatalog.Get(AugmentId.HookedEdge));sockets.Attach(oldModule,spare);
            CheckProjection(sockets,new RewardOffer(sword.def),null,PlaceKind.Weapon,0);
            ReviewCheck(sword.augment.def.module.id==AugmentId.Crosscut&&sockets.Inventory.Contains(oldModule),"fusion keeps keeper socket and returns consumed host module");
            CheckProjection(sockets,null,oldModule,PlaceKind.Weapon,0,sword);
            ReviewCheck(sword.augment==oldModule,"owned module attaches without acquiring another reward");
            CheckProjection(sockets,null,oldModule,PlaceKind.Inventory);
            ReviewCheck(sword.augment==null&&sockets.Inventory.Contains(oldModule),"installed module can return to storage");
            var hammer=sockets.Acquire(LoadoutCatalog.Find("hammer"));sockets.Move(hammer,PlaceKind.Weapon,0);
            CheckProjection(sockets,null,sockets.Basics[0],PlaceKind.Sequence,3);
            ReviewCheck(sockets.Slots[3]==sockets.Basics[0]&&sockets.Slots[4]==sockets.Basics[1],"two-handed basic reorder projects both locked actions");
        }
        void WorkbenchFixture()
        {
            game.Begin();game.HoldCheckpointAfterPick=true;storageFilter=0;inventoryScroll=Vector2.zero;ClearLoadoutGesture();
            var d=game.Deck;
            void Equip(string id,int rank,PlaceKind place,int index=0){d.Move(d.Acquire(LoadoutCatalog.Find(id),rank),place,index);}
            Equip("sword",2,PlaceKind.Weapon,0);Equip("pistol",3,PlaceKind.Weapon,1);Equip("visor",1,PlaceKind.Helmet);d.Acquire(LoadoutCatalog.Find("visor"));
            Equip("plate",2,PlaceKind.Armor);Equip("treads",1,PlaceKind.Boots);Equip("faultline",1,PlaceKind.Rig);Equip("cyclone",3,PlaceKind.Ultimate);
            Equip("GravityWell",2,PlaceKind.Sequence,2);Equip("ReapingCut",1,PlaceKind.Sequence,3);Equip("MassDriver",1,PlaceKind.Sequence,4);
            foreach(var def in LoadoutCatalog.All.Where(i=>i.kind!=ItemKind.Basic).Take(18))d.Acquire(def,3);
            d.Open(DraftKind.Mixed,false);d.Offers[0]=new RewardOffer(LoadoutCatalog.Find("visor"));d.Offers[1]=new RewardOffer(LoadoutCatalog.Find("hammer"),1,3);d.Offers[2]=new RewardOffer(LoadoutCatalog.Find("ArcBolt"),1,2);
            reviewPointer=new Vector2(-200,-200);
        }
        IEnumerator RunLoadoutReview(bool rendered)
        {
            tutorialReviewActive=rendered;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception||type==LogType.Error){File.WriteAllText(Path.Combine(Application.dataPath,"..","loadout-failure.txt"),message+"\n"+trace);Application.Quit(3);}};
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","LoadoutReview"));Directory.CreateDirectory(directory);
            CheckLoadoutProjections();
            if(rendered)
            {
                Screen.SetResolution(1280,720,false);WorkbenchFixture();workbenchTextAudit=true;for(int i=0;i<12;i++)yield return null;
                ReviewCheck(dropRegions.Count(r=>r.offer<0&&r.place==PlaceKind.Sequence&&!r.socket)==5&&dropRegions.Count(r=>r.place==PlaceKind.Weapon&&!r.socket)==2&&dropRegions.Any(r=>r.place==PlaceKind.Helmet)&&dropRegions.Any(r=>r.place==PlaceKind.Inventory&&r.index==-1),"full build remains visible before choosing a reward");
                ReviewCheck(!inlineComparisonVisible&&comparisonRect==Rect.zero,"idle workbench has no suggestion panel or reserved comparison column");
                ReviewCheck(dropRegions.Count(r=>r.offer<0&&r.place==PlaceKind.Inventory&&r.item!=null&&!r.socket)==12,"expanded storage shows twelve named items");
                yield return CapturePresentation(directory,"01-whole-build");
                string before=LoadoutShape(game.Deck);reviewPointer=dropRegions.First(r=>r.offer==0).rect.center;yield return null;yield return null;
                ReviewCheck(!inlineComparisonVisible,"reward hover highlights destinations without opening a suggestion panel");
                game.SelectOffer(0);reviewPointer=ReviewRegion(PlaceKind.Helmet).rect.center;yield return CapturePresentation(directory,"02-hover-fusion");
                ReviewCheck(loadoutHover.projection.fusionRanks.Last()==2&&loadoutHover.target.place==PlaceKind.Helmet&&LoadoutShape(game.Deck)==before&&!game.Deck.RewardClaimed,"hover forecasts fusion without acquiring or moving anything");
                reviewPointer=comparisonRect.center;yield return null;yield return null;
                ReviewCheck(loadoutHover.offerIndex==0,"hover details stay available when pointer enters their scroll area");
                ReviewCheck(!dropRegions.Any(r=>r.rect.Overlaps(comparisonRect)),"hover details never cover a reward, mount, stored item or sequence destination");
                reviewPointer=new Vector2(-200,-200);yield return null;yield return null;
                ReviewCheck(!inlineComparisonVisible&&comparisonRect==Rect.zero,"hover comparison disappears when the pointer leaves");
                game.SelectOffer(1);reviewPointer=ReviewRegion(PlaceKind.Weapon,0).rect.center;yield return CapturePresentation(directory,"03-two-hands");
                ReviewCheck(loadoutHover.fate.Contains("Edge Sword")&&loadoutHover.fate.Contains("Pulse Pistol")&&loadoutHover.heading.Contains("BOTH HANDS"),"two-handed hover names both displaced weapons and both mounts");
                yield return ReviewDoubleClick(dropRegions.First(r=>r.offer==1).rect.center);
                ReviewCheck(!game.Deck.RewardClaimed&&!game.Deck.TwoHanded,"reward double-click selects without silently equipping");
                reviewPointer=ReviewRegion(PlaceKind.Armor).rect.center;yield return null;yield return null;
                ReviewCheck(loadoutHover.invalid&&!game.Deck.RewardClaimed,"wrong-slot hover explains invalid destination");
                before=LoadoutShape(game.Deck);yield return ReviewDrag(dropRegions.First(r=>r.offer==1).rect.center,ReviewRegion(PlaceKind.Armor).rect.center);
                ReviewCheck(!game.Deck.RewardClaimed&&LoadoutShape(game.Deck)==before,"invalid native drop cannot apply a different action than its preview");
                yield return ReviewDrag(dropRegions.First(r=>r.offer==1).rect.center,ReviewRegion(PlaceKind.Weapon,1).rect.center);
                ReviewCheck(game.Deck.TwoHanded&&game.Deck.Inventory.Any(i=>i.def.id=="sword"&&i.rank==2)&&game.Deck.Inventory.Any(i=>i.def.id=="pistol"&&i.rank==3),"native drag produces the previewed two-handed replacement");
                yield return CapturePresentation(directory,"04-two-handed-equipped");
                WorkbenchFixture();yield return null;yield return null;
                yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,ReviewRegion(PlaceKind.Helmet).rect.center);
                ReviewCheck(game.Deck.Armor[0].rank==2&&game.Deck.RewardClaimed,"native fusion drop upgrades equipped helmet");
                yield return CapturePresentation(directory,"05-upgrade-result");
                WorkbenchFixture();game.Deck.Acquire(LoadoutCatalog.Find("visor"),2);game.Deck.Acquire(LoadoutCatalog.Find("visor"),2);yield return null;yield return null;
                yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,ReviewRegion(PlaceKind.Helmet).rect.center);
                yield return CapturePresentation(directory,"05b-chain-upgrade-result");
                ReviewCheck(shownFusion.before==1&&shownFusion.after==3&&game.Deck.Armor[0].rank==3&&game.FusionHistory.Count==2,"chain result celebrates final Rare tier without mutating fusion history");
                WorkbenchFixture();yield return null;yield return null;
                game.SelectOffer(2);reviewPointer=ReviewRegion(PlaceKind.Sequence,3).rect.center;yield return CapturePresentation(directory,"06-ability-replacement");
                ReviewCheck(loadoutHover.fate.Contains("Reaping Cut"),"explicit sequence hover names the displaced ability");
                yield return ReviewDrag(dropRegions.First(r=>r.offer==2).rect.center,ReviewRegion(PlaceKind.Sequence,3).rect.center);
                ReviewCheck(game.Deck.Slots[3].def.id=="ArcBolt"&&game.Deck.Inventory.Any(i=>i.def.id=="ReapingCut"&&i.rank==1),"native ability replacement preserves stored item");
                WorkbenchFixture();yield return null;yield return null;
                storageFilter=4;inventoryScroll.y=0;game.Deck.Acquire(AugmentCatalog.Get(AugmentId.Crosscut));yield return null;yield return null;
                var module=game.Deck.Inventory.First(i=>i.def.id=="aug-Crosscut");var moduleRegion=dropRegions.First(r=>r.item==module&&!r.socket);
                yield return ReviewDrag(moduleRegion.rect.center,ReviewRegion(PlaceKind.Weapon,0).rect.center);
                ReviewCheck(game.Deck.Weapons[0].augment==module&&!game.Deck.RewardClaimed,"native stored module attachment preserves unclaimed reward");
                yield return CapturePresentation(directory,"07-module-attached");
                yield return ReviewDrag(SocketRect(ReviewRegion(PlaceKind.Weapon,0)).center,ReviewRegion(PlaceKind.Inventory,-1).rect.center);
                ReviewCheck(game.Deck.Weapons[0].augment==null&&game.Deck.Inventory.Contains(module),"native socket removal returns module to storage");
                WorkbenchFixture();yield return null;yield return null;
                var storedSword=game.Deck.Inventory.First(i=>i.def.id=="sword"&&i.rank==3);game.SelectOffer(0);
                var storedRect=dropRegions.First(r=>r.item==storedSword&&!r.socket).rect;
                yield return ReviewMouse(EventType.MouseDown,storedRect.center);yield return ReviewMouse(EventType.MouseUp,storedRect.center);
                ReviewCheck(selectedItem==storedSword,"native click selects owned source while reward remains pending");
                ConfirmWorkbench();yield return null;yield return null;
                ReviewCheck(game.Deck.Weapons[0]==storedSword&&!game.Deck.RewardClaimed,"Enter and primary button equip owned source without claiming pending reward");
                WorkbenchFixture();yield return null;yield return null;
                storedSword=game.Deck.Inventory.First(i=>i.def.id=="sword"&&i.rank==3);
                yield return ReviewDrag(dropRegions.First(r=>r.item==storedSword&&!r.socket).rect.center,ReviewRegion(PlaceKind.Sequence,0).rect.center);
                ReviewCheck(game.Deck.Weapons[0]==storedSword&&game.Deck.Slots[0].def.kind==ItemKind.Basic&&!game.Deck.RewardClaimed,"owned weapon can use its basic-attack destination like a reward weapon");
                game.Begin();game.HoldCheckpointAfterPick=true;ClearLoadoutGesture();game.Deck.Open(DraftKind.Mixed,false);game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("ArcBolt"));game.SelectOffer(0);yield return null;yield return null;
                reviewPointer=ReviewRegion(PlaceKind.Sequence,4).rect.center;yield return null;yield return null;
                ReviewCheck(loadoutHover.explicitTarget&&loadoutHover.target.index==4&&loadoutHover.projection.after.Slots[4]?.def.id=="ArcBolt"&&game.Deck.Slots[4]==null,"hover on an empty later slot previews that exact slot, not the suggested first gap");
                yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,ReviewRegion(PlaceKind.Sequence,4).rect.center);
                ReviewCheck(game.Deck.Slots[4]?.def.id=="ArcBolt"&&game.Deck.Slots[2]==null,"native empty-slot drop matches its exact preview");
                game.Deck.Open(DraftKind.Mixed,false);game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("sword"));game.SelectOffer(0);yield return null;yield return null;
                reviewPointer=ReviewRegion(PlaceKind.Inventory,-1).rect.center;yield return null;yield return null;
                ReviewCheck(loadoutHover.explicitTarget&&loadoutHover.target.place==PlaceKind.Inventory&&loadoutHover.projection.after.Inventory.Any(i=>i.def.id=="sword")&&loadoutHover.projection.after.Weapons.All(i=>i==null),"hover on Store previews storage rather than suggested equip");
                yield return CapturePresentation(directory,"07b-store-preview");
                game.Begin();game.HoldCheckpointAfterPick=true;ClearLoadoutGesture();storageFilter=0;inventoryScroll=Vector2.zero;
                var ownedArc=game.Deck.Acquire(LoadoutCatalog.Find("ArcBolt"));game.Deck.Open(DraftKind.Mixed,false);yield return null;yield return null;
                game.Deck.Assign(4);game.SelectOffer(0);
                var ownedArcPoint=dropRegions.First(r=>r.item==ownedArc&&!r.socket).rect.center;
                yield return ReviewMouse(EventType.MouseDown,ownedArcPoint);yield return ReviewMouse(EventType.MouseUp,ownedArcPoint);
                reviewPointer=new Vector2(-200,-200);yield return null;yield return null;
                ReviewCheck(loadoutHover.target.index==2&&loadoutHover.projection.after.Slots[2]?.def.id=="ArcBolt","owned ability preview ignores a different pending reward keyboard slot");
                ConfirmWorkbench();ReviewCheck(game.Deck.Slots[2]==ownedArc&&game.Deck.Slots[4]==null&&!game.Deck.RewardClaimed,"owned ability primary action matches preview with pending reward slot selected");
                foreach(int width in new[]{1280,1920,2560})
                {
                    Screen.SetResolution(width,width==1280?720:1080,false);WorkbenchFixture();workbenchTextAudit=true;for(int i=0;i<12;i++)yield return null;
                    game.SelectOffer(1);reviewPointer=ReviewRegion(PlaceKind.Weapon,0).rect.center;yield return CapturePresentation(directory,"08-layout-"+width);
                    ReviewCheck(dropRegions.Where(r=>r.offer>=0).All(r=>r.rect.yMax<comparisonRect.y)&&dropRegions.Where(r=>r.offer<0&&r.place==PlaceKind.Sequence).All(r=>r.rect.y>comparisonRect.yMax),"transient comparison fits between rewards and sequence at "+width);
                    ReviewCheck(!dropRegions.Any(r=>r.rect.Overlaps(comparisonRect)),"hover details clear every interaction target at "+width);
                    ReviewCheck(dropRegions.Where(r=>r.place==PlaceKind.Inventory&&r.item!=null&&!r.socket).All(r=>r.rect.width>=120),"storage names have readable space at "+width);
                }
            }
            File.WriteAllText(Path.Combine(directory,rendered?"rendered-checks.txt":"model-checks.txt"),string.Join("\n",tutorialChecks));
            Debug.Log("LOADOUT_REVIEW_SUCCESS: "+tutorialChecks.Count+" assertions; "+(rendered?"native input and visual capture":"model projections"));Application.Quit(0);
        }
    }
}
