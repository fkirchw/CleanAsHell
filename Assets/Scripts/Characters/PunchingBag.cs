using Interfaces;
using UnityEngine;
using System.Collections;

public class PunchingBag : MonoBehaviour, IDamageable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private float health = 100;
    private Material spriteMaterial;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Create a unique material instance to avoid affecting other sprites
        spriteMaterial = spriteRenderer.material;
    }

    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        BloodSystem.Instance.OnEnemyHit(this.transform.position, knockbackDir, false, damage);
        
        // Stop any existing flash and start a new one
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashInverted());
    }

    private IEnumerator FlashInverted()
    {
        // Invert colors
        spriteRenderer.color = new Color(-1, -1, -1, 1);
        yield return new WaitForSeconds(0.5f);
        
        // Normal colors
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.5f);
        
        // Invert colors again
        spriteRenderer.color = new Color(-1, -1, -1, 1);
        yield return new WaitForSeconds(0.5f);
        
        // Return to normal
        spriteRenderer.color = Color.white;
        flashCoroutine = null;
    }

    public void DealDamage(float attackDistance)
    {
        //Unused here, only for testing stuff
        return;
    }
}