using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        // Null on existing Freight Champions, including actors loaded from old saves.
        [Serializable] sealed class DistrictChampionState
        {
            public int kind,phase,linkedAtStart,cycles,attacks,hits,breaks,guardedHits;
            public float age,duration,vfx;
            public bool delivered;
            public Vector3 origin,aim,previous;
            public List<int> links=new List<int>();
        }
        bool originalChampions; // Isolated comparison runners only.
        int SpawnChampionKind=>JourneyActive&&!originalChampions?District:0;
        public static string ChampionTitle(int kind)=>kind==1?"SHIELD CAPTAIN":kind==2?"DISPATCH INTERCEPTOR":"FREIGHT CHAMPION";
        static string ChampionTitle(Enemy e)=>ChampionTitle(e.districtChampion?.kind??0);
        static bool DistrictChampionBusy(Enemy e)=>e.districtChampion?.phase>0;
        void SetChampionPhase(Enemy e,int phase,float duration)
        {
            var c=e.districtChampion;c.phase=phase;c.age=0;c.duration=duration;c.vfx=0;
            e.attack=-1;e.engaged=false;e.delivered=false;
        }
        void RecoverDistrictChampion(Enemy e)
        {
            var c=e.districtChampion;if(c==null)return;
            if(c.phase==1||c.phase==2)c.breaks++;
            c.links.Clear();SetChampionPhase(e,3,3);
        }
        bool CaptainLinkValid(Enemy captain,Enemy ally)=>ally!=null&&ally!=captain&&ally.deathAge<0&&!ally.boss&&ally.durability?.champion!=true&&ally.stun<=0&&
            EnemyCanAttack(ally)&&Vector3.Distance(ally.physics.Center,captain.physics.Center)<6&&CrewLine(captain.physics.Center,ally.physics.Center);
        Enemy CaptainGuardFor(Enemy target)=>enemies.FirstOrDefault(e=>e.deathAge<0&&e.districtChampion?.kind==1&&e.districtChampion.phase==1&&
            e.districtChampion.links.Contains(target.id)&&EnemyCanAttack(e)&&CaptainLinkValid(e,target));
        float CaptainGuardDamage(Enemy e,float damage)
        {
            if(damage<=0)return damage;var captain=CaptainGuardFor(e);if(captain==null)return damage;captain.districtChampion.guardedHits++;
            Note("Captain guard reduced escort damage");return damage*.65f;
        }
        void BeginChampionAttack(Enemy e)
        {
            var c=e.districtChampion;c.cycles++;c.delivered=false;c.links.Clear();
            SetChampionPhase(e,1,c.kind==1?4.5f:1.5f);
            c.origin=c.previous=e.rig.Root.position;
            if(c.kind==1)
            {
                c.links=enemies.Where(a=>CaptainLinkValid(e,a)).OrderBy(a=>(a.physics.Center-e.physics.Center).sqrMagnitude).Take(2).Select(a=>a.id).ToList();
                c.linkedAtStart=c.links.Count;
            }
            else
            {
                var target=HeroPosition+RoadForward*tuning.walkSpeed*c.duration;
                var delta=target-c.origin;delta.y=0;
                c.aim=c.origin+(delta.sqrMagnitude>.01f?delta.normalized:RoadForward)*Mathf.Clamp(delta.magnitude+2,3,9);
            }
        }
        void StepDistrictChampion(Enemy e,float dt)
        {
            var c=e.districtChampion;var d=e.durability;
            if(e.deathAge>=0)return;
            if(c.phase==3)
            {
                if(Elapsed>=d.exposedUntil){d.pressure=0;d.pressureActions.Clear();SetChampionPhase(e,0,.6f);}
                return;
            }
            bool can=EnemyCanAttack(e)&&e.stun<=0&&!flights.Any(f=>f.body==e);
            if(!can)
            {
                // An unseen or obstructed attack cannot keep ticking toward a hit.
                if(c.phase==2){c.phase=0;ExposeChampion(e,"Champion recovering");}
                else if(c.phase>0){c.links.Clear();SetChampionPhase(e,0,.6f);}
                return;
            }
            c.age+=dt;
            float distance=Vector3.Distance(e.rig.Root.position,HeroPosition);
            if(c.phase==0)
            {
                if(c.age>=c.duration&&distance<(c.kind==1?3.1f:7))BeginChampionAttack(e);
                return;
            }
            c.vfx-=dt;
            if(c.kind==1)
            {
                if(c.phase==1)
                {
                    c.links.RemoveAll(id=>!byId.TryGetValue(id,out var ally)||!CaptainLinkValid(e,ally));
                    if(c.linkedAtStart>0&&c.links.Count==0){ExposeChampion(e,"Escort guard broken");return;}
                    if(c.vfx<=0)
                    {
                        foreach(int id in c.links){var ally=byId[id];fx.Arc(e.rig.Body.position,ally.rig.Body.position,new Color(1,.78f,.25f));fx.Ring(ally.physics.Center,.85f,new Color(1,.78f,.25f),.25f);}
                        c.vfx=.18f;
                    }
                    if(c.age>=c.duration){c.links.Clear();SetChampionPhase(e,2,1.2f);}
                }
                else
                {
                    if(c.vfx<=0){fx.Shape(e.rig.Root.position,Vector3.forward,3.1f,180,VisualFactory.Orange,.25f);c.vfx=.18f;}
                    if(c.age>=c.duration)
                    {
                        c.attacks++;
                        if(distance<3.1f){ReceiveDamage(e,tuning.enemyDamage*1.8f);c.hits++;}
                        // Natural recovery is distinct from a successful interruption.
                        c.phase=0;ExposeChampion(e,"Captain recovering");
                    }
                }
            }
            else if(c.phase==1)
            {
                if(c.vfx<=0){var delta=c.aim-c.origin;delta.y=0;fx.Corridor(c.origin,delta.normalized,delta.magnitude,1.25f,new Color(1,.25f,.35f),.25f);c.vfx=.18f;}
                if(c.age>=c.duration){c.attacks++;SetChampionPhase(e,2,.85f);c.previous=e.rig.Root.position;}
            }
            else
            {
                var p=e.rig.Root.position;
                if(!c.delivered&&InCombatView(p)&&CrewLine(c.previous,p)&&CrewSegmentDistance(HeroPosition,c.previous,p)<1.25f&&CrewLine(p,HeroPosition))
                {c.delivered=true;c.hits++;ReceiveDamage(e,tuning.enemyDamage*2.2f);Note("Interceptor charge connected");}
                c.previous=p;
                var direction=c.aim-c.origin;direction.y=0;
                bool wall=!FastSimulation&&Physics.SphereCast(p+Vector3.up*1.2f,.55f,direction.normalized,out _,1,1<<EntryLayout.WorldLayer);
                if(c.age>=c.duration||Vector3.Distance(p,c.aim)<.7f||wall)
                {c.phase=0;ExposeChampion(e,wall?"Charge stopped at wall":"Interceptor recovering");}
            }
        }
        Vector3 DistrictChampionVelocity(Enemy e,Vector3 desired)
        {
            var c=e.districtChampion;if(c==null||e.stun>0)return desired;
            if(c.kind==2&&c.phase==2){var direction=c.aim-c.origin;direction.y=0;return direction.normalized*12;}
            return c.phase>0?Vector3.zero:desired;
        }
        string DistrictChampionPhase(Enemy e)
        {
            var c=e.districtChampion;if(c==null)return "";
            if(c.phase==0)return "CLOSING DISTANCE";
            if(c.phase==3)return "RECOVERING";
            string action=c.kind==1?(c.phase==1?"GUARD / "+c.links.Count+" ESCORTS":"BASH"):c.phase==1?"CHARGE WIND-UP":"CHARGING";
            return action+" · "+Mathf.Max(0,c.duration-c.age).ToString("0.0")+"s";
        }
        int ForkDestinationWave=>Mathf.Clamp((forkWave>0?forkWave:Wave)+1,1,WaveCount);
        public int ForkChampionKind=>JourneyActive?Mathf.Clamp((ForkDestinationWave-1)/7,0,2):0;
        public bool ForkLeadsToBoss=>JourneyActive&&LongJourney.Encounters[ForkDestinationWave-1].boss;
        public string ForkThreatLabel(EncounterRoute route)=>route!=EncounterRoute.EliteShift?ForkLabel(route):ForkLeadsToBoss?"HEAVY ESCORT":ChampionTitle(ForkChampionKind);
        public string ForkRewardLabel(EncounterRoute route)=>ForkLeadsToBoss&&RegionalEncounters.Find(route)?.Combat==true?"BOSS ESCORT":ForkBenefit(route);
        public string ForkDestinationName(EncounterRoute route)=>route==EncounterRoute.EliteShift?ForkThreatLabel(route):RouteName(route);
        string ChampionEncounterPreview(int wave)=>ChampionTitle(Mathf.Clamp((wave-1)/7,0,2))+" · "+((wave-1)/7==1?"break escort guard or punish bash recovery":(wave-1)/7==2?"marked charge → exposed recovery":"launch breaks brace · body throws")+" · +6 salvage";
    }
}
