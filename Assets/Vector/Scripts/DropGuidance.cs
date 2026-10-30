using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        // One interaction color for every category. Item borders alone encode rarity.
        void DestinationGlow(Rect r,Color ignored,bool primary=true)
        {
            Color color=Accent;float pulse=game.ReducedMotion?1:.78f+.22f*Mathf.Sin(Time.unscaledTime*3.5f);
            for(int n=3;n>=1;n--){var outer=new Rect(r.x-n*2,r.y-n*2,r.width+n*4,r.height+n*4);Outline(outer,new Color(color.r,color.g,color.b,.06f*pulse),8,2);}
            Outline(r,new Color(color.r,color.g,color.b,(primary?.95f:.45f)*pulse),8,primary?2.5f:1);
        }
        string RewardHeading=>game.Deck.Draft==DraftKind.Ability?"ABILITY / SEQUENCE SLOT":game.Deck.Draft==DraftKind.Weapon?"WEAPON / HAND SLOT":game.Deck.Draft==DraftKind.Equipment?"EQUIPMENT / MATCHING SLOT":game.Deck.Draft==DraftKind.Augment?"AUGMENT / + SOCKET":game.Deck.Draft==DraftKind.Ultimate?"ULTIMATE / SEPARATE SLOT":"REWARD OPTIONS";
        int FocusedReward()
        {
            if(FocusedCheckpoint)return PreviewOffer();
            if(game.Deck.RewardClaimed)return -1;
            if(dragItem!=null)return -1;
            if(dragOffer>=0)return dragOffer;
            if(presentationOffer!=null){int p=Array.IndexOf(game.Deck.Offers,presentationOffer);if(p>=0)return p;}
            var hover=dropRegions.FirstOrDefault(r=>r.rect.Contains(Pointer));
            if(hover!=null){if(hover.offer>=0)return hover.offer;if(hover.item!=null)return -1;}
            return selectedItem==null?game.Deck.Selected:-1;
        }
        bool OfferFitsRegion(int offer,DropRegion r)
        {
            if(r.offer>=0||!game.TutorialOfferAllowed(offer))return false;
            var def=game.Deck.Offers[offer].def;
            if(def.kind==ItemKind.Augment&&Host(r) is LoadoutItem host&&host.Augmentable)return game.TutorialAttachAllowed(host)&&def.module.Fits(host.def);
            if(r.place==PlaceKind.Inventory&&!r.socket)return r.index==-1&&game.Deck.CanClaimTo(offer,PlaceKind.Inventory);
            if(r.socket)return false;
            RewardTarget(offer,r,out var place,out int index);return game.Deck.CanClaimTo(offer,place,index);
        }
        bool StoredFitsRegion(LoadoutItem item,DropRegion r)
        {
            if(r.offer>=0||r.item==item)return false;
            if(item.def.kind==ItemKind.Augment&&Host(r) is LoadoutItem host&&host.Augmentable)return game.TutorialAttachAllowed(host)&&item.def.module.Fits(host.def);
            if(!game.TutorialMoveAllowed(item,r.place,r.index))return false;
            if(r.place==PlaceKind.Inventory&&!r.socket)return r.index==-1&&!game.Deck.Inventory.Contains(item)&&game.Deck.CanMove(item,PlaceKind.Inventory);
            return !r.socket&&game.Deck.CanMove(item,r.place,r.index);
        }
        string DestinationName(ItemDefinition def)=>def.kind==ItemKind.Ability?"SEQUENCE":def.kind==ItemKind.Weapon?(def.hands==2?"BOTH HANDS":"HAND SLOT"):def.kind==ItemKind.Augment?"COMPATIBLE + SOCKET":def.kind==ItemKind.Rig?"RIG SLOT":def.kind==ItemKind.Ultimate?"ULTIMATE SLOT":def.kind.ToString().ToUpperInvariant()+" SLOT";
        string RewardHint
        {
            get
            {
                int offer=FocusedReward();
                if(offer>=0){var def=game.Deck.Offers[offer].def;if(def.kind==ItemKind.Augment&&!game.Deck.CompatibleHosts(def).Any())return "NO COMPATIBLE HOST / STORE FOR LATER";return Category(def)+"  >  "+DestinationName(def);}
                return game.Deck.RewardClaimed?"REWARD TAKEN / ADJUST YOUR BUILD":"HOVER A CARD TO SEE ITS SLOTS";
            }
        }
        void DrawDestinationGuidance(float w,float sequenceY)
        {
            if(iconGuide||waveDetails)return;
            int offer=FocusedReward();var item=dragItem??(hoveredOffer==null&&game.Deck.Items.Contains(hoveredItem)?hoveredItem:null)??selectedItem;
            bool categoryPreview=offer<0&&item==null&&!game.Deck.RewardClaimed&&game.Deck.Draft!=DraftKind.Mixed;
            if(offer<0&&item==null&&!categoryPreview){if(game.TutorialActive)TutorialHighlights();return;}
            var def=offer>=0?game.Deck.Offers[offer].def:item?.def;
            bool ability=def?.kind==ItemKind.Ability||(categoryPreview&&game.Deck.Draft==DraftKind.Ability);
            if(ability)DestinationGlow(FocusedCheckpoint?focusedSequence:new Rect(20,sequenceY-24,w-290,124),Accent,false);
            foreach(var r in dropRegions)
            {
                bool valid=offer>=0?OfferFitsRegion(offer,r):item!=null?StoredFitsRegion(item,r):Enumerable.Range(0,game.Deck.Offers.Length).Any(i=>OfferFitsRegion(i,r));
                if(!valid)continue;
                bool storage=r.place==PlaceKind.Inventory&&!r.socket;
                // Storage is secondary; never light every occupied inventory cell.
                if(storage&&categoryPreview)continue;
                DestinationGlow(r.rect,Accent,!storage);
                bool module=def?.kind==ItemKind.Augment||(categoryPreview&&game.Deck.Draft==DraftKind.Augment);
                if(module&&!storage&&!r.socket&&Host(r)?.Augmentable==true)DestinationGlow(SocketRect(r),Accent);
            }
            if(offer>=0){var source=dropRegions.FirstOrDefault(r=>r.offer==offer);if(source!=null)DestinationGlow(new Rect(source.rect.x-3,source.rect.y-3,source.rect.width+6,source.rect.height+6),Accent,false);}
            if(game.TutorialActive)TutorialHighlights();
        }
    }
}
