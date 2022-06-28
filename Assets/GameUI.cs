using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    public GameObject gameLoseUI;
    public GameObject gameWinUI;
    bool gameOver = false;

    void Start()
    {
        Guard.OnPlayerSeen += ShowGameLoseUI;
        FindObjectOfType<Player>().OnReachedWinZone += ShowGameWinUI;
    }

    private void Update() {
        if (gameOver) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                // restart game
                SceneManager.LoadScene(0);
            }
        }
    }

    void ShowGameLoseUI() {
        OnGameOver(gameLoseUI);
    }

    void ShowGameWinUI() {
        OnGameOver(gameWinUI);
    }

    void OnGameOver(GameObject gameOverUI) {
        gameOverUI.SetActive(true);
        gameOver = true;
        Guard.OnPlayerSeen -= ShowGameLoseUI;
        FindObjectOfType<Player>().OnReachedWinZone -= ShowGameWinUI;
    }
}
