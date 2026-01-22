using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelSelector : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeOption
    {
        public string optionName;
        public Button leftArrow;
        public Button rightArrow;
        public Image[] levelTiles;
        public TextMeshProUGUI levelText;

        [HideInInspector] public int currentLevel = 0;
        [HideInInspector] public int previewLevel = 0;
        [HideInInspector] public int maxLevel = 3;
    }

    public UpgradeOption[] upgradeOptions;
    public Sprite selectedFrame;
    public Sprite normalFrame;
    public Sprite previewFrame;

    public int[] upgradeCosts = new int[] { 20, 50, 100 };
    public float vitalityHealthPerLevel = 10f;
    public float attackDamageMultiplierPerLevel = 0.25f;
    public float cleaningRadiusMultiplierPerLevel = 0.25f;
    public int cleaningRegenerationPerLevel = 1;

    public TextMeshProUGUI bloodCounterText;

    private Dictionary<string, int> originalLevels = new Dictionary<string, int>();
    private int availableBlood = 0;

    void Start()
    {
        LoadUpgradeDataFromStateManager();
        SaveInitialState();
        InitializeAllUpgrades();
        UpdateBloodCounterDisplay();
    }

    void OnEnable()
    {
        UpdateBloodCounterDisplay();
        UpdateAllArrowsInteractability();
    }

    void SaveInitialState()
    {
        originalLevels.Clear();

        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            originalLevels[upgradeOptions[i].optionName] = upgradeOptions[i].currentLevel;
        }
    }

    void LoadUpgradeDataFromStateManager()
    {
        if (LevelStateManager.Instance != null)
        {
            int[] savedLevels = LevelStateManager.Instance.GetCurrentUpgradeLevels();

            for (int i = 0; i < upgradeOptions.Length && i < savedLevels.Length; i++)
            {
                upgradeOptions[i].currentLevel = savedLevels[i];
                upgradeOptions[i].previewLevel = savedLevels[i];
            }
        }
    }

    void InitializeAllUpgrades()
    {
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            int index = i;

            UpdateTilesDisplay(i);

            if (upgradeOptions[i].leftArrow != null)
            {
                upgradeOptions[i].leftArrow.onClick.RemoveAllListeners();
                upgradeOptions[i].leftArrow.onClick.AddListener(() => OnLeftArrowClicked(index));
            }

            if (upgradeOptions[i].rightArrow != null)
            {
                upgradeOptions[i].rightArrow.onClick.RemoveAllListeners();
                upgradeOptions[i].rightArrow.onClick.AddListener(() => OnRightArrowClicked(index));
            }

            UpdateArrowInteractability(i);
        }
    }

    void OnLeftArrowClicked(int upgradeIndex)
    {
        UpgradeOption option = upgradeOptions[upgradeIndex];

        if (option.previewLevel > option.currentLevel)
        {
            option.previewLevel--;
            UpdateTilesDisplay(upgradeIndex);
            UpdateAllArrowsInteractability();
            UpdateBloodCounterDisplay();
        }
    }

    void OnRightArrowClicked(int upgradeIndex)
    {
        UpgradeOption option = upgradeOptions[upgradeIndex];

        if (option.previewLevel >= option.maxLevel)
        {
            // Already maxed out - do nothing or shake
            return;
        }

        int nextLevelCost = GetUpgradeCostForLevel(option.previewLevel, upgradeIndex);
        int currentTotalCost = CalculateTotalUpgradeCost();

        // Check if we can afford it
        if (availableBlood >= currentTotalCost + nextLevelCost)
        {
            // Can afford - apply the upgrade
            option.previewLevel++;
            UpdateTilesDisplay(upgradeIndex);
            UpdateAllArrowsInteractability(); // This now only updates visuals, not interactability
            UpdateBloodCounterDisplay();
        }
        else
        {
            // Can't afford - shake the button
            StartCoroutine(ShakeButton(option.rightArrow.transform));
        }
    }

    private System.Collections.IEnumerator ShakeButton(Transform buttonTransform)
    {
        Vector3 originalPos = buttonTransform.localPosition;
        float shakeDuration = 0.3f;
        float shakeMagnitude = 5f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = originalPos.x + Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = originalPos.y + Random.Range(-shakeMagnitude, shakeMagnitude);
            buttonTransform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.unscaledDeltaTime; // unscaled because you pause with timeScale
            yield return null;
        }

        buttonTransform.localPosition = originalPos;
    }

    int GetUpgradeCostForLevel(int targetLevel, int upgradeIndex)
    {
        if (targetLevel >= 0 && targetLevel < upgradeCosts.Length)
        {
            return upgradeCosts[targetLevel];
        }

        return 0;
    }

    int CalculateTotalUpgradeCost()
    {
        int totalCost = 0;
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            for (int level = upgradeOptions[i].currentLevel; level < upgradeOptions[i].previewLevel; level++)
            {
                totalCost += GetUpgradeCostForLevel(level, i);
            }
        }

        return totalCost;
    }

    void UpdateTilesDisplay(int upgradeIndex)
    {
        UpgradeOption option = upgradeOptions[upgradeIndex];

        for (int i = 0; i < option.levelTiles.Length; i++)
        {
            if (i < option.currentLevel)
            {
                option.levelTiles[i].sprite = selectedFrame;
            }
            else if (i < option.previewLevel)
            {
                option.levelTiles[i].sprite = previewFrame;
            }
            else
            {
                option.levelTiles[i].sprite = normalFrame;
            }
        }

        if (option.levelText != null)
        {
            option.levelText.text = $"Level {option.previewLevel}/{option.levelTiles.Length}";
        }
    }

    void UpdateArrowInteractability(int upgradeIndex)
    {
        UpgradeOption option = upgradeOptions[upgradeIndex];
    
        // Left arrow visual state (but keep interactable)
        if (option.leftArrow != null)
        {
            bool canGoBack = (option.previewLevel > option.currentLevel);
            UpdateButtonVisual(option.leftArrow, canGoBack);
        }
    
        // Right arrow visual state (but keep interactable)
        if (option.rightArrow != null)
        {
            bool canUpgrade = false;
            if (option.previewLevel < option.maxLevel)
            {
                int nextLevelCost = GetUpgradeCostForLevel(option.previewLevel, upgradeIndex);
                int currentTotalCost = CalculateTotalUpgradeCost();
                int bloodNeeded = currentTotalCost + nextLevelCost;
                canUpgrade = (availableBlood >= bloodNeeded);
            }
        
            UpdateButtonVisual(option.rightArrow, canUpgrade);
        }
    }

    void UpdateButtonVisual(Button button, bool affordable)
    {
        // Keep button always interactable
        button.interactable = true;
    
        // Change visual appearance based on affordability
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            if (affordable)
            {
                buttonImage.color = Color.white; // Normal color
            }
            else
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Grayed out
            }
        }
    }

    void UpdateAllArrowsInteractability()
    {
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            UpdateArrowInteractability(i);
        }
    }

    void UpdateBloodCounterDisplay()
    {
        if (LevelStateManager.Instance != null)
        {
            availableBlood = LevelStateManager.Instance.GetDisplayBloodCounter();

            if (bloodCounterText != null)
            {
                int totalCost = CalculateTotalUpgradeCost();
                int remainingBlood = availableBlood - totalCost;
                bloodCounterText.text = $"Blood: {availableBlood}";

                if (totalCost > 0)
                {
                    bloodCounterText.text += $"\nCost: {totalCost}\nRemaining: {remainingBlood}";
                }
            }
        }
    }

    public void ApplyUpgrade()
    {
        int totalCost = CalculateTotalUpgradeCost();

        if (LevelStateManager.Instance != null && LevelStateManager.Instance.GetDisplayBloodCounter() >= totalCost)
        {
            LevelStateManager.Instance.SpendBloodOnUpgrades(totalCost);

            for (int i = 0; i < upgradeOptions.Length; i++)
            {
                upgradeOptions[i].currentLevel = upgradeOptions[i].previewLevel;
                UpdateTilesDisplay(i);
            }

            int[] currentLevels = GetCurrentUpgradeLevels();
            LevelStateManager.Instance.SaveUpgradeLevels(currentLevels);

            SaveInitialState();

            UpdateBloodCounterDisplay();
            UpdateAllArrowsInteractability();
        }
    }

    void ApplyUpgradeEffects()
    {
        if (LevelStateManager.Instance != null)
        {
            LevelStateManager.Instance.SaveUpgradeLevels(GetCurrentUpgradeLevels());
        }
    }

    int[] GetCurrentUpgradeLevels()
    {
        int[] levels = new int[5];
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            if (i < levels.Length)
            {
                levels[i] = upgradeOptions[i].currentLevel;
            }
        }

        return levels;
    }

    public void CancelUpgrade()
    {
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            upgradeOptions[i].previewLevel = originalLevels[upgradeOptions[i].optionName];
            UpdateTilesDisplay(i);
        }

        UpdateAllArrowsInteractability();
        UpdateBloodCounterDisplay();
    }

    public void ResetAllToZero()
    {
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            upgradeOptions[i].currentLevel = 0;
            upgradeOptions[i].previewLevel = 0;
            UpdateTilesDisplay(i);
        }

        if (LevelStateManager.Instance != null)
        {
            LevelStateManager.Instance.ResetAllGameData();
        }

        SaveInitialState();
        UpdateAllArrowsInteractability();
        UpdateBloodCounterDisplay();
    }

    public void ResetToSaved()
    {
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            upgradeOptions[i].previewLevel = upgradeOptions[i].currentLevel;
            UpdateTilesDisplay(i);
        }

        UpdateAllArrowsInteractability();
        UpdateBloodCounterDisplay();
    }

    public bool HasChanges()
    {
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            if (upgradeOptions[i].previewLevel != originalLevels[upgradeOptions[i].optionName])
                return true;
        }

        return false;
    }

    void Update()
    {
        if (LevelStateManager.Instance != null)
        {
            int currentBlood = LevelStateManager.Instance.GetDisplayBloodCounter();
            if (currentBlood != availableBlood)
            {
                availableBlood = currentBlood;
                UpdateBloodCounterDisplay();
                UpdateAllArrowsInteractability();
            }
        }
    }

    void OnDisable()
    {
    }

    public void OnCancelButtonClicked()
    {
        CancelUpgrade();
    }
}