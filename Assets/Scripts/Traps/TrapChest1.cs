using System.Collections;
using System.Collections.Generic;
using Characters.Interfaces;
using UnityEngine;

namespace Traps
{
    public class TrapChest1 : MonoBehaviour, IDamageable
    {
        [SerializeField] private GameObject floorTile;
        [SerializeField] private GameObject destroyEffect;
        [SerializeField] private List<TrapChest1> linkedTraps = new List<TrapChest1>();
    
        private BoxCollider2D boxCollider;
        private SpriteRenderer spriteRenderer;
        private bool isDestroyed = false;
        private int health = 0;
    
        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer != -1)
            {
                gameObject.layer = enemyLayer;
            }
        }
    
        public void LinkTrap(TrapChest1 trapToLink)
        {
            if (!linkedTraps.Contains(trapToLink))
            {
                linkedTraps.Add(trapToLink);
            }
        }
    
        public void AutoLinkNeighbors(float maxDistance = 1.5f)
        {
            TrapChest1[] allTraps = FindObjectsOfType<TrapChest1>();
        
            foreach (TrapChest1 trap in allTraps)
            {
                if (trap != this && Vector2.Distance(transform.position, trap.transform.position) <= maxDistance)
                {
                    LinkTrap(trap);
                    trap.LinkTrap(this);
                }
            }
        }
    
        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            if (isDestroyed) return;
        
            TriggerFloorDisappear();
        }

        public int GetMaxHealth()
        {
            return health;
        }

        private void TriggerFloorDisappear()
        {
            isDestroyed = true;
        
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }

            if (floorTile != null)
            {
                Destroy(floorTile);
            }

            if (boxCollider != null)
                boxCollider.enabled = false;
            
            StartCoroutine(DestroyTrapAndLinked());
        }
    
        private IEnumerator DestroyTrapAndLinked()
        {
            yield return StartCoroutine(FadeOut());
        
            foreach (TrapChest1 linkedTrap in linkedTraps)
            {
                if (linkedTrap != null && !linkedTrap.isDestroyed)
                {
                    linkedTrap.isDestroyed = true;
                    linkedTrap.TriggerFloorDisappear();
                }
            }
        
            Destroy(gameObject);
        }
    
        private IEnumerator FadeOut()
        {
            if (spriteRenderer == null)
            {
                Destroy(gameObject);
                yield break;
            }
        
            float duration = 0.5f;
            float elapsed = 0f;
            Color startColor = spriteRenderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / duration);
                yield return null;
            }
        }
    
        public void OnFinishedDeathAniEvent() { }
        public void OnFinishedHurtAniEvent() { }
        public void OnDamageDeltAniEvent() { }
    }
}