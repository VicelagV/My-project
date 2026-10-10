using System.Collections.Generic;
using System.Linq;
using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class ShopVendor:PrototypeInteractable
    {
        public sealed class Offer
        {
            public RailwayVendor Rail;public CombatMerchant Combat;
            public string Title=>Rail!=null?(Rail.Product==RailwayProduct.Steel?"Steel · 5 metres":Rail.Product.ToString()):Combat.Product.ToString();
            public int Price=>Rail!=null?Rail.Price:Combat.Price;
            public bool Physical=>Rail!=null?(Rail.Product==RailwayProduct.Coal||Rail.Product==RailwayProduct.Saw||Rail.Product==RailwayProduct.Connector||Rail.Product==RailwayProduct.Wrench):Combat.Product!=CombatPurchase.Ammunition;
            public string Description=>Rail!=null?Rail.Product==RailwayProduct.Steel?"Repair missing rails. Added to your supply ledger.":Rail.Product==RailwayProduct.Coal?"40 steam per lump. Collect from the tray and fuel the furnace.":Physical?"Physical tool. Collect from the tray; use at the repair site.":"Repair supplies. Added to your supply ledger.":Combat.Product==CombatPurchase.Ammunition?"10 rounds. Added to your shared ammunition reserve.":Combat.Product==CombatPurchase.Medical?"Heal 35 health. Collect the bottle and use M1.":Combat.Product==CombatPurchase.Recovery?"Full recovery tonic. Collect and use M1.":"Physical weapon. Collect from the tray. No friendly fire.";
        }
        private JourneyUI ui;private RailwayInventory wallet;private readonly List<PickupObject> deliveries=new List<PickupObject>();
        public readonly List<Offer> Offers=new List<Offer>();
        public string ShopTitle {get;private set;}
        public Vector3 CollectionPoint=>transform.parent.TransformPoint(new Vector3(-.95f,1.8f,-.1f));
        public int Pending=>deliveries.Count(p=>p!=null&&!p.IsHeld&&Vector3.Distance(p.transform.position,CollectionPoint)<2.4f);
        public override string Prompt=>"E: trade with "+ShopTitle;
        public override bool Interact(PlayerInteraction actor)=>ui!=null&&ui.OpenShop(this,actor);
        public bool Buy(int index,PlayerInteraction actor,out string message)
        {
            message="Purchase unavailable.";if(actor==null||index<0||index>=Offers.Count||wallet.Settled)return false;
            var offer=Offers[index];if(!wallet.CanPurchase(offer.Price)){message="Not enough Gold or available credit.";return false;}
            if(offer.Physical&&Pending>=1){message="Collect your items from the tray first.";return false;}
            Vector3 point=CollectionPoint+transform.parent.right*(Pending%3)*.34f+Vector3.up*.25f;
            bool success=offer.Rail!=null?offer.Rail.TryBuy(actor,point):offer.Combat.TryBuy(actor,point);
            if(!success)return false;
            if(offer.Physical)
            {
                var item=gameObject.scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PickupObject>()).Where(p=>!p.IsHeld&&!deliveries.Contains(p)).OrderBy(p=>Vector3.Distance(p.transform.position,point)).FirstOrDefault();
                if(item!=null&&Vector3.Distance(item.transform.position,point)<.2f)deliveries.Add(item);
            }
            message=offer.Title+" purchased. "+(offer.Physical?"Collect it from the tray with E.":"Added to supplies.");return true;
        }
        public int SalePrice(PlayerInteraction actor){var cargo=actor!=null&&actor.HeldItem!=null?actor.HeldItem.GetComponent<LootCargo>():null;var merchant=Offers.FirstOrDefault(o=>o.Rail!=null)?.Rail;return cargo!=null&&merchant!=null?merchant.SalePrice(cargo):0;}
        public bool Sell(PlayerInteraction actor)
        {int price=SalePrice(actor);if(price<=0||wallet.Settled||!actor.ConsumeHeldItem())return false;wallet.Earn(price);return true;}
        public static void Install(JourneyUI ui,ProceduralRailway world)
        {
            var rail=world.GetComponentsInChildren<RailwayVendor>();var combat=world.GetComponentsInChildren<CombatMerchant>();
            var shops=rail.Select(v=>v.transform.parent.parent).Concat(combat.Select(v=>v.transform.parent.parent)).Distinct().ToArray();
            foreach(var shop in shops)
            {
                if(shop.GetComponentInChildren<ShopVendor>()!=null)continue;
                foreach(var source in shop.GetComponentsInChildren<PrototypeInteractable>().Where(p=>p is RailwayVendor||p is CombatMerchant))
                {foreach(var draw in source.transform.parent.GetComponentsInChildren<Renderer>())draw.enabled=false;foreach(var oldShape in source.transform.parent.GetComponentsInChildren<Collider>())oldShape.enabled=false;}
                foreach(string name in new[]{"Purchase Console Backing","Weapon Console"}){var old=shop.Find(name);if(old!=null)old.gameObject.SetActive(false);}
                foreach(var stock in shop.GetComponentsInChildren<Transform>().Where(t=>t.name=="Steel Stock"||t.name=="Coal Stock"))stock.gameObject.SetActive(false);
                var obj=new GameObject("Shopkeeper");obj.transform.SetParent(shop,false);obj.transform.localPosition=new Vector3(.4f,.65f,.95f);
                var shape=obj.AddComponent<BoxCollider>();shape.center=new Vector3(0,1.2f,0);shape.size=new Vector3(.9f,1.5f,.7f);
                var vendor=obj.AddComponent<ShopVendor>();vendor.ui=ui;vendor.wallet=world.Train.GetComponent<RailwayInventory>();vendor.ShopTitle=shop.name.StartsWith("Guns And Supplies")?"Guns & Field Supplies":shop.GetComponentsInChildren<RailwayVendor>().Any(v=>v.Product==RailwayProduct.Steel)?"Frontier Supply Co.":"Odds & Ends";
                foreach(var product in shop.GetComponentsInChildren<RailwayVendor>())vendor.Offers.Add(new Offer{Rail=product});
                foreach(var product in shop.GetComponentsInChildren<CombatMerchant>())vendor.Offers.Add(new Offer{Combat=product});
                var prefab=Resources.Load<GameObject>(shop.name.StartsWith("Guns")?"BrightCharacterYellow":"BrightCharacterGreen");if(prefab!=null){var visual=Object.Instantiate(prefab,obj.transform,false);visual.transform.localRotation=Quaternion.Euler(0,180,0);}
                var palette=world.Train.GetComponent<TrainDayEconomy>().Palette;
                var cap=FrontierModelMesh.Part(obj.transform,"Shopkeeper Cap",new Vector3(0,1.75f,0),new Vector3(.7f,.1f,.7f),palette[6],FrontierModelMesh.Cylinder(12));
                FrontierModelMesh.Box(obj.transform,"Shopkeeper Apron",new Vector3(0,.7f,-.22f),new Vector3(.4f,.5f,.045f),palette[5]);
                obj.AddComponent<ControlHighlight>().Configure(palette[9],new[]{cap.GetComponent<Renderer>()});
                RouteProps.Box(shop,"Collection Tray",new Vector3(-.8f,1.36f,-.1f),new Vector3(1.2f,.14f,.85f),palette[2],true);
                RouteProps.Board(shop,"COLLECT HERE",new Vector3(-.8f,1.25f,-.58f),new Vector3(1.15f,.2f,.04f),palette,Quaternion.identity);
            }
        }
    }
}
