using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TrainFurnace : MonoBehaviour
    {
        [SerializeField] private TrainResources resources;
        public void Configure(TrainResources supply) => resources = supply;
        private void OnTriggerEnter(Collider other) => AcceptLooseCoal(other);
        private void OnTriggerStay(Collider other) => AcceptLooseCoal(other);
        private void AcceptLooseCoal(Collider other)
        {
            var coal = other.GetComponentInParent<CoalItem>();
            var item = coal != null ? coal.GetComponent<PickupObject>() : null;
            if (item == null || item.IsHeld || item.IsWelded || coal.IsStored || !coal.TryFuel(resources)) return;
            // Disable immediately: multiple colliders/trigger callbacks must count a lump only once.
            coal.gameObject.SetActive(false);
            Destroy(coal.gameObject);
        }
    }
}
