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

    //game score
    public int chaserScore = 0;
    public int evaderScore = 0;
    public int tokenCount = 0;
    ScoreBoard scoreBoard;

    public float gameTimeLimitInMins = 60 * 5f; // 5 minutes
    private float startTime;
    public bool isGameOver;
    public bool isSomeOneFrozen = false;

    public GameObject gameOverPanel;
    public GameObject player;

    private void Start() {
        currentState = GameState.Playing;
        startTime = Time.time;
        StartCoroutine(GameTimeLimitCoroutine());
        scoreBoard = FindObjectOfType<ScoreBoard>();
        player = GameObject.Find("player");
    }

    private void Update() {
        switch (currentState) {
            case GameState.Playing:
                if (player.CompareTag("Chaser")) {
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
        if (player.CompareTag("Chaser")) {
            scoreBoard.titleUI.text = "YOU DIED";
            scoreBoard.report.text = "The player cannot come out of his unfrozen state and his life stops forever.";
        }
        else if ((evaderScore!=0 ?evaderScore:0) < ((tokenCount != 0) ? tokenCount / 2: 0) ) {
            scoreBoard.titleUI.text = "GAME FAILURE";
            scoreBoard.report.text = "The player did not get enough tickets to support his departure";
        }
        else {
            scoreBoard.titleUI.text = "GAME COMPLETED";
            scoreBoard.report.text = "You win, the chaser is amazed by your skills.";
        }

        // display game over panel
        gameOverPanel.SetActive(true);
    }

    public void AddScore(string colEntTag, int tokenTotal) {
        if (colEntTag == "Chaser") {
            chaserScore++;
            scoreBoard.UpdateScore(colEntTag, chaserScore,tokenTotal);
        }
        else if(colEntTag == "Evader") {
            evaderScore++;
            scoreBoard.UpdateScore(colEntTag, evaderScore, tokenTotal);
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        currentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void QuitGame() {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
    }

    

}
