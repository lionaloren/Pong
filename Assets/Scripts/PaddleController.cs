using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{

    public float moveSpeed = 8f; 
    [Tooltip("TRUE jika ini adalah paddle Player 1 (Kiri/Merah). FALSE untuk Player 2 (Kanan/Biru).")]
    public bool isPlayerOne = true; 

    [Tooltip("Kecepatan maksimum bola setelah dipukul")]
    public float maxBallSpeed = 15f; 
    [Tooltip("Kontrol seberapa besar sudut vertikal dipengaruhi oleh titik tabrakan (0.1 - 1.0)")]
    public float hitFactor = 0.5f; 

    [Header("AI Settings")]
    public bool isAI = false;
    public Transform ballTransform; 

    [Tooltip("Delay respons AI. Nilai lebih besar = lebih mudah dikalahkan")]
    public float aiReactionTime = 0.6f; 

    private Rigidbody2D rb;
    private Animator animator;
    private float moveLimitY; 
    private Vector3 initialPosition; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb.gravityScale != 0)
        {
            rb.gravityScale = 0;
        }

        if (Camera.main != null)
        {
            float halfHeight = Camera.main.orthographicSize;
            float paddleHeight = GetComponent<BoxCollider2D>().size.y * transform.localScale.y;
            moveLimitY = halfHeight - (paddleHeight / 2f);
        }

        initialPosition = transform.position; 

    }

    void FixedUpdate()
    {

        if (GameManager.Instance == null || !GameManager.Instance.isGameRunning)
        {

            rb.velocity = Vector2.zero;
            return;
        }

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

            if (isPlayerOne)
            {

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
                    movementInput = 1f;
                }

                else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.A))
                {
                    movementInput = -1f;
                }
            }
        }
        else 
        {

            if (isPlayerOne) 

            {

                if (Input.GetKey(KeyCode.W)) 
                {
                    movementInput = 1f;
                }

                else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A)) 
                {
                    movementInput = -1f;
                }
            }
            else 

            {

                if (Input.GetKey(KeyCode.UpArrow)) movementInput = 1f;

                else if (Input.GetKey(KeyCode.DownArrow)) movementInput = -1f;
            }
        }

        Vector2 velocity = new Vector2(0, movementInput * moveSpeed);

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

        float newY = Mathf.Lerp(currentY, targetY, Time.fixedDeltaTime * (speed / aiReactionTime));

        newY = Mathf.Clamp(newY, -moveLimitY, moveLimitY);

        rb.MovePosition(new Vector2(transform.position.x, newY));
    }

    public void SetAI(bool setAI)
    {
        isAI = setAI;

        rb.velocity = Vector2.zero;
    }

    public void ResetPaddlePosition()
    {
        rb.velocity = Vector2.zero; 
        transform.position = initialPosition; 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {

            if (animator != null)
            {
                animator.SetTrigger("Hit"); 
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxPaddleHit);
            }

            BallController ballController = collision.gameObject.GetComponent<BallController>(); 
            if (ballController != null)
            {

                ballController.lastPlayerHit = isPlayerOne ? 1 : 2; 
            }

            Rigidbody2D ballRb = collision.gameObject.GetComponent<Rigidbody2D>();

            float yPos = collision.transform.position.y - transform.position.y;
            BoxCollider2D paddleCollider = GetComponent<BoxCollider2D>();
            float paddleHeight = paddleCollider.size.y * transform.localScale.y; 
            float yFactor = (yPos / (paddleHeight / 2f)) * hitFactor; 

            float xDirection = transform.position.x < 0 ? 1 : -1;

            Vector2 newDirection = new Vector2(xDirection, yFactor).normalized;

            float currentSpeed = ballRb.velocity.magnitude;

            currentSpeed = Mathf.Min(currentSpeed * 1.05f, maxBallSpeed); 

            ballRb.velocity = newDirection * currentSpeed;
        }
    }
}
