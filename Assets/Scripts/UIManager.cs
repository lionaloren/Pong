using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // --- PERBAIKAN: MENAMBAH DEFINISI SINGLETON ---
    public static UIManager Instance;

    // --- Referensi UI (WAJIB di-Drag dari Hierarchy ke Inspector) ---
    [Header("Game UI References")]
    public TextMeshProUGUI scoreTextP1;
    public TextMeshProUGUI scoreTextP2;
    public TextMeshProUGUI timerText;

    [Header("Screen Panels")]
    public GameObject mainMenuPanel;
    public GameObject selectTimePanel;
    public GameObject winLosePanel;
    public GameObject gameUIPanel;
    public GameObject howToPlayPanel; 
    public GameObject pausePanel; 
    public GameObject goldenGoalPanel; // ⭐ BARU: Panel Golden Goal (untuk transisi)

    [Header("Win/Lose Image Assets")]
    public GameObject player1WinImage; 
    public GameObject player2WinImage; 
    public GameObject youWinImage;
    public GameObject youLoseImage; 

    [Header("Audio Controls")]
    public UnityEngine.UI.Image[] muteButtons; // Array untuk semua gambar tombol Mute
    public Sprite muteOnSprite; // Gambar ketika suara AKTIF
    public Sprite muteOffSprite; // Gambar ketika suara MATI
    
    // Variabel untuk menyimpan pilihan sementara
    private float _tempTime;
    private bool _tempIsSinglePlayer;

    // --- SETUP ---
    void Awake()
    {
        // Logika inisialisasi Singleton
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.uiManager = this;
        }
        // Tampilkan Main Menu saat game pertama kali dijalankan
        ShowPanel(mainMenuPanel);
        UpdateMuteButtons();
    }
    
    // --- GAME UI UPDATES ---
    public void UpdateScoreUI(int score1, int score2)
    {
        if (scoreTextP1 != null) scoreTextP1.text = score1.ToString();
        if (scoreTextP2 != null) scoreTextP2.text = score2.ToString();
    }

    public void UpdateTimerUI(float timeRemaining)
    {
        if (timerText == null) return;

        if (timeRemaining < 0) timeRemaining = 0;
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // --- PANEL MANAGEMENT ---
    
    public void ShowWinScreen(int playerWhoWon, bool isSinglePlayer, bool isGoldenGoalEnd) 
    {
        // Nonaktifkan semua gambar kemenangan/kekalahan
        player1WinImage.SetActive(false);
        player2WinImage.SetActive(false);
        youWinImage.SetActive(false);
        youLoseImage.SetActive(false);
        
        // Logika kemenangan biasa atau setelah Golden Goal berakhir
        if (isSinglePlayer)
        {
            if (playerWhoWon == 1) 
            {
                youWinImage.SetActive(true);
            }
            else 
            {
                youLoseImage.SetActive(true);
            }
        }
        else
        {
            if (playerWhoWon == 1)
            {
                player1WinImage.SetActive(true);
            }
            else
            {
                player2WinImage.SetActive(true);
            }
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxWinLose);
        }

        ShowPanel(winLosePanel);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        // 1. Nonaktifkan semua panel menu/overlay utama
        mainMenuPanel.SetActive(false);
        selectTimePanel.SetActive(false);
        winLosePanel.SetActive(false);
        howToPlayPanel.SetActive(false); 
        pausePanel.SetActive(false);
        goldenGoalPanel.SetActive(false); // ⭐ BARU: Nonaktifkan Golden Goal Panel

        // 2. Aktifkan panel yang diminta (jika itu menu/pause)
        if (panelToShow != null && panelToShow != gameUIPanel)
        {
            panelToShow.SetActive(true);
        }
        
        // 3. Tentukan kapan GameUIPanel (Score/Timer HUD) harus aktif
        bool showHUD = (panelToShow == gameUIPanel || panelToShow == pausePanel || panelToShow == howToPlayPanel);

        // Aktifkan GameUIPanel berdasarkan kondisi showHUD
        gameUIPanel.SetActive(showHUD); 

        bool isMenuPanel = (panelToShow == mainMenuPanel || panelToShow == selectTimePanel || panelToShow == howToPlayPanel); 

        if (SoundManager.Instance != null && !SoundManager.Instance.GetMuteStatus())
        {
            if (isMenuPanel)
            {
                SoundManager.Instance.PlayBGM(SoundManager.Instance.bgmMenu); 
            }
            else
            {
                SoundManager.Instance.StopBGM();
            }
        }
    }
    
    public GameObject GetActiveMenuPanel()
    {
        if (mainMenuPanel.activeSelf) return mainMenuPanel;
        if (selectTimePanel.activeSelf) return selectTimePanel;
        if (howToPlayPanel.activeSelf) return howToPlayPanel;
        if (pausePanel.activeSelf) return pausePanel; 
        if (winLosePanel.activeSelf) return winLosePanel; 
        
        return null;
    }

    // --- UI BUTTON FUNCTIONS ---
    
    public void OnSelectModeClick(bool isSingle)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxUIClick);
        }
        _tempIsSinglePlayer = isSingle;
        ShowPanel(selectTimePanel);
    }

    public void OnSelectTime(float timeSelected)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxUIClick);
        }
        _tempTime = timeSelected;
        ShowPanel(howToPlayPanel);
    }

    public void OnPlayGameClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxUIClick);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(_tempTime, _tempIsSinglePlayer); 
        }
        ShowPanel(gameUIPanel); 
    }

    public void OnPauseButtonClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxUIClick);
        }

        if (GameManager.Instance == null) return;
        
        GameManager.Instance.PauseGame(); 
        ShowPanel(pausePanel); 
    }

    public void OnResumeButtonClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxUIClick);
        }

        if (GameManager.Instance == null) return;
        
        ShowPanel(gameUIPanel); 
        GameManager.Instance.ResumeGame(); 
    }

    public void OnBackToMenuClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxUIClick);
        }
        
        if (GameManager.Instance == null) return;
        
        GameManager.Instance.ResetGameState(); 
        ShowPanel(mainMenuPanel);
    }

    public void OnQuitGameClick()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
        #else
            Application.Quit(); 
        #endif
    }

    public void UpdateMuteButtons()
    {
        if (SoundManager.Instance == null) return;

        bool isMuted = SoundManager.Instance.GetMuteStatus();
        Sprite targetSprite = isMuted ? muteOffSprite : muteOnSprite;

        foreach (UnityEngine.UI.Image img in muteButtons)
        {
            if (img != null)
            {
                img.sprite = targetSprite;
            }
        }
    }

    // Dipanggil dari tombol Mute di semua panel
    public void OnMuteButtonClick()
    {
        if (SoundManager.Instance == null) return;
    
        SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxUIClick); 
        SoundManager.Instance.ToggleMute();
        UpdateMuteButtons();
        
        // --- LOGIKA UNMUTE (Memanggil ShowPanel untuk me-restart BGM jika perlu) ---

        if (!SoundManager.Instance.GetMuteStatus())
        {
            // Panggil ShowPanel() untuk memastikan BGM mulai lagi jika kita berada di panel menu.
            GameObject activePanel = GetActiveMenuPanel();
            if (activePanel != null)
            {
                // Panggil ulang ShowPanel untuk me-restart logika BGM
                ShowPanel(activePanel);
            }
        }
    }
}