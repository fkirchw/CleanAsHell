using UnityEngine;
using UnityEngine.Tilemaps;

public class FakeWallBg : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int originalLayerOrder = 0; // Order in Layer original
    [SerializeField] private int behindPlayerLayerOrder = -1; // Order in Layer când player-ul este în față
    
    [Header("References")]
    [SerializeField] private TilemapRenderer tilemapRenderer;
    [SerializeField] private Collider2D triggerCollider;
    
    private void Awake()
    {
        // Obține referințele dacă nu sunt setate în inspector
        if (tilemapRenderer == null)
        {
            tilemapRenderer = GetComponent<TilemapRenderer>();
            if (tilemapRenderer == null)
            {
                tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
            }
        }
        
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider == null)
            {
                triggerCollider = GetComponentInChildren<Collider2D>();
            }
        }
        
        // Asigură-te că collider-ul este trigger
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
        
        // Inițializează cu layer order original
        if (tilemapRenderer != null)
        {
            tilemapRenderer.sortingOrder = originalLayerOrder;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifică dacă player-ul a intrat în trigger
        if (other.CompareTag("Player"))
        {
            ChangeTilemapLayerOrder(behindPlayerLayerOrder);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // Verifică dacă player-ul a ieșit din trigger
        if (other.CompareTag("Player"))
        {
            ChangeTilemapLayerOrder(originalLayerOrder);
        }
    }
    
    /// <summary>
    /// Schimbă Sorting Order-ul TilemapRenderer-ului
    /// </summary>
    /// <param name="newOrder">Noul Sorting Order</param>
    private void ChangeTilemapLayerOrder(int newOrder)
    {
        if (tilemapRenderer != null)
        {
            tilemapRenderer.sortingOrder = newOrder;
            
            // Debug pentru a verifica schimbarea
            Debug.Log($"{gameObject.name}: Changed Tilemap Sorting Order to {newOrder}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: TilemapRenderer is missing!");
        }
    }
    
    /// <summary>
    /// Metodă publică pentru a seta manual layer order (pentru scripturi externe)
    /// </summary>
    public void SetLayerOrder(int order)
    {
        originalLayerOrder = order;
        if (tilemapRenderer != null)
        {
            tilemapRenderer.sortingOrder = order;
        }
    }
    
    /// <summary>
    /// Metodă publică pentru a obține layer order-ul curent
    /// </summary>
    public int GetCurrentLayerOrder()
    {
        return tilemapRenderer != null ? tilemapRenderer.sortingOrder : originalLayerOrder;
    }
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        // În editor, actualizează automat Sorting Order-ul
        if (tilemapRenderer != null && !Application.isPlaying)
        {
            tilemapRenderer.sortingOrder = originalLayerOrder;
        }
    }
    
    private void Reset()
    {
        // Automatizează setarea componentelor la adăugarea scriptului
        tilemapRenderer = GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
            tilemapRenderer = GetComponentInChildren<TilemapRenderer>();
        }
        
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider == null)
        {
            // Adaugă un BoxCollider2D dacă nu există
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
        }
    }
    #endif
}