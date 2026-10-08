using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainWeight : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float playerWeight = 2f;
        [SerializeField, Min(0f)] private float extraTestWeight;
        public float ExtraTestWeight { get => extraTestWeight; set => extraTestWeight = Mathf.Max(0f, value); }
        public float Calculate(TrainController train)
        {
            float weight = extraTestWeight;
            foreach (TrainCar car in train.Consist) weight += car.BaseWeight;
            foreach (FirstPersonMotor player in Object.FindObjectsByType<FirstPersonMotor>(FindObjectsSortMode.None))
            {
                bool aboard = player.SupportBody != null && train.Contains(player.SupportBody.GetComponent<TrainCar>());
                if (!aboard) foreach (TrainCar car in train.Consist)
                {
                    Vector3 local = car.transform.InverseTransformPoint(player.transform.position);
                    if (Mathf.Abs(local.x) < 2f && Mathf.Abs(local.z) < 4.8f && local.y > 0.8f && local.y < 7f) { aboard = true; break; }
                }
                if (aboard) weight += playerWeight;
            }
            foreach (PickupObject item in Object.FindObjectsByType<PickupObject>(FindObjectsSortMode.None))
                if (item.IsWelded && item.WeldedBody != null && train.Contains(item.WeldedBody.GetComponent<TrainCar>())) weight += item.Body.mass;
            return Mathf.Max(1f, weight);
        }
    }
}
