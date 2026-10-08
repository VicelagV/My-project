using UnityEngine;
namespace AllAboard.Train
{
    public sealed class TrainWheel : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float radius = 0.4f;
        private float angle;
        public float Radius => radius;
        public float Angle => angle;
        public void Configure(float wheelRadius) => radius = wheelRadius;
        public void Advance(float distance)
        { angle = Mathf.Repeat(angle + distance / radius * Mathf.Rad2Deg, 360f); transform.localRotation = Quaternion.AngleAxis(angle, Vector3.right); }
    }
}
