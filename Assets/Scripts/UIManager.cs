using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance;

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
    public GameObject goldenGoalPanel; 

    [Header("Win/Lose Image Assets")]
    public GameObject player1WinImage; 
    public GameObject player2WinImage; 
    public GameObject youWinImage;
    public GameObject youLoseImage; 

    [Header("Audio Controls")]
    public UnityEngine.UI.Image[] muteButtons; 

    public Sprite muteOnSprite; 

    public Sprite muteOffSprite; 

    private float _tempTime;
    private bool _tempIsSinglePlayer;

    void Awake()
    {

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

        ShowPanel(mainMenuPanel);
        UpdateMuteButtons();
    }

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

    public void ShowWinScreen(int playerWhoWon, bool isSinglePlayer, bool isGoldenGoalEnd) 
    {

        player1WinImage.SetActive(false);
        player2WinImage.SetActive(false);
        youWinImage.SetActive(false);
        youLoseImage.SetActive(false);

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

        mainMenuPanel.SetActive(false);
        selectTimePanel.SetActive(false);
        winLosePanel.SetActive(false);
        howToPlayPanel.SetActive(false); 
        pausePanel.SetActive(false);
        goldenGoalPanel.SetActive(false); 

        if (panelToShow != null && panelToShow != gameUIPanel)
        {
            panelToShow.SetActive(true);
        }

        bool showHUD = (panelToShow == gameUIPanel || panelToShow == pausePanel || panelToShow == howToPlayPanel);

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

    public void OnMuteButtonClick()
    {
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxUIClick); 
        SoundManager.Instance.ToggleMute();
        UpdateMuteButtons();

        if (!SoundManager.Instance.GetMuteStatus())
        {

            GameObject activePanel = GetActiveMenuPanel();
            if (activePanel != null)
            {

                ShowPanel(activePanel);
            }
        }
    }
}
