using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonHandler : MonoBehaviour
{
    public Button upgradeButton;
    public Button cancelButton;
    public LevelSelector levelSelector;

    void Start()
    {
        if (upgradeButton == null)
            upgradeButton = GetComponent<Button>();

        if (levelSelector == null)
            levelSelector = FindObjectOfType<LevelSelector>();

        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        UpdateButtonInteractability();
    }

    void Update()
    {
        if (levelSelector != null)
        {
            UpdateButtonInteractability();
        }
    }

    void UpdateButtonInteractability()
    {
        if (levelSelector != null && upgradeButton != null)
        {
            Image buttonImage = upgradeButton.GetComponent<Image>();
            Color originalColor = buttonImage.color;
            if (levelSelector.HasChanges())
            {
                // Normal color
                buttonImage.color = originalColor;
            }
            else
            {
                // Gray out but keep interactable
                buttonImage.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
            }
        }
    }

    void OnUpgradeClicked()
    {
        if (levelSelector != null && levelSelector.HasChanges())
        {
            levelSelector.ApplyUpgrade();

            StartCoroutine(ButtonFeedback(upgradeButton));
        }
    }

    void OnCancelClicked()
    {
        if (levelSelector != null)
        {
            levelSelector.CancelUpgrade();

            if (cancelButton != null)
            {
                StartCoroutine(ButtonFeedback(cancelButton));
            }
        }
    }

    System.Collections.IEnumerator ButtonFeedback(Button button)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color originalColor = buttonImage.color;
            yield return new WaitForSeconds(0.3f);
            buttonImage.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }
}