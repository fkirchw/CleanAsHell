using Blood;
using Characters.Interfaces;
using Characters.Player;
using System.Collections;
using GameManager;
using UnityEngine;
namespace Characters.Enemies
{
    public class CleaningDemonScript : MonoBehaviour, IDamageable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackDistance = 3f;
        [SerializeField] private int health = 10;
        [SerializeField] private int damage = 5;
        [SerializeField] private Vector2 knockbackDir = new Vector2(1, 1);
        [SerializeField] private float knockbackForce = 2f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private PlayerData playerData;

        private Transform playerPosition;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Vector2 direction;

        private bool isDead;

        private bool playerDetected;

        private bool isInAttack = false;

        private bool canAttack = true;
        private int maxHealth;


        [Header("SoundClips")]
        [SerializeField] private AudioClip cleaningDemonAttackClip;


        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            if(playerData == null )
            {
                return;
            }
            maxHealth =  health;
           
        }

        void Update()
        {
            if(playerData.IsDead)
            {
                animator.SetBool("isIdle", true);
                return;
                
            }

            if (isDead)
            {
                return;
            }

            if (playerDetected && !isInAttack)
            {
                HandleMovement();
                HandleAttack();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerDetected = true;
                playerPosition = collision.transform;
                animator.SetBool("isRunning", true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerDetected = false;
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isRunning", false);
            }
        }

        //Called by Animation Event
        public void OnDamageDelt()
        {
            DealDamage();
            isInAttack = false;
        }

        private void DealDamage()
        {
            if (!playerDetected)
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            if (distanceToPlayer < attackDistance)
            {
                IDamageable playerScript = playerPosition.GetComponent<IDamageable>();
                if (playerScript != null)
                {

                    if (direction.x < 0)
                    {
                        knockbackDir.x *= -1;
                    }

                    playerScript.TakeDamage(damage, knockbackDir, knockbackForce);
                }
            }
        }

        private void HandleAttack()
        {

            if (!canAttack)
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));

            if (distanceToPlayer < attackDistance)
            {
                isInAttack = true;
                animator.SetTrigger("hitTrigger");
                StartCoroutine(AttackRoutine());
                StartCoroutine(AttackCooldown());
                
                //Attack Sound
                SoundManager.instance.PlaySoundFxClip(cleaningDemonAttackClip, transform, 0.5f);
            }
        }

        private void HandleMovement()
        {
            direction = (playerPosition.position - transform.position).normalized;

            Vector2 myDirection = Vector2.zero;

            if (direction.x < 0) 
            { 
                transform.rotation = Quaternion.Euler(0, 180, 0);
                myDirection = Vector2.left;
            }
            else if (direction.x > 0) 
            {
                transform.rotation = Quaternion.identity;
                myDirection = Vector2.right;
                
            }

            float moveX = moveSpeed * direction.x;

            bool isGround = isGroundInMoveDir(myDirection);
            // Rigidbody bewegen
            if (isGround)
                rb.linearVelocity = new Vector2(moveX, rb.linearVelocity.y);
            else
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            animator.SetBool("isRunning", isGround);

        }

        private bool isGroundInMoveDir(Vector2 moveDir)
        {
            float distanceDown = 2f;
            float forwardOffset = 0.5f;

            // startingpoint straight ahead of player
            Vector2 origin = (Vector2)transform.position + moveDir * forwardOffset;

            // cast ray straight down
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distanceDown, LayerMask.GetMask("Ground"));

            Debug.DrawRay(origin, Vector2.down * distanceDown, Color.red);

            return hit.collider != null;
            
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

                //increase Mobs killed
                LevelStateManager.Instance.IncreaseEnemiesKilled();
            }
        }

        public int GetMaxHealth()
        {
            return maxHealth;
        }

        private void OnFinishedDeathAniEvent()
        {
            Destroy(gameObject);
        }

        private IEnumerator AttackRoutine()
        {
            yield return new WaitForSeconds(0.4f);
            OnDamageDelt();
        }

        public IEnumerator AttackCooldown()
        {
            canAttack = false;

            yield return new WaitForSeconds(attackCooldown);

            canAttack = true;
        }

    }
}

