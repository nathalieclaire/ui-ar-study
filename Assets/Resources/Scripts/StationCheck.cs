using UnityEngine;

public class StationCheck : MonoBehaviour
{
    public string myStationTag; // set in inspector: "SunStation" or "WaterStation"
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

        var trial = other.GetComponentInParent<CubeTrial>(); // cube carried in
        if (trial == null) return;

        // only react to the currently active cube
        if (flow.GetActiveTrial() != trial) return;

        bool correct = (trial.requiredStationTag == myStationTag);

        if (stationRenderer != null)
            stationRenderer.material.color = correct ? okColor : wrongColor;

        // later: if correct, tell flow "station success" and move to next step
        // if (correct) flow.OnPlacedAtCorrectStation();
    }
}