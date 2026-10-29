using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed class ContactRelay : MonoBehaviour
    {
        public int owner;public VectorEncounter encounter;
        void OnCollisionEnter(Collision hit){Report(hit);}
        void OnCollisionStay(Collision hit){if(hit.relativeVelocity.sqrMagnitude>1)Report(hit);}
        void Report(Collision hit)
        {
            var other=hit.collider.GetComponentInParent<ContactRelay>();
            if(other!=null&&other.owner==owner)return;
            encounter.QueueContact(owner,other==null?-1:other.owner,hit.relativeVelocity.magnitude,hit.contactCount>0?hit.GetContact(0).point:transform.position);
        }
    }

    // Animated fighters use a dynamic capsule; defeated fighters switch to eleven jointed bodies.
    public sealed class FighterPhysics
    {
        public readonly Rigidbody Motor;
        readonly CapsuleCollider capsule;
        readonly FighterRig rig;
        readonly int owner;
        readonly VectorEncounter encounter;
        readonly List<Rigidbody> bodies=new List<Rigidbody>();
        readonly List<Collider> colliders=new List<Collider>();
        public bool Ragdoll {get;private set;}
        public bool Severed {get;private set;}
        public int BodyCount=>bodies.Count;
        public Vector3 Center=>Ragdoll?bodies[0].position:Motor.position+Vector3.up;
        public Vector3 Velocity=>Ragdoll?bodies[0].linearVelocity:Motor.linearVelocity;
        public FighterPhysics(FighterRig rig,int owner,VectorEncounter encounter,bool hero=false,bool heavy=false)
        {
            this.rig=rig;this.owner=owner;this.encounter=encounter;
            capsule=rig.Root.gameObject.AddComponent<CapsuleCollider>();capsule.center=Vector3.up;capsule.height=2;capsule.radius=heavy?.43f:.37f;
            Motor=rig.Root.gameObject.AddComponent<Rigidbody>();Motor.mass=hero?180:heavy?100:65;
            Motor.constraints=RigidbodyConstraints.FreezeRotation;Motor.isKinematic=hero;Motor.useGravity=!hero;
            Motor.collisionDetectionMode=hero?CollisionDetectionMode.ContinuousSpeculative:CollisionDetectionMode.ContinuousDynamic;
            Motor.maxDepenetrationVelocity=3;Motor.solverIterations=12;Motor.solverVelocityIterations=8;
            Motor.interpolation=RigidbodyInterpolation.None;
            Relay(rig.Root);
        }
        void Relay(Transform t){var relay=t.gameObject.AddComponent<ContactRelay>();relay.owner=owner;relay.encounter=encounter;}
        public void Impulse(Vector3 velocity)
        {
            if(Ragdoll){foreach(var body in bodies)body.AddForce(velocity,ForceMode.VelocityChange);}
            else Motor.AddForce(velocity,ForceMode.VelocityChange);
        }
        public void IgnoreWalker(FighterPhysics walker,bool ignore)
        {
            foreach(var collider in colliders)Physics.IgnoreCollision(collider,walker.capsule,ignore);
        }
        public bool IgnoresWalker(FighterPhysics walker)
        {
            foreach(var collider in colliders)if(!Physics.GetIgnoreCollision(collider,walker.capsule))return false;return true;
        }
        public bool ShoveAside(Vector3 walker,Vector3 velocity,float dt)
        {
            velocity.y=0;if(!Ragdoll||velocity.sqrMagnitude<.04f)return false;
            bool moved=false;Vector3 forward=velocity.normalized,right=new Vector3(forward.z,0,-forward.x);
            foreach(var body in bodies)
            {
                Vector3 delta=body.worldCenterOfMass-walker;
                if(delta.y<-.4f||delta.y>2.2f)continue;delta.y=0;
                if(delta.sqrMagnitude>1.35f*1.35f)continue;
                float lateral=Vector3.Dot(delta,right),ahead=Vector3.Dot(delta,forward);
                if(Mathf.Abs(lateral)>.85f||ahead<-.7f||ahead>1.15f)continue;
                float side=Mathf.Abs(lateral)>.04f?Mathf.Sign(lateral):(owner%2==0?1:-1);
                Vector3 desired=right*side*3.2f+forward*Mathf.Min(velocity.magnitude,3);
                Vector3 current=body.linearVelocity;current.y=0;
                body.AddForce(Vector3.ClampMagnitude(desired-current,4)*Mathf.Min(1,dt*10),ForceMode.VelocityChange);moved=true;
            }
            return moved;
        }
        public void Die(Vector3 addedVelocity)
        {
            if(Ragdoll)return;
            Vector3 inherited=Motor.linearVelocity+addedVelocity;Motor.linearVelocity=Vector3.zero;Motor.isKinematic=true;capsule.enabled=false;
            Transform[] bones=rig.PhysicsBones;
            int[] parents={-1,0,1,1,3,1,5,0,7,0,9};
            float[] masses={9,12,3,3,2,3,2,5,3,5,3};
            for(int i=0;i<bones.Length;i++)
            {
                Transform bone=bones[i];Collider collider;
                if(i<2)
                {
                    var box=bone.gameObject.AddComponent<BoxCollider>();box.center=i==0?Vector3.zero:new Vector3(0,.34f,0);box.size=i==0?new Vector3(.44f,.28f,.30f):new Vector3(.56f,.60f,.34f);collider=box;
                }
                else if(i==2){var sphere=bone.gameObject.AddComponent<SphereCollider>();sphere.radius=.19f;collider=sphere;}
                else
                {
                    var limb=bone.gameObject.AddComponent<CapsuleCollider>();limb.direction=1;limb.center=new Vector3(0,-.20f,0);limb.height=i>=7?.44f:.39f;limb.radius=i>=7?.12f:.105f;collider=limb;
                }
                colliders.Add(collider);
                var body=bone.gameObject.AddComponent<Rigidbody>();body.mass=masses[i];body.linearDamping=.25f;body.angularDamping=.75f;
                body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.maxDepenetrationVelocity=2.5f;body.maxAngularVelocity=12;body.solverIterations=14;body.solverVelocityIterations=10;
                if(parents[i]>=0)
                {
                    var joint=bone.gameObject.AddComponent<CharacterJoint>();joint.connectedBody=bodies[parents[i]];joint.anchor=Vector3.zero;
                    joint.axis=Vector3.right;joint.swingAxis=Vector3.up;joint.enableProjection=true;joint.projectionDistance=.08f;joint.projectionAngle=8;joint.enablePreprocessing=false;
                    joint.lowTwistLimit=new SoftJointLimit{limit=-35};joint.highTwistLimit=new SoftJointLimit{limit=35};
                    joint.swing1Limit=new SoftJointLimit{limit=i>=3?55:30};joint.swing2Limit=new SoftJointLimit{limit=30};
                }
                body.linearVelocity=inherited;body.angularVelocity=new Vector3(.8f,0,owner%2==0?.6f:-.6f);
                bodies.Add(body);Relay(bone);
            }
            for(int i=0;i<colliders.Count;i++)for(int j=i+1;j<colliders.Count;j++)Physics.IgnoreCollision(colliders[i],colliders[j]);
            Ragdoll=true;rig.Telegraph(false,false);
        }
        public void Sever()
        {
            if(!Ragdoll||Severed)return;Severed=true;
            var waist=rig.Body.GetComponent<CharacterJoint>();if(waist!=null)Object.Destroy(waist);
            rig.Body.SetParent(rig.Root,true);
            var cap=VisualFactory.Part("Upper waist cut",rig.Body,PrimitiveType.Cylinder,Vector3.zero,new Vector3(.5f,.025f,.33f),VisualFactory.RedGlow);
            VisualFactory.Part("Lower waist cut",rig.PhysicsBones[0],PrimitiveType.Cylinder,new Vector3(0,.17f,0),new Vector3(.47f,.025f,.32f),VisualFactory.RedGlow);
            bodies[1].AddForce(Vector3.up*2+Vector3.right*(owner%2==0?2:-2),ForceMode.VelocityChange);
            bodies[0].AddForce(Vector3.left*(owner%2==0?1:-1),ForceMode.VelocityChange);
        }
        public bool Finite()
        {
            if(!FiniteVector(Center))return false;
            foreach(var body in bodies)if(!FiniteVector(body.position)||body.position.y< -4||body.linearVelocity.sqrMagnitude>10000)return false;
            return true;
        }
        static bool FiniteVector(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
    }
}
