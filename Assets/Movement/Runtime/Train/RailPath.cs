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
        public float ControlPointDistance(int index)
        { EnsureCache(); return distances[Mathf.Clamp(index * samplesPerSection, 0, distances.Length - 1)]; }
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
                Evaluate(section,t,out samples[i],out _);
                if (i > 0) distances[i] = distances[i - 1] + Vector3.Distance(samples[i], samples[i - 1]);
            }
        }
        public void Sample(float distance, out Vector3 position, out Quaternion rotation)
        {
            EnsureCache(); distance = Mathf.Clamp(distance, 0f, Length);
            int low = 0, high = distances.Length - 1;
            while (high - low > 1) { int middle = (low + high) / 2; if (distances[middle] <= distance) low = middle; else high = middle; }
            float t = Mathf.InverseLerp(distances[low], distances[high], distance);
            int section = Mathf.Min(low / samplesPerSection, points.Length - 2);
            float curveT = (low - section * samplesPerSection + t) / samplesPerSection;
            Evaluate(section, curveT, out Vector3 local, out Vector3 tangent);
            position = transform.TransformPoint(local);
            rotation = Quaternion.LookRotation(transform.TransformDirection(tangent), Vector3.up);
        }
        private void Evaluate(int section,float t,out Vector3 position,out Vector3 tangent)
        {
            Vector3 b=points[section],c=points[section+1];
            Vector3 a=section>0?points[section-1]:b*2f-c;
            Vector3 d=section+2<points.Length?points[section+2]:c*2f-b;
            Vector3 quadratic=2f*a-5f*b+4f*c-d,cubic=-a+3f*b-3f*c+d;
            position=0.5f*(2f*b+(-a+c)*t+quadratic*t*t+cubic*t*t*t);
            tangent=(0.5f*((-a+c)+2f*quadratic*t+3f*cubic*t*t)).normalized;
            if(tangent.sqrMagnitude<0.001f)tangent=(c-b).normalized;
        }
    }
}
