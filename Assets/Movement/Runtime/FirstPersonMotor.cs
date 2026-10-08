using UnityEngine;

namespace AllAboard.Movement
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    public sealed class FirstPersonMotor : MonoBehaviour
    {
        public event System.Action Jumped;
        public event System.Action<float> Landed;
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
        private PlayerInteraction interaction;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float groundGraceRemaining;
        private float jumpBufferRemaining;
        private bool jumpConsumed;
        private Vector3 spawnPosition;
        private float standingHeight;
        private Vector3 standingCenter;
        private Rigidbody support;
        private Vector3 supportLocalPosition;
        private Vector3 supportWorldPosition;
        private Quaternion supportRotation;
        private Vector3 supportVelocity;
        private Vector3 airMomentum;
        private Transform climbCarrier;
        private Vector3 climbStart;
        private Vector3 climbEnd;
        private float climbTime;
        private float climbDuration;
        private Transform seatCarrier;
        private Vector3 seatedLocal;
        private Vector3 seatExitLocal;
        public bool IsSeated => seatCarrier != null;
        public bool IsClimbing => climbCarrier != null;
        public Rigidbody SupportBody => support;
        public Vector3 WorldVelocity { get; private set; }
        public Vector3 SupportDisplacement { get; private set; }
        public float RelativeGroundDistance { get; private set; }
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
            SetCrouched(input.HasControl && input.CrouchHeld);
            Simulate(input.HasControl ? input.Move : Vector2.zero, input.HasControl && input.JumpPressed,
                Time.deltaTime, input.HasControl && input.SprintHeld);
        }

        // Explicit movement commands keep simulation independent of keyboard and camera input.
        public void Simulate(Vector2 movement, bool jumpPressed, float deltaTime, bool sprint = false)
        {
            if (deltaTime <= 0f) return;
            if (IsSeated) { AdvanceSeat(); return; }
            if (IsClimbing) { AdvanceClimb(deltaTime); return; }
            if (!controller.enabled) { controller.enabled = true; support = null; }
            Vector3 frameStart = transform.position;
            bool previouslyGrounded = controller.isGrounded;
            SupportDisplacement = Vector3.zero;
            if (support != null)
            {
                Vector3 next = support.transform.TransformPoint(supportLocalPosition);
                Vector3 delta = next - supportWorldPosition;
                // Dynamic contact/interpolation can change pose between render frames.
                // Use the physics velocity rather than dividing that jump by a tiny frame time.
                supportVelocity = support.GetPointVelocity(transform.position);
                controller.Move(delta);
                SupportDisplacement = transform.position - frameStart;
                Quaternion turn = support.transform.rotation * Quaternion.Inverse(supportRotation);
                transform.Rotate(Vector3.up, Mathf.DeltaAngle(0f, turn.eulerAngles.y), Space.World);
            }
            bool grounded = (previouslyGrounded || FindSupport() != null) && verticalVelocity <= 0f;
            groundGraceRemaining = Mathf.Max(0f, groundGraceRemaining - deltaTime);
            jumpBufferRemaining = Mathf.Max(0f, jumpBufferRemaining - deltaTime);
            if (grounded)
            {
                groundGraceRemaining = coyoteTime;
                jumpConsumed = false;
                verticalVelocity = -2f;
                airMomentum = Vector3.zero;
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
                if (support != null)
                {
                    airMomentum = supportVelocity;
                    support = null;
                }
                jumpBufferRemaining = 0f;
                jumpConsumed = true;
                Jumped?.Invoke();
            }

            verticalVelocity = Mathf.Max(verticalVelocity - gravity * deltaTime, -50f);
            float fallSpeed = -verticalVelocity;
            Vector3 relativeStart = transform.position;
            if (interaction == null) interaction = GetComponent<PlayerInteraction>();
            Vector3 displacement = (horizontalVelocity + airMomentum) * deltaTime;
            if (interaction != null) displacement = interaction.ConstrainCarryMovement(displacement);
            CollisionFlags collision = controller.Move(displacement + Vector3.up * verticalVelocity * deltaTime);
            if (!grounded && (collision & CollisionFlags.Below) != 0 && fallSpeed > 3f) Landed?.Invoke(fallSpeed);
            if ((collision & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
            // Pickup collision-pair changes can clear Below for a frame; probe the actual ground.
            Rigidbody nextSupport = verticalVelocity <= 0f ? FindSupport() : null;
            if (support != null && nextSupport == null) airMomentum = supportVelocity;
            support = nextSupport;
            if ((collision & CollisionFlags.Below) != 0 && verticalVelocity <= 0f) airMomentum = Vector3.zero;
            if (support != null)
            {
                supportLocalPosition = support.transform.InverseTransformPoint(transform.position);
                supportWorldPosition = transform.position;
                supportRotation = support.transform.rotation;
            }
            WorldVelocity = (transform.position - frameStart) / deltaTime;
            Vector3 relativeMove = transform.position - relativeStart;
            RelativeGroundDistance = new Vector2(relativeMove.x, relativeMove.z).magnitude;
            if (transform.position.y < respawnBelow) Respawn();
        }

        public void Respawn()
        {
            climbCarrier = null;
            seatCarrier = null;
            controller.enabled = false;
            transform.position = spawnPosition;
            ApplyStance(false);
            controller.enabled = true;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            groundGraceRemaining = jumpBufferRemaining = 0f;
            jumpConsumed = false;
            support = null;
            supportVelocity = airMomentum = WorldVelocity = SupportDisplacement = Vector3.zero;
            RelativeGroundDistance = 0f;
        }

        public bool SetCrouched(bool crouched)
        {
            if (IsClimbing || IsSeated) return false;
            if (crouched == IsCrouched) return true;
            if (!crouched && IsCrouched && !CanStand()) return false;
            ApplyStance(crouched);
            return true;
        }

        private Rigidbody FindSupport()
        {
            Vector3 feet = transform.TransformPoint(controller.center - Vector3.up * (controller.height * 0.5f));
            float radius = Mathf.Max(0.05f, controller.radius - controller.skinWidth);
            Rigidbody nearest = null;
            float distance = float.PositiveInfinity;
            foreach (RaycastHit hit in Physics.SphereCastAll(feet + Vector3.up * (radius + 0.08f), radius,
                Vector3.down, 0.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (IgnorePlayerCollision(hit.collider) || Vector3.Dot(hit.normal, Vector3.up) < 0.6f || hit.distance >= distance) continue;
                distance = hit.distance;
                // Follow the body directly: a loose or welded cube can be the rider's
                // immediate support even when its own motion comes from a platform.
                nearest = hit.collider.attachedRigidbody;
            }
            return nearest;
        }

        private bool CanStand()
        {
            Vector3 center = transform.TransformPoint(standingCenter);
            // Inset the query by skin width so touching the floor does not block standing.
            float radius = Mathf.Max(0.01f, controller.radius - controller.skinWidth);
            float halfSegment = Mathf.Max(0f, standingHeight * 0.5f - controller.radius);
            foreach (Collider hit in Physics.OverlapCapsule(center - transform.up * halfSegment,
                center + transform.up * halfSegment, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                if (!IgnorePlayerCollision(hit) && !hit.transform.IsChildOf(transform)) return false;
            return true;
        }

        private bool IgnorePlayerCollision(Collider collider)
        {
            if (collider == controller || Physics.GetIgnoreCollision(controller, collider)) return true;
            var item = collider.attachedRigidbody != null ? collider.attachedRigidbody.GetComponent<PickupObject>() : null;
            return item != null && item.IsHeld;
        }

        private void ApplyStance(bool crouched)
        {
            float height = crouched ? Mathf.Clamp(crouchHeight, controller.radius * 2f, standingHeight) : standingHeight;
            controller.height = height;
            controller.center = standingCenter - Vector3.up * ((standingHeight - height) * 0.5f);
            IsCrouched = crouched;
        }

        public bool BeginClimb(Transform carrier, Vector3 destinationLocal, float duration = 1.2f)
        {
            if (carrier == null || IsClimbing || IsSeated || carrier.GetComponent<Rigidbody>() == null) return false;
            climbCarrier = carrier;
            climbStart = carrier.InverseTransformPoint(transform.position);
            climbEnd = destinationLocal;
            climbTime = 0f;
            climbDuration = Mathf.Max(0.1f, duration);
            supportRotation = carrier.rotation;
            support = carrier.GetComponent<Rigidbody>();
            horizontalVelocity = airMomentum = Vector3.zero;
            verticalVelocity = -2f;
            controller.enabled = false;
            return true;
        }

        private void AdvanceClimb(float deltaTime)
        {
            climbTime += deltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(climbTime / climbDuration));
            transform.position = climbCarrier.TransformPoint(Vector3.Lerp(climbStart, climbEnd, amount));
            Quaternion turn = climbCarrier.rotation * Quaternion.Inverse(supportRotation);
            transform.Rotate(Vector3.up, Mathf.DeltaAngle(0f, turn.eulerAngles.y), Space.World);
            supportRotation = climbCarrier.rotation;
            WorldVelocity = support.GetPointVelocity(transform.position) + (climbEnd - climbStart) / climbDuration;
            SupportDisplacement = Vector3.zero;
            RelativeGroundDistance = 0f;
            if (climbTime < climbDuration) return;
            climbCarrier = null;
            controller.enabled = true;
            supportLocalPosition = support.transform.InverseTransformPoint(transform.position);
            supportWorldPosition = transform.position;
        }

        public bool BeginSeat(Transform carrier, Vector3 localFeet, Vector3 localExit)
        {
            if (carrier == null || IsClimbing || IsSeated || carrier.GetComponent<Rigidbody>() == null) return false;
            seatCarrier = carrier; seatedLocal = localFeet; seatExitLocal = localExit;
            support = carrier.GetComponent<Rigidbody>(); supportRotation = carrier.rotation;
            controller.enabled = false; ApplyStance(true);
            horizontalVelocity = airMomentum = Vector3.zero;
            AdvanceSeat(); return true;
        }
        private void AdvanceSeat()
        {
            Vector3 before = transform.position;
            transform.position = seatCarrier.TransformPoint(seatedLocal);
            Quaternion turn = seatCarrier.rotation * Quaternion.Inverse(supportRotation);
            transform.Rotate(Vector3.up, Mathf.DeltaAngle(0f, turn.eulerAngles.y), Space.World);
            supportRotation = seatCarrier.rotation;
            SupportDisplacement = transform.position - before;
            WorldVelocity = support.GetPointVelocity(transform.position);
            RelativeGroundDistance = 0f;
        }
        public bool ExitSeat()
        {
            if (!IsSeated) return false;
            transform.position = seatCarrier.TransformPoint(seatExitLocal);
            seatCarrier = null; ApplyStance(false); controller.enabled = true;
            verticalVelocity = -2f;
            supportLocalPosition = support.transform.InverseTransformPoint(transform.position);
            supportWorldPosition = transform.position;
            return true;
        }
    }
}
