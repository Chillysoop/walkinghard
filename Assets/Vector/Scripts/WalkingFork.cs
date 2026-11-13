using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public const float ForkDecisionSeconds=6;
        public EncounterRoute ForkChoice {get;private set;}=EncounterRoute.ShieldLine;
        public float ForkSeconds=>RoutePending?Mathf.Max(0,ForkDecisionSeconds-forkAge):0;
        public bool ForkApproach=>forkWave==Wave&&forkWave>0&&WaveElapsed<forkStarted+10;
        public bool ShowForkSigns=>State==RunState.Running&&(RoutePending||forkLockedAge>0);
        public bool IsForkWave=>(Wave==2||Wave==5)&&Wave<tuning.waveCount&&Controlled==null&&!CombatPreviewActive&&!TutorialActive;
        int forkWave;
        float forkAge,forkStarted,forkLockedAge;
        Vector3 forkOrigin;
        bool followingFork;
        WalkingForkVisual forkVisual;
        void ResetWalkingFork()
        {
            RoutePending=false;forkWave=0;forkAge=forkStarted=forkLockedAge=0;followingFork=false;ForkChoice=EncounterRoute.ShieldLine;
            forkVisual?.Dispose();forkVisual=null;
        }
        void BeginWalkingFork()
        {
            forkVisual?.Dispose();forkVisual=null;
            forkWave=Wave;forkAge=0;forkStarted=WaveElapsed;forkLockedAge=0;forkOrigin=HeroPosition;followingFork=false;
            ForkChoice=EncounterRoute.ShieldLine;RoutePending=true;
            if(!FastSimulation&&!(Application.isBatchMode&&Array.IndexOf(Environment.GetCommandLineArgs(),"-nographics")>=0))
                forkVisual=new WalkingForkVisual(transform,forkOrigin);
            if(qa)ChooseRoute(simulation&&simulationEncounterRoute>0?(EncounterRoute)simulationEncounterRoute:(Seed+Wave)%2==0?EncounterRoute.ShieldLine:EncounterRoute.VolatileBreach);
        }
        void CommitWalkingFork()
        {
            if(!RoutePending)return;
            SelectedRoute=ForkChoice;routedWave=Wave+1;RoutePending=false;forkLockedAge=2;followingFork=true;
            forkVisual?.Select(ForkChoice==EncounterRoute.ShieldLine?0:1);
        }
        void StepWalkingFork(float dt)
        {
            if(State!=RunState.Running)return;
            if(IsForkWave&&forkWave!=Wave&&WaveElapsed>=Mathf.Max(0,tuning.waveArrivals-10))BeginWalkingFork();
            if(RoutePending){forkAge+=dt;if(forkAge+0.00001f>=ForkDecisionSeconds)CommitWalkingFork();}
            else forkLockedAge=Mathf.Max(0,forkLockedAge-dt);
        }
        Vector3 ForkWalkingPosition(Vector3 next,float dt)
        {
            if(!followingFork)return next;
            float distance=next.z-forkOrigin.z;
            float x=forkOrigin.x+(SelectedRoute==EncounterRoute.ShieldLine?-1:1)*WalkingForkVisual.LaneOffset(distance);
            // Route choice owns only a bounded walking offset. Physics, combat order
            // and displacement of bodies continue through the ordinary motor.
            next.x=Mathf.MoveTowards(next.x,x,.95f*dt);
            if(distance>=32&&Mathf.Abs(next.x-forkOrigin.x)<.01f)followingFork=false;
            return next;
        }
        public Vector3 ForkSignViewport(int side)=>cameraRig.WorldToViewportPoint(forkOrigin+new Vector3(side==0?-3.3f:3.3f,.15f,6.8f));
    }

    // Flat, fork-shaped road surfaces preserve the corridor's collision topology.
    // The two bypass lanes reconnect; their selected encounter/reward remains distinct.
    public sealed class WalkingForkVisual
    {
        public readonly Transform Root;
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Renderer>[] edges={new List<Renderer>(),new List<Renderer>()};
        public static float LaneOffset(float distance)
        {
            if(distance<8)return 4*Mathf.SmoothStep(0,1,Mathf.Clamp01((distance-2)/6));
            if(distance<22)return 4;
            return 4*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((distance-22)/10)));
        }
        public WalkingForkVisual(Transform parent,Vector3 origin)
        {
            Root=VisualFactory.Node("Walking fork / route lanes",parent,new Vector3(origin.x,0,origin.z));
            VisualFactory.Part("Junction apron",Root,PrimitiveType.Cube,new Vector3(0,.017f,17),new Vector3(17,.02f,38),VisualFactory.Stone);
            // No raised divider or new collider can catch a corpse or alter a launch.
            Ribbon("Approach",new[]{new Vector3(0,.031f,-2),new Vector3(0,.031f,2.5f)},3.2f,VisualFactory.Ground);
            Ribbon("Exit",new[]{new Vector3(0,.031f,31.5f),new Vector3(0,.031f,36)},3.2f,VisualFactory.Ground);
            for(int side=0;side<2;side++)
            {
                float sign=side==0?-1:1;var centers=new Vector3[51];
                for(int i=0;i<centers.Length;i++){float z=2+i*.6f;centers[i]=new Vector3(sign*LaneOffset(z),.035f,z);}
                Ribbon("Route pavement",centers,2.6f,VisualFactory.Ground);
                foreach(float edge in new[]{-1.3f,1.3f})
                {
                    var points=new List<Vector3>();for(int i=0;i<centers.Length;i++)
                        if(edge*sign>0||Mathf.Abs(centers[i].x)>1.45f)points.Add(centers[i]+new Vector3(edge,.006f,0));
                    edges[side].Add(Ribbon("Route edge",points.ToArray(),.12f,VisualFactory.Trim));
                }
                var arrow=new[]{new Vector3(sign*2.9f,.048f,6),new Vector3(sign*3.1f,.048f,7),new Vector3(sign*3.0f,.048f,8)};
                edges[side].Add(Ribbon("Lane guide",arrow,.12f,VisualFactory.Trim));
                var tip=arrow[2];edges[side].Add(Ribbon("Arrow left",new[]{tip+new Vector3(-.42f,0,-.55f),tip},.12f,VisualFactory.Trim));
                edges[side].Add(Ribbon("Arrow right",new[]{tip+new Vector3(.42f,0,-.55f),tip},.12f,VisualFactory.Trim));
            }
            Select(0);
        }
        Renderer Ribbon(string name,Vector3[] centers,float width,Material material)
        {
            var vertices=new Vector3[centers.Length*2];var triangles=new int[(centers.Length-1)*6];
            for(int i=0;i<centers.Length;i++)
            {
                vertices[i*2]=centers[i]+Vector3.left*width*.5f;vertices[i*2+1]=centers[i]+Vector3.right*width*.5f;
                if(i==centers.Length-1)continue;int n=i*2,j=i*6;
                triangles[j]=n;triangles[j+1]=n+2;triangles[j+2]=n+1;triangles[j+3]=n+1;triangles[j+4]=n+2;triangles[j+5]=n+3;
            }
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var node=VisualFactory.Node(name,Root,Vector3.zero);node.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=node.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return renderer;
        }
        public void Select(int side){for(int i=0;i<2;i++)foreach(var r in edges[i])r.sharedMaterial=i==side?VisualFactory.Glow:VisualFactory.Trim;}
        public void Dispose(){if(Root!=null){Root.gameObject.SetActive(false);UnityEngine.Object.Destroy(Root.gameObject);}foreach(var mesh in meshes)UnityEngine.Object.Destroy(mesh);}
    }

    public sealed partial class PrototypeHUD
    {
        internal Rect ForkChoiceRect(int side,float w,float h)
        {
            var p=game.ForkSignViewport(side);float x=p.x*w;
            // Keep signs attached to the two lanes, inside the gap between combat HUDs.
            x=side==0?Mathf.Clamp(x,455,w/2-118):Mathf.Clamp(x,w/2+118,w-455);
            return new Rect(x-105,Mathf.Clamp((1-p.y)*h-115,160,h-360),210,98);
        }
        internal bool HandleForkPointer(EventType type,Vector2 pointer,float w,float h)
        {
            if(type!=EventType.MouseDown||!game.RoutePending||game.State!=VectorEncounter.RunState.Running)return false;
            for(int side=0;side<2;side++)if(ForkChoiceRect(side,w,h).Contains(pointer))
                return game.ChooseRoute(side==0?EncounterRoute.ShieldLine:EncounterRoute.VolatileBreach);
            return false;
        }
        void WalkingForkControls(float w,float h)
        {
            if(game.ForkApproach&&game.CombatSpeed>1)Label(new Rect(20,205,306,19),"JUNCTION APPROACH · 1x",11,Accent,true,TextAnchor.MiddleCenter);
            if(!game.ShowForkSigns)return;
            if(Event.current.button==0&&HandleForkPointer(Event.current.type,Pointer,w,h))Event.current.Use();
            var banner=new Rect(w/2-215,22,430,66);Panel(banner);
            string title=game.RoutePending?"FORK AHEAD  /  "+game.ForkSeconds.ToString("0.0")+"s":"PATH LOCKED";
            Label(new Rect(banner.x+10,banner.y+8,410,24),title,19,white,true,TextAnchor.MiddleCenter);
            Label(new Rect(banner.x+10,banner.y+36,410,20),game.RoutePending?"A / D or click  ·  highlighted path is automatic":"NEXT: "+VectorEncounter.RouteName(game.ForkChoice),12,muted,false,TextAnchor.MiddleCenter);
            for(int side=0;side<2;side++)
            {
                var route=side==0?EncounterRoute.ShieldLine:EncounterRoute.VolatileBreach;
                var r=ForkChoiceRect(side,w,h);bool selected=game.ForkChoice==route;Color color=selected?Accent:Strong;
                Plate(r,ink,color,9,selected?2:1);
                var symbol=r.position+new Vector2(27,30);
                if(side==0)
                {
                    Vector2[] points={new Vector2(-11,-12),new Vector2(11,-12),new Vector2(9,4),new Vector2(0,13),new Vector2(-9,4),new Vector2(-11,-12)};
                    for(int i=1;i<points.Length;i++)Stroke(symbol+points[i-1],symbol+points[i],color,2);
                }
                else {Ring(symbol,9,color);for(int i=0;i<6;i++){float a=i*Mathf.PI/3;var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Stroke(symbol+v*12,symbol+v*16,color,2);}}
                Label(new Rect(r.x+52,r.y+13,150,26),side==0?"SHIELDS":"VOLATILES",18,color,true);
                Label(new Rect(r.x+52,r.y+42,150,20),side==0?"MORE GEAR":"MORE ABILITIES",12,white,true);
                Label(new Rect(r.x+12,r.y+73,r.width-24,18),(side==0?"[ A / ← ]":"[ D / → ]")+(selected?"   FOLLOWING":""),11,color,true,TextAnchor.MiddleCenter);
                var anchor=game.ForkSignViewport(side);var point=new Vector2(anchor.x*w,(1-anchor.y)*h);
                Stroke(new Vector2(r.center.x,r.yMax),point,color,selected?2:1);Ring(point,4,color);
            }
        }
    }
}
