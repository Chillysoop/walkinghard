using UnityEngine;

namespace VectorGame
{
    public sealed partial class FighterRig
    {
        Transform captainCrown,interceptorRam;
        public void DistrictChampionEquipment(int kind)
        {
            if(lightweight)return;CinderworksWorld.InitializeMaterials();
            foreach(int side in new[]{-1,1})
            {
                var plate=VisualFactory.Node(kind==1?"Captain shield projector":"Interceptor shoulder ram",Body,new Vector3(side*(kind==1?.76f:.6f),.55f,.18f));
                VisualFactory.Part("Champion armor",plate,PrimitiveType.Cube,Vector3.zero,kind==1?new Vector3(.48f,1.45f,.3f):new Vector3(.46f,.6f,1.1f),CinderworksWorld.Ceramic);
                VisualFactory.Part("Role lamp",plate,PrimitiveType.Cube,new Vector3(0,kind==1?.35f:.15f,kind==1?.18f:.58f),kind==1?new Vector3(.35f,.65f,.06f):new Vector3(.34f,.12f,.08f),kind==1?VisualFactory.GoldGlow:VisualFactory.SiphonGlow);
                if(side<0)championLeft=plate;else championRight=plate;
            }
            if(kind==1)
            {
                captainCrown=VisualFactory.Node("Captain shield crest",Body,new Vector3(0,1.55f,-.25f));
                VisualFactory.Part("Crest top",captainCrown,PrimitiveType.Cube,Vector3.zero,new Vector3(.8f,.6f,.17f),CinderworksWorld.Ceramic);
                var point=VisualFactory.Part("Crest point",captainCrown,PrimitiveType.Cube,new Vector3(0,-.29f,0),new Vector3(.43f,.43f,.18f),VisualFactory.GoldGlow);point.localRotation=Quaternion.Euler(0,0,45);
            }
            else
            {
                interceptorRam=VisualFactory.Node("Interceptor arrow ram",Body,new Vector3(0,.35f,.6f));
                foreach(int side in new[]{-1,1})
                {
                    var prong=VisualFactory.Part("Swept ram prong",interceptorRam,PrimitiveType.Cube,new Vector3(side*.23f,0,.25f),new Vector3(.15f,.23f,.9f),CinderworksWorld.Iron);prong.localRotation=Quaternion.Euler(0,side*25,0);
                    var fin=VisualFactory.Part("Dispatch swept fin",Body,PrimitiveType.Cube,new Vector3(side*.35f,1.3f,-.4f),new Vector3(.14f,.9f,.4f),CinderworksWorld.Ceramic);fin.localRotation=Quaternion.Euler(-25,0,side*-20);
                }
            }
            championCore=VisualFactory.Part("Exposed champion core",Body,PrimitiveType.Cube,new Vector3(0,.5f,.48f),new Vector3(.45f,.5f,.08f),VisualFactory.Glow);championCore.gameObject.SetActive(false);
        }
        public void DistrictChampionPose(int kind,int phase,float pressure,bool exposed)
        {
            ChampionPose(!exposed,pressure);
            if(interceptorRam!=null)interceptorRam.localRotation=Quaternion.Euler(phase==1?-12:phase==2?8:0,0,0);
            if(captainCrown!=null)captainCrown.localRotation=Quaternion.Euler(phase==2?-12:0,0,0);
        }
    }
    public sealed partial class PrototypeHUD
    {
        void DrawChampionGlyph(int kind,Vector2 p,Color color)
        {
            if(kind==1)
            {
                Vector2[] v={new Vector2(-11,-12),new Vector2(11,-12),new Vector2(10,5),new Vector2(0,14),new Vector2(-10,5),new Vector2(-11,-12)};
                for(int i=1;i<v.Length;i++)Stroke(p+v[i-1],p+v[i],color,2);
                Stroke(p+new Vector2(-6,-4),p+new Vector2(6,-4),color,2);
                Stroke(p+new Vector2(-4,2),p+new Vector2(4,2),color,2);
            }
            else if(kind==2)
            {
                foreach(int y in new[]{-6,5}){Stroke(p+new Vector2(-11,y+5),p+new Vector2(0,y-5),color,3);Stroke(p+new Vector2(0,y-5),p+new Vector2(11,y+5),color,3);}
                Stroke(p+new Vector2(-14,14),p+new Vector2(-14,3),color,2);Stroke(p+new Vector2(14,14),p+new Vector2(14,3),color,2);
            }
            else
            {
                foreach(int side in new[]{-1,1}){Stroke(p+new Vector2(side*11,-13),p+new Vector2(side*11,10),color,3);Stroke(p+new Vector2(side*11,10),p+new Vector2(side*4,10),color,3);}
                Plate(new Rect(p.x-6,p.y-6,12,12),ink,color,2);
            }
        }
    }
}
