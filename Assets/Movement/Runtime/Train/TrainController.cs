using System.Collections.Generic;
using UnityEngine;

namespace AllAboard.Train
{
    [RequireComponent(typeof(TrainResources), typeof(TrainWeight))]
    public sealed class TrainController : MonoBehaviour
    {
        [SerializeField] private RailPath path;
        [SerializeField] private TrainCar[] allCars;
        [SerializeField] private float headDistance = 80f;
        [SerializeField, Min(9f)] private float spacing = 9.5f;
        [SerializeField] private float forwardSpeed = 12f;
        [SerializeField] private float reverseSpeed = 4f;
        [SerializeField] private float acceleration = 2.5f;
        [SerializeField] private float brakingForce = 4.5f;
        [SerializeField] private float referenceWeight = 26f;
        private bool journeyStopped;
        public System.Action<float,float> RailSafetyCheck;
        public bool IsDerailed {get;private set;}
        public void Derail()
        {
            IsDerailed=true;StopJourney();
            foreach(var car in Consist)
            {car.Body.position+=car.Body.rotation*Vector3.right*.65f;car.Body.rotation*=Quaternion.Euler(0f,0f,9f);}
        }
        public void StopJourney(){journeyStopped=true;Speed=0f;Throttle=0f;Braking=true;}
        public void HoldAtCheckpoint(){Speed=0f;Throttle=0f;Braking=true;}
        private TrainResources resources;
        private TrainWeight weight;
        public float Speed { get; private set; }
        public float Throttle { get; set; }
        private float brakeAmount;
        public float BrakeAmount { get => brakeAmount; set => brakeAmount = Mathf.Clamp01(value); }
        public bool Braking { get => brakeAmount > 0.001f; set => brakeAmount = value ? 1f : 0f; }
        public bool Reversing { get; set; }
        public float TotalWeight { get; private set; }
        public float HeadDistance => headDistance;
        public RailPath Path => path;
        public IReadOnlyList<TrainCar> Cars => allCars;
        public TrainResources Resources { get { if (resources == null) resources = GetComponent<TrainResources>(); return resources; } }
        public TrainWeight Weight { get { if (weight == null) weight = GetComponent<TrainWeight>(); return weight; } }
        public IEnumerable<TrainCar> Consist
        {
            get
            {
                TrainCar car = allCars != null && allCars.Length > 0 ? allCars[0] : null;
                int guard = 0;
                while (car != null && guard++ < 32) { yield return car; car = car.Rear; }
            }
        }
        public int ConnectedCount { get { int count = 0; foreach (var car in Consist) count++; return count; } }
        public bool Contains(TrainCar sought) { if (sought == null) return false; foreach (var car in Consist) if (car == sought) return true; return false; }
        public void Configure(RailPath rail, TrainCar[] cars, float startDistance = 80f)
        {
            journeyStopped=false;IsDerailed=false;path = rail; allCars = cars; headDistance = startDistance;
            for (int i = 0; i < cars.Length; i++)
            {
                cars[i].Assign(this, headDistance - spacing * i);
                cars[i].Front = i > 0 ? cars[i - 1] : null;
                cars[i].Rear = i + 1 < cars.Length ? cars[i + 1] : null;
                cars[i].Place(path, cars[i].Distance, true);
            }
        }
        private void Awake() { TotalWeight = Weight.Calculate(this); }
        private void FixedUpdate() => Step(Time.fixedDeltaTime);
        public void Step(float deltaTime)
        {
            if(journeyStopped){Speed=0f;Throttle=0f;Braking=true;return;}
            if (path == null || allCars == null || deltaTime <= 0f) return;
            Throttle = Mathf.Clamp01(Throttle);
            TotalWeight = Weight.Calculate(this);
            float performance = Resources.Performance;
            float target = (Reversing ? -reverseSpeed : forwardSpeed) * Throttle * performance;
            float force = acceleration * referenceWeight / TotalWeight * performance;
            if (Braking) { target = 0f; force = brakingForce * BrakeAmount * Resources.BrakeEfficiency; }
            else if (Throttle < 0.01f || performance <= 0f) { target = 0f; force = 0.35f; }
            Speed = Mathf.MoveTowards(Speed, target, force * deltaTime);
            float requested = headDistance + Speed * deltaTime;
            float previousDistance=headDistance;
            headDistance = Mathf.Clamp(requested, (ConnectedCount - 1) * spacing + 5f, path.Length - 9f);
            RailSafetyCheck?.Invoke(previousDistance,headDistance);if(journeyStopped)return;
            if (Mathf.Abs(headDistance - requested) > 0.0001f) { Speed = 0f; Braking = true; }
            Resources.Step(Throttle, Braking, deltaTime);
            int index = 0;
            foreach (var car in Consist) car.Place(path, headDistance - spacing * index++);
            foreach (var car in allCars) if (!Contains(car)) car.Place(path, car.Distance);
        }
        public bool Disconnect(TrainCar car, bool front)
        {
            if (car == null || car.Owner != this) return false;
            TrainCar other = front ? car.Front : car.Rear;
            if (other == null) return false;
            if (front) { other.Rear = null; car.Front = null; }
            else { car.Rear = null; other.Front = null; }
            return true;
        }
        public bool Couple(TrainCoupler a, TrainCoupler b)
        {
            if (a == null || b == null || a.Car == b.Car || a.IsFront == b.IsFront || Mathf.Abs(Speed) > 0.75f ||
                a.Car.Owner != this || b.Car.Owner != this || Vector3.Distance(a.Point.position, b.Point.position) > 1.3f ||
                Vector3.Dot(a.Outward, b.Outward) > -0.8f) return false;
            TrainCar leading = a.IsFront ? b.Car : a.Car;
            TrainCar following = a.IsFront ? a.Car : b.Car;
            if (following == allCars[0] || leading.Rear != null || following.Front != null) return false;
            for (TrainCar check = following; check != null; check = check.Rear) if (check == leading) return false;
            leading.Rear = following; following.Front = leading; return true;
        }
    }
}
