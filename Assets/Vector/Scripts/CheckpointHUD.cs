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
        Vector2 inventoryScroll;
        LoadoutItem selectedItem,dragItem;
        int dragOffer=-1;
        bool dragging;
        Vector2 dragStart;
        void Draft(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.01f,.025f,.03f,.96f));
            var deck=game.Deck;if(selectedItem!=null&&!deck.Items.Contains(selectedItem))selectedItem=null;
            float x=24,width=w-48,cw=(width-24)/3,sw=(width-40)/5,ew=(width-36)/4;
            var inventory=deck.Inventory.ToArray();int cols=Mathf.Max(1,(int)(width/178));
            Rect invRect=new Rect(x,542,width,h-620);
            dropRegions.Clear();
            for(int i=0;i<3;i++)if(!deck.RewardClaimed)dropRegions.Add(new DropRegion{rect=new Rect(x+i*(cw+12),91,cw,160),offer=i});
            for(int i=0;i<5;i++)dropRegions.Add(new DropRegion{rect=new Rect(x+i*(sw+10),330,sw,76),place=PlaceKind.Sequence,index=i,item=deck.Slots[i]});
            for(int i=0;i<2;i++)dropRegions.Add(new DropRegion{rect=new Rect(x+i*(ew+12),440,ew,76),place=PlaceKind.Weapon,index=i,item=deck.Weapons[i]});
            dropRegions.Add(new DropRegion{rect=new Rect(x+2*(ew+12),440,ew,76),place=PlaceKind.Rig,item=deck.Rig});
            dropRegions.Add(new DropRegion{rect=new Rect(x+3*(ew+12),440,ew,76),place=PlaceKind.Ultimate,item=deck.Ultimate});
            dropRegions.Add(new DropRegion{rect=invRect,place=PlaceKind.Inventory,index=-1});
            for(int i=0;i<inventory.Length;i++)
            {
                var r=new Rect(x+(i%cols)*(width/cols),invRect.y+(i/cols)*70-inventoryScroll.y,width/cols-8,63);
                float top=Mathf.Max(r.y,invRect.y),bottom=Mathf.Min(r.yMax,invRect.yMax);
                if(bottom>top)dropRegions.Insert(dropRegions.Count-1,new DropRegion{rect=new Rect(r.x,top,r.width,bottom-top),place=PlaceKind.Inventory,item=inventory[i]});
            }
            HandleLoadoutInput();
            string phase=game.OpeningDraft?"OPENING DRAFT "+(game.OpeningPicks+1)+" / 2":"CHECKPOINT "+game.Wave+" / 2";
            Label(new Rect(x,17,width-320,25),phase+" / COMBAT AND COUNTDOWN PAUSED",16,Accent,true);
            Label(new Rect(w-340,17,316,25),"INTEGRITY "+Mathf.CeilToInt(game.Health)+" / "+(int)game.tuning.heroHealth,15,white,true,TextAnchor.UpperRight);
            Label(new Rect(x,48,width,32),deck.Draft==DraftKind.Ability?"1. PICK AN ABILITY AND AN EMPTY POSITION":deck.Draft==DraftKind.Weapon?"1. PICK YOUR FIRST WEAPON":"1. PICK A REWARD — KEEP IT OR CHANGE YOUR BUILD",24,white,true);
            for(int i=0;i<3;i++)
            {
                var offer=deck.Offers[i];float cx=x+i*(cw+12);Rect r=new Rect(cx,91,cw,160);
                Box(r,deck.Selected==i?new Color(.09f,.23f,.21f):new Color(.055f,.11f,.13f));Border(r,RarityColor(offer.rank),deck.Selected==i?3:1);
                bool upgrade=offer.owned>0;
                TypeIcon(new Rect(cx+11,99,20,20),offer.def.kind,TypeColor(offer.def.kind),upgrade);
                Label(new Rect(cx+37,101,cw*.6f-37,18),(deck.RewardClaimed?(deck.Selected==i?"CLAIMED / ":"PASSED / "):"PICK "+(i+1)+" / ")+OfferType(offer),10,TypeColor(offer.def.kind),true);
                Label(new Rect(cx+cw*.60f,101,cw*.4f-12,18),ItemRarity.Names[offer.rank].ToUpperInvariant(),11,RarityColor(offer.rank),true,TextAnchor.UpperRight);
                Label(new Rect(cx+12,124,cw-24,27),offer.Name,22,white,true);
                Label(new Rect(cx+12,154,cw-24,19),ItemStats(offer.def,offer.rank)+" / "+ItemRarity.StatBonus(offer.rank),11,RarityColor(offer.rank),true);
                Label(new Rect(cx+12,178,cw-24,38),offer.def.description,13,white);
                Label(new Rect(cx+12,221,cw-24,26),UpgradePreview(offer),12,upgrade?Gold:muted);

            }
            GUI.enabled=!deck.RewardClaimed&&deck.Rerolls>0;
            if(Button(new Rect(x,261,200,34),"REROLL / "+deck.Rerolls+" [ TAB ]")){game.Reroll();selectedItem=null;}GUI.enabled=true;
            GUI.enabled=deck.CanClaim;
            if(Button(new Rect(x+212,261,240,34),deck.Draft==DraftKind.Ability?"CLAIM INTO CHOSEN SLOT":"CLAIM TO INVENTORY",true)){game.ConfirmDraft();selectedItem=null;}
            GUI.enabled=true;
            Label(new Rect(x+468,261,width-468,38),deck.RewardClaimed?deck.Receipt:deck.Draft==DraftKind.Ability?"Choose a card, then an empty position below. Dragging a card chooses both.":"Choose one reward. Weapons and gear enter your inventory; drag them into a mount.",12,deck.RewardClaimed?Accent:muted);
            Label(new Rect(x,305,width,21),"2. ARRANGE YOUR FIVE ACTIONS / DRAG TO REORDER / MATCHING NEIGHBORS ACT TOGETHER",13,Accent,true);
            for(int i=0;i<5;i++)
            {
                Rect r=new Rect(x+i*(sw+10),330,sw,76);var item=deck.Slots[i];
                ItemPanel(r,item,deck.SelectedSlot==i,deck.SlotName(i),"SLOT "+(i+1)+(item?.def.kind==ItemKind.Basic?" / WEAPON "+(item.mount+1).ToString():""));
                if(deck.PairAfter(i)>=0)Box(new Rect(r.x,r.yMax-4,r.width,4),Gold);
            }
            GroupOutline(x,330,sw,10,76);
            Label(new Rect(x,416,width,21),"3. EQUIP / EMPTY MOUNTS USE FISTS / TWO-HANDED FILLS BOTH MOUNTS / ULTIMATE STAYS OUTSIDE THE LOOP",13,Accent,true);
            for(int i=0;i<2;i++)
            {
                Rect r=new Rect(x+i*(ew+12),440,ew,76);var item=deck.Weapons[i];
                ItemPanel(r,item,false,item?.Name??"Fists / always available","WEAPON "+(i+1).ToString()+(deck.TwoHanded?" / 2H · BOTH MOUNTS":item==null?"": " / 1H"),ItemKind.Weapon);
                if(deck.TwoHanded)Box(new Rect(r.x,r.yMax-3,r.width,3),Gold);
            }
            ItemPanel(new Rect(x+2*(ew+12),440,ew,76),deck.Rig,false,deck.Rig?.Name??"No rig equipped","RIG / ONE ACTIVE",ItemKind.Rig);
            ItemPanel(new Rect(x+3*(ew+12),440,ew,76),deck.Ultimate,false,deck.Ultimate?.Name??"No ultimate equipped","ULTIMATE / "+game.UltimateCooldown+"s / "+(game.UltimateAutomatic?"AUTO":"MANUAL"),ItemKind.Ultimate);
            Label(new Rect(x,521,width,19),"INVENTORY / "+inventory.Length+" STORED / DRAG HERE TO UNEQUIP / 3 IDENTICAL COPIES + SAME RARITY FUSE / LEGENDARY IS MAX",11,muted,true);
            Panel(invRect);
            inventoryScroll=GUI.BeginScrollView(invRect,inventoryScroll,new Rect(0,0,width-18,Mathf.Max(invRect.height-1,Mathf.Ceil(inventory.Length/(float)cols)*70)));
            for(int i=0;i<inventory.Length;i++)
            {
                var item=inventory[i];Rect r=new Rect((i%cols)*(width/cols),(i/cols)*70,width/cols-8,63);
                ItemPanel(r,item,false,item.Name,item.def.kind.ToString().ToUpperInvariant()+" / "+deck.Copies(item.def,item.rank)+" OF 3");
            }
            if(inventory.Length==0)Label(new Rect(15,16,width-60,40),"Your stored abilities, weapons and gear appear here. Drag an equipped item here to store it.",14,muted);
            GUI.EndScrollView();
            string help=selectedItem==null?(deck.Selected>=0?SynergyText(deck.Offers[deck.Selected].def)+" / "+deck.Offers[deck.Selected].def.upgrade:deck.MoveMessage):SynergyText(selectedItem.def)+" / "+selectedItem.def.upgrade+" / Kills: "+game.KillsFor(selectedItem);
            if(!string.IsNullOrEmpty(GUI.tooltip))help=GUI.tooltip;
            Label(new Rect(x,h-64,width-350,44),string.IsNullOrEmpty(help)?"ODDS: Common 55% / Uncommon 27% / Rare 12% / Epic 5% / Legendary 1%. Click an item to inspect; click a destination to move.":help,12,muted);
            GUI.enabled=deck.RewardClaimed;
            string next=game.OpeningDraft&&game.OpeningPicks==0?"NEXT OPENING PICK":!game.OpeningDraft&&game.Wave==1&&deck.Draft==DraftKind.Ability?"NEXT: WEAPON PICK":"CONTINUE TO FIGHT";
            if(Button(new Rect(w-350,h-61,326,43),next+" [ ENTER ]",true)){selectedItem=null;game.ConfirmDraft();}
            GUI.enabled=true;DrawDragPreview();
        }
        void ItemPanel(Rect r,LoadoutItem item,bool chosen,string name,string heading,ItemKind emptyKind=ItemKind.Ability)
        {
            bool highlight=chosen||item!=null&&item==selectedItem;
            var shown=item?.def.kind==ItemKind.Basic?game.Deck.WeaponFor(item):item;
            ItemKind kind=item?.def.kind??emptyKind;
            Color tint=TypeColor(kind);Box(r,highlight?new Color(.12f,.23f,.24f):new Color(.07f,.13f,.15f));
            if(shown!=null)Box(new Rect(r.x,r.y,r.width,2),RarityColor(shown.rank));
            if(highlight)Border(r,Accent,2);
            TypeIcon(new Rect(r.x+8,r.y+7,17,17),kind,tint);
            Label(new Rect(r.x+29,r.y+8,r.width-39,16),heading,9,tint,true);
            string shortName=item?.def.kind==ItemKind.Basic?item.def.name+" / "+shown.def.name:shown?.def.name??name;
            Label(new Rect(r.x+10,r.y+28,r.width-20,22),shortName,14,white,true);
            if(shown!=null)
            {
                Label(new Rect(r.x+10,r.y+51,r.width-44,18),ItemRarity.Names[shown.rank]+" / Kills: "+game.ActionKills(item),10,RarityColor(shown.rank));
                EffectIcon(new Rect(r.xMax-27,r.y+48,20,20),EffectFor(shown.def),Accent);
                GUI.Label(r,new GUIContent("",SynergyText(shown.def)+" / "+ItemStats(shown.def,shown.rank)+" / "+shown.def.upgrade),GUIStyle.none);
            }

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
                    if(dragItem!=null)game.MoveItem(dragItem,hit.place,hit.index);
                    else if(dragOffer>=0&&hit.place==PlaceKind.Sequence){game.SelectOffer(dragOffer);game.AssignSlot(hit.index);}
                }
                GUIUtility.hotControl=0;selectedItem=null;e.Use();
            }
            else if(hit!=null)
            {
                if(hit.offer>=0){game.SelectOffer(hit.offer);selectedItem=null;}
                else if(selectedItem!=null&&hit.item!=selectedItem){game.MoveItem(selectedItem,hit.place,hit.index);selectedItem=null;}
                else if(hit.item!=null)selectedItem=selectedItem==hit.item?null:hit.item;
                else if(hit.place==PlaceKind.Sequence)game.AssignSlot(hit.index);
                e.Use();
            }
            dragging=false;dragItem=null;dragOffer=-1;
        }
        void DrawDragPreview()
        {
            if(!dragging)return;
            string name=dragItem?.Name??(dragOffer>=0?game.Deck.Offers[dragOffer].Name:"");
            foreach(var region in dropRegions)if(region.offer<0&&region.rect.Contains(Event.current.mousePosition))
            {
                bool valid=dragItem!=null?game.Deck.CanMove(dragItem,region.place,region.index):region.place==PlaceKind.Sequence&&game.Deck.Slots[region.index]==null;
                Box(new Rect(region.rect.x,region.rect.y,region.rect.width,3),valid?Accent:Gold);break;
            }
            var m=Event.current.mousePosition;var r=new Rect(m.x+14,m.y-24,240,46);Panel(r);Label(new Rect(r.x+10,r.y+11,220,26),name,17,Accent,true);
        }
    }
}
