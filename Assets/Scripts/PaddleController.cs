using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    // --- PENGATURAN UMUM ---
    public float moveSpeed = 8f; 
    [Tooltip("TRUE jika ini adalah paddle Player 1 (Kiri/Merah). FALSE untuk Player 2 (Kanan/Biru).")]
    public bool isPlayerOne = true; // WAJIB DISET DI INSPECTOR
    
    // --- PENGATURAN BOLA & PANTULAN ---
    [Tooltip("Kecepatan maksimum bola setelah dipukul")]
    public float maxBallSpeed = 15f; 
    [Tooltip("Kontrol seberapa besar sudut vertikal dipengaruhi oleh titik tabrakan (0.1 - 1.0)")]
    public float hitFactor = 0.5f; 
    
    // --- AI Settings (WAJIB Ada) ---
    [Header("AI Settings")]
    public bool isAI = false;
    public Transform ballTransform; // Drag objek Ball ke sini
    [Tooltip("Delay respons AI. Nilai lebih besar = lebih mudah dikalahkan")]
    public float aiReactionTime = 0.6f; 
    
    // --- VARIABEL PRIVAT ---
    private Rigidbody2D rb;
    private Animator animator;
    private float moveLimitY; 
    private Vector3 initialPosition; // Posisi awal paddle

    // --- SETUP ---
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb.gravityScale != 0)
        {
            rb.gravityScale = 0;
        }
        
        // Hitung batasan vertikal
        if (Camera.main != null)
        {
            float halfHeight = Camera.main.orthographicSize;
            float paddleHeight = GetComponent<BoxCollider2D>().size.y * transform.localScale.y;
            moveLimitY = halfHeight - (paddleHeight / 2f);
        }
        
        initialPosition = transform.position; // Ambil posisi awal
    }

    void FixedUpdate()
    {
        // PENTING: Cek apakah Game Manager ada DAN game sedang berjalan
        if (GameManager.Instance == null || !GameManager.Instance.isGameRunning)
        {
            // Jika game belum dimulai/di-pause, hentikan semua gerakan
            rb.velocity = Vector2.zero;
            return;
        }

        // Logika AI atau Pemain HANYA berjalan jika isGameRunning = true
        if (isAI)
        {
            HandleAIMovement();
        }
        else
        {
            HandlePlayerMovement();
        }
    }

    private void HandlePlayerMovement()
    {
        float movementInput = 0f;

        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.isSinglePlayerMode)
        {
            // --- MODE SINGLE PLAYER (P1): Up/Down, W/S, W/A ---
            // Hanya P1 (isPlayerOne=true) yang direspon. P2 seharusnya AI.
            if (isPlayerOne)
            {
                // P1: Up Movement (W atau UpArrow)
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    movementInput = 1f;
                }
                // P1: Down Movement (S, DownArrow, atau A)
                else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.A))
                {
                    movementInput = -1f;
                }
            }
        }
        else 
        {
            // --- MODE MULTIPLAYER (P1: W/S atau W/A, P2: Up/Down) ---
            if (isPlayerOne) // Paddle Red (P1 - Kiri)
            {
                // P1: Up Movement (W)
                if (Input.GetKey(KeyCode.W)) 
                {
                    movementInput = 1f;
                }
                // P1: Down Movement (S atau A)
                else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A)) 
                {
                    movementInput = -1f;
                }
            }
            else // Paddle Blue (P2 - Kanan)
            {
                // P2: Up Movement (UpArrow)
                if (Input.GetKey(KeyCode.UpArrow)) movementInput = 1f;
                // P2: Down Movement (DownArrow)
                else if (Input.GetKey(KeyCode.DownArrow)) movementInput = -1f;
            }
        }

        // --- PENERAPAN PERGERAKAN ---
        Vector2 velocity = new Vector2(0, movementInput * moveSpeed);
        
        // Batasan Gerak manual untuk Player
        Vector3 newPos = transform.position + (Vector3)velocity * Time.fixedDeltaTime;
        newPos.y = Mathf.Clamp(newPos.y, -moveLimitY, moveLimitY);
        rb.MovePosition(newPos);
    }

    private void HandleAIMovement()
    {
        if (ballTransform == null) return;
        
        float targetY = ballTransform.position.y;
        float currentY = transform.position.y;
        float speed = moveSpeed;
        
        // Membuat pergerakan AI lebih halus dan realistis (Lerp)
        float newY = Mathf.Lerp(currentY, targetY, Time.fixedDeltaTime * (speed / aiReactionTime));

        // Batasan Gerak (agar paddle tidak keluar layar)
        newY = Mathf.Clamp(newY, -moveLimitY, moveLimitY);

        // Terapkan Posisi Baru
        rb.MovePosition(new Vector2(transform.position.x, newY));
    }
    
    // Fungsi yang dipanggil oleh GameManager untuk mengaktifkan mode AI (WAJIB PUBLIC)
    public void SetAI(bool setAI)
    {
        isAI = setAI;
        // Hentikan paddle jika AI beralih mode
        rb.velocity = Vector2.zero;
    }

    // Dipanggil oleh GameManager saat Start atau Restart
    public void ResetPaddlePosition()
    {
        rb.velocity = Vector2.zero; 
        transform.position = initialPosition; 
    }

    // --- PHYSICS LOGIC ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            // 1. Panggil animasi Squash/Stretch (jika ada)
            if (animator != null)
            {
                animator.SetTrigger("Hit"); 
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxPaddleHit);
            }
            
            // ⭐ PERUBAHAN: Catat siapa yang memukul bola terakhir
            BallController ballController = collision.gameObject.GetComponent<BallController>(); 
            if (ballController != null)
            {
                // Jika isPlayerOne=TRUE (P1), catat ID 1, jika FALSE (P2), catat ID 2.
                ballController.lastPlayerHit = isPlayerOne ? 1 : 2; 
            }
            
            // 2. Logika Sudut Pantulan
            Rigidbody2D ballRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            float yPos = collision.transform.position.y - transform.position.y;
            BoxCollider2D paddleCollider = GetComponent<BoxCollider2D>();
            float paddleHeight = paddleCollider.size.y * transform.localScale.y; 
            float yFactor = (yPos / (paddleHeight / 2f)) * hitFactor; 
            
            float xDirection = transform.position.x < 0 ? 1 : -1;
            
            Vector2 newDirection = new Vector2(xDirection, yFactor).normalized;
            
            float currentSpeed = ballRb.velocity.magnitude;
            
            // Sedikit peningkatan kecepatan dan batas maksimum
            currentSpeed = Mathf.Min(currentSpeed * 1.05f, maxBallSpeed); 

            ballRb.velocity = newDirection * currentSpeed;
        }
    }
}