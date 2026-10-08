using UnityEngine;

namespace AllAboard.Train
{
    public enum InstrumentKind { Steam, Pressure, Speed }
    public sealed class TrainInstrument : MonoBehaviour
    {
        [SerializeField] private TrainController train;
        [SerializeField] private InstrumentKind kind;
        [SerializeField] private TextMesh label;
        [SerializeField] private DamageableComponent condition;
        public string DisplayedText => label != null ? label.text : "";
        public DamageableComponent Condition => condition;
        public void Configure(TrainController controller, InstrumentKind type, TextMesh text, DamageableComponent damage = null)
        { train = controller; kind = type; label = text; condition = damage; }
        private void Update() => Refresh();
        public void Refresh()
        {
            if (train == null || label == null) return;
            if (kind == InstrumentKind.Speed)
            {
                label.text = condition != null && condition.IsBroken ? "SPEED: BROKEN" : "SPEED\n" + Mathf.Abs(train.Speed).ToString("F1") + " m/s";
                label.text += "\nCondition " + (condition != null ? condition.Health.ToString("F0") : "100");
                label.color = condition != null && condition.IsBroken ? Color.red : Color.white;
            }
            else
            {
                label.text = kind == InstrumentKind.Steam ? "STEAM\n" + train.Resources.Steam.ToString("F0") : "PRESSURE\n" + train.Resources.Pressure.ToString("F0");
                bool warning = kind == InstrumentKind.Steam && train.Resources.Critical;
                if (warning) label.text += "\nCRITICAL";
                label.color = warning ? Color.red : Color.white;
            }
        }
    }
}
