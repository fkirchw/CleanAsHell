using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isGrounded = true; // Zum Prüfen, ob Spieler auf dem Boden ist

    private void Awake()
    {   
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        // Bewegung
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // Richtung umdrehen
        if (horizontal < 0)
            spriteRenderer.flipX = true;
        else if (horizontal > 0)
            spriteRenderer.flipX = false;

        // Animation
        animator.SetBool("isRunning", horizontal != 0);

        // Sprung
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; // verhindert Doppelsprünge
            animator.speed = 0f;
        }

        if (Input.GetMouseButtonDown(1))
        {
            LuciferMain lucifer = GetComponent<LuciferMain>();
            lucifer.TriggeredByPlayer();
        }
    }

    // Prüfen, ob der Spieler wieder auf dem Boden ist
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.speed = 1f;
        }
    }

    
}
