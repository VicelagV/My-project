using UnityEngine;

namespace AllAboard.Movement
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(PlayerInputReader), typeof(FirstPersonView))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        public event System.Action PickedUp;
        public event System.Action<float> Released;
        public event System.Action Welded;
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
        private PrototypeInteractable hoveredControl;
        private AllAboard.Train.TrainLever grabbedLever;
        private AllAboard.Train.RailSawControl grabbedSaw;
        private AllAboard.Train.MaintenanceWrench grabbedWrench;
        [SerializeField] private bool showInteractionPrompts = true;
        public bool IsUsingHandControl => grabbedLever != null || grabbedSaw != null || grabbedWrench != null;
        public bool TryGrabWrench(AllAboard.Train.MaintenanceWrench wrench){if(IsHolding||IsUsingHandControl||wrench==null||!wrench.Mounted||wrench.Finished)return false;grabbedWrench=wrench;view.SetHandControlTarget(wrench.transform);return true;}
        public bool TryGrabSaw(AllAboard.Train.RailSawControl saw)
        {
            if(IsHolding||IsUsingHandControl||saw==null||!saw.Mounted||saw.Finished)return false;
            grabbedSaw=saw;view.SetHandControlTarget(saw.transform);return true;
        }
        public bool IsPressingHoveredLever => input.GrabPressed && hoveredControl is AllAboard.Train.TrainLever && !IsHolding && GetComponent<FirstPersonMotor>().IsSeated;
        public void ShowInteractionPrompts(bool show) => showInteractionPrompts = show;
        public bool TryGrabThrottle(AllAboard.Train.TrainLever lever)
        {
            if (grabbedLever != null || lever == null || !lever.BeginGrip(this)) return false;
            grabbedLever = lever;
            view.SetHandControlTarget(lever.Handle);
            return true;
        }
        public bool DragThrottle(Vector2 delta)
        {
            if (grabbedLever == null || !grabbedLever.Drag(this, delta)) { ReleaseThrottle(); return false; }
            return true;
        }
        public void ReleaseThrottle()
        {
            if (grabbedLever != null) grabbedLever.EndGrip(this);
            grabbedLever = null;
            grabbedSaw = null;
            grabbedWrench=null;
            if (view != null) view.SetHandControlTarget(null);
        }
        private Texture2D dot;
        private bool charging;
        private float chargeTime;
        private const float CarryGap = 0.025f;
        private const float MinimumCarryDistance = 0.65f;
        public float Charge => Mathf.Clamp01(chargeTime / chargeDuration);

        public bool IsHolding => held != null;
        public PickupObject HeldItem => held;
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

        private void LateUpdate()
        {
            SetHovered(null);
            if (hoveredControl != null) hoveredControl.SetHighlighted(false);
            hoveredControl = null;
            if (cameraPivot == null || !input.HasControl)
            {
                ReleaseThrottle();
                charging = false;
                chargeTime = 0f;
                return;
            }
            if(grabbedWrench!=null){if(!input.GrabHeld||input.InteractPressed||Vector3.Distance(cameraPivot.position,grabbedWrench.transform.position)>reach||!grabbedWrench.Drag(input.Look.y))ReleaseThrottle();return;}
            if (grabbedSaw != null)
            {
                if(!input.GrabHeld||input.InteractPressed||Vector3.Distance(cameraPivot.position,grabbedSaw.transform.position)>reach||!grabbedSaw.Drag(input.Look.y))ReleaseThrottle();
                return;
            }
            if (grabbedLever != null)
            {
                if (!input.GrabHeld || input.InteractPressed) ReleaseThrottle();
                else DragThrottle(input.Look);
                return;
            }
            RefreshHoveredTarget();
            if(held!=null&&input.GrabPressed&&hoveredControl==null){var weapon=held.GetComponent<AllAboard.Train.WeaponItem>();if(weapon!=null){weapon.Attack(this,cameraPivot,Time.time);return;}var medicine=held.GetComponent<AllAboard.Train.MedicalItem>();if(medicine!=null){medicine.Use(this);return;}}
            if(input.GrabPressed&&hoveredControl is AllAboard.Train.MaintenanceWrench wrench&&TryGrabWrench(wrench))return;
            if(input.GrabPressed&&hoveredControl is AllAboard.Train.RailSawControl saw&&TryGrabSaw(saw))return;
            if(input.GrabPressed&&(hoveredControl is AllAboard.Train.RailwayVendor||hoveredControl!=null&&hoveredControl.GetComponent<AllAboard.Train.TrainButton>()!=null))
            {hoveredControl.Interact(this);return;}
            if (input.GrabPressed && TryGrabHoveredLever())
            {
                DragThrottle(input.Look);
                return;
            }
            if (input.InteractPressed)
            {
                var motor = GetComponent<FirstPersonMotor>();
                if (motor.IsSeated && (hoveredControl == null || !hoveredControl.AllowedWhileSeated)) { motor.ExitSeat(); return; }
                if (hoveredControl != null) { hoveredControl.Interact(this); return; }
            }
            if (held != null)
            {
                if (input.WeldPressed && TryWeld()) return;
                if (input.DropPressed) { charging = true; chargeTime = 0f; }
                if (charging && input.DropHeld) chargeTime += Time.deltaTime;
                if (charging && input.DropReleased) ReleaseCharged(chargeTime);
            }
            else if (input.WeldPressed && hovered != null && hovered.IsWelded) TryUnweld(hovered);
            else if (input.InteractPressed && hovered != null) TryPickup(hovered);
        }

        public bool TryGrabHoveredLever()=>hoveredControl is AllAboard.Train.TrainLever lever&&TryGrabThrottle(lever);
        public void RefreshHoveredTarget()
        {
            SetHovered(null);if(hoveredControl!=null)hoveredControl.SetHighlighted(false);hoveredControl=null;
            if(cameraPivot==null)return;
            // Interpolated carriage and camera transforms now describe this rendered frame.
            Physics.SyncTransforms();
            float nearestDistance = reach;
            foreach (RaycastHit hit in Physics.RaycastAll(cameraPivot.position, cameraPivot.forward, reach,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == playerCollider || (held != null && hit.collider.attachedRigidbody == held.Body) || hit.distance > nearestDistance) continue;
                nearestDistance = hit.distance;
                hoveredControl = hit.collider.GetComponentInParent<PrototypeInteractable>();
                SetHovered(held == null ? hit.collider.GetComponentInParent<PickupObject>() : null);
            }
            if(GetComponent<FirstPersonMotor>().IsSeated&&!IsHolding&&hoveredControl==null)
            {
                float nearestLever=reach;
                foreach(var hit in Physics.SphereCastAll(cameraPivot.position,0.10f,cameraPivot.forward,reach,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
                {
                    var candidate=hit.collider.GetComponentInParent<AllAboard.Train.TrainLever>();
                    if(candidate==null||hit.distance>=nearestLever)continue;
                    Vector3 direction=candidate.Handle.position-cameraPivot.position;bool blocked=false;
                    foreach(var obstacle in Physics.RaycastAll(cameraPivot.position,direction.normalized,direction.magnitude,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
                        if(obstacle.collider!=playerCollider&&obstacle.collider.GetComponentInParent<AllAboard.Train.TrainLever>()!=candidate){blocked=true;break;}
                    if(blocked)continue;
                    hoveredControl=candidate;nearestLever=hit.distance;SetHovered(null);
                }
            }
            if (hoveredControl != null) hoveredControl.SetHighlighted(true);
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
            if(item.GetComponent<AllAboard.Train.StoredRailTool>()!=null){item.transform.SetParent(null,true);Destroy(item.GetComponent<AllAboard.Train.StoredRailTool>());}
            charging = false;
            chargeTime = 0f;
            SetHovered(null);
            view.SetHeldObject(held.transform, held.GripHalfWidth);
            held.GetComponent<AllAboard.Train.WeaponItem>()?.SetHolder(this);
            PickedUp?.Invoke();
            return true;
        }

        public void UpdateHeldPose()
        {
            if (held == null || cameraPivot == null) return;
            Vector3 offset = cameraPivot.forward * (holdDistance + (charging ? Charge * 0.15f : 0f))
                + cameraPivot.up * holdVerticalOffset + cameraPivot.TransformVector(view.InertiaOffset);
            Quaternion rotation = cameraPivot.rotation * view.InertiaRotation * (held.GetComponent<AllAboard.Train.WeaponItem>()?.PoseOffset??Quaternion.identity);
            // Shape casts do not resolve an overlapping starting volume. Clear it first,
            // including thin floor edges, before finding the nearest legal grip position.
            Vector3 origin = ResolveCarryOverlap(cameraPivot.position, rotation);
            Vector3 target = cameraPivot.position + offset;
            Vector3 path = target - origin;
            float distance = path.magnitude;
            Vector3 direction = path / Mathf.Max(distance, 0.0001f);
            // Match the cube's shape; a bounding sphere pulls it too close when aiming at the floor.
            foreach (RaycastHit hit in Physics.BoxCastAll(origin, held.HalfExtents,
                direction, rotation, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == playerCollider || hit.collider.attachedRigidbody == held.Body) continue;
                distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - CarryGap));
            }
            // Held items are constrained poses. Free items resume normal fixed-step simulation on release.
            Vector3 position = ResolveCarryOverlap(origin + direction * distance, rotation);
            held.Body.position = position;
            held.Body.rotation = rotation;
            // Rigidbody setters can leave the rendered transform on the previous physics pose.
            held.transform.SetPositionAndRotation(position, rotation);
        }

        public Vector3 ConstrainCarryMovement(Vector3 movement)
        {
            if (held == null || cameraPivot == null || movement.sqrMagnitude < 1e-12f) return movement;
            Quaternion rotation = cameraPivot.rotation;
            Vector3 gripDirection = (cameraPivot.forward * holdDistance + cameraPivot.up * holdVerticalOffset).normalized;
            Vector3 origin = ResolveCarryOverlap(cameraPivot.position + gripDirection * MinimumCarryDistance, rotation);
            Vector3 allowed = Vector3.zero;
            Vector3 remaining = movement;
            // Retraction is allowed, but the body stops before it would fold the grip into
            // the camera. Project remaining movement along walls so strafing still works.
            for (int pass = 0; pass < 3 && remaining.sqrMagnitude > 1e-12f; pass++)
            {
                float distance = remaining.magnitude;
                RaycastHit nearest = default;
                bool blocked = false;
                foreach (RaycastHit hit in Physics.BoxCastAll(origin + allowed, held.HalfExtents,
                    remaining / distance, rotation, distance + CarryGap, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider == playerCollider || hit.collider.attachedRigidbody == held.Body ||
                        Vector3.Dot(remaining / distance, hit.normal) >= -0.00001f || (blocked && hit.distance >= nearest.distance)) continue;
                    nearest = hit;
                    blocked = true;
                }
                if (!blocked) { allowed += remaining; break; }
                Vector3 advance = remaining.normalized * Mathf.Clamp(nearest.distance - CarryGap, 0f, distance);
                allowed += advance;
                remaining = Vector3.ProjectOnPlane(remaining - advance, nearest.normal);
                remaining.y = 0f;
            }
            return allowed;
        }

        private Vector3 ResolveCarryOverlap(Vector3 position, Quaternion rotation)
        {
            for (int pass = 0; pass < 4; pass++)
            {
                bool moved = false;
                foreach (Collider other in Physics.OverlapBox(position, held.HalfExtents, rotation,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    if (other == playerCollider || other.attachedRigidbody == held.Body) continue;
                    if (!Physics.ComputePenetration(held.Shape, position, rotation, other,
                        other.transform.position, other.transform.rotation, out Vector3 normal, out float depth)) continue;
                    position += normal * (depth + CarryGap);
                    moved = true;
                }
                if (!moved) break;
            }
            return position;
        }

        public void Drop()
        {
            Release(Vector3.zero);
        }

        public bool TryWeld()
        {
            if (held == null || cameraPivot == null) return false;
            UpdateHeldPose();
            const float weldReach = 0.15f;
            RaycastHit nearest = default;
            bool found = false;
            foreach (RaycastHit hit in Physics.BoxCastAll(held.transform.position, held.HalfExtents,
                cameraPivot.forward, held.transform.rotation, weldReach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == playerCollider || hit.collider.attachedRigidbody == held.Body ||
                    (found && hit.distance >= nearest.distance)) continue;
                nearest = hit;
                found = true;
            }
            if (!found) return false;
            Vector3 position = held.transform.position + cameraPivot.forward * Mathf.Max(0f, nearest.distance - 0.005f);
            // Reject a snap that would bury the cube in a neighbouring surface at a corner.
            foreach (Collider other in Physics.OverlapBox(position, held.HalfExtents, held.transform.rotation,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (other == playerCollider || other.attachedRigidbody == held.Body) continue;
                if (Physics.ComputePenetration(held.Shape, position, held.transform.rotation, other,
                    other.transform.position, other.transform.rotation, out _, out float depth) && depth > 0.002f) return false;
            }
            held.Body.position = position;
            held.transform.position = position;
            if (!held.WeldTo(nearest.collider, nearest.point)) return false;
            held = null;
            charging = false;
            chargeTime = 0f;
            view.SetHeldObject(null);
            Welded?.Invoke();
            return true;
        }

        public bool TryUnweld(PickupObject item)
        {
            if (held != null || item == null || cameraPivot == null || !item.IsWelded ||
                Vector3.Distance(cameraPivot.position, item.transform.position) > reach) return false;
            // Aimed release must respect intervening walls, just like highlighting.
            float nearest = reach;
            PickupObject aimed = null;
            foreach (RaycastHit hit in Physics.RaycastAll(cameraPivot.position, cameraPivot.forward, reach,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == playerCollider || hit.distance > nearest) continue;
                nearest = hit.distance;
                aimed = hit.collider.GetComponentInParent<PickupObject>();
            }
            if (aimed != item || !item.Unweld()) return false;
            Welded?.Invoke();
            return true;
        }

        public bool ConsumeHeldItem()
        {
            if (held == null) return false;
            GameObject consumed = held.gameObject;
            held = null;
            charging = false;
            chargeTime = 0f;
            view.SetHeldObject(null);
            Destroy(consumed);
            return true;
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
            var motor = GetComponent<FirstPersonMotor>();
            held.GetComponent<AllAboard.Train.WeaponItem>()?.CancelReload();
            held.Release((motor != null ? motor.WorldVelocity : Vector3.zero) + throwVelocity);
            held = null;
            charging = false;
            chargeTime = 0f;
            view.SetHeldObject(null);
            Released?.Invoke(throwVelocity.magnitude);
        }

        private void OnGUI()
        {
            if(grabbedWrench!=null&&input.HasControl){GUI.Label(new Rect(Screen.width*.5f-140,Screen.height*.5f+35,280,24),"Turn wrench forward / back");GUI.Box(new Rect(Screen.width*.5f-90,Screen.height*.5f+60,180*grabbedWrench.Progress,12),"");}
            if(grabbedSaw!=null&&input.HasControl)
            {
                GUI.Label(new Rect(Screen.width*.5f-95f,Screen.height*.5f+35f,250f,25f),"SAW: forward / backward");
                GUI.Box(new Rect(Screen.width*.5f-90f,Screen.height*.5f+60f,180f,12f),"");
                GUI.Box(new Rect(Screen.width*.5f-90f,Screen.height*.5f+60f,180f*grabbedSaw.Progress,12f),"");
            }
            if ((held == null || held.GetComponent<AllAboard.Train.WeaponItem>()!=null) && !IsUsingHandControl && dot != null && input.HasControl)
                GUI.DrawTexture(new Rect((Screen.width - 7f) * 0.5f, (Screen.height - 7f) * 0.5f, 7f, 7f), dot);
            if (showInteractionPrompts && input.HasControl && hoveredControl != null)
                GUI.Label(new Rect(Screen.width * 0.5f - 160f, Screen.height * 0.5f + 20f, 320f, 40f), hoveredControl.Prompt);
            else if (showInteractionPrompts && input.HasControl && hovered != null && hovered.IsWelded)
                GUI.Label(new Rect(Screen.width * 0.5f - 70f, Screen.height * 0.5f + 20f, 200f, 30f), input.WeldKeyLabel + ": release weld");
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
            ReleaseThrottle();
            if (hoveredControl != null) hoveredControl.SetHighlighted(false);
            SetHovered(null);
            Drop();
        }

        private void OnDestroy()
        {
            if (dot != null) Destroy(dot);
        }
    }
}
