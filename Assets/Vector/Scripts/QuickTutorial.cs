using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public enum Lesson { Ability, Weapon, Fusion, Augment, Order, Practice, Finished }
        public bool TutorialActive { get; private set; }
        public Lesson TutorialLesson { get; private set; }
        public string TutorialHint { get; private set; }="";
        bool tutorialSavedHold;
        float tutorialSavedSpeed,tutorialPracticeTime;
        ItemDefinition tutorialWeapon;
        int tutorialFusionBefore;
        public LoadoutItem TutorialArc=>Deck.Items.FirstOrDefault(i=>i.def.id=="ArcBolt");
        public bool TutorialReady
        {
            get
            {
                switch(TutorialLesson)
                {
                    case Lesson.Ability:return Deck.RewardClaimed&&Deck.Count==3;
                    case Lesson.Weapon:return Deck.RewardClaimed&&Deck.Weapons.Any(i=>i!=null);
                    case Lesson.Fusion:return Deck.Fusions>tutorialFusionBefore&&Deck.Weapons.Any(i=>i?.def==tutorialWeapon&&i.rank>=2);
                    case Lesson.Augment:return TutorialArc?.augment?.def.module.id==AugmentId.Echo;
                    case Lesson.Order:return Deck.Slots[0]==TutorialArc;
                    case Lesson.Practice:return tutorialPracticeTime>=8&&AbilityCasts>=3;
                    default:return true;
                }
            }
        }
        public void BeginTutorial()
        {
            Begin();tutorialSavedHold=HoldCheckpointAfterPick;tutorialSavedSpeed=CombatSpeed;
            TutorialActive=true;TutorialLesson=Lesson.Ability;HoldCheckpointAfterPick=true;SetCombatSpeed(1);
            OpeningPicks=2;Wave=1;TutorialHint="";
            Deck.Move(Deck.Acquire(LoadoutCatalog.Find("ArcBolt")),PlaceKind.Sequence,3);
            Deck.Move(Deck.Acquire(LoadoutCatalog.Find("ForcePush")),PlaceKind.Sequence,4);
            TutorialOffers(DraftKind.Ability,"GravityWell","MassDriver","Breaker");hud?.ResetTutorialUI();
        }
        void EndTutorial()
        {
            if(!TutorialActive)return;
            TutorialActive=false;HoldCheckpointAfterPick=tutorialSavedHold;SetCombatSpeed(tutorialSavedSpeed);TutorialHint="";
            hud?.ResetTutorialUI();
        }
        public void MainMenu()
        {Begin();State=RunState.Ready;hud?.ResetTutorialUI();}
        void TutorialOffers(DraftKind kind,params string[] ids)
        {
            Deck.Open(kind,false);
            for(int i=0;i<3;i++)Deck.Offers[i]=new RewardOffer(LoadoutCatalog.Find(ids[i]));
        }
        public void TutorialNext()
        {
            if(!TutorialActive||!TutorialReady)return;
            TutorialHint="";hud?.ResetTutorialUI();
            if(TutorialLesson==Lesson.Finished){Begin();return;}
            TutorialLesson++;
            switch(TutorialLesson)
            {
                case Lesson.Weapon:TutorialOffers(DraftKind.Weapon,"sword","pistol","hammer");break;
                case Lesson.Fusion:
                    tutorialWeapon=Deck.Weapons.First(i=>i!=null).def;tutorialFusionBefore=Deck.Fusions;
                    Deck.Acquire(tutorialWeapon,1);TutorialOffers(DraftKind.Mixed,tutorialWeapon.id,"visor","Counterguard");break;
                case Lesson.Augment:TutorialOffers(DraftKind.Augment,"aug-Echo","aug-Focused","aug-Afterfield");break;
                case Lesson.Order:break;
                case Lesson.Practice:
                    tutorialPracticeTime=0;BeginWave();break;
                case Lesson.Finished:State=RunState.Drafting;break;
            }
        }
        bool TutorialClaimAllowed(int offer)
        {
            if(!TutorialActive)return true;
            bool allowed=TutorialOfferAllowed(offer);
            if(!allowed)TutorialHint=TutorialLesson==Lesson.Fusion?"For this lesson, take the matching Common weapon on the left.":TutorialLesson==Lesson.Augment?"Use Echo on the left, then attach it to Arc Bolt.":"Finish the highlighted task, then choose Next.";
            return allowed;
        }
        public bool TutorialOfferAllowed(int offer)=>!TutorialActive||TutorialLesson<=Lesson.Weapon||((TutorialLesson==Lesson.Fusion||TutorialLesson==Lesson.Augment)&&offer==0);
        public bool TutorialAttachAllowed(LoadoutItem host)
        {return !TutorialActive||(TutorialLesson==Lesson.Augment&&host==TutorialArc);}
        public bool TutorialMoveAllowed(LoadoutItem item,PlaceKind place,int index)
        {
            if(!TutorialActive)return true;if(item==null)return false;
            switch(TutorialLesson)
            {
                case Lesson.Ability:return item.def.kind==ItemKind.Ability&&place==PlaceKind.Sequence;
                case Lesson.Weapon:case Lesson.Fusion:return item.def.kind==ItemKind.Weapon&&(place==PlaceKind.Weapon||place==PlaceKind.Inventory);
                case Lesson.Augment:return item.def.kind==ItemKind.Augment&&(place==PlaceKind.Inventory||Deck.HostAt(place,index)==TutorialArc);
                case Lesson.Order:return place==PlaceKind.Sequence&&(item.def.kind==ItemKind.Basic||item.def.kind==ItemKind.Ability);
                default:return false;
            }
        }
        void StepTutorial(float delta)
        {
            if(Input.GetKeyDown(KeyCode.Escape)){MainMenu();return;}
            if(hud?.HandleOverlayInput()==true)return;
            if(Input.GetKeyDown(KeyCode.Return)&&TutorialReady){TutorialNext();return;}
            if(TutorialLesson!=Lesson.Practice)return;
            if(Input.GetKeyDown(KeyCode.Space))TogglePause();
            AdvanceTutorialPractice(delta);
        }
        internal void AdvanceTutorialPractice(float delta)
        {
            if(TutorialLesson!=Lesson.Practice||State!=RunState.Running)return;
            tutorialPracticeTime+=Mathf.Min(delta,.1f);AdvanceCombat(delta);
            if(tutorialPracticeTime>=16||State==RunState.Defeated){TutorialLesson=Lesson.Finished;State=RunState.Drafting;hud?.ResetTutorialUI();}
        }
    }

    public sealed partial class PrototypeHUD
    {
        internal void ResetTutorialUI()
        {ClearLoadoutGesture();iconGuide=waveDetails=fusionPinned=false;shownFusion=null;seenFusions=game.FusionHistory.Count;fusionDeck=game.Deck;}
        string TutorialTitle=>new[]{"Pick an ability","Equip a weapon","Fuse an upgrade","Attach an augment","Change the order","Try your build","You're ready"}[(int)game.TutorialLesson];
        string TutorialInstruction
        {
            get
            {
                switch(game.TutorialLesson)
                {
                    case VectorEncounter.Lesson.Ability:return "Drag one card into the empty glowing action slot. Double-click also equips.";
                    case VectorEncounter.Lesson.Weapon:return "Drag a weapon into a glowing hand slot, or double-click to equip.";
                    case VectorEncounter.Lesson.Fusion:return "Take the matching Common weapon on the left. Three matching copies fuse automatically.";
                    case VectorEncounter.Lesson.Augment:return "Drag Echo onto Arc Bolt's glowing + socket.";
                    case VectorEncounter.Lesson.Order:return game.Deck.TwoHanded&&game.Deck.Slots[0].def.kind==ItemKind.Basic?"Move the locked weapon pair one slot right, then drag Arc Bolt into slot 1.":"Drag Arc Bolt into slot 1. The sequence repeats from left to right.";
                    default:return "Watch the active tile: your chosen actions execute automatically in order.";
                }
            }
        }
        void TutorialHeader(float w)
        {
            Panel(new Rect(20,18,w-40,58));
            Label(new Rect(34,26,210,20),"QUICK TUTORIAL  /  "+((int)game.TutorialLesson+1)+" OF 6",12,Accent,true);
            Label(new Rect(255,25,w-405,23),TutorialTitle.ToUpperInvariant(),17,white,true);
            Label(new Rect(34,51,w-200,22),TutorialInstruction,12,muted);
            if(Button(new Rect(w-145,30,108,28),"EXIT [ ESC ]")){game.MainMenu();GUIUtility.ExitGUI();}
        }
        void TutorialCoach(Rect r)
        {
            r.height-=70; // Keep the passive rig slot and its drop target unobscured.
            Panel(r);Label(new Rect(r.x+16,r.y+14,r.width-32,24),TutorialTitle,20,Accent,true);
            string text;
            switch(game.TutorialLesson)
            {
                case VectorEncounter.Lesson.Ability:text="Pick your third power.\n\nFive actions repeat in order. BASIC actions use your weapons; empty hands use fists.";break;
                case VectorEncounter.Lesson.Weapon:text="Weapons change BASIC attacks.\n\n1H: one hand.\n2H: both hands; linked actions.\n\nArmor and rigs grant passive effects.";break;
                case VectorEncounter.Lesson.Fusion:text="You own two Common copies. Take the third.\n\n3 identical items + same rarity = next rarity.\n\nOther rarities stay separate.";break;
                case VectorEncounter.Lesson.Augment:text="Echo adds a repeat hit.\n\nDrop it on Arc Bolt or its + socket.\n\nAugments scale with their host. They do not fuse.";break;
                default:text="Set up your combos.\n\nMatching weapon basics fire together when adjacent.\n\n2H basics move as one locked pair.";break;
            }
            Label(new Rect(r.x+16,r.y+48,r.width-32,r.height-91),text,13,white);
            Label(new Rect(r.x+16,r.yMax-32,r.width-32,24),game.TutorialReady?"DONE  /  NEXT LESSON BELOW":"FOLLOW THE GLOWING SLOTS",11,game.TutorialReady?Accent:muted,true);
        }
        void TutorialHighlights()
        {
            if(game.TutorialLesson!=VectorEncounter.Lesson.Order)return;
            bool group=game.Deck.TwoHanded&&game.Deck.Slots[0].def.kind==ItemKind.Basic;
            foreach(var r in dropRegions.Where(r=>r.place==PlaceKind.Sequence&&r.offer<0&&!r.socket))
                if(group?(r.item?.def.kind==ItemKind.Basic||r.index==1):(r.item==game.TutorialArc||r.index==0))DestinationGlow(r.rect,group?Strong:Accent);
        }
        void TutorialCombat(float w,float h)
        {
            if(game.TutorialLesson==VectorEncounter.Lesson.Finished)
            {
                Box(new Rect(0,0,w,h),new Color(.004f,.02f,.03f,.85f));float x=w/2-310,y=h/2-230;Panel(new Rect(x,y,620,460));
                Label(new Rect(x+30,y+25,560,40),"YOU'RE READY.",34,Accent,true);
                Label(new Rect(x+30,y+88,560,180),"Choose one of three rewards.\nEquip weapons and abilities in their matching slots.\nFuse three identical items at the same rarity.\nAttach augments to compatible + sockets.\nDrag the five actions into the order you want.",18,white);
                Label(new Rect(x+30,y+265,560,58),"Your ultimate unlocks after wave 3 and has its own recharge.\nPractice items reset when you start a real run.",14,muted);
                if(Button(new Rect(x+30,y+340,560,46),"START RUN [ ENTER ]",true)){game.Begin();GUIUtility.ExitGUI();}
                if(Button(new Rect(x+30,y+399,560,32),"MAIN MENU")){game.MainMenu();GUIUtility.ExitGUI();}
                return;
            }
            CombatBar(w,h);float px=w/2-295;Panel(new Rect(px,22,590,155));
            Label(new Rect(px+20,35,550,25),"6 / 6  —  WATCH YOUR SEQUENCE",20,Accent,true);
            Label(new Rect(px+20,70,550,44),"Movement and attacks are automatic. Your chosen order repeats.\nSpace pauses. Ultimates arrive after wave 3, on a separate timer.",14,white);
            GUI.enabled=game.TutorialReady;
            if(Button(new Rect(px+20,126,350,32),game.TutorialReady?"FINISH TUTORIAL [ ENTER ]":"WATCH A FEW ACTIONS...",true))game.TutorialNext();GUI.enabled=true;
            if(Button(new Rect(px+385,126,185,32),"EXIT [ ESC ]")){game.MainMenu();GUIUtility.ExitGUI();}
            if(game.State==VectorEncounter.RunState.Paused&&Button(new Rect(px+150,190,290,42),"RESUME [ SPACE ]",true))game.TogglePause();
        }
    }
}
