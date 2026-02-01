using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameManager
{
    public class SoundManager : MonoBehaviour
    {

        public static SoundManager instance;

        [SerializeField] private AudioSource soundFXObj;
        [SerializeField] private AudioSource musicObj;    // F�r Musik

        [Header("Music Clip")]
        [SerializeField] private AudioClip levelMusic;
    
        [SerializeField] private AudioClip titleMusic;


        private bool isMusicPlaying = false;
        private LevelStateManager LevelStateManager => LevelStateManager.Instance;
        
        private void Awake()
        {

            if (instance == null)
            {
                instance = this;

                DontDestroyOnLoad(gameObject);

            } else
            {
                Destroy(gameObject);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
       
            if (scene.name.Contains("Level"))
            {
                PlayMusic(levelMusic, true, 0.1f);
            }

            if (scene.name.Contains("Menu"))
            {
                PlayMusic(titleMusic, true, 0.1f);
            }

        }


        public void PlaySoundFxClip(AudioClip audioClip, Transform spawnTransform, float controlVolume)
        {

            float volume = LevelStateManager.SfxVolume * LevelStateManager.MasterVolume * controlVolume;
            //spawn gameObj
            AudioSource audioSource = Instantiate(soundFXObj, spawnTransform.position, Quaternion.identity);

            //assign the audio
            audioSource.clip = audioClip;
            //assign volume

            audioSource.volume = volume;
            //get length of FX clip

            audioSource.Play();

            float clipLength = audioSource.clip.length;

            Destroy(audioSource.gameObject, clipLength);
        }


        public void FadeMusicOut(float strength)
        {
            float fadeOutSpeed = 1.0f / (strength *10);
            if (musicObj == null) return;

            StartCoroutine(FadeOut(fadeOutSpeed));
        }


        private IEnumerator FadeOut(float fadeOutSpeed)
        {        
            while (musicObj.volume > 0.001f)
            {
                musicObj.volume -= fadeOutSpeed * Time.deltaTime;
                yield return null;
            }

            StopMusic();
        }

        public void StopMusic()
        {
            musicObj.Stop();
            isMusicPlaying = false;
        }

    
        public void SetMasterVolume(float volume)
        {
            LevelStateManager.MasterVolume = volume;
            UpdateMusicVolume();
        }



        public void SetSfxVolume(float volume)
        {
            LevelStateManager.SfxVolume = volume;
        
        }

        public void SetMusicVolume(float volume)
        {
            LevelStateManager.MusicVolume = volume;
            UpdateMusicVolume();
        
        }

        public void PlayMusic(AudioClip musicClip, bool loop, float controlVolume)
        {
        
            if (musicObj == null || isMusicPlaying) return;

            float volume = LevelStateManager.SfxVolume * LevelStateManager.MasterVolume * controlVolume;

            musicObj.volume = LevelStateManager.MusicVolume;

            musicObj.clip = musicClip;
            musicObj.loop = loop;
            musicObj.volume = volume;
            musicObj.Play();
        }

        private void UpdateMusicVolume()
        {
            if(musicObj == null) return;
            musicObj.volume = LevelStateManager.MusicVolume * LevelStateManager.MasterVolume;
        }

        public float GetMusicVolume()
        {
            return LevelStateManager.MusicVolume;
        }

        public float GetMasterVolume()
        {
            return LevelStateManager.MasterVolume;
        }

        public float GetSfxVolume()
        {
            return LevelStateManager.SfxVolume;
        }
        void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

    }
}


