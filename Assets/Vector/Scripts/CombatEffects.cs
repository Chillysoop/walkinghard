using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorGame
{
    public sealed class CombatEffects
    {
        class Effect { public Transform root; public LineRenderer line; public float age,life,radius,arc,width;public Vector3 velocity,origin,aim,normal;public Color color;public bool ring,sweep,reverse,wave;public Vector3 scale; }
        readonly List<Effect> effects=new List<Effect>();
        readonly Transform parent;
        readonly AudioSource audio;
        readonly AudioClip punch,finish,push,damage,slash,crush,electric,shot;
        public bool muted;
        readonly bool disabled=Application.isBatchMode&&System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-nographics")>=0;
        public CombatEffects(Transform p,bool silent)
        {
            parent=p;muted=silent;
            audio=p.gameObject.AddComponent<AudioSource>();audio.spatialBlend=0;audio.volume=.4f;
            punch=Sound("Fist impact",.11f,130,22,.6f);
            finish=Sound("Palm finisher",.20f,75,13,.5f);
            push=Sound("Field discharge",.65f,210,10,.8f);
            damage=Sound("Armor hit",.10f,65,24,.3f);
            slash=Sound("Blade shear",.18f,630,19,.82f);
            crush=Sound("Hammer crush",.29f,48,12,.4f);
            electric=Sound("Arc crackle",.22f,880,17,.76f);
            shot=Sound("Pulse strike",.12f,240,27,.65f);
        }
        static AudioClip Sound(string name,float seconds,float freq,float decay,float noise)
        {
            int count=(int)(44100*seconds);var data=new float[count];var random=new System.Random(name.Length*87);
            for(int i=0;i<count;i++){float t=(float)i/44100;float envelope=Mathf.Exp(-t*decay)*Mathf.Min(1,t*450);float tone=Mathf.Sin(2*Mathf.PI*freq*(t-t*t*.6f));data[i]=(tone*(1-noise)+((float)random.NextDouble()*2-1)*noise)*envelope*.52f;}
            var c=AudioClip.Create(name,count,1,44100,false);c.SetData(data,0);return c;
        }
        public void Play(int kind){if(!disabled&&!muted)audio.PlayOneShot(kind==0?punch:kind==1?finish:kind==2?push:damage);}
        public void PlayImpact(ImpactKind kind)
        {if(!disabled&&!muted)audio.PlayOneShot(kind==ImpactKind.Slash?slash:kind==ImpactKind.Crush?crush:kind==ImpactKind.Electric?electric:kind==ImpactKind.Bullet?shot:kind==ImpactKind.Force||kind==ImpactKind.Pull?push:punch,.6f);}
        public void Contact(ImpactReaction hit,bool lethal)
        {
            if(disabled)return;
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
                var t=VisualFactory.Part("Impact debris",parent,PrimitiveType.Cube,point,new Vector3(.05f,.04f,.17f),material);
                Vector3 velocity=(direction+Random.insideUnitSphere*.6f)*force+Vector3.up*.8f;t.rotation=Quaternion.LookRotation(velocity);
                effects.Add(new Effect{root=t,velocity=velocity,life=.18f+Random.value*.18f,scale=t.localScale});
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
        void Outline(Vector3[] points,Color color,float life,bool closed=true)
        {
            if(disabled)return;
            var go=new GameObject("Gameplay footprint");go.transform.SetParent(parent);var l=go.AddComponent<LineRenderer>();l.sharedMaterial=VisualFactory.Fx;l.useWorldSpace=true;l.loop=closed;l.positionCount=points.Length;l.SetPositions(points);l.widthMultiplier=.065f;l.shadowCastingMode=ShadowCastingMode.Off;
            effects.Add(new Effect{root=go.transform,line=l,life=life,color=color});
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
            var go=new GameObject(reverse?"Return cut":"Outgoing cut");go.transform.SetParent(parent);
            var line=go.AddComponent<LineRenderer>();line.sharedMaterial=VisualFactory.Fx;line.useWorldSpace=true;line.positionCount=9;line.shadowCastingMode=ShadowCastingMode.Off;
            origin.y+=1.2f;var normal=(Vector3.up+Vector3.Cross(Vector3.up,aim)*tilt*(reverse?-1:1)-aim*.08f).normalized;
            effects.Add(new Effect{root=go.transform,line=line,life=life,color=color,origin=origin,aim=aim.normalized,normal=normal,radius=radius,arc=halfArc,sweep=true,reverse=reverse});
        }
        public void GroundWave(Vector3 origin,Vector3 aim,float length,float halfWidth,Color color,float life)
        {
            if(disabled)return;Corridor(origin,aim,length,halfWidth,color,life);
            var go=new GameObject("Ground shockwave");go.transform.SetParent(parent);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=VisualFactory.Fx;line.useWorldSpace=true;line.positionCount=5;line.shadowCastingMode=ShadowCastingMode.Off;
            origin.y=.17f;effects.Add(new Effect{root=go.transform,line=line,life=life,color=color,origin=origin,aim=aim.normalized,radius=length,width=halfWidth,wave=true});
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
            var go=new GameObject("Expanding field ring");go.transform.SetParent(parent);go.transform.position=center;
            var l=go.AddComponent<LineRenderer>();l.sharedMaterial=VisualFactory.Fx;l.useWorldSpace=false;l.loop=true;l.positionCount=65;l.widthMultiplier=.09f;l.numCornerVertices=2;l.shadowCastingMode=ShadowCastingMode.Off;
            effects.Add(new Effect{root=go.transform,line=l,life=life,radius=radius,color=color,ring=true});
        }
        public void Burst(Vector3 position,Color color,int count=7,float force=3)
        {
            if(disabled)return;
            for(int i=0;i<count;i++)
            {
                var t=VisualFactory.Part("Impact spark",parent,PrimitiveType.Cube,position,new Vector3(.045f,.045f,.18f),color.r>.8f?VisualFactory.GoldGlow:VisualFactory.Glow);
                var v=Random.onUnitSphere*force;v.y=Mathf.Abs(v.y)+.7f;t.rotation=Random.rotation;
                effects.Add(new Effect{root=t,velocity=v,life=.2f+Random.value*.23f,color=color,scale=t.localScale});
            }
        }
        public void Arc(Vector3 from,Vector3 to,Color color)
        {
            if(disabled)return;
            var go=new GameObject("Strike trail");go.transform.SetParent(parent);
            var l=go.AddComponent<LineRenderer>();l.sharedMaterial=VisualFactory.Fx;l.positionCount=3;l.SetPosition(0,from);l.SetPosition(1,(from+to)*.5f+Vector3.up*.17f);l.SetPosition(2,to);l.widthMultiplier=.065f;
            effects.Add(new Effect{root=go.transform,line=l,life=.17f,color=color});
        }
        public void Step(float dt)
        {
            for(int i=effects.Count-1;i>=0;i--)
            {
                var e=effects[i];e.age+=dt;float t=e.age/e.life;
                if(t>=1){Object.Destroy(e.root.gameObject);effects.RemoveAt(i);continue;}
                if(e.line!=null)
                {
                    var c=e.color;c.a=1-t;e.line.startColor=e.line.endColor=c;e.line.widthMultiplier=(e.sweep?.16f:e.ring?.11f:.08f)*(1-t*.5f);
                    if(e.ring)for(int j=0;j<65;j++){float a=j/64f*Mathf.PI*2;float r=e.radius*Mathf.Pow(t,.65f);e.line.SetPosition(j,new Vector3(Mathf.Sin(a)*r,.05f+Mathf.Sin(t*Mathf.PI)*.15f,Mathf.Cos(a)*r));}
                    if(e.sweep)for(int j=0;j<9;j++){float p=Mathf.Clamp01(t-j*.065f);float a=Mathf.Lerp(-e.arc,e.arc,e.reverse?1-p:p);var offset=Quaternion.AngleAxis(a,Vector3.up)*e.aim*e.radius;offset.y=-(offset.x*e.normal.x+offset.z*e.normal.z)/e.normal.y;e.line.SetPosition(j,e.origin+offset);}
                    if(e.wave)for(int j=0;j<5;j++){float side=(j-2)/2f;float along=Mathf.Clamp(e.radius*t-Mathf.Abs(side)*.18f,0,e.radius);e.line.SetPosition(j,e.origin+e.aim*along+Vector3.Cross(Vector3.up,e.aim)*side*e.width);}
                }
                else{e.velocity+=Vector3.down*7*dt;e.root.position+=e.velocity*dt;e.root.localScale=e.scale*(1-t);}
            }
        }
        public void Clear(){foreach(var e in effects)if(e.root!=null)Object.Destroy(e.root.gameObject);effects.Clear();}
    }
}
