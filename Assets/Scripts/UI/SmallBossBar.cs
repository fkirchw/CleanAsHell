using Characters.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SmallBossBar : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private SmallBoss smallBoss;
        private Slider slider;
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        //INITIALIZE all UI components in Start, not Awake to avoid timing issues.
        void Start()
        {
            smallBoss = FindFirstObjectByType<SmallBoss>();

            if (!smallBoss)
            {
                throw new UnityException("PlayerData not found");
            }

            slider = GetComponent<Slider>();
        }

        // Update is called once per frame
        void Update()
        {
            slider.value = smallBoss.GetHealthPercent();

            if(slider.value <= 0 )
            {
                Destroy(gameObject);
            }
        }
    }
}
