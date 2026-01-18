using UnityEngine;
using Characters.Interfaces;
using System.Collections;

namespace Characters.Enemies
{
    public class TrapChest : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] public int health { get; private set; }  = 0;
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

        // This function confirms if Unity's AI detects any collision
        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"Something touched the chest trigger: {collision.name}");
        }

        public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
        {
            if (isDestroyed) return;

            health -= damage;

            Debug.Log($"<color=yellow>TrapChest took {damage} damage!</color> Remaining health: {health}");

            if (health <= 0)
            {
                Debug.Log("<color=red>TrapChest has been destroyed!</color>");
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
            Debug.Log("Chest has been reset.");
        }
    }
}