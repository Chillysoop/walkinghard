using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        LoadoutItem waitingItem;
        float waitingAge;
        bool countedWait;
        public enum TargetWaitReason { None, OutOfReach, Obstructed, NoEnemy }
        public TargetWaitReason WaitingReason {get;private set;}
        public int WaitingSlot {get;private set;}=-1;
        public int WaitingPairSlot {get;private set;}=-1;
        public float WaitingRange {get;private set;}
        Enemy waitingEnemy;
        public bool ShowTargetWait=>State==RunState.Running&&ActiveSlot<0&&WaitingSlot>=0&&waitingAge>=.35f;
        public string TargetWaitLabel=>WaitingReason==TargetWaitReason.OutOfReach?"OUT OF REACH":WaitingReason==TargetWaitReason.Obstructed?"PATH BLOCKED":"AWAITING ENEMIES";
        public string TargetWaitDetail=>waitingItem==null?"":waitingItem.def.name+" · "+(WaitingReason==TargetWaitReason.OutOfReach?"reach "+WaitingRange.ToString("0.0")+"m":WaitingReason==TargetWaitReason.Obstructed?"waiting for a clear path":"waiting for an enemy to enter combat")+" · sequence held";
        void EndTargetWait(){waitingItem=null;waitingAge=0;countedWait=false;WaitingSlot=WaitingPairSlot=-1;WaitingReason=TargetWaitReason.None;waitingEnemy=null;}
        void ObserveTargetWait(LoadoutItem item,float dt,TargetWaitReason reason=TargetWaitReason.OutOfReach)
        {
            if(waitingItem!=item){EndTargetWait();waitingItem=item;}
            var c=ContributionFor(item);c.rangeWait+=dt;waitingAge+=dt;
            if(reason==TargetWaitReason.OutOfReach)c.outOfReachWait+=dt;
            else if(reason==TargetWaitReason.Obstructed)c.obstructedWait+=dt;
            if(!countedWait&&waitingAge>=.5f){c.waitEpisodes++;countedWait=true;}
        }
        void WaitForTarget(int slot,int pairedSlot,AbilityCard card)
        {
            var item=slot==5?Deck.Ultimate:Deck.HostAt(PlaceKind.Sequence,slot);
            Enemy nearest=null;float distance=float.MaxValue;bool blocked=false;
            foreach(var e in enemies)
            {
                if(e.entering||e.health<=0)continue;
                float d=Vector3.Distance(HeroPosition,e.rig.Root.position);
                if(d<=card.range)blocked=true;
                if(d<distance){distance=d;nearest=e;}
            }
            var reason=blocked?TargetWaitReason.Obstructed:nearest!=null?TargetWaitReason.OutOfReach:TargetWaitReason.NoEnemy;
            ObserveTargetWait(item,1f/60,reason);WaitingSlot=slot;WaitingPairSlot=pairedSlot;WaitingRange=card.range;WaitingReason=reason;
            waitingEnemy=nearest!=null&&InCombatView(nearest.rig.Root.position)?nearest:null;
            ActionStatus="WAITING FOR TARGET / "+card.name;
        }
        public Vector3 WaitScreenPoint(Vector3 world)=>cameraRig.WorldToViewportPoint(world);
        public bool TryGetWaitTarget(out Vector3 point,out float distance)
        {
            point=Vector3.zero;distance=0;
            if(!ShowTargetWait||waitingEnemy==null||waitingEnemy.deathAge>=0||!InCombatView(waitingEnemy.rig.Root.position)||WaitingReason!=TargetWaitReason.OutOfReach)return false;
            point=waitingEnemy.rig.Root.position;distance=Vector3.Distance(HeroPosition,point);return true;
        }
        public WaveItemFeedback TargetWaitWarning=>LastWaveFeedback?.items
            .Where(i=>i.values.outOfReachWait>=3&&i.values.outOfReachWait>=LastWaveFeedback.seconds*.1f&&
                (i.values.waitEpisodes>=2||i.values.outOfReachWait>=6))
            .OrderByDescending(i=>i.values.outOfReachWait).FirstOrDefault();
        internal void PrepareWaitReview()
        {
            var item=Deck.Slots.FirstOrDefault(i=>i?.def.kind==ItemKind.Ability);
            BeginFeedbackWave();ObserveTargetWait(item,2);EndTargetWait();ObserveTargetWait(item,2);
            Elapsed+=20;CaptureWaveFeedback();EndTargetWait();
        }
    }
    public sealed partial class PrototypeHUD
    {
        static readonly Color WaitColor=new Color(1,.75f,.32f);
        void TargetWaitCombat(float w,float h)
        {
            if(!game.ShowTargetWait)return;
            if(game.TryGetWaitTarget(out var target,out float distance))
            {
                // One quiet reach outline and one visible opponent marker; no pulse,
                // particles or concealed-enemy locator. This never changes targeting.
                Vector2 ScreenPoint(Vector3 p){var v=game.WaitScreenPoint(p);return new Vector2(v.x*w,(1-v.y)*h);}
                var center=game.HeroPosition+Vector3.up*.06f;
                for(int i=0;i<48;i++)
                {
                    float a=i*Mathf.PI*2/48,b=(i+1)*Mathf.PI*2/48;
                    var p=center+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*game.WaitingRange;
                    var q=center+new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*game.WaitingRange;
                    if(game.WaitScreenPoint(p).z>0&&game.WaitScreenPoint(q).z>0)Stroke(ScreenPoint(p),ScreenPoint(q),new Color(1,.75f,.32f,.45f),1);
                }
                var marker=ScreenPoint(target+Vector3.up*.1f);var edge=ScreenPoint(center+(target-center).normalized*game.WaitingRange);
                Stroke(edge,marker,WaitColor,1);Stroke(marker+new Vector2(-8,-4),marker+new Vector2(0,4),WaitColor,2);Stroke(marker+new Vector2(0,4),marker+new Vector2(8,-4),WaitColor,2);
                Label(new Rect(Mathf.Clamp(marker.x-75,20,w-170),Mathf.Clamp(marker.y+8,140,h-240),150,18),distance.ToString("0.0")+"m / "+game.WaitingRange.ToString("0.0")+"m reach",11,WaitColor,true,TextAnchor.MiddleCenter);
            }
            Plate(new Rect(w/2-350,h-218,700,49),ink,WaitColor,6);
            Label(new Rect(w/2-338,h-215,676,21),game.TargetWaitLabel,13,WaitColor,true,TextAnchor.MiddleCenter);
            Label(new Rect(w/2-338,h-194,676,19),game.TargetWaitDetail,12,white,false,TextAnchor.MiddleCenter);
        }
        void TargetWaitCheckpoint(float w,float y)
        {
            var warning=game.TargetWaitWarning;if(warning==null)return;
            string text="LAST WAVE OUT OF REACH: "+warning.name+" · "+warning.values.outOfReachWait.ToString("0.0")+"s · DETAILS";
            if(Button(new Rect(355,y,w-390,20),text)){waveDetails=true;detailItem=null;iconGuide=false;}
        }
    }
}
