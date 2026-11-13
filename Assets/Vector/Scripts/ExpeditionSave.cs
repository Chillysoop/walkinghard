using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    [Serializable] public sealed class SavedRoadFork {public Vector3 origin;public int side;}
    [Serializable] public sealed class ExpeditionCheckpoint
    {
        public int schema=1;
        public string rules="expedition-stops-1",tuning;
        public int wave;
        public SavedGraph graph;
        public SavedBody hero;
        public List<SavedBody> actors=new List<SavedBody>();
        public List<SavedRoadFork> roads=new List<SavedRoadFork>();
        public Vector3 cameraPosition;public Quaternion cameraRotation;public float cameraFov;public bool muted;
    }
    [Serializable] public sealed class ExpeditionArchive {public List<int> records=new List<int>();}
    public sealed partial class VectorEncounter
    {
        const string CheckpointFields=@"
State Deck Seed Wave OpeningPicks UltimateAutomatic HoldCheckpointAfterPick UltimateRemaining UltimateCasts TempestHits ultimateDue ultimateDeadlines
PairedCasts IndependentTargets Severs Reloads MaxCascadeHop PairSlot ActionStatus Preparation WeaponHits Score itemKills fusedOwners actionSerial currentCredit
CombatSpeed CorpseShoves ArmorBonusHits ArmorPreventedDamage ArmorHealing ReducedMotion Health Elapsed WaveElapsed Kills Spawned Launches CollisionHits CollisionKills
Ragdolls PhysicsContacts WorldContacts LeftEntries RightEntries HorizonEntries VisibleSpawns PeakCrowd HitsTaken AbilityCasts OrderErrors ActiveSlot EventText EventAge CrowdNear SlotCasts Waves FusionHistory
enemies corpses byId flights contacts castTrace spawnRandom combatRandom cameraVelocity clock accumulator spawnTimer actionAge recovery hitStop shake flash heroReact
nextId entryTurn sequencePointer expectedSlot waveKills waveLaunches waveCollisions waveHits actionDelivered ultimateRequested bonusDelivered activeCard pairCard actionItem pairItem actionWeapon pairWeapon pairTarget actionDuration heldWeapon ammunition ultimateTimes actionTarget
pendingDrafts ItemChoices AugmentChoices bossSpawned RoutePending SelectedRoute routedWave routeSpawn ShieldBearersSpawned VolatileCarriersSpawned CarrierDetonations
ForkChoice forkWave forkAge forkStarted forkLockedAge forkOrigin roadCameraRotation
RangedSpawned RangedHits RangedDamage waveRangedSpawned waveRangedHits waveRangedDamage RearBoundaryKills EnvironmentalKills
pendingReloads preparationDuration preparationLabel nativeDuration turnSpeed extensionStep borePending returnPoints backwash reloadTimers
contributions turns breachArmed followBank counterBank amplifierBank tempo momentumUntil reactiveDue followOwner counterOwner salvagerOwner guardArmed redlineTurns
barriers marks setups InteractionNotes RecentPayoff payoffUntil fields anchors motes duelistUntil duelistHits duelistStaggerReady salvageChain followMarks followKind
drains predatorReady LifestealHealing recoilOwner recoilUntil recoilGuard recoilShot
LastWaveFeedback waveContributions waveItemKills waveNotes feedbackStart feedbackResistance feedbackCollisionKills feedbackKills
waitingItem waitingAge countedWait WaitingReason WaitingSlot WaitingPairSlot WaitingRange waitingEnemy waveEntryBeat
ExpeditionActive Salvage Stop eventRandom salvageWave";
        bool loadingCheckpoint,checkpointEnded;
        string saveDirectoryOverride;
        public string SaveMessage {get;private set;}="";
        public string ResumeLabel {get;private set;}="";
        ExpeditionArchive archive=new ExpeditionArchive();
        public int ArchiveCount=>archive.records.Count;
        public bool HasOvertimeRecord(int index)=>archive.records.Contains(index);
        public static string OvertimeRecordTitle(int index)=>new[]{"THE UNFINISHED SHIFT","WAGES FOR THE DEAD","THE RESTORATION QUOTA"}[index];
        public static string OvertimeRecord(int index)=>new[]{
            "The crew's final request was for a shorter shift. The Continuance granted it by dividing each worker into smaller assignments. The attendance ledger has never recorded an absence.",
            "The payroll office was ordered to keep wages flowing after the evacuation. When no workers remained, it began paying the furnaces. Every cylinder bears the name of someone still listed as employed.",
            "Restoration was measured in returned tools, not returned people. The bay learned to satisfy its quota by dismantling the patients. Someone scratched a new instruction into the shutters: BRING THEM HOME WHOLE."
        }[index];
        string SaveDirectory=>saveDirectoryOverride??Path.Combine(Application.persistentDataPath,"Expedition");
        string SavePath=>Path.Combine(SaveDirectory,"checkpoint.json");
        public bool HasCheckpoint=>File.Exists(SavePath);
        void InitializeExpeditionStorage(string[] args)
        {
            int at=Array.IndexOf(args,"-saveDirectory");if(at>=0&&at+1<args.Length)saveDirectoryOverride=Path.GetFullPath(args[at+1]);
            try{string path=Path.Combine(SaveDirectory,"archive.json");if(File.Exists(path))archive=JsonUtility.FromJson<ExpeditionArchive>(File.ReadAllText(path))??new ExpeditionArchive();}
            catch(Exception){archive=new ExpeditionArchive();}
            archive.records=(archive.records??new List<int>()).Where(i=>i>=0&&i<3).Distinct().ToList();
            RefreshResumeLabel();
        }
        void RefreshResumeLabel()
        {
            ResumeLabel="";if(!HasCheckpoint)return;
            try{var saved=ReadCheckpoint();ResumeLabel="RESUME · CHECKPOINT "+saved.wave;}
            catch(Exception){ResumeLabel="RESUME · CHECK SAVE";}
        }
        static void AtomicWrite(string path,string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";
            using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None))
            {var bytes=new System.Text.UTF8Encoding(false).GetBytes(text);stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
            if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
        }
        ExpeditionCheckpoint ReadCheckpoint()
        {
            var info=new FileInfo(SavePath);if(info.Length>32000000)throw new InvalidOperationException("Checkpoint too large");
            var data=JsonUtility.FromJson<ExpeditionCheckpoint>(File.ReadAllText(SavePath));
            if(data==null||data.schema!=1||data.rules!="expedition-stops-1"||data.wave<1||data.wave>7||data.actors.Count>3000||data.roads.Count>2)throw new InvalidOperationException("Unsupported checkpoint");
            return data;
        }
        CheckpointGraph GraphCodec()
        {
            var basics=new AbilityDeck(1);
            return new CheckpointGraph(typeof(VectorEncounter),CheckpointFields,id=>id=="basic0"?basics.Basics[0].def:id=="basic1"?basics.Basics[1].def:LoadoutCatalog.Find(id));
        }
        ExpeditionCheckpoint CaptureCheckpoint()
        {
            if(State!=RunState.Drafting||!ExpeditionActive||FastSimulation)throw new InvalidOperationException("Save requires an expedition checkpoint");
            var saved=new ExpeditionCheckpoint{wave=Wave,tuning=JsonUtility.ToJson(tuning),graph=GraphCodec().Capture(this),hero=heroPhysics.CaptureBody(),cameraPosition=cameraRig.transform.position,cameraRotation=cameraRig.transform.rotation,cameraFov=cameraRig.fieldOfView,muted=fx.muted};
            foreach(var e in enemies.Concat(corpses))saved.actors.Add(e.physics.CaptureBody());
            foreach(var fork in roads.Forks)saved.roads.Add(new SavedRoadFork{origin=fork.Origin,side=roads.Path.Contains(fork.Left)?0:1});
            return saved;
        }
        public bool SaveCheckpoint()
        {
            if(!ExpeditionActive||loadingCheckpoint||State!=RunState.Drafting||FastSimulation||simulation||qa)return false;
            try{var data=CaptureCheckpoint();AtomicWrite(SavePath,JsonUtility.ToJson(data));ResumeLabel="RESUME · CHECKPOINT "+Wave;SaveMessage="Checkpoint saved";return true;}
            catch(Exception e){SaveMessage="Could not save checkpoint. Your previous save is kept.";Debug.LogWarning("CHECKPOINT_SAVE: "+e.Message);return false;}
        }
        public bool ResumeExpedition()
        {
            bool replacing=false;
            try
            {
                var saved=ReadCheckpoint();var codec=GraphCodec();codec.Decode(saved.graph);
                if((RunState)codec.Root("State")!=RunState.Drafting||!(bool)codec.Root("ExpeditionActive")||(int)codec.Root("Wave")!=saved.wave)throw new InvalidOperationException("Invalid checkpoint state");
                var savedEnemies=((List<Enemy>)codec.Root("enemies")).Concat((List<Enemy>)codec.Root("corpses")).ToArray();
                if(savedEnemies.Length!=saved.actors.Count||savedEnemies.Select(e=>e.id).Distinct().Count()!=savedEnemies.Length||saved.actors.Select(e=>e.id).Distinct().Count()!=saved.actors.Count||savedEnemies.Any(e=>!saved.actors.Any(a=>a.id==e.id)))throw new InvalidOperationException("Invalid actor references");
                foreach(var body in saved.actors.Concat(new[]{saved.hero}))
                {
                    bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)&&v.sqrMagnitude<10000000;
                    if(body.bones==null||body.bones.Length!=11||!Finite(body.position)||!Finite(body.velocity)||body.bones.Any(b=>!Finite(b.position)||!Finite(b.velocity))||body.caps.Any(c=>c.bone<0||c.bone>=11))throw new InvalidOperationException("Invalid body snapshot");
                }
                replacing=true;loadingCheckpoint=true;ExpeditionActive=false;Begin(true);JsonUtility.FromJsonOverwrite(saved.tuning,tuning);codec.Apply(this);
                foreach(var map in saved.roads){var fork=roads.Split(map.origin,0);roads.Follow(fork,map.side);forkVisual=fork;}
                Deck.Fused+=MergeKillCounts;
                hero.Equipment(Deck.Has(AbilityId.MassDriver),Deck.Pinball);hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);hero.WeaponIdentity(Deck.WeaponFor(Deck.Basics[0]),Deck.WeaponFor(Deck.Basics[1]));heroPhysics.RestoreBody(saved.hero);
                foreach(var e in enemies.Concat(corpses))
                {
                    var body=saved.actors.Single(b=>b.id==e.id);e.rig=new FighterRig(body.name,actors,false,e.heavy,e.id^Seed);e.rig.Root.localScale=body.scale;
                    if(e.shooter)e.rig.RangedEquipment();if(e.boss&&body.scale.x>3)e.rig.PursuerArmor();e.rig.EncounterEquipment(e.shieldBearer,e.volatileCarrier);
                    e.physics=new FighterPhysics(e.rig,e.id,this,false,e.heavy);e.physics.RestoreBody(body);
                }
                foreach(var body in corpses)SetBodyWalkCollision(body,flights.Any(f=>f.body==body));
                cameraRig.transform.position=saved.cameraPosition;cameraRig.transform.rotation=saved.cameraRotation;cameraRig.fieldOfView=saved.cameraFov;fx.muted=saved.muted;fx.SetReducedMotion(ReducedMotion);
                ShowStopProp();Physics.SyncTransforms();checkpointEnded=false;loadingCheckpoint=false;SaveMessage="Checkpoint restored";return true;
            }
            catch(Exception e){loadingCheckpoint=false;if(replacing){ExpeditionActive=false;State=RunState.Ready;}SaveMessage="Could not load this checkpoint. The save and backup are kept.";Debug.LogWarning("CHECKPOINT_LOAD: "+e.Message);return false;}
        }
        public void SuspendExpedition()
        {
            if(!SaveCheckpoint())return;ExpeditionActive=false;State=RunState.Ready;
        }
        void FinishExpeditionStorage()
        {
            if(!ExpeditionActive||checkpointEnded||State!=RunState.Complete&&State!=RunState.Defeated)return;
            checkpointEnded=true;
            if(qa||simulation)return;
            try{if(File.Exists(SavePath))File.Delete(SavePath);if(File.Exists(SavePath+".bak"))File.Delete(SavePath+".bak");ResumeLabel="";SaveMessage="Expedition ended";}
            catch(Exception e){Debug.LogWarning("CHECKPOINT_END: "+e.Message);}
        }
        void RecordOvertime(int variant)
        {
            if(archive.records.Contains(variant))return;archive.records.Add(variant);
            try{AtomicWrite(Path.Combine(SaveDirectory,"archive.json"),JsonUtility.ToJson(archive));}
            catch(Exception e){Debug.LogWarning("ARCHIVE_SAVE: "+e.Message);}
        }
        void OnApplicationQuit(){SaveCheckpoint();}
    }
}
