using UnityEngine;

namespace VectorGame
{
    public sealed partial class FighterRig
    {
        Transform[] foremanFeet,foremanDoors;
        Renderer[] foremanGauge;
        public void ForemanEquipment()
        {
            if(lightweight||foremanFeet!=null)return;CinderworksWorld.InitializeMaterials();
            foremanFeet=new Transform[2];foremanDoors=new Transform[2];foremanGauge=new Renderer[5];
            var shell=VisualFactory.Node("Foreman furnace suit",Body,Vector3.zero);
            foreach(int side in new[]{-1,1})
            {
                VisualFactory.Part("Pressure shoulder",shell,PrimitiveType.Cylinder,new Vector3(side*.62f,.6f,-.12f),new Vector3(.52f,.48f,.52f),CinderworksWorld.Enamel);
                VisualFactory.Part("Ceramic shoulder cap",shell,PrimitiveType.Cube,new Vector3(side*.62f,1.06f,-.12f),new Vector3(.62f,.16f,.58f),CinderworksWorld.Ceramic);
                var foot=VisualFactory.Node("Planted stabilizer",pelvis,new Vector3(side*.43f,.1f,-.08f));foremanFeet[(side+1)/2]=foot;
                VisualFactory.Part("Stabilizer strut",foot,PrimitiveType.Cube,new Vector3(side*.24f,-.45f,0),new Vector3(.18f,1.05f,.24f),CinderworksWorld.Ceramic);
                VisualFactory.Part("Stabilizer foot",foot,PrimitiveType.Cube,new Vector3(side*.24f,-.94f,.12f),new Vector3(.5f,.1f,.55f),CinderworksWorld.Iron);
                var door=VisualFactory.Node("Opening furnace shutter",Body,new Vector3(side*.38f,.35f,.39f));foremanDoors[(side+1)/2]=door;
                VisualFactory.Part("Furnace shutter",door,PrimitiveType.Cube,new Vector3(-side*.19f,0,.05f),new Vector3(.37f,.7f,.15f),CinderworksWorld.Iron);
                VisualFactory.Part("Shutter warning stripe",door,PrimitiveType.Cube,new Vector3(-side*.19f,.2f,.14f),new Vector3(.3f,.13f,.03f),CinderworksWorld.Ceramic);
            }
            VisualFactory.Part("Exposed furnace core",shell,PrimitiveType.Cube,new Vector3(0,.35f,.34f),new Vector3(.7f,.65f,.15f),VisualFactory.Glow);
            for(int i=0;i<5;i++)foremanGauge[i]=VisualFactory.Part("Pressure gauge",Body,PrimitiveType.Cube,new Vector3(-.26f+i*.13f,.83f,.28f),new Vector3(.09f,.13f,.1f),CinderworksWorld.Iron).GetComponent<Renderer>();
            VisualFactory.Part("Press handle",RightHand,PrimitiveType.Cylinder,new Vector3(0,-.05f,.3f),new Vector3(.13f,.55f,.13f),CinderworksWorld.Ceramic);
            VisualFactory.Part("Press head",RightHand,PrimitiveType.Cube,new Vector3(0,.45f,.3f),new Vector3(.75f,.4f,.55f),CinderworksWorld.Iron);
            PresentationGeometry.CompactBone(shell,"cinderworks/foreman/v1");ForemanPose(false,false,0);
        }
        public void ForemanPose(bool braced,bool venting,float pressure)
        {
            if(foremanFeet==null)return;
            for(int i=0;i<2;i++)
            {
                float side=i==0?-1:1;foremanFeet[i].localRotation=Quaternion.Euler(0,0,side*(braced?18:76));
                foremanDoors[i].localRotation=Quaternion.Euler(0,venting?side*115:0,0);
            }
            for(int i=0;i<5;i++)foremanGauge[i].sharedMaterial=venting?VisualFactory.Glow:pressure>i*.2f?CinderworksWorld.Heat:CinderworksWorld.Iron;
        }
    }
}
