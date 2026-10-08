using UnityEngine;

namespace AllAboard.Train
{
    [ExecuteAlways]
    public sealed class RailTrackVisual : MonoBehaviour
    {
        [SerializeField] private RailPath path;
        [SerializeField] private Material ballast,wood,rail;
        public void Configure(RailPath curve,Material gravel,Material sleepers,Material steel)
        {path=curve;ballast=gravel;wood=sleepers;rail=steel;Rebuild();}
        private void OnEnable(){if(path!=null&&ballast!=null&&wood!=null&&rail!=null)Rebuild();}
        public void Rebuild()
        {
            var old=transform.Find("Generated Test Track");
            if(old!=null){old.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}
            var root=new GameObject("Generated Test Track");root.transform.SetParent(transform,false);
            if(!Application.isPlaying)root.hideFlags=HideFlags.DontSaveInEditor;
            root.AddComponent<RouteGeometry>().BuildTrack(path,ballast,wood,rail);
        }
    }
}
