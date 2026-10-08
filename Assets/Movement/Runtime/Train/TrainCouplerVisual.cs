using UnityEngine;
namespace AllAboard.Train
{
    public sealed class TrainCouplerVisual : MonoBehaviour
    {
        [SerializeField] private TrainCoupler coupler;
        [SerializeField] private Transform link;
        [SerializeField] private Renderer lockIndicator;
        private MaterialPropertyBlock tint;
        public void Configure(TrainCoupler control, Transform eye, Renderer indicator) { coupler = control; link = eye; lockIndicator = indicator; }
        private void Update()
        {
            if (coupler == null) return;
            if (link != null) link.localRotation = Quaternion.Euler(0f, coupler.Connected ? 0f : -65f, 0f);
            if (lockIndicator != null)
            {
                if (tint == null) tint = new MaterialPropertyBlock();
                lockIndicator.GetPropertyBlock(tint); tint.SetColor("_BaseColor", coupler.Connected ? new Color(0.3f, 0.65f, 0.4f) : new Color(0.75f, 0.16f, 0.13f));
                lockIndicator.SetPropertyBlock(tint);
            }
        }
    }
}
