using UnityEngine;

public class SessionBootstrap : MonoBehaviour
{
    void Start()
    {
        var manager = FindAnyObjectByType<AnchorCalibrationManager>();
        if (manager != null) manager.StartCalibration();
        else Debug.LogWarning("[SessionBootstrap] No AnchorCalibrationManager found.");
    }
}