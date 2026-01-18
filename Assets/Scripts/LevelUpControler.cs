using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LevelUpController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject visualPanel;
    [SerializeField] private Button closeButton;
    
    [Header("Input")]
    [SerializeField] private InputActionReference upgradeActionRef;
    [SerializeField] private InputActionReference cancelActionRef;
    
    private InputAction upgradeAction;
    private InputAction cancelAction;
    private bool canOpenLevelUp = true;

    private void Awake()
    {
        if (upgradeActionRef != null)
        {
            upgradeAction = upgradeActionRef.action;
        }
        else
        {
            Debug.LogError("Upgrade Action Reference not assigned!");
        }
        
        if (cancelActionRef != null)
        {
            cancelAction = cancelActionRef.action;
        }
        else
        {
            Debug.LogError("Cancel Action Reference not assigned!");
        }
    }

    private void OnEnable()
    {
        if (upgradeAction != null)
        {
            upgradeAction.performed += OnUpgradeTriggered;
            upgradeAction.Enable();
        }
        
        if (cancelAction != null)
        {
            cancelAction.performed += OnCancelTriggered;
            cancelAction.Enable();
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseLevelUp);
        }
    }

    private void OnDisable()
    {
        if (upgradeAction != null)
        {
            upgradeAction.performed -= OnUpgradeTriggered;
        }
        
        if (cancelAction != null)
        {
            cancelAction.performed -= OnCancelTriggered;
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseLevelUp);
        }
    }

    private void Start()
    {
        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
        }
        
        Time.timeScale = 1f;
    }

    private void OnUpgradeTriggered(InputAction.CallbackContext context)
    {
        if (!canOpenLevelUp || visualPanel == null) return;

        if (visualPanel.activeSelf)
        {
            CloseLevelUp();
        }
        else
        {
            OpenLevelUp();
        }
    }

    private void OnCancelTriggered(InputAction.CallbackContext context)
    {
        // Only close if menu is open
        if (visualPanel != null && visualPanel.activeSelf)
        {
            CloseLevelUp();
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
            
            // Clear UI selection when closing
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    public void EnableLevelUp(bool enable)
    {
        canOpenLevelUp = enable;
    }
}