using Characters.Enemies;
using System.Collections;
using UnityEngine;

public class AttackTongue : MonoBehaviour, IAttack
{
    [Header("Tongue")]
    public GameObject tongue;          
    public Transform tongueIKTarget;   
    public Transform tongueIKBase;

    [SerializeField] private float tongueSpeed = 10f;    
    [SerializeField] public float retractSpeed = 15f;   
    [SerializeField] private float attackDistance = 7f;
    [SerializeField] private float attackDuration = 1.5f;

    [SerializeField] private float attackCooldown = 5f;

    [SerializeField] private int damage = 1;

    [SerializeField] private Vector2 knockbackDir = new Vector2(-4, 2);

    [SerializeField] private float knockBackForce = 3f;

    [SerializeField] private Vector2 damageRange = new Vector2(7f, 9999999999);


    [Header("Player")]
    [SerializeField] public GameObject player;

    private LuciferController controller;

    bool canAttack = true;

    private Vector3 initialTonguePos;
    private bool isShooting = false;
    private bool isRetracting = false;
    private float attackTimer = 0f;

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer =  transform.Find("tongue").GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) return;

        if (player == null) return;

        if (tongue != null)
            spriteRenderer.enabled = false;

        controller = GetComponent<LuciferController>();
    }

    void Update()
    {
        if (isShooting)
        {
            ShootTongue();
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= attackDuration)
            {
                isShooting = false;
                isRetracting = true;
                attackTimer = 0f;
            }
        }
        else if (isRetracting && !isShooting)
        {
           RetractTongue();
        }
    }

    public void Attack()
    {
        if (tongue == null || player == null) return;

        spriteRenderer.enabled = true;
        isShooting = true;
        isRetracting = false;
        attackTimer = 0f;
    }

    private void ShootTongue()
    {
        tongueIKTarget.position = Vector3.MoveTowards(
               tongueIKTarget.position,
               player.transform.position,
               tongueSpeed * Time.deltaTime
           );

        Vector2 dirTarget = tongueIKTarget.position - tongueIKBase.position;
        float angleTarget = Mathf.Atan2(dirTarget.y, dirTarget.x) * Mathf.Rad2Deg;

        tongueIKBase.rotation = Quaternion.Euler(0, 0, angleTarget);

        Vector2 dirPlayer = player.transform.position - tongueIKBase.position;
        float anglePlayer = Mathf.Atan2(dirPlayer.y, dirPlayer.x) * Mathf.Rad2Deg;

        tongueIKTarget.rotation = Quaternion.Euler(0, 0, anglePlayer);

        if (Vector3.Distance(tongueIKTarget.position, player.transform.position) < 1f)
        {
            isShooting = false;
            isRetracting = true;
            attackTimer = 0f;

            controller.OnDamageDelt(this);
        }
    }

    private void RetractTongue()
    {        
        tongueIKTarget.position = Vector3.MoveTowards(
            tongueIKTarget.position,
            new Vector3(controller.transform.position.x, controller.transform.position.y + 0.527f),
            retractSpeed * Time.deltaTime
        );

        tongueIKBase.rotation = Quaternion.Euler(0, 0, 0);

        tongueIKTarget.rotation = Quaternion.Euler(0, 0, 0);

        if (Vector3.Distance(tongueIKTarget.position, new Vector3(controller.transform.position.x, controller.transform.position.y + 0.527f)) < 1f)
        {
            isRetracting = false;
            spriteRenderer.enabled = false;

            controller.OnAttackAnimationOver();
        }
    }

    public string GetAttackName()
    {
        return GetType().Name;
    }

    public IEnumerator AttackCooldown()
    {
        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    public float GetAttackCooldown() => attackCooldown;

    public float GetAttackDistance() => attackDistance;

    public bool GetCanAttack() => canAttack;

    public int GetDamage() => damage;

    public Vector2 GetKnockbackDir() => knockbackDir;

    public float GetKnockbackForce() => knockBackForce;

    public Vector2 GetDamageRange() => damageRange;
}