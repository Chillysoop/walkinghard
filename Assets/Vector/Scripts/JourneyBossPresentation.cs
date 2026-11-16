using UnityEngine;

namespace VectorGame
{
    public sealed partial class FighterRig
    {
        Transform[] commandBells,dispatchShutters;
        Renderer[] commandLights;
        Renderer dispatchCore;
        public void JourneyBossEquipment(int kind)
        {
            if(lightweight)return;CinderworksWorld.InitializeMaterials();
            if(kind==1&&commandBells==null)
            {
                commandBells=new Transform[3];commandLights=new Renderer[3];
                var frame=VisualFactory.Node("Bell Foreman / command frame",Body,Vector3.zero);
                foreach(int side in new[]{-1,1})
                {
                    VisualFactory.Part("Conductor upright",frame,PrimitiveType.Cube,new Vector3(side*.8f,.85f,-.25f),new Vector3(.16f,1.9f,.22f),CinderworksWorld.Ceramic);
                    VisualFactory.Part("Conductor shoulder",frame,PrimitiveType.Cube,new Vector3(side*.62f,.48f,.04f),new Vector3(.42f,.42f,.62f),VisualFactory.GoldGlow);
                }
                VisualFactory.Part("Three-bell crown",frame,PrimitiveType.Cube,new Vector3(0,1.8f,-.25f),new Vector3(1.9f,.18f,.25f),CinderworksWorld.Ceramic);
                for(int i=0;i<3;i++)
                {
                    var bell=commandBells[i]=VisualFactory.Node("Command bell "+(i+1),Body,new Vector3((i-1)*.58f,1.64f,-.24f));
                    VisualFactory.Part("Bell shell",bell,PrimitiveType.Cylinder,new Vector3(0,-.23f,0),new Vector3(.42f,.24f,.42f),CinderworksWorld.Enamel);
                    VisualFactory.Part("Flared bell rim",bell,PrimitiveType.Cylinder,new Vector3(0,-.47f,0),new Vector3(.54f,.06f,.54f),CinderworksWorld.Ceramic);
                    commandLights[i]=VisualFactory.Part("Linked escort lamp",bell,PrimitiveType.Cube,new Vector3(0,-.23f,.23f),new Vector3(.23f,.18f,.06f),VisualFactory.GoldGlow).GetComponent<Renderer>();
                }
                VisualFactory.Part("Conductor baton",RightHand,PrimitiveType.Cylinder,new Vector3(0,.15f,.1f),new Vector3(.09f,.58f,.09f),CinderworksWorld.Ceramic);
                PresentationGeometry.CompactBone(frame,"journey/conductor/frame/v1");
                JourneyBossPose(1,0,false,false);
            }
            if(kind==2&&dispatchShutters==null)
            {
                dispatchShutters=new Transform[2];
                dispatchCore=VisualFactory.Part("Final dispatch core",Body,PrimitiveType.Cube,new Vector3(0,.3f,.48f),new Vector3(.52f,.5f,.06f),VisualFactory.RedGlow).GetComponent<Renderer>();
                for(int i=0;i<2;i++)
                {
                    float side=i==0?-1:1;var shutter=dispatchShutters[i]=VisualFactory.Node("Execution shutter",Body,new Vector3(side*.32f,.3f,.53f));
                    VisualFactory.Part("Sealed dispatch plate",shutter,PrimitiveType.Cube,new Vector3(-side*.16f,0,0),new Vector3(.32f,.6f,.1f),CinderworksWorld.Ceramic);
                    VisualFactory.Part("Execution red stripe",shutter,PrimitiveType.Cube,new Vector3(-side*.16f,.08f,.065f),new Vector3(.24f,.09f,.04f),VisualFactory.RedGlow);
                }
                JourneyBossPose(2,0,false,false);
            }
        }
        public void JourneyBossPose(int kind,int links,bool commanding,bool open)
        {
            if(kind==1&&commandBells!=null)for(int i=0;i<3;i++)
            {
                commandBells[i].localRotation=Quaternion.Euler(commanding&&i<links?25:0,0,0);
                commandLights[i].sharedMaterial=open?VisualFactory.Glow:commanding&&i<links?VisualFactory.GoldGlow:CinderworksWorld.Iron;
            }
            if(kind==2&&dispatchShutters!=null)
            {
                for(int i=0;i<2;i++)dispatchShutters[i].localRotation=Quaternion.Euler(0,open?(i==0?-105:105):0,0);
                dispatchCore.sharedMaterial=open?VisualFactory.Glow:VisualFactory.RedGlow;
            }
        }
    }
}
