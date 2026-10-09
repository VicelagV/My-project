using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace AllAboard.Train
{
    // Decorative geometry never changes the interaction or collision hulls.
    public static class CraftGeometry
    {
        public static Transform Group(Transform parent, string name)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            return root;
        }

        public static GameObject Box(Transform parent, string name, Vector3 at, Vector3 size, Material material)
            => RouteProps.Box(parent, name, at, size, material, false);

        public static GameObject Round(Transform parent, string name, Vector3 at, Vector3 size, Material material, Quaternion rotation)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.name = name; part.transform.SetParent(parent, false);
            part.transform.localPosition = at; part.transform.localScale = size; part.transform.localRotation = rotation;
            part.GetComponent<Renderer>().sharedMaterial = material;
            var collider = part.GetComponent<Collider>(); collider.enabled = false; Dispose(collider);
            return part;
        }

        public static void Rod(Transform parent, string name, Vector3 start, Vector3 end, float width, Material material)
        {
            var delta = end - start;
            Round(parent, name, (start + end) * .5f, new Vector3(width, delta.magnitude * .5f, width), material,
                Quaternion.FromToRotation(Vector3.up, delta.normalized));
        }

        public static void Bolt(Transform parent, Vector3 at, float diameter, Material material, Vector3 normal)
        {
            Round(parent, "Craft Bolt Head", at, new Vector3(diameter, diameter * .18f, diameter), material,
                Quaternion.FromToRotation(Vector3.up, normal));
        }

        public static void Frame(Transform parent, Vector3 center, Vector2 size, float thickness, Material material)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Box(parent, "Craft Frame Stile", center + Vector3.right * side * size.x * .5f,
                    new Vector3(thickness, size.y + thickness, thickness), material);
                Box(parent, "Craft Frame Rail", center + Vector3.up * side * size.y * .5f,
                    new Vector3(size.x, thickness, thickness), material);
            }
        }

        public static void Crate(Transform parent, Vector3 size, Material[] m)
        {
            var group = Group(parent, "Craft Crate Joinery");
            for (int i = 1; i < 5; i++)
            {
                float y = -size.y * .5f + size.y * i / 5f;
                foreach (float side in new[] { -1f, 1f })
                {
                    Box(group, "Crate Plank Joint", new Vector3(0, y, side * (size.z * .5f + .002f)), new Vector3(size.x, .008f, .004f), m[5]);
                    Box(group, "Crate Side Joint", new Vector3(side * (size.x * .5f + .002f), y, 0), new Vector3(.004f, .008f, size.z), m[5]);
                }
            }
            foreach (float side in new[] { -1f, 1f }) foreach (float x in new[] { -.36f, .36f })
            {
                Box(group, "Crate Corner Batten", new Vector3(x * size.x, 0, side * (size.z * .5f + .013f)), new Vector3(.035f, size.y, .022f), m[2]);
                foreach (float y in new[] { -.36f, .36f }) Bolt(group, new Vector3(x * size.x, y * size.y, side * (size.z * .5f + .029f)), .022f, m[3], Vector3.forward * side);
            }
            foreach (float side in new[] { -1f, 1f })
            {
                Rod(group, "Crate Carry Handle", new Vector3(side * (size.x * .5f + .012f), -.055f, -.075f),
                    new Vector3(side * (size.x * .5f + .012f), -.055f, .075f), .025f, m[3]);
            }
            Bake(group);
        }

        // Combine only the new static decorations, never control pivots, lights or hover outlines.
        public static void Bake(Transform root)
        {
            var filters = root.GetComponentsInChildren<MeshFilter>();
            var owner = root.gameObject.AddComponent<CraftMeshOwner>();
            foreach (var materialGroup in filters.Where(f => f.sharedMesh != null).GroupBy(f => f.GetComponent<Renderer>().sharedMaterial))
            {
                var parts = materialGroup.Select(f => new CombineInstance { mesh = f.sharedMesh,
                    transform = root.worldToLocalMatrix * f.transform.localToWorldMatrix }).ToArray();
                var mesh = new Mesh { name = root.name + " " + materialGroup.Key.name, indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(parts, true, true); mesh.RecalculateBounds(); owner.Own(mesh);
                var draw = new GameObject("Craft Batch " + materialGroup.Key.name);
                draw.transform.SetParent(root, false); draw.AddComponent<MeshFilter>().sharedMesh = mesh;
                draw.AddComponent<MeshRenderer>().sharedMaterial = materialGroup.Key;
            }
            foreach (var filter in filters) { filter.gameObject.SetActive(false); Dispose(filter.gameObject); }
        }

        public static void Dispose(Object value) { if (Application.isPlaying) Object.Destroy(value); else Object.DestroyImmediate(value); }
    }

    public sealed class CraftMeshOwner : MonoBehaviour
    {
        private readonly List<Mesh> meshes = new List<Mesh>();
        public void Own(Mesh mesh) => meshes.Add(mesh);
        private void OnDestroy() { foreach (var mesh in meshes) if (mesh != null) CraftGeometry.Dispose(mesh); }
    }
}
