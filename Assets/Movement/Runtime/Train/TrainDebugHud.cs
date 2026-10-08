using UnityEngine;

namespace AllAboard.Train
{
    public sealed class TrainDebugHud : MonoBehaviour
    {
        [SerializeField] private TrainController train;
        [SerializeField] private DamageableComponent speedometer;
        public void Configure(TrainController controller, DamageableComponent instrument) { train = controller; speedometer = instrument; }
        private void OnGUI()
        {
            if (train == null) return;
            string speed = speedometer != null && speedometer.IsBroken ? "BROKEN" : Mathf.Abs(train.Speed).ToString("F1") + " m/s";
            GUI.Box(new Rect(12f, 12f, 275f, 160f), "TRAIN PROTOTYPE");
            GUI.Label(new Rect(24f, 40f, 260f, 130f), "Speed: " + speed + " | " + (train.Reversing ? "REVERSE" : "FORWARD") +
                "\nThrottle: " + (train.Throttle * 100f).ToString("F0") + "% | Brake: " + (train.Braking ? "ON" : "OFF") +
                "\nSteam: " + train.Resources.Steam.ToString("F0") + " | Pressure: " + train.Resources.Pressure.ToString("F0") +
                "\nCoal left: " + train.Resources.CoalSupply + " | Cars: " + train.ConnectedCount +
                "\nWeight: " + train.TotalWeight.ToString("F1") + (train.Resources.Critical ? "\nCRITICAL STEAM - VENT NOW" : "\nAim at labelled controls and press E"));
        }
    }
}
