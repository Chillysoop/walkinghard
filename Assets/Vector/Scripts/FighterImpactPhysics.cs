using UnityEngine;

namespace VectorGame
{
    public sealed partial class FighterPhysics
    {
        readonly bool[] cutJoints=new bool[11];
        public int DetachedParts {get;private set;}
        public int LastCutBone {get;private set;}=-1;
        public bool CutAt(int bone)=>bone>=0&&bone<cutJoints.Length&&cutJoints[bone];
        public void Sever(ImpactReaction hit)
        {
            if(!Ragdoll)return;
            Severed=true;if(fast)return;
            var bones=rig.PhysicsBones;int primary=-1;float closest=float.MaxValue;
            for(int i=1;i<bones.Length;i++)
            {
                if(cutJoints[i])continue;
                float distance=(bones[i].position-hit.point).sqrMagnitude;
                if(distance<closest){closest=distance;primary=i;}
            }
            if(primary<0)return;
            Detach(primary,hit);LastCutBone=primary;
            if(!hit.wide)return;
            // A broad two-hand cut can cross additional limbs, capped at three seams.
            // Geometry and blade plane choose the parts; enemy IDs/random rolls do not.
            for(int n=0;n<2;n++)
            {
                int next=-1;float score=float.MaxValue;
                for(int i=2;i<bones.Length;i++)
                {
                    if(cutJoints[i])continue;
                    Vector3 offset=bones[i].position-hit.point;
                    float plane=Mathf.Abs(Vector3.Dot(offset,hit.normal));
                    if(plane>.24f||offset.sqrMagnitude>1.1f*1.1f)continue;
                    float d=plane*2+offset.magnitude;
                    if(d<score){score=d;next=i;}
                }
                if(next<0)break;Detach(next,hit);
            }
        }
        void Detach(int index,ImpactReaction hit)
        {
            if(cutJoints[index])return;
            var bone=rig.PhysicsBones[index];var joint=bone.GetComponent<CharacterJoint>();
            if(joint==null)return;
            var parent=joint.connectedBody;Vector3 seam=bone.position;
            // Combat resolves before manual Physics.Simulate, never from a collision callback.
            // Remove the constraint now so all fixed substeps see the separated body.
            Object.DestroyImmediate(joint);bone.SetParent(rig.Root,true);cutJoints[index]=true;DetachedParts++;
            Vector3 size=index==1?new Vector3(.48f,.025f,.31f):index==2?new Vector3(.23f,.02f,.23f):new Vector3(.20f,.02f,.20f);
            CutCap(bone,seam,hit.normal,size,"Severed surface");if(parent!=null)CutCap(parent.transform,seam,-hit.normal,size,"Exposed surface");
            Vector3 side=hit.direction*hit.strength+hit.normal*.65f;
            bodies[index].AddForce(side*2.1f,ForceMode.VelocityChange);
            bodies[index].AddTorque(Vector3.Cross(hit.normal,hit.direction)*3.2f*hit.strength,ForceMode.VelocityChange);
            if(parent!=null)parent.AddForce(-side*.7f,ForceMode.VelocityChange);
        }
        static void CutCap(Transform parent,Vector3 point,Vector3 normal,Vector3 size,string name)
        {
            var cap=VisualFactory.Part(name,parent,PrimitiveType.Cylinder,Vector3.zero,size,VisualFactory.RedGlow);
            cap.position=point;cap.rotation=Quaternion.FromToRotation(Vector3.up,normal);
        }
        public void ReactToImpact(ImpactReaction hit)
        {
            if(fast||!Ragdoll)return;
            int nearest=0;float closest=float.MaxValue;
            for(int i=0;i<bodies.Count;i++){float d=(bodies[i].worldCenterOfMass-hit.point).sqrMagnitude;if(d<closest){closest=d;nearest=i;}}
            Vector3 direction=hit.direction;float strength=hit.strength;
            if(hit.kind==ImpactKind.Crush)
            {
                // The struck upper body folds down while the hips keep their momentum.
                bodies[nearest].AddForce((direction*2.2f+Vector3.down*2.5f)*strength,ForceMode.VelocityChange);
                bodies[1].AddTorque(Vector3.Cross(Vector3.up,direction)*6*strength,ForceMode.VelocityChange);
            }
            else if(hit.kind==ImpactKind.Electric)
            {
                bodies[1].AddTorque(rig.Root.right*4,ForceMode.VelocityChange);
                bodies[3].AddTorque(rig.Root.forward*5,ForceMode.VelocityChange);bodies[5].AddTorque(-rig.Root.forward*5,ForceMode.VelocityChange);
            }
            else if(hit.kind!=ImpactKind.Slash)
            {
                float force=hit.kind==ImpactKind.Force?2.5f:hit.kind==ImpactKind.Bullet?1.3f:1.8f;
                bodies[nearest].AddForceAtPosition(direction*force*strength,hit.point,ForceMode.VelocityChange);
                bodies[nearest].AddTorque(Vector3.Cross(Vector3.up,direction)*2*strength,ForceMode.VelocityChange);
            }
        }
        public void ElectricalSpasm(float time,float weight,float dt,bool reducedMotion)
        {
            if(fast||!Ragdoll)return;
            float pulse=Mathf.Sin(time*95)*weight*(reducedMotion?1.5f:5);
            // Opposing impulses create a short joint spasm, not unlimited upward energy.
            bodies[3].AddTorque(rig.Root.forward*pulse*dt*60,ForceMode.VelocityChange);
            bodies[5].AddTorque(-rig.Root.forward*pulse*dt*60,ForceMode.VelocityChange);
            bodies[2].AddTorque(rig.Root.right*pulse*.25f*dt*60,ForceMode.VelocityChange);
        }
    }
}
