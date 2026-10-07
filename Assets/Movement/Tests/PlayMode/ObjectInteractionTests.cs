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
}
#endif
