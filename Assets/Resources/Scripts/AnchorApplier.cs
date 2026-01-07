using UnityEngine;

public class AnchorApplier : MonoBehaviour
{
    [Header("Targets in THIS scene (roots)")]
    public Transform plantTarget;   // PlantAnchorTarget
    public Transform waterTarget;   // WaterAnchorTarget
    public Transform sunTarget;     // SunAnchorTarget

    const string KeyPrefix   = "AnchorsV2_";          // must match AnchorManager
    const string HasSavedKey = "AnchorsV2_UserSaved"; // same as AnchorManager

    void Start()
    {
        bool userSaved = PlayerPrefs.GetInt(HasSavedKey, 0) == 1;

        if (!userSaved)
        {
            Debug.Log("[Anchors] No saved anchors yet → using scene defaults.");
            return;
        }

        ApplyAnchor("Plant", plantTarget);
        ApplyAnchor("Water", waterTarget);
        ApplyAnchor("Sun",   sunTarget);
    }

    void ApplyAnchor(string id, Transform target)
    {
        if (target == null) return;

        string baseKey = KeyPrefix + id + "_px";
        if (!PlayerPrefs.HasKey(baseKey))
        {
            Debug.Log($"[Anchors] No data for {id}, keeping scene default.");
            return;
        }

        // read saved WORLD position
        Vector3 savedPos = new Vector3(
            PlayerPrefs.GetFloat(KeyPrefix + id + "_px"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_py"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_pz")
        );

        // read saved WORLD rotation
        Quaternion savedRot = new Quaternion(
            PlayerPrefs.GetFloat(KeyPrefix + id + "_rx"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_ry"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_rz"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_rw")
        );

        // force horizontal: only yaw
        Vector3 e = savedRot.eulerAngles;
        float yaw = e.y;

        target.position = savedPos;
        target.rotation = Quaternion.Euler(0f, yaw, 0f);

        Debug.Log($"[Anchors] Applied {id} → WORLD pos {savedPos}, yaw {yaw}");
    }
}