using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Flow & State")]
    public float gameTimeInSeconds = 180f; 
    private float timeLeft;
    public bool isGameRunning = false;
    public bool isGoldenGoal = false;
    public bool isSinglePlayerMode = false;
    public bool isGamePaused = false;

    private float tempTimeAtTie;
    private int tempScorePlayer1AtTie;
    private int tempScorePlayer2AtTie;

    [Header("Score")]
    public int scorePlayer1 = 0; 

    public int scorePlayer2 = 0; 

    [Header("References")]
    public BallController ball; 
    public UIManager uiManager;
    public PaddleController paddleRed;   

    public PaddleController paddleBlue;  

    private Vector3 initialBallPosition; 
    private Vector2 ballVelocityBeforePause; 

    [Header("Power Up Logic")]
    private Coroutine doublePointCoroutine;
    private Coroutine speedUpCoroutine;

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

    public void StartGame(float timeSelected, bool singlePlayer) 
    {
        if (!isGoldenGoal)
        {
            gameTimeInSeconds = timeSelected;
            timeLeft = gameTimeInSeconds;
            scorePlayer1 = 0;
            scorePlayer2 = 0;
            isGamePaused = false; 

            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.StartSpawning();
            }
        } 

        isGameRunning = true;
        isSinglePlayerMode = singlePlayer;

        paddleRed.SetAI(false); 
        paddleBlue.SetAI(singlePlayer); 

        paddleRed.ResetPaddlePosition(); 
        paddleBlue.ResetPaddlePosition(); 

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
        if (uiManager != null && uiManager.goldenGoalPanel != null)
        {
            uiManager.ShowPanel(uiManager.goldenGoalPanel);
        }

        yield return new WaitForSecondsRealtime(delay); 

        isGoldenGoal = true;
        Time.timeScale = 1f; 

        StartGame(gameTimeInSeconds, isSinglePlayerMode); 

        if (uiManager != null)
        {
            uiManager.ShowPanel(uiManager.gameUIPanel);
        }
    }

    public void GoalScored(int playerWhoScored)
    {
        if (!isGameRunning && !isGoldenGoal) return; 

        if (playerWhoScored == 1) 
        {
            scorePlayer1 += 1; 

        }
        else if (playerWhoScored == 2) 
        {
            scorePlayer2 += 1; 

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

        if (doublePointCoroutine != null) StopCoroutine(doublePointCoroutine);
        if (speedUpCoroutine != null) StopCoroutine(speedUpCoroutine);

        if (ball != null) ball.ResetBallSpeed(); 

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

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.StartSpawning();
        }

        StartGame(gameTimeInSeconds, isSinglePlayerMode); 
        uiManager.ShowPanel(uiManager.gameUIPanel);
    }

    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        if (ball == null) return;

        int playerID = ball.lastPlayerHit; 

        switch (type)
        {
            case PowerUpType.DoublePoint:

                if (playerID == 0) return; 

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
                uiManager.UpdateScoreUI(scorePlayer1, scorePlayer2); 

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

    IEnumerator SpeedUpRoutine(float duration)
    {
        ball.IncreaseBallSpeed(3f); 
        yield return new WaitForSeconds(duration);
        ball.ResetBallSpeed(); 
    }
}
