using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SpinnerHealthBar : MonoBehaviour
{
    [Header("Refs")]
    public SpinnerController target;
    public Image foreground;
    public Camera viewCamera;
    public Transform billboardRoot;

    [Header("Display")]
    public bool hideWhenDead = true;

    void Reset()
    {
        target = GetComponentInParent<SpinnerController>();
        foreground = GetComponent<Image>();
        billboardRoot = transform;
    }

    void Awake()
    {
        if (target == null)
            target = GetComponentInParent<SpinnerController>();

        if (billboardRoot == null)
            billboardRoot = transform;

        if (viewCamera == null)
            viewCamera = Camera.main;
    }

    void LateUpdate()
    {
        UpdateFill();
        FaceCamera();
    }

    void UpdateFill()
    {
        if (target == null || foreground == null)
            return;

        float maxHealth = Mathf.Max(1, target.maxHealth);
        foreground.fillAmount = Mathf.Clamp01(target.CurrentHealth / maxHealth);

        if (hideWhenDead)
            foreground.transform.parent.gameObject.SetActive(target.IsAlive);
    }

    void FaceCamera()
    {
        Camera cam = viewCamera != null ? viewCamera : Camera.main;
        if (cam == null || billboardRoot == null)
            return;

        billboardRoot.rotation = Quaternion.LookRotation(
            cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up
        );
    }
}
