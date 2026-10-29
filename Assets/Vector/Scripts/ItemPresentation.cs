using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        static readonly Color Violet=new Color(.78f,.64f,1),Blue=new Color(.48f,.74f,1);
        static Color RarityColor(int rarity)=>rarity==5?new Color(1,.61f,.22f):rarity==4?new Color(.78f,.48f,1):rarity==3?new Color(.36f,.66f,1):rarity==2?new Color(.45f,.87f,.43f):new Color(.79f,.82f,.84f);
        static Color TypeColor(ItemKind kind)=>kind==ItemKind.Weapon||kind==ItemKind.Basic?Gold:kind==ItemKind.Ultimate?Violet:kind==ItemKind.Rig?Blue:Accent;
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
            return offer.def.kind.ToString().ToUpperInvariant()+(owned?" UPGRADE":"");
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
            if(def.kind==ItemKind.Weapon)
            {var card=LoadoutCatalog.Basic(new LoadoutItem(def,rarity));return (def.hands==2?"2H · BOTH MOUNTS":def.weapon==WeaponKind.Fists?"UNARMED":"1H")+" / "+card.damage.ToString("0")+" DAMAGE / "+card.duration.ToString("0.00")+"s";}
            if(def.kind==ItemKind.Ability||def.kind==ItemKind.Ultimate)return (def.ability.damage*power).ToString("0")+" DAMAGE / "+(def.kind==ItemKind.Ultimate?(def.id=="tempest"?"16":"12")+"s RECHARGE":def.ability.duration.ToString("0.00")+"s ACTION");
            return def.id=="overclock"?"+"+(20*power).ToString("0")+"% SEQUENCE SPEED":def.id=="amplifier"?"+"+(25*power).ToString("0")+"% DIRECT DAMAGE":"+"+(14*power).ToString("0")+" COLLISION DAMAGE";
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
            if(def.kind==ItemKind.Rig)return def.description;
            switch(def.weapon)
            {
                case WeaponKind.Sword:return "CREATES cut fragments · USES clustered targets";
                case WeaponKind.Hammer:return "CREATES launch + collisions · USES nearby bodies";
                case WeaponKind.Pistol:return "CREATES stagger · RARE+ ricochet";
                case WeaponKind.Rifle:return "CREATES piercing hits · USES aligned targets";
                default:return "CREATES stagger · FEEDS Arc Bolt";
            }
        }
        void GroupOutline(float x,float y,float width,float gap,float height)
        {
            if(!game.Deck.TwoHanded)return;
            int a=System.Array.IndexOf(game.Deck.Slots,game.Deck.Basics[0]);
            Rect r=new Rect(x+a*(width+gap)-4,y-4,width*2+gap+8,height+8);
            Border(r,Gold,2);var c=new Vector2(r.x+width+gap/2+4,r.y+height/2);
            Box(new Rect(c.x-13,c.y-13,26,26),ink);LockIcon(c,Gold);
        }
    }
}
