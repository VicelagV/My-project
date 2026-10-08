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
        private readonly Dictionary<Vector2Int,List<Vector3[]>> corridors=new Dictionary<Vector2Int,List<Vector3[]>>();
        private RouteNetwork graph;
        public int MeshCount=>owned.Count;
        public void Build(RouteNetwork network,Material earth,Material ballast,Material wood,Material rail,Material leaves)
        {
            graph=network;
            var samples=new List<Vector3>();
            foreach(var edge in graph.edges)
            {
                var temp=new GameObject("Curve Sampling");temp.transform.SetParent(transform,false);
                var path=temp.AddComponent<RailPath>();path.Configure(edge.points);
                Vector3 previous=edge.points[0];
                for(float d=0;d<path.Length;d+=2f)
                {
                    path.Sample(d,out Vector3 p,out Quaternion q);
                    Box(p+Vector3.up*0.0125f,new Vector3(4.65f,0.025f,0.28f),q,wood);
                    if(d%8f<0.01f)
                    {
                        path.Sample(Mathf.Min(d+8f,path.Length),out Vector3 next,out _);
                        Vector3 middle=(p+next)*0.5f;Quaternion along=Quaternion.LookRotation(next-p);
                        Box(middle-Vector3.up*0.025f,new Vector3(5.2f,0.05f,Vector3.Distance(p,next)+0.15f),along,ballast);
                        foreach(float side in new[]{-1.98f,1.98f}) Box(middle+along*new Vector3(side,0.045f,0f),new Vector3(0.14f,0.04f,Vector3.Distance(p,next)+0.08f),along,rail);
                    }
                    if(d%10f<0.01f) {AddCorridor(previous,p);previous=p;samples.Add(p);}
                }
                AddCorridor(previous,edge.points[edge.points.Length-1]);
                Dispose(temp);
            }
            // Redfort's departure track and terminal overrun are part of the drivable itinerary.
            AddStraight(graph.Node("Redfort").position-Vector3.forward*80f,graph.Node("Redfort").position,ballast,wood,rail);
            AddStraight(graph.Node("Terminum").position,graph.Node("Terminum").position+Vector3.forward*160f,ballast,wood,rail);
            AddStraight(graph.Node("Return").position,graph.Node("Return").position+Vector3.forward*160f,ballast,wood,rail);
            Terrain(earth);
            Trees(samples,wood,leaves);
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
            for(float d=0;d<Vector3.Distance(a,b);d+=2f)
            {
                Vector3 p=a+Vector3.forward*d;
                Box(p+Vector3.forward,new Vector3(5.2f,0.04f,2.1f),Quaternion.identity,ballast);
                Box(p+Vector3.up*0.0125f,new Vector3(4.65f,0.025f,0.28f),Quaternion.identity,wood);
                foreach(float x in new[]{-1.98f,1.98f})Box(p+new Vector3(x,0.045f,1f),new Vector3(0.14f,0.04f,2.04f),Quaternion.identity,rail);
            }
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
            foreach(var node in graph.nodes)if(IsStop(node.place)&&Mathf.Abs(p.x-node.position.x)<35f&&Mathf.Abs(p.z-node.position.z)<65f)return 0f;
            float noise=Mathf.PerlinNoise(p.x*0.006f+graph.seed%997,p.z*0.006f+graph.seed%991);
            return Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(48f,120f,clearance))*(3f+noise*25f);
        }
        public static bool IsStop(RoutePlace place)=>place==RoutePlace.Redfort||place==RoutePlace.SupplyStop||place==RoutePlace.Station||place==RoutePlace.Town||place==RoutePlace.Bluefort||place==RoutePlace.Terminum;
        private void Terrain(Material material)
        {
            Vector3 min=graph.nodes[0].position,max=min;
            foreach(var edge in graph.edges)foreach(var p in edge.points){min=Vector3.Min(min,p);max=Vector3.Max(max,p);}
            min-=new Vector3(180f,0f,180f);max+=new Vector3(180f,0f,340f);
            float x0=Mathf.Floor(min.x/32f)*32f,z0=Mathf.Floor(min.z/32f)*32f;
            for(float x=x0;x<max.x;x+=32f)for(float z=z0;z<max.z;z+=32f)
            {
                Vector3 a=new Vector3(x,0,z),b=new Vector3(x,0,z+32f),c=new Vector3(x+32f,0,z+32f),d=new Vector3(x+32f,0,z);
                a.y=GroundHeight(a);b.y=GroundHeight(b);c.y=GroundHeight(c);d.y=GroundHeight(d);
                var batch=GetBatch((a+c)*0.5f,material,"Terrain");Triangle(batch,a,b,c);Triangle(batch,a,c,d);
            }
        }
        private void Trees(List<Vector3> samples,Material trunk,Material leaves)
        {
            var random=new System.Random(unchecked(graph.seed*7919+17));
            for(int i=0;i<samples.Count;i+=6)
            {
                Vector3 p=samples[i]+new Vector3((float)(random.NextDouble()-0.5)*180f,0f,(float)(random.NextDouble()-0.5)*40f);
                if(DistanceToTrack(p)<17f)continue;
                bool nearStop=false;foreach(var node in graph.nodes)if(IsStop(node.place)&&Vector3.Distance(p,node.position)<48f){nearStop=true;break;}if(nearStop)continue;
                p.y=GroundHeight(p);float height=4f+(float)random.NextDouble()*4f;
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
            for(int i=0;i<faces.Length;i+=4){Triangle(batch,v[faces[i]],v[faces[i+1]],v[faces[i+2]]);Triangle(batch,v[faces[i]],v[faces[i+2]],v[faces[i+3]]);}
        }
        private static void Triangle(Batch batch,Vector3 a,Vector3 b,Vector3 c)
        {
            int first=batch.vertices.Count;batch.vertices.Add(a);batch.vertices.Add(b);batch.vertices.Add(c);Vector3 normal=Vector3.Cross(b-a,c-a).normalized;
            for(int i=0;i<3;i++){batch.triangles.Add(first+i);batch.normals.Add(normal);}
        }
        private static void Dispose(UnityEngine.Object obj){if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
        private void OnDestroy(){foreach(var collider in GetComponentsInChildren<MeshCollider>())collider.sharedMesh=null;foreach(var filter in GetComponentsInChildren<MeshFilter>())filter.sharedMesh=null;foreach(var mesh in owned)if(mesh!=null)Dispose(mesh);}
    }
}
