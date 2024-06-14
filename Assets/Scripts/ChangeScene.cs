using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    public void MoveToScene(int id)
    {
        SceneManager.LoadScene(id,LoadSceneMode.Single);
         GameManager.Instance.SaveSettings();
        if (id == 0)
        {
            PauseAndContinue.gameIsPaused = false;
        }
        else { Time.timeScale = 1f; }
    }
    public void RestartScene()
    {        
        PauseAndContinue.gameIsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex,LoadSceneMode.Single);
        GameManager.Instance.SaveSettings();
    }
}
