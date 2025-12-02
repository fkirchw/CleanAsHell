using System;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

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


    [Header("Combat Settings")] [SerializeField]
    private Animator animator;
    [SerializeField] private int health = 10;
    [SerializeField] private float attackDistance = 1;
    private int damage = 5;

    private new Rigidbody2D rigidbody2D;
    private new Collider2D collider2D;
    private bool isGrounded = true;
    private bool isKnockback;
    private bool isDead;
    private bool isCleaning;
    private float horizontalPressed;
    private float foamParticleTimer;
    private Vector2 facingDirection = Vector2.right;

    // Ground-based cleaning cache
    private float groundY;
    private bool hasGroundHeight;

    // Debugging
    private float bloodCleanedThisFrame;
    private int cleaningFrames;
    private Vector2[] raycastPositions; // For debug visualization

    // region basic
    private void Start()
    {
        collider2D = GetComponent<Collider2D>();

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

        HandleMovement();
    }

    // endregion

    // region movement

    // Check if player has touched down
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        // Check if collision is from below (landing on ground)
        bool landedOnGround = IsLandingCollision(collision);

        if (landedOnGround)
        {
            isGrounded = true;
            animator.speed = 1f;
            isKnockback = false;
        }
        // If hitting from side or below, don't set grounded
    }

    private bool IsLandingCollision(Collision2D collision)
    {
        // Get the player's collider bounds
        Bounds playerBounds = GetComponent<Collider2D>().bounds;

        // Check each contact point
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Calculate how far the contact point is from the bottom of the player
            // contact.point.y will be at or near playerBounds.min.y if landing on top
            float distanceFromBottom = contact.point.y - playerBounds.min.y;

            // Also check the normal - if pointing up, we hit from above (landed on it)
            // Normal points away from the surface we hit
            bool normalPointsUp = contact.normal.y > 0.5f; // Allow some tolerance

            // If contact is near bottom AND normal points up, it's a landing
            if (distanceFromBottom < 0.1f && normalPointsUp)
            {
                return true;
            }
        }

        return false;
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
        if (IsLandingCollision(collision))
        {
            isGrounded = true;
            animator.speed = 1f;
            isKnockback = false;
        }
    }

    private void HandleMovement()
    {
        bool fire1Pressed = Input.GetButtonDown("Fire1");
        bool fire2Down = Input.GetButton("Fire2");
        bool isHitting = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
        horizontalPressed = Input.GetAxisRaw("Horizontal");
        
        rigidbody2D.linearVelocity = new Vector2(horizontalPressed * moveSpeed, rigidbody2D.linearVelocity.y);

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

        animator.SetBool("isRunning", horizontalPressed != 0);

        if (fire1Pressed && !isHitting)
        {
            animator.Play("Hit");
            DealDamage(attackDistance);
            isHitting = true;
        }

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

        // Jump pressed
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigidbody2D.linearVelocity = new Vector2(rigidbody2D.linearVelocity.x, jumpForce);
            animator.speed = 0f;
        }

        // Jump released - cut jump if we're still moving upward fast enough
        if (Input.GetButtonUp("Jump"))
        {
            if (rigidbody2D.linearVelocity.y > minJumpVelocity)
            {
                rigidbody2D.linearVelocity = new Vector2(
                    rigidbody2D.linearVelocity.x,
                    rigidbody2D.linearVelocity.y * jumpCutMultiplier
                );
            }
        }
        
        UpdateJumpAnimation(isHitting);
    }
    
    private void UpdateJumpAnimation(bool isHitting)
    {
        if (!isGrounded)
        {
            if(isHitting)
            {
                animator.speed = 1f;
            }
            else
            {
                animator.speed = 0f;
                if (rigidbody2D.linearVelocity.y > 0.1f)
                {
                    // Rising
                    animator.Play("Run", 0, 0.5f);
                }
                else if (rigidbody2D.linearVelocity.y < -0.1f)
                {
                    // Falling - use different animation or later frame
                    animator.Play("Run", 0, 0.625f);
                }
            }
        }
    }

    // endregion

    // region combat

    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        health -= damage;
        animator.speed = 0f;
        rigidbody2D.linearVelocity = knockbackDir * knockbackForce;
        isKnockback = true;

        if (health <= 0)
        {
            isDead = true;
            animator.Play("Die");
            animator.SetBool("isDead", true);
        }
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

    //endregion

    // region cleaning
    // subregion mechanics
    void StartCleaning()
    {
        isCleaning = true;
        foamParticleTimer = 0f;
        cleaningFrames = 0;
        hasGroundHeight = false;

        if (useGroundBasedCleaning)
        {
            FindGroundHeight();
        }

        Debug.Log($"Started cleaning! Ground-based: {useGroundBasedCleaning}, Ground Y: {groundY:F2}");
    }

    void StopCleaning()
    {
        isCleaning = false;
        hasGroundHeight = false;
        raycastPositions = null;
        Debug.Log($"Stopped cleaning. Cleaned for {cleaningFrames} frames.");
    }

    void FindGroundHeight()
    {
        if (useMultiRaycast)
        {
            FindGroundHeightMultiRaycast();
        }
        else
        {
            FindGroundHeightSingleRaycast();
        }
    }

    void FindGroundHeightSingleRaycast()
    {
        // Original single raycast method
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            maxCleaningHeight,
            groundLayer
        );

        if (hit.collider != null)
        {
            groundY = hit.point.y;
            hasGroundHeight = true;
        }
        else
        {
            groundY = transform.position.y;
            hasGroundHeight = true;
            Debug.LogWarning($"No ground found below player, using player Y={groundY:F2}");
        }
    }

    void FindGroundHeightMultiRaycast()
    {
        // Cast multiple rays in a line to catch platform edges
        // This helps when player is at edge of platform with pit below

        raycastPositions = new Vector2[raycastCount];
        float closestGroundY = float.NegativeInfinity;
        bool foundAnyGround = false;

        // Calculate ray positions spread out horizontally
        float startX = -raycastSpread / 2f;
        float stepX = raycastCount > 1 ? raycastSpread / (raycastCount - 1) : 0;

        for (int i = 0; i < raycastCount; i++)
        {
            float offsetX = startX + (stepX * i);
            Vector2 rayOrigin = new Vector2(transform.position.x + offsetX, transform.position.y);
            raycastPositions[i] = rayOrigin;

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.down,
                maxCleaningHeight,
                groundLayer
            );

            if (hit.collider != null)
            {
                // Found ground - use the closest (highest) ground
                if (hit.point.y > closestGroundY)
                {
                    closestGroundY = hit.point.y;
                    foundAnyGround = true;
                }
            }
        }

        if (foundAnyGround)
        {
            groundY = closestGroundY;
            hasGroundHeight = true;
            Debug.Log($"Multi-raycast found ground at Y={groundY:F2}");
        }
        else
        {
            // No ground found by any raycast - fallback to player height
            groundY = transform.position.y;
            hasGroundHeight = true;
            Debug.LogWarning($"Multi-raycast found no ground, using player Y={groundY:F2}");
        }
    }

    void PerformCleaning()
    {
        if (BloodSystem.Instance == null)
        {
            Debug.LogWarning("Cannot clean - BloodSystem.Instance is null!");
            return;
        }

        // Update ground height occasionally in case player moved
        if (useGroundBasedCleaning && cleaningFrames % 10 == 0)
        {
            FindGroundHeight();
        }

        float bloodBefore = BloodSystem.Instance.GetBloodInRadius(transform.position, cleanRadius);

        float cleanAmount = cleanRate * Time.deltaTime;

        if (useGroundBasedCleaning && hasGroundHeight)
        {
            BloodSystem.Instance.CleanBloodOnGround(
                transform.position,
                groundY,
                verticalTolerance,
                cleanRadius,
                cleanAmount
            );
        }
        else
        {
            BloodSystem.Instance.CleanBlood(
                transform.position,
                cleanRadius,
                cleanAmount
            );
        }

        float bloodAfter = BloodSystem.Instance.GetBloodInRadius(transform.position, cleanRadius);
        bloodCleanedThisFrame = bloodBefore - bloodAfter;
        cleaningFrames++;

        if (cleaningFrames % 30 == 0)
        {
            Debug.Log(
                $"Cleaning: {bloodBefore:F3} -> {bloodAfter:F3} (cleaned {bloodCleanedThisFrame:F4}, rate: {cleanAmount:F4})");
        }
    }

    void SpawnGroundFoam()
    {
        if (!foamParticles)
        {
            return;
        }

        foamParticleTimer += Time.deltaTime;
        float interval = 1f / foamParticlesPerSecond;

        while (foamParticleTimer >= interval)
        {
            foamParticleTimer -= interval;
            EmitFoamParticle();
        }
    }

    //endsubregion
    //subregion cosmetic

    void EmitFoamParticle()
    {
        Vector2 randomOffset = Random.insideUnitCircle * cleanRadius;
        Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

        float targetY = useGroundBasedCleaning && hasGroundHeight ? groundY : transform.position.y;

        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(spawnPosition.x, targetY + groundCheckDistance),
            Vector2.down,
            groundCheckDistance * 2f,
            groundLayer
        );

        if (hit.collider)
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = hit.point + Vector2.up * 0.1f;

            Vector2 horizontalVel = Random.insideUnitCircle * 0.5f;
            emitParams.velocity = new Vector3(horizontalVel.x, 0, 0);

            emitParams.startSize = Random.Range(0.2f, 0.5f);
            emitParams.rotation = Random.Range(0f, 360f);
            emitParams.startLifetime = Random.Range(1.5f, 2.5f);

            foamParticles.Emit(emitParams, 1);
        }
    }

    //endsubregion
    //endregion

    //region debug

    void OnDrawGizmosSelected()
    {
        // Draw cleaning radius
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, cleanRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cleanRadius);

        // Draw ground-based cleaning visualization
        if (Application.isPlaying && useGroundBasedCleaning && hasGroundHeight)
        {
            // Draw the ground line
            Gizmos.color = Color.green;
            Vector2 left = new Vector2(transform.position.x - cleanRadius, groundY);
            Vector2 right = new Vector2(transform.position.x + cleanRadius, groundY);
            Gizmos.DrawLine(left, right);

            // Draw vertical tolerance
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Vector3 toleranceSize = new Vector3(cleanRadius * 2, verticalTolerance * 2, 0);
            Gizmos.DrawCube(new Vector3(transform.position.x, groundY, 0), toleranceSize);

            // Draw multi-raycast positions
            if (useMultiRaycast && raycastPositions != null)
            {
                for (int i = 0; i < raycastPositions.Length; i++)
                {
                    Vector2 rayStart = raycastPositions[i];
                    Vector2 rayEnd = new Vector2(rayStart.x, groundY);

                    // Color based on whether this ray hit ground
                    bool hitGround = Mathf.Abs(rayEnd.y - groundY) < 0.1f;
                    Gizmos.color = hitGround ? Color.yellow : Color.red;

                    Gizmos.DrawLine(rayStart, rayEnd);
                    Gizmos.DrawWireSphere(rayStart, 0.1f);
                }
            }
            else
            {
                // Draw single raycast
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, groundY));
            }
        }

        // Draw attack range
        Vector2 direction = facingDirection;
        Vector2 origin = (Vector2)transform.position + direction * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + direction * attackDistance);
        Gizmos.DrawWireSphere(origin + direction * attackDistance, 0.2f);
    }

    private void OnFinishedDeathAniEvent()
    {
       Destroy(gameObject);
    }

    public bool DeathStatus()
    {
        return isDead;
    }

    //endregion

    //region properties

    public bool IsCleaning() => isCleaning;
    public float GetGroundY() => groundY;
    public bool HasGroundHeight() => hasGroundHeight;

    //endregion
}