using UnityEngine;
using System.Collections;
using Characters.Player; 

public class SpikeTrap : MonoBehaviour
{
    [Header("SpikeTrap Timers")]
    [SerializeField] private float activateDelay;
    [SerializeField] private float activeTime;
    [SerializeField] private float damage;
    
    private Animator anim;
    private SpriteRenderer spriteRend;
    private bool triggered;
    private bool active;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if (!triggered)
            {
                StartCoroutine(ActivateSpikeTrap());
            }
            if(active)
            {
                
                {
                    PlayerData playerData = collision.GetComponent<PlayerData>();
                    if (playerData != null)
                    {
                        // playerData.TakeDamage(damage);
                    }
                }
            }
        }
    }

    private IEnumerator ActivateSpikeTrap()
    {
        triggered = true;
        spriteRend.color = Color.red;
        yield return new WaitForSeconds(activateDelay);
        active = true;
        yield return new WaitForSeconds(activeTime);
        active = false;
        triggered = false;
    }
}