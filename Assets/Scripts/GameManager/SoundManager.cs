using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{

    public static SoundManager instance;

    [SerializeField] private AudioSource soundFXObj;
    [SerializeField] private AudioSource musicObj;    // Für Musik

    [Header("Music Clip")]
    [SerializeField] private AudioClip levelMusic;
    
    [SerializeField] private AudioClip titleMusic;


    private bool isMusicPlaying = false;


    private float masterVolume = 1.0f;
    private float sfxVolume = 1.0f;
    private float musicVolume = 0.3f;

    


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
       
        if (scene.name.Contains("Level01"))
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

        float volume = sfxVolume * masterVolume * controlVolume;
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
        masterVolume = volume;
        UpdateMusicVolume();
    }



    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        UpdateMusicVolume();
        
    }

    /*public void OnDestroy()
    {
        if(LevelStateManager.Instance == null)
        {
            return;
        }

        LevelStateManager.Instance.SetMusicAudioValue(musicVolume);
        LevelStateManager.Instance.SetSfxAudioValue(sfxVolume);
        LevelStateManager.Instance.SetMasterAudioValue(masterVolume);
    }*/

    

    public void PlayMusic(AudioClip musicClip, bool loop, float controlVolume)
    {
        
        if (musicObj == null || isMusicPlaying) return;

        float volume = sfxVolume * masterVolume * controlVolume;

        // gleiche Musik schon aktiv

        musicObj.volume = musicVolume;

        musicObj.clip = musicClip;
        musicObj.loop = loop;
        musicObj.volume = volume;
        musicObj.Play();
    }

    private void UpdateMusicVolume()
    {
        if(musicObj == null) return;
        musicObj.volume = musicVolume * masterVolume;
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }

    public float GetSfxVolume()
    {
        return sfxVolume;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

}


