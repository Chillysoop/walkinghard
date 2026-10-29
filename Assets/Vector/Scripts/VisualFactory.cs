using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VectorGame
{
    public static class VisualFactory
    {
        public static Material Hero, Armor, Enemy, EnemyHeavy, Joint, Skin, Glow, RedGlow, GoldGlow, Ground, Stone, Trim, Black, Fx;
        public static readonly Color Cyan = new Color(.25f, .95f, .86f);
        public static readonly Color Orange = new Color(1f, .39f, .16f);
        public static Material Lit(string name, Color color, float metal = 0, Color? emission = null)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.name = name; m.color = color; m.enableInstancing=true; m.SetFloat("_Smoothness", .3f); m.SetFloat("_Metallic", metal);
            if (emission.HasValue) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emission.Value); }
            return m;
        }
        public static void Initialize()
        {
            if (Hero != null) return;
            Hero = Lit("Hero | slate coat", new Color(.20f, .39f, .38f), .15f);
            Armor = Lit("Hero | titanium", new Color(.58f,.74f,.70f), .4f);
            Enemy = Lit("Raider | burnt red", new Color(.48f,.15f,.09f), .15f);
            EnemyHeavy = Lit("Enforcer | sand armor", new Color(.53f,.39f,.22f), .5f);
            Joint = Lit("Undersuit", new Color(.035f,.065f,.075f));
            Skin = Lit("Skin", new Color(.61f,.47f,.34f));
            Glow = Lit("Jade field", Cyan, .25f, Cyan * 2.4f);
            RedGlow = Lit("Enemy optics", Orange, .2f, Orange * 2.4f);
            GoldGlow = Lit("Amber lights", new Color(.9f,.68f,.3f), .2f, new Color(1.6f, .72f,.16f));
            Ground = Lit("Basalt floor", new Color(.13f,.18f,.20f), .25f);
            Stone = Lit("Architectural concrete", new Color(.18f,.23f,.25f), .1f);
            Trim = Lit("Metal trims", new Color(.32f,.39f,.40f), .6f);
            Black = Lit("Black recess", new Color(.028f,.045f,.055f));
            Fx = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            Fx.SetFloat("_Surface", 1); Fx.SetFloat("_Blend", 0); Fx.SetFloat("_ZWrite",0);
            Fx.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha); Fx.SetInt("_DstBlend",(int)BlendMode.One);
            Fx.renderQueue=3000; Fx.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            Fx.color=Cyan;
        }
        public static Transform Part(string name, Transform parent, PrimitiveType shape, Vector3 local, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(shape); go.name = name;
            go.transform.SetParent(parent,false); go.transform.localPosition=local; go.transform.localScale=size;
            var collider=go.GetComponent<Collider>(); if(collider!=null)Object.Destroy(collider);
            var r=go.GetComponent<Renderer>();r.sharedMaterial=material;
            r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
            return go.transform;
        }
        public static Transform Node(string name, Transform parent, Vector3 position)
        { var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t; }
        public static void World(Transform parent)
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.48f,.59f,.63f);
            RenderSettings.ambientEquatorColor=new Color(.27f,.36f,.37f);
            RenderSettings.ambientGroundColor=new Color(.07f,.10f,.12f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.10f,.17f,.20f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.014f;
            var sun=new GameObject("Soft directional key").AddComponent<Light>();sun.transform.SetParent(parent);sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(48,-32,0);sun.color=new Color(.9f,.94f,.88f);sun.intensity=2.2f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.8f;
            var fill=new GameObject("Cool rim").AddComponent<Light>();fill.transform.SetParent(parent);fill.type=LightType.Directional;fill.transform.rotation=Quaternion.Euler(25,155,0);fill.color=new Color(.32f,.63f,.75f);fill.intensity=.65f;
            Part("Endless concourse",parent,PrimitiveType.Cube,new Vector3(0,-.28f,52),new Vector3(26,.5f,155),Ground);
            for(int z=-12;z<130;z+=5)
            {
                Part("Cross joint",parent,PrimitiveType.Cube,new Vector3(0,-.015f,z),new Vector3(17,.018f,.045f),Black);
                for(int side=-1;side<=1;side+=2)
                {
                    Part("Edge channel",parent,PrimitiveType.Cube,new Vector3(side*8.6f,.03f,z),new Vector3(.11f,.05f,4.85f),Glow);
                    Part("Raised service walk",parent,PrimitiveType.Cube,new Vector3(side*10.4f,.15f,z),new Vector3(2.7f,.6f,4.94f),Stone);
                    Part("Kerb",parent,PrimitiveType.Cube,new Vector3(side*9.0f,.19f,z),new Vector3(.17f,.45f,4.9f),Trim);
                    Part("Floor guide",parent,PrimitiveType.Cube,new Vector3(side*2.9f,.008f,z),new Vector3(.035f,.025f,4.5f),Trim);
                    if(z%10==3)
                    {
                        Part("Ribbed column",parent,PrimitiveType.Cube,new Vector3(side*11.8f,3.6f,z),new Vector3(1.35f,7.5f,1.35f),Stone);
                        Part("Column recess",parent,PrimitiveType.Cube,new Vector3(side*11.04f,3.1f,z),new Vector3(.08f,4.7f,.7f),Black);
                        Part("Column light",parent,PrimitiveType.Cube,new Vector3(side*10.98f,3.2f,z),new Vector3(.09f,2.7f,.1f),GoldGlow);
                        Part("Upper beam",parent,PrimitiveType.Cube,new Vector3(side*9.9f,7.25f,z),new Vector3(3.4f,.3f,.7f),Trim);
                    }
                    else if(z%15==8)
                    {
                        Part("Service crate",parent,PrimitiveType.Cube,new Vector3(side*10.1f,.9f,z),new Vector3(1.3f,1.0f,1.5f),Trim);
                        Part("Crate status",parent,PrimitiveType.Cube,new Vector3(side*9.4f,1.0f,z),new Vector3(.03f,.09f,.6f),GoldGlow);
                    }
                }
            }
            // Low borders frame the route without covering the circling enemies.
            Part("Left boundary",parent,PrimitiveType.Cube,new Vector3(-12.4f,1.6f,52),new Vector3(.6f,3.5f,155),Stone);
            Part("Right boundary",parent,PrimitiveType.Cube,new Vector3(12.4f,1.6f,52),new Vector3(.6f,3.5f,155),Stone);
            var volume=new GameObject("Cinematic grade").AddComponent<Volume>();volume.transform.SetParent(parent);volume.isGlobal=true;volume.profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom=volume.profile.Add<Bloom>();bloom.intensity.Override(.28f);bloom.threshold.Override(1.1f);bloom.scatter.Override(.55f);
            var vignette=volume.profile.Add<Vignette>();vignette.intensity.Override(.23f);vignette.smoothness.Override(.7f);
            var tone=volume.profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);
            var color=volume.profile.Add<ColorAdjustments>();color.postExposure.Override(.35f);color.contrast.Override(12);color.saturation.Override(-8);
        }
    }
}
