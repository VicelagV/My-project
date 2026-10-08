using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainLadder : PrototypeInteractable
    {
        [SerializeField] private TrainCar car;
        [SerializeField] private Vector3 bottom = new Vector3(1.1f, 1.18f, -3f);
        [SerializeField] private Vector3 top = new Vector3(0.15f, 4.18f, -3f);
        public override string Prompt => "E: climb ladder / return inside";
        public void Configure(TrainCar owner) => car = owner;
        public override bool Interact(PlayerInteraction actor)
        {
            if (car == null || actor == null) return false;
            FirstPersonMotor motor = actor.GetComponent<FirstPersonMotor>();
            Vector3 local = car.transform.InverseTransformPoint(actor.transform.position);
            return motor != null && motor.BeginClimb(car.transform, local.y > 3f ? bottom : top);
        }
    }
}
