using UnityEngine;
using UnityEngine.Rendering;

namespace VectorGame
{
    public sealed partial class CombatEffects
    {
        bool previewEnabled;PreviewParticles previewParticles;
        public void PreviewStyle(bool enabled)
        {previewEnabled=enabled&&!disabled;if(previewEnabled&&previewParticles==null)previewParticles=new PreviewParticles(parent);if(!enabled)previewParticles?.Clear();}
        public void PreviewReducedMotion(bool reduced){if(previewParticles!=null)previewParticles.ReducedMotion=reduced;}
        public int PreviewActive=>previewParticles?.Active??0;
        public void PreviewGather(Vector3 origin,Vector3 aim,Vector3 center,float radius,float width,bool corridor)
        {if(previewEnabled)previewParticles.Gather(origin,aim,center,radius,width,corridor);}
        public void PreviewCharge(Vector3 center,string id,float duration)
        {if(previewEnabled)previewParticles.Ultimate(center,id,duration,false);}
        public void PreviewUltimate(Vector3 center,string id,float radius)
        {if(previewEnabled)previewParticles.Ultimate(center,id,Mathf.Min(radius,8),true);}
    }
    // Fixed pool, cached meshes/material and local deterministic variation: no physics or gameplay RNG.
    sealed class PreviewParticles
    {
        const int Capacity=160;
        sealed class Particle {public Transform t;public MeshRenderer r;public Vector3 origin,velocity;public float age,life,size,endSize,angle;public Color color;public bool ground,contract;}
        readonly Particle[] particles=new Particle[Capacity];readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        readonly Camera viewCamera;
        static Mesh disk,ring;static Material material;
        int cursor;
        static readonly int BaseColor=Shader.PropertyToID("_BaseColor");
        public int Active {get;private set;}
        public bool ReducedMotion {get;set;}
        public PreviewParticles(Transform parent)
        {
            viewCamera=Camera.main;
            if(disk==null){disk=Disc(false);ring=Disc(true);material=new Material(VisualFactory.Fx){name="Preview soft effects"};material.SetInt("_Cull",0);}
            var root=VisualFactory.Node("Bounded presentation particles",parent,Vector3.zero);
            for(int i=0;i<Capacity;i++)
            {
                var node=VisualFactory.Node("Pooled effect",root,Vector3.zero);node.gameObject.AddComponent<MeshFilter>().sharedMesh=i%2==0?ring:disk;
                var renderer=node.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                node.gameObject.SetActive(false);particles[i]=new Particle{t=node,r=renderer,life=0};
            }
        }
        static Mesh Disc(bool hollow)
        {
            const int sides=32;var vertices=new Vector3[sides*3];var colors=new Color[vertices.Length];var indices=new int[sides*12];
            for(int band=0;band<3;band++)for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,r=band==0?(hollow?.78f:0):band==1?(hollow?.9f:.25f):1;
                vertices[band*sides+i]=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0)*.5f;
                colors[band*sides+i]=new Color(1,1,1,band==1?1:0);
            }
            int n=0;for(int band=0;band<2;band++)for(int i=0;i<sides;i++)
            {int a=band*sides+i,b=band*sides+(i+1)%sides,c=a+sides,d=b+sides;indices[n++]=a;indices[n++]=c;indices[n++]=b;indices[n++]=b;indices[n++]=c;indices[n++]=d;}
            var mesh=new Mesh{name=hollow?"Soft shock ring":"Soft impact disk",vertices=vertices,colors=colors,triangles=indices};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void Emit(Vector3 p,Vector3 velocity,float size,float endSize,float life,Color color,bool ground=false,bool hollow=false,bool contract=false)
        {
            if(ReducedMotion){velocity*=.25f;endSize=Mathf.Lerp(size,endSize,.4f);color.a*=.7f;}
            int slot=cursor++%Capacity;var particle=particles[slot];
            particle.t.GetComponent<MeshFilter>().sharedMesh=hollow?ring:disk;particle.origin=p;particle.velocity=velocity;particle.size=size;particle.endSize=endSize;particle.life=life;particle.age=0;particle.color=color;particle.ground=ground;particle.contract=contract;particle.angle=slot*137.5f;
            particle.t.position=p;particle.t.localScale=Vector3.one*size;particle.t.gameObject.SetActive(true);
            particle.r.SetPropertyBlock(null);Draw(particle,0);
        }
        void Draw(Particle p,float t)
        {
            p.t.position=p.contract?p.origin+p.velocity*(1-t):p.origin+p.velocity*p.age;
            p.t.localScale=Vector3.one*Mathf.Lerp(p.size,p.endSize,t);
            p.t.rotation=p.ground?Quaternion.Euler(90,0,p.angle):viewCamera!=null?viewCamera.transform.rotation:Quaternion.identity;
            Color color=p.color;color.a*=Mathf.Pow(1-t,.8f);block.SetColor(BaseColor,color);block.SetColor("_Color",color);p.r.SetPropertyBlock(block);
        }
        public void Step(float dt)
        {Active=0;foreach(var p in particles){if(p.life<=0)continue;p.age+=dt;if(p.age>=p.life){p.life=0;p.t.gameObject.SetActive(false);continue;}Active++;Draw(p,p.age/p.life);}}
        public void Clear(){foreach(var p in particles){p.life=0;p.t.gameObject.SetActive(false);}Active=0;}
        public void Contact(ImpactReaction hit,bool lethal)
        {
            var color=hit.kind==ImpactKind.Slash?new Color(.75f,.94f,1):hit.kind==ImpactKind.Crush?new Color(1,.55f,.18f):hit.kind==ImpactKind.Electric?Color.cyan:hit.kind==ImpactKind.Bullet?new Color(1,.83f,.35f):VisualFactory.Cyan;
            Emit(hit.point,Vector3.zero,lethal?.75f:.46f,.1f,.16f,color);
            int count=ReducedMotion?2:lethal?7:4;
            for(int i=0;i<count;i++)
            {
                float a=(cursor+i*137.5f)*Mathf.Deg2Rad;var outward=new Vector3(Mathf.Sin(a),.2f+Mathf.Abs(Mathf.Cos(a)),Mathf.Cos(a));
                Emit(hit.point,hit.direction*2+outward*1.5f,.17f,.02f,.22f+i*.025f,color);
            }
            if(hit.kind==ImpactKind.Crush)Emit(new Vector3(hit.point.x,.13f,hit.point.z),Vector3.zero,.3f,lethal?3.6f:2.2f,.45f,new Color(.85f,.53f,.28f,.55f),true,true);
            if(hit.kind==ImpactKind.Slash)Emit(hit.point,hit.direction*.4f,.75f,1.45f,.19f,new Color(.7f,.95f,1,.45f),false,true);
        }
        public void Gather(Vector3 origin,Vector3 aim,Vector3 center,float radius,float width,bool corridor)
        {
            // The native footprint owns the range. Motes show actual pull direction inside it.
            origin.y=center.y=.45f;int count=ReducedMotion?4:10;
            if(!corridor)Emit(center-Vector3.up*.3f,Vector3.zero,1.4f,.25f,.55f,new Color(.15f,.8f,1,.5f),true,true);
            Vector3 side=Vector3.Cross(Vector3.up,aim).normalized;
            for(int i=0;i<count;i++)
            {
                float a=i*Mathf.PI*2/count;
                Vector3 target=corridor?origin+aim*(radius*(.15f+.7f*i/count)):center;
                Vector3 start=corridor?target+side*(i%2==0?-1:1)*width*.8f:origin+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius*.85f;
                Emit(target,start-target,.23f,.05f,.55f,VisualFactory.Cyan,false,false,true);
            }
        }
        public void Ultimate(Vector3 center,string id,float value,bool release)
        {
            Color color=id=="singularity"?new Color(.72f,.4f,1):id=="cyclone"?new Color(.68f,.9f,1):id=="tempest"?Color.cyan:id=="harvest"?new Color(1,.12f,.28f):new Color(1,.45f,.1f);
            if(!release)
            {
                Emit(center+Vector3.up*.13f,Vector3.zero,3.2f,.9f,Mathf.Max(.2f,value),color*.7f,true,true);
                int count=ReducedMotion?4:8;
                for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count;Emit(center+Vector3.up, new Vector3(Mathf.Cos(a)*1.7f,.6f,Mathf.Sin(a)*1.7f),.25f,.06f,Mathf.Max(.2f,value),color,false,false,true);}
                return;
            }
            // Redline accelerates attacks; it must not resemble an area-damage blast.
            if(id=="redline")
            {for(int i=-1;i<=1;i+=2)Emit(center+new Vector3(i*.55f,1,0),Vector3.up*1.7f,.6f,.1f,.45f,color);return;}
            // Chain lightning keeps its native target-to-target arcs as the payoff.
            if(id=="tempest")
            {Emit(center+Vector3.up,Vector3.up*.5f,.9f,.2f,.3f,new Color(color.r,color.g,color.b,.6f));return;}
            if(id=="harvest"){Gather(center,Vector3.forward,center,value,0,false);return;}
            Emit(center+Vector3.up*.14f,Vector3.zero,.6f,value*2,.65f,new Color(color.r,color.g,color.b,.65f),true,true);
            Emit(center+Vector3.up,Vector3.zero,2.3f,.2f,.18f,new Color(color.r,color.g,color.b,.6f));
            int rays=ReducedMotion?6:16;
            for(int i=0;i<rays;i++){float a=i*Mathf.PI*2/rays;Emit(center+Vector3.up*.6f,new Vector3(Mathf.Cos(a),.25f,Mathf.Sin(a))*value,.38f,.05f,.55f,color);}
        }
    }
}
