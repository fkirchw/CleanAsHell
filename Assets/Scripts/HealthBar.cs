using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] PlayerData player;
    private Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        
    }

    void Start()
    {
        if (PlayerData.Instance != null)
        {
            player = PlayerData.Instance;
        }
        else
        {
            Debug.Log("Player Instance not set");
            return;
        }

        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = player.GetHealthPercent();
    }
}
