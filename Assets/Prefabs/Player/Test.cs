using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    private bool isPaused = false;
    public GameObject deathScreen;

    private void Start()
    {
        isPaused = false;

        if (deathScreen != null)
        {
            deathScreen.SetActive(false); // Make sure it's off at start
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KILLBOX") && !isPaused)
        {
            if (deathScreen != null)
            {
                Time.timeScale = 0f; // Pause the game
                deathScreen.SetActive(true);
                isPaused = true;
            }
            else
            {
                Debug.LogWarning("Death screen GameObject not assigned in the inspector.");
            }
        }
    }
}