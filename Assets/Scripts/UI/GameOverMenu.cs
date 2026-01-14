using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Characters.Player; // Adaugă acest using pentru a accesa PlayerData

public class GameOverMenu : MonoBehaviour
{
    private PlayerData playerData;
    [SerializeField] private GameObject visualPanel;
    private bool timerStarted = false;
    
    void Start()
    {
        // Găsește PlayerData la fel ca în HealthBar
        playerData = FindFirstObjectByType<PlayerData>();
        
        if (playerData == null)
        {
            Debug.LogError("PlayerData not found");
        }
        
        visualPanel.SetActive(false); 
    }
    
    public void MainMenu()
    {
        SceneManager.LoadSceneAsync(0);
        Time.timeScale = 1f; // Reset time scale dacă l-ai oprit
    }
    
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f; // Reset time scale dacă l-ai oprit
    }

    void Update()
    {
        if (playerData == null) return; 
        
        // Folosește IsDead în loc de DeathStatus()
        if (playerData.IsDead && !timerStarted) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            timerStarted = true;
            
            StartCoroutine(ShowMenuAfterDelay(2f)); 
        }
    }

    private IEnumerator ShowMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); 
        visualPanel.SetActive(true);
        
        // Opțional: Oprește timpul când apare meniul de Game Over
        Time.timeScale = 0f;
    }
}