using UnityEngine;

public class LuciferMain : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2f;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private bool playerDetected = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (playerDetected && player != null)
        {

            Vector2 direction = (player.position - transform.position).normalized;

            // Nur die horizontale Richtung verwenden (2D Plattform)
            float moveX =  moveSpeed * direction.x;

            // Rigidbody bewegen
            rb.linearVelocity = new Vector2(moveX, 0);
            // Richtung berechnen

            // Gegner bewegt sich auf Spieler zu (nur horizontal)

            // Gegner Richtung flippen
            if (direction.x < 0)
                spriteRenderer.flipX = false;
            else if (direction.x > 0)
                spriteRenderer.flipX = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = true;
            player = collision.transform;
            animator.SetBool("isWalking", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = false;
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);

        }
    }

    public void TriggeredByPlayer()
    {
        Debug.Log("Trigger");
        animator.SetBool("isDead", true);
    }
}