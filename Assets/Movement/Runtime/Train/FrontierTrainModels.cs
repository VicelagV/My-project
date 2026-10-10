using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class FrontierTrainModels : MonoBehaviour
    {
        public const string ModelRoot="Rebuilt Frontier Train";
        private readonly List<Material> paints=new List<Material>();
        private Material[] m;
        public void Build(TrainCar car,Material[] palette)
        {
            if(transform.Find(ModelRoot)!=null)return;
            m=palette;var old=transform.Find("Train Visual Details");
            if(old==null)return;
            var craft=GetComponent<TrainCraftsmanship>();if(craft!=null)craft.enabled=false;
            var oldCraft=transform.Find("Detailed Train Craftsmanship");if(oldCraft!=null)oldCraft.gameObject.SetActive(false);
            foreach(var renderer in old.GetComponentsInChildren<MeshRenderer>())
            {
                string name=renderer.name;
                bool keep=name.Contains("Coupler")||name.Contains("Janney")||name.Contains("Knuckle")||name.Contains("Buffer")||name.Contains("Brake Hose")||name.Contains("Button")||name.Contains("Label")||name.Contains("Ladder")||name.Contains("Hinge")||name=="Moving Connecting Rod";
                if(!keep)renderer.enabled=false;
            }
            var root=CraftGeometry.Group(transform,ModelRoot);if(!Application.isPlaying)root.gameObject.hideFlags=HideFlags.DontSaveInEditor;
            Box(root,"Heavy Chassis",new Vector3(0,.86f,0),new Vector3(3.8f,.36f,8.8f),m[5]);
            foreach(float side in new[]{-1f,1f})
            {
                Box(root,"Continuous Chassis Sill",new Vector3(side*1.84f,.68f,0),new Vector3(.23f,.34f,8.5f),m[3]);
                foreach(float z in new[]{-2.8f,2.8f})
                {
                    Box(root,"Broad Bearing Housing",new Vector3(side*1.91f,.54f,z),new Vector3(.27f,.28f,.5f),m[3]);
                    Box(root,"Single Leaf Spring Pack",new Vector3(side*1.86f,.73f,z),new Vector3(.17f,.13f,.9f),m[5]);
                }
            }
            if(car.Kind==CarKind.Coal)Tender(root);else Cabin(root,car.Kind==CarKind.Locomotive);
            if(car.Kind==CarKind.Locomotive)Engine(root,old);
            if(car.Kind==CarKind.Passenger)Coach(root);
            CraftGeometry.Bake(root);
            foreach(var wheel in car.GetComponentsInChildren<TrainWheel>())Wheel(wheel);
            var rod=old.GetComponentsInChildren<TrainSideRod>().ToArray();foreach(var part in rod)part.transform.localScale=new Vector3(.14f,.17f,3f);
            Door();FrontWindows();
        }
        private void Cabin(Transform root,bool cab)
        {
            Material wall=cab?m[8]:m[5];float windowY=cab?2.96f:2.77f,windowH=cab?1.4f:1.1f;
            foreach(float side in new[]{-1f,1f})
            {
                var spans=side<0?new[]{new Vector2(0,8.8f)}:new[]{new Vector2(-1.95f,4.9f),new Vector2(3.45f,1.9f)};
                foreach(var span in spans)
                {
                    Box(root,"Solid Lower Side",new Vector3(side*1.9f,1.69f,span.x),new Vector3(.15f,1.08f,span.y),cab?m[5]:m[2]);
                    Box(root,"Broad Waist Moulding",new Vector3(side*1.94f,2.2f,span.x),new Vector3(.2f,.14f,span.y),wall);
                    Box(root,"Solid Upper Header",new Vector3(side*1.9f,cab?3.795f:3.59f,span.x),new Vector3(.15f,cab?.16f:.53f,span.y),wall);
                    if(!cab)Box(root,"Single Coach Colour Band",new Vector3(side*1.992f,1.69f,span.x),new Vector3(.028f,.36f,span.y-.1f),m[5]);
                }
                var holes=side<0?new[]{new Vector2(-2.8f,2.05f),new Vector2(0,2.05f),new Vector2(2.8f,2.05f)}:new[]{new Vector2(-2.85f,2.1f),new Vector2(-.55f,1.65f),new Vector2(3.45f,1.5f)};
                var intervals=new List<Vector2>();
                foreach(var pane in holes)
                {
                    intervals.Add(new Vector2(pane.x-pane.y*.5f,pane.x+pane.y*.5f));
                    foreach(float edge in new[]{-1f,1f})
                    {
                        Box(root,"Thick Window Jamb",new Vector3(side*1.97f,windowY,pane.x+edge*pane.y*.5f),new Vector3(.22f,windowH+.16f,.12f),m[8]);
                        Box(root,"Deep Window Sill",new Vector3(side*1.97f,windowY+edge*windowH*.5f,pane.x),new Vector3(.25f,.12f,pane.y+.12f),m[8]);
                    }
                    if(!cab)Box(root,"Coach Sash Bar",new Vector3(side*1.98f,windowY,pane.x),new Vector3(.12f,.055f,pane.y),m[8]);
                }
                if(side>0)intervals.Add(new Vector2(.5f,2.5f));float start=-4.4f;
                foreach(var span in intervals.OrderBy(i=>i.x).Concat(new[]{new Vector2(4.4f,4.4f)}))
                {if(span.x>start)Box(root,"Closed Window Infill",new Vector3(side*1.9f,windowY,(start+span.x)*.5f),new Vector3(.15f,cab?1.56f:1.18f,span.x-start+.018f),wall);start=Mathf.Max(start,span.y);}
                Box(root,"Rolled Roof Edge",new Vector3(side*1.93f,3.87f,0),new Vector3(.3f,.15f,8.98f),m[5]);
            }
            // Keep the established ladder opening and walkable roof; the shell is newly proportioned.
            var oldRoof=transform.Find("Train Visual Details/Rounded Roof").GetComponent<MeshFilter>().sharedMesh;
            var roof=new GameObject("Broad Barrel Roof",typeof(MeshFilter),typeof(MeshRenderer));roof.transform.SetParent(root,false);roof.transform.localPosition=new Vector3(0,3.84f,0);roof.transform.localScale=new Vector3(4.18f,.52f,9.06f);roof.GetComponent<MeshFilter>().sharedMesh=oldRoof;roof.GetComponent<Renderer>().sharedMaterial=m[5];
            Box(root,"Roof Walking Strip",new Vector3(-.15f,4.11f,0),new Vector3(1.1f,.03f,8.8f),m[3]);
            foreach(float end in new[]{-4.4f,4.4f})
            {
                if(cab&&end>0)continue;
                foreach(float x in new[]{-1.3f,1.3f})Box(root,"Closed End Pier",new Vector3(x,2.5f,end),new Vector3(1.2f,2.8f,.15f),wall);
                Box(root,"End Door Header",new Vector3(0,3.65f,end),new Vector3(1.42f,.5f,.18f),wall);
            }
        }
        private void Engine(Transform root,Transform old)
        {
            Round(root,"Sixteen Face Boiler",new Vector3(0,2.12f,6.35f),new Vector3(2.23f,4.2f,2.23f),m[5],Quaternion.Euler(90,0,0),16);
            foreach(float z in new[]{4.8f,7.1f})Round(root,"Wide Boiler Jacket",new Vector3(0,2.12f,z),new Vector3(2.29f,.16f,2.29f),m[6],Quaternion.Euler(90,0,0),16);
            Round(root,"Heavy Smokebox",new Vector3(0,2.12f,8.48f),new Vector3(2.15f,.2f,2.15f),m[3],Quaternion.Euler(90,0,0),16);
            Box(root,"Smokebox Cross Handle",new Vector3(0,2.12f,8.61f),new Vector3(.55f,.12f,.12f),m[6]);
            Round(root,"Tapered Diamond Stack",new Vector3(0,3.8f,7.48f),new Vector3(.9f,1.5f,.9f),m[5],Quaternion.identity,8,.31f,.5f);
            Round(root,"Thick Stack Crown",new Vector3(0,4.6f,7.48f),new Vector3(1.02f,.16f,1.02f),m[3],Quaternion.identity,8);
            Round(root,"Rounded Steam Dome",new Vector3(0,3.3f,5.5f),new Vector3(.85f,.55f,.85f),m[6],Quaternion.identity,12,.5f,.37f);
            Round(root,"Headlamp Body",new Vector3(0,2.95f,8.58f),new Vector3(.66f,.34f,.66f),m[5],Quaternion.Euler(90,0,0),8);
            Round(root,"Large Warm Headlamp Lens",new Vector3(0,2.95f,8.77f),new Vector3(.5f,.025f,.5f),m[8],Quaternion.Euler(90,0,0),12);
            Box(root,"Wide Front Running Deck",new Vector3(0,1.05f,6.45f),new Vector3(3.8f,.2f,4.1f),m[3]);
            Box(root,"Boiler Top Walkway",new Vector3(0,3.23f,6.4f),new Vector3(.9f,.08f,4.15f),m[3]);
            foreach(float side in new[]{-1f,1f})
            {
                Round(root,"Large Steam Cylinder",new Vector3(side*1.59f,.89f,7.98f),new Vector3(.65f,.9f,.65f),m[5],Quaternion.Euler(90,0,0),10);
                Round(root,"Cylinder End Cap",new Vector3(side*1.59f,.89f,8.47f),new Vector3(.66f,.1f,.66f),m[3],Quaternion.Euler(90,0,0),10);
                Box(root,"Single Boiler Handrail",new Vector3(side*1.14f,2.55f,6.3f),new Vector3(.085f,.085f,3.5f),m[6]);
                for(int bar=0;bar<3;bar++)Box(root,"Split Pilot Bar",new Vector3(side*(.68f+bar*.42f),.49f+bar*.06f,8.67f),new Vector3(.18f,.47f,.24f),m[7],Quaternion.Euler(22,0,side*-12));
                Box(root,"Heavy Front Apron",new Vector3(side*1.18f,.74f,8.43f),new Vector3(1.4f,.26f,.25f),m[7]);
            }
            var smoke=old.Find("Chimney Smoke");if(smoke!=null)smoke.localPosition=new Vector3(0,4.7f,7.48f);
        }
        private void Tender(Transform root)
        {
            foreach(float side in new[]{-1f,1f})
            {
                Box(root,"Thick Tender Guard",new Vector3(side*1.9f,1.37f,0),new Vector3(.16f,.46f,8.8f),m[5]);
                Box(root,"Tender Rolled Lip",new Vector3(side*1.9f,1.63f,0),new Vector3(.23f,.12f,8.8f),m[3]);
                foreach(float z in new[]{-3.6f,0,3.6f})Box(root,"Broad Tender Stay",new Vector3(side*1.995f,1.38f,z),new Vector3(.075f,.45f,.18f),m[3]);
            }
            foreach(float end in new[]{-4.4f,4.4f})foreach(float side in new[]{-1f,1f})Box(root,"Tender End Cheek",new Vector3(side*1.3f,1.37f,end),new Vector3(1.2f,.46f,.16f),m[5]);
        }
        private void Coach(Transform root)
        {
            var upholstery=new Material(m[7]){name="Frontier Warm Coach Leather"};upholstery.SetColor("_BaseColor",new Color(.44f,.22f,.16f));paints.Add(upholstery);
            foreach(float z in new[]{-3.05f,-1.7f,-.35f,3.05f})foreach(float side in new[]{-1f,1f})
            {
                float x=side*1.16f;
                Box(root,"Broad Upholstered Cushion",new Vector3(x,1.65f,z),new Vector3(1.24f,.24f,.76f),upholstery);
                FrontierModelMesh.Part(root,"Clipped Coach Seat Back",new Vector3(x,2.04f,z-.33f),new Vector3(1.24f,.86f,.18f),upholstery,FrontierModelMesh.ArchBack());
                foreach(float arm in new[]{-.57f,.57f}){Box(root,"Solid Timber Seat Arm",new Vector3(x+arm,1.92f,z),new Vector3(.13f,.12f,.8f),m[2]);Box(root,"Bench Pedestal",new Vector3(x+arm,1.35f,z),new Vector3(.13f,.36f,.5f),m[2]);}
            }
            Box(root,"Plain Aisle Runner",new Vector3(0,1.159f,0),new Vector3(.87f,.014f,8.5f),m[5]);
            foreach(float side in new[]{-1f,1f})Box(root,"Timber Luggage Rack",new Vector3(side*1.5f,3.43f,0),new Vector3(.64f,.09f,8.4f),m[2]);
            foreach(float z in new[]{-2.5f,0,2.5f})Round(root,"Large Coach Ceiling Lamp",new Vector3(0,3.7f,z),new Vector3(.33f,.16f,.33f),m[8],Quaternion.identity,8);
        }
        private void Wheel(TrainWheel wheel)
        {
            foreach(var draw in wheel.GetComponentsInChildren<Renderer>())draw.enabled=false;
            var root=CraftGeometry.Group(wheel.transform,"Rebuilt Six Spoke Wheel");float r=wheel.Radius,side=Mathf.Sign(wheel.transform.localPosition.x);
            FrontierModelMesh.Part(root,"Heavy Wheel Tyre",Vector3.zero,new Vector3(r*2.1f,r*2.1f,.22f),m[3],FrontierModelMesh.Ring(),Quaternion.Euler(0,90,0));
            FrontierModelMesh.Part(root,"Broad Painted Wheel Rim",new Vector3(side*.13f,0,0),new Vector3(r*1.88f,r*1.88f,.1f),r>.5f?m[7]:m[5],FrontierModelMesh.Ring(16,.36f),Quaternion.Euler(0,90,0));
            for(int spoke=0;spoke<6;spoke++){var a=Quaternion.AngleAxis(spoke*60,Vector3.right);Box(root,"Wide Wheel Spoke",a*Vector3.up*r*.43f,new Vector3(.14f,r*.94f,.14f),r>.5f?m[7]:m[5],a);}
            Round(root,"Large Axle Hub",Vector3.zero,new Vector3(r*.48f,.32f,r*.48f),m[3],Quaternion.Euler(0,0,90),12);
            CraftGeometry.Bake(root);
        }
        private void Door()
        {
            var door=transform.Find("Sliding Door/Door Visual Details");if(door==null)return;
            foreach(var draw in door.GetComponentsInChildren<Renderer>())draw.enabled=false;
            var root=CraftGeometry.Group(door,"Rebuilt Timber Door");
            Box(root,"Broad Door Lower Panel",new Vector3(0,-.47f,0),new Vector3(.17f,1.27f,2),m[2]);
            foreach(float z in new[]{-.92f,.92f})Box(root,"Cream Door Stile",new Vector3(0,.48f,z),new Vector3(.18f,1.1f,.17f),m[8]);
            Box(root,"Cream Door Header",new Vector3(0,1.03f,0),new Vector3(.18f,.15f,2),m[8]);
            foreach(float side in new[]{-1f,1f})Round(root,"Door Grip",new Vector3(side*.13f,0,.65f),new Vector3(.09f,.3f,.09f),m[3],Quaternion.identity,8);
            CraftGeometry.Bake(root);
        }
        private void FrontWindows()
        {
            var root=transform.Find("Cab Visual Details");if(root==null)return;
            foreach(var draw in root.GetComponentsInChildren<MeshRenderer>())if(draw.name.Contains("Front Window")||draw.name.Contains("Front Cab Corner")||draw.name=="Front Centre Mullion")draw.sharedMaterial=m[8];
        }
        private void Box(Transform p,string name,Vector3 at,Vector3 size,Material paint,Quaternion? rotation=null)=>FrontierModelMesh.Box(p,name,at,size,paint,rotation);
        private void Round(Transform p,string name,Vector3 at,Vector3 size,Material paint,Quaternion rotation,int sides,float low=.5f,float high=.5f)=>FrontierModelMesh.Part(p,name,at,size,paint,FrontierModelMesh.Cylinder(sides,low,high),rotation);
        private void OnDestroy(){foreach(var paint in paints)if(paint!=null)CraftGeometry.Dispose(paint);}
    }
}
