using System.Collections;
using System.Linq;
using AllAboard.Train;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace AllAboard.Movement.Tests
{
    public sealed class JourneyUITests
    {
        private Scene scene;private ProceduralRailway world;private JourneyUI ui;private PlayerInteraction actor;private RailwayInventory bag;
        [UnitySetUp] public IEnumerator Setup()
        {yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity",new LoadSceneParameters(LoadSceneMode.Additive));yield return null;scene=SceneManager.GetSceneByPath("Assets/Movement/Scenes/ProceduralRailwayPlayground.unity");world=scene.GetRootGameObjects().Select(g=>g.GetComponent<ProceduralRailway>()).First(w=>w!=null);ui=world.GetComponent<JourneyUI>();actor=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PlayerInteraction>()).First();bag=world.Train.GetComponent<RailwayInventory>();}
        [UnityTearDown] public IEnumerator Cleanup(){yield return SceneManager.UnloadSceneAsync(scene);}
        [Test] public void VendorOpensMenuWithoutBuyingAndCloseRestoresInputState()
        {var vendor=world.GetComponentInChildren<ShopVendor>();int gold=bag.Gold;Assert.That(vendor.Interact(actor),Is.True);Assert.That(ui.Visible,Is.True);Assert.That(actor.GetComponent<PlayerInputReader>().MenuOpen,Is.True);Assert.That(bag.Gold,Is.EqualTo(gold));Assert.That(ui.Document.rootVisualElement.Query<Button>().ToList().Count,Is.GreaterThan(vendor.Offers.Count));ui.Close();Assert.That(actor.GetComponent<PlayerInputReader>().MenuOpen,Is.False);}
        [Test] public void StocksCanBePurchasedWithOccupiedHandsAndPhysicalItemsWaitOnTray()
        {var vendor=world.GetComponentsInChildren<ShopVendor>().First(v=>v.Offers.Any(o=>o.Rail!=null&&o.Rail.Product==RailwayProduct.Steel));var obj=WeaponArt.Create(WeaponKind.Club,bag,world.Train.GetComponent<TrainDayEconomy>().Palette);SceneManager.MoveGameObjectToScene(obj,scene);obj.transform.position=actor.transform.position+Vector3.up+actor.transform.forward;Assert.That(actor.TryPickup(obj.GetComponent<PickupObject>()),Is.True);int steel=vendor.Offers.FindIndex(o=>o.Rail!=null&&o.Rail.Product==RailwayProduct.Steel),coal=vendor.Offers.FindIndex(o=>o.Rail!=null&&o.Rail.Product==RailwayProduct.Coal);Assert.That(vendor.Buy(steel,actor,out _),Is.True);Assert.That(bag.SteelMetres,Is.EqualTo(25));Assert.That(vendor.Buy(coal,actor,out _),Is.True);Assert.That(actor.HeldItem.gameObject,Is.SameAs(obj));Assert.That(vendor.Pending,Is.EqualTo(1));int gold=bag.Gold;Assert.That(vendor.Buy(coal,actor,out _),Is.False);Assert.That(bag.Gold,Is.EqualTo(gold));}
        [Test] public void FailedPurchaseDoesNotSpawnOrChargeAndCannotSpendAfterSettlement()
        {var vendor=world.GetComponentsInChildren<ShopVendor>().First(v=>v.Offers.Any(o=>o.Combat!=null&&o.Combat.Product==CombatPurchase.Rifle));int rifle=vendor.Offers.FindIndex(o=>o.Combat!=null&&o.Combat.Product==CombatPurchase.Rifle);bag.Spend(bag.Gold);bag.Purchase(bag.Config.creditLimit);int before=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PickupObject>()).Count();Assert.That(vendor.Buy(rifle,actor,out _),Is.False);Assert.That(vendor.Pending,Is.Zero);Assert.That(scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PickupObject>()).Count(),Is.EqualTo(before));bag.FailDay("Test failure");Assert.That(vendor.Buy(rifle,actor,out _),Is.False);}
        [UnityTest] public IEnumerator FinishedJourneyDisplaysReportAndLocksGameplayInput()
        {bag.Earn(3000);bag.PayEntry();world.Train.GetComponent<TrainDayEconomy>().Finish(true);yield return null;Assert.That(world.DayEnded,Is.True);Assert.That(ui.Visible,Is.True);Assert.That(actor.GetComponent<PlayerInputReader>().MenuOpen,Is.True);var labels=ui.Document.rootVisualElement.Query<Label>().ToList();Assert.That(labels.Any(l=>l.text.Contains("Income earned")),Is.True);Assert.That(bag.TotalEarned,Is.GreaterThanOrEqualTo(3000));}
        [Test] public void AllLiveShopsHaveFriendlyVendorsAndNoClickableOldPurchaseCaps()
        {foreach(var vendor in world.GetComponentsInChildren<ShopVendor>()){Assert.That(vendor.Offers.Count,Is.GreaterThan(1));Assert.That(vendor.GetComponent<Collider>(),Is.Not.Null);foreach(var old in vendor.transform.parent.GetComponentsInChildren<CombatMerchant>())Assert.That(old.GetComponent<Collider>().enabled,Is.False);foreach(var old in vendor.transform.parent.GetComponentsInChildren<RailwayVendor>())Assert.That(old.GetComponent<Collider>().enabled,Is.False);}}
        [UnityTest] public IEnumerator HudCannotBypassBrokenSpeedometer()
        {world.Train.Cars[0].GetComponentsInChildren<TrainInstrument>().First(i=>i.Kind==InstrumentKind.Speed).Condition.Damage(1000);yield return new WaitForSeconds(.2f);Assert.That(ui.Document.rootVisualElement.Q<Label>("journey-status").text,Does.Contain("Speed BROKEN"));}
    }
}
