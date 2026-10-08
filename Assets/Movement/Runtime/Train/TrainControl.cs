using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public enum ControlKind { Brake, Reverse, Firebox, Vent, Whistle, Bell, Door, DamageSpeedometer }
    public sealed class TrainControl : PrototypeInteractable
    {
        [SerializeField] private TrainController train;
        [SerializeField] private ControlKind kind;
        [SerializeField] private TrainDoor door;
        [SerializeField] private DamageableComponent damageTarget;
        [SerializeField] private AudioClip sound;
        private AudioSource source;
        public ControlKind Kind => kind;
        public override bool AllowedWhileSeated => true;
        public override string Prompt => "E: " + (kind == ControlKind.Firebox ? "feed coal | Hold Q, release: throw" : kind == ControlKind.Brake ? "toggle brake" : kind == ControlKind.Reverse ? "toggle reverse" : kind == ControlKind.Vent ? "vent pressure" : kind == ControlKind.DamageSpeedometer ? "test speedometer damage" : kind == ControlKind.Door ? "toggle door" : kind.ToString().ToLowerInvariant());
        public void Configure(TrainController controller, ControlKind action, TrainDoor doorTarget = null, DamageableComponent damage = null, AudioClip clip = null)
        { train = controller; kind = action; door = doorTarget; damageTarget = damage; sound = clip; }
        public override bool Interact(PlayerInteraction actor)
        {
            if (train == null) return false;
            var button = GetComponent<TrainButton>();
            if (button != null && button.IsPressed) return false;
            if (kind == ControlKind.Reverse && (actor == null || !actor.GetComponent<FirstPersonMotor>().IsSeated)) return false;
            switch (kind)
            {
                case ControlKind.Brake: train.Braking = !train.Braking; break;
                case ControlKind.Reverse: train.Reversing = !train.Reversing; break;
                case ControlKind.Vent: train.Resources.Vent(); break;
                case ControlKind.Firebox:
                    if (actor == null || actor.HeldItem == null || actor.HeldItem.GetComponent<CoalItem>() == null || !actor.HeldItem.GetComponent<CoalItem>().TryFuel(train.Resources)) return false;
                    return actor.ConsumeHeldItem();
                case ControlKind.Door: if (door == null) return false; door.Toggle(); break;
                case ControlKind.DamageSpeedometer: if (damageTarget == null) return false; damageTarget.Damage(25f); break;
                case ControlKind.Whistle:
                case ControlKind.Bell:
                    if (sound == null) return false;
                    if (source == null) { source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 1f; source.minDistance = 3f; source.maxDistance = 70f; source.dopplerLevel = 0f; }
                    source.PlayOneShot(sound, 0.25f); break;
            }
            if (button != null) button.Press(actor);
            return true;
        }
    }
}
