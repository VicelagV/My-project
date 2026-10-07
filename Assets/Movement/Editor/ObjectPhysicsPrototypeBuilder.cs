using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using AllAboard.Movement;

namespace AllAboard.Editor
{
    public static class ObjectPhysicsPrototypeBuilder
    {
        private const string Root = "Assets/Movement";
        private const string PlayerPath = Root + "/Prefabs/PrototypePlayer.prefab";
        private const string CubePath = Root + "/Prefabs/BluePhysicsCube.prefab";

        [MenuItem("All Aboard/Set Up Object Physics Prototype")]
        public static void Apply()
        {
            HandOutlineBuilder.Apply();
            GameObject player = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                SetupPlayer(player, player.transform.Find("Player Camera"));
                PrefabUtility.SaveAsPrefabAsset(player, PlayerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }

            GameObject cubePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CubePath);
            if (cubePrefab == null) cubePrefab = CreateCube();
            Scene previous = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && string.IsNullOrEmpty(previous.path) && previous.isDirty)
                throw new InvalidOperationException("Save the current untitled scene before setting up object physics.");
            bool additive = !string.IsNullOrEmpty(previous.path) && previous.path != MovementPrototypeBuilder.ScenePath;
            Scene scene = previous.path == MovementPrototypeBuilder.ScenePath ? previous :
                EditorSceneManager.OpenScene(MovementPrototypeBuilder.ScenePath, additive ? OpenSceneMode.Additive : OpenSceneMode.Single);
            bool found = false;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.GetComponent<PickupObject>() != null) found = true;
            if (!found)
            {
                var cube = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab, scene);
                cube.transform.position = new Vector3(0f, 0.3f, -6.5f);
                EditorSceneManager.SaveScene(scene);
            }
            if (additive) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            Debug.Log("Object physics prototype ready: blue cube, pickup, charged throw, sprint.");
        }

        public static void SetupPlayer(GameObject player, Transform camera)
        {
            var interaction = player.GetComponent<PlayerInteraction>();
            if (interaction == null) interaction = player.AddComponent<PlayerInteraction>();
            interaction.Configure(camera);
        }

        private static GameObject CreateCube()
        {
            var blue = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            blue.color = new Color(0.05f, 0.3f, 0.95f);
            AssetDatabase.CreateAsset(blue, Root + "/Materials/BlueCube.mat");
            var white = new Material(Shader.Find("AllAboard/Hand Outline"));
            white.SetColor("_OutlineColor", Color.white);
            // A scaled shell keeps hard cube corners connected.
            white.SetFloat("_OutlineWidth", 0f);
            AssetDatabase.CreateAsset(white, Root + "/Materials/HoverOutline.mat");
            var physics = new PhysicsMaterial("ObjectPhysics")
            {
                staticFriction = 0.6f,
                dynamicFriction = 0.5f,
                bounciness = 0.05f,
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            AssetDatabase.CreateAsset(physics, Root + "/Materials/ObjectPhysics.physicMaterial");
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Blue Physics Cube";
            cube.transform.localScale = Vector3.one * 0.5f;
            cube.GetComponent<MeshRenderer>().sharedMaterial = blue;
            cube.GetComponent<BoxCollider>().sharedMaterial = physics;
            Rigidbody body = cube.AddComponent<Rigidbody>();
            body.mass = 2f;
            body.linearDamping = 0.1f;
            body.angularDamping = 0.5f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            GameObject outline = new GameObject("White Hover Outline", typeof(MeshFilter), typeof(MeshRenderer));
            outline.transform.SetParent(cube.transform, false);
            outline.transform.localScale = Vector3.one * 1.05f;
            outline.GetComponent<MeshFilter>().sharedMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            var renderer = outline.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = white;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.enabled = false;
            cube.AddComponent<PickupObject>().Configure(renderer);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cube, CubePath);
            UnityEngine.Object.DestroyImmediate(cube);
            return prefab;
        }
    }
}
