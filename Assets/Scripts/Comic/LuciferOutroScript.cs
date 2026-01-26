using Inputs;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LuciferOutroScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Image comicImage = null;         // Das UI-Image im Canvas
    public Sprite[] panels;          // Deine Comic-Bilder in Reihenfolge
    private int nextImgIdx = 0;

    public float autoDelay = 5f;     // 0 = manuell per Klick; > 0 = automatisch
    private float timer = 0f;
    private InputSystemActions inputActions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        comicImage.sprite = panels[0];

        nextImgIdx++;

    }

    private void Awake()
    {
        inputActions = new InputSystemActions();
        inputActions.Intro.Enable();
    }

    // Update is called once per frame
    void Update()
    {

        // Automatisch weiterschalten
        if (autoDelay > 0)
        {
            timer += Time.deltaTime;
            if (timer >= autoDelay)
            {
                ShowNextPanel();
                timer = 0f;
            }

        }

    }
    IEnumerator SwitchPanelWithFade(Sprite nextSprite)
    {
        CanvasGroup cg = comicImage.GetComponent<CanvasGroup>();
        if (cg == null) cg = comicImage.gameObject.AddComponent<CanvasGroup>();

        float fadeSpeed = 1.5f;

        // Fade out
        for (float t = 1; t > 0; t -= Time.deltaTime * fadeSpeed)
        {
            cg.alpha = t;
            yield return null;
        }

        // Sprite wechseln wenn ausgeblendet
        comicImage.sprite = nextSprite;

        // Fade in
        for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
        {
            cg.alpha = t;
            yield return null;
        }

        cg.alpha = 1;


        nextImgIdx++;


    }
    private void ShowNextPanel()
    {
        if (nextImgIdx < panels.Length)
        {
            StartCoroutine(SwitchPanelWithFade(panels[nextImgIdx]));
        }
        else
        {
           inputActions.Intro.Disable();
           SceneManager.LoadScene("Scenes/Main Menu");
        }
    }
}
