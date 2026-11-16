using System;
using UnityEngine;

namespace VectorGame
{
    // Original etched schematics, built from the same line/symbol vocabulary as the tablet.
    // They remain sharp at every HUD scale and do not encode rarity into identity artwork.
    static class ConstellationGlyphs
    {
        static void Line(Vector2 a,Vector2 b,Color c,float thickness=2)
        {var matrix=GUI.matrix;var before=GUI.color;var delta=b-a;GUI.color=c;GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg),Vector3.one);GUI.DrawTexture(new Rect(0,-thickness/2,delta.magnitude,thickness),Texture2D.whiteTexture);GUI.matrix=matrix;GUI.color=before;}
        static void RectLine(Rect r,Color c,float t=2){Line(r.position,new Vector2(r.xMax,r.y),c,t);Line(new Vector2(r.xMax,r.y),new Vector2(r.xMax,r.yMax),c,t);Line(new Vector2(r.xMax,r.yMax),new Vector2(r.x,r.yMax),c,t);Line(new Vector2(r.x,r.yMax),r.position,c,t);}
        public static bool Draw(Rect rect,ItemDefinition item)
        {
            if(!ConstellationContent.IsNew(item.id))return false;var matrix=GUI.matrix;GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(rect.x,rect.y,0),Quaternion.identity,new Vector3(rect.width/100,rect.height/100,1));
            int branch=ConstellationContent.Branch(item.id),index=Array.IndexOf(ConstellationContent.BranchItems[branch],item.id);Color c=ConstellationContent.ColorFor(branch),light=new Color(.88f,.95f,1);var before=GUI.color;GUI.color=new Color(.025f,.06f,.075f);GUI.DrawTexture(new Rect(0,0,100,100),Texture2D.whiteTexture);GUI.color=before;
            void L(float x,float y,float u,float v,Color color,float t=3)=>Line(new Vector2(x,y),new Vector2(u,v),color,t);
            void Ring(float x,float y,float radius,Color color){for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;L(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius,color,2);}}
            // Item silhouette occupies the left/center; the branch mechanism forms its right edge.
            if(item.kind==ItemKind.Weapon)
            {
                if(item.weapon==WeaponKind.Fists){RectLine(new Rect(20,40,32,28),light,4);for(int i=0;i<3;i++)RectLine(new Rect(19+i*12,27,10,18),c,3);L(19,69,48,73,light,5);}
                else if(item.weapon==WeaponKind.Sword){L(25,76,61,22,light,7);L(17,58,42,75,c,5);L(62,22,64,36,c,3);}
                else if(item.weapon==WeaponKind.Hammer){L(34,76,53,35,light,7);RectLine(new Rect(29,20,46,22),c,5);L(25,72,44,80,c,3);}
                else {RectLine(new Rect(18,34,item.hands==2?59:44,19),light,4);L(37,54,29,73,light,8);L(24,28,43,28,c,4);L(63,44,82,44,c,4);}
            }
            else if(item.kind==ItemKind.Helmet){L(20,54,22,30,light,4);L(22,30,47,21,light,4);L(47,21,65,35,light,4);L(65,35,61,61,light,4);L(20,54,37,70,light,4);L(37,70,61,61,light,4);L(26,43,56,43,c,5);}
            else if(item.kind==ItemKind.Armor){L(20,30,35,22,light,4);L(35,22,47,34,light,4);L(47,34,59,22,light,4);L(59,22,73,32,light,4);L(20,30,25,72,light,4);L(73,32,66,72,light,4);L(25,72,66,72,light,4);L(31,48,59,48,c,5);}
            else if(item.kind==ItemKind.Boots){L(27,23,26,62,light,7);L(26,62,47,70,light,7);L(50,25,49,58,light,7);L(49,58,69,68,light,7);L(21,79,74,79,c,3);}
            else if(item.kind==ItemKind.Rig){RectLine(new Rect(22,24,42,45),light,4);for(int i=0;i<3;i++){L(14,32+i*13,22,32+i*13,c,3);L(64,32+i*13,73,32+i*13,c,3);}Ring(43,46,12,c);}
            else {Ring(45,47,item.kind==ItemKind.Ultimate?29:23,light);if(item.kind==ItemKind.Ultimate)for(int i=0;i<8;i++){float a=i*Mathf.PI/4;L(45+Mathf.Cos(a)*32,47+Mathf.Sin(a)*32,45+Mathf.Cos(a)*38,47+Mathf.Sin(a)*38,c,3);}}
            if(branch==1){L(65,66,80,45,c,5);L(73,45,80,45,c,4);L(80,45,83,54,c,4);L(69,75,85,75,c,3);}
            if(branch==2){L(80,19,68,47,c,5);L(68,47,83,45,c,5);L(83,45,69,73,c,5);}
            if(branch==3){for(int i=0;i<3;i++)L(67+i*7,28,56+i*7,70,c,3);}
            if(branch==4){Ring(73,62,14,c);L(73,41,73,53,c,3);L(73,70,73,83,c,3);L(52,62,64,62,c,3);L(82,62,93,62,c,3);}
            if(branch==5){L(75,35,63,60,c,3);L(63,60,75,74,c,3);L(75,74,87,60,c,3);L(87,60,75,35,c,3);L(69,59,81,59,c,3);}
            if(branch==6){L(65,47,78,41,c,3);L(78,41,91,47,c,3);L(91,47,87,66,c,3);L(87,66,78,76,c,3);L(78,76,68,66,c,3);L(68,66,65,47,c,3);}
            // Each star has a unique circuit address in addition to its item silhouette.
            for(int i=0;i<5;i++){var tint=i==index?light:new Color(c.r*.45f,c.g*.45f,c.b*.45f);RectLine(new Rect(17+i*14,89,7,4),tint,2);}
            GUI.matrix=matrix;return true;
        }
    }
}
