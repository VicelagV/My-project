namespace AllAboard.Train
{
    public sealed class TrainThrottleLever : TrainLever
    {
        public override string Prompt => "Hold M1 + move mouse: throttle";
        protected override float Value { get => train.Throttle; set => train.Throttle = value; }
    }
}
