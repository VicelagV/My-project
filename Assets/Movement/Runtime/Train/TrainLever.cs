using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public abstract class TrainLever : PrototypeInteractable
    {
        [SerializeField] protected TrainController train;
        [SerializeField] private ConductorSeat seat;
        [SerializeField] private Transform handle;
        [SerializeField, Min(1f)] private float fullTravelPixels = 300f;
        [SerializeField] private float minimumAngle = -35f;
        [SerializeField] private float maximumAngle = 35f;
        private PlayerInteraction occupant;
        public Transform Handle => handle;
        public bool IsGripped => occupant != null;
        public override bool AllowedWhileSeated => true;
        public abstract override string Prompt { get; }
        protected abstract float Value { get; set; }
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
            Value = Mathf.Clamp01(Value + delta.y / fullTravelPixels);
            ApplyPose();
            return true;
        }
        public void EndGrip(PlayerInteraction actor) { if (occupant == actor) occupant = null; }
        private void LateUpdate() => ApplyPose();
        private void ApplyPose()
        {
            if (train != null) transform.localRotation = Quaternion.Euler(Mathf.Lerp(minimumAngle, maximumAngle, Value), 0f, 0f);
        }
        private void OnDisable()
        {
            if (occupant != null) occupant.ReleaseThrottle();
            occupant = null;
        }
    }
}
