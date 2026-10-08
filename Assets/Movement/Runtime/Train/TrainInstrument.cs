using UnityEngine;

namespace AllAboard.Train
{
    public enum InstrumentKind { Steam, Pressure, Speed }
    public sealed class TrainInstrument : AllAboard.Movement.PrototypeInteractable
    {
        [SerializeField] private TrainController train;
        [SerializeField] private InstrumentKind kind;
        [SerializeField] private TextMesh label;
        [SerializeField] private DamageableComponent condition;
        [SerializeField] private Transform needle;
        public Transform Needle => needle;
        public override bool AllowedWhileSeated => true;
        public override string Prompt => kind + " gauge";
        public override bool Interact(AllAboard.Movement.PlayerInteraction actor) => false;
        public void ConfigureDial(Transform pointer) { needle = pointer; Refresh(); }
        public string DisplayedText => label != null ? label.text : "";
        public DamageableComponent Condition => condition;
        public void Configure(TrainController controller, InstrumentKind type, TextMesh text, DamageableComponent damage = null)
        { train = controller; kind = type; label = text; condition = damage; }
        private void Update() => Refresh();
        public void Refresh()
        {
            if (train == null || label == null) return;
            if (needle != null)
            {
                bool broken = kind == InstrumentKind.Speed && condition != null && condition.IsBroken;
                needle.gameObject.SetActive(!broken);
                float value = kind == InstrumentKind.Speed ? Mathf.Abs(train.Speed) / 15f :
                    (kind == InstrumentKind.Steam ? train.Resources.Steam : train.Resources.Pressure) / 100f;
                needle.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(135f, -135f, Mathf.Clamp01(value)));
            }
            if (kind == InstrumentKind.Speed)
            {
                label.text = condition != null && condition.IsBroken ? "SPEED: BROKEN" : "SPEED\n" + Mathf.Abs(train.Speed).ToString("F1") + " m/s";
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
