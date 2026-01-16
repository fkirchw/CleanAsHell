using Characters.Player;
using UnityEngine;

public class LevelStateManager : MonoBehaviour
{

    private int playerHealth;
    private int bloodCounter;
    private float levelCleaned;
    private int enemiesKilled;
    private int deathCounter;

    private int MAX_HEALTH;

    public static LevelStateManager Instance;

    private PlayerCombat playerCombat;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitilizeHealth();

        deathCounter = 0;
        enemiesKilled = 0;
        bloodCounter = 0;
        levelCleaned = 0;

        MAX_HEALTH = playerHealth;
 
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


    // Update is called once per frame
    public int GetPlayerHealth() => playerHealth;
    public float GetBloodCounter() => bloodCounter;

    public  float GetLevelCleaned() => levelCleaned;

    public int GetEnemiesKilled() => enemiesKilled;

    public int GetDeathCounter() => deathCounter;

    public void SetPlayerHealth(int health)
    {
        playerHealth = health;
    }

    public void SetBloodCounter(int bloodCount)
    {
        

        bloodCounter = bloodCount;
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
        bloodCounter = 0;
        levelCleaned = 0;
    }

    

}
