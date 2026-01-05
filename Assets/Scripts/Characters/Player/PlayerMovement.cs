using System;
using Characters.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Jumping")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;
        

        private Rigidbody2D rb;
        private Collider2D col;
        private PlayerInputHandler input;
        private bool isKnockback;

        // PUBLIC STATE - read by others
        public bool IsGrounded { get; private set; }
        public Vector2 FacingDirection { get; private set; } = Vector2.right;
        public float HorizontalInput { get; private set; }
        public Vector2 Velocity => rb.linearVelocity;
        

        // PUBLIC CONTROL - called by combat
        public void ApplyKnockback(Vector2 dir, float force)
        {
            rb.linearVelocity = dir * force;
            isKnockback = true;
        }

        public void SetKnockbackState(bool state)
        {
            isKnockback = state;
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
            if (isKnockback) return;
            HandleJump();
            UpdateFacingDirection();
        }

        private void FixedUpdate()
        {
            if (isKnockback) return;
            HandleMovementInput();
        }

        private void HandleMovementInput()
        {
            HorizontalInput = input.MoveInput;
            rb.linearVelocity = new Vector2(HorizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        private void HandleJump()
        {
            if (input.JumpPressed && IsGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
                isKnockback = false;
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