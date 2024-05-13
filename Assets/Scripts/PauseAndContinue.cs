using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseAndContinue : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool gameIsPaused = false; // 

    private void Awake(){

    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        gameIsPaused = true;
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameIsPaused = false;
        pauseMenu.SetActive(false);
    }
}
