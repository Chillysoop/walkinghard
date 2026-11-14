using UnityEngine;

namespace VectorGame
{
    public sealed partial class FighterRig
    {
        public void CrewEquipment(CrewRole role)
        {
            if(lightweight||role==CrewRole.None)return;CinderworksWorld.InitializeMaterials();
            var root=VisualFactory.Node("Crew / "+role,Body,Vector3.zero);
            var iron=CinderworksWorld.Iron;var ceramic=CinderworksWorld.Ceramic;
            void Box(string name,Vector3 p,Vector3 size,Material m)=>VisualFactory.Part(name,root,PrimitiveType.Cube,p,size,m);
            if(role==CrewRole.Repair)
            {
                Box("White repair pack",new Vector3(0,.42f,-.42f),new Vector3(.75f,.75f,.35f),ceramic);
                Box("Repair pack cross upright",new Vector3(0,.42f,-.61f),new Vector3(.13f,.53f,.04f),iron);
                Box("Repair pack cross arms",new Vector3(0,.42f,-.61f),new Vector3(.5f,.12f,.04f),iron);
                Box("Repair antenna",new Vector3(.44f,.97f,-.35f),new Vector3(.08f,.9f,.08f),ceramic);
                Box("Repair antenna tip",new Vector3(.44f,1.44f,-.35f),new Vector3(.22f,.18f,.22f),VisualFactory.Glow);
                VisualFactory.Part("Repair welder",RightHand,PrimitiveType.Cube,new Vector3(0,0,.17f),new Vector3(.2f,.2f,.5f),ceramic);
            }
            else if(role==CrewRole.Freight)
            {
                foreach(int side in new[]{-1,1})
                {
                    Box("Freight shoulder piston",new Vector3(side*.55f,.45f,0),new Vector3(.38f,.85f,.5f),CinderworksWorld.Enamel);
                    Box("Piston warning band",new Vector3(side*.55f,.63f,.27f),new Vector3(.36f,.16f,.06f),ceramic);
                    Box("Throwing fork",new Vector3(side*.38f,.12f,.62f),new Vector3(.14f,.22f,.8f),iron);
                }
                Box("Freight counterweight",new Vector3(0,.3f,-.55f),new Vector3(1,.7f,.4f),iron);
            }
            else
            {
                Box("Retrieval boom",new Vector3(0,1.1f,-.3f),new Vector3(.16f,1.5f,.18f),iron);
                Box("Retrieval crossbar",new Vector3(.35f,1.82f,-.3f),new Vector3(.85f,.16f,.18f),ceramic);
                Box("Retrieval hook",new Vector3(.72f,1.55f,-.3f),new Vector3(.12f,.55f,.14f),ceramic);
                Box("Hook return",new Vector3(.63f,1.3f,-.3f),new Vector3(.3f,.12f,.14f),ceramic);
                var spool=VisualFactory.Part("Retrieval cable drum",root,PrimitiveType.Cylinder,new Vector3(0,.4f,-.48f),new Vector3(.65f,.3f,.65f),iron);spool.localRotation=Quaternion.Euler(0,0,90);
            }
            PresentationGeometry.CompactBone(root,"cinderworks/crew/"+role+"/v1");
        }
    }
}
