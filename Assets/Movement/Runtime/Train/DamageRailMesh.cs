using System.Collections.Generic;
using UnityEngine;
namespace AllAboard.Train
{
    public static class DamageRailMesh
    {
        public static Vector3 DeformedPoint(RailDamageSection section,int side,float d,bool bent)
        {
            var p=section.Point(side,d);
            if(bent)
            {
                Vector3 tangent=(section.Point(side,Mathf.Min(8f,d+.05f))-section.Point(side,Mathf.Max(0f,d-.05f))).normalized;
                Vector3 lateral=Vector3.Cross(Vector3.up,tangent).normalized;
                float envelope=Mathf.Sin(d*Mathf.PI/8f);
                p+=lateral*Mathf.Sin(d*Mathf.PI/2f)*.7f*envelope+Vector3.up*Mathf.Sin(d*Mathf.PI/4f)*.1f*envelope;
            }
            return p;
        }
        public static void Build(Transform parent,RailDamageSection section,int side,float from,float to,bool bent,Material material)
        {
            if(to-from<.001f)return;
            Band(parent,section,side,from,to,bent,material,"Rail Head",.14f,-.011f,.011f);
            Band(parent,section,side,from,to,bent,material,"Rail Web",.055f,-.041f,-.011f);
            Band(parent,section,side,from,to,bent,material,"Rail Foot",.22f,-.059f,-.041f);
        }
        private static void Band(Transform parent,RailDamageSection section,int side,float from,float to,bool bent,Material material,string name,float width,float bottom,float top)
        {
            int count=Mathf.Max(1,Mathf.CeilToInt((to-from)/.18f));var vertices=new Vector3[(count+1)*4];var triangles=new List<int>();
            for(int i=0;i<=count;i++)
            {
                float d=Mathf.Lerp(from,to,i/(float)count);var p=DeformedPoint(section,side,d,bent);
                var tangent=DeformedPoint(section,side,Mathf.Min(8f,d+.03f),bent)-DeformedPoint(section,side,Mathf.Max(0f,d-.03f),bent);
                var q=Quaternion.LookRotation(tangent.normalized,Vector3.up);
                vertices[i*4]=parent.InverseTransformPoint(p+q*new Vector3(-width/2,bottom,0));vertices[i*4+1]=parent.InverseTransformPoint(p+q*new Vector3(width/2,bottom,0));
                vertices[i*4+2]=parent.InverseTransformPoint(p+q*new Vector3(width/2,top,0));vertices[i*4+3]=parent.InverseTransformPoint(p+q*new Vector3(-width/2,top,0));
                if(i==count)continue;
                for(int j=0;j<4;j++){int a=i*4+j,b=i*4+(j+1)%4,c=a+4,e=b+4;triangles.AddRange(new[]{a,b,c,b,e,c});}
            }
            triangles.AddRange(new[]{0,2,1,0,3,2});int last=count*4;triangles.AddRange(new[]{last,last+1,last+2,last,last+2,last+3});
            var mesh=new Mesh{name=name+" Closed Damage Mesh",vertices=vertices,triangles=triangles.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();
            var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;obj.AddComponent<GeneratedRailMesh>().Own(mesh);
        }
    }
}
