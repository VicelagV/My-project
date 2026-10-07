using System.IO;
using AllAboard.Movement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AllAboard.Editor
{
    public static class MovementPrototypeBuilder
    {
        private const string Root = "Assets/Movement";
        public const string ScenePath = Root + "/Scenes/MovementPlayground.unity";

        [MenuItem("All Aboard/Create Movement Playground")]
        public static void Create()
        {
            // Existing scenes and prefabs may contain hand edits; never overwrite them.
            if (File.Exists(ScenePath))
            {
                Debug.Log("Movement playground already exists: " + ScenePath);
                return;
            }
            Directory.CreateDirectory(Root + "/Scenes");
            Directory.CreateDirectory(Root + "/Prefabs");
            Directory.CreateDirectory(Root + "/Materials");
            AssetDatabase.Refresh();
            Scene previous = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && string.IsNullOrEmpty(previous.path) && previous.isDirty)
            {
                Debug.LogWarning("Save the current untitled scene before creating the playground.");
                return;
            }
            bool additive = !string.IsNullOrEmpty(previous.path);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                additive ? NewSceneMode.Additive : NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);

            Material floor = Material("Floor", new Color(0.19f, 0.24f, 0.28f));
            Material obstacle = Material("Obstacles", new Color(0.36f, 0.58f, 0.61f));
            Material hands = Material("Hands", new Color(1f, 0.69f, 0.25f));
            Box("Floor", new Vector3(0f, -0.25f, 0f), new Vector3(24f, 0.5f, 30f), floor);
            Box("Back wall", new Vector3(0f, 1.5f, 14.5f), new Vector3(24f, 3f, 0.5f), obstacle);
            Box("Left wall", new Vector3(-11.75f, 1.5f, 0f), new Vector3(0.5f, 3f, 30f), obstacle);
            Box("Right wall", new Vector3(11.75f, 1.5f, 0f), new Vector3(0.5f, 3f, 30f), obstacle);
            for (int i = 0; i < 4; i++)
            {
                float height = 0.2f * (i + 1);
                Box("Step " + (i + 1), new Vector3(-4f, height / 2f, i * 0.7f), new Vector3(3f, height, 0.7f), obstacle);
            }
            Box("Jump block", new Vector3(0f, 0.5f, 3f), new Vector3(2f, 1f, 2f), obstacle);
            Box("Landing block", new Vector3(0f, 0.5f, 6.5f), new Vector3(2f, 1f, 2f), obstacle);
            GameObject ramp = Box("Ramp", new Vector3(5f, 0.5f, 3f), new Vector3(3f, 0.25f, 5f), obstacle);
            ramp.transform.rotation = Quaternion.Euler(-15f, 0f, 0f);
            Box("Ceiling jump test", new Vector3(-4f, 2.4f, -5f), new Vector3(3f, 0.3f, 3f), obstacle);

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 2f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.5f, 0.55f);

            const string prefabPath = Root + "/Prefabs/PrototypePlayer.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                GameObject player = new GameObject("PrototypePlayer");
                var controller = player.AddComponent<CharacterController>();
                controller.height = 1.8f;
                controller.radius = 0.3f;
                controller.center = new Vector3(0f, 0.9f, 0f);
                controller.stepOffset = 0.3f;
                controller.slopeLimit = 45f;
                controller.skinWidth = 0.03f;
                controller.minMoveDistance = 0f;
                player.AddComponent<PlayerInputReader>();
                player.AddComponent<FirstPersonMotor>();
                var camera = new GameObject("Player Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.transform.SetParent(player.transform, false);
                camera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                camera.fieldOfView = 75f;
                camera.nearClipPlane = 0.03f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.12f, 0.17f, 0.23f);
                camera.gameObject.AddComponent<AudioListener>();
                var handRoot = new GameObject("Floating Hands").transform;
                handRoot.SetParent(camera.transform, false);
                for (int i = 0; i < 2; i++)
                {
                    GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    hand.name = i == 0 ? "Left Hand" : "Right Hand";
                    Object.DestroyImmediate(hand.GetComponent<Collider>());
                    hand.transform.SetParent(handRoot, false);
                    hand.transform.localPosition = new Vector3(i == 0 ? -0.25f : 0.25f, -0.2f, 0.5f);
                    hand.transform.localScale = Vector3.one * 0.16f;
                    var renderer = hand.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = hands;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
                player.AddComponent<FirstPersonView>().Configure(camera.transform, handRoot);
                HandOutlineBuilder.AddOutlines(player);
                ObjectPhysicsPrototypeBuilder.SetupPlayer(player, camera.transform);
                prefab = PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
                Object.DestroyImmediate(player);
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.position = new Vector3(0f, 0.1f, -9f);
            EditorSceneManager.SaveScene(scene, ScenePath);
            if (additive)
            {
                EditorSceneManager.CloseScene(scene, true);
                SceneManager.SetActiveScene(previous);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Created movement playground: " + ScenePath);
            ObjectPhysicsPrototypeBuilder.Apply();
        }

        private static Material Material(string name, Color color)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            return box;
        }
    }
}
