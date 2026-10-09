using System.Linq;
using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
 public enum EncounterState { Waiting,Hostile,Paid,Defeated,Escaped }
 public sealed class BanditEncounter:MonoBehaviour
 {
  public EncounterState State {get;private set;}public int Toll {get;private set;}public string EdgeId {get;private set;}public CombatWorld Combat {get;private set;}private BanditAgent[] agents;private TextMesh sign;private bool warned;
  public string Prompt=>State==EncounterState.Waiting?"M1 / E: pay bandit toll · "+Toll+" Gold":State.ToString();
  public void Configure(CombatWorld owner,RouteEdge edge,Material[] m)
  {
   Combat=owner;EdgeId=edge.id;var config=owner.World.Train.GetComponent<RailwayInventory>().Config;Toll=new System.Random(WorldArtAssets.Hash(edge.id+owner.World.DailySeed)).Next(config.banditToll.x,config.banditToll.y+1);
   RouteProps.Box(transform,"Warning Post",new Vector3(4,1,-90),new Vector3(.15f,2,.15f),m[2],true);RouteProps.Board(transform,"BANDIT TOLL AHEAD\nSTOP OR TAKE YOUR CHANCES",new Vector3(4,2.2f,-90),new Vector3(3.4f,.7f,.08f),m,Quaternion.identity);
   RouteProps.Box(transform,"Toll Desk",new Vector3(4.7f,.6f,-22),new Vector3(2.8f,1.2f,1),m[2],true);sign=RouteProps.Board(transform,"BANDIT TOLL · "+Toll+" GOLD",new Vector3(4.7f,2.3f,-22.62f),new Vector3(3,.5f,.08f),m,Quaternion.identity);
   RouteProps.Box(transform,"Toll Console Backing",new Vector3(4.7f,1.8f,-22.48f),new Vector3(2.8f,1.6f,.18f),m[2],false);
   var button=WorldButtonArt.Create(transform,new Vector3(4.7f,1.4f,-22.6f),"PAY TOLL\n"+Toll+" GOLD",m);button.AddComponent<BanditTollButton>().Configure(this);
   foreach(float x in new[]{-6f,6f}){RouteProps.Box(transform,"Bandit Barricade",new Vector3(x,.7f,0),new Vector3(3,1.4f,.6f),m[2],true);RouteProps.Box(transform,"Barricade Iron",new Vector3(x,1.2f,-.34f),new Vector3(2.6f,.12f,.08f),m[3],false);}
   RouteProps.Box(transform,"Camp Shelter Roof",new Vector3(12,2.8f,10),new Vector3(6,.16f,6),m[7],false);foreach(float x in new[]{9f,15f})foreach(float z in new[]{7f,13f})RouteProps.Box(transform,"Camp Shelter Post",new Vector3(x,1.4f,z),new Vector3(.13f,2.8f,.13f),m[2],true);
   agents=new BanditAgent[3];for(int i=0;i<agents.Length;i++){var obj=new GameObject("Bandit "+(i+1));obj.transform.SetParent(transform,false);obj.transform.localPosition=new Vector3(i==0?-5:7+i*2,0,i*3);var capsule=obj.AddComponent<CapsuleCollider>();capsule.radius=.28f;capsule.height=1.7f;capsule.center=Vector3.up*.85f;var body=obj.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;var health=obj.AddComponent<CombatHealth>();health.Configure(config.banditHealth);var prefab=Resources.Load<GameObject>("BrightCharacterYellow");Object.Instantiate(prefab,obj.transform,false);RouteProps.Box(obj.transform,"Bandit Hat Brim",new Vector3(0,1.72f,0),new Vector3(.9f,.08f,.65f),m[5],false);RouteProps.Box(obj.transform,"Bandit Hat Crown",new Vector3(0,1.88f,0),new Vector3(.5f,.27f,.45f),m[5],false);var gun=WeaponArt.Create(WeaponKind.Revolver,owner.World.Train.GetComponent<RailwayInventory>(),m);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gun,gameObject.scene);gun.transform.SetParent(obj.transform,false);gun.transform.localPosition=new Vector3(.35f,1.05f,.24f);gun.GetComponent<Rigidbody>().isKinematic=true;gun.GetComponent<Collider>().enabled=false;gun.GetComponent<PickupObject>().enabled=false;agents[i]=obj.AddComponent<BanditAgent>();agents[i].Configure(this,m);}
  }
  public bool Pay(){if(State!=EncounterState.Waiting||Combat.World.DayEnded||Mathf.Abs(Combat.World.Train.Speed)>.1f||Vector3.Distance(Combat.World.Train.Cars[0].transform.position,transform.position)>70)return false;var bag=Combat.World.Train.GetComponent<RailwayInventory>();if(!bag.Spend(Toll))return false;State=EncounterState.Paid;Refresh();return true;}
  public void Provoke(){if(State==EncounterState.Waiting||State==EncounterState.Paid){State=EncounterState.Hostile;Refresh();}}
  public void MemberDefeated(){if(agents!=null&&agents.All(a=>a.GetComponent<CombatHealth>().Down)){State=EncounterState.Defeated;Refresh();}}
  private void Refresh(){if(sign==null)return;sign.text=State==EncounterState.Waiting?"BANDIT TOLL · "+Toll+" GOLD":"CAMP · "+State.ToString().ToUpperInvariant();sign.GetComponent<PlaqueLettering>()?.Fit();}
  private void Update(){if(Combat.World.DayEnded)return;var loco=Combat.World.Train.Cars[0].transform;var relative=transform.InverseTransformPoint(loco.position);if(!warned&&Mathf.Abs(relative.x)<15&&Mathf.Abs(relative.z)<100){warned=true;foreach(var p in Combat.World.Train.GetComponent<TrainDayEconomy>().Passengers.Where(p=>!p.Resolved))p.ChangeHappiness(-8);}if(State==EncounterState.Waiting&&Mathf.Abs(relative.x)<12&&Mathf.Abs(relative.z)<16)Provoke();if(State==EncounterState.Hostile&&relative.z>180){State=EncounterState.Escaped;Refresh();}}
 }
 public sealed class BanditTollButton:PrototypeInteractable
 {
  private BanditEncounter encounter;public void Configure(BanditEncounter owner)=>encounter=owner;public override string Prompt=>encounter.Prompt;public override bool Interact(PlayerInteraction actor){if(actor==null||GetComponent<TrainButton>().IsPressed||!encounter.Pay())return false;GetComponent<TrainButton>().Press(actor);return true;}
 }
 public sealed class BanditAgent:MonoBehaviour
 {
  private BanditEncounter encounter;private CombatHealth health;private Material[] m;private float shootAt;private Vector3 home;private AudioSource shots;
  public void Configure(BanditEncounter owner,Material[] palette){encounter=owner;m=palette;home=transform.position;health=GetComponent<CombatHealth>();health.Defeated+=Defeated;shots=gameObject.AddComponent<AudioSource>();shots.spatialBlend=1;shots.maxDistance=65;}
  private void Defeated(){var loot=new GameObject("Bandit Salvage");loot.transform.SetParent(encounter.transform,true);loot.transform.position=transform.position+Vector3.up*.35f;loot.AddComponent<Rigidbody>().mass=.8f;loot.AddComponent<BoxCollider>().size=Vector3.one*.35f;var pickup=loot.AddComponent<PickupObject>();RouteProps.Box(loot.transform,"Salvage Box",Vector3.zero,Vector3.one*.34f,m[2],false);CraftGeometry.Crate(loot.transform,Vector3.one*.34f,m);var outline=RouteProps.Box(loot.transform,"Loot Outline",Vector3.zero,Vector3.one*.39f,m[9],false).GetComponent<Renderer>();pickup.Configure(outline);outline.enabled=false;var cargo=loot.AddComponent<LootCargo>();cargo.Value=40;cargo.Category=SalvageCategory.General;GetComponent<Collider>().enabled=false;foreach(var r in GetComponentsInChildren<Renderer>())r.enabled=false;encounter.MemberDefeated();}
  private void Update()
  {
   if(health.Down||encounter.Combat.World.DayEnded)return;if(health.Health<health.Maximum)encounter.Provoke();if(encounter.State!=EncounterState.Hostile)return;
   var target=encounter.Combat.World.gameObject.scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<CombatHealth>()).Where(h=>h.IsPlayer&&!h.Down).OrderBy(h=>(h.transform.position-transform.position).sqrMagnitude).FirstOrDefault();if(target==null)return;float distance=Vector3.Distance(target.transform.position,transform.position);if(distance>35)return;
   var delta=target.transform.position-transform.position;delta.y=0;if(delta.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(delta);
   if(distance>7&&Vector3.Distance(transform.position,home)<20){var step=delta.normalized*Time.deltaTime*2;bool blocked=Physics.CapsuleCastAll(transform.position+Vector3.up*.35f,transform.position+Vector3.up*1.35f,.29f,step.normalized,step.magnitude+.05f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore).Any(h=>!h.collider.transform.IsChildOf(transform));if(!blocked){var p=transform.position+step;p.y=encounter.Combat.World.Geometry.GroundHeight(p);GetComponent<Rigidbody>().position=p;transform.position=p;}}
   if(Time.time<shootAt)return;shootAt=Time.time+encounter.Combat.World.Train.GetComponent<RailwayInventory>().Config.banditFireInterval;
   Vector3 start=transform.position+Vector3.up*1.2f,end=target.transform.position+Vector3.up*1.1f;foreach(var hit in Physics.RaycastAll(start,(end-start).normalized,distance+2,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(hit.collider.transform.IsChildOf(transform))continue;float damage=encounter.Combat.World.Train.GetComponent<RailwayInventory>().Config.banditDamage;
    if(hit.collider.GetComponentInParent<CombatHealth>()==target)target.Damage(damage);
    else{var car=hit.collider.GetComponentInParent<TrainCar>();if(car!=null&&car.Owner==encounter.Combat.World.Train){car.Owner.GetComponent<TrainMaintenance>()?.TrackImpact(.8f);foreach(var passenger in car.GetComponentsInChildren<PassengerJourney>())passenger.ChangeHappiness(-.5f);}hit.collider.GetComponentInParent<CargoCrate>()?.Damage(damage*.5f);}
    shots.PlayOneShot(CombatEffects.Sound(WeaponKind.Revolver));CombatEffects.Trace(gameObject.scene,start,hit.point);break;}
  }
 }
}
