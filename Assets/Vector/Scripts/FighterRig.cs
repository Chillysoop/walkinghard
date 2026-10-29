using UnityEngine;

namespace VectorGame
{
    // Purpose-built articulated mannequins. The joints remain replaceable by imported animation later.
    public sealed class FighterRig
    {
        public Transform Root, Body, Head, LeftHand, RightHand;
        readonly Transform leftArm, rightArm, leftElbow, rightElbow, leftThigh, rightThigh, leftKnee, rightKnee;
        readonly Transform coatLeft, coatRight;
        readonly Transform intent, leftCuff, rightCuff;
        readonly Renderer intentRenderer;
        readonly Transform pelvis;
        public Transform[] PhysicsBones=>new[]{pelvis,Body,Head,leftArm,leftElbow,rightArm,rightElbow,leftThigh,leftKnee,rightThigh,rightKnee};
        readonly bool hero;
        readonly float phase;
        Transform weaponRight,weaponLeft;
        WeaponKind displayedRight=WeaponKind.Fists,displayedLeft=WeaponKind.Fists;
        bool twoHanded;
        internal Transform RightWeapon=>weaponRight;
        internal Transform LeftWeapon=>weaponLeft;
        public FighterRig(string name, Transform parent, bool isHero, bool heavy = false)
        {
            hero=isHero;phase=isHero?0:Random.value*6.28f;
            Root=VisualFactory.Node(name,parent,Vector3.zero);
            var hips=VisualFactory.Node("Pelvis",Root,new Vector3(0,1.02f,0));pelvis=hips;
            var cloth=hero?VisualFactory.Hero:heavy?VisualFactory.EnemyHeavy:VisualFactory.Enemy;
            var plates=hero?VisualFactory.Armor:cloth;
            var light=hero?VisualFactory.Glow:VisualFactory.RedGlow;
            VisualFactory.Part("Hip armor",hips,PrimitiveType.Cube,Vector3.zero,new Vector3(.48f,.3f,.35f),VisualFactory.Joint);
            Body=VisualFactory.Node("Spine",hips,new Vector3(0,.08f,0));
            VisualFactory.Part("Torso",Body,PrimitiveType.Cube,new Vector3(0,.32f,0),new Vector3(.65f,.58f,.38f),cloth);
            VisualFactory.Part("Chest plate",Body,PrimitiveType.Cube,new Vector3(0,.39f,.21f),new Vector3(.52f,.39f,.06f),plates);
            VisualFactory.Part("Collar",Body,PrimitiveType.Cube,new Vector3(0,.65f,0),new Vector3(.48f,.13f,.4f),cloth);
            VisualFactory.Part("Spine rig",Body,PrimitiveType.Cube,new Vector3(0,.35f,-.208f),new Vector3(.09f,.52f,.065f),light);
            Head=VisualFactory.Node("Head",Body,new Vector3(0,.85f,0));
            VisualFactory.Part("Helmet",Head,PrimitiveType.Sphere,Vector3.zero,new Vector3(.39f,.43f,.38f),hero?VisualFactory.Joint:plates);
            VisualFactory.Part("Visor",Head,PrimitiveType.Cube,new Vector3(0,.035f,.184f),new Vector3(.30f,.075f,.055f),light);
            if(hero)
            {
                VisualFactory.Part("Scarf",Body,PrimitiveType.Cube,new Vector3(0,.62f,-.20f),new Vector3(.5f,.14f,.16f),VisualFactory.Armor);
                coatLeft=VisualFactory.Part("Coat tail left",hips,PrimitiveType.Cube,new Vector3(-.19f,-.32f,-.20f),new Vector3(.32f,.65f,.09f),cloth);
                coatRight=VisualFactory.Part("Coat tail right",hips,PrimitiveType.Cube,new Vector3(.19f,-.32f,-.20f),new Vector3(.32f,.65f,.09f),cloth);
            }
            leftArm=Arm(-1,Body,plates,light,out leftElbow,out LeftHand);
            rightArm=Arm(1,Body,plates,light,out rightElbow,out RightHand);
            if(hero)
            {
                leftCuff=VisualFactory.Part("Flux bracer L",leftElbow,PrimitiveType.Cube,new Vector3(0,-.25f,0),new Vector3(.31f,.16f,.30f),VisualFactory.Glow);
                rightCuff=VisualFactory.Part("Flux bracer R",rightElbow,PrimitiveType.Cube,new Vector3(0,-.25f,0),new Vector3(.31f,.16f,.30f),VisualFactory.Glow);
                Equipment(false,false);
            }
            else
            {
                intent=VisualFactory.Part("Attack opening / stagger",Root,PrimitiveType.Cylinder,new Vector3(0,.025f,0),new Vector3(.9f,.012f,.9f),VisualFactory.RedGlow);
                intentRenderer=intent.GetComponent<Renderer>();intentRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;intent.gameObject.SetActive(false);
            }
            leftThigh=Leg(-1,hips,cloth,out leftKnee);rightThigh=Leg(1,hips,cloth,out rightKnee);
            if(heavy){Root.localScale=Vector3.one*1.17f;VisualFactory.Part("Reactor",Body,PrimitiveType.Cube,new Vector3(0,.4f,.255f),new Vector3(.13f,.21f,.04f),light);}
        }
        Transform Arm(int side,Transform parent,Material armor,Material light,out Transform elbow,out Transform hand)
        {
            var shoulder=VisualFactory.Node(side<0?"Left shoulder":"Right shoulder",parent,new Vector3(side*.43f,.53f,0));
            VisualFactory.Part("Pauldron",shoulder,PrimitiveType.Sphere,new Vector3(0,-.02f,0),new Vector3(.30f,.33f,.32f),armor);
            VisualFactory.Part("Upper arm",shoulder,PrimitiveType.Capsule,new Vector3(0,-.20f,0),new Vector3(.20f,.22f,.20f),VisualFactory.Joint);
            elbow=VisualFactory.Node("Elbow",shoulder,new Vector3(0,-.40f,0));
            VisualFactory.Part("Forearm",elbow,PrimitiveType.Capsule,new Vector3(0,-.17f,0),new Vector3(.24f,.20f,.24f),armor);
            VisualFactory.Part("Wrist capacitor",elbow,PrimitiveType.Cube,new Vector3(0,-.28f,.11f),new Vector3(.16f,.12f,.045f),light);
            hand=VisualFactory.Node("Fist",elbow,new Vector3(0,-.39f,0));
            VisualFactory.Part("Glove",hand,PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.22f,.24f),armor);
            return shoulder;
        }
        Transform Leg(int side,Transform parent,Material material,out Transform knee)
        {
            var thigh=VisualFactory.Node(side<0?"Left hip":"Right hip",parent,new Vector3(side*.185f,-.06f,0));
            VisualFactory.Part("Thigh",thigh,PrimitiveType.Capsule,new Vector3(0,-.20f,0),new Vector3(.24f,.24f,.25f),material);
            knee=VisualFactory.Node("Knee",thigh,new Vector3(0,-.43f,0));
            VisualFactory.Part("Shin",knee,PrimitiveType.Capsule,new Vector3(0,-.19f,0),new Vector3(.21f,.23f,.23f),VisualFactory.Joint);
            VisualFactory.Part("Boot",knee,PrimitiveType.Cube,new Vector3(0,-.39f,.09f),new Vector3(.26f,.20f,.43f),VisualFactory.Joint);
            return thigh;
        }
        public void Pose(float clock,float speed,int strike,float progress,float hitReact,bool pushing,float pushProgress,int weaponHand=-1,bool pairedAttack=false,bool twoHandedAttack=false)
        {
            float cycle=clock*(hero?8:10)+phase;
            float stride=Mathf.Sin(cycle)*Mathf.Clamp01(speed)*26f;
            float bob=Mathf.Abs(Mathf.Sin(cycle))*.035f*Mathf.Clamp01(speed);
            Body.localPosition=new Vector3(0,.08f+bob,0);
            Body.localRotation=Quaternion.Euler(hitReact*18f,Mathf.Sin(cycle)*3f,0);
            Head.localRotation=Quaternion.Euler(-4,0,0);
            leftThigh.localRotation=Quaternion.Euler(stride,0,-2);rightThigh.localRotation=Quaternion.Euler(-stride,0,2);
            leftKnee.localRotation=Quaternion.Euler(Mathf.Max(0,-stride)*1.4f,0,0);rightKnee.localRotation=Quaternion.Euler(Mathf.Max(0,stride)*1.4f,0,0);
            leftArm.localRotation=Quaternion.Euler(-35,0,-12);rightArm.localRotation=Quaternion.Euler(-25,0,12);
            leftElbow.localRotation=Quaternion.Euler(-75,0,0);rightElbow.localRotation=Quaternion.Euler(-85,0,0);
            if(coatLeft!=null){coatLeft.localRotation=Quaternion.Euler(-12+stride*.25f,0,4);coatRight.localRotation=Quaternion.Euler(-12-stride*.25f,0,-4);}
            if(strike>=0)
            {
                // Pull back -> rapid extension -> follow through -> return to guard.
                float extension=progress<.27f?0:progress<.44f?Mathf.SmoothStep(0,1,(progress-.27f)/.17f):1-Mathf.SmoothStep(0,1,(progress-.44f)/.56f);
                if(strike==0)
                {
                    leftArm.localRotation=Quaternion.Euler(Mathf.Lerp(-25,-105,extension),-12,Mathf.Lerp(-18,8,extension));
                    leftElbow.localRotation=Quaternion.Euler(Mathf.Lerp(-105,-5,extension),0,0);
                    Body.localRotation=Quaternion.Euler(0,Mathf.Lerp(12,-19,extension),-extension*5);
                }
                else if(strike==1)
                {
                    rightArm.localRotation=Quaternion.Euler(Mathf.Lerp(-20,-112,extension),15,Mathf.Lerp(22,-10,extension));
                    rightElbow.localRotation=Quaternion.Euler(Mathf.Lerp(-105,-7,extension),0,0);
                    Body.localRotation=Quaternion.Euler(-extension*5,Mathf.Lerp(-22,27,extension),extension*5);
                }
                else
                {
                    // Wide, two-arm palm strike reads from the chase camera and clears a cone.
                    Body.localRotation=Quaternion.Euler(Mathf.Lerp(-9,15,extension),Mathf.Lerp(-20,15,extension),0);
                    leftArm.localRotation=Quaternion.Euler(Mathf.Lerp(-32,-96,extension),-20,Mathf.Lerp(-40,-10,extension));
                    rightArm.localRotation=Quaternion.Euler(Mathf.Lerp(-32,-96,extension),20,Mathf.Lerp(40,10,extension));
                    leftElbow.localRotation=rightElbow.localRotation=Quaternion.Euler(Mathf.Lerp(-90,-4,extension),0,0);
                    rightThigh.localRotation=Quaternion.Euler(-18*extension,0,8);leftThigh.localRotation=Quaternion.Euler(12*extension,0,-8);
                }
                Body.localPosition+=new Vector3(0,-extension*.045f,extension*(strike==2?.23f:.13f));
            }
            if(strike>=3)
            {
                float swing=Mathf.SmoothStep(0,1,Mathf.Clamp01((progress-.16f)/.38f));
                float returnToGuard=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((progress-.65f)/.35f));
                if(strike==3)
                {
                    Body.localRotation=Quaternion.Euler(0,Mathf.Lerp(-48,48,swing)*returnToGuard,0);
                    rightArm.localRotation=Quaternion.Euler(-95,Mathf.Lerp(55,-35,swing),15);
                    rightElbow.localRotation=Quaternion.Euler(-12,0,0);
                }
                if(strike==4)
                {
                    rightArm.localRotation=Quaternion.Euler(Mathf.Lerp(-170,-65,swing),0,12);
                    rightElbow.localRotation=Quaternion.Euler(-15,0,0);
                    Body.localRotation=Quaternion.Euler(Mathf.Lerp(-15,24,swing)*returnToGuard,0,0);
                }
                if(strike==5){rightArm.localRotation=Quaternion.Euler(-90,0,8);rightElbow.localRotation=Quaternion.Euler(-5,0,0);}
            }
            if(weaponHand>=0)
            {
                // Weapon poses are authored on the right; the fist jab starts on the left.
                // Resolve the attacking hand from the mount, never from the sequence position.
                if(strike==0){rightArm.localRotation=Mirror(leftArm.localRotation);rightElbow.localRotation=Mirror(leftElbow.localRotation);Body.localRotation=Mirror(Body.localRotation);}
                leftArm.localRotation=Quaternion.Euler(-35,0,-12);leftElbow.localRotation=Quaternion.Euler(-75,0,0);
                if(weaponHand==1||pairedAttack||twoHandedAttack)
                {
                    leftArm.localRotation=Mirror(rightArm.localRotation);leftElbow.localRotation=Mirror(rightElbow.localRotation);
                    if(weaponHand==1&&!pairedAttack&&!twoHandedAttack)
                    {
                        rightArm.localRotation=Quaternion.Euler(-25,0,12);rightElbow.localRotation=Quaternion.Euler(-85,0,0);
                        Body.localRotation=Mirror(Body.localRotation);
                    }
                }
            }
            if(pushing)
            {
                float extension=Mathf.SmoothStep(0,1,Mathf.Clamp01((pushProgress-.18f)/.28f));
                if(pushProgress>.65f)extension*=1-(pushProgress-.65f)/.35f;
                Body.localRotation=Quaternion.Euler(8*extension,0,0);
                leftArm.localRotation=Quaternion.Euler(-85*extension-20,0,-22);rightArm.localRotation=Quaternion.Euler(-85*extension-20,0,22);
                leftElbow.localRotation=rightElbow.localRotation=Quaternion.Euler(Mathf.Lerp(-110,-5,extension),0,0);
            }
            if(hitReact>0)
            {
                Body.localRotation*=Quaternion.Euler(-27*hitReact,0,hitReact*7);
                Body.localPosition+=Vector3.back*hitReact*.16f;
                Head.localRotation=Quaternion.Euler(-18*hitReact,0,0);
            }
        }
        public void Telegraph(bool attacking,bool stunned)
        {
            if(intent==null)return;
            intent.gameObject.SetActive(attacking||stunned);intentRenderer.sharedMaterial=stunned?VisualFactory.Glow:VisualFactory.RedGlow;
            intent.localScale=new Vector3(attacking?1.2f:.72f,.012f,attacking?1.2f:.72f);
        }
        public void Equipment(bool launch,bool pinball)
        {
            if(leftCuff==null)return;
            leftCuff.gameObject.SetActive(launch);rightCuff.gameObject.SetActive(launch);
            leftCuff.GetComponent<Renderer>().sharedMaterial=rightCuff.GetComponent<Renderer>().sharedMaterial=pinball?VisualFactory.GoldGlow:VisualFactory.Glow;
        }
        static Quaternion Mirror(Quaternion q)=>new Quaternion(q.x,-q.y,-q.z,q.w);
        public void EquipWeapons(WeaponKind right,WeaponKind left,bool bothHands)
        {
            if(right==displayedRight&&left==displayedLeft&&bothHands==twoHanded)return;
            if(weaponRight!=null)Object.Destroy(weaponRight.gameObject);
            if(weaponLeft!=null)Object.Destroy(weaponLeft.gameObject);
            displayedRight=right;displayedLeft=left;twoHanded=bothHands;
            weaponRight=right==WeaponKind.Fists?null:MakeWeapon(RightHand,right);
            weaponLeft=left==WeaponKind.Fists||bothHands?null:MakeWeapon(LeftHand,left);
        }
        Transform MakeWeapon(Transform hand,WeaponKind kind)
        {
            var root=VisualFactory.Node(kind.ToString(),hand,Vector3.zero);
            if(kind==WeaponKind.Sword)
            {
                VisualFactory.Part("Blade",root,PrimitiveType.Cube,new Vector3(0,-.05f,.85f),new Vector3(.09f,.16f,1.45f),VisualFactory.Glow);
                VisualFactory.Part("Guard",root,PrimitiveType.Cube,new Vector3(0,0,.15f),new Vector3(.42f,.15f,.10f),VisualFactory.Armor);
            }
            else if(kind==WeaponKind.Hammer)
            {
                VisualFactory.Part("Haft",root,PrimitiveType.Cube,new Vector3(0,0,.65f),new Vector3(.1f,.1f,1.4f),VisualFactory.Trim);
                VisualFactory.Part("Hammer head",root,PrimitiveType.Cube,new Vector3(0,0,1.35f),new Vector3(.95f,.5f,.42f),VisualFactory.Armor);
                VisualFactory.Part("Impact core",root,PrimitiveType.Cube,new Vector3(0,0,1.57f),new Vector3(.6f,.28f,.04f),VisualFactory.GoldGlow);
            }
            else
            {
                float length=kind==WeaponKind.Pistol?.55f:1.1f;
                VisualFactory.Part("Receiver",root,PrimitiveType.Cube,new Vector3(0,0,length*.45f),new Vector3(.18f,.22f,length),VisualFactory.Trim);
                VisualFactory.Part("Muzzle",root,PrimitiveType.Cube,new Vector3(0,0,length),new Vector3(.12f,.12f,.12f),VisualFactory.Glow);
            }
            return root;
        }
        public void PreviewLoadout(WeaponKind right,WeaponKind left,bool twoHandedWeapon)
        {
            EquipWeapons(right,left,twoHandedWeapon);
            Pose(0,0,-1,0,0,false,0);
            leftArm.localRotation=Quaternion.Euler(5,0,-13);rightArm.localRotation=Quaternion.Euler(5,0,13);
            leftElbow.localRotation=rightElbow.localRotation=Quaternion.Euler(-8,0,0);
            if(weaponRight!=null)weaponRight.localRotation=Quaternion.Euler(40,0,0);
            if(weaponLeft!=null)weaponLeft.localRotation=Quaternion.Euler(40,0,0);
        }
        public void Airborne(float remaining)
        {
            float lift=Mathf.Sin(Mathf.Clamp01(1-remaining/1.05f)*Mathf.PI)*.5f;
            Body.localPosition=new Vector3(0,.08f+lift,0);Body.localRotation=Quaternion.Euler(-55,0,12);
            leftArm.localRotation=Quaternion.Euler(-40,0,-55);rightArm.localRotation=Quaternion.Euler(-50,0,55);
            leftThigh.localRotation=Quaternion.Euler(35,0,-12);rightThigh.localRotation=Quaternion.Euler(22,0,12);
            Telegraph(false,false);
        }
    }
}
