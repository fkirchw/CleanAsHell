using UnityEngine;
using UnityEngine.UI;

namespace Blood
{
    /// <summary>
    /// Displays blood statistics in the UI.
    /// Useful for objectives like "Clean 90% of the blood"
    /// </summary>
    public class BloodUIDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text bloodPercentageText;
        [SerializeField] private Text bloodTotalText;
        [SerializeField] private Text bloodyCellsText;
        [SerializeField] private Slider bloodPercentageSlider;
    
        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.5f; // Update twice per second
    
        private float updateTimer = 0f;

        void Update()
        {
            updateTimer += Time.deltaTime;
        
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateDisplay();
            }
        }

        void UpdateDisplay()
        {
            if (!BloodSystem.Instance)
                return;
        
            float percentage = BloodSystem.Instance.GetBloodPercentage();
            float total = BloodSystem.Instance.GetTotalBlood();
            int cellCount = BloodSystem.Instance.GetBloodyCellCount();
        
            if (bloodPercentageText)
            {
                bloodPercentageText.text = $"Blood: {percentage:F1}%";
            }
        
            if (bloodTotalText)
            {
                bloodTotalText.text = $"Total Blood: {total:F2}";
            }
        
            if (bloodyCellsText)
            {
                bloodyCellsText.text = $"Dirty Tiles: {cellCount}";
            }
        
            if (bloodPercentageSlider)
            {
                bloodPercentageSlider.value = percentage / 100f;
            }
        }
    }
}
