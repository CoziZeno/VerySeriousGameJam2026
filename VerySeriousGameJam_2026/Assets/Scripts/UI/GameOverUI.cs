using UnityEngine;

[DisallowMultipleComponent]
public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverCanvas;
    public bool pauseGameOnGameOver = true;

    bool _isGameOver;

    void Awake()
    {
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        if (pauseGameOnGameOver)
            Time.timeScale = 0f;
    }
}
