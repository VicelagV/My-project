using System.Collections.Generic;
using UnityEngine;
namespace AllAboard.Train
{
    public enum LandmarkKind { RiverBridge, Farm, Canyon, Tunnel, Mine, LoggingCamp, Ruins, Checkpoint, HostileCamp }
    public sealed class LandmarkPlan
    {public LandmarkKind kind;public Vector3 position;public Quaternion rotation;public string edgeId;}
    public static class LandmarkArt
    {
        public static List<LandmarkPlan> Plan(RouteNetwork graph,Transform owner)
        {
            var result=new List<LandmarkPlan>();int i=0;
            foreach(var edge in graph.edges)
            {
                if(edge.crossover||edge.id=="Opening"||edge.id=="ForestExit")continue;
                var temp=new GameObject("Landmark Survey");temp.transform.SetParent(owner,false);var path=temp.AddComponent<RailPath>();path.Configure(edge.points);path.Sample(path.Length*.37f,out var p,out var q);
                var kind=(LandmarkKind)(i++%9);
                if(kind==LandmarkKind.Tunnel)
                {
                    bool found=false;
                    for(float distance=160f;distance<path.Length-160f;distance+=32f)
                    {
                        path.Sample(distance,out var candidate,out var facing);
                        if(WorldRegions.At(graph,candidate)!=LandscapeRegion.Mountains||WorldRegions.Height(graph,candidate)<120f)continue;
                        bool station=false;foreach(var node in graph.nodes)if(RouteGeometry.IsStop(node.place)&&Vector3.Distance(candidate,node.position)<260f){station=true;break;}if(station)continue;
                        path.Sample(distance-60f,out var before,out _);path.Sample(distance+60f,out var after,out _);
                        if(Mathf.Abs((Quaternion.Inverse(facing)*(before-candidate)).x)>.65f||Mathf.Abs((Quaternion.Inverse(facing)*(after-candidate)).x)>.65f)continue;
                        p=candidate;q=facing;found=true;break;
                    }
                    if(!found)kind=LandmarkKind.Mine;
                }
                result.Add(new LandmarkPlan{kind=kind,position=p,rotation=q,edgeId=edge.id});
                if(Application.isPlaying)Object.Destroy(temp);else Object.DestroyImmediate(temp);
            }
            return result;
        }
        private static GameObject Box(Transform p,string n,Vector3 at,Vector3 size,Material m,bool solid=false)=>RouteProps.Box(p,n,at,size,m,solid);
        public static void Build(Transform parent,LandmarkPlan plan,RouteGeometry geometry,Material[] m)
        {
            var root=new GameObject(plan.kind+" - "+plan.edgeId).transform;root.SetParent(parent,false);root.position=plan.position;root.rotation=plan.rotation;
            if(plan.kind==LandmarkKind.RiverBridge||plan.kind==LandmarkKind.Canyon)
            {
                Box(root,"Bridge Deck",new Vector3(0f,-.12f,0f),new Vector3(6.5f,.24f,60f),m[2],true);
                foreach(float side in new[]{-1f,1f})
                {
                    Box(root,"Bridge Guard Rail",new Vector3(side*3.25f,.65f,0f),new Vector3(.13f,.13f,60f),m[3]);
                    Box(root,"Bridge Main Girder",new Vector3(side*2.85f,-.75f,0f),new Vector3(.35f,1.3f,60f),m[3]);
                    for(float z=-27f;z<30f;z+=6f)
                    {Box(root,"Bridge Railing Post",new Vector3(side*3.25f,.35f,z),new Vector3(.12f,.7f,.12f),m[3]);Box(root,"Bridge Stone Pier",new Vector3(side*2.75f,-3f,z),new Vector3(1.4f,6f,1.8f),m[1]);}
                }
                if(plan.kind==LandmarkKind.RiverBridge)
                {
                    var assets=parent.GetComponent<WorldArtAssets>();if(assets==null)assets=parent.gameObject.AddComponent<WorldArtAssets>();var water=assets.Paint(m[3],"River Water",new Color(.21f,.44f,.48f));
                    Box(root,"River",new Vector3(0f,-1.45f,0f),new Vector3(180f,.1f,42f),water);
                    for(int i=0;i<12;i++)Box(root,"Water Ripple",new Vector3(-80f+i*14f,-1.38f,(i%3-1)*10f),new Vector3(5f,.01f,.14f),m[8]);
                }
                else foreach(float side in new[]{-1f,1f})Box(root,"Canyon Cliff",new Vector3(side*90f,-1f,0f),new Vector3(12f,12f,85f),m[1]);
                return;
            }
            if(plan.kind==LandmarkKind.Tunnel)
            {
                TunnelMountain.Build(root,WorldRegions.Height(root.GetComponentInParent<ProceduralRailway>().Network,plan.position),m);
                foreach(float side in new[]{-1f,1f})Box(root,"Tunnel Wall",new Vector3(side*4.2f,3f,0f),new Vector3(.7f,6f,120f),m[1],true);
                Box(root,"Tunnel Vault",new Vector3(0f,6.4f,0f),new Vector3(9.5f,.8f,120f),m[1],true);
                foreach(float z in new[]{-60f,60f})
                {
                    foreach(float side in new[]{-1f,1f})Box(root,"Tunnel Portal Pier",new Vector3(side*4.5f,3.4f,z),new Vector3(1f,6.8f,1.3f),m[2]);
                    Box(root,"Tunnel Portal Crown",new Vector3(0f,6.65f,z),new Vector3(10f,.75f,1.3f),m[2]);
                    var facing=new GameObject("Tunnel Portal Dressing").transform;facing.SetParent(root,false);facing.localPosition=new Vector3(0f,0f,z);facing.localRotation=Quaternion.Euler(0f,z>0f?180f:0f,0f);
                    PortalArch.Build(facing,0f,m[8]);
                    for(int i=1;i<12;i++){float angle=i*15f;var joint=Box(facing,"Arch Mortar Joint",new Vector3(Mathf.Cos(angle*Mathf.Deg2Rad)*4.425f,2f+Mathf.Sin(angle*Mathf.Deg2Rad)*4.425f,-.845f),new Vector3(.025f,.73f,.025f),m[2]);joint.transform.localRotation=Quaternion.Euler(0f,0f,angle-90f);}
                }
                return;
            }
            var site=new GameObject("Accessible Landmark").transform;site.SetParent(root,false);site.localPosition=new Vector3(plan.kind==LandmarkKind.Checkpoint?10f:28f,0f,0f);
            site.position=new Vector3(site.position.x,geometry.GroundHeight(site.position),site.position.z);
            if(plan.kind==LandmarkKind.Farm)
            {
                Shelter(site,"Barn",m,8f,6f);
                for(int i=0;i<6;i++)Box(site,"Crop Row",new Vector3(15f+i*2f,.12f,0f),new Vector3(.5f,.24f,22f),m[4]);
                Fence(site,new Vector3(14f,0f,-14f),22f,m);
            }
            if(plan.kind==LandmarkKind.Mine)
            {
                foreach(float z in new[]{-1.8f,1.8f})
                {
                    foreach(float x in new[]{-3f,3f})Box(site,"Mine Headframe Leg",new Vector3(x,4f,z),new Vector3(.55f,8f,.55f),m[2],true);
                    Box(site,"Mine Headframe Beam",new Vector3(0f,8f,z),new Vector3(7.5f,.6f,.6f),m[2]);
                    foreach(float side in new[]{-1f,1f}){var brace=Box(site,"Headframe Diagonal Brace",new Vector3(side*1.5f,5.7f,z),new Vector3(.22f,5.4f,.22f),m[2]);brace.transform.localRotation=Quaternion.Euler(0f,0f,side*36f);}
                }
                Box(site,"Headframe Cross Tie",new Vector3(0f,8f,0f),new Vector3(.6f,.6f,4.2f),m[2]);
                var pulley=GameObject.CreatePrimitive(PrimitiveType.Cylinder);pulley.name="Mine Winding Drum";pulley.transform.SetParent(site,false);pulley.transform.localPosition=new Vector3(0f,8.6f,0f);pulley.transform.localRotation=Quaternion.Euler(0f,0f,90f);pulley.transform.localScale=new Vector3(1.3f,.7f,1.3f);pulley.GetComponent<Renderer>().sharedMaterial=m[3];pulley.GetComponent<Collider>().enabled=false;if(Application.isPlaying)Object.Destroy(pulley.GetComponent<Collider>());else Object.DestroyImmediate(pulley.GetComponent<Collider>());
                Box(site,"Mine Hoist Cable",new Vector3(0f,4.1f,.62f),new Vector3(.04f,7.4f,.04f),m[5]);
                Box(site,"Mine Entrance Shadow",new Vector3(0f,1.9f,3.5f),new Vector3(5.5f,3.8f,.15f),m[5]);
                Shelter(site,"Mine Supply Shed",m,5f,4f,new Vector3(9f,0f,1f));
                for(int i=0;i<4;i++)Box(site,"Ore Pile",new Vector3(-6f+i*1.1f,.4f,3f),new Vector3(1f,.8f,1f),m[3]);
            }
            if(plan.kind==LandmarkKind.LoggingCamp)
            {
                Shelter(site,"Logging Supply Shed",m,6f,4f);
                for(int i=0;i<8;i++)Box(site,"Cut Timber",new Vector3(11f,.4f+(i/3)*.6f,(i%3)*.7f),new Vector3(9f,.55f,.55f),m[2]);
                Box(site,"Log Crane Mast",new Vector3(16f,3f,5f),new Vector3(.3f,6f,.3f),m[3]);Box(site,"Crane Jib",new Vector3(13f,6f,5f),new Vector3(6f,.25f,.25f),m[3]);
                Box(site,"Crane Cable",new Vector3(10.2f,4.4f,5f),new Vector3(.035f,3.1f,.035f),m[5]);
                Box(site,"Crane Hook Shank",new Vector3(10.2f,2.78f,5f),new Vector3(.10f,.23f,.10f),m[3]);Box(site,"Crane Hook Tip",new Vector3(10.28f,2.66f,5f),new Vector3(.23f,.1f,.1f),m[3]);
                var jibBrace=Box(site,"Crane Jib Brace",new Vector3(14.7f,5.2f,5f),new Vector3(2.7f,.12f,.12f),m[3]);jibBrace.transform.localRotation=Quaternion.Euler(0f,0f,-31f);
            }
            if(plan.kind==LandmarkKind.Ruins)
            {
                for(int i=0;i<5;i++)Box(site,"Ruined Storehouse Wall",new Vector3(-4f+i*2f,1.1f+(i%2)*.7f,4f),new Vector3(1.9f,2.2f+(i%2)*1.4f,.4f),m[1],true);
                Box(site,"Ruined Side Wall",new Vector3(-5f,1.5f,0f),new Vector3(.4f,3f,8f),m[1],true);
                for(int i=0;i<5;i++){var timber=Box(site,"Collapsed Beam",new Vector3(-3f+i, .18f, i%2),new Vector3(.2f,.25f,6f),m[2]);timber.transform.localRotation=Quaternion.Euler(0f,25f+i*13f,0f);}
            }
            if(plan.kind==LandmarkKind.Checkpoint)
            {
                Shelter(site,"Border Office",m,5f,4f);
                RouteProps.Board(site,"BORDER RAILWAY\nCHECKPOINT",new Vector3(0f,3.2f,-3.2f),new Vector3(4f,.8f,.1f),m,Quaternion.identity);
                Fence(site,new Vector3(-2f,0f,-8f),7f,m);
            }
            if(plan.kind==LandmarkKind.HostileCamp)
            {
                Shelter(site,"Bandit Camp Shelter",m,5f,3.5f);
                foreach(float x in new[]{-6f,6f})Box(site,"Camp Barricade",new Vector3(x,.7f,-4f),new Vector3(4f,1.4f,.6f),m[2],true);
                for(int i=0;i<8;i++){float angle=i*Mathf.PI/4f;Box(site,"Camp Fire Stone",new Vector3(Mathf.Cos(angle)*.8f,.12f,-8f+Mathf.Sin(angle)*.8f),new Vector3(.35f,.24f,.35f),m[1]);}
                foreach(float side in new[]{-1f,1f}){var log=Box(site,"Firewood",new Vector3(0f,.13f,-8f+side*.16f),new Vector3(1f,.18f,.18f),m[2]);log.transform.localRotation=Quaternion.Euler(0f,side*28f,0f);}
            }
            if(plan.kind!=LandmarkKind.Checkpoint)
                for(int i=0;i<2;i++)Loot(site,new Vector3(-1f+i*2f,.5f,1f),40+i*20,m);
        }
        private static void Shelter(Transform parent,string name,Material[] m,float width,float height,Vector3 offset=default)
        {
            var shed=new GameObject(name).transform;shed.SetParent(parent,false);shed.localPosition=offset;
            Box(shed,"Shed Floor",new Vector3(0f,.08f,0f),new Vector3(width,.16f,6f),m[2],true);
            foreach(float side in new[]{-1f,1f})Box(shed,"Shed Side",new Vector3(side*width*.5f,height*.5f,0f),new Vector3(.18f,height,6f),m[2],true);
            Box(shed,"Shed Back",new Vector3(0f,height*.5f,3f),new Vector3(width,height,.18f),m[2],true);
            BuildingRoof.Build(shed,"Shed Roof",Vector3.zero,width+.18f,6.18f,height,18f,m[7],m[2],.3f);
            float opening=Mathf.Min(2.6f,width-1f),sideWidth=(width-opening)*.5f;
            foreach(float side in new[]{-1f,1f})
            {
                Box(shed,"Shed Front Pier",new Vector3(side*(opening+sideWidth)*.5f,height*.5f,-3f),new Vector3(sideWidth,height,.18f),m[2],true);
                Box(shed,"Entrance Frame",new Vector3(side*opening*.5f,1.35f,-3.14f),new Vector3(.14f,2.7f,.16f),m[8]);
                Box(shed,"Shed Corner Trim",new Vector3(side*width*.5f,height*.5f,-3.12f),new Vector3(.12f,height,.12f),m[8]);
            }
            Box(shed,"Shed Door Lintel",new Vector3(0f,(height+2.6f)*.5f,-3f),new Vector3(opening,height-2.6f,.18f),m[2],true);
            Box(shed,"Entrance Header",new Vector3(0f,2.65f,-3.14f),new Vector3(opening+.2f,.16f,.16f),m[8]);
            Box(shed,"Shed Step",new Vector3(0f,.06f,-3.25f),new Vector3(opening,.12f,.6f),m[1],true);
            for(float x=-width*.5f+.4f;x<width*.5f;x+=.55f)Box(shed,"Shed Cladding Seam",new Vector3(x,height*.5f,3.11f),new Vector3(.025f,height,.025f),m[8]);
        }
        private static void Fence(Transform p,Vector3 at,float length,Material[] m)
        {for(float x=0f;x<length;x+=2f)Box(p,"Fence Post",at+new Vector3(x,.65f,0f),new Vector3(.12f,1.3f,.12f),m[2]);foreach(float y in new[]{.5f,1f})Box(p,"Fence Rail",at+new Vector3(length*.5f,y,0f),new Vector3(length,.1f,.1f),m[2]);}
        private static void Loot(Transform p,Vector3 at,int value,Material[] m)
        {
            var item=new GameObject("Salvage Crate");item.transform.SetParent(p,false);item.transform.localPosition=at;
            item.AddComponent<Rigidbody>().mass=3f;item.AddComponent<BoxCollider>().size=Vector3.one*.65f;
            var pickup=item.AddComponent<AllAboard.Movement.PickupObject>();item.AddComponent<LootCargo>().Value=value;
            Box(item.transform,"Cargo Wood",Vector3.zero,Vector3.one*.65f,m[2]);
            foreach(float x in new[]{-.24f,.24f})Box(item.transform,"Cargo Binding",new Vector3(x,0f,0f),new Vector3(.055f,.67f,.67f),m[3]);
            var outline=Box(item.transform,"Cargo Outline",Vector3.zero,Vector3.one*.65f,m[9]).GetComponent<Renderer>();pickup.Configure(outline);outline.enabled=false;
        }
    }
}
