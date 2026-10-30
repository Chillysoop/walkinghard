using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD : MonoBehaviour
    {
        public VectorEncounter game;
        readonly Dictionary<string,GUIStyle> styles=new Dictionary<string,GUIStyle>();
        Color ink=>new Color(.018f,.045f,.060f,game!=null&&game.State==VectorEncounter.RunState.Running?.66f:.94f);
        readonly Color line=new Color(.29f,.38f,.41f,.8f),white=new Color(.91f,.97f,1),muted=new Color(.60f,.68f,.71f);
        static readonly Color Accent=new Color(.25f,.94f,.92f),Strong=new Color(.90f,.95f,.97f);
        GUIStyle Style(int size,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
        {
            string key=size+"/"+bold+"/"+anchor;if(styles.TryGetValue(key,out var s))return s;
            s=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,alignment=anchor,wordWrap=true,padding=new RectOffset(0,0,0,0)};styles[key]=s;return s;
        }
        void Label(Rect r,string text,int size,Color color,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
        {var s=Style(size,bold,anchor);s.normal.textColor=color;GUI.Label(r,text,s);}
        static void Box(Rect r,Color color){var previous=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=previous;}
        void Panel(Rect r)
        {
            Plate(r,ink,line,11);
            Color corner=new Color(.58f,.78f,.9f,.9f);
            Stroke(new Vector2(r.x,r.y+14),new Vector2(r.x+14,r.y),corner,2);
            Stroke(new Vector2(r.xMax-14,r.yMax),new Vector2(r.xMax,r.yMax-14),corner,2);
        }
        bool Button(Rect r,string text,bool primary=false)
        {
            bool hover=r.Contains(Pointer);
            Color tint=!GUI.enabled?line:primary?Accent:muted;
            Plate(r,hover&&GUI.enabled?new Color(.07f,.18f,.21f,.98f):ink,tint,5,primary?1.5f:1);
            Label(r,text,12,!GUI.enabled?line:primary?Accent:white,true,TextAnchor.MiddleCenter);
            return GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        void OnGUI()
        {
            if(game==null)return;float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f)*(game.State==VectorEncounter.RunState.Running?.8f:1),w=Screen.width/scale,h=Screen.height/scale;
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(scale,scale,1));
            ReviewTextFit();
            detailValid=detailItem!=null&&(detailOffer!=null?game.State==VectorEncounter.RunState.Drafting&&System.Array.IndexOf(game.Deck.Offers,detailOffer)>=0:game.Deck.Items.Contains(detailItem));
            detailHovered=!dragging&&detailValid&&detailPanel.Contains(Pointer);
            if(detailHovered)detailLastHover=Time.realtimeSinceStartup;
            hoveredItem=detailHovered?detailItem:null;hoveredOffer=detailHovered?detailOffer:null;
            if(game.State==VectorEncounter.RunState.Ready){Intro(w,h);return;}
            if(game.TutorialActive&&game.TutorialLesson>=VectorEncounter.Lesson.Practice){TutorialCombat(w,h);return;}
            if(game.State==VectorEncounter.RunState.Revealing){FinaleReveal(w,h);return;}
            if(game.State==VectorEncounter.RunState.Drafting)
            {
                if(game.RoutePending){RouteChoice(w,h);return;}
                var eventType=Event.current.type;
                bool blocked=(iconGuide||detailHovered||waveDetails)&&(Event.current.isMouse||eventType==EventType.ScrollWheel);
                if(blocked)Event.current.type=EventType.Ignore;
                Draft(w,h);if(blocked)Event.current.type=eventType;DrawFusionFeedback(w,h);if(!waveDetails)DrawItemTooltip(w,h);DrawIconGuide(w,h);DrawWaveDetails(w,h);return;
            }
            if(game.State==VectorEncounter.RunState.Paused){Pause(w,h);return;}
            if(game.State==VectorEncounter.RunState.Complete||game.State==VectorEncounter.RunState.Defeated){Summary(w,h);return;}
            CombatHeader(w,h);CombatBar(w,h);
            if(game.MechanicReview)Label(new Rect(340,25,w-680,60),"MECHANIC REVIEW / "+game.ReviewLabel+"\n← / → previous / next; Space pauses",14,white,true,TextAnchor.UpperCenter);
            if(game.EventAge<1.4f&&game.Preparation<=0&&!game.ActionStatus.StartsWith("WAITING"))Label(new Rect(w/2-320,h-193,640,30),game.EventText,18,game.EventText.Contains("COLLISION")?Strong:Accent,true,TextAnchor.MiddleCenter);
            if(game.DamageFlash>0&&!game.ReducedMotion){var c=new Color(1,.23f,.1f,game.DamageFlash*.3f);Box(new Rect(0,0,w,5),c);Box(new Rect(0,h-5,w,5),c);}
            DrawFusionFeedback(w,h);DrawItemTooltip(w,h);
        }
        void Intro(float w,float h)
        {
            float x=40,y=Mathf.Max(50,(h-510)/2);Panel(new Rect(x,y,470,510));
            Label(new Rect(x+28,y+28,414,25),"WALKING HARD",27,white,true);
            Label(new Rect(x+28,y+85,414,100),"BUILD THE ORDER.\nBREAK THE CROWD.",31,Accent,true);
            Label(new Rect(x+28,y+200,414,60),"Two weapons. Three abilities.\nYour sequence does the fighting.",18,muted);
            if(Button(new Rect(x+28,y+288,414,52),"START RUN  [ ENTER ]",true))game.Begin();
            if(Button(new Rect(x+28,y+356,414,52),"QUICK TUTORIAL  [ T ]"))game.BeginTutorial();
            Label(new Rect(x+28,y+416,414,25),"Equip, fuse, augment and reorder. Hands-on practice.",12,muted);
            if(Button(new Rect(x+28,y+460,130,27),"QUIT"))game.Quit();
            Label(new Rect(x+180,y+464,262,22),"v"+Application.version,12,muted,false,TextAnchor.UpperRight);
        }
        void CombatHeader(float w,float h)
        {
            Panel(new Rect(20,18,306,105));
            TypeIcon(new Rect(36,42,36,36),ItemKind.Rig,muted);
            Label(new Rect(84,32,223,28),"WALKING HARD",22,white,true);
            Label(new Rect(84,69,220,19),"INTEGRITY     "+Mathf.CeilToInt(game.Health)+" / "+(int)game.MaxHealth,13,muted,true);
            Segments(new Rect(38,101,270,7),game.Health/game.MaxHealth,game.Health<game.MaxHealth*.3f?Strong:Accent,14);
            if(Button(new Rect(20,132,148,28),"PAUSE [ SPACE ]"))game.TogglePause();
            if(Button(new Rect(178,132,148,28),"NEW RUN [ R ]"))game.Begin();
            Label(new Rect(21,177,53,22),"SPEED",11,muted,true);
            float[] speeds={.5f,1,2,4};string[] labels={"0.5x","1x","2x","4x"};
            for(int i=0;i<4;i++)if(Button(new Rect(78+i*63,169,58,29),labels[i],game.CombatSpeed==speeds[i]))game.SetCombatSpeed(speeds[i]);
            Panel(new Rect(20,214,238,142));
            Label(new Rect(38,227,202,18),"SCORE",13,muted,true);
            Label(new Rect(38,247,205,36),game.Score.Banked.ToString("N0"),32,white,true);
            Label(new Rect(38,288,205,27),game.Score.Kills>0?"x"+game.Score.Multiplier+"  +"+game.Score.Pending.ToString("N0"):"BEST "+game.Score.Best.ToString("N0"),22,Strong,true);
            Label(new Rect(38,321,204,18),game.Score.Kills>0?game.Score.Kills+" KILL COMBO  /  "+game.Score.Reason:"CHAIN + BULK KILLS",10,muted,true);
            Segments(new Rect(38,343,202,5),game.Score.Remaining/ComboScore.Window,Strong,10);
            float x=w-330;
            Panel(new Rect(x,18,310,108));
            Label(new Rect(x+20,34,190,23),game.FinaleActive?"FINAL STAND":game.CleaningUp?"CLEANUP":"WAVE  "+game.Wave.ToString("00")+" / "+game.WaveCount.ToString("00"),17,white,true);
            Label(new Rect(x+212,29,80,40),game.FinaleActive?Mathf.CeilToInt(game.BossFraction*100)+"%":Mathf.CeilToInt(game.WaveRemaining)+"s",game.FinaleActive?24:31,white,true,TextAnchor.UpperRight);
            Stroke(new Vector2(x+20,64),new Vector2(x+290,64),line,1);
            string[] values={game.Kills.ToString(),game.EnemyCount.ToString(),game.CollisionHits.ToString()};
            string[] names={"TAKEDOWNS","INCOMING","IMPACTS"};
            for(int i=0;i<3;i++){Label(new Rect(x+20+i*96,75,90,24),values[i],18,white,true);Label(new Rect(x+20+i*96,101,90,16),names[i],10,muted,true);}
            UltimatePanel(new Rect(x,138,310,game.Deck.Ultimate==null?76:180));
            if(game.BossFraction>0){Rect boss=new Rect(w/2-200,20,400,62);Panel(boss);Label(new Rect(boss.x+12,boss.y+9,376,22),"THE PURSUER / "+(game.BossControlOpen?"CONTROL WINDOW":"BRACED · REDUCED CONTROL"),12,game.BossControlOpen?Accent:Strong,true);Segments(new Rect(boss.x+12,boss.y+39,376,8),game.BossFraction,Strong,20);}
            Label(new Rect(20,365,310,45),"BARRIER "+game.BarrierAmount.ToString("0")+" / "+game.ArmedState ,11,Accent,true);CombatPayoff();
        }
        void UltimatePanel(Rect r)
        {
            var item=game.Deck.Ultimate;Panel(r);Outline(r,new Color(Accent.r,Accent.g,Accent.b,.55f),11);
            if(item==null)
            {
                Label(new Rect(r.x+16,r.y+14,r.width-32,20),game.Deck.UltimatesUnlocked?"ULTIMATE / EMPTY":"ULTIMATE / CHECKPOINT 3",12,muted,true);
                Label(new Rect(r.x+16,r.y+42,r.width-32,20),game.Deck.UltimatesUnlocked?"Equip an ultimate at a checkpoint.":"Choose one after wave 3.",12,white);return;
            }
            ActionGlyph(new Rect(r.x+13,r.y+19,40,40),item?.def,Accent);
            Box(new Rect(r.x+43,r.y+46,18,18),ink);TypeIcon(new Rect(r.x+43,r.y+46,18,18),ItemKind.Ultimate,muted);
            Label(new Rect(r.x+65,r.y+14,r.width-80,18),"ULTIMATE / "+ItemRarity.Names[item?.rank??1].ToUpperInvariant(),9,item==null?muted:RarityColor(item.rank),true);
            Label(new Rect(r.x+65,r.y+34,r.width-80,24),item?.def.name??"EMPTY",17,Accent,true);
            bool delayed=game.TryGetActionDelay(5,out var delay);
            string status=delayed?delay.label+" "+delay.remaining.ToString("0.0")+"s":item==null?"UNEQUIPPED":game.ActiveSlot==5?"EXECUTING":game.UltimateRemaining>0?game.UltimateRemaining.ToString("0.0")+"s":game.UltimateQueued?"QUEUED":"READY";
            Label(new Rect(r.x+16,r.y+68,145,23),status,delayed?13:21,delayed?Strong:Accent,true);
            Label(new Rect(r.x+161,r.y+73,133,18),"Kills: "+game.KillsFor(item),12,muted,true,TextAnchor.UpperRight);
            EffectLine(new Rect(r.x+16,r.y+98,r.width-32,18),item);
            Label(new Rect(r.x+16,r.y+120,r.width-32,18),CombatStats(item)+" / "+game.UltimateCooldown.ToString("0")+"s CD",10,muted,true);
            Segments(new Rect(r.x+16,r.y+140,r.width-32,4),delayed?delay.Progress:1-game.UltimateRemaining/game.UltimateCooldown,delayed?Strong:Accent,20);
            ItemTooltip(new Rect(r.x,r.y,r.width,146),item);
            if(Button(new Rect(r.x+16,r.y+151,133,23),(game.UltimateAutomatic?"AUTO":"MANUAL")+" [ U ]"))game.ToggleUltimate();
            GUI.enabled=item!=null&&game.UltimateRemaining<=0&&game.ActiveSlot!=5;
            if(Button(new Rect(r.x+158,r.y+151,136,23),"QUEUE [ F ]",true))game.RequestUltimate();GUI.enabled=true;
        }
        void CombatBar(float w,float h)
        {
            float y=h-151,width=(w-100)/5;
            Plate(new Rect(16,y-10,w-32,157),new Color(.018f,.045f,.060f,.18f),line,11);
            for(int i=0;i<5;i++)
            {
                float x=30+i*(width+10);var item=game.Deck.Slots[i];var shown=item?.def.kind==ItemKind.Basic?game.Deck.WeaponFor(item):item;
                bool active=i==game.ActiveSlot||i==game.PairSlot,next=!active&&i==game.NextSlot;
                bool delayed=game.TryGetActionDelay(i,out var delay);
                Color edge=delayed?Strong:active?Accent:next?Strong:line;
                Plate(new Rect(x,y,width,137),ink,edge,8,active?2:1);
                if(i<4){Stroke(new Vector2(x+width,y+65),new Vector2(x+width+10,y+65),edge);Box(new Rect(x+width+3,y+63,4,4),edge);}
                ActionGlyph(new Rect(x+10,y+10,35,35),shown?.def,delayed?Strong:active?Accent:muted);
                if(shown!=null){Box(new Rect(x+33,y+34,17,17),ink);TypeIcon(new Rect(x+33,y+34,17,17),shown.def.kind,muted);}
                Label(new Rect(x+54,y+9,width-65,15),(i+1).ToString("00")+"  "+(delayed?delay.label+" "+delay.remaining.ToString("0.0")+"s":active?"ACTIVE":next?"NEXT":item?.def.kind==ItemKind.Basic?"WEAPON ATTACK":"ABILITY"),10,edge,true);
                Label(new Rect(x+54,y+28,width-65,23),shown?.def.name??"Empty",15,white,true);
                if(delayed){Box(new Rect(x+12,y+51,width-24,4),new Color(.25f,.18f,.08f));Box(new Rect(x+12,y+51,(width-24)*delay.Progress,4),Strong);}
                EffectLine(new Rect(x+12,y+57,width-24,18),item,12);
                if(shown!=null)DrawSynergyChips(new Rect(x+12,y+77,width-24,18),shown.def);
                Label(new Rect(x+12,y+97,width-24,17),CombatStats(item),11,muted,true);
                Label(new Rect(x+12,y+115,width-24,17),shown==null?"SKIPPED":"Kills: "+game.ActionKills(item)+"  /  "+ItemRarity.Names[shown.rank],11,shown==null?muted:RarityColor(shown.rank));
                if(active&&!delayed)Box(new Rect(x+12,y+133,(width-24)*game.ActionProgress,2),Accent);
                if(shown?.augment!=null){bool shared=item.def.kind==ItemKind.Basic&&item.mount==1&&game.Deck.WeaponFor(game.Deck.Basics[0])==shown;Label(new Rect(x+12,y-18,width-24,17),(shared?"SHARED SOCKET":shown.augment.Name)+(game.BreachArmed(shown)?" · ARMED":""),10,Accent,true);}
                ItemTooltip(new Rect(x,y,width,137),item);
            }
            GroupOutline(30,y,width,10,137);
            if(game.Preparation>0||game.ActionStatus.StartsWith("WAITING"))Label(new Rect(w/2-300,y-38,600,24),game.ActionStatus,13,Strong,true,TextAnchor.MiddleCenter);
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
            if(Button(new Rect(x+30,y+330,235,38),"MAIN MENU"))game.MainMenu();
            if(Button(new Rect(x+275,y+330,235,38),"QUIT"))game.Quit();
        }
        Vector2 recapScroll;
        void Summary(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.015f,.035f,.045f,.87f));float x=w/2-480,y=h/2-305;Panel(new Rect(x,y,960,610));
            bool win=game.State==VectorEncounter.RunState.Complete;
            Label(new Rect(x+32,y+24,896,22),win?game.WaveCount+" WAVES SURVIVED":"OVERRUN IN WAVE "+game.Wave,15,Accent,true);
            Label(new Rect(x+32,y+66,896,60),"YOUR ORDER. YOUR FIGHT.",40,white,true);
            Label(new Rect(x+32,y+137,896,40),game.Deck.Loadout,17,muted);
            string[] values={game.Score.Banked.ToString("N0"),game.Kills.ToString(),game.Score.Best.ToString("N0"),game.CollisionHits.ToString()};
            string[] labels={"SCORE","KILLS","BEST COMBO","COLLISION HITS"};
            for(int i=0;i<4;i++){Label(new Rect(x+32+i*226,y+194,212,42),values[i],33,white,true);Label(new Rect(x+32+i*226,y+241,212,20),labels[i],13,muted);}
            Rect viewport=new Rect(x+28,y+282,904,235);float contentHeight=42+game.Waves.Count*23+game.Deck.Items.Count*41+game.InteractionNotes.Count*19+25;
            recapScroll=GUI.BeginScrollView(viewport,recapScroll,new Rect(0,0,880,contentHeight));
            Label(new Rect(4,0,860,20),"WAVE     KILLS     LAUNCHES     COLLISIONS     INTEGRITY",12,muted,true);
            float rowY=25;foreach(var row in game.Waves){Label(new Rect(4,rowY,860,22),row.wave.ToString("00")+"          "+row.kills+"             "+row.launches+"                  "+row.collisions+"                 "+row.health.ToString("0"),13,white);rowY+=23;}
            Label(new Rect(4,rowY,860,20),"ITEM CONTRIBUTIONS · setup assists never add kills",12,Accent,true);rowY+=25;
            foreach(var item in game.Deck.Items){if(item.def.kind==ItemKind.Basic||item.def.kind==ItemKind.Augment)continue;var c=game.ContributionFor(item);Label(new Rect(4,rowY,870,18),item.Name+" / Kills: "+game.KillsFor(item)+" / direct "+c.direct.ToString("0")+" / collision "+c.collision.ToString("0")+" / control "+c.control.ToString("0.0")+"s / absorb "+c.absorption.ToString("0")+" / healing "+c.healing.ToString("0.#")+" / assists "+c.assists,11,white);Label(new Rect(4,rowY+18,870,18),"Reload "+c.reload.ToString("0.0")+"s / prepare "+c.preparation.ToString("0.0")+"s / range wait "+c.rangeWait.ToString("0.0")+"s / augment damage "+c.augment.ToString("0"),10,muted);rowY+=41;}
            foreach(var note in game.InteractionNotes){Label(new Rect(4,rowY,870,18),note.Key+" × "+note.Value,11,Accent);rowY+=19;}
            GUI.EndScrollView();
            if(Button(new Rect(x+32,y+534,430,46),"NEW DRAFT [ R ]",true))game.Begin();
            if(Button(new Rect(x+478,y+534,290,46),"REPEAT SEED [ T ]"))game.Begin(true);
            if(Button(new Rect(x+784,y+534,144,46),"MAIN MENU"))game.MainMenu();
        }
    }
}
