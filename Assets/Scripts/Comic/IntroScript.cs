using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Inputs;

public class IntroScript : MonoBehaviour
{
    public Image comicImage = null;
    public Sprite[] panels;

    public float autoDelay = 5f; // 0 = nur manuell
    private float timer = 0f;

    private int nextImgIdx = 0;
    private bool isSwitching = false;

    private InputSystemActions inputActions;

    private void Awake()
    {
        inputActions = new InputSystemActions();
        inputActions.Intro.Enable();
    }

    void Start()
    {
        comicImage.sprite = panels[0];
        nextImgIdx = 1;

        SoundManager.instance.FadeMusicOut(1);
    }

    void Update()
    {
        // Manuell weiterschalten
        if (inputActions.Intro.Next.WasPressedThisFrame())
        {
            timer = 0f;
            ShowNextPanel();
        }

        // Automatisch weiterschalten (nur wenn kein Fade läuft)
        if (autoDelay > 0 && !isSwitching)
        {
            timer += Time.deltaTime;
            if (timer >= autoDelay)
            {
                timer = 0f;
                ShowNextPanel();
            }
        }
    }

    private void ShowNextPanel()
    {
        if (isSwitching) return;

        if (nextImgIdx < panels.Length)
        {
            isSwitching = true;
            StartCoroutine(SwitchPanelWithFade(panels[nextImgIdx]));
        }
        else
        {
            inputActions.Intro.Disable();
            SceneManager.LoadScene("Scenes/Tutorial");
        }
    }

    IEnumerator SwitchPanelWithFade(Sprite nextSprite)
    {
        CanvasGroup cg = comicImage.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = comicImage.gameObject.AddComponent<CanvasGroup>();

        float fadeSpeed = 2f;

        // Fade Out
        for (float t = 1; t > 0; t -= Time.deltaTime * fadeSpeed)
        {
            cg.alpha = t;
            yield return null;
        }

        comicImage.sprite = nextSprite;
        nextImgIdx++;

        // Fade In
        for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
        {
            cg.alpha = t;
            yield return null;
        }

        cg.alpha = 1f;
        isSwitching = false;
    }
}
