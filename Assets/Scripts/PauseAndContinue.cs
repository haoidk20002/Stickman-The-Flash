using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class PauseAndContinue : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool gameIsPaused = false;
    public AudioSource music;


    private void Awake()
    {
       
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        gameIsPaused = true;
        pauseMenu.SetActive(true);
        music.Pause();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameIsPaused = false;
        pauseMenu.SetActive(false);
        music.UnPause();
    }
}
