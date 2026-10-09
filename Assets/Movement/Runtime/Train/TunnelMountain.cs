using System.Collections.Generic;
using UnityEngine;
namespace AllAboard.Train
{
    public sealed class TunnelMountain:MonoBehaviour
    {
        private Mesh owned;
        public static void Build(Transform root,float height,Material[] materials)
        {
            height=Mathf.Max(120f,height);
            var asset=root.parent.GetComponent<WorldArtAssets>();if(asset==null)asset=root.parent.gameObject.AddComponent<WorldArtAssets>();
            var rock=asset.Paint(materials[1],"Tunnel Mountain Rock",WorldRegions.GroundColour(LandscapeRegion.Mountains));
            var verts=new List<Vector3>();var faces=new List<int>();
            float[] xs={-160f,-125f,-85f,-45f,-12f,-4f,0f,4f,12f,45f,85f,125f,160f};
            float[] zs={-60f,-38f,-10f,15f,40f,60f};
            System.Func<float,float,float> top=(x,z)=>Mathf.Max(7.1f,height*Mathf.Pow(Mathf.Max(0f,1f-Mathf.Abs(x)/160f),1.3f)*(1f-.62f*Mathf.Abs(z)/60f));
            System.Action<Vector3,Vector3,Vector3,Vector3> quad=(a,b,c,d)=>
            {int start=verts.Count;verts.Add(a);verts.Add(b);verts.Add(c);verts.Add(d);faces.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});};
            for(int z=0;z<zs.Length-1;z++)for(int x=0;x<xs.Length-1;x++)
                quad(new Vector3(xs[x],top(xs[x],zs[z]),zs[z]),new Vector3(xs[x],top(xs[x],zs[z+1]),zs[z+1]),new Vector3(xs[x+1],top(xs[x+1],zs[z+1]),zs[z+1]),new Vector3(xs[x+1],top(xs[x+1],zs[z]),zs[z]));
            foreach(float end in new[]{-60f,60f})for(int x=0;x<xs.Length-1;x++)
            {
                float floor=xs[x]>=-4f&&xs[x+1]<=4f?6.8f:0f;
                Vector3 a=new Vector3(xs[x],floor,end),b=new Vector3(xs[x+1],floor,end),c=new Vector3(xs[x+1],top(xs[x+1],end),end),d=new Vector3(xs[x],top(xs[x],end),end);
                if(end<0f)quad(b,a,d,c);else quad(a,b,c,d);
            }
            foreach(float side in new[]{-160f,160f})for(int z=0;z<zs.Length-1;z++)
            {
                Vector3 a=new Vector3(side,0f,zs[z]),b=new Vector3(side,0f,zs[z+1]),c=new Vector3(side,top(side,zs[z+1]),zs[z+1]),d=new Vector3(side,top(side,zs[z]),zs[z]);
                if(side<0f)quad(a,b,c,d);else quad(b,a,d,c);
            }
            var mesh=new Mesh{name="Large Mountain with Tunnel Openings"};mesh.SetVertices(verts);mesh.SetTriangles(faces,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var model=new GameObject("Large Tunnel Mountain",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));model.transform.SetParent(root,false);
            model.GetComponent<MeshFilter>().sharedMesh=mesh;model.GetComponent<MeshRenderer>().sharedMaterial=rock;model.GetComponent<MeshCollider>().sharedMesh=mesh;
            model.AddComponent<TunnelMountain>().owned=mesh;
        }
        private void OnDestroy(){if(owned!=null){GetComponent<MeshCollider>().sharedMesh=null;GetComponent<MeshFilter>().sharedMesh=null;if(Application.isPlaying)Destroy(owned);else DestroyImmediate(owned);}}
    }
}
