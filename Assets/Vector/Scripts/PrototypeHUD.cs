using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD : MonoBehaviour
    {
        public VectorEncounter game;
        readonly Dictionary<string,GUIStyle> styles=new Dictionary<string,GUIStyle>();
        readonly Color ink=new Color(.035f,.065f,.075f,.96f),line=new Color(.25f,.4f,.42f,.6f),white=new Color(.93f,.98f,.97f),muted=new Color(.68f,.79f,.80f);
        static readonly Color Accent=new Color(.45f,1,.79f),Gold=new Color(1,.77f,.37f);
        GUIStyle Style(int size,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
        {
            string key=size+"/"+bold+"/"+anchor;if(styles.TryGetValue(key,out var s))return s;
            s=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,alignment=anchor,wordWrap=true,padding=new RectOffset(0,0,0,0)};styles[key]=s;return s;
        }
        void Label(Rect r,string text,int size,Color color,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
        {var s=Style(size,bold,anchor);s.normal.textColor=color;GUI.Label(r,text,s);}
        static void Box(Rect r,Color color){var previous=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=previous;}
        void Panel(Rect r){Box(r,ink);Box(new Rect(r.x,r.y,r.width,1),line);}
        bool Button(Rect r,string text,bool primary=false)
        {
            bool hover=r.Contains(Event.current.mousePosition);
            Box(r,!GUI.enabled?new Color(.08f,.13f,.15f):primary?(hover?new Color(.65f,1,.86f):Accent):(hover?new Color(.17f,.29f,.29f):new Color(.09f,.16f,.18f)));
            Label(r,text,14,!GUI.enabled?muted:primary?ink:white,true,TextAnchor.MiddleCenter);
            return GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        void OnGUI()
        {
            if(game==null)return;float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f),w=Screen.width/scale,h=Screen.height/scale;
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(scale,scale,1));
            if(game.State==VectorEncounter.RunState.Ready){Intro(w,h);return;}
            if(game.State==VectorEncounter.RunState.Drafting){Draft(w,h);return;}
            if(game.State==VectorEncounter.RunState.Paused){Pause(w,h);return;}
            if(game.State==VectorEncounter.RunState.Complete||game.State==VectorEncounter.RunState.Defeated){Summary(w,h);return;}
            Panel(new Rect(25,24,270,118));Label(new Rect(43,36,235,26),"WALKING HARD",24,white,true);
            Label(new Rect(43,76,225,20),"INTEGRITY   "+Mathf.CeilToInt(game.Health)+" / "+(int)game.tuning.heroHealth,14,muted);
            Box(new Rect(43,111,234,5),line);Box(new Rect(43,111,234*Mathf.Clamp01(game.Health/game.tuning.heroHealth),5),game.Health<game.tuning.heroHealth*.3f?Gold:Accent);
            Panel(new Rect(w-277,24,252,118));Label(new Rect(w-257,37,160,20),game.CleaningUp?"CLEANUP":"WAVE "+game.Wave+" / 3",15,Accent,true);
            Label(new Rect(w-105,36,60,40),Mathf.CeilToInt(game.WaveRemaining).ToString("00")+"s",29,white,true,TextAnchor.UpperRight);
            Label(new Rect(w-257,86,220,40),game.Kills+" takedowns  /  "+game.EnemyCount+" incoming\n"+game.CollisionHits+" body impacts",14,muted);
            if(game.EventAge<1.4f)Label(new Rect(w/2-330,h-200,660,36),game.EventText,21,game.EventText.Contains("COLLISION")?Gold:Accent,true,TextAnchor.MiddleCenter);
            CombatBar(w,h);
            if(game.DamageFlash>0&&!game.ReducedMotion){var c=new Color(1,.23f,.1f,game.DamageFlash*.3f);Box(new Rect(0,0,w,5),c);Box(new Rect(0,h-5,w,5),c);}
        }
        void Intro(float w,float h)
        {
            Panel(new Rect(28,115,470,485));
            Label(new Rect(54,142,410,24),"WALKING HARD  /  PROTOTYPE 0.3",15,Accent,true);
            Label(new Rect(52,189,420,125),"BUILD THE ORDER.\nBREAK THE CROWD.",40,white,true);
            Label(new Rect(55,333,400,90),"Draft two abilities. Choose their slots.\nYour fighter repeats them in that order.\nFill the third slot at the first checkpoint.",18,muted);
            Label(new Rect(55,438,400,48),"Enemies enter through side doors and the horizon. Bodies collide, tumble, and fly.",16,white);
            if(Button(new Rect(55,521,410,49),"DRAFT YOUR LOADOUT  [ ENTER ]",true))game.Begin();
            Label(new Rect(29,h-50,850,25),"THREE WAVES  /  CHOOSE ONE OF THREE  /  A NEW DRAFT EACH RUN",14,muted);
        }
        void CombatBar(float w,float h)
        {
            float y=h-153;
            Panel(new Rect(25,y,260,125));
            if(Button(new Rect(40,y+12,230,34),"AUTO COMBAT: "+(game.Automatic?"ON":"OFF")+"  [ A ]",game.Automatic))game.ToggleAutomatic();
            GUI.enabled=game.ActiveSlot<0;
            if(Button(new Rect(40,y+53,230,29),"ADVANCE NEXT SLOT  [ F ]"))game.Advance();
            GUI.enabled=true;
            if(Button(new Rect(40,y+89,230,25),"PAUSE  [ SPACE ]"))game.TogglePause();
            float width=(w-345)/3;
            for(int i=0;i<3;i++)
            {
                float x=305+i*(width+10);var card=game.Deck.Slots[i];bool active=i==game.ActiveSlot,next=game.ActiveSlot<0&&i==game.NextSlot;
                Panel(new Rect(x,y,width,125));Box(new Rect(x,y,width,3),active?Accent:next?Gold:line);
                Label(new Rect(x+16,y+15,width-32,22),"SLOT "+(i+1)+(active?" / EXECUTING":next?" / NEXT":""),13,active?Accent:next?Gold:muted,true);
                Label(new Rect(x+16,y+47,width-32,32),card?.name??"Empty",23,white,true);
                Label(new Rect(x+16,y+85,width-32,27),card==null?"Fill at checkpoint one":game.SlotCasts[i]+" activations",14,muted);
                if(active)Box(new Rect(x+16,y+116,(width-32)*game.ActionProgress,3),Accent);
            }
            Label(new Rect(25,h-23,w-50,20),"1 > 2 > 3 > REPEAT     ·     EMPTY SLOTS ARE SKIPPED     ·     R new run     T repeat seed "+game.Seed,12,muted,false,TextAnchor.MiddleCenter);
        }
        void Draft(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.01f,.025f,.03f,.88f));float x=40,y=(h-720)/2+20,width=w-80;
            Panel(new Rect(x,y,width,680));
            string phase=game.OpeningDraft?"OPENING DRAFT "+(game.OpeningPicks+1)+" / 2":"CHECKPOINT "+game.Wave+" / 2";
            Label(new Rect(x+24,y+19,700,22),phase+"  /  COMBAT PAUSED",14,Accent,true);
            Label(new Rect(x+width-274,y+19,250,22),"INTEGRITY  "+Mathf.CeilToInt(game.Health)+" / "+(int)game.tuning.heroHealth,14,white,true,TextAnchor.UpperRight);
            Label(new Rect(x+24,y+55,width-48,48),game.Deck.IsAbilityDraft?"CHOOSE THE NEXT MOVE.":"STRENGTHEN YOUR SEQUENCE.",35,white,true);
            Label(new Rect(x+24,y+111,width-48,42),game.OpeningDraft?"Choose one of three abilities, then choose an empty slot. Slot order is attack order.":game.NextThreat,17,muted);
            float gap=16,cw=(width-48-gap*2)/3,cy=y+167;
            HandleDraftDrag(x+24,cy,y+443,cw,gap);
            for(int i=0;i<3;i++)
            {
                float cx=x+24+i*(cw+gap);bool selected=game.Deck.Selected==i;var rect=new Rect(cx,cy,cw,220);
                Box(rect,selected?new Color(.10f,.24f,.23f):new Color(.055f,.105f,.12f));Box(new Rect(cx,cy,cw,3),selected?Accent:line);
                if(GUI.Button(rect,GUIContent.none,GUIStyle.none))game.SelectOffer(i);
                Label(new Rect(cx+17,cy+14,cw-34,21),"PICK "+(i+1),13,muted,true);
                string name,description,interaction;
                if(game.Deck.IsAbilityDraft){var a=game.Deck.Offers[i];name=a.name;description=a.description;interaction=a.interaction;}
                else{var m=game.Deck.ModifierOffers[i];name=m.name;description=m.description;interaction="Applied to your current loadout.";}
                Label(new Rect(cx+17,cy+45,cw-34,36),name,25,white,true);
                Label(new Rect(cx+17,cy+94,cw-34,62),description,16,white);
                if(game.Deck.IsAbilityDraft)DrawSynergies(new Rect(cx+17,cy+165,cw-34,46),game.Deck.Offers[i].id);
                else Label(new Rect(cx+17,cy+168,cw-34,44),interaction,14,muted);
            }
            Label(new Rect(x+24,y+408,width-48,22),"CHARACTER SHEET  /  YOUR ATTACK SEQUENCE",14,Accent,true);
            float sy=y+443;string[] keys={"Q","W","E"};
            for(int i=0;i<3;i++)
            {
                float cx=x+24+i*(cw+gap);var slot=game.Deck.Slots[i];bool chosen=game.Deck.SelectedSlot==i;
                Box(new Rect(cx,sy,cw,76),chosen?new Color(.12f,.28f,.24f):new Color(.065f,.12f,.14f));
                if(game.Deck.IsAbilityDraft&&slot==null&&GUI.Button(new Rect(cx,sy,cw,76),GUIContent.none,GUIStyle.none))game.AssignSlot(i);
                Label(new Rect(cx+16,sy+12,cw-32,21),"SLOT "+(i+1)+"  [ "+keys[i]+" ]",13,chosen?Accent:muted,true);
                Label(new Rect(cx+16,sy+40,cw-32,28),slot?.name??(chosen?"Assign selected ability here":"Choose this empty slot"),17,white,true);
            }
            Label(new Rect(x+24,y+524,width-48,22),"DRAG ABILITIES BETWEEN SLOTS TO REORDER  /  DRAG A DRAFT CARD TO AN EMPTY SLOT",12,muted);
            GUI.enabled=game.Deck.Rerolls>0;
            if(Button(new Rect(x+24,y+552,250,44),"REROLL [ TAB ]  /  "+game.Deck.Rerolls+" BANKED"))game.Reroll();GUI.enabled=true;
            Label(new Rect(x+292,y+556,width-680,46),"Unused rerolls carry forward.\nCards: 1 / 2 / 3   ·   Slots: Q / W / E",14,muted);
            GUI.enabled=game.Deck.CanInstall;
            if(Button(new Rect(x+width-354,y+552,330,44),game.OpeningDraft&&game.OpeningPicks==0?"INSTALL & DRAFT AGAIN [ ENTER ]":"INSTALL & CONTINUE [ ENTER ]",true))game.ConfirmDraft();
            GUI.enabled=true;
            Label(new Rect(x+24,y+621,width-48,38),game.Deck.IsAbilityDraft?"Pick a card and a slot. Nothing starts until you install.":"Your abilities keep their order. This upgrade changes how they perform.",15,Accent);
            DrawDragPreview();
        }
        void Pause(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.01f,.03f,.04f,.78f));float x=w/2-270,y=h/2-200;Panel(new Rect(x,y,540,400));
            Label(new Rect(x+30,y+26,480,46),"TAKE A BREATH.",34,white,true);
            Label(new Rect(x+30,y+86,480,50),game.Deck.Loadout,16,muted);
            if(Button(new Rect(x+30,y+150,480,45),"RESUME [ SPACE ]",true))game.TogglePause();
            if(Button(new Rect(x+30,y+210,235,36),game.Muted?"SOUND: OFF":"SOUND: ON"))game.ToggleSound();
            if(Button(new Rect(x+275,y+210,235,36),game.ReducedMotion?"REDUCED MOTION: ON":"REDUCED MOTION: OFF"))game.ToggleMotion();
            if(Button(new Rect(x+30,y+267,235,38),"NEW RUN [ R ]"))game.Begin();
            if(Button(new Rect(x+275,y+267,235,38),"REPEAT SEED [ T ]"))game.Begin(true);
            if(Button(new Rect(x+30,y+330,480,38),"QUIT"))game.Quit();
        }
        void Summary(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.015f,.035f,.045f,.87f));float x=w/2-480,y=h/2-305;Panel(new Rect(x,y,960,610));
            bool win=game.State==VectorEncounter.RunState.Complete;
            Label(new Rect(x+32,y+24,896,22),win?"THREE WAVES SURVIVED":"OVERRUN IN WAVE "+game.Wave,15,Accent,true);
            Label(new Rect(x+32,y+66,896,60),"YOUR ORDER. YOUR FIGHT.",40,white,true);
            Label(new Rect(x+32,y+137,896,40),game.Deck.Loadout,17,muted);
            string[] values={game.AbilityCasts.ToString(),game.Kills.ToString(),game.Launches.ToString(),game.CollisionHits.ToString()};
            string[] labels={"ABILITY ACTIVATIONS","TAKEDOWNS","BODIES LAUNCHED","COLLISION HITS"};
            for(int i=0;i<4;i++){Label(new Rect(x+32+i*226,y+194,212,42),values[i],33,white,true);Label(new Rect(x+32+i*226,y+241,212,20),labels[i],13,muted);}
            float[] columns={32,190,370,548,728};string[] headers={"WAVE","TAKEDOWNS","LAUNCHES","COLLISIONS","INTEGRITY"};
            for(int j=0;j<5;j++)Label(new Rect(x+columns[j],y+292,150,24),headers[j],13,muted,true);
            for(int i=0;i<game.Waves.Count;i++)
            {
                var row=game.Waves[i];string[] cells={row.wave.ToString("00"),row.kills.ToString(),row.launches.ToString(),row.collisions.ToString(),Mathf.CeilToInt(row.health).ToString()};
                for(int j=0;j<5;j++)Label(new Rect(x+columns[j],y+327+i*35,150,28),cells[j],18,j==3&&row.collisions>0?Gold:white,true);
            }
            Label(new Rect(x+32,y+449,896,42),"Try another order or a different draft. Seed "+game.Seed+"  /  "+game.Ragdolls+" ragdolls.",16,muted);
            if(Button(new Rect(x+32,y+534,430,46),"NEW DRAFT [ R ]",true))game.Begin();
            if(Button(new Rect(x+478,y+534,290,46),"REPEAT SEED [ T ]"))game.Begin(true);
            if(Button(new Rect(x+784,y+534,144,46),"QUIT"))game.Quit();
        }
    }
}
