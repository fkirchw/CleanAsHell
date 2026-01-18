using UnityEngine;

namespace Characters.Interfaces
{
    public interface IDamageable
    {
        public int health { get; }
        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce);
    }
}