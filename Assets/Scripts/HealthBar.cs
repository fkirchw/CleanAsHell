using Characters.Player;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private PlayerData playerData;
    private Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    { 

        playerData = FindFirstObjectByType<PlayerData>();

        if (!playerData)
        {
            throw new UnityException("PlayerData not found");
        }

    }

    void Start()
    {
       
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = playerData.HealthPercent;
    }
}
