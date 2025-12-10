using UnityEngine;

public class AngularScaling : MonoBehaviour
{
    private Transform cam;
    private Vector3 initialScale;
    public float baseDistance = 1f;   // distance at which UI keeps its original size

    void Start()
    {
        cam = Camera.main.transform;

        // Save the size the UI had in the editor
        initialScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
        }

        float distance = Vector3.Distance(transform.position, cam.position);

        // Scale changes relative to distance, but starting from your original UI size
        float scaleFactor = distance / baseDistance;

        transform.localScale = initialScale * scaleFactor;
    }
}
