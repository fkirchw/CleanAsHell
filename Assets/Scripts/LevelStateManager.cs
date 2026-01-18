using Characters.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStateManager : MonoBehaviour
{
    private int playerHealth;
    private int bloodCounter; // Total sânge colectat în nivelul curent
    private int allLevelsBloodCollected = 0; // Total sânge colectat din toate nivelurile anterioare
    private float levelCleaned;
    private int enemiesKilled;
    private int deathCounter;

    private int MAX_HEALTH;
    
    private int[] upgradeLevels = new int[5]; // Upgrade-uri persistente între scene
    
    private float vitalityHealthBonus = 0;
    private float heavyAttackMultiplier = 1f;
    private float lightAttackMultiplier = 1f;
    private float cleaningRangeMultiplier = 1f;
    private float cleaningRegenerationBonus = 0;

    private int totalUpgradeCostSpent = 0; // Total cheltuit pe upgrade-uri în sesiunea curentă

    public static LevelStateManager Instance;

    private PlayerCombat playerCombat;
    private string currentSceneName = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Doar la prima creare resetează sesiunea
            ResetSessionData();
            ApplyUpgradeEffects();
            
            // Abonează-te la schimbarea scenei
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
            // Dacă există o scenă anterioară și aceasta a fost tot de joc și este diferită de cea curentă
            if (wasPreviousSceneGame && currentSceneName != scene.name)
            {
                // Adaugă sângele colectat în nivelul anterior la totalul general
                allLevelsBloodCollected += bloodCounter;
            }
            // Altfel, dacă este aceeași scenă de joc, nu adăuga (reîncărcare)
            // Sau dacă anterior a fost meniu, nu adăuga.

            // Resetăm bloodCounter pentru noul nivel (sau pentru reîncărcare)
            bloodCounter = 0;
            levelCleaned = 0; // Resetăm și levelCleaned pentru noul nivel
        }
        
        // Actualizăm currentSceneName
        currentSceneName = scene.name;
    }
    
    bool IsGameScene(string sceneName)
    {
        // Presupunem că scenele de joc nu conțin "Menu"
        // Poți modifica această logică în funcție de numele scenelor tale
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
    
    // Metoda care returnează sânge colectat din TOATE nivelurile MINUS costul upgrade-urilor
    public int GetDisplayBloodCounter() => (allLevelsBloodCollected + bloodCounter) - totalUpgradeCostSpent;
    
    // Metoda care returnează doar sângele colectat în nivelul curent
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

    // Resetează doar datele sesiunii (nu și upgrade-urile)
    public void ResetSessionData()
    {
        bloodCounter = 0;
        allLevelsBloodCollected = 0;
        totalUpgradeCostSpent = 0;
        levelCleaned = 0;
        enemiesKilled = 0;
        deathCounter = 0;
        
        // NU reseta upgrade-urile aici - ele persistă între scene
    }

    // Resetează TOT (inclusiv upgrade-urile)
    public void ResetAllGameData()
    {
        ResetSessionData();
        
        // Resetează și upgrade-urile la 0
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

    // Metodă pentru a obține nivelurile curente de upgrade
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
        // Dezabonează-te de la eveniment când obiectul este distrus
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}