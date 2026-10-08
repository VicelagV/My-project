using AllAboard.Movement;
using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainDoor : PrototypeInteractable
    {
        [SerializeField] private Transform panel;
        [SerializeField] private Vector3 closedPosition;
        [SerializeField] private bool open = true;
        [SerializeField] private Vector3 hingeOffset = new Vector3(0f, 0f, -1f);
        [SerializeField] private float openAngle = 100f;
        private float angle;
        public Transform Panel => panel;
        public Vector3 HingePosition => panel.parent.TransformPoint(closedPosition + hingeOffset);
        public void ConfigureHinge(Vector3 offset, float degrees = 100f) { hingeOffset = offset; openAngle = degrees; angle = open ? openAngle : 0f; ApplyPose(); }
        // Kept for older builder callers; doors now always rotate about their hinge.
        public void ConfigureTravel(Vector3 travel) => ConfigureHinge(new Vector3(0f,0f,-1f));
        public bool IsOpen => open;
        public override string Prompt => open ? "E: close door" : "E: open door";
        public void Configure(Transform doorPanel, Vector3 closed, bool initiallyOpen = true)
        { panel = doorPanel; closedPosition = closed; open = initiallyOpen; angle = open ? openAngle : 0f; ApplyPose(); }
        private void Awake() { angle = open ? openAngle : 0f; ApplyPose(); }
        public override bool Interact(PlayerInteraction actor)
        {
            TrainButton nearest = null; float distance = float.PositiveInfinity;
            foreach (TrainButton button in GetComponentsInChildren<TrainButton>())
            {
                float next = actor != null ? (button.transform.position - actor.transform.position).sqrMagnitude : 0f;
                if (next < distance) { nearest = button; distance = next; }
            }
            if (nearest != null && !nearest.Press(actor)) return false;
            open = !open; return true;
        }
        public void Toggle() => open = !open;
        private void Update()
        {
            angle = Mathf.MoveTowards(angle, open ? openAngle : 0f, Time.deltaTime * 125f);
            ApplyPose();
        }
        private void ApplyPose()
        {
            if (panel == null) return;
            var turn=Quaternion.Euler(0f,angle,0f);
            panel.localPosition=closedPosition+hingeOffset-turn*hingeOffset;
            panel.localRotation=turn;
        }
    }
}
