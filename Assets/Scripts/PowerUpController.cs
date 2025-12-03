using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Pilih tipe power up ini di Inspector.")]
    public PowerUpType type; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            if (GameManager.Instance != null && PowerUpManager.Instance != null)
            {
                float duration = PowerUpManager.Instance.powerUpDuration;
                GameManager.Instance.ActivatePowerUp(type, duration);
            }

            if (SoundManager.Instance != null && SoundManager.Instance.sfxPowerUp != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxPowerUp);
            }

            gameObject.SetActive(false); 
        }
    }
}
