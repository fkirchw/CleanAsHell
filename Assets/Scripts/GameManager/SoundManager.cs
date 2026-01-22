using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager instance;

    [SerializeField] private AudioSource soundFXObj;

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


    public void PlaySoundFxClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
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

}