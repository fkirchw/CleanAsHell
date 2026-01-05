using UnityEngine;

namespace Characters.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        
        [Header("Jumping")]
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float jumpBufferTime = 0.1f;
        
        [Header("Knockback")]
        [SerializeField] private float knockbackDuration = 0.3f;

        private Rigidbody2D rb;
        private Collider2D col;
        private PlayerInputHandler input;
        
        private bool isKnockback;
        private float knockbackTimer;
        private float jumpBufferCounter;

        // PUBLIC STATE - read by others
        public bool IsGrounded { get; private set; }
        public Vector2 FacingDirection { get; private set; } = Vector2.right;
        public float HorizontalInput { get; private set; }
        public Vector2 Velocity => rb.linearVelocity;

        // PUBLIC CONTROL - called by combat
        public void ApplyKnockback(Vector2 dir, float force)
        {
            rb.linearVelocity = dir * force;
            knockbackTimer = knockbackDuration;
            isKnockback = true;
        }

        // Ground detection for cleaning
        public float? GetGroundHeightBelow(float maxDistance, LayerMask groundLayer)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                maxDistance,
                groundLayer
            );
            return hit.collider ? hit.point.y : null;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            input = GetComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            // Update knockback timer
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.deltaTime;
                if (knockbackTimer <= 0f)
                {
                    isKnockback = false;
                }
                return; // Don't process input during knockback
            }

            UpdateJumpBuffer();
            HandleJump();
            UpdateFacingDirection();
        }

        private void FixedUpdate()
        {
            if (isKnockback) return;
            HandleMovementInput();
        }

        private void UpdateJumpBuffer()
        {
            if (input.JumpPressed)
                jumpBufferCounter = jumpBufferTime;
            else
                jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);
        }

        private void HandleMovementInput()
        {
            HorizontalInput = input.MoveInput;
            rb.linearVelocity = new Vector2(HorizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        private void HandleJump()
        {
            // Execute jump if we have buffered input and are grounded
            if (jumpBufferCounter > 0f && IsGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpBufferCounter = 0f;
            }

            // Jump cut
            if (input.JumpReleased && rb.linearVelocity.y > 2f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }

        private void UpdateFacingDirection()
        {
            if (HorizontalInput < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
                FacingDirection = Vector2.left;
            }
            else if (HorizontalInput > 0)
            {
                transform.rotation = Quaternion.identity;
                FacingDirection = Vector2.right;
            }
        }

        // Collision detection
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground") && IsLandingCollision(collision))
            {
                IsGrounded = true;
                isKnockback = false;
                knockbackTimer = 0f;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Ground")) return;

            float checkDistance = 0.2f;
            Vector2 rayOrigin = new Vector2(transform.position.x, col.bounds.min.y);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, checkDistance, LayerMask.GetMask("Ground"));

            if (hit.collider == null)
                IsGrounded = false;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground") && IsLandingCollision(collision))
            {
                IsGrounded = true;
            }
        }

        private bool IsLandingCollision(Collision2D collision)
        {
            Bounds playerBounds = col.bounds;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                float distanceFromBottom = contact.point.y - playerBounds.min.y;
                bool normalPointsUp = contact.normal.y > 0.5f;
                if (distanceFromBottom < 0.1f && normalPointsUp)
                    return true;
            }

            return false;
        }
    }
}