using UnityEngine;

namespace AllAboard.Movement
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    public sealed class FirstPersonMotor : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float walkSpeed = 5f;
        [SerializeField, Min(0f)] private float sprintSpeed = 8f;
        [SerializeField, Min(0f)] private float crouchSpeed = 2.5f;
        [SerializeField, Min(0.6f)] private float crouchHeight = 1.1f;
        [SerializeField, Min(0f)] private float groundAcceleration = 35f;
        [SerializeField, Min(0f)] private float airAcceleration = 12f;
        [SerializeField, Min(0.01f)] private float jumpHeight = 1.2f;
        [SerializeField, Min(0.01f)] private float gravity = 24f;
        [SerializeField, Min(0f)] private float coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float jumpBuffer = 0.12f;
        [SerializeField] private float respawnBelow = -12f;

        private CharacterController controller;
        private PlayerInputReader input;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float groundGraceRemaining;
        private float jumpBufferRemaining;
        private bool jumpConsumed;
        private Vector3 spawnPosition;
        private float standingHeight;
        private Vector3 standingCenter;
        public bool IsCrouched { get; private set; }
        public float EyeHeightOffset => controller.height - standingHeight;
        public float HorizontalSpeed => horizontalVelocity.magnitude;
        public bool Grounded => controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputReader>();
            spawnPosition = transform.position;
            standingHeight = controller.height;
            standingCenter = controller.center;
        }

        private void Update()
        {
            if (input.HasControl && input.CrouchPressed) SetCrouched(!IsCrouched);
            Simulate(input.HasControl ? input.Move : Vector2.zero, input.HasControl && input.JumpPressed,
                Time.deltaTime, input.HasControl && input.SprintHeld);
        }

        // Explicit movement commands keep simulation independent of keyboard and camera input.
        public void Simulate(Vector2 movement, bool jumpPressed, float deltaTime, bool sprint = false)
        {
            if (deltaTime <= 0f) return;
            bool grounded = controller.isGrounded && verticalVelocity <= 0f;
            groundGraceRemaining = Mathf.Max(0f, groundGraceRemaining - deltaTime);
            jumpBufferRemaining = Mathf.Max(0f, jumpBufferRemaining - deltaTime);
            if (grounded)
            {
                groundGraceRemaining = coyoteTime;
                jumpConsumed = false;
                verticalVelocity = -2f;
            }

            movement = Vector2.ClampMagnitude(movement, 1f);
            float speed = IsCrouched ? crouchSpeed : sprint ? sprintSpeed : walkSpeed;
            Vector3 target = (transform.right * movement.x + transform.forward * movement.y) * speed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target,
                (grounded ? groundAcceleration : airAcceleration) * deltaTime);

            if (jumpPressed) jumpBufferRemaining = jumpBuffer;
            if (!jumpConsumed && (grounded || groundGraceRemaining > 0f) && (jumpPressed || jumpBufferRemaining > 0f))
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
                jumpBufferRemaining = 0f;
                jumpConsumed = true;
            }

            verticalVelocity = Mathf.Max(verticalVelocity - gravity * deltaTime, -50f);
            CollisionFlags collision = controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime);
            if ((collision & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
            if (transform.position.y < respawnBelow) Respawn();
        }

        public void Respawn()
        {
            controller.enabled = false;
            transform.position = spawnPosition;
            ApplyStance(false);
            controller.enabled = true;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            groundGraceRemaining = jumpBufferRemaining = 0f;
            jumpConsumed = false;
        }

        public bool SetCrouched(bool crouched)
        {
            if (!crouched && IsCrouched && !CanStand()) return false;
            ApplyStance(crouched);
            return true;
        }

        private bool CanStand()
        {
            Vector3 center = transform.TransformPoint(standingCenter);
            // Inset the query by skin width so touching the floor does not block standing.
            float radius = Mathf.Max(0.01f, controller.radius - controller.skinWidth);
            float halfSegment = Mathf.Max(0f, standingHeight * 0.5f - controller.radius);
            foreach (Collider hit in Physics.OverlapCapsule(center - transform.up * halfSegment,
                center + transform.up * halfSegment, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                if (hit != controller && !hit.transform.IsChildOf(transform)) return false;
            return true;
        }

        private void ApplyStance(bool crouched)
        {
            float height = crouched ? Mathf.Clamp(crouchHeight, controller.radius * 2f, standingHeight) : standingHeight;
            controller.height = height;
            controller.center = standingCenter - Vector3.up * ((standingHeight - height) * 0.5f);
            IsCrouched = crouched;
        }
    }
}
