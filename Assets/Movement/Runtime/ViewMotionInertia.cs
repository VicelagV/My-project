using UnityEngine;

namespace AllAboard.Movement
{
    // Bounded, damped visual inertia; it does not change player movement or aim.
    public sealed class ViewMotionInertia
    {
        public Vector3 Offset { get; private set; }
        public Vector3 Angles { get; private set; }
        private Vector3 offsetVelocity;
        private Vector3 angleVelocity;
        private Vector3 previousPosition;
        private Quaternion previousRotation;
        private bool initialized;

        public void Step(Vector3 position, Quaternion rotation, float deltaTime,
            float movementLag = 0.015f, float turnLag = 0.0003f, float responseTime = 0.1f,
            float maximumOffset = 0.12f, float maximumAngle = 5f)
        {
            if (deltaTime <= 0f) return;
            if (!initialized || (position - previousPosition).sqrMagnitude > 25f)
            {
                Offset = Angles = offsetVelocity = angleVelocity = Vector3.zero;
                previousPosition = position;
                previousRotation = rotation;
                initialized = true;
                return;
            }
            Vector3 localVelocity = Quaternion.Inverse(rotation) * ((position - previousPosition) / deltaTime);
            Vector3 turn = (Quaternion.Inverse(previousRotation) * rotation).eulerAngles;
            Vector2 turnRate = new Vector2(Mathf.DeltaAngle(0f, turn.x), Mathf.DeltaAngle(0f, turn.y)) / deltaTime;
            Vector3 target = -localVelocity * movementLag + new Vector3(-turnRate.y, turnRate.x, 0f) * turnLag;
            target = Vector3.ClampMagnitude(target, maximumOffset);
            Vector3 targetAngles = Vector3.ClampMagnitude(new Vector3(-turnRate.x, -turnRate.y, 0f) * 0.018f, maximumAngle);
            Vector3 offset = Vector3.SmoothDamp(Offset, target, ref offsetVelocity, responseTime, Mathf.Infinity, deltaTime);
            Vector3 angles = Vector3.SmoothDamp(Angles, targetAngles, ref angleVelocity, responseTime, Mathf.Infinity, deltaTime);
            Offset = Vector3.ClampMagnitude(offset, maximumOffset);
            Angles = Vector3.ClampMagnitude(angles, maximumAngle);
            previousPosition = position;
            previousRotation = rotation;
        }
    }
}
