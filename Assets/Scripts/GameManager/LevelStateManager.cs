using Characters.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameManager
{
    public class LevelStateManager : MonoBehaviour
    {
    
        private int playerHealth;
        private int bloodCounter;
        private int allLevelsBloodCollected = 0;
        private float levelCleaned;
        private int enemiesKilled;
        private int deathCounter;
        private int MAX_HEALTH;
    
        private int[] upgradeLevels = new int[5];
        private int[] currentSceneUpgradeLevels = new int[5]; // Track upgrades made in current scene
        private int currentSceneUpgradeCost = 0; // Track upgrade cost spent in current scene
    
        private float vitalityHealthBonus = 0;
        private int heavyAttackBonus = 0; 
        private int lightAttackBonus = 0; 
        private float cleaningRangeMultiplier = 1f;
        private float cleaningRegenerationBonus = 0;
    

        private int totalUpgradeCostSpent = 0;

        public static LevelStateManager Instance;

        private PlayerCombat playerCombat;
        private string currentSceneName = "";

    


        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            
                for (int i = 0; i < upgradeLevels.Length; i++)
                {
                    upgradeLevels[i] = 0;
                    currentSceneUpgradeLevels[i] = 0;
                }
            
                ResetSessionData();
                ApplyUpgradeEffects();
            
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitilizeHealth();
            MAX_HEALTH = playerHealth;
        }
    
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string sceneName = scene.name;
    
            if (sceneName.Contains("Menu"))
            {
                ResetAllGameData();
                currentSceneName = sceneName;
                return;
            }

            bool wasPreviousSceneTutorial = currentSceneName.Contains("Tutorial");
 
            if (wasPreviousSceneTutorial && !sceneName.Contains("Tutorial"))
            {

                playerHealth = MAX_HEALTH;
        

                allLevelsBloodCollected = 0;
                bloodCounter = 0;
                totalUpgradeCostSpent = 0;
                currentSceneUpgradeCost = 0;
        
                Debug.Log($"Reset after Tutorial: Health={playerHealth}, Blood reset to 0");
            }
    
            bool isNewSceneGame = IsGameScene(sceneName);
            bool wasPreviousSceneGame = IsGameScene(currentSceneName);
    
            if (isNewSceneGame)
            {
      
                if (wasPreviousSceneGame && currentSceneName != sceneName)
                {
                    // When moving to next scene, make current scene upgrades permanent
                    for (int i = 0; i < upgradeLevels.Length; i++)
                    {
                        upgradeLevels[i] += currentSceneUpgradeLevels[i];
                        currentSceneUpgradeLevels[i] = 0;
                    }
                    allLevelsBloodCollected += bloodCounter;
                    totalUpgradeCostSpent += currentSceneUpgradeCost;
                    currentSceneUpgradeCost = 0;
                }
                else
                {
                    // Reset current scene upgrades on new game scene (after death)
                    ResetCurrentSceneUpgrades();
                }
        
                if (!sceneName.Contains("Tutorial"))
                {
                    enemiesKilled = 0; 
                }
        
                bloodCounter = 0;
                levelCleaned = 0;
            }
    
            currentSceneName = sceneName;
    
            Debug.Log($"Scene loaded: {sceneName}, EnemiesKilled: {enemiesKilled}, Health: {playerHealth}/{MAX_HEALTH}");
        }
    
        private void ResetCurrentSceneUpgrades()
        {
            // Reset only upgrades made in current scene
            for (int i = 0; i < currentSceneUpgradeLevels.Length; i++)
            {
                currentSceneUpgradeLevels[i] = 0;
            }
            currentSceneUpgradeCost = 0;
            ApplyUpgradeEffects();
        }
    
        bool IsGameScene(string sceneName)
        {
            return !sceneName.Contains("Menu");
        }
    
        void Start()
        {
            InitilizeHealth();
        }
    
        private void InitilizeHealth()
        {
            playerCombat = FindAnyObjectByType<PlayerCombat>();

            if (playerCombat != null)
            {
                playerHealth = playerCombat.Health;
            }
            else
            {
                Debug.LogWarning("PlayerData not found. PlayerHealth not initialized.");
            }
        }

        public int GetPlayerHealth() => playerHealth;
    
        public int GetDisplayBloodCounter() => (allLevelsBloodCollected + bloodCounter) - (totalUpgradeCostSpent + currentSceneUpgradeCost);
    
        public int GetCurrentLevelBloodCounter() => bloodCounter;
    
        public float GetLevelCleaned() => levelCleaned;
        public int GetEnemiesKilled() => enemiesKilled;
        public int GetDeathCounter() => deathCounter;

        public void SetPlayerHealth(int health)
        {
            playerHealth = health;
        }

        public void AddBlood(int amount)
        {
            bloodCounter += amount;
        }

        public void SetBloodCounter(int bloodCount)
        {
            bloodCounter = bloodCount;
        }

        public void SpendBloodOnUpgrades(int amount)
        {
            // Only track current scene upgrade cost
            currentSceneUpgradeCost += amount;
        }

        public void IncreaseEnemiesKilled()
        {
            enemiesKilled++;
        }

        public void SetLevelCleaned(float cleanPercent)
        {
            levelCleaned = cleanPercent;
        }

        public void IncreaseDeathCounter()
        {
            deathCounter++;
            // When player dies, reset current scene upgrades
            ResetCurrentSceneUpgrades();
        }

        public void ResetStats()
        {
            playerHealth = MAX_HEALTH;
            enemiesKilled = 0;
            levelCleaned = 0;
        }

        public void ResetSessionData()
        {
            bloodCounter = 0;
            allLevelsBloodCollected = 0;
            totalUpgradeCostSpent = 0;
            currentSceneUpgradeCost = 0;
            levelCleaned = 0;
            enemiesKilled = 0;
            deathCounter = 0;
            ResetCurrentSceneUpgrades();
        }

        public void ResetAllGameData()
        {
            ResetSessionData();
        
            for (int i = 0; i < upgradeLevels.Length; i++)
            {
                upgradeLevels[i] = 0;
            }
        
            ApplyUpgradeEffects();
        }
    
        public void SaveUpgradeLevels(int[] levels)
        {
            // Save only current scene upgrades
            for (int i = 0; i < levels.Length && i < currentSceneUpgradeLevels.Length; i++)
            {
                currentSceneUpgradeLevels[i] = levels[i];
            }
        
            ApplyUpgradeEffects();
        }
    
        public int[] LoadUpgradeLevels()
        {
            // Return total upgrade levels (base + current scene)
            int[] totalLevels = new int[upgradeLevels.Length];
            for (int i = 0; i < upgradeLevels.Length; i++)
            {
                totalLevels[i] = upgradeLevels[i] + currentSceneUpgradeLevels[i];
            }
            return totalLevels;
        }
    
        void ApplyUpgradeEffects()
        {
            // Calculate using base upgrades + current scene upgrades
            int vitalityLevel = upgradeLevels[0] + currentSceneUpgradeLevels[0];
            int heavyAttackLevel = upgradeLevels[1] + currentSceneUpgradeLevels[1];
            int lightAttackLevel = upgradeLevels[2] + currentSceneUpgradeLevels[2];
            int cleaningRangeLevel = upgradeLevels[3] + currentSceneUpgradeLevels[3];
            int cleaningRegenLevel = upgradeLevels[4] + currentSceneUpgradeLevels[4];
        
            vitalityHealthBonus = vitalityLevel * 10;
            heavyAttackBonus = heavyAttackLevel * 2; 
            lightAttackBonus = lightAttackLevel * 1; 
            cleaningRangeMultiplier = 1f + (cleaningRangeLevel * 0.25f);
            cleaningRegenerationBonus = cleaningRegenLevel * 0.5f;
        
            MAX_HEALTH = 10 + (int)vitalityHealthBonus;
        }
    
        public float GetVitalityHealthBonus() => vitalityHealthBonus;
        public int GetHeavyAttackBonus() => heavyAttackBonus;
        public int GetLightAttackBonus() => lightAttackBonus;
        public float GetCleaningRangeMultiplier() => cleaningRangeMultiplier;
        public float GetCleaningRegenerationBonus() => cleaningRegenerationBonus;

        public int[] GetCurrentUpgradeLevels()
        {
            // Return total upgrade levels
            return LoadUpgradeLevels();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
    }
}