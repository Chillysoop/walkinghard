using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class FighterRig
    {
        bool presentationHeavy;
        readonly List<Renderer> originalSurfaces=new List<Renderer>();
        readonly List<Transform> presentationRoots=new List<Transform>();
        bool presentationBuilt,presentationEnabled;
        string weaponIdentity="";
        readonly List<Transform> identityParts=new List<Transform>();
        public void WeaponIdentity(LoadoutItem right,LoadoutItem left)
        {
            if(lightweight)return;
            string key=right.def.id+"/"+left.def.id+"/"+twoHanded+"/"+VisualFactory.ArmoredStyle;
            if(key==weaponIdentity)return;weaponIdentity=key;
            foreach(var part in identityParts)if(part!=null)Object.Destroy(part.gameObject);identityParts.Clear();
            if(!VisualFactory.ArmoredStyle)return;
            void Identity(LoadoutItem item,Transform hand,Transform weapon)
            {
                if(item.def.id=="wraps")
                    for(int i=0;i<3;i++)identityParts.Add(PresentationGeometry.Plate("Conductive knuckle coil",hand,new Vector3((i-1)*.085f,-.015f,.11f),new Vector3(.035f,.21f,.09f),VisualFactory.Glow));
                if(weapon==null)return;
                foreach(var renderer in weapon.GetComponentsInChildren<Renderer>())
                    if(renderer.name=="Blade channel")renderer.sharedMaterial=item.def.id=="siphonblade"?VisualFactory.SiphonGlow:VisualFactory.Glow;
                if(item.def.id=="siphonblade")
                    for(int side=-1;side<=1;side+=2)identityParts.Add(PresentationGeometry.Plate("Siphon blade hook",weapon,new Vector3(side*.14f,0,1.45f),new Vector3(.12f,.11f,.32f),VisualFactory.SiphonGlow,side*12));
            }
            Identity(right,RightHand,weaponRight);if(!twoHanded)Identity(left,LeftHand,weaponLeft);
        }
        public void RangedEquipment()
        {
            if(lightweight)return;
            EquipWeapons(WeaponKind.Pistol,WeaponKind.Fists,false);
            if(VisualFactory.ArmoredStyle)
                PresentationGeometry.Plate("Ranged sight mast",Body,new Vector3(.3f,.81f,-.16f),new Vector3(.09f,.45f,.09f),VisualFactory.GoldGlow);
        }
        Transform SkinNode(Transform bone)
        {var node=VisualFactory.Node("Presentation armor",bone,Vector3.zero);presentationRoots.Add(node);return node;}
        public void ApplyPresentation(bool enabled)
        {
            if(lightweight)return;
            if(enabled&&!presentationBuilt)
            {
                foreach(var r in Root.GetComponentsInChildren<Renderer>(true))
                    if(r!=intentRenderer&&!r.transform.IsChildOf(LeftHand)&&!r.transform.IsChildOf(RightHand)&&r.transform!=leftCuff&&r.transform!=rightCuff)originalSurfaces.Add(r);
                var cloth=hero?VisualFactory.Hero:presentationHeavy?VisualFactory.EnemyHeavy:VisualFactory.Enemy;
                var armor=hero?VisualFactory.Armor:cloth;var dark=VisualFactory.Joint;var light=hero?VisualFactory.Glow:VisualFactory.RedGlow;
                var spine=SkinNode(Body);var head=SkinNode(Head);var hips=SkinNode(pelvis);
                PresentationGeometry.Plate("Tapered cuirass",spine,new Vector3(0,.3f,0),new Vector3(hero?.72f:.62f,.63f,.47f),cloth);
                PresentationGeometry.Plate("Upper breastplate",spine,new Vector3(0,.43f,.25f),new Vector3(.57f,.29f,.11f),armor);
                PresentationGeometry.Plate("Lower breastplate",spine,new Vector3(0,.18f,.24f),new Vector3(.43f,.19f,.10f),armor);
                PresentationGeometry.Plate("Back plate",spine,new Vector3(0,.35f,-.27f),new Vector3(.54f,.43f,.16f),armor);
                for(int side=-1;side<=1;side+=2)
                {
                    PresentationGeometry.Plate("Back vent",spine,new Vector3(side*.18f,.39f,-.36f),new Vector3(.045f,.28f,.035f),light);
                    PresentationGeometry.Plate("Hip guard",hips,new Vector3(side*.22f,-.15f,.02f),new Vector3(.23f,.39f,.40f),armor,side*-9);
                }
                PresentationGeometry.Plate("Helmet shell",head,Vector3.zero,new Vector3(.43f,.47f,.43f),armor);
                PresentationGeometry.Plate("Dark faceplate",head,new Vector3(0,-.01f,.21f),new Vector3(.35f,.28f,.08f),dark);
                PresentationGeometry.Plate("Visor slit",head,new Vector3(0,.06f,.258f),new Vector3(.35f,.062f,.025f),light);
                PresentationGeometry.Plate("Helmet crest",head,new Vector3(0,.24f,-.02f),new Vector3(.1f,.075f,.32f),dark);
                for(int side=0;side<2;side++)
                {
                    float sign=side==0?-1:1;var shoulder=SkinNode(side==0?leftArm:rightArm);var elbow=SkinNode(side==0?leftElbow:rightElbow);
                    var thigh=SkinNode(side==0?leftThigh:rightThigh);var knee=SkinNode(side==0?leftKnee:rightKnee);
                    PresentationGeometry.Plate("Shoulder armor",shoulder,new Vector3(sign*.03f,-.055f,0),new Vector3(presentationHeavy?.46f:.38f,.32f,.44f),armor,sign*-12);
                    PresentationGeometry.Plate("Upper arm",shoulder,new Vector3(0,-.25f,0),new Vector3(.21f,.33f,.25f),dark);
                    PresentationGeometry.Plate("Bracer",elbow,new Vector3(0,-.18f,.025f),new Vector3(.29f,.35f,.31f),armor);
                    PresentationGeometry.Plate("Bracer inset",elbow,new Vector3(0,-.18f,.19f),new Vector3(.12f,.16f,.025f),dark);
                    PresentationGeometry.Plate("Thigh plate",thigh,new Vector3(0,-.22f,.015f),new Vector3(.29f,.37f,.31f),cloth);
                    PresentationGeometry.Plate("Knee guard",knee,new Vector3(0,-.03f,.14f),new Vector3(.28f,.22f,.15f),armor);
                    PresentationGeometry.Plate("Shin plate",knee,new Vector3(0,-.23f,.06f),new Vector3(.24f,.30f,.29f),armor);
                    PresentationGeometry.Plate("Armored boot",knee,new Vector3(0,-.39f,.11f),new Vector3(.29f,.21f,.46f),dark);
                    PresentationGeometry.Plate("Toe cap",knee,new Vector3(0,-.37f,.31f),new Vector3(.29f,.14f,.15f),armor);
                }
                for(int i=0;i<presentationRoots.Count;i++)PresentationGeometry.CompactBone(presentationRoots[i],(hero?"hero":presentationHeavy?"heavy":"raider")+"/"+i);
                presentationBuilt=true;
            }
            foreach(var r in originalSurfaces)if(r!=null)r.enabled=!enabled;
            foreach(var node in presentationRoots)if(node!=null)node.gameObject.SetActive(enabled);
            if(enabled!=presentationEnabled)
            {
                presentationEnabled=enabled;var right=displayedRight;var left=displayedLeft;bool both=twoHanded;
                displayedRight=(WeaponKind)(-1);EquipWeapons(right,left,both);
            }
        }
        Transform MakePresentationWeapon(Transform hand,WeaponKind kind,bool two)
        {
            var root=VisualFactory.Node(kind.ToString(),hand,Vector3.zero);
            void P(string name,Vector3 p,Vector3 size,Material mat)=>PresentationGeometry.Plate(name,root,p,size,mat);
            P("Grip",new Vector3(0,-.035f,.13f),new Vector3(.14f,.19f,.38f),VisualFactory.Joint);
            if(kind==WeaponKind.Sword)
            {
                P("Crossguard",new Vector3(0,0,.27f),new Vector3(two?.63f:.46f,.16f,.16f),VisualFactory.Armor);
                P("Blade steel",new Vector3(0,0,1.04f),new Vector3(two?.33f:.20f,.095f,1.44f),VisualFactory.Armor);
                P("Blade channel",new Vector3(0,.055f,1.03f),new Vector3(.055f,.025f,1.24f),VisualFactory.Glow);
                P("Pommel",new Vector3(0,0,-.1f),new Vector3(.19f,.21f,.15f),VisualFactory.Trim);
            }
            else if(kind==WeaponKind.Hammer)
            {
                P("Reinforced haft",new Vector3(0,0,.7f),new Vector3(.14f,.14f,1.35f),VisualFactory.Trim);
                P("Hammer housing",new Vector3(0,0,1.35f),new Vector3(two?1.05f:.78f,.48f,.48f),VisualFactory.Joint);
                for(int sign=-1;sign<=1;sign+=2)P("Striking face",new Vector3(sign*(two?.48f:.35f),0,1.35f),new Vector3(.18f,.52f,.51f),VisualFactory.Armor);
                P("Impact core",new Vector3(0,.255f,1.35f),new Vector3(.32f,.045f,.29f),VisualFactory.GoldGlow);
            }
            else
            {
                float length=kind==WeaponKind.Pistol?.56f:kind==WeaponKind.Shotgun?.84f:1.25f;
                P("Receiver",new Vector3(0,.02f,length*.45f),new Vector3(.26f,.28f,length*.72f),VisualFactory.Armor);
                P("Barrel",new Vector3(0,.02f,length*.9f),new Vector3(.12f,.13f,length*.48f),VisualFactory.Joint);
                P("Muzzle rail",new Vector3(0,.1f,length),new Vector3(.15f,.045f,.12f),VisualFactory.Glow);
                P("Magazine",new Vector3(0,-.23f,.34f),new Vector3(.17f,.24f,.23f),VisualFactory.Joint);
                if(kind==WeaponKind.Rifle){P("Stock",new Vector3(0,-.025f,-.24f),new Vector3(.2f,.27f,.36f),VisualFactory.Joint);P("Optic",new Vector3(0,.23f,.4f),new Vector3(.14f,.14f,.28f),VisualFactory.Trim);}
                if(kind==WeaponKind.Shotgun)P("Second bore",new Vector3(.2f,.02f,length*.78f),new Vector3(.15f,.19f,length*.64f),VisualFactory.Joint);
            }
            return root;
        }
        void PresentationEncounterEquipment(bool shield,bool carrier)
        {
            if(shield)
            {
                enemyShield=VisualFactory.Node("Stagger opens guard",Body,new Vector3(-.28f,.25f,.5f));
                PresentationGeometry.Plate("Shield rim",enemyShield,Vector3.zero,new Vector3(.98f,1.3f,.19f),VisualFactory.Joint);
                PresentationGeometry.Plate("Shield face",enemyShield,new Vector3(0,0,.09f),new Vector3(.81f,1.1f,.09f),VisualFactory.Trim);
                PresentationGeometry.Plate("Guard slit",enemyShield,new Vector3(0,.16f,.15f),new Vector3(.1f,.65f,.035f),VisualFactory.RedGlow);
                PresentationGeometry.Plate("Shield lower brace",enemyShield,new Vector3(0,-.38f,.15f),new Vector3(.7f,.1f,.035f),VisualFactory.EnemyHeavy);
            }
            if(carrier)
            {
                for(int sign=-1;sign<=1;sign+=2)
                {
                    VisualFactory.Part("Volatile canister",Body,PrimitiveType.Cylinder,new Vector3(sign*.39f,.4f,-.3f),new Vector3(.27f,.4f,.27f),VisualFactory.GoldGlow);
                    foreach(float y in new[]{.12f,.68f})PresentationGeometry.Plate("Canister clamp",Body,new Vector3(sign*.39f,y,-.3f),new Vector3(.34f,.12f,.34f),VisualFactory.Joint);
                }
                PresentationGeometry.Plate("Volatile warning panel",Body,new Vector3(0,.4f,.28f),new Vector3(.4f,.29f,.08f),VisualFactory.GoldGlow);
            }
        }
        void PresentationPursuerArmor()
        {
            for(int side=-1;side<=1;side+=2)
            {
                PresentationGeometry.Plate("Pursuer mantle",Body,new Vector3(side*.51f,.56f,0),new Vector3(.5f,.36f,.66f),VisualFactory.Stone,side*-12);
                PresentationGeometry.Plate("Pursuer mantle marker",Body,new Vector3(side*.52f,.57f,.33f),new Vector3(.3f,.07f,.04f),VisualFactory.RedGlow);
                PresentationGeometry.Plate("Pursuer back tower",Body,new Vector3(side*.24f,.54f,-.34f),new Vector3(.22f,.6f,.25f),VisualFactory.Joint);
            }
            PresentationGeometry.Plate("Pursuer breastplate",Body,new Vector3(0,.33f,.28f),new Vector3(.64f,.58f,.2f),VisualFactory.Trim);
            PresentationGeometry.Plate("Pursuer reactor",Body,new Vector3(0,.38f,.39f),new Vector3(.12f,.27f,.03f),VisualFactory.RedGlow);
        }
    }
}
