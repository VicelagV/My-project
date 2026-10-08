using UnityEngine;
namespace AllAboard.Train
{
    public sealed class BuildingGable:MonoBehaviour
    {
        private Mesh owned;
        public static void Build(Transform house,float width,float height,float rise,Material material)
        {
            var mesh=new Mesh{name="Closed House Gable"};mesh.vertices=new[]{new Vector3(-width*.5f,height,-3.52f),new Vector3(width*.5f,height,-3.52f),new Vector3(0f,height+rise,-3.52f),new Vector3(-width*.5f,height,3.52f),new Vector3(width*.5f,height,3.52f),new Vector3(0f,height+rise,3.52f)};mesh.triangles=new[]{0,2,1,3,4,5};mesh.RecalculateNormals();mesh.RecalculateBounds();
            var model=new GameObject("Closed House Gable",typeof(MeshFilter),typeof(MeshRenderer));model.transform.SetParent(house,false);model.GetComponent<MeshFilter>().sharedMesh=mesh;model.GetComponent<MeshRenderer>().sharedMaterial=material;model.AddComponent<BuildingGable>().owned=mesh;
        }
        private void OnDestroy(){if(owned!=null){GetComponent<MeshFilter>().sharedMesh=null;if(Application.isPlaying)Destroy(owned);else DestroyImmediate(owned);}}
    }
}
