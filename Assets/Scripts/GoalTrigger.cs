using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{

    public int playerToScore = 1; 

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Ball"))
        {

            if (GameManager.Instance != null && GameManager.Instance.isGameRunning)
            {
                GameManager.Instance.GoalScored(playerToScore);
            }
        }
    }
}
