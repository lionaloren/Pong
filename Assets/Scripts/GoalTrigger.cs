using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    // Atur di Inspector: 1 (skor utk P1) atau 2 (skor utk P2)
    public int playerToScore = 1; 

    // Dipanggil saat objek lain melewati Collider yang disetel sebagai Trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang masuk ke Trigger adalah Bola
        if (other.CompareTag("Ball"))
        {
            // PENTING: Cek apakah GameManager ada DAN game sedang berjalan
            if (GameManager.Instance != null && GameManager.Instance.isGameRunning)
            {
                GameManager.Instance.GoalScored(playerToScore);
            }
        }
    }
}
