using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Căutăm scriptul LevelCompletedScript în scenă
            LevelCompletedScript levelCompletedMenu = FindObjectOfType<LevelCompletedScript>();
            
            if (levelCompletedMenu != null)
            {
                // 2. Apelăm funcția care afișează meniul de Level Completed
                levelCompletedMenu.ShowLevelCompletedMenu();
                
                // Meniul va aștepta 2 secunde, va afișa panoul, și apoi poți da clic pe NextLevel/MainMenu
            }
            else
            {
                Debug.LogWarning("LevelCompletedScript nu a fost găsit. Încărc BossScene direct ca metodă de rezervă.");
            }
        }
    }
}