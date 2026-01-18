using UnityEngine;

public class PortalScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LevelCompletedScript levelCompleted = FindObjectOfType<LevelCompletedScript>();

            if (levelCompleted != null)
            {
                levelCompleted.ShowLevelCompletedMenu();
            }
        }
    }
}