using System.Collections;
using Characters.Enemies;
using UnityEngine;

public class AttackTrident : MonoBehaviour, IAttack
{
    [SerializeField] private float attackCooldown = 4f;
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private Vector2 knockbackDir = new Vector2(3,1);
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private int damage = 5;
    [SerializeField] private Vector2 damageRange = new Vector2(3, 1);
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.3f;

    private bool canAttack = true;
    private LuciferController controller;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<LuciferController>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Attack()
    {
        StartCoroutine(AttackRoutine());
    }

    public IEnumerator AttackCooldown()
    {
        canAttack = false;
        
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    private IEnumerator AttackRoutine()
    {
        float timer = 0f;
        Vector2 dashDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        
        while (timer < dashDuration)
        {
            rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, 0);
            timer += Time.deltaTime;
            yield return null;
        }
        
        rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(0.2f);
        
        controller.OnDamageDelt(this);
    }

    public Vector2 GetKnockbackDir() => knockbackDir;

    public float GetKnockbackForce() => knockbackForce;

    public float GetAttackCooldown() => attackCooldown;

    public float GetAttackDistance() => attackDistance;

    public string GetAttackName() => GetType().Name;

    public bool GetCanAttack() => canAttack;

    public int GetDamage() => damage;

    Vector2 IAttack.GetDamageRange() => damageRange;
}