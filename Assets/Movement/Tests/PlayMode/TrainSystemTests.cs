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
    public sealed class TrainSystemTests
    {
        private Scene scene;
        private TrainController train;
        private TrainCar[] cars;
        private PlayerInteraction actor;
        private float previousMaxDelta;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousMaxDelta = Time.maximumDeltaTime; Time.maximumDeltaTime = 1f / 30f;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Movement/Scenes/TrainPlayground.unity", new LoadSceneParameters(LoadSceneMode.Additive));
            scene = SceneManager.GetSceneByPath("Assets/Movement/Scenes/TrainPlayground.unity");
            train = scene.GetRootGameObjects().Select(x => x.GetComponent<TrainController>()).First(x => x != null);
            train.enabled = false; cars = train.Consist.ToArray();
            actor = scene.GetRootGameObjects().Select(x => x.GetComponent<PlayerInteraction>()).First(x => x != null);
            actor.GetComponent<PlayerInputReader>().enabled = false;
            actor.GetComponent<FirstPersonMotor>().enabled = false;
        }
        [UnityTearDown]
        public IEnumerator TearDown() { yield return SceneManager.UnloadSceneAsync(scene); Time.maximumDeltaTime = previousMaxDelta; }
        private IEnumerator Wait(float seconds) { for (float t = 0f; t < seconds; t += Time.deltaTime) yield return null; }
        private TrainControl Control(ControlKind kind) => cars[0].GetComponentsInChildren<TrainControl>().First(x => x.Kind == kind);
        private void StepFor(float seconds) { for (float t = 0f; t < seconds; t += 0.02f) train.Step(0.02f); }

        [Test]
        public void SteamRangesCoalVentAndBrakesUseIndependentResources()
        {
            var resources = train.Resources;
            resources.SetLevels(10f, 40f); float low = resources.Performance;
            resources.SetLevels(50f, 40f); Assert.That(resources.Performance, Is.GreaterThan(low));
            resources.Step(0.5f, false, 1f); float normalDrain = 50f - resources.Steam, normalPressure = resources.Pressure - 40f;
            resources.SetLevels(90f, 40f); Assert.That(resources.Performance, Is.GreaterThan(1f));
            resources.Step(0.5f, false, 1f);
            Assert.That(90f - resources.Steam, Is.EqualTo(normalDrain).Within(0.001f));
            Assert.That(resources.Pressure - 40f, Is.GreaterThan(normalPressure));
            resources.SetLevels(90f, 95f); Assert.That(resources.AddCoal(), Is.True);
            Assert.That(resources.Steam, Is.EqualTo(100f)); Assert.That(resources.Pressure, Is.EqualTo(100f));
            Assert.That(resources.Critical, Is.True);
            resources.Vent(); Assert.That(resources.Steam, Is.EqualTo(90f)); Assert.That(resources.Pressure, Is.EqualTo(80f));
            resources.Step(0.5f, false, 1f); Assert.That(resources.Critical, Is.False);
            resources.SetLevels(0f, 30f); resources.Step(1f, true, 1f);
            Assert.That(resources.Steam, Is.Zero); Assert.That(resources.Pressure, Is.LessThan(30f));
            Assert.That(resources.BrakeEfficiency, Is.EqualTo(1f));
        }

        [Test]
        public void SteamBalanceMatchesFortyPerCoalAndHalfPerSecondAtFullThrottle()
        {
            var resources = train.Resources;
            resources.SetLevels(10f, 30f);
            Assert.That(resources.AddCoal(), Is.True);
            Assert.That(resources.Steam, Is.EqualTo(50f));
            Assert.That(resources.Pressure, Is.EqualTo(35f));
            resources.Step(1f, false, 2f); Assert.That(resources.Steam, Is.EqualTo(49f).Within(0.001f));
            resources.Step(0.5f, false, 2f); Assert.That(resources.Steam, Is.EqualTo(48.5f).Within(0.001f));
            resources.Step(0f, false, 100f); Assert.That(resources.Steam, Is.EqualTo(48.5f).Within(0.001f));
            resources.Step(1f, true, 10f); Assert.That(resources.Steam, Is.EqualTo(48.5f).Within(0.001f));
            resources.SetLevels(50f, 40f);
            resources.Step(1f, false, 80f); Assert.That(resources.Steam, Is.EqualTo(10f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator StoredCoalStaysFixedOnCurveAndBecomesPhysicalAfterPickup()
        {
            Assert.That(cars[1].Kind, Is.EqualTo(CarKind.Coal));
            Assert.That(cars[2].Kind, Is.EqualTo(CarKind.Passenger));
            Assert.That(cars[3].Kind, Is.EqualTo(CarKind.Passenger));
            Assert.That(cars[1].transform.Find("Walkable Roof"), Is.Null);
            Assert.That(cars[1].GetComponentInChildren<TrainDoor>(), Is.Null);
            var coal = cars[1].GetComponentsInChildren<CoalItem>();
            Assert.That(coal.Length, Is.EqualTo(10));
            var initial = coal.Select(x => x.transform.localPosition).ToArray();
            train.Configure(train.Path, cars, 235f);
            train.Throttle = 1f; train.enabled = true;
            yield return Wait(3f);
            for (int i = 0; i < coal.Length; i++)
            {
                Assert.That(coal[i].IsStored, Is.True);
                Assert.That(coal[i].GetComponent<Rigidbody>().isKinematic, Is.True);
                Assert.That(Vector3.Distance(coal[i].transform.localPosition, initial[i]), Is.LessThan(0.001f));
            }
            var item = coal[0].GetComponent<PickupObject>();
            Assert.That(item.BeginHold(actor.GetComponent<CharacterController>()), Is.True);
            Assert.That(coal[0].IsStored, Is.False);
            Assert.That(item.transform.parent, Is.Null);
            item.Release(cars[1].Body.GetPointVelocity(item.transform.position));
            Assert.That(item.Body.isKinematic, Is.False);
            Assert.That(item.Body.useGravity, Is.True);
        }

        [Test]
        public void SceneOnlyHasGaugeLabelsAndNoThrottleButtons()
        {
            Assert.That(scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<MonoBehaviour>(true)).Any(x => x == null), Is.False, "Scene must have no missing scripts.");
            var labels = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<TextMesh>()).ToArray();
            Assert.That(labels.Length, Is.EqualTo(3));
            Assert.That(labels.All(x => x.GetComponent<TrainInstrument>() != null), Is.True);
            Assert.That(cars[0].GetComponentsInChildren<TrainThrottleLever>().Length, Is.EqualTo(1));
        }

        [Test]
        public void SteamAndWheelAudioFollowThrottleSpeedAndStopWithoutSteam()
        {
            var audio = train.GetComponent<TrainDriveAudio>();
            Assert.That(audio, Is.Not.Null);
            audio.enabled = false;
            train.Throttle = 0f; audio.Tick(1f);
            Assert.That(audio.SteamVolume, Is.Zero); Assert.That(audio.RollingVolume, Is.Zero);
            var loops = cars.SelectMany(x => x.GetComponentsInChildren<AudioSource>()).Where(x => x.loop).ToArray();
            Assert.That(loops.Length, Is.EqualTo(5));
            Assert.That(loops.All(x => x.clip != null && x.clip.length >= 3.9f && x.spatialBlend == 1f), Is.True);
            train.Throttle = 0.5f; train.Step(1f); audio.Tick(1f);
            float half = audio.SteamVolume;
            Assert.That(half, Is.GreaterThan(0f)); Assert.That(audio.RollingVolume, Is.GreaterThan(0f));
            train.Throttle = 1f; audio.Tick(1f); Assert.That(audio.SteamVolume, Is.GreaterThan(half));
            train.Throttle = 0f; train.Braking = true; train.Step(2f); audio.Tick(1f);
            Assert.That(audio.SteamVolume, Is.Zero); Assert.That(audio.RollingVolume, Is.Zero);
            train.Resources.SetLevels(0f, 40f); train.Throttle = 1f; train.Braking = false; audio.Tick(1f);
            Assert.That(audio.SteamVolume, Is.Zero);
        }

        [Test]
        public void WeightSlowsAccelerationWithoutAdditionalSteamConsumption()
        {
            train.Throttle = 1f; train.Resources.SetLevels(50f, 40f);
            StepFor(1f); float lightSpeed = train.Speed, lightSteam = train.Resources.Steam;
            train.Braking = true; StepFor(2f); Assert.That(train.Speed, Is.Zero);
            train.Braking = false; train.Weight.ExtraTestWeight = 100f; train.Resources.SetLevels(50f, 40f);
            StepFor(1f);
            Assert.That(train.Speed, Is.LessThan(lightSpeed * 0.4f));
            Assert.That(train.Resources.Steam, Is.EqualTo(lightSteam).Within(0.001f));
        }

        [Test]
        public void ForwardBrakingReverseAndStationStopArePredictable()
        {
            train.Throttle = 0.5f;
            StepFor(2f); Assert.That(train.Speed, Is.GreaterThan(4f));
            float atBrake = train.HeadDistance;
            train.Resources.SetLevels(0f, 40f); train.Braking = true; StepFor(2f);
            Assert.That(train.Speed, Is.Zero);
            Assert.That(train.HeadDistance - atBrake, Is.InRange(1.5f, 4f));
            train.Path.Sample(train.HeadDistance, out Vector3 stopped, out _);
            Assert.That(stopped.z, Is.InRange(-26f, 36f), "Stop must remain alongside station platform.");
            train.Reversing = true; train.Braking = false; train.Throttle = 1f; train.Resources.SetLevels(50f, 40f);
            StepFor(3f); Assert.That(train.Speed, Is.InRange(-4.01f, -3.9f));
            Assert.That(cars[0].Distance - cars[3].Distance, Is.EqualTo(28.5f).Within(0.01f));
            Assert.That(train.Path.Length, Is.GreaterThan(350f));
        }

        [UnityTest]
        public IEnumerator DetachedCarsStayBehindAndCanBeReattachedAfterReturning()
        {
            float fullWeight = train.Weight.Calculate(train);
            var rear = cars[0].GetComponentsInChildren<TrainCoupler>().First(x => !x.IsFront);
            var front = cars[1].GetComponentsInChildren<TrainCoupler>().First(x => x.IsFront);
            Assert.That(rear.Interact(actor), Is.True); Assert.That(train.ConnectedCount, Is.EqualTo(1));
            Assert.That(train.Weight.Calculate(train), Is.LessThan(fullWeight));
            Vector3 detached = cars[1].transform.position;
            train.Throttle = 0.4f; train.enabled = true;
            yield return Wait(1f);
            Assert.That(Vector3.Distance(detached, cars[1].transform.position), Is.LessThan(0.01f));
            Assert.That(train.Couple(rear, front), Is.False, "Moving/distant couplers must not snap together.");
            train.Throttle = 0f; train.Braking = true;
            yield return Wait(1.5f); train.enabled = false;
            Assert.That(train.Speed, Is.Zero);
            // Return the locomotive to the detached consist without restarting the scene.
            train.Configure(train.Path, new[] { cars[0] }, cars[1].Distance + 9.5f);
            yield return new WaitForFixedUpdate();
            Assert.That(rear.Interact(actor), Is.True);
            Assert.That(train.ConnectedCount, Is.EqualTo(4));
            train.Configure(train.Path, cars, train.HeadDistance);
            train.Braking = false; train.Throttle = 0.4f; train.enabled = true;
            yield return Wait(0.6f);
            Assert.That(cars[1].Distance, Is.GreaterThan(70.5f));
        }

        [UnityTest]
        public IEnumerator LooseAndWeldedScrapRemainAboardAndWeldAddsWeight()
        {
            var scrap = scene.GetRootGameObjects().Select(x => x.GetComponent<PickupObject>()).First(x => x != null && x.GetComponent<CoalItem>() == null);
            yield return Wait(0.6f);
            Vector3 local = cars[2].transform.InverseTransformPoint(scrap.transform.position);
            train.Throttle = 0.5f; train.enabled = true;
            yield return Wait(2f);
            Vector3 moved = cars[2].transform.InverseTransformPoint(scrap.transform.position);
            Assert.That(Vector3.Distance(moved, local), Is.LessThan(0.3f), "Loose scrap should ride through contact friction.");
            float weight = train.Weight.Calculate(train);
            Collider floor = cars[2].transform.Find("Walkable Floor").GetComponent<Collider>();
            Assert.That(scrap.BeginHold(actor.GetComponent<CharacterController>()), Is.True);
            Assert.That(scrap.WeldTo(floor, scrap.transform.position - Vector3.up * 0.25f), Is.True);
            Assert.That(train.Weight.Calculate(train), Is.EqualTo(weight + scrap.Body.mass).Within(0.01f));
            local = cars[2].transform.InverseTransformPoint(scrap.transform.position);
            yield return Wait(2f);
            Assert.That(Vector3.Distance(local, cars[2].transform.InverseTransformPoint(scrap.transform.position)), Is.LessThan(0.025f));
            Assert.That(actor.TryPickup(scrap), Is.False);
            Vector3 expected = cars[2].Body.GetPointVelocity(scrap.transform.position);
            Assert.That(scrap.Unweld(), Is.True);
            Assert.That(Vector3.Distance(scrap.Body.linearVelocity, expected), Is.LessThan(0.02f));
            Assert.That(train.Weight.Calculate(train), Is.EqualTo(weight).Within(0.01f));
            yield return Wait(0.5f);
            Assert.That(cars[2].transform.InverseTransformPoint(scrap.transform.position).y, Is.GreaterThan(1.3f));
        }

        [UnityTest]
        public IEnumerator LooseScrapRidesGentleCurveWithoutParenting()
        {
            train.Configure(train.Path, cars, 235f);
            var scrap = scene.GetRootGameObjects().Select(x => x.GetComponent<PickupObject>()).First(x => x != null && x.GetComponent<CoalItem>() == null);
            scrap.Body.interpolation = RigidbodyInterpolation.None;
            scrap.Body.position = cars[2].transform.TransformPoint(new Vector3(0.3f, 1.43f, 0f));
            scrap.Body.rotation = cars[2].transform.rotation;
            scrap.Body.interpolation = RigidbodyInterpolation.Interpolate;
            Physics.SyncTransforms();
            yield return Wait(0.6f);
            Vector3 before = cars[2].transform.InverseTransformPoint(scrap.transform.position);
            train.Throttle = 0.5f; train.enabled = true;
            yield return Wait(3f);
            Vector3 after = cars[2].transform.InverseTransformPoint(scrap.transform.position);
            Assert.That(scrap.transform.parent, Is.Null);
            Assert.That(Vector3.Distance(before, after), Is.LessThan(0.4f));
            Assert.That(after.y, Is.GreaterThan(1.3f));
            Assert.That(train.Speed, Is.GreaterThan(4f));
        }
        [UnityTest]
        public IEnumerator CabSoundsDoorAndDamageDoNotBreakOtherSystems()
        {
            Assert.That(Control(ControlKind.Whistle).Interact(actor), Is.True);
            Assert.That(Control(ControlKind.Bell).Interact(actor), Is.True);
            var door = cars[0].GetComponentInChildren<TrainDoor>();
            Assert.That(door.IsOpen, Is.True); Control(ControlKind.Door).Interact(actor);
            Assert.That(door.IsOpen, Is.False); yield return Wait(0.8f);
            Assert.That(cars[0].transform.Find("Sliding Door").localPosition.y, Is.EqualTo(2.25f).Within(0.01f));
            TrainInstrument speedometer = cars[0].GetComponentsInChildren<TrainInstrument>().First(x => x.Condition != null);
            for (int i = 0; i < 4; i++) Assert.That(Control(ControlKind.DamageSpeedometer).Interact(actor), Is.True);
            speedometer.Refresh(); Assert.That(speedometer.Condition.IsBroken, Is.True);
            StringAssert.Contains("BROKEN", speedometer.DisplayedText);
            Assert.That(train.Resources.Steam, Is.EqualTo(50f)); Assert.That(train.Resources.Pressure, Is.EqualTo(40f));
            train.Throttle = 0.5f; StepFor(1f); Assert.That(train.Speed, Is.GreaterThan(0f));
            Assert.That(Control(ControlKind.Firebox).Interact(actor), Is.False);
            Assert.That(train.Resources.CoalSupply, Is.EqualTo(10));
        }
    }
}
#endif
