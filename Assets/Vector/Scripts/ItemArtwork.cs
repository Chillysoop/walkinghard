using System.Collections.Generic;
using UnityEngine;

namespace VectorGame
{
    // One identity image per definition, shared by every UI surface and rarity.
    public static class ItemArtwork
    {
        readonly struct Entry
        {
            public readonly int sheet,index,columns,rows;
            public Entry(int sheet,int index,int columns,int rows){this.sheet=sheet;this.index=index;this.columns=columns;this.rows=rows;}
        }
        static readonly Dictionary<string,Entry> entries=new Dictionary<string,Entry>();
        static readonly Texture2D[] textures=new Texture2D[8];
        static readonly string[] files={"ItemArt/Equipment","ItemArt/Actions","ItemArt/Augments","ItemArt/Siphon-siphonblade","ItemArt/Siphon-DrainBrand","ItemArt/Siphon-reclaimer","ItemArt/Siphon-predator","ItemArt/Siphon-harvest"};
        static ItemArtwork()
        {
            Add(3,1,1,"siphonblade");Add(4,1,1,"DrainBrand");Add(5,1,1,"reclaimer");Add(6,1,1,"predator");Add(7,1,1,"harvest");
            Add(0,6,5,"fists wraps sword greatsword breachhammer hammer pistol rifle scattergun visor surveyor salvager capacitor plate reactive grounding duelist treads momentum loader arcstep faultline overclock amplifier relay magnet");
            Add(1,4,4,"SteelJab Breaker ForcePush MassDriver GravityWell ArcBolt ConductiveBrand VectorTether ReapingCut Counterguard singularity tempest cyclone redline");
            Add(2,4,4,"aug-FollowThrough aug-Focused aug-Echo aug-Afterfield aug-SplitPayload aug-Backwash aug-Discharge aug-Anchor aug-Flurry aug-PalmDrive aug-Crosscut aug-HookedEdge aug-Groundbreaker aug-Piledriver aug-BoreRounds aug-BreachLoad");
        }
        static void Add(int sheet,int columns,int rows,string ids)
        {int index=0;foreach(string id in ids.Split(' '))entries.Add(id,new Entry(sheet,index++,columns,rows));}
        public static bool Has(string id)=>entries.ContainsKey(id);
        public static bool Draw(Rect r,ItemDefinition def)
        {
            if(def==null||!entries.TryGetValue(def.id,out var e))return false;
            if(textures[e.sheet]==null)textures[e.sheet]=Resources.Load<Texture2D>(files[e.sheet]);
            var texture=textures[e.sheet];if(texture==null)return false;
            // Inset a fraction of a pixel to avoid sampling an adjacent cell.
            float px=.5f/texture.width,py=.5f/texture.height;
            Rect uv=new Rect((e.index%e.columns)/(float)e.columns+px,1-(e.index/e.columns+1)/(float)e.rows+py,1f/e.columns-2*px,1f/e.rows-2*py);
            Color before=GUI.color;GUI.color=Color.white;GUI.DrawTextureWithTexCoords(r,texture,uv);GUI.color=before;return true;
        }
    }
}
