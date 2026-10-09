using UnityEngine;
namespace AllAboard.Train
{
    public static class VillageRoads
    {
        // A continuous, closed ribbon avoids cracks and coplanar overlapping road boxes.
        public static Vector3[] MainStreet=>new[]{new Vector3(43,0,-57),new Vector3(46,0,-34),new Vector3(43,0,-12),new Vector3(47,0,12),new Vector3(44,0,34),new Vector3(39,0,53)};
        public static void Build(Transform parent,Material dirt,Material[] m,SettlementKind kind)
        {
            Ribbon(parent,"Village Main Street",MainStreet,7f,dirt,.025f);
            Ribbon(parent,"Station Village Road",new[]{new Vector3(10.2f,0,-29),new Vector3(15,0,-20),new Vector3(29,0,-14),new Vector3(43,0,-12)},4.4f,dirt,.03f);
            Ribbon(parent,"Marketplace Lane",new[]{new Vector3(44,0,34),new Vector3(35,0,41),new Vector3(28,0,50.7f)},4.8f,dirt,.035f);
            Ribbon(parent,"Back Lane",new[]{new Vector3(45,0,-15),new Vector3(55,0,-7),new Vector3(59,0,9),new Vector3(48,0,17)},2.8f,dirt,.04f);
            if(kind==SettlementKind.Mining||kind==SettlementKind.Lumber)
                Ribbon(parent,"Work Yard Road",new[]{new Vector3(47,0,12),new Vector3(65,0,20),new Vector3(79,0,27)},4f,dirt,.04f);
            // Boardwalks are short frontage pieces, rather than two identical town-long slabs.
            foreach(var at in new[]{new Vector3(38.7f,.12f,-21),new Vector3(51.4f,.12f,6),new Vector3(36.2f,.12f,40)})
                RouteProps.Box(parent,"Village Frontage Boardwalk",at,new Vector3(2.2f,.24f,9f),m[2],true);
            var trough=new GameObject("Village Water Trough").transform;trough.SetParent(parent,false);trough.localPosition=new Vector3(33,.0f,26);
            RouteProps.Box(trough,"Trough Bottom",new Vector3(0,.3f,0),new Vector3(2.6f,.25f,.9f),m[2],true);
            foreach(float side in new[]{-1f,1f}){RouteProps.Box(trough,"Trough Side",new Vector3(0,.65f,side*.49f),new Vector3(2.8f,.7f,.12f),m[2],true);RouteProps.Box(trough,"Trough End",new Vector3(side*1.35f,.65f,0),new Vector3(.12f,.7f,1.1f),m[2],true);}
            RouteProps.Box(trough,"Water Surface",new Vector3(0,.73f,0),new Vector3(2.5f,.025f,.84f),m[4]);
            foreach(float z in new[]{-48f,29f}){RouteProps.Box(parent,"Roadside Hitching Rail",new Vector3(36,1.05f,z),new Vector3(3.2f,.15f,.16f),m[2],true);foreach(float x in new[]{34.6f,37.4f})RouteProps.Box(parent,"Hitching Post",new Vector3(x,.55f,z),new Vector3(.18f,1.1f,.18f),m[2],true);}
        }
        public static void HouseLane(Transform parent,Vector3 house,float side,Material dirt,Material[] m)
        {
            var points=MainStreet;Vector3 road=points[0];
            for(int i=0;i<points.Length-1;i++)if(house.z>=points[i].z&&house.z<=points[i+1].z){road=Vector3.Lerp(points[i],points[i+1],Mathf.InverseLerp(points[i].z,points[i+1].z,house.z));break;}
            Vector3 entry=new Vector3(house.x-side*6.2f,0,house.z);
            Ribbon(parent,"House Entry Lane",new[]{road,new Vector3((road.x+entry.x)*.5f,0,house.z+1.3f*side),entry},2f,dirt,.045f);
        }
        public static GameObject Ribbon(Transform parent,string name,Vector3[] points,float width,Material material,float top)
        {
            var vertices=new Vector3[points.Length*4];var triangles=new System.Collections.Generic.List<int>();
            for(int i=0;i<points.Length;i++)
            {
                Vector3 before=(points[i]-points[Mathf.Max(0,i-1)]).normalized,after=(points[Mathf.Min(points.Length-1,i+1)]-points[i]).normalized;
                if(i==0)before=after;if(i==points.Length-1)after=before;
                var normal=Vector3.Cross(Vector3.up,(before+after).normalized);float stretch=Mathf.Min(1.4f,1f/Mathf.Max(.1f,Vector3.Dot(normal,Vector3.Cross(Vector3.up,after))));
                var offset=normal*width*.5f*stretch;
                vertices[i*4]=points[i]-offset+Vector3.up*top;vertices[i*4+1]=points[i]+offset+Vector3.up*top;
                vertices[i*4+2]=points[i]-offset+Vector3.up*(top-.06f);vertices[i*4+3]=points[i]+offset+Vector3.up*(top-.06f);
            }
            void Quad(int a,int b,int c,int d){triangles.AddRange(new[]{a,b,c,a,c,d});}
            for(int i=0;i<points.Length-1;i++){int a=i*4,b=a+4;Quad(a,b,b+1,a+1);Quad(a+2,a+3,b+3,b+2);Quad(a,a+2,b+2,b);Quad(a+1,b+1,b+3,a+3);}
            Quad(0,1,3,2);int end=vertices.Length-4;Quad(end,end+2,end+3,end+1);
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
            var road=new GameObject(name);road.transform.SetParent(parent,false);road.AddComponent<MeshFilter>().sharedMesh=mesh;road.AddComponent<MeshRenderer>().sharedMaterial=material;road.AddComponent<GeneratedRailMesh>().Own(mesh);return road;
        }
    }
}
