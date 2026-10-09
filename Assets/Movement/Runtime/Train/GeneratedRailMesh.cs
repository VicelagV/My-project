using UnityEngine;
namespace AllAboard.Train
{
    public sealed class GeneratedRailMesh:MonoBehaviour
    {
        private Mesh mesh;
        public void Own(Mesh generated)=>mesh=generated;
        private void OnDestroy(){if(mesh==null)return;if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}
    }
}
