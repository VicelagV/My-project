#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AllAboard.Movement.Tests
{
    public sealed class ObjectInteractionTests
    {
        private GameObject player;
        private GameObject cube;
        private GameObject floor;
        private GameObject wall;
        private PlayerInteraction interaction;
        private PickupObject item;

        [SetUp]
        public void SetUp()
        {
            player = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Movement/Prefabs/PrototypePlayer.prefab"));
            player.GetComponent<FirstPersonMotor>().enabled = false;
            interaction = player.GetComponent<PlayerInteraction>();
            cube = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Movement/Prefabs/BluePhysicsCube.prefab"));
            cube.transform.position = new Vector3(0f, 1.5f, 1f);
            item = cube.GetComponent<PickupObject>();
            item.Body.interpolation = RigidbodyInterpolation.None;
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -0.25f, 0f);
            floor.transform.localScale = new Vector3(30f, 0.5f, 30f);
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(cube);
            Object.DestroyImmediate(floor);
            if (wall != null) Object.DestroyImmediate(wall);
        }

        [UnityTest]
        public IEnumerator PickupMovesHandsAndTapDropRestoresGravity()
        {
            Assert.That(interaction.TryPickup(item), Is.True);
            Assert.That(item.Body.isKinematic, Is.True);
            yield return new WaitForFixedUpdate();
            yield return null;
            Transform hands = player.transform.Find("Player Camera/Floating Hands");
            Vector3 left = hands.Find("Left Hand").position;
            Vector3 right = hands.Find("Right Hand").position;
            Assert.That(Vector3.Dot(right - left, item.transform.right), Is.GreaterThan(0.5f));
            interaction.ReleaseCharged(0.05f);
            Assert.That(interaction.IsHolding, Is.False);
            Assert.That(item.Body.isKinematic, Is.False);
            Assert.That(item.Body.useGravity, Is.True);
            Assert.That(item.Body.linearVelocity.magnitude, Is.LessThan(0.1f));
            float start = cube.transform.position.y;
            for (int i = 0; i < 15; i++) yield return new WaitForFixedUpdate();
            Assert.That(cube.transform.position.y, Is.LessThan(start - 0.1f));
            for (int i = 0; i < 80; i++) yield return new WaitForFixedUpdate();
            Assert.That(cube.transform.position.y, Is.InRange(0.23f, 0.3f));
        }

        [Test]
        public void ChargedThrowLaunchesForwardAndLongerChargeIsStronger()
        {
            Assert.That(interaction.TryPickup(item), Is.True);
            interaction.ReleaseCharged(0.3f);
            float shortSpeed = item.Body.linearVelocity.z;
            Assert.That(shortSpeed, Is.GreaterThan(3f));
            Assert.That(interaction.TryPickup(item), Is.True);
            interaction.ReleaseCharged(1.2f);
            Assert.That(item.Body.linearVelocity.z, Is.EqualTo(12f).Within(0.01f));
            Assert.That(item.Body.linearVelocity.z, Is.GreaterThan(shortSpeed));
            Assert.That(item.Body.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode.ContinuousDynamic));
        }

        [Test]
        public void CannotPickUpAnOutOfReachCubeOrGrabAHeldItemTwice()
        {
            cube.transform.position = Vector3.forward * 10f;
            Assert.That(interaction.TryPickup(item), Is.False);
            cube.transform.position = new Vector3(0f, 1.5f, 1f);
            Assert.That(interaction.TryPickup(item), Is.True);
            Assert.That(interaction.TryPickup(item), Is.False);
            Assert.That(item.BeginHold(null), Is.False);
        }

        [UnityTest]
        public IEnumerator CrouchingLowersCameraAndHeldCubeTrailsMovement()
        {
            var motor = player.GetComponent<FirstPersonMotor>();
            Transform camera = player.transform.Find("Player Camera");
            Assert.That(motor.SetCrouched(true), Is.True);
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            Assert.That(camera.localPosition.y, Is.EqualTo(0.9f).Within(0.03f));
            cube.transform.position = camera.TransformPoint(new Vector3(0f, -0.18f, 1.1f));
            Physics.SyncTransforms();
            Assert.That(interaction.TryPickup(item), Is.True);
            for (float elapsed = 0f; elapsed < 0.5f; elapsed += Time.deltaTime)
            {
                player.transform.position += Vector3.forward * (4f * Time.deltaTime);
                yield return null;
            }
            Assert.That(player.GetComponent<FirstPersonView>().InertiaOffset.z, Is.LessThan(-0.005f));
            float heldDistance = Vector3.Dot(cube.transform.position - camera.position, camera.forward);
            Assert.That(heldDistance, Is.InRange(0.8f, 1.1f));
            interaction.Drop();
        }

        [UnityTest]
        public IEnumerator CarryPoseStaysAlignedThroughRepeatedCrouchChanges()
        {
            var motor = player.GetComponent<FirstPersonMotor>();
            var view = player.GetComponent<FirstPersonView>();
            Transform camera = player.transform.Find("Player Camera");
            item.Body.interpolation = RigidbodyInterpolation.Interpolate;
            Assert.That(interaction.TryPickup(item), Is.True);
            Assert.That(item.Body.interpolation, Is.EqualTo(RigidbodyInterpolation.None));
            var probe = player.AddComponent<GripPoseProbe>();
            probe.Camera = camera;
            probe.Cube = cube.transform;
            probe.View = view;
            for (int cycle = 0; cycle < 4; cycle++)
            {
                motor.SetCrouched(cycle % 2 == 0);
                for (float elapsed = 0f; elapsed < 0.25f; elapsed += Time.deltaTime) yield return null;
            }
            Assert.That(probe.MaximumGripError, Is.LessThan(0.015f));
            Assert.That(probe.MaximumVerticalInertia, Is.LessThan(0.005f));
            interaction.Drop();
            Assert.That(item.Body.interpolation, Is.EqualTo(RigidbodyInterpolation.Interpolate));
        }

        [UnityTest]
        public IEnumerator LookingDownWhileCrouchedKeepsHeldCubeOutOfCameraAndFloor()
        {
            player.GetComponent<PlayerInputReader>().enabled = false;
            player.GetComponent<FirstPersonMotor>().SetCrouched(true);
            Transform camera = player.transform.Find("Player Camera");
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            cube.transform.position = camera.TransformPoint(new Vector3(0f, -0.18f, 1.1f));
            Physics.SyncTransforms();
            Assert.That(interaction.TryPickup(item), Is.True);
            camera.localRotation = Quaternion.Euler(80f, 0f, 0f);
            for (float elapsed = 0f; elapsed < 0.4f; elapsed += Time.deltaTime) yield return null;
            Vector3 localCube = camera.InverseTransformPoint(cube.transform.position);
            Assert.That(localCube.z, Is.GreaterThan(0.5f));
            Assert.That(cube.GetComponent<BoxCollider>().bounds.min.y, Is.GreaterThanOrEqualTo(-0.01f));
            Transform hands = camera.Find("Floating Hands");
            Assert.That(camera.InverseTransformPoint(hands.Find("Left Hand").position).z, Is.GreaterThan(0.5f));
            Assert.That(camera.InverseTransformPoint(hands.Find("Right Hand").position).z, Is.GreaterThan(0.5f));
        }

        [UnityTest]
        public IEnumerator SprintingIntoWallKeepsCubeAndHandsInFrontWithoutPenetration()
        {
            player.GetComponent<PlayerInputReader>().enabled = false;
            var motor = player.GetComponent<FirstPersonMotor>();
            var driver = player.AddComponent<PlatformMovementDriver>();
            driver.Motor = motor;
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0f, 2f, 3f);
            wall.transform.localScale = new Vector3(8f, 4f, 0.5f);
            Physics.SyncTransforms();
            Assert.That(interaction.TryPickup(item), Is.True);
            driver.Movement = Vector2.up;
            driver.Sprint = true;
            for (float elapsed = 0f; elapsed < 1.2f; elapsed += Time.deltaTime) yield return null;
            Transform camera = player.transform.Find("Player Camera");
            Assert.That(camera.InverseTransformPoint(cube.transform.position).z, Is.GreaterThan(0.5f));
            Assert.That(Physics.ComputePenetration(cube.GetComponent<BoxCollider>(), cube.transform.position, cube.transform.rotation,
                wall.GetComponent<Collider>(), wall.transform.position, wall.transform.rotation, out _, out _), Is.False);
            Transform hands = camera.Find("Floating Hands");
            foreach (string name in new[] { "Left Hand", "Right Hand" })
            {
                Vector3 local = camera.InverseTransformPoint(hands.Find(name).position);
                Assert.That(local.z, Is.GreaterThan(0.45f));
                Assert.That(Mathf.Abs(local.x), Is.LessThan(0.65f));
            }
            float contactX = player.transform.position.x;
            float contactZ = player.transform.position.z;
            driver.Movement = Vector2.one;
            for (float elapsed = 0f; elapsed < 0.3f; elapsed += Time.deltaTime) yield return null;
            Assert.That(player.transform.position.x - contactX, Is.GreaterThan(0.5f));
            Assert.That(camera.InverseTransformPoint(cube.transform.position).z, Is.GreaterThan(0.5f));
            driver.Movement = Vector2.down;
            // Include time to brake the previous forward component before backing away.
            for (float elapsed = 0f; elapsed < 0.6f; elapsed += Time.deltaTime) yield return null;
            Assert.That(contactZ - player.transform.position.z, Is.GreaterThan(0.5f));
        }

        [UnityTest]
        public IEnumerator CrouchedFloorPickupAboveHoleDoesNotLiftOrDropPlayer()
        {
            floor.transform.position = new Vector3(0f, 1.97f, 0f);
            floor.transform.localScale = new Vector3(4f, 0.06f, 4f);
            player.transform.position = new Vector3(0f, 2.04f, 0f);
            player.GetComponent<PlayerInputReader>().enabled = false;
            var motor = player.GetComponent<FirstPersonMotor>();
            var driver = player.AddComponent<PlatformMovementDriver>();
            driver.Motor = motor;
            driver.Crouch = true;
            cube.transform.position = new Vector3(0f, 2.28f, 0.8f);
            Physics.SyncTransforms();
            for (float elapsed = 0f; elapsed < 0.3f; elapsed += Time.deltaTime) yield return null;
            Transform camera = player.transform.Find("Player Camera");
            camera.localRotation = Quaternion.Euler(80f, 0f, 0f);
            float feet = player.transform.position.y;
            Assert.That(interaction.TryPickup(item), Is.True);
            for (float elapsed = 0f; elapsed < 0.6f; elapsed += Time.deltaTime)
            {
                yield return null;
                Assert.That(player.transform.position.y, Is.EqualTo(feet).Within(0.04f));
                Assert.That(motor.Grounded, Is.True);
                Assert.That(camera.InverseTransformPoint(cube.transform.position).z, Is.GreaterThan(0.5f));
            }
            interaction.Drop();
            for (float elapsed = 0f; elapsed < 0.4f; elapsed += Time.deltaTime) yield return null;
            Assert.That(player.transform.position.y, Is.EqualTo(feet).Within(0.04f));
            Assert.That(cube.transform.position.y, Is.GreaterThan(2.2f));
        }

        [UnityTest]
        public IEnumerator HandsReturnToRestInsteadOfFollowingThrownCube()
        {
            Assert.That(interaction.TryPickup(item), Is.True);
            yield return new WaitForFixedUpdate();
            yield return null;
            interaction.ReleaseCharged(1.2f);
            for (int i = 0; i < 30; i++) yield return new WaitForFixedUpdate();
            yield return null;
            Transform hands = player.transform.Find("Player Camera/Floating Hands");
            Assert.That(hands.Find("Left Hand").localPosition.z, Is.EqualTo(0.5f).Within(0.08f));
            Assert.That(hands.Find("Right Hand").localPosition.z, Is.EqualTo(0.5f).Within(0.08f));
            Assert.That(cube.transform.position.z, Is.GreaterThan(3f));
            Assert.That(interaction.IsHolding, Is.False);
        }
    }

    [DefaultExecutionOrder(100)]
    public sealed class GripPoseProbe : MonoBehaviour
    {
        public Transform Camera;
        public Transform Cube;
        public FirstPersonView View;
        public float MaximumGripError { get; private set; }
        public float MaximumVerticalInertia { get; private set; }

        private void LateUpdate()
        {
            if (Camera == null || Cube == null || View == null) return;
            Vector3 expected = new Vector3(0f, -0.18f, 1.1f) + View.InertiaOffset;
            MaximumGripError = Mathf.Max(MaximumGripError, Vector3.Distance(Camera.InverseTransformPoint(Cube.position), expected));
            MaximumVerticalInertia = Mathf.Max(MaximumVerticalInertia, Mathf.Abs(View.InertiaOffset.y));
        }
    }
}
#endif
