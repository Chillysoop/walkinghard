using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VectorGame.Editor
{
    public static class PrototypeBuilder
    {
        [MenuItem("VECTOR/Generate scene and settings")]
        public static void Generate()
        {
            Directory.CreateDirectory("Assets/Vector/Scenes");Directory.CreateDirectory("Assets/Resources");
            var config=AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/Resources/CombatTuning.asset");
            if(config==null){config=ScriptableObject.CreateInstance<CombatTuning>();AssetDatabase.CreateAsset(config,"Assets/Resources/CombatTuning.asset");}
            // Referenced materials prevent runtime shader discovery from relying on unreferenced variants.
            foreach(var shaderName in new[]{"Universal Render Pipeline/Lit","Universal Render Pipeline/Particles/Unlit"})
            {
                string path="Assets/Resources/"+(shaderName.EndsWith("Lit")?"SurfaceShader":"EffectShader")+".mat";
                if(AssetDatabase.LoadAssetAtPath<Material>(path)==null){var shader=Shader.Find(shaderName);if(shader==null)throw new System.Exception("Missing shader: "+shaderName);var mat=new Material(shader);mat.enableInstancing=true;if(shaderName.Contains("Particles")){mat.SetFloat("_Surface",1);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");}else{mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",Color.cyan*2);}AssetDatabase.CreateAsset(mat,path);}
            }
            foreach(string name in new[]{"Equipment","Actions","Augments"})
            {
                string path="Assets/Resources/ItemArt/"+name+".png";
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)throw new System.Exception("Missing item artwork: "+path);
                if(importer.mipmapEnabled||importer.npotScale!=TextureImporterNPOTScale.None||importer.textureCompression!=TextureImporterCompression.Uncompressed)
                {importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();}
            }
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
            if(pipeline==null)throw new System.Exception("URP PC asset missing.");
            pipeline.shadowDistance=45;pipeline.msaaSampleCount=4;pipeline.renderScale=1f;pipeline.supportsHDR=true;
            GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
            QualitySettings.shadows=UnityEngine.ShadowQuality.All;QualitySettings.shadowResolution=UnityEngine.ShadowResolution.High;QualitySettings.vSyncCount=1;
            PlayerSettings.companyName="Banjo Gaming";PlayerSettings.productName="Walking Hard";PlayerSettings.bundleVersion="0.6.9";
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
            PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            var playerSettings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input=playerSettings.FindProperty("activeInputHandler");if(input!=null)input.intValue=0;playerSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorSettings.serializationMode=SerializationMode.ForceText;
            EditorUtility.SetDirty(pipeline);AssetDatabase.SaveAssets();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            config=AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/Resources/CombatTuning.asset");
            if(config==null)throw new System.Exception("Combat tuning asset did not persist.");
            var root=new GameObject("VECTOR | encounter controller");root.AddComponent<VectorEncounter>().tuning=config;
            string scenePath="Assets/Vector/Scenes/CombatStudy.unity";EditorSceneManager.SaveScene(scene,scenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
            AssetDatabase.SaveAssets();
            Debug.Log("VECTOR scene and settings generated.");
        }
        [MenuItem("VECTOR/Build Windows prototype")]
        public static void BuildWindows()
        {
            Generate();string output="Build/Windows";
            var args=System.Environment.GetCommandLineArgs();int index=System.Array.IndexOf(args,"-walkingHardOutput");
            if(index>=0&&index+1<args.Length)output=args[index+1];
            Directory.CreateDirectory(output);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Vector/Scenes/CombatStudy.unity"},locationPathName=Path.Combine(output,"WalkingHard.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("VECTOR build failed: "+report.summary.result);
            File.WriteAllText(Path.Combine(output,"simulation-capabilities.json"),"{\"schema\":1,\"version\":\""+PlayerSettings.bundleVersion+"\",\"fixedStepHz\":60,\"physics\":\"PhysX\",\"modes\":[\"Full\",\"Fast\"]}");
            Debug.Log("VECTOR_BUILD_SUCCESS "+report.summary.totalSize+" bytes in "+report.summary.totalTime);
        }
    }
}
