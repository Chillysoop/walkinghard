using System.Collections;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        IEnumerator RunCheckpointClarityReview(string directory)
        {
            Screen.SetResolution(1280,720,false);game.Begin();game.HoldCheckpointAfterPick=true;
            yield return null;yield return null;
            ReviewCheck(FocusedCheckpoint,"ordinary checkpoints start focused");
            ReviewCheck(dropRegions.All(r=>r.offer>=0||r.place==PlaceKind.Sequence),"ability opening hides unrelated gear and storage");
            ReviewCheck(CheckpointActionLabel()=="CHOOSE A REWARD","empty selection explains required action");
            yield return CapturePresentation(directory,"20-focused-opening");
            foreach(string id in new[]{"sword","hammer","visor","plate","treads","faultline","ArcBolt","singularity"})
            {
                game.Deck.Open(id=="singularity"?DraftKind.Ultimate:DraftKind.Mixed,false);
                game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find(id));game.SelectOffer(0);ClearLoadoutGesture();
                yield return null;yield return null;
                var destinations=dropRegions.Where(r=>r.offer<0&&!r.socket&&r.index!=-1).ToArray();
                ReviewCheck(destinations.Length>0,"focused destination exists: "+id);
                ReviewCheck(destinations.Any(r=>OfferFitsRegion(0,r)),"focused compatible destination: "+id);
                ReviewCheck(CheckpointActionLabel()!="AUTO-EQUIP [ ENTER ]","concrete action label: "+id);
                yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,destinations.First(r=>OfferFitsRegion(0,r)).rect.center);
                ReviewCheck(game.Deck.RewardClaimed&&game.State==VectorEncounter.RunState.Drafting,"focused drop equips and honors edit-after-pick: "+id);
            }
            SetManageBuild(true);yield return null;yield return null;
            ReviewCheck(dropRegions.Any(r=>r.place==PlaceKind.Inventory&&r.index==-1)&&dropRegions.Count(r=>r.place==PlaceKind.Sequence&&!r.socket)==5,"Manage build exposes storage and all five actions");
            yield return CapturePresentation(directory,"21-manage-build");SetManageBuild(false);yield return null;
            game.Begin();game.HoldCheckpointAfterPick=true;
            var stored=game.Deck.Acquire(LoadoutCatalog.Find("sword"));game.Deck.Open(DraftKind.Augment,false);
            game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("aug-Crosscut"));game.SelectOffer(0);yield return null;yield return null;
            ReviewCheck(dropRegions.Any(r=>r.item==stored&&OfferFitsRegion(0,r)),"stored compatible host appears in focused augment view");
            ReviewCheck(CheckpointActionLabel()=="ATTACH TO EDGE SWORD","stored host is named in primary action");
            game.AcceptSelectedReward(false);ReviewCheck(stored.augment?.def.id=="aug-Crosscut"&&game.Deck.RewardClaimed,"primary action attaches augment to sole stored host");
            game.Deck.Open(DraftKind.Augment,false);game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("aug-Crosscut"));
            var second=game.Deck.Acquire(LoadoutCatalog.Find("sword"),2);game.SelectOffer(0);yield return null;yield return null;
            game.AcceptSelectedReward(false);ReviewCheck(!game.Deck.RewardClaimed&&CheckpointActionLabel()=="CHOOSE AN ITEM","multiple hosts require an explicit destination");
            yield return CapturePresentation(directory,"22-augment-hosts");
            yield return ReviewDrag(dropRegions.First(r=>r.offer==0).rect.center,dropRegions.First(r=>r.item==second&&!r.socket).rect.center);
            ReviewCheck(second.augment!=null,"focused augment drop targets exact stored copy");
            var replacement=game.Deck.Acquire(LoadoutCatalog.Find("aug-HookedEdge"));game.Deck.Move(second,PlaceKind.Weapon,0);
            game.Deck.Move(stored,PlaceKind.Inventory);game.Deck.Items.Remove(stored);
            ReviewCheck(game.AutoEquip(replacement)&&second.augment==replacement,"owned augment auto-equips with one host");
            game.Begin();game.HoldCheckpointAfterPick=true;
            var pendingWeapon=game.Deck.Acquire(LoadoutCatalog.Find("sword"));game.Deck.Open(DraftKind.Mixed,false);
            game.Deck.Offers[0]=new RewardOffer(LoadoutCatalog.Find("plate"));game.SelectOffer(0);yield return null;yield return null;
            SetManageBuild(true);yield return null;yield return null;
            var storedCenter=dropRegions.First(r=>r.place==PlaceKind.Inventory&&r.item==pendingWeapon&&!r.socket).rect.center;
            yield return ReviewMouse(EventType.MouseDown,storedCenter);yield return ReviewMouse(EventType.MouseUp,storedCenter);
            ReviewCheck(selectedItem==pendingWeapon&&!game.Deck.RewardClaimed,"stored item click preserves pending reward");
            var handCenter=ReviewRegion(PlaceKind.Weapon,0).rect.center;
            yield return ReviewMouse(EventType.MouseDown,handCenter);yield return ReviewMouse(EventType.MouseUp,handCenter);
            ReviewCheck(game.Deck.Weapons[0]==pendingWeapon&&!game.Deck.RewardClaimed,"click equip stored weapon preserves pending reward");
            game.Begin();game.HoldCheckpointAfterPick=true;game.Deck.Open(DraftKind.Mixed,false);
            string[] mixed={"sword","GravityWell","plate"};for(int i=0;i<3;i++)game.Deck.Offers[i]=new RewardOffer(LoadoutCatalog.Find(mixed[i]),1,i+2);
            for(int size=0;size<3;size++)
            {
                Screen.SetResolution(size==0?1280:size==1?1920:2560,size==0?720:1080,false);for(int frame=0;frame<12;frame++)yield return null;
                game.SelectOffer(size);yield return null;yield return null;
                yield return CapturePresentation(directory,"23-focused-mixed-"+Screen.width);
                ReviewCheck(dropRegions.Where(r=>r.offer>=0).All(r=>r.rect.yMax<=399),"reward cards do not overlap destinations at "+Screen.width);
            }
            game.MainMenu();
        }
    }
}
