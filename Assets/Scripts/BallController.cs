using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{

    public float initialSpeed = 10f;

    [Tooltip("Kecepatan rotasi bola saat mengikuti arah gerak")]
    public float rotationSpeed = 10f; 

    [Header("Power Up Status")]
    public float currentSpeed; 
    public int lastPlayerHit = 0; 

    private Rigidbody2D rb;
    private TrailRenderer trail; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>(); 

        if (trail != null)
        {
            trail.emitting = false;
        }

        currentSpeed = initialSpeed; 
    }

    void Start()
    {
    }

    public void LaunchBall()
    {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(-1f, 1f);

        Vector2 direction = new Vector2(x, y).normalized;

        rb.velocity = direction * currentSpeed;

        if (trail != null)
        {
            trail.emitting = true;
        }
    }

    public void StopAndClearTrail() 
    {
        rb.velocity = Vector2.zero;

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear(); 
        }
    }

    void FixedUpdate()
    {
        if (rb.velocity.magnitude < currentSpeed && rb.velocity.magnitude > 0.01f)
        {
            rb.velocity = rb.velocity.normalized * currentSpeed;
        }
        HandleRotation();
    }

    public void IncreaseBallSpeed(float multiplier)
    {
        currentSpeed = initialSpeed * multiplier; 

        if (rb.velocity.magnitude > 0.01f)
        {
            rb.velocity = rb.velocity.normalized * currentSpeed;
        }
    }

    public void ResetBallSpeed()
    {
        if (currentSpeed == initialSpeed) return; 

        currentSpeed = initialSpeed;

        if (rb.velocity.magnitude > 0.01f)
        {
            rb.velocity = rb.velocity.normalized * currentSpeed;
        }
    }

    public void ChangeDirection()
    {
        rb.velocity = -rb.velocity;
    }

    private void HandleRotation()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            Vector2 direction = rb.velocity.normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle + 90f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                Time.fixedDeltaTime * rotationSpeed
            );
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall")) 
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxWallHit);
            }
        }
        else if (collision.gameObject.CompareTag("Paddle")) 
        {

        }
    }
}
