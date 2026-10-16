using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Class")]
    [SerializeField] private Audio[] audios;

    [Header("AudioSource")]
    public AudioSource sfxAudioSource;
    public AudioSource backgroundAudioSource;

    [Header("Array")]
    public AudioClip[] audioClips;

    [Header("ScrollBar")]
    public GameObject musicVolumeBar;
    public GameObject sfxVolumeBar;

    private int audioIndex = 0;

    [Header("SO")]
    [SerializeField] private Data allData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if(SceneManager.GetActiveScene().name == "Scene1")
        {

            StartCoroutine(GetVoumeValue());

            // Get a random index within the range of the audioClips array
            audioIndex = UnityEngine.Random.Range(0, audioClips.Length);

            // Get the AudioClip at the random index
            AudioClip randomClip = audioClips[audioIndex];

            ChangeBackgroundMusic(randomClip);


        }
    }

    private void Update()
    {
        if (backgroundAudioSource != null)
            CheckAndPlayNextBackgroundMusic(); // Check and update the music in each frame

    }

    /// <summary>
    /// Gives a delay to fetch the value of volume from another scene.
    /// </summary>
    private IEnumerator GetVoumeValue()
    {
        yield return new WaitForSeconds(0.1f);

        musicVolumeBar.GetComponent<Scrollbar>().value = allData.volumeValue;
    }


    /// <summary>
    /// This is used to play music on Audio Source.
    /// </summary>
    public void PlayMusic(string musicName)
    {
        Audio audio = Array.Find(audios, x => x.audioClipName.Equals(musicName));

        if (audio == null)
            return;

        sfxAudioSource.clip = audio.audioClip;
        sfxAudioSource.Play();
    }

    /// <summary>
    /// This is used to change background music.
    /// </summary>
    public void ChangeBackgroundMusic(AudioClip audio)
    {
        // Audio audio = Array.Find(audioClips, x => x.audioClipName.Equals(musicName));

        // if (audio == null)
        //     return;

        backgroundAudioSource.clip = audio;
        backgroundAudioSource.Play();
    }

    /// <summary>
    /// Increase/Decrease the volume of audio source, It is called in slider "ValueOnChanged()".
    /// </summary>
    public void MusicVolumeController()
    {
        backgroundAudioSource.volume = musicVolumeBar.GetComponent<Scrollbar>().value;
        allData.volumeValue = musicVolumeBar.GetComponent<Scrollbar>().value;
    }
    public void SFXVolumeController() => sfxAudioSource.volume = sfxVolumeBar.GetComponent<Scrollbar>().value;

    /// <summary>
    /// Method to check if the music has ended and play the next one.
    /// </summary>
    private void CheckAndPlayNextBackgroundMusic()
    {
        if (!backgroundAudioSource.isPlaying)
        {
            if(audioClips.Length == audioIndex - 1)
                audioIndex = 0;
            else
                audioIndex++;
            // Get the AudioClip at the random index
            AudioClip randomClip = audioClips[audioIndex];

            ChangeBackgroundMusic(randomClip);
        }
    }
}



[Serializable]
public class Audio
{
    public string audioClipName;
    public AudioClip audioClip;
}