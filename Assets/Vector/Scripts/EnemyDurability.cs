using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        // Kept on the actor so checkpoint carry-over, delayed impacts and fusion retain identity.
        [Serializable] sealed class DurabilityState
        {
            public bool champion,firstActionKill;
            public int wave,actions,firstAction,starts,interrupts,carried;
            public float pressure,braceAge,exposedUntil,hitUntil,healthBeforeHit,lastPushTime,guardBrokenUntil;
            public HashSet<int> hitActions=new HashSet<int>();
            public Dictionary<int,float> pressureActions=new Dictionary<int,float>();
        }
        bool durabilityBaseline; // Matched QA only; never a player preference.
        static float RoleHealth(bool heavy,CrewRole role,bool champion=false)=>champion?5.5f:role==CrewRole.Linebreaker?2:role==CrewRole.Repair||role==CrewRole.Bellkeeper?1.5f:heavy||role==CrewRole.Freight?3:1;
        float EnemyRoleHealth(bool heavy,CrewRole role,bool champion=false)=>durabilityBaseline||JourneyActive&&Wave<3?heavy?1.9f:1:RoleHealth(heavy,role,champion);
        bool EliteEncounter=>JourneyActive&&!ForemanWave&&!PursuerWave&&Wave>=3&&RegionalWave?.id==EncounterRoute.EliteShift;
        bool ChampionDue=>!durabilityBaseline&&EliteEncounter&&WaveElapsed>=5&&!enemies.Concat(corpses).Any(e=>e.durability?.champion==true&&e.durability.wave==Wave);
        bool ChampionPresent=>enemies.Any(e=>e.durability?.champion==true);
        CrewRole? ChampionEscort()
        {
            var champion=enemies.FirstOrDefault(e=>e.durability?.champion==true&&e.durability.wave==Wave);
            return champion!=null&&!enemies.Concat(corpses).Any(e=>e.id>champion.id&&e.crew==CrewRole.Repair)?CrewRole.Repair:(CrewRole?)null;
        }
        bool EliteBlocked=>!durabilityBaseline&&EliteEncounter&&(ChampionDue||ChampionPresent);
        bool ChampionBraced(Enemy e)=>e.durability?.champion==true&&e.durability.exposedUntil<=Elapsed;
        bool ResistsInterrupt(Enemy e)=>ForemanBraced(e)||ChampionBraced(e)||JourneyBossCommitted(e);
        void InitializeDurability(Enemy e,bool champion=false,int kind=0)
        {
            e.durability=new DurabilityState{champion=champion,wave=Wave,healthBeforeHit=e.health};
            if(!champion)return;
            if(kind>0)e.districtChampion=new DistrictChampionState{kind=kind,duration=.6f};
            e.rig.Root.name=ChampionTitle(e);e.rig.Root.localScale=Vector3.one*1.3f;
            if(kind==0)e.rig.ChampionEquipment();else e.rig.DistrictChampionEquipment(kind);
        }
        void ObserveDurabilityHit(Enemy e,float dealt)
        {
            var d=e.durability;if(d==null)return;
            if(dealt>0){d.healthBeforeHit=Mathf.Max(e.health,d.hitUntil>Elapsed?d.healthBeforeHit:0);d.hitUntil=Elapsed+.45f;}
            if(dealt<=0||currentCredit==null)return;
            if(d.hitActions.Add(currentCredit.action)){if(d.actions++==0)d.firstAction=currentCredit.action;}
        }
        void ExposeChampion(Enemy e,string reason="Champion brace broken")
        {
            if(!ChampionBraced(e)||e.deathAge>=0)return;var d=e.durability;
            d.exposedUntil=Elapsed+3;d.pressure=1;d.braceAge=0;RecoverDistrictChampion(e);
            e.attack=-1;e.engaged=false;e.cooldown=Mathf.Max(e.cooldown,1);CancelCrew(e);
            fx.Ring(e.physics.Center,1.8f,VisualFactory.Cyan,.5f);fx.Burst(e.rig.Body.position,VisualFactory.Cyan,10,3);
            Note(reason+" / +25% damage",true);
        }
        void ChampionPressure(Enemy e,float amount)
        {
            if(!ChampionBraced(e)||e.deathAge>=0)return;var d=e.durability;int action=currentCredit?.action??actionSerial;
            d.pressureActions.TryGetValue(action,out float old);float extra=Mathf.Max(0,amount-old);
            d.pressureActions[action]=Mathf.Max(old,amount);d.pressure=Mathf.Min(1,d.pressure+extra);
            if(d.pressure>=1)ExposeChampion(e);
        }
        void StepChampion(Enemy e,float dt)
        {
            if(e.durability?.champion!=true)return;
            if(e.districtChampion!=null){StepDistrictChampion(e,dt);return;}var d=e.durability;
            if(ChampionBraced(e))
            {
                if(d.pressure>=1){d.pressure=0;d.pressureActions.Clear();}
                if(EnemyCanAttack(e)){d.braceAge+=dt;if(d.braceAge>=8)ExposeChampion(e);}
            }
            e.rig.ChampionPose(ChampionBraced(e),d.pressure);
        }
        float ChampionDamage(Enemy e,float damage)
        {
            if(e.durability?.champion!=true)return damage;
            if((currentCredit?.hop??0)>0)ChampionPressure(e,.45f);
            return damage*(ChampionBraced(e)?1:1.25f);
        }
        bool ChampionDefeatedThisWave=>corpses.Any(e=>e.durability?.champion==true&&e.durability.wave==Wave);
        void AwardEncounterSalvage()
        {
            if(!ExpeditionActive||Wave<=salvageWave)return;
            int bonus=ChampionDefeatedThisWave||!JourneyActive&&ActiveRoute==EncounterRoute.EliteShift?6:0;Salvage+=4+bonus;salvageWave=Wave;
            if(bonus>0)Note("Champion defeated / +6 salvage",true);
        }
        string DurabilityRole(Enemy e)=>e.durability?.champion==true?ChampionTitle(e):e.boss?"Boss":e.crew!=CrewRole.None?e.crew.ToString():e.heavy?"Enforcer":e.shooter?"Shooter":e.shieldBearer?"Shield bearer":"Raider";
    }
    public sealed partial class FighterRig
    {
        Transform championLeft,championRight,championCore;
        public void ChampionEquipment()
        {
            if(lightweight)return;CinderworksWorld.InitializeMaterials();
            foreach(int side in new[]{-1,1})
            {
                var plate=VisualFactory.Node("Champion pressure armor",Body,new Vector3(side*.63f,.75f,.13f));
                VisualFactory.Part("Reinforced plate",plate,PrimitiveType.Cube,Vector3.zero,new Vector3(.52f,.75f,.65f),CinderworksWorld.Ceramic);
                VisualFactory.Part("Brace lamp",plate,PrimitiveType.Cube,new Vector3(0,.05f,.34f),new Vector3(.35f,.13f,.06f),VisualFactory.GoldGlow);
                if(side<0)championLeft=plate;else championRight=plate;
            }
            VisualFactory.Part("Champion crown",Body,PrimitiveType.Cube,new Vector3(0,1.5f,-.4f),new Vector3(1.1f,.14f,.2f),VisualFactory.GoldGlow);
            foreach(int side in new[]{-1,1})VisualFactory.Part("Rank plate support",Body,PrimitiveType.Cube,new Vector3(side*.44f,1.02f,-.4f),new Vector3(.08f,.9f,.12f),CinderworksWorld.Ceramic);
            championCore=VisualFactory.Part("Exposed champion core",Body,PrimitiveType.Cube,new Vector3(0,.4f,.39f),new Vector3(.35f,.48f,.08f),VisualFactory.Glow);championCore.gameObject.SetActive(false);
        }
        public void ChampionPose(bool braced,float pressure)
        {
            if(championLeft==null)return;
            championLeft.localRotation=Quaternion.Euler(0,0,braced?pressure*8:55);
            championRight.localRotation=Quaternion.Euler(0,0,braced?-pressure*8:-55);
            championCore.gameObject.SetActive(!braced);
        }
    }
}
