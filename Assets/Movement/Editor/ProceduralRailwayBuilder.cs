using System.Linq;
using AllAboard.Movement;
using AllAboard.Train;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AllAboard.Editor
{
    public static class ProceduralRailwayBuilder
    {
        public const string ScenePath="Assets/Movement/Scenes/ProceduralRailwayPlayground.unity";
        [MenuItem("All Aboard/Create Procedural Railway Playground")]
        public static void Create()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)!=null){Debug.Log("Procedural playground already exists; preserving edits.");return;}
            if(!AssetDatabase.CopyAsset(TrainPrototypeBuilder.ScenePath,ScenePath))throw new System.InvalidOperationException("Could not copy the existing train setup.");
            var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.OpenScene(ScenePath,Application.isBatchMode?OpenSceneMode.Single:OpenSceneMode.Additive);SceneManager.SetActiveScene(scene);
            var roots=scene.GetRootGameObjects();var train=roots.Select(x=>x.GetComponent<TrainController>()).First(x=>x!=null);
            var path=roots.Select(x=>x.GetComponent<RailPath>()).First(x=>x!=null);
            var player=roots.Select(x=>x.GetComponent<FirstPersonMotor>()).First(x=>x!=null);
            foreach(var root in roots)
            {
                bool keep=root==train.gameObject||root==path.gameObject||root==player.gameObject||train.Cars.Any(x=>x.gameObject==root)||root.GetComponent<Light>()!=null;
                if(!keep)Object.DestroyImmediate(root);
            }
            path.name="Active Railway Itinerary";train.name="Procedural Train Systems";
            var materials=new[]{Paint("RouteEarth",new Color(0.29f,0.35f,0.23f)),Paint("RouteBallast",new Color(0.29f,0.30f,0.29f)),
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/TrainWalnut.mat"),AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/TrainMachinedSteel.mat"),
                Paint("RouteFoliage",new Color(0.25f,0.39f,0.18f)),AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/TrainNavy.mat"),
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/TrainBrass.mat"),AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/TrainOxideRed.mat"),
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/TrainIvory.mat"),AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/HoverOutline.mat")};
            train.gameObject.AddComponent<ProceduralRailway>().Configure(train,path,AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Movement/Prefabs/Coal.prefab"),materials);
            player.transform.position=new Vector3(4.8f,1.2f,1.5f);player.transform.rotation=Quaternion.Euler(0f,-90f,0f);
            var camera=player.GetComponentInChildren<Camera>();camera.farClipPlane=1200f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(0.46f,0.59f,0.64f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=400f;RenderSettings.fogEndDistance=1150f;RenderSettings.fogColor=camera.backgroundColor;
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(!Application.isBatchMode){EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);}
            Debug.Log("Created separate seeded railway playground. World geometry builds at startup.");
        }
        [MenuItem("All Aboard/Preview Current Procedural Railway")]
        public static void Preview()
        {
            var world=Object.FindFirstObjectByType<ProceduralRailway>();if(world==null){Debug.LogWarning("Open ProceduralRailwayPlayground first.");return;}
            world.BuildInitialWorld();SceneView.RepaintAll();
        }
        private static Material Paint(string name,Color color)
        {
            string path="Assets/Movement/Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material!=null)return material;material=new Material(Shader.Find("AllAboard/Stylized Train"));material.SetColor("_BaseColor",color);material.SetColor("_EmissionColor",Color.black);AssetDatabase.CreateAsset(material,path);return material;
        }
    }
}
