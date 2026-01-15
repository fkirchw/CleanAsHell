using UnityEngine;
using Characters.Interfaces;
using System.Collections;

namespace Characters.Enemies
{
    public class TrapChest : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] private int health = 1;
        [SerializeField] private string uniqueID; 
        [SerializeField] private GameObject floorTile; 
        [SerializeField] private GameObject destroyEffect; 

        private bool isDestroyed = false;

        private void Awake()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = "Chest_" + transform.position.ToString();
            }

            if (PlayerPrefs.GetInt(uniqueID, 0) == 1)
            {
                DestroyChestDirectly();
            }
        }

        // Aceasta functie ne confirma daca AI-ul Unity vede vreo coliziune
        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"Ceva a atins Trigger-ul cufarului: {collision.name}");
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            if (isDestroyed) return;

            health -= damage;

            Debug.Log($"<color=yellow>TrapChest a primit {damage} damage!</color> Viata ramasa: {health}");

            if (health <= 0)
            {
                Debug.Log("<color=red>TrapChest a fost distrus!</color>");
                TriggerChestTrap();
            }
        }

        private void TriggerChestTrap()
        {
            isDestroyed = true;
            PlayerPrefs.SetInt(uniqueID, 1);
            PlayerPrefs.Save();

            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }

            if (floorTile != null)
            {
                Destroy(floorTile);
            }

            Destroy(gameObject);
        }

        private void DestroyChestDirectly()
        {
            if (floorTile != null) Destroy(floorTile);
            Destroy(gameObject);
        }

        [ContextMenu("Reset Chest State")]
        public void ResetChest()
        {
            PlayerPrefs.DeleteKey(uniqueID);
            Debug.Log("Cufarul a fost resetat.");
        }
    }
}