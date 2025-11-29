using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isGrounded = true; // Check if player is grounded

    private void Awake()
    {   
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        // horizontal movement
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // Filp direction
        if (horizontal < 0)
            spriteRenderer.flipX = true;
        else if (horizontal > 0)
            spriteRenderer.flipX = false;
        
        // set animation
        animator.SetBool(IsRunning, horizontal != 0);

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; // prevent double jumps
            animator.speed = 0f;
        }
    }

    // Pr�fen, ob der Spieler wieder auf dem Boden ist
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // player has landed and can jump again
            animator.speed = 1f;
        }
    }
}
