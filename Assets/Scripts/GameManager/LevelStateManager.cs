using Characters.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStateManager : MonoBehaviour
{
    
    private int playerHealth;
    private int bloodCounter;
    private int allLevelsBloodCollected = 0;
    private float levelCleaned;
    private int enemiesKilled;
    private int deathCounter;

    //Audio Settings
    private float masterAudioValue = 1f;
    private float sfxAudioValue = 1f;
    private float musicAudioValue = 1f;

    private int MAX_HEALTH;
    
    private int[] upgradeLevels = new int[5];
    
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
        
        Debug.Log($"Reset after Tutorial: Health={playerHealth}, Blood reset to 0");
    }
    
    bool isNewSceneGame = IsGameScene(sceneName);
    bool wasPreviousSceneGame = IsGameScene(currentSceneName);
    
    if (isNewSceneGame)
    {
      
        if (wasPreviousSceneGame && currentSceneName != sceneName)
        {

            allLevelsBloodCollected += bloodCounter;
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
    
    public int GetDisplayBloodCounter() => (allLevelsBloodCollected + bloodCounter) - totalUpgradeCostSpent;
    
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

    public void ResetSessionData()
    {
        bloodCounter = 0;
        allLevelsBloodCollected = 0;
        totalUpgradeCostSpent = 0;
        levelCleaned = 0;
        enemiesKilled = 0;
        deathCounter = 0;
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
        heavyAttackBonus = upgradeLevels[1] * 2; 
        lightAttackBonus = upgradeLevels[2] * 1; 
        cleaningRangeMultiplier = 1f + (upgradeLevels[3] * 0.25f);
        cleaningRegenerationBonus = upgradeLevels[4] * 0.5f;
        
        MAX_HEALTH = 10 + (int)vitalityHealthBonus;
        
    }
    
    public float GetVitalityHealthBonus() => vitalityHealthBonus;
    public int GetHeavyAttackBonus() => heavyAttackBonus;
    public int GetLightAttackBonus() => lightAttackBonus;
    public float GetCleaningRangeMultiplier() => cleaningRangeMultiplier;
    public float GetCleaningRegenerationBonus() => cleaningRegenerationBonus;

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
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public float GetMusicAudioValue()
    {
        return musicAudioValue;
    }

    public float GetMasterAudioValue()
    {
        return masterAudioValue;
    }

    public float GetSfxAudioValue()
    {
        return sfxAudioValue;
    }

    public void SetMusicAudioValue(float level)
    {
        musicAudioValue = level;
    }

    public void SetMasterAudioValue(float level)
    {
        masterAudioValue = level;
    }

    public void SetSfxAudioValue(float level)
    {
        sfxAudioValue = level;
    }
}