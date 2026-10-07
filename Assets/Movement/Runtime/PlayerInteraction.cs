using UnityEngine;

namespace AllAboard.Movement
{
    [RequireComponent(typeof(PlayerInputReader), typeof(FirstPersonView))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField, Min(0.5f)] private float reach = 3f;
        [SerializeField, Min(0.5f)] private float holdDistance = 1.1f;
        [SerializeField] private float holdVerticalOffset = -0.18f;
        [SerializeField, Min(0.2f)] private float chargeDuration = 1.2f;
        [SerializeField, Min(0f)] private float tapDuration = 0.18f;
        [SerializeField, Min(0f)] private float minimumThrowSpeed = 3f;
        [SerializeField, Min(0f)] private float maximumThrowSpeed = 12f;
        private PlayerInputReader input;
        private FirstPersonView view;
        private CharacterController playerCollider;
        private PickupObject hovered;
        private PickupObject held;
        private Texture2D dot;
        private bool charging;
        private float chargeTime;
        public float Charge => Mathf.Clamp01(chargeTime / chargeDuration);

        public bool IsHolding => held != null;
        public void Configure(Transform pivot) => cameraPivot = pivot;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            view = GetComponent<FirstPersonView>();
            playerCollider = GetComponent<CharacterController>();
            dot = new Texture2D(7, 7, TextureFormat.RGBA32, false);
            dot.filterMode = FilterMode.Point;
            for (int y = 0; y < 7; y++)
                for (int x = 0; x < 7; x++)
                    dot.SetPixel(x, y, (new Vector2(x - 3, y - 3)).sqrMagnitude <= 9f ? Color.white : Color.clear);
            dot.Apply();
        }

        private void Update()
        {
            SetHovered(null);
            if (cameraPivot == null || !input.HasControl)
            {
                charging = false;
                chargeTime = 0f;
                return;
            }
            if (held == null)
            {
                if (Physics.Raycast(cameraPivot.position, cameraPivot.forward, out RaycastHit hit, reach,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    SetHovered(hit.collider.GetComponentInParent<PickupObject>());
            }
            if (held != null)
            {
                if (input.DropPressed) { charging = true; chargeTime = 0f; }
                if (charging && input.DropHeld) chargeTime += Time.deltaTime;
                if (charging && input.DropReleased) ReleaseCharged(chargeTime);
            }
            else if (input.InteractPressed && hovered != null) TryPickup(hovered);
        }

        private void SetHovered(PickupObject item)
        {
            if (hovered != null) hovered.SetHighlighted(false);
            hovered = item != null && !item.IsHeld ? item : null;
            if (hovered != null) hovered.SetHighlighted(true);
        }

        public bool TryPickup(PickupObject item)
        {
            if (held != null || item == null || cameraPivot == null ||
                Vector3.Distance(cameraPivot.position, item.transform.position) > reach || !item.BeginHold(playerCollider))
                return false;
            held = item;
            charging = false;
            chargeTime = 0f;
            SetHovered(null);
            view.SetHeldObject(held.transform, held.GripHalfWidth);
            return true;
        }

        private void FixedUpdate()
        {
            if (held == null || cameraPivot == null) return;
            Vector3 offset = cameraPivot.forward * (holdDistance + (charging ? Charge * 0.15f : 0f))
                + cameraPivot.up * holdVerticalOffset + cameraPivot.TransformVector(view.InertiaOffset);
            float distance = offset.magnitude;
            Vector3 direction = offset / distance;
            // A swept volume stops the held cube at walls rather than passing through them.
            foreach (RaycastHit hit in Physics.SphereCastAll(cameraPivot.position, held.Radius,
                direction, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == playerCollider || hit.collider.attachedRigidbody == held.Body) continue;
                distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - 0.02f));
            }
            held.Body.MovePosition(cameraPivot.position + direction * distance);
            held.Body.MoveRotation(cameraPivot.rotation * view.InertiaRotation);
        }

        public void Drop()
        {
            Release(Vector3.zero);
        }

        public void ReleaseCharged(float secondsHeld)
        {
            float amount = Mathf.Clamp01(secondsHeld / chargeDuration);
            Vector3 throwVelocity = secondsHeld < tapDuration || cameraPivot == null ? Vector3.zero
                : cameraPivot.forward * Mathf.Lerp(minimumThrowSpeed, maximumThrowSpeed, amount);
            Release(throwVelocity);
        }

        private void Release(Vector3 throwVelocity)
        {
            if (held == null) return;
            held.Release((playerCollider != null ? playerCollider.velocity : Vector3.zero) + throwVelocity);
            held = null;
            charging = false;
            chargeTime = 0f;
            view.SetHeldObject(null);
        }

        private void OnGUI()
        {
            if (held == null && dot != null && input.HasControl)
                GUI.DrawTexture(new Rect((Screen.width - 7f) * 0.5f, (Screen.height - 7f) * 0.5f, 7f, 7f), dot);
            if (held != null && charging && input.HasControl)
            {
                Rect bar = new Rect(Screen.width * 0.5f - 60f, Screen.height * 0.5f + 30f, 120f, 8f);
                Color previous = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.7f);
                GUI.DrawTexture(bar, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(bar.x + 1f, bar.y + 1f, (bar.width - 2f) * Charge, bar.height - 2f), Texture2D.whiteTexture);
                GUI.color = previous;
            }
        }

        private void OnDisable()
        {
            SetHovered(null);
            Drop();
        }

        private void OnDestroy()
        {
            if (dot != null) Destroy(dot);
        }
    }
}
