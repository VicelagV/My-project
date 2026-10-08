using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainCoupler : PrototypeInteractable
    {
        [SerializeField] private TrainCar car;
        [SerializeField] private bool front;
        [SerializeField] private Transform point;
        public TrainCar Car => car;
        public bool IsFront => front;
        public Transform Point => point;
        public Vector3 Outward => car.transform.forward * (front ? 1f : -1f);
        public bool Connected => front ? car.Front != null : car.Rear != null;
        public override string Prompt => Connected ? "E: disconnect coupler" : "E: connect nearby coupler (stop train first)";
        public void Configure(TrainCar owner, bool isFront, Transform anchor) { car = owner; front = isFront; point = anchor; }
        public override bool Interact(PlayerInteraction actor)
        {
            if (car == null || car.Owner == null) return false;
            if (Connected) return car.Owner.Disconnect(car, front);
            foreach (TrainCoupler other in Object.FindObjectsByType<TrainCoupler>(FindObjectsSortMode.None))
                if (other != this && !other.Connected && car.Owner.Couple(this, other)) return true;
            return false;
        }
    }
}
