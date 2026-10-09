using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class TrainPart:PrototypeInteractable
    {
        private RailwayInventory wallet;private Renderer panel;private MaterialPropertyBlock block;private Renderer preview,patch;
        public DamageableComponent Condition {get;private set;}
        public RailwayProduct Product {get;private set;}
        public bool Improvised {get;private set;}
        public bool CanRepair=>!wallet.Settled&&Condition.DamageFraction>.001f&&(wallet.Stock(Product)>0||wallet.Stock(RailwayProduct.Scrap)>0&&wallet.Stock(RailwayProduct.Screws)>0);
        public override string Prompt=>Product+" · "+Condition.Health.ToString("0")+"%\n"+(CanRepair?"Hold wrench, E: mount · M1: turn forward/back":Condition.DamageFraction<.001f?"Good condition":"Need replacement part or scrap + screws");
        public void Configure(RailwayProduct product,RailwayInventory inventory,Material[] m)
        {
            Product=product;wallet=inventory;Condition=gameObject.AddComponent<DamageableComponent>();panel=GetComponentInChildren<Renderer>();block=new MaterialPropertyBlock();
            preview=RouteProps.Box(transform,"Repair Preview",new Vector3(0,0,0),new Vector3(.16f,.53f,.67f),m[9],false).GetComponent<Renderer>();preview.enabled=false;
            patch=RouteProps.Box(transform,"Visible Repair Patch",Vector3.zero,new Vector3(.15f,.28f,.38f),m[6],false).GetComponent<Renderer>();patch.enabled=false;
        }

        private void Update(){if(panel==null)return;preview.enabled=GetComponent<ControlHighlight>().Highlighted&&CanRepair;float damage=Condition.DamageFraction;block.SetColor("_BaseColor",Color.Lerp(new Color(.15f,.19f,.23f),new Color(.56f,.16f,.08f),damage));panel.SetPropertyBlock(block);}
        public override bool Interact(PlayerInteraction actor)
        {
            if(actor==null||!CanRepair||actor.HeldItem==null)return false;var wrench=actor.HeldItem.GetComponent<MaintenanceWrench>();if(wrench==null)return false;
            actor.ReleaseCharged(0);wrench.Mount(this);return true;
        }
        public bool CompleteRepair()
        {
            if(!CanRepair)return false;
            if(wallet.Stock(Product)>0){wallet.ConsumeStock(Product);Condition.Repair(100);Improvised=false;}
            else{wallet.ConsumeStock(RailwayProduct.Scrap);wallet.ConsumeStock(RailwayProduct.Screws);Condition.Repair(35);Improvised=true;}
            patch.enabled=true;preview.enabled=false;return true;
        }
        private void OnCollisionEnter(Collision collision){float speed=collision.relativeVelocity.magnitude;if(speed>4f)Condition.Damage((speed-4f)*2f);}
    }
}
