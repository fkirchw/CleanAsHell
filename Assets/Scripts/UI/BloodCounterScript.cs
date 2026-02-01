using GameManager;
using TMPro;
using UnityEngine;

namespace UI
{
    public class BloodCounterScript : MonoBehaviour
    {
        public TMP_Text bloodCounter;
    
        private void Start()
        {
            UpdateBloodCounter();
        }
    
        private void OnEnable()
        {
            UpdateBloodCounter();
        }
    
        void Update()
        {
            UpdateBloodCounter();
        }
    
        void UpdateBloodCounter()
        {
            if (LevelStateManager.Instance != null && bloodCounter != null)
            {
                // Folosește noua metodă GetDisplayBloodCounter()
                bloodCounter.text = LevelStateManager.Instance.GetDisplayBloodCounter().ToString();
            }
        }
    }
}