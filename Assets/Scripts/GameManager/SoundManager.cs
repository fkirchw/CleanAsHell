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


    private float masterVolume = 1.0f;
    private float sfxVolume = 1.0f;
    private float musicVolume = 0.3f;
    
    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (LevelStateManager.Instance == null)
        {
            return;
        }

       

        musicVolume = LevelStateManager.Instance.GetMusicAudioValue();
        sfxVolume = LevelStateManager.Instance.GetSfxAudioValue();
        masterVolume = LevelStateManager.Instance.GetMasterAudioValue();
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

    public void OnDestroy()
    {
        if(LevelStateManager.Instance == null)
        {
            return;
        }

        LevelStateManager.Instance.SetMusicAudioValue(musicVolume);
        LevelStateManager.Instance.SetSfxAudioValue(sfxVolume);
        LevelStateManager.Instance.SetMasterAudioValue(masterVolume);
    }

    private AudioSource musicSource;
    public void PlayMusic(AudioClip musicClip, bool loop, float controlVolume)
    {
        if (musicObj == null) return;

        float volume = sfxVolume * masterVolume * controlVolume;


        musicSource = Instantiate(musicObj, transform.position, Quaternion.identity);
        // gleiche Musik schon aktiv

        musicSource.volume = musicVolume;

        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.Play();
    }

    private void UpdateMusicVolume()
    {
        if(musicSource == null) return;
        musicSource.volume = musicVolume * masterVolume;
    }

}


