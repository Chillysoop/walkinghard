using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        RenderTexture portraitTexture;
        Camera portraitCamera;
        FighterRig portrait;
        Transform portraitRoot;
        string portraitLoadout="";
        void Update()
        {
            if(game==null||Application.isBatchMode)return;
            bool visible=game.State==VectorEncounter.RunState.Drafting;
            if(portraitCamera!=null)portraitCamera.enabled=visible;
            if(!visible)return;
            if(portrait==null)
            {
                portraitRoot=new GameObject("Character sheet preview").transform;portraitRoot.SetParent(transform);portraitRoot.position=new Vector3(500,0,0);
                portrait=new FighterRig("Walker equipment preview",portraitRoot,true);portrait.Root.rotation=Quaternion.Euler(0,18,0);
                portraitTexture=new RenderTexture(576,720,24,RenderTextureFormat.ARGB32);portraitTexture.Create();
                portraitCamera=new GameObject("Character portrait camera").AddComponent<Camera>();portraitCamera.transform.SetParent(portraitRoot);
                portraitCamera.transform.position=portraitRoot.position+new Vector3(0,1.5f,5);portraitCamera.transform.LookAt(portraitRoot.position+new Vector3(0,1.12f,0));
                portraitCamera.orthographic=true;portraitCamera.orthographicSize=1.45f;portraitCamera.nearClipPlane=.1f;portraitCamera.farClipPlane=10;
                portraitCamera.backgroundColor=Color.clear;portraitCamera.clearFlags=CameraClearFlags.SolidColor;portraitCamera.targetTexture=portraitTexture;portraitCamera.cullingMask=1<<30;portraitCamera.depth=-20;
                Camera.main.cullingMask&=~(1<<30);
            }
            var d=game.Deck;string key=(d.Weapons[0]?.Name??"fists")+"/"+(d.Weapons[1]?.Name??"fists")+"/"+d.TwoHanded;
            if(portraitLoadout!=key)
            {
                portraitLoadout=key;portrait.PreviewLoadout(d.WeaponFor(d.Basics[0]).def.weapon,d.WeaponFor(d.Basics[1]).def.weapon,d.TwoHanded);
                foreach(var t in portraitRoot.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
            }
        }
        void DrawPortrait(Rect r)
        {
            Color glow=new Color(Accent.r,Accent.g,Accent.b,.5f);
            Vector2 center=new Vector2(r.center.x,r.yMax-18);
            for(int i=0;i<40;i++)
            {
                float a=i*Mathf.PI*2/40,b=(i+1)*Mathf.PI*2/40;
                Stroke(center+new Vector2(Mathf.Cos(a)*r.width*.39f,Mathf.Sin(a)*19),center+new Vector2(Mathf.Cos(b)*r.width*.39f,Mathf.Sin(b)*19),glow,2);
            }
            if(portraitTexture!=null)GUI.DrawTexture(r,portraitTexture,ScaleMode.ScaleToFit,true);
        }
        void OnDestroy()
        {
            if(portraitTexture!=null){portraitTexture.Release();Destroy(portraitTexture);}
        }
    }
}
