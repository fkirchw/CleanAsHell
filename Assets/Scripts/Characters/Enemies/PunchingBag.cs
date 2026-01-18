using System.Collections;
using Blood;
using Characters.Interfaces;
using UnityEngine;

namespace Characters.Enemies
{
    public class PunchingBag : MonoBehaviour, IDamageable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool isAirbourne;
        private int health =  0;
        private Material spriteMaterial;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            // Create a unique material instance to avoid affecting other sprites
            spriteMaterial = spriteRenderer.material;
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            BloodSystem.Instance.OnEnemyHit(this.transform.position, knockbackDir, isAirbourne, damage);
        
            // Stop any existing flash and start a new one
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashInverted());
        }

        public int GetMaxHealth()
        {
            return health;
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
    }
}