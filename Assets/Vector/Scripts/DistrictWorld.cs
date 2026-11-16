using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class CinderworksWorld
    {
        int roadDistrict=-1;
        static Material bellFloor,bellIron,brass,bellLight,dispatchFloor,dispatchIron,dispatchWhite,dispatchLight;
        static readonly Dictionary<int,List<(Mesh mesh,Material material)>> districtMeshes=new Dictionary<int,List<(Mesh,Material)>>();
        void InitializeDistrictMaterials()
        {
            InitializeMaterials();if(brass!=null)return;
            bellFloor=VisualFactory.Lit("Foundry | muted brass deck",new Color(.22f,.20f,.15f),.4f);
            bellIron=VisualFactory.Lit("Foundry | dark bronze",new Color(.16f,.15f,.12f),.55f);
            brass=VisualFactory.Lit("Foundry | cast brass",new Color(.48f,.36f,.16f),.55f);
            bellLight=VisualFactory.Lit("Foundry | banked amber",new Color(.56f,.39f,.12f),.2f,new Color(.9f,.55f,.1f));
            dispatchFloor=VisualFactory.Lit("Dispatch | blue slate",new Color(.14f,.18f,.24f),.35f);
            dispatchIron=VisualFactory.Lit("Dispatch | registry blue",new Color(.12f,.17f,.25f),.5f);
            dispatchWhite=VisualFactory.Lit("Dispatch | ivory enamel",new Color(.55f,.62f,.69f),.3f);
            dispatchLight=VisualFactory.Lit("Dispatch | cold paper light",new Color(.46f,.57f,.72f),.1f,new Color(.6f,.8f,1.15f));
        }
        Material DistrictMaterial(Material source,int district)
        {
            if(source==VisualFactory.Ground)return district==0?Floor:district==1?bellFloor:dispatchFloor;
            if(source==VisualFactory.Stone)return district==0?Iron:district==1?bellIron:dispatchIron;
            if(source==VisualFactory.Trim)return district==0?Ceramic:district==1?brass:dispatchWhite;
            if(source==VisualFactory.Glow)return district==0?Heat:district==1?bellLight:dispatchLight;
            return source;
        }
        internal static void DistrictAtmosphere(int district)
        {
            RenderSettings.fogColor=district==0?new Color(.16f,.13f,.105f):district==1?new Color(.16f,.15f,.11f):new Color(.10f,.15f,.22f);
            RenderSettings.ambientSkyColor=district==0?new Color(.58f,.51f,.43f):district==1?new Color(.55f,.53f,.43f):new Color(.46f,.56f,.68f);
            RenderSettings.ambientEquatorColor=district==0?new Color(.35f,.29f,.24f):district==1?new Color(.33f,.31f,.23f):new Color(.25f,.33f,.42f);
        }
        Transform BuildDistrictBay(Transform parent,int z,int district)
        {
            if(district==0)return BuildBay(parent,z);
            var bay=VisualFactory.Node(district==1?"Foundry / bell gallery":"Dispatch / registry stacks",parent,new Vector3(0,0,z));
            if(districtMeshes.TryGetValue(district,out var cached))
            {foreach(var p in cached){var t=VisualFactory.Node("Batched district architecture",bay,Vector3.zero);t.gameObject.AddComponent<MeshFilter>().sharedMesh=p.mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=p.material;}return bay;}
            void Box(string name,Vector3 p,Vector3 size,Material m)=>VisualFactory.Part(name,bay,PrimitiveType.Cube,p,size,m);
            for(int side=-1;side<=1;side+=2)
            {
                if(district==1)
                {
                    Box("Bell gallery base",new Vector3(side*12,1.1f,9),new Vector3(4.2f,2.2f,9),bellIron);
                    foreach(int end in new[]{5,13})Box("Bell gallery pillar",new Vector3(side*11.8f,5,end),new Vector3(.5f,8,.5f),brass);
                    Box("Bell suspension beam",new Vector3(side*11.8f,8.7f,9),new Vector3(.55f,.5f,9),brass);
                    for(int n=0;n<3;n++)
                    {
                        float at=6+n*3;Box("Bell suspension",new Vector3(side*11.8f,7.6f,at),new Vector3(.16f,2,.16f),brass);
                        VisualFactory.Part("Foundry bell",bay,PrimitiveType.Cylinder,new Vector3(side*11.8f,5.8f,at),new Vector3(2.2f,1.1f,2.2f),brass);
                        VisualFactory.Part("Bell flared lip",bay,PrimitiveType.Cylinder,new Vector3(side*11.8f,4.7f,at),new Vector3(2.7f,.13f,2.7f),bellIron);
                        Box("Gallery light",new Vector3(side*9.08f,2.6f,at),new Vector3(.08f,1.2f,.3f),bellLight);
                    }
                }
                else
                {
                    Box("Registry foundation",new Vector3(side*12,1,9),new Vector3(4,2,10),dispatchIron);
                    for(int row=0;row<3;row++)for(int col=0;col<5;col++)
                    {
                        var p=new Vector3(side*11,2.6f+row*1.45f,4.8f+col*2.1f);
                        Box("Sealed mail drawer",p,new Vector3(2.1f,1.3f,1.9f),row%2==0?dispatchWhite:dispatchIron);
                        Box("Drawer label",p+new Vector3(-side*1.08f,0,0),new Vector3(.04f,.18f,.8f),dispatchLight);
                    }
                    Box("Dispatch signal post",new Vector3(side*10,4.2f,2),new Vector3(.3f,8.4f,.3f),dispatchWhite);
                    Box("Dispatch signal board",new Vector3(side*10,7,3.2f),new Vector3(.24f,1.2f,3.2f),dispatchIron);
                    for(int i=0;i<3;i++)Box("Final order stencil",new Vector3(side*9.84f,7,2.3f+i*.8f),new Vector3(.03f,.65f,.35f),dispatchLight);
                }
            }
            PresentationGeometry.CompactBone(bay,"journey/district/"+district+"/v1");cached=new List<(Mesh,Material)>();
            foreach(var mesh in bay.GetComponentsInChildren<MeshFilter>())if(mesh.gameObject.activeSelf)cached.Add((mesh.sharedMesh,mesh.GetComponent<MeshRenderer>().sharedMaterial));districtMeshes[district]=cached;
            return bay;
        }
    }
}
