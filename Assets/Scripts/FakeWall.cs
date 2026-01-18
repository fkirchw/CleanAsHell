using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Characters.Interfaces;
using Blood;

public class FakeWall : MonoBehaviour, IDamageable
{
    [Header("Wall Settings")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private List<FakeWall> linkedWalls = new List<FakeWall>();
    [SerializeField] private bool spawnBlood = false;
    
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private bool isDestroyed = false;
    private int health = 0;
    
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // SET ENEMY LAYER AT RUNTIME
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            gameObject.layer = enemyLayer;
            if (debugMode) Debug.Log($"FakeWall {name}: Set layer to Enemy ({enemyLayer})");
        }
        
        if (debugMode)
        {
            Debug.Log($"=== FAKEWALL CREATED ===");
            Debug.Log($"Name: {name}");
            Debug.Log($"Position: {transform.position}");
            Debug.Log($"Linked walls: {linkedWalls.Count}");
        }
    }
    
    // Method to link walls manually or via script
    public void LinkWall(FakeWall wallToLink)
    {
        if (!linkedWalls.Contains(wallToLink))
        {
            linkedWalls.Add(wallToLink);
        }
    }
    
    // Automatically detects neighboring walls
    public void AutoLinkNeighbors(float maxDistance = 1.5f)
    {
        FakeWall[] allWalls = FindObjectsOfType<FakeWall>();
        
        foreach (FakeWall wall in allWalls)
        {
            if (wall != this && Vector2.Distance(transform.position, wall.transform.position) <= maxDistance)
            {
                LinkWall(wall);
                wall.LinkWall(this); // Reciprocal link
            }
        }
    }
    
    // THIS METHOD MUST BE IDENTICAL TO THE ONE IN FlyingDemonScript
    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        if (isDestroyed) return;
        
        if (debugMode)
        {
            Debug.Log($"=== FAKEWALL DAMAGE RECEIVED ===");
            Debug.Log($"Wall: {name}");
            Debug.Log($"Damage: {damage}");
            Debug.Log($"Linked walls to destroy: {linkedWalls.Count}");
        }
        
        health -= damage;
        
        // BLOOD EFFECT
        if (spawnBlood && BloodSystem.Instance != null)
        {
            BloodSystem.Instance.OnEnemyHit(transform.position, knockbackDir, true, damage);
        }
        
        // FLASH EFFECT
        StartCoroutine(HitFlash());
        
        if (health <= 0)
        {
            isDestroyed = true;
            DestroyWallAndLinked();
        }
    }

    public int GetMaxHealth()
    {
        return health;
    }

    private void DestroyWallAndLinked()
    {
        if (debugMode) Debug.Log($"Destroying FakeWall: {name} and {linkedWalls.Count} linked walls");
        
        // Destroy this wall
        StartCoroutine(DestroySingleWall(this));
        
        // Destroy all linked walls
        foreach (FakeWall linkedWall in linkedWalls)
        {
            if (linkedWall != null && !linkedWall.isDestroyed)
            {
                linkedWall.isDestroyed = true;
                StartCoroutine(DestroySingleWall(linkedWall));
            }
        }
    }
    
    private IEnumerator DestroySingleWall(FakeWall wall)
    {
        if (wall.boxCollider != null)
            wall.boxCollider.enabled = false;
            
        yield return StartCoroutine(wall.FadeOut());
        
        Destroy(wall.gameObject);
    }
    
    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
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
        
        Destroy(gameObject);
    }
    
    public void OnFinishedDeathAniEvent() { }
    public void OnFinishedHurtAniEvent() { }
    public void OnDamageDeltAniEvent() { }
}