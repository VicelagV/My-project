using System.Collections;
using System.Linq;
using AllAboard.Train;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace AllAboard.Movement.Tests
{
    public sealed class DayAccountingTests
    {
        private GameObject root;private RailwayInventory wallet;private Scene journeyScene;
        [SetUp] public void SetUp(){root=new GameObject("Day Ledger Test");wallet=root.AddComponent<RailwayInventory>();wallet.BeginDay();}
        [TearDown] public void TearDown()=>Object.DestroyImmediate(root);
        [UnityTearDown] public IEnumerator UnloadJourney(){if(journeyScene.IsValid()&&journeyScene.isLoaded)yield return SceneManager.UnloadSceneAsync(journeyScene);}
        [Test] public void CreditIsCappedAcrossMultiplePurchasesAndGoldNeverGoesNegative()
        {Assert.That(wallet.Purchase(550),Is.True);Assert.That(wallet.Gold,Is.Zero);Assert.That(wallet.CreditDebt,Is.EqualTo(50));Assert.That(wallet.Purchase(50),Is.True);Assert.That(wallet.Purchase(1),Is.False);wallet.Earn(20);Assert.That(wallet.Purchase(21),Is.False);Assert.That(wallet.Purchase(-1),Is.False);Assert.That(wallet.CreditDebt,Is.EqualTo(100));}
        [TestCase(RoutePlace.Terminum,100)][TestCase(RoutePlace.Town,200)][TestCase(RoutePlace.Village,300)][TestCase(RoutePlace.Station,500)]
        public void FareCombinesJobDestinationAndClampedTip(RoutePlace place,int reward)
        {Assert.That(wallet.PayPassenger(65,place,50,PassengerOutcome.Delivered),Is.EqualTo(65+reward+50));}
        [TestCase(PassengerOutcome.Dead)][TestCase(PassengerOutcome.Kidnapped)][TestCase(PassengerOutcome.Abandoned)][TestCase(PassengerOutcome.HappinessLost)]
        public void FailedJourneysPayNothing(PassengerOutcome outcome)
        {Assert.That(wallet.PayPassenger(100,RoutePlace.Station,100,outcome),Is.Zero);Assert.That(wallet.Gold,Is.EqualTo(500));}
        [Test] public void ForcedExitPaysOnlyFiftyAndNegativeHappinessPaysNothing()
        {Assert.That(wallet.PayPassenger(100,RoutePlace.Station,100,PassengerOutcome.ForcedExit),Is.EqualTo(50));Assert.That(wallet.PayPassenger(100,RoutePlace.Station,-1,PassengerOutcome.Delivered),Is.Zero);}
        [Test] public void CargoPaysLessWhenDamagedAndFailedCargoCreatesObligations()
        {wallet.ResolveCargo(400,CargoOutcome.Intact);wallet.ResolveCargo(400,CargoOutcome.Damaged);wallet.ResolveCargo(400,CargoOutcome.Failed);Assert.That(wallet.Gold,Is.EqualTo(1200));Assert.That(wallet.Obligations,Is.EqualTo(140));}
        [Test] public void EntryFeeIsPaidOnceAndCannotUseCredit()
        {Assert.That(wallet.PayEntry(),Is.False);wallet.Earn(500);Assert.That(wallet.PayEntry(),Is.True);Assert.That(wallet.PayEntry(),Is.True);Assert.That(wallet.Gold,Is.Zero);}
        [Test] public void AccountingPaysObligationsThenQuotaRepairsAndBlumtOnce()
        {wallet.Purchase(550);wallet.Earn(2500);wallet.Charge(100);Assert.That(wallet.FinishDay(200,EconomyDifficulty.Hard,false),Is.True);Assert.That(wallet.Blumt,Is.EqualTo(975));Assert.That(wallet.FinishDay(200,EconomyDifficulty.Hard,false),Is.True);Assert.That(wallet.Blumt,Is.EqualTo(975));Assert.That(wallet.Purchase(0),Is.False);}
        [Test] public void RepairDebtCarriesIntoNextQuotaOnlyAfterQuotaIsPaid()
        {wallet.Earn(1100);Assert.That(wallet.FinishDay(300,EconomyDifficulty.Normal,false),Is.True);Assert.That(wallet.RepairDebt,Is.EqualTo(200));wallet.BeginDay(2);Assert.That(wallet.Quota,Is.EqualTo(1950));Assert.That(wallet.Gold,Is.EqualTo(500));}
        [Test] public void UnpaidCapitalEntryOrQuotaCannotBecomeSuccess()
        {wallet.Earn(2000);Assert.That(wallet.FinishDay(0,EconomyDifficulty.Normal,true),Is.False);Assert.That(wallet.Blumt,Is.Zero);}
        [Test] public void VendorMarkupAndSpecializedSalvageUseConfiguredPrices()
        {
            var train=root.AddComponent<TrainController>();var vendor=root.AddComponent<RailwayVendor>();vendor.Configure(train,RailwayProduct.Steel,null,null);
            vendor.SetPriceMultiplier(wallet.Config.villageMultiplier);Assert.That(vendor.Price,Is.EqualTo(60));
            var cargo=root.AddComponent<LootCargo>();cargo.Value=100;cargo.Category=SalvageCategory.RailParts;
            vendor.SetSpecialization(SalvageCategory.RailParts);Assert.That(vendor.SalePrice(cargo),Is.EqualTo(100));
            vendor.SetSpecialization(SalvageCategory.Fuel);Assert.That(vendor.SalePrice(cargo),Is.EqualTo(30));
            cargo.Category=SalvageCategory.General;Assert.That(vendor.SalePrice(cargo),Is.EqualTo(60));
            vendor.SetSpecialization(SalvageCategory.Fuel,true);Assert.That(vendor.SalePrice(cargo),Is.EqualTo(100));
        }
        [Test] public void CharacterVariantsHaveNoEyebrowsAndNoExtraPhysicsColliders()
        {
            foreach(string name in new[]{"BrightCharacter","BrightCharacterYellow","BrightCharacterGreen"})
            {var prefab=Resources.Load<GameObject>(name);Assert.That(prefab,Is.Not.Null);Assert.That(prefab.GetComponentsInChildren<Transform>().Any(t=>t.name.ToLowerInvariant().Contains("eyebrow")),Is.False);Assert.That(prefab.GetComponentsInChildren<Collider>().Length,Is.Zero);Assert.That(prefab.GetComponentsInChildren<Renderer>().Length,Is.EqualTo(17));}
        }
        [Test] public void OwnBodyIsHiddenFromCameraWhileKeepingShadows()
        {
            var player=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Movement/Prefabs/PrototypePlayer.prefab"));
            try{var avatar=player.GetComponent<PlayerAvatar>();Assert.That(avatar.Model,Is.Not.Null);Assert.That(avatar.Model.GetComponentsInChildren<Renderer>().All(r=>r.shadowCastingMode==ShadowCastingMode.ShadowsOnly),Is.True);avatar.SetLocalVisibility(false);Assert.That(avatar.Model.GetComponentsInChildren<Renderer>().All(r=>r.shadowCastingMode==ShadowCastingMode.On),Is.True);}
            finally{Object.DestroyImmediate(player);}
        }
        [UnityTest] public IEnumerator SafeRoutePassengersFundEntryAndQuotaThroughActualStops()
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity",new LoadSceneParameters(LoadSceneMode.Additive));yield return null;
            journeyScene=SceneManager.GetSceneByPath("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity");
            var world=journeyScene.GetRootGameObjects().Select(g=>g.GetComponent<ProceduralRailway>()).First(w=>w!=null);var train=world.Train;var ledger=train.GetComponent<RailwayInventory>();var economy=train.GetComponent<TrainDayEconomy>();
            Assert.That(economy.Passengers.Count,Is.EqualTo(14));
            train.Configure(world.Path,train.Cars.ToArray(),world.NodeDistance("Gate"));train.Braking=true;
            yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            Assert.That(economy.GateBlocked,Is.True);Assert.That(train.Speed,Is.Zero);Assert.That(ledger.EntryPaid,Is.False);Assert.That(world.DayEnded,Is.False);
            foreach(var node in world.Network.nodes.Where(n=>n.id!="Redfort"&&RouteGeometry.IsStop(n.place)&&world.NodeDistance(n.id)>=0f).OrderBy(n=>world.NodeDistance(n.id)))
            {
                train.Configure(world.Path,train.Cars.ToArray(),world.NodeDistance(node.id));train.Braking=true;
                yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            }
            Assert.That(economy.Passengers.All(p=>p.Resolved),Is.True);
            Assert.That(ledger.EntryPaid,Is.True);Assert.That(world.DayEnded,Is.True);Assert.That(ledger.DayReport,Does.StartWith("Day complete"));Assert.That(ledger.Blumt,Is.GreaterThan(0));
        }
    }
}
