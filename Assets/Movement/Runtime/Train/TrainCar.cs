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
        private TrainWheel[] detailedWheels;
        private float swayWeight;
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
            if(kind!=CarKind.Locomotive)
            {
                float target=owner!=null&&owner.Contains(this)?Mathf.Clamp01(Mathf.Abs(owner.Speed)/12f):0f;
                swayWeight=immediate?0f:Mathf.MoveTowards(swayWeight,target,Time.fixedDeltaTime*1.5f);
                float phase=distance*0.24f;
                position+=rotation*new Vector3(Mathf.Sin(phase)*0.008f,Mathf.Sin(phase*1.7f)*0.006f,0f)*swayWeight;
                rotation*=Quaternion.Euler(0f,0f,Mathf.Sin(phase)*0.35f*swayWeight);
            }
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
            if (detailedWheels == null) detailedWheels = GetComponentsInChildren<TrainWheel>();
            foreach (TrainWheel wheel in detailedWheels) wheel.Advance(travelled);
            if (rearBridge != null) rearBridge.SetActive(rear != null);
            if (roofBridge != null) roofBridge.SetActive(rear != null && rear.Kind != CarKind.Coal && kind != CarKind.Coal);
        }
    }
}
