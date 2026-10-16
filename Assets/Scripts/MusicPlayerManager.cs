using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class MusicPlayerManager : MonoBehaviour
{
    public static MusicPlayerManager Instance;

    [Header("Array")]
    public AudioClip[] audioClips;

    [Header("AudioSource")]
    private AudioSource audioSource;
    private AudioSource sfxAudioSource;

    [Header("int")]
    private int currentTrackIndex = 0;

    [Header("ScrollBar")]
    public Scrollbar volumeController;

    [Header("GameObject")]
    [SerializeField] private GameObject equalizer;

    [Header("SO")]
    [SerializeField] private Data allData;

    [Header("Class")]
    [SerializeField] private Audio[] audios;

    [Header("GameObject")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject pauseButton;

    private int audioIndex = 0;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sfxAudioSource = this.transform.GetChild(0).GetComponent<AudioSource>();

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
        audioSource.volume = allData.volumeValue;
        PlayRandomTrack();
        equalizer.GetComponent<Animator>().enabled = true;
        sfxAudioSource.volume = allData.sfxVolumeValue;
        StartCoroutine(GetVolumeValue());
    }

    /// <summary>
    /// Gives a delay to fetch the value of volume from another scene.
    /// </summary>
    private IEnumerator GetVolumeValue()
    {
        yield return new WaitForSeconds(0.1f);
        volumeController.value = allData.volumeValue;
    }

    private void Update()
    {
        if (audioSource.isPlaying)
            equalizer.GetComponent<Animator>().enabled = true;
        else if(equalizer.GetComponent<Animator>().enabled == true)
        {
            if(audioClips.Length == audioIndex)
                audioIndex = 0;
            else
                audioIndex++;
            PlayTrack(audioIndex);
        }
        else
            equalizer.GetComponent<Animator>().enabled = false;
    }

    /// <summary>
    /// Go To Previous Sound Track
    /// </summary>
    public void PreviousTrack()
    {
        currentTrackIndex = (currentTrackIndex - 1 + audioClips.Length) % audioClips.Length;
        PlayTrack(currentTrackIndex);

        if (playButton.activeInHierarchy)
        {
            pauseButton.SetActive(true);
            playButton.SetActive(false);
        }
    }


    /// <summary>
    /// Go To Next Sound Track
    /// </summary>
    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % audioClips.Length;
        PlayTrack(currentTrackIndex);

        if (playButton.activeInHierarchy)
        {
            pauseButton.SetActive(true);
            playButton.SetActive(false);
        }
    }

    /// <summary>
    /// Go To Previous Sound Track
    /// </summary>
    public void PlayRandomTrack()
    {
        // Get a random index within the range of the audioClips array
        audioIndex = UnityEngine.Random.Range(0, audioClips.Length);

        currentTrackIndex = audioIndex;
        PlayTrack(currentTrackIndex);
    }

    /// <summary>
    /// Play the Current Track
    /// </summary>
    public void Play()
    {
        audioSource.Pause();
    }


    /// <summary>
    /// Pause the Current Track
    /// </summary>
    public void Pause()
    {
        audioSource.UnPause();
    }


    /// <summary>
    /// Increase/Decrease the volume of audio source, It is called in slider "ValueOnChanged()".
    /// </summary>
    public void VolumeController()
    {
        audioSource.volume = volumeController.value;
        // allData.volumeValue = volumeController.value;
    }


    /// <summary>
    /// Play Sound Track
    /// </summary>
    /// <param name="index"></param>
    private void PlayTrack(int index)
    {
        if (audioClips != null && index >= 0 && index < audioClips.Length)
        {
            audioSource.clip = audioClips[index];
            audioSource.Play();
        }
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


    public void PlayJumpButtonMusic(string musicName)
    {
        if(CockPitManager.Instance.jumpBar.fillAmount == 1)
        {
            Audio audio = Array.Find(audios, x => x.audioClipName.Equals(musicName));

            if (audio == null)
                return;

            sfxAudioSource.clip = audio.audioClip;
            sfxAudioSource.Play();
        }
    }

    // /// <summary>
    // /// Method to check if the music has ended and play the next one.
    // /// </summary>
    // private void CheckAndPlayNextBackgroundMusic()
    // {
    //     if (!audioSource.isPlaying)
    //     {
    //         audioIndex++;
    //         PlayTrack(audioIndex);
    //     }
    // }

}