using System.Linq;
using AllAboard.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
namespace AllAboard.Train
{
 public enum WeaponKind { Revolver,Rifle,Shotgun,Club,Hatchet }
 public sealed class WeaponItem:MonoBehaviour
 {
  public WeaponKind Kind {get;private set;}public int Loaded {get;private set;}public int Capacity=>Kind==WeaponKind.Revolver?6:Kind==WeaponKind.Rifle?5:Kind==WeaponKind.Shotgun?2:0;public bool IsGun=>Capacity>0;public float ReloadRemaining=>Mathf.Max(0,reloadAt-Time.time);public Quaternion PoseOffset=>Quaternion.Euler(-kick*12f,IsGun?0:Mathf.Sin(kick*Mathf.PI)*55f,IsGun?0:-kick*35f);
  private EconomyConfig config;private RailwayInventory wallet;private PlayerInteraction holder;private float readyAt,reloadAt,kick;private int pending;private Transform muzzle;private AudioSource sound;private Renderer flash;
  public void Configure(WeaponKind kind,RailwayInventory bag,Transform barrel,Renderer light){Kind=kind;wallet=bag;config=bag.Config;muzzle=barrel;flash=light;flash.enabled=false;sound=gameObject.AddComponent<AudioSource>();sound.spatialBlend=1;sound.maxDistance=65;}
  public float Damage=>config.weaponDamage[(int)Kind];public float Range=>config.weaponRange[(int)Kind];
  public bool Reload(float now){if(!IsGun||pending>0||Loaded>=Capacity||holder==null)return false;pending=Mathf.Min(Capacity-Loaded,wallet.Stock(RailwayProduct.Ammunition));if(pending==0)return false;wallet.ConsumeStock(RailwayProduct.Ammunition,pending);reloadAt=now+config.weaponReloadSeconds[(int)Kind];return true;}
  public void FinishReload(float now){if(pending>0&&now>=reloadAt){Loaded+=pending;pending=0;reloadAt=0;}}
  private void Update(){kick=Mathf.MoveTowards(kick,0,Time.deltaTime*5);if(flash!=null)flash.enabled=IsGun&&kick>.75f;var pickup=GetComponent<PickupObject>();if(!pickup.IsHeld){holder=null;CancelReload();return;}FinishReload(Time.time);if(holder!=null&&holder.GetComponent<PlayerInputReader>().HasControl&&Keyboard.current!=null&&Keyboard.current.rKey.wasPressedThisFrame)Reload(Time.time);}
  public void SetHolder(PlayerInteraction actor){holder=actor;}
  public void CancelReload(){if(pending>0){wallet.AddStock(RailwayProduct.Ammunition,pending);pending=0;reloadAt=0;}}
  private void OnDestroy()=>CancelReload();
  public bool Attack(PlayerInteraction actor,Transform aim,float now)
  {
   if(actor==null||aim==null||actor.HeldItem!=GetComponent<PickupObject>()||pending>0||now<readyAt||wallet.Settled||IsGun&&Loaded<=0)return false;
   holder=actor;if(IsGun)Loaded--;readyAt=now+config.weaponCooldown[(int)Kind];kick=1;
   int pellets=Kind==WeaponKind.Shotgun?6:1;
   for(int i=0;i<pellets;i++)
   {
    Vector3 direction=aim.forward;if(pellets>1){float angle=i*Mathf.PI/3;direction=(direction+aim.right*Mathf.Cos(angle)*.045f+aim.up*Mathf.Sin(angle)*.045f).normalized;}
    var hits=(IsGun?Physics.RaycastAll(aim.position,direction,Range,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore):Physics.SphereCastAll(aim.position,.2f,direction,Range,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)).OrderBy(h=>h.distance);
    Vector3 end=aim.position+direction*Range;foreach(var hit in hits){if(hit.collider.transform.IsChildOf(actor.transform)||hit.collider.transform.IsChildOf(transform))continue;end=hit.point;hit.collider.GetComponentInParent<CombatHealth>()?.Damage(Damage,true);break;}
    if(IsGun)CombatEffects.Trace(gameObject.scene,muzzle.position,end);else CombatEffects.Impact(gameObject.scene,end);
   }
   sound.PlayOneShot(CombatEffects.Sound(Kind));return true;
  }
  public string Status=>Kind+" · "+(IsGun?(pending>0?"Reloading…":Loaded+" / "+Capacity+" · reserve "+wallet.Stock(RailwayProduct.Ammunition))+" · M1 fire · R reload":"M1 swing")+" · Q drop";
 }
}
