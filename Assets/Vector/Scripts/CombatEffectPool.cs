using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorGame
{
    public sealed partial class CombatEffects
    {
        readonly Queue<Effect> freeLines=new Queue<Effect>(),freeThreats=new Queue<Effect>(),freeDebris=new Queue<Effect>();
        int lineCount,threatCount,debrisCount;
        internal bool ReviewUnpooled;
        Material threatMaterial;
        bool reducedMotion;
        int contactFrame=-1,contactCount;
        internal int EffectObjects=>lineCount+threatCount+debrisCount;
        internal int ActiveEffects=>effects.Count;
        internal int ThreatEffects=>effects.FindAll(e=>e.threat).Count;
        internal int RecycledEffects {get;private set;}
        Effect Rent(bool line,bool threat=false)
        {
            var pool=threat?freeThreats:line?freeLines:freeDebris;
            int count=threat?threatCount:line?lineCount:debrisCount,limit=threat?16:line?128:64;
            Effect e;
            if(pool.Count>0)e=pool.Dequeue();
            else if(count>=limit&&!ReviewUnpooled)
            {
                int index=effects.FindIndex(x=>x.threat==threat&&(x.line!=null)==line);
                e=effects[index];effects.RemoveAt(index);RecycledEffects++;
            }
            else
            {
                var root=line?VisualFactory.Node(threat?"Threat footprint":"Combat line",parent,Vector3.zero):VisualFactory.Part("Impact debris",parent,PrimitiveType.Cube,Vector3.zero,Vector3.one,VisualFactory.Glow);
                e=new Effect{root=root};
                if(line){e.line=root.gameObject.AddComponent<LineRenderer>();e.line.shadowCastingMode=ShadowCastingMode.Off;e.line.receiveShadows=false;}
                if(threat)threatCount++;else if(line)lineCount++;else debrisCount++;
            }
            e.age=0;e.life=1;e.ring=e.sweep=e.reverse=e.wave=false;e.threat=threat;e.velocity=Vector3.zero;e.root.localScale=Vector3.one;e.root.rotation=Quaternion.identity;e.root.position=Vector3.zero;e.root.gameObject.SetActive(true);
            if(e.line!=null)
            {
                if(threat&&threatMaterial==null){threatMaterial=new Material(Resources.Load<Shader>("CombatThreat")){name="Threats / foreground priority",renderQueue=3300};}
                e.line.sharedMaterial=threat?threatMaterial:VisualFactory.Fx;e.line.useWorldSpace=true;e.line.loop=false;e.line.numCornerVertices=0;e.line.startColor=e.line.endColor=Color.white;e.line.widthMultiplier=threat?.13f:.065f;
            }
            effects.Add(e);return e;
        }
        void Release(Effect e)
        {if(ReviewUnpooled){if(e.threat)threatCount--;else if(e.line!=null)lineCount--;else debrisCount--;Object.Destroy(e.root.gameObject);return;}e.root.gameObject.SetActive(false);(e.threat?freeThreats:e.line!=null?freeLines:freeDebris).Enqueue(e);}
        public void ThreatShape(Vector3 center,Vector3 aim,float radius,float halfArc,Color color,float life)
        {
            if(disabled)return;center.y=.19f;var points=new List<Vector3>();if(halfArc<180)points.Add(center);
            for(int j=0;j<=24;j++)points.Add(center+Quaternion.AngleAxis(Mathf.Lerp(-halfArc,halfArc,j/24f),Vector3.up)*aim.normalized*radius);
            Outline(points.ToArray(),color,life,true,true);
        }
        public void ThreatCorridor(Vector3 origin,Vector3 aim,float length,float width,Color color,float life)
        {if(disabled)return;origin.y=.19f;var side=Vector3.Cross(Vector3.up,aim.normalized)*width;Outline(new[]{origin-side,origin+side,origin+aim*length+side,origin+aim*length-side},color,life,true,true);}
        bool PrimaryContact()
        {if(contactFrame!=Time.frameCount){contactFrame=Time.frameCount;contactCount=0;}return ++contactCount<=3;}
    }
}
