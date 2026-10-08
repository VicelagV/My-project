using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainDoor : PrototypeInteractable
    {
        [SerializeField] private Transform panel;
        [SerializeField] private Vector3 closedPosition;
        [SerializeField] private bool open = true;
        public bool IsOpen => open;
        public override string Prompt => open ? "E: close door" : "E: open door";
        public void Configure(Transform doorPanel, Vector3 closed, bool initiallyOpen = true)
        { panel = doorPanel; closedPosition = closed; open = initiallyOpen; panel.localPosition = closedPosition + (open ? Vector3.up * 2.7f : Vector3.zero); }
        public override bool Interact(PlayerInteraction actor) { open = !open; return true; }
        public void Toggle() => open = !open;
        private void Update()
        {
            if (panel != null) panel.localPosition = Vector3.MoveTowards(panel.localPosition, closedPosition + (open ? Vector3.up * 2.7f : Vector3.zero), Time.deltaTime * 4f);
        }
    }
}
