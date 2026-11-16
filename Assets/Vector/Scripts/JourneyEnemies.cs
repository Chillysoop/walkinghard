using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] sealed class SpecialistState
        {
            public int stage;
            public bool hit;
            public Vector3 origin,aim,previous;
        }
        static bool IsSpecialist(CrewRole role)=>role>=CrewRole.Linebreaker;
        static string SpecialistPreview(int wave)=>wave<=3?"LINEBREAKERS · interrupt the red charge lane":wave<=7?"CINDERLOBBERS · interrupt the orange blast circle":wave<=14?"BELLKEEPERS · interrupt the gold rally":"MIXED SPECIALISTS · stagger or launch breaks wind-ups";
        CrewRole NextSpecialist()
        {
            if(!JourneyActive||enemies.Count(e=>IsSpecialist(e.crew))>=2||Spawned%8!=5)return CrewRole.None;
            return DistrictEncounters.Specialist(Wave,Spawned/8+Wave);
        }
        void CancelSpecialist(Enemy e)
        {
            if(e.specialist==null)return;
            if(e.specialist.stage!=0){if(e.durability!=null)e.durability.interrupts++;e.crewNotice=Elapsed+1.1f;e.crewCooldown=3;}
            e.specialist.stage=0;e.crewClock=0;
        }
        bool StepSpecialist(Enemy e,float dt)
        {
            if(e.specialist==null)e.specialist=new SpecialistState();var s=e.specialist;
            e.crewCooldown=Mathf.Max(0,e.crewCooldown-dt);
            bool can=EnemyCanAttack(e)&&e.stun<=0&&!flights.Any(f=>f.body==e);
            if(!can){CancelSpecialist(e);return false;}
            float distance=Vector3.Distance(e.rig.Root.position,HeroPosition);
            if(s.stage==0&&e.crewCooldown<=0&&distance<11&&distance>(e.crew==CrewRole.Linebreaker?2:0))
            {
                s.stage=1;s.hit=false;s.origin=s.previous=e.rig.Root.position;
                if(e.durability!=null)e.durability.starts++;
                s.aim=HeroPosition+RoadForward*tuning.walkSpeed*CrewDuration(e.crew);s.aim.y=.06f;
                e.crewClock=.0001f;e.crewVfx=0;e.engaged=false;e.attack=-1;
            }
            if(s.stage==0)return false;
            e.crewClock+=dt;e.crewVfx-=dt;
            if(s.stage==2)
            {
                var p=e.rig.Root.position;
                if(!s.hit&&CrewSegmentDistance(HeroPosition,s.previous,p)<1.1f&&CrewLine(p,HeroPosition))
                {s.hit=true;ReceiveDamage(e,tuning.enemyDamage*1.7f);Note("Linebreaker charge connected");}
                s.previous=p;
                if(e.crewClock>=CrewDuration(e.crew)+.65f){s.stage=0;e.crewClock=0;e.crewCooldown=6;e.crewUses++;}
                return true;
            }
            if(e.crewVfx<=0)
            {
                var color=CrewColor(e.crew);
                if(e.crew==CrewRole.Linebreaker){var d=s.aim-s.origin;d.y=0;fx.Corridor(s.origin,d.normalized,Mathf.Min(12,d.magnitude+2),.75f,color,.25f);}
                else if(e.crew==CrewRole.Cinderlobber){fx.Shape(s.aim,Vector3.forward,2.2f,180,color,.25f);fx.Arc(e.rig.RightHand.position,s.aim+Vector3.up*.2f,color);}
                else
                {
                    fx.Shape(e.rig.Root.position,Vector3.forward,5.5f,180,color,.25f);
                    foreach(var ally in RallyTargets(e))fx.Arc(e.rig.Body.position,ally.rig.Body.position,color);
                }
                e.crewVfx=.18f;
            }
            if(e.crewClock<CrewDuration(e.crew))return true;
            if(e.crew==CrewRole.Linebreaker){s.stage=2;s.previous=e.rig.Root.position;return true;}
            if(e.crew==CrewRole.Cinderlobber)
            {
                fx.Ring(s.aim,2.2f,CrewColor(e.crew),.5f);fx.Burst(s.aim,CrewColor(e.crew),12,4);
                if(Vector3.Distance(HeroPosition,s.aim)<2.2f&&CrewLine(s.aim,HeroPosition))ReceiveDamage(e,tuning.enemyDamage*1.6f);
                Note("Cinderlobber blast resolved");
            }
            else
            {
                foreach(var ally in RallyTargets(e))
                {ally.cooldown=Mathf.Min(ally.cooldown,.1f);fx.Ring(ally.physics.Center,.8f,CrewColor(e.crew),.6f);}
                Note("Bellkeeper rallied nearby attackers");
            }
            s.stage=0;e.crewClock=0;e.crewCooldown=e.crew==CrewRole.Bellkeeper?8:6;e.crewUses++;
            return true;
        }
        Enemy[] RallyTargets(Enemy e)=>enemies.Where(a=>a!=e&&!a.boss&&a.crew==CrewRole.None&&a.stun<=0&&!a.entering&&Vector3.Distance(a.rig.Root.position,e.rig.Root.position)<5.5f&&CrewLine(e.rig.Root.position,a.rig.Root.position)).OrderBy(a=>(a.rig.Root.position-e.rig.Root.position).sqrMagnitude).Take(3).ToArray();
        Vector3 SpecialistVelocity(Enemy e,Vector3 desired)
        {
            if(e.specialist?.stage!=2||e.stun>0)return desired;
            var direction=e.specialist.aim-e.specialist.origin;direction.y=0;return direction.normalized*12;
        }
    }
    public sealed partial class FighterRig
    {
        public void SpecialistEquipment(CrewRole role)
        {
            if(lightweight||role<CrewRole.Linebreaker)return;CinderworksWorld.InitializeMaterials();
            var root=VisualFactory.Node("Specialist / "+role,Body,Vector3.zero);
            void Box(string name,Vector3 p,Vector3 size,Material m)=>VisualFactory.Part(name,root,PrimitiveType.Cube,p,size,m);
            if(role==CrewRole.Linebreaker)
            {
                foreach(int side in new[]{-1,1})
                {
                    Box("Charge prong",new Vector3(side*.48f,.35f,.6f),new Vector3(.19f,.28f,1.25f),CinderworksWorld.Ceramic);
                    Box("Red ram lamp",new Vector3(side*.48f,.51f,.76f),new Vector3(.18f,.08f,.7f),VisualFactory.SiphonGlow);
                }
                Box("Ram crossbar",new Vector3(0,.32f,.28f),new Vector3(1.4f,.45f,.4f),CinderworksWorld.Iron);
            }
            else if(role==CrewRole.Cinderlobber)
            {
                foreach(int side in new[]{-1,1})
                {
                    var tube=VisualFactory.Part("Mortar chimney",root,PrimitiveType.Cylinder,new Vector3(side*.34f,.9f,-.4f),new Vector3(.4f,.68f,.4f),CinderworksWorld.Enamel);tube.localRotation=Quaternion.Euler(-25,0,0);
                    Box("Mortar warning band",new Vector3(side*.34f,1.33f,-.63f),new Vector3(.45f,.12f,.45f),VisualFactory.RedGlow);
                }
            }
            else
            {
                Box("Bell frame",new Vector3(0,1.1f,-.4f),new Vector3(.13f,1.8f,.14f),CinderworksWorld.Ceramic);
                Box("Bell crown",new Vector3(0,1.91f,-.35f),new Vector3(1.15f,.14f,.18f),CinderworksWorld.Ceramic);
                foreach(int side in new[]{-1,1})VisualFactory.Part("Rally bell",root,PrimitiveType.Cylinder,new Vector3(side*.44f,1.6f,-.35f),new Vector3(.4f,.25f,.4f),VisualFactory.GoldGlow);
            }
            PresentationGeometry.CompactBone(root,"journey/specialist/"+role+"/v1");
        }
    }
}
