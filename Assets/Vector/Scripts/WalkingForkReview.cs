using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        [Serializable] sealed class ForkReviewReport
        {
            public bool passed;public int assertions,productionAssertions;public float decisionSeconds;
            public string scope="Controlled native fork fixtures with real combat and physics. Durable targets and elevated health exercise the transition; not a difficulty estimate.";
            public List<string> checks=new List<string>();
        }
        IEnumerator ReviewWalkingFork()
        {
            enabled=false;qa=false;fx.muted=true;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception)Application.Quit(3);};
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;Screen.SetResolution(1920,1080,false);
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-reviewOutput");
            string directory=at>=0?args[at+1]:Path.Combine(Application.dataPath,"..","WalkingForkReview");Directory.CreateDirectory(directory);
            var report=new ForkReviewReport{decisionSeconds=ForkDecisionSeconds};
            void Require(bool value,string name){if(!value)throw new InvalidOperationException("WALKING_FORK_REVIEW: "+name);report.assertions++;report.checks.Add(name);}
            IEnumerator Snapshot(string name)
            {
                yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();
                Require(texture.width==Screen.width&&texture.height==Screen.height,"native capture "+name);
                File.WriteAllBytes(Path.Combine(directory,name+".png"),texture.EncodeToPNG());Destroy(texture);yield return null;
            }
            void Advance(int frames){for(int i=0;i<frames;i++)AdvanceCombat(1f/60);}
            void Setup()
            {
                tuning.heroHealth=10000;Begin(true);OpeningPicks=2;Wave=2;tuning.waveCount=8;Health=10000;
                Deck.Move(Deck.Acquire(LoadoutCatalog.Find("sword"),3),PlaceKind.Weapon,0);
                Deck.Move(Deck.Acquire(LoadoutCatalog.Find("sword"),3),PlaceKind.Weapon,1);
                for(int i=0;i<3;i++)Deck.Move(Deck.Acquire(LoadoutCatalog.Find(new[]{"GravityWell","ReapingCut","MassDriver"}[i]),3),PlaceKind.Sequence,i+2);
                Deck.Move(Deck.Acquire(LoadoutCatalog.Find("cyclone")),PlaceKind.Ultimate);
                BeginWave();Announce(Formation(Wave));WaveElapsed=19.7f;spawnTimer=100;SetCombatSpeed(1);ReducedMotion=false;fx.SetReducedMotion(false);
                for(int i=0;i<12;i++){var e=ProbeEnemy(-4+(i%5)*2,3+(i/5)*3,200);e.cooldown=1;e.radius=2.7f;e.orbit=i%2==0?1:-1;Spawned++;}
            }
            for(int i=0;i<12;i++)yield return null;
            CheckCombatMechanics();report.productionAssertions=mechanicAssertions;
            var colliders=regionalWorld.GetComponentsInChildren<Collider>().Select(c=>c.GetInstanceID()).ToArray();
            Setup();for(int i=0;i<90&&!RoutePending;i++)Advance(1);yield return null;
            Require(RoutePending&&State==RunState.Running&&ForkChoice==EncounterRoute.ShieldLine,"default is visible during actual combat");
            Require(forkVisual!=null&&forkVisual.Root.GetComponentsInChildren<Collider>(true).Length==0,"fork geometry adds no colliders");
            Require(colliders.SequenceEqual(regionalWorld.GetComponentsInChildren<Collider>().Select(c=>c.GetInstanceID())),"covered corridor physics retained");
            yield return Snapshot("01-fork-ahead");
            Require(!hud.HandleForkPointer(EventType.MouseDown,new Vector2(5,5),1600,900),"outside click does not choose");
            Require(hud.HandleForkPointer(EventType.MouseDown,hud.ForkChoiceRect(1,1600,900).center,1600,900)&&ForkChoice==EncounterRoute.VolatileBreach,"production pointer handler selects right sign");
            Vector3 before=HeroPosition;float elapsed=Elapsed;int casts=SlotCasts.Sum();Advance(120);
            Require(HeroPosition.z>before.z&&Elapsed>elapsed&&SlotCasts.Sum()>casts,"walking and ordered attacks continue during selection");
            yield return Snapshot("02-right-selected");
            float remaining=ForkSeconds;before=HeroPosition;elapsed=Elapsed;TogglePause();Advance(120);
            Require(ForkSeconds==remaining&&HeroPosition==before&&Elapsed==elapsed,"pause freezes movement combat and deadline");
            Require(!ChooseRoute(EncounterRoute.ShieldLine)&&!hud.HandleForkPointer(EventType.MouseDown,hud.ForkChoiceRect(0,1600,900).center,1600,900),"paused selection rejected");TogglePause();
            while(ForkSeconds>.06f)Advance(1);
            Require(ChooseRoute(EncounterRoute.ShieldLine)&&ChooseRoute(EncounterRoute.VolatileBreach),"late change is accepted before deadline");
            while(RoutePending)Advance(1);
            Require(SelectedRoute==EncounterRoute.VolatileBreach&&routedWave==3&&!ChooseRoute(EncounterRoute.ShieldLine),"deadline locks right route exactly once");
            yield return Snapshot("03-path-locked");
            for(int i=0;i<200&&State==RunState.Running;i++)Advance(1);
            Require(HeroPosition.x>.4f,"walker physically follows selected right lane");
            yield return Snapshot("04-taking-right-lane");
            int rerolls=Deck.Rerolls;if(State==RunState.Running)FinishWave();
            Require(State==RunState.Drafting&&!RoutePending&&Deck.Emphasis==RewardEmphasis.Abilities,"normal reward checkpoint follows moving decision");
            Require(Deck.Offers.Take(2).All(o=>o.def.kind==ItemKind.Ability)&&pendingDrafts.Single()==DraftKind.Equipment,"right route biases existing mixed pick and retains equipment pick");
            Require(Deck.Rerolls==rerolls+1,"one checkpoint reroll only");
            yield return Snapshot("05-route-reward");
            Setup();while(!RoutePending)Advance(1);SetCombatSpeed(4);elapsed=Elapsed;Advance(60);
            Require(Elapsed-elapsed<1.05f&&EffectiveCombatSpeed==1&&CombatSpeed==4,"4x preference is capped only for approach");
            yield return Snapshot("06-speed-cap");
            while(RoutePending)Advance(1);
            Require(SelectedRoute==EncounterRoute.ShieldLine,"no input takes highlighted default");
            for(int i=0;i<200&&State==RunState.Running;i++)Advance(1);
            Require(HeroPosition.x<-.4f,"default physically follows left lane");
            if(State==RunState.Running)FinishWave();
            Require(Deck.Emphasis==RewardEmphasis.Equipment&&Deck.Offers.Take(2).All(o=>o.def.kind==ItemKind.Weapon||o.def.kind==ItemKind.Rig||LoadoutCatalog.IsArmor(o.def.kind)),"left route biases equipment");
            Wave=3;Require(ActiveRoute==EncounterRoute.ShieldLine&&EffectiveCombatSpeed==4,"chosen encounter and preferred speed resume next wave");Wave=4;Require(ActiveRoute==EncounterRoute.Standard,"route lasts exactly one encounter");
            Setup();while(!RoutePending)Advance(1);ReducedMotion=true;fx.SetReducedMotion(true);
            Screen.SetResolution(1280,720,false);for(int i=0;i<8;i++)yield return null;
            Require(hud.HandleForkPointer(EventType.MouseDown,hud.ForkChoiceRect(1,1600,900).center,1600,900),"720p sign uses same logical hit region");
            yield return Snapshot("07-720p-reduced-motion");
            State=RunState.Defeated;remaining=ForkSeconds;Advance(120);Require(ForkSeconds==remaining&&!ChooseRoute(EncounterRoute.ShieldLine),"defeat stops fork and rejects input");
            Begin(true);yield return null;
            Require(!RoutePending&&forkVisual==null&&routedWave==0&&HeroPosition.x==0,"restart clears route geometry and state");
            report.passed=true;File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));
            Debug.Log("WALKING_FORK_REVIEW_SUCCESS / "+report.assertions+" assertions");Application.Quit(0);
        }
    }
}
