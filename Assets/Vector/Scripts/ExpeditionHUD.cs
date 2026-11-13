using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        bool expeditionArchive;
        public bool ExpeditionOverlayOpen=>expeditionArchive;
        bool ExpeditionButton(Rect rect,string text,bool available,bool primary=false)
        {
            if(available)return Button(rect,text,primary);
            Plate(rect,ink,line,5);Label(rect,text,12,muted,true,TextAnchor.MiddleCenter);return false;
        }
        void ExpeditionIntro(float w,float h,float y)
        {
            float x=540,width=w-x-40;Panel(new Rect(x,y,width,570));
            Label(new Rect(x+28,y+28,width-56,25),"CINDERWORKS / EXPEDITION",22,Accent,true);
            Label(new Rect(x+28,y+82,width-56,72),"The factory remembers your orders.\nIt has forgotten who gave them.",23,white,true);
            Label(new Rect(x+28,y+177,width-56,94),"Eight waves. Choose roads while you walk.\nRisk salvage at the overtime lockers, then spend it on repairs or matching gear at a salvage stop.",17,muted);
            if(Button(new Rect(x+28,y+288,width-56,52),"NEW EXPEDITION",true))game.BeginExpedition();
            Label(new Rect(x+28,y+344,width-56,30),game.HasCheckpoint?"Starting a new expedition replaces your stored run.":"Saves at reward checkpoints. Combat resumes from the last checkpoint.",11,muted);
            if(ExpeditionButton(new Rect(x+28,y+389,width-56,45),game.HasCheckpoint?game.ResumeLabel:"NO SAVED EXPEDITION",game.HasCheckpoint))game.ResumeExpedition();
            if(Button(new Rect(x+28,y+454,width-56,42),"RECOVERED RECORDS  ·  "+game.ArchiveCount+" / 3"))expeditionArchive=true;
            Label(new Rect(x+28,y+514,width-56,40),game.SaveMessage,12,muted);
            if(expeditionArchive)ExpeditionRecords(w,h);
        }
        void ExpeditionRecords(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(0,0,0,.86f));float x=(w-900)/2,y=(h-560)/2;Panel(new Rect(x,y,900,560));
            Label(new Rect(x+28,y+25,820,35),"RECOVERED RECORDS",25,Accent,true);
            Label(new Rect(x+28,y+70,820,40),"Fragments recovered from overtime stops. Kept between expeditions; no combat bonuses.",14,muted);
            for(int i=0;i<3;i++)
            {
                float top=y+130+i*115;bool found=game.HasOvertimeRecord(i);
                Label(new Rect(x+28,top,820,25),found?VectorEncounter.OvertimeRecordTitle(i):"SEALED RECORD",17,found?white:muted,true);
                Label(new Rect(x+28,top+33,820,65),found?VectorEncounter.OvertimeRecord(i):"Recover this fragment by leaving an overtime event, whether you bank the haul or lose it.",15,muted);
            }
            if(Button(new Rect(x+28,y+493,844,40),"BACK"))expeditionArchive=false;
        }
        void ExpeditionStopScreen(float w,float h)
        {
            float x=(w-1120)/2,y=(h-600)/2;var stop=game.Stop;bool overtime=stop.kind==EncounterRoute.Overtime;
            Box(new Rect(0,0,w,h),new Color(0,0,0,.64f));Panel(new Rect(x,y,1120,600));
            Label(new Rect(x+26,y+22,650,30),overtime?game.OvertimeTitle:"IVEN'S SALVAGE BENCH",24,Accent,true);
            Label(new Rect(x+26,y+64,680,24),"BANKED SALVAGE  "+game.Salvage+"     ·     INTEGRITY  "+Mathf.CeilToInt(game.Health)+" / "+Mathf.RoundToInt(game.MaxHealth),15,white,true);
            if(Button(new Rect(x+855,y+22,239,36),"SAVE & MAIN MENU"))game.SuspendExpedition();
            Stroke(new Vector2(x+26,y+108),new Vector2(x+1094,y+108),line,1);
            Label(new Rect(x+26,y+135,305,190),overtime?game.OvertimeStory:"Iven sorts useful parts from the factory's wreckage. The Continuance calls this theft. Iven calls it returning equipment to the living.",18,white);
            Label(new Rect(x+26,y+353,305,112),overtime?"Each attempt risks the whole unbanked haul. The odds fall and the stakes rise. Banked salvage is safe.\n\nUp to four attempts. You can leave with the starting haul.":"One repair and one matching copy per visit. Stock is reserved from your current build.\n\nThree copies of the same item and rarity fuse automatically.",15,muted);
            Label(new Rect(x+26,y+531,305,44),"Travel and combat are paused.\n"+game.SaveMessage,12,muted);
            float right=x+370,width=724;
            if(overtime)
            {
                Label(new Rect(right,y+135,width,26),"UNBANKED HAUL  "+stop.stash+" SALVAGE",24,Accent,true);
                Label(new Rect(right,y+176,width,24),"ATTEMPTS  "+stop.attempt+" / 4",13,muted,true);
                if(stop.reveal)
                {
                    Label(new Rect(right,y+225,width,140),stop.result,23,stop.failed?new Color(1,.63f,.35f):white,true);
                    Label(new Rect(right,y+385,width,52),stop.failed?"The recovered record survives. Your banked salvage was untouched.":stop.attempt==4?"All accessible lockers are open. Bank the haul and continue.":"Keep this haul, or inspect the next attempt before deciding.",16,muted);
                    if(Button(new Rect(right,y+500,width,56),stop.failed?"LEAVE":stop.attempt==4?"BANK "+stop.stash+" SALVAGE":"SEE NEXT RISK",true))game.ContinueOvertime();
                    if(!stop.failed&&stop.attempt<4&&Button(new Rect(right,y+440,width,44),"BANK "+stop.stash+" SALVAGE & LEAVE"))game.CashOutStop();
                }
                else
                {
                    Label(new Rect(right,y+225,width,55),stop.chance+"% SUCCESS",38,white,true);
                    Label(new Rect(right,y+294,width,42),"SUCCESS   +"+stop.gain+" salvage added to the haul",21,Accent,true);
                    Label(new Rect(right,y+350,width,65),"FAILURE   lose the "+stop.stash+" salvage haul\nand take "+stop.harm+" integrity damage",20,new Color(1,.68f,.43f),true);
                    if(ExpeditionButton(new Rect(right,y+440,width,50),"PUSH YOUR LUCK  ·  "+stop.chance+"%",game.Health>stop.harm,true))game.PushOvertime();
                    if(Button(new Rect(right,y+506,width,50),"BANK "+stop.stash+" SALVAGE & LEAVE"))game.CashOutStop();
                    if(game.Health<=stop.harm)Label(new Rect(right,y+410,width,24),"Too damaged to risk another attempt. Bank the haul safely.",13,white);
                }
            }
            else
            {
                Label(new Rect(right,y+131,width,25),"REPAIR  ·  RESTORE UP TO "+game.RepairAmount+" INTEGRITY",18,white,true);
                if(ExpeditionButton(new Rect(right,y+166,width,40),stop.repaired?"REPAIR USED":game.Health>=game.MaxHealth?"FULL INTEGRITY":"REPAIR  ·  12 SALVAGE",!stop.repaired&&game.Salvage>=12&&game.Health<game.MaxHealth))game.BuyRepair();
                Label(new Rect(right,y+230,width,24),"MATCHING COPY  ·  CHOOSE ONE",16,Accent,true);
                for(int i=0;i<stop.stock.Length;i++)
                {
                    var offer=stop.stock[i];float row=y+268+i*56;
                    ActionGlyph(new Rect(right,row,34,34),offer.def,RarityColor(offer.rank));
                    int copies=game.Deck.Copies(offer.def,offer.rank);bool fuses=!stop.boughtCopy&&copies>=2;
                    Label(new Rect(right+46,row,width-270,21),offer.def.name,15,white,true);
                    Label(new Rect(right+46,row+22,width-270,17),ItemRarity.Names[offer.rank]+" · owned "+copies+" / 3"+(fuses?" → "+ItemRarity.Names[offer.rank+1]:""),12,fuses?RarityColor(offer.rank+1):muted,true);
                    Label(new Rect(right+46,row+40,width-46,16),ItemSummary(new LoadoutItem(offer.def,offer.rank)),11,muted);
                    if(ExpeditionButton(new Rect(right+width-202,row,202,36),stop.boughtCopy?"COPY PURCHASED":(fuses?"FUSE":"BUY")+" · "+VectorEncounter.CopyCost(offer.rank)+" SALVAGE",!stop.boughtCopy&&game.Salvage>=VectorEncounter.CopyCost(offer.rank)))game.BuyCopy(i);
                }
                if(stop.stock.Length==0)Label(new Rect(right,y+278,width,60),"No eligible equipment to copy this visit.",16,muted);
                Label(new Rect(right,y+448,width,60),stop.result,13,white);
                if(Button(new Rect(right,y+516,width,40),"LEAVE & CHOOSE REWARDS",true))game.CashOutStop();
            }
        }
    }
}
