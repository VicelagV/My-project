using UnityEngine;
namespace AllAboard.Train
{
    public sealed class CoalItem : MonoBehaviour
    {
        [SerializeField] private TrainCar storageCar;
        [SerializeField] private RigidbodyInterpolation freeInterpolation = RigidbodyInterpolation.Interpolate;
        [SerializeField] private CollisionDetectionMode freeCollisionMode = CollisionDetectionMode.ContinuousDynamic;
        public bool IsStored => storageCar != null;
        public TrainCar StorageCar => storageCar;

        public void StoreIn(TrainCar car)
        {
            if (car == null || IsStored) return;
            Rigidbody body = GetComponent<Rigidbody>();
            freeInterpolation = body.interpolation;
            freeCollisionMode = body.collisionDetectionMode;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.None;
            storageCar = car;
            transform.SetParent(car.transform, true);
        }

        public void TakeFromStorage()
        {
            if (!IsStored) return;
            storageCar = null;
            transform.SetParent(null, true);
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = false;
            body.useGravity = true;
            body.collisionDetectionMode = freeCollisionMode;
            body.interpolation = freeInterpolation;
            body.WakeUp();
        }
    }
}
