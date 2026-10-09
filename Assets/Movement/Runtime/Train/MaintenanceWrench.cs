using AllAboard.Movement;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class MaintenanceWrench:PrototypeInteractable
    {
        private TrainPart part;private float stroke;private int direction,strokes;private Quaternion rest;
        public bool Mounted=>part!=null;
        public bool Finished {get;private set;}
        public float Progress=>strokes/8f;
        public override string Prompt=>Mounted&&!Finished?"Hold M1: turn wrench forward/back · E: take wrench":"E: take wrench";
        public static GameObject Create(Material[] m)
        {
            var obj=new GameObject("Maintenance Wrench");var pickup=obj.AddComponent<PickupObject>();obj.GetComponent<BoxCollider>().size=new Vector3(.2f,.12f,.65f);obj.GetComponent<Rigidbody>().mass=.6f;
            RouteProps.Box(obj.transform,"Steel Wrench Handle",Vector3.zero,new Vector3(.085f,.055f,.47f),m[3],false);
            RouteProps.Box(obj.transform,"Wrench Grip",new Vector3(0,0,-.12f),new Vector3(.11f,.08f,.22f),m[7],false);
            foreach(float side in new[]{-1f,1f})RouteProps.Box(obj.transform,"Open Wrench Jaw",new Vector3(side*.08f,0,.26f),new Vector3(.07f,.065f,.13f),m[3],false);
            RouteProps.Box(obj.transform,"Jaw Bridge",new Vector3(0,0,.2f),new Vector3(.23f,.065f,.055f),m[3],false);
            var outline=RouteProps.Box(obj.transform,"Wrench Hover Outline",Vector3.zero,new Vector3(.27f,.14f,.68f),m[9],false).GetComponent<Renderer>();pickup.Configure(outline);outline.enabled=false;obj.AddComponent<MaintenanceWrench>();return obj;
        }
        public void Mount(TrainPart owner){part=owner;transform.SetParent(owner.transform,false);transform.localPosition=new Vector3(owner.Product==RailwayProduct.BoilerPart?-.14f:.14f,.16f,0);transform.localRotation=Quaternion.LookRotation(Vector3.down,owner.Product==RailwayProduct.BoilerPart?Vector3.left:Vector3.right);rest=transform.localRotation;GetComponent<Rigidbody>().isKinematic=true;GetComponent<Rigidbody>().useGravity=false;strokes=direction=0;stroke=0;Finished=false;}
        public bool Drag(float mouseY)
        {
            if(!Mounted||Finished||!part.CanRepair)return false;if(Mathf.Abs(mouseY)<.01f)return true;int sign=mouseY>0?1:-1;if(sign==direction)return true;if(Mathf.Sign(stroke)!=sign)stroke=0;stroke+=mouseY;transform.localRotation=rest*Quaternion.Euler(0,Mathf.Clamp(stroke,-30,30),0);
            if(Mathf.Abs(stroke)>=24){direction=sign;stroke=0;strokes++;if(strokes>=8){Finished=part.CompleteRepair();transform.localRotation=rest;}}return true;
        }
        public override bool Interact(PlayerInteraction actor){if(actor==null||actor.IsHolding||actor.IsUsingHandControl)return false;transform.SetParent(null,true);if(!actor.TryPickup(GetComponent<PickupObject>()))return false;part=null;Finished=false;return true;}
    }
}
