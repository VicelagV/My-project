using UnityEngine;
namespace AllAboard.Train
{
    public enum SalvageCategory { RailParts, Fuel, General }
    public sealed class LootCargo:MonoBehaviour { public int Value=40; public SalvageCategory Category=SalvageCategory.RailParts; }
}
