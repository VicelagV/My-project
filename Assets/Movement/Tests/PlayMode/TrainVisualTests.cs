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
    public sealed class TrainVisualTests
    {
        private Scene scene;
        private TrainController train;
        private TrainCar[] cars;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Movement/Scenes/TrainPlayground.unity", new LoadSceneParameters(LoadSceneMode.Additive));
            scene=SceneManager.GetSceneByPath("Assets/Movement/Scenes/TrainPlayground.unity");
            train=scene.GetRootGameObjects().Select(x=>x.GetComponent<TrainController>()).First(x=>x!=null);
            train.enabled=false;cars=train.Consist.ToArray();
            foreach(var player in scene.GetRootGameObjects().Where(x=>x.GetComponent<FirstPersonMotor>()!=null)) {player.GetComponent<FirstPersonMotor>().enabled=false;player.GetComponent<PlayerInputReader>().enabled=false;}
        }
        [UnityTearDown] public IEnumerator TearDown(){yield return SceneManager.UnloadSceneAsync(scene);}
        [UnityTest]
        public IEnumerator WheelTravelAndConnectingRodsStayAlignedWhenReversing()
        {
            TrainWheel wheel=cars[0].GetComponentsInChildren<TrainWheel>().First(x=>x.Radius>0.7f);
            float angle=wheel.Angle, before=cars[0].Distance;
            train.Throttle=0.5f;train.Step(0.1f);
            Assert.That(Mathf.DeltaAngle(angle,wheel.Angle),Is.EqualTo((cars[0].Distance-before)/wheel.Radius*Mathf.Rad2Deg).Within(0.002f));
            yield return null;
            var rod=cars[0].GetComponentsInChildren<TrainSideRod>()[0];
            Vector3 rodStart=rod.transform.localPosition;
            var group=cars[0].GetComponentsInChildren<TrainWheel>().Where(x=>x.Radius>0.7f&&x.transform.localPosition.x<0f).OrderBy(x=>x.transform.localPosition.z).ToArray();
            Vector3 offset=Quaternion.AngleAxis(group[0].Angle,Vector3.right)*Vector3.up*0.25f+Vector3.left*0.18f;
            Assert.That(Vector3.Distance(rod.transform.localPosition,(group[0].transform.localPosition+group[2].transform.localPosition)*0.5f+offset),Is.LessThan(0.002f));
            Assert.That(rod.transform.localScale.z,Is.EqualTo(3f).Within(0.002f));
            train.Reversing=true;
            for(int i=0;i<40;i++)train.Step(0.1f);
            yield return null;
            Assert.That(train.Speed,Is.LessThan(0f));Assert.That(Vector3.Distance(rodStart,rod.transform.localPosition),Is.GreaterThan(0.02f));
        }
        [UnityTest]
        public IEnumerator SteamEffectsStopWhenEmptyAndCouplerFeedbackFollowsConnection()
        {
            var effect=cars[0].GetComponentInChildren<TrainSteamVisual>();
            var smoke=cars[0].GetComponentsInChildren<ParticleSystem>().First(x=>x.name=="Chimney Smoke");
            train.Throttle=1f;effect.Refresh();Assert.That(smoke.emission.rateOverTime.constant,Is.GreaterThan(10f));
            train.Resources.SetLevels(0f,40f);effect.Refresh();Assert.That(smoke.emission.rateOverTime.constant,Is.Zero);
            var rear=cars[0].GetComponentsInChildren<TrainCoupler>().First(x=>!x.IsFront);
            var heads=cars[0].GetComponentsInChildren<Transform>().Where(x=>x.name=="Janney Coupler").ToArray();
            Assert.That(heads.Length,Is.EqualTo(2));
            var link=heads.First(x=>x.localPosition.z<0f).Find("Coupler Knuckle Pivot");
            Assert.That(link.Find("Knuckle Hook"),Is.Not.Null);
            yield return null;Quaternion connected=link.localRotation;
            Assert.That(rear.Interact(null),Is.True);yield return null;
            Assert.That(Quaternion.Angle(connected,link.localRotation),Is.GreaterThan(40f));
        }
        [Test]
        public void CoachHasSixteenSeatsAndAnUnobstructedAisleAndEntrance()
        {
            var coach=cars[2];var rows=coach.GetComponentsInChildren<Transform>().Where(x=>x.name=="Passenger Seat Pair").ToArray();
            Assert.That(rows.Length,Is.EqualTo(8));
            Assert.That(rows.Count(x=>x.localPosition.x<0f),Is.EqualTo(4));
            Physics.SyncTransforms();
            Assert.That(Physics.Raycast(coach.transform.TransformPoint(new Vector3(0f,1.9f,-4f)),coach.transform.forward,8f,~0,QueryTriggerInteraction.Ignore),Is.False);
            Assert.That(Physics.Raycast(coach.transform.TransformPoint(new Vector3(0f,1.8f,1.5f)),coach.transform.right,3f,~0,QueryTriggerInteraction.Ignore),Is.False);
        }
        [UnityTest]
        public IEnumerator TrailingCarBodiesSwayGentlyAndSettleWhenStopped()
        {
            train.Throttle=1f;
            for(int i=0;i<60;i++){train.Step(0.02f);yield return new WaitForFixedUpdate();}
            var coach=cars[2];train.Path.Sample(coach.Distance,out Vector3 p,out Quaternion q);
            Assert.That(Vector3.Distance(coach.Body.rotation*Vector3.up,q*Vector3.up),Is.InRange(0.00005f,0.0062f));
            Assert.That(Vector3.Distance(coach.Body.position,p),Is.InRange(0.0001f,0.011f));
            train.Path.Sample(cars[0].Distance,out _,out Quaternion loco);
            Assert.That(Quaternion.Angle(cars[0].Body.rotation,loco),Is.LessThan(0.001f));
            train.Throttle=0f;train.Braking=true;
            for(int i=0;i<150;i++){train.Step(0.02f);yield return new WaitForFixedUpdate();}
            train.Path.Sample(coach.Distance,out p,out q);
            Assert.That(Vector3.Distance(coach.Body.position,p),Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(coach.Body.rotation,q),Is.LessThan(0.001f));
        }
        [UnityTest]
        public IEnumerator SideWallPanelsAndWindowGlassCloseUnintendedGaps()
        {
            foreach(var car in cars.Where(x=>x.Kind!=CarKind.Coal))
            {
                var details=car.transform.Find("Train Visual Details");
                foreach(var r in details.GetComponentsInChildren<MeshRenderer>().Where(x=>new[]{"Solid Window Infill","Clear Coach Window","Coach Lower Body","Coach Upper Body","Roof Wall Seam"}.Contains(x.name)))
                {r.gameObject.layer=30;r.gameObject.AddComponent<MeshCollider>().sharedMesh=r.GetComponent<MeshFilter>().sharedMesh;}
                Physics.SyncTransforms();
                foreach(float side in new[]{-1f,1f})foreach(float y in new[]{2.8f,3.85f})
                    for(float z=-4.3f;z<4.35f;z+=0.08f)
                    {
                        if(side>0f&&z>0.48f&&z<2.52f)continue; // the intended open sliding doorway
                        Vector3 origin=car.transform.TransformPoint(new Vector3(side*3f,y,z));
                        Assert.That(Physics.Raycast(origin,-car.transform.right*side,1.3f,1<<30),Is.True,car.name+" wall gap at "+side+", "+y+", "+z);
                    }
            }
            yield return null;
        }
        [Test]
        public void DriverCanSeeAheadOverTheBoilerThroughFrontWindows()
        {
            var cab=cars[0].transform.Find("Cab Visual Details");
            Assert.That(cab.GetComponentsInChildren<Renderer>().Count(x=>x.name=="Front Cab Window"),Is.EqualTo(2));
            Physics.SyncTransforms();
            Vector3 eye=cars[0].transform.TransformPoint(new Vector3(-0.85f,3.25f,4.4f));
            Assert.That(Physics.Raycast(eye,cars[0].transform.forward,25f,~0,QueryTriggerInteraction.Ignore),Is.False,"The engine must not block the driver's forward sightline.");
            Assert.That(cab.Find("Front Window Safety Glass").GetComponent<Collider>(),Is.Not.Null);
        }
        [Test]
        public void NoseDeckHasCarrierCollisionsAndDecorativeGeometryDoesNotObstructCab()
        {
            Assert.That(scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<MonoBehaviour>(true)).Any(x=>x==null),Is.False);
            var floor=cars[0].transform.Find("Train Visual Details/Locomotive Front Running Deck");
            Assert.That(floor.GetComponent<Collider>().attachedRigidbody,Is.EqualTo(cars[0].Body));
            Assert.That(floor.GetComponent<Collider>().sharedMaterial,Is.EqualTo(cars[0].transform.Find("Walkable Floor").GetComponent<Collider>().sharedMaterial));
            Assert.That(cars[0].transform.Find("Cab Visual Details").GetComponentsInChildren<Collider>().Length,Is.EqualTo(1));
            Assert.That(cars[1].GetComponentsInChildren<TrainWheel>().Length,Is.EqualTo(4));
            Assert.That(cars[2].transform.Find("Train Visual Details").GetComponentsInChildren<Renderer>().Any(x=>x.name=="Clear Coach Window"),Is.True);
        }
    }
}
#endif
