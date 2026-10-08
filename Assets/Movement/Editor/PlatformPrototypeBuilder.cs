using AllAboard.Movement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AllAboard.Editor
{
    public static class PlatformPrototypeBuilder
    {
        public const string PrefabPath = "Assets/Movement/Prefabs/MovingTestPlatform.prefab";

        [MenuItem("All Aboard/Add Moving Platform Test")]
        public static void Apply()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                const string materialPath = "Assets/Movement/Materials/MovingPlatform.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    material.color = new Color(0.92f, 0.51f, 0.19f);
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                var root = new GameObject("Moving Test Platform");
                root.AddComponent<MovingPlatform>();
                Part(root.transform, "Deck", Vector3.zero, new Vector3(6f, 0.4f, 4f), material);
                // A low roof gives crouching and carrying a second clearance test in motion.
                Part(root.transform, "Low Roof", new Vector3(-1.6f, 1.65f, 0f), new Vector3(1.8f, 0.3f, 2f), material);
                Part(root.transform, "Outer Roof Support", new Vector3(-2.4f, 0.85f, 0f), new Vector3(0.2f, 1.3f, 2f), material);
                prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Object.DestroyImmediate(root);
            }
            Scene previous = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && string.IsNullOrEmpty(previous.path) && previous.isDirty)
            {
                Debug.LogWarning("Save the current untitled scene before adding the moving platform.");
                return;
            }
            bool additive = !string.IsNullOrEmpty(previous.path) && previous.path != MovementPrototypeBuilder.ScenePath;
            Scene scene = previous.path == MovementPrototypeBuilder.ScenePath ? previous :
                EditorSceneManager.OpenScene(MovementPrototypeBuilder.ScenePath, additive ? OpenSceneMode.Additive : OpenSceneMode.Single);
            bool exists = false;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.GetComponent<MovingPlatform>() != null) exists = true;
            if (!exists)
            {
                var platform = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                platform.transform.position = new Vector3(0f, 0.65f, 10.5f);
                var cube = (GameObject)PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Movement/Prefabs/BluePhysicsCube.prefab"), scene);
                cube.name = "Platform Physics Cube";
                cube.transform.position = new Vector3(1f, 1.15f, 10.5f);
                EditorSceneManager.SaveScene(scene);
            }
            if (additive) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            Debug.Log("Moving platform ready: orange deck beyond the jump blocks, with a loose cube and low roof.");
        }

        private static void Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
            part.GetComponent<Collider>().sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(
                "Assets/Movement/Materials/ObjectPhysics.physicMaterial");
        }
    }
}
