using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        void CheckCombatFlow()
        {
            var savedState=State;
            void Reset()
            {
                ProbeReset();EndTargetWait();State=RunState.Running;ActiveSlot=PairSlot=-1;
                sequencePointer=expectedSlot=0;recovery=Preparation=0;UltimateAutomatic=false;
                cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
            }
            void MoveTarget(Enemy e,float z)
            {e.rig.Root.position=new Vector3(0,.04f,z);e.physics.Motor.position=e.rig.Root.position;Physics.SyncTransforms();}
            foreach(bool reversed in new[]{false,true})
            {
                Reset();var pistol=Deck.Acquire(LoadoutCatalog.Find("pistol"));var scatter=Deck.Acquire(LoadoutCatalog.Find("scattergun"));
                Deck.Move(pistol,PlaceKind.Weapon,reversed?1:0);Deck.Move(scatter,PlaceKind.Weapon,reversed?0:1);
                var enemy=ProbeEnemy(0,8);int casts=AbilityCasts,pairs=PairedCasts;int[] slots=(int[])SlotCasts.Clone();
                bool waited=true;for(int i=0;i<30;i++)waited&=!Advance();
                MechanicRequire(waited,"mixed gun pair waits for both reaches in either order");
                MechanicRequire(ActiveSlot<0&&AbilityCasts==casts&&PairedCasts==pairs&&NextSlot==0&&SlotCasts[0]==slots[0]&&SlotCasts[1]==slots[1],"waiting pair spends no sequence turn");
                MechanicRequire(Ammo(pistol)==6&&Ammo(scatter)==3&&Preparation==0,"waiting pair spends no ammunition or preparation");
                MechanicRequire(WaitingSlot==(reversed?0:1)&&WaitingPairSlot==(reversed?1:0)&&WaitingReason==TargetWaitReason.OutOfReach&&ShowTargetWait,"pair identifies the limiting hand");
                MechanicRequire(ContributionFor(scatter).outOfReachWait>.49f&&ContributionFor(pistol).rangeWait==0,"only the limiting weapon owns pair range delay");
                MoveTarget(enemy,5);MechanicRequire(Advance()&&PairSlot==1&&actionTarget==enemy&&pairTarget==enemy,"both hands may share one reachable target");
                MechanicRequire(!ShowTargetWait&&WaitingSlot<0&&WaitingPairSlot<0,"starting a turn clears wait indicators immediately");
                Preparation=0;StepHero(.34f);
                MechanicRequire(Ammo(pistol)==5&&Ammo(scatter)==2,"both contacts fire on the simultaneous turn");
            }
            Reset();var longGun=Deck.Acquire(LoadoutCatalog.Find("pistol"));var shortGun=Deck.Acquire(LoadoutCatalog.Find("scattergun"));
            Deck.Move(longGun,PlaceKind.Weapon,0);Deck.Move(shortGun,PlaceKind.Weapon,1);
            var close=ProbeEnemy(0,5);var threat=ProbeEnemy(1,8);RecordEnemyPressure(threat,2);RecordEnemyPressure(threat,2);
            MechanicRequire(Advance()&&actionTarget==threat&&pairTarget==close,"different reachable targets preserve pressure priority and independent pair attacks");

            Reset();var rifle=Deck.Acquire(LoadoutCatalog.Find("rifle"));Deck.Move(rifle,PlaceKind.Weapon,0);ProbeEnemy(0,11);
            MechanicRequire(Advance()&&PairSlot==1&&Deck.TwoHanded&&actionWeapon==pairWeapon,"two-handed locked group still starts once at its own reach");
            Reset();Deck.Move(Deck.Acquire(LoadoutCatalog.Find("pistol")),PlaceKind.Weapon,0);ProbeEnemy(0,8);
            MechanicRequire(Advance()&&PairSlot<0,"nonmatching hands remain independent sequence turns");
            foreach(var def in LoadoutCatalog.Ultimates)
            {
                Reset();var ultimate=Deck.Acquire(def,5);Deck.Move(ultimate,PlaceKind.Ultimate);UltimateRemaining=0;UltimateAutomatic=true;
                var target=ProbeEnemy(0,def.ability.range+.3f);
                MechanicRequire(Advance()&&ActiveSlot==5&&PairSlot<0,"ready ultimate uses its actual rarity-scaled reach: "+def.id);
                MechanicRequire(NextSlot==0,"starting a ready ultimate preserves the next sequence action: "+def.id);
                Preparation=0;StepHero(.6f);
                MechanicRequire(def.id=="redline"?redlineTurns==6:target.health<target.maxHealth,"ultimate delivers its effect inside added rarity reach: "+def.id);
            }
            Reset();Deck.Move(Deck.Acquire(LoadoutCatalog.Find("tempest")),PlaceKind.Ultimate);UltimateRemaining=0;UltimateAutomatic=true;ProbeEnemy(0,12.3f);
            MechanicRequire(!Advance()&&ActiveSlot<0,"Common ultimate does not inherit higher-rarity reach");
            Reset();Deck.Move(Deck.Acquire(LoadoutCatalog.Find("pistol")),PlaceKind.Weapon,0);Deck.Move(Deck.Acquire(LoadoutCatalog.Find("scattergun")),PlaceKind.Weapon,1);
            Deck.Move(Deck.Basics[0],PlaceKind.Sequence,4);Deck.Move(Deck.Basics[1],PlaceKind.Sequence,0);sequencePointer=expectedSlot=4;
            var wrapTarget=ProbeEnemy(0,8);MechanicRequire(!Advance()&&WaitingSlot==0&&WaitingPairSlot==4&&NextSlot==4,"wrapped pair waits without rotating the sequence");
            MoveTarget(wrapTarget,5);MechanicRequire(Advance()&&ActiveSlot==4&&PairSlot==0,"wrapped pair starts across the repeat boundary");

            Reset();for(int i=0;i<12;i++)Advance();
            MechanicRequire(!ShowTargetWait&&WaitingReason==TargetWaitReason.NoEnemy,"brief arrival waits do not flash a banner");
            for(int i=0;i<18;i++)Advance();
            MechanicRequire(ShowTargetWait&&ContributionFor(Deck.Fists).ArrivalWait>.49f&&ContributionFor(Deck.Fists).outOfReachWait==0,"empty encounter is arrival time, never a reach warning");
            var entering=ProbeEnemy(0,2);entering.entering=true;Advance();
            MechanicRequire(WaitingReason==TargetWaitReason.NoEnemy&&!TryGetWaitTarget(out _,out _),"concealed entry does not expose a target marker");
            entering.entering=false;MoveTarget(entering,5);Advance();
            MechanicRequire(WaitingReason==TargetWaitReason.OutOfReach&&TryGetWaitTarget(out _,out float distance)&&distance>4.9f,"visible out-of-range enemy provides a truthful reach marker");
            State=RunState.Paused;MechanicRequire(!ShowTargetWait,"pause hides live wait overlay");State=RunState.Running;
            MoveTarget(entering,2);
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=EntryLayout.WorldLayer;wall.transform.position=new Vector3(0,1.2f,1);wall.transform.localScale=new Vector3(1,2,.1f);Physics.SyncTransforms();
            MechanicRequire(!Advance()&&WaitingReason==TargetWaitReason.Obstructed&&!TryGetWaitTarget(out _,out _),"obstructed contact is not advertised as a range problem");
            DestroyImmediate(wall);Physics.SyncTransforms();MechanicRequire(Advance()&&!ShowTargetWait,"clearing the path resumes the held action");

            Reset();BeginFeedbackWave();ObserveTargetWait(Deck.Fists,8,TargetWaitReason.NoEnemy);Elapsed+=20;CaptureWaveFeedback();
            MechanicRequire(TargetWaitWarning==null&&LastWaveFeedback.items[0].values.ArrivalWait==8,"arrival lulls never recommend a build change");
            EndTargetWait();BeginFeedbackWave();ObserveTargetWait(Deck.Fists,8,TargetWaitReason.OutOfReach);Elapsed+=20;CaptureWaveFeedback();
            MechanicRequire(TargetWaitWarning!=null&&TargetWaitWarning.values.outOfReachWait==8,"real range stalls retain actionable checkpoint feedback");
            ResetWaveFeedback();ProbeReset();State=savedState;UltimateAutomatic=true;
        }
    }
}
