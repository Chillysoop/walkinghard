using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        int seenFusions;
        float fusionStarted=-10;
        bool fusionPinned;
        VectorEncounter.FusionResult shownFusion;
        AbilityDeck fusionDeck;
        void DrawFusionFeedback(float w,float h)
        {
            if(fusionDeck!=game.Deck){fusionDeck=game.Deck;seenFusions=0;shownFusion=null;fusionPinned=false;fusionStarted=-10;}
            float age=Time.realtimeSinceStartup-fusionStarted;
            if(seenFusions<game.FusionHistory.Count&&(shownFusion==null||age>4.8f))
            {shownFusion=game.FusionHistory[seenFusions++];fusionStarted=Time.realtimeSinceStartup;age=0;}
            if(shownFusion==null||(!fusionPinned&&age>4.8f))return;
            bool draft=game.State==VectorEncounter.RunState.Drafting;
            Rect r=draft?new Rect(20,244,266,246):new Rect(w/2-260,22,520,206);
            var f=shownFusion;Color color=RarityColor(f.after);
            Plate(r,new Color(.012f,.04f,.052f,.995f),color,11,2);
            Label(new Rect(r.x+16,r.y+12,r.width-32,21),"FUSION  /  3 COPIES BECOME 1",12,Accent,true);
            Label(new Rect(r.x+16,r.y+37,r.width-32,26),f.definition.name,19,white,true);
            float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.7f)),cy=r.y+80;
            for(int i=-1;i<=1;i++)
            {
                float cx=r.center.x+i*65*(1-t),size=25+9*t;
                ActionGlyph(new Rect(cx-size/2,cy-size/2,size,size),f.definition,age<.7f?RarityColor(f.before):color);
                if(t==1)break;
            }
            Label(new Rect(r.x+16,r.y+104,r.width-32,18),ItemRarity.Names[f.before].ToUpperInvariant()+"  >  "+ItemRarity.Names[f.after].ToUpperInvariant(),12,color,true);
            Label(new Rect(r.x+16,r.y+130,r.width-32,draft?31:18),"BEFORE  "+ItemStats(f.definition,f.before),11,muted,true);
            Label(new Rect(r.x+16,r.y+(draft?166:155),r.width-32,draft?31:18),"AFTER    "+ItemStats(f.definition,f.after),11,color,true);
            string change=f.definition.kind==ItemKind.Weapon?AttackDescription(new LoadoutItem(f.definition,f.after)):f.definition.upgrade;
            Label(new Rect(r.x+16,r.y+(draft?204:181),r.width-32,draft?36:22),change,11,white);
        }
        void FusionHistoryButton(Rect r)
        {
            if(game.FusionHistory.Count==0)return;
            if(Button(r,fusionPinned?"CLOSE FUSION DETAILS":"LAST FUSION / BEFORE + AFTER"))
            {shownFusion=game.FusionHistory[game.FusionHistory.Count-1];fusionPinned=!fusionPinned;fusionStarted=Time.realtimeSinceStartup-1;}
        }
    }
}
