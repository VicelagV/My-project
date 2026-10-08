#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace AllAboard.Movement.Tests
{
    public sealed class WeldingTests
    {
        private GameObject player;
        private GameObject cube;
        private GameObject surface;
        private PlayerInteraction interaction;
        private PickupObject item;

        [SetUp]
        public void SetUp()
        {
            player = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Movement/Prefabs/PrototypePlayer.prefab"));
            player.GetComponent<FirstPersonMotor>().enabled = false;
            player.GetComponent<PlayerInputReader>().enabled = false;
            interaction = player.GetComponent<PlayerInteraction>();
            cube = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Movement/Prefabs/BluePhysicsCube.prefab"));
            item = cube.GetComponent<PickupObject>();
            cube.transform.position = new Vector3(0f, 1.42f, 1f);
            surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = "Weld Test Wall";
            surface.transform.position = new Vector3(0f, 1.5f, 1.65f);
            surface.transform.localScale = new Vector3(4f, 3f, 0.3f);
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(cube);
            if (surface != null) Object.DestroyImmediate(surface);
        }

        private IEnumerator Wait(float seconds)
        {
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime) yield return null;
        }

        [UnityTest]
        public IEnumerator WeldToWallStaysFixedUntilAimedWeldKeyRelease()
        {
            int feedback = 0;
            interaction.Welded += () => feedback++;
            Assert.That(interaction.TryPickup(item), Is.True);
            Assert.That(interaction.TryWeld(), Is.True);
            Assert.That(interaction.IsHolding, Is.False);
            Assert.That(item.IsWelded, Is.True);
            Assert.That(feedback, Is.EqualTo(1));
            Assert.That(interaction.TryWeld(), Is.False);
            Vector3 position = cube.transform.position;
            Quaternion rotation = cube.transform.rotation;
            yield return Wait(0.7f);
            Assert.That(Vector3.Distance(cube.transform.position, position), Is.LessThan(0.015f));
            Assert.That(Quaternion.Angle(cube.transform.rotation, rotation), Is.LessThan(1f));
            Assert.That(cube.transform.parent, Is.Null);
            Transform hands = player.transform.Find("Player Camera/Floating Hands");
            Assert.That(hands.Find("Left Hand").localPosition.z, Is.EqualTo(0.5f).Within(0.08f));
            Assert.That(interaction.TryPickup(item), Is.False);
            Assert.That(item.BeginHold(player.GetComponent<CharacterController>()), Is.False);
            player.transform.Find("Player Camera").rotation = Quaternion.LookRotation(cube.transform.position - player.transform.Find("Player Camera").position);
            Assert.That(interaction.TryUnweld(item), Is.True);
            Assert.That(item.IsWelded, Is.False);
            yield return null;
            Assert.That(cube.GetComponent<FixedJoint>(), Is.Null);
            yield return Wait(0.3f);
            Assert.That(cube.transform.position.y, Is.LessThan(position.y - 0.2f));
            cube.transform.position = new Vector3(0f, 1.42f, 1f);
            Physics.SyncTransforms();
            Assert.That(interaction.TryPickup(item), Is.True);
        }

        [UnityTest]
        public IEnumerator WeldReleaseRequiresAimAndClearLineOfSight()
        {
            Assert.That(interaction.TryPickup(item), Is.True);
            Assert.That(interaction.TryWeld(), Is.True);
            Transform camera = player.transform.Find("Player Camera");
            camera.rotation = Quaternion.LookRotation(Vector3.back);
            Assert.That(interaction.TryUnweld(item), Is.False);
            camera.rotation = Quaternion.LookRotation(cube.transform.position - camera.position);
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                blocker.transform.position = Vector3.Lerp(camera.position, cube.transform.position, 0.5f);
                blocker.transform.localScale = Vector3.one * 0.2f;
                Physics.SyncTransforms();
                Assert.That(interaction.TryUnweld(item), Is.False);
            }
            finally { Object.DestroyImmediate(blocker); }
            yield return null;
            Assert.That(interaction.TryUnweld(item), Is.True);
            Assert.That(interaction.IsHolding, Is.False);
        }
        [Test]
        public void WeldUsesPhysicalYKeyForGermanZ()
        {
            Keyboard previous = Keyboard.current;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            var reader = player.GetComponent<PlayerInputReader>();
            reader.enabled = true;
            var background = InputSystem.settings.backgroundBehavior;
            var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            // The headless runner has no focused Game view; route the virtual device to gameplay.
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            try
            {
                InputSystem.EnableDevice(keyboard);
                InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Y));
                InputSystem.Update();
                Assert.That(keyboard.yKey.isPressed, Is.True);
                Assert.That(reader.WeldPressed, Is.True);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                Assert.That(reader.WeldPressed, Is.False);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Z));
                InputSystem.Update();
                Assert.That(reader.WeldPressed, Is.False);
            }
            finally
            {
                reader.enabled = false;
                InputSystem.settings.backgroundBehavior = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
                InputSystem.RemoveDevice(keyboard);
                if (previous != null && previous.added) previous.MakeCurrent();
            }
        }

        [Test]
        public void CannotWeldWithoutNearbySurfaceOrWhileEmptyHanded()
        {
            Assert.That(interaction.TryWeld(), Is.False);
            surface.transform.position = Vector3.forward * 8f;
            Physics.SyncTransforms();
            Assert.That(interaction.TryPickup(item), Is.True);
            Assert.That(interaction.TryWeld(), Is.False);
            Assert.That(interaction.IsHolding, Is.True);
            Assert.That(item.IsWelded, Is.False);
            Assert.That(cube.GetComponent<FixedJoint>(), Is.Null);
        }

        [UnityTest]
        public IEnumerator WeldedCubeFollowsMovingBodyAndCarriesRider()
        {
            var platform = surface.AddComponent<MovingPlatform>();
            platform.Configure(new Vector3(2f, 0f, 0f), 8f, 10f);
            Physics.SyncTransforms();
            Assert.That(interaction.TryPickup(item), Is.True);
            Assert.That(interaction.TryWeld(), Is.True);
            Assert.That(cube.GetComponent<FixedJoint>().connectedBody, Is.EqualTo(surface.GetComponent<Rigidbody>()));
            player.SetActive(false);
            yield return Wait(0.2f);
            Vector3 local = surface.transform.InverseTransformPoint(cube.transform.position);
            Quaternion localRotation = Quaternion.Inverse(surface.transform.rotation) * cube.transform.rotation;
            Vector3 start = cube.transform.position;
            player.transform.position = cube.transform.position + Vector3.up * 0.29f;
            var driver = player.AddComponent<PlatformMovementDriver>();
            driver.Motor = player.GetComponent<FirstPersonMotor>();
            player.SetActive(true);
            Physics.SyncTransforms();
            yield return Wait(0.15f);
            Vector3 riderLocal = cube.transform.InverseTransformPoint(player.transform.position);
            yield return Wait(0.7f);
            Assert.That(Vector3.Distance(cube.transform.position, start), Is.GreaterThan(0.5f));
            Assert.That(Vector3.Distance(surface.transform.InverseTransformPoint(cube.transform.position), local), Is.LessThan(0.025f));
            Assert.That(Quaternion.Angle(Quaternion.Inverse(surface.transform.rotation) * cube.transform.rotation, localRotation), Is.LessThan(1f));
            Assert.That(Vector3.Distance(cube.transform.InverseTransformPoint(player.transform.position), riderLocal), Is.LessThan(0.1f));
            Assert.That(driver.Motor.Grounded, Is.True);
        }

        [UnityTest]
        public IEnumerator RemovingWeldWallLetsCubeFall()
        {
            Assert.That(interaction.TryPickup(item), Is.True);
            Assert.That(interaction.TryWeld(), Is.True);
            float height = cube.transform.position.y;
            Object.Destroy(surface);
            yield return Wait(0.4f);
            Assert.That(item.IsWelded, Is.False);
            Assert.That(cube.transform.position.y, Is.LessThan(height - 0.3f));
        }
    }
}
#endif
