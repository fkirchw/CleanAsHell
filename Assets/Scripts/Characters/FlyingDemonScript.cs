using UnityEngine;
using Interfaces;
using Unity.VisualScripting;

public class FlyingDemonScript : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator animator;
    private Transform playerPosition;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 direction;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private int health = 10;
    [SerializeField] private int damage = 10;

    private bool isDead = false;

    private bool playerDetected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (playerDetected && playerPosition != null)
        {
            HandleMovement();
        }

    }

    public void DealDamage(float attackDistance)
    {
        float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(playerPosition.position.x, playerPosition.position.y));

        if (distanceToPlayer > attackDistance)
        {
            return;
        }


        IDamageable playerScript = playerPosition.GetComponent<PlayerScript>();
        if (playerScript != null)
        {

            Vector2 knockbackDir = new Vector2(2, 1).normalized;

            if (direction.x < 0)
            {
                knockbackDir.x *= -1;
            }

            playerScript.TakeDamage(damage, knockbackDir, 6f);
        }

    }

    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        health -= damage;

        animator.SetBool("isHurt", true);

        rb.linearVelocity = knockbackDir * knockbackForce;

        if (health <= 0)
        {
            isDead = true;
            animator.Play("Death");
            animator.SetBool("isDead", true);
        }
    }

    private void HandleMovement()
    {
        direction = (playerPosition.position - transform.position).normalized;

        if (direction.x < 0)
            spriteRenderer.flipX = false;
        else if (direction.x > 0)
            spriteRenderer.flipX = true;

        float moveX = moveSpeed * direction.x;
        float movey = moveSpeed * direction.y;

        rb.linearVelocity = new Vector2(moveX, movey);

        float distanceToPlayer = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(playerPosition.position.x, playerPosition.position.y));

        animator.SetBool("isAttacking", distanceToPlayer < attackDistance);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = true;
            playerPosition = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerDetected = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void OnFinishedDeathAniEvent()
    {
        Destroy(gameObject);
    }

    public void OnFinishedHurtAniEvent()
    {
        animator.SetBool("isHurt", false);

    }

    public void OnDamageDeltAniEvent()
    {
        animator.SetBool("isAttacking", false);
        DealDamage(attackDistance);
    }
}
