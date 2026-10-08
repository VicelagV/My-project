using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainButton : MonoBehaviour
    {
        [SerializeField] private Vector3 travel = new Vector3(0f, 0f, 0.045f);
        [SerializeField] private bool rotary;
        [SerializeField] private Transform contact;
        private Vector3 rest;
        private Quaternion rotation;
        private float remaining;
        public bool IsPressed => remaining > 0f;
        public void Configure(Vector3 direction, bool valve = false, Transform grip = null) { travel = direction; rotary = valve; contact = grip; }
        private void Awake() { rest = transform.localPosition; rotation = transform.localRotation; }
        public bool Press(PlayerInteraction actor)
        {
            if (IsPressed) return false;
            remaining = 0.4f;
            if (actor != null) actor.GetComponent<FirstPersonView>().ReachForButton(contact != null ? contact : transform);
            return true;
        }
        private void Update()
        {
            remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            float amount = Mathf.Clamp01(Mathf.Min((0.4f - remaining) / 0.07f, remaining / 0.12f));
            transform.localPosition = rest + travel * amount;
            transform.localRotation = rotary ? rotation * Quaternion.Euler(0f, 0f, amount * 80f) : rotation;
        }
        private void OnDisable() { remaining = 0f; transform.localPosition = rest; transform.localRotation = rotation; }
    }
}
