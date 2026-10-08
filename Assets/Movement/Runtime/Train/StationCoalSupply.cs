using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class StationCoalSupply : PrototypeInteractable
    {
        private TrainResources resources;
        private GameObject coal;
        public override string Prompt=>"E: take coal";
        public void Configure(TrainResources owner,GameObject prefab){resources=owner;coal=prefab;}
        public override bool Interact(PlayerInteraction actor)
        {
            if(actor==null||actor.IsHolding||coal==null||resources==null)return false;
            var item=Instantiate(coal,transform.position+Vector3.up*0.6f,Quaternion.identity);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(item,gameObject.scene);
            if(!actor.TryPickup(item.GetComponent<PickupObject>())){Destroy(item);return false;}
            resources.RegisterSupplyCoal();return true;
        }
    }
}
