using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private float timer;
    private int frames;

    private void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= 0.5f)
        {
            int fps = Mathf.RoundToInt(frames / timer);
            fpsText.text = fps + " FPS";

            frames = 0;
            timer = 0f;
        }
    }
}