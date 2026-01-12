using Blood;
using Characters.Interfaces;
using UnityEngine;
using System.Collections;
using Characters.Player;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
namespace Characters.Enemies
{
    public class LuciferController : MonoBehaviour, IDamageable
    {

        [SerializeField] private Animator animator;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int health = 10;
        [SerializeField] private PlayerData playerData;

        private Transform playerPosition;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Vector2 direction;

        private float attackCooldown;

        private bool isDead;

        private bool playerDetected;

        private float selectedAttackDistance;

        private bool isInAttack = false;

        private List<IAttack> attacks;

        private float playerOffset = -1.05f;

        private float MAX_HEALTH;

        void Start()
        {
            MAX_HEALTH = health;

            spriteRenderer = GetComponent<SpriteRenderer>();
           
            rb = GetComponent<Rigidbody2D>();

            if (playerData == null)
            {
                return;
            }

            attacks = new List<IAttack>();
            AttackTongue tongueAttack = GetComponent<AttackTongue>();
            AttackTrident attackTrident = GetComponent<AttackTrident>();
            AttackSweep attackSweep = GetComponent<AttackSweep>();

            attacks.Add(tongueAttack);
            attacks.Add(attackTrident);
            attacks.Add(attackSweep);
            //Sort ASC
            attacks.Sort((a, b) =>
            {
                return a.GetAttackDistance().CompareTo(b.GetAttackDistance());
            });

        }

        void Update()
        {

            if(playerData.IsDead) {
                ResetAllParameters();
                rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
                animator.SetBool("isIdle", true);
                return; 
            }

            if (isInAttack)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerDetected = false;
                rb.linearVelocity = Vector2.zero;
                ResetAllParameters();
            }
        }

        //Called by Animation Event
        public void OnDamageDelt(IAttack attack)
        {

            DealDamage(attack);
        }

        public void OnAttackAnimationOver()
        {
            isInAttack = false;
            
        }

        private void DealDamage(IAttack attack)
        {

            float deltaX = Mathf.Abs(playerPosition.position.x - transform.position.x);
            float deltaY = Mathf.Abs(transform.position.y + playerOffset - playerPosition.position.y);


            if (attack.GetDamageRange().x >= deltaX && attack.GetDamageRange().y >= deltaY)
            {

                IDamageable playerScript = playerPosition.GetComponent<IDamageable>();
                if (playerScript != null)
                {
                    Vector2 knockbackDir = attack.GetKnockbackDir();

                    if (direction.x < 0)
                    {
                        knockbackDir.x *= -1;
                    }

                    playerScript.TakeDamage(attack.GetDamage(), knockbackDir, 6f);
                }
            }
        }

        private void HandleMovement()
        {
            animator.SetBool("isWalking", true);


            direction = (playerPosition.position - transform.position).normalized;

            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;

            float moveX = moveSpeed * direction.x;

            // Rigidbody bewegen
            rb.linearVelocity = new Vector2(moveX, 0);

        }

        private void HandleAttack()
        {
            IAttack myNextAttack = ChooseAttack();

            if (myNextAttack==null)
            {
                return;
            }


            ResetAllParameters();
            isInAttack = true;
            StartCoroutine(myNextAttack.AttackCooldown());
            StartCoroutine(BringInAttackState(myNextAttack));
            
            //isWalking = false;
        }

        private IAttack ChooseAttack()
        {
            float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                new Vector2(playerPosition.position.x, playerPosition.position.y));


            foreach (IAttack possibleAttack in attacks)
            {

               if(distanceToPlayer < possibleAttack.GetAttackDistance() && possibleAttack.GetCanAttack() )
                {
                    return possibleAttack;
                    
                }

            }

            return null;

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
                rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
                isDead = true;
                animator.SetTrigger("DieTrigger");
            }
        }

        private void OnFinishedDeathAniEvent()
        {
            Destroy(gameObject);
            SceneManager.LoadScene("Outro");
        }

        void ResetAllParameters()
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                    animator.SetBool(param.name, false);
            }
        }
        
        IEnumerator BringInAttackState(IAttack attack)
        {
            
            string triggerName = attack.GetAttackName() + "Trigger";


            animator.SetTrigger(triggerName);
           

            // Warten bis Animator im State "TongueShoot" ist
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(attack.GetAttackName()))
            {
                
                yield return null; // Warte 1 Frame
            }

            // Hier Angriff ausführen (Collider aktivieren, IK bewegen etc.)
            attack.Attack(); // optional kurze Attacke
            
        }

        public float GetHealthPercent() =>(float)health / MAX_HEALTH;


    }
}

