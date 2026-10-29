using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorGame
{
    public sealed class CombatEffects
    {
        class Effect { public Transform root; public LineRenderer line; public float age,life,radius;public Vector3 velocity;public Color color;public bool ring;public Vector3 scale; }
        readonly List<Effect> effects=new List<Effect>();
        readonly Transform parent;
        readonly AudioSource audio;
        readonly AudioClip punch,finish,push,damage;
        public bool muted;
        public CombatEffects(Transform p,bool silent)
        {
            parent=p;muted=silent;
            audio=p.gameObject.AddComponent<AudioSource>();audio.spatialBlend=0;audio.volume=.4f;
            punch=Sound("Fist impact",.11f,130,22,.6f);
            finish=Sound("Palm finisher",.20f,75,13,.5f);
            push=Sound("Field discharge",.65f,210,10,.8f);
            damage=Sound("Armor hit",.10f,65,24,.3f);
        }
        static AudioClip Sound(string name,float seconds,float freq,float decay,float noise)
        {
            int count=(int)(44100*seconds);var data=new float[count];var random=new System.Random(name.Length*87);
            for(int i=0;i<count;i++){float t=(float)i/44100;float envelope=Mathf.Exp(-t*decay)*Mathf.Min(1,t*450);float tone=Mathf.Sin(2*Mathf.PI*freq*(t-t*t*.6f));data[i]=(tone*(1-noise)+((float)random.NextDouble()*2-1)*noise)*envelope*.52f;}
            var c=AudioClip.Create(name,count,1,44100,false);c.SetData(data,0);return c;
        }
        public void Play(int kind){if(!muted)audio.PlayOneShot(kind==0?punch:kind==1?finish:kind==2?push:damage);}
        public void Ring(Vector3 center,float radius,Color color,float life=.4f)
        {
            var go=new GameObject("Expanding field ring");go.transform.SetParent(parent);go.transform.position=center;
            var l=go.AddComponent<LineRenderer>();l.sharedMaterial=VisualFactory.Fx;l.useWorldSpace=false;l.loop=true;l.positionCount=65;l.widthMultiplier=.09f;l.numCornerVertices=2;l.shadowCastingMode=ShadowCastingMode.Off;
            effects.Add(new Effect{root=go.transform,line=l,life=life,radius=radius,color=color,ring=true});
        }
        public void Burst(Vector3 position,Color color,int count=7,float force=3)
        {
            for(int i=0;i<count;i++)
            {
                var t=VisualFactory.Part("Impact spark",parent,PrimitiveType.Cube,position,new Vector3(.045f,.045f,.18f),color.r>.8f?VisualFactory.GoldGlow:VisualFactory.Glow);
                var v=Random.onUnitSphere*force;v.y=Mathf.Abs(v.y)+.7f;t.rotation=Random.rotation;
                effects.Add(new Effect{root=t,velocity=v,life=.2f+Random.value*.23f,color=color,scale=t.localScale});
            }
        }
        public void Arc(Vector3 from,Vector3 to,Color color)
        {
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
                    var c=e.color;c.a=1-t;e.line.startColor=e.line.endColor=c;e.line.widthMultiplier=(e.ring?.11f:.08f)*(1-t*.75f);
                    if(e.ring)for(int j=0;j<65;j++){float a=j/64f*Mathf.PI*2;float r=e.radius*Mathf.Pow(t,.65f);e.line.SetPosition(j,new Vector3(Mathf.Sin(a)*r,.05f+Mathf.Sin(t*Mathf.PI)*.15f,Mathf.Cos(a)*r));}
                }
                else{e.velocity+=Vector3.down*7*dt;e.root.position+=e.velocity*dt;e.root.localScale=e.scale*(1-t);}
            }
        }
        public void Clear(){foreach(var e in effects)if(e.root!=null)Object.Destroy(e.root.gameObject);effects.Clear();}
    }
}
