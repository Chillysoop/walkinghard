using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        public VectorEncounter game;
        readonly Dictionary<string,GUIStyle> styles=new Dictionary<string,GUIStyle>();
        readonly Color ink=new Color(.035f,.065f,.075f,.94f),line=new Color(.25f,.4f,.42f,.6f),white=new Color(.93f,.98f,.97f),muted=new Color(.68f,.79f,.80f);
        static readonly Color Accent=new Color(.45f,1,.79f);
        GUIStyle Style(int size,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
        {
            if(size<12)size=12;
            string key=size+"/"+bold+"/"+anchor;
            if(styles.TryGetValue(key,out var s))return s;
            s=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,alignment=anchor,wordWrap=true,padding=new RectOffset(0,0,0,0)};styles[key]=s;return s;
        }
        void Label(Rect r,string text,int size,Color color,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
        {var s=Style(size,bold,anchor);r.height=Mathf.Max(r.height,s.fontSize+5);s.normal.textColor=color;GUI.Label(r,text,s);}
        static void Box(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
        void Panel(Rect r){Box(r,ink);Box(new Rect(r.x,r.y,r.width,1),line);}
        bool Button(Rect r,string text,bool primary=false)
        {
            bool hover=r.Contains(Event.current.mousePosition);
            Box(r,primary?(hover?new Color(.65f,1,.86f):Accent):(hover?new Color(.17f,.29f,.29f):new Color(.09f,.16f,.18f)));
            Label(r,text,12,primary?ink:white,true,TextAnchor.MiddleCenter);
            return GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        void OnGUI()
        {
            if(game==null)return;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            float w=Screen.width/scale,h=Screen.height/scale;
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(scale,scale,1));
            GUI.depth=0;
            // A quiet frame leaves the world and the hero in charge.
            Panel(new Rect(25,23,244,71));Box(new Rect(25,23,3,71),Accent);
            Label(new Rect(43,33,200,28),"V E C T O R",24,white,true);
            Label(new Rect(44,67,205,16),"COMBAT STUDY  /  01",10,muted);
            if(game.State==VectorEncounter.RunState.Ready){Intro(w,h);return;}
            Panel(new Rect(w-278,23,253,71));
            Label(new Rect(w-259,34,120,16),"TIME REMAINING",10,muted);
            int remain=Mathf.CeilToInt(Mathf.Max(0,game.tuning.duration-game.Elapsed));
            Label(new Rect(w-130,31,84,42),remain.ToString("00")+"s",32,white,true,TextAnchor.MiddleRight);
            Label(new Rect(w-259,59,140,19),game.StageName.Substring(5),11,Accent);
            Panel(new Rect(25,103,244,51));Label(new Rect(43,111,100,15),"INTEGRITY",10,muted);
            Label(new Rect(180,109,70,18),Mathf.CeilToInt(game.Health)+" / "+(int)game.tuning.heroHealth,11,white,false,TextAnchor.UpperRight);
            Box(new Rect(43,135,207,5),new Color(.17f,.24f,.26f));Box(new Rect(43,135,207*Mathf.Clamp01(game.Health/game.tuning.heroHealth),5),game.Health>game.tuning.heroHealth*.35f?Accent:new Color(1,.43f,.25f));
            Panel(new Rect(w-278,103,253,51));
            Label(new Rect(w-259,112,135,14),"HOSTILES IN REACH",10,muted);Label(new Rect(w-88,110,43,25),((int)game.CrowdNear).ToString("00"),21,game.CrowdNear>9?new Color(1,.56f,.31f):white,true,TextAnchor.UpperRight);
            Label(new Rect(w-259,132,190,14),game.Kills+" NEUTRALIZED   /   "+game.EnemyCount+" ACTIVE",9,muted);
            if(game.EventAge<1.1f&&game.IsAttacking||game.IsCasting)
                Label(new Rect(w*.5f-180,h-176,360,30),game.EventText,18,Accent,true,TextAnchor.MiddleCenter);
            BottomBar(w,h);
            if(game.DamageFlash>0&&!game.ReducedMotion)
            {Color danger=new Color(1,.23f,.11f,game.DamageFlash*.3f);Box(new Rect(0,0,w,5),danger);Box(new Rect(0,h-5,w,5),danger);Box(new Rect(0,0,5,h),danger);Box(new Rect(w-5,0,5,h),danger);}
            if(game.State==VectorEncounter.RunState.Paused)Pause(w,h);
            if(game.State==VectorEncounter.RunState.Complete||game.State==VectorEncounter.RunState.Defeated)Summary(w,h);
        }
        void Intro(float w,float h)
        {
            Panel(new Rect(25,178,428,419));
            Label(new Rect(49,201,370,19),"AN AUTOMATIC MARTIAL-ARTS ENCOUNTER",10,Accent,true);
            Label(new Rect(48,240,380,114),"ONE MAN.\nNO WAY BACK.",44,white,true);
            Label(new Rect(50,365,360,57),"Walk into the crowd. Chain three strikes.\nBreak the encirclement with a kinetic pulse.",15,muted);
            Box(new Rect(50,439,350,1),line);
            Label(new Rect(50,455,355,42),"Movement, fighting, and force push are automatic.\nWatch how the crowd changes around him.",12,white);
            if(Button(new Rect(50,523,350,48),"BEGIN THE WALK     [ ENTER ]",true))game.Begin();
            Label(new Rect(27,h-60,530,21),"60 SECONDS   /   STYLIZED PROTOTYPE   /   ELEVATED CHASE CAMERA",10,muted);
            Label(new Rect(w-320,h-60,295,23),"SPACE  pause     R  restart     M  mute",11,muted,false,TextAnchor.UpperRight);
        }
        void BottomBar(float w,float h)
        {
            float y=h-117;
            Panel(new Rect(25,y,460,92));
            Label(new Rect(43,y+12,280,13),"AUTOMATIC COMBO",9,muted);
            string[] names={"01  JAB","02  CROSS","03  BREAKER"};
            for(int i=0;i<3;i++)
            {
                bool active=game.IsAttacking&&game.ComboStep==i;
                var r=new Rect(43+i*140,y+34,130,34);Box(r,active?new Color(.19f,.36f,.31f):new Color(.08f,.13f,.15f));
                Box(new Rect(r.x,r.y+32,r.width,2),active?Accent:line);Label(r,names[i],11,active?Accent:muted,active,TextAnchor.MiddleCenter);
            }
            Panel(new Rect(w-329,y,304,92));
            Label(new Rect(w-310,y+13,260,14),"KINETIC FIELD  /  "+(game.AutoPush?"AUTO":"MANUAL TEST"),9,muted);
            Label(new Rect(w-310,y+34,180,25),game.IsCasting?"DISCHARGING":game.PushCharge>=1?"FORCE PUSH READY":"RECHARGING",14,game.PushCharge>=1?Accent:white,true);
            Box(new Rect(w-310,y+70,265,4),line);Box(new Rect(w-310,y+70,265*game.PushCharge,4),Accent);
            if(Button(new Rect(w-100,y+32,56,28),"F"))game.RequestPush();
            float middle=(w-485-329)/2+485;
            if(Button(new Rect(middle-70,y+12,140,28),"PAUSE  [ SPACE ]"))game.TogglePause();
            Label(new Rect(middle-110,y+52,220,21),"A  auto push    R  restart",10,muted,false,TextAnchor.MiddleCenter);
            Label(new Rect(w*.5f-180,h-22,360,17),"PROTOTYPE 0.1  ·  ALL GAMEPLAY RUNS AUTOMATICALLY",8,muted,false,TextAnchor.MiddleCenter);
        }
        void Pause(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.015f,.035f,.045f,.7f));var x=w*.5f-220;var y=h*.5f-176;Panel(new Rect(x,y,440,352));
            Label(new Rect(x+30,y+25,380,44),"TAKE A BREATH.",31,white,true);Label(new Rect(x+30,y+77,380,34),"The fight is paused. Your next move can wait.",13,muted);
            if(Button(new Rect(x+30,y+130,380,43),"RESUME  [ SPACE ]",true))game.TogglePause();
            if(Button(new Rect(x+30,y+187,185,34),game.AutoPush?"AUTO PUSH: ON":"AUTO PUSH: OFF"))game.ToggleAutoPush();
            if(Button(new Rect(x+225,y+187,185,34),game.Muted?"SOUND: OFF":"SOUND: ON"))game.ToggleSound();
            if(Button(new Rect(x+30,y+230,380,34),game.ReducedMotion?"REDUCED MOTION: ON":"REDUCED MOTION: OFF"))game.ToggleMotion();
            if(Button(new Rect(x+30,y+279,185,34),"RESTART ENCOUNTER"))game.Begin();
            if(Button(new Rect(x+225,y+279,185,34),"QUIT"))game.Quit();
        }
        void Summary(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.015f,.035f,.045f,.72f));float x=w*.5f-285,y=h*.5f-214;Panel(new Rect(x,y,570,428));
            bool win=game.State==VectorEncounter.RunState.Complete;
            Label(new Rect(x+32,y+26,500,20),win?"ENCOUNTER COMPLETE":"THE CROWD BROKE THROUGH",10,Accent,true);
            Label(new Rect(x+32,y+62,510,65),win?"STILL MOVING.":"OVERRUN.",43,white,true);
            Label(new Rect(x+32,y+133,500,42),win?"Sixty seconds. One fighter. Room to keep walking.":"Pressure accumulated faster than it could be cleared.",14,muted);
            Box(new Rect(x+32,y+191,506,1),line);
            string[] values={game.Kills.ToString(),game.ComboFinishers.ToString(),game.Pushes.ToString(),game.PeakCrowd.ToString()};
            string[] labels={"TAKEDOWNS","FINISHERS","FORCE PUSHES","PEAK CROWD"};
            for(int i=0;i<4;i++){Label(new Rect(x+32+i*130,y+211,116,40),values[i],29,white,true);Label(new Rect(x+32+i*130,y+257,116,20),labels[i],9,muted);}
            Label(new Rect(x+32,y+297,505,25),"NEXT TEST: Does the fighter feel powerful while the crowd stays dangerous?",11,muted);
            if(Button(new Rect(x+32,y+350,344,45),"RUN IT AGAIN  [ R ]",true))game.Begin();
            if(Button(new Rect(x+389,y+350,149,45),"QUIT"))game.Quit();
        }
    }
}
