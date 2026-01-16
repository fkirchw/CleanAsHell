using Blood;
using Characters.Interfaces;
using UnityEngine;

namespace Characters.Enemies
{
    public class SmallBoss : MonoBehaviour, IDamageable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackDistance = 3f;
        [SerializeField] private int health = 10;
        [SerializeField] private GameObject wallToDestroy;
        [SerializeField] private GameObject GateToBoss;
        [SerializeField] private float attackCooldown = 2f; 
        [SerializeField] private float attackDuration = 1f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float contactDamageCooldown = 0.5f;
        [SerializeField] private Vector2 contactKnockbackDir = new Vector2(1, 1);
        [SerializeField] private float contactKnockbackForce = 3f;
        [SerializeField] private BossManager bossManager;

        private Transform playerPosition;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Vector2 direction;
        private bool isDead;
        private bool playerDetected;
        private bool gateActivated = false;
        private bool isAttacking = false; 
        private float attackTimer = 0f;
        private float attackDurationTimer = 0f;
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

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }

            if (isAttacking)
            {
                attackDurationTimer -= Time.deltaTime;
                
                rb.linearVelocity = Vector2.zero;
                
                if (attackDurationTimer <= 0)
                {
                    EndAttack();
                }
                
                return;
            }

            if (playerDetected && playerPosition)
            {
                HandleMovement();
                CheckForAttack();
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

                if (isAttacking)
                {
                    EndAttack();
                }
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

        public void OnDamageDeltAniEvent()
        {
            if (isDead) return;
            DealDamage();
        }

        private void DealDamage()
        {
            if (!playerDetected || isDead)
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(new Vector2(rb.position.x, rb.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            if (distanceToPlayer < attackDistance)
            {
                IDamageable playerScript = playerPosition.GetComponent<IDamageable>();
                if (playerScript != null)
                {
                    Vector2 knockbackDir = new Vector2(1, 1).normalized;

                    if (direction.x < 0)
                    {
                        knockbackDir.x *= -1;
                    }

                    playerScript.TakeDamage(5, knockbackDir, 6f);
                }
            }
        }

        private void CheckForAttack()
        {
            if (isAttacking || attackTimer > 0 || !playerDetected || isDead)
                return;

            float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            if (distanceToPlayer < attackDistance)
            {
                StartAttack();
            }
        }

        private void StartAttack()
        {
            isAttacking = true;
            attackDurationTimer = attackDuration;
            
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", true);
        }

        private void EndAttack()
        {
            isAttacking = false;
            attackTimer = attackCooldown;
            animator.SetBool("isAttacking", false);
            
            if (playerDetected && !isDead)
            {
                animator.SetBool("isWalking", true);
            }
        }

        private void HandleMovement()
        {
            if (isAttacking || isDead)
                return;

            direction = (playerPosition.position - transform.position).normalized;

            if (direction.x < 0)
                spriteRenderer.flipX = false;
            else if (direction.x > 0)
                spriteRenderer.flipX = true;

            float moveX = moveSpeed * direction.x;

            rb.linearVelocity = new Vector2(moveX, 0);
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            if (isDead)
                return;

            health -= damage;

            if (BloodSystem.Instance)
            {
                BloodSystem.Instance.OnEnemyHit(this.transform.position, knockbackDir, false, damage);
            }

            if (health <= 0)
            {
                isDead = true;
                animator.SetBool("isDead", true);
                
                rb.linearVelocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                isAttacking = false;
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

        public float GetHealthPercent() => (float)health / MAX_HEALTH;
        public string GetBossName() => "Belphegor";
    }
}