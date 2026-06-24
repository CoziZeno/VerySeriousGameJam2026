using UnityEngine;

public class VFXFollowTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public bool stopFollowingWhenTargetInactive = true;

    void LateUpdate()
    {
        if (target == null)
        {
            enabled = false;
            return;
        }

        if (stopFollowingWhenTargetInactive && !target.gameObject.activeInHierarchy)
        {
            enabled = false;
            return;
        }

        transform.position = target.position + offset;
    }
}
