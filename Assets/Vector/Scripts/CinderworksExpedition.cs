using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class CinderworksWorld
    {
        readonly Dictionary<Renderer,Material> roadMaterials=new Dictionary<Renderer,Material>();
        readonly Dictionary<RoadPiece,List<(int z,Transform bay)>> roadBays=new Dictionary<RoadPiece,List<(int,Transform)>>();
        Transform roadDecoration;
        public Transform RoadDecoration=>roadDecoration;
        public void ResetRoads()
        {
            foreach(var p in roadMaterials)if(p.Key!=null)p.Key.sharedMaterial=p.Value;roadMaterials.Clear();roadBays.Clear();
            if(roadDecoration!=null){roadDecoration.gameObject.SetActive(false);Destroy(roadDecoration.gameObject);roadDecoration=null;}
        }
        public void DecorateRoads(PhysicalRoads roads)
        {
            InitializeMaterials();
            if(roadDecoration==null)roadDecoration=VisualFactory.Node("Cinderworks / branching architecture",transform,Vector3.zero);
            foreach(var road in roads.Pieces)
            {
                foreach(var r in road.Root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    // Branch selection remains cyan, independent of the region's furnace glow.
                    if(r.name=="Branch edge light"||roadMaterials.ContainsKey(r))continue;
                    var m=r.sharedMaterial;roadMaterials.Add(r,m);
                    r.sharedMaterial=m==VisualFactory.Ground?Floor:m==VisualFactory.Stone?Iron:m==VisualFactory.Trim?Ceramic:m==VisualFactory.Glow?Heat:m;
                }
                if(!roadBays.TryGetValue(road,out var bays))
                {
                    bays=new List<(int,Transform)>();roadBays.Add(road,bays);
                    var root=VisualFactory.Node("Road furnace modules",roadDecoration,road.Root.position);root.rotation=road.Rotation;
                    // The first 36m of a branch stay free of large machinery at the Y.
                    for(int z=road.Parent==null?-225:36;z+18<road.End-3;z+=18)bays.Add((z,BuildBay(root,z)));
                }
                foreach(var bay in bays)bay.bay.gameObject.SetActive(bay.z+18<road.End-3);
            }
        }
    }
    public sealed partial class VectorEncounter
    {
        void ResetExpeditionAppearance()
        {
            regionalWorld.ResetRoads();
            if(CombatPreviewActive)return;
            regionalWorld.SetVisible(false);VisualFactory.CinderworksStyle=ExpeditionActive;
            if(ExpeditionActive){VisualFactory.ArmoredStyle=true;regionalWorld.DecorateRoads(roads);}
            else VisualFactory.ArmoredStyle=defaultArmoredStyle;
        }
        void RefreshExpeditionRoads(){if(ExpeditionActive)regionalWorld.DecorateRoads(roads);}
        public string WaveFormation=>DescribeExpeditionFormation(Wave);
        string DescribeExpeditionFormation(int wave)
        {
            if(!ExpeditionActive||wave<2||wave==8)return Formation(wave);
            return wave==2?"REPAIR CREW · stagger or launch interrupts protection":wave==3?"FREIGHT BRUTE · interrupt the throw or launch its body first":wave==4?"RECLAIMERS · retrieval tethers move bodies away from you":"CINDERWORKS CREWS · interrupt repairs, throws and retrieval";
        }
    }
}
