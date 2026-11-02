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
        // Optional alternate endings, using ordinary damage, health and impact physics.
        IEnumerator CaptureTrailerFinish()
        {
            enabled=false;QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception){Debug.LogError("FINISH_CAPTURE_FAILED: "+message);Application.Quit(3);}};
            Screen.SetResolution(1920,1080,false);
            for(int i=0;i<12;i++)yield return null;
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","FinishCapture"));
            foreach(string ultimate in new[]{"singularity","cyclone"})
            {
                Begin(true);qaBuild="ballet";varietyFixture=false;ConfigureVarietyFixture();
                foreach(var item in Deck.Items.ToArray())if(item.augment!=null)Deck.Move(item.augment,PlaceKind.Inventory);
                Deck.Items.RemoveAll(item=>item.def.kind==ItemKind.Augment);
                Deck.Move(Deck.Basics[0],PlaceKind.Sequence,0);Deck.Move(Deck.Basics[1],PlaceKind.Sequence,1);
                Deck.Move(Deck.Acquire(LoadoutCatalog.Find("greatsword"),3),PlaceKind.Weapon,0);
                Deck.Move(Deck.Acquire(LoadoutCatalog.Find("MassDriver"),3),PlaceKind.Sequence,4);
                Deck.Move(Deck.Basics[0],PlaceKind.Sequence,2);
                var gravity=Deck.Slots.First(i=>i?.def.id=="GravityWell");
                var mass=Deck.Slots.First(i=>i?.def.id=="MassDriver");
                Deck.Move(gravity,PlaceKind.Sequence,0);Deck.Move(mass,PlaceKind.Sequence,1);
                Deck.Open(DraftKind.Ultimate,false);Deck.Move(Deck.Acquire(LoadoutCatalog.Find(ultimate),3),PlaceKind.Ultimate);
                OpeningPicks=2;Wave=6;Health=MaxHealth;State=RunState.Running;
                ActiveSlot=PairSlot=-1;Preparation=0;sequencePointer=0;expectedSlot=Deck.NextOccupied(0);recovery=.15f;
                UltimateAutomatic=false;UltimateRemaining=0;SetCombatSpeed(1);fx.muted=true;
                hud.TrailerCaptureMode();hud.TrailerClearSelection();
                hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,true);
                for(int i=0;i<1080;i++)AdvanceCombat(1f/60);
                // Start the selected sequence at its gather action; then queue the ultimate.
                ActiveSlot=PairSlot=-1;Preparation=0;sequencePointer=0;expectedSlot=Deck.NextOccupied(0);recovery=.15f;
                string directory=Path.Combine(root,ultimate);Directory.CreateDirectory(directory);
                var cues=new List<string>{"frame,action,kills,impacts,ultimate,health"};
                bool requested=false;int initialCasts=UltimateCasts;
                var endFrame=new WaitForEndOfFrame();
                for(int f=0;f<360;f++)
                {
                    if(!requested&&((f>25&&ActiveSlot==0&&actionDelivered)||f==90)){RequestUltimate();requested=true;}
                    AdvanceCombat(1f/60);
                    if(State!=RunState.Running||Health<=0)throw new InvalidOperationException("Finisher ended early: "+ultimate);
                    cues.Add(f+","+ActionStatus.Replace(',',' ')+","+Kills+","+CollisionHits+","+UltimateCasts+","+Health);
                    yield return endFrame;
                    var frame=ScreenCapture.CaptureScreenshotAsTexture();
                    if(frame.width!=1920||frame.height!=1080)throw new InvalidOperationException("Unexpected finisher size");
                    File.WriteAllBytes(Path.Combine(directory,f.ToString("00000")+".png"),frame.EncodeToPNG());Destroy(frame);
                    yield return null;
                }
                if(UltimateCasts<=initialCasts||Reloads!=0||Deck.Items.Any(i=>i.augment!=null))throw new InvalidOperationException("Invalid finisher mechanics");
                File.WriteAllLines(Path.Combine(directory,"cues.csv"),cues);
                Debug.Log("FINISH_CAPTURE_TAKE: "+ultimate);
            }
            Debug.Log("FINISH_CAPTURE_SUCCESS");Application.Quit(0);
        }
    }
}
