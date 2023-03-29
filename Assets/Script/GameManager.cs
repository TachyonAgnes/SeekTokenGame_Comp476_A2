using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    // afterLoaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // if MenuScene, hide pause Menu
        if (scene.name == "MenuScene") {
            pauseMenuUI.SetActive(false);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (GameIsPaused) {
                Resume();
            }
            else {
                Pause();
            }
        }
    }

    public void Resume() {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Resume(); 
    }

    public void QuitGame() {
        SceneManager.LoadScene("Menu");
        Resume(); 
    }
}