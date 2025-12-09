using Interfaces;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour, IDamageable
{
    [Header("Cleaning Settings")] [SerializeField]
    private float cleanRadius = 2f;

    [SerializeField] private float cleanRate = 0.5f;

    [Header("Ground-Based Cleaning")] [SerializeField]
    private bool useGroundBasedCleaning = true;

    [SerializeField] private float maxCleaningHeight = 2f;
    [SerializeField] private float verticalTolerance = 0.3f;
    [SerializeField] private bool useMultiRaycast = true; // NEW: Use multiple raycasts
    [SerializeField] private int raycastCount = 5; // NEW: Number of raycasts to use
    [SerializeField] private float raycastSpread = 1f; // NEW: How far apart the raycasts are

    [Header("Foam Visual Feedback")] [SerializeField]
    private ParticleSystem foamParticles;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 5f;
    [SerializeField] private int foamParticlesPerSecond = 20;

    [Header("Movement Settings")] [SerializeField]
    private float moveSpeed = 5f;

    [Header("Jump Settings")] [SerializeField]
    private float jumpForce = 10f;
    [SerializeField] private float jumpCutMultiplier = 0.5f; // How much to cut velocity by (0.5 = cut to 50%)
    [SerializeField] private float minJumpVelocity = 2f; // Minimum upward velocity before we allow cutting


    [Header("Combat Settings")] 
    [SerializeField] private Animator animator;
    [SerializeField] private int health = 10;
    [SerializeField] private float attackDistance = 1;
    private int damage = 5;

    private Rigidbody2D rigidbody2D;
    private Collider2D collider2D;

    public bool isGrounded = true;
    public bool isCleaning;
    public Vector2 facingDirection = Vector2.right;

    private bool isKnockback;
    private bool isDead;
    
    // Ground-based cleaning cache
    //private float groundY;
    public bool hasGroundHeight;

    // Debugging
    
    
    private Vector2[] raycastPositions; // For debug visualization


    //Composite classes
    private PlayerMovement playerMovement;
    private PlayerCleaning playerCleaning;

    // region basic
    private void Start()
    {
        
        if (BloodSystem.Instance == null)
        {
            Debug.LogError("BloodSystem.Instance is null! Make sure BloodSystem is in the scene.");
        }
        else
        {
            Debug.Log("BloodSystem found and ready.");
        }
    }

    private void Awake()
    {
        collider2D = GetComponent<Collider2D>();

        rigidbody2D = GetComponent<Rigidbody2D>();

        playerMovement = new PlayerMovement(this);
        playerCleaning = new PlayerCleaning(this);
    }

    private void Update()
    {
       if (health <= 0) {
            isDead = true;
            collider2D.enabled = false;
            return;
        }

        if (isKnockback)
        {
            return;
        }

        playerMovement.Update();
        playerCleaning.Update();
    }

 
    // Check if player has touched down
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        // Check if collision is from below (landing on ground)
        bool landedOnGround = playerMovement.IsLandingCollision(collision);

        if (landedOnGround)
        {
            isGrounded = true;
            animator.speed = 1f;
            isKnockback = false;
        }
        // If hitting from side or below, don't set grounded
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        // When leaving any ground collision, verify if we're still on ground
        // Use a small raycast downward from the bottom of our collider
        float checkDistance = 0.2f;
        Vector2 rayOrigin = new Vector2(transform.position.x, collider2D.bounds.min.y);

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            checkDistance,
            groundLayer
        );

        // Only unground if we're not touching ground below us
        if (hit.collider == null)
        {
            isGrounded = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        // Continuously check if we're landing/standing on ground
        if (playerMovement.IsLandingCollision(collision))
        {
            isGrounded = true;
            animator.speed = 1f;
        }
    }
    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {

        health -= damage;
        animator.speed = 0f;
        //rigidbody2D.linearVelocity = knockbackDir * knockbackForce;


        rigidbody2D.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);


        isKnockback = true;

        //StartCoroutine(KnockbackRecovery(0.5f));

    }

    public void DealDamage(float attackDistance)
    {
        Vector2 dir = facingDirection;
        Vector2 origin = (Vector2)transform.position + (dir * 0.5f);

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

    //Debug

    void OnDrawGizmosSelected()
    {
        playerCleaning.DebugCleaning();

        playerMovement.DebugCombat();

    }

    /* TODO
    private void OnFinishedDeathAniEvent()
    {
       Destroy(gameObject);
    }*/

    public bool DeathStatus()
    {
        return isDead;
    }

    //endregion

    //Getters

    public bool IsCleaning() => isCleaning;
    //public float GetGroundY() => groundY;
    public bool HasGroundHeight() => hasGroundHeight;
    
    public float GetHealthPercent() => health/10f;

    public float GetMoveSpeed() => moveSpeed;
    public Rigidbody2D GetRb() => rigidbody2D;
    public Animator GetPlayerAnimator() => animator;
    public float GetJumpForce() => jumpForce;
    public float GetMinJumpVelocity() => minJumpVelocity;
    public float GetJumpMultiplier() => jumpCutMultiplier;

    public float GetAttackDistance() => attackDistance;

    public ParticleSystem GetFoamParticles() => foamParticles;

    public bool GetUseGroundBasedCleaining() => useGroundBasedCleaning;

    public bool GetUseMultiRayCast() => useMultiRaycast;

    public float GetMaxCleaningHeight() => maxCleaningHeight;

    public LayerMask GetGroundLayer() => groundLayer;

    public int GetRayCastCount() => raycastCount; 
    //endregion
    public float GetRayCastSpread() => raycastSpread;

    public float GetCleanRadius() => cleanRadius;

    public float GetCleanRate() => cleanRate;

    public float GetVerticalTolerance() => verticalTolerance;

    public int GetFoamParitclesPerSeconde() => foamParticlesPerSecond;

    public float GetGroundCheckDistance() => groundCheckDistance;
}