using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BloodCounterScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_Text bloodCounter;

    private void Start()
    {
        bloodCounter.text = LevelStateManager.Instance.GetBloodCounter().ToString();
    }

    private void OnEnable()
    {
        GameEvents.OnBloodScoreChanged += UpdateScoreText;
    }

    private void OnDisable()
    {
        GameEvents.OnBloodScoreChanged -= UpdateScoreText;
    }

    public void UpdateScoreText(int newScore)
    {
        bloodCounter.text = newScore.ToString();
    }
}
