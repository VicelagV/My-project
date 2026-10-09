using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class StationCoalSupply : PrototypeInteractable
    {
        private TrainResources resources;
        private GameObject coal;
        private RailwayInventory Wallet=>resources!=null?resources.GetComponent<RailwayInventory>():null;
        public override string Prompt=>Wallet==null||Wallet.Config.freePrototypeCoal?"E: take test coal (free)":"E: buy coal | "+Wallet.Config.coalPrice+" Gold";
        public void Configure(TrainResources owner,GameObject prefab){resources=owner;coal=prefab;}
        public override bool Interact(PlayerInteraction actor)
        {
            if(actor==null||actor.IsHolding||coal==null||resources==null)return false;
            var wallet=Wallet;bool paid=wallet!=null&&!wallet.Config.freePrototypeCoal;
            if(paid&&!wallet.CanPurchase(wallet.Config.coalPrice))return false;
            var item=Instantiate(coal,transform.position+Vector3.up*0.6f,Quaternion.identity);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(item,gameObject.scene);
            if(!actor.TryPickup(item.GetComponent<PickupObject>())){Destroy(item);return false;}
            if(paid)wallet.Purchase(wallet.Config.coalPrice);
            resources.RegisterSupplyCoal();return true;
        }
    }
}
