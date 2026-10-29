using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        static string Category(ItemDefinition d)
        {
            if(d.kind==ItemKind.Augment)return d.module.family<0?"ABILITY AUGMENT":"WEAPON AUGMENT";
            if(d.kind==ItemKind.Weapon)return (d.hands==2?"2H":"1H")+" WEAPON";
            return d.kind==ItemKind.Rig?"UTILITY RIG":d.kind.ToString().ToUpperInvariant();
        }
        static Color ItemEdge(LoadoutItem item)=>item.def.kind==ItemKind.Augment?ModuleEdge:RarityColor(item.rank);
        string OtherRarities(ItemDefinition def,int rank)
        {
            return string.Join(", ",Enumerable.Range(1,5).Where(r=>r!=rank&&game.Deck.Copies(def,r)>0).Select(r=>ItemRarity.Names[r]+" x"+game.Deck.Copies(def,r)));
        }
        string FusionSummary(ItemDefinition def,int rank,int incoming=0)
        {
            if(def.kind==ItemKind.Augment)return "NO FUSION / SCALES WITH HOST";
            if(rank==5)return "LEGENDARY / MAX RARITY";
            int count=game.Deck.Copies(def,rank),total=count+incoming;
            return incoming>0?(total>=3?"AUTO-FUSES TO "+ItemRarity.Names[rank+1].ToUpperInvariant():count+" OWNED + "+incoming+" = "+total+"/3 "+ItemRarity.Names[rank].ToUpperInvariant()):count+"/3 "+ItemRarity.Names[rank].ToUpperInvariant()+" / NEED "+Mathf.Max(0,3-count)+" MORE";
        }
        void FusionPips(Rect r,int owned,int incoming,Color color)
        {
            float width=(r.width-8)/3;
            for(int i=0;i<3;i++)
            {
                Rect p=new Rect(r.x+i*(width+4),r.y,width,r.height);
                Box(p,i<owned?color:ink);Border(p,i<owned+incoming?color:line,1);
                if(i>=owned&&i<owned+incoming)Label(p,"+",11,color,true,TextAnchor.MiddleCenter);
            }
        }
        void DrawFusionReadout(Rect r,LoadoutItem item,RewardOffer offer)
        {
            var d=item.def;int incoming=offer!=null&&!game.Deck.RewardClaimed?offer.copies:0;Color color=ItemEdge(item);
            Plate(r,new Color(.025f,.07f,.09f),line,5);
            if(offer!=null&&game.Deck.RewardClaimed)
            {
                bool claimed=game.Deck.Selected>=0&&game.Deck.Offers[game.Deck.Selected]==offer;
                bool fused=claimed&&game.Deck.Receipt.Contains("FUSED:");
                var result=fused?game.FusionHistory.LastOrDefault(f=>f.definition==d):null;
                Label(new Rect(r.x+10,r.y+8,r.width-20,17),claimed?(fused?"CLAIMED / FUSION COMPLETE":"CLAIMED / ADDED TO YOUR ITEMS"):"PASSED / NOT ADDED",11,claimed?Accent:muted,true);
                string message=result!=null?"Result: "+d.name+" / "+ItemRarity.Names[result.after]+".\nThe original copies were consumed automatically.":claimed?"Find this reward in your equipped items or storage.":"This reward was not selected. It did not add a copy toward fusion.";
                Label(new Rect(r.x+10,r.y+32,r.width-20,42),message,11,white);return;
            }
            if(d.kind==ItemKind.Augment)
            {
                Label(new Rect(r.x+10,r.y+8,r.width-20,17),"AUGMENT / DOES NOT FUSE",11,Violet,true);
                Label(new Rect(r.x+10,r.y+29,r.width-20,32),"Attach to a compatible weapon or ability.\nIts effect scales from that host's rarity.",11,white);return;
            }
            Label(new Rect(r.x+10,r.y+7,r.width-20,17),FusionSummary(d,item.rank,incoming),11,color,true);
            if(item.rank==5){Label(new Rect(r.x+10,r.y+29,r.width-20,32),"Already at maximum rarity. Extra copies remain separate items.",11,white);return;}
            int owned=game.Deck.Copies(d,item.rank);
            FusionPips(new Rect(r.x+10,r.y+30,70,13),owned,incoming,color);
            int spare=incoming>0&&owned+incoming>=3?(owned+incoming)%3:0;
            Label(new Rect(r.x+90,r.y+28,r.width-100,20),"3 "+ItemRarity.Names[item.rank]+" → 1 "+ItemRarity.Names[item.rank+1]+(spare>0?" / "+spare+" left":""),11,white,true);
            string others=OtherRarities(d,item.rank);
            Label(new Rect(r.x+10,r.y+51,r.width-20,31),others.Length>0?"NOT COUNTED: "+others+". Rarities must match.":"Exact same item + rarity. Equipped and stored copies count. Fusion happens automatically.",10,others.Length>0?Gold:muted);
        }
        void StorageIdentity(Rect r,LoadoutItem item)
        {
            float size=Mathf.Min(r.width-6,r.height-17);
            ActionGlyph(new Rect(r.center.x-size/2,r.y+2,size,size),item.def,white);
            Rect badge=new Rect(r.xMax-20,r.y+3,17,17);Box(badge,ink);TypeIcon(badge,item.def.kind,muted);
            Box(new Rect(r.x+3,r.yMax-16,r.width-6,13),ink);
            string text=item.def.kind==ItemKind.Augment?"MOD":ItemRarity.Names[item.rank].Substring(0,1)+" "+(item.rank==5?"MAX":game.Deck.Copies(item.def,item.rank)+"/3");
            Label(new Rect(r.x+5,r.yMax-16,r.width-23,13),text,9,ItemEdge(item),true);
            ItemTooltip(r,item);
        }
        void HighlightFusionMatches()
        {
            var item=dragItem??hoveredItem??selectedItem;var offer=hoveredOffer;
            if(item==null&&dragOffer>=0){offer=game.Deck.Offers[dragOffer];item=new LoadoutItem(offer.def,offer.rank);}
            if(item==null)return;
            if(item.def.kind==ItemKind.Basic)item=game.Deck.WeaponFor(item);
            if(item.def.kind==ItemKind.Augment||item.rank==5)return;
            foreach(var region in dropRegions)
            {
                if(region.offer>=0||region.socket)continue;var shown=Host(region);
                if(shown!=null&&shown.def.id==item.def.id&&shown.rank==item.rank)Outline(region.rect,Accent,6,2);
            }
        }
        string MoveFeedback(LoadoutItem item,DropRegion target)
        {
            var other=Host(target);
            if(other!=null&&other!=item&&other.def.id==item.def.id)
            {
                if(item.def.kind==ItemKind.Augment)return "Augments do not fuse. Attach to a host.";
                if(item.rank==5)return "Legendary is maximum rarity. No fusion.";
                if(item.rank!=other.rank)return "RARITY MISMATCH. Need 3 matching "+ItemRarity.Names[item.rank]+" copies.";
                return FusionSummary(item.def,item.rank)+". Fusion is automatic.";
            }
            return "";
        }
    }
}
