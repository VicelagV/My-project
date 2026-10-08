using System.Collections.Generic;
using UnityEngine;

namespace AllAboard.Movement
{
    public sealed class ControlHighlight : MonoBehaviour
    {
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Renderer[] targets;
        private readonly List<Renderer> outlines = new List<Renderer>();
        public bool Highlighted { get; private set; }
        public void Configure(Material material, Renderer[] geometry) { outlineMaterial = material; targets = geometry; }
        private void Awake()
        {
            if (outlineMaterial == null || targets == null) return;
            foreach (Renderer target in targets)
            {
                if (target == null || target.GetComponent<MeshFilter>() == null) continue;
                var shell = new GameObject("Control Hover Outline");
                shell.transform.SetParent(target.transform, false);
                shell.transform.localScale = Vector3.one * 1.08f;
                shell.AddComponent<MeshFilter>().sharedMesh = target.GetComponent<MeshFilter>().sharedMesh;
                var renderer = shell.AddComponent<MeshRenderer>(); renderer.sharedMaterial = outlineMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.enabled = false; outlines.Add(renderer);
            }
        }
        public void SetHighlighted(bool highlighted)
        { Highlighted = highlighted; foreach (Renderer outline in outlines) if (outline != null) outline.enabled = highlighted; }
        private void OnDisable() => SetHighlighted(false);
    }
}
