using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        LoadoutItem hoveredItem;
        void ItemTooltip(Rect r,LoadoutItem item)
        {
            if(item!=null&&r.Contains(Event.current.mousePosition))hoveredItem=item;
        }
        void DrawItemTooltip(float w,float h)
        {
            var item=hoveredItem;if(item==null||dragging)return;
            var shown=item.def.kind==ItemKind.Basic?game.Deck.WeaponFor(item):item;
            bool offensive=shown.def.kind==ItemKind.Weapon||shown.def.kind==ItemKind.Ability||shown.def.kind==ItemKind.Ultimate;
            string stats=offensive?CombatStats(item):ItemStats(shown.def,shown.rank);
            string body=(offensive?AttackDescription(item):shown.def.description)+"\n\n"+stats;
            if(shown.def.kind==ItemKind.Weapon)
            {
                body+="\n"+(shown.def.hands==2?"Two-handed: both mounts; one attack across the locked pair.":"Adjacent matching basic attacks fire together; independent targets.");
                int magazine=LoadoutCatalog.Magazine(shown);if(magazine>0)body+="\n"+magazine+" rounds / 1.2s reload when empty.";
            }
            if(shown.def.kind==ItemKind.Ultimate)body+="\n"+(shown.def.id=="tempest"?"16":"12")+" combat seconds to recharge. Queues after the current action; U toggles Auto/Manual, F queues when ready.";
            string rules=EffectRules(shown);if(rules.Length>0)body+="\n"+rules;
            if(offensive)body+="\n\n"+SynergyText(shown.def);
            body+="\n\nRARITY EFFECTS\n"+shown.def.upgrade;
            if(offensive)body+="\n\nKills: "+game.ActionKills(item)+" (follows this item)";
            body+="\n"+(shown.rank==5?"Legendary: maximum rarity.":game.Deck.Copies(shown.def,shown.rank)+" / 3 identical "+ItemRarity.Names[shown.rank]+" copies toward "+ItemRarity.Names[shown.rank+1]+".");
            float width=362,height=Style(13).CalcHeight(new GUIContent(body),width-32)+108;
            Vector2 mouse=Event.current.mousePosition;float x=mouse.x+18,y=mouse.y+18;
            if(x+width>w-12)x=mouse.x-width-18;
            if(y+height>h-12)y=mouse.y-height-18;
            Rect r=new Rect(Mathf.Clamp(x,12,w-width-12),Mathf.Clamp(y,12,Mathf.Max(12,h-height-12)),width,height);
            Plate(r,new Color(.012f,.033f,.048f,.995f),RarityColor(shown.rank),10,2);
            ActionGlyph(new Rect(r.x+14,r.y+13,33,33),shown.def,TypeColor(shown.def.kind));
            Label(new Rect(r.x+58,r.y+14,width-73,24),shown.def.name,18,white,true);
            Label(new Rect(r.x+58,r.y+42,width-73,17),shown.def.kind.ToString().ToUpperInvariant()+" / "+ItemRarity.Names[shown.rank].ToUpperInvariant(),11,RarityColor(shown.rank),true);
            Stroke(new Vector2(r.x+16,r.y+67),new Vector2(r.xMax-16,r.y+67),line,1);
            Label(new Rect(r.x+16,r.y+77,width-32,height-88),body,13,white);
        }
        static readonly Color Violet=new Color(.78f,.64f,1),Blue=new Color(.48f,.74f,1);
        static Color RarityColor(int rarity)=>rarity==5?new Color(1,.61f,.22f):rarity==4?new Color(.78f,.48f,1):rarity==3?new Color(.36f,.66f,1):rarity==2?new Color(.45f,.87f,.43f):new Color(.79f,.82f,.84f);
        static Color TypeColor(ItemKind kind)=>kind==ItemKind.Weapon||kind==ItemKind.Basic?Gold:kind==ItemKind.Ultimate?Violet:kind==ItemKind.Rig||LoadoutCatalog.IsArmor(kind)?Blue:Accent;
        static void Border(Rect r,Color color,float thickness=2)
        {Box(new Rect(r.x,r.y,r.width,thickness),color);Box(new Rect(r.x,r.yMax-thickness,r.width,thickness),color);Box(new Rect(r.x,r.y,thickness,r.height),color);Box(new Rect(r.xMax-thickness,r.y,thickness,r.height),color);}
        static void LockIcon(Vector2 center,Color color)
        {
            Ring(center+Vector2.up*4,5,color);Box(new Rect(center.x-7,center.y-2,14,10),color);
            Box(new Rect(center.x-1,center.y+1,2,5),new Color(.03f,.06f,.07f));
        }
        static void TypeIcon(Rect r,ItemKind kind,Color color,bool upgrade=false)
        {
            Vector2 c=r.center;
            if(LoadoutCatalog.IsArmor(kind)){ActionGlyph(r,new ItemDefinition("icon","",kind,""),color);return;}
            if(kind==ItemKind.Weapon||kind==ItemKind.Basic)
            {Stroke(c+new Vector2(-6,7),c+new Vector2(7,-7),color,3);Stroke(c+new Vector2(-8,1),c+new Vector2(-1,8),color,2);}
            else if(kind==ItemKind.Ability)
            {Ring(c,8,color);Stroke(c+new Vector2(2,-6),c+new Vector2(-3,1),color);Stroke(c+new Vector2(-3,1),c+new Vector2(3,0),color);Stroke(c+new Vector2(3,0),c+new Vector2(-2,6),color);}
            else if(kind==ItemKind.Ultimate)
            {for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Stroke(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*3,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*9,color,2);}Ring(c,3,color);}
            else
            {Border(new Rect(c.x-5,c.y-5,10,10),color);for(int i=-1;i<=1;i++){Stroke(c+new Vector2(i*4,-9),c+new Vector2(i*4,-5),color);Stroke(c+new Vector2(i*4,5),c+new Vector2(i*4,9),color);}}
            if(upgrade){Stroke(c+new Vector2(6,5),c+new Vector2(10,1),color);Stroke(c+new Vector2(10,1),c+new Vector2(14,5),color);}
        }
        string OfferType(RewardOffer offer)
        {
            bool owned=offer.owned>0;
            return offer.def.kind.ToString().ToUpperInvariant()+(owned?(offer.rank==5?" COPY":" UPGRADE"):"");
        }
        string EffectRules(LoadoutItem item)
        {
            var def=item.def;
            if(def.kind==ItemKind.Ability)
            {
                switch(def.ability.id)
                {
                    case AbilityId.SteelJab:return "One target; 0.45s stagger.";
                    case AbilityId.Breaker:return "Forward sweep; 1s stagger to each target.";
                    case AbilityId.ForcePush:return "Area knockback + 0.65s stagger. Also launches fallen bodies.";
                    case AbilityId.GravityWell:return "Area pull to a point ahead of you; 0.65s stagger.";
                    case AbilityId.MassDriver:return "Launches a target. Cascades share 6 impacts, up to 2 extra hops, with declining damage.";
                    case AbilityId.ArcBolt:return "Up to 3 targets. Secondary hits: "+(18*ItemRarity.Power[item.rank]*game.Deck.Damage).ToString("0.#")+" damage. Stagger bonus: +"+(12*ItemRarity.Power[item.rank]*game.Deck.Damage).ToString("0.#")+" per hit.";
                }
            }
            if(def.id=="tempest")return "Up to 7 targets. +"+(12*ItemRarity.Power[item.rank]*game.Deck.Damage).ToString("0.#")+" damage against staggered targets.";
            if(def.kind==ItemKind.Weapon&&def.weapon==WeaponKind.Fists)return item.rank>=2?"Two hits per action, each at the listed damage. Brief stagger.":"One hit per action; 0.5s stagger.";
            if(def.kind==ItemKind.Weapon&&def.weapon==WeaponKind.Rifle)return "Pierces up to "+Mathf.Min(item.rank+1,4)+" aligned targets.";
            if(def.kind==ItemKind.Weapon&&def.weapon==WeaponKind.Pistol&&item.rank>=2)return "Up to "+Mathf.Min(item.rank-1,2)+" ricochets, each at 65% damage.";
            return "";
        }
        string UpgradePreview(RewardOffer offer)
        {
            int owned=offer.owned,total=owned+offer.copies;
            if(offer.rank==5)return "LEGENDARY · MAX RARITY / "+offer.def.upgrade;
            return total>=3?"FUSES NOW → "+ItemRarity.Names[offer.rank+1].ToUpperInvariant()+" / "+ItemRarity.StatBonus(offer.rank+1):total+"/3 toward "+ItemRarity.Names[offer.rank+1]+" / identical item + rarity";
        }
        string ItemStats(ItemDefinition def,int rarity)
        {
            float power=ItemRarity.Power[rarity];
            if(def.kind==ItemKind.Helmet)return "+"+(8*power).ToString("0.#")+"% DAMAGE / +"+(6*power).ToString("0.#")+" VS STAGGER";
            if(def.kind==ItemKind.Armor)return "+"+(40*power).ToString("0")+" INTEGRITY / "+(12*power).ToString("0.#")+"% RESIST";
            if(def.kind==ItemKind.Boots)return "+"+(8*power).ToString("0.#")+"% SPEED / "+(.4f*power).ToString("0.##")+" HP/s";
            if(def.kind==ItemKind.Weapon)
            {var card=LoadoutCatalog.Basic(new LoadoutItem(def,rarity));return (def.hands==2?"2H":def.weapon==WeaponKind.Fists?"UNARMED":"1H")+" / "+card.damage.ToString("0.#")+" DMG / "+card.range.ToString("0.#")+"m / "+card.duration.ToString("0.00")+"s";}
            if(def.kind==ItemKind.Ability||def.kind==ItemKind.Ultimate)return (def.ability.damage*power).ToString("0")+" DAMAGE / "+(def.kind==ItemKind.Ultimate?(def.id=="tempest"?"16":"12")+"s RECHARGE":def.ability.duration.ToString("0.00")+"s ACTION");
            return def.id=="overclock"?"+"+(20*power).ToString("0")+"% SEQUENCE SPEED":def.id=="amplifier"?"+"+(25*power).ToString("0")+"% DIRECT DAMAGE":"+"+(14*power).ToString("0")+" COLLISION DAMAGE";
        }
        string AttackDescription(LoadoutItem item)
        {
            if(item==null)return "Empty position; skipped in combat.";
            if(item.def.kind==ItemKind.Basic)item=game.Deck.WeaponFor(item);
            if(item.def.kind==ItemKind.Weapon)
            {
                switch(item.def.weapon)
                {
                    case WeaponKind.Sword:return item.rank>=3?"Wide cuts; sever and launch bodies.":"Sweeping cuts; lethal hits sever.";
                    case WeaponKind.Hammer:return item.rank>=3?"Crush and launch up to three targets.":item.rank==2?"Crush, launch and stagger the crowd.":"Crush one target into a collision chain.";
                    case WeaponKind.Pistol:return item.rank>=2?"Ranged shots with ricochets.":"Rapid shots; six-round magazine.";
                    case WeaponKind.Rifle:return "Pierce aligned targets; four rounds.";
                    default:return item.rank>=3?"Double strike + stagger shockwave.":item.rank==2?"Two quick hits; brief stagger.":"Quick close strike; brief stagger.";
                }
            }
            if(item.def.id=="singularity")return "Blast nearby bodies into collision chains.";
            if(item.def.id=="tempest")return "Chain seven targets; bonus vs stagger.";
            return item.def.description;
        }
        string CombatStats(LoadoutItem item)
        {
            if(item==null)return "";
            var actual=item.def.kind==ItemKind.Basic?game.Deck.WeaponFor(item):item;
            var card=actual.def.kind==ItemKind.Weapon?LoadoutCatalog.Basic(actual):actual.def.ability;
            if(card==null)return ItemStats(actual.def,actual.rank);
            float damage=card.damage*(actual.def.kind==ItemKind.Weapon?1:ItemRarity.Power[actual.rank])*game.Deck.Damage;
            float duration=card.duration/(actual.def.kind==ItemKind.Ultimate?1:game.Deck.Speed);
            return damage.ToString("0.#")+" DMG  /  "+card.range.ToString("0.#")+"m  /  "+duration.ToString("0.00")+"s";
        }
        static int EffectFor(ItemDefinition def)
        {
            if(def.kind==ItemKind.Weapon)return def.weapon==WeaponKind.Sword?5:def.weapon==WeaponKind.Hammer?1:def.weapon==WeaponKind.Rifle?6:0;
            if(def.kind==ItemKind.Rig)return 2;
            return def.ability?.id==AbilityId.GravityWell?3:def.ability?.id==AbilityId.ArcBolt?4:def.ability?.id==AbilityId.MassDriver||def.kind==ItemKind.Ultimate?1:0;
        }
        string SynergyText(ItemDefinition def)
        {
            if(def.ability!=null)return def.ability.interaction;
            if(def.kind==ItemKind.Rig||LoadoutCatalog.IsArmor(def.kind))return def.description;
            switch(def.weapon)
            {
                case WeaponKind.Sword:return "CREATES cut fragments · USES clustered targets";
                case WeaponKind.Hammer:return "CREATES launch + collisions · USES nearby bodies";
                case WeaponKind.Pistol:return "CREATES stagger · UNCOMMON+ ricochet";
                case WeaponKind.Rifle:return "CREATES piercing hits · USES aligned targets";
                default:return "CREATES stagger · FEEDS Arc Bolt";
            }
        }
        void GroupOutline(float x,float y,float width,float gap,float height)
        {
            if(!game.Deck.TwoHanded)return;
            int a=System.Array.IndexOf(game.Deck.Slots,game.Deck.Basics[0]);
            Rect r=new Rect(x+a*(width+gap)-4,y-4,width*2+gap+8,height+8);
            Outline(r,Gold,9,2);var c=new Vector2(r.x+width+gap/2+4,r.y+height/2);
            Box(new Rect(c.x-13,c.y-13,26,26),ink);LockIcon(c,Gold);
        }
    }
}
