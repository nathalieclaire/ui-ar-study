using UnityEngine;
using UnityEngine.SceneManagement;

public class AnchorManager : MonoBehaviour
{
    [Header("Assign the objects that actually MOVE (the cubes)")]
    public Transform plantAnchor;   // drag PlantAnchor cube here
    public Transform waterAnchor;   // drag WaterAnchor cube here
    public Transform sunAnchor;     // drag SunAnchor cube here

    const string KeyPrefix   = "AnchorsV2_";           // new prefix → ignores old data
    const string HasSavedKey = "AnchorsV2_UserSaved";  // gate: only load after first save

    void Start()
    {
        bool userSaved = PlayerPrefs.GetInt(HasSavedKey, 0) == 1;

        if (userSaved)
        {
            Debug.Log("[Anchors] Start() – loading saved transforms...");
            LoadTransform("Plant", plantAnchor);
            LoadTransform("Water", waterAnchor);
            LoadTransform("Sun",   sunAnchor);
        }
        else
        {
            Debug.Log("[Anchors] Start() – no saved anchors yet, using scene defaults.");
            // do nothing → cubes stay where you placed them in the scene
        }
    }

    // ------------------------------------------------------------------
    // UI BUTTONS
    // ------------------------------------------------------------------

    // Called by "Save & Main Menu"
    public void SaveAnchors()
    {
        SaveTransform("Plant", plantAnchor);
        SaveTransform("Water", waterAnchor);
        SaveTransform("Sun",   sunAnchor);

        PlayerPrefs.SetInt(HasSavedKey, 1);   // from now on we trust saved data
        PlayerPrefs.Save();

        Debug.Log(
            $"[Anchors] Saved. Keys now – Plant={HasTransformKeys("Plant")}, " +
            $"Water={HasTransformKeys("Water")}, Sun={HasTransformKeys("Sun")}"
        );
    }

    // Called by "Reset Anchors"
    public void ResetAnchors()
    {
        // forget saved data completely
        PlayerPrefs.DeleteKey(HasSavedKey);
        DeleteTransformKeys("Plant");
        DeleteTransformKeys("Water");
        DeleteTransformKeys("Sun");
        PlayerPrefs.Save();

        // reload scene → cubes jump back to prefab/default positions
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);

        Debug.Log("[Anchors] Reset: cleared saved data and reloaded scene.");
    }

    // ------------------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------------------

    void SaveTransform(string id, Transform t)
    {
        if (t == null) return;

        Debug.Log($"[Anchors] Saving {id} at {t.position}");

        PlayerPrefs.SetFloat(KeyPrefix + id + "_px", t.position.x);
        PlayerPrefs.SetFloat(KeyPrefix + id + "_py", t.position.y);
        PlayerPrefs.SetFloat(KeyPrefix + id + "_pz", t.position.z);

        PlayerPrefs.SetFloat(KeyPrefix + id + "_rx", t.rotation.x);
        PlayerPrefs.SetFloat(KeyPrefix + id + "_ry", t.rotation.y);
        PlayerPrefs.SetFloat(KeyPrefix + id + "_rz", t.rotation.z);
        PlayerPrefs.SetFloat(KeyPrefix + id + "_rw", t.rotation.w);
    }

    void LoadTransform(string id, Transform t)
    {
        if (t == null) return;
        if (!HasTransformKeys(id)) return;

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

        Debug.Log($"[Anchors] LoadTransform: applying {id} at {pos}");
        t.SetPositionAndRotation(pos, rot);
    }

    bool HasTransformKeys(string id)
    {
        return PlayerPrefs.HasKey(KeyPrefix + id + "_px");
    }

    void DeleteTransformKeys(string id)
    {
        PlayerPrefs.DeleteKey(KeyPrefix + id + "_px");
        PlayerPrefs.DeleteKey(KeyPrefix + id + "_py");
        PlayerPrefs.DeleteKey(KeyPrefix + id + "_pz");
        PlayerPrefs.DeleteKey(KeyPrefix + id + "_rx");
        PlayerPrefs.DeleteKey(KeyPrefix + id + "_ry");
        PlayerPrefs.DeleteKey(KeyPrefix + id + "_rz");
        PlayerPrefs.DeleteKey(KeyPrefix + id + "_rw");
    }
}