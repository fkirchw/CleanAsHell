using System;
using System.Collections;
using System.Collections.Generic;
using Blood;
using Characters.Interfaces;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerCombat : MonoBehaviour, IDamageable
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int Dead = Animator.StringToHash("isDead");

        [Header("Components")] [SerializeField]
        private Animator animator;

        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private int health = 10;

        [Header("Light Attack")] [SerializeField]
        private int lightAttackPower = 5;

        [SerializeField] private float lightAttackDistance = 1f;
        [SerializeField] private float lightAttackRadius = 0.7f;
        [SerializeField] private float lightAttackKnockback = 5f;

        [Header("Heavy Attack")] [SerializeField]
        private int heavyAttackPower = 10;

        [SerializeField] private float heavyAttackDistance = 1.5f;
        [SerializeField] private float heavyAttackRadius = 1.0f;
        [SerializeField] private float heavyAttackKnockback = 8f;

        [Header("Invincibility")] [SerializeField]
        private float invincibilityDuration = 2f;

        [SerializeField] private bool enableFlashEffect = true;
        [SerializeField] private float flashInterval = 0.1f;

        private PlayerMovement movement;
        private Collider2D playerCollider;
        private PlayerInputHandler input;
        private HashSet<IDamageable> hitThisAttack = new HashSet<IDamageable>();
        private bool isInvincible = false;
        private Coroutine invincibilityCoroutine;

        public bool IsDead { get; private set; }
        public bool IsAttacking { get; private set; }
        public float HealthPercent => (float)health / GetMaxHealth();
        public int Health => health;

        private int maxHealth;

        private int regenerationCount = 0;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            playerCollider = GetComponent<Collider2D>();
            input = GetComponent<PlayerInputHandler>();

            if (!(spriteRenderer && TryGetComponent(out spriteRenderer)))
            {
                Debug.LogError("No sprite renderer found on " + gameObject.name);
            }

            UpdateMaxHealth();
        }

        private void UpdateMaxHealth()
        {
            if (LevelStateManager.Instance != null)
            {
                maxHealth = 10 + (int)LevelStateManager.Instance.GetVitalityHealthBonus();
            }
            else
            {
                maxHealth = health;
            }
        }

        private void Start()
        {
            UpdateMaxHealth();
            
            health = LevelStateManager.Instance.GetPlayerHealth();
            
            if (health > maxHealth)
            {
                health = maxHealth;
                LevelStateManager.Instance.SetPlayerHealth(health);
            }
            
            int lightBonus = LevelStateManager.Instance.GetLightAttackBonus();
            int heavyBonus = LevelStateManager.Instance.GetHeavyAttackBonus();
            int vitBonus = (int)LevelStateManager.Instance.GetVitalityHealthBonus();
            
            Debug.Log($"PlayerCombat Start - Health: {health}/{maxHealth}, LightBonus: {lightBonus}, HeavyBonus: {heavyBonus}, VitBonus: {vitBonus}");
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

        private void OnRegenerationHandler(float regeneratedHealth)
        {
            regenerationCount++;
            IncreaseHealth(Mathf.RoundToInt(regeneratedHealth));
        }

        private bool attackRequested = false;

        private void HandleCombatInput()
        {
            bool isInHitAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
            bool isInHeavySwipeAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("HeavySwipe");

            if (isInHitAnimation || isInHeavySwipeAnimation)
                return;

            if (input.HeavySweepPressed)
            {
                hitThisAttack.Clear();
                attackRequested = true;
                animator.Play("HeavySwipe");
            }
            else if (input.AttackPressed)
            {
                hitThisAttack.Clear();
                attackRequested = true;
                animator.Play("Hit");
            }
        }

        private void UpdateAnimations()
        {
            if (IsDead) return;

            bool isInHitAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
            bool isInHeavySwipeAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("HeavySwipe");
            IsAttacking = isInHitAnimation || isInHeavySwipeAnimation || attackRequested;

            if ((isInHitAnimation || isInHeavySwipeAnimation) && attackRequested)
            {
                attackRequested = false;
            }

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
            if (isInvincible) return;

            DecreaseHealth(damage);

            animator.speed = 0f;
            movement.ApplyKnockback(knockbackDir, knockbackForce);

            BloodSystem.Instance.OnEnemyHit(this.transform.position, knockbackDir, false, damage);

            if (health <= 0)
            {
                IsDead = true;
                animator.Play("Die");
                animator.SetBool(Dead, true);

                LevelStateManager.Instance.ResetStats();
                LevelStateManager.Instance.IncreaseDeathCounter();
            }
            else
            {
                if (invincibilityCoroutine != null)
                {
                    StopCoroutine(invincibilityCoroutine);
                }

                invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine());
            }
        }

        public int GetMaxHealth()
        {
            UpdateMaxHealth();
            return maxHealth;
        }

        private IEnumerator InvincibilityCoroutine()
        {
            isInvincible = true;

            if (enableFlashEffect && spriteRenderer != null)
            {
                float elapsed = 0f;
                Color originalColor = spriteRenderer.color;

                while (elapsed < invincibilityDuration)
                {
                    spriteRenderer.color = new Color(
                        originalColor.r,
                        originalColor.g,
                        originalColor.b,
                        spriteRenderer.color.a > 0.75f ? 0.3f : 1f
                    );

                    yield return new WaitForSeconds(flashInterval);
                    elapsed += flashInterval;
                }

                spriteRenderer.color = originalColor;
            }
            else
            {
                yield return new WaitForSeconds(invincibilityDuration);
            }

            isInvincible = false;
            invincibilityCoroutine = null;
        }

        private void DealDamage()
        {
            int attackBonus = LevelStateManager.Instance.GetLightAttackBonus();
            int attackPower = lightAttackPower + attackBonus;
            
            Debug.Log($"Light Attack - Base: {lightAttackPower}, Bonus: {attackBonus}, Final: {attackPower}");
            
            PerformAttack(attackPower, lightAttackDistance, lightAttackRadius, lightAttackKnockback);
        }

        private void DealHeavyDamage()
        {
            int attackBonus = LevelStateManager.Instance.GetHeavyAttackBonus();
            int attackPower = heavyAttackPower + attackBonus;
            
            Debug.Log($"Heavy Attack - Base: {heavyAttackPower}, Bonus: {attackBonus}, Final: {attackPower}");
            
            PerformAttack(attackPower, heavyAttackDistance, heavyAttackRadius, heavyAttackKnockback);
        }

        private void PerformAttack(int attackPower, float attackDistance, float attackRadius, float knockbackForce)
        {
            Debug.Log($"PerformAttack with power: {attackPower}");
            
            Vector2 dir = movement.FacingDirection;
            Vector2 attackCenter = (Vector2)transform.position + (dir * attackDistance * 0.5f);
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRadius, LayerMask.GetMask("Enemy"));

            Debug.Log($"Found {hits.Length} enemies in attack area");

            foreach (Collider2D hit in hits)
            {
                Vector2 closestPoint = hit.ClosestPoint(transform.position);

                Vector2 toEnemy = closestPoint - (Vector2)transform.position;

                if (Vector2.Dot(toEnemy.normalized, dir) > 0.3f)
                {
                    if (!hit.isTrigger && hit.TryGetComponent(out IDamageable damageable))
                    {
                        if (hitThisAttack.Add(damageable))
                        {
                            Debug.Log($"Dealing {attackPower} damage to enemy");
                            damageable.TakeDamage(attackPower, dir, knockbackForce);
                        }
                    }
                }
            }
        }

        private void DecreaseHealth(int damage)
        {
            health -= damage;
            LevelStateManager.Instance.SetPlayerHealth(health);
        }

        private void IncreaseHealth(int healthPoints)
        {
            health += healthPoints;
            health = Mathf.Min(health, GetMaxHealth());
            LevelStateManager.Instance.SetPlayerHealth(health);
        }

        private void OnFinishedDeathAniEvent()
        {
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Vector2 direction = movement ? movement.FacingDirection : Vector2.right;

            Vector2 lightAttackCenter = (Vector2)transform.position + direction * lightAttackDistance * 0.5f;
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(lightAttackCenter, lightAttackRadius);

            Vector2 heavyAttackCenter = (Vector2)transform.position + direction * heavyAttackDistance * 0.5f;
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawSphere(heavyAttackCenter, heavyAttackRadius);
        }

        private void OnEnable()
        {
            GameEvents.OnRegenerationEvent += OnRegenerationHandler;
        }

        private void OnDisable()
        {
            GameEvents.OnRegenerationEvent -= OnRegenerationHandler;
        }
    }
}