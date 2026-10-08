using UnityEngine;
namespace AllAboard.Train
{
    public sealed class TrainSteamVisual : MonoBehaviour
    {
        [SerializeField] private ParticleSystem chimney;
        [SerializeField] private ParticleSystem cylinders;
        private TrainController train;
        public void Configure(ParticleSystem smoke, ParticleSystem steam) { chimney = smoke; cylinders = steam; }
        public void Refresh()
        {
            if (train == null) train = GetComponentInParent<TrainCar>()?.Owner;
            if (train == null) return;
            bool powered = train.Resources.Steam > 0.01f;
            if (chimney != null) { var emission = chimney.emission; emission.rateOverTime = powered ? 2f + (train.Braking ? 0f : train.Throttle * 12f) : 0f; }
            if (cylinders != null) { var emission = cylinders.emission; emission.rateOverTime = powered && Mathf.Abs(train.Speed) > 0.2f ? 5f : 0f; }
        }
        private void Update() => Refresh();
    }
}
