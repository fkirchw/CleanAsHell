using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompletedScript : MonoBehaviour
{
    private GameObject playerObject;
    [SerializeField] private GameObject visualPanel;
    private bool menuShown = false;

    void Start()
    {
        // Găsește player-ul folosind tag
        playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject == null)
        {
            Debug.LogError("Player object with tag 'Player' not found");
        }
        
        visualPanel.SetActive(false); 
    }

    public void ShowLevelCompletedMenu()
    {
        if (menuShown) return; 

        if (playerObject != null)
        {
            // Oprește toate scripturile de pe player
            MonoBehaviour[] scripts = playerObject.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                script.enabled = false;
            }
            
            // Oprește fizica
            Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true; 
            }
        }
        else
        {
            Debug.LogWarning("Player object is null when trying to show level completed menu");
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 
        menuShown = true;
        visualPanel.SetActive(true);
        
        if (visualPanel != null && visualPanel.activeSelf)
        {
            if (Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
            }
        }
    }
    
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
    
    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1f;
    }
}