using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Assign your Canvas or UI Root here")]
    public Transform uiRoot;

    [Header("Distance from the face")]
    public float forwardDistance = 0.5f;

    void Start()
    {
        Mount();
    }

    void Mount()
    {
        if (uiRoot == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Parent UI to the headset camera
        uiRoot.SetParent(cam.transform, false);

        // Place it in front of the face
        uiRoot.localPosition = new Vector3(0f, 0f, forwardDistance);

        // Keep it upright
        uiRoot.localRotation = Quaternion.identity;
    }
}
