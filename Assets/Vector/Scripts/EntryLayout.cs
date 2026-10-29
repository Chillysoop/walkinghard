using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public static class EntryLayout
    {
        public const int WorldLayer=8;
        public sealed class Door {public int side;public float z;public Vector3 origin,exit;}
        public static readonly List<Door> Doors=new List<Door>();
        public static Transform Solid(string name,Transform parent,Vector3 center,Vector3 size,Material material)
        {
            var t=VisualFactory.Part(name,parent,PrimitiveType.Cube,center,size,material);
            t.gameObject.layer=WorldLayer;t.gameObject.AddComponent<BoxCollider>();return t;
        }
        public static void Build(Transform parent)
        {
            Doors.Clear();Solid("Concourse / physical floor",parent,new Vector3(0,-.27f,150),new Vector3(40,.5f,1000),VisualFactory.Ground);
            for(int z=-222;z<550;z+=6)
            {
                VisualFactory.Part("Floor seam",parent,PrimitiveType.Cube,new Vector3(0,-.012f,z),new Vector3(18,.02f,.035f),VisualFactory.Black);
                for(int side=-1;side<=1;side+=2)
                {
                    VisualFactory.Part("Guide",parent,PrimitiveType.Cube,new Vector3(side*3,.005f,z),new Vector3(.035f,.02f,5.8f),VisualFactory.Trim);
                    VisualFactory.Part("Edge light",parent,PrimitiveType.Cube,new Vector3(side*8.8f,.015f,z),new Vector3(.06f,.025f,5.8f),VisualFactory.Glow);
                }
            }
            for(int side=-1;side<=1;side+=2)
            {
                float previous=-241;
                for(int z=-207;z<540;z+=18)
                {
                    float end=z-2;
                    Solid("Concourse wall",parent,new Vector3(side*9.5f,2.3f,(previous+end)*.5f),new Vector3(.7f,4.7f,end-previous),VisualFactory.Stone);
                    Solid("Door lintel",parent,new Vector3(side*9.5f,4.1f,z),new Vector3(.9f,1,4),VisualFactory.Trim);
                    // The covered room conceals creation; the fighter walks continuously through its opening.
                    Solid("Entry passage roof",parent,new Vector3(side*13.7f,4.5f,z),new Vector3(8.8f,.35f,4.6f),VisualFactory.Stone);
                    Solid("Entry passage front",parent,new Vector3(side*13.7f,2.2f,z-2.1f),new Vector3(8.8f,4.4f,.35f),VisualFactory.Stone);
                    Solid("Entry passage rear",parent,new Vector3(side*13.7f,2.2f,z+2.1f),new Vector3(8.8f,4.4f,.35f),VisualFactory.Stone);
                    Solid("Entry passage back",parent,new Vector3(side*18,2.2f,z),new Vector3(.35f,4.4f,4.4f),VisualFactory.Black);
                    for(int edge=-1;edge<=1;edge+=2)
                        VisualFactory.Part("Door light",parent,PrimitiveType.Cube,new Vector3(side*9.05f,1.65f,z+edge*1.9f),new Vector3(.09f,3.3f,.09f),VisualFactory.GoldGlow);
                    VisualFactory.Part("Threshold",parent,PrimitiveType.Cube,new Vector3(side*9.3f,.01f,z),new Vector3(.5f,.03f,3.6f),VisualFactory.Trim);
                    Doors.Add(new Door{side=side,z=z,origin=new Vector3(side*15.5f,.05f,z),exit=new Vector3(side*7.8f,.05f,z)});
                    previous=z+2;
                }
                Solid("Far wall",parent,new Vector3(side*9.5f,2.3f,(previous+565)*.5f),new Vector3(.7f,4.7f,565-previous),VisualFactory.Stone);
            }
        }
        public static Door Ahead(int side,float heroZ,int direction=1)
        {
            Door best=null;foreach(var door in Doors)if(door.side==side&&direction*(door.z-heroZ)>3&&(best==null||direction*(door.z-best.z)<0))best=door;return best;
        }
        public static bool Concealed(Camera camera,Vector3 origin)
        {
            var bounds=new Bounds(origin+Vector3.up*1.2f,new Vector3(1.8f,3,1.8f));
            if(!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera),bounds))return true;
            Vector3[] points={origin+Vector3.up*.3f,origin+Vector3.up*1.2f,origin+Vector3.up*2.5f,origin+new Vector3(.65f,1.5f,0),origin+new Vector3(-.65f,1.5f,0)};
            foreach(var point in points)
            {
                Vector3 delta=point-camera.transform.position;
                if(!Physics.Raycast(camera.transform.position,delta.normalized,delta.magnitude-.3f,1<<WorldLayer,QueryTriggerInteraction.Ignore))return false;
            }
            return true;
        }
        public static Vector3 Horizon(Camera camera,Vector3 hero,float x,int direction=1)
        {
            Vector3 p=hero+new Vector3(x,.05f,32*direction);
            // Explicitly test full character bounds, rather than assuming a fixed spawn distance is hidden.
            for(int i=0;i<60;i++){if(Concealed(camera,p))return p;p.z+=3*direction;}
            return hero+Vector3.forward*(190*direction)+new Vector3(x,.05f,0);
        }
    }
}
