using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        bool iconGuide;
        public bool HandleOverlayInput()
        {
            if(!iconGuide)return false;
            if(Input.GetKeyDown(KeyCode.Escape))iconGuide=false;
            return true;
        }
        static readonly string[] synergyKeys={"stagger","launch","collision","sever","mark","chain","gather","pierce","reload","guard","sustain","tempo","burst","push","close"};
        static string SynergyLabel(string key)=>key=="mark"?"CONDUCTIVE":key=="sustain"?"HEALING":key.ToUpperInvariant();
        static int SynergySymbol(string key)=>key=="stagger"?0:key=="launch"?1:key=="collision"?2:key=="gather"?3:key=="chain"?4:key=="sever"?5:key=="pierce"?6:key=="guard"?7:key=="mark"?8:key=="reload"?9:key=="sustain"?10:key=="tempo"?11:key=="burst"?12:key=="push"?13:14;
        void DrawSynergyChips(Rect r,ItemDefinition def)
        {
            if(def.kind==ItemKind.Augment)
            {
                Label(r,"MODIFIES "+(def.module.family<0?"ABILITY":"WEAPON")+" / HOVER FOR TRADEOFF",9,Violet,true);return;
            }
            var tags=def.tags.Split(' ');float x=r.x;
            foreach(string tag in tags.Where(t=>synergyKeys.Contains(t)).Take(2))
            {
                string label=SynergyLabel(tag);float width=Style(9,true).CalcSize(new GUIContent(label)).x+25;
                if(x+width>r.xMax)break;
                Plate(new Rect(x,r.y,width,18),new Color(.03f,.08f,.10f),line,3);
                EffectIcon(new Rect(x+1,r.y-1,20,20),SynergySymbol(tag),muted);
                Label(new Rect(x+23,r.y+2,width-23,15),label,9,muted,true);x+=width+5;
            }
        }
        void DrawIconGuide(float w,float h)
        {
            if(!iconGuide)return;
            hoveredItem=null;Box(new Rect(0,0,w,h),new Color(0,0,0,.78f));
            Rect r=new Rect(w/2-320,h/2-265,640,530);Panel(r);
            Label(new Rect(r.x+24,r.y+18,550,27),"READ YOUR LOADOUT",23,white,true);
            if(Button(new Rect(r.xMax-114,r.y+18,90,25),"CLOSE [ESC]"))iconGuide=false;
            Label(new Rect(r.x+24,r.y+57,592,34),"Artwork identifies the item. Symbols identify its type and synergies.\nThe border and written rarity identify which copies can fuse.",13,muted);
            var kinds=new[]{ItemKind.Weapon,ItemKind.Ability,ItemKind.Ultimate,ItemKind.Helmet,ItemKind.Armor,ItemKind.Boots,ItemKind.Rig,ItemKind.Augment};
            for(int i=0;i<kinds.Length;i++)
            {
                float x=r.x+24+i%4*149,y=r.y+108+i/4*35;
                TypeIcon(new Rect(x,y,22,22),kinds[i],muted);Label(new Rect(x+29,y+3,120,20),kinds[i]==ItemKind.Rig?"RIG":kinds[i].ToString().ToUpperInvariant(),11,white,true);
            }
            Label(new Rect(r.x+24,r.y+185,592,22),"RARITY / THREE IDENTICAL COPIES AT ONE TIER",12,Accent,true);
            for(int rank=1;rank<=5;rank++)
            {
                Rect badge=new Rect(r.x+24+(rank-1)*119,r.y+216,111,43);Plate(badge,ink,RarityColor(rank),5);
                Label(badge,ItemRarity.Names[rank].ToUpperInvariant()+"\n"+(rank==5?"MAX RARITY":"3 COPIES → NEXT"),10,RarityColor(rank),true,TextAnchor.MiddleCenter);
            }
            Label(new Rect(r.x+24,r.y+274,592,39),"2 Common + 1 Rare do not fuse. You need a third Common.\nAugments use the host's rarity and never fuse.",13,white);
            Label(new Rect(r.x+24,r.y+328,592,22),"SYNERGY SYMBOLS / SAME SYMBOL, SAME MEANING",12,Accent,true);
            for(int i=0;i<synergyKeys.Length;i++)
            {
                float x=r.x+24+i%4*149,y=r.y+363+i/4*32;
                EffectIcon(new Rect(x,y,22,22),SynergySymbol(synergyKeys[i]),muted);
                Label(new Rect(x+28,y+3,118,20),SynergyLabel(synergyKeys[i]),10,white,true);
            }
            // Do not let clicks through the modal rearrange the checkpoint behind it.
            if(Event.current.isMouse||Event.current.type==EventType.ScrollWheel)Event.current.Use();
        }
    }
}
