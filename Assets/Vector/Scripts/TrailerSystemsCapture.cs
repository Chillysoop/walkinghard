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
        // Opt-in footage: native combat, weapon choice, ability order and a queued ultimate.
        // This take contains no augments or guns; its final soundtrack is music only.
        IEnumerator CaptureSystemsTrailer()
        {
            enabled=false;QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
            Application.logMessageReceived+=(message,trace,type)=>{if(type==LogType.Exception){Debug.LogError("SYSTEMS_TRAILER_FAILED: "+message);Application.Quit(3);}};
            Screen.SetResolution(1920,1080,false);
            for(int i=0;i<12;i++)yield return null;
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","SystemsTrailerCapture"));Directory.CreateDirectory(root);
            Begin(true);qaBuild="ballet";varietyFixture=false;ConfigureVarietyFixture();
            foreach(var item in Deck.Items.ToArray())if(item.augment!=null)Deck.Move(item.augment,PlaceKind.Inventory);
            Deck.Items.RemoveAll(item=>item.def.kind==ItemKind.Augment);
            Deck.Move(Deck.Acquire(LoadoutCatalog.Find("breachhammer"),3),PlaceKind.Weapon,1);
            Deck.Move(Deck.Acquire(LoadoutCatalog.Find("MassDriver"),3),PlaceKind.Sequence,4);
            var gravity=Deck.Slots.First(i=>i?.def.id=="GravityWell");
            var mass=Deck.Slots.First(i=>i?.def.id=="MassDriver");
            var reaping=Deck.Slots.First(i=>i?.def.id=="ReapingCut");
            var order=new[]{mass,gravity,Deck.Basics[0],Deck.Basics[1],reaping};
            for(int i=0;i<order.Length;i++)Deck.Move(order[i],PlaceKind.Sequence,i);
            Deck.Open(DraftKind.Ultimate,false);OpeningPicks=2;Wave=6;Health=MaxHealth;State=RunState.Running;
            ActiveSlot=PairSlot=-1;Preparation=0;sequencePointer=0;expectedSlot=Deck.NextOccupied(0);recovery=.15f;
            UltimateAutomatic=false;UltimateRemaining=0;SetCombatSpeed(1);HoldCheckpointAfterPick=true;
            hud.TrailerCaptureMode();hud.TrailerClearSelection();fx.muted=true;
            hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);
            for(int i=0;i<840;i++)AdvanceCombat(1f/60);
            var cursor=gameObject.AddComponent<TrailerCapturePointer>();
            Vector2 from=Vector2.zero,to=Vector2.zero;
            var cues=new List<string>{"frame,action,active,preparation,kills,impacts,reloads,ultimates,health"};
            var endFrame=new WaitForEndOfFrame();
            for(int f=0;f<1200;f++)
            {
                if(f==240)
                {
                    State=RunState.Drafting;pendingDrafts.Clear();Deck.Open(DraftKind.Weapon,false);
                    var ids=new[]{"sword","greatsword","hammer"};
                    for(int i=0;i<3;i++)Deck.Offers[i]=new RewardOffer(LoadoutCatalog.Find(ids[i]),1,3);
                    hud.TrailerClearSelection();hud.TrailerPoint(new Vector2(640,120));cursor.visible=true;
                }
                if(f>=240&&f<420)
                {
                    int local=f-240;
                    if(local==25)from=hud.TrailerOfferPoint(1);
                    if(local>=25&&local<40)hud.TrailerPoint(Vector2.Lerp(new Vector2(640,120),from,(local-25)/15f));
                    if(local==40)hud.TrailerPoint(from,EventType.MouseDown);
                    if(local==43)to=hud.TrailerDestinationPoint(PlaceKind.Weapon,0);
                    if(local>=44&&local<104)hud.TrailerPoint(Vector2.Lerp(from,to,Mathf.SmoothStep(0,1,(local-44)/59f)),EventType.MouseDrag);
                    if(local==104)hud.TrailerPoint(to,EventType.MouseUp);
                    if(local>=108)hud.TrailerPoint(new Vector2(1080,641));
                }
                if(f==420)
                {
                    if(!Deck.RewardClaimed||Deck.Weapons[0]?.def.id!="greatsword"||!Deck.TwoHanded)throw new InvalidOperationException("Native weapon drop did not equip the two-handed greatsword");
                    hud.TrailerClearSelection();hud.TrailerManageBuild();hud.TrailerPoint(new Vector2(640,120));
                }
                if(f>=420&&f<600)
                {
                    int local=f-420;
                    if(local==25){from=hud.TrailerDestinationPoint(PlaceKind.Sequence,1);to=hud.TrailerDestinationPoint(PlaceKind.Sequence,0);}
                    if(local>=25&&local<40)hud.TrailerPoint(Vector2.Lerp(new Vector2(640,120),from,(local-25)/15f));
                    if(local==40)hud.TrailerPoint(from,EventType.MouseDown);
                    if(local>=43&&local<104)hud.TrailerPoint(Vector2.Lerp(from,to,Mathf.SmoothStep(0,1,(local-43)/60f)),EventType.MouseDrag);
                    if(local==104)hud.TrailerPoint(to,EventType.MouseUp);
                    if(local>=108)hud.TrailerPoint(new Vector2(640,120));
                }
                if(f==600)
                {
                    if(Deck.Slots[0]!=gravity||Deck.Slots[1]!=mass)throw new InvalidOperationException("Native ability reorder did not put gather before launch");
                    if(Deck.Items.Any(i=>i.augment!=null))throw new InvalidOperationException("Unexpected augment in music-only combat take");
                    hud.TrailerClearSelection();cursor.visible=false;State=RunState.Running;
                    ActiveSlot=PairSlot=-1;Preparation=0;sequencePointer=0;expectedSlot=Deck.NextOccupied(0);recovery=.15f;
                    hero.EquipWeapons(Deck.WeaponFor(Deck.Basics[0]).def.weapon,Deck.WeaponFor(Deck.Basics[1]).def.weapon,Deck.TwoHanded);
                }
                if(f==935)RequestUltimate();
                if(State==RunState.Running){hud.TrailerPoint(new Vector2(-200,-200));AdvanceCombat(1f/60);}
                if(State==RunState.Defeated||Health<=0)throw new InvalidOperationException("Systems take defeated");
                cursor.point=hud.TrailerPointer;
                cues.Add(f+","+ActionStatus.Replace(',',' ')+","+ActiveSlot+","+Preparation.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+Kills+","+CollisionHits+","+Reloads+","+UltimateCasts+","+Health);
                yield return endFrame;
                var frame=ScreenCapture.CaptureScreenshotAsTexture();
                if(frame.width!=1920||frame.height!=1080)throw new InvalidOperationException("Unexpected systems frame size");
                File.WriteAllBytes(Path.Combine(root,f.ToString("00000")+".png"),frame.EncodeToPNG());Destroy(frame);
                if(f%300==0)Debug.Log("SYSTEMS_TRAILER_FRAME: "+f);
                yield return null;
            }
            if(UltimateCasts==0)throw new InvalidOperationException("Razor Cyclone did not execute");
            if(Reloads!=0)throw new InvalidOperationException("Unexpected reload in melee-only take");
            File.WriteAllLines(Path.Combine(root,"cues.csv"),cues);
            File.WriteAllText(Path.Combine(root,"take.txt"),"Full native 1920x1080 UI at 60fps, normal health and 1x combat. Staged wave-six build with native mouse handlers: select two-handed Greatsword; reorder Gravity Well ahead of Mass Driver; follow with blade attacks and a queued Razor Cyclone. No augments or guns. Native sound muted; final edit uses original music only.\n"+Deck.Loadout);
            Debug.Log("SYSTEMS_TRAILER_CAPTURE_SUCCESS");Application.Quit(0);
        }
    }
    public sealed partial class PrototypeHUD
    {
        internal void TrailerManageBuild()=>SetManageBuild(true);
    }
}
