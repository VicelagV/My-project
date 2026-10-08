using UnityEngine;

namespace AllAboard.Train
{
    public sealed class RailPath : MonoBehaviour
    {
        [SerializeField] private Vector3[] points;
        [SerializeField, Range(8, 128)] private int samplesPerSection = 64;
        private Vector3[] samples;
        private float[] distances;
        public float Length { get { EnsureCache(); return distances[distances.Length - 1]; } }
        public void Configure(Vector3[] nodes) { points = nodes; Rebuild(); }
        private void Awake() => Rebuild();
        private void OnValidate() { samples = null; }
        private void EnsureCache() { if (samples == null) Rebuild(); }
        private void Rebuild()
        {
            if (points == null || points.Length < 2) points = new[] { Vector3.zero, Vector3.forward * 300f };
            int count = (points.Length - 1) * samplesPerSection + 1;
            samples = new Vector3[count]; distances = new float[count];
            for (int i = 0; i < count; i++)
            {
                int section = Mathf.Min(i / samplesPerSection, points.Length - 2);
                float t = (float)(i - section * samplesPerSection) / samplesPerSection;
                Vector3 b = points[section], c = points[section + 1];
                Vector3 a = section > 0 ? points[section - 1] : b * 2f - c;
                Vector3 d = section + 2 < points.Length ? points[section + 2] : c * 2f - b;
                samples[i] = 0.5f * ((2f * b) + (-a + c) * t + (2f * a - 5f * b + 4f * c - d) * t * t + (-a + 3f * b - 3f * c + d) * t * t * t);
                if (i > 0) distances[i] = distances[i - 1] + Vector3.Distance(samples[i], samples[i - 1]);
            }
        }
        public void Sample(float distance, out Vector3 position, out Quaternion rotation)
        {
            EnsureCache(); distance = Mathf.Clamp(distance, 0f, Length);
            int low = 0, high = distances.Length - 1;
            while (high - low > 1) { int middle = (low + high) / 2; if (distances[middle] <= distance) low = middle; else high = middle; }
            float t = Mathf.InverseLerp(distances[low], distances[high], distance);
            position = transform.TransformPoint(Vector3.Lerp(samples[low], samples[high], t));
            rotation = Quaternion.LookRotation(transform.TransformDirection((samples[high] - samples[low]).normalized), Vector3.up);
        }
    }
}
