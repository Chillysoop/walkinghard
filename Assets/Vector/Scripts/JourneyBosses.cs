using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        // New public-journey bosses keep their state with the actor, including corpses.
        // Legacy developer fixtures retain their original attacks when this is null.
        [Serializable] sealed class JourneyBossState
        {
            public int kind,phase,cycles,beats,strikes,breaks,rallies,linkedAtStart;
            public float age,duration,disruption,extension,vfx;
            public bool broken;
            public Vector3 aim;
            public List<int> links=new List<int>(),impactActions=new List<int>();
            public Dictionary<int,float> actionPressure=new Dictionary<int,float>();
        }
        Enemy JourneyBoss=>enemies.FirstOrDefault(e=>e.journeyBoss!=null);
        public bool JourneyBossPanelVisible=>JourneyBoss?.journeyBoss.kind>0;
        static bool JourneyBossCommitted(Enemy e)=>e.journeyBoss!=null&&e.journeyBoss.kind>0&&(e.journeyBoss.phase==1||e.journeyBoss.phase==2);
        static bool JourneyBossOpen(Enemy e)=>e.journeyBoss?.phase==3;
        public string ActiveBossName=>JourneyBoss?.journeyBoss.kind==1?"BELL FOREMAN":JourneyBoss?.journeyBoss.kind==2?"THE PURSUER":"SHIFT FOREMAN";
        float BossPhaseLeft(Enemy e)=>Mathf.Max(0,e.journeyBoss.duration-e.journeyBoss.age);
        public string JourneyBossPhase
        {
            get
            {
                var e=JourneyBoss;if(e==null)return "";var b=e.journeyBoss;
                if(b.phase==0)return b.kind==1?"THE CONDUCTOR APPROACHES":"PURSUIT / CLOSING DISTANCE";
                if(b.kind==1)return b.phase==1?"RALLY / "+b.links.Count+" LINKED ESCORTS":b.phase==2?"TOLL / "+b.links.Count+" BELLS":b.broken?"SILENCED / +25% DAMAGE":"BELLS COOLING";
                return b.phase==1?"EXECUTION / COMMITTED SWEEP":b.phase==2?"SWEEP FOLLOW-THROUGH":"RECOVERING / +25% DAMAGE";
            }
        }
        public string JourneyBossCue
        {
            get
            {
                var e=JourneyBoss;if(e==null)return "";var b=e.journeyBoss;
                string seconds=BossPhaseLeft(e).ToString("0.0")+"s";
                if(b.phase==0)return b.kind==1?"Clear escorts or disrupt the command":"Control is strongest after its sweep";
                if(b.kind==1)return b.phase<=2?seconds+" · Clear gold links / damage + stagger disrupt":seconds+" · "+(b.broken?"Command broken. Attack now!":"Next command is preparing");
                return seconds+" · "+(b.phase==3?"Body impacts extend recovery":b.phase==1?"Braced: reduced control · prepare protection":"Recovery follows this strike");
            }
        }
        public float JourneyBossMeter=>JourneyBoss is Enemy e?(e.journeyBoss.kind==1&&e.journeyBoss.phase>0&&e.journeyBoss.phase<=2?e.journeyBoss.disruption:BossPhaseLeft(e)/Mathf.Max(.01f,e.journeyBoss.duration)):0;
        public string JourneyBossMeterLabel=>JourneyBoss is Enemy e?(e.journeyBoss.phase==0?"APPROACH":e.journeyBoss.kind==1&&e.journeyBoss.phase<=2?"DISRUPTION":e.journeyBoss.phase==3?"RECOVERY":"STRIKE"):"";
        public bool JourneyBossWindow=>JourneyBoss is Enemy e&&JourneyBossOpen(e);
        public Rect? BossSilhouetteViewport
        {
            get
            {
                var boss=enemies.FirstOrDefault(e=>e.boss);if(boss==null)return null;
                // Reserve the visible body and crown so escort labels cannot cover
                // the boss's armor opening. Eight corners also handle rotated roads.
                var min=new Vector2(float.MaxValue,float.MaxValue);var max=new Vector2(float.MinValue,float.MinValue);
                foreach(float x in new[]{-1.1f,1.1f})foreach(float y in new[]{.4f,3f})foreach(float z in new[]{-.7f,.7f})
                {var p=cameraRig.WorldToViewportPoint(boss.rig.Root.TransformPoint(new Vector3(x,y,z)));if(p.z<=0)return null;min=Vector2.Min(min,p);max=Vector2.Max(max,p);}
                return Rect.MinMaxRect(min.x,1-max.y,max.x,1-min.y);
            }
        }
        void SetBossPhase(Enemy e,int phase,float duration)
        {
            var b=e.journeyBoss;b.phase=phase;b.age=0;b.duration=duration;b.vfx=0;e.attack=-1;e.delivered=false;
            if(phase==3){b.extension=0;b.impactActions.Clear();e.stun=0;}
        }
        void BeginBellCommand(Enemy e)
        {
            var b=e.journeyBoss;SetBossPhase(e,1,3.8f);b.disruption=0;b.broken=false;b.actionPressure.Clear();b.cycles++;
            b.links=enemies.Where(a=>a!=e&&!a.boss&&a.durability?.champion!=true&&!a.entering&&a.stun<=0&&EnemyCanAttack(a)&&Vector3.Distance(a.physics.Center,e.physics.Center)<12&&CrewLine(a.physics.Center,e.physics.Center)).OrderBy(a=>(a.physics.Center-e.physics.Center).sqrMagnitude).Take(3).Select(a=>a.id).ToList();
            b.linkedAtStart=b.links.Count;e.foreman.stage=1;
        }
        void BreakBellCommand(Enemy e,string reason)
        {
            var b=e.journeyBoss;if(b.kind!=1||b.phase!=1&&b.phase!=2)return;
            b.breaks++;RecordBellBreak(reason);b.broken=true;b.links.Clear();SetBossPhase(e,3,3.5f);e.foreman.stage=2;
            fx.Ring(e.physics.Center,3.2f,VisualFactory.Cyan,.55f);Note("Bell command broken / +25% damage",true);
        }
        void AddBellDisruption(Enemy e,float amount)
        {
            var b=e.journeyBoss;if(b?.kind!=1||e.deathAge>=0||b.phase!=1&&b.phase!=2)return;
            int action=currentCredit?.action??actionSerial;b.actionPressure.TryGetValue(action,out float prior);
            b.actionPressure[action]=Mathf.Max(prior,amount);b.disruption=Mathf.Clamp01(b.disruption+Mathf.Max(0,amount-prior));
            if(b.disruption>=1)BreakBellCommand(e,"control");
        }
        float JourneyBossDamage(Enemy e,float damage)
        {
            var b=e.journeyBoss;if(b==null||b.kind==0)return damage;
            float scale=b.phase==3&&(b.kind==2||b.broken)?1.25f:1;
            if(b.kind==1&&(b.phase==1||b.phase==2)&&damage>0)
            {
                // Damage is a universal alternative to control or escort clearing.
                b.disruption=Mathf.Clamp01(b.disruption+Mathf.Min(e.health,damage)/(e.maxHealth*BellDamageThreshold));
                if(b.disruption>=1)BreakBellCommand(e,"damage");
            }
            if(b.kind==2&&b.phase==3&&(currentCredit?.hop??0)>0&&damage>0&&b.extension<2)
            {
                int action=currentCredit?.action??actionSerial;
                if(!b.impactActions.Contains(action)){b.impactActions.Add(action);float extra=Mathf.Min(.5f,2-b.extension);b.extension+=extra;b.duration+=extra;fx.Ring(e.physics.Center,2.5f,VisualFactory.Cyan,.3f);}
            }
            return damage*scale;
        }
        void MoveJourneyBoss(Enemy e,Vector3 delta,float distance,float dt,bool planted,float speed)
        {
            var desired=planted?Vector3.zero:delta.normalized*Mathf.Clamp((distance-(e.journeyBoss.kind==2?PursuerStopDistance:2.15f))*3,0,speed);
            var v=e.physics.LinearVelocity;var planar=new Vector3(v.x,0,v.z);
            var flat=e.stun>0?Vector3.Lerp(planar,Vector3.zero,1-Mathf.Exp(-1.4f*dt)):Vector3.MoveTowards(planar,desired,16*dt);
            e.physics.LinearVelocity=new Vector3(flat.x,v.y,flat.z);
            if(delta.sqrMagnitude>.01f)e.physics.MoveRotation(Quaternion.LookRotation(delta));
        }
        void StepBellForeman(Enemy e,float dt)
        {
            var b=e.journeyBoss;var delta=HeroPosition-e.rig.Root.position;delta.y=0;float distance=delta.magnitude;
            e.stun=Mathf.Max(0,e.stun-dt);e.react=Mathf.Max(0,e.react-dt*3);b.vfx-=dt;
            bool visible=EnemyCanAttack(e);
            if(!visible&&b.phase!=0){SetBossPhase(e,0,0);b.links.Clear();e.foreman.stage=0;}
            if(visible&&distance<3.1f&&b.phase==0&&e.stun<=0)BeginBellCommand(e);
            if(b.phase>0)b.age+=dt;
            if(b.phase==1||b.phase==2)
            {
                b.links.RemoveAll(id=>!byId.TryGetValue(id,out var ally)||ally.deathAge>=0||ally.stun>0||ally.entering||!EnemyCanAttack(ally)||Vector3.Distance(ally.physics.Center,e.physics.Center)>12||!CrewLine(ally.physics.Center,e.physics.Center));
                if(b.linkedAtStart>0&&b.links.Count==0)BreakBellCommand(e,"escorts");
            }
            if(b.phase==1&&b.age>=b.duration)
            {
                foreach(int id in b.links)if(byId.TryGetValue(id,out var ally))ally.cooldown=Mathf.Min(ally.cooldown,.1f);
                b.rallies++;SetBossPhase(e,2,1.6f);
            }
            else if(b.phase==2&&b.age>=b.duration)
            {
                if(visible&&distance<4.7f)ReceiveDamage(e,tuning.enemyDamage*(1+b.links.Count*.65f));
                b.strikes++;fx.Ring(e.rig.Root.position,4.7f,CrewColor(CrewRole.Bellkeeper),.5f);fx.PlayImpact(ImpactKind.Crush);
                SetBossPhase(e,3,2.6f);e.foreman.stage=2;
            }
            else if(b.phase==3&&b.age>=b.duration&&e.stun<=0)BeginBellCommand(e);
            if(b.vfx<=0&&visible)
            {
                if(b.phase==1||b.phase==2)
                {
                    foreach(int id in b.links)if(byId.TryGetValue(id,out var ally)){fx.Arc(e.rig.Body.position+Vector3.up,ally.rig.Body.position,CrewColor(CrewRole.Bellkeeper));fx.Ring(ally.rig.Root.position,.7f,CrewColor(CrewRole.Bellkeeper),.25f);}
                    if(b.phase==2)fx.Shape(e.rig.Root.position,Vector3.forward,4.7f,180,CrewColor(CrewRole.Bellkeeper),.25f);
                }
                b.vfx=.2f;
            }
            MoveJourneyBoss(e,delta,distance,dt,b.phase==2,3.8f);
            e.rig.Pose(clock*.65f,e.physics.LinearVelocity.magnitude/3,b.phase==1||b.phase==2?2:-1,b.phase==1?b.age/b.duration*.45f:b.phase==2?.45f+b.age/b.duration*.55f:0,e.react,false,0);
            e.rig.JourneyBossPose(1,b.links.Count,b.phase==1||b.phase==2,b.phase==3&&b.broken);
            e.rig.Telegraph(b.phase==2,e.stun>0);StepReaction(e,dt,true);
        }
        void StepJourneyPursuer(Enemy e,float dt)
        {
            var b=e.journeyBoss;var delta=HeroPosition-e.rig.Root.position;delta.y=0;float distance=delta.magnitude;
            e.stun=Mathf.Max(0,e.stun-dt);e.react=Mathf.Max(0,e.react-dt*3);b.vfx-=dt;
            bool visible=EnemyCanAttack(e);
            if(!visible&&b.phase!=0)SetBossPhase(e,0,0);
            if(b.phase==0&&visible&&distance<3.3f&&e.stun<=0)
            {SetBossPhase(e,1,e.health<e.maxHealth*.5f?1.6f:2);b.aim=delta.normalized;b.cycles++;}
            if(b.phase>0)b.age+=dt;
            if(b.phase==1&&b.age>=b.duration)
            {
                if(visible&&distance<4.7f&&Vector3.Dot(delta.normalized,b.aim)>.25f)ReceiveDamage(e,tuning.enemyDamage*1.8f);
                b.strikes++;fx.Shape(e.rig.Root.position,b.aim,4.7f,75,VisualFactory.Orange,.5f);fx.PlayImpact(ImpactKind.Crush);SetBossPhase(e,2,.65f);
            }
            else if(b.phase==2&&b.age>=b.duration){SetBossPhase(e,3,3.2f);fx.Ring(e.physics.Center,3,VisualFactory.Cyan,.45f);}
            else if(b.phase==3&&b.age>=b.duration)SetBossPhase(e,0,0);
            if(b.phase==1&&b.vfx<=0){fx.Shape(e.rig.Root.position,b.aim,4.7f,75,VisualFactory.Orange,.25f);fx.Corridor(e.rig.Root.position,b.aim,4.7f,.65f,VisualFactory.Orange,.25f);b.vfx=.18f;}
            MoveJourneyBoss(e,delta,distance,dt,b.phase==1||b.phase==2,1.05f);
            pursuerGaitClock+=dt*.3f;pursuerStride=Mathf.MoveTowards(pursuerStride,e.physics.LinearVelocity.magnitude/1.05f,dt*2);
            e.attack=b.phase==1?b.age:b.phase==2?2+b.age:-1;
            e.rig.Pose(pursuerGaitClock,pursuerStride,b.phase==1||b.phase==2?2:-1,b.phase==1?b.age/b.duration*.65f:b.phase==2?.65f+b.age/b.duration*.35f:0,e.react,false,0);
            e.rig.JourneyBossPose(2,0,b.phase==1||b.phase==2,b.phase==3);e.rig.Telegraph(b.phase==1,e.stun>0);StepReaction(e,dt,true);
        }
    }
    public sealed partial class PrototypeHUD
    {
        void JourneyBossPanel(float w)
        {
            var r=new Rect(w/2-250,18,500,122);Panel(r);var color=game.JourneyBossWindow?Accent:new Color(1,.72f,.35f);
            Label(new Rect(r.x+12,r.y+8,r.width-24,20),game.ActiveBossName,16,white,true,TextAnchor.MiddleCenter);
            Segments(new Rect(r.x+15,r.y+34,r.width-30,8),game.BossFraction,white,24);
            Label(new Rect(r.x+12,r.y+48,r.width-24,17),game.JourneyBossPhase,11,color,true,TextAnchor.MiddleCenter);
            Label(new Rect(r.x+15,r.y+67,85,14),game.JourneyBossMeterLabel,9,color,true);
            Segments(new Rect(r.x+104,r.y+70,r.width-119,6),game.JourneyBossMeter,color,18);
            Label(new Rect(r.x+10,r.y+85,r.width-20,32),game.JourneyBossCue,11,color,true,TextAnchor.MiddleCenter);
        }
    }
}
