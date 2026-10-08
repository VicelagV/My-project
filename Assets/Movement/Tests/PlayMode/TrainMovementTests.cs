#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using AllAboard.Train;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
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
                    if (next != null) difference = next.transform.position - player.transform.position;
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
        public IEnumerator MovingLadderFullRoofWalkAndReturnInsideStayStable()
        {
            SpawnOn(cars[0], new Vector3(1.1f, 1.18f, -2.7f));
            train.Throttle = 0.5f; train.enabled = true;
            TrainLadder ladder = cars[0].GetComponentsInChildren<TrainLadder>()[0];
            Assert.That(ladder.Interact(actor), Is.True);
            yield return Wait(1.4f);
            Assert.That(motor.IsClimbing, Is.False);
            Assert.That(cars[0].transform.InverseTransformPoint(player.transform.position).y, Is.InRange(4.08f, 4.2f));
            yield return WalkTo(cars[3], 4.18f);
            yield return WalkTo(cars[0], 4.18f);
            Assert.That(ladder.Interact(actor), Is.True);
            yield return Wait(1.4f);
            Assert.That(cars[0].transform.InverseTransformPoint(player.transform.position).y, Is.InRange(1.14f, 1.22f));
            Assert.That(motor.Grounded, Is.True);
        }

        [UnityTest]
        public IEnumerator RoofJumpKeepsTrainVelocityAndLandsBackOnTrain()
        {
            train.Configure(train.Path, cars, 235f);
            SpawnOn(cars[1], new Vector3(0f, 4.18f, 0f));
            train.Throttle = 0.5f; train.enabled = true;
            yield return Wait(2f);
            Vector3 before = player.transform.position;
            Vector3 velocity = cars[1].Body.GetPointVelocity(before); velocity.y = 0f;
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
            SpawnOn(cars[3], new Vector3(0f, 1.18f, 0f));
            var coal = scene.GetRootGameObjects().Select(x => x.GetComponent<CoalItem>()).First(x => x != null).GetComponent<PickupObject>();
            coal.Body.position = player.transform.Find("Player Camera").TransformPoint(Vector3.forward);
            Physics.SyncTransforms();
            Assert.That(actor.TryPickup(coal), Is.True);
            train.Throttle = 0.25f; train.enabled = true;
            yield return WalkTo(cars[0], 1.18f);
            Assert.That(actor.HeldItem, Is.EqualTo(coal));
            float steam = train.Resources.Steam, pressure = train.Resources.Pressure;
            var firebox = cars[0].GetComponentsInChildren<TrainControl>().First(x => x.Kind == ControlKind.Firebox);
            Assert.That(firebox.Interact(actor), Is.True);
            Assert.That(train.Resources.Steam, Is.EqualTo(steam + 10f).Within(0.01f));
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
            TrainControl throttle = cars[0].GetComponentsInChildren<TrainControl>().First(x => x.Kind == ControlKind.ThrottleUp);
            Assert.That(throttle.Interact(actor), Is.False);
            StringAssert.Contains("Sit", throttle.Prompt);
            Assert.That(train.Throttle, Is.Zero);
            Assert.That(seat.Interact(actor), Is.True);
            Assert.That(throttle.Interact(actor), Is.True);
            Assert.That(train.Throttle, Is.EqualTo(0.25f));
            var lower = cars[0].GetComponentsInChildren<TrainControl>().First(x => x.Kind == ControlKind.ThrottleDown);
            Assert.That(lower.Interact(actor), Is.True); Assert.That(train.Throttle, Is.Zero);
            Assert.That(throttle.Interact(actor), Is.True);
            yield return Wait(0.3f);
            Physics.SyncTransforms();
            Transform camera = player.transform.Find("Player Camera");
            foreach (TrainControl control in cars[0].GetComponentsInChildren<TrainControl>())
            {
                Vector3 direction = control.transform.position - camera.position;
                Assert.That(direction.magnitude, Is.LessThan(3f), control.Kind + " must be reachable from the seat.");
                Assert.That(Physics.Raycast(camera.position, direction.normalized, out RaycastHit hit, 3f), Is.True);
                Assert.That(hit.collider.GetComponent<TrainControl>(), Is.EqualTo(control), control.Kind + " must have clear aim from the seat.");
            }
            train.enabled = true; driver.Movement = Vector2.one; driver.Jump = true;
            Vector3 local = cars[0].transform.InverseTransformPoint(player.transform.position);
            yield return Wait(1f);
            Assert.That(motor.IsSeated, Is.True);
            Assert.That(Vector3.Distance(cars[0].transform.InverseTransformPoint(player.transform.position), local), Is.LessThan(0.015f));
            Assert.That(motor.ExitSeat(), Is.True);
            driver.Movement = Vector2.zero;
            Assert.That(throttle.Interact(actor), Is.False);
            yield return Wait(0.3f);
            Assert.That(motor.Grounded, Is.True);
        }
    }
}
#endif
