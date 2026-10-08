using UnityEngine;
namespace AllAboard.Train
{
    [ExecuteAlways]
    public sealed class PlaqueLettering:MonoBehaviour
    {
        [SerializeField] private float width=3f,height=.5f;
        public void Configure(float w,float h){width=w;height=h;Fit();}
        private void LateUpdate()=>Fit();
        public void Fit()
        {
            var label=GetComponent<TextMesh>();if(label==null)return;
            Vector3 size=GetComponent<Renderer>().localBounds.size;
            if(size.x<.0001f||size.y<.0001f)return;
            float factor=Mathf.Min((width-.18f)/size.x,(height-.12f)/size.y);
            transform.localScale=Vector3.one*Mathf.Clamp(factor,.03f,8f);
        }
    }
}
