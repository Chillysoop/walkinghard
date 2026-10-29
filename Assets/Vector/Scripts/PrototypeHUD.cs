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
            Box(r,!GUI.enabled?new Color(.08f,.13f,.15f):primary?(hover?new Color(.65f,1,.86f):Accent):(hover?new Color(.17f,.29f,.29f):new Color(.09f,.16f,.18f)));
            Label(r,text,12,!GUI.enabled?muted:primary?ink:white,true,TextAnchor.MiddleCenter);
            return GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        void OnGUI()
        {
            if(game==null)return;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            float w=Screen.width/scale,h=Screen.height/scale;
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(scale,scale,1));
            GUI.depth=0;
            if(game.State==VectorEncounter.RunState.Checkpoint){Checkpoint(w,h);return;}
            if(game.State==VectorEncounter.RunState.Paused){Pause(w,h);return;}
            if(game.State==VectorEncounter.RunState.Complete||game.State==VectorEncounter.RunState.Defeated){Summary(w,h);return;}
            // A quiet frame leaves the world and the hero in charge.
            Panel(new Rect(25,23,244,71));Box(new Rect(25,23,3,71),Accent);
            Label(new Rect(43,33,200,28),"V E C T O R",24,white,true);
            Label(new Rect(44,67,205,16),"HUMAN PINBALL  /  0.2",12,muted);
            if(game.State==VectorEncounter.RunState.Ready){Intro(w,h);return;}
            Panel(new Rect(w-278,23,253,71));
            Label(new Rect(w-259,34,140,16),game.CleaningUp?"CLEANUP WINDOW":"WAVE "+game.Wave+" / "+game.tuning.waveCount,12,muted);
            int remain=Mathf.CeilToInt(game.WaveRemaining);
            Label(new Rect(w-130,31,84,42),remain.ToString("00")+"s",32,white,true,TextAnchor.MiddleRight);
            Label(new Rect(w-259,59,140,19),game.StageName,12,Accent);
            Panel(new Rect(25,103,244,51));Label(new Rect(43,111,100,15),"INTEGRITY",10,muted);
            Label(new Rect(180,109,70,18),Mathf.CeilToInt(game.Health)+" / "+(int)game.tuning.heroHealth,11,white,false,TextAnchor.UpperRight);
            Box(new Rect(43,135,207,5),new Color(.17f,.24f,.26f));Box(new Rect(43,135,207*Mathf.Clamp01(game.Health/game.tuning.heroHealth),5),game.Health>game.tuning.heroHealth*.35f?Accent:new Color(1,.43f,.25f));
            Panel(new Rect(w-278,103,253,51));
            Label(new Rect(w-259,112,135,14),"HOSTILES IN REACH",10,muted);Label(new Rect(w-88,110,43,25),((int)game.CrowdNear).ToString("00"),21,game.CrowdNear>9?new Color(1,.56f,.31f):white,true,TextAnchor.UpperRight);
            Label(new Rect(w-259,132,190,14),game.Kills+" NEUTRALIZED   /   "+game.EnemyCount+" ACTIVE",9,muted);
            if(game.HighlightAge<1.8f)
                Label(new Rect(w*.5f-280,h-186,560,32),game.Highlight,20,game.Draft.Pinball?new Color(1,.77f,.37f):Accent,true,TextAnchor.MiddleCenter);
            else if((game.EventAge<1.1f&&game.IsAttacking)||game.IsCasting)
                Label(new Rect(w*.5f-180,h-176,360,30),game.EventText,18,Accent,true,TextAnchor.MiddleCenter);
            Panel(new Rect(25,164,244,74));
            Label(new Rect(43,176,210,18),game.Draft.Pinball?"HUMAN PINBALL ONLINE":game.Draft.Launch?"FINISHER LAUNCH ONLINE":"FISTS + KINETIC FIELD",12,game.Draft.Pinball?new Color(1,.77f,.37f):Accent,true);
            Label(new Rect(43,202,210,26),"LAUNCHES  "+game.Launches+"    COLLISIONS  "+game.CollisionHits,12,white);
            BottomBar(w,h);
            if(game.DamageFlash>0&&!game.ReducedMotion)
            {Color danger=new Color(1,.23f,.11f,game.DamageFlash*.3f);Box(new Rect(0,0,w,5),danger);Box(new Rect(0,h-5,w,5),danger);Box(new Rect(0,0,5,h),danger);Box(new Rect(w-5,0,5,h),danger);}
        }
        void Intro(float w,float h)
        {
            Panel(new Rect(25,178,428,419));
            Label(new Rect(49,201,370,19),"THREE WAVES. TWO CHOICES. YOUR BUILD.",12,Accent,true);
            Label(new Rect(48,240,380,114),"ONE MAN.\nNO WAY BACK.",44,white,true);
            Label(new Rect(50,365,360,64),"Start with fists and a force field.\nInstall upgrades between waves.\nTurn an enemy into your next projectile.",16,muted);
            Box(new Rect(50,439,350,1),line);
            Label(new Rect(50,455,355,50),"Combat is automatic. Choose Flux Bracers, then Fault Line to try Human Pinball.",15,white);
            if(Button(new Rect(50,523,350,48),"BEGIN THE WALK     [ ENTER ]",true))game.Begin();
            Label(new Rect(27,h-60,630,21),"3 WAVES   /   30s ARRIVALS + UP TO 8s CLEANUP   /   BANKED REROLLS",12,muted);
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
            Label(new Rect(w*.5f-230,h-22,460,17),"PROTOTYPE 0.2  /  YOUR CHOICES CHANGE THE FIGHT",12,muted,false,TextAnchor.MiddleCenter);
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
        void Checkpoint(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.01f,.025f,.03f,.88f));
            float x=40,y=(h-720)*.5f+25,width=w-80;
            Panel(new Rect(x,y,width,670));
            Label(new Rect(x+24,y+19,540,20),"CHECKPOINT "+game.Wave+" / "+(game.tuning.waveCount-1)+"     ·     COMBAT PAUSED",14,Accent,true);
            Label(new Rect(x+width-280,y+19,255,20),"INTEGRITY  "+Mathf.CeilToInt(game.Health)+" / "+(int)game.tuning.heroHealth,14,white,true,TextAnchor.UpperRight);
            Label(new Rect(x+24,y+53,width-48,49),"MAKE THE CROWD YOUR WEAPON.",34,white,true);
            var report=game.Waves[game.Waves.Count-1];
            Label(new Rect(x+24,y+105,width-48,26),"WAVE "+game.Wave+" COMPLETE   /   "+report.kills+" takedowns   /   "+report.launches+" launches   /   "+report.collisions+" collision hits   /   "+report.carried+" enemies carry forward",15,muted);
            Label(new Rect(x+24,y+146,width-48,42),"NEXT: "+game.NextThreat,16,white);
            float gap=16,cardWidth=(width-48-gap*2)/3,cardY=y+207;
            for(int i=0;i<game.Draft.Offers.Length;i++)
            {
                var card=game.Draft.Offers[i];float cx=x+24+i*(cardWidth+gap);bool selected=game.Draft.Selected==i;
                var rect=new Rect(cx,cardY,cardWidth,300);
                bool hover=rect.Contains(Event.current.mousePosition);
                Box(rect,selected?new Color(.10f,.24f,.23f):hover?new Color(.10f,.18f,.20f):new Color(.055f,.105f,.12f));
                Box(new Rect(cx,cardY,cardWidth,3),selected?Accent:line);
                if(GUI.Button(rect,GUIContent.none,GUIStyle.none))game.SelectUpgrade(i);
                Label(new Rect(cx+18,cardY+15,cardWidth-36,20),"0"+(i+1)+"  /  "+(i==0?"FEATURED PATH":"ALTERNATIVE"),12,selected?Accent:muted,true);
                Label(new Rect(cx+18,cardY+46,cardWidth-36,37),card.title,26,white,true);
                Label(new Rect(cx+18,cardY+86,cardWidth-36,22),card.family,12,i==0?Accent:muted,true);
                Label(new Rect(cx+18,cardY+122,cardWidth-36,78),card.description,16,white);
                Box(new Rect(cx+18,cardY+207,cardWidth-36,1),line);
                Label(new Rect(cx+18,cardY+221,cardWidth-36,66),card.preview,14,muted);
            }
            GUI.enabled=game.Draft.Rerolls>0;
            if(Button(new Rect(x+24,y+530,246,44),"REROLL  [ TAB ]   ·   "+game.Draft.Rerolls+" BANKED"))game.Reroll();
            GUI.enabled=true;
            Label(new Rect(x+290,y+531,width-680,48),"Unused rerolls carry forward. Each checkpoint adds one. The featured path stays available in this slice.",13,muted);
            GUI.enabled=game.Draft.Selected>=0;
            if(Button(new Rect(x+width-354,y+530,330,44),"INSTALL & CONTINUE  [ ENTER ]",true))game.ConfirmUpgrade();
            GUI.enabled=true;
            string selectedText=game.Draft.Selected>=0?"SELECTED: "+game.Draft.Offers[game.Draft.Selected].title+"  ·  Install to begin wave "+(game.Wave+1):"CHOOSE ONE CARD  /  Click a card or press 1, 2, or 3. Then install to continue.";
            Label(new Rect(x+24,y+599,width-48,29),selectedText,16,Accent,true);
            Label(new Rect(x+24,y+635,width-48,21),"CURRENT BUILD: "+game.Draft.Loadout,13,muted);
        }
        void Summary(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.015f,.035f,.045f,.87f));float x=w*.5f-480,y=h*.5f-315;Panel(new Rect(x,y,960,630));
            bool win=game.State==VectorEncounter.RunState.Complete;
            Label(new Rect(x+32,y+23,880,20),win?"THREE WAVES SURVIVED":"THE CROWD BROKE THROUGH IN WAVE "+game.Wave,14,Accent,true);
            Label(new Rect(x+32,y+57,880,65),win?"YOUR CHOICES. YOUR FIGHT.":"OVERRUN.",40,white,true);
            Label(new Rect(x+32,y+125,880,42),game.Draft.Loadout,17,muted);
            string[] values={game.Kills.ToString(),game.Launches.ToString(),game.CollisionHits.ToString(),game.CollisionKills.ToString()};
            string[] labels={"TAKEDOWNS","BODIES LAUNCHED","COLLISION HITS","COLLISION KILLS"};
            for(int i=0;i<4;i++){Label(new Rect(x+32+i*226,y+182,212,42),values[i],33,white,true);Label(new Rect(x+32+i*226,y+228,212,20),labels[i],13,muted);}
            Box(new Rect(x+32,y+267,896,1),line);
            float[] columns={32,171,355,525,706,828};
            string[] headers={"WAVE","TAKEDOWNS","LAUNCHES","COLLISIONS","CARRIED","INTEGRITY"};
            for(int j=0;j<headers.Length;j++)Label(new Rect(x+columns[j],y+284,120,23),headers[j],13,muted,true);
            for(int i=0;i<game.Waves.Count;i++)
            {
                var row=game.Waves[i];float rowY=y+319+i*39;
                string[] cells={"0"+row.wave,row.kills.ToString(),row.launches.ToString(),row.collisions.ToString(),row.carried.ToString(),Mathf.CeilToInt(row.healthEnd).ToString()};
                for(int j=0;j<cells.Length;j++)Label(new Rect(x+columns[j],rowY,120,25),cells[j],18,j==3&&row.collisions>0?new Color(1,.77f,.37f):white,true);
            }
            Label(new Rect(x+32,y+451,896,51),game.Draft.Pinball?"Fault Line dealt "+Mathf.RoundToInt(game.CollisionDamage)+" collision damage. Wave pressure increases; these totals show what happened, not a controlled damage comparison.":"Try Flux Bracers, then Fault Line to turn finishers into Human Pinball. Replay uses the same encounter seed.",15,muted);
            if(Button(new Rect(x+32,y+548,700,48),"RUN IT AGAIN  [ R ]",true))game.Begin();
            if(Button(new Rect(x+750,y+548,178,48),"QUIT"))game.Quit();
        }
    }
}
