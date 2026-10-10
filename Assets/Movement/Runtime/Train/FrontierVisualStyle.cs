using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AllAboard.Train
{
    // Runtime material copies keep imported assets and interaction hulls intact.
    public sealed class FrontierVisualStyle : MonoBehaviour
    {
        private readonly Dictionary<Material,Material> copies = new Dictionary<Material,Material>();
        private readonly HashSet<Material> owned = new HashSet<Material>();
        private VolumeProfile profile;
        private Material sky, oldSkybox;
        private bool prepared, oldFog;
        private FogMode oldFogMode;
        private Color oldFogColour, oldAmbient, oldSky, oldEquator, oldGround;
        private float oldFogStart, oldFogEnd;
        private AmbientMode oldAmbientMode;
        public bool Ready => prepared;

        public Material[] Prepare(Material[] palette)
        {
            if(!prepared)
            {
                oldSkybox=RenderSettings.skybox;oldFog=RenderSettings.fog;oldFogMode=RenderSettings.fogMode;oldFogColour=RenderSettings.fogColor;
                oldFogStart=RenderSettings.fogStartDistance;oldFogEnd=RenderSettings.fogEndDistance;
                oldAmbientMode=RenderSettings.ambientMode;oldAmbient=RenderSettings.ambientLight;
                oldSky=RenderSettings.ambientSkyColor;oldEquator=RenderSettings.ambientEquatorColor;oldGround=RenderSettings.ambientGroundColor;
                prepared=true;
            }
            var styledPalette=palette.Select(Style).ToArray();
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.72f,.83f,.85f);
            RenderSettings.fogStartDistance=60;RenderSettings.fogEndDistance=480;
            if(sky==null)
            {
                sky=new Material(Shader.Find("Skybox/Procedural")){name="Frontier Hazy Sky"};
                sky.SetColor("_SkyTint",new Color(.47f,.64f,.77f));sky.SetColor("_GroundColor",new Color(.57f,.64f,.58f));
                sky.SetFloat("_AtmosphereThickness",.75f);sky.SetFloat("_Exposure",1.2f);sky.SetFloat("_SunSize",.035f);
            }
            RenderSettings.skybox=sky;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.60f,.72f,.80f);
            RenderSettings.ambientEquatorColor=new Color(.48f,.55f,.57f);
            RenderSettings.ambientGroundColor=new Color(.29f,.31f,.27f);
            foreach(var root in gameObject.scene.GetRootGameObjects())
            {
                foreach(var sun in root.GetComponentsInChildren<Light>().Where(l=>l.type==LightType.Directional))
                {
                    sun.transform.rotation=Quaternion.Euler(32,-35,0);sun.color=new Color(1,.90f,.76f);
                    sun.intensity=1.35f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.78f;
                    sun.shadowBias=.04f;sun.shadowNormalBias=.3f;
                }
                foreach(var camera in root.GetComponentsInChildren<Camera>())
                {
                    camera.backgroundColor=RenderSettings.fogColor;camera.clearFlags=CameraClearFlags.Skybox;
                    var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;
                    data.antialiasing=AntialiasingMode.FastApproximateAntialiasing;
                }
            }
            if(profile==null)
            {
                profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name="Frontier Soft Daylight";
                var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.Neutral);
                var colour=profile.Add<ColorAdjustments>(true);colour.postExposure.Override(.3f);colour.contrast.Override(3);colour.saturation.Override(-5);
                var bloom=profile.Add<Bloom>(true);bloom.threshold.Override(1.15f);bloom.intensity.Override(.12f);bloom.scatter.Override(.45f);
                var volume=gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.priority=5;volume.sharedProfile=profile;
                if(!Application.isPlaying){profile.hideFlags=HideFlags.DontSaveInEditor;volume.hideFlags=HideFlags.DontSaveInEditor;}
            }
            return styledPalette;
        }
        public void ApplyTrain(TrainController train)
        {
            foreach(var car in train.Cars)
                foreach(var draw in car.GetComponentsInChildren<Renderer>(true))
                    draw.sharedMaterials=draw.sharedMaterials.Select(Style).ToArray();
        }
        private Material Style(Material source)
        {
            if(source==null||owned.Contains(source))return source;
            if(copies.TryGetValue(source,out var cached))return cached;
            if(source.shader==null||source.shader.name!="AllAboard/Stylized Train")return source;
            var paint=new Material(source){name=source.name+" Frontier"};copies[source]=paint;owned.Add(paint);
            string name=source.name.ToLowerInvariant();string texture=null;
            if(name.Contains("earth"))texture="Earth";
            else if(name.Contains("ballast"))texture="Gravel";
            else if(name.Contains("walnut"))texture="Wood";
            else if(name.Contains("foliage"))texture="Foliage";
            else if(name.Contains("steel")||name.Contains("navy"))texture="Metal";
            if(texture!=null)paint.SetTexture("_BaseMap",Resources.Load<Texture2D>("FrontierTextures/"+texture));
            paint.SetFloat("_GrainScale",texture=="Metal"?2: .7f);
            if(name.Contains("navy"))paint.SetColor("_BaseColor",new Color(.075f,.13f,.20f));
            else if(name.Contains("brass"))paint.SetColor("_BaseColor",new Color(.72f,.53f,.22f));
            else if(name.Contains("oxidered"))paint.SetColor("_BaseColor",new Color(.58f,.13f,.11f));
            else if(name.Contains("walnut"))paint.SetColor("_BaseColor",new Color(.30f,.19f,.12f));
            else if(name.Contains("foliage"))paint.SetColor("_BaseColor",new Color(.26f,.39f,.22f));
            return paint;
        }
        private void OnDestroy()
        {
            if(prepared)
            {
                RenderSettings.skybox=oldSkybox;RenderSettings.fog=oldFog;RenderSettings.fogMode=oldFogMode;RenderSettings.fogColor=oldFogColour;
                RenderSettings.fogStartDistance=oldFogStart;RenderSettings.fogEndDistance=oldFogEnd;
                RenderSettings.ambientMode=oldAmbientMode;RenderSettings.ambientLight=oldAmbient;
                RenderSettings.ambientSkyColor=oldSky;RenderSettings.ambientEquatorColor=oldEquator;RenderSettings.ambientGroundColor=oldGround;
            }
            if(sky!=null)CraftGeometry.Dispose(sky);
            foreach(var paint in owned)if(paint!=null)CraftGeometry.Dispose(paint);
            if(profile!=null){foreach(var setting in profile.components)CraftGeometry.Dispose(setting);CraftGeometry.Dispose(profile);}
        }
    }
}
