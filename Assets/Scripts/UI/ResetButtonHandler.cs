using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// Adaugă această linie

namespace UI
{
    public class ResetButtonHandler : MonoBehaviour
    {
        public Button resetButton;
        public LevelSelector levelSelector;
        public TextMeshProUGUI confirmationText; // Acum va funcționa cu using TMPro
        public float confirmationDisplayTime = 2f;
    
        void Start()
        {
            if (resetButton == null)
                resetButton = GetComponent<Button>();
            
            if (levelSelector == null)
                levelSelector = FindObjectOfType<LevelSelector>();
            
            resetButton.onClick.AddListener(OnResetClicked);
        
            if (confirmationText != null)
            {
                confirmationText.gameObject.SetActive(false);
            }
        }
    
        void OnResetClicked()
        {
            if (levelSelector != null)
            {
                levelSelector.ResetAllToZero();
                ShowConfirmationMessage("All upgrades reset to zero!");
                StartCoroutine(ButtonFeedback());
            }
        }
    
        IEnumerator ButtonFeedback()
        {
            Image buttonImage = resetButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color originalColor = buttonImage.color;
                buttonImage.color = Color.red;
                yield return new WaitForSeconds(0.5f);
                buttonImage.color = originalColor;
            }
        }
    
        void ShowConfirmationMessage(string message)
        {
            if (confirmationText != null)
            {
                confirmationText.text = message;
                confirmationText.gameObject.SetActive(true);
                Invoke("HideConfirmationMessage", confirmationDisplayTime);
            }
        }
    
        void HideConfirmationMessage()
        {
            if (confirmationText != null)
            {
                confirmationText.gameObject.SetActive(false);
            }
        }
    }
}