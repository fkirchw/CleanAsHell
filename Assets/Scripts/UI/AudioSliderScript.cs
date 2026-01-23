using UnityEngine;
using UnityEngine.UI;   // <-- DAS
using UnityEngine.SceneManagement;

public class AudioSliderScript : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider masterSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if(LevelStateManager.Instance == null)
        {
            return;
        }

        if (musicSlider != null)
        {
            musicSlider.value = LevelStateManager.Instance.GetMusicAudioValue();
            //SoundManager.instance.SetMusicVolume(LevelStateManager.Instance.GetMusicAudioValue());
        }

        if(sfxSlider != null)
        {
            sfxSlider.value = LevelStateManager.Instance.GetSfxAudioValue();
            //SoundManager.instance.SetSfxVolume(LevelStateManager.Instance.GetSfxAudioValue());
        }

        if (masterSlider != null)
        {
            masterSlider.value = LevelStateManager.Instance.GetMasterAudioValue();
            //SoundManager.instance.SetMasterVolume(LevelStateManager.Instance.GetMasterAudioValue());
        }
    }

}
