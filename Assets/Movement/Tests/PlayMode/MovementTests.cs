using NUnit.Framework;
using UnityEngine;

namespace AllAboard.Movement.Tests
{
    public sealed class MovementTests
    {
        private GameObject player;
        private GameObject floor;
        private GameObject obstruction;
        private FirstPersonMotor motor;
        private const float Step = 1f / 60f;

        [SetUp]
        public void SetUp()
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -0.25f, 0f);
            floor.transform.localScale = new Vector3(100f, 0.5f, 100f);
            player = new GameObject("Test player");
            player.transform.position = Vector3.up * 0.1f;
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.center = Vector3.up * 0.9f;
            controller.radius = 0.3f;
            controller.skinWidth = 0.03f;
            controller.minMoveDistance = 0f;
            motor = player.AddComponent<FirstPersonMotor>();
            motor.enabled = false;
            Physics.SyncTransforms();
            Tick(Vector2.zero, 30);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(floor);
            if (obstruction != null) Object.DestroyImmediate(obstruction);
        }

        private void Tick(Vector2 direction, int count)
        {
            for (int i = 0; i < count; i++) motor.Simulate(direction, false, Step);
        }

        [Test]
        public void JumpLandsAndCannotJumpAgainInMidair()
        {
            Assert.That(motor.Grounded, Is.True);
            float start = player.transform.position.y;
            motor.Simulate(Vector2.zero, true, Step);
            Tick(Vector2.zero, 15);
            float height = player.transform.position.y;
            motor.Simulate(Vector2.zero, true, Step);
            Tick(Vector2.zero, 60);
            Assert.That(height - start, Is.InRange(1f, 1.3f));
            Assert.That(player.transform.position.y, Is.EqualTo(start).Within(0.05f));
            Assert.That(motor.Grounded, Is.True);
        }

        [Test]
        public void DiagonalInputDoesNotIncreaseSpeed()
        {
            Tick(Vector2.up, 60);
            float forwardSpeed = motor.HorizontalSpeed;
            Tick(Vector2.one, 60);
            Assert.That(motor.HorizontalSpeed, Is.EqualTo(forwardSpeed).Within(0.001f));
        }

        [Test]
        public void SprintIsFasterAndStillClampsDiagonalSpeed()
        {
            Tick(Vector2.up, 60);
            float walking = motor.HorizontalSpeed;
            for (int i = 0; i < 60; i++) motor.Simulate(Vector2.one, false, Step, true);
            Assert.That(motor.HorizontalSpeed, Is.EqualTo(8f).Within(0.001f));
            Assert.That(motor.HorizontalSpeed, Is.GreaterThan(walking));
        }

        [Test]
        public void WallBlocksMovement()
        {
            obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.transform.position = new Vector3(0f, 1.5f, 2f);
            obstruction.transform.localScale = new Vector3(10f, 3f, 0.5f);
            Physics.SyncTransforms();
            Tick(Vector2.up, 120);
            Assert.That(player.transform.position.z, Is.InRange(1.3f, 1.55f));
        }

        [Test]
        public void CeilingStopsJumpAndPlayerReturnsToFloor()
        {
            obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.transform.position = new Vector3(0f, 2.4f, 0f);
            obstruction.transform.localScale = new Vector3(4f, 0.3f, 4f);
            Physics.SyncTransforms();
            float start = player.transform.position.y;
            motor.Simulate(Vector2.zero, true, Step);
            float highest = start;
            for (int i = 0; i < 90; i++)
            {
                motor.Simulate(Vector2.zero, false, Step);
                highest = Mathf.Max(highest, player.transform.position.y);
            }
            Assert.That(highest - start, Is.InRange(0.2f, 0.5f));
            Assert.That(player.transform.position.y, Is.EqualTo(start).Within(0.05f));
        }
    }
}
