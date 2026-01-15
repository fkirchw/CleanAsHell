using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject visualPanel;

    private InputAction pauseAction;

    void Start()
    {
        // At start, ensure everything flows normally
        Time.timeScale = 1f;

        // Set up pause action at runtime
        pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
        pauseAction.Enable();
    }

    void Update()
    {
        // Ignore ESC in the main menu
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        // --- ESC BUTTON LOGIC ---
        if (pauseAction.triggered)
        {
            if (visualPanel == null) return;

            // If visible -> close it
            if (visualPanel.activeSelf) 
            {
                Resume();
            }
            // If NOT visible -> open it
            else
            {
                Pause();
            }
        }

        // --- "NUCLEAR" SOLUTION (FORCING) ---
        // If the panel is open, ensure EVERY frame that time is 0.
        // This fixes the bug where time "escapes" control.
        if (visualPanel != null && visualPanel.activeSelf)
        {
            if (Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
            }
        }
    }

    public void Pause()
    {
        if (visualPanel != null) visualPanel.SetActive(true);
        Time.timeScale = 0f; 
        
        // Optional: Unlock the mouse so you can click
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (visualPanel != null) visualPanel.SetActive(false);
        Time.timeScale = 1f; 

    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0);
    }

    private void OnDestroy()
    {
        pauseAction?.Disable();
        pauseAction?.Dispose();
    }
}