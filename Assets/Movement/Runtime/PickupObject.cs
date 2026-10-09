using System.Collections;
using UnityEngine;

namespace AllAboard.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public sealed class PickupObject : MonoBehaviour
    {
        [SerializeField] private Renderer hoverOutline;
        private Rigidbody body;
        private BoxCollider box;
        private Collider holder;
        private Coroutine restoreCollision;
        private CollisionDetectionMode releasedCollisionMode;
        private RigidbodyInterpolation releasedInterpolation;
        private FixedJoint weld;
        private Collider weldedSurface;
        public bool IsHeld { get; private set; }
        public bool IsWelded => weld != null;
        public Rigidbody WeldedBody => weld != null ? weld.connectedBody : null;
        public Rigidbody Body => body;
        public BoxCollider Shape => box;
        public float Radius => Vector3.Scale(box.size, transform.lossyScale).magnitude * 0.5f;
        public Vector3 HalfExtents => Vector3.Scale(box.size, transform.lossyScale) * 0.5f;
        public float GripHalfWidth => Mathf.Abs(box.size.x * transform.lossyScale.x) * 0.5f;

        public void Configure(Renderer outline) => hoverOutline = outline;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            box = GetComponent<BoxCollider>();
            SetHighlighted(false);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (hoverOutline != null) hoverOutline.enabled = highlighted && !IsHeld;
        }

        public bool BeginHold(Collider playerCollider)
        {
            if (IsHeld || IsWelded) return false;
            GetComponent<AllAboard.Train.CoalItem>()?.TakeFromStorage();
            if (restoreCollision != null)
            {
                StopCoroutine(restoreCollision);
                restoreCollision = null;
            }
            if (holder != null) Physics.IgnoreCollision(box, holder, false);
            holder = playerCollider;
            if (holder != null) Physics.IgnoreCollision(box, holder, true);
            if(!body.isKinematic){body.linearVelocity = Vector3.zero;body.angularVelocity = Vector3.zero;}
            releasedCollisionMode = body.collisionDetectionMode;
            releasedInterpolation = body.interpolation;
            body.interpolation = RigidbodyInterpolation.None;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.isKinematic = true;
            IsHeld = true;
            SetHighlighted(false);
            return true;
        }

        public void Release(Vector3 velocity)
        {
            if (!IsHeld) return;
            IsHeld = false;
            body.isKinematic = false;
            body.collisionDetectionMode = releasedCollisionMode;
            body.interpolation = releasedInterpolation;
            body.useGravity = true;
            body.linearVelocity = velocity;
            body.WakeUp();
            // A drop aimed down can overlap the player: restore collision after separation.
            if (holder != null && isActiveAndEnabled) restoreCollision = StartCoroutine(RestorePlayerCollision());
        }

        public bool WeldTo(Collider surface, Vector3 contactPoint)
        {
            if (!IsHeld || surface == null || surface.isTrigger || surface.attachedRigidbody == body || surface == holder) return false;
            Rigidbody connected = surface.attachedRigidbody;
            Release(connected != null ? connected.GetPointVelocity(contactPoint) : Vector3.zero);
            body.angularVelocity = connected != null ? connected.angularVelocity : Vector3.zero;
            weldedSurface = surface;
            weld = gameObject.AddComponent<FixedJoint>();
            weld.autoConfigureConnectedAnchor = false;
            weld.connectedBody = connected;
            weld.anchor = transform.InverseTransformPoint(contactPoint);
            weld.connectedAnchor = connected != null ? connected.transform.InverseTransformPoint(contactPoint) : contactPoint;
            weld.enableCollision = false;
            weld.breakForce = weld.breakTorque = Mathf.Infinity;
            return true;
        }

        private void RemoveWeld()
        {
            if (weld != null)
            {
                // Restore the body pair before destroying the joint so resting items
                // immediately regain floor contacts when their weld is released.
                weld.enableCollision = true;
                weld.connectedBody = null;
                Destroy(weld);
            }
            weld = null;
            weldedSurface = null;
            if (body != null && !body.isKinematic) body.WakeUp();
        }

        public bool Unweld()
        {
            if (!IsWelded || IsHeld) return false;
            Vector3 velocity = weld.connectedBody != null ? weld.connectedBody.GetPointVelocity(body.position) : Vector3.zero;
            Vector3 angular = weld.connectedBody != null ? weld.connectedBody.angularVelocity : Vector3.zero;
            RemoveWeld();
            // Rebuild contacts after the joint suppressed collisions with its connected body.
            box.enabled = false;
            box.enabled = true;
            body.linearVelocity = velocity;
            body.angularVelocity = angular;
            return true;
        }

        private void FixedUpdate()
        {
            // A static-wall weld must not leave an invisible world anchor if the wall disappears.
            if (weld != null && weldedSurface == null) RemoveWeld();
        }

        private IEnumerator RestorePlayerCollision()
        {
            while (holder != null && box.bounds.Intersects(holder.bounds)) yield return new WaitForFixedUpdate();
            if (holder != null) Physics.IgnoreCollision(box, holder, false);
            holder = null;
            restoreCollision = null;
        }

        private void OnDisable()
        {
            if (body == null) return;
            if (IsHeld) Release(Vector3.zero);
            if (holder != null) Physics.IgnoreCollision(box, holder, false);
            holder = null;
            restoreCollision = null;
        }
    }
}
