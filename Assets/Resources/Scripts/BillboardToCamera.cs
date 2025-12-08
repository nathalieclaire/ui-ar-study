using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
        }

        // CORRECTION: face camera correctly (no flipping)
        Vector3 toCam = (transform.position - cam.position).normalized;
        transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }
}
