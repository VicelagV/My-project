using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainThrottleLever : PrototypeInteractable
    {
        [SerializeField] private TrainController train;
        [SerializeField] private ConductorSeat seat;
        [SerializeField] private Transform handle;
        [SerializeField, Min(1f)] private float fullTravelPixels = 300f;
        [SerializeField] private float minimumAngle = -35f;
        [SerializeField] private float maximumAngle = 35f;
        private PlayerInteraction occupant;
        public Transform Handle => handle;
        public bool IsGripped => occupant != null;
        public override bool AllowedWhileSeated => true;
        public override string Prompt => "Hold M1: move throttle";
        public override bool Interact(PlayerInteraction actor) => false;
        public void Configure(TrainController controller, ConductorSeat conductorSeat, Transform grip)
        { train = controller; seat = conductorSeat; handle = grip; ApplyPose(); }
        public bool BeginGrip(PlayerInteraction actor)
        {
            if (actor == null || train == null || seat == null || handle == null || IsGripped || actor.IsHolding ||
                seat.Occupant != actor.GetComponent<FirstPersonMotor>()) return false;
            occupant = actor;
            return true;
        }
        public bool Drag(PlayerInteraction actor, Vector2 delta)
        {
            if (occupant != actor || actor == null) return false;
            if (seat.Occupant != actor.GetComponent<FirstPersonMotor>()) { EndGrip(actor); return false; }
            train.Throttle = Mathf.Clamp01(train.Throttle + delta.y / fullTravelPixels);
            ApplyPose();
            return true;
        }
        public void EndGrip(PlayerInteraction actor) { if (occupant == actor) occupant = null; }
        private void LateUpdate() => ApplyPose();
        private void ApplyPose()
        {
            if (train != null) transform.localRotation = Quaternion.Euler(Mathf.Lerp(minimumAngle, maximumAngle, train.Throttle), 0f, 0f);
        }
        private void OnDisable()
        {
            if (occupant != null) occupant.ReleaseThrottle();
            occupant = null;
        }
    }
}
