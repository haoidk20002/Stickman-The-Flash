using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void MoveToScene(int id){
        SceneManager.LoadScene(id);
        if (id == 0){
            PauseAndContinue.gameIsPaused = false;
        }
        if (id == 1){ Time.timeScale = 1f;}
    }
    public void RestartScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        PauseAndContinue.gameIsPaused = false;
        Time.timeScale = 1f;
    }
}
