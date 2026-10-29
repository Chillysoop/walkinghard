using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        static void Plate(Rect r,Color fill,Color edge,float cut=9,float thickness=1)
        {
            cut=Mathf.Min(cut,Mathf.Min(r.width,r.height)/3);
            Box(new Rect(r.x+cut,r.y,r.width-cut*2,r.height),fill);
            Box(new Rect(r.x,r.y+cut,cut,r.height-cut*2),fill);Box(new Rect(r.xMax-cut,r.y+cut,cut,r.height-cut*2),fill);
            for(int i=0;i<Mathf.CeilToInt(cut);i++)
            {
                float n=Mathf.Min(1,cut-i),inset=cut-i;
                Box(new Rect(r.x+inset,r.y+i,cut-inset,n),fill);Box(new Rect(r.xMax-cut,r.y+i,cut-inset,n),fill);
                Box(new Rect(r.x+inset,r.yMax-i-n,cut-inset,n),fill);Box(new Rect(r.xMax-cut,r.yMax-i-n,cut-inset,n),fill);
            }
            Outline(r,edge,cut,thickness);
        }
        static void Outline(Rect r,Color color,float cut=9,float thickness=1)
        {
            Vector2[] p={new Vector2(r.x+cut,r.y),new Vector2(r.xMax-cut,r.y),new Vector2(r.xMax,r.y+cut),new Vector2(r.xMax,r.yMax-cut),new Vector2(r.xMax-cut,r.yMax),new Vector2(r.x+cut,r.yMax),new Vector2(r.x,r.yMax-cut),new Vector2(r.x,r.y+cut)};
            for(int i=0;i<8;i++)Stroke(p[i],p[(i+1)%8],color,thickness);
        }
        static void Segments(Rect r,float value,Color color,int count=12)
        {
            Box(r,new Color(.025f,.07f,.09f));float part=(r.width-2)/count;
            for(int i=0;i<count;i++)
            {
                float filled=Mathf.Clamp01(value*count-i);if(filled>0)Box(new Rect(r.x+1+i*part,r.y+1,(part-2)*filled,r.height-2),color);
            }
        }
        static void ActionGlyph(Rect r,ItemDefinition def,Color color)
        {
            if(def==null)return;
            var matrix=GUI.matrix;GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(r.center.x,r.center.y,0),Quaternion.identity,new Vector3(r.width/64,r.height/64,1));
            void L(float x,float y,float a,float b,float width=3)=>Stroke(new Vector2(x,y),new Vector2(a,b),color,width);
            if(def.kind==ItemKind.Helmet)
            {L(-21,13,-21,-8);L(-21,-8,-11,-22);L(-11,-22,11,-22);L(11,-22,21,-8);L(21,-8,21,13);L(-21,13,-8,21);L(21,13,8,21);L(-18,-3,18,-3,5);L(-8,21,-8,10);L(8,21,8,10);}
            else if(def.kind==ItemKind.Armor)
            {L(-10,-23,-24,-15);L(-24,-15,-18,18);L(-18,18,0,25);L(0,25,18,18);L(18,18,24,-15);L(24,-15,10,-23);L(-10,-23,0,-14);L(0,-14,10,-23);L(-13,-3,13,-3);L(0,-8,0,15);}
            else if(def.kind==ItemKind.Boots)
            {for(int i=-1;i<=1;i+=2){L(i*12-6,-23,i*12+6,-23);L(i*12-6,-23,i*12-6,20);L(i*12-6,20,i*12+12,20);L(i*12+12,20,i*12+12,10);L(i*12+12,10,i*12+6,6);L(i*12+6,6,i*12+6,-23);}}
            else if(def.kind==ItemKind.Weapon&&def.weapon==WeaponKind.Sword)
            {L(-18,20,18,-20,5);L(18,-20,12,-6);L(18,-20,4,-13);L(-19,4,-4,19);L(-24,25,-18,19,5);}
            else if(def.kind==ItemKind.Weapon&&def.weapon==WeaponKind.Hammer)
            {L(-19,23,10,-9,5);L(-1,-21,23,3,11);L(-5,-17,19,7,8);}
            else if(def.kind==ItemKind.Weapon&&(def.weapon==WeaponKind.Pistol||def.weapon==WeaponKind.Rifle))
            {L(-22,-7,21,-7,9);L(-13,-3,-18,19,9);L(21,-7,28,-7,3);if(def.weapon==WeaponKind.Rifle){L(-22,-7,-28,7,5);L(6,-7,6,-16);L(0,-16,12,-16);L(4,0,1,14,7);}}
            else if((def.kind==ItemKind.Weapon&&def.weapon==WeaponKind.Fists)||def.ability?.id==AbilityId.SteelJab)
            {L(-16,15,-3,25,11);L(-12,9,3,-13,10);L(0,14,15,-8,9);L(10,10,23,-6,7);L(-20,-2,-9,-17,7);L(-16,10,-22,-2,8);}
            else if(def.ability?.id==AbilityId.GravityWell||def.id=="singularity")
            {
                for(int arm=0;arm<3;arm++)for(int i=0;i<16;i++)
                {float a=arm*Mathf.PI*2/3+i*.12f,b=a+.12f;float rad=5+i*1.15f;Stroke(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*(rad+1.15f),color,4);}
                Ring(Vector2.zero,27,new Color(color.r,color.g,color.b,.55f));
            }
            else if(def.ability?.id==AbilityId.Breaker)
            {L(-24,20,-13,-8);L(-13,-8,-9,5);L(-9,5,2,-23);L(2,-23,4,7);L(4,7,24,-8);L(24,-8,13,16);L(13,16,-24,20);}
            else if(def.ability?.id==AbilityId.MassDriver)
            {for(int i=-1;i<=1;i++){L(-20+i*8,19+i*3,12+i*8,-13+i*3,4);L(12+i*8,-13+i*3,3+i*8,-10+i*3);L(12+i*8,-13+i*3,9+i*8,-4+i*3);}}
            else if(def.ability?.id==AbilityId.ForcePush)
            {for(int i=-1;i<=1;i++){L(-20+i*15,-19,-7+i*15,0);L(-7+i*15,0,-20+i*15,19);}L(-27,-10,-27,10);}
            else if(def.ability?.id==AbilityId.ArcBolt||def.id=="tempest")
            {L(11,-25,-11,1,5);L(-11,1,9,-2,5);L(9,-2,-10,24,5);L(-23,-12,-28,-5,2);L(24,11,29,4,2);}
            else {TypeIcon(new Rect(-24,-24,48,48),def.kind,color);Ring(Vector2.zero,24,color);}
            GUI.matrix=matrix;
        }
        static void Stroke(Vector2 a,Vector2 b,Color color,float thickness=2)
        {
            var before=GUI.matrix;var d=b-a;
            GUI.matrix=before*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg),Vector3.one);
            Box(new Rect(0,-thickness/2,d.magnitude,thickness),color);GUI.matrix=before;
        }
        static void Ring(Vector2 c,float radius,Color color)
        {for(int i=0;i<16;i++){float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;Stroke(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,color,1.5f);}}
        static void EffectIcon(Rect r,int kind,Color color)
        {
            Vector2 c=r.center;
            if(kind==0){for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Stroke(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*3,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*9,color);}}
            if(kind==1){Stroke(c+new Vector2(-7,7),c+new Vector2(7,-7),color);Stroke(c+new Vector2(0,-7),c+new Vector2(7,-7),color);Stroke(c+new Vector2(7,0),c+new Vector2(7,-7),color);}
            if(kind==2){Ring(c+Vector2.left*6,4,color);Ring(c+Vector2.right*6,4,color);Stroke(c+Vector2.up*8,c+Vector2.up*3,color);Stroke(c+Vector2.down*8,c+Vector2.down*3,color);}
            if(kind==3){foreach(int sign in new[]{-1,1}){Stroke(c+new Vector2(sign*10,0),c+new Vector2(sign*2,0),color);Stroke(c+new Vector2(sign*6,-4),c+new Vector2(sign*2,0),color);Stroke(c+new Vector2(sign*6,4),c+new Vector2(sign*2,0),color);}}
            if(kind==5){Stroke(c+new Vector2(-8,6),c+new Vector2(8,-6),color);Stroke(c+new Vector2(-6,-6),c+new Vector2(-1,-2),color);Stroke(c+new Vector2(2,2),c+new Vector2(6,6),color);}
            if(kind==6){Stroke(c+new Vector2(-10,0),c+new Vector2(10,0),color);Stroke(c+new Vector2(5,-4),c+new Vector2(10,0),color);Stroke(c+new Vector2(5,4),c+new Vector2(10,0),color);Stroke(c+new Vector2(-3,-7),c+new Vector2(-3,7),color);}
            if(kind==4){Ring(c+new Vector2(-4,3),5,color);Ring(c+new Vector2(4,-3),5,color);Stroke(c+new Vector2(-4,3),c+new Vector2(4,-3),color);}
        }
        void EffectTag(float x,float y,int kind,string label,bool consumes)
        {EffectIcon(new Rect(x,y,20,20),kind,consumes?Gold:Accent);Label(new Rect(x+25,y+1,250,20),(consumes?"USES ":"CREATES ")+label,12,consumes?Gold:muted);}
        void DrawSynergies(Rect r,AbilityId id)
        {
            switch(id)
            {
                case AbilityId.SteelJab:EffectTag(r.x,r.y,0,"brief stagger",false);break;
                case AbilityId.Breaker:EffectTag(r.x,r.y,0,"stagger",false);EffectTag(r.x,r.y+23,3,"clustered targets",true);break;
                case AbilityId.ForcePush:EffectTag(r.x,r.y,1,"knockback",false);EffectTag(r.x,r.y+23,3,"clustered targets",true);break;
                case AbilityId.MassDriver:EffectTag(r.x,r.y,1,"launch + collision",false);EffectTag(r.x,r.y+23,2,"nearby bodies",true);break;
                case AbilityId.GravityWell:EffectTag(r.x,r.y,3,"clustered targets",false);break;
                case AbilityId.ArcBolt:EffectTag(r.x,r.y,4,"chain discharge",false);EffectTag(r.x,r.y+23,0,"stagger (+12 damage)",true);break;
            }
        }
    }
}
