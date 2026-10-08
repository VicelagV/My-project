using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public enum RailwayProduct { Steel, Coal, Saw, Connector }
    public sealed class RailwayVendor : PrototypeInteractable
    {
        private TrainController train;
        private GameObject coal;
        private Material[] palette;
        public RailwayProduct Product { get; private set; }
        public int Price => Product == RailwayProduct.Steel ? RailwayInventory.SteelPrice : Product == RailwayProduct.Coal ? 20 : Product == RailwayProduct.Saw ? 25 : 15;
        public override string Prompt => "M1 / E: buy " + (Product == RailwayProduct.Steel ? "5 m steel" : Product.ToString().ToLowerInvariant()) + " | " + Price + " Gold | held salvage: sell";
        public void Configure(TrainController owner, RailwayProduct product, GameObject coalPrefab, Material[] materials)
        { train = owner; Product = product; coal = coalPrefab; palette = materials; }
        public override bool Interact(PlayerInteraction actor)
        {
            if (actor == null || train == null) return false;
            var wallet = train.GetComponent<RailwayInventory>();
            var button = GetComponent<TrainButton>();
            var cargo=actor.HeldItem!=null?actor.HeldItem.GetComponent<LootCargo>():null;
            if(cargo!=null&&wallet!=null)
            {if(button!=null&&button.IsPressed)return false;int value=cargo.Value;if(!actor.ConsumeHeldItem())return false;wallet.Earn(value);button?.Press(actor);return true;}
            if(Product!=RailwayProduct.Steel&&actor.IsHolding)return false;
            if (wallet == null || (button != null && button.IsPressed) || wallet.Gold < Price) return false;
            if (Product == RailwayProduct.Steel)
            { if (!wallet.BuySteel()) return false; }
            else
            {
                if (Product == RailwayProduct.Coal && coal == null) return false;
                var item = Product == RailwayProduct.Coal ? Instantiate(coal) : RailTool.Create(Product == RailwayProduct.Saw ? RailToolKind.Saw : RailToolKind.Connector, palette);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(item, gameObject.scene);
                item.transform.position = transform.position + Vector3.up * .7f;
                if (!actor.TryPickup(item.GetComponent<PickupObject>())) { Destroy(item); return false; }
                wallet.Spend(Price);
                if (Product == RailwayProduct.Coal) train.Resources.RegisterSupplyCoal();
            }
            button?.Press(actor);
            return true;
        }
    }
}
