using System.Collections;
using Characters.Interfaces;
using UnityEngine;

namespace Traps
{
    public class SpikeTrap : MonoBehaviour
    {
        [Header("SpikeTrap Timers")]
        [SerializeField] private float activateDelay;
        [SerializeField] private float activeTime;
        [SerializeField] private float damage;
        [SerializeField] private float damageInterval = 2f; // Time between hits (e.g., every 0.5s)

        private Animator anim;
        private SpriteRenderer spriteRend;
        private bool active;
    
        // Reference to the player inside the trap
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
            // If the trap is active, the player is inside, and the cooldown has passed
            if (active && playerInRange != null && Time.time >= nextDamageTime)
            {
                ApplyDamage();
            }
        }

        private void ApplyDamage()
        {
            playerInRange.TakeDamage((int)damage, Vector2.zero, 0f);
            nextDamageTime = Time.time + damageInterval; // Set the time for the next hit
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerInRange = collision.GetComponent<IDamageable>();
            
                // If they enter exactly when it's active, deal damage immediately
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
                playerInRange = null; // Player has exited, stop dealing damage
            }
        }

        private IEnumerator AutomaticSpikeTrapCycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(activateDelay);
            
                // Warning period (turns red before spikes come out)
                yield return new WaitForSeconds(activateDelay);
            
                // ACTIVATION
                active = true;
                anim.SetBool("activated", true);

                // Check if the player was already on the trap when it activated
                if (playerInRange != null)
                {
                    ApplyDamage();
                }

                yield return new WaitForSeconds(activeTime);
            
                // DEACTIVATION
                active = false;
                anim.SetBool("activated", false);
            }
        }
    }
}