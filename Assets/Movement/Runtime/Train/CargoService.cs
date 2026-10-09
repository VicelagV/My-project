using System.Collections.Generic;
using System.Linq;
using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class CargoOffer
    {public string id,title,origin,destination;public int reward;public bool Accepted,Resolved;public CargoOutcome Outcome;public CargoCrate crate;}
    public sealed class CargoService:MonoBehaviour
    {
        private readonly List<CargoOffer> offers=new List<CargoOffer>();
        public IReadOnlyList<CargoOffer> Offers=>offers;
        public TrainDayEconomy Economy {get;private set;}
        private Transform root;
        public void Configure(TrainDayEconomy owner,Transform parent)
        {
            foreach(var old in offers){old.Resolved=true;if(old.crate!=null){old.crate.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.crate.gameObject);else DestroyImmediate(old.crate.gameObject);}}
            offers.Clear();Economy=owner;root=parent;var rng=new System.Random(owner.World.DailySeed^6347);
            foreach(var node in owner.World.Network.nodes.Where(PassengerSchedule.BoardsAt))
            {
                var destinations=PassengerSchedule.Destinations(owner.World.Network,node.id);if(destinations.Length==0)continue;
                for(int i=0;i<2;i++)
                {
                    var to=i==0?destinations.OrderBy(n=>n.position.z).First():destinations[rng.Next(destinations.Length)];
                    int minimum=owner.Wallet.Config.cargoReward.x,maximum=owner.Wallet.Config.cargoReward.y;
                    if(node.family==RouteFamily.Hard)minimum=(minimum+maximum)/2;
                    offers.Add(new CargoOffer{id=node.id+"/"+i,title=i==0?"Machine parts":"Merchant goods",origin=node.id,destination=to.id,reward=rng.Next(minimum,maximum+1)});
                }
            }
        }
        public bool Accept(CargoOffer offer)
        {
            if(offer==null||!offers.Contains(offer)||offer.Accepted||offer.Resolved||Economy.Wallet.Settled||!Economy.AtSettlement(offer.origin)||offers.Count(o=>o.Accepted&&!o.Resolved)>=Economy.Wallet.Config.maxContracts||Economy.Palette==null)return false;
            var node=Economy.World.Network.Node(offer.origin);var obj=new GameObject("Contract Crate - "+offer.title);obj.transform.SetParent(root,false);obj.transform.position=node.position+new Vector3(11.7f,1.8f,-6.7f);
            var pickup=obj.AddComponent<PickupObject>();obj.GetComponent<BoxCollider>().size=new Vector3(.7f,.62f,.7f);obj.GetComponent<Rigidbody>().mass=4f;obj.GetComponent<Rigidbody>().interpolation=RigidbodyInterpolation.Interpolate;obj.GetComponent<Rigidbody>().collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            var m=Economy.Palette;
            RouteProps.Box(obj.transform,"Wooden Crate",Vector3.zero,new Vector3(.68f,.6f,.68f),m[2],false);
            foreach(float x in new[]{-.25f,.25f})RouteProps.Box(obj.transform,"Crate Iron Strap",new Vector3(x,0f,0f),new Vector3(.055f,.63f,.71f),m[3],false);
            var seal=RouteProps.Box(obj.transform,"Contract Seal",new Vector3(0f,.12f,-.355f),new Vector3(.19f,.14f,.025f),m[6],false).GetComponent<Renderer>();
            RouteProps.Board(obj.transform,Economy.DestinationTitle(offer.destination),new Vector3(0f,-.11f,-.36f),new Vector3(.5f,.16f,.025f),m,Quaternion.identity);
            var outline=RouteProps.Box(obj.transform,"Cargo Hover Outline",Vector3.zero,new Vector3(.75f,.67f,.75f),m[9],false).GetComponent<Renderer>();pickup.Configure(outline);outline.enabled=false;
            offer.Accepted=true;offer.crate=obj.AddComponent<CargoCrate>();offer.crate.Configure(this,offer,seal);return true;
        }
        public bool Deliver(CargoOffer offer,PlayerInteraction actor,Vector3 deliveryPoint)
        {
            if(offer==null||!offer.Accepted||offer.Resolved||offer.crate==null||Economy.Wallet.Settled||!Economy.AtSettlement(offer.destination)||Vector3.Distance(offer.crate.transform.position,deliveryPoint)>4f)return false;
            var pickup=offer.crate.GetComponent<PickupObject>();if(pickup.IsHeld&&(actor==null||actor.HeldItem!=pickup))return false;
            offer.Outcome=offer.crate.Health<99f?CargoOutcome.Damaged:CargoOutcome.Intact;offer.Resolved=true;Economy.Wallet.ResolveCargo(offer.reward,offer.Outcome);
            if(pickup.IsHeld)actor.ConsumeHeldItem();else{offer.crate.gameObject.SetActive(false);Destroy(offer.crate.gameObject);}return true;
        }
        public void Fail(CargoOffer offer)
        {if(offer==null||!offer.Accepted||offer.Resolved)return;offer.Resolved=true;offer.Outcome=CargoOutcome.Failed;Economy.Wallet.ResolveCargo(offer.reward,CargoOutcome.Failed);}
        public void FailOutstanding(){foreach(var offer in offers.Where(o=>o.Accepted&&!o.Resolved))Fail(offer);}
    }
}
