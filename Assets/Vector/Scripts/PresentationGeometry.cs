using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    // Closed, faceted meshes stay children of the original severable physics bones.
    public static class PresentationGeometry
    {
        static Mesh shell;
        static readonly Dictionary<string,Mesh> boneMeshes=new Dictionary<string,Mesh>();
        public static void CompactBone(Transform root,string identity)
        {
            // Only combine decoration attached to one bone. Cuts and ragdoll joints retain
            // their original hierarchy, colliders and independent motion.
            var parts=root.GetComponentsInChildren<MeshFilter>();
            foreach(var group in parts.GroupBy(p=>p.GetComponent<MeshRenderer>().sharedMaterial))
            {
                var source=group.ToArray();if(source.Length<2)continue;
                string key=identity+"/"+group.Key.name;
                if(!boneMeshes.TryGetValue(key,out var mesh))
                {
                    mesh=new Mesh{name="Armored bone / "+key};
                    mesh.CombineMeshes(source.Select(p=>new CombineInstance{mesh=p.sharedMesh,transform=root.worldToLocalMatrix*p.transform.localToWorldMatrix}).ToArray(),true,true);
                    boneMeshes.Add(key,mesh);
                }
                var combined=VisualFactory.Node(source[0].name,root,Vector3.zero);
                combined.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                combined.gameObject.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
                foreach(var part in source){part.gameObject.SetActive(false);Object.Destroy(part.gameObject);}
            }
        }
        public static Transform Plate(string name,Transform parent,Vector3 position,Vector3 size,Material material,float rotation=0)
        {
            if(shell==null)
            {
                var vertices=new List<Vector3>();var triangles=new List<int>();
                var ring=new[]{new Vector2(-.34f,-.5f),new Vector2(.34f,-.5f),new Vector2(.5f,-.34f),new Vector2(.5f,.34f),new Vector2(.34f,.5f),new Vector2(-.34f,.5f),new Vector2(-.5f,.34f),new Vector2(-.5f,-.34f)};
                void Tri(Vector3 a,Vector3 b,Vector3 c){int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);}
                Vector3 P(int i,float y,float scale){var p=ring[i%8]*scale;return new Vector3(p.x,y,p.y);}
                for(int i=0;i<8;i++)
                {
                    Tri(P(i,-.5f,.78f),P(i,.34f,1),P(i+1,.34f,1));Tri(P(i,-.5f,.78f),P(i+1,.34f,1),P(i+1,-.5f,.78f));
                    Tri(P(i,.34f,1),P(i,.5f,.78f),P(i+1,.5f,.78f));Tri(P(i,.34f,1),P(i+1,.5f,.78f),P(i+1,.34f,1));
                    Tri(Vector3.up*.5f,P(i+1,.5f,.78f),P(i,.5f,.78f));Tri(Vector3.down*.5f,P(i,-.5f,.78f),P(i+1,-.5f,.78f));
                }
                shell=new Mesh{name="Closed chamfered armor"};shell.SetVertices(vertices);shell.SetTriangles(triangles,0);shell.RecalculateNormals();shell.RecalculateBounds();
            }
            var node=VisualFactory.Node(name,parent,position);node.localScale=size;node.localRotation=Quaternion.Euler(0,0,rotation);
            node.gameObject.AddComponent<MeshFilter>().sharedMesh=shell;node.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;return node;
        }
    }
}
