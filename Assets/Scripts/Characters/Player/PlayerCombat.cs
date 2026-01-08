using System;
using System.Collections.Generic;
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
        private HashSet<IDamageable> hitThisAttack = new HashSet<IDamageable>();

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
                hitThisAttack.Clear();
                animator.Play("Hit");
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
        
        // Important: Now called by event in the hit animation, not by direct invocation in this file.
        // To change timing, adjust animation event marker in the hit animation.
        // See https://docs.unity3d.com/6000.3/Documentation/Manual/script-AnimationWindowEvent.html
        private void DealDamage()
        {
            Vector2 dir = movement.FacingDirection;
            Vector2 attackCenter = (Vector2)transform.position + (dir * attackDistance * 0.5f);
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, 0.7f, LayerMask.GetMask("Enemy"));
            foreach (Collider2D hit in hits)
            {
                Vector2 toEnemy = hit.transform.position - transform.position;
                if (Vector2.Dot(toEnemy.normalized, dir) > 0.3f) // ~70° cone
                {
                    if (!hit.isTrigger && hit.TryGetComponent(out IDamageable damageable))
                    {
                        if (hitThisAttack.Add(damageable))
                        {
                            damageable.TakeDamage(attackPower, dir, 5f);
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
            Vector2 attackCenter = (Vector2)transform.position + direction * attackDistance * 0.5f;
    
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(attackCenter, 0.7f);
        }
    }
}