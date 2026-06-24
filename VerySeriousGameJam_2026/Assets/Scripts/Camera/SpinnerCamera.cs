using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SpinnerCamera : MonoBehaviour
{
    public static SpinnerCamera Instance { get; private set; }

    public Transform target;

    [Header("Follow")]
    public Vector3 offset = new Vector3(0f, 12f, -12f);
    public float followSpeed = 10f;

    [Header("Speed Zoom")]
    public float minFOV = 45f;
    public float maxFOV = 65f;
    public float zoomSpeed = 5f;

    [Header("Manual Zoom")]
    public float scrollSpeed = 5f;
    public float maxManualZoom = 15f;

    private Camera cam;
    private float manualZoomOffset;
    private float shakeTimeRemaining;
    private float shakeDuration;
    private float shakeStrength;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void Shake(float strength, float duration)
    {
        if (Instance == null)
            return;

        Instance.AddShake(strength, duration);
    }

    public void AddShake(float strength, float duration)
    {
        if (strength <= 0f || duration <= 0f)
            return;

        shakeStrength = Mathf.Max(shakeStrength, strength);
        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTimeRemaining = Mathf.Max(shakeTimeRemaining, duration);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Follow Player
        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        if (shakeTimeRemaining > 0f)
        {
            float progress = shakeDuration > 0f ? shakeTimeRemaining / shakeDuration : 0f;
            Vector3 shakeOffset = Random.insideUnitSphere * shakeStrength * progress;
            shakeOffset.y *= 0.35f;
            transform.position += shakeOffset;

            shakeTimeRemaining -= Time.deltaTime;
            if (shakeTimeRemaining <= 0f)
            {
                shakeTimeRemaining = 0f;
                shakeDuration = 0f;
                shakeStrength = 0f;
            }
        }

        // Look At Player
        transform.LookAt(target);

        // Mouse Wheel Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.001f)
        {
            manualZoomOffset -= scroll * scrollSpeed;
            manualZoomOffset = Mathf.Clamp(
                manualZoomOffset,
                -maxManualZoom,
                maxManualZoom
            );
        }

        // Get Player Speed
        float speed = 0f;

        if (target.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            speed = rb.linearVelocity.magnitude;
        }

        // Dynamic FOV Based On Speed
        float targetFOV = Mathf.Lerp(
            minFOV,
            maxFOV,
            Mathf.InverseLerp(0f, 20f, speed)
        );

        // Apply Manual Zoom Offset
        targetFOV += manualZoomOffset;

        targetFOV = Mathf.Clamp(
            targetFOV,
            minFOV - maxManualZoom,
            maxFOV + maxManualZoom
        );

        // Smooth FOV
        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            zoomSpeed * Time.deltaTime
        );
    }
}
