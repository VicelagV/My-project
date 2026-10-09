using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AllAboard.Train
{
    // Mesh batches are spatially split for culling; no individual rail/sleeper GameObjects.
    public sealed class RouteGeometry : MonoBehaviour
    {
        private sealed class Batch
        {
            public Material material;
            public readonly List<Vector3> vertices=new List<Vector3>();
            public readonly List<int> triangles=new List<int>();
            public readonly List<Vector3> normals=new List<Vector3>();
        }
        private readonly Dictionary<string,Batch> batches=new Dictionary<string,Batch>();
        private readonly List<Mesh> owned=new List<Mesh>();
        private readonly List<Material> paints=new List<Material>();
        private readonly Dictionary<Vector2Int,List<Vector3[]>> corridors=new Dictionary<Vector2Int,List<Vector3[]>>();
        private readonly HashSet<Vector3Int> sleeperPositions=new HashSet<Vector3Int>();
        private readonly HashSet<string> trackSegments=new HashSet<string>();
        private RouteNetwork graph;
        public IReadOnlyList<LandmarkPlan> Landmarks {get;private set;}
        public int MeshCount=>owned.Count;
        public void Build(RouteNetwork network,Material earth,Material ballast,Material wood,Material rail,Material leaves)
        {
            graph=network;Landmarks=LandmarkArt.Plan(graph,transform);
            var samples=new List<Vector3>();
            foreach(var edge in graph.edges)
            {
                var temp=new GameObject("Curve Sampling");temp.transform.SetParent(transform,false);
                var path=temp.AddComponent<RailPath>();path.Configure(edge.points);
                Track(path,ballast,wood,rail,edge);
                Vector3 previous=edge.points[0];
                for(float d=10f;d<path.Length;d+=10f)
                {path.Sample(d,out Vector3 p,out _);AddCorridor(previous,p);previous=p;samples.Add(p);}
                AddCorridor(previous,edge.points[edge.points.Length-1]);
                Dispose(temp);
            }
            // Redfort's departure track and terminal overrun are part of the drivable itinerary.
            AddStraight(graph.Node("Redfort").position-Vector3.forward*80f,graph.Node("Redfort").position,ballast,wood,rail);
            AddStraight(graph.Node("Terminum").position,graph.Node("Terminum").position+Vector3.forward*160f,ballast,wood,rail);
            AddStraight(graph.Node("Return").position,graph.Node("Return").position+Vector3.forward*160f,ballast,wood,rail);
            Terrain(earth);
            Trees(samples,wood,leaves);
            EmitMeshes();
        }
        public void BuildTrack(RailPath path,Material ballast,Material wood,Material rail)
        {Track(path,ballast,wood,rail);EmitMeshes();}
        private void EmitMeshes()
        {
            foreach(var pair in batches)
            {
                var data=pair.Value;if(data.vertices.Count==0)continue;
                Mesh mesh=new Mesh{name="Generated "+pair.Key,indexFormat=data.vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
                mesh.SetVertices(data.vertices);mesh.SetNormals(data.normals);mesh.SetTriangles(data.triangles,0);mesh.RecalculateBounds();owned.Add(mesh);
                var obj=new GameObject(pair.Key);obj.transform.SetParent(transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=data.material;
                if(pair.Key.StartsWith("Terrain"))obj.AddComponent<MeshCollider>().sharedMesh=mesh;
            }
            batches.Clear();
        }
        private void AddStraight(Vector3 a,Vector3 b,Material ballast,Material wood,Material rail)
        {
            AddCorridor(a,b);
            var temp=new GameObject("Straight Rail Sampling");temp.transform.SetParent(transform,false);
            var path=temp.AddComponent<RailPath>();path.Configure(new[]{a,b});Track(path,ballast,wood,rail);Dispose(temp);
        }
        private static Vector3Int TrackKey(Vector3 p)=>new Vector3Int(Mathf.RoundToInt(p.x*100f),Mathf.RoundToInt(p.y*100f),Mathf.RoundToInt(p.z*100f));
        private void Track(RailPath path,Material ballast,Material wood,Material rail,RouteEdge edge=null)
        {
            for(float d=0f;d<path.Length;d+=0.8f)
            {
                path.Sample(d,out Vector3 p,out Quaternion q);if(!sleeperPositions.Add(TrackKey(p)))continue;
                Box(p+Vector3.up*0.002f,new Vector3(4.65f,0.045f,0.25f),q,wood);
                foreach(float side in new[]{-1.98f,1.98f})
                    Box(p+q*new Vector3(side,0.034f,0f),new Vector3(0.29f,0.017f,0.32f),q,rail);
            }
            for(float d=0f;d<path.Length;d+=2f)
            {
                path.Sample(d,out Vector3 a,out Quaternion qa);path.Sample(Mathf.Min(d+2f,path.Length),out Vector3 b,out Quaternion qb);
                string key=TrackKey(a)+"/"+TrackKey(b);if(!trackSegments.Add(key))continue;
                // A sloping gravel shoulder and three-part rail section replace flat strips.
                Strip(a,b,qa,qb,0f,5.0f,0.035f,-0.0115f,ballast,6.0f,d==0f,d+2f>=path.Length);
                if(edge!=null&&RailDamagePlan.HasDamage(edge)&&d>=RailDamagePlan.Start(edge,path.Length)&&d<RailDamagePlan.Start(edge,path.Length)+RailDamagePlan.Length)continue;
                bool damageBoundary=edge!=null&&RailDamagePlan.HasDamage(edge);
                float damagedAt=damageBoundary?RailDamagePlan.Start(edge,path.Length):-100f;
                bool beginCap=d==0f||(damageBoundary&&Mathf.Abs(d-damagedAt-RailDamagePlan.Length)<.001f);
                bool endCap=d+2f>=path.Length||(damageBoundary&&Mathf.Abs(d+2f-damagedAt)<.001f);
                foreach(float side in new[]{-1.98f,1.98f})
                {
                    Strip(a,b,qa,qb,side,0.22f,0.018f,0.035f,rail,capStart:beginCap,capEnd:endCap);
                    Strip(a,b,qa,qb,side,0.055f,0.030f,0.055f,rail,capStart:beginCap,capEnd:endCap);
                    Strip(a,b,qa,qb,side,0.14f,0.022f,0.080f,rail,capStart:beginCap,capEnd:endCap);
                }
            }
        }
        private void Strip(Vector3 a,Vector3 b,Quaternion qa,Quaternion qb,float offset,float width,float height,float y,Material material,float baseWidth=0f,bool capStart=false,bool capEnd=false)
        {
            var batch=GetBatch((a+b)*0.5f,material,"Railway");var v=new Vector3[8];
            for(int i=0;i<8;i++)
            {
                Vector3 cross=new Vector3(offset+((i&1)==0?-1f:1f)*(((i&2)==0&&baseWidth>0f)?baseWidth:width)*0.5f,y+((i&2)==0?-height:height)*0.5f,0f);
                v[i]=((i&4)==0?a:b)+((i&4)==0?qa:qb)*cross;
            }
            int[] faces={0,4,6,2,1,3,7,5,0,1,5,4,2,6,7,3,0,2,3,1,4,5,7,6};
            for(int i=0;i<16;i+=4)Quad(batch,v[faces[i]],v[faces[i+1]],v[faces[i+2]],v[faces[i+3]]);
            if(capStart)Quad(batch,v[0],v[2],v[3],v[1]);if(capEnd)Quad(batch,v[4],v[5],v[7],v[6]);
        }
        private void AddCorridor(Vector3 a,Vector3 b)
        {
            int x0=Mathf.FloorToInt(Mathf.Min(a.x,b.x)/64f)-1,x1=Mathf.FloorToInt(Mathf.Max(a.x,b.x)/64f)+1;
            int z0=Mathf.FloorToInt(Mathf.Min(a.z,b.z)/64f)-1,z1=Mathf.FloorToInt(Mathf.Max(a.z,b.z)/64f)+1;
            for(int x=x0;x<=x1;x++)for(int z=z0;z<=z1;z++)
            {var key=new Vector2Int(x,z);if(!corridors.TryGetValue(key,out var list)){list=new List<Vector3[]>();corridors.Add(key,list);}list.Add(new[]{a,b});}
        }
        public float DistanceToTrack(Vector3 p)
        {
            float nearest=128f;var key=new Vector2Int(Mathf.FloorToInt(p.x/64f),Mathf.FloorToInt(p.z/64f));
            if(corridors.TryGetValue(key,out var list))foreach(var line in list)
            {Vector3 ab=line[1]-line[0];float t=ab.sqrMagnitude>0f?Mathf.Clamp01(Vector3.Dot(p-line[0],ab)/ab.sqrMagnitude):0f;nearest=Mathf.Min(nearest,Vector3.Distance(p,line[0]+ab*t));}
            return nearest;
        }
        public float GroundHeight(Vector3 p)
        {
            float clearance=DistanceToTrack(new Vector3(p.x,0f,p.z));
            float pad=1f;
            foreach(var node in graph.nodes)if(IsStop(node.place))pad=Mathf.Min(pad,Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(140f,230f,Vector3.Distance(p,node.position))));
            float capitalDistance=Vector3.Distance(p,graph.Node("Terminum").position);
            pad=Mathf.Min(pad,Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(600f,750f,capitalDistance)));
            if(Landmarks!=null)foreach(var landmark in Landmarks)
                if(landmark.kind==LandmarkKind.RiverBridge||landmark.kind==LandmarkKind.Canyon)
                {
                    Vector3 local=Quaternion.Inverse(landmark.rotation)*(p-landmark.position);
                    if(Mathf.Abs(local.z)<30f&&Mathf.Abs(local.x)<95f)return -(landmark.kind==LandmarkKind.Canyon?8f:3f)*Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(30f,20f,Mathf.Abs(local.z)))*Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(95f,80f,Mathf.Abs(local.x)));
                }
            return pad*Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(48f,120f,clearance))*WorldRegions.Height(graph,p);
        }
        public static bool IsStop(RoutePlace place)=>place==RoutePlace.Redfort||place==RoutePlace.SupplyStop||place==RoutePlace.Station||place==RoutePlace.Town||place==RoutePlace.Village||place==RoutePlace.Bluefort||place==RoutePlace.Terminum;
        private void Terrain(Material material)
        {
            var regions=new Dictionary<LandscapeRegion,Material>();
            foreach(LandscapeRegion region in Enum.GetValues(typeof(LandscapeRegion))){var paint=new Material(material){name="Ground "+region};paint.SetColor("_BaseColor",WorldRegions.GroundColour(region));regions.Add(region,paint);paints.Add(paint);}
            Vector3 min=graph.nodes[0].position,max=min;
            foreach(var edge in graph.edges)foreach(var p in edge.points){min=Vector3.Min(min,p);max=Vector3.Max(max,p);}
            min-=new Vector3(180f,0f,180f);max+=new Vector3(180f,0f,340f);
            float x0=Mathf.Floor(min.x/32f)*32f,z0=Mathf.Floor(min.z/32f)*32f;
            for(float x=x0;x<max.x;x+=32f)for(float z=z0;z<max.z;z+=32f)
            {
                Vector3 a=new Vector3(x,0,z),b=new Vector3(x,0,z+32f),c=new Vector3(x+32f,0,z+32f),d=new Vector3(x+32f,0,z);
                a.y=GroundHeight(a);b.y=GroundHeight(b);c.y=GroundHeight(c);d.y=GroundHeight(d);
                var batch=GetBatch((a+c)*0.5f,regions[WorldRegions.At(graph,(a+c)*0.5f)],"Terrain");Triangle(batch,a,b,c);Triangle(batch,a,c,d);
            }
        }
        private void Trees(List<Vector3> samples,Material trunk,Material leaves)
        {
            var random=new System.Random(unchecked(graph.seed*7919+17));
            for(int i=0;i<samples.Count;i+=2)
            {
                Vector3 p=samples[i]+new Vector3((float)(random.NextDouble()-0.5)*180f,0f,(float)(random.NextDouble()-0.5)*40f);
                if(Vector3.Distance(p,graph.Node("Terminum").position)<650f)continue;
                if(DistanceToTrack(p)<17f)continue;
                bool nearStop=false;foreach(var node in graph.nodes)if(IsStop(node.place)&&Vector3.Distance(p,node.position)<145f){nearStop=true;break;}if(nearStop)continue;
                bool nearLandmark=false;foreach(var landmark in Landmarks)if(Vector3.Distance(p,landmark.position)<55f){nearLandmark=true;break;}if(nearLandmark)continue;
                p.y=GroundHeight(p);float height=4f+(float)random.NextDouble()*4f;
                var region=WorldRegions.At(graph,p);
                if(region==LandscapeRegion.Desert)
                {if(random.NextDouble()<0.45){Box(p+Vector3.up*1.4f,new Vector3(.4f,2.8f,.4f),Quaternion.identity,leaves);Box(p+new Vector3(.55f,1.7f,0f),new Vector3(1.1f,.3f,.3f),Quaternion.identity,leaves);Box(p+new Vector3(.9f,2f,0f),new Vector3(.3f,.9f,.3f),Quaternion.identity,leaves);}continue;}
                if(region!=LandscapeRegion.Forest&&random.NextDouble()<0.72)continue;
                Box(p+Vector3.up*height*0.25f,new Vector3(0.45f,height*0.5f,0.45f),Quaternion.identity,trunk);
                Cone(p+Vector3.up*height*0.23f,height*0.65f,height*0.29f,leaves);
                Cone(p+Vector3.up*height*0.50f,height*0.50f,height*0.22f,leaves);
                var collider=new GameObject("Tree Trunk");collider.transform.SetParent(transform,false);collider.transform.localPosition=p+Vector3.up*height*0.25f;
                var capsule=collider.AddComponent<CapsuleCollider>();capsule.height=height*0.5f;capsule.radius=0.25f;
            }
        }
        private void Cone(Vector3 p,float height,float radius,Material material)
        {
            Batch batch=GetBatch(p,material,"Trees");
            for(int i=0;i<8;i++)
            {float a=i*Mathf.PI/4f,b=(i+1)*Mathf.PI/4f;Vector3 first=p+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius),second=p+new Vector3(Mathf.Cos(b)*radius,0,Mathf.Sin(b)*radius);Triangle(batch,first,p+Vector3.up*height,second);Triangle(batch,p,first,second);}
        }
        private Batch GetBatch(Vector3 p,Material material,string kind)
        {
            string key=kind+" "+Mathf.FloorToInt(p.x/512f)+" "+Mathf.FloorToInt(p.z/512f)+" "+material.name;
            if(!batches.TryGetValue(key,out var batch)){batch=new Batch{material=material};batches.Add(key,batch);}return batch;
        }
        private void Box(Vector3 center,Vector3 size,Quaternion rotation,Material material)
        {
            var batch=GetBatch(center,material,"Track and Trees");Matrix4x4 matrix=Matrix4x4.TRS(center,rotation,size);
            var v=new Vector3[8];for(int i=0;i<8;i++)v[i]=matrix.MultiplyPoint3x4(new Vector3((i&1)==0?-0.5f:0.5f,(i&2)==0?-0.5f:0.5f,(i&4)==0?-0.5f:0.5f));
            int[] faces={0,4,6,2,1,3,7,5,0,1,5,4,2,6,7,3,0,2,3,1,4,5,7,6};
            for(int i=0;i<faces.Length;i+=4)Quad(batch,v[faces[i]],v[faces[i+1]],v[faces[i+2]],v[faces[i+3]]);
        }
        private static void Quad(Batch batch,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
        {
            int first=batch.vertices.Count;batch.vertices.Add(a);batch.vertices.Add(b);batch.vertices.Add(c);batch.vertices.Add(d);
            Vector3 normal=Vector3.Cross(b-a,c-a).normalized;for(int i=0;i<4;i++)batch.normals.Add(normal);
            batch.triangles.Add(first);batch.triangles.Add(first+1);batch.triangles.Add(first+2);
            batch.triangles.Add(first);batch.triangles.Add(first+2);batch.triangles.Add(first+3);
        }
        private static void Triangle(Batch batch,Vector3 a,Vector3 b,Vector3 c)
        {
            int first=batch.vertices.Count;batch.vertices.Add(a);batch.vertices.Add(b);batch.vertices.Add(c);Vector3 normal=Vector3.Cross(b-a,c-a).normalized;
            for(int i=0;i<3;i++){batch.triangles.Add(first+i);batch.normals.Add(normal);}
        }
        private static void Dispose(UnityEngine.Object obj){if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
        private void OnDestroy(){foreach(var collider in GetComponentsInChildren<MeshCollider>())collider.sharedMesh=null;foreach(var filter in GetComponentsInChildren<MeshFilter>())filter.sharedMesh=null;foreach(var mesh in owned)if(mesh!=null)Dispose(mesh);foreach(var paint in paints)if(paint!=null)Dispose(paint);}
    }
}
