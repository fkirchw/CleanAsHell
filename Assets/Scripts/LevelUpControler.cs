using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LevelUpControler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject visualPanel;
    [SerializeField] private Button closeButton;
    
    private InputAction levelUpAction;
    private bool canOpenLevelUp = true;

    void Start()
    {
        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseLevelUp);
        }

        levelUpAction = new InputAction("LevelUp", binding: "<Keyboard>/e");
        levelUpAction.Enable();
        
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!canOpenLevelUp) return;

        if (levelUpAction.triggered)
        {
            if (visualPanel == null) return;

            if (visualPanel.activeSelf)
            {
                CloseLevelUp();
            }
            else
            {
                OpenLevelUp();
            }
        }

        if (visualPanel != null && visualPanel.activeSelf)
        {
            if (Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
            }
        }
    }

    public void OpenLevelUp()
    {
        if (visualPanel != null)
        {
            visualPanel.SetActive(true);
            Time.timeScale = 0f;
            
        }
    }

    public void CloseLevelUp()
    {
        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
            Time.timeScale = 1f;
            
        }
    }

    public void EnableLevelUp(bool enable)
    {
        canOpenLevelUp = enable;
    }

    private void OnDestroy()
    {
        levelUpAction?.Disable();
        levelUpAction?.Dispose();
    }
}