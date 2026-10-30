using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public bool MechanicReview {get;private set;}
        public string ReviewLabel {get;private set;}="";
        int reviewIndex,reviewFrame,reviewLastIndex=44;float reviewAge;bool reviewCaptures,reviewImpacts;
        float[] reviewCaptureTimes={.05f,.22f,.4f,.6f,.8f,1.05f,1.3f,1.65f,2,2.4f,3.2f,5.2f};
        void StartRequestedMechanicReview()
        {
            var args=Environment.GetCommandLineArgs();reviewImpacts=Array.IndexOf(args,"-reviewImpacts")>=0;int at=Array.IndexOf(args,"-reviewStart");int start=at>=0?int.Parse(args[at+1]):0;at=Array.IndexOf(args,"-reviewCount");int count=at>=0?int.Parse(args[at+1]):ReviewCases.Length;
            if(start<0||start>=ReviewCases.Length||count<1)throw new ArgumentException("Invalid review case range");reviewLastIndex=Math.Min(ReviewCases.Length-1,start+count-1);
            if(Array.IndexOf(args,"-reviewDense")>=0)reviewCaptureTimes=Enumerable.Range(0,60).Select(i=>.025f+i*.1f).ToArray();StartMechanicReview(start);
        }
        string ReviewDirectory=>Path.Combine(Application.dataPath,"..","NativeReview");
        [Serializable] sealed class ReviewTrace {public string mechanic;public float time,actionAge,delay;public string phase;public string[] targets;}
        readonly List<ReviewTrace> reviewTraces=new List<ReviewTrace>();
        RenderTexture reviewTarget;Texture2D reviewPixels;
        void CaptureMechanicFrame(string path)
        {
            // Render directly from the game camera. Hidden windows have no usable screen
            // backbuffer; this records native scene pixels without desktop automation.
            if(reviewTarget==null){reviewTarget=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);reviewTarget.Create();reviewPixels=new Texture2D(1600,900,TextureFormat.RGB24,false);}
            var request=new RenderPipeline.StandardRequest{destination=reviewTarget};
            if(!RenderPipeline.SupportsRenderRequest(cameraRig,request))throw new InvalidOperationException("Pipeline cannot render the mechanic review offscreen");
            RenderPipeline.SubmitRenderRequest(cameraRig,request);
            var previous=RenderTexture.active;RenderTexture.active=reviewTarget;reviewPixels.ReadPixels(new Rect(0,0,1600,900),0,0);reviewPixels.Apply();RenderTexture.active=previous;
            File.WriteAllBytes(path,reviewPixels.EncodeToPNG());
        }
        string[] ReviewCases=>reviewImpacts?LoadoutCatalog.Weapons.Concat(LoadoutCatalog.Abilities).Concat(LoadoutCatalog.Ultimates).Select(d=>d.id).ToArray():AugmentCatalog.All.Select(d=>d.id).Concat(LoadoutCatalog.Armor.Select(d=>d.id)).Concat(LoadoutCatalog.Rigs.Select(d=>d.id)).Concat(new[]{"wraps","greatsword","breachhammer","scattergun","ConductiveBrand","VectorTether","ReapingCut","Counterguard","singularity","tempest","cyclone","redline","SuppressiveVolley"}).ToArray();
        public void StartMechanicReview(int index=0)
        {
            combatRandom=new System.Random(7);MechanicReview=true;reviewIndex=index;reviewAge=0;reviewFrame=0;reviewCaptures=Array.IndexOf(Environment.GetCommandLineArgs(),"-reviewCapture")>=0;WaveElapsed=0;ProbeReset();
            var def=LoadoutCatalog.Find(ReviewCases[index%ReviewCases.Length]);LoadoutItem host;int start;
            if(def.kind==ItemKind.Augment){host=ProbeHost(LoadoutCatalog.All.First(def.module.Fits),def.module.id);start=host.def.kind==ItemKind.Weapon?0:2;}
            else if(def.kind==ItemKind.Weapon||def.kind==ItemKind.Ability){host=ProbeHost(def);start=def.kind==ItemKind.Weapon?0:2;}
            else
            {
                host=Deck.Acquire(def);Deck.SuggestedPlace(def,out var place,out int mount);Deck.Move(host,place,mount);start=0;
                string weapon=new[]{Signature.Surveyor,Signature.Salvager,Signature.Salvage,Signature.Recovery,Signature.Arcstep}.Contains(def.signature)?"sword":def.signature==Signature.Loader||def.signature==Signature.Recoil||def.signature==Signature.Duelist||def.kind==ItemKind.Ultimate&&def.id=="redline"?"pistol":"fists";
                var basic=Deck.Acquire(LoadoutCatalog.Find(weapon));Deck.Move(basic,PlaceKind.Weapon,0);if(LoadoutCatalog.Magazine(basic)>0)ammunition[basic]=0;
                AbilityId[] skills=def.signature==Signature.Relay?new[]{AbilityId.ConductiveBrand,AbilityId.MassDriver,AbilityId.ArcBolt}:new[]{AbilityId.ArcBolt,AbilityId.MassDriver,AbilityId.Breaker};
                for(int i=0;i<3;i++)Deck.Move(Deck.Acquire(LoadoutCatalog.Abilities.First(d=>d.ability.id==skills[i])),PlaceKind.Sequence,i+2);
                if(def.signature==Signature.Amplifier||def.signature==Signature.Grounding||def.signature==Signature.Relay||def.signature==Signature.FaultLine||def.signature==Signature.Momentum)start=def.signature==Signature.FaultLine||def.signature==Signature.Momentum?3:2;
            }
            ReviewLabel=(index%ReviewCases.Length+1)+" / "+ReviewCases.Length+" · "+def.name+(def.kind==ItemKind.Augment?" on "+host.def.name:"");
            if(!Deck.TwoHanded&&def.kind==ItemKind.Augment&&host.def.kind==ItemKind.Weapon)MoveBasicReview(Deck.Basics[1],4);
            sequencePointer=start;hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);
            bool lethal=new[]{Signature.Salvager,Signature.Salvage,Signature.Recovery,Signature.Arcstep}.Contains(def.signature);
            var a=ProbeEnemy(0,reviewImpacts?1.9f:2.3f,reviewImpacts?10:lethal?20:125);var b=ProbeEnemy(.7f,reviewImpacts?2.6f:3.6f,reviewImpacts?10:def.signature==Signature.Arcstep?20:125);ProbeEnemy(-.8f,3.3f,125);ProbeEnemy(3.2f,3,125);ProbeEnemy(-3.5f,5,125);
            if(def.signature==Signature.Tactician||def.signature==Signature.Capacitor)a.stun=2;
            if(def.signature==Signature.Reactive){ProbeEnemy(-1,2,125);ProbeEnemy(1,2,125);}
            if(def.signature==Signature.Recovery)Health=200;
            if(def.module?.id==AugmentId.BreachLoad)ammunition[host]=0;
            ActiveSlot=PairSlot=-1;Preparation=recovery=0;UltimateAutomatic=def.kind==ItemKind.Ultimate;UltimateRemaining=UltimateAutomatic?0:100;State=RunState.Running;expectedSlot=Deck.NextOccupied(sequencePointer);
            cameraRig.transform.position=HeroPosition+tuning.cameraOffset;AimCamera();
        }
        void MoveBasicReview(LoadoutItem basic,int index)=>Deck.Move(basic,PlaceKind.Sequence,index);
        void StepMechanicReview(float realDelta)
        {
            if(Input.GetKeyDown(KeyCode.RightArrow)){StartMechanicReview(reviewIndex+1);return;}
            if(Input.GetKeyDown(KeyCode.LeftArrow)){StartMechanicReview((reviewIndex+ReviewCases.Length-1)%ReviewCases.Length);return;}
            if(Input.GetKeyDown(KeyCode.Space)){State=State==RunState.Paused?RunState.Running:RunState.Paused;return;}
            if(State!=RunState.Running)return;float dt=Mathf.Min(realDelta,.05f);accumulator+=dt;
            while(accumulator>=1f/60){const float step=1f/60;accumulator-=step;Elapsed+=step;WaveElapsed+=step;clock+=step;reviewAge+=step;StepSignatures(step);StepEnemies(step);StepHero(step);Physics.SyncTransforms();Physics.Simulate(step);ProcessContacts();StepBodies(step);fx.Step(step);UpdateCamera(step);}
            if(reviewCaptures&&reviewFrame<reviewCaptureTimes.Length&&reviewAge>=reviewCaptureTimes[reviewFrame])
            {
                Directory.CreateDirectory(ReviewDirectory);CaptureMechanicFrame(Path.Combine(ReviewDirectory,(reviewIndex+1).ToString("00")+"-"+ReviewCases[reviewIndex%ReviewCases.Length]+"-"+reviewFrame.ToString("00")+".png"));
                var trace=new ReviewTrace{mechanic=ReviewLabel,time=reviewAge,actionAge=actionAge,delay=Preparation,phase=ActionStatus,targets=enemies.Concat(corpses).Select(e=>e.id+" pos="+e.physics.Center+" velocity="+e.physics.Velocity+" hp="+e.health.ToString("0.0")+" stun="+e.stun.ToString("0.00")+" severed="+e.physics.Severed+" parts="+e.physics.DetachedParts+" cut="+e.physics.LastCutBone+" reaction="+e.reaction.kind+" reacting="+e.reactionLeft.ToString("0.00")).ToArray()};
                File.AppendAllText(Path.Combine(ReviewDirectory,"motion.jsonl"),JsonUtility.ToJson(trace)+"\n");reviewFrame++;
            }
            if(reviewAge>=6){if(reviewCaptures&&reviewIndex>=reviewLastIndex){Application.Quit();return;}StartMechanicReview(reviewIndex+1);}
        }
    }
}
