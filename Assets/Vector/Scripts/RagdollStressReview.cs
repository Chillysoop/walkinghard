using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] sealed class RagdollStressSample
        {
            public int seed,bodies,bones,cuts,launches,worldContacts,collisionHits,steps;
            public string fixture,error="";public bool passed;public float maxSpeed,minY=999;
        }
        IEnumerator ReviewRagdollStress(string output)
        {
            forcedSeed=true;string path=Path.Combine(output,"stress.jsonl");if(File.Exists(path))throw new IOException("Use a fresh stress output directory");
            foreach(int seed in new[]{1741,4319,6229,9137,11749,15401,19391,24107})
            foreach(string fixture in new[]{"open-launch","wide-sever","left-wall","right-wall","fork-nose","left-branch","right-branch"})
            {
                tuning.seed=seed;StartChapter();ProbeReset();OpeningPicks=2;Wave=5;State=RunState.Running;yield return null;
                RoadPiece road=roads.Path[0];Vector3 origin=new Vector3(0,.04f,8);Quaternion rotation=Quaternion.identity;
                if(fixture.StartsWith("fork")||fixture.EndsWith("branch"))
                {
                    var fork=roads.Split(HeroPosition,35);roads.Follow(fork,fixture=="left-branch"?0:1);
                    road=fixture=="left-branch"?fork.Left:fork.Right;rotation=fixture=="fork-nose"?fork.Rotation:road.Rotation;
                    origin=(fixture=="fork-nose"?fork.Origin-Vector3.forward*3:road.Point(14))+Vector3.up*.04f;
                    hero.Root.position=origin-rotation*Vector3.forward*6;
                }
                ProbeHost(LoadoutCatalog.Find("greatsword"),rank:5);
                var random=new System.Random(seed);var sample=new RagdollStressSample{seed=seed,fixture=fixture};int worlds=WorldContacts,collisions=CollisionHits,launches=Launches;
                for(int i=0;i<12;i++)
                {
                    float x=(i%4-1.5f)*1.5f+(fixture=="left-wall"?-6:fixture=="right-wall"?6:0),z=(i/4)*1.8f;
                    Vector3 point=origin+rotation*new Vector3(x,0,z);var e=ProbeEnemy(point.x,point.z,1000);e.rig.Root.rotation=rotation;Spawned++;
                    if(i<8)
                    {
                        KillEnemy(e,Vector3.zero,false);
                        if(fixture!="open-launch")e.physics.Sever(new ImpactReaction(ImpactKind.Slash,e.physics.Center,rotation*Vector3.right,1.8f,true));
                    }
                }
                Physics.SyncTransforms();
                for(int frame=0;frame<720;frame++)
                {
                    try
                    {
                        if(frame%60==0&&frame<360)
                        {
                            actionSerial++;currentCredit=new KillCredit{item=actionItem,weapon=actionWeapon,action=actionSerial};var chain=new Cascade{credit=currentCredit};
                            foreach(var e in corpses.Take(6))
                            {
                                Vector3 direction=fixture=="left-wall"?Vector3.left:fixture=="right-wall"?Vector3.right:new Vector3((float)random.NextDouble()-.5f,0,1).normalized;
                                Launch(e,chain,energy:1.6f,forcedDirection:rotation*direction);
                            }
                        }
                        Elapsed+=1f/60;Physics.SyncTransforms();Physics.Simulate(1f/60);ProcessContacts();StepBodies(1f/60);fx.Step(1f/60);sample.steps++;
                        foreach(var e in corpses){e.physics.SampleBoneMotion(out float speed,out float y);sample.maxSpeed=Mathf.Max(sample.maxSpeed,speed);sample.minY=Mathf.Min(sample.minY,y);}
                    }
                    catch(Exception e){sample.error=e.Message;break;}
                    if(frame%120==119)yield return null;
                }
                sample.bodies=corpses.Count;sample.bones=corpses.Sum(e=>e.rig.PhysicsBones.Length);sample.cuts=corpses.Sum(e=>e.physics.DetachedParts);
                sample.launches=Launches-launches;sample.worldContacts=WorldContacts-worlds;sample.collisionHits=CollisionHits-collisions;
                sample.passed=sample.error.Length==0&&sample.steps==720&&enemies.Count+corpses.Count==12&&corpses.All(e=>e.physics.Finite());
                File.AppendAllText(path,JsonUtility.ToJson(sample)+Environment.NewLine);
                if(!sample.passed)throw new InvalidOperationException("RAGDOLL_STRESS "+fixture+" seed="+seed+" "+sample.error);
            }
            Debug.Log("RAGDOLL_STRESS 56 full-physics fixtures passed");
        }
    }
    public sealed partial class FighterPhysics
    {
        internal void SampleBoneMotion(out float maxSpeed,out float minY)
        {maxSpeed=0;minY=999;foreach(var b in bodies){maxSpeed=Mathf.Max(maxSpeed,b.linearVelocity.magnitude);minY=Mathf.Min(minY,b.position.y);}}
    }
}
