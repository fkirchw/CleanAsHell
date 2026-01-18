using UnityEngine;

namespace Characters.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float moveSpeed = 5f;

        [Header("Jumping")] [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Knockback")] [SerializeField] private float knockbackDuration = 0.3f;

        [Header("Fast Fall")] [SerializeField] private float fastFallForce = 10f;

        [Header("Look Down")] [SerializeField] private float lookDownDelay = 1f;
        [SerializeField] private float lookDownDelayAfterFastFall = 0.5f;

        private Rigidbody2D rb;
        private Collider2D col;
        private PlayerInputHandler input;
        private PlayerData playerData;

        private bool isKnockback;
        private float knockbackTimer;
        private float jumpBufferCounter;
        private bool canFastFall = true;
        private float lookDownDelayTimer = 0f;
        private bool didFastFall = false;
        private float fastFallGroundTimer = 0f;

        public bool IsGrounded { get; private set; }
        public Vector2 FacingDirection { get; private set; } = Vector2.right;
        public float HorizontalInput { get; private set; }
        public Vector2 Velocity => rb.linearVelocity;

        public void ApplyKnockback(Vector2 dir, float force)
        {
            rb.linearVelocity = dir * force;
            knockbackTimer = knockbackDuration;
            isKnockback = true;
        }

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
            playerData = GetComponent<PlayerData>();
        }

        private void Update()
        {
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.deltaTime;
                if (knockbackTimer <= 0f)
                {
                    isKnockback = false;
                }

                return;
            }

            UpdateJumpBuffer();
            HandleJump();
            UpdateFacingDirection();
            UpdateLookDown();
            HandleFastFall();
            UpdateFastFallGroundTimer();
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
            if (jumpBufferCounter > 0f && IsGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpBufferCounter = 0f;
                canFastFall = true;
                didFastFall = false;
            }

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

        private void UpdateLookDown()
        {
            if (input.VerticalInput < -0.5f)
            {
                if (IsGrounded)
                {
                    if (didFastFall && fastFallGroundTimer > 0f)
                    {
                        lookDownDelayTimer += Time.deltaTime;
                        if (lookDownDelayTimer >= lookDownDelayAfterFastFall)
                        {
                            playerData.IsLookingDown = true;
                        }
                    }
                    else
                    {
                        playerData.IsLookingDown = true;
                        lookDownDelayTimer = 0f;
                    }
                }
                else
                {
                    lookDownDelayTimer += Time.deltaTime;
                    if (lookDownDelayTimer >= lookDownDelay)
                    {
                        playerData.IsLookingDown = true;
                    }
                }
            }
            else
            {
                playerData.IsLookingDown = false;
                lookDownDelayTimer = 0f;
            }
        }

        private void HandleFastFall()
        {
            if (input.VerticalInput < -0.5f && !IsGrounded && canFastFall && rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallForce);
                canFastFall = false;
                didFastFall = true;
            }
        }

        private void UpdateFastFallGroundTimer()
        {
            if (didFastFall && IsGrounded)
            {
                fastFallGroundTimer += Time.deltaTime;
                if (fastFallGroundTimer >= 0.5f)
                {
                    didFastFall = false;
                    fastFallGroundTimer = 0f;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground") && IsLandingCollision(collision))
            {
                IsGrounded = true;
                isKnockback = false;
                knockbackTimer = 0f;
                canFastFall = true;
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
            float playerCenterY = playerBounds.center.y;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                bool normalPointsUp = contact.normal.y > 0.5f;

                bool isBelowCenter = contact.point.y < playerCenterY;

                float distanceFromBottom = contact.point.y - playerBounds.min.y;
                bool isInBottomHalf = distanceFromBottom < playerBounds.size.y * 0.5f;

                if (normalPointsUp && isBelowCenter && isInBottomHalf)
                    return true;
            }

            return false;
        }
    }
}