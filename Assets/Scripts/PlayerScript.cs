using UnityEngine;
using Interfaces;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private Animator animator;
    [SerializeField] private int health = 10;
    private int demage = 5;

    private Rigidbody2D rb;
    private bool isGrounded = true; // Check if player is grounded
    private bool isKnockback = false;

    private void Awake()
    {

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(isKnockback)
        {
            return;
        }

        // TODO: Sweeping;
        // TODO: Broom should only listen to collisions during hit animation -> should probably be script of broom
        HandleMovement();

        if(health <= 0)
        {
            GetComponent<Collider2D>().enabled = false;
        }

    }

    // Check if player has touched down
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO: only count collisions from below

        // check if ground 
        if (!collision.gameObject.CompareTag("Ground"))
        {
            return;
        }
        isGrounded = true; // player has landed and can jump again
        animator.speed = 1f;
        isKnockback = false;
    }

    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        health -= damage;

        // Animation stoppen
        animator.speed = 0f;

        // Knockback anwenden
        rb.linearVelocity = knockbackDir * knockbackForce;

        // Knockback Timer setzen
        isKnockback = true;
        
        // Nach Knockback Animation wieder starten
    }

    private void HandleMovement()
    {
        bool fire1Pressed = Input.GetButtonDown("Fire1");
        bool isHitting = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
        float horizontalPressed = Input.GetAxisRaw("Horizontal");

        // horizontal movement
        rb.linearVelocity = new Vector2(horizontalPressed * moveSpeed, rb.linearVelocity.y);


       

        // Filp direction
        if (horizontalPressed < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (horizontalPressed > 0)
            transform.rotation = Quaternion.identity;

        // set animation

        animator.SetBool("isRunning", horizontalPressed != 0);


        if (fire1Pressed && !isHitting)
        {



            animator.Play("Hit");

            IsDemageDelt(horizontalPressed > 0, 3f);


        }

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; // prevent double jumps
            animator.speed = 0f;
        }
    }

    private void IsDemageDelt(bool looksRight, float demageDistance)
    {
        Vector2 origin = (Vector2)transform.position +
                         (looksRight ? Vector2.right : Vector2.left) * 0.5f;

        Vector2 dir = looksRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            dir,
            demageDistance,
            LayerMask.GetMask("Enemy")
        );

        if (hit.collider == null)
            return;

        float distance = Vector2.Distance(transform.position, hit.transform.position);

        IDemageable demageable = hit.collider.GetComponent<IDemageable>();
        if (demageable != null && distance <= demageDistance)
        {
            demageable.TakeDemage(demage);
        }
    }

}
