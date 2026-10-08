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
        [SerializeField, Min(0f)] private float movementLag = 0.015f;
        [SerializeField, Min(0f)] private float turnLag = 0.0003f;
        [SerializeField, Min(0.01f)] private float inertiaResponse = 0.1f;
        [SerializeField, Min(0f)] private float maximumInertia = 0.12f;
        [SerializeField, Min(0f)] private float maximumTurnLag = 5f;
        [SerializeField, Min(0.01f)] private float crouchCameraSpeed = 6f;

        private PlayerInputReader input;
        private FirstPersonMotor motor;
        private Vector3 handRestPosition;
        private float pitch;
        private float bobPhase;
        private Transform leftHand;
        private Transform rightHand;
        private Vector3 leftRest;
        private Vector3 rightRest;
        private Transform heldObject;
        private Transform handControlTarget;
        public bool IsUsingHandControl => handControlTarget != null;
        public void SetHandControlTarget(Transform target) => handControlTarget = target;
        private float gripHalfWidth;
        private float standingEyeHeight;
        private PlayerInteraction interaction;
        private readonly ViewMotionInertia inertia = new ViewMotionInertia();
        public Vector3 InertiaOffset => inertia.Offset;
        public Quaternion InertiaRotation => Quaternion.Euler(inertia.Angles);

        public void SetHeldObject(Transform item, float halfWidth = 0f)
        {
            heldObject = item;
            gripHalfWidth = halfWidth;
        }

        public void Configure(Transform pivot, Transform handRoot)
        {
            cameraPivot = pivot;
            hands = handRoot;
        }

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            motor = GetComponent<FirstPersonMotor>();
            interaction = GetComponent<PlayerInteraction>();
            if (cameraPivot != null) standingEyeHeight = cameraPivot.localPosition.y;
            if (hands != null)
            {
                handRestPosition = hands.localPosition;
                leftHand = hands.Find("Left Hand");
                rightHand = hands.Find("Right Hand");
                if (leftHand != null) leftRest = leftHand.localPosition;
                if (rightHand != null) rightRest = rightHand.localPosition;
            }
        }

        private void Update()
        {
            if (cameraPivot == null || !input.HasControl) return;
            if (interaction != null && interaction.IsPressingHoveredLever) return;
            ApplyLook(input.Look);
        }

        public void ApplyLook(Vector2 delta)
        {
            if (cameraPivot == null || IsUsingHandControl) return;
            transform.Rotate(Vector3.up, delta.x * mouseSensitivity, Space.Self);
            pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, -pitchLimit, pitchLimit);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (cameraPivot != null)
            {
                Vector3 eye = cameraPivot.localPosition;
                eye.y = Mathf.MoveTowards(eye.y, standingEyeHeight + motor.EyeHeightOffset,
                    crouchCameraSpeed * Time.deltaTime);
                cameraPivot.localPosition = eye;
                // Crouch eye-height animation is not movement: do not kick the hands upward.
                inertia.Step(transform.position, cameraPivot.rotation, Time.deltaTime,
                    movementLag, turnLag, inertiaResponse, maximumInertia, maximumTurnLag, motor.SupportDisplacement);
            }
            // Resolve the constrained grip with this frame's camera before posing the hands.
            if (interaction != null) interaction.UpdateHeldPose();
            if (hands == null) return;
            if (heldObject != null)
            {
                hands.localPosition = handRestPosition;
                hands.localRotation = Quaternion.identity;
                Vector3 side = heldObject.right * (gripHalfWidth + 0.06f);
                if (leftHand != null) leftHand.position = heldObject.position - side;
                if (rightHand != null) rightHand.position = heldObject.position + side;
                return;
            }
            float blend = 1f - Mathf.Exp(-14f * Time.deltaTime);
            if (leftHand != null) leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, leftRest, blend);
            if (rightHand != null) rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rightRest, blend);
            Vector2 delta = input.HasControl ? Vector2.ClampMagnitude(input.Look, 35f) : Vector2.zero;
            float strength = motor.Grounded ? Mathf.Clamp01(motor.HorizontalSpeed / 5f) : 0f;
            bobPhase += Time.deltaTime * motor.HorizontalSpeed * 2.2f;
            Vector3 bob = new Vector3(Mathf.Sin(bobPhase), Mathf.Sin(bobPhase * 2f), 0f) * handBob * strength;
            Vector3 sway = new Vector3(-delta.x, -delta.y, 0f) * handSway;
            hands.localPosition = Vector3.Lerp(hands.localPosition, handRestPosition + bob + sway + inertia.Offset,
                1f - Mathf.Exp(-14f * Time.deltaTime));
            hands.localRotation = InertiaRotation;
            if (handControlTarget != null && rightHand != null)
                rightHand.position = handControlTarget.position + handControlTarget.right * 0.11f;
        }
    }
}
