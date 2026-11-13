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
        public bool ForkApproach=>forkWave==Wave&&forkStarted>=0&&WaveElapsed<forkStarted+10;
        public bool ShowForkSigns=>State==RunState.Running&&(RoutePending||forkLockedAge>0);
        public bool IsForkWave=>(Wave==2||Wave==5)&&Wave<tuning.waveCount&&Controlled==null&&!CombatPreviewActive&&!TutorialActive;
        int forkWave;
        float forkAge,forkStarted,forkLockedAge;
        Vector3 forkOrigin;
        RoadFork forkVisual;
        PhysicalRoads roads;
        Quaternion roadCameraRotation=Quaternion.identity;
        Quaternion RoadRotation=>roads==null?Quaternion.identity:roads.Rotation(HeroPosition);
        Vector3 RoadForward=>RoadRotation*Vector3.forward;
        Vector3 RoadVector(Vector3 local)=>RoadRotation*local;
        bool ForkAwaitingChoice=>forkVisual!=null&&forkWave==Wave&&routedWave!=Wave+1;
        float ForkCameraWeight
        {
            get
            {
                if(forkVisual==null||finaleActive)return 0;
                float distance=roads.Progress(HeroPosition)-forkVisual.Distance;
                return Mathf.SmoothStep(0,1,Mathf.Clamp01((distance+16)/10))*Mathf.SmoothStep(0,1,Mathf.Clamp01((24-distance)/20));
            }
        }
        void ResetWalkingFork()
        {
            RoutePending=false;forkWave=0;forkAge=forkLockedAge=0;forkStarted=-1;ForkChoice=EncounterRoute.ShieldLine;
            roads?.Reset();forkVisual=null;roadCameraRotation=Quaternion.identity;
        }
        void PrepareWalkingFork()
        {
            if(!IsForkWave||forkWave==Wave)return;
            forkWave=Wave;forkStarted=-1;forkAge=forkLockedAge=0;
            forkVisual=roads.Split(HeroPosition,16);forkOrigin=forkVisual.Origin;
        }
        void BeginWalkingFork()
        {
            PrepareWalkingFork();forkAge=0;forkStarted=WaveElapsed;forkLockedAge=0;
            ForkChoice=ForkLeft;RoutePending=true;
            if(qa)ChooseRoute(simulation&&simulationEncounterRoute>0?(EncounterRoute)simulationEncounterRoute:(Seed+Wave)%2==0?EncounterRoute.ShieldLine:EncounterRoute.VolatileBreach);
        }
        void CommitWalkingFork()
        {
            if(!RoutePending)return;
            SelectedRoute=ForkChoice;routedWave=Wave+1;RoutePending=false;forkLockedAge=2;
            roads.Follow(forkVisual,ForkChoice==ForkLeft?0:1);
        }
        void StepWalkingFork(float dt)
        {
            if(State!=RunState.Running)return;
            PrepareWalkingFork();
            if(ForkAwaitingChoice&&!RoutePending&&forkStarted<0&&forkVisual.Distance-roads.Progress(HeroPosition)<=tuning.walkSpeed*ForkDecisionSeconds+1)BeginWalkingFork();
            if(RoutePending){forkAge+=dt;if(forkAge+0.00001f>=ForkDecisionSeconds)CommitWalkingFork();}
            else forkLockedAge=Mathf.Max(0,forkLockedAge-dt);
        }
        Vector3 ForkWalkingPosition(float distance)
        {
            return roads.Forks.Count==0?HeroPosition+Vector3.forward*distance:roads.Walk(HeroPosition,distance);
        }
        public Vector3 ForkSignViewport(int side)=>cameraRig.WorldToViewportPoint(forkOrigin+forkVisual.Rotation*new Vector3(side==0?-7.5f:7.5f,.15f,12));
    }

    public sealed partial class PrototypeHUD
    {
        internal Rect ForkChoiceRect(int side,float w,float h)
        {
            var p=game.ForkSignViewport(side);float x=p.x*w;
            // Keep signs attached to the two roads, inside the gap between combat HUDs.
            x=side==0?Mathf.Clamp(x,455,w/2-118):Mathf.Clamp(x,w/2+118,w-455);
            return new Rect(x-105,Mathf.Clamp((1-p.y)*h-115,160,h-360),210,98);
        }
        internal bool HandleForkPointer(EventType type,Vector2 pointer,float w,float h)
        {
            if(type!=EventType.MouseDown||!game.RoutePending||game.State!=VectorEncounter.RunState.Running)return false;
            for(int side=0;side<2;side++)if(ForkChoiceRect(side,w,h).Contains(pointer))
                return game.ChooseRoute(side==0?game.ForkLeft:game.ForkRight);
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
                var route=side==0?game.ForkLeft:game.ForkRight;
                var r=ForkChoiceRect(side,w,h);bool selected=game.ForkChoice==route;Color color=selected?Accent:Strong;
                Plate(r,ink,color,9,selected?2:1);
                var symbol=r.position+new Vector2(27,30);
                if(route==EncounterRoute.Overtime)
                {Plate(new Rect(symbol.x-12,symbol.y-12,24,24),ink,color,2);foreach(var d in new[]{new Vector2(-5,-5),new Vector2(5,-5),new Vector2(-5,5),new Vector2(5,5)})Ring(symbol+d,1.5f,color);}
                else if(route==EncounterRoute.SalvageStop)
                {Stroke(symbol+new Vector2(-11,0),symbol+new Vector2(11,0),color,5);Stroke(symbol+new Vector2(0,-11),symbol+new Vector2(0,11),color,5);}
                else if(route==EncounterRoute.ShieldLine)
                {
                    Vector2[] points={new Vector2(-11,-12),new Vector2(11,-12),new Vector2(9,4),new Vector2(0,13),new Vector2(-9,4),new Vector2(-11,-12)};
                    for(int i=1;i<points.Length;i++)Stroke(symbol+points[i-1],symbol+points[i],color,2);
                }
                else {Ring(symbol,9,color);for(int i=0;i<6;i++){float a=i*Mathf.PI/3;var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Stroke(symbol+v*12,symbol+v*16,color,2);}}
                Label(new Rect(r.x+52,r.y+13,150,26),VectorEncounter.ForkLabel(route),18,color,true);
                Label(new Rect(r.x+52,r.y+42,150,20),VectorEncounter.ForkBenefit(route),12,white,true);
                Label(new Rect(r.x+12,r.y+73,r.width-24,18),(side==0?"[ A / ← ]":"[ D / → ]")+(selected?"   FOLLOWING":""),11,color,true,TextAnchor.MiddleCenter);
                var anchor=game.ForkSignViewport(side);var point=new Vector2(anchor.x*w,(1-anchor.y)*h);
                Stroke(new Vector2(r.center.x,r.yMax),point,color,selected?2:1);Ring(point,4,color);
            }
        }
    }
}
