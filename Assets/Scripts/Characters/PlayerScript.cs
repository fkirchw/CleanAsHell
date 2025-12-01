using System;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour, IDamageable
{
    [Header("Cleaning Settings")] 
    [SerializeField] private float cleanRadius = 2f;
    [SerializeField] private float cleanRate = 0.5f; // Blood cleaned per second

    [Header("Foam Visual Feedback")] 
    [SerializeField] private ParticleSystem foamParticles; // Foam particle system
    [SerializeField] private LayerMask groundLayer; // Layer for ground detection (set to "Ground")
    [SerializeField] private float groundCheckDistance = 5f;
    [SerializeField] private int foamParticlesPerSecond = 20;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    
    [Header("Combat Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private int health = 10;
    [SerializeField] private float attackDistance = 1;
    private int damage = 5;

    private new Rigidbody2D rigidbody2D; // rigidbody2D was a property in MonoBehaviour but is deprecated as of now.
                                         // Since it is useful here we reinitialize (replace) it with our own prop.
    private new Collider2D collider2D; // same as above.
    private bool isGrounded = true; // Check if player is grounded
    private bool isKnockback;
    private bool isDead;
    private bool isCleaning;
    private float horizontalPressed;
    private float foamParticleTimer;
    private Vector2 facingDirection = Vector2.right;

    private void Start()
    {
        collider2D =  GetComponent<Collider2D>();
    }

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead)
        {
            collider2D.enabled = false;
            return;
        }


        if (isKnockback)
        {
            return;
        }

        // TODO: Sweeping;
        // TODO: Broom should only listen to collisions during hit animation -> should probably be script of broom
        HandleMovement();
    }

    // Check if player has touched down
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO: only count collisions from below

        // check if ground 
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        isGrounded = true; // player has landed and can jump again
        animator.speed = 1f;
        isKnockback = false;
    }

    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        health -= damage;

        // stop animation
        animator.speed = 0f;

        // apply knockback
        rigidbody2D.linearVelocity = knockbackDir * knockbackForce;

        // activate knockback timer
        isKnockback = true;

        if (health <= 0)
        {
            isDead = true;
            animator.Play("Die");
            animator.SetBool("isDead", true);
        }
    }
    
    private void HandleMovement()
    {
        bool fire1Pressed = Input.GetButtonDown("Fire1");
        bool fire2Down = Input.GetButton("Fire2");
        bool isHitting = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
        horizontalPressed = Input.GetAxisRaw("Horizontal");

        // horizontal movement
        rigidbody2D.linearVelocity = new Vector2(horizontalPressed * moveSpeed, rigidbody2D.linearVelocity.y);


        // Filp direction
        if (horizontalPressed < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            facingDirection = Vector2.left;
        }
        else if (horizontalPressed > 0)
        {
            transform.rotation = Quaternion.identity;
            facingDirection = Vector2.right;
        }

        // set animation

        animator.SetBool("isRunning", horizontalPressed != 0);

        if (fire1Pressed && !isHitting)
        {
            animator.Play("Hit");
            DealDamage(attackDistance);
        }

        // Check if player is holding clean button
        if (fire2Down && !isHitting)
        {
            if (!isCleaning)
            {
                StartCleaning();
            }

            PerformCleaning();
            SpawnGroundFoam();
        }
        else if (isCleaning)
        {
            StopCleaning();
        }

        // jump
        if (Input.GetButton("Jump") && isGrounded)
        {
            rigidbody2D.linearVelocity = new Vector2(rigidbody2D.linearVelocity.x, jumpForce);
            isGrounded = false; // prevent double jumps
            animator.speed = 0f;
        }
    }

    public void DealDamage(float attackDistance)
    {
        Vector2 dir = facingDirection;
        
        Vector2 origin = (Vector2)transform.position +
                         (dir) * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            dir,
            attackDistance,
            LayerMask.GetMask("Enemy")
        );

        if (!hit.collider)
            return;

        float distance = Vector2.Distance(transform.position, hit.transform.position);

        if (distance <= attackDistance && hit.collider.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage, facingDirection, 0f);
        }
    }

    void StartCleaning()
    {
        isCleaning = true;
        foamParticleTimer = 0f; // Reset timer when starting
    }

    void StopCleaning()
    {
        isCleaning = false;
    }

    void PerformCleaning()
    {
        if (!BloodSystem.Instance)
        {
            return;
        }
        
        // Clean blood around player position
        BloodSystem.Instance.CleanBlood(
            transform.position,
            cleanRadius,
            cleanRate * Time.deltaTime
        );
    }

    void SpawnGroundFoam()
    {
        if (!foamParticles)
        {
            return;
        }
        
        foamParticleTimer += Time.deltaTime;
        float interval = 1f / foamParticlesPerSecond;
        
        // Spawn multiple particles if enough time has passed
        while (foamParticleTimer >= interval)
        {
            foamParticleTimer -= interval;
            EmitFoamParticle();
        }
    }

    void EmitFoamParticle()
    {
        // Random position within clean radius around player
        Vector2 randomOffset = Random.insideUnitCircle * cleanRadius;
        Vector2 spawnPosition = (Vector2)transform.position + randomOffset;
        
        
        // Raycast down from above to find ground
        RaycastHit2D hit = Physics2D.Raycast(
            spawnPosition + Vector2.up * groundCheckDistance,
            Vector2.down,
            groundCheckDistance * 2f,
            groundLayer
        );
        
        if (hit.collider)
        {
            // Spawn particle at ground level
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = hit.point + Vector2.up * 0.1f; // Slightly above ground
            
            // Random horizontal velocity for spreading effect
            Vector2 horizontalVel = Random.insideUnitCircle * 0.5f;
            emitParams.velocity = new Vector3(horizontalVel.x, 0, 0);
            
            // Random size variation
            emitParams.startSize = Random.Range(0.2f, 0.5f);
            
            // Random rotation for variety
            emitParams.rotation = Random.Range(0f, 360f);
            
            // Random lifetime variation
            emitParams.startLifetime = Random.Range(1.5f, 2.5f);
            
            foamParticles.Emit(emitParams, 1);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw cleaning radius in editor
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, cleanRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cleanRadius);
    
        // Draw attack range
        Vector2 direction = facingDirection;
        Vector2 origin = (Vector2)transform.position + direction * 0.5f;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + direction * attackDistance);
    
        // Optional: Draw a sphere at the attack endpoint for clarity
        Gizmos.DrawWireSphere(origin + direction * attackDistance, 0.2f);
    }
    
    private void OnFinishedDeathAniEvent()
    {
        //To Do 
    }
}