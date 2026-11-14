using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        public bool CombatPreviewActive {get;private set;}
        public bool CinderworksActive {get;private set;}
        public bool CombatPreviewStyle {get;private set;}=true;
        public int CombatPreviewPreset {get;private set;}
        public static readonly string[] CombatPreviewNames={"FISTS","DUAL BLADES","GREATSWORD","HAMMER","CLOSE QUARTERS","RIFLE","PURSUER"};
        bool startingCombatPreview,savedPreviewStyle;
        float savedPreviewSpeed;
        int savedPreviewSeed;
        public void BeginCinderworksPreview()
        {
            EndCombatPreview();CinderworksActive=true;BeginCombatPreview();
        }
        public void BeginCombatPreview(int preset=0,bool newStyle=true)
        {
            if(!CombatPreviewActive){savedPreviewStyle=VisualFactory.ArmoredStyle;savedPreviewSpeed=CombatSpeed;savedPreviewSeed=Seed;}
            CombatPreviewActive=true;CombatPreviewPreset=Mathf.Clamp(preset,0,CombatPreviewNames.Length-1);CombatPreviewStyle=newStyle;
            VisualFactory.ArmoredStyle=CinderworksActive||newStyle;
            VisualFactory.CinderworksStyle=CinderworksActive&&newStyle;
            regionalWorld.ResetRoads();regionalWorld.SetVisible(VisualFactory.CinderworksStyle);Seed=1741;
            startingCombatPreview=true;Begin(true);startingCombatPreview=false;
            fx.SetPresentation(VisualFactory.ArmoredStyle);fx.SetReducedMotion(ReducedMotion);SetCombatSpeed(1);
            LoadoutItem Equip(string id,PlaceKind place,int index=0){var item=Deck.Acquire(LoadoutCatalog.Find(id),3);MoveItem(item,place,index);return item;}
            string[][] actions={
                new[]{"GravityWell","SteelJab","MassDriver"},new[]{"GravityWell","ReapingCut","ForcePush"},
                new[]{"GravityWell","ReapingCut","MassDriver"},new[]{"ConductiveBrand","Breaker","MassDriver"},
                new[]{"Counterguard","Breaker","ForcePush"},new[]{"VectorTether","ConductiveBrand","SuppressiveVolley"},
                new[]{"GravityWell","ReapingCut","MassDriver"}};
            string[] right={"fists","sword","greatsword","hammer","fists","rifle","greatsword"};
            string[] left={"fists","sword","","","scattergun","",""};
            string[] ultimates={"singularity","cyclone","cyclone","tempest","singularity","redline","singularity"};
            int p=CombatPreviewPreset;
            if(right[p]!="fists")Equip(right[p],PlaceKind.Weapon,0);
            if(left[p]!=""&&left[p]!="fists")Equip(left[p],PlaceKind.Weapon,1);
            for(int i=0;i<3;i++)Equip(actions[p][i],PlaceKind.Sequence,i+2);
            // Gather/control precedes the weapon turn; matching hands remain adjacent.
            MoveItem(Deck.Slots[2],PlaceKind.Sequence,0);
            MoveItem(Deck.Basics[0],PlaceKind.Sequence,1);
            MoveItem(Deck.Basics[1],PlaceKind.Sequence,2);
            Equip(p==3?"relay":"amplifier",PlaceKind.Rig);
            Equip("plate",PlaceKind.Armor);Equip("treads",PlaceKind.Boots);Equip("surveyor",PlaceKind.Helmet);
            Equip(ultimates[p],PlaceKind.Ultimate);
            OpeningPicks=2;Wave=p==6?8:6;Health=MaxHealth;
            SelectedRoute=p==3||p==4?EncounterRoute.VolatileBreach:EncounterRoute.ShieldLine;routedWave=Wave;
            BeginWave();UltimateAutomatic=false;UltimateRemaining=0;
            if(p!=6)
            {
                // A visible practice formation, followed by the ordinary wave's covered entries.
                for(int i=0;i<9;i++)
                {
                    bool heavy=i==5;
                    var e=new Enemy{id=++nextId,heavy=heavy,health=tuning.enemyHealth*(heavy?1.8f:1)*(1+.035f*(Wave-1)),radius=2.7f+(i%3)*.3f,orbit=i%2==0?1:-1,cooldown=.5f,phase=i};
                    e.maxHealth=e.health;e.rig=new FighterRig("Practice formation",actors,false,heavy,i);
                    e.rig.Root.position=new Vector3(-3+(i%3)*3,.04f,7+(i/3)*3);e.physics=new FighterPhysics(e.rig,e.id,this,false,heavy);
                    enemies.Add(e);byId[e.id]=e;
                    if(i==3){e.shieldBearer=true;ShieldBearersSpawned++;e.rig.EncounterEquipment(true,false);}
                    if(i==4){e.volatileCarrier=true;VolatileCarriersSpawned++;e.rig.EncounterEquipment(false,true);}
                    Spawned++;
                }
                Physics.SyncTransforms();
            }
            Announce("PRACTICE / F TO QUEUE ULTIMATE");
        }
        void EndCombatPreview()
        {
            if(!CombatPreviewActive)return;
            CombatPreviewActive=CinderworksActive=false;VisualFactory.CinderworksStyle=false;regionalWorld.SetVisible(false);
            VisualFactory.ArmoredStyle=savedPreviewStyle;fx.SetPresentation(savedPreviewStyle);Seed=savedPreviewSeed;SetCombatSpeed(savedPreviewSpeed);
        }
        public void ToggleCombatPreviewStyle()=>BeginCombatPreview(CombatPreviewPreset,!CombatPreviewStyle);
        void StepCombatPreview(float dt)
        {
            if(hud?.HandleOverlayInput()!=true)
            {
                if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape))TogglePause();
                if(Input.GetKeyDown(KeyCode.F))RequestUltimate();
                if(Input.GetKeyDown(KeyCode.U))ToggleUltimate();
                if(Input.GetKeyDown(KeyCode.M))ToggleSound();
                if(Input.GetKeyDown(KeyCode.R))BeginCombatPreview(CombatPreviewPreset,CombatPreviewStyle);
                if(Input.GetKeyDown(KeyCode.V))ToggleCombatPreviewStyle();
                if(Input.GetKeyDown(KeyCode.LeftArrow))BeginCombatPreview((CombatPreviewPreset+6)%7,CombatPreviewStyle);
                if(Input.GetKeyDown(KeyCode.RightArrow))BeginCombatPreview((CombatPreviewPreset+1)%7,CombatPreviewStyle);
            }
            if(State==RunState.Revealing)StepFinaleReveal(dt);
            else if(State==RunState.Running)AdvanceCombat(dt);
            // This is a single encounter. Normal runs keep their drafts and eight-wave progression.
            if(State==RunState.Drafting)State=RunState.Complete;
        }
    }
    public sealed partial class PrototypeHUD
    {
        void CombatPreviewControls(float w)
        {
            string title=game.CinderworksActive?(game.CombatPreviewStyle?"CINDERWORKS  /  THE SHIFT NEVER ENDS":"CINDERWORKS  /  CURRENT LOOK COMPARISON"):
                "COMBAT PREVIEW  /  "+(game.CombatPreviewStyle?"NEW ARMOR + EFFECTS":"ORIGINAL LOOK");
            if(game.State==VectorEncounter.RunState.Revealing)
            {
                // Keep all preview UI in the letterbox while the camera turns.
                Label(new Rect(20,18,w-610,23),title,13,Accent,true,TextAnchor.MiddleCenter);
                if(Button(new Rect(w-540,14,160,28),"COMPARE LOOK [ V ]"))game.ToggleCombatPreviewStyle();
                if(Button(new Rect(w-366,14,160,28),"EXIT PREVIEW"))game.MainMenu();
                return;
            }
            float x=350,width=w-700;
            Panel(new Rect(x,18,width,120));
            Label(new Rect(x+12,25,width-24,20),title,13,Accent,true,TextAnchor.MiddleCenter);
            float step=(width-24)/7;
            for(int i=0;i<7;i++)if(Button(new Rect(x+12+i*step,52,step-3,27),VectorEncounter.CombatPreviewNames[i],game.CombatPreviewPreset==i))game.BeginCombatPreview(i,game.CombatPreviewStyle);
            if(Button(new Rect(x+12,88,width*.38f,28),"COMPARE LOOK [ V ]"))game.ToggleCombatPreviewStyle();
            if(Button(new Rect(x+width*.4f+6,88,width*.31f,28),"RESTART [ R ]"))game.BeginCombatPreview(game.CombatPreviewPreset,game.CombatPreviewStyle);
            if(Button(new Rect(x+width*.73f,88,width*.27f-12,28),"EXIT PREVIEW"))game.MainMenu();
        }
    }
}
