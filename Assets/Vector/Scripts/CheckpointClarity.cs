using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        bool manageBuild;
        AbilityDeck clarityDeck;
        int clarityWave=-1;
        Vector2 hostScroll;
        Rect focusedSequence;
        bool FocusedCheckpoint=>!manageBuild&&!game.TutorialActive;

        void Draft(float w,float h)
        {
            if(displayedWave!=game.LastWaveFeedback){displayedWave=game.LastWaveFeedback;waveDetails=false;waveDetailsScroll=Vector2.zero;}
            if(clarityDeck!=game.Deck||clarityWave!=game.Wave)
            {clarityDeck=game.Deck;clarityWave=game.Wave;manageBuild=false;hostScroll=Vector2.zero;ClearLoadoutGesture();}
            if(FocusedCheckpoint)FocusedDraft(w,h);else ExpandedDraft(w,h);
        }
        void SetManageBuild(bool open)
        {manageBuild=open;ClearLoadoutGesture();hostScroll=Vector2.zero;}
        void CheckpointHeader(float w,bool expanded)
        {
            Label(new Rect(32,23,270,18),"WALKING HARD  /  COMBAT PAUSED",11,muted,true);
            string phase=game.OpeningDraft?"OPENING PICK "+(game.OpeningPicks+1)+" / 2":"CHECKPOINT "+game.Wave.ToString("00");
            Label(new Rect(32,47,600,30),expanded?phase+" / YOUR BUILD":phase,23,white,true);
            if(expanded&&game.LastWaveFeedback!=null&&Button(new Rect(w-397,30,171,31),"LAST WAVE DETAILS",waveDetails))
            {waveDetails=!waveDetails;detailItem=null;iconGuide=false;}
            if(Button(new Rect(w-210,27,178,39),expanded?"CLOSE BUILD":"MANAGE BUILD",expanded))
            {SetManageBuild(!expanded);GUIUtility.ExitGUI();}
        }
        static string ItemType(ItemDefinition def)=>def.kind==ItemKind.Basic||def.kind==ItemKind.Weapon?"WEAPON":LoadoutCatalog.IsArmor(def.kind)||def.kind==ItemKind.Rig?"GEAR":def.kind.ToString().ToUpperInvariant();
        static string ItemRole(ItemDefinition def)=>def.kind==ItemKind.Weapon?(def.hands==2?"Two hands / weapon attack":"One hand / weapon attack"):def.kind==ItemKind.Basic?"Uses your equipped weapon":def.kind==ItemKind.Ability?"Action in your sequence":def.kind==ItemKind.Augment?(def.module.family<0?"Modifies an ability":"Modifies a weapon"):def.kind==ItemKind.Ultimate?"Separate recharge timer":(def.kind==ItemKind.Rig?"Rig":def.kind.ToString())+" / passive effect";
        string RewardStats(ItemDefinition def,int rank)
        {
            if(def.kind==ItemKind.Augment)return "SCALES WITH HOST";
            if(def.kind==ItemKind.Ultimate){var item=new LoadoutItem(def,rank);return LoadoutCatalog.Action(item).damage.ToString("0.#")+" DMG / "+LoadoutCatalog.Cooldown(item).ToString("0")+"s RECHARGE";}
            string text=ItemStats(def,rank);
            if(def.kind==ItemKind.Weapon)return text.Substring(5);
            if(LoadoutCatalog.IsArmor(def.kind)||def.kind==ItemKind.Rig)
            {string signature=def.signature.ToString().ToUpperInvariant();if(text.EndsWith(signature))text=text.Substring(0,text.Length-signature.Length).Trim(' ','/');}
            return text.Length==0?"PASSIVE EFFECT":text;
        }
        void CategoryArt(Rect r,ItemDefinition def)
        {
            // Shape identifies type; the card outline and badge identify rarity.
            Color edge=muted;float cx=r.center.x,cy=r.center.y;
            if(def.kind==ItemKind.Ability)Ring(r.center,r.width*.49f,edge);
            else if(def.kind==ItemKind.Ultimate)
            {Stroke(new Vector2(cx,r.y),new Vector2(r.xMax,cy),edge);Stroke(new Vector2(r.xMax,cy),new Vector2(cx,r.yMax),edge);Stroke(new Vector2(cx,r.yMax),new Vector2(r.x,cy),edge);Stroke(new Vector2(r.x,cy),new Vector2(cx,r.y),edge);}
            else if(LoadoutCatalog.IsArmor(def.kind)||def.kind==ItemKind.Rig)
            {Stroke(new Vector2(r.x,r.y),new Vector2(r.xMax,r.y),edge);Stroke(new Vector2(r.x,r.y),new Vector2(r.x,cy+8),edge);Stroke(new Vector2(r.xMax,r.y),new Vector2(r.xMax,cy+8),edge);Stroke(new Vector2(r.x,cy+8),new Vector2(cx,r.yMax),edge);Stroke(new Vector2(r.xMax,cy+8),new Vector2(cx,r.yMax),edge);}
            else Outline(r,edge,def.kind==ItemKind.Augment?r.width*.28f:7);
            float inset=def.kind==ItemKind.Ultimate?r.width*.22f:r.width*.13f;
            ActionGlyph(new Rect(r.x+inset,r.y+inset,r.width-2*inset,r.height-2*inset),def,white);
        }
        void RewardCard(Rect r,int index,bool compact=false)
        {
            var d=game.Deck;var offer=d.Offers[index];var def=offer.def;bool selected=d.Selected==index;
            Color edge=def.kind==ItemKind.Augment?ModuleEdge:RarityColor(offer.rank);
            Plate(r,selected?new Color(.035f,.13f,.17f,.98f):ink,edge,9,selected?3:2);
            TypeIcon(new Rect(r.x+14,r.y+13,21,21),def.kind,white);
            Label(new Rect(r.x+43,r.y+13,r.width-150,24),ItemType(def),compact?15:18,white,true);
            if(def.kind!=ItemKind.Augment)
            {
                Rect badge=new Rect(r.xMax-101,r.y+12,87,23);Plate(badge,ink,RarityColor(offer.rank),4);
                Label(badge,ItemRarity.Names[offer.rank].ToUpperInvariant(),10,RarityColor(offer.rank),true,TextAnchor.MiddleCenter);
            }
            float size=compact?59:78,top=compact?45:54,left=r.x+size+30;
            CategoryArt(new Rect(r.x+14,r.y+top,size,size),def);
            Label(new Rect(left,r.y+top,r.xMax-left-14,compact?36:48),offer.Name,compact?17:22,white,true);
            Label(new Rect(left,r.y+top+(compact?38:52),r.xMax-left-14,19),ItemRole(def),10,muted);
            EffectLine(new Rect(r.x+14,r.y+(compact?123:145),r.width-28,compact?18:25),new LoadoutItem(def,offer.rank),compact?11:14);
            bool fusion=def.kind!=ItemKind.Augment&&offer.rank<5&&d.Copies(def,offer.rank)>0;
            string bottom=d.RewardClaimed?(selected?"TAKEN":"PASSED"):fusion?FusionSummary(def,offer.rank,offer.copies):RewardStats(def,offer.rank);
            Rect bottomRect=compact?new Rect(left,r.y+103,r.xMax-left-14,16):new Rect(r.x+14,r.yMax-26,r.width-28,19);
            Label(bottomRect,bottom,compact?10:11,fusion?RarityColor(offer.rank):muted,fusion);
            ItemTooltip(r,new LoadoutItem(def,offer.rank),offer);
        }
        string CheckpointActionLabel()
        {
            var d=game.Deck;if(d.RewardClaimed)return game.OpeningDraft&&game.OpeningPicks==0?"NEXT OPENING PICK":game.PendingDraft;
            if(d.Selected<0)return "CHOOSE A REWARD";
            var def=d.Offers[d.Selected].def;
            if(def.kind==ItemKind.Augment){var hosts=d.CompatibleHosts(def).ToArray();return hosts.Length==0?"STORE AUGMENT":hosts.Length>1?"CHOOSE AN ITEM":"ATTACH TO "+hosts[0].def.name.ToUpperInvariant();}
            d.SuggestedPlace(def,out var place,out int index);
            if(def.kind==ItemKind.Ability&&d.SelectedSlot>=0)place=PlaceKind.Sequence;
            return place==PlaceKind.Inventory?"STORE "+ItemType(def):def.kind==ItemKind.Ability?"ADD ABILITY":def.kind==ItemKind.Weapon?"EQUIP WEAPON":def.kind==ItemKind.Ultimate?"EQUIP ULTIMATE":"EQUIP "+def.kind.ToString().ToUpperInvariant();
        }
        int PreviewOffer()
        {
            var d=game.Deck;if(d.RewardClaimed||dragItem!=null||selectedItem!=null)return -1;
            if(dragOffer>=0)return dragOffer;
            var hover=dropRegions.FirstOrDefault(r=>r.offer>=0&&r.rect.Contains(Pointer));
            return hover?.offer??d.Selected;
        }
        void FocusedDraft(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.006f,.018f,.028f,.86f));var d=game.Deck;
            if(selectedItem!=null&&!d.Items.Contains(selectedItem))selectedItem=null;
            float cardWidth=(w-84)/3,cardY=164;
            dropRegions.Clear();focusedSequence=Rect.zero;
            if(!d.RewardClaimed)for(int i=0;i<3;i++)dropRegions.Add(new DropRegion{rect=new Rect(32+i*(cardWidth+10),cardY,cardWidth,210),offer=i});
            int preview=PreviewOffer();var pending=dragItem??selectedItem;
            var def=preview>=0?d.Offers[preview].def:pending?.def;
            Rect destination=new Rect(32,399,w-64,146);
            bool sequence=d.RewardClaimed||def?.kind==ItemKind.Ability||def==null&&d.Draft==DraftKind.Ability;
            if(sequence)
            {
                focusedSequence=new Rect(32,399,w-64,146);float sw=(destination.width-32-40)/5;
                for(int i=0;i<5;i++)dropRegions.Add(new DropRegion{rect=new Rect(48+i*(sw+10),440,sw,91),place=PlaceKind.Sequence,index=i,item=d.Slots[i]});
            }
            else if(def?.kind==ItemKind.Weapon)
            {for(int i=0;i<2;i++)dropRegions.Add(new DropRegion{rect=new Rect(48+i*((w-106)/2+10),440,(w-106)/2,91),place=PlaceKind.Weapon,index=i,item=d.Weapons[i]});}
            else if(def?.kind==ItemKind.Augment)
            {
                var hosts=d.CompatibleHosts(def).ToArray();int page=Mathf.Clamp((int)hostScroll.y,0,Mathf.Max(0,hosts.Length-4));
                for(int i=page;i<Mathf.Min(hosts.Length,page+4);i++)
                {
                    var host=hosts[i];int slot=Array.IndexOf(d.Slots,host),hand=Array.IndexOf(d.Weapons,host);
                    if(host==d.Fists)hand=Array.FindIndex(d.Weapons,x=>x==null);
                    dropRegions.Add(new DropRegion{rect=new Rect(48+(i-page)*((w-126)/4+10),440,(w-126)/4,91),place=slot>=0?PlaceKind.Sequence:hand>=0?PlaceKind.Weapon:PlaceKind.Inventory,index=slot>=0?slot:hand>=0?hand:0,item=slot>=0?d.Slots[slot]:host});
                }
            }
            else if(def!=null)
            {
                d.SuggestedPlace(def,out var place,out int index);var current=place==PlaceKind.Rig?d.Rig:place==PlaceKind.Ultimate?d.Ultimate:d.Armor[Mathf.Clamp(LoadoutCatalog.ArmorIndex(def.kind),0,2)];
                dropRegions.Add(new DropRegion{rect=new Rect(48,440,w-96,91),place=place,index=index,item=current});
            }
            bool store=!d.RewardClaimed&&d.Draft!=DraftKind.Ability||pending!=null;
            Rect storageButton=new Rect(w-512,h-57,184,42);
            if(store)dropRegions.Add(new DropRegion{rect=storageButton,place=PlaceKind.Inventory,index=-1});
            AddSocketRegions();if(!iconGuide&&!detailHovered&&!waveDetails)HandleLoadoutInput();
            CheckpointHeader(w,false);
            Label(new Rect(32,99,w-400,37),d.RewardClaimed?"Ready when you are.":"Choose 1 "+(d.Draft==DraftKind.Ability?"ability":d.Draft==DraftKind.Weapon?"weapon":d.Draft==DraftKind.Augment?"augment":d.Draft==DraftKind.Ultimate?"ultimate":"reward")+".",29,white,true);
            Label(new Rect(32,139,w-64,20),d.RewardClaimed?"Manage your build, or continue.":"Pick a card. Its matching slots light up.",13,muted);
            Label(new Rect(w-340,108,308,22),"1 PICK  /  2 EQUIP  /  3 CONTINUE",11,muted,true,TextAnchor.UpperRight);
            if(!d.RewardClaimed)for(int i=0;i<3;i++)RewardCard(new Rect(32+i*(cardWidth+10),cardY,cardWidth,210),i);
            else ClaimedCard(new Rect(32,cardY,w-64,210));
            Panel(destination);
            string heading=sequence?"ATTACK SEQUENCE":def==null?"CHOOSE A REWARD TO SEE WHERE IT GOES":def.kind==ItemKind.Weapon?(def.hands==2?"BOTH HANDS / ONE LOCKED ATTACK":"WEAPON HANDS"):def.kind==ItemKind.Augment?"ATTACH TO A COMPATIBLE ITEM":DestinationName(def);
            Label(new Rect(48,413,w-96,20),heading,13,def==null&&!sequence?muted:Accent,true);
            if(sequence)TargetWaitCheckpoint(w,413);
            foreach(var r in dropRegions.Where(r=>r.offer<0&&!r.socket&&r.index!=-1))
            {
                if(r.place==PlaceKind.Sequence)ItemPanel(r.rect,r.item,false,"Empty ability slot",(r.index+1).ToString("00")+" / "+(r.item?.def.kind==ItemKind.Basic?"WEAPON ATTACK":"ABILITY"));
                else
                {
                    var shown=r.place==PlaceKind.Weapon?d.WeaponFor(d.Basics[r.index]):r.item;
                    string label=r.place==PlaceKind.Weapon?"HAND "+(r.index+1):r.place==PlaceKind.Inventory?"STORED ITEM":r.place.ToString().ToUpperInvariant();
                    ItemPanel(r.rect,shown,false,"Empty",label,def?.kind??ItemKind.Ability);
                }
            }
            if(sequence)GroupOutline(48,440,(destination.width-72)/5,10,91);
            if(def?.kind==ItemKind.Augment)
            {
                int count=d.CompatibleHosts(def).Count();if(count==0)Label(new Rect(48,455,w-96,50),"No compatible item yet. Store this augment for later.",16,muted);
                if(count>4)
                {if(Button(new Rect(w-236,407,88,25),"PREVIOUS"))hostScroll.y=Mathf.Max(0,hostScroll.y-4);if(Button(new Rect(w-140,407,92,25),"MORE ITEMS"))hostScroll.y=Mathf.Min(count-4,hostScroll.y+4);}
            }
            var hoveredTarget=dropRegions.FirstOrDefault(r=>r.offer<0&&!r.socket&&r.rect.Contains(Pointer));
            string hint=def==null?"Full details on hover. Your character sheet is in Manage build.":DestinationPreview(def,hoveredTarget,preview);
            Label(new Rect(32,555,w-64,38),hint,12,white);
            BuildFitLine(new Rect(32,596,w-64,20));
            if(!string.IsNullOrEmpty(loadoutFeedback))Label(new Rect(32,139,w-64,20),loadoutFeedback,12,Strong,true);
            if(store)
            {
                GUI.enabled=pending!=null?d.CanMove(pending,PlaceKind.Inventory):d.CanClaimTo(dragOffer>=0?dragOffer:d.Selected,PlaceKind.Inventory);
                // The shared mouse handler owns clicks and drops on this button.
                Button(storageButton,pending!=null?"MOVE TO STORAGE":"ADD TO STORAGE");GUI.enabled=true;
            }
            GUI.enabled=!d.RewardClaimed&&d.Rerolls>0;
            if(Button(new Rect(32,h-99,135,30),"REROLL "+d.Rerolls+" [ TAB ]")){game.Reroll();ClearLoadoutGesture();GUIUtility.ExitGUI();}GUI.enabled=true;
            if(Button(new Rect(32,h-52,208,32),"EDIT AFTER PICK: "+(editAfterPick?"ON":"OFF"),editAfterPick))editAfterPick=!editAfterPick;
            Label(new Rect(260,h-47,w-(store?800:600),26),d.RewardClaimed?"Open Manage build to change gear or order.":editAfterPick?"Take a reward, then tinker before continuing.":"Taking a reward advances to the next pick or fight.",11,muted);
            bool many=d.Selected>=0&&!d.RewardClaimed&&d.Offers[d.Selected].def.kind==ItemKind.Augment&&d.CompatibleHosts(d.Offers[d.Selected].def).Count()>1;
            GUI.enabled=(d.RewardClaimed||d.Selected>=0)&&!many;
            if(Button(new Rect(w-316,h-57,284,42),CheckpointActionLabel(),true)){if(d.RewardClaimed)game.ConfirmDraft();else game.AcceptSelectedReward(!editAfterPick);ClearLoadoutGesture();GUIUtility.ExitGUI();}GUI.enabled=true;
            if(game.FusionHistory.Count>0)FusionHistoryButton(new Rect(w-260,h-99,228,30));
            Label(new Rect(208,h-92,w-493,21),"NEXT: "+game.EncounterPreview.Split('·')[0].Trim(),11,muted);
            DrawSockets();ApplyPresentationReviewHover();HighlightFusionMatches();DrawDestinationGuidance(w,440);HighlightBuildPartner();DrawDragPreview();
        }
        string DestinationPreview(ItemDefinition def,DropRegion target,int offer)
        {
            var d=game.Deck;
            if(target!=null&&target.index==-1)return "Store this item for later. Equipped items stay in place.";
            if(def.kind==ItemKind.Augment)
            {var host=target==null?null:Host(target);return host!=null&&def.module.Fits(host.def)?host.def.name+": "+(host.augment?.def.name??"Empty socket")+" → "+def.name+". "+(host.augment!=null?"Previous augment goes to storage.":"Host keeps its slot."):"Choose an equipped or stored item. One augment per item.";}
            if(def.kind==ItemKind.Weapon&&def.hands==2)return "Replaces both hands. Weapon attacks lock together; displaced weapons go to storage.";
            if(target==null)
            {
                d.SuggestedPlace(def,out var place,out int index);if(def.kind==ItemKind.Ability&&d.SelectedSlot>=0){place=PlaceKind.Sequence;index=d.SelectedSlot;}
                if(place==PlaceKind.Inventory)return "Sequence full: choose an action to replace, or store this ability.";
                target=dropRegions.FirstOrDefault(r=>r.offer<0&&!r.socket&&r.place==place&&r.index==index);
            }
            if(target==null)return "Choose a glowing slot, or open Manage build.";
            if(offer>=0&&!OfferFitsRegion(offer,target))return "That slot does not fit. Choose a glowing slot.";
            var current=Host(target);
            if(current==null)return "Fills an empty "+(target.place==PlaceKind.Sequence?"ability":target.place.ToString().ToLowerInvariant())+" slot.";
            if(current.def==def&&offer>=0&&d.Copies(def,d.Offers[offer].rank)+d.Offers[offer].copies>=3)return "Three matching copies fuse automatically. The equipped item gains a rarity.";
            if(target.item?.def.kind==ItemKind.Basic&&def.kind==ItemKind.Ability)return "This weapon attack moves to another position. An ability may move to storage if the sequence is full.";
            return current.def.name+" → "+def.name+". "+(current==d.Fists?"Empty hands still use fists.":"Replaced item goes to storage.");
        }
        void ClaimedCard(Rect r)
        {
            var d=game.Deck;Panel(r);if(d.Selected<0)return;var offer=d.Offers[d.Selected];
            CategoryArt(new Rect(r.x+24,r.y+48,100,100),offer.def);
            Label(new Rect(r.x+151,r.y+28,r.width-175,22),"REWARD TAKEN / "+ItemType(offer.def),13,Accent,true);
            Label(new Rect(r.x+151,r.y+65,r.width-175,37),offer.def.name,27,white,true);
            EffectLine(new Rect(r.x+151,r.y+113,r.width-175,26),new LoadoutItem(offer.def,offer.rank),16);
            Label(new Rect(r.x+151,r.y+154,r.width-175,25),"Your choice is saved. Open Manage build to equip, store or reorder.",13,muted);
        }
    }
}
