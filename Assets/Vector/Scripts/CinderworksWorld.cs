using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    // Regional visuals decorate the existing covered corridor. They never create
    // a collider, change an entrance, consume combat RNG or move an actor.
    public sealed partial class CinderworksWorld : MonoBehaviour
    {
        public static Material Enamel,Iron,Ceramic,Floor,Heat;
        static List<(Mesh mesh,Material material)> bayMeshes;
        readonly List<(Renderer renderer,Material original)> surfaces=new List<(Renderer,Material)>();
        Transform decoration;
        public bool Visible {get;private set;}
        public Transform Decoration=>decoration;
        public static void InitializeMaterials()
        {
            if(Enamel!=null)return;
            Enamel=VisualFactory.Lit("Cinderworks | worn furnace enamel",new Color(.53f,.23f,.075f),.35f);
            Iron=VisualFactory.Lit("Cinderworks | cast iron",new Color(.11f,.135f,.14f),.65f);
            Ceramic=VisualFactory.Lit("Cinderworks | ceramic heat shield",new Color(.69f,.65f,.52f),.15f);
            Floor=VisualFactory.Lit("Cinderworks | work deck",new Color(.19f,.195f,.18f),.45f);
            Heat=VisualFactory.Lit("Cinderworks | banked furnace",new Color(.68f,.22f,.035f),.2f,new Color(1.65f,.36f,.045f));
        }
        public void SetVisible(bool visible)
        {
            if(Visible==visible)return;
            if(decoration==null&&visible)Build();
            Visible=visible;
            if(decoration!=null)decoration.gameObject.SetActive(visible);
            foreach(var entry in surfaces)
            {
                if(entry.renderer==null)continue;
                Material material=entry.original;
                if(visible)
                {
                    if(material==VisualFactory.Ground)material=Floor;
                    else if(material==VisualFactory.Stone)material=Iron;
                    else if(material==VisualFactory.Glow)material=Heat;
                    else if(material==VisualFactory.Trim)material=Ceramic;
                }
                entry.renderer.sharedMaterial=material;
            }
        }
        void Build()
        {
            InitializeMaterials();
            foreach(var r in GetComponentsInChildren<MeshRenderer>())surfaces.Add((r,r.sharedMaterial));
            decoration=VisualFactory.Node("Cinderworks / visual architecture",transform,Vector3.zero);
            // Shared, material-batched modules cover both directions of the boss camera.
            for(int z=-225;z<540;z+=18)BuildBay(decoration,z);
        }
        internal static Transform BuildBay(Transform parent,int z)
        {
                var bay=VisualFactory.Node("Furnace bay",parent,new Vector3(0,0,z));
                if(bayMeshes!=null)
                {
                    foreach(var part in bayMeshes){var t=VisualFactory.Node("Batched furnace architecture",bay,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=part.mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=part.material;}
                    return bay;
                }
                void Box(string name,Vector3 p,Vector3 size,Material material)=>VisualFactory.Part(name,bay,PrimitiveType.Cube,p,size,material);
                Transform Drum(string name,Vector3 p,Vector3 size,Material material)=>VisualFactory.Part(name,bay,PrimitiveType.Cylinder,p,size,material);
                for(int side=-1;side<=1;side+=2)
                {
                    Box("Freight rail",new Vector3(side*6.4f,.012f,9),new Vector3(.13f,.028f,17.8f),VisualFactory.Trim);
                    Box("Rail recess",new Vector3(side*6.65f,-.002f,9),new Vector3(.18f,.022f,17.8f),VisualFactory.Black);
                    for(int i=0;i<6;i++)
                    {
                        Box("Freight sleeper",new Vector3(side*6.5f,.004f,1.5f+i*3),new Vector3(.85f,.022f,.18f),Iron);
                        Box("Edge hazard block",new Vector3(side*8.4f,.012f,1.5f+i*3),new Vector3(.45f,.028f,.9f),Enamel);
                    }
                    // Bay centers lie between the existing door openings, not across them.
                    Box("Furnace plinth",new Vector3(side*12,1.3f,9),new Vector3(5,2.6f,7),Iron);
                    Drum("Pressure chamber",new Vector3(side*12,5.2f,9),new Vector3(4.1f,2.7f,4.1f),Enamel);
                    for(int band=0;band<3;band++)Drum("Chamber reinforcement",new Vector3(side*12,3.2f+band*2,9),new Vector3(4.35f,.12f,4.35f),Iron);
                    Drum("Ceramic chimney",new Vector3(side*12,9.4f,9),new Vector3(1.3f,1.7f,1.3f),Ceramic);
                    Drum("Chimney rim",new Vector3(side*12,11.05f,9),new Vector3(1.65f,.13f,1.65f),Iron);
                    Box("Furnace door rim",new Vector3(side*9.04f,2.2f,9),new Vector3(.16f,2.8f,4.4f),Enamel);
                    Box("Banked furnace opening",new Vector3(side*8.94f,2.2f,9),new Vector3(.07f,1.65f,3.35f),Heat);
                    for(int grate=0;grate<6;grate++)Box("Furnace grate",new Vector3(side*8.87f,2.2f,7.5f+grate*.6f),new Vector3(.13f,1.9f,.18f),Iron);
                    Box("Furnace buttress",new Vector3(side*10.1f,2.8f,1),new Vector3(.8f,5.6f,.85f),Iron);
                    Box("Buttress enamel inset",new Vector3(side*9.65f,3.2f,1),new Vector3(.08f,3.6f,.42f),Enamel);
                    Box("Freight duct",new Vector3(side*10.1f,4.8f,9),new Vector3(.65f,.3f,18),Ceramic);
                    var pipe=Drum("Steam main",new Vector3(side*13.4f,7.8f,9),new Vector3(.55f,9,.55f),Iron);pipe.localRotation=Quaternion.Euler(90,0,0);
                    Box("Door lintel stencil",new Vector3(side*9.02f,4.12f,0),new Vector3(.07f,.25f,2.8f),Enamel);
                }
                // No cross-lane beam: the finale camera rises and turns through this volume.
                PresentationGeometry.CompactBone(bay,"cinderworks/bay/v1");bayMeshes=new List<(Mesh,Material)>();
                foreach(var mesh in bay.GetComponentsInChildren<MeshFilter>())if(mesh.gameObject.activeSelf)bayMeshes.Add((mesh.sharedMesh,mesh.GetComponent<MeshRenderer>().sharedMaterial));
                return bay;
        }
    }
}
