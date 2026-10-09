using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
 public static class WeaponArt
 {
  public static GameObject Create(WeaponKind kind,RailwayInventory bag,Material[] m)
  {
   var root=new GameObject(kind.ToString());root.AddComponent<Rigidbody>().mass=kind==WeaponKind.Rifle?2.5f:1.3f;var collider=root.AddComponent<BoxCollider>();float length=kind==WeaponKind.Rifle?1.12f:kind==WeaponKind.Shotgun?.96f:kind==WeaponKind.Revolver?.48f:.72f;collider.size=new Vector3(kind==WeaponKind.Hatchet?.32f:.18f,kind<=WeaponKind.Shotgun?.44f:.32f,length);collider.center=new Vector3(kind==WeaponKind.Hatchet?.065f:0,kind<=WeaponKind.Shotgun?-.035f:0,0);var pickup=root.AddComponent<PickupObject>();
   void Box(string name,Vector3 at,Vector3 size,int material)=>RouteProps.Box(root.transform,name,at,size,m[material],false);
   void Round(string name,Vector3 at,Vector3 size,int material){var obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder);obj.name=name;obj.transform.SetParent(root.transform,false);obj.transform.localPosition=at;obj.transform.localRotation=Quaternion.Euler(90,0,0);obj.transform.localScale=size;obj.GetComponent<Renderer>().sharedMaterial=m[material];var c=obj.GetComponent<Collider>();c.enabled=false;if(Application.isPlaying)Object.Destroy(c);else Object.DestroyImmediate(c);}
   float tip=length*.5f;
   if(kind<=WeaponKind.Shotgun)
   {
    Box("Wooden Grip",new Vector3(0,-.09f,-length*.3f),new Vector3(.11f,.26f,.13f),2);Box("Receiver",new Vector3(0,.03f,-.08f),new Vector3(.15f,.13f,.22f),3);
    if(kind==WeaponKind.Revolver)Round("Revolver Cylinder",new Vector3(0,.04f,-.08f),new Vector3(.16f,.085f,.16f),3);
    else{Box("Wooden Stock",new Vector3(0,-.03f,-length*.33f),new Vector3(.11f,.18f,length*.34f),2);Box("Brass Butt Plate",new Vector3(0,-.03f,-length*.5f),new Vector3(.12f,.19f,.025f),6);Box("Wooden Foregrip",new Vector3(0,-.01f,length*.2f),new Vector3(.1f,.08f,length*.27f),2);}
    int count=kind==WeaponKind.Shotgun?2:1;for(int i=0;i<count;i++){float x=count==1?0:(i==0?-.047f:.047f);Round("Steel Barrel",new Vector3(x,.07f,length*.2f),new Vector3(.075f,length*.3f,.075f),3);Round("Dark Muzzle",new Vector3(x,.07f,tip+.002f),new Vector3(.052f,.006f,.052f),5);}
    Box("Front Sight",new Vector3(0,.125f,tip-.04f),new Vector3(.025f,.05f,.025f),6);
    foreach(float side in new[]{-1f,1f})Box("Trigger Guard Side",new Vector3(side*.055f,-.105f,-.08f),new Vector3(.025f,.09f,.11f),6);Box("Trigger Guard Base",new Vector3(0,-.16f,-.08f),new Vector3(.13f,.025f,.11f),6);Box("Trigger",new Vector3(0,-.095f,-.08f),new Vector3(.025f,.06f,.025f),3);
   }
   else
   {
    Box("Wooden Handle",new Vector3(0,0,-.05f),new Vector3(.09f,.09f,.62f),2);Box("Handle Grip",new Vector3(0,0,-.25f),new Vector3(.11f,.11f,.2f),7);
    if(kind==WeaponKind.Hatchet){Box("Hatchet Steel Head",new Vector3(.07f,0,.27f),new Vector3(.26f,.075f,.16f),3);Box("Hatchet Cutting Edge",new Vector3(.205f,0,.27f),new Vector3(.015f,.09f,.18f),8);}else{Round("Club Head",new Vector3(0,0,.18f),new Vector3(.18f,.14f,.18f),2);Round("Club Iron Band",new Vector3(0,0,.26f),new Vector3(.19f,.025f,.19f),3);}
   }
   var bevel=Resources.Load<Mesh>("CraftBevelBox");
   if(bevel!=null)foreach(var part in root.GetComponentsInChildren<MeshFilter>())if(part.name=="Receiver"||part.name=="Wooden Grip"||part.name=="Wooden Stock"||part.name=="Wooden Foregrip"||part.name=="Hatchet Steel Head"||part.name=="Handle Grip")part.sharedMesh=bevel;
   if(kind==WeaponKind.Revolver)root.transform.Find("Receiver").localScale=new Vector3(.12f,.055f,.22f);
   EquipmentCraftsmanship.Weapon(root.transform,kind,length,m);
   var muzzle=new GameObject("Muzzle").transform;muzzle.SetParent(root.transform,false);muzzle.localPosition=new Vector3(0,.07f,tip+.06f);var flash=RouteProps.Box(root.transform,"Muzzle Flash",muzzle.localPosition,new Vector3(.14f,.14f,.17f),m[8],false).GetComponent<Renderer>();
   var outline=RouteProps.Box(root.transform,"Weapon Hover Outline",Vector3.zero,collider.size*1.04f,m[9],false).GetComponent<Renderer>();pickup.Configure(outline);outline.enabled=false;root.AddComponent<WeaponItem>().Configure(kind,bag,muzzle,flash);return root;
  }
 }
}
