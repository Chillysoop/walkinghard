using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        AbilityDeck clarityDeck;
        int clarityWave=-1;

        void Draft(float w,float h)
        {
            if(displayedWave!=game.LastWaveFeedback){displayedWave=game.LastWaveFeedback;waveDetails=false;waveDetailsScroll=Vector2.zero;}
            if(clarityDeck!=game.Deck||clarityWave!=game.Wave)
            {clarityDeck=game.Deck;clarityWave=game.Wave;ClearLoadoutGesture();}
            if(game.TutorialActive)ExpandedDraft(w,h);else UnifiedDraft(w,h);
        }
        void SetManageBuild(bool open)
        {ClearLoadoutGesture();}
        void CheckpointHeader(float w,bool expanded)
        {
            Label(new Rect(32,23,270,18),"WALKING HARD  /  COMBAT PAUSED",11,muted,true);
            if(game.ExpeditionActive){Label(new Rect(320,23,310,18),"SALVAGE "+game.Salvage+"  ·  "+(game.ChapterActive?game.ChapterSparks+" SPARKS BANKED":game.SaveMessage),11,Accent,true);if(Button(new Rect(w-600,27,180,31),game.ChapterActive?"SAVE & OFFICE":"SAVE & MAIN MENU"))game.SuspendExpedition();}
            string phase=game.OpeningDraft?"OPENING PICK "+(game.OpeningPicks+1)+" / 2":"CHECKPOINT "+game.Wave.ToString("00");
            Label(new Rect(32,47,600,30),expanded?phase+" / YOUR BUILD":game.JourneyActive?"DISTRICT "+(game.District+1)+" / "+phase:phase,23,white,true);
            if(expanded&&game.LastWaveFeedback!=null&&Button(new Rect(w-397,30,171,31),"LAST WAVE DETAILS",waveDetails))
            {waveDetails=!waveDetails;detailItem=null;iconGuide=false;}
            if(Button(new Rect(w-210,27,178,39),expanded?"CLOSE BUILD":"MANAGE BUILD",expanded))
            {SetManageBuild(!expanded);GUIUtility.ExitGUI();}
        }
        static string ItemType(ItemDefinition def)=>def.kind==ItemKind.Basic||def.kind==ItemKind.Weapon?"WEAPON":LoadoutCatalog.IsArmor(def.kind)||def.kind==ItemKind.Rig?"GEAR":def.kind.ToString().ToUpperInvariant();
        static string ItemRole(ItemDefinition def)=>def.kind==ItemKind.Weapon?(def.hands==2?"Two hands / weapon attack":"One hand / weapon attack"):def.kind==ItemKind.Basic?"Uses your equipped weapon":def.kind==ItemKind.Ability?"Action in your sequence":def.kind==ItemKind.Augment?(def.module.family<0?"Modifies an ability":"Modifies a weapon"):def.kind==ItemKind.Ultimate?"Separate recharge timer":(def.kind==ItemKind.Rig?"Rig":def.kind.ToString())+" / passive effect";
        string RewardStats(ItemDefinition def,int rank)
        {
            if(def.kind==ItemKind.Augment)return def.module.id==AugmentId.Livewire?"USES MARK STRENGTH":"SCALES WITH HOST";
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
            var previousColor=GUI.color;if(d.RewardClaimed&&!selected)GUI.color=new Color(1,1,1,.38f);
            Color edge=def.kind==ItemKind.Augment?ModuleEdge:RarityColor(offer.rank);
            if(UnifiedCheckpoint)
            {
                TabletCell(r,new Color(.12f,.17f,.21f),edge,selected,r.Contains(Pointer));
                Box(new Rect(r.x+7,r.y+6,r.width-14,32),new Color(.03f,.06f,.08f,.40f));
                if(selected)Outline(new Rect(r.x-1,r.y-1,r.width+2,r.height+2),Accent,6,2);
            }
            else Plate(r,selected?new Color(.035f,.13f,.17f,.98f):ink,edge,9,selected?3:2);
            TypeIcon(new Rect(r.x+14,r.y+13,21,21),def.kind,white);
            bool discovery=d.WeaponDiscovery&&index==2;
            Label(new Rect(r.x+43,r.y+13,r.width-150,24),(selected?"✓ ":"")+(discovery?"WEAPON DISCOVERY":ItemType(def)),discovery?11:compact?15:18,discovery?Accent:white,true);
            if(def.kind!=ItemKind.Augment)
            {
                Rect badge=new Rect(r.xMax-101,r.y+12,87,23);Plate(badge,ink,RarityColor(offer.rank),4);
                Label(badge,ItemRarity.Names[offer.rank].ToUpperInvariant(),10,RarityColor(offer.rank),true,TextAnchor.MiddleCenter);
            }
            float size=compact?59:78,top=compact?45:54,left=r.x+size+30;
            if(UnifiedCheckpoint)TabletArtWell(new Rect(r.x+12,r.y+top-2,size+4,size+4));
            CategoryArt(new Rect(r.x+14,r.y+top,size,size),def);
            Label(new Rect(left,r.y+top,r.xMax-left-14,compact?36:48),offer.Name,compact?17:22,white,true);
            Label(new Rect(left,r.y+top+(compact?38:52),r.xMax-left-14,19),ItemRole(def),10,UnifiedCheckpoint?TabletText:muted);
            EffectLine(new Rect(r.x+14,r.y+(compact?123:145),r.width-28,compact?18:25),new LoadoutItem(def,offer.rank),compact?11:14);
            bool fusion=def.kind!=ItemKind.Augment&&offer.rank<5&&d.Copies(def,offer.rank)>0;
            var forecast=!d.RewardClaimed&&fusion?d.PreviewChange(offer,null,PlaceKind.Inventory):null;
            bool upgrades=forecast?.fusionRanks.Count>0;int resultRank=upgrades?forecast.fusionRanks.Last():offer.rank;
            string bottom=d.RewardClaimed?(selected?"TAKEN":"PASSED"):upgrades?"↑ FUSES → "+ItemRarity.Names[resultRank].ToUpperInvariant():fusion?FusionSummary(def,offer.rank,offer.copies):RewardStats(def,offer.rank);
            if(upgrades){float pulse=game.ReducedMotion?1:.8f+.2f*Mathf.Sin(Time.unscaledTime*3);Outline(new Rect(r.x-2,r.y-2,r.width+4,r.height+4),RarityColor(resultRank)*new Color(1,1,1,pulse),9,2);}
            Rect bottomRect=compact?new Rect(left,r.y+103,r.xMax-left-14,16):new Rect(r.x+14,r.yMax-26,r.width-28,19);
            Label(bottomRect,bottom,compact?10:11,upgrades?RarityColor(resultRank):fusion?RarityColor(offer.rank):UnifiedCheckpoint?TabletText:muted,fusion);
            ItemTooltip(r,new LoadoutItem(def,offer.rank),offer);GUI.color=previousColor;
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
    }
}
