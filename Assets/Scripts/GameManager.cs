using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton Instance
    public static GameManager Instance;

    [Header("Game Flow & State")]
    public float gameTimeInSeconds = 180f; 
    private float timeLeft;
    public bool isGameRunning = false;
    public bool isGoldenGoal = false;
    public bool isSinglePlayerMode = false;
    public bool isGamePaused = false; // Status Pause
    
    // Menyimpan skor dan waktu sebelum masuk Golden Goal
    private float tempTimeAtTie;
    private int tempScorePlayer1AtTie;
    private int tempScorePlayer2AtTie;

    [Header("Score")]
    public int scorePlayer1 = 0; // Merah/Kiri
    public int scorePlayer2 = 0; // Biru/Kanan
    // Variabel scoreMultiplier lama dihilangkan karena Double Point sekarang Redeem Instan

    [Header("References")]
    public BallController ball; 
    public UIManager uiManager;
    // Referensi ke kedua paddle (Wajib untuk AI)
    public PaddleController paddleRed;   // Player 1 (Kiri/Merah)
    public PaddleController paddleBlue;  // Player 2 (Kanan/Biru/AI)

    private Vector3 initialBallPosition; 
    private Vector2 ballVelocityBeforePause; // Simpan kecepatan bola sebelum Pause

    [Header("Power Up Logic")]
    private Coroutine doublePointCoroutine; // Dibiarkan jika ingin dipakai untuk durasi PowerUp lain
    private Coroutine speedUpCoroutine;

    void Awake()
    {
        // Implementasi Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (ball != null)
        {
            initialBallPosition = ball.transform.position;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isGameRunning || isGoldenGoal)
        {
            UpdateTimer();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGameRunning || isGoldenGoal || isGamePaused) 
            {
                TogglePause();
            }
        }
    }

    // --- GAME FLOW LOGIC ---
    public void StartGame(float timeSelected, bool singlePlayer) 
    {
        // Jika ini bukan Golden Goal, reset skor dan waktu.
        if (!isGoldenGoal)
        {
            gameTimeInSeconds = timeSelected;
            timeLeft = gameTimeInSeconds;
            scorePlayer1 = 0;
            scorePlayer2 = 0;
            // scoreMultiplier = 1; // Dihapus
            isGamePaused = false; 

            // ⭐ BARU: Mulai Spawning Power Up
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.StartSpawning();
            }
        } 
        
        isGameRunning = true;
        isSinglePlayerMode = singlePlayer;
        
        // Atur Mode Paddle
        paddleRed.SetAI(false); 
        paddleBlue.SetAI(singlePlayer); 
        
        paddleRed.ResetPaddlePosition(); 
        paddleBlue.ResetPaddlePosition(); 

        // Update UI dan Reset Bola
        uiManager.UpdateScoreUI(scorePlayer1, scorePlayer2);
        uiManager.UpdateTimerUI(timeLeft); 
        ResetBall();
    }

    private void UpdateTimer()
    {
        if (!isGoldenGoal)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                uiManager.UpdateTimerUI(timeLeft);
            }
            
            if (timeLeft <= 0)
            {
                isGameRunning = false;
                CheckWinCondition();
            }
        } 
    }

    private void CheckWinCondition()
    {
        if (scorePlayer1 == scorePlayer2)
        {
            isGameRunning = false;
            Time.timeScale = 0f; 
            tempTimeAtTie = timeLeft; 
            tempScorePlayer1AtTie = scorePlayer1; 
            tempScorePlayer2AtTie = scorePlayer2; 
            
            // ⭐ BARU: Hentikan Spawning Power Up saat transisi Golden Goal
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.StopSpawningAndClear();
            }
            
            StartCoroutine(GoldenGoalTransition(2f)); 
        }
        else
        {
            int winner = (scorePlayer1 > scorePlayer2) ? 1 : 2; 
            if (uiManager != null)
            {
                uiManager.ShowWinScreen(winner, isSinglePlayerMode, false); 
            }
        }
    }
    
    IEnumerator GoldenGoalTransition(float delay)
    {
        // 1. Tampilkan Panel Golden Goal
        if (uiManager != null && uiManager.goldenGoalPanel != null)
        {
            uiManager.ShowPanel(uiManager.goldenGoalPanel);
            // (Play SFX Golden Goal)
        }
        
        // 2. Tunggu sebentar
        yield return new WaitForSecondsRealtime(delay); 
        
        // 3. Set status Golden Goal aktif
        isGoldenGoal = true;
        Time.timeScale = 1f; // Lanjutkan waktu
        
        // 4. Mulai ulang game dalam mode Golden Goal (tanpa timer, hanya skor)
        StartGame(gameTimeInSeconds, isSinglePlayerMode); 
        
        // 5. Tampilkan kembali HUD Game 
        if (uiManager != null)
        {
            uiManager.ShowPanel(uiManager.gameUIPanel);
        }
    }


    // --- SCORE & RESET ---
    public void GoalScored(int playerWhoScored)
    {
        if (!isGameRunning && !isGoldenGoal) return; 

        // ⭐ PERUBAHAN: Selalu tambahkan 1 poin. Logika Double Point sudah instan di ActivatePowerUp.
        if (playerWhoScored == 1) 
        {
            scorePlayer1 += 1; // Sebelumnya += scoreMultiplier;
        }
        else if (playerWhoScored == 2) 
        {
            scorePlayer2 += 1; // Sebelumnya += scoreMultiplier;
        }
        
        uiManager.UpdateScoreUI(scorePlayer1, scorePlayer2);
        
        if (isGoldenGoal)
        {
            isGameRunning = false;
            isGoldenGoal = false; 
            
            int winner = (playerWhoScored == 1) ? 1 : 2; 
            uiManager.ShowWinScreen(winner, isSinglePlayerMode, true); 
            return;
        }

        ResetBall(); 
        
        // ⭐ BARU: Reset Speed Multiplier setelah setiap gol
        if (ball != null)
        {
            if (speedUpCoroutine != null) StopCoroutine(speedUpCoroutine);
            ball.ResetBallSpeed();
        }
    }

    private void ResetBall()
    {
        if (ball != null)
        {
            ball.StopAndClearTrail(); 
        }

        if (ball != null) 
        {
            ball.transform.position = initialBallPosition;
            // Reset lastPlayerHit agar power up berikutnya tidak langsung aktif tanpa dipukul
            ball.lastPlayerHit = 0; 
        }
        
        StartCoroutine(LaunchNewRound(1f)); 
    }

    IEnumerator LaunchNewRound(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); 
        if (isGameRunning || isGoldenGoal)
        {
            ball.LaunchBall(); 
        }
    }

    // --- PAUSE/RESUME/RESTART LOGIC ---
    // ... (Tidak Berubah) ...
    public void TogglePause()
    {
        if (isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        if (!isGameRunning && !isGoldenGoal) return; 
        
        isGamePaused = true; 
        
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            ballVelocityBeforePause = ballRb.velocity;
            ballRb.velocity = Vector2.zero; 
        }

        Time.timeScale = 0f; 
        
        if (uiManager != null && uiManager.pausePanel != null)
        {
            uiManager.ShowPanel(uiManager.pausePanel); 
        }
    }

    public void ResumeGame()
    {
        isGamePaused = false; 
        Time.timeScale = 1f; 
        
        if (uiManager != null)
        {
            uiManager.ShowPanel(uiManager.gameUIPanel); 
        }
        
        if (isGameRunning || isGoldenGoal)
        {
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null && ballVelocityBeforePause != Vector2.zero)
            {
                ballRb.velocity = ballVelocityBeforePause;
                ballVelocityBeforePause = Vector2.zero;
            } else {
                ball.LaunchBall();
            }
        }
    }

    public void ResetGameState()
    {
        isGameRunning = false;
        isGoldenGoal = false;
        isGamePaused = false; 
        Time.timeScale = 1f; 
        
        // ⭐ PERUBAHAN: Hentikan dan Reset semua efek Power Up
        if (doublePointCoroutine != null) StopCoroutine(doublePointCoroutine);
        if (speedUpCoroutine != null) StopCoroutine(speedUpCoroutine);
        // scoreMultiplier = 1; // Dihapus
        if (ball != null) ball.ResetBallSpeed(); 
        
        // ⭐ BARU: Hentikan Spawning Power Up saat ke Main Menu
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.StopSpawningAndClear();
        }

        if (ball != null)
        {
            ball.StopAndClearTrail(); 
            ball.transform.position = initialBallPosition;
        }
        
        StopAllCoroutines(); 
    }

    public void RestartGame()
    {
        if (Time.timeScale == 0f)
        {
            ResumeGame();
        }
        
        // ⭐ BARU: Pastikan Power Up Manager mulai lagi
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.StartSpawning();
        }

        StartGame(gameTimeInSeconds, isSinglePlayerMode); 
        uiManager.ShowPanel(uiManager.gameUIPanel);
    }
    
    // ⭐ FUNGSI BARU: Logika Aktivasi Power Up (dipanggil dari PowerUpController)
    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        if (ball == null) return;

        // Ambil ID pemain yang terakhir memukul bola
        int playerID = ball.lastPlayerHit; 
        // Hanya Power Up SpeedUp dan ChangeDirection yang bisa diaktifkan jika playerID = 0 (misalnya: bola diluncurkan dari tengah)
        
        switch (type)
        {
            case PowerUpType.DoublePoint:
                // ⭐ LOGIKA BARU: Terapkan Skor Saat Ini x 2 (Redeem Now)
                if (playerID == 0) return; // Pastikan ada pemain yang memukul
                
                if (playerID == 1)
                {
                    scorePlayer1 *= 2; 
                    Debug.Log($"Pemain 1 mendapatkan Double Point! Skor menjadi: {scorePlayer1}");
                }
                else if (playerID == 2)
                {
                    scorePlayer2 *= 2;
                    Debug.Log($"Pemain 2 mendapatkan Double Point! Skor menjadi: {scorePlayer2}");
                }
                uiManager.UpdateScoreUI(scorePlayer1, scorePlayer2); // Update UI
                // Coroutine doublePointCoroutine dihapus/tidak digunakan lagi.
                break;

            case PowerUpType.ChangeDirection:
                ball.ChangeDirection();
                break;

            case PowerUpType.SpeedUp:
                if (speedUpCoroutine != null) StopCoroutine(speedUpCoroutine);
                speedUpCoroutine = StartCoroutine(SpeedUpRoutine(duration));
                break;
        }
    }

    // ⭐ COROUTINE: Speed Up (Tidak Berubah)
    IEnumerator SpeedUpRoutine(float duration)
    {
        ball.IncreaseBallSpeed(3f); 
        yield return new WaitForSeconds(duration);
        ball.ResetBallSpeed(); 
    }
}