using UnityEngine;

public class DebugAnchorInspector : MonoBehaviour
{
    const string KeyPrefix   = "AnchorsV2_";
    const string HasSavedKey = "AnchorsV2_UserSaved";

    [Header("Optional: scene objects to inspect")]
    public Transform plantInScene;
    public Transform waterInScene;
    public Transform sunInScene;

    void Start()
    {
        Debug.Log("────────── [DEBUG ANCHORS] SCENE START ──────────");

        bool userSaved = PlayerPrefs.GetInt(HasSavedKey, 0) == 1;
        Debug.Log($"[DEBUG] HasSavedKey = {userSaved}");

        DumpOne("Plant");
        DumpOne("Water");
        DumpOne("Sun");

        if (plantInScene != null)
            Debug.Log($"[DEBUG] Scene Plant pos={plantInScene.position}, rot={plantInScene.rotation.eulerAngles}");
        if (waterInScene != null)
            Debug.Log($"[DEBUG] Scene Water pos={waterInScene.position}, rot={waterInScene.rotation.eulerAngles}");
        if (sunInScene != null)
            Debug.Log($"[DEBUG] Scene Sun pos={sunInScene.position}, rot={sunInScene.rotation.eulerAngles}");

        Debug.Log("────────── [DEBUG ANCHORS] END ──────────");
    }

    void DumpOne(string id)
    {
        string baseKey = KeyPrefix + id + "_px";
        if (!PlayerPrefs.HasKey(baseKey))
        {
            Debug.Log($"[DEBUG] No PlayerPrefs keys for {id}");
            return;
        }

        Vector3 pos = new Vector3(
            PlayerPrefs.GetFloat(KeyPrefix + id + "_px"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_py"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_pz")
        );

        Quaternion rot = new Quaternion(
            PlayerPrefs.GetFloat(KeyPrefix + id + "_rx"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_ry"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_rz"),
            PlayerPrefs.GetFloat(KeyPrefix + id + "_rw")
        );

        Debug.Log($"[DEBUG] {id} storedPos={pos}, storedEuler={rot.eulerAngles}");
    }
}