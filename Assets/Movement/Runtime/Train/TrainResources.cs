using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainResources : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float steam = 50f;
        [SerializeField, Range(0f, 100f)] private float pressure = 40f;
        [SerializeField] private int coalSupply = 10;
        [SerializeField] private float steamPerCoal = 40f;
        [SerializeField] private float pressurePerCoal = 5f;
        [SerializeField, Min(0f)] private float fullThrottleDrain = 0.5f;
        public float Steam => steam;
        public float Pressure => pressure;
        public int CoalSupply => coalSupply;
        public void RegisterSupplyCoal() => coalSupply++;
        public bool Critical { get; private set; }
        public float Performance => steam <= 0.01f ? 0f : steam < 20f ? Mathf.Lerp(0.2f, 1f, steam / 20f) : steam > 80f ? 1.25f : 1f;
        public float BrakeEfficiency => Mathf.Lerp(0.35f, 1f, Mathf.Clamp01(pressure / 10f));
        public float ConsumptionPerSecond(float throttle, bool braking) => braking ? 0f : Mathf.Clamp01(throttle) * fullThrottleDrain;
        public void SetLevels(float newSteam, float newPressure) { steam = Mathf.Clamp(newSteam, 0f, 100f); pressure = Mathf.Clamp(newPressure, 0f, 100f); UpdateWarning(); }
        public bool AddCoal()
        {
            if (coalSupply <= 0) return false;
            coalSupply--; steam = Mathf.Clamp(steam + steamPerCoal, 0f, 100f);
            pressure = Mathf.Clamp(pressure + pressurePerCoal, 0f, 100f); UpdateWarning(); return true;
        }
        public void Vent() { pressure = Mathf.Max(0f, pressure - 20f); steam = Mathf.Max(0f, steam - 10f); UpdateWarning(); }
        public void Step(float throttle, bool braking, float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (throttle > 0f && !braking && steam > 0f)
            {
                float high = steam > 80f ? 1.75f : 1f;
                steam = Mathf.Max(0f, steam - ConsumptionPerSecond(throttle, braking) * deltaTime);
                pressure = Mathf.Min(100f, pressure + throttle * (high > 1f ? 1.2f : 0.15f) * deltaTime);
            }
            if (braking) pressure = Mathf.Max(0f, pressure - 0.3f * deltaTime);
            UpdateWarning();
        }
        private void UpdateWarning() { if (steam >= 100f) Critical = true; else if (steam < 90f) Critical = false; }
    }
}
