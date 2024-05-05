using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseAndContinue : MonoBehaviour
{
    public GameObject pauseMenu;

    private void Awake(){

    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }
}
