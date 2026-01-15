using UnityEngine;
using System.Collections;
using Characters.Interfaces;

public class SpikeTrap : MonoBehaviour
{
    [Header("SpikeTrap Timers")]
    [SerializeField] private float activateDelay;
    [SerializeField] private float activeTime;
    [SerializeField] private float damage;
    [SerializeField] private float damageInterval = 2f; // Timpul între lovituri (ex: la fiecare 0.5s)

    private Animator anim;
    private SpriteRenderer spriteRend;
    private bool active;
    
    // Referință către jucătorul aflat în capcană
    private IDamageable playerInRange;
    private float nextDamageTime;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        StartCoroutine(AutomaticSpikeTrapCycle());
    }

    private void Update()
    {
        // Dacă capcana e activă, jucătorul e înăuntru și a trecut timpul de cooldown
        if (active && playerInRange != null && Time.time >= nextDamageTime)
        {
            ApplyDamage();
        }
    }

    private void ApplyDamage()
    {
        playerInRange.TakeDamage((int)damage, Vector2.zero, 0f);
        nextDamageTime = Time.time + damageInterval; // Setează momentul următoarei lovituri
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = collision.GetComponent<IDamageable>();
            
            // Dacă intră exact când e activă, dă-i damage imediat
            if (active && playerInRange != null)
            {
                ApplyDamage();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = null; // Jucătorul a ieșit, oprim damage-ul
        }
    }

    private IEnumerator AutomaticSpikeTrapCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(activateDelay);
            
            // Perioada de avertizare (se face roșu înainte să iasă țepii)
            yield return new WaitForSeconds(activateDelay);
            
            // ACTIVARE
            active = true;
            anim.SetBool("activated", true);

            // Verificăm dacă jucătorul era deja pe capcană când s-a activat
            if (playerInRange != null)
            {
                ApplyDamage();
            }

            yield return new WaitForSeconds(activeTime);
            
            // DEZACTIVARE
            active = false;
            anim.SetBool("activated", false);
        }
    }
}