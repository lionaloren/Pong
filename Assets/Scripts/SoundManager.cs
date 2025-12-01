using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    // Source untuk BGM (looping)
    public AudioSource bgmSource;
    // Source untuk SFX (non-looping, bisa multiple)
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
    
    // Status Mute
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

        // 1. Ambil status mute dari PlayerPrefs saat startup (Default 0 = Tidak Mute)
        isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1; 
        
        // 2. Terapkan volume berdasarkan status mute
        ApplyVolume();
        
        // BARU: Tambahkan Coroutine untuk memastikan UIManager terinisialisasi
        // agar tombol mute bisa langsung terupdate saat game dimulai.
        StartCoroutine(InitializeMuteButtonDisplay());
    }
    
    // Digunakan untuk menunggu UIManager.Instance siap sebelum memanggil UpdateMuteButtons
    IEnumerator InitializeMuteButtonDisplay()
    {
        yield return null; // Tunggu 1 frame
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMuteButtons();
        }
    }
    
    // --- PUBLIK FUNCTOINS ---

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // Cek apakah klip yang sama sedang dimainkan DAN sedang berjalan
        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return; // Jangan restart, biarkan terus berjalan
        }
        
        // Set klip baru
        bgmSource.clip = clip;
        bgmSource.loop = true;
        
        if (!bgmSource.isPlaying)
        {
             bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        // Hanya panggil Stop() saat kita benar-benar ingin musik berhenti total 
        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        // SFX tidak boleh dimainkan jika isMuted = true.
        if (clip == null || isMuted) return;

        // PlayOneShot memungkinkan beberapa SFX dimainkan secara bersamaan
        sfxSource.PlayOneShot(clip);
    }

    public bool GetMuteStatus()
    {
        return isMuted;
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        // Simpan status mute
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
        PlayerPrefs.Save();

        // Terapkan perubahan volume (BGM dan SFX)
        ApplyVolume();
        
        // Panggil UIManager untuk memperbarui semua gambar tombol!
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMuteButtons();
            
            // Logika UIManager untuk me-restart BGM menu jika di-unmute dan berada di Menu/Panel
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
        // Jika muted, set volume ke 0f (senyap). Jika tidak, set ke 1f.
        float volume = isMuted ? 0f : 1f;
        
        bgmSource.volume = volume;
        sfxSource.volume = volume;
    }
}