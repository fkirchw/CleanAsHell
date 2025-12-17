using UnityEngine;

namespace Blood
{
    /// <summary>
    /// Example enemy that spawns blood when taking damage.
    /// This demonstrates how to integrate with the blood system.
    /// </summary>
    public class EnemyBloodExample : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;
    
        [Header("Blood Settings")]
        [SerializeField] private float bloodPerHit = 0.3f;
        [SerializeField] private bool isGrounded = true;
    
        private Rigidbody2D rb;

        void Start()
        {
            currentHealth = maxHealth;
            rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Call this when enemy takes damage
        /// </summary>
        public void TakeDamage(float damage, Vector2 hitDirection)
        {
            currentHealth -= damage;
        
            // Determine if enemy is airborne
            bool airborne = !isGrounded;
            if (rb != null)
            {
                airborne = Mathf.Abs(rb.linearVelocity.y) > 0.1f;
            }
        
            // Spawn blood
            if (BloodSystem.Instance != null)
            {
                BloodSystem.Instance.OnEnemyHit(
                    transform.position,
                    hitDirection,
                    airborne,
                    bloodPerHit
                );
            }
        
            if (currentHealth <= 0)
            {
                Die(hitDirection);
            }
        }

        void Die(Vector2 deathDirection)
        {
            // Spawn extra blood on death
            if (BloodSystem.Instance != null)
            {
                BloodSystem.Instance.OnEnemyHit(
                    transform.position,
                    deathDirection,
                    false,
                    bloodPerHit * 2f // More blood on death
                );
            }
        
            Destroy(gameObject);
        }

        // Example: Detect collision with player weapon
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("PlayerWeapon"))
            {
                // Calculate direction from weapon to enemy
                Vector2 hitDirection = (transform.position - collision.transform.position).normalized;
            
                TakeDamage(25f, hitDirection);
            }
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            // Simple ground detection
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = true;
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = false;
            }
        }
    }
}
