using System;
using Characters.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Characters.Player
{
    public class PlayerCombat : MonoBehaviour, IDamageable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private int health = 10;

        [FormerlySerializedAs("damage")] [SerializeField]
        private int attackPower = 5;

        [SerializeField] private float attackDistance = 1f;

        private PlayerMovement movement;
        private Collider2D playerCollider;
        private PlayerInputHandler input;

        public bool IsDead { get; private set; }
        public bool IsAttacking { get; private set; }
        public float HealthPercent => health / 10f;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            playerCollider = GetComponent<Collider2D>();
            input =  GetComponent<PlayerInputHandler>();
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

        private void HandleCombatInput()
        {
            bool isInHitAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");

            if (input.AttackPressed && !isInHitAnimation)
            {
                animator.Play("Hit");
                DealDamage();
            }
        }

        private void UpdateAnimations()
        {
            // Combat priority animations
            if (IsDead) return;

            // Otherwise update movement animations
            animator.SetBool("isRunning", movement.HorizontalInput != 0);

            if (!movement.IsGrounded)
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
                animator.SetBool("isDead", true);
            }
        }

        private void DealDamage()
        {
            Vector2 dir = movement.FacingDirection;
            Vector2 origin = (Vector2)transform.position + (dir * 0.5f);

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, attackDistance, LayerMask.GetMask("Enemy"));
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider && !hit.collider.isTrigger && hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(attackPower, dir, 0f);
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
            Vector2 origin = (Vector2)transform.position + direction * 0.5f;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + direction * attackDistance);
            Gizmos.DrawWireSphere(origin + direction * attackDistance, 0.2f);
        }
    }
}