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
            foreach(var r in dropRegions.Where(r=>r.offer<0))
            {
                bool fits=p.offer!=null?OfferFitsRegion(p.offerIndex,r):StoredFitsRegion(p.source,r);
                if(fits)DestinationGlow(r.rect,Accent,r==p.target);
                if(p.projection==null||r.socket)continue;
                var before=RegionItem(game.Deck,r);var after=r.place==PlaceKind.Inventory?p.projection.Copy(r.item):RegionItem(p.projection.after,r);
                bool consumed=before!=null&&r.place==PlaceKind.Inventory&&!p.projection.after.Items.Contains(after);
                bool upgraded=before!=null&&after!=null&&before.def==after.def&&after.rank>before.rank;
                bool changed=r.place!=PlaceKind.Inventory&&(before?.def!=after?.def||upgraded||before?.augment?.def!=after?.augment?.def);
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
            if(rules.Length>0)text+="\n"+rules;
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
            Rect r=comparisonRect;Panel(r);var p=loadoutHover;
            if(p?.Def==null)
            {
                Label(new Rect(r.x+14,r.y+12,r.width-28,25),"WHAT WILL CHANGE?",14,white,true);
                Label(new Rect(r.x+14,r.y+52,r.width-28,79),"Hover a reward or stored item.\n\nIts matching slots light up, and its before / after comparison appears here.",14,muted);
                Label(new Rect(r.x+14,r.y+151,r.width-28,75),"1  Select an item.\n2  Click a glowing slot or drag it there.\n3  Continue when your build is ready.",13,white);
                Label(new Rect(r.x+14,r.yMax-53,r.width-28,43),"Three identical copies of the same rarity fuse automatically. Replaced gear stays in storage.",11,muted);return;
            }
            Color accent=p.projection?.fusionRanks.Count>0?RarityColor(p.projection.fusionRanks.Last()):Accent;
            Label(new Rect(r.x+12,r.y+10,r.width-24,27),p.heading,12,p.invalid?Strong:accent,true);
            bool pair=p.before!=null&&!p.inspect;
            float half=(r.width-34)/2;
            if(pair)
            {
                if(p.beforeSecond==null)ComparisonIdentity(new Rect(r.x+12,r.y+39,half,75),p.before,"NOW");
                else
                {
                    Label(new Rect(r.x+12,r.y+39,half,15),"NOW / BOTH HANDS",10,muted,true);
                    var hands=new[]{p.before,p.beforeSecond};
                    for(int i=0;i<2;i++){var hand=hands[i];float y=r.y+61+i*28;ActionGlyph(new Rect(r.x+12,y,22,22),hand.def,white);Label(new Rect(r.x+40,y,half-28,15),hand.def.name,11,white,true);Label(new Rect(r.x+40,y+14,half-28,13),ItemRarity.Names[hand.rank],9,ItemEdge(hand));}
                }
                ComparisonIdentity(new Rect(r.x+half+22,r.y+39,half,75),p.after,"AFTER");
                Label(new Rect(r.center.x-10,r.y+63,20,20),"→",17,Accent,true,TextAnchor.MiddleCenter);
            }
            else ComparisonIdentity(new Rect(r.x+12,r.y+39,r.width-24,75),p.after??p.source,"ITEM / "+(p.inspect?"CURRENT":p.offer!=null?"REWARD":"OWNED"));
            float top=r.y+120;
            if(p.projection?.fusionRanks.Count>0)
            {
                int rank=p.projection.fusionRanks.Last();Rect banner=new Rect(r.x+10,top,r.width-20,31);
                Plate(banner,new Color(.05f,.10f,.12f),accent,4,2);
                Label(banner,"↑ FUSION: "+ItemRarity.Names[p.offer.rank].ToUpperInvariant()+" → "+ItemRarity.Names[rank].ToUpperInvariant()+(p.projection.fusionRanks.Count>1?" / CHAIN":""),12,accent,true,TextAnchor.MiddleCenter);top+=38;
            }
            string fate=p.fate;
            if(p.offer!=null&&p.offer.def.kind!=ItemKind.Augment&&p.projection?.fusionRanks.Count==0)
                fate+=(fate.Length>0?"\n":"")+(p.offer.rank==5?"Legendary: maximum rarity.":FusionSummary(p.offer.def,p.offer.rank,p.offer.copies)+". Different rarities don't combine.");
            float fateHeight=Mathf.Min(77,Style(11).CalcHeight(new GUIContent(fate),r.width-24));
            Label(new Rect(r.x+12,top,r.width-24,fateHeight),fate,11,white);top+=fateHeight+9;
            string key=p.Def.id+":"+p.before?.def.id+":"+p.before?.rank+":"+p.after?.rank+":"+p.heading;
            if(key!=comparisonKey){comparisonKey=key;comparisonScroll=Vector2.zero;}
            Rect viewport=new Rect(r.x+12,top,r.width-24,r.yMax-top-24);
            if(viewport.height<25)return;
            string body=pair?"NOW  "+ComparisonStats(p.before)+(p.beforeSecond==null?"":"\nOTHER HAND  "+ComparisonStats(p.beforeSecond))+"\nAFTER  "+ComparisonStats(p.after)+"\n\nAFTER EFFECT\n"+ComparisonEffects(p.after)+"\n\nCURRENT EFFECT\n"+ComparisonEffects(p.before)+(p.beforeSecond==null?"":"\n\nOTHER HAND EFFECT\n"+ComparisonEffects(p.beforeSecond)):ComparisonStats(p.after??p.source)+"\n\n"+ComparisonEffects(p.after??p.source);
            if(p.Def.kind!=ItemKind.Augment){var fits=BuildSynergies.Find(game.Deck,p.Def,incomingItem:p.after).Take(2);foreach(var fit in fits)body+="\n\nWITH "+fit.partner.def.name.ToUpperInvariant()+"\n"+fit.text;body+="\n\nRARITY SCALING\n"+p.Def.upgrade;}
            float bodyHeight=Style(12).CalcHeight(new GUIContent(body),viewport.width-19)+8;
            comparisonScroll=GUI.BeginScrollView(viewport,comparisonScroll,new Rect(0,0,viewport.width-19,bodyHeight));
            Label(new Rect(0,0,viewport.width-19,bodyHeight),body,12,white);GUI.EndScrollView();
            Label(new Rect(r.x+12,r.yMax-19,r.width-24,15),"SCROLL HERE FOR EFFECTS, SOCKETS & SYNERGIES",9,muted);
        }
        void WorkbenchFusionResult(float age)
        {
            var f=shownFusion;Rect r=comparisonRect;var color=RarityColor(f.after);
            // The result occupies the comparison area, never the character, storage,
            // timeline, or their hit regions. After the brief merge, a hover takes priority.
            if(loadoutHover?.Def!=null&&!fusionPinned&&age>1.15f)return;
            Plate(r,new Color(.012f,.04f,.052f,1),color,10,2);
            Label(new Rect(r.x+14,r.y+12,r.width-28,24),"↑ RARITY UPGRADED",18,color,true);
            Label(new Rect(r.x+14,r.y+43,r.width-28,27),f.definition.name,20,white,true);
            float t=game.ReducedMotion?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.65f));
            for(int i=-1;i<=1;i++){float size=30+14*t;ActionGlyph(new Rect(r.center.x+i*75*(1-t)-size/2,r.y+80,size,size),f.definition,white);if(t==1)break;}
            Label(new Rect(r.x+14,r.y+135,r.width-28,25),(f.after>f.before+1?"CHAIN: ":"3 ")+ItemRarity.Names[f.before].ToUpperInvariant()+(f.after>f.before+1?" → ":" → 1 ")+ItemRarity.Names[f.after].ToUpperInvariant(),15,color,true);
            Label(new Rect(r.x+14,r.y+173,r.width-28,42),"BEFORE  "+ItemStats(f.definition,f.before)+"\nAFTER    "+ItemStats(f.definition,f.after),12,white);
            Label(new Rect(r.x+14,r.y+225,r.width-28,55),f.definition.upgrade,12,muted);
            foreach(var region in dropRegions.Where(x=>x.offer<0&&!x.socket))
            {var item=Host(region);if(item?.def==f.definition&&item.rank==f.after)Outline(region.rect,color,6,game.ReducedMotion?2:2+Mathf.Sin(age*5)*.6f);}
        }
    }
}
