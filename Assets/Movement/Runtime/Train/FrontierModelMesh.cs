using System.Collections.Generic;
using UnityEngine;
namespace AllAboard.Train
{
    public static class FrontierModelMesh
    {
        public static GameObject Box(Transform parent,string name,Vector3 at,Vector3 size,Material paint,Quaternion? rotation=null)
        {
            var part=CraftGeometry.Box(parent,name,at,size,paint);
            part.GetComponent<MeshFilter>().sharedMesh=Resources.Load<Mesh>("CraftBevelBox");
            if(rotation.HasValue)part.transform.localRotation=rotation.Value;return part;
        }
        public static GameObject Part(Transform parent,string name,Vector3 at,Vector3 size,Material paint,Mesh mesh,Quaternion? rotation=null)
        {
            var part=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(parent,false);
            part.transform.localPosition=at;part.transform.localScale=size;part.transform.localRotation=rotation??Quaternion.identity;
            part.GetComponent<MeshFilter>().sharedMesh=mesh;part.GetComponent<Renderer>().sharedMaterial=paint;
            var owner=parent.GetComponent<CraftMeshOwner>();if(owner==null)owner=parent.gameObject.AddComponent<CraftMeshOwner>();owner.Own(mesh);return part;
        }
        // Capped polygonal cylinder, with independent lower and upper radii; unit height.
        public static Mesh Cylinder(int sides=12,float lower=.5f,float upper=.5f)
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                var l=new Vector3(Mathf.Cos(a)*lower,-.5f,Mathf.Sin(a)*lower);var r=new Vector3(Mathf.Cos(b)*lower,-.5f,Mathf.Sin(b)*lower);
                var u=new Vector3(Mathf.Cos(a)*upper,.5f,Mathf.Sin(a)*upper);var w=new Vector3(Mathf.Cos(b)*upper,.5f,Mathf.Sin(b)*upper);
                Face(v,t,l,u,w);Face(v,t,l,w,r);Face(v,t,Vector3.down*.5f,l,r);Face(v,t,Vector3.up*.5f,w,u);
            }
            return Finish("Frontier Capped Cylinder",v,t);
        }
        // Closed annular wheel, axis Z, for wide rims rather than thin decorative rings.
        public static Mesh Ring(int sides=16,float inner=.34f)
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                var outerA=new Vector3(Mathf.Cos(a)*.5f,Mathf.Sin(a)*.5f,0);var outerB=new Vector3(Mathf.Cos(b)*.5f,Mathf.Sin(b)*.5f,0);
                var innerA=outerA*(inner/.5f);var innerB=outerB*(inner/.5f);
                Quad(v,t,outerA+Vector3.forward*.5f,innerA+Vector3.forward*.5f,innerB+Vector3.forward*.5f,outerB+Vector3.forward*.5f);
                Quad(v,t,outerB-Vector3.forward*.5f,innerB-Vector3.forward*.5f,innerA-Vector3.forward*.5f,outerA-Vector3.forward*.5f);
                Quad(v,t,outerB+Vector3.forward*.5f,outerB-Vector3.forward*.5f,outerA-Vector3.forward*.5f,outerA+Vector3.forward*.5f);
                Quad(v,t,innerA+Vector3.forward*.5f,innerA-Vector3.forward*.5f,innerB-Vector3.forward*.5f,innerB+Vector3.forward*.5f);
            }
            for(int i=0;i<t.Count;i+=3){int swap=t[i+1];t[i+1]=t[i+2];t[i+2]=swap;}
            return Finish("Frontier Closed Wide Rim",v,t);
        }
        public static Mesh ArchBack()
        {
            var v=new List<Vector3>();var t=new List<int>();
            Vector3[] front={new Vector3(-.5f,-.5f,-.5f),new Vector3(.5f,-.5f,-.5f),new Vector3(.5f,.3f,-.5f),new Vector3(.38f,.5f,-.5f),new Vector3(-.38f,.5f,-.5f),new Vector3(-.5f,.3f,-.5f)};
            for(int i=0;i<front.Length;i++){int next=(i+1)%front.Length;Face(v,t,new Vector3(0,0,-.5f),front[next],front[i]);Face(v,t,new Vector3(0,0,.5f),front[i]+Vector3.forward,front[next]+Vector3.forward);Quad(v,t,front[i],front[next],front[next]+Vector3.forward,front[i]+Vector3.forward);}
            return Finish("Frontier Clipped Seat Back",v,t);
        }
        private static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d){Face(v,t,a,b,c);Face(v,t,a,c,d);}
        private static void Face(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c){int n=v.Count;v.Add(a);v.Add(b);v.Add(c);t.Add(n);t.Add(n+1);t.Add(n+2);}
        private static Mesh Finish(string name,List<Vector3> v,List<int> t){var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
    }
}
