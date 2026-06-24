using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
public class PlayerGameOverTrigger : MonoBehaviour
{
    [Header("References")]
    public GameOverUI gameOverUI;

    [Header("Void Detection")]
    public string voidTag = "Void";

    private SpinnerController _controller;
    private bool _gameOverTriggered;

    void Awake()
    {
        _controller = GetComponent<SpinnerController>();

        if (gameOverUI == null)
            gameOverUI = FindObjectOfType<GameOverUI>(true);
    }

    void OnEnable()
    {
        if (_controller == null)
            _controller = GetComponent<SpinnerController>();

        _controller.OnEliminated += HandlePlayerEliminated;
    }

    void OnDisable()
    {
        if (_controller != null)
            _controller.OnEliminated -= HandlePlayerEliminated;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(voidTag))
            TriggerGameOver();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(voidTag))
            TriggerGameOver();
    }

    void HandlePlayerEliminated(SpinnerController eliminated)
    {
        TriggerGameOver();
    }

    void TriggerGameOver()
    {
        if (_gameOverTriggered)
            return;

        _gameOverTriggered = true;

        if (_controller != null)
            _controller.enabled = false;

        if (gameOverUI != null)
            gameOverUI.ShowGameOver();
        else
            Debug.LogWarning("GameOverUI not assigned or found.");
    }
}