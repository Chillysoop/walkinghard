using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        IEnumerator CheckForkEntranceClearance(Action<bool,string> require)
        {
            foreach(int side in new[]{0,1})foreach(float offset in new[]{-5f,0,3.06f,5.9f})
            {
                forcedSeed=true;tuning.seed=4319;StartChapter();ProbeReset();State=RunState.Running;yield return null;
                var first=roads.Split(new Vector3(0,.04f,28.44f),16);roads.Follow(first,side);var parent=roads.Path.Last();
                var doors=EntryLayout.Doors.Where(d=>d.road==parent&&d.localZ==54).ToArray();Vector3 walker=parent.Point(54+offset-16)+Vector3.up*.04f;
                float safe=roads.SafeForkAhead(walker,16,Array.Empty<Vector3>());var next=roads.Split(walker,safe);roads.Follow(next,1-side);
                require(Mathf.Abs(parent.End-60)<.001f,"fork stays six metres beyond nearby entry rooms on either road");
                foreach(var door in doors)
                {
                    require(EntryLayout.PassageClear(door),"both full-height champion entry passages remain clear at fork offset "+offset);
                    hero.Root.position=parent.Point(50)+Vector3.up*.04f;var e=ProbeEnemy(door.origin.x,door.origin.z);e.rig.Root.localScale=Vector3.one*1.3f;e.entering=true;e.doorway=door.exit;Physics.SyncTransforms();
                    for(int frame=0;frame<240&&e.entering;frame++){StepEnemies(1f/60);Physics.SyncTransforms();Physics.Simulate(1f/60);}
                    require(!e.entering,"champion-sized actor physically walks out of the retained entrance");enemies.Remove(e);byId.Remove(e.id);DisposeEnemy(e);
                }
            }
            StartChapter();ProbeReset();State=RunState.Running;yield return null;var fork=roads.Split(new Vector3(0,.04f,28.44f),16);roads.Follow(fork,0);var road=roads.Path.Last();
            var occupied=EntryLayout.Doors.First(d=>d.road==road&&d.localZ==72);var heroPoint=road.Point(41.06f);
            float ahead=roads.SafeForkAhead(heroPoint,16,new[]{occupied.exit});roads.Split(heroPoint,ahead);
            require(road.End>=78&&EntryLayout.PassageClear(occupied),"pending arrivals keep their complete room and exit when a new fork is prepared");
            StartChapter();ProbeReset();State=RunState.Running;yield return null;fork=roads.Split(new Vector3(0,.04f,28.44f),16);roads.Follow(fork,0);road=roads.Path.Last();
            var oldDoors=EntryLayout.Doors.Where(d=>d.road==road&&d.localZ==54).ToArray();roads.Split(road.Point(41.06f),16);
            require(oldDoors.All(d=>!EntryLayout.PassageClear(d)),"regression reproduces the old junction wall blocking both entrances");
            foreach(int side in new[]{-1,1}){var offered=roads.Entrance(side,road.Point(60),1,cameraRig);require(offered==null||EntryLayout.PassageClear(offered),"old saved geometry cannot offer a blocked spawn passage");}
        }
    }
}
