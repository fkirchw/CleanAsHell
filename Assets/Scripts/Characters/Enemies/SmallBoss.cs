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

        private Transform playerPosition;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Vector2 direction;

        private bool isDead;

        private bool playerDetected;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (isDead)
            {
                return;
            }

            if (playerDetected && playerPosition)
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

        //Called by Animation Event
        public void OnDamageDeltAniEvent()
        {
            DealDamage();
        }

        private void DealDamage()
        {
            if (!playerDetected)
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

        private void HandleMovement()
        {
            direction = (playerPosition.position - transform.position).normalized;

            if (direction.x < 0)
                spriteRenderer.flipX = false;
            else if (direction.x > 0)
                spriteRenderer.flipX = true;

            float moveX = moveSpeed * direction.x;

            // Rigidbody bewegen
            rb.linearVelocity = new Vector2(moveX, 0);

            float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            animator.SetBool("isAttacking", distanceToPlayer < attackDistance);
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            health -= damage;

            // Add blood
            if (BloodSystem.Instance)
            {
                BloodSystem.Instance.OnEnemyHit(this.transform.position, knockbackDir, false, damage);
            }

            if (health <= 0)
            {
                isDead = true;
                animator.SetBool("isDead", true);
            }
        }

        private void OnFinishedDeathAniEvent()
        {
            Destroy(gameObject);
        }
    }
}