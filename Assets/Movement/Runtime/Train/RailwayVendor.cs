using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public enum RailwayProduct { Steel, Coal, Saw, Connector, Scrap, Screws, Wood }
    public sealed class RailwayVendor : PrototypeInteractable
    {
        private TrainController train;
        private GameObject coal;
        private Material[] palette;
        public RailwayProduct Product { get; private set; }
        private float priceMultiplier=1f;
        private SalvageCategory specialization=SalvageCategory.RailParts;
        private bool allPurpose;
        public int Price => Mathf.RoundToInt((train!=null?train.GetComponent<RailwayInventory>()?.Config:EconomyConfig.Default)?.ProductPrice(Product)*priceMultiplier??0f);
        public override string Prompt => "M1 / E: buy " + (Product == RailwayProduct.Steel ? "5 m steel" : Product.ToString().ToLowerInvariant()) + " | " + Price + " Gold | held salvage: sell";
        public void Configure(TrainController owner, RailwayProduct product, GameObject coalPrefab, Material[] materials)
        { train = owner; Product = product; coal = coalPrefab; palette = materials; }
        public void SetPriceMultiplier(float multiplier)=>priceMultiplier=Mathf.Max(.01f,multiplier);
        public void SetSpecialization(SalvageCategory category,bool general=false){specialization=category;allPurpose=general;}
        public int SalePrice(LootCargo cargo)
        {var config=train.GetComponent<RailwayInventory>().Config;return Mathf.RoundToInt(cargo.Value*(allPurpose||cargo.Category==specialization?1f:cargo.Category==SalvageCategory.General?config.relevantSaleMultiplier:config.irrelevantSaleMultiplier));}
        public override bool Interact(PlayerInteraction actor)
        {
            if (actor == null || train == null) return false;
            var wallet = train.GetComponent<RailwayInventory>();
            if(wallet==null||wallet.Settled)return false;
            var button = GetComponent<TrainButton>();
            var cargo=actor.HeldItem!=null?actor.HeldItem.GetComponent<LootCargo>():null;
            if(cargo!=null&&wallet!=null)
            {if(button!=null&&button.IsPressed)return false;int value=SalePrice(cargo);if(!actor.ConsumeHeldItem())return false;wallet.Earn(value);button?.Press(actor);return true;}
            if(Product!=RailwayProduct.Steel&&actor.IsHolding)return false;
            if (wallet == null || (button != null && button.IsPressed) || !wallet.CanPurchase(Price)) return false;
            if (Product == RailwayProduct.Steel)
            { if (!wallet.PurchaseSteel(Price)) return false; }
            else if(Product>=RailwayProduct.Scrap){if(!wallet.Purchase(Price))return false;wallet.AddStock(Product);}
            else
            {
                if (Product == RailwayProduct.Coal && coal == null) return false;
                var item = Product == RailwayProduct.Coal ? Instantiate(coal) : RailTool.Create(Product == RailwayProduct.Saw ? RailToolKind.Saw : RailToolKind.Connector, palette);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(item, gameObject.scene);
                item.transform.position = transform.position + Vector3.up * .7f;
                if (!actor.TryPickup(item.GetComponent<PickupObject>())) { Destroy(item); return false; }
                wallet.Purchase(Price);
                if (Product == RailwayProduct.Coal) train.Resources.RegisterSupplyCoal();
            }
            button?.Press(actor);
            return true;
        }
    }
}
