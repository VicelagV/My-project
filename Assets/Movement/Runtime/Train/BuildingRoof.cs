using System.Collections.Generic;
using UnityEngine;
namespace AllAboard.Train
{
    // A closed roof shell; the same pitch defines both slopes and the gable fill.
    public sealed class BuildingRoof : MonoBehaviour
    {
        private readonly List<Mesh> owned = new List<Mesh>();
        public static float Rise(float width,float pitch)=>width*.5f*Mathf.Tan(pitch*Mathf.Deg2Rad);
        public static Transform Build(Transform parent,string name,Vector3 position,float width,float depth,float wallTop,float pitch,Material roof,Material wall,float overhang=.3f,bool acrossDepth=false)
        {
            var root=new GameObject(name).transform;root.SetParent(parent,false);root.localPosition=position;
            if(acrossDepth)root.localRotation=Quaternion.Euler(0f,90f,0f);
            var owner=root.gameObject.AddComponent<BuildingRoof>();
            float half=width*.5f+overhang,end=depth*.5f+overhang,slope=Mathf.Tan(pitch*Mathf.Deg2Rad),ridge=wallTop+Rise(width,pitch),eave=wallTop-overhang*slope,thickness=.16f;
            var v=new List<Vector3>();var t=new List<int>();
            System.Action<Vector3,Vector3,Vector3,Vector3,Vector3> quad=(a,b,c,d,n)=>{Face(v,t,a,b,c,n);Face(v,t,a,c,d,n);};
            foreach(float side in new[]{-1f,1f})
            {
                Vector3 a=new Vector3(0f,ridge,-end),b=new Vector3(side*half,eave,-end),c=new Vector3(side*half,eave,end),d=new Vector3(0f,ridge,end);
                quad(a,b,c,d,Vector3.up);quad(a-Vector3.up*thickness,b-Vector3.up*thickness,c-Vector3.up*thickness,d-Vector3.up*thickness,Vector3.down);
                quad(b,c,c-Vector3.up*thickness,b-Vector3.up*thickness,Vector3.right*side);
                quad(a,b,b-Vector3.up*thickness,a-Vector3.up*thickness,Vector3.back);
                quad(d,c,c-Vector3.up*thickness,d-Vector3.up*thickness,Vector3.forward);
            }
            owner.Model(root,"Closed Pitched Roof",v,t,roof);
            v=new List<Vector3>();t=new List<int>();
            // A solid triangular prism closes the attic, with a slight hidden seam overlap.
            float w=width*.5f,z=depth*.5f+.012f,baseY=wallTop-.025f,top=ridge-thickness+.02f;
            Vector3 l=new Vector3(-w,baseY,-z),r=new Vector3(w,baseY,-z),p=new Vector3(0f,top,-z),offset=Vector3.forward*z*2f;
            Face(v,t,l,p,r,Vector3.back);Face(v,t,l+offset,r+offset,p+offset,Vector3.forward);
            quad=(a,b,c,d,n)=>{Face(v,t,a,b,c,n);Face(v,t,a,c,d,n);};
            quad(l,r,r+offset,l+offset,Vector3.down);quad(l,p,p+offset,l+offset,Vector3.left);quad(r,p,p+offset,r+offset,Vector3.right);
            owner.Model(root,"Closed Attic Gable",v,t,wall);
            return root;
        }
        private static void Face(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 outward)
        {
            if(Vector3.Dot(Vector3.Cross(b-a,c-a),outward)<0f){var swap=b;b=c;c=swap;}
            int i=v.Count;v.Add(a);v.Add(b);v.Add(c);t.Add(i);t.Add(i+1);t.Add(i+2);
        }
        private void Model(Transform root,string name,List<Vector3> v,List<int> t,Material material)
        {
            var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();owned.Add(mesh);
            var model=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));model.transform.SetParent(root,false);model.GetComponent<MeshFilter>().sharedMesh=mesh;model.GetComponent<MeshRenderer>().sharedMaterial=material;
        }
        private void OnDestroy(){foreach(var mesh in owned)if(mesh!=null){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
    }
}
