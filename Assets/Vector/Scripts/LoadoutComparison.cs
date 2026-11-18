using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        LoadoutItem RegionItem(AbilityDeck d,DropRegion r)
        {
            if(r.place==PlaceKind.Sequence){var item=d.Slots[r.index];return item?.def.kind==ItemKind.Basic?d.WeaponFor(item):item;}
            if(r.place==PlaceKind.Weapon)return d.WeaponFor(d.Basics[r.index]);
            if(r.place==PlaceKind.Helmet)return d.Armor[0];if(r.place==PlaceKind.Armor)return d.Armor[1];if(r.place==PlaceKind.Boots)return d.Armor[2];
            return r.place==PlaceKind.Rig?d.Rig:r.place==PlaceKind.Ultimate?d.Ultimate:r.item;
        }
        void DrawWorkbenchGuidance()
        {
            var p=loadoutHover;if(p?.Def==null||p.inspect||iconGuide||waveDetails)return;
            var projection=p.projection==null?null:!p.explicitTarget&&p.offer!=null?game.Deck.PreviewChange(p.offer,null,PlaceKind.Inventory):p.projection;
            foreach(var r in dropRegions.Where(r=>r.offer<0))
            {
                bool fits=p.offer!=null?OfferFitsRegion(p.offerIndex,r):StoredFitsRegion(p.source,r);
                if(fits)DestinationGlow(r.rect,Accent,!p.explicitTarget||r==p.target);
                if(p.projection==null||r.socket)continue;
                var before=RegionItem(game.Deck,r);var after=r.place==PlaceKind.Inventory?projection.Copy(r.item):RegionItem(projection.after,r);
                bool consumed=before!=null&&r.place==PlaceKind.Inventory&&!projection.after.Items.Contains(after);
                bool upgraded=before!=null&&after!=null&&before.def==after.def&&after.rank>before.rank;
                bool changed=r.place!=PlaceKind.Inventory&&(before?.def!=after?.def||upgraded||before?.augment?.def!=after?.augment?.def);
                if(!p.explicitTarget&&!upgraded&&!consumed)continue;
                if(!changed&&!consumed)continue;
                Color c=upgraded?ItemEdge(after):consumed?ItemEdge(p.projection.incoming):Accent;
                Outline(r.rect,c,6,2.5f);
                string label=consumed?"FUSES":upgraded?"↑ "+ItemRarity.Names[after.rank].ToUpperInvariant():after==null?"→ EMPTY":after.def==LoadoutCatalog.Fists?"→ FISTS":"→ "+after.def.name.ToUpperInvariant();
                Rect badge=new Rect(r.rect.x+3,r.rect.yMax-16,r.rect.width-6,14);Box(badge,ink);Label(badge,label,r.rect.width<150?8:9,c,true,TextAnchor.MiddleCenter);
            }
            if(p.offerIndex>=0){var source=dropRegions.FirstOrDefault(r=>r.offer==p.offerIndex);if(source!=null)DestinationGlow(source.rect,Accent);}
            if(p.invalid&&p.target!=null)Outline(p.target.rect,Strong,6,2);
        }
        string ComparisonStats(LoadoutItem item)
        {
            if(item==null)return "No item equipped";
            if(item.def.kind==ItemKind.Basic)return "Weapon attack / moves with its sequence slot";
            return item.def.profile!=null||item.def.ability!=null?CombatStats(item):RewardStats(item.def,item.rank);
        }
        string ComparisonEffects(LoadoutItem item)
        {
            if(item==null)return "Empty slot.";
            if(item.def.kind==ItemKind.Augment)return item.def.description+"\n"+item.def.tradeoff;
            string text=AttackDescription(item),rules=EffectRules(item);
            if(rules.Length>0)text=rules.StartsWith(text,System.StringComparison.Ordinal)?rules:text+"\n"+rules;
            if(item.augment!=null)text+="\nSOCKET: "+item.augment.def.name+"\n"+AugmentCatalog.Values(item,DisplayShare(item));
            if(item.def.kind==ItemKind.Ultimate)text+="\nRecharge: "+LoadoutCatalog.Cooldown(item).ToString("0")+"s. Separate from sequence.";
            if(item.def.kind==ItemKind.Weapon){int mag=LoadoutCatalog.Magazine(item);if(mag>0)text+="\nMagazine: "+mag+" / reload: 1.2s before gear.";}
            return text;
        }
        void ComparisonIdentity(Rect r,LoadoutItem item,string title)
        {
            Label(new Rect(r.x,r.y,r.width,14),title,10,muted,true);
            if(item==null){Label(new Rect(r.x,r.y+22,r.width,23),"Empty slot",16,muted,true);return;}
            ActionGlyph(new Rect(r.x,r.y+23,32,32),item.def,white);
            Label(new Rect(r.x+40,r.y+20,r.width-40,35),item.def.name,14,white,true);
            Label(new Rect(r.x+40,r.y+58,r.width-40,16),item.def.kind==ItemKind.Augment?"AUGMENT":ItemRarity.Names[item.rank].ToUpperInvariant(),10,ItemEdge(item),true);
        }
        void DrawWorkbenchComparison()
        {
            var p=loadoutHover;var hit=dropRegions.FirstOrDefault(x=>x.rect.Contains(Pointer));
            bool retained=inlineComparisonVisible&&!dragging&&comparisonRect.Contains(Pointer);
            if(p?.Def==null||iconGuide||waveDetails||presentationHover==null&&(hit?.offer>=0||hit==null&&!retained))
            {inlineComparisonVisible=false;comparisonRect=Rect.zero;return;}
            if(!retained)
            {
                float width=Mathf.Min(300,portraitSpace.width),height=portraitSpace.height;
                Rect anchor=hit?.rect??portraitSpace;
                float y=Mathf.Clamp(anchor.y,portraitSpace.y,portraitSpace.yMax-height);
                Rect[] candidates={new Rect(anchor.xMax+8,y,width,height),new Rect(anchor.x-width-8,y,width,height),new Rect(portraitSpace.center.x-width/2,portraitSpace.y,width,height)};
                comparisonRect=candidates.FirstOrDefault(c=>c.x>=20&&c.xMax<=workbenchWidth-20&&!dropRegions.Any(d=>d.rect.Overlaps(c)));
                if(comparisonRect.width==0)comparisonRect=candidates[2];
            }
            inlineComparisonVisible=true;Rect r=comparisonRect;TabletMaterial(r,0,8);Outline(r,new Color(.45f,.62f,.65f),6);
            Color accent=p.projection?.fusionRanks.Count>0?RarityColor(p.projection.fusionRanks.Last()):Accent;
            bool unresolved=p.Def.kind==ItemKind.Augment&&p.projection==null;
            var shown=p.explicitTarget||p.inspect?p.after??p.source:p.source??new LoadoutItem(p.Def,p.offer.rank);shown=DisplayItem(shown);
            string heading=p.explicitTarget||p.inspect||p.invalid||unresolved?p.heading:Category(p.Def);
            Label(new Rect(r.x+12,r.y+10,r.width-24,27),heading,12,p.invalid?Strong:accent,true);
            bool pair=p.explicitTarget&&p.before!=null&&!p.inspect;
            float half=(r.width-34)/2;
            if(pair)
            {
                if(p.beforeSecond==null)ComparisonIdentity(new Rect(r.x+12,r.y+39,half,75),p.before,"NOW");
                else
                {
                    Label(new Rect(r.x+12,r.y+39,half,15),"NOW / BOTH HANDS",10,muted,true);
                    var hands=new[]{p.before,p.beforeSecond};
                    for(int i=0;i<2;i++){var hand=hands[i];float y=r.y+61+i*28;ActionGlyph(new Rect(r.x+12,y,22,22),hand.def,white);int size=11;while(size>8&&Style(size,true).CalcSize(new GUIContent(hand.def.name)).x>half-28)size--;Label(new Rect(r.x+40,y,half-28,15),hand.def.name,size,white,true);Label(new Rect(r.x+40,y+14,half-28,13),ItemRarity.Names[hand.rank],9,ItemEdge(hand));}
                }
                ComparisonIdentity(new Rect(r.x+half+22,r.y+39,half,75),p.after,"AFTER");
                Label(new Rect(r.center.x-10,r.y+63,20,20),"→",17,Accent,true,TextAnchor.MiddleCenter);
            }
            else ComparisonIdentity(new Rect(r.x+12,r.y+39,r.width-24,75),shown,"ITEM / "+(p.inspect?"CURRENT":p.offer!=null?"REWARD":"OWNED"));
            float top=r.y+120;
            if(p.projection?.fusionRanks.Count>0)
            {
                int rank=p.projection.fusionRanks.Last();Rect banner=new Rect(r.x+10,top,r.width-20,31);
                Plate(banner,new Color(.05f,.10f,.12f),accent,4,2);
                Label(banner,"↑ FUSION: "+ItemRarity.Names[p.offer.rank].ToUpperInvariant()+" → "+ItemRarity.Names[rank].ToUpperInvariant()+(p.projection.fusionRanks.Count>1?" / CHAIN":""),12,accent,true,TextAnchor.MiddleCenter);top+=38;
            }
            string fate=p.explicitTarget||p.invalid||unresolved?p.fate:p.inspect?"":p.Def.kind==ItemKind.Augment?"Select, then click a glowing host or Store.":"Select, then click a glowing slot or drag to equip.";
            if(p.offer!=null&&p.offer.def.kind!=ItemKind.Augment&&p.projection?.fusionRanks.Count==0)
                fate+=(fate.Length>0?"\n":"")+(p.offer.rank==5?"Legendary: maximum rarity.":FusionSummary(p.offer.def,p.offer.rank,p.offer.copies)+". Different rarities don't combine.");
            float fateHeight=Mathf.Min(77,Style(11).CalcHeight(new GUIContent(fate),r.width-24));
            Label(new Rect(r.x+12,top,r.width-24,fateHeight),fate,11,white);top+=fateHeight+9;
            string key=p.Def.id+":"+p.before?.def.id+":"+p.before?.rank+":"+p.after?.rank+":"+p.heading;
            if(key!=comparisonKey){comparisonKey=key;comparisonScroll=Vector2.zero;}
            Rect viewport=new Rect(r.x+12,top,r.width-24,r.yMax-top-24);
            if(viewport.height<25)return;
            string body=pair?"NOW  "+ComparisonStats(p.before)+(p.beforeSecond==null?"":"\nOTHER HAND  "+ComparisonStats(p.beforeSecond))+"\nAFTER  "+ComparisonStats(p.after)+"\n\nAFTER EFFECT\n"+ComparisonEffects(p.after)+"\n\nCURRENT EFFECT\n"+ComparisonEffects(p.before)+(p.beforeSecond==null?"":"\n\nOTHER HAND EFFECT\n"+ComparisonEffects(p.beforeSecond)):ComparisonStats(shown)+"\n\n"+ComparisonEffects(shown);
            string previous=p.offer==null&&p.source!=null?game.LastPassiveFeedback(DisplayItem(p.source)):"";if(previous.Length>0)body=previous+"\n\n"+body;
            if(p.Def.kind!=ItemKind.Augment){var fits=BuildSynergies.Find(game.Deck,p.Def,incomingItem:p.after).Take(2);foreach(var fit in fits)body+="\n\nWITH "+fit.partner.def.name.ToUpperInvariant()+"\n"+fit.text;body+="\n\nRARITY SCALING\n"+p.Def.upgrade;}
            float bodyHeight=Style(12).CalcHeight(new GUIContent(body),viewport.width-19)+8;
            comparisonScroll=GUI.BeginScrollView(viewport,comparisonScroll,new Rect(0,0,viewport.width-19,bodyHeight));
            Label(new Rect(0,0,viewport.width-19,bodyHeight),body,12,white);GUI.EndScrollView();
            Label(new Rect(r.x+12,r.yMax-19,r.width-24,15),"SCROLL HERE FOR EFFECTS, SOCKETS & SYNERGIES",9,muted);
        }
        void WorkbenchFusionResult(float age)
        {
            var f=shownFusion;var color=RarityColor(f.after);var d=game.Deck;
            foreach(var region in dropRegions.Where(x=>x.offer<0&&!x.socket))
            {var item=Host(region);if(item?.def==f.definition&&item.rank==f.after)Outline(region.rect,color,6,game.ReducedMotion?2:2+Mathf.Sin(age*5)*.6f);}
            bool taken=d.RewardClaimed&&d.Selected>=0&&d.Offers[d.Selected].def==f.definition;
            if(!taken&&!fusionPinned)return;
            float cardWidth=(workbenchWidth-60)/3;
            Rect r=taken?new Rect(20+d.Selected*(cardWidth+10),94,cardWidth,144):new Rect(portraitSpace.x,portraitSpace.y,portraitSpace.width,144);
            Plate(r,new Color(.012f,.04f,.052f,1),color,9,2);
            Label(new Rect(r.x+12,r.y+10,r.width-24,20),"↑ "+(f.after>f.before+1?"CHAIN UPGRADE: ":"UPGRADED: ")+ItemRarity.Names[f.before].ToUpperInvariant()+" → "+ItemRarity.Names[f.after].ToUpperInvariant(),12,color,true);
            float t=game.ReducedMotion?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.65f));
            for(int i=-1;i<=1;i++){float size=19+19*t;ActionGlyph(new Rect(r.x+51+i*22*(1-t)-size/2,r.y+62,size,size),f.definition,white);if(t==1)break;}
            Label(new Rect(r.x+101,r.y+42,r.width-113,38),f.definition.name,17,white,true);
            Label(new Rect(r.x+101,r.y+85,r.width-113,45),"BEFORE  "+RewardStats(f.definition,f.before)+"\nAFTER    "+RewardStats(f.definition,f.after),11,white);
        }
    }
}
