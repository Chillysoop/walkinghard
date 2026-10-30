using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class VectorEncounter
    {
        readonly Dictionary<string,float> synergyCueTimes=new Dictionary<string,float>();
        float synergyCueUntil,synergyCueNext;
        public string PayoffKey {get;private set;}="";
        public string PayoffTitle {get;private set;}="";
        public string PayoffDetail {get;private set;}="";
        public bool PayoffVisible=>Time.unscaledTime<synergyCueUntil;
        void ResetSynergyCues(){synergyCueTimes.Clear();synergyCueUntil=synergyCueNext=0;PayoffKey=PayoffTitle=PayoffDetail="";}
        void SynergyCue(string key,string title,string detail)
        {
            if(simulation)return;float now=Time.unscaledTime;
            if(now<synergyCueNext||synergyCueTimes.TryGetValue(title,out float next)&&now<next)return;
            synergyCueTimes[title]=now+5;synergyCueNext=now+1.9f;synergyCueUntil=now+1.6f;
            PayoffKey=key;PayoffTitle=title;PayoffDetail=detail;
        }
        void NotePayoff(string text)
        {
            switch(text)
            {
                case "Lightning consumed Conductive":case "Body impact consumed Conductive":SynergyCue("mark","CONDUCTIVE DISCHARGE","Mark consumed → bonus lightning");break;
                case "Gather / pull enabled a follow-up hit":SynergyCue("gather","GATHER FOLLOW-UP","A gathered foe took a follow-up hit");break;
                case "Siphon restored health":SynergyCue("siphon","LIFESTEAL","Damage restored health");break;
                case "Duelist ward blocked damage":SynergyCue("guard","DUELIST WARD","Reload / sever guard absorbed damage");break;
                case "Healing let Predator speed an ability":SynergyCue("tempo","HEALING → HASTE","Stored healing boost spent");break;
                case "Launch let Momentum speed an ability":SynergyCue("tempo","LAUNCH → HASTE","Stored launch boost spent");break;
            }
        }
    }
    public sealed partial class PrototypeHUD
    {
        BuildSynergies.Link CurrentBuildLink()
        {
            int offer=PreviewOffer();var item=dragItem??selectedItem;
            var def=offer>=0?game.Deck.Offers[offer].def:item?.def;
            var target=dropRegions.FirstOrDefault(r=>r.offer<0&&!r.socket&&r.place!=PlaceKind.Inventory&&r.rect.Contains(Pointer));
            if(target==null&&def!=null&&def.kind!=ItemKind.Augment)
            {
                game.Deck.SuggestedPlace(def,out var place,out int index);
                if(def.kind==ItemKind.Ability&&game.Deck.SelectedSlot>=0){place=PlaceKind.Sequence;index=game.Deck.SelectedSlot;}
                target=dropRegions.FirstOrDefault(r=>r.offer<0&&!r.socket&&r.place==place&&r.index==index);
            }
            var displaced=target==null?null:Host(target);
            if(target?.item?.def.kind==ItemKind.Basic&&def?.kind==ItemKind.Ability)displaced=null;
            return BuildSynergies.Find(game.Deck,def,displaced,item).FirstOrDefault();
        }
        void BuildFitLine(Rect r)
        {
            var fit=CurrentBuildLink();if(fit==null)return;
            EffectIcon(new Rect(r.x,r.y,19,19),SynergySymbol(fit.key),white);
            Label(new Rect(r.x+27,r.y,r.width-27,r.height),"WITH "+fit.partner.def.name+"  ·  "+fit.text,12,white);
        }
        void HighlightBuildPartner()
        {
            var fit=CurrentBuildLink();if(fit==null)return;
            foreach(var region in dropRegions.Where(r=>r.offer<0&&!r.socket&&r.place!=PlaceKind.Inventory&&Host(r)==fit.partner))
            {
                // White link badge is distinct from rarity frames and cyan drop guidance.
                Rect badge=new Rect(region.rect.xMax-23,region.rect.y+4,18,18);
                Box(badge,ink);EffectIcon(badge,SynergySymbol(fit.key),white);
                Stroke(new Vector2(region.rect.x+8,region.rect.yMax-4),new Vector2(region.rect.xMax-8,region.rect.yMax-4),white,2);
            }
        }
        void CombatPayoff()
        {
            if(!game.PayoffVisible)return;Rect r=new Rect(20,419,310,52);Panel(r);
            EffectIcon(new Rect(r.x+10,r.y+14,24,24),SynergySymbol(game.PayoffKey),white);
            Label(new Rect(r.x+43,r.y+8,258,19),game.PayoffTitle,12,white,true);
            Label(new Rect(r.x+43,r.y+28,258,17),game.PayoffDetail,10,muted);
        }
    }
}
