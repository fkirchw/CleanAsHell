using UnityEngine;
using System.Collections.Generic;

public class Arena : MonoBehaviour
{
    [Header("Enemies Settings")]
    [SerializeField] private List<GameObject> linkedEnemies;
    
    [Header("Objects to Control")]
    [SerializeField] private GameObject[] objectsToAppear; // Apare când intri în arena
    [SerializeField] private GameObject[] objectsToDisappear; // Dispare când toți inamicii mor
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool isActivated = false;
    private bool isCompleted = false;
    
    void Start()
    {
        if (debugMode) Debug.Log($"[Arena] Start called on {gameObject.name}");
        
        // Dezactivează obiectele care trebuie să apară (vor fi activate când intri în arena)
        foreach (GameObject obj in objectsToAppear)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                if (debugMode) Debug.Log($"[Arena] Deactivated {obj.name}");
            }
        }
        
        // Dezactivează și obiectele care trebuie să dispară (vor fi activate când intri în arena)
        foreach (GameObject obj in objectsToDisappear)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                if (debugMode) Debug.Log($"[Arena] Deactivated {obj.name}");
            }
        }
        
        // Dezactivează inamicii la început (vor fi activați când intri în arena)
        foreach (GameObject enemy in linkedEnemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(false);
                if (debugMode) Debug.Log($"[Arena] Deactivated enemy {enemy.name}");
            }
        }
    }
    
    void Update()
    {
        if (!isActivated || isCompleted) return;
        
        CheckEnemiesStatus();
    }
    
    private void CheckEnemiesStatus()
    {
        bool allEnemiesDead = true;
        
        foreach (GameObject enemy in linkedEnemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                allEnemiesDead = false;
                break;
            }
        }
        
        if (allEnemiesDead)
        {
            if (debugMode) Debug.Log($"[Arena] All enemies defeated!");
            CompleteArena();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isActivated)
        {
            if (debugMode) Debug.Log($"[Arena] Player entered trigger on {gameObject.name}");
            ActivateArena();
        }
    }
    
    private void ActivateArena()
    {
        if (isActivated) return;
        
        isActivated = true;
        if (debugMode) Debug.Log($"[Arena] Activating arena on {gameObject.name}");
        
        // Activează toți inamicii
        foreach (GameObject enemy in linkedEnemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
                if (debugMode) Debug.Log($"[Arena] Activated enemy {enemy.name}");
            }
        }
        
        // Activează obiectele care trebuie să apară
        foreach (GameObject obj in objectsToAppear)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                if (debugMode) Debug.Log($"[Arena] Appeared {obj.name}");
            }
            else
            {
                if (debugMode) Debug.LogWarning($"[Arena] Null object in objectsToAppear array");
            }
        }
        
        // Activează și obiectele care trebuie să dispară mai târziu
        foreach (GameObject obj in objectsToDisappear)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                if (debugMode) Debug.Log($"[Arena] Activated {obj.name} (will disappear when arena is completed)");
            }
            else
            {
                if (debugMode) Debug.LogWarning($"[Arena] Null object in objectsToDisappear array");
            }
        }
    }
    
    private void CompleteArena()
    {
        if (isCompleted) return;
        
        isCompleted = true;
        if (debugMode) Debug.Log($"[Arena] Completing arena on {gameObject.name}");
        
        // Dezactivează obiectele care au apărut
        foreach (GameObject obj in objectsToAppear)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                if (debugMode) Debug.Log($"[Arena] Disappeared {obj.name}");
            }
        }
        
        // Dezactivează obiectele care trebuie să dispară
        foreach (GameObject obj in objectsToDisappear)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                if (debugMode) Debug.Log($"[Arena] Disappeared {obj.name}");
            }
        }
    }
    
    // Metodă pentru debugging în inspector
    void OnDrawGizmos()
    {
        if (!debugMode) return;
        
        // Arată zona de trigger
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.color = isActivated ? Color.red : (isCompleted ? Color.green : Color.yellow);
            Gizmos.DrawWireCube(transform.position + (Vector3)collider.offset, collider.size);
        }
        
        // Arată conexiunile cu inamicii
        Gizmos.color = Color.red;
        foreach (GameObject enemy in linkedEnemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawLine(transform.position, enemy.transform.position);
            }
        }
    }
}