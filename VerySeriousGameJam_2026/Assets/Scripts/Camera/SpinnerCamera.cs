using UnityEngine;

[DisallowMultipleComponent]
public class SpinnerCamera : MonoBehaviour
{
    public Transform target;

    [Header("Isometric Follow")]
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    public float followSmoothTime = 0.12f;
    public float lookHeight = 1.2f;

    [Header("Zoom")]
    public bool dynamicZoom = true;
    public float minDistance = 8f;
    public float maxDistance = 13f;
    public float zoomSpeed = 1.5f;
    public float manualZoomSpeed = 2f;
    public float minManualZoom = -4f;
    public float maxManualZoom = 4f;

    Vector3 _velocity;
    Camera _cam;
    float _manualZoom;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
            _manualZoom = Mathf.Clamp(_manualZoom - scroll * manualZoomSpeed, minManualZoom, maxManualZoom);

        Vector3 desiredOffset = offset;

        if (dynamicZoom)
        {
            float speed = 0f;
            SpinnerController ctrl = target.GetComponentInParent<SpinnerController>();
            if (ctrl != null && ctrl.TryGetComponent<Rigidbody>(out var rb))
                speed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

            float t = Mathf.InverseLerp(0f, 10f, speed * zoomSpeed);
            float dist = Mathf.Lerp(minDistance, maxDistance, t);
            dist = Mathf.Clamp(dist + _manualZoom, minDistance + minManualZoom, maxDistance + maxManualZoom);

            Vector3 flatOffset = new Vector3(offset.x, 0f, offset.z).normalized * dist;
            desiredOffset = new Vector3(flatOffset.x, offset.y, flatOffset.z);
        }
        else if (Mathf.Abs(_manualZoom) > 0.001f)
        {
            Vector3 flatOffset = new Vector3(offset.x, 0f, offset.z);
            float dist = Mathf.Clamp(flatOffset.magnitude + _manualZoom, minDistance + minManualZoom, maxDistance + maxManualZoom);
            flatOffset = flatOffset.normalized * dist;
            desiredOffset = new Vector3(flatOffset.x, offset.y, flatOffset.z);
        }

        Vector3 desiredPosition = target.position + desiredOffset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, followSmoothTime);

        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 12f * Time.deltaTime);
    }
}
