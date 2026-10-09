using System.Collections.Generic;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class PortalArch : MonoBehaviour
    {
        private Mesh owned;
        public static PortalArch Build(Transform parent,float z,Material stone)
        {
            var v=new List<Vector3>();var t=new List<int>();
            System.Action<Vector3,Vector3,Vector3,Vector3,Vector3> quad=(a,b,c,d,n)=>
            {
                int first=v.Count;v.Add(a);v.Add(b);v.Add(c);v.Add(d);
                if(Vector3.Dot(Vector3.Cross(b-a,c-a),n)>0f)t.AddRange(new[]{first,first+1,first+2,first,first+2,first+3});
                else t.AddRange(new[]{first,first+2,first+1,first,first+3,first+2});
            };
            System.Func<float,float,float,Vector3> point=(angle,radius,depth)=>new Vector3(Mathf.Cos(angle)*radius,2f+Mathf.Sin(angle)*radius,z+depth);
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI/24f,b=(i+1)*Mathf.PI/24f,mid=(a+b)*.5f;
                var ia=point(a,4.05f,-.83f);var ib=point(b,4.05f,-.83f);var oa=point(a,4.8f,-.83f);var ob=point(b,4.8f,-.83f);
                Vector3 back=Vector3.forward*.35f,radial=new Vector3(Mathf.Cos(mid),Mathf.Sin(mid),0f);
                quad(ia,ib,ob,oa,Vector3.back);quad(ia+back,ib+back,ob+back,oa+back,Vector3.forward);
                quad(oa,ob,ob+back,oa+back,radial);quad(ia,ib,ib+back,ia+back,-radial);
                if(i==0)quad(ia,oa,oa+back,ia+back,Vector3.down);
                if(i==23)quad(ib,ob,ob+back,ib+back,Vector3.down);
            }
            var model=new GameObject("Continuous Stone Portal Arch",typeof(MeshFilter),typeof(MeshRenderer));model.transform.SetParent(parent,false);
            var mesh=new Mesh{name="Closed Stone Arch"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            model.GetComponent<MeshFilter>().sharedMesh=mesh;model.GetComponent<MeshRenderer>().sharedMaterial=stone;var owner=model.AddComponent<PortalArch>();owner.owned=mesh;return owner;
        }
        private void OnDestroy(){if(owned!=null){GetComponent<MeshFilter>().sharedMesh=null;if(Application.isPlaying)Destroy(owned);else DestroyImmediate(owned);}}
    }
}
