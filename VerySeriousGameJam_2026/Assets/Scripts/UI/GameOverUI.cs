using UnityEngine;

[DisallowMultipleComponent]
public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverCanvas;
    public bool pauseGameOnGameOver = true;
    public float showDelay = 1f;

    bool _isGameOver;
    CanvasGroup _canvasGroup;

    void Awake()
    {
        if (gameOverCanvas == null)
            gameOverCanvas = gameObject;

        if (gameOverCanvas == gameObject)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SetGameOverVisible(false);
    }

    void SetGameOverVisible(bool isVisible)
    {
        if (gameOverCanvas == gameObject && _canvasGroup != null)
        {
            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
            return;
        }

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(isVisible);
    }

    public void ShowGameOver()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;
        StartCoroutine(ShowGameOverRoutine());
    }

    System.Collections.IEnumerator ShowGameOverRoutine()
    {
        if (showDelay > 0f)
            yield return new WaitForSeconds(showDelay);

        SetGameOverVisible(true);

        if (pauseGameOnGameOver)
            Time.timeScale = 0f;
    }
}
