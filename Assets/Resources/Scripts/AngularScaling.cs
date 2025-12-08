// using UnityEngine;

// public class AngularScaling : MonoBehaviour
// {
//     public float baseDistance = 1.0f;       // distance at which "baseScale" looks correct
//     public float baseScale = 0.001f;        // normal UI scale at base distance
//     private Transform cam;

//     void Start()
//     {
//         if (Camera.main != null)
//             cam = Camera.main.transform;

//         // initialize scale
//         transform.localScale = Vector3.one * baseScale;
//     }

//     void LateUpdate()
//     {
//         if (cam == null)
//         {
//             if (Camera.main == null) return;
//             cam = Camera.main.transform;
//         }

//         float distance = Vector3.Distance(cam.position, transform.position);

//         // compute scale so angular size stays constant
//         float scaleFactor = distance / baseDistance;

//         transform.localScale = Vector3.one * (baseScale * scaleFactor);
//     }
// }

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
