using UnityEngine;

namespace AllAboard.Movement
{
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(PlayerInputReader), typeof(FirstPersonMotor))]
    public sealed class FirstPersonView : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform hands;
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Range(1f, 89f)] private float pitchLimit = 85f;
        [SerializeField, Min(0f)] private float handBob = 0.012f;
        [SerializeField, Min(0f)] private float handSway = 0.0006f;

        private PlayerInputReader input;
        private FirstPersonMotor motor;
        private Vector3 handRestPosition;
        private float pitch;
        private float bobPhase;

        public void Configure(Transform pivot, Transform handRoot)
        {
            cameraPivot = pivot;
            hands = handRoot;
        }

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            motor = GetComponent<FirstPersonMotor>();
            if (hands != null) handRestPosition = hands.localPosition;
        }

        private void Update()
        {
            if (cameraPivot == null || !input.HasControl) return;
            Vector2 delta = input.Look;
            transform.Rotate(Vector3.up, delta.x * mouseSensitivity, Space.Self);
            pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, -pitchLimit, pitchLimit);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (hands == null) return;
            Vector2 delta = input.HasControl ? Vector2.ClampMagnitude(input.Look, 35f) : Vector2.zero;
            float strength = motor.Grounded ? Mathf.Clamp01(motor.HorizontalSpeed / 5f) : 0f;
            bobPhase += Time.deltaTime * motor.HorizontalSpeed * 2.2f;
            Vector3 bob = new Vector3(Mathf.Sin(bobPhase), Mathf.Sin(bobPhase * 2f), 0f) * handBob * strength;
            Vector3 sway = new Vector3(-delta.x, -delta.y, 0f) * handSway;
            hands.localPosition = Vector3.Lerp(hands.localPosition, handRestPosition + bob + sway,
                1f - Mathf.Exp(-14f * Time.deltaTime));
        }
    }
}
