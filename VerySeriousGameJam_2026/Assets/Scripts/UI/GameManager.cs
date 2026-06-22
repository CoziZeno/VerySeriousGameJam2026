using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [Header("Scenes")]
    public string gameSceneName = "GameScene";
    public string menuSceneName = "MenuScene";

    public void StartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void RetryButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
