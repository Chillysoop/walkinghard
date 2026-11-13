using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    [Serializable] public sealed class SavedBone
    {public Vector3 position,velocity,angular,force,torque,anchor,connectedAnchor;public Quaternion rotation;public bool cut,sleeping;}
    [Serializable] public sealed class SavedCutCap
    {public int bone;public string name;public Vector3 position,scale;public Quaternion rotation;}
    [Serializable] public sealed class SavedBody
    {public int id;public string name;public Vector3 position,scale,velocity,angular,force,torque;public Quaternion rotation;public bool ragdoll,severed;public int lastCut;public SavedBone[] bones;public List<SavedCutCap> caps=new List<SavedCutCap>();}
    public sealed partial class FighterPhysics
    {
        public SavedBody CaptureBody()
        {
            var saved=new SavedBody{id=owner,name=rig.Root.name,position=rig.Root.position,rotation=rig.Root.rotation,scale=rig.Root.localScale,velocity=LinearVelocity,angular=Motor.angularVelocity,ragdoll=Ragdoll,severed=Severed,lastCut=LastCutBone,bones=new SavedBone[rig.PhysicsBones.Length]};
            if(!Motor.isKinematic){saved.force=Motor.GetAccumulatedForce(1f/60);saved.torque=Motor.GetAccumulatedTorque(1f/60);}
            for(int i=0;i<saved.bones.Length;i++)
            {
                var bone=rig.PhysicsBones[i];var body=Ragdoll?bodies[i]:null;var joint=bone.GetComponent<CharacterJoint>();
                saved.bones[i]=new SavedBone{position=bone.position,rotation=bone.rotation,cut=cutJoints[i],velocity=body==null?Vector3.zero:body.linearVelocity,angular=body==null?Vector3.zero:body.angularVelocity,sleeping=body!=null&&body.IsSleeping(),anchor=joint==null?Vector3.zero:joint.anchor,connectedAnchor=joint==null?Vector3.zero:joint.connectedAnchor};
                if(body!=null){saved.bones[i].force=body.GetAccumulatedForce(1f/60);saved.bones[i].torque=body.GetAccumulatedTorque(1f/60);}
                foreach(Transform child in bone)if(child.name=="Severed surface"||child.name=="Exposed surface")saved.caps.Add(new SavedCutCap{bone=i,name=child.name,position=child.localPosition,rotation=child.localRotation,scale=child.localScale});
            }
            return saved;
        }
        public void RestoreBody(SavedBody saved)
        {
            rig.Root.position=saved.position;rig.Root.rotation=saved.rotation;rig.Root.localScale=saved.scale;
            Motor.position=saved.position;Motor.rotation=saved.rotation;
            if(saved.ragdoll)Die(Vector3.zero);
            for(int i=0;i<saved.bones.Length;i++)
            {
                var value=saved.bones[i];var bone=rig.PhysicsBones[i];var joint=bone.GetComponent<CharacterJoint>();
                if(value.cut&&joint!=null){UnityEngine.Object.DestroyImmediate(joint);joint=null;bone.SetParent(rig.Root,true);cutJoints[i]=true;DetachedParts++;}
                bone.position=value.position;bone.rotation=value.rotation;
                if(joint!=null){joint.autoConfigureConnectedAnchor=false;joint.anchor=value.anchor;joint.connectedAnchor=value.connectedAnchor;}
                if(Ragdoll){var body=bodies[i];body.position=value.position;body.rotation=value.rotation;body.linearVelocity=value.velocity;body.angularVelocity=value.angular;if(value.force!=Vector3.zero)body.AddForce(value.force,ForceMode.Force);if(value.torque!=Vector3.zero)body.AddTorque(value.torque,ForceMode.Force);if(value.sleeping)body.Sleep();}
            }
            foreach(var cap in saved.caps){var t=VisualFactory.Part(cap.name,rig.PhysicsBones[cap.bone],PrimitiveType.Cylinder,cap.position,cap.scale,VisualFactory.RedGlow);t.localRotation=cap.rotation;}
            Severed=saved.severed;LastCutBone=saved.lastCut;
            if(!Motor.isKinematic){Motor.linearVelocity=saved.velocity;Motor.angularVelocity=saved.angular;if(saved.force!=Vector3.zero)Motor.AddForce(saved.force,ForceMode.Force);if(saved.torque!=Vector3.zero)Motor.AddTorque(saved.torque,ForceMode.Force);}
        }
    }
}
