using GameManager;
using UnityEngine;
using UnityEngine.UI;
// <-- DAS

namespace UI
{
    public class AudioSliderScript : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider masterSlider;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

            if(SoundManager.instance == null)
            {
                return;
            }

            if (musicSlider != null)
            {
                musicSlider.value = SoundManager.instance.GetMusicVolume();
                //SoundManager.instance.SetMusicVolume(LevelStateManager.Instance.GetMusicAudioValue());
            }

            if(sfxSlider != null)
            {
                sfxSlider.value = SoundManager.instance.GetSfxVolume();
                //SoundManager.instance.SetSfxVolume(LevelStateManager.Instance.GetSfxAudioValue());
            }

            if (masterSlider != null)
            {
                masterSlider.value = SoundManager.instance.GetMasterVolume();
                //SoundManager.instance.SetMasterVolume(LevelStateManager.Instance.GetMasterAudioValue());
            }
        }

    }
}
