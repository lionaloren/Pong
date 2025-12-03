using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]

    public AudioSource bgmSource;

    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip bgmMenu;
    public AudioClip sfxUIClick;
    public AudioClip sfxGoal;
    public AudioClip sfxWinLose;
    public AudioClip sfxPaddleHit;
    public AudioClip sfxWallHit;
    public AudioClip sfxGoldenGoal;
    public AudioClip sfxPowerUp;

    private const string MUTE_KEY = "IsMuted";
    private bool isMuted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1; 

        ApplyVolume();

        StartCoroutine(InitializeMuteButtonDisplay());
    }

    IEnumerator InitializeMuteButtonDisplay()
    {
        yield return null; 

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMuteButtons();
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return; 

        }

        bgmSource.clip = clip;
        bgmSource.loop = true;

        if (!bgmSource.isPlaying)
        {
             bgmSource.Play();
        }
    }

    public void StopBGM()
    {

        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {

        if (clip == null || isMuted) return;

        sfxSource.PlayOneShot(clip);
    }

    public bool GetMuteStatus()
    {
        return isMuted;
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyVolume();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMuteButtons();

            if (!isMuted)
            {
                 GameObject activePanel = UIManager.Instance.GetActiveMenuPanel();
                 if (activePanel != null)
                 {
                     UIManager.Instance.ShowPanel(activePanel);
                 }
            }
        }
    }

    private void ApplyVolume()
    {

        float volume = isMuted ? 0f : 1f;

        bgmSource.volume = volume;
        sfxSource.volume = volume;
    }
}
