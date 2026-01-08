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

    SpriteRenderer spriteRenderer;
    // Sprite verschwindet
    // wieder sichtbar

    void Start()
    {
        spriteRenderer =  transform.Find("tongue").GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) return;

        if (player == null) return;

        if (tongue != null)
            spriteRenderer.enabled = false;  //tongue is disabled

        controller = GetComponent<LuciferController>();
    }

    void Update()
    {

        if (isShooting)
        {
            // tongue move towards player

            ShootTongue();
        }
         if (isRetracting && !isShooting)
        {
           RetractTongue();
        }
    }

    // Call from LuciferController
    public void Attack()
    {
        if (tongue == null || player == null) return;

        spriteRenderer.enabled = true;        // Make tongue visible
        isShooting = true;
        isRetracting = false;

       
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


        // is player reached
        if (Vector3.Distance(tongueIKTarget.position, player.transform.position) < 1f)
        {
            isShooting = false;
            isRetracting = true;

            controller.OnDamageDelt(this);

        }
    }

    private void RetractTongue()
    {        
        // retract tongue
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
            spriteRenderer.enabled = false; // tongue dissapear

            // Start Cooldown after attack is finished!!!!

            controller.OnAttackAnimationOver();
        }
    }

    public string GetAttackName()
    {
        return GetType().Name
;
    }

    public IEnumerator AttackCooldown()
    {
        canAttack = false;

        // Hier kommt dein Angriffscode

        // Zunge abfeuern, Animation triggern, Collider aktivieren, etc.

        // Cooldown abwarten
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

