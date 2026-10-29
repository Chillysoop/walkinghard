using UnityEngine;
using System.Linq;

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
            if(shown.augment!=null)body+="\nBase item: "+shown.def.description;
            if(shown.def.kind==ItemKind.Weapon)
            {
                body+="\n"+(shown.def.hands==2?"Two-handed: both mounts; one attack across the locked pair.":"Adjacent matching basic attacks fire together; independent targets.");
                int magazine=LoadoutCatalog.Magazine(shown);if(magazine>0)body+="\n"+magazine+" rounds / "+(shown.HasAugment(AugmentId.BreachLoad)?"1.5":"1.2")+"s reload before Loader reduction.";
            }
            if(shown.def.kind==ItemKind.Ultimate)body+="\n"+LoadoutCatalog.Cooldown(shown).ToString("0")+" combat seconds to recharge. Queues after the current action; U toggles Auto/Manual, F queues when ready.";
            string rules=EffectRules(shown);if(rules.Length>0)body+="\n"+rules;
            if(offensive)body+="\n\n"+SynergyText(shown.def);
            body+="\n\nRARITY EFFECTS\n"+shown.def.upgrade;if(shown.Augmentable)body+="\n\n"+AugmentCatalog.Values(shown,DisplayShare(shown));
            if(offensive)body+="\n\nKills: "+game.ActionKills(item)+" (follows this item)";
            body+="\n"+(shown.rank==5?"Legendary: maximum rarity.":game.Deck.Copies(shown.def,shown.rank)+" / 3 identical "+ItemRarity.Names[shown.rank]+" copies toward "+ItemRarity.Names[shown.rank+1]+".");
            if(shown.def.kind==ItemKind.Augment)body=ModuleDetails(shown);
            float width=440,height=Style(12).CalcHeight(new GUIContent(body),width-32)+108;
            Vector2 mouse=Event.current.mousePosition;float x=mouse.x+18,y=mouse.y+18;
            if(x+width>w-12)x=mouse.x-width-18;
            if(y+height>h-12)y=mouse.y-height-18;
            Rect r=new Rect(Mathf.Clamp(x,12,w-width-12),Mathf.Clamp(y,12,Mathf.Max(12,h-height-12)),width,height);
            Plate(r,new Color(.012f,.033f,.048f,.995f),RarityColor(shown.rank),10,2);
            ActionGlyph(new Rect(r.x+14,r.y+13,33,33),shown.def,TypeColor(shown.def.kind));
            Label(new Rect(r.x+58,r.y+14,width-73,24),shown.def.name,18,white,true);
            Label(new Rect(r.x+58,r.y+42,width-73,17),shown.def.kind.ToString().ToUpperInvariant()+" / "+(shown.def.kind==ItemKind.Augment?"HOST SCALED":ItemRarity.Names[shown.rank].ToUpperInvariant()),11,RarityColor(shown.rank),true);
            Stroke(new Vector2(r.x+16,r.y+67),new Vector2(r.xMax-16,r.y+67),line,1);
            Label(new Rect(r.x+16,r.y+77,width-32,height-88),body,12,white);
        }
        static readonly Color Violet=new Color(.78f,.64f,1),Blue=new Color(.48f,.74f,1);
        static Color RarityColor(int rarity)=>rarity==5?new Color(1,.61f,.22f):rarity==4?new Color(.78f,.48f,1):rarity==3?new Color(.36f,.66f,1):rarity==2?new Color(.45f,.87f,.43f):new Color(.79f,.82f,.84f);
        static Color TypeColor(ItemKind kind)=>kind==ItemKind.Augment?Violet:kind==ItemKind.Weapon||kind==ItemKind.Basic?Gold:kind==ItemKind.Ultimate?Violet:kind==ItemKind.Rig||LoadoutCatalog.IsArmor(kind)?Blue:Accent;
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
            if(item.def.profile!=null){var p=item.def.profile;return "Native: "+p.style+" / "+p.force+" force. "+(item.def.weapon==WeaponKind.Rifle?"Pierces up to "+Mathf.Min(item.rank+1,4)+" aligned targets.":item.def.weapon==WeaponKind.Hammer?"Launch cap "+(item.HasAugment(AugmentId.Piledriver)?1:p.style==WeaponStyle.Impact&&item.rank>=3?3:1)+"; shared 6-hit / 2-hop cascade.":"");}
            float scale=LoadoutCatalog.EffectScale(item);var effect=item.def.effect;
            if(item.def.signature!=Signature.None)return GearEffectRules(item)+"\nLimit: "+item.def.tradeoff;
            if(item.def.kind==ItemKind.Ability){switch(item.def.ability.id){
                case AbilityId.SteelJab:return (.5f*scale).ToString("0.00")+"s interrupt; attacking targets take +"+((.25f+.05f*(item.rank-1))*100).ToString("0")+"% damage.";
                case AbilityId.Breaker:return (1*scale).ToString("0.00")+"s stagger; "+(100+5*(item.rank-1))+"° half arc before augments.";
                case AbilityId.ForcePush:return (11*scale).ToString("0.0")+" outward force; living targets and up to six fallen bodies.";
                case AbilityId.MassDriver:return "One launch; "+scale.ToString("0.00")+"× impulse; six shared collision hits, two extra hops.";
                case AbilityId.GravityWell:return "Gather point 2.8m ahead; pull capped at "+(10*scale).ToString("0.0")+" impulse.";
                case AbilityId.VectorTether:return "Forward corridor "+LoadoutCatalog.Action(item).range.ToString("0.0")+"m long × "+(2*(1.8f+.1f*(item.rank-1))).ToString("0.0")+"m wide; pull to its center line.";
                case AbilityId.ArcBolt:return (3+(item.rank>=4?1:0))+" targets; consumes Conductive once; stagger adds "+(8*scale).ToString("0.0")+" damage.";
                case AbilityId.ConductiveBrand:return "Up to five marks; "+(4*scale).ToString("0.0")+"s duration; "+(10*ItemRarity.Power[item.rank]).ToString("0.0")+" conditional discharge damage.";
                case AbilityId.ReapingCut:return "Weak below "+((.5f+.025f*(item.rank-1))*100).ToString("0")+"% HP: +"+((.4f+.05f*(item.rank-1))*100).ToString("0")+"% damage; lethal hits sever.";
                case AbilityId.Counterguard:return LoadoutCatalog.CounterguardCapacity(item).ToString("0.#")+" barrier / "+(3.5f*scale).ToString("0.0")+"s. Bank 80% of absorption, capped at "+(45*scale).ToString("0")+". Total barriers capped at 90.";
            }}
            return item.def.tradeoff;
        }
        string GearEffectRules(LoadoutItem item)
        {
            var e=item.def.effect;float s=LoadoutCatalog.EffectScale(item);string power=e.Power(item.rank).ToString("0.##"),radius=e.Radius(item.rank).ToString("0.##"),seconds=e.Seconds(item.rank).ToString("0.##");
            switch(item.def.signature){
                case Signature.Tactician:return "+"+(6*ItemRarity.Power[item.rank]).ToString("0.#")+" damage against stagger. First qualifying hit per turn transfers "+seconds+"s stagger to one neighbor within "+radius+"m.";
                case Signature.Bulwark:return "Completing the loop arms "+power+" guard capacity. Absorbing a hit staggers nearby enemies for "+seconds+"s within "+radius+"m.";
                case Signature.Recovery:return "First kill per turn drops a "+power+" HP mote. Collect within "+radius+"m before its 10s expiry. Up to 24 motes.";
                case Signature.Surveyor:return "A turn hitting two targets echoes "+power+" damage onto its last survivor; 35% splash to two neighbors within "+radius+"m.";
                case Signature.Salvager:return "Launch/sever arms one retrieval. The next basic pulls a corpse within "+radius+"m toward its impact with "+(.6f*s).ToString("0.00")+"× launch impulse.";
                case Signature.Capacitor:return "First direct hit against stagger applies one Conductive mark: "+(4*s).ToString("0.0")+"s duration, "+(10*ItemRarity.Power[item.rank]).ToString("0.#")+" damage if later discharged.";
                case Signature.Reactive:return "Three enemies within 3m grant "+power+" barrier for 3s. Break/expiry repels enemies within 3m. 8s cooldown.";
                case Signature.Grounding:return "Convert "+(20*s).ToString("0")+"% of a lightning/body hit into barrier, capped at "+power+" once per turn; lasts 4s. Attackers hitting barrier become Conductive.";
                case Signature.Duelist:return "Reload/sever arms a "+seconds+"s ward: reduce the next hit by "+(Mathf.Min(.65f,e.Power(item.rank))*100).ToString("0")+"% and stagger its attacker for "+(.4f*s).ToString("0.00")+"s.";
                case Signature.Momentum:return "A launch stores "+(Mathf.Min(.4f,e.Power(item.rank))*100).ToString("0")+"% haste for the next ability, expiring after "+seconds+"s. No stacking.";
                case Signature.Loader:return "Preparation/reloads are "+(Mathf.Min(.4f,e.Power(item.rank))*100).ToString("0")+"% shorter. Actual refill staggers up to five foes within "+radius+"m for "+seconds+"s, once per turn.";
                case Signature.Arcstep:return "Two kills in one turn mark up to "+(2+(item.rank-1)/2)+" survivors within "+radius+"m for "+(4*s).ToString("0.0")+"s. Later discharges deal "+(10*ItemRarity.Power[item.rank]).ToString("0.#")+" bonus damage.";
                case Signature.FaultLine:return "+"+power+" body-collision damage. First impact per turn staggers up to four neighbors within "+radius+"m for "+seconds+"s.";
                case Signature.Overclock:return "+"+(e.Power(item.rank)*100).ToString("0.#")+"% tempo per completed turn, capped at "+(Mathf.Min(.4f,.25f+.04f*(item.rank-1))*100).ToString("0")+"%. Resets when the loop wraps.";
                case Signature.Amplifier:return "Each ability banks "+power+" damage, up to "+(e.Power(item.rank)*3).ToString("0.#")+". The next basic spends it once in a "+radius+"m burst against up to five foes.";
                case Signature.Relay:return "Body impacts consume Conductive; chain "+power+" damage to "+(1+(item.rank>=3?1:0)+(item.rank==5?1:0))+" neighbors within "+radius+"m. Arcs cannot retrigger themselves.";
                case Signature.Salvage:return "First sever per turn launches its body/fragments at "+(.7f+.1f*(item.rank-1)).ToString("0.0")+"× impulse. One enemy identity; six collision hits and two extra hops.";
            }
            return item.def.description;
        }
        string UpgradePreview(RewardOffer offer)
        {
            if(offer.def.kind==ItemKind.Augment)return "NO FUSION / "+game.Deck.CompatibleHosts(offer.def).Count()+" COMPATIBLE HOSTS";
            int owned=offer.owned,total=owned+offer.copies;
            if(offer.rank==5)return "LEGENDARY · MAX RARITY / "+offer.def.upgrade;
            return total>=3?"FUSES NOW → "+ItemRarity.Names[offer.rank+1].ToUpperInvariant()+" / "+ItemRarity.StatBonus(offer.rank+1):total+"/3 toward "+ItemRarity.Names[offer.rank+1]+" / identical item + rarity";
        }
        string ItemStats(ItemDefinition def,int rarity)
        {
            if(def.kind==ItemKind.Augment)return def.tradeoff;
            var item=new LoadoutItem(def,rarity);var card=def.profile!=null||def.ability!=null?LoadoutCatalog.Action(item):null;
            if(card!=null)return (def.kind==ItemKind.Weapon?(def.hands==2?"2H / ":"1H / "):"")+card.damage.ToString("0.#")+" DMG / "+card.duration.ToString("0.00")+"s";
            float p=ItemRarity.Power[rarity];var stats=def.stats;
            return (stats.health>0?"+"+(stats.health*p).ToString("0")+" HP / ":"")+(stats.damage>0?"+"+(stats.damage*p*100).ToString("0.#")+"% DMG / ":"")+(stats.resistance>0?(stats.resistance*p*100).ToString("0.#")+"% RESIST / ":"")+(stats.speed>0?"+"+(stats.speed*p*100).ToString("0.#")+"% SPEED / ":"")+def.signature.ToString().ToUpperInvariant();
        }
        string AttackDescription(LoadoutItem item)
        {
            if(item==null)return "Empty position; skipped in combat.";
            if(item.def.kind==ItemKind.Basic)item=game.Deck.WeaponFor(item);
            if(item.augment!=null)switch(item.augment.def.module.id){
                case AugmentId.FollowThrough:return "Weaker stagger; banks a pulse for the next basic.";
                case AugmentId.Focused:return "Narrower coverage; stronger primary payoff.";
                case AugmentId.Echo:return "Weaker opening; one delayed repeat hit.";
                case AugmentId.Afterfield:return "Weaker opening; leaves a two-second field.";
                case AugmentId.SplitPayload:return "Launch up to two bodies with less force.";
                case AugmentId.Backwash:return "Weaker push, then pull struck survivors back.";
                case AugmentId.Discharge:return "Barrier absorbs damage, then bursts forward.";
                case AugmentId.Anchor:return "Weaker pull; successful moves end in stagger.";
                case AugmentId.Flurry:return "Weaker opening, then two quick follow-up hits.";
                case AugmentId.PalmDrive:return "Less damage; shove the first contact away.";
                case AugmentId.Crosscut:return "Weaker swing, then a return cut. Lethal cuts sever.";
                case AugmentId.HookedEdge:return "Narrow cut; pull the first surviving target inward.";
                case AugmentId.Groundbreaker:return "Less launch force; stagger along a ground wave.";
                case AugmentId.Piledriver:return "One target; stronger launch with no splash.";
                case AugmentId.BoreRounds:return "Extra preparation; focused, penetrating shot.";
                case AugmentId.BreachLoad:return "Longer reload arms one impact stagger burst.";
            }
            return item.def.description;
        }
        string CombatStats(LoadoutItem item)
        {
            if(item==null)return "";var actual=item.def.kind==ItemKind.Basic?game.Deck.WeaponFor(item):item;
            if(actual.def.profile==null&&actual.def.ability==null)return ItemStats(actual.def,actual.rank);
            var card=LoadoutCatalog.Action(actual);float damage=card.damage*game.Deck.Damage*AugmentCatalog.Opening(actual);
            return damage.ToString("0.#")+" DMG / "+(card.range*(actual.HasAugment(AugmentId.HookedEdge)?.85f:1)).ToString("0.#")+"m / "+((card.duration+AugmentCatalog.Extension(actual))/game.Deck.Speed).ToString("0.00")+"s";
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
