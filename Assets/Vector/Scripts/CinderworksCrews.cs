using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public enum CrewRole { None, Repair, Freight, Reclaimer }
    public sealed partial class VectorEncounter
    {
        // Only set by matched development fixtures; ordinary play always enables crews.
        bool crewBaseline;
        public static string CrewName(CrewRole role)=>role==CrewRole.Repair?"REPAIR CREW":role==CrewRole.Freight?"FREIGHT BRUTE":"RECLAIMER";
        public static Color CrewColor(CrewRole role)=>role==CrewRole.Repair?new Color(.45f,1,.65f):role==CrewRole.Freight?new Color(1,.55f,.22f):new Color(.8f,.65f,1);
        static float CrewDuration(CrewRole role)=>role==CrewRole.Repair?1.4f:role==CrewRole.Freight?1.3f:2f;
        public int CrewUses(CrewRole role)=>enemies.Concat(corpses).Where(e=>e.crew==role).Sum(e=>e.crewUses);
        public int CrewSpawned(CrewRole role)=>enemies.Concat(corpses).Count(e=>e.crew==role);
        CrewRole NextCrew()
        {
            if(crewBaseline||!ExpeditionActive||Wave<2||Wave==8||Spawned%7!=3||enemies.Count(e=>e.crew!=CrewRole.None)>=3)return CrewRole.None;
            return Wave==2?CrewRole.Repair:Wave==3?CrewRole.Freight:Wave==4?CrewRole.Reclaimer:(CrewRole)(1+(Spawned/7+Wave)%3);
        }
        bool CrewLine(Vector3 a,Vector3 b)=>FastSimulation||!Physics.Linecast(a+Vector3.up*.5f,b+Vector3.up*.5f,1<<EntryLayout.WorldLayer);
        bool CrewBodyAvailable(Enemy source,Enemy body)=>body!=null&&body.deathAge>=0&&body.freightLeft<=0&&!flights.Any(f=>f.body==body)&&Vector3.Distance(source.physics.Center,body.physics.Center)<7&&InCombatView(body.physics.Center)&&CrewLine(source.physics.Center,body.physics.Center)&&!enemies.Any(e=>e!=source&&e.crewClock>0&&e.crewTarget==body.id);
        Enemy CrewTarget(Enemy source)
        {
            if(source.crew==CrewRole.Repair)return enemies.Where(e=>e!=source&&!e.boss&&e.crew!=CrewRole.Repair&&!e.entering&&e.crewGuard<=0&&!flights.Any(f=>f.body==e)&&Vector3.Distance(source.physics.Center,e.physics.Center)<6&&InCombatView(e.rig.Root.position)&&CrewLine(source.physics.Center,e.physics.Center)).OrderByDescending(e=>e.shieldBearer).ThenBy(e=>(e.physics.Center-source.physics.Center).sqrMagnitude).FirstOrDefault();
            return corpses.Where(e=>CrewBodyAvailable(source,e)).OrderBy(e=>(e.physics.Center-HeroPosition).sqrMagnitude).FirstOrDefault();
        }
        bool CrewTargetValid(Enemy e,Enemy target)=>target!=null&&(e.crew==CrewRole.Repair?target.deathAge<0&&!target.entering&&target.crewGuard<=0&&!flights.Any(f=>f.body==target)&&InCombatView(target.rig.Root.position)&&Vector3.Distance(target.physics.Center,e.physics.Center)<7&&CrewLine(e.physics.Center,target.physics.Center):CrewBodyAvailable(e,target));
        void CancelCrew(Enemy e)
        {if(e.crewClock<=0)return;e.crewClock=0;e.crewTarget=0;e.crewCooldown=2.5f;e.crewNotice=Elapsed+1.1f;}
        bool StepCrew(Enemy e,float dt)
        {
            if(e.crewGuard>0&&e.crewGuardUntil<=Elapsed)e.crewGuard=0;
            if(e.crew==CrewRole.None)return false;
            e.crewCooldown=Mathf.Max(0,e.crewCooldown-dt);
            bool can=EnemyCanAttack(e)&&e.stun<=0&&Vector3.Distance(e.rig.Root.position,HeroPosition)<12&&!flights.Any(f=>f.body==e);
            if(!can){CancelCrew(e);return false;}
            if(e.crewClock<=0&&e.crewCooldown<=0)
            {
                var target=CrewTarget(e);if(target==null){e.crewCooldown=.6f;return false;}
                e.crewTarget=target.id;e.crewClock=.0001f;e.crewVfx=0;e.attack=-1;e.engaged=false;
                e.crewOrigin=target.physics.Center;e.crewAim=HeroPosition+RoadForward*tuning.walkSpeed*1.3f;
            }
            if(e.crewClock<=0)return false;
            byId.TryGetValue(e.crewTarget,out var body);
            if(!CrewTargetValid(e,body)){CancelCrew(e);return false;}
            e.crewClock+=dt;e.crewVfx-=dt;
            if(e.crewVfx<=0)
            {
                var color=CrewColor(e.crew);fx.Arc(e.rig.RightHand.position,body.physics.Center,color);
                if(e.crew==CrewRole.Freight){var direction=e.crewAim-e.crewOrigin;direction.y=0;fx.Corridor(e.crewOrigin,direction.normalized,Mathf.Min(15,direction.magnitude+3),.65f,color,.23f);}
                else fx.Ring(body.physics.Center,.65f,color,.23f);
                e.crewVfx=.2f;
            }
            if(e.crew==CrewRole.Reclaimer)
            {
                var away=e.rig.Root.position-HeroPosition;away.y=0;var destination=e.rig.Root.position+away.normalized*2;
                var delta=destination-body.physics.Center;delta.y=0;
                body.physics.Impulse(Vector3.ClampMagnitude(delta*2-body.physics.Velocity,8)*Mathf.Min(1,dt*5));
            }
            if(e.crewClock<CrewDuration(e.crew))return true;
            e.crewClock=0;e.crewTarget=0;e.crewCooldown=e.crew==CrewRole.Freight?7:6;e.crewUses++;
            if(e.crew==CrewRole.Repair)
            {body.crewGuard=18;body.crewGuardUntil=Elapsed+6;fx.Ring(body.physics.Center,1,CrewColor(e.crew),.6f);Note("Enemy repair restored protection");}
            else if(e.crew==CrewRole.Freight)
            {
                var direction=e.crewAim-body.physics.Center;direction.y=0;
                body.freightLeft=1.4f;body.freightSource=e.id;body.freightPrevious=body.physics.Center;body.freightHitHero=false;body.freightHits=new List<int>();
                body.physics.Impulse(direction.normalized*12+Vector3.up*1.1f-body.physics.Velocity);SetBodyWalkCollision(body,true);Note("Freight brute threw a body");
            }
            else Note("Reclaimer retrieved a body");
            return true;
        }
        float CrewProtection(Enemy e,float damage)
        {
            if(e.crewGuardUntil<=Elapsed)e.crewGuard=0;
            float absorbed=Mathf.Min(e.crewGuard,Mathf.Max(0,damage));e.crewGuard-=absorbed;
            if(absorbed>0){fx.Symbol(e.physics.Center+Vector3.up,CrewColor(CrewRole.Repair),.25f,false);Note("Repaired protection absorbed damage");}
            return damage-absorbed;
        }
        static float CrewSegmentDistance(Vector3 p,Vector3 a,Vector3 b)
        {p.y=a.y=b.y=0;var line=b-a;return Vector3.Distance(p,a+line*Mathf.Clamp01(Vector3.Dot(p-a,line)/Mathf.Max(.001f,line.sqrMagnitude)));}
        void StepFreightBodies(float dt)
        {
            if(!ExpeditionActive&&!ForemanPractice)return;
            foreach(var body in corpses.ToArray())
            {
                if(body.freightLeft<=0)continue;
                var p=body.physics.Center;var previous=body.freightPrevious;body.freightPrevious=p;body.freightLeft-=dt;
                // Cover the camera-visible, unobstructed swept body; each victim is hit once.
                if(InCombatView(p)&&p.y<3&&CrewLine(previous,p))
                {
                    fx.Arc(previous,p,CrewColor(CrewRole.Freight));
                    if(!body.freightHitHero&&CrewSegmentDistance(HeroPosition,previous,p)<1.1f&&CrewLine(p,HeroPosition)&&byId.TryGetValue(body.freightSource,out var source))
                    {body.freightHitHero=true;ReceiveDamage(source,tuning.enemyDamage*1.5f);Note("Freight body hit the Walker");}
                    foreach(var target in enemies.ToArray())
                    {
                        if(target.id==body.freightSource||body.freightHits.Count>=2||body.freightHits.Contains(target.id)||target.entering||target.boss||!CrewLine(p,target.physics.Center)||CrewSegmentDistance(target.physics.Center,previous,p)>=1.1f)continue;
                        body.freightHits.Add(target.id);target.health-=CrewProtection(target,18);target.stun=Mathf.Max(target.stun,.45f);target.engaged=false;target.attack=-1;target.lastPushCredit=null;
                        var impulse=(p-previous).normalized*4;target.physics.Impulse(impulse);ExposeShield(target,.45f);CancelCrew(target);
                        fx.Ring(target.physics.Center,.7f,CrewColor(CrewRole.Freight),.25f);
                        if(target.health<=0){var credit=currentCredit;currentCredit=null;KillEnemy(target,Vector3.zero,false);currentCredit=credit;}
                    }
                }
                if(body.freightLeft<=0)SetBodyWalkCollision(body,false);
            }
        }
        public struct CrewReadout {public CrewRole role;public Vector3 position;public string state;public float progress;public bool detailed;}
        public IEnumerable<CrewReadout> CrewReadouts()
        {
            foreach(var e in enemies)
            {
                if(e.crew!=CrewRole.None&&InCombatView(e.rig.Root.position))yield return new CrewReadout{role=e.crew,position=e.rig.Root.position+Vector3.up*2.6f,progress=e.crewClock/CrewDuration(e.crew),detailed=e.crewClock>0||e.crewNotice>Elapsed,state=e.crewNotice>Elapsed?"INTERRUPTED":(e.crew==CrewRole.Repair?"REPAIRING ":e.crew==CrewRole.Freight?"THROWING ":"RETRIEVING ")+Mathf.Max(0,CrewDuration(e.crew)-e.crewClock).ToString("0.0")+"s"};
                else if(e.crewGuard>0&&e.crewGuardUntil>Elapsed&&InCombatView(e.rig.Root.position))yield return new CrewReadout{role=CrewRole.Repair,position=e.rig.Root.position+Vector3.up*2.4f,state="PATCHED "+e.crewGuard.ToString("0"),progress=-1};
            }
        }
        public Vector3 CrewViewport(Vector3 world)=>cameraRig.WorldToViewportPoint(world);
    }
    public sealed partial class PrototypeHUD
    {
        void CrewCombatLabels(float w,float h)
        {
            var occupied=new List<Rect>();
            foreach(var view in game.CrewReadouts().OrderByDescending(v=>v.detailed))
            {
                var p=game.CrewViewport(view.position);if(p.z<=0)continue;float x=Mathf.Clamp(p.x*w,350,w-350),y=Mathf.Clamp((1-p.y)*h,135,h-245);var color=VectorEncounter.CrewColor(view.role);
                var rect=new Rect(x-100,y-21,200,view.detailed?49:24);var origin=rect.position;
                foreach(var offset in new[]{Vector2.zero,new Vector2(0,-55),new Vector2(0,55),new Vector2(-206,0),new Vector2(206,0),new Vector2(-206,-55),new Vector2(206,-55),new Vector2(0,-110),new Vector2(0,110)})
                {rect.position=new Vector2(Mathf.Clamp(origin.x+offset.x,250,w-450),Mathf.Clamp(origin.y+offset.y,114,h-245));if(!occupied.Any(r=>r.Overlaps(rect)))break;}
                occupied.Add(new Rect(rect.x-2,rect.y-2,rect.width+4,rect.height+4));
                if((rect.position-origin).sqrMagnitude>16)Stroke(new Vector2(p.x*w,(1-p.y)*h),new Vector2(rect.center.x,rect.yMax),color,1);
                Plate(rect,new Color(.015f,.035f,.04f,.92f),color,5);
                if(view.detailed){Label(new Rect(rect.x+6,rect.y+3,188,18),VectorEncounter.CrewName(view.role),12,color,true,TextAnchor.MiddleCenter);Label(new Rect(rect.x+6,rect.y+23,188,17),view.state,10,white,true,TextAnchor.MiddleCenter);Segments(new Rect(rect.x+10,rect.y+44,180,3),view.progress,color,10);}
                else Label(rect,view.progress<0?view.state:VectorEncounter.CrewName(view.role),11,color,true,TextAnchor.MiddleCenter);
            }
        }
    }
}
