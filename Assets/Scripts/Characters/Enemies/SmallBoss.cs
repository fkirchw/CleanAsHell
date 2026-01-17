using Blood;
using Characters.Interfaces;
using System.Collections;
using UnityEngine;

namespace Characters.Enemies
{
    public class SmallBoss : MonoBehaviour, IDamageable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackDistance = 3f;
        
        [Header("Combat Settings")]
        [SerializeField] private int health = 10;
        [SerializeField] private int attackDamage = 5;
        [SerializeField] private Vector2 attackKnockbackDir = new Vector2(3, 1).normalized;
        [SerializeField] private float attackKnockbackForce = 15f;
        [SerializeField] private float attackCooldown = 2f; 
        
        [Header("Environment Settings")]
        [SerializeField] private GameObject wallToDestroy;
        [SerializeField] private GameObject GateToBoss;
        
        [Header("Contact Damage Settings")]
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float contactDamageCooldown = 0.5f;
        [SerializeField] private Vector2 contactKnockbackDir = new Vector2(1, 1);
        [SerializeField] private float contactKnockbackForce = 3f;
        
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private BossManager bossManager;
        
        private Transform playerPosition;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Vector2 direction;
        private bool isDead;
        private bool playerDetected;
        private bool gateActivated = false;
        private bool canAttack = true;
        private float lastContactDamageTime;
        private float MAX_HEALTH;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            MAX_HEALTH = health;
            
            if (GateToBoss != null)
            {
                GateToBoss.SetActive(false);
            }
        }

        void Update()
        {
            if (isDead)
            {
                return;
            }

            if (playerDetected && playerPosition != null)
            {
                HandleMovement();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerDetected = true;
                playerPosition = collision.transform;
                animator.SetBool("isWalking", true);
                
                if (!gateActivated && GateToBoss != null)
                {
                    GateToBoss.SetActive(true);
                    gateActivated = true;
                }
                
                if (bossManager != null)
                {
                    bossManager.ShowBossHealthBar();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerDetected = false;
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", false);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (isDead) return;
            
            if (collision.gameObject.CompareTag("Player"))
            {
                if (Time.time >= lastContactDamageTime + contactDamageCooldown)
                {
                    IDamageable playerScript = collision.gameObject.GetComponent<IDamageable>();
                    if (playerScript != null)
                    {
                        Vector2 knockbackDir = contactKnockbackDir.normalized;
                        
                        if (direction.x < 0)
                        {
                            knockbackDir.x *= -1;
                        }
                        
                        playerScript.TakeDamage(contactDamage, knockbackDir, contactKnockbackForce);
                        lastContactDamageTime = Time.time;
                    }
                }
            }
        }

        private void HandleMovement()
        {
            if (isDead || playerPosition == null) return;

            direction = (playerPosition.position - transform.position).normalized;

            if (direction.x < 0)
                spriteRenderer.flipX = false;
            else if (direction.x > 0)
                spriteRenderer.flipX = true;

            float distanceToPlayer = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            // Controlăm animația de atac bazată pe distanță
            animator.SetBool("isAttacking", distanceToPlayer < attackDistance);

            // Opțional: putem opri mișcarea când atacă
            if (distanceToPlayer < attackDistance)
            {
                // Oprim mișcarea când suntem suficient de aproape pentru atac
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                // Ne mișcăm către jucător
                float moveX = moveSpeed * direction.x;
                float moveY = moveSpeed * direction.y;
                rb.linearVelocity = new Vector2(moveX, moveY);
            }
        }

        public void OnDamageDeltAniEvent()
        {
            if (isDead) return;
            
            // Resetează animația de atac
            animator.SetBool("isAttacking", false);
            
            // Verifică distanța și aplică daune
            float distanceToPlayer = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            if (distanceToPlayer <= attackDistance)
            {
                DealDamage();
            }
        }

        private void DealDamage()
        {
            if (!playerDetected || isDead || playerPosition == null)
            {
                return;
            }

            IDamageable playerScript = playerPosition.GetComponent<IDamageable>();
            if (playerScript != null)
            {
                Vector2 knockbackDir = attackKnockbackDir.normalized;

                if (direction.x < 0)
                {
                    knockbackDir.x *= -1;
                }

                playerScript.TakeDamage(attackDamage, knockbackDir, attackKnockbackForce);
            }
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            if (isDead)
                return;

            health -= damage;

            // Animație de hurt
            animator.SetBool("isHurt", true);

            if (BloodSystem.Instance != null)
            {
                BloodSystem.Instance.OnEnemyHit(this.transform.position, knockbackDir, true, damage);
            }

            // Aplică knockback
            rb.linearVelocity = knockbackDir * knockbackForce;

            if (health <= 0)
            {
                isDead = true;
                animator.SetBool("isDead", true);
                
                rb.linearVelocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", false);
                
                DisableAllColliders();

                if (wallToDestroy != null)
                {
                    Destroy(wallToDestroy);
                }
                
                if (GateToBoss != null)
                {
                    GateToBoss.SetActive(false);
                }
                
                if (bossManager != null)
                {
                    bossManager.HideBossHealthBar();
                }
            }
        }

        private void DisableAllColliders()
        {
            Collider2D[] allColliders = GetComponents<Collider2D>();
            foreach (Collider2D coll in allColliders)
            {
                coll.enabled = false;
            }
            
            Collider2D[] allChildColliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D coll in allChildColliders)
            {
                coll.enabled = false;
            }
        }

        private void OnFinishedDeathAniEvent()
        {
            Destroy(gameObject);
        }

        public void OnFinishedHurtAniEvent()
        {
            animator.SetBool("isHurt", false);
        }

        public float GetHealthPercent() => (float)health / MAX_HEALTH;
        public string GetBossName() => "Belphegor";
    }
}