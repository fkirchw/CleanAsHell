using Blood;
using Characters.Interfaces;
using UnityEngine;

namespace Characters.Enemies
{
    public class FlyingDemonScript : MonoBehaviour, IDamageable
    {
        [SerializeField] private Animator animator;
        private Transform playerPosition;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Vector2 direction;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float attackDistance = 4f;
        [SerializeField] private int health = 10;
        [SerializeField] private int damage = 10;
        [SerializeField] private float knockbackForce = 15f;

        private bool isDead = false;
        private int maxHealth;

        private bool playerDetected = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            maxHealth = health;
        }

        // Update is called once per frame
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
        
        //TODO: (kfe) Am I thinking wrong or does this a radial check? So would it hit a player *behind* the attacker?
        // and if so, should we keep it? its a valid behaviour for a flying guy.
        private void DealDamage()
        {
            IDamageable playerScript = playerPosition.GetComponent<IDamageable>();
            if (playerScript != null)
            {
                Vector2 knockbackDir = new Vector2(3, 1).normalized;

                if (direction.x < 0)
                {
                    knockbackDir.x *= -1;
                }

                playerScript.TakeDamage(damage, knockbackDir, knockbackForce);
            }
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            health -= damage;

            animator.SetBool("isHurt", true);

            BloodSystem.Instance.OnEnemyHit(this.transform.position, knockbackDir, true, damage);

            rb.linearVelocity = knockbackDir * knockbackForce;

            if (health <= 0)
            {
                isDead = true;
                animator.Play("Death");
                animator.SetBool("isDead", true);

                //increase Mobs killed
                LevelStateManager.Instance.IncreaseEnemiesKilled();
            }
        }

        public int GetMaxHealth()
        {
            return maxHealth;
        }

        private void HandleMovement()
        {
            direction = (playerPosition.position - transform.position).normalized;

            if (direction.x < 0)
                spriteRenderer.flipX = false;
            else if (direction.x > 0)
                spriteRenderer.flipX = true;

            float moveX = moveSpeed * direction.x;
            float movey = moveSpeed * direction.y;

            rb.linearVelocity = new Vector2(moveX, movey);

            float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            animator.SetBool("isAttacking", distanceToPlayer < attackDistance);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerDetected = true;
                playerPosition = collision.transform;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerDetected = false;
                rb.linearVelocity = Vector2.zero;
            }
        }

        public void OnFinishedDeathAniEvent()
        {
            Destroy(gameObject);
        }

        public void OnFinishedHurtAniEvent()
        {
            animator.SetBool("isHurt", false);
        }

        public void OnDamageDeltAniEvent()
        {
            animator.SetBool("isAttacking", false);

            float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            if (distanceToPlayer <= attackDistance)
            {
                DealDamage();
            }
            
        }
    }
}