using UnityEngine;

namespace AllAboard.Train
{
    public enum CarKind { Locomotive, Passenger, Coal }
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TrainCar : MonoBehaviour
    {
        [SerializeField] private CarKind kind;
        [SerializeField, Min(1f)] private float baseWeight = 6f;
        [SerializeField] private float distance;
        [SerializeField] private GameObject rearBridge;
        [SerializeField] private GameObject roofBridge;
        [SerializeField] private Transform[] wheels;
        [SerializeField] private TrainController owner;
        [SerializeField] private TrainCar front;
        [SerializeField] private TrainCar rear;
        private Rigidbody body;
        private float wheelAngle;
        public CarKind Kind => kind;
        public float BaseWeight => baseWeight;
        public float Distance => distance;
        public TrainController Owner => owner;
        public TrainCar Front { get => front; set => front = value; }
        public TrainCar Rear { get => rear; set => rear = value; }
        public Rigidbody Body { get { if (body == null) body = GetComponent<Rigidbody>(); return body; } }
        public void Configure(CarKind type, float weight, GameObject bridge, GameObject roof, Transform[] visualWheels)
        { kind = type; baseWeight = weight; rearBridge = bridge; roofBridge = roof; wheels = visualWheels; }
        public void Assign(TrainController controller, float initialDistance) { owner = controller; distance = initialDistance; }
        private void Awake()
        {
            Body.isKinematic = true; Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        public void Place(RailPath path, float pathDistance, bool immediate = false)
        {
            float travelled = pathDistance - distance;
            distance = pathDistance;
            path.Sample(distance, out Vector3 position, out Quaternion rotation);
            if (immediate)
            {
                // Reset interpolation history when spawning or relocating the consist.
                Body.interpolation = RigidbodyInterpolation.None;
                Body.position = position; Body.rotation = rotation;
                transform.SetPositionAndRotation(position, rotation);
                Body.interpolation = RigidbodyInterpolation.Interpolate;
            }
            else { Body.MovePosition(position); Body.MoveRotation(rotation); }
            wheelAngle += travelled / 0.4f * Mathf.Rad2Deg;
            if (wheels != null) foreach (Transform wheel in wheels) if (wheel != null) wheel.localRotation = Quaternion.Euler(wheelAngle, 0f, 90f);
            if (rearBridge != null) rearBridge.SetActive(rear != null);
            if (roofBridge != null) roofBridge.SetActive(rear != null && rear.Kind != CarKind.Coal && kind != CarKind.Coal);
        }
    }
}
