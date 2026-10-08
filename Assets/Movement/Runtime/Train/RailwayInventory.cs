using UnityEngine;
namespace AllAboard.Train
{
    public sealed class RailwayInventory : MonoBehaviour
    {
        public const int SteelPrice = 50;
        public int Gold { get; private set; } = 500;
        public float SteelMetres { get; private set; } = 20f;
        public void BeginDay() { Gold = 500; SteelMetres = 20f; }
        public bool Spend(int amount) { if (amount < 0 || Gold < amount) return false; Gold -= amount; return true; }
        public void Earn(int amount) { if (amount > 0) Gold += amount; }
        public bool BuySteel() { if (!Spend(SteelPrice)) return false; SteelMetres += 5f; return true; }
        public bool UseSteel(float metres) { if (metres <= 0f || metres > 5.001f || SteelMetres + .001f < metres) return false; SteelMetres = Mathf.Max(0f, SteelMetres - metres); return true; }
    }
}
