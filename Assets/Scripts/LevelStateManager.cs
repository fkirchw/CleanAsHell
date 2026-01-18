using Characters.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStateManager : MonoBehaviour
{
    private int playerHealth;
    private int bloodCounter; // Total blood collected in the current level
    private int allLevelsBloodCollected = 0; // Total blood collected from all previous levels
    private float levelCleaned;
    private int enemiesKilled;
    private int deathCounter;

    private int MAX_HEALTH;
    
    private int[] upgradeLevels = new int[5]; // Persistent upgrades between scenes
    
    private float vitalityHealthBonus = 0;
    private float heavyAttackMultiplier = 1f;
    private float lightAttackMultiplier = 1f;
    private float cleaningRangeMultiplier = 1f;
    private float cleaningRegenerationBonus = 0;

    private int totalUpgradeCostSpent = 0; // Total spent on upgrades in the current session

    public static LevelStateManager Instance;

    private PlayerCombat playerCombat;
    private string currentSceneName = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Only reset session data on first creation
            ResetSessionData();
            ApplyUpgradeEffects();
            
            // Subscribe to scene change event
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
        bool isNewSceneGame = IsGameScene(scene.name);
        bool wasPreviousSceneGame = IsGameScene(currentSceneName);

        if (isNewSceneGame)
        {
            // If there was a previous scene and it was also a game scene and is different from the current one
            if (wasPreviousSceneGame && currentSceneName != scene.name)
            {
                // Add blood collected in the previous level to the overall total
                allLevelsBloodCollected += bloodCounter;
            }
            // Otherwise, if it's the same game scene, don't add (reload)
            // Or if previously it was a menu, don't add.

            // Reset bloodCounter for the new level (or for reload)
            bloodCounter = 0;
            levelCleaned = 0; // Also reset levelCleaned for the new level
        }
        
        // Update currentSceneName
        currentSceneName = scene.name;
    }
    
    bool IsGameScene(string sceneName)
    {
        // We assume that game scenes don't contain "Menu"
        // You can modify this logic based on your scene names
        return !sceneName.Contains("Menu");
    }
    
    void Start()
    {
        // Initialize health after scene is loaded
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
    
    // Method that returns blood collected from ALL levels MINUS upgrade costs
    public int GetDisplayBloodCounter() => (allLevelsBloodCollected + bloodCounter) - totalUpgradeCostSpent;
    
    // Method that returns only blood collected in the current level
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
        if (GetDisplayBloodCounter() >= amount)
        {
            totalUpgradeCostSpent += amount;
        }
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
    }

    public void ResetStats()
    {
        playerHealth = MAX_HEALTH;
        enemiesKilled = 0;
        levelCleaned = 0;
    }

    // Resets only session data (not upgrades)
    public void ResetSessionData()
    {
        bloodCounter = 0;
        allLevelsBloodCollected = 0;
        totalUpgradeCostSpent = 0;
        levelCleaned = 0;
        enemiesKilled = 0;
        deathCounter = 0;
        
        // DO NOT reset upgrades here - they persist between scenes
    }

    // Resets EVERYTHING (including upgrades)
    public void ResetAllGameData()
    {
        ResetSessionData();
        
        // Also reset upgrades to 0
        for (int i = 0; i < upgradeLevels.Length; i++)
        {
            upgradeLevels[i] = 0;
        }
        
        ApplyUpgradeEffects();
    }
    
    public void SaveUpgradeLevels(int[] levels)
    {
        for (int i = 0; i < levels.Length && i < upgradeLevels.Length; i++)
        {
            upgradeLevels[i] = levels[i];
        }
        
        ApplyUpgradeEffects();
    }
    
    public int[] LoadUpgradeLevels()
    {
        return GetCurrentUpgradeLevels();
    }
    
    void ApplyUpgradeEffects()
    {
        vitalityHealthBonus = upgradeLevels[0] * 10;
        heavyAttackMultiplier = 1f + (upgradeLevels[1] * 0.25f);
        lightAttackMultiplier = 1f + (upgradeLevels[2] * 0.25f);
        cleaningRangeMultiplier = 1f + (upgradeLevels[3] * 0.25f);
        cleaningRegenerationBonus = upgradeLevels[4] * 0.5f;
        
        MAX_HEALTH = 10 + (int)vitalityHealthBonus;
    }
    
    public float GetVitalityHealthBonus() => vitalityHealthBonus;
    public float GetHeavyAttackMultiplier() => heavyAttackMultiplier;
    public float GetLightAttackMultiplier() => lightAttackMultiplier;
    public float GetCleaningRangeMultiplier() => cleaningRangeMultiplier;
    public float GetCleaningRegenerationBonus() => cleaningRegenerationBonus;

    // Method to get current upgrade levels
    public int[] GetCurrentUpgradeLevels()
    {
        int[] levels = new int[upgradeLevels.Length];
        for (int i = 0; i < upgradeLevels.Length; i++)
        {
            levels[i] = upgradeLevels[i];
        }
        return levels;
    }

    void OnDestroy()
    {
        // Unsubscribe from event when object is destroyed
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}