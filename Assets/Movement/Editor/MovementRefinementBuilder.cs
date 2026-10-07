using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AllAboard.Editor
{
    public static class MovementRefinementBuilder
    {
        [MenuItem("All Aboard/Add Crouch Test Tunnel")]
        public static void Apply()
        {
            const string prefabPath = "Assets/Movement/Prefabs/PrototypePlayer.prefab";
            GameObject player = PrefabUtility.LoadPrefabContents(prefabPath);
            try { PrefabUtility.SaveAsPrefabAsset(player, prefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(player); }

            Scene previous = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && string.IsNullOrEmpty(previous.path) && previous.isDirty)
            {
                Debug.LogWarning("Save the current untitled scene before adding the crouch tunnel.");
                return;
            }
            bool additive = !string.IsNullOrEmpty(previous.path) && previous.path != MovementPrototypeBuilder.ScenePath;
            Scene scene = previous.path == MovementPrototypeBuilder.ScenePath ? previous :
                EditorSceneManager.OpenScene(MovementPrototypeBuilder.ScenePath,
                    additive ? OpenSceneMode.Additive : OpenSceneMode.Single);
            bool exists = false;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Crouch Test Tunnel") exists = true;
            if (!exists)
            {
                var root = new GameObject("Crouch Test Tunnel");
                SceneManager.MoveGameObjectToScene(root, scene);
                root.transform.position = new Vector3(5f, 0f, -4f);
                Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Movement/Materials/Obstacles.mat");
                Part(root.transform, "Low Roof", new Vector3(0f, 1.45f, 0f), new Vector3(3f, 0.3f, 3f), material);
                Part(root.transform, "Left Side", new Vector3(-1.35f, 0.65f, 0f), new Vector3(0.3f, 1.3f, 3f), material);
                Part(root.transform, "Right Side", new Vector3(1.35f, 0.65f, 0f), new Vector3(0.3f, 1.3f, 3f), material);
                EditorSceneManager.SaveScene(scene);
            }
            if (additive) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            Debug.Log("Movement refinement ready: inertia settings and crouch test tunnel.");
        }

        private static void Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}
