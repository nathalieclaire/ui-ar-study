using UnityEngine;

using System.Collections;

public class StationCheck : MonoBehaviour
{
    public string myStationTag;               // "WaterStation" or "SunStation"
    public Transform snapPoint;               // assign SnapPoint here
    public Renderer stationRenderer;
    public Color okColor = Color.green;
    public Color wrongColor = Color.red;

    SceneFlowManager flow;

    void Start()
    {
        flow = FindFirstObjectByType<SceneFlowManager>();
        if (stationRenderer == null) stationRenderer = GetComponentInChildren<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (flow == null) return;

        var trial = other.GetComponentInParent<CubeTrial>();
        if (trial == null) return;
        if (flow.GetActiveTrial() != trial) return;

        bool correct = (trial.requiredStationTag == myStationTag);

        if (stationRenderer != null)
            stationRenderer.material.color = correct ? okColor : wrongColor;

        if (correct)
            StartCoroutine(SnapWhenReleased(trial));
    }

    IEnumerator SnapWhenReleased(CubeTrial trial)
    {
        var grab = trial.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        // wait until user releases
        while (grab != null && grab.isSelected)
            yield return null;

        // snap
        if (snapPoint != null)
        {
            trial.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
        }

        // disable symbol visibility checking so it can’t re-trigger UI 3
        var symbol = trial.symbol?.GetComponent<SymbolVisibilitySuccess>();
        if (symbol != null) symbol.enabled = false;

        flow.OnPlantSnappedCorrectly();
        flow.ShowPage4();

        // freeze it in place
        var rb = trial.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // prevent re-grabbing
        if (grab != null) grab.enabled = false;

        // optional: parent it to the station so it stays
        trial.transform.SetParent(transform, true);
    }
}