#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AllAboard.Movement.Tests
{
    public sealed class MovingPlatformTests
    {
        private GameObject platform;
        private GameObject player;
        private GameObject cube;
        private PlatformMovementDriver driver;
        private FirstPersonMotor motor;

        [SetUp]
        public void SetUp()
        {
            platform = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Movement/Prefabs/MovingTestPlatform.prefab"));
            player = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Movement/Prefabs/PrototypePlayer.prefab"));
            player.transform.position = new Vector3(0.5f, 0.24f, -0.5f);
            player.GetComponent<PlayerInputReader>().enabled = false;
            motor = player.GetComponent<FirstPersonMotor>();
            motor.enabled = false;
            driver = player.AddComponent<PlatformMovementDriver>();
            driver.Motor = motor;
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            if (cube != null) Object.DestroyImmediate(cube);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(platform);
        }

        private IEnumerator Wait(float seconds)
        {
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime) yield return null;
        }

        private PickupObject SpawnCube(Vector3 position)
        {
            cube = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Movement/Prefabs/BluePhysicsCube.prefab"));
            cube.transform.position = position;
            Physics.SyncTransforms();
            return cube.GetComponent<PickupObject>();
        }

        [UnityTest]
        public IEnumerator IdleRiderFollowsTranslationAndRotationWithoutWalkingOrHandLag()
        {
            yield return Wait(0.2f);
            Vector3 local = platform.transform.InverseTransformPoint(player.transform.position);
            float heading = Mathf.DeltaAngle(platform.transform.eulerAngles.y, player.transform.eulerAngles.y);
            Vector3 start = player.transform.position;
            yield return Wait(1f);
            Assert.That(Vector3.Distance(player.transform.position, start), Is.GreaterThan(0.5f));
            Assert.That(Vector3.Distance(platform.transform.InverseTransformPoint(player.transform.position), local), Is.LessThan(0.08f));
            Assert.That(Mathf.DeltaAngle(platform.transform.eulerAngles.y, player.transform.eulerAngles.y), Is.EqualTo(heading).Within(0.5f));
            Assert.That(motor.Grounded, Is.True);
            Assert.That(motor.RelativeGroundDistance, Is.LessThan(0.005f));
            Assert.That(player.GetComponent<FirstPersonView>().InertiaOffset.magnitude, Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator JumpRetainsPlatformMomentumAndLeavesTheDeck()
        {
            yield return Wait(0.3f);
            Vector3 velocity = motor.WorldVelocity;
            Assert.That(velocity.x, Is.GreaterThan(1f));
            float startX = player.transform.position.x;
            float startY = player.transform.position.y;
            driver.Jump = true;
            yield return Wait(0.2f);
            Assert.That(player.transform.position.y - startY, Is.GreaterThan(0.7f));
            Assert.That(player.transform.position.x - startX, Is.EqualTo(velocity.x * 0.2f).Within(0.15f));
            Assert.That(motor.Grounded, Is.False);
        }

        [UnityTest]
        public IEnumerator LooseCubeTravelsOnDeckThroughFriction()
        {
            player.SetActive(false);
            SpawnCube(new Vector3(1f, 0.48f, 0f));
            yield return Wait(0.4f);
            Vector3 local = platform.transform.InverseTransformPoint(cube.transform.position);
            Vector3 start = cube.transform.position;
            yield return Wait(1f);
            Vector3 current = platform.transform.InverseTransformPoint(cube.transform.position);
            Assert.That(cube.transform.position.x - start.x, Is.GreaterThan(0.5f));
            Assert.That(Vector3.Distance(current, local), Is.LessThan(0.25f));
            Assert.That(current.y, Is.InRange(0.43f, 0.55f));
            Assert.That(cube.transform.parent, Is.Null);
        }

        [UnityTest]
        public IEnumerator HeldCubeStaysAlignedWhileCrouchingAndDropInheritsDeckVelocity()
        {
            yield return Wait(0.3f);
            Transform camera = player.transform.Find("Player Camera");
            var interaction = player.GetComponent<PlayerInteraction>();
            PickupObject item = SpawnCube(camera.TransformPoint(new Vector3(0f, -0.18f, 1.1f)));
            Assert.That(interaction.TryPickup(item), Is.True);
            var probe = player.AddComponent<GripPoseProbe>();
            probe.Camera = camera;
            probe.Cube = cube.transform;
            probe.View = player.GetComponent<FirstPersonView>();
            driver.Crouch = true;
            yield return Wait(0.3f);
            Assert.That(motor.IsCrouched, Is.True);
            driver.Crouch = false;
            yield return Wait(0.3f);
            Assert.That(motor.IsCrouched, Is.False);
            Assert.That(probe.MaximumGripError, Is.LessThan(0.015f));
            Vector3 velocity = motor.WorldVelocity;
            interaction.Drop();
            Assert.That(item.Body.linearVelocity, Is.EqualTo(velocity));
            Assert.That(item.Body.linearVelocity.x, Is.GreaterThan(1f));
        }

        [UnityTest]
        public IEnumerator ReleasingCrouchUnderMovingRoofStandsAsSoonAsClear()
        {
            player.transform.position = new Vector3(-1.4f, 0.24f, 0f);
            driver.Crouch = true;
            Physics.SyncTransforms();
            yield return Wait(0.2f);
            driver.Crouch = false;
            yield return Wait(0.2f);
            Assert.That(motor.IsCrouched, Is.True);
            driver.Movement = Vector2.right;
            yield return Wait(0.65f);
            Assert.That(motor.IsCrouched, Is.False);
            Assert.That(motor.Grounded, Is.True);
        }
    }

    public sealed class PlatformMovementDriver : MonoBehaviour
    {
        public FirstPersonMotor Motor;
        public Vector2 Movement;
        public bool Crouch;
        public bool Jump;
        private void Update()
        {
            if (Motor == null) return;
            Motor.SetCrouched(Crouch);
            Motor.Simulate(Movement, Jump, Time.deltaTime);
            Jump = false;
        }
    }
}
#endif
