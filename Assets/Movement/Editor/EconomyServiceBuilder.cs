using System.Linq;
using AllAboard.Train;
using UnityEditor;
using UnityEngine;
namespace AllAboard.Editor
{
    public static class EconomyServiceBuilder
    {
        [MenuItem("All Aboard/Apply Economy Service Configuration")]
        public static void Apply()
        {
            var config=AssetDatabase.LoadAssetAtPath<EconomyConfig>("Assets/Movement/Resources/EconomyDefaults.asset");
            if(config==null)throw new System.InvalidOperationException("Build the bright character/economy defaults first.");
            EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();
            string path="Assets/Movement/Scenes/ProceduralRailwayPlayground.unity";
            if(!EditorBuildSettings.scenes.Any(s=>s.path==path))EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(path,true)}).ToArray();
            Debug.Log("ECONOMY SERVICE configuration applied");
        }
    }
}
