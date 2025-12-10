using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private PlayerData player;
    private Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    { 

        player = FindFirstObjectByType<PlayerData>();

        if (player == null)
        {
            Debug.LogError("PlayerData fehlt auf " + gameObject.name);
            return;
        }

    }

    void Start()
    {
       
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = player.GetHealthPercent();
    }
}
