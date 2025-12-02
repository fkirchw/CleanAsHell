using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [Header("Referințe")]
    [SerializeField] private GameObject visualPanel; 

    void Start()
    {
        // La start, ne asigurăm că totul curge normal
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Ignorăm ESC în meniul principal
        if (SceneManager.GetActiveScene().buildIndex == 0) return;

        // --- LOGICA DE BUTON ESC ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (visualPanel == null) return;

            // Dacă se vede -> îl închidem
            if (visualPanel.activeSelf) 
            {
                Resume();
            }
            // Dacă NU se vede -> îl deschidem
            else
            {
                Pause();
            }
        }

        // --- SOLUȚIA "NUCLEARĂ" (FORȚARE) ---
        // Dacă panoul este deschis, ne asigurăm în FIECARE cadru că timpul e 0.
        // Asta repară bug-ul în care timpul "scapă" de sub control.
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
        
        // Opțional: Deblochează mouse-ul ca să poți da click
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (visualPanel != null) visualPanel.SetActive(false);
        Time.timeScale = 1f; 

        // Opțional: Blochează mouse-ul la loc pentru joc
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0);
    }
}