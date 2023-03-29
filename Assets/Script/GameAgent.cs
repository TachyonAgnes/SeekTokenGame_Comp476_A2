using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// I seen it as a gameEngine
/// </summary>
/// 
public enum GameState {
    Playing,
    GameOver,
}
public class GameAgent : MonoBehaviour
{
    public GameState currentState;

    [Header("Camera")]
    [SerializeField] GameObject fullViewCam;
    [SerializeField] GameObject followCam;
    [SerializeField] GameObject fullViewNotif;
    [SerializeField] GameObject followNotif;
    [SerializeField] GameObject miniMapDisplay;

    //game score
    public int playerScore = 0;
    public int seekerScore = 0;
    ScoreBoard scoreBoard;

    public float gameTimeLimitInMins = 60 * 5f; // 5 minutes
    private float startTime;
    public bool isGameOver;

    // use gameAgent as a singleton
    public Dictionary<AIAgent,Vector3> lastPosCollector = new Dictionary<AIAgent,Vector3>();

    public GameObject gameOverPanel;
    public GameObject player;
    public TokenSpawner tokenSpawner;
    GameManager manager;

    bool shiftPressed = false;
     
    private void Start() {
        currentState = GameState.Playing;
        startTime = Time.time;
        StartCoroutine(GameTimeLimitCoroutine());
        scoreBoard = FindObjectOfType<ScoreBoard>();
        player = GameObject.Find("player(Clone)");
        tokenSpawner = FindObjectOfType<TokenSpawner>();
        manager = GetComponent<GameManager>();
    }

    private void Update() {
        // switch camera
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            shiftPressed = !shiftPressed; 
        }

        if (shiftPressed) {
                followCam.SetActive(true);
                followNotif.SetActive(true);
                fullViewCam.SetActive(false);
                fullViewNotif.SetActive(false);
                miniMapDisplay.SetActive(true);
        }
        else {
            followCam.SetActive(false);
            followNotif.SetActive(false);
            fullViewCam.SetActive(true);
            fullViewNotif.SetActive(true);
            miniMapDisplay.SetActive(false);
        }

        switch (currentState) {
            case GameState.Playing:
                if (player.CompareTag("KnockedOut") || (seekerScore + playerScore) >= tokenSpawner.tokenTotal) {
                    isGameOver = true;
                }
                if (isGameOver) {
                    currentState = GameState.GameOver;
                }
                break;
            case GameState.GameOver:
                HandleGameOver();
                Time.timeScale = 0f;
                break;
        }
    }

    IEnumerator GameTimeLimitCoroutine() {
        yield return new WaitForSeconds(gameTimeLimitInMins);
        if (Time.time - startTime > gameTimeLimitInMins) {
            isGameOver = true;
        }
    }

    private void HandleGameOver() {
        if (player.CompareTag("KnockedOut")) {
            scoreBoard.titleUI.text = "YOU DIED";
            scoreBoard.report.text = "The player knockedOut";
        }
        else if((seekerScore + playerScore) < tokenSpawner.tokenTotal) {
            scoreBoard.titleUI.text = "FAILURE";
            scoreBoard.report.text = "You didn't collect enough token in time";
        }

        else {
            scoreBoard.titleUI.text = "GAME COMPLETED";
            scoreBoard.report.text = "You win, the chaser is amazed by your skills.";
        }

        // display game over panel
        gameOverPanel.SetActive(true);
    }

    public void AddScore(string colEntTag) {
        if (colEntTag == "player(Clone)") {
            playerScore++;
        }
        else if(colEntTag == "Evader") {
            seekerScore++;
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        currentState = GameState.Playing;
        manager.Resume();
    }

    public void QuitGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        currentState = GameState.Playing;
        manager.Resume();
    }

    

}
