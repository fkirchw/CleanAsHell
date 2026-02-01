using Characters.Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class HealthBar : MonoBehaviour
{
    private PlayerData playerData;
    private Slider slider;
    private TextMeshProUGUI hpText;
    
    void Start()
    {
        playerData = FindFirstObjectByType<PlayerData>();

        if (!playerData)
        {
            throw new UnityException("PlayerData not found");
        }

        slider = GetComponent<Slider>();
        
        slider.maxValue = 1f; 
        slider.value = playerData.HealthPercent;
        
        hpText = GameObject.Find("HP")?.GetComponent<TextMeshProUGUI>();
        
        if (!hpText)
        {
            Debug.LogWarning("TextMeshPro with name 'HP' not found");
        }
    }

    void Update()
    {
        slider.value = playerData.HealthPercent;
        
        if (hpText != null)
        {
            hpText.text = $"{playerData.Health}";
        }
    }
}