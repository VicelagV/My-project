using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public enum ControlKind { ThrottleUp, ThrottleDown, Brake, Reverse, Firebox, Vent, Whistle, Bell, Door, DamageSpeedometer }
    public sealed class TrainControl : PrototypeInteractable
    {
        [SerializeField] private TrainController train;
        [SerializeField] private ControlKind kind;
        [SerializeField] private TrainDoor door;
        [SerializeField] private DamageableComponent damageTarget;
        [SerializeField] private AudioClip sound;
        [SerializeField] private ConductorSeat seat;
        private AudioSource source;
        public ControlKind Kind => kind;
        public override bool AllowedWhileSeated => true;
        public override string Prompt
        {
            get
            {
                if ((kind == ControlKind.ThrottleUp || kind == ControlKind.ThrottleDown) && (seat == null || seat.Occupant == null))
                    return "Sit in the conductor seat to use the throttle";
                return "E: " + (kind == ControlKind.ThrottleUp ? "increase throttle" : kind == ControlKind.ThrottleDown ? "decrease throttle" :
                    kind == ControlKind.Firebox ? "feed held coal" : kind == ControlKind.Brake ? "toggle brake" :
                    kind == ControlKind.Reverse ? "toggle reverse" : kind.ToString());
            }
        }
        public void Configure(TrainController controller, ControlKind action, TrainDoor doorTarget = null, DamageableComponent damage = null, AudioClip clip = null)
        { train = controller; kind = action; door = doorTarget; damageTarget = damage; sound = clip; }
        public void RestrictToSeat(ConductorSeat conductorSeat) => seat = conductorSeat;
        public override bool Interact(PlayerInteraction actor)
        {
            if (train == null) return false;
            if ((kind == ControlKind.ThrottleUp || kind == ControlKind.ThrottleDown) &&
                (seat == null || actor == null || seat.Occupant != actor.GetComponent<FirstPersonMotor>())) return false;
            switch (kind)
            {
                case ControlKind.ThrottleUp: train.Throttle = Mathf.Clamp01(train.Throttle + 0.25f); break;
                case ControlKind.ThrottleDown: train.Throttle = Mathf.Clamp01(train.Throttle - 0.25f); break;
                case ControlKind.Brake: train.Braking = !train.Braking; break;
                case ControlKind.Reverse: train.Reversing = !train.Reversing; break;
                case ControlKind.Vent: train.Resources.Vent(); break;
                case ControlKind.Firebox:
                    if (actor == null || actor.HeldItem == null || actor.HeldItem.GetComponent<CoalItem>() == null || !train.Resources.AddCoal()) return false;
                    return actor.ConsumeHeldItem();
                case ControlKind.Door: if (door == null) return false; door.Toggle(); break;
                case ControlKind.DamageSpeedometer: if (damageTarget == null) return false; damageTarget.Damage(25f); break;
                case ControlKind.Whistle:
                case ControlKind.Bell:
                    if (sound == null) return false;
                    if (source == null) { source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 1f; source.minDistance = 3f; source.maxDistance = 70f; source.dopplerLevel = 0f; }
                    source.PlayOneShot(sound, 0.25f); break;
            }
            return true;
        }
    }
}
