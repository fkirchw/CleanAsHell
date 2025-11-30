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
        // TODO: Sweeping;
        // TODO: Broom should only listen to collisions during hit animation -> should probably be script of broom
        bool fire1Pressed = Input.GetButtonDown("Fire1");
        bool isHitting = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
        float horizontal = Input.GetAxisRaw("Horizontal");

        // horizontal movement
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // Filp direction
        if (horizontal < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (horizontal > 0)
            transform.rotation = Quaternion.identity;
        
        // set animation
        animator.SetBool(IsRunning, horizontal != 0);

        if (fire1Pressed && !isHitting)
            animator.Play("Hit");
        
        // jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; // prevent double jumps
            animator.speed = 0f;
        }

        if (Input.GetMouseButtonDown(1))
        {
            LuciferMain lucifer = GetComponent<LuciferMain>();
            lucifer.TriggeredByPlayer();
        }
    }

    // Check if player has touched down
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO: only count collisions from below
        if (!collision.gameObject.CompareTag("Ground")) return;
        
        isGrounded = true; // player has landed and can jump again
        animator.speed = 1f;
    }
}
