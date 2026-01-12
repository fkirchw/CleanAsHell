using System.Collections.Generic;
using Characters.Interfaces;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerCombat : MonoBehaviour, IDamageable
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int Dead = Animator.StringToHash("isDead");

        [Header("Components")]
        [SerializeField] private Animator animator;
        
        [Header("Health")]
        [SerializeField] private int health = 10;

        [Header("Light Attack")]
        [SerializeField] private int lightAttackPower = 5;
        [SerializeField] private float lightAttackDistance = 1f;
        [SerializeField] private float lightAttackRadius = 0.7f;
        [SerializeField] private float lightAttackKnockback = 5f;

        [Header("Heavy Attack")]
        [SerializeField] private int heavyAttackPower = 10;
        [SerializeField] private float heavyAttackDistance = 1.5f;
        [SerializeField] private float heavyAttackRadius = 1.0f;
        [SerializeField] private float heavyAttackKnockback = 8f;

        private PlayerMovement movement;
        private Collider2D playerCollider;
        private PlayerInputHandler input;
        private HashSet<IDamageable> hitThisAttack = new HashSet<IDamageable>();

        public bool IsDead { get; private set; }
        public bool IsAttacking { get; private set; }
        public float HealthPercent => health / 10f;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            playerCollider = GetComponent<Collider2D>();
            input = GetComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            if (IsDead)
            {
                playerCollider.enabled = false;
                return;
            }

            HandleCombatInput();
            UpdateAnimations();
        }

        private bool attackRequested = false;

        private void HandleCombatInput()
        {
            bool isInHitAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
            bool isInHeavySwipeAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("HeavySwipe");

            // Prevent new attacks while already attacking
            if (isInHitAnimation || isInHeavySwipeAnimation)
                return;

            if (input.HeavySweepPressed)
            {
                hitThisAttack.Clear();
                attackRequested = true; // SET FLAG
                animator.Play("HeavySwipe");
            }
            else if (input.AttackPressed)
            {
                hitThisAttack.Clear();
                attackRequested = true; // SET FLAG
                animator.Play("Hit");
            }
        }

        private void UpdateAnimations()
        {
            // Combat priority animations
            if (IsDead) return;

            // Update IsAttacking based on current animation state
            bool isInHitAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
            bool isInHeavySwipeAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("HeavySwipe");
            IsAttacking = isInHitAnimation || isInHeavySwipeAnimation || attackRequested; // CHECK FLAG TOO

            // Clear flag once we're actually in the animation
            if ((isInHitAnimation || isInHeavySwipeAnimation) && attackRequested)
            {
                attackRequested = false;
            }

            // Otherwise update movement animations
            animator.SetBool(IsRunning, movement.HorizontalInput != 0);

            if (!movement.IsGrounded && !IsAttacking)
            {
                animator.speed = 0f;
                if (movement.Velocity.y > 0.1f)
                    animator.Play("Run", 0, 0.5f);
                else if (movement.Velocity.y < -0.1f)
                    animator.Play("Run", 0, 0.625f);
            }
            else
            {
                animator.speed = 1f;
            }
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            health -= damage;
            animator.speed = 0f;
            movement.ApplyKnockback(knockbackDir, knockbackForce);

            if (health <= 0)
            {
                IsDead = true;
                animator.Play("Die");
                animator.SetBool(Dead, true);
            }
        }
        
        // Important: Now called by event in the hit animation, not by direct invocation in this file.
        // To change timing, adjust animation event marker in the hit animation.
        // See https://docs.unity3d.com/6000.3/Documentation/Manual/script-AnimationWindowEvent.html
        private void DealDamage()
        {
            PerformAttack(lightAttackPower, lightAttackDistance, lightAttackRadius, lightAttackKnockback);
        }

        // Called by animation event in the HeavySweep animation
        private void DealHeavyDamage()
        {
            PerformAttack(heavyAttackPower, heavyAttackDistance, heavyAttackRadius, heavyAttackKnockback);
        }

        private void PerformAttack(int attackPower, float attackDistance, float attackRadius, float knockbackForce)
        {
            Vector2 dir = movement.FacingDirection;
            Vector2 attackCenter = (Vector2)transform.position + (dir * attackDistance * 0.5f);
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRadius, LayerMask.GetMask("Enemy"));
            
            foreach (Collider2D hit in hits)
            {
                Vector2 toEnemy = hit.transform.position - transform.position;
                if (Vector2.Dot(toEnemy.normalized, dir) > 0.3f) // ~70° cone
                {
                    if (!hit.isTrigger && hit.TryGetComponent(out IDamageable damageable))
                    {
                        if (hitThisAttack.Add(damageable))
                        {
                            damageable.TakeDamage(attackPower, dir, knockbackForce);
                        }
                    }
                }
            }
        }

        private void OnFinishedDeathAniEvent()
        {
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Vector2 direction = movement ? movement.FacingDirection : Vector2.right;
            
            // Light attack gizmo (red)
            Vector2 lightAttackCenter = (Vector2)transform.position + direction * lightAttackDistance * 0.5f;
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(lightAttackCenter, lightAttackRadius);
            
            // Heavy attack gizmo (orange)
            Vector2 heavyAttackCenter = (Vector2)transform.position + direction * heavyAttackDistance * 0.5f;
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawSphere(heavyAttackCenter, heavyAttackRadius);
        }
    }
}