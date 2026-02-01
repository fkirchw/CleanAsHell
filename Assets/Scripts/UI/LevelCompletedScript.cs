using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LevelCompletedScript : MonoBehaviour
{
    private GameObject playerObject;
    [SerializeField] private GameObject visualPanel;
    [SerializeField] private GameObject victoryPanelPrefab;
    [SerializeField] private bool alwaysShowS;
    [SerializeField] private bool resetStateAfterLevel;
    private bool menuShown = false;
    private bool sceneLoading = false;
    private float levelStartTime;
    private int totalEnemiesInLevel;

    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        levelStartTime = Time.time;
        totalEnemiesInLevel = CountAllEnemiesInLevel();
        
        if (playerObject == null)
        {
            Debug.LogError("Player object with tag 'Player' not found");
        }
        
        if (visualPanel == null)
        {
            visualPanel = GameObject.Find("VictoryPanel");
            
            if (visualPanel == null)
            {
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "VictoryPanel" && obj.scene.name == SceneManager.GetActiveScene().name)
                    {
                        visualPanel = obj;
                        break;
                    }
                }
            }
            
            if (visualPanel == null)
            {
                Debug.Log("VictoryPanel not found in scene. It might be a prefab.");
            }
        }
        
        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
        }
    }

    private int CountAllEnemiesInLevel()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        List<GameObject> uniqueEnemies = new List<GameObject>();
        
        foreach (GameObject enemy in enemies)
        {
            Transform root = enemy.transform;
            while (root.parent != null && root.parent.CompareTag("Enemy"))
            {
                root = root.parent;
            }
            
            if (!uniqueEnemies.Contains(root.gameObject))
            {
                uniqueEnemies.Add(root.gameObject);
            }
        }
        
        Debug.Log($"Total unique enemies found in level: {uniqueEnemies.Count}");
        return uniqueEnemies.Count;
    }

    public void ShowLevelCompletedMenu()
    {
        if (menuShown) return;

        if (visualPanel == null)
        {
            if (victoryPanelPrefab != null)
            {
                visualPanel = Instantiate(victoryPanelPrefab);
                visualPanel.name = "VictoryPanel";
            }
            else
            {
                visualPanel = GameObject.Find("VictoryPanel");
                
                if (visualPanel == null)
                {
                    Debug.LogError("Cannot show menu: VictoryPanel not found and no prefab assigned!");
                    return;
                }
            }
        }

        if (playerObject != null)
        {
            MonoBehaviour[] scripts = playerObject.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                script.enabled = false;
            }
            
            Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
            }
        }
        else
        {
            Debug.LogWarning("Player object is null when trying to show level completed menu");
        }
        
        UpdateScoreTexts();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        menuShown = true;
        visualPanel.SetActive(true);
        
        if (Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }
    }

    private void UpdateScoreTexts()
    {
        if (visualPanel == null || LevelStateManager.Instance == null) return;

        string timeFormatted = GetFormattedTime();
        float bloodScore = LevelStateManager.Instance.GetCurrentLevelBloodCounter();
        float cleanedScore = LevelStateManager.Instance.GetLevelCleaned();
        int enemiesKilled = LevelStateManager.Instance.GetEnemiesKilled();
        string enemiesFormatted = $"{enemiesKilled}/{totalEnemiesInLevel}";
        
        float timeMultiplier = CalculateTimeMultiplier();
        float enemyMultiplier = CalculateEnemyMultiplier(enemiesKilled);
        float cleanedMultiplier = CalculateCleanedMultiplier(cleanedScore);
        
        float totalScore = enemiesKilled * 100 * timeMultiplier * enemyMultiplier * cleanedMultiplier;
        
        string grade = "S";
        if (!alwaysShowS)
        {
            grade = CalculateGrade(totalScore);
        }

        SetTextIfExists("TotalTimeScore", timeFormatted);
        SetTextIfExists("TotalBloodScore", bloodScore.ToString("F0"));
        SetTextIfExists("LevelCleaneadScore", cleanedScore.ToString("F0") + "%");
        SetTextIfExists("EnemiesKilledScore", enemiesFormatted);
        SetTextIfExists("TotalScore", grade);
    }

    private float CalculateTimeMultiplier()
    {
        float elapsedTime = Time.time - levelStartTime;
        
        if (elapsedTime <= 60f) return 2.0f;
        if (elapsedTime <= 120f) return 1.5f;
        if (elapsedTime <= 180f) return 1.2f;
        if (elapsedTime <= 240f) return 1.0f;
        if (elapsedTime <= 300f) return 0.8f;
        return 0.5f;
    }

    private float CalculateEnemyMultiplier(int enemiesKilled)
    {
        if (totalEnemiesInLevel == 0) return 1.0f;
        
        float killPercentage = (float)enemiesKilled / totalEnemiesInLevel;
        
        if (killPercentage >= 1.0f) return 2.0f;
        if (killPercentage >= 0.9f) return 1.5f;
        if (killPercentage >= 0.75f) return 1.2f;
        if (killPercentage >= 0.5f) return 1.0f;
        if (killPercentage >= 0.25f) return 0.8f;
        return 0.5f;
    }

    private float CalculateCleanedMultiplier(float cleanedScore)
    {
        if (cleanedScore >= 95f) return 1.5f;
        if (cleanedScore >= 85f) return 1.2f;
        if (cleanedScore >= 70f) return 1.0f;
        if (cleanedScore >= 50f) return 0.8f;
        if (cleanedScore >= 30f) return 0.6f;
        return 0.4f;
    }

private string CalculateGrade(float totalScore)
{
    if (totalEnemiesInLevel == 0)
    {
        return "S";
    }
    
    int enemiesKilled = LevelStateManager.Instance.GetEnemiesKilled();
    
    bool allEnemiesKilled = enemiesKilled >= totalEnemiesInLevel;
    
    float killPercentage = totalEnemiesInLevel > 0 ? (float)enemiesKilled / totalEnemiesInLevel : 1f;
    bool atLeast90PercentKilled = killPercentage >= 0.9f;

    if (totalScore >= 1200f)
    {
        return allEnemiesKilled ? "S" : "A+";
    }
    
    if (totalScore >= 1000f)
    {
        return atLeast90PercentKilled ? "A+" : "B+";
    }
    
    if (totalScore >= 900f)
    {
        return atLeast90PercentKilled ? "A" : "B";
    }
    
    if (totalScore >= 800f)
    {
        return atLeast90PercentKilled ? "A-" : "B-";
    }
    
    if (totalScore >= 700f) return "B+";
    if (totalScore >= 600f) return "B";
    if (totalScore >= 500f) return "B-";
    if (totalScore >= 400f) return "C+";
    if (totalScore >= 300f) return "C";
    if (totalScore >= 200f) return "C-";
    if (totalScore >= 150f) return "D+";
    if (totalScore >= 100f) return "D";
    if (totalScore >= 50f) return "D-";
    return "F";
}

    private string GetFormattedTime()
    {
        float elapsedTime = Time.time - levelStartTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void SetTextIfExists(string gameObjectName, string textValue)
    {
        Transform textTransform = FindInChildren(visualPanel.transform, gameObjectName);
        if (textTransform != null)
        {
            TextMeshProUGUI textComponent = textTransform.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = textValue;
            }
            else
            {
                Debug.LogWarning($"TextMeshProUGUI component not found on {gameObjectName}");
            }
        }
        else
        {
            Debug.LogWarning($"GameObject {gameObjectName} not found in VictoryPanel");
        }
    }

    private Transform FindInChildren(Transform parent, string name)
    {
        if (parent.name == name) return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindInChildren(child, name);
            if (result != null) return result;
        }

        return null;
    }
    
    public void MainMenu()
    {
        if (sceneLoading) return;
        
        sceneLoading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    
    public void NextLevel()
    {
        if (sceneLoading) return;
        
        sceneLoading = true;
        
        if(resetStateAfterLevel)
        {
            LevelStateManager.Instance.ResetAllGameData();
        }
        
        StartCoroutine(LoadNextLevelAsync());
    }
    

    private IEnumerator LoadNextLevelAsync()
    {
        Time.timeScale = 1f;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                break;
            }

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        asyncLoad.allowSceneActivation = true;
    }
}