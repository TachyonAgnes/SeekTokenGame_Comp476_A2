using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void StartGame() {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
