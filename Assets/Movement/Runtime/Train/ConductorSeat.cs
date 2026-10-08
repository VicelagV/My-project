using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class ConductorSeat : PrototypeInteractable
    {
        [SerializeField] private TrainCar car;
        [SerializeField] private Vector3 feet = new Vector3(-0.85f, 1.45f, 1.6f);
        [SerializeField] private Vector3 exit = new Vector3(0f, 1.18f, 1.2f);
        private FirstPersonMotor occupant;
        public FirstPersonMotor Occupant => occupant != null && occupant.IsSeated ? occupant : null;
        public override string Prompt => Occupant == null ? "E: sit in conductor's seat" : "E: stand up";
        public void Configure(TrainCar owner) => car = owner;
        public override bool Interact(PlayerInteraction actor)
        {
            if (actor == null || car == null) return false;
            var motor = actor.GetComponent<FirstPersonMotor>();
            if (Occupant == motor) return motor.ExitSeat();
            if (Occupant != null || !motor.BeginSeat(car.transform, feet, exit)) return false;
            occupant = motor; return true;
        }
    }
}
