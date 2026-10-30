using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        // Explicit native QA mode exercises the production mouse handler, without OS input.
        bool tutorialReviewActive,reviewAuditPending;
        Event reviewMouse;
        Vector2 reviewPointer=new Vector2(-200,-200);
        Vector2 Pointer=>tutorialReviewActive?reviewPointer:Event.current.mousePosition;
        readonly List<string> tutorialChecks=new List<string>();
        Event TakeReviewMouse()
        {
            if(!tutorialReviewActive||reviewMouse==null||Event.current.type!=EventType.Repaint)return null;
            var e=reviewMouse;reviewMouse=null;return e;
        }
        void ReviewCheck(bool value,string name)
        {if(!value)throw new InvalidOperationException("TUTORIAL_REVIEW_FAILED: "+name);tutorialChecks.Add(name);}
        IEnumerator ReviewMouse(EventType type,Vector2 point)
        {
            reviewPointer=point;reviewMouse=new Event{type=type,button=0,mousePosition=point};
            float deadline=Time.realtimeSinceStartup+3;
            while(reviewMouse!=null&&Time.realtimeSinceStartup<deadline)yield return null;
            ReviewCheck(reviewMouse==null,"mouse handler consumed "+type);yield return null;
        }
        DropRegion ReviewRegion(PlaceKind place,int index=0)=>dropRegions.First(r=>r.offer<0&&!r.socket&&r.place==place&&r.index==index);
        IEnumerator ReviewDrag(Vector2 from,Vector2 to)
        {
            yield return ReviewMouse(EventType.MouseDown,from);
            yield return ReviewMouse(EventType.MouseDrag,to);
            yield return ReviewMouse(EventType.MouseUp,to);
            reviewPointer=new Vector2(-200,-200);yield return null;
        }
        IEnumerator ReviewDoubleClick(Vector2 at)
        {
            lastClickTime=-10;
            for(int i=0;i<2;i++){yield return ReviewMouse(EventType.MouseDown,at);yield return ReviewMouse(EventType.MouseUp,at);}
            reviewPointer=new Vector2(-200,-200);yield return null;
        }
        void ReviewTextFit()
        {
            if(!reviewAuditPending||Event.current.type!=EventType.Repaint)return;reviewAuditPending=false;
            foreach(var def in LoadoutCatalog.All.Concat(AugmentCatalog.All))
            {
                ReviewCheck(Style(17,true).CalcSize(new GUIContent(def.name)).x<=281,"reward title fits: "+def.id);
                ReviewCheck(Style(11,true).CalcSize(new GUIContent(Category(def))).x<=258,"category fits: "+def.id);
                ReviewCheck(Style(22,true).CalcHeight(new GUIContent(def.name),275)<=52,"focused reward title fits: "+def.id);
                ReviewCheck(Style(10).CalcSize(new GUIContent(ItemRole(def))).x<=275,"item role fits: "+def.id);
                ReviewCheck(Style(18,true).CalcSize(new GUIContent(ItemType(def))).x<=245,"large item type fits: "+def.id);
                for(int rank=1;rank<=5;rank++)
                {
                    var item=new LoadoutItem(def,rank);
                    ReviewCheck(Style(10,true).CalcSize(new GUIContent(RewardStats(def,rank))).x<=275,"reward stats fit: "+def.id+"/"+rank);
                    ReviewCheck(Style(9).CalcSize(new GUIContent(ItemSummary(item))).x<=167,"sequence summary fits: "+def.id+"/"+rank);
                }
            }
            foreach(var host in LoadoutCatalog.All.Where(d=>d.kind==ItemKind.Ability||d.kind==ItemKind.Weapon))
                foreach(var module in AugmentCatalog.All.Where(d=>d.module.Fits(host)))
                    ReviewCheck(Style(9).CalcSize(new GUIContent(ItemSummary(new LoadoutItem(host){augment=new LoadoutItem(module)}))).x<=167,"augmented summary fits: "+host.id+"/"+module.id);
        }
        IEnumerator RunTutorialReview(bool rendered)
        {
            tutorialReviewActive=rendered;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception||type==LogType.Error){File.WriteAllText(Path.Combine(Application.dataPath,"..","tutorial-failure.txt"),message+"\n"+trace);Application.Quit(3);}};
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","TutorialReview"));Directory.CreateDirectory(directory);
            yield return null;
            for(int ability=0;ability<3;ability++)for(int weapon=0;weapon<3;weapon++)
            {
                bool hold=(ability+weapon)%2==0;game.HoldCheckpointAfterPick=hold;game.SetCombatSpeed(hold?4:.5f);game.BeginTutorial();
                string path=ability+"/"+weapon+": ";
                game.TutorialNext();ReviewCheck(game.TutorialLesson==VectorEncounter.Lesson.Ability,path+"cannot skip task");
                int rerolls=game.Deck.Rerolls;game.Reroll();ReviewCheck(game.Deck.Rerolls==rerolls,path+"curated rewards cannot reroll");
                ReviewCheck(!game.ClaimReward(ability,PlaceKind.Weapon,0),path+"wrong category rejected");
                ReviewCheck(game.ClaimReward(ability,PlaceKind.Sequence,2)&&game.TutorialReady,path+"ability equipped; no auto advance");game.TutorialNext();
                PlaceKind destination=weapon==1?PlaceKind.Inventory:PlaceKind.Weapon;
                ReviewCheck(game.ClaimReward(weapon,destination,0),path+"weapon direct equip or store");
                if(destination==PlaceKind.Inventory){ReviewCheck(!game.TutorialReady,path+"stored weapon is not equipped");ReviewCheck(game.AutoEquip(game.Deck.Inventory.First(i=>i.def.kind==ItemKind.Weapon)),path+"stored weapon auto equip");}
                game.TutorialNext();var chosen=game.Deck.Weapons.First(i=>i!=null);
                ReviewCheck(game.Deck.Copies(chosen.def,1)==2,path+"two same-rarity copies before fusion");
                ReviewCheck(!game.ClaimReward(1,PlaceKind.Inventory,0)&&!game.Deck.RewardClaimed,path+"unrelated fusion choice rejected atomically");
                ReviewCheck(game.ClaimReward(0,PlaceKind.Inventory,0)&&chosen.rank==2&&game.TutorialReady,path+"third copy fuses equipped weapon");
                ReviewCheck(game.FusionHistory.Count==1,path+"fusion result available");game.TutorialNext();
                ReviewCheck(!game.ClaimAugmentToHost(0,game.Deck.Slots.First(i=>i?.def.id=="ForcePush"),false),path+"incompatible host rejected");
                ReviewCheck(game.ClaimAugmentToHost(0,game.TutorialArc,false)&&game.TutorialReady,path+"Echo attached");game.TutorialNext();
                if(game.Deck.TwoHanded){ReviewCheck(!game.MoveItem(game.TutorialArc,PlaceKind.Sequence,0),path+"linked pair cannot split");ReviewCheck(game.MoveItem(game.Deck.Basics[0],PlaceKind.Sequence,1),path+"linked pair moves");}
                ReviewCheck(game.MoveItem(game.TutorialArc,PlaceKind.Sequence,0)&&game.TutorialReady,path+"Arc starts sequence");game.TutorialNext();
                ReviewCheck(game.State==VectorEncounter.RunState.Running&&!game.TutorialReady,path+"practice starts; cannot instantly finish");
                ReviewCheck(!game.AutoEquip(game.TutorialArc.augment),path+"augment auto-equip cannot change a running fight");
                for(int step=0;step<180&&game.TutorialLesson==VectorEncounter.Lesson.Practice;step++)game.AdvanceTutorialPractice(.1f);
                ReviewCheck(game.TutorialLesson==VectorEncounter.Lesson.Finished&&game.AbilityCasts>=3,path+"practice uses production combat and completes");
                game.TutorialNext();ReviewCheck(!game.TutorialActive&&game.OpeningPicks==0&&game.Deck.Count==0&&game.Deck.Weapons.All(i=>i==null)&&game.Deck.Ultimate==null&&game.FusionHistory.Count==0,path+"real run is clean");
                ReviewCheck(game.HoldCheckpointAfterPick==hold&&game.CombatSpeed==(hold?4:.5f),path+"preferences restored");
                game.MainMenu();ReviewCheck(game.State==VectorEncounter.RunState.Ready,path+"return to main menu");yield return null;
            }
            if(rendered)
            {
                Screen.SetResolution(1280,720,false);for(int i=0;i<12;i++)yield return null;
                yield return CapturePresentation(directory,"01-main-menu");game.BeginTutorial();
                reviewAuditPending=true;yield return CapturePresentation(directory,"02-ability-guidance");
                ReviewCheck(!reviewAuditPending,"text fit audit ran in native GUI");
                yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,ReviewRegion(PlaceKind.Sequence,2).rect.center);
                ReviewCheck(game.TutorialReady,"mouse drag equips ability");game.TutorialNext();yield return null;
                reviewPointer=dropRegions.First(r=>r.offer==2).rect.center;yield return CapturePresentation(directory,"03-weapon-destinations");ReviewCheck(FocusedReward()==2,"weapon hover focuses exact reward");
                yield return ReviewDoubleClick(dropRegions.First(r=>r.offer==2).rect.center);
                ReviewCheck(game.Deck.TwoHanded&&game.TutorialReady,"double-click equips two-handed weapon");game.TutorialNext();
                ReviewCheck(SocketRect(ReviewRegion(PlaceKind.Weapon,0)).yMax<ReviewRegion(PlaceKind.Weapon,0).rect.y+63,"weapon socket clears item name");
                yield return CapturePresentation(directory,"04-fusion-before");
                yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,ReviewRegion(PlaceKind.Weapon,0).rect.center);
                ReviewCheck(game.TutorialReady,"mouse drop triggers fusion");yield return CapturePresentation(directory,"05-fusion-after");
                game.TutorialNext();yield return null;reviewPointer=dropRegions.First(r=>r.offer==0).rect.center;
                yield return CapturePresentation(directory,"06-augment-destinations");ReviewCheck(FocusedReward()==0,"augment hover focuses exact reward");
                yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,SocketRect(ReviewRegion(PlaceKind.Sequence,3)).center);
                ReviewCheck(game.TutorialReady,"mouse drop attaches augment to socket");game.TutorialNext();yield return CapturePresentation(directory,"07-linked-order");
                yield return ReviewDrag(ReviewRegion(PlaceKind.Sequence,0).rect.center,ReviewRegion(PlaceKind.Sequence,1).rect.center);
                yield return ReviewDrag(ReviewRegion(PlaceKind.Sequence,3).rect.center,ReviewRegion(PlaceKind.Sequence,0).rect.center);
                ReviewCheck(game.TutorialReady,"mouse reorders linked pair and ability");game.TutorialNext();
                float end=Time.realtimeSinceStartup+9;while(Time.realtimeSinceStartup<end)yield return null;
                yield return CapturePresentation(directory,"08-practice");game.TogglePause();float elapsed=game.Elapsed;yield return new WaitForSecondsRealtime(.25f);
                ReviewCheck(game.Elapsed==elapsed,"practice pause freezes combat");game.TogglePause();
                while(game.TutorialLesson==VectorEncounter.Lesson.Practice)yield return null;
                yield return CapturePresentation(directory,"09-ready");game.TutorialNext();
                yield return CapturePresentation(directory,"10-normal-ability-guidance");
                foreach(string id in new[]{"sword","hammer","visor","plate","treads","faultline","ArcBolt","aug-Echo","aug-Crosscut","singularity"})
                {
                    game.Deck.Open(id=="singularity"?DraftKind.Ultimate:DraftKind.Mixed,false);
                    game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find(id));game.SelectOffer(0);yield return null;yield return null;
                    var def=game.Deck.Offers[0].def;
                    foreach(var region in dropRegions.Where(r=>r.offer<0&&!r.socket&&r.place!=PlaceKind.Inventory))
                    {
                        RewardTarget(0,region,out var place,out int index);
                        bool expected=def.kind==ItemKind.Augment?Host(region)?.Augmentable==true&&def.module.Fits(Host(region).def):game.Deck.CanClaimTo(0,place,index);
                        ReviewCheck(OfferFitsRegion(0,region)==expected,"normal drop guidance matches placement: "+id+"/"+region.place+"/"+region.index);
                    }
                    if(id=="aug-Crosscut")
                    {
                        game.SelectOffer(0);ReviewCheck(RewardHint.Contains("STORE FOR LATER"),"no-compatible-host instruction");yield return CapturePresentation(directory,"11-store-augment");
                        game.AcceptSelectedReward(false);ReviewCheck(game.Deck.RewardClaimed&&game.Deck.Inventory.Any(i=>i.def.id==id),"no compatible host: primary action stores augment");
                    }
                }
                game.Begin();var storedWeapon=game.Deck.Acquire(LoadoutCatalog.Find("sword"));game.Deck.Open(DraftKind.Mixed,false);game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("aug-Crosscut"));game.SelectOffer(0);
                yield return null;yield return null;SetManageBuild(true);yield return null;
                ReviewCheck(OfferFitsRegion(0,dropRegions.First(r=>r.item==storedWeapon&&!r.socket)),"augment highlights compatible stored host");
                dragItem=storedWeapon;ReviewCheck(FocusedReward()==-1&&StoredFitsRegion(storedWeapon,ReviewRegion(PlaceKind.Weapon,0)),"dragged inventory item overrides selected reward");dragItem=null;
                selectedItem=storedWeapon;ReviewCheck(FocusedReward()==-1,"selected inventory item overrides selected reward");ClearLoadoutGesture();
                game.Begin();game.Deck.Open(DraftKind.Mixed,false);
                string[] mixed={"visor","plate","treads"};for(int i=0;i<3;i++)game.Deck.Offers[i]=new RewardOffer(LoadoutCatalog.Find(mixed[i]),1,i+2);
                for(int resolution=0;resolution<3;resolution++)
                {
                    int width=resolution==0?1280:resolution==1?1920:2560;Screen.SetResolution(width,resolution==0?720:1080,false);
                    for(int frame=0;frame<12;frame++)yield return null;
                    reviewPointer=dropRegions.First(r=>r.offer==resolution).rect.center;yield return CapturePresentation(directory,"12-equipment-"+width);ReviewCheck(FocusedReward()==resolution,"equipment hover focuses exact reward at "+width);
                }
                game.MainMenu();
                yield return RunCheckpointClarityReview(directory);
                yield return RunSynergyReview(directory);
            }
            File.WriteAllText(Path.Combine(directory,"checks.txt"),string.Join("\n",tutorialChecks));
            Debug.Log("TUTORIAL_REVIEW_SUCCESS: "+tutorialChecks.Count+" assertions; "+(rendered?"native mouse handlers and captures":"model checks"));Application.Quit(0);
        }
    }
}
