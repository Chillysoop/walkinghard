using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    // Road pieces own their floors, walls and covered entrances in world space.
    // The tree stays in place behind the player, including bodies on abandoned roads.
    public sealed class RoadPiece
    {
        public Transform Root;
        public RoadPiece Parent;
        public float Start,End=300,Min;
        public readonly List<Renderer> Guides=new List<Renderer>();
        public Vector3 Local(Vector3 p)=>Root.InverseTransformPoint(p);
        public Vector3 Point(float z,float x=0)=>Root.TransformPoint(new Vector3(x,0,z));
        public Quaternion Rotation=>Root.rotation;
        public Vector3 Nearest(Vector3 p){var v=Local(p);return Point(Mathf.Clamp(v.z,Min,End));}
    }
    public sealed class RoadFork
    {
        public Transform Root;
        public RoadPiece Parent,Left,Right;
        public float Distance;
        public Vector3 Origin=>Root.position;
        public Quaternion Rotation=>Root.rotation;
        public void Select(int side)
        {
            foreach(var r in Left.Guides)r.sharedMaterial=side==0?VisualFactory.Glow:VisualFactory.Trim;
            foreach(var r in Right.Guides)r.sharedMaterial=side==1?VisualFactory.Glow:VisualFactory.Trim;
        }
    }
    public sealed class PhysicalRoads
    {
        public const float Turn=32;
        readonly Transform world;
        readonly RoadPiece initial;
        readonly List<(Transform t,Vector3 position,Vector3 scale,bool active)> originals=new List<(Transform,Vector3,Vector3,bool)>();
        readonly List<EntryLayout.Door> originalDoors;
        public readonly List<RoadPiece> Pieces=new List<RoadPiece>();
        public readonly List<RoadPiece> Path=new List<RoadPiece>();
        public readonly List<RoadFork> Forks=new List<RoadFork>();
        Transform foundation;
        public PhysicalRoads(Transform world)
        {
            this.world=world;initial=new RoadPiece{Root=EntryLayout.InitialRoot,Min=-350,End=565};
            foreach(Transform t in initial.Root)originals.Add((t,t.localPosition,t.localScale,t.gameObject.activeSelf));
            originalDoors=new List<EntryLayout.Door>(EntryLayout.Doors);
            foreach(var d in originalDoors){d.road=initial;d.localZ=d.z;}
            Pieces.Add(initial);Path.Add(initial);
        }
        public void Reset()
        {
            foreach(var f in Forks){f.Root.gameObject.SetActive(false);Object.Destroy(f.Root.gameObject);}
            Forks.Clear();Pieces.Clear();Path.Clear();initial.End=565;Pieces.Add(initial);Path.Add(initial);
            foreach(var v in originals){v.t.localPosition=v.position;v.t.localScale=v.scale;v.t.gameObject.SetActive(v.active);}
            EntryLayout.Doors.Clear();EntryLayout.Doors.AddRange(originalDoors);
            if(foundation!=null){foundation.gameObject.SetActive(false);Object.Destroy(foundation.gameObject);foundation=null;}
        }
        static float FlatSquared(Vector3 p){p.y=0;return p.sqrMagnitude;}
        public RoadPiece Nearest(Vector3 p,bool activeOnly=true)
        {
            RoadPiece best=initial;float distance=float.MaxValue;
            foreach(var road in activeOnly?Path:Pieces){float d=FlatSquared(p-road.Nearest(p));if(d<distance){distance=d;best=road;}}
            return best;
        }
        public float Progress(Vector3 p){var r=Nearest(p);return r.Start+Mathf.Clamp(r.Local(p).z,r.Min,r.End);}
        public Vector3 Point(float distance)
        {
            var road=Path[0];foreach(var r in Path)if(distance>=r.Start)road=r;
            return road.Point(Mathf.Clamp(distance-road.Start,road.Min,road.End));
        }
        public Quaternion Rotation(Vector3 p)=>Nearest(p).Rotation;
        public Vector3 Walk(Vector3 p,float distance)
        {var next=Point(Progress(p)+distance);next.y=p.y;return next;}
        public void Follow(RoadFork fork,int side)
        {
            var selected=side==0?fork.Left:fork.Right;
            if(!Path.Contains(selected))Path.Add(selected);fork.Select(side);
        }
        public RoadFork Split(Vector3 hero,float ahead)
        {
            var parent=Path[Path.Count-1];float cut=parent.Local(hero).z+ahead;
            if(foundation==null)foundation=EntryLayout.Solid("Service ground beneath separate roads",world,new Vector3(0,-.37f,150),new Vector3(1500,.5f,1500),VisualFactory.Black);
            // End the old corridor. No straight-ahead road survives the junction.
            foreach(Transform t in parent.Root)
            {
                if(t.GetComponent<MeshRenderer>()==null)continue;
                float end=cut+(t.name=="Concourse / physical floor"||t.name=="Branch / physical floor"?2:-3);
                float low=t.localPosition.z-t.localScale.z*.5f,high=t.localPosition.z+t.localScale.z*.5f;
                if(low>=end){t.gameObject.SetActive(false);continue;}
                if(high>end){var size=t.localScale;size.z=end-low;t.localScale=size;var p=t.localPosition;p.z=(low+end)*.5f;t.localPosition=p;}
            }
            parent.End=cut;
            var root=VisualFactory.Node("Physical Y junction",world,parent.Point(cut));root.rotation=parent.Rotation;
            var fork=new RoadFork{Root=root,Parent=parent,Distance=parent.Start+cut};
            fork.Left=Branch(root,parent,fork.Distance,-1);fork.Right=Branch(root,parent,fork.Distance,1);
            Forks.Add(fork);Pieces.Add(fork.Left);Pieces.Add(fork.Right);
            foreach(int side in new[]{-1,1})
            {
                float angle=Turn*Mathf.Deg2Rad;float outerStart=9.5f*Mathf.Tan(angle);
                Vector3 outer=Quaternion.Euler(0,side*Turn,0)*new Vector3(side*9.5f,0,outerStart);
                WallBetween(root,new Vector3(side*9.5f,0,-3),outer);
            }
            fork.Select(0);Physics.SyncTransforms();return fork;
        }
        static void WallBetween(Transform root,Vector3 a,Vector3 b)
        {
            var wall=EntryLayout.Solid("Junction outer wall",root,(a+b)*.5f+Vector3.up*2.3f,new Vector3(.7f,4.7f,Vector3.Distance(a,b)+.1f),VisualFactory.Stone);
            wall.localRotation=Quaternion.LookRotation(b-a);
        }
        RoadPiece Branch(Transform junction,RoadPiece parent,float distance,int side)
        {
            var root=VisualFactory.Node(side<0?"Left road / independent corridor":"Right road / independent corridor",junction,Vector3.zero);root.localRotation=Quaternion.Euler(0,side*Turn,0);
            var road=new RoadPiece{Root=root,Parent=parent,Start=distance};
            EntryLayout.Solid("Branch / physical floor",root,new Vector3(0,-.27f,150),new Vector3(19.7f,.5f,300),VisualFactory.Ground);
            for(int z=3;z<300;z+=6)
            {
                if(z>21)VisualFactory.Part("Road seam",root,PrimitiveType.Cube,new Vector3(0,-.012f,z),new Vector3(18,.02f,.035f),VisualFactory.Black);
                foreach(int edge in new[]{-1,1})
                {
                    if(z<(edge==side?9:21))continue;
                    var guide=VisualFactory.Part("Branch edge light",root,PrimitiveType.Cube,new Vector3(edge*8.8f,.015f,z),new Vector3(.08f,.025f,5.8f),VisualFactory.Trim);
                    road.Guides.Add(guide.GetComponent<Renderer>());
                }
            }
            foreach(int edge in new[]{-1,1})
            {
                float previous=edge==side?9.5f*Mathf.Tan(Turn*Mathf.Deg2Rad):9.5f/Mathf.Tan(Turn*Mathf.Deg2Rad);
                for(int z=54;z<285;z+=18)
                {
                    EntryLayout.Solid("Branch wall",root,new Vector3(edge*9.5f,2.3f,(previous+z-2)*.5f),new Vector3(.7f,4.7f,z-2-previous),VisualFactory.Stone);
                    EntryLayout.Solid("Door lintel",root,new Vector3(edge*9.5f,4.1f,z),new Vector3(.9f,1,4),VisualFactory.Trim);
                    EntryLayout.Solid("Entry passage floor",root,new Vector3(edge*13.7f,-.27f,z),new Vector3(8.8f,.5f,4.6f),VisualFactory.Ground);
                    EntryLayout.Solid("Entry passage roof",root,new Vector3(edge*13.7f,4.5f,z),new Vector3(8.8f,.35f,4.6f),VisualFactory.Stone);
                    EntryLayout.Solid("Entry passage front",root,new Vector3(edge*13.7f,2.2f,z-2.1f),new Vector3(8.8f,4.4f,.35f),VisualFactory.Stone);
                    EntryLayout.Solid("Entry passage rear",root,new Vector3(edge*13.7f,2.2f,z+2.1f),new Vector3(8.8f,4.4f,.35f),VisualFactory.Stone);
                    EntryLayout.Solid("Entry passage back",root,new Vector3(edge*18,2.2f,z),new Vector3(.35f,4.4f,4.4f),VisualFactory.Black);
                    foreach(int light in new[]{-1,1})VisualFactory.Part("Door light",root,PrimitiveType.Cube,new Vector3(edge*9.05f,1.65f,z+light*1.9f),new Vector3(.09f,3.3f,.09f),VisualFactory.GoldGlow);
                    var origin=road.Point(z,edge*15.5f)+Vector3.up*.05f;
                    EntryLayout.Doors.Add(new EntryLayout.Door{side=edge,z=origin.z,localZ=z,road=road,origin=origin,exit=road.Point(z,edge*7.8f)+Vector3.up*.05f});previous=z+2;
                }
                EntryLayout.Solid("Branch far wall",root,new Vector3(edge*9.5f,2.3f,(previous+300)*.5f),new Vector3(.7f,4.7f,300-previous),VisualFactory.Stone);
            }
            return road;
        }
        public EntryLayout.Door Entrance(int side,Vector3 hero,int direction,Camera camera)
        {
            float at=Progress(hero);
            var doors=EntryLayout.Doors.Where(d=>d.side==side&&Path.Contains(d.road)&&d.localZ<d.road.End-3&&EntryLayout.Concealed(camera,d.origin));
            // Before the choice locks, use covered entrances on the approach.
            // This also lets existing enemies walk out of the unselected road.
            return doors.OrderBy(d=>direction*(d.road.Start+d.localZ-at)>3?0:1).ThenBy(d=>Mathf.Abs(d.road.Start+d.localZ-at)).FirstOrDefault();
        }
        public Vector3 Horizon(Camera camera,Vector3 hero,float x,int direction)
        {
            float at=Progress(hero);
            for(int i=0;i<80;i++)
            {
                float distance=at+(32+i*3)*direction;var center=Point(distance);var r=Nearest(center);
                var p=center+r.Rotation*new Vector3(x,.05f,0);if(EntryLayout.Concealed(camera,p))return p;
            }
            // A pending fork ends the active road. Covered approach doors are safe fallbacks.
            var door=Entrance(x<0?-1:1,hero,direction,camera);if(door!=null)return door.origin;
            return Point(at-100)+Vector3.up*.05f;
        }
        public bool Navigation(Vector3 p,Vector3 target,out Vector3 waypoint)
        {
            var from=Nearest(p,false);var to=Nearest(target);waypoint=target;
            if(from==to)
            {
                if(from.Parent==null||Mathf.Abs(from.Local(p).x)<9)return false;
                waypoint=from.Point(0);return FlatSquared(p-waypoint)>9;
            }
            var ancestors=new List<RoadPiece>();for(var r=to;r!=null;r=r.Parent)ancestors.Add(r);
            if(ancestors.Contains(from))
            {var child=to;while(child.Parent!=from&&child.Parent!=null)child=child.Parent;waypoint=child.Point(0);}
            else waypoint=from.Point(0);
            return FlatSquared(p-waypoint)>9;
        }
        public Vector3 ClampFast(Vector3 p,float radius)
        {
            var road=Nearest(p,false);var local=road.Local(p);
            if(road.Parent==null||local.z>20)local.x=Mathf.Clamp(local.x,-8.9f+radius,8.9f-radius);
            return road.Root.TransformPoint(local);
        }
    }
}
