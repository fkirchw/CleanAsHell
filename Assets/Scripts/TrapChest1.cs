using UnityEngine;

public class TrapChest1 : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string uniqueID; // ID unic pentru salvare
    [SerializeField] private GameObject floorTile; // Obiectul FloorChest care dispare
    [SerializeField] private GameObject destroyEffect; // Opțional: particule/explozie

    private void Awake()
    {
        // Generăm un ID automat dacă este gol
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = "FloorTrap_" + transform.position.x + "_" + transform.position.y;
        }

        // Dacă a fost deja activată în trecut, o ștergem direct
        if (PlayerPrefs.GetInt(uniqueID, 0) == 1)
        {
            RemoveTrapDirectly();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificăm dacă jucătorul a intrat în trigger
        if (collision.CompareTag("Player"))
        {
            TriggerFloorDisappear();
        }
    }

    private void TriggerFloorDisappear()
    {
        // Salvăm starea în cache
        PlayerPrefs.SetInt(uniqueID, 1);
        PlayerPrefs.Save();

        // Spawn efect vizual dacă există
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // Dezactivăm podeaua și obiectul curent
        if (floorTile != null)
        {
            Destroy(floorTile); // Sau floorTile.SetActive(false);
        }

        Debug.Log("<color=orange>Podeaua a dispărut!</color>");
        Destroy(gameObject);
    }

    private void RemoveTrapDirectly()
    {
        if (floorTile != null) Destroy(floorTile);
        Destroy(gameObject);
    }

    [ContextMenu("Reset This Trap")]
    public void ResetTrap()
    {
        PlayerPrefs.DeleteKey(uniqueID);
        Debug.Log("Trap resetată.");
    }
}