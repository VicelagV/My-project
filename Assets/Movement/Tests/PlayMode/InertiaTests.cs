using NUnit.Framework;
using UnityEngine;

namespace AllAboard.Movement.Tests
{
    public sealed class InertiaTests
    {
        [Test]
        public void ForwardMotionLagsBehindAndSettlesAfterStopping()
        {
            var inertia = new ViewMotionInertia();
            float step = 1f / 60f;
            inertia.Step(Vector3.zero, Quaternion.identity, step);
            Vector3 position = Vector3.zero;
            for (int i = 0; i < 60; i++)
            {
                position += Vector3.forward * (8f * step);
                inertia.Step(position, Quaternion.identity, step);
                Assert.That(inertia.Offset.magnitude, Is.LessThanOrEqualTo(0.12001f));
            }
            Assert.That(inertia.Offset.z, Is.LessThan(-0.08f));
            for (int i = 0; i < 120; i++) inertia.Step(position, Quaternion.identity, step);
            Assert.That(inertia.Offset.magnitude, Is.LessThan(0.001f));
        }

        [Test]
        public void RightTurnLagsLeftAndTeleportClearsInertia()
        {
            var inertia = new ViewMotionInertia();
            float step = 1f / 60f;
            inertia.Step(Vector3.zero, Quaternion.identity, step);
            for (int i = 1; i <= 30; i++) inertia.Step(Vector3.zero, Quaternion.Euler(0f, i * 2f, 0f), step);
            Assert.That(inertia.Offset.x, Is.LessThan(-0.01f));
            Assert.That(inertia.Angles.y, Is.LessThan(0f));
            inertia.Step(Vector3.forward * 20f, Quaternion.identity, step);
            Assert.That(inertia.Offset, Is.EqualTo(Vector3.zero));
            Assert.That(inertia.Angles, Is.EqualTo(Vector3.zero));
        }
    }
}
