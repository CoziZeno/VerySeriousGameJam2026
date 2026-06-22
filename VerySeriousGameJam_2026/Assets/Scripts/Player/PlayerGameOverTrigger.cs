using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpinnerController))]
public class PlayerGameOverTrigger : MonoBehaviour
{
    public GameOverUI gameOverUI;
    public string voidTag = "Void";

    SpinnerController _controller;

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
        if (gameOverUI != null)
            gameOverUI.ShowGameOver();
    }
}
