#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AllAboard.Train;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace AllAboard.Movement.Tests
{
    public sealed class RouteNetworkTests
    {
        [Test]
        public void SeedsAreReproducibleAndDoNotConsumeGameplayRandomness()
        {
            var state=Random.state;
            try
            {
                Random.InitState(817);float expected=Random.value;Random.InitState(817);
                string first=JsonUtility.ToJson(RouteNetwork.Generate(8008,1f));Assert.That(Random.value,Is.EqualTo(expected));
                Assert.That(JsonUtility.ToJson(RouteNetwork.Generate(8008,1f)),Is.EqualTo(first));
                Assert.That(JsonUtility.ToJson(RouteNetwork.Generate(8009,1f)),Is.Not.EqualTo(first));
            }
            finally{Random.state=state;}
        }
        [TestCase(0)] [TestCase(42)] [TestCase(8008)] [TestCase(-71)]
        public void EverySeedKeepsRequiredPlacesConnectedAndAllowsCrossovers(int seed)
        {
            var graph=RouteNetwork.Generate(seed,1f);
            Assert.That(graph.nodes.Count(x=>x.place==RoutePlace.ForestRepair),Is.EqualTo(1));
            Assert.That(graph.Outgoing("Starter").Select(x=>x.to).Distinct().Single(),Is.EqualTo("Supply"));
            Assert.That(graph.Outgoing("Starter").Count(),Is.EqualTo(2));
            Assert.That(graph.nodes.Any(x=>x.family==RouteFamily.Safe&&x.place==RoutePlace.Town),Is.True);
            Assert.That(graph.nodes.Any(x=>x.family==RouteFamily.Safe&&x.place==RoutePlace.Station),Is.True);
            foreach(var edge in graph.edges){Assert.That(edge.points.First(),Is.EqualTo(graph.Node(edge.from).position));Assert.That(edge.points.Last(),Is.EqualTo(graph.Node(edge.to).position));}
            foreach(var node in graph.nodes.Where(x=>x.place!=RoutePlace.Return))
            {
                var seen=new HashSet<string>();var pending=new Queue<string>();pending.Enqueue(node.id);
                while(pending.Count>0){string id=pending.Dequeue();if(!seen.Add(id))continue;foreach(var edge in graph.Outgoing(id))pending.Enqueue(edge.to);}
                Assert.That(seen.Contains("Terminum"),Is.True,node.id+" must connect to Terminum");
            }
            Assert.That(graph.edges.Any(x=>x.crossover&&x.from.StartsWith("Safe")&&x.to.StartsWith("Mixed")),Is.True);
            Assert.That(graph.edges.Any(x=>x.crossover&&x.from.StartsWith("Mixed")&&x.to.StartsWith("Hard")),Is.True);
        }
    }
    public sealed class ProceduralRailwayTests
    {
        private Scene scene;private ProceduralRailway world;private PlayerInteraction actor;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            scene=SceneManager.GetSceneByPath("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity");
            world=scene.GetRootGameObjects().Select(x=>x.GetComponent<ProceduralRailway>()).First(x=>x!=null);world.Train.enabled=false;
            actor=scene.GetRootGameObjects().Select(x=>x.GetComponent<PlayerInteraction>()).First(x=>x!=null);actor.enabled=false;actor.GetComponent<FirstPersonMotor>().enabled=false;actor.GetComponent<PlayerInputReader>().enabled=false;
        }
        [UnityTearDown]public IEnumerator TearDown(){yield return SceneManager.UnloadSceneAsync(scene);yield return null;}
        [Test]
        public void FifteenMinuteRouteHasBoardableStationsAndClearGround()
        {
            Assert.That(world.NodeDistance("Terminum")-80f,Is.EqualTo(15f*60f*12f).Within(2f));
            Assert.That(world.Train.ConnectedCount,Is.EqualTo(4));Assert.That(world.Geometry.MeshCount,Is.GreaterThan(10));
            foreach(var node in world.Network.nodes.Where(x=>RouteGeometry.IsStop(x.place)))
            {
                var station=world.transform.Find("Generated Route World/Station - "+node.title);
                Assert.That(station,Is.Not.Null);Assert.That(station.Find("Boarding Platform").GetComponent<Collider>().bounds.max.y,Is.EqualTo(1.15f).Within(0.002f));
            }
            for(float d=0f;d<world.Path.Length;d+=80f)
            {
                world.Path.Sample(d,out Vector3 p,out _);
                var hits=Physics.RaycastAll(p+Vector3.up*60f,Vector3.down,65f,~0,QueryTriggerInteraction.Ignore);
                var terrain=hits.Where(x=>x.collider is MeshCollider&&x.collider.GetComponentInParent<RouteGeometry>()==world.Geometry).ToArray();
                Assert.That(terrain.Length,Is.GreaterThan(0),"Missing ground at distance "+d);
                Assert.That(terrain.Min(x=>Mathf.Abs(x.point.y)),Is.LessThan(0.005f),"Terrain obstructs rails at "+d);
            }
            Assert.That(scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<MonoBehaviour>(true)).Any(x=>x==null),Is.False);
        }
        [UnityTest]
        public IEnumerator ChangingAnUpcomingSwitchPreservesTrainAndPastTrack()
        {
            float head=world.Train.HeadDistance;Vector3[] before=new Vector3[9];for(int i=0;i<before.Length;i++)world.Path.Sample(i*10f,out before[i],out _);
            Assert.That(world.Choose("Starter",1),Is.True);Assert.That(world.SelectedTitle("Starter"),Is.EqualTo("RISKY"));
            Assert.That(world.Train.HeadDistance,Is.EqualTo(head));
            for(int i=0;i<before.Length;i++){world.Path.Sample(i*10f,out Vector3 after,out _);Assert.That(Vector3.Distance(before[i],after),Is.LessThan(0.001f));}
            world.Train.Throttle=1f;world.Train.Step(0.1f);Assert.That(world.Choose("Supply",1),Is.False,"Cannot throw points while the train is moving.");
            yield return null;
        }
        [UnityTest]
        public IEnumerator SwitchHighlightsMovesLeverAndCannotBeChangedFromSeat()
        {
            var control=world.GetComponentsInChildren<TrackSwitch>().First(x=>x.NodeId=="Starter");
            var lever=control.transform.Find("Switch Unit Scale/Switch Lever Pivot");Quaternion before=lever.localRotation;
            var highlight=control.GetComponent<ControlHighlight>();control.SetHighlighted(true);
            Assert.That(highlight.Highlighted,Is.True);Assert.That(control.GetComponentsInChildren<Renderer>().Any(x=>x.name=="Control Hover Outline"&&x.enabled),Is.True);
            Assert.That(world.Train.Cars[0].GetComponentInChildren<ConductorSeat>().Interact(actor),Is.True);
            Assert.That(control.Interact(actor),Is.False);actor.GetComponent<FirstPersonMotor>().ExitSeat();
            Assert.That(control.Interact(actor),Is.True);yield return new WaitForSeconds(0.2f);
            Assert.That(Quaternion.Angle(before,lever.localRotation),Is.GreaterThan(45f));
        }
        [Test]
        public void BypassCrossoversAndDayEndCanBeDrivenWithoutResettingTheConsist()
        {
            Assert.That(world.Choose("Supply",1),Is.True);
            Assert.That(world.Choose("Mixed0",2),Is.True);
            Assert.That(world.Choose("Hard1",1),Is.True);
            Assert.That(world.Choose("Gate",1),Is.True);
            Assert.That(world.NodeDistance("Terminum"),Is.LessThan(0f));
            Assert.That(world.NodeDistance("Return"),Is.GreaterThan(80f));
            Assert.That(world.transform.Find("Generated Route World/Marked Repair Site"),Is.Not.Null);
            for(int i=0;i<6000&&!world.DayEnded;i++)
            {
                world.Train.Resources.SetLevels(60f,40f);world.Train.Throttle=1f;
                world.Train.Step(0.25f);world.CheckDayEnd();
            }
            Assert.That(world.DayEnded,Is.True);Assert.That(world.Train.Speed,Is.EqualTo(0f));
            float distance=world.Train.HeadDistance;
            world.Train.Throttle=1f;world.Train.Braking=false;world.Train.Step(1f);
            Assert.That(world.Train.HeadDistance,Is.EqualTo(distance));
            Assert.That(world.Train.ConnectedCount,Is.EqualTo(4));
            for(int i=1;i<4;i++)Assert.That(world.Train.Cars[i-1].Distance-world.Train.Cars[i].Distance,Is.EqualTo(9.5f).Within(0.01f));
        }
        [Test]
        public void DetachedCarStillLocksThePointsItOccupies()
        {
            var car=world.Train.Cars[3];Assert.That(world.Train.Disconnect(car,true),Is.True);
            car.Place(world.Path,world.NodeDistance("Starter")+5f,true);
            Assert.That(world.Choose("Starter",1),Is.False);
        }
        [UnityTest]
        public IEnumerator FreeSupplyCoalIsPhysicalAndCanFuelTheTrainOnce()
        {
            var supply=world.GetComponentsInChildren<StationCoalSupply>().First();
            var cc=actor.GetComponent<CharacterController>();cc.enabled=false;actor.transform.position=supply.transform.position+Vector3.back;cc.enabled=true;Physics.SyncTransforms();
            int stock=world.Train.Resources.CoalSupply;Assert.That(supply.Interact(actor),Is.True);Assert.That(actor.IsHolding,Is.True);
            Assert.That(world.Train.Resources.CoalSupply,Is.EqualTo(stock+1));Assert.That(supply.Interact(actor),Is.False);
            var item=actor.HeldItem.GetComponent<CoalItem>();world.Train.Resources.SetLevels(20f,20f);
            Assert.That(item.TryFuel(world.Train.Resources),Is.True);Assert.That(world.Train.Resources.Steam,Is.EqualTo(60f));Assert.That(item.TryFuel(world.Train.Resources),Is.False);
            actor.ConsumeHeldItem();yield return null;
        }
    }
}
#endif
