#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using AllAboard.Train;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AllAboard.Movement.Tests
{
    public sealed class TrainMovementTests
    {
        private Scene scene;
        private TrainController train;
        private TrainCar[] cars;
        private GameObject player;
        private FirstPersonMotor motor;
        private PlatformMovementDriver driver;
        private PlayerInteraction actor;
        private float previousMaxDelta;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousMaxDelta = Time.maximumDeltaTime; Time.maximumDeltaTime = 1f / 30f;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Movement/Scenes/TrainPlayground.unity", new LoadSceneParameters(LoadSceneMode.Additive));
            scene = SceneManager.GetSceneByPath("Assets/Movement/Scenes/TrainPlayground.unity");
            train = scene.GetRootGameObjects().Select(x => x.GetComponent<TrainController>()).First(x => x != null);
            train.enabled = false;
            cars = train.Consist.ToArray();
            player = scene.GetRootGameObjects().First(x => x.GetComponent<FirstPersonMotor>() != null);
            player.GetComponent<PlayerInputReader>().enabled = false;
            motor = player.GetComponent<FirstPersonMotor>(); motor.enabled = false;
            driver = player.AddComponent<PlatformMovementDriver>(); driver.Motor = motor;
            actor = player.GetComponent<PlayerInteraction>();
        }
        [UnityTearDown]
        public IEnumerator TearDown() { yield return SceneManager.UnloadSceneAsync(scene); Time.maximumDeltaTime = previousMaxDelta; }
        private IEnumerator Wait(float seconds) { for (float t = 0; t < seconds; t += Time.deltaTime) yield return null; }
        private void SpawnOn(TrainCar car, Vector3 local)
        {
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.SetPositionAndRotation(car.transform.TransformPoint(local), car.transform.rotation);
            player.GetComponent<CharacterController>().enabled = true;
            Physics.SyncTransforms();
        }
        private IEnumerator WalkTo(TrainCar target, float height, float timeout = 9f)
        {
            for (float elapsed = 0f; elapsed < timeout; elapsed += Time.deltaTime)
            {
                Vector3 goal = target.transform.TransformPoint(new Vector3(0f, height, 0f));
                Vector3 difference = goal - player.transform.position; difference.y = 0f;
                if (difference.magnitude < 0.35f) { driver.Movement = Vector2.zero; yield break; }
                // Steer along each car's aisle; on a curve the player has to turn between cars.
                TrainCar current = motor.SupportBody != null ? motor.SupportBody.GetComponent<TrainCar>() : null;
                if (current != null && current != target)
                {
                    TrainCar next = System.Array.IndexOf(cars, current) > System.Array.IndexOf(cars, target) ? current.Front : current.Rear;
                    if (next != null)
                    {
                        float direction = next == current.Front ? 1f : -1f;
                        Vector3 local = current.transform.InverseTransformPoint(player.transform.position);
                        Vector3 waypoint = local.z * direction < 4f ? current.transform.TransformPoint(new Vector3(0f, height, direction * 4.6f)) : next.transform.position;
                        difference = waypoint - player.transform.position;
                    }
                    difference.y = 0f;
                }
                difference.Normalize();
                driver.Movement = new Vector2(Vector3.Dot(difference, player.transform.right), Vector3.Dot(difference, player.transform.forward));
                yield return null;
                Assert.That(player.transform.position.y, Is.GreaterThan(0.8f), "Rider fell through a car or walkway.");
            }
            driver.Movement = Vector2.zero;
            Assert.Fail("Could not walk to " + target.name + "; rider local position " + target.transform.InverseTransformPoint(player.transform.position));
        }

        [UnityTest]
        public IEnumerator FourCarStaticShellAllowsFullInteriorWalkthrough()
        {
            Assert.That(cars.Length, Is.EqualTo(4));
            Assert.That(cars.Count(x => x.Kind == CarKind.Passenger), Is.EqualTo(2));
            SpawnOn(cars[3], new Vector3(0f, 1.18f, 0f));
            yield return WalkTo(cars[0], 1.18f);
            Assert.That(motor.Grounded, Is.True);
            Assert.That(cars[0].transform.InverseTransformPoint(player.transform.position).y, Is.EqualTo(1.18f).Within(0.08f));
        }

        [UnityTest]
        public IEnumerator MovingCurveCarriesIdleRiderAndAllowsCarTransitions()
        {
            train.Configure(train.Path, cars, 235f);
            SpawnOn(cars[3], new Vector3(0f, 1.18f, 0f));
            train.Throttle = 0.5f; train.enabled = true;
            yield return Wait(0.3f);
            Vector3 local = cars[3].transform.InverseTransformPoint(player.transform.position);
            yield return Wait(1f);
            Assert.That(Vector3.Distance(cars[3].transform.InverseTransformPoint(player.transform.position), local), Is.LessThan(0.1f));
            yield return WalkTo(cars[0], 1.18f);
            Assert.That(motor.Grounded, Is.True);
            Assert.That(train.Speed, Is.GreaterThan(2f));
        }

        [UnityTest]
        public IEnumerator MovingLocomotiveLadderAndRoofReturnStayStable()
        {
            SpawnOn(cars[0], new Vector3(1.1f, 1.18f, -2.7f));
            train.Throttle = 0.5f; train.enabled = true;
            TrainLadder ladder = cars[0].GetComponentsInChildren<TrainLadder>()[0];
            Assert.That(ladder.Interact(actor), Is.True);
            yield return Wait(1.4f);
            Assert.That(motor.IsClimbing, Is.False);
            Assert.That(cars[0].transform.InverseTransformPoint(player.transform.position).y, Is.InRange(4.08f, 4.2f));
            yield return WalkTo(cars[0], 4.18f);
            Assert.That(ladder.Interact(actor), Is.True);
            yield return Wait(1.4f);
            Assert.That(cars[0].transform.InverseTransformPoint(player.transform.position).y, Is.InRange(1.14f, 1.22f));
            Assert.That(motor.Grounded, Is.True);
        }

        [UnityTest]
        public IEnumerator MovingPassengerRoofsStillAllowCarTransitions()
        {
            SpawnOn(cars[3], new Vector3(0f, 4.18f, 0f));
            train.Throttle = 0.5f; train.enabled = true;
            yield return WalkTo(cars[2], 4.18f);
            yield return WalkTo(cars[3], 4.18f);
            Assert.That(motor.Grounded, Is.True);
            Assert.That(cars[3].transform.InverseTransformPoint(player.transform.position).y, Is.InRange(4.08f, 4.2f));
        }

        [UnityTest]
        public IEnumerator RoofJumpKeepsTrainVelocityAndLandsBackOnTrain()
        {
            train.Configure(train.Path, cars, 235f);
            SpawnOn(cars[2], new Vector3(0f, 4.18f, 0f));
            train.Throttle = 0.5f; train.enabled = true;
            yield return Wait(2f);
            Vector3 before = player.transform.position;
            Vector3 velocity = cars[2].Body.GetPointVelocity(before); velocity.y = 0f;
            driver.Jump = true;
            yield return Wait(0.2f);
            Assert.That(player.transform.position.y - before.y, Is.GreaterThan(0.7f));
            Vector3 moved = player.transform.position - before; moved.y = 0f;
            Assert.That(Vector3.Dot(moved, velocity.normalized), Is.EqualTo(velocity.magnitude * 0.2f).Within(0.25f));
            yield return Wait(0.8f);
            Assert.That(motor.Grounded, Is.True);
            Assert.That(motor.SupportBody.GetComponent<TrainCar>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator PhysicalCoalCanBeCarriedThroughMovingCarsAndFedIntoFirebox()
        {
            SpawnOn(cars[1], new Vector3(0f, 1.18f, 0f));
            var coal = cars[1].GetComponentInChildren<CoalItem>().GetComponent<PickupObject>();
            Transform camera = player.transform.Find("Player Camera");
            camera.rotation = Quaternion.LookRotation(coal.transform.position - camera.position);
            Physics.SyncTransforms();
            Assert.That(Physics.Raycast(camera.position, camera.forward, out RaycastHit hit, 3f), Is.True);
            Assert.That(hit.collider.GetComponent<PickupObject>(), Is.EqualTo(coal));
            Assert.That(actor.TryPickup(coal), Is.True);
            Assert.That(coal.GetComponent<CoalItem>().IsStored, Is.False);
            camera.localRotation = Quaternion.identity;
            train.Throttle = 0.25f; train.enabled = true;
            yield return WalkTo(cars[0], 1.18f);
            Assert.That(actor.HeldItem, Is.EqualTo(coal));
            float steam = train.Resources.Steam, pressure = train.Resources.Pressure;
            var firebox = cars[0].GetComponentsInChildren<TrainControl>().First(x => x.Kind == ControlKind.Firebox);
            Assert.That(firebox.Interact(actor), Is.True);
            Assert.That(train.Resources.Steam, Is.EqualTo(steam + 40f).Within(0.01f));
            Assert.That(train.Resources.Pressure, Is.EqualTo(pressure + 5f).Within(0.01f));
            Assert.That(train.Resources.CoalSupply, Is.EqualTo(9));
            Assert.That(actor.IsHolding, Is.False);
            yield return null;
            Assert.That(coal == null, Is.True);
        }

        [UnityTest]
        public IEnumerator MovingSideDoorAllowsBoardingAndReturnToStationPlatform()
        {
            SpawnOn(cars[0], new Vector3(3f, 1.18f, 1.5f));
            train.Throttle = 0.1f; train.enabled = true;
            driver.Movement = Vector2.left;
            yield return Wait(0.4f);
            driver.Movement = Vector2.zero;
            yield return Wait(0.1f);
            Assert.That(motor.SupportBody, Is.EqualTo(cars[0].Body));
            driver.Movement = Vector2.right;
            yield return Wait(0.9f);
            driver.Movement = Vector2.zero;
            yield return Wait(0.3f);
            Assert.That(motor.Grounded, Is.True);
            Assert.That(motor.SupportBody, Is.Null);
            Assert.That(player.transform.position.y, Is.InRange(1.1f, 1.22f));
            Assert.That(motor.WorldVelocity.magnitude, Is.LessThan(1f));
        }
        [UnityTest]
        public IEnumerator ConductorSeatLocksMovementAndRestrictsThrottle()
        {
            SpawnOn(cars[0], new Vector3(0f, 1.18f, 1.3f));
            ConductorSeat seat = cars[0].GetComponentInChildren<ConductorSeat>();
            TrainThrottleLever throttle = cars[0].GetComponentInChildren<TrainThrottleLever>();
            actor.enabled = false;
            Assert.That(actor.TryGrabThrottle(throttle), Is.False);
            Assert.That(train.Throttle, Is.Zero);
            Assert.That(seat.Interact(actor), Is.True);
            Assert.That(actor.TryGrabThrottle(throttle), Is.True);
            Assert.That(actor.DragThrottle(new Vector2(500f, 75f)), Is.True);
            Assert.That(train.Throttle, Is.EqualTo(0.25f));
            var view = player.GetComponent<FirstPersonView>();
            Transform camera = player.transform.Find("Player Camera");
            Quaternion beforeLook = camera.rotation;
            view.ApplyLook(new Vector2(100f, 100f));
            Assert.That(Quaternion.Angle(camera.rotation, beforeLook), Is.LessThan(0.001f));
            Assert.That(actor.DragThrottle(new Vector2(0f, -75f)), Is.True);
            Assert.That(train.Throttle, Is.Zero);
            actor.DragThrottle(new Vector2(0f, 75f));
            yield return Wait(0.3f);
            Transform hands = camera.Find("Floating Hands");
            Assert.That(Vector3.Distance(hands.Find("Right Hand").position, throttle.Handle.position + throttle.Handle.right * 0.11f), Is.LessThan(0.015f));
            Assert.That(Vector3.Distance(hands.Find("Left Hand").position, throttle.Handle.position), Is.GreaterThan(0.2f));
            actor.ReleaseThrottle();
            Assert.That(train.Throttle, Is.EqualTo(0.25f));
            Assert.That(view.IsUsingHandControl, Is.False);
            view.ApplyLook(new Vector2(10f, 10f));
            Assert.That(Quaternion.Angle(camera.rotation, beforeLook), Is.GreaterThan(0.5f));
            camera.localRotation = Quaternion.identity;
            Physics.SyncTransforms();
            Vector3 leverDirection = throttle.Handle.position - camera.position;
            Assert.That(Physics.Raycast(camera.position, leverDirection.normalized, out RaycastHit leverHit, 3f), Is.True);
            Assert.That(leverHit.collider.GetComponentInParent<TrainThrottleLever>(), Is.EqualTo(throttle));
            foreach (TrainControl control in cars[0].GetComponentsInChildren<TrainControl>())
            {
                Vector3 direction = control.transform.position - camera.position;
                Assert.That(direction.magnitude, Is.LessThan(3f));
                Assert.That(Physics.Raycast(camera.position, direction.normalized, out RaycastHit hit, 3f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore), Is.True);
                Assert.That(hit.collider.GetComponent<TrainControl>(), Is.EqualTo(control), "Target " + control.name + " hit " + hit.collider.name + " at " + hit.point);
            }
            train.enabled = true; driver.Movement = Vector2.one; driver.Jump = true;
            Vector3 local = cars[0].transform.InverseTransformPoint(player.transform.position);
            yield return Wait(1f);
            Assert.That(motor.IsSeated, Is.True);
            Assert.That(Vector3.Distance(cars[0].transform.InverseTransformPoint(player.transform.position), local), Is.LessThan(0.015f));
            Assert.That(train.Speed, Is.GreaterThan(0f));
            var speedometer = cars[0].GetComponentsInChildren<TrainInstrument>().First(x => x.Condition != null);
            speedometer.Refresh(); StringAssert.Contains(train.Speed.ToString("F1"), speedometer.DisplayedText);
            Assert.That(actor.TryGrabThrottle(throttle), Is.True);
            Assert.That(motor.ExitSeat(), Is.True);
            Assert.That(actor.DragThrottle(new Vector2(0f, 75f)), Is.False);
            Assert.That(actor.IsUsingHandControl, Is.False);
            driver.Movement = Vector2.zero;
            Assert.That(actor.TryGrabThrottle(throttle), Is.False);
            yield return Wait(0.3f);
            Assert.That(motor.Grounded, Is.True);
        }

        [Test]
        public void LeverGrabUsesHeldLeftMouseButtonAndReadsForwardMotion()
        {
            Mouse previous = Mouse.current;
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            var reader = player.GetComponent<PlayerInputReader>();
            var background = InputSystem.settings.backgroundBehavior;
            var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            reader.enabled = true;
            try
            {
                InputSystem.EnableDevice(mouse); InputSystem.Update();
                InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1, delta = new Vector2(0f, 75f) }); InputSystem.Update();
                Assert.That(reader.GrabPressed, Is.True); Assert.That(reader.GrabHeld, Is.True);
                Assert.That(reader.Look.y, Is.EqualTo(75f));
                InputSystem.QueueStateEvent(mouse, new MouseState()); InputSystem.Update();
                Assert.That(reader.GrabHeld, Is.False); Assert.That(reader.GrabPressed, Is.False);
            }
            finally
            {
                reader.enabled = false;
                InputSystem.settings.backgroundBehavior = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
                InputSystem.RemoveDevice(mouse); if (previous != null && previous.added) previous.MakeCurrent();
            }
        }
        [UnityTest]
        public IEnumerator MovingDriverCanGrabBothLeversWithAnImpreciseAim()
        {
            var seat=cars[0].GetComponentInChildren<ConductorSeat>();Assert.That(seat.Interact(actor),Is.True);
            train.Throttle=0.5f;train.enabled=true;yield return Wait(0.5f);
            Assert.That(train.Speed,Is.GreaterThan(0f));
            foreach(var lever in cars[0].GetComponentsInChildren<TrainLever>())
            {
                var camera=player.GetComponentInChildren<Camera>().transform;
                camera.rotation=Quaternion.LookRotation(lever.Handle.position+cars[0].transform.right*0.14f-camera.position);
                actor.RefreshHoveredTarget();Assert.That(actor.TryGrabHoveredLever(),Is.True,lever.name);
                Assert.That(actor.DragThrottle(new Vector2(0f,30f)),Is.True);
                actor.ReleaseThrottle();
            }

        }
    }
}
#endif
