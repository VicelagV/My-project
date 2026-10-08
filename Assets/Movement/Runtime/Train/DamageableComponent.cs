using UnityEngine;

namespace AllAboard.Train
{
    public sealed class DamageableComponent : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumHealth = 100f;
        [SerializeField] private float health = 100f;
        public float Health => health;
        public bool IsBroken => health <= 0f;
        public void Damage(float amount) { health = Mathf.Clamp(health - Mathf.Max(0f, amount), 0f, maximumHealth); }
    }
}
