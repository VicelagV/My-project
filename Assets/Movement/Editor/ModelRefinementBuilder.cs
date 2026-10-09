using System.Linq;
using AllAboard.Train;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AllAboard.Editor
{
    public static class ModelRefinementBuilder
    {
        [MenuItem("All Aboard/Refine All Model Details")]
        public static void Apply()
        {
            PlayerAvatarBuilder.RefineModels();
            var names=new[]{"TrainNavy","TrainBrass","TrainMachinedSteel","TrainWalnut","TrainUpholstery","TrainIvory","TrainOxideRed"};
            var paints=names.Select(n=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/"+n+".mat")).ToArray();
            foreach(var paint in paints)if(paint==null)throw new System.InvalidOperationException("Build the base train appearance first.");
            foreach(var kind in new[]{"Locomotive","Passenger","CoalWagon"})
            {
                string path="Assets/Movement/Prefabs/Train"+kind+".prefab";
                var prefab=PrefabUtility.LoadPrefabContents(path);
                try { Decorate(prefab.GetComponent<TrainCar>(),paints); PrefabUtility.SaveAsPrefabAsset(prefab,path); }
                finally { PrefabUtility.UnloadPrefabContents(prefab); }
            }
            foreach(var path in new[]{"Assets/Movement/Scenes/TrainPlayground.unity","Assets/Movement/Scenes/ProceduralRailwayPlayground.unity"})
            {
                var scene=EditorSceneManager.OpenScene(path);
                foreach(var car in Object.FindObjectsByType<TrainCar>(FindObjectsInactive.Include))Decorate(car,paints);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();Debug.Log("MODEL REFINEMENT assets built successfully");
        }
        private static void Decorate(TrainCar car,Material[] paints)
        {
            var detail=car.GetComponent<TrainCraftsmanship>();
            if(detail==null)detail=car.gameObject.AddComponent<TrainCraftsmanship>();
            detail.Configure(paints);EditorUtility.SetDirty(detail);
            if(PrefabUtility.IsPartOfPrefabInstance(detail))PrefabUtility.RecordPrefabInstancePropertyModifications(detail);
        }
    }
}
