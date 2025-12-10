using Interfaces;
using UnityEngine;


public class PlayerController:MonoBehaviour, IDamageable
{   
    private PlayerData player;
    private  Rigidbody2D rigidbody2D;
    private Animator animator;

    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpCutMultiplier = 0.5f; // How much to cut velocity by (0.5 = cut to 50%)
    [SerializeField] private float minJumpVelocity = 2f; // Minimum upward velocity before we allow cutting

    [Header("Combat Settings")]
    [SerializeField] private float attackDistance;
    [SerializeField] private int damage = 5;
    private bool isKnockback;

    private Vector2 facingDirection;

    public void Start()
    {
        if (PlayerData.Instance != null)
        {
            player = PlayerData.Instance;
        } else
        {
            Debug.Log("Instanz nicht gesetzt");
            return;
        }
        animator = player.GetPlayerAnimator();
        rigidbody2D = player.GetRb();
    }
    
    public void Update()
    {
        if(!player.IsDead() && !isKnockback)
        {
            Combat();
            Move();
        }
    }

    private void Move()
    {
        bool fire1Pressed = Input.GetButtonDown("Fire1"); 
        bool fire2Down = Input.GetButton("Fire2");
        float horizontalPressed = Input.GetAxisRaw("Horizontal");
        bool isHitting = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");

        rigidbody2D.linearVelocity = new Vector2(horizontalPressed * moveSpeed, rigidbody2D.linearVelocity.y);

        if (horizontalPressed < 0)
        {
            player.transform.rotation = Quaternion.Euler(0, 180, 0);
            facingDirection = Vector2.left;
        }
        else if (horizontalPressed> 0)
        {
            player.transform.rotation = Quaternion.identity;
            facingDirection = Vector2.right;
        }

        if (player.isGrounded)
        {
            animator.SetBool("isRunning", horizontalPressed != 0);

        }

        // Jump pressed
        if (Input.GetButtonDown("Jump") && player.isGrounded)
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

        UpdateJumpAnimation(isHitting, horizontalPressed);
    }


    private void Combat()
    {
        bool fire1Pressed = Input.GetButtonDown("Fire1");
        bool fire2Down = Input.GetButton("Fire2");
        float horizontalPressed = Input.GetAxisRaw("Horizontal");
        bool isHitting = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");

        if (fire1Pressed && !isHitting)
        {
            animator.Play("Hit");
            //Debug.Log(attackDistance);
            DealDamage(attackDistance);
            isHitting = true;
        }
    }
    private void UpdateJumpAnimation(bool isHitting, float horizontalPressed)
    {
        if (!player.isGrounded)
        {
            if (isHitting)
            {
                animator.speed = 1f;
            }
            else
            {   
                //only when horizontalPressed (prevents weird flicking bug when jumping when not running!)
                animator.speed = 0f;
                if (rigidbody2D.linearVelocity.y > 0.1f && horizontalPressed != 0)
                {
                    // Rising
                    animator.Play("Run", 0, 0.5f);
                }
                else if (rigidbody2D.linearVelocity.y < -0.1f && horizontalPressed != 0)
                {
                    // Falling - use different animation or later frame
                    animator.Play("Run", 0, 0.625f);
                }
            }
        }
    }

    public bool IsLandingCollision(Collision2D collision)
    {
        // Get the player's collider bounds
        Bounds playerBounds = player.GetComponent<Collider2D>().bounds;

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

    public void DebugCombat()
    {
        if (player == null) return;
        // Draw attack range
        Vector2 direction = facingDirection;
        Vector2 origin = (Vector2)player.transform.position + direction * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + direction * attackDistance);
        Gizmos.DrawWireSphere(origin + direction * attackDistance, 0.2f);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        // Check if collision is from below (landing on ground)
        bool landedOnGround = IsLandingCollision(collision);

        if (landedOnGround)
        {
            player.isGrounded = true;
            animator.speed = 1f;
            isKnockback = false;
        }
        // If hitting from side or below, don't set grounded
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        // When leaving any ground collision, verify if we're still on ground
        // Use a small raycast downward from the bottom of our collider
        float checkDistance = 0.2f;
        Vector2 rayOrigin = new Vector2(player.transform.position.x, player.GetCollider2D().bounds.min.y);

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            checkDistance,
            player.groundLayer
        );

        // Only unground if we're not touching ground below us
        if (hit.collider == null)
        {
            player.isGrounded = false;
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }

        // Continuously check if we're landing/standing on ground
        if (IsLandingCollision(collision))
        {
            player.isGrounded = true;
            animator.speed = 1f;
        }
    }
    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        player.SetHealth(damage);
        animator.speed = 0f;
        //rigidbody2D.linearVelocity = knockbackDir * knockbackForce;


        rigidbody2D.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);


        isKnockback = true;
    }

    public void DealDamage(float attackDistance)
    {
        Vector2 dir = facingDirection;
        Vector2 origin = (Vector2)player.transform.position + (dir * 0.5f);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            dir,
            attackDistance,
            LayerMask.GetMask("Enemy")
        );

        if (!hit.collider)
            return;

        float distance = Vector2.Distance(player.transform.position, hit.transform.position);

        if (distance <= attackDistance && hit.collider.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage, facingDirection, 0f);
        }
    }

    public bool GetIsKnockback() => isKnockback;

    void OnDrawGizmosSelected()
    {
        DebugCombat();
    }

}
