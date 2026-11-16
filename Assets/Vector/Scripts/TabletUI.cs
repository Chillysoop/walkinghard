using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        static Texture2D tabletMaterials;
        static readonly Color TabletText=new Color(.76f,.84f,.86f);
        static readonly Color TabletTeal=new Color(.10f,.24f,.25f);
        static readonly Color TabletCargo=new Color(.20f,.21f,.23f);
        static readonly Color TabletBlue=new Color(.13f,.20f,.28f);

        // Four generated material tiles, sliced independently. Corners stay a fixed
        // size in canvas units; the live layout and text never stretch with the art.
        void TabletMaterial(Rect r,int tile,float border=10)
        {
            if(tabletMaterials==null)tabletMaterials=Resources.Load<Texture2D>("TabletUI/PanelMaterials");
            if(tabletMaterials==null){Plate(r,tile==1?TabletTeal:tile==2?TabletCargo:TabletBlue,line,6);return;}
            float inset=.5f/tabletMaterials.width,span=.5f-2*inset,edge=span*.06f;
            float u=(tile%2)*.5f+inset,v=1-(tile/2+1)*.5f+inset;
            border=Mathf.Min(border,Mathf.Min(r.width,r.height)*.25f);
            for(int y=0;y<3;y++)for(int x=0;x<3;x++)
            {
                float dx=x==0?r.x:x==1?r.x+border:r.xMax-border;
                float dy=y==0?r.y:y==1?r.y+border:r.yMax-border;
                float dw=x==1?r.width-2*border:border,dh=y==1?r.height-2*border:border;
                float su=u+(x==0?0:x==1?edge:span-edge),sw=x==1?span-2*edge:edge;
                float sh=y==1?span-2*edge:edge,sv=v+(y==0?span-edge:y==1?edge:0);
                GUI.DrawTextureWithTexCoords(new Rect(dx,dy,dw,dh),tabletMaterials,new Rect(su,sv,sw,sh));
            }
        }
        void TabletFrame(float w,float h)
        {
            Box(new Rect(0,0,w,h),new Color(.018f,.027f,.033f));
            TabletMaterial(new Rect(3,3,w-6,h-6),0,15);
            // A quiet screen inset leaves the machined outer rim visible.
            Box(new Rect(16,14,w-32,h-28),new Color(.025f,.040f,.048f,.65f));
            Box(new Rect(20,15,3,18),Accent);
            Stroke(new Vector2(20,87),new Vector2(w-20,87),new Color(.35f,.51f,.55f,.55f));
        }
        void TabletSection(Rect r,int material,string title)
        {
            TabletMaterial(r,material);
            Box(new Rect(r.x+10,r.y+7,r.width-20,23),new Color(.025f,.05f,.06f,.50f));
            Box(new Rect(r.x+11,r.y+10,3,15),material==1?new Color(.37f,.69f,.67f):new Color(.59f,.65f,.72f));
            Label(new Rect(r.x+22,r.y+10,r.width-34,18),title,12,white,true);
        }
        void TabletCell(Rect r,Color fill,Color rarity,bool selected=false,bool hover=false)
        {
            Box(new Rect(r.x+3,r.y+3,r.width-3,r.height-1),new Color(0,0,0,.22f));
            Plate(r,selected?Color.Lerp(fill,Accent,.16f):hover?Color.Lerp(fill,Color.white,.06f):fill,
                selected?Accent:new Color(.42f,.54f,.57f,.55f),5,selected?2:1);
            Box(new Rect(r.x+1,r.y+6,3,r.height-12),rarity);
            Stroke(new Vector2(r.x+7,r.y+1),new Vector2(r.xMax-7,r.y+1),new Color(1,1,1,.10f));
        }
        void TabletArtWell(Rect r)
        {
            Plate(r,new Color(.018f,.034f,.042f,.80f),new Color(.40f,.55f,.59f,.30f),4);
        }
        bool TabletButton(Rect r,string text,bool selected=false,bool commit=false)
        {
            bool hover=r.Contains(Pointer),enabled=GUI.enabled;
            Color fill=commit&&enabled?Accent:selected?new Color(.10f,.28f,.30f):new Color(.13f,.18f,.21f);
            if(hover&&enabled)fill=Color.Lerp(fill,Color.white,.12f);
            Plate(r,enabled?fill:new Color(.10f,.13f,.15f),enabled&&(selected||hover)?Accent:new Color(.35f,.46f,.49f),5,selected?1.5f:1);
            Label(r,text,12,!enabled?new Color(.49f,.57f,.60f):commit?new Color(.025f,.08f,.09f):selected?Accent:white,true,TextAnchor.MiddleCenter);
            return GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
    }
}
