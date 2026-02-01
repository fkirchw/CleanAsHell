using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameManager
{
    public class BossManager : MonoBehaviour
    {
        [SerializeField] private GameObject bossHealthBarPanel;
    
        void Start()
        {
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(false);
            }
        
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(false);
            }
        }
    
        public void ShowBossHealthBar()
        {
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(true);
            }
        }
    
        public void HideBossHealthBar()
        {
            if (bossHealthBarPanel != null)
            {
                bossHealthBarPanel.SetActive(false);
            }
        }
    }
}