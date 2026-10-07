using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AllAboard.Editor
{
    public static class HandOutlineBuilder
    {
        private const string PrefabPath = "Assets/Movement/Prefabs/PrototypePlayer.prefab";
        private const string MaterialPath = "Assets/Movement/Materials/HandOutline.mat";

        [MenuItem("All Aboard/Apply Hand Outlines")]
        public static void Apply()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                AddOutlines(root);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Applied black outlines to the two floating hands.");
        }

        public static void AddOutlines(GameObject player)
        {
            Shader shader = Shader.Find("AllAboard/Hand Outline");
            if (shader == null) throw new InvalidOperationException("Hand outline shader is missing.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                material.SetColor("_OutlineColor", Color.black);
                material.SetFloat("_OutlineWidth", 0.025f);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }
            Transform hands = player.transform.Find("Player Camera/Floating Hands");
            if (hands == null) throw new InvalidOperationException("Player floating hands are missing.");
            foreach (Transform hand in hands)
            {
                MeshFilter source = hand.GetComponent<MeshFilter>();
                if (source == null) continue;
                Transform outline = hand.Find("Black Outline");
                if (outline == null)
                {
                    outline = new GameObject("Black Outline", typeof(MeshFilter), typeof(MeshRenderer)).transform;
                    outline.SetParent(hand, false);
                }
                outline.GetComponent<MeshFilter>().sharedMesh = source.sharedMesh;
                MeshRenderer renderer = outline.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }
    }
}
