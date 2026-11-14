using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] sealed class ForemanState
        {
            public int stage,vents,forcedVents,slams,crewCalls;
            public float pressure,ventLeft,vfx,bracedSeconds,ventSeconds,bracedDamage,ventDamage;
            public Dictionary<int,float> actionPressure=new Dictionary<int,float>();
            public Vector3 anchor;
        }
        Enemy Foreman=>enemies.FirstOrDefault(e=>e.foreman!=null);
        public bool ForemanPractice=>CombatPreviewActive&&CombatPreviewPreset==7;
        bool ForemanWave=>(ExpeditionActive&&!CombatPreviewActive&&Wave==6)||ForemanPractice;
        public bool ForemanPresent=>Foreman!=null;
        public float ForemanPressureFraction=>Foreman?.foreman.pressure??0;
        public float ForemanVentRemaining=>Foreman is Enemy e&&e.foreman.stage==2?e.foreman.ventLeft:0;
        public string ForemanPhaseLabel=>Foreman?.foreman.stage==2?"VENTING / +35% DAMAGE":Foreman?.foreman.stage==1?"BRACED / 35% LESS DAMAGE":"APPROACHING";
        public string ForemanCue=>Foreman is Enemy e&&e.attack>=0&&!e.delivered?"PRESS SLAM · "+Mathf.Max(0,1.5f-e.attack).ToString("0.0")+"s":ForemanVentRemaining>0?"VENT OPEN · "+ForemanVentRemaining.ToString("0.0")+"s":"PRESSURE "+Mathf.RoundToInt(ForemanPressureFraction*100)+"% · Stagger + impacts";
        static bool ForemanBraced(Enemy e)=>e.foreman!=null&&e.foreman.stage==1;
        void StartForeman()
        {
            var position=roads.Point(roads.Progress(HeroPosition)+32)+Vector3.up*.04f;
            var planes=GeometryUtility.CalculateFrustumPlanes(cameraRig);
            for(int i=0;i<70&&GeometryUtility.TestPlanesAABB(planes,new Bounds(position+Vector3.up*2.7f,new Vector3(5,6,5)));i++)position+=RoadForward*3;
            if(GeometryUtility.TestPlanesAABB(planes,new Bounds(position+Vector3.up*2.7f,new Vector3(5,6,5))))throw new InvalidOperationException("No concealed Foreman entrance");
            var e=new Enemy{id=++nextId,boss=true,heavy=true,health=tuning.enemyHealth*16*(1+.035f*5),cooldown=2,radius=2.2f,foreman=new ForemanState{anchor=HeroPosition}};
            e.maxHealth=e.health;e.rig=new FighterRig("Shift Foreman",actors,false,true,e.id^Seed,FastSimulation);e.rig.Root.localScale=Vector3.one*2.2f;e.rig.Root.position=position;e.rig.Root.rotation=RoadRotation*Quaternion.Euler(0,180,0);e.rig.ForemanEquipment();
            e.physics=new FighterPhysics(e.rig,e.id,this,false,true);enemies.Add(e);byId.Add(e.id,e);Spawned++;HorizonEntries++;
            if(!FastSimulation)foreach(var body in corpses)body.physics.IgnoreWalker(e.physics,body.freightLeft<=0&&!flights.Any(f=>f.body==body));
            Physics.SyncTransforms();Announce("SHIFT FOREMAN / BREAK THE PRESSURE");
        }
        void OpenForemanVent(Enemy e,bool forced)
        {
            var f=e.foreman;if(f==null||f.stage!=1)return;
            f.stage=2;f.pressure=0;f.ventLeft=4;f.vents++;if(forced)f.forcedVents++;
            e.attack=-1;e.delivered=false;e.cooldown=2;e.stun=0;
            e.rig.ForemanPose(false,true,0);
            fx.Ring(e.physics.Center,2.3f,Color.cyan,.55f);fx.Arc(e.rig.Body.position,e.rig.Body.position+Vector3.up*2,Color.cyan);
            Note(forced?"Foreman pressure broken / vent exposed":"Foreman vents / damage opening",true);
        }
        void AddForemanPressure(Enemy e,float amount)
        {
            if(!ForemanBraced(e)||e.deathAge>=0)return;var f=e.foreman;
            int action=currentCredit?.action??actionSerial;
            f.actionPressure.TryGetValue(action,out float contributed);
            // Multiple hands and derived contacts share one originating action. A later
            // body impact upgrades its contribution rather than spending a second budget.
            float extra=Mathf.Max(0,amount-contributed);f.actionPressure[action]=Mathf.Max(amount,contributed);
            f.pressure=Mathf.Min(1,f.pressure+extra);if(f.pressure>=1)OpenForemanVent(e,true);
        }
        float ForemanDamage(Enemy e,float damage)
        {
            if(e.foreman==null)return damage;
            if((currentCredit?.hop??0)>0)AddForemanPressure(e,.28f);
            float result=damage*(e.foreman.stage==1?.65f:e.foreman.stage==2?1.35f:1);
            if(e.foreman.stage==1)e.foreman.bracedDamage+=Mathf.Min(e.health,result);else if(e.foreman.stage==2)e.foreman.ventDamage+=Mathf.Min(e.health,result);
            return result;
        }
        void ForemanReinforcements()
        {
            var boss=Foreman;if(boss==null||spawnTimer>0||WaveElapsed>=30||enemies.Count>=12)return;
            var f=boss.foreman;var role=CrewRole.None;
            if(!crewBaseline&&f.crewCalls<3&&WaveElapsed>=4+f.crewCalls*8&&enemies.Count(e=>e.crew!=CrewRole.None)<3){role=RegionalWave?.crew??(CrewRole)(1+f.crewCalls);if(role==CrewRole.None)role=(CrewRole)(1+f.crewCalls);f.crewCalls++;}
            SpawnEntry(NextFormationEntry(),null,role);spawnTimer=1.25f;
        }
        void StepForeman(Enemy e,float dt)
        {
            var f=e.foreman;var delta=HeroPosition-e.rig.Root.position;delta.y=0;float distance=delta.magnitude;
            e.cooldown-=dt;e.stun=Mathf.Max(0,e.stun-dt);e.react=Mathf.Max(0,e.react-dt*3);f.vfx-=dt;
            bool visible=EnemyCanAttack(e);
            if(f.stage==0&&distance<2.6f&&visible){f.stage=1;f.pressure=0;e.cooldown=1.8f;}
            if(f.stage==1&&visible){f.bracedSeconds+=dt;f.pressure=Mathf.Min(1,f.pressure+dt/9);if(f.pressure>=1)OpenForemanVent(e,false);}
            else if(f.stage==2){f.ventSeconds+=dt;f.ventLeft=Mathf.Max(0,f.ventLeft-dt);if(f.ventLeft<=0){f.stage=1;f.pressure=0;e.cooldown=1.8f;}}
            if(!visible){e.attack=-1;e.delivered=false;e.cooldown=Mathf.Max(e.cooldown,.5f);}
            if(f.stage!=1&&e.stun>0){e.attack=-1;e.cooldown=Mathf.Max(e.cooldown,1);}
            // Close gaps caused by physics even during a vent: melee windows must remain reachable.
            Vector3 desired=delta.normalized*Mathf.Clamp((distance-2.15f)*3,0,f.stage==0?4.5f:2.8f);
            if(e.attack>=0||distance<=2.2f)desired=Vector3.zero;
            var velocity=e.physics.LinearVelocity;var flat=Vector3.MoveTowards(new Vector3(velocity.x,0,velocity.z),desired,14*dt);
            e.physics.LinearVelocity=new Vector3(flat.x,velocity.y,flat.z);if(delta.sqrMagnitude>.01f)e.physics.MoveRotation(Quaternion.LookRotation(delta));
            if(visible&&f.stage==1&&e.attack<0&&e.cooldown<=0&&distance<3.2f){e.attack=0;e.delivered=false;}
            if(e.attack>=0)
            {
                e.attack+=dt;
                if(!e.delivered&&f.vfx<=0){fx.Shape(e.rig.Root.position,e.rig.Root.forward,3.8f,60,new Color(1,.55f,.2f),.22f);f.vfx=.18f;}
                if(!e.delivered&&e.attack>=1.5f)
                {
                    e.delivered=true;f.slams++;
                    if(distance<3.8f&&visible)ReceiveDamage(e,tuning.enemyDamage*2.5f);
                    fx.Ring(e.rig.Root.position+e.rig.Root.forward*1.5f,2.3f,VisualFactory.Orange,.4f);fx.PlayImpact(ImpactKind.Crush);
                }
                if(e.attack>=2.1f){e.attack=-1;e.cooldown=2.2f;}
            }
            e.rig.Pose(clock*.65f,flat.magnitude/3,e.attack>=0?2:-1,e.attack/2.1f,e.react,false,0);StepReaction(e,dt,true);
            e.rig.ForemanPose(f.stage==1&&distance<3.2f,f.stage==2,f.pressure);
            e.rig.Telegraph(e.attack>=0&&!e.delivered,e.stun>0);
        }
        Vector3 FlowAroundForeman(Enemy ally,Vector3 desired)
        {
            var boss=Foreman;if(boss==null)return desired;
            var delta=Quaternion.Inverse(RoadRotation)*(boss.rig.Root.position-ally.rig.Root.position);
            if(delta.z>2||delta.z< -7||Mathf.Abs(delta.x)>2.3f)return desired;
            float side=Mathf.Abs(delta.x)>.2f?-Mathf.Sign(delta.x):ally.id%2==0?1:-1;
            var goal=boss.rig.Root.position+RoadVector(new Vector3(side*2.4f,0,-1.5f));var toward=goal-ally.rig.Root.position;toward.y=0;
            return toward.normalized*Mathf.Max(tuning.enemySpeed,desired.magnitude);
        }
    }
    public sealed partial class PrototypeHUD
    {
        void ForemanPanel(float w)
        {
            var r=game.CombatPreviewActive?new Rect(w-330,330,310,132):new Rect(w/2-245,18,490,122);Panel(r);
            var color=game.ForemanVentRemaining>0?Accent:new Color(1,.64f,.3f);
            Label(new Rect(r.x+12,r.y+8,r.width-24,20),"SHIFT FOREMAN",16,white,true,TextAnchor.MiddleCenter);
            Segments(new Rect(r.x+15,r.y+34,r.width-30,8),game.BossFraction,white,24);
            Label(new Rect(r.x+12,r.y+48,r.width-24,17),game.ForemanPhaseLabel,11,color,true,TextAnchor.MiddleCenter);
            Segments(new Rect(r.x+15,r.y+70,r.width-30,6),game.ForemanVentRemaining>0?game.ForemanVentRemaining/4:game.ForemanPressureFraction,color,18);
            Label(new Rect(r.x+10,r.y+85,r.width-20,32),game.ForemanCue,12,color,true,TextAnchor.MiddleCenter);
        }
    }
}
