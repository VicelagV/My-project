using UnityEngine;

namespace AllAboard.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 travel = new Vector3(4f, 0f, 0f);
        [SerializeField, Min(1f)] private float period = 12f;
        [SerializeField] private float yawAmplitude = 8f;
        private Rigidbody body;
        private Vector3 origin;
        private Quaternion originalRotation;
        private float elapsed;

        public void Configure(Vector3 displacement, float cycleSeconds, float yawDegrees)
        {
            travel = displacement;
            period = Mathf.Max(1f, cycleSeconds);
            yawAmplitude = yawDegrees;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            origin = body.position;
            originalRotation = body.rotation;
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            float wave = Mathf.Sin(elapsed * Mathf.PI * 2f / period);
            body.MovePosition(origin + travel * wave);
            body.MoveRotation(originalRotation * Quaternion.Euler(0f, yawAmplitude * wave, 0f));
        }
    }
}
