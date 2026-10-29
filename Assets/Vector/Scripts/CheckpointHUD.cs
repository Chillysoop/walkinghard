using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        sealed class DropRegion {public Rect rect;public LoadoutItem item;public PlaceKind place;public int index,offer=-1;}
        readonly List<DropRegion> dropRegions=new List<DropRegion>();
        Vector2 inventoryScroll,dragStart;
        LoadoutItem selectedItem,dragItem,lastClickItem;
        RewardOffer[] lastClickOffers;
        int dragOffer=-1,lastClickOffer=-1;
        bool dragging;
        bool editAfterPick {get=>game.HoldCheckpointAfterPick;set=>game.HoldCheckpointAfterPick=value;}
        float lastClickTime=-10;
        string loadoutFeedback="";
        void Draft(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.006f,.018f,.028f,.73f));
            var deck=game.Deck;if(selectedItem!=null&&!deck.Items.Contains(selectedItem))selectedItem=null;
            float full=w-40,rewardW=(full-44)/3,sequenceY=h-143,sequenceW=(w-302-40)/5;
            Rect stats=new Rect(20,244,266,h-402),center=new Rect(302,244,(w-334)*.5f,h-402);
            Rect storage=new Rect(center.xMax+16,244,w-center.xMax-36,h-402);
            Rect inventoryRect=new Rect(storage.x+13,storage.y+40,storage.width-26,storage.height-67);
            const int columns=6;float cellW=(inventoryRect.width-5*6-15)/6,cellH=Mathf.Min(cellW,(inventoryRect.height-18)/4);
            var inventory=deck.Inventory.ToArray();int rows=Mathf.Max(4,Mathf.CeilToInt(inventory.Length/6f));
            Rect[] mounts={new Rect(center.x+10,center.y+3,98,85),new Rect(center.xMax-108,center.y+3,98,85),new Rect(center.x,center.y+105,98,85),new Rect(center.xMax-98,center.y+105,98,85),new Rect(center.xMax-108,center.yMax-92,98,85)};
            Rect rigRect=new Rect(stats.x+12,stats.yMax-64,stats.width-24,55),ultRect=new Rect(w-264,sequenceY,244,91);
            inventoryScroll.y=Mathf.Clamp(Mathf.Round(inventoryScroll.y),0,Mathf.Max(0,rows-4));
            if(Event.current.type==EventType.ScrollWheel&&inventoryRect.Contains(Event.current.mousePosition))
            {inventoryScroll.y=Mathf.Clamp(inventoryScroll.y+Mathf.Sign(Event.current.delta.y),0,Mathf.Max(0,rows-4));Event.current.Use();}
            int firstStored=(int)inventoryScroll.y*columns;
            dropRegions.Clear();
            for(int i=0;i<3;i++)if(!deck.RewardClaimed)dropRegions.Add(new DropRegion{rect=new Rect(32+i*(rewardW+10),104,rewardW,116),offer=i});
            for(int i=0;i<5;i++)dropRegions.Add(new DropRegion{rect=new Rect(30+i*(sequenceW+10),sequenceY,sequenceW,91),place=PlaceKind.Sequence,index=i,item=deck.Slots[i]});
            dropRegions.Add(new DropRegion{rect=mounts[0],place=PlaceKind.Helmet,item=deck.Armor[0]});
            dropRegions.Add(new DropRegion{rect=mounts[1],place=PlaceKind.Armor,item=deck.Armor[1]});
            dropRegions.Add(new DropRegion{rect=mounts[4],place=PlaceKind.Boots,item=deck.Armor[2]});
            for(int i=0;i<2;i++)dropRegions.Add(new DropRegion{rect=mounts[2+i],place=PlaceKind.Weapon,index=i,item=deck.Weapons[i]});
            dropRegions.Add(new DropRegion{rect=rigRect,place=PlaceKind.Rig,item=deck.Rig});
            dropRegions.Add(new DropRegion{rect=ultRect,place=PlaceKind.Ultimate,item=deck.Ultimate});
            dropRegions.Add(new DropRegion{rect=inventoryRect,place=PlaceKind.Inventory,index=-1});
            for(int i=firstStored;i<Mathf.Min(inventory.Length,firstStored+24);i++)
            {
                int cell=i-firstStored;var r=new Rect(inventoryRect.x+(cell%columns)*(cellW+6),inventoryRect.y+(cell/columns)*(cellH+6),cellW,cellH);
                float top=Mathf.Max(r.y,inventoryRect.y),bottom=Mathf.Min(r.yMax,inventoryRect.yMax);
                if(bottom>top)dropRegions.Insert(dropRegions.Count-1,new DropRegion{rect=new Rect(r.x,top,r.width,bottom-top),place=PlaceKind.Inventory,item=inventory[i]});
            }
            HandleLoadoutInput();
            Panel(new Rect(20,18,w-40,58));TypeIcon(new Rect(34,31,32,32),ItemKind.Rig,muted);
            Label(new Rect(76,33,165,24),"WALKING HARD",17,white,true);
            Label(new Rect(250,25,260,29),game.OpeningDraft?"OPENING DRAFT":"CHECKPOINT "+game.Wave,25,white,true);
            Label(new Rect(520,30,230,22),"COMBAT PAUSED",15,Accent,true);
            Label(new Rect(251,56,610,18),"Drag rewards onto your loadout or into storage. Hover any item for full details.",11,muted);
            Label(new Rect(w-290,31,250,25),(deck.RewardClaimed?"CHOICE CLAIMED":"CHOOSE 1 OF 3")+(game.OpeningDraft?" / "+(game.OpeningPicks+1)+" OF 2":""),16,deck.RewardClaimed?Accent:white,true,TextAnchor.UpperRight);
            Panel(new Rect(20,86,w-40,146));Label(new Rect(33,86,500,17),deck.Draft==DraftKind.Weapon?"WEAPON REWARDS":"REWARDS",11,Accent,true);
            for(int i=0;i<3;i++)
            {
                var offer=deck.Offers[i];Rect r=new Rect(32+i*(rewardW+10),104,rewardW,116);bool selected=deck.Selected==i;Color color=RarityColor(offer.rank);
                Plate(r,selected?new Color(.035f,.13f,.17f,.98f):ink,color,10,selected?3:1.5f);
                Plate(new Rect(r.x+14,r.y+15,76,76),new Color(.02f,.06f,.09f),line,7);
                ActionGlyph(new Rect(r.x+21,r.y+22,62,62),offer.def,TypeColor(offer.def.kind));
                Label(new Rect(r.x+103,r.y+11,r.width-115,15),OfferType(offer)+(deck.RewardClaimed?(selected?" / CLAIMED":" / PASSED"):""),10,TypeColor(offer.def.kind),true);
                Label(new Rect(r.x+103,r.y+31,r.width-115,27),offer.Name,20,white,true);
                Label(new Rect(r.x+103,r.y+62,r.width-115,27),ItemStats(offer.def,offer.rank),11,muted,true);
                Label(new Rect(r.x+14,r.y+96,r.width-28,15),ItemRarity.Names[offer.rank].ToUpperInvariant()+" / "+UpgradePreview(offer),9,color,true);
                ItemTooltip(r,new LoadoutItem(offer.def,offer.rank));
            }
            CharacterStats(stats);
            Plate(center,new Color(.01f,.03f,.045f,.96f),new Color(.12f,.28f,.34f,.25f),12);
            DrawPortrait(new Rect(center.x+72,center.y-14,center.width-144,center.height+18));
            ConnectMount(mounts[0],new Vector2(center.center.x-15,center.y+62));
            ConnectMount(mounts[1],new Vector2(center.center.x+27,center.y+121));
            ConnectMount(mounts[2],new Vector2(center.center.x-49,center.y+185));
            ConnectMount(mounts[3],new Vector2(center.center.x+49,center.y+185));
            ConnectMount(mounts[4],new Vector2(center.center.x+15,center.yMax-42));
            EquipmentMount(mounts[0],deck.Armor[0],ItemKind.Helmet,"HELMET");
            EquipmentMount(mounts[1],deck.Armor[1],ItemKind.Armor,"ARMOR");
            EquipmentMount(mounts[4],deck.Armor[2],ItemKind.Boots,"BOOTS");
            for(int i=0;i<2;i++)EquipmentMount(mounts[2+i],deck.Weapons[i]??deck.Fists,ItemKind.Weapon,"WEAPON "+(i+1)+(deck.TwoHanded?" / 2H":""));
            if(deck.TwoHanded)Label(new Rect(center.x+112,center.yMax-28,center.width-224,22),"2H / BOTH MOUNTS",11,Gold,true,TextAnchor.MiddleCenter);
            ItemPanel(rigRect,deck.Rig,false,"No rig equipped","UTILITY RIG",ItemKind.Rig);
            Panel(storage);Label(new Rect(storage.x+16,storage.y+12,storage.width-32,21),"STORAGE",16,white,true);
            Label(new Rect(storage.x+16,storage.y+15,storage.width-32,20),inventory.Length+" ITEMS",11,muted,true,TextAnchor.UpperRight);
            for(int i=0;i<24;i++)
            {
                Rect r=new Rect(inventoryRect.x+(i%columns)*(cellW+6),inventoryRect.y+(i/columns)*(cellH+6),cellW,cellH);int stored=firstStored+i;var item=stored<inventory.Length?inventory[stored]:null;
                Plate(r,new Color(.015f,.045f,.065f,.82f),item==null?new Color(.17f,.31f,.39f):RarityColor(item.rank),6,item==selectedItem&&item!=null?2:1);
                if(item!=null){float icon=Mathf.Min(cellW,cellH)-15;ActionGlyph(new Rect(r.center.x-icon/2,r.center.y-icon/2,icon,icon),item.def,TypeColor(item.def.kind));ItemTooltip(r,item);}
                else {float inset=8;Stroke(new Vector2(r.x+inset,r.y+inset),new Vector2(r.x+inset+5,r.y+inset),line,1);Stroke(new Vector2(r.x+inset,r.y+inset),new Vector2(r.x+inset,r.y+inset+5),line,1);}
            }
            if(rows>4)inventoryScroll.y=Mathf.Round(GUI.VerticalScrollbar(new Rect(inventoryRect.xMax-12,inventoryRect.y,12,inventoryRect.height),inventoryScroll.y,4,0,rows));
            Label(new Rect(storage.x+15,storage.yMax-22,storage.width-30,17),"DROP HERE TO STORE / DOUBLE-CLICK TO EQUIP",10,muted,false,TextAnchor.MiddleCenter);
            Panel(new Rect(20,sequenceY-24,w-40,124));Label(new Rect(32,sequenceY-22,w-64,18),"ACTION SEQUENCE / DRAG TO REORDER",11,muted,true);
            for(int i=0;i<5;i++)
            {
                Rect r=new Rect(30+i*(sequenceW+10),sequenceY,sequenceW,91);var item=deck.Slots[i];
                ItemPanel(r,item,deck.SelectedSlot==i,deck.SlotName(i),(i+1).ToString("00")+" / "+(item?.def.kind==ItemKind.Basic?"BASIC "+(item.mount+1):"ABILITY"));
                if(i<4)Stroke(new Vector2(r.xMax,r.center.y),new Vector2(r.xMax+10,r.center.y),deck.PairAfter(i)>=0?Gold:line,2);
            }
            GroupOutline(30,sequenceY,sequenceW,10,91);
            ItemPanel(ultRect,deck.Ultimate,false,"No ultimate equipped","ULTIMATE / "+game.UltimateCooldown+"s / "+(game.UltimateAutomatic?"AUTO":"MANUAL"),ItemKind.Ultimate);
            GUI.enabled=!deck.RewardClaimed&&deck.Rerolls>0;
            if(Button(new Rect(20,h-34,151,25),"REROLL "+deck.Rerolls+" [ TAB ]")){game.Reroll();ClearLoadoutGesture();GUIUtility.ExitGUI();}GUI.enabled=true;
            if(Button(new Rect(180,h-34,210,25),"EDIT AFTER PICK: "+(editAfterPick?"ON":"OFF"),editAfterPick))editAfterPick=!editAfterPick;
            FusionHistoryButton(new Rect(400,h-34,235,25));
            if(game.FusionHistory.Count==0)Label(new Rect(402,h-30,230,20),"Replaced gear moves to storage.",11,muted);
            GUI.enabled=deck.RewardClaimed||deck.Selected>=0;
            string next=!deck.RewardClaimed?"AUTO-EQUIP [ ENTER ]":game.OpeningDraft&&game.OpeningPicks==0?"NEXT OPENING PICK":!game.OpeningDraft&&game.Wave==1&&deck.Draft==DraftKind.Ability?"NEXT: WEAPON PICK":"LET'S FIGHT";
            if(Button(new Rect(w-274,h-35,254,27),next,true)){if(deck.RewardClaimed)game.ConfirmDraft();else game.AcceptSelectedReward(!editAfterPick);ClearLoadoutGesture();GUIUtility.ExitGUI();}GUI.enabled=true;
            if(!string.IsNullOrEmpty(loadoutFeedback))Label(new Rect(650,h-33,w-940,27),loadoutFeedback,10,Gold);
            DrawDragPreview();
        }
        void CharacterStats(Rect r)
        {
            var d=game.Deck;Panel(r);TypeIcon(new Rect(r.x+14,r.y+13,22,22),ItemKind.Rig,muted);
            Label(new Rect(r.x+44,r.y+13,r.width-58,24),"THE WALKER",20,white,true);
            Stroke(new Vector2(r.x+16,r.y+48),new Vector2(r.xMax-16,r.y+48),line,1);
            Label(new Rect(r.x+16,r.y+60,r.width-32,22),"INTEGRITY",12,muted,true);
            Label(new Rect(r.x+16,r.y+58,r.width-32,22),Mathf.CeilToInt(game.Health)+" / "+game.MaxHealth.ToString("0"),18,white,true,TextAnchor.UpperRight);
            Segments(new Rect(r.x+16,r.y+88,r.width-32,10),game.Health/game.MaxHealth,Accent,12);
            string[] names={"DIRECT DAMAGE","DAMAGE RESIST","ACTION SPEED","REGEN / SECOND"};
            string[] values={"+"+((d.Damage-1)*100).ToString("0.#")+"%",((1-d.DamageTaken)*100).ToString("0.#")+"%","+"+((d.Speed-1)*100).ToString("0.#")+"%",d.Regeneration.ToString("0.##")};
            for(int i=0;i<4;i++){float y=r.y+119+i*28;Label(new Rect(r.x+16,y,r.width-90,20),names[i],12,muted,true);Label(new Rect(r.x+16,y,r.width-32,20),values[i],15,white,true,TextAnchor.UpperRight);Stroke(new Vector2(r.x+16,y+23),new Vector2(r.xMax-16,y+23),new Color(line.r,line.g,line.b,.35f),1);}
            string passive=d.Armor[0]!=null?"EXPOSE +"+d.StaggerBonus.ToString("0.#")+" damage vs staggered targets.":d.Armor.Any(a=>a!=null)?"Armor passives active. Hover gear for details.":"Armor slots grant stats and passive effects.";
            Label(new Rect(r.x+16,r.y+235,r.width-32,18),passive,10,Accent);
        }
        void ConnectMount(Rect slot,Vector2 target)
        {
            Vector2 origin=new Vector2(slot.center.x<target.x?slot.xMax:slot.x,slot.center.y),elbow=new Vector2((origin.x+target.x)/2,origin.y);
            Stroke(origin,elbow,new Color(Accent.r,Accent.g,Accent.b,.6f),1);Stroke(elbow,target,new Color(Accent.r,Accent.g,Accent.b,.6f),1);Box(new Rect(origin.x-2,origin.y-2,4,4),Accent);
        }
        void EquipmentMount(Rect r,LoadoutItem item,ItemKind kind,string label)
        {
            Panel(r);Label(new Rect(r.x+8,r.y+5,r.width-16,13),label,9,TypeColor(kind),true);
            ItemDefinition icon=item?.def??(LoadoutCatalog.IsArmor(kind)?LoadoutCatalog.Armor[LoadoutCatalog.ArmorIndex(kind)]:null);
            ActionGlyph(new Rect(r.center.x-20,r.y+22,40,40),icon,item==null?muted:TypeColor(kind));
            Label(new Rect(r.x+6,r.y+63,r.width-12,19),item?.def.name??"Empty",10,item==null?muted:white,true,TextAnchor.MiddleCenter);
            if(item!=null){Outline(new Rect(r.x+6,r.y+20,r.width-12,42),RarityColor(item.rank),5);ItemTooltip(r,item);}
        }
        string SuggestedDestination(ItemDefinition def)
        {
            game.Deck.SuggestedPlace(def,out var place,out int index);
            return place==PlaceKind.Sequence?"SEQUENCE "+(index+1):place==PlaceKind.Weapon?(def.hands==2?"BOTH WEAPON MOUNTS":"WEAPON "+(index+1)+" (REPLACED GEAR IS STORED)"):place==PlaceKind.Inventory?"STORE (SEQUENCE IS FULL)":place.ToString().ToUpperInvariant()+" MOUNT";
        }
        void ItemPanel(Rect r,LoadoutItem item,bool chosen,string name,string heading,ItemKind emptyKind=ItemKind.Ability)
        {
            bool highlight=chosen||item!=null&&item==selectedItem;
            var shown=item?.def.kind==ItemKind.Basic?game.Deck.WeaponFor(item):item;
            ItemKind kind=item?.def.kind??emptyKind;Color tint=TypeColor(kind);
            Plate(r,highlight?new Color(.05f,.15f,.19f):ink,highlight?Accent:line,8,highlight?2:1);
            if(shown!=null)Stroke(new Vector2(r.x+10,r.y+1),new Vector2(r.xMax-10,r.y+1),RarityColor(shown.rank),2);
            Label(new Rect(r.x+10,r.y+9,r.width-20,15),heading,9,tint,true);
            if(r.height<70)
            {
                ActionGlyph(new Rect(r.x+10,r.y+26,20,20),shown?.def,muted);
                Label(new Rect(r.x+38,r.y+28,r.width-48,20),shown?.def.name??name,13,white,true);ItemTooltip(r,item);return;
            }
            ActionGlyph(new Rect(r.x+10,r.y+32,28,28),shown?.def,muted);
            Label(new Rect(r.x+46,r.y+30,r.width-56,22),shown?.def.name??name,r.width<200?12:14,white,true);
            if(shown!=null)
            {
                Label(new Rect(r.x+46,r.y+55,r.width-56,16),ItemRarity.Names[shown.rank]+(LoadoutCatalog.IsArmor(shown.def.kind)||shown.def.kind==ItemKind.Rig?" / PASSIVE":" / Kills: "+game.ActionKills(item)),10,RarityColor(shown.rank));
                ItemTooltip(r,item);
            }
        }
        void ClearLoadoutGesture()
        {
            dragging=false;dragItem=selectedItem=lastClickItem=null;dragOffer=lastClickOffer=-1;lastClickOffers=null;lastClickTime=-10;GUIUtility.hotControl=0;loadoutFeedback="";
        }
        void RewardTarget(int offer,DropRegion hit,out PlaceKind place,out int index)
        {
            place=hit.place;index=hit.index;
            if(place==PlaceKind.Sequence&&game.Deck.Offers[offer].def.kind==ItemKind.Weapon&&hit.item?.def.kind==ItemKind.Basic){place=PlaceKind.Weapon;index=hit.item.mount;}
        }
        void DropReward(int offer,DropRegion hit)
        {
            RewardTarget(offer,hit,out var place,out int index);
            if(game.ClaimReward(offer,place,index,!editAfterPick)){ClearLoadoutGesture();GUIUtility.ExitGUI();}
            loadoutFeedback="That destination does not fit. Drop abilities into the sequence, weapons into mounts, or spare rewards into inventory.";
        }
        void HandleLoadoutInput()
        {
            var e=Event.current;DropRegion hit=dropRegions.Find(r=>r.rect.Contains(e.mousePosition));
            if(e.type==EventType.MouseDown&&e.button==0&&hit!=null)
            {dragStart=e.mousePosition;dragItem=hit.item;dragOffer=hit.offer;dragging=false;e.Use();}
            if(e.type==EventType.MouseDrag&&(dragItem!=null||dragOffer>=0)&&Vector2.Distance(e.mousePosition,dragStart)>6){dragging=true;e.Use();}
            if(e.type!=EventType.MouseUp||e.button!=0)return;
            if(dragging)
            {
                if(hit!=null&&hit.offer<0)
                {
                    if(dragItem!=null){game.MoveItem(dragItem,hit.place,hit.index);loadoutFeedback=game.Deck.MoveMessage;}
                    else if(dragOffer>=0)DropReward(dragOffer,hit);
                }
                lastClickTime=-10;GUIUtility.hotControl=0;selectedItem=null;e.Use();
            }
            else if(hit!=null)
            {
                bool twice=Time.realtimeSinceStartup-lastClickTime<.4f&&lastClickOffers==game.Deck.Offers&&lastClickOffer==hit.offer&&lastClickItem==hit.item;
                lastClickTime=Time.realtimeSinceStartup;lastClickOffer=hit.offer;lastClickItem=hit.item;lastClickOffers=game.Deck.Offers;
                if(twice&&hit.offer>=0){game.SelectOffer(hit.offer);game.AcceptSelectedReward(!editAfterPick);ClearLoadoutGesture();GUIUtility.ExitGUI();}
                else if(twice&&hit.item!=null){game.AutoEquip(hit.item);selectedItem=null;loadoutFeedback=game.Deck.MoveMessage;}
                else if(hit.offer>=0){game.SelectOffer(hit.offer);selectedItem=null;}
                else if(selectedItem!=null&&hit.item!=selectedItem){game.MoveItem(selectedItem,hit.place,hit.index);selectedItem=null;loadoutFeedback=game.Deck.MoveMessage;}
                else if(!game.Deck.RewardClaimed&&game.Deck.Selected>=0)DropReward(game.Deck.Selected,hit);
                else if(hit.item!=null)selectedItem=selectedItem==hit.item?null:hit.item;
                e.Use();
            }
            dragging=false;dragItem=null;dragOffer=-1;
        }
        void DrawDragPreview()
        {
            if(!dragging)return;
            string name=dragItem?.Name??(dragOffer>=0?game.Deck.Offers[dragOffer].Name:"");
            foreach(var region in dropRegions)if(region.offer<0)
            {
                bool valid;
                if(dragItem!=null)valid=game.Deck.CanMove(dragItem,region.place,region.index);
                else {RewardTarget(dragOffer,region,out var place,out int index);valid=game.Deck.CanClaimTo(dragOffer,place,index);}
                if(valid)Outline(region.rect,Accent,8,region.rect.Contains(Event.current.mousePosition)?3:1);
                else if(region.rect.Contains(Event.current.mousePosition))Outline(region.rect,Gold,8,2);
            }
            var m=Event.current.mousePosition;var r=new Rect(m.x+14,m.y-24,270,46);Panel(r);Label(new Rect(r.x+12,r.y+12,246,25),name,16,Accent,true);
        }
    }
}
