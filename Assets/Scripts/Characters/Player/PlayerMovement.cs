using UnityEngine;



public class PlayerMovement
{   

    private readonly PlayerScript player;
    private readonly Animator animator;
    private readonly Rigidbody2D rigidbody2D;
    private readonly float jumpForce;
    private readonly float moveSpeed;
    private readonly float minJumpVelocity;
    private float jumpCutMultiplyer;
    private float attackDistance;

    public PlayerMovement(PlayerScript player)
    {
        this.player = player;
        animator = player.GetPlayerAnimator();
        rigidbody2D = player.GetRb();
        jumpForce = player.GetJumpForce();
        moveSpeed = player.GetMoveSpeed();
        minJumpVelocity = player.GetMinJumpVelocity();
        jumpCutMultiplyer = player.GetJumpMultiplier();
        attackDistance = player.GetAttackDistance();
    }


    public void Update()
    {
        Combat();
        Move();   
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
            player.facingDirection = Vector2.left;
        }
        else if (horizontalPressed> 0)
        {
            player.transform.rotation = Quaternion.identity;
            player.facingDirection = Vector2.right;
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
                    rigidbody2D.linearVelocity.y * jumpCutMultiplyer
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
            player.DealDamage(attackDistance);
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
        // Draw attack range
        Vector2 direction = player.facingDirection;
        Vector2 origin = (Vector2)player.transform.position + direction * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + direction * attackDistance);
        Gizmos.DrawWireSphere(origin + direction * attackDistance, 0.2f);
    }

}
