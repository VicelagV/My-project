namespace AllAboard.Train
{
    public sealed class TrainBrakeLever : TrainLever
    {
        public override string Prompt => "Hold M1 + move mouse: brake";
        protected override float Value { get => train.BrakeAmount; set => train.BrakeAmount = value; }
    }
}
