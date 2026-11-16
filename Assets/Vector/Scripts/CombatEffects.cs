using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorGame
{
    public sealed partial class CombatEffects
    {
        class Effect { public Transform root; public LineRenderer line; public float age,life,radius,arc,width;public Vector3 velocity,origin,aim,normal;public Color color;public bool ring,sweep,reverse,wave,threat;public Vector3 scale; }
        readonly List<Effect> effects=new List<Effect>();
        readonly Transform parent;
        internal static System.Action<AudioClip,float> CaptureSound;
        readonly bool disabled=Application.isBatchMode&&System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-nographics")>=0;
        public CombatEffects(Transform p,bool silent)
        {
            parent=p;muted=silent;
            if(!disabled)sound=new CombatAudio(p);
        }
        public void Contact(ImpactReaction hit,bool lethal)
        {
            if(disabled)return;
            if(previewEnabled){previewParticles.Contact(hit,lethal,PrimaryContact());if(hit.kind!=ImpactKind.Electric&&hit.kind!=ImpactKind.Pull)return;}
            var p=hit.point;float power=hit.strength;
            if(hit.kind==ImpactKind.Slash)
            {
                Vector3 across=Vector3.ProjectOnPlane(hit.direction,hit.normal).normalized;
                Outline(new[]{p-across*.65f,p+across*.65f},Color.white,.13f,false);
                DirectedBurst(p,across,lethal?9:4,4*power,VisualFactory.GoldGlow);
            }
            else if(hit.kind==ImpactKind.Crush)
            {
                Ring(new Vector3(p.x,.14f,p.z),lethal?1.65f:1.05f,VisualFactory.Orange,.24f);
                DirectedBurst(p,hit.direction,lethal?12:6,4.5f*power,VisualFactory.Trim);
            }
            else if(hit.kind==ImpactKind.Electric)
            {Lightning(p-Vector3.up*.5f,p+Vector3.up*.8f,Color.cyan,.2f);DirectedBurst(p,Vector3.up,4,2,VisualFactory.Glow);}
            else if(hit.kind==ImpactKind.Bullet)
            {DirectedBurst(p,hit.direction,lethal?7:3,4*power,VisualFactory.GoldGlow);Outline(new[]{p-hit.direction*.35f,p+hit.direction*.5f},new Color(1,.8f,.45f),.09f,false);}
            else if(hit.kind==ImpactKind.Pull)
            {for(int i=-1;i<=1;i++)Arc(p+Vector3.up*i*.25f,p+hit.direction*.8f+Vector3.up*i*.25f,VisualFactory.Cyan);}
            else
            {DirectedBurst(p,hit.direction,lethal?7:4,3.5f*power,VisualFactory.Glow);Ring(p,.45f,Color.white,.14f);}
        }
        void DirectedBurst(Vector3 point,Vector3 direction,int count,float force,Material material)
        {
            for(int i=0;i<count;i++)
            {
                var e=Rent(false);var t=e.root;t.position=point;t.localScale=new Vector3(.05f,.04f,.17f);t.GetComponent<Renderer>().sharedMaterial=material;
                Vector3 velocity=(direction+Random.insideUnitSphere*.6f)*force+Vector3.up*.8f;t.rotation=Quaternion.LookRotation(velocity);
                e.velocity=velocity*(reducedMotion?.25f:1);e.life=.18f+Random.value*.18f;e.scale=t.localScale;
            }
        }
        public void Lightning(Vector3 from,Vector3 to,Color color,float life=.15f)
        {
            if(disabled)return;
            var points=new Vector3[7];Vector3 side=Vector3.Cross(to-from,Vector3.forward).normalized;
            if(side.sqrMagnitude<.1f)side=Vector3.right;
            for(int i=0;i<7;i++)points[i]=Vector3.Lerp(from,to,i/6f)+(i==0||i==6?Vector3.zero:side*(i%2==0?-.13f:.13f));
            Outline(points,color,life,false);
        }
        public void Shield(Vector3 center,Vector3 forward,bool struck)
        {
            if(disabled)return;
            center+=Vector3.up*1.1f+forward*.65f;Vector3 right=Vector3.Cross(Vector3.up,forward).normalized;
            var points=new Vector3[6];for(int i=0;i<6;i++){float a=i*Mathf.PI/3;points[i]=center+right*Mathf.Cos(a)*.8f+Vector3.up*Mathf.Sin(a)*.85f;}
            Outline(points,struck?Color.white:VisualFactory.Cyan,struck?.16f:.4f);
            if(struck)DirectedBurst(center,-forward,5,2,VisualFactory.Glow);
        }
        public void Overdrive(Vector3 rightHand,Vector3 leftHand)
        {
            if(disabled)return;
            var color=new Color(1,.35f,.12f);Lightning(rightHand,rightHand+Vector3.up*.5f,color,.22f);Lightning(leftHand,leftHand+Vector3.up*.5f,color,.22f);
            DirectedBurst(rightHand,Vector3.up,4,1.8f,VisualFactory.GoldGlow);DirectedBurst(leftHand,Vector3.up,4,1.8f,VisualFactory.GoldGlow);
        }
        void Outline(Vector3[] points,Color color,float life,bool closed=true,bool threat=false)
        {
            if(disabled)return;
            var e=Rent(true,threat);var l=e.line;l.loop=closed;l.positionCount=points.Length;l.SetPositions(points);e.life=life;e.color=color;
        }
        public void Shape(Vector3 center,Vector3 aim,float radius,float halfArc,Color color,float life,bool reverse=false)
        {
            if(disabled)return;
            var points=new List<Vector3>();center.y=.12f;if(halfArc<180)points.Add(center);
            for(int j=0;j<=24;j++){float angle=Mathf.Lerp(-halfArc,halfArc,reverse?1-j/24f:j/24f);points.Add(center+Quaternion.AngleAxis(angle,Vector3.up)*aim.normalized*radius);}
            Outline(points.ToArray(),color,life);
        }
        public void Sweep(Vector3 origin,Vector3 aim,float radius,float halfArc,Color color,float life,bool reverse=false,float tilt=.18f)
        {
            if(disabled)return;
            var e=Rent(true);e.line.positionCount=9;
            origin.y+=1.2f;var normal=(Vector3.up+Vector3.Cross(Vector3.up,aim)*tilt*(reverse?-1:1)-aim*.08f).normalized;
            e.life=life;e.color=color;e.origin=origin;e.aim=aim.normalized;e.normal=normal;e.radius=radius;e.arc=halfArc;e.sweep=true;e.reverse=reverse;
        }
        public void GroundWave(Vector3 origin,Vector3 aim,float length,float halfWidth,Color color,float life)
        {
            if(disabled)return;Corridor(origin,aim,length,halfWidth,color,life);
            var e=Rent(true);e.line.positionCount=5;
            origin.y=.17f;e.life=life;e.color=color;e.origin=origin;e.aim=aim.normalized;e.radius=length;e.width=halfWidth;e.wave=true;
        }
        public void Corridor(Vector3 origin,Vector3 aim,float length,float halfWidth,Color color,float life)
        {if(disabled)return;origin.y=.13f;var side=Vector3.Cross(Vector3.up,aim.normalized)*halfWidth;Outline(new[]{origin-side,origin+side,origin+aim*length+side,origin+aim*length-side},color,life);}
        public void Symbol(Vector3 position,Color color,float life,bool lightning)
        {
            if(disabled)return;
            var points=lightning?new[]{position+new Vector3(.1f,.3f,0),position+new Vector3(-.12f,.02f,0),position+new Vector3(.12f,.02f,0),position+new Vector3(-.1f,-.25f,0)}:new[]{position+Vector3.up*.18f,position+Vector3.right*.18f,position-Vector3.up*.18f,position-Vector3.right*.18f};Outline(points,color,life,!lightning);
        }
        public void FollowupContact(Vector3 position,bool echo)
        {
            if(disabled)return;
            // A target marker above the crowd identifies an actual extra contact,
            // while the hand trail still shows its source. This is not an area ring.
            var center=position+Vector3.up*1.35f;var color=echo?new Color(1,.48f,1):new Color(1,.7f,.25f);
            if(echo)Outline(new[]{center+Vector3.up*.3f,center+Vector3.right*.3f,center-Vector3.up*.3f,center-Vector3.right*.3f},color,.32f);
            else Outline(new[]{center+new Vector3(-.23f,-.18f,0),center+new Vector3(0,.2f,0),center+new Vector3(.23f,-.18f,0)},color,.18f,false);
        }
        public void Ring(Vector3 center,float radius,Color color,float life=.4f)
        {
            if(disabled)return;
            var e=Rent(true);e.root.position=center;var l=e.line;l.useWorldSpace=false;l.loop=true;l.positionCount=65;l.numCornerVertices=2;
            e.life=life;e.radius=radius;e.color=color;e.ring=true;
        }
        public void Burst(Vector3 position,Color color,int count=7,float force=3)
        {
            if(disabled)return;
            for(int i=0;i<count;i++)
            {
                var e=Rent(false);var t=e.root;t.position=position;t.localScale=new Vector3(.045f,.045f,.18f);t.GetComponent<Renderer>().sharedMaterial=color.r>.8f?VisualFactory.GoldGlow:VisualFactory.Glow;
                var v=Random.onUnitSphere*force;v.y=Mathf.Abs(v.y)+.7f;t.rotation=Random.rotation;
                e.velocity=v*(reducedMotion?.25f:1);e.life=.2f+Random.value*.23f;e.color=color;e.scale=t.localScale;
            }
        }
        public void Arc(Vector3 from,Vector3 to,Color color)
        {
            if(disabled)return;
            var e=Rent(true);var l=e.line;l.positionCount=3;l.SetPosition(0,from);l.SetPosition(1,(from+to)*.5f+Vector3.up*.17f);l.SetPosition(2,to);e.life=.17f;e.color=color;
        }
        public void Step(float dt)
        {
            if(previewEnabled)previewParticles.Step(dt);
            for(int i=effects.Count-1;i>=0;i--)
            {
                var e=effects[i];e.age+=dt;float t=e.age/e.life;
                if(t>=1){Release(e);effects.RemoveAt(i);continue;}
                if(e.line!=null)
                {
                    var c=e.color;c.a*=e.threat?Mathf.Min(1,(1-t)*5):1-t;e.line.startColor=e.line.endColor=c;e.line.widthMultiplier=(e.threat?.13f:e.sweep?.16f:e.ring?.11f:.08f)*(e.threat?1:1-t*.5f);
                    if(e.ring)for(int j=0;j<65;j++){float a=j/64f*Mathf.PI*2;float r=e.radius*Mathf.Pow(t,.65f);e.line.SetPosition(j,new Vector3(Mathf.Sin(a)*r,.05f+(reducedMotion?0:Mathf.Sin(t*Mathf.PI)*.15f),Mathf.Cos(a)*r));}
                    if(e.sweep)for(int j=0;j<9;j++){float p=Mathf.Clamp01(t-j*.065f);float a=Mathf.Lerp(-e.arc,e.arc,e.reverse?1-p:p);var offset=Quaternion.AngleAxis(a,Vector3.up)*e.aim*e.radius;offset.y=-(offset.x*e.normal.x+offset.z*e.normal.z)/e.normal.y;e.line.SetPosition(j,e.origin+offset);}
                    if(e.wave)for(int j=0;j<5;j++){float side=(j-2)/2f;float along=Mathf.Clamp(e.radius*t-Mathf.Abs(side)*.18f,0,e.radius);e.line.SetPosition(j,e.origin+e.aim*along+Vector3.Cross(Vector3.up,e.aim)*side*e.width);}
                }
                else{e.velocity+=Vector3.down*7*dt;e.root.position+=e.velocity*dt;e.root.localScale=e.scale*(1-t);}
            }
        }
        public void Clear(){foreach(var e in effects)Release(e);effects.Clear();previewParticles?.Clear();sound?.Clear();}
    }
}
