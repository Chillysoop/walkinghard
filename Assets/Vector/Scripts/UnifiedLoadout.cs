using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        bool UnifiedCheckpoint=>game.State==VectorEncounter.RunState.Drafting&&!game.TutorialActive&&!game.StopOpen;
        Rect comparisonRect,workbenchBody,portraitSpace;
        bool inlineComparisonVisible;
        float workbenchWidth;
        Vector2 comparisonScroll;
        string comparisonKey="";
        int storageFilter;
        static readonly string[] StorageFilters={"ALL","WEAPONS","ABILITIES","GEAR","AUGMENTS"};
        sealed class LoadoutHover
        {
            public RewardOffer offer;
            public int offerIndex=-1;
            public LoadoutItem source,before,beforeSecond,after;
            public DropRegion target;
            public AbilityDeck.Projection projection;
            public bool inspect,explicitTarget,invalid;
            public string heading="",fate="";
            public ItemDefinition Def=>offer?.def??source?.def;
        }
        LoadoutHover loadoutHover;
        bool StorageVisible(LoadoutItem i)=>storageFilter==0||storageFilter==1&&i.def.kind==ItemKind.Weapon||storageFilter==2&&i.def.kind==ItemKind.Ability||storageFilter==3&&(LoadoutCatalog.IsArmor(i.def.kind)||i.def.kind==ItemKind.Rig||i.def.kind==ItemKind.Ultimate)||storageFilter==4&&i.def.kind==ItemKind.Augment;
        string PlaceLabel(DropRegion r)=>r==null?"CHOOSE A SLOT":r.socket?"AUGMENT SOCKET":r.place==PlaceKind.Sequence?"SEQUENCE "+(r.index+1):r.place==PlaceKind.Weapon?"HAND "+(r.index+1):r.place==PlaceKind.Inventory?(r.item==null?"STORAGE":"STORED "+r.item.def.name.ToUpperInvariant()):r.place.ToString().ToUpperInvariant();
        DropRegion SuggestedRegion(ItemDefinition def,bool reward=true)
        {
            var d=game.Deck;
            if(def.kind==ItemKind.Augment){var hosts=d.CompatibleHosts(def).ToArray();return hosts.Length==1?(dropRegions.FirstOrDefault(r=>r.offer<0&&!r.socket&&Host(r)==hosts[0])??new DropRegion{place=PlaceKind.Inventory,item=hosts[0]}):hosts.Length==0?dropRegions.FirstOrDefault(r=>r.place==PlaceKind.Inventory&&r.index==-1):null;}
            d.SuggestedPlace(def,out var place,out int index);
            if(reward&&def.kind==ItemKind.Ability&&d.SelectedSlot>=0){place=PlaceKind.Sequence;index=d.SelectedSlot;}
            return dropRegions.FirstOrDefault(r=>r.offer<0&&!r.socket&&r.place==place&&(place==PlaceKind.Inventory?r.index==-1:r.index==index));
        }
        LoadoutItem DisplayItem(LoadoutItem item)=>item?.def.kind==ItemKind.Basic?game.Deck.WeaponFor(item):item;
        LoadoutHover ResolveLoadoutHover()
        {
            var d=game.Deck;
            if(inlineComparisonVisible&&!dragging&&comparisonRect.Contains(Pointer)&&loadoutHover?.Def!=null&&(loadoutHover.source==null||d.Items.Contains(loadoutHover.source))&&(loadoutHover.offer==null||!d.RewardClaimed&&System.Array.IndexOf(d.Offers,loadoutHover.offer)>=0))return loadoutHover;
            var p=new LoadoutHover();var hit=dropRegions.FirstOrDefault(r=>r.rect.Contains(Pointer));
            if(presentationHover!=null){p.offer=presentationOffer;p.offerIndex=p.offer==null?-1:Array.IndexOf(d.Offers,p.offer);p.source=p.offer==null?presentationHover:null;}
            else if(dragItem!=null)p.source=dragItem;
            else if(!d.RewardClaimed&&(dragOffer>=0||hit?.offer>=0))p.offerIndex=dragOffer>=0?dragOffer:hit.offer;
            else if(selectedItem!=null)p.source=selectedItem;
            else if(hit?.item!=null&&hit.place==PlaceKind.Inventory&&!hit.socket&&!(d.Selected>=0&&!d.RewardClaimed&&d.Offers[d.Selected].def.kind==ItemKind.Augment))p.source=hit.item;
            else if(!d.RewardClaimed&&d.Selected>=0)p.offerIndex=d.Selected;
            else if(hit?.item!=null){p.source=DisplayItem(hit.item);p.inspect=true;}
            if(p.offerIndex>=0)p.offer=d.Offers[p.offerIndex];
            if(p.Def==null)return p;
            if(p.inspect){p.after=p.source;p.heading="EQUIPPED / "+PlaceLabel(hit);return p;}
            p.explicitTarget=hit!=null&&hit.offer<0&&(p.source==null||hit.item!=p.source);
            p.target=p.explicitTarget?hit:SuggestedRegion(p.Def,p.offer!=null);
            if(p.source?.def.kind==ItemKind.Augment&&!d.CompatibleHosts(p.Def).Any()){p.after=p.source;p.heading="NO COMPATIBLE HOST";p.fate="This augment stays in storage until you own a matching weapon or ability.";return p;}
            if(p.target==null){p.after=p.source??new LoadoutItem(p.Def,p.offer.rank);p.heading="CHOOSE A COMPATIBLE ITEM";p.fate="Select this augment, then click a glowing item or socket. Check ALL storage pages for stored hosts.";return p;}
            bool fits=p.offer!=null?OfferFitsRegion(p.offerIndex,p.target):StoredFitsRegion(p.source,p.target)||!p.explicitTarget&&p.target.item==p.source;
            if(!fits){p.invalid=true;p.after=p.source??new LoadoutItem(p.Def,p.offer.rank);p.heading="DOESN'T FIT HERE";p.fate="Use a glowing "+DestinationName(p.Def).ToLowerInvariant()+". Hover only previews; nothing has changed.";return p;}
            var target=p.target;PlaceKind place=target.place;int index=target.index;
            if(p.offer!=null)RewardTarget(p.offerIndex,target,out place,out index);else StoredTarget(p.source,target,out place,out index);
            var host=p.Def.kind==ItemKind.Augment&&Host(target)?.Augmentable==true?Host(target):null;
            p.projection=d.PreviewChange(p.offer,p.source,place,index,host);
            p.before=host!=null?host:place==PlaceKind.Weapon?d.WeaponFor(d.Basics[index]):DisplayItem(target.item);
            if(p.Def.kind==ItemKind.Weapon&&p.Def.hands==2&&place==PlaceKind.Weapon&&!d.TwoHanded)p.beforeSecond=d.WeaponFor(d.Basics[1-index]);
            p.after=host!=null?p.projection.Copy(host):p.projection.incoming;
            if(p.after?.def.kind==ItemKind.Basic)p.after=p.projection.after.WeaponFor(p.after);
            p.heading=(p.explicitTarget?(place==PlaceKind.Inventory&&host==null?"STORE → ":host!=null?"ATTACH → ":"EQUIP → "):"COMPARE → ")+(p.Def.kind==ItemKind.Weapon&&p.Def.hands==2&&place==PlaceKind.Weapon?"BOTH HANDS":PlaceLabel(target));
            var after=p.projection.after;
            var stored=d.Items.Where(i=>!d.Inventory.Contains(i)&&after.Inventory.Contains(p.projection.Copy(i))).Select(i=>i.def.name).Distinct().ToArray();
            var consumed=d.Items.Where(i=>!after.Items.Contains(p.projection.Copy(i))).ToArray();
            if(stored.Length>0)p.fate="TO STORAGE: "+string.Join(", ",stored)+".";
            else if(p.Def.kind==ItemKind.Augment&&host==null&&!d.CompatibleHosts(p.Def).Any())p.fate="No compatible host yet. Store this augment for later.";
            else if(place==PlaceKind.Inventory)p.fate=p.projection.fusionRanks.Count>0?"Copies combine automatically, even when you choose Store.":"Stored for later. Your equipped build stays the same.";
            else p.fate=p.source!=null?"Moves your owned item; no reward is taken.":"This takes one reward choice. The other offers are passed.";
            if(p.Def.kind==ItemKind.Weapon&&place==PlaceKind.Weapon)p.fate+=" "+(p.Def.hands==2?"Both attacks become one locked 2H group.":d.TwoHanded?"The 2H group unlocks; the other hand uses fists.":"Matching adjacent weapons attack together.");
            if(place==PlaceKind.Sequence&&target.item?.def.kind==ItemKind.Basic)p.fate+=" Basic moves to another sequence slot.";
            if(consumed.Length>0)p.fate+=" "+consumed.Length+" owned "+(consumed.Length==1?"copy combines":"copies combine")+" into the fusion.";
            return p;
        }
        void UnifiedDraft(float w,float h)
        {
            var d=game.Deck;TabletFrame(w,h);
            if(selectedItem!=null&&!d.Items.Contains(selectedItem))selectedItem=null;
            float middleY=250,middleH=h-424,characterW=(w-52)*.55f,storageW=w-52-characterW;
            if(!Mathf.Approximately(workbenchWidth,w)||!Mathf.Approximately(workbenchBody.height,middleH))
            {inlineComparisonVisible=false;comparisonRect=Rect.zero;}
            workbenchWidth=w;
            Rect character=new Rect(20,middleY,characterW,middleH),storage=new Rect(character.xMax+12,middleY,storageW,middleH);
            workbenchBody=new Rect(20,middleY,w-40,middleH);
            float cardW=(w-60)/3,sequenceY=h-143,seqW=(w-300)/5;
            Rect[] mounts=new Rect[6];float mountW=character.width*.25f;
            portraitSpace=new Rect(character.x+mountW+28,character.y+10,character.width-2*mountW-56,character.height-20);
            float mountStep=(middleH-54-76)/2;
            for(int i=0;i<6;i++)mounts[i]=new Rect(i%2==0?character.x+10:character.xMax-mountW-10,character.y+34+(i/2)*mountStep,mountW,76);
            var inventory=d.Inventory.OrderBy(i=>i.def.kind).ThenBy(i=>i.def.name).ThenBy(i=>i.rank).Where(StorageVisible).ToArray();
            int storageRows=Mathf.Clamp(Mathf.FloorToInt((middleH-106)/64),3,6),pageSize=storageRows*4;
            int pages=Mathf.Max(1,Mathf.CeilToInt(inventory.Length/(float)pageSize));inventoryScroll.y=Mathf.Clamp(Mathf.Round(inventoryScroll.y),0,pages-1);
            Rect inventoryRect=new Rect(storage.x+10,storage.y+63,storage.width-20,storage.height-111),storeRect=new Rect(storage.x+10,storage.yMax-33,storage.width-20,24);
            if(Event.current.type==EventType.ScrollWheel&&inventoryRect.Contains(Pointer)){inventoryScroll.y=Mathf.Clamp(inventoryScroll.y+Mathf.Sign(Event.current.delta.y),0,pages-1);Event.current.Use();}
            float cellW=(inventoryRect.width-15)/4,cellH=(inventoryRect.height-5*(storageRows-1))/storageRows;int first=(int)inventoryScroll.y*pageSize;
            dropRegions.Clear();
            if(!d.RewardClaimed)for(int i=0;i<3;i++)dropRegions.Add(new DropRegion{rect=new Rect(20+i*(cardW+10),94,cardW,144),offer=i});
            for(int i=0;i<5;i++)dropRegions.Add(new DropRegion{rect=new Rect(20+i*(seqW+8),sequenceY,seqW,87),place=PlaceKind.Sequence,index=i,item=d.Slots[i]});
            PlaceKind[] places={PlaceKind.Helmet,PlaceKind.Armor,PlaceKind.Weapon,PlaceKind.Weapon,PlaceKind.Rig,PlaceKind.Boots};
            LoadoutItem[] worn={d.Armor[0],d.Armor[1],d.Weapons[0],d.Weapons[1],d.Rig,d.Armor[2]};
            for(int i=0;i<6;i++)dropRegions.Add(new DropRegion{rect=mounts[i],place=places[i],index=i==3?1:0,item=worn[i]});
            Rect ultRect=new Rect(w-240,sequenceY,220,87);dropRegions.Add(new DropRegion{rect=ultRect,place=PlaceKind.Ultimate,item=d.Ultimate});
            for(int i=0;i<Mathf.Min(pageSize,inventory.Length-first);i++)dropRegions.Add(new DropRegion{rect=new Rect(inventoryRect.x+i%4*(cellW+5),inventoryRect.y+i/4*(cellH+5),cellW,cellH),place=PlaceKind.Inventory,item=inventory[first+i]});
            dropRegions.Add(new DropRegion{rect=storeRect,place=PlaceKind.Inventory,index=-1});
            AddSocketRegions();AuditWorkbenchText(mountW,cellW,cellH,cardW);
            if(!iconGuide&&!waveDetails)HandleLoadoutInput();
            loadoutHover=ResolveLoadoutHover();
            Label(new Rect(30,16,600,20),"DEAD LETTER OFFICE / FIELD TERMINAL  ·  COMBAT PAUSED",11,TabletText,true);
            string phase=game.OpeningDraft?"Opening pick "+(game.OpeningPicks+1)+" / 2":"Checkpoint "+game.Wave.ToString("00");
            if(game.JourneyActive)phase="District "+(game.District+1)+" / "+phase;
            Label(new Rect(20,36,750,32),phase+" · "+(d.RewardClaimed?"Reward taken":"Choose one reward"),24,white,true);
            Label(new Rect(20,72,w-40,18),d.RewardClaimed?"Your build is ready. Rearrange items, or continue.":"Select an item, then hover a slot to compare. Click the slot or drag to equip.",12,TabletText);
            if(game.ExpeditionActive&&Button(new Rect(w-185,15,165,30),"SAVE & OFFICE"))game.SuspendExpedition();
            if(game.LastWaveFeedback!=null&&Button(new Rect(w-355,15,158,30),"LAST WAVE DETAILS",waveDetails))waveDetails=!waveDetails;
            if(game.ExpeditionActive)Label(new Rect(w-450,50,430,20),game.Salvage+" SALVAGE / "+game.ChapterSparks+" SPARKS BANKED",11,muted,false,TextAnchor.UpperRight);
            for(int i=0;i<3;i++)RewardCard(new Rect(20+i*(cardW+10),94,cardW,144),i,true);
            TabletSection(character,1,"EQUIPPED / YOUR WALKER");
            DrawPortrait(new Rect(character.x+mountW+8,character.y+28,character.width-mountW*2-16,character.height-35));
            string[] titles={"HELMET","ARMOR","HAND 1","HAND 2","RIG","BOOTS"};
            for(int i=0;i<6;i++)
            {
                ConnectMount(mounts[i],new Vector2(character.center.x, mounts[i].center.y));
                var shown=i==2||i==3?d.WeaponFor(d.Basics[i-2]):worn[i];
                WorkbenchSlot(mounts[i],shown,titles[i],i==2||i==3?ItemKind.Weapon:i==4?ItemKind.Rig:i==0?ItemKind.Helmet:i==1?ItemKind.Armor:ItemKind.Boots);
            }
            TabletSection(storage,2,"STORAGE / "+d.Inventory.Count()+" ITEMS");
            for(int i=0;i<5;i++){float fw=(storage.width-20)/5;bool was=GUI.enabled;GUI.enabled=!dragging;if(StorageTab(new Rect(storage.x+10+i*fw,storage.y+34,fw-3,21),StorageFilters[i],storageFilter==i)){storageFilter=i;inventoryScroll.y=0;}GUI.enabled=was;}
            foreach(var r in dropRegions.Where(r=>r.place==PlaceKind.Inventory&&r.item!=null&&!r.socket))
            {
                TabletCell(r.rect,TabletCargo,ItemEdge(r.item),r.item==selectedItem,r.rect.Contains(Pointer));
                TabletArtWell(new Rect(r.rect.x+5,r.rect.y+3,27,27));
                ActionGlyph(new Rect(r.rect.x+5,r.rect.y+4,25,25),r.item.def,white);
                Label(new Rect(r.rect.x+5,r.rect.y+31,r.rect.width-10,r.rect.height-32),r.item.def.name,10,white,true);
                if(r.item.def.kind!=ItemKind.Augment)Label(new Rect(r.rect.x+32,r.rect.y+9,r.rect.width-36,15),ItemRarity.Names[r.item.rank],10,ItemEdge(r.item),true,TextAnchor.UpperRight);
                ItemTooltip(r.rect,r.item);
            }
            if(inventory.Length==0)Label(inventoryRect,storageFilter==0?"Nothing stored yet.\nReplaced gear comes here.":"No items in this category.",12,muted,false,TextAnchor.MiddleCenter);
            if(pages>1){if(Button(new Rect(storage.x+10,storage.yMax-51,26,17),"‹"))inventoryScroll.y=Mathf.Max(0,inventoryScroll.y-1);if(Button(new Rect(storage.xMax-36,storage.yMax-51,26,17),"›"))inventoryScroll.y=Mathf.Min(pages-1,inventoryScroll.y+1);}
            Label(new Rect(storage.x+40,storage.yMax-51,storage.width-80,16),"PAGE "+(inventoryScroll.y+1)+" / "+pages+" · SCROLL TO BROWSE",10,TabletText,false,TextAnchor.MiddleCenter);
            Plate(storeRect,new Color(.10f,.14f,.16f),new Color(.45f,.56f,.60f),4);Label(storeRect,"CLICK / DROP HERE TO STORE",11,TabletText,true,TextAnchor.MiddleCenter);
            TabletMaterial(new Rect(20,sequenceY-26,w-40,121),3,8);Label(new Rect(30,sequenceY-23,315,20),"SEQUENCE / DRAG TO REORDER / REPEATS",11,white,true);TargetWaitCheckpoint(w,sequenceY-23);
            for(int i=0;i<5;i++)
            {
                Rect r=new Rect(20+i*(seqW+8),sequenceY,seqW,87);var item=d.Slots[i];
                ItemPanel(r,item,false,"Empty ability slot",(i+1)+" / "+(item?.def.kind==ItemKind.Basic?"HAND "+(item.mount+1)+" ATTACK":"ABILITY"));
                if(i<4)Label(new Rect(r.xMax,r.y+30,8,20),"›",14,muted);
            }
            GroupOutline(20,sequenceY,seqW,8,87);
            ItemPanel(ultRect,d.Ultimate,false,"No ultimate equipped","ULTIMATE / SEPARATE TIMER",ItemKind.Ultimate);
            DrawSockets();DrawWorkbenchGuidance();DrawWorkbenchComparison();DrawDragPreview();
            GUI.enabled=!d.RewardClaimed&&d.Rerolls>0;
            if(Button(new Rect(20,h-40,126,28),"REROLL "+d.Rerolls+" [ TAB ]")){game.Reroll();ClearLoadoutGesture();GUIUtility.ExitGUI();}GUI.enabled=true;
            if(Button(new Rect(154,h-40,190,28),editAfterPick?"AFTER PICK: KEEP EDITING":"AFTER PICK: CONTINUE",editAfterPick))editAfterPick=!editAfterPick;
            if(Button(new Rect(352,h-40,90,28),"ICON KEY"))iconGuide=!iconGuide;
            if(game.FusionHistory.Count>0)FusionHistoryButton(new Rect(450,h-40,226,28));
            else Label(new Rect(453,h-35,w-770,23),string.IsNullOrEmpty(loadoutFeedback)?"Replaced equipment is kept in storage.":loadoutFeedback,11,muted);
            GUI.enabled=selectedItem!=null?(selectedItem.def.kind==ItemKind.Augment?d.CompatibleHosts(selectedItem.def).Count()==1:SuggestedRegion(selectedItem.def,false)!=null):d.RewardClaimed||d.Selected>=0&&!(d.Offers[d.Selected].def.kind==ItemKind.Augment&&d.CompatibleHosts(d.Offers[d.Selected].def).Count()>1);
            if(TabletButton(new Rect(w-288,h-40,268,28),UnifiedActionLabel(),true,true))
            {ConfirmWorkbench();GUIUtility.ExitGUI();}GUI.enabled=true;
        }
        internal bool ConfirmWorkbench()
        {
            if(!UnifiedCheckpoint)return false;
            if(selectedItem!=null){if(!game.AutoEquip(selectedItem)){loadoutFeedback="Choose a glowing destination for this item.";return true;}}
            else if(game.Deck.RewardClaimed)game.ConfirmDraft();else game.AcceptSelectedReward(!editAfterPick);
            ClearLoadoutGesture();return true;
        }
        string UnifiedActionLabel()
        {
            var d=game.Deck;if(selectedItem?.def.kind==ItemKind.Augment&&!d.CompatibleHosts(selectedItem.def).Any())return "NO COMPATIBLE HOST";if(selectedItem!=null){var ownedTarget=SuggestedRegion(selectedItem.def,false);return ownedTarget==null?"CHOOSE A GLOWING ITEM":"MOVE → "+PlaceLabel(ownedTarget);}
            if(d.RewardClaimed)return CheckpointActionLabel();
            if(d.Selected<0)return "SELECT A REWARD";
            var def=d.Offers[d.Selected].def;if(def.kind==ItemKind.Augment)return CheckpointActionLabel();
            var target=SuggestedRegion(def);return (target?.place==PlaceKind.Inventory?"TAKE → ":"EQUIP → ")+(def.kind==ItemKind.Weapon&&def.hands==2?"BOTH HANDS":PlaceLabel(target));
        }
        bool StorageTab(Rect r,string label,bool selected)
        {Plate(r,selected?new Color(.15f,.32f,.33f):new Color(.12f,.15f,.18f),selected?Accent:line,4);if(selected)Box(new Rect(r.x+5,r.yMax-2,r.width-10,2),Accent);int size=10;while(size>8&&Style(size,true).CalcSize(new GUIContent(label)).x>r.width-4)size--;Label(r,label,size,selected?Accent:white,true,TextAnchor.MiddleCenter);return GUI.Button(r,GUIContent.none,GUIStyle.none);}
        void WorkbenchSlot(Rect r,LoadoutItem item,string heading,ItemKind kind)
        {
            TabletCell(r,TabletTeal,item==null?line:ItemEdge(item),item!=null&&item==selectedItem,r.Contains(Pointer));
            Box(new Rect(r.x+6,r.y+3,r.width-12,17),new Color(.02f,.07f,.08f,.40f));
            Label(new Rect(r.x+7,r.y+5,r.width-14,14),heading,10,TabletText,true);
            if(item==null){TypeIcon(new Rect(r.x+9,r.y+28,22,22),kind,line);Label(new Rect(r.x+37,r.y+29,r.width-43,25),"Empty",12,muted);return;}
            TabletArtWell(new Rect(r.x+6,r.y+23,31,31));ActionGlyph(new Rect(r.x+6,r.y+23,30,30),item.def,white);
            Label(new Rect(r.x+40,r.y+23,r.width-46,32),item.def.name,11,white,true);
            Label(new Rect(r.x+7,r.yMax-17,r.width-30,14),ItemRarity.Names[item.rank],10,ItemEdge(item));ItemTooltip(r,item);
        }
    }
}
