using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [Header("Music Clip")]
    [SerializeField] private AudioClip titleMusic;

    public void Start()
    {
        if(SoundManager.instance == null)
        {
            return;
        }
        SoundManager.instance.PlayMusic(titleMusic, true, 0.2f);
    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
