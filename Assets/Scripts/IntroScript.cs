using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroScript : MonoBehaviour
{
    public Image comicImage = null;         // Das UI-Image im Canvas
    public Sprite[] panels;          // Deine Comic-Bilder in Reihenfolge
    private int nextImgIdx = 0;

    public float autoDelay = 5f;     // 0 = manuell per Klick; > 0 = automatisch
    private float timer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        comicImage.sprite = panels[0];

            nextImgIdx++;
        
    }

    // Update is called once per frame
    void Update()
    {
       

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            timer = 0f;
            ShowNextPanel();
        }

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

        float fadeSpeed = 2f;

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
            SceneManager.LoadScene("Level01");
        }
    }

}
