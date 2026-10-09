using System.Linq;
using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
 public sealed class CombatWorld:MonoBehaviour
 {
  public ProceduralRailway World {get;private set;}private Transform root;private Material[] palette;public System.Collections.Generic.List<BanditEncounter> Encounters {get;}=new System.Collections.Generic.List<BanditEncounter>();
  public void Configure(ProceduralRailway world,Material[] m)
  {
   World=world;palette=m;if(root!=null){root.gameObject.SetActive(false);if(Application.isPlaying)Destroy(root.gameObject);else DestroyImmediate(root.gameObject);}root=new GameObject("Weapons And Encounters").transform;root.SetParent(world.transform,false);if(!Application.isPlaying)root.gameObject.hideFlags=HideFlags.DontSaveInEditor;Encounters.Clear();
   foreach(var player in world.gameObject.scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<FirstPersonMotor>())){var health=player.GetComponent<CombatHealth>();if(health==null){health=player.gameObject.AddComponent<CombatHealth>();health.Configure(100,true);health.Defeated+=CrewDown;}var status=player.GetComponent<CombatPlayerStatus>();if(status==null)status=player.gameObject.AddComponent<CombatPlayerStatus>();status.Configure(this);}
   var bag=world.Train.GetComponent<RailwayInventory>();
   foreach(var node in world.Network.nodes.Where(n=>(n.place==RoutePlace.Redfort||n.place==RoutePlace.Town||n.place==RoutePlace.Bluefort||n.place==RoutePlace.Village)&&n.settlement!=SettlementKind.Abandoned))Shop(node,bag);
   if(world.Train.Cars[1].transform.Find("Starting Club")==null){var club=WeaponArt.Create(WeaponKind.Club,bag,m);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(club,gameObject.scene);club.name="Starting Club";club.transform.SetParent(world.Train.Cars[1].transform,false);club.transform.localPosition=new Vector3(1.4f,1.85f,-1.4f);club.GetComponent<Rigidbody>().isKinematic=true;club.AddComponent<StoredRailTool>();}
   var practice=new GameObject("Redfort Practice Range").transform;practice.SetParent(root,false);practice.position=world.Network.Node("Redfort").position+new Vector3(-14,0,25);RouteProps.Board(practice,"PRACTICE RANGE",new Vector3(0,2,-3),new Vector3(3,.4f,.08f),palette,Quaternion.identity);
   foreach(float x in new[]{-1.2f,1.2f})RouteProps.Box(practice,"Practice Sign Post",new Vector3(x,.9f,-3),new Vector3(.12f,1.8f,.12f),m[2],true);
   for(int i=0;i<3;i++){var dummy=RouteProps.Box(practice,"Practice Target",new Vector3((i-1)*2f,1.4f,4+i*3),new Vector3(.7f,1.3f,.3f),m[2],true);dummy.AddComponent<CombatHealth>().Configure(1000);RouteProps.Board(dummy.transform,"TARGET",new Vector3(0,0,-.55f),new Vector3(.6f,.25f,.06f),m,Quaternion.identity);}
   foreach(var edge in world.Network.edges.Where(e=>!e.crossover&&e.family==RouteFamily.Hard&&e.id.Contains("Stage")).Take(1))Encounter(edge);
   // Guarantee the first encounter on a main hard branch, independently of scenic landmark rolls.
   if(Encounters.Count==0){var edge=world.Network.edges.First(e=>e.family==RouteFamily.Hard&&!e.crossover&&e.id!="StarterRisky");Encounter(edge);}
  }
  private void Encounter(RouteEdge edge){var pathObj=new GameObject("Encounter Survey");var path=pathObj.AddComponent<RailPath>();path.Configure(edge.points);path.Sample(path.Length*.65f,out var p,out var q);if(Application.isPlaying)Destroy(pathObj);else DestroyImmediate(pathObj);var site=new GameObject("Bandit Toll Camp");site.transform.SetParent(root,false);site.transform.SetPositionAndRotation(p,q);var encounter=site.AddComponent<BanditEncounter>();encounter.Configure(this,edge,palette);Encounters.Add(encounter);}
  private void Shop(RouteNode node,RailwayInventory bag){var shop=new GameObject("Guns And Supplies - "+node.title).transform;shop.SetParent(root,false);shop.position=node.position+new Vector3(28,0,73);VillageRoads.Ribbon(shop,"Outfitter Access Lane",new[]{new Vector3(0,0,-8),new Vector3(0,0,-3)},3f,palette[2],.04f);RouteProps.Box(shop,"Outfitter Boardwalk",new Vector3(0,.2f,0),new Vector3(8,.4f,6),palette[2],true);RouteProps.Box(shop,"Weapon Console",new Vector3(0,1.65f,1.24f),new Vector3(3.4f,2.4f,.2f),palette[2],false);RouteProps.Board(shop,"GUNS & FIELD SUPPLIES",new Vector3(0,2.75f,1.1f),new Vector3(3.6f,.4f,.08f),palette,Quaternion.identity);LandmarkCraftsmanship.Outfitter(shop,palette);for(int i=0;i<8;i++){var product=(CombatPurchase)i;float factor=node.place==RoutePlace.Village?bag.Config.villageMultiplier:1;string label=(product==CombatPurchase.Ammunition?"AMMO 10":product.ToString().ToUpperInvariant())+"\n"+Mathf.RoundToInt(bag.Config.CombatPrice(product)*factor)+" GOLD";var cap=WorldButtonArt.Create(shop,new Vector3(-1.2f+(i%4)*.8f,1.25f+(i/4)*.72f,1.12f),label,palette);cap.AddComponent<CombatMerchant>().Configure(bag,palette,product,factor);}}
  private void CrewDown(){if(World.DayEnded)return;var crew=World.gameObject.scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<CombatHealth>()).Where(h=>h.IsPlayer).ToArray();if(crew.Length>0&&crew.All(h=>h.Down)){World.Train.GetComponent<TrainDayEconomy>().Abort("Run ended — crew unconscious");World.FinishEconomicDay("Run ended — crew unconscious");}}
 }
 public sealed class CombatPlayerStatus:MonoBehaviour
 {
  private CombatWorld combat;public void Configure(CombatWorld owner)=>combat=owner;
  private void OnGUI(){if(combat==null)return;var health=GetComponent<CombatHealth>();GUI.Label(new Rect(18,18,350,28),"Health "+health.Health.ToString("0")+" / "+health.Maximum.ToString("0"));var item=GetComponent<PlayerInteraction>().HeldItem;var weapon=item!=null?item.GetComponent<WeaponItem>():null;if(weapon!=null)GUI.Label(new Rect(Screen.width*.5f-230,Screen.height-105,460,30),weapon.Status);else if(item!=null&&item.GetComponent<MedicalItem>()!=null)GUI.Label(new Rect(Screen.width*.5f-140,Screen.height-105,280,30),"M1: use medical item");}
 }
}
