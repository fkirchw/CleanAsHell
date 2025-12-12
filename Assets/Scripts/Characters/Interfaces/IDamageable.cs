using UnityEngine;

namespace Characters.Interfaces
{
    public interface IDamageable
    {
        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce);

        public void DealDamage(float attackDistance);
    }
}