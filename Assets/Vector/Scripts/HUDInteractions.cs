using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        static void Stroke(Vector2 a,Vector2 b,Color color,float thickness=2)
        {
            var before=GUI.matrix;var d=b-a;GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg,a);Box(new Rect(a.x,a.y-thickness/2,d.magnitude,thickness),color);GUI.matrix=before;
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
