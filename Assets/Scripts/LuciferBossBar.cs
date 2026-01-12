using Characters.Enemies;
using Characters.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LuciferBossBar : MonoBehaviour
{
    private LuciferController luciferController;
    private Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //INITIALIZE all UI components in Start, not Awake to avoid timing issues.
    void Start()
    {
        luciferController = FindFirstObjectByType<LuciferController>();

        if (!luciferController)
        {
            throw new UnityException("PlayerData not found");
        }

        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = luciferController.GetHealthPercent();

        if (slider.value <= 0)
        {
            Destroy(gameObject);
        }
    }
}
