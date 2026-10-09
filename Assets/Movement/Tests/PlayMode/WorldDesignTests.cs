using System.Collections;
using System.Linq;
using AllAboard.Train;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;
namespace AllAboard.Movement.Tests
{
    public sealed class RailwayEconomyTests
    {
        [Test] public void DailyBudgetAndSteelPurchasesRespectLimits()
        {
            var go=new GameObject("Inventory Test");try
            {
                var bag=go.AddComponent<RailwayInventory>();bag.BeginDay();Assert.That(bag.Gold,Is.EqualTo(500));Assert.That(bag.SteelMetres,Is.EqualTo(20f));
                Assert.That(bag.UseSteel(6f),Is.False);Assert.That(bag.UseSteel(-1f),Is.False);Assert.That(bag.Spend(-1),Is.False);
                Assert.That(bag.BuySteel(),Is.True);Assert.That(bag.Gold,Is.EqualTo(450));Assert.That(bag.SteelMetres,Is.EqualTo(25f));
                for(int i=0;i<9;i++)Assert.That(bag.BuySteel(),Is.True);
                Assert.That(bag.BuySteel(),Is.False);Assert.That(bag.Gold,Is.Zero);
                bag.BeginDay();Assert.That(bag.SteelMetres,Is.EqualTo(20f));
            }finally{Object.DestroyImmediate(go);}
        }
        [Test] public void SettlementShopCountsAndIsolatedFareAreDeterministic()
        {
            int villages=0,towns=0;bool station=false,mining=false,lumber=false,abandoned=false;
            for(int seed=0;seed<100;seed++)
            {
                var graph=RouteNetwork.Generate(seed,1f);
                Assert.That(graph.nodes.Select(x=>x.title).Distinct().Count(),Is.EqualTo(graph.nodes.Count));
                foreach(var n in graph.nodes)
                {
                    if(n.place==RoutePlace.Village){Assert.That(n.shops,Is.InRange(1,2));villages++;}
                    if(n.place==RoutePlace.Town){Assert.That(n.shops,Is.InRange(2,4));towns++;}
                    if(n.place==RoutePlace.Station){Assert.That(n.shops,Is.Zero);Assert.That(n.fareMultiplier,Is.EqualTo(5));station=true;}
                    mining|=n.settlement==SettlementKind.Mining;lumber|=n.settlement==SettlementKind.Lumber;abandoned|=n.settlement==SettlementKind.Abandoned;
                }
            }
            Assert.That(villages,Is.GreaterThan(10));Assert.That(towns,Is.GreaterThan(10));Assert.That(station&&mining&&lumber&&abandoned,Is.True);
            Assert.That(RailDamagePlan.DerailChance(3f),Is.Zero);Assert.That(RailDamagePlan.DerailChance(12f),Is.EqualTo(1f));
        }
    }
    public sealed class WorldDesignTests
    {
        private Scene scene;private ProceduralRailway world;private PlayerInteraction actor;
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            scene=SceneManager.GetSceneByPath("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity");
            world=scene.GetRootGameObjects().Select(x=>x.GetComponent<ProceduralRailway>()).First(x=>x!=null);world.Train.enabled=false;
            actor=scene.GetRootGameObjects().Select(x=>x.GetComponent<PlayerInteraction>()).First(x=>x!=null);actor.enabled=false;actor.GetComponent<FirstPersonMotor>().enabled=false;actor.GetComponent<PlayerInputReader>().enabled=false;
        }
        [UnityTearDown]public IEnumerator TearDown(){yield return SceneManager.UnloadSceneAsync(scene);yield return null;}
        private void MoveActor(Vector3 at){var cc=actor.GetComponent<CharacterController>();cc.enabled=false;actor.transform.position=at;cc.enabled=true;Physics.SyncTransforms();}
        private RailTool TakeTool(RailToolKind kind)
        {
            var tool=world.Train.Cars[1].GetComponentsInChildren<RailTool>().First(x=>x.Kind==kind);
            MoveActor(tool.transform.position+Vector3.back*.9f);Assert.That(actor.TryPickup(tool.GetComponent<PickupObject>()),Is.True);return tool;
        }
        [UnityTest] public IEnumerator PassengerDoorSwingsAroundAFixedHinge()
        {
            var door=world.Train.Cars[2].GetComponentInChildren<TrainDoor>();var hinge=door.HingePosition;
            Assert.That(door.IsOpen,Is.True);door.Toggle();yield return new WaitForSeconds(.9f);
            var closed=door.Panel.position;Assert.That(Quaternion.Angle(door.Panel.localRotation,Quaternion.identity),Is.LessThan(.1f));
            door.Toggle();yield return new WaitForSeconds(.9f);
            Assert.That(Vector3.Distance(door.HingePosition,hinge),Is.LessThan(.001f));
            Assert.That(Vector3.Distance(door.Panel.TransformPoint(new Vector3(0f,0f,-.5f)),hinge),Is.LessThan(.002f));
            Assert.That(Mathf.Abs(door.Panel.position.y-closed.y),Is.LessThan(.002f));Assert.That(Vector3.Distance(door.Panel.position,closed),Is.GreaterThan(1f));
        }
        [Test] public void EveryOpenVendorHasFourPhysicalPurchaseButtons()
        {
            var shops=world.GetComponentsInChildren<Transform>().Where(x=>x.name=="Vendor Shop").ToArray();int open=0;
            foreach(var shop in shops)
            {
                var vendors=shop.GetComponentsInChildren<RailwayVendor>();if(vendors.Length==0)continue;open++;
                Assert.That(vendors.Select(x=>x.Product).Distinct().Count(),Is.GreaterThanOrEqualTo(4));
                if(vendors.Length>4)foreach(var needed in new[]{RailwayProduct.Scrap,RailwayProduct.Screws,RailwayProduct.BoilerPart,RailwayProduct.BrakePart,RailwayProduct.CoachPart})Assert.That(vendors.Any(v=>v.Product==needed),Is.True);
                Assert.That(vendors.All(x=>x.GetComponent<TrainButton>()!=null&&x.GetComponent<Collider>()!=null),Is.True);
            }
            Assert.That(open,Is.GreaterThan(0));
            var buy=world.GetComponentsInChildren<RailwayVendor>().First(x=>x.Product==RailwayProduct.Steel);
            MoveActor(buy.transform.position+Vector3.back);Assert.That(buy.Interact(actor),Is.True);
            Assert.That(world.Train.GetComponent<RailwayInventory>().Gold,Is.EqualTo(450));Assert.That(buy.Interact(actor),Is.False,"A depressed button must not double-buy.");
        }
        [Test] public void CorrectToolsAreRequiredAndSteelBridgesLongGapsInPieces()
        {
            var section=world.GetComponentsInChildren<RailDamageSection>().First(x=>x.Kind==RailDamageKind.Missing);
            var ends=section.GetComponentsInChildren<RailRepairEnd>();var saw=TakeTool(RailToolKind.Saw);
            Assert.That(ends[0].Interact(actor),Is.False,"Saw cannot install missing rails.");actor.ReleaseCharged(0f);
            var tool=TakeTool(RailToolKind.Connector);var inventory=world.Train.GetComponent<RailwayInventory>();
            for(int side=0;side<2;side++)for(int piece=0;piece<2;piece++)
            {
                ends=section.GetComponentsInChildren<RailRepairEnd>();var first=ends.First(x=>x.Side==side&&!x.IsFarEnd);var last=ends.First(x=>x.Side==side&&x.IsFarEnd);
                Assert.That(last.Interact(actor),Is.False,"Must align the starting endpoint first.");
                Assert.That(first.Interact(actor),Is.True);
                var preview=Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Exclude).Single(x=>x.name=="Steel Placement Preview");
                Assert.That(Vector3.Distance(preview.GetPosition(0),preview.GetPosition(10)),Is.LessThanOrEqualTo(5.01f));
                Assert.That(last.Interact(actor),Is.True);
            }
            Assert.That(section.IsRepaired,Is.True);Assert.That(inventory.SteelMetres,Is.EqualTo(4f).Within(.001f));
        }
        [Test] public void SawNeedsAlternatingStrokesAndTwoRailCuts()
        {
            var section=world.GetComponentsInChildren<RailDamageSection>().First(x=>x.Kind==RailDamageKind.Strained);
            var tool=TakeTool(RailToolKind.Connector);Assert.That(section.GetComponentsInChildren<RailRepairEnd>()[0].Interact(actor),Is.False);actor.ReleaseCharged(0f);
            var saw=TakeTool(RailToolKind.Saw);var control=saw.GetComponent<RailSawControl>();
            for(int side=0;side<2;side++)
            {
                var end=section.GetComponentsInChildren<RailRepairEnd>().First(x=>x.name=="Saw Mount");
                Assert.That(end.Interact(actor),Is.True);control.Drag(100f);for(int i=0;i<20;i++)control.Drag(100f);
                Assert.That(control.Progress,Is.EqualTo(.125f));
                for(int i=0;i<7;i++)control.Drag(i%2==0?-100f:100f);
                Assert.That(control.Finished,Is.True);Assert.That(section.CanSaw(side),Is.False);Assert.That(section.IsMissing,Is.True);
                MoveActor(saw.transform.position+Vector3.back*.8f);Assert.That(control.Interact(actor),Is.True);
            }
            Assert.That(section.IsRepaired,Is.False);
        }
        [Test] public void MissingRailsDerailAndStopTheJourney()
        {
            Assert.That(world.Choose("Supply",2),Is.True);
            var section=world.GetComponentsInChildren<RailDamageSection>().First(x=>x.EdgeId=="AlongHard2");
            float at=world.NodeDistance("Hard1")+section.Start;
            world.Train.Configure(world.Path,world.Train.Cars.ToArray(),at-7f);
            world.Train.Throttle=1f;world.Train.Step(.1f);
            Assert.That(world.DayEnded,Is.True);Assert.That(world.Train.IsDerailed,Is.True);Assert.That(world.Train.Speed,Is.Zero);
            float stopped=world.Train.HeadDistance;world.Train.Step(1f);Assert.That(world.Train.HeadDistance,Is.EqualTo(stopped));
        }
        [Test] public void LandmarkSetAndCapitalSkylineExistWithoutBlockingTheTrack()
        {
            foreach(LandmarkKind kind in System.Enum.GetValues(typeof(LandmarkKind)))if(kind!=LandmarkKind.Tunnel)Assert.That(world.Geometry.Landmarks.Any(x=>x.kind==kind),Is.True,kind.ToString());
            foreach(var tunnel in world.Geometry.Landmarks.Where(x=>x.kind==LandmarkKind.Tunnel))
            {Assert.That(WorldRegions.At(world.Network,tunnel.position),Is.EqualTo(LandscapeRegion.Mountains));Assert.That(WorldRegions.Height(world.Network,tunnel.position),Is.GreaterThanOrEqualTo(120f));}
            var city=world.transform.Find("Generated Route World/Station - TERMINUM/Victorian Capital Skyline");Assert.That(city,Is.Not.Null);
            Assert.That(city.GetComponentsInChildren<Transform>().Count(x=>x.name=="Capital Terrace"),Is.EqualTo(16));
            Assert.That(city.GetComponentsInChildren<Collider>().All(x=>Mathf.Abs(x.bounds.center.x)>20f),Is.True);
        }
        [Test] public void RedfortHasSixHouseFormsAndTunnelsHaveClearEntrances()
        {
            var station=world.transform.Find("Generated Route World/Station - REDFORT");
            foreach(string name in new[]{"False Front House","Gabled Cottage","Balcony Shopfront","Log Cabin","Frontier Workshop","Stone Bay Cottage"})Assert.That(station.Find(name),Is.Not.Null);
            Assert.That(WorldRegions.Height(world.Network,world.Network.Node("Bluefort").position+Vector3.right*300f),Is.GreaterThan(35f));
            foreach(var plan in world.Geometry.Landmarks.Where(x=>x.kind==LandmarkKind.Tunnel))
            {
                var from=plan.position+plan.rotation*Vector3.back*70f+Vector3.up*2.5f;
                Assert.That(Physics.BoxCast(from,new Vector3(1.99f,1.7f,1f),plan.rotation*Vector3.forward,out var hit,plan.rotation,140f,~0,QueryTriggerInteraction.Ignore),Is.False,"Tunnel blocks the train: "+(hit.collider!=null?hit.collider.name:""));
            }
        }
    }
}
