using Characters.Player;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private PlayerData playerData;
    private Slider slider;
    
    void Start()
    {
        playerData = FindFirstObjectByType<PlayerData>();

        if (!playerData)
        {
            throw new UnityException("PlayerData not found");
        }

        slider = GetComponent<Slider>();
        
        // Initialize slider max value
        slider.maxValue = 1f; // For percent
        slider.value = playerData.HealthPercent;
    }

    void Update()
    {
        // Update health percent every frame
        slider.value = playerData.HealthPercent;
    }
}