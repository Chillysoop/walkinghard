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
            Label(new Rect(54,142,410,24),"WALKING HARD  /  PROTOTYPE 0.4",15,Accent,true);
            Label(new Rect(52,189,420,125),"BUILD THE ORDER.\nBREAK THE CROWD.",40,white,true);
            Label(new Rect(55,333,400,90),"Two weapon attacks. Three chosen abilities.\nArrange all five; combat runs automatically.\nYour ultimate has its own 12-second timer.",18,muted);
            Label(new Rect(55,438,400,48),"Find weapons at checkpoints. Pair matching attacks, split bodies, and build collision cascades.",16,white);
            if(Button(new Rect(55,521,410,49),"DRAFT YOUR LOADOUT  [ ENTER ]",true))game.Begin();
            Label(new Rect(29,h-50,850,25),"THREE WAVES  /  CHOOSE ONE OF THREE  /  A NEW DRAFT EACH RUN",14,muted);
        }
        void CombatBar(float w,float h)
        {
            float y=h-137,width=(w-90)/5;
            for(int i=0;i<5;i++)
            {
                float x=25+i*(width+10);var item=game.Deck.Slots[i];bool active=i==game.ActiveSlot||i==game.PairSlot,next=game.ActiveSlot<0&&i==game.NextSlot;
                Panel(new Rect(x,y,width,108));Box(new Rect(x,y,width,3),active?Accent:next?Gold:line);
                Label(new Rect(x+12,y+11,width-24,20),"SLOT "+(i+1)+(active?" / EXECUTING":next?" / NEXT":""),12,active?Accent:next?Gold:muted,true);
                Label(new Rect(x+12,y+36,width-24,42),item==null?"Empty":item.def.kind==ItemKind.Basic?item.def.name+" / "+game.Deck.WeaponFor(item).def.name:item.def.name,17,white,true);
                Label(new Rect(x+12,y+81,width-24,18),item==null?"Empty / skipped":"Kills: "+game.ActionKills(item)+(game.Deck.PairAfter(i)>=0?(game.Deck.TwoHanded?" / LOCKED 2H":" / PAIRED"):""),12,game.Deck.PairAfter(i)>=0?Gold:muted);

                if(active)Box(new Rect(x+12,y+102,(width-24)*game.ActionProgress,3),Accent);
            }
            GroupOutline(25,y,width,10,108);
            Label(new Rect(25,h-24,w-50,20),"SEQUENCE ALWAYS AUTOMATIC  /  EMPTY SLOTS SKIP  /  SPACE pause  /  R new run  /  T repeat seed "+game.Seed,12,muted,false,TextAnchor.MiddleCenter);
            Panel(new Rect(w/2-215,24,430,118));TypeIcon(new Rect(w/2+184,33,20,20),ItemKind.Ultimate,Violet);
            string status=game.Deck.Ultimate==null?"UNEQUIPPED":game.ActiveSlot==5?"EXECUTING":game.UltimateRemaining>0?game.UltimateRemaining.ToString("0.0")+"s":game.UltimateQueued?"QUEUED · NEXT ACTION":"READY";
            Label(new Rect(w/2-199,35,398,21),"ULTIMATE / "+(game.Deck.Ultimate?.def.name.ToUpperInvariant()??"EMPTY"),14,Violet,true);
            Label(new Rect(w/2-199,61,398,23),status+" / Kills: "+game.KillsFor(game.Deck.Ultimate),19,Violet,true);
            if(Button(new Rect(w/2-199,99,192,29),"ULT: "+(game.UltimateAutomatic?"AUTO":"MANUAL")+" [ U ]"))game.ToggleUltimate();
            GUI.enabled=game.Deck.Ultimate!=null&&game.UltimateRemaining<=0&&game.ActiveSlot!=5;
            if(Button(new Rect(w/2+4,99,195,29),"QUEUE ULTIMATE [ F ]",true))game.RequestUltimate();GUI.enabled=true;
            if(game.Preparation>0||game.ActionStatus.StartsWith("WAITING"))Label(new Rect(w/2-300,y-32,600,26),game.ActionStatus,15,Gold,true,TextAnchor.MiddleCenter);
            Panel(new Rect(25,194,270,112));
            Label(new Rect(43,206,234,20),"SCORE  "+game.Score.Banked.ToString("N0"),17,white,true);
            Label(new Rect(43,235,234,29),game.Score.Kills>0?game.Score.Base.ToString("N0")+" × "+game.Score.Multiplier+" = "+game.Score.Pending.ToString("N0"):"BEST COMBO "+game.Score.Best.ToString("N0"),game.Score.Kills>0?17:15,Gold,true);
            Label(new Rect(43,269,234,20),game.Score.Kills>0?game.Score.Reason+" / "+game.Score.Kills+" KILLS":"MULTI-KILLS + CHAIN KILLS",11,muted);
            Box(new Rect(43,295,234*game.Score.Remaining/ComboScore.Window,3),Gold);
            if(Button(new Rect(43,149,234,28),"PAUSE [ SPACE ]"))game.TogglePause();
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
            string[] values={game.Score.Banked.ToString("N0"),game.Kills.ToString(),game.Score.Best.ToString("N0"),game.CollisionHits.ToString()};
            string[] labels={"SCORE","KILLS","BEST COMBO","COLLISION HITS"};
            for(int i=0;i<4;i++){Label(new Rect(x+32+i*226,y+194,212,42),values[i],33,white,true);Label(new Rect(x+32+i*226,y+241,212,20),labels[i],13,muted);}
            float[] columns={32,190,370,548,728};string[] headers={"WAVE","TAKEDOWNS","LAUNCHES","COLLISIONS","INTEGRITY"};
            for(int j=0;j<5;j++)Label(new Rect(x+columns[j],y+292,150,24),headers[j],13,muted,true);
            for(int i=0;i<game.Waves.Count;i++)
            {
                var row=game.Waves[i];string[] cells={row.wave.ToString("00"),row.kills.ToString(),row.launches.ToString(),row.collisions.ToString(),Mathf.CeilToInt(row.health).ToString()};
                for(int j=0;j<5;j++)Label(new Rect(x+columns[j],y+327+i*35,150,28),cells[j],18,j==3&&row.collisions>0?Gold:white,true);
            }
            Label(new Rect(x+32,y+449,896,42),"Try another order or a different draft. Seed "+game.Seed+"  /  "+game.Ragdolls+" ragdolls / "+game.Severs+" severs / "+game.UltimateCasts+" ultimates.",16,muted);
            if(Button(new Rect(x+32,y+534,430,46),"NEW DRAFT [ R ]",true))game.Begin();
            if(Button(new Rect(x+478,y+534,290,46),"REPEAT SEED [ T ]"))game.Begin(true);
            if(Button(new Rect(x+784,y+534,144,46),"QUIT"))game.Quit();
        }
    }
}
