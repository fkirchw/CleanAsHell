using UnityEngine;

/// <summary>
/// Example component for player to clean blood.
/// Attach to player GameObject.
/// </summary>
public class BloodCleaner : MonoBehaviour
{
    [Header("Cleaning Settings")]
    [SerializeField] private float cleanRadius = 2f;
    [SerializeField] private float cleanRate = 0.5f; // Blood cleaned per second
    [SerializeField] private KeyCode cleanKey = KeyCode.E;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject cleaningEffect; // Optional particle effect
    
    private bool isCleaning = false;

    void Update()
    {
        // Check if player is holding clean button
        if (Input.GetKey(cleanKey))
        {
            if (!isCleaning)
            {
                StartCleaning();
            }
            
            PerformCleaning();
        }
        else if (isCleaning)
        {
            StopCleaning();
        }
    }

    void StartCleaning()
    {
        isCleaning = true;
        
        if (cleaningEffect != null)
        {
            cleaningEffect.SetActive(true);
        }
    }

    void StopCleaning()
    {
        isCleaning = false;
        
        if (cleaningEffect != null)
        {
            cleaningEffect.SetActive(false);
        }
    }

    void PerformCleaning()
    {
        if (BloodSystem.Instance == null)
            return;
        
        // Clean blood around player position
        BloodSystem.Instance.CleanBlood(
            transform.position,
            cleanRadius,
            cleanRate * Time.deltaTime
        );
    }

    void OnDrawGizmosSelected()
    {
        // Draw cleaning radius in editor
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, cleanRadius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cleanRadius);
    }
}
