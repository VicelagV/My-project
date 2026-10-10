using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
 public enum CombatPurchase { Revolver,Rifle,Shotgun,Club,Hatchet,Ammunition,Medical,Recovery }
 public sealed class CombatMerchant:PrototypeInteractable
 {
  private RailwayInventory bag;private Material[] m;private float multiplier;public CombatPurchase Product {get;private set;}public int Price=>Mathf.RoundToInt(bag.Config.CombatPrice(Product)*multiplier);
  public void Configure(RailwayInventory inventory,Material[] palette,CombatPurchase product,float markup=1){bag=inventory;m=palette;Product=product;multiplier=markup;}
  public override string Prompt=>"M1 / E: buy "+Product+" · "+Price+" Gold";
  public override bool Interact(PlayerInteraction actor)
  {
   var button=GetComponent<TrainButton>();if(button!=null&&button.IsPressed)return false;if(!TryBuy(actor))return false;button?.Press(actor);return true;
  }
  public bool TryBuy(PlayerInteraction actor,Vector3? delivery=null)
  {
   if(actor==null||bag.Settled||!bag.CanPurchase(Price))return false;
   if(Product==CombatPurchase.Ammunition){if(!bag.Purchase(Price))return false;bag.AddStock(RailwayProduct.Ammunition,10);}
   else{if(actor.IsHolding&&!delivery.HasValue)return false;var item=Product<=CombatPurchase.Hatchet?WeaponArt.Create((WeaponKind)Product,bag,m):MedicalItem.Create(Product==CombatPurchase.Recovery,m);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(item,gameObject.scene);item.transform.position=delivery??(transform.position+Vector3.up*.7f);if(!delivery.HasValue&&!actor.TryPickup(item.GetComponent<PickupObject>())){Destroy(item);return false;}bag.Purchase(Price);}
   return true;
  }
 }
 public sealed class MedicalItem:MonoBehaviour
 {
  private float healing;
  public static GameObject Create(bool recovery,Material[] m){var root=new GameObject(recovery?"Recovery Tonic":"Medical Bottle");root.AddComponent<Rigidbody>().mass=.35f;root.AddComponent<BoxCollider>().size=new Vector3(.2f,.36f,.2f);var pickup=root.AddComponent<PickupObject>();RouteProps.Box(root.transform,"Bottle",Vector3.zero,new Vector3(.17f,.3f,.17f),m[4],false);RouteProps.Box(root.transform,"Bottle Cap",new Vector3(0,.17f,0),new Vector3(.13f,.05f,.13f),m[6],false);RouteProps.Board(root.transform,recovery?"RECOVERY":"MEDICAL",new Vector3(0,0,-.1f),new Vector3(.16f,.1f,.03f),m,Quaternion.identity);var bevel=Resources.Load<Mesh>("CraftBevelBox");if(bevel!=null)root.transform.Find("Bottle").GetComponent<MeshFilter>().sharedMesh=bevel;var fittings=CraftGeometry.Group(root.transform,"Detailed Medical Bottle");for(int i=0;i<8;i++){float angle=i*Mathf.PI/4;CraftGeometry.Box(fittings,"Bottle Cap Serration",new Vector3(Mathf.Cos(angle)*.065f,.17f,Mathf.Sin(angle)*.065f),new Vector3(.018f,.035f,.018f),m[3]);}CraftGeometry.Bake(fittings);var outline=RouteProps.Box(root.transform,"Bottle Hover Outline",Vector3.zero,new Vector3(.23f,.4f,.23f),m[9],false).GetComponent<Renderer>();pickup.Configure(outline);outline.enabled=false;root.AddComponent<MedicalItem>().healing=recovery?100:35;return root;}
  public bool Use(PlayerInteraction actor){if(actor==null||actor.HeldItem!=GetComponent<PickupObject>())return false;var health=actor.GetComponent<CombatHealth>();if(health==null||!health.Heal(healing))return false;return actor.ConsumeHeldItem();}
 }
}
