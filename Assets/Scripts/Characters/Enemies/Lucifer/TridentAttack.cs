using System.Collections;
using Characters.Enemies;
using Characters.Enemies;
using UnityEngine;

public class AttackTrident : MonoBehaviour, IAttack
{

    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private Vector2 knockbackDir = new Vector2(3,1);
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private int damage = 5;
    [SerializeField] private Vector2 damageRange = new Vector2(3, 1);

    private bool canAttack = true;
    private LuciferController controller;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<LuciferController>();
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
        yield return new WaitForSeconds(0.5f); // 10 ms
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
}
