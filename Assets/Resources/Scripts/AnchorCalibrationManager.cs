using System;
using UnityEngine;

public class AnchorCalibrationManager : MonoBehaviour
{
    [Header("Calibration cubes (the ones you grab)")]
    public Transform plantCalibCube;
    public Transform waterCalibCube;
    public Transform sunCalibCube;

    [Header("Targets that define the world layout")]
    public Transform plantAnchorTarget; // PlantAnchorTarget
    public Transform waterAnchorTarget; // WaterAnchorTarget
    public Transform sunAnchorTarget;   // SunAnchorTarget

    [Header("Calibration UI root (contains Done/Reset buttons)")]
    public GameObject calibrationUIRoot; // Calibration_UI (GameObject)

    [Header("Head UI distance (same as main menu)")]
    public float uiDistance = 0.5f;

    [Header("Optional: force horizontal rotation only (yaw)")]
    public bool yawOnly = true;

    // auto-found (no Inspector dragging)
    UIAnchorController calibrationUIAnchor;

    // store editor defaults so Reset() can restore them
    Vector3 plantCubePos0, waterCubePos0, sunCubePos0;
    Quaternion plantCubeRot0, waterCubeRot0, sunCubeRot0;

    Vector3 plantTargetPos0, waterTargetPos0, sunTargetPos0;
    Quaternion plantTargetRot0, waterTargetRot0, sunTargetRot0;

    public event Action CalibrationFinished;

    void Awake()
    {
        CacheDefaults();

        if (calibrationUIRoot != null)
        {
            calibrationUIAnchor = calibrationUIRoot.GetComponent<UIAnchorController>();
            calibrationUIRoot.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[AnchorCalibration] calibrationUIRoot is not assigned.");
        }

        // Calibration cubes should be visible at scene start
        SetCalibrationCubesVisible(true);
    }

    void CacheDefaults()
    {
        if (plantCalibCube != null) { plantCubePos0 = plantCalibCube.position; plantCubeRot0 = plantCalibCube.rotation; }
        if (waterCalibCube != null) { waterCubePos0 = waterCalibCube.position; waterCubeRot0 = waterCalibCube.rotation; }
        if (sunCalibCube != null)   { sunCubePos0   = sunCalibCube.position;   sunCubeRot0   = sunCalibCube.rotation; }

        if (plantAnchorTarget != null) { plantTargetPos0 = plantAnchorTarget.position; plantTargetRot0 = plantAnchorTarget.rotation; }
        if (waterAnchorTarget != null) { waterTargetPos0 = waterAnchorTarget.position; waterTargetRot0 = waterAnchorTarget.rotation; }
        if (sunAnchorTarget != null)   { sunTargetPos0   = sunAnchorTarget.position;   sunTargetRot0   = sunAnchorTarget.rotation; }
    }

    // ─────────────────────────────────────────────────────────────
    // ENTRY POINT (called from SessionBootstrap / scene start)
    // ─────────────────────────────────────────────────────────────

    public void StartCalibration()
    {
        // ensure cubes are visible when calibration starts
        SetCalibrationCubesVisible(true);

        if (calibrationUIRoot != null)
            calibrationUIRoot.SetActive(true);

        if (calibrationUIAnchor != null)
        {
            calibrationUIAnchor.headAnchored = true;
            calibrationUIAnchor.Mount();

            if (calibrationUIAnchor.uiRoot != null)
                calibrationUIAnchor.uiRoot.localPosition = new Vector3(0f, 0f, uiDistance);
        }

        ResetCalibration();
    }

    // ─────────────────────────────────────────────────────────────
    // UI BUTTONS
    // ─────────────────────────────────────────────────────────────

    // Hook to RESET button
    public void ResetCalibration()
    {
        if (plantCalibCube != null) plantCalibCube.SetPositionAndRotation(plantCubePos0, plantCubeRot0);
        if (waterCalibCube != null) waterCalibCube.SetPositionAndRotation(waterCubePos0, waterCubeRot0);
        if (sunCalibCube != null)   sunCalibCube.SetPositionAndRotation(sunCubePos0,   sunCubeRot0);

        if (plantAnchorTarget != null) plantAnchorTarget.SetPositionAndRotation(plantTargetPos0, plantTargetRot0);
        if (waterAnchorTarget != null) waterAnchorTarget.SetPositionAndRotation(waterTargetPos0, waterTargetRot0);
        if (sunAnchorTarget != null)   sunAnchorTarget.SetPositionAndRotation(sunTargetPos0,   sunTargetRot0);
    }

    // Hook to DONE button
    public void DoneCalibration()
    {
        ApplyCubeToTarget(plantCalibCube, plantAnchorTarget);
        ApplyCubeToTarget(waterCalibCube, waterAnchorTarget);
        ApplyCubeToTarget(sunCalibCube,   sunAnchorTarget);

        if (calibrationUIRoot != null)
            calibrationUIRoot.SetActive(false);

        // 🔒 hide calibration cubes completely
        SetCalibrationCubesVisible(false);

        CalibrationFinished?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────
    // INTERNALS
    // ─────────────────────────────────────────────────────────────

    void ApplyCubeToTarget(Transform cube, Transform target)
    {
        if (cube == null || target == null) return;

        Vector3 pos = cube.position;
        Quaternion rot = cube.rotation;

        // keep upright (yaw only)
        if (yawOnly)
        {
            float yaw = rot.eulerAngles.y;
            rot = Quaternion.Euler(0f, yaw, 0f);
        }

        target.SetPositionAndRotation(pos, rot);
    }

    void SetCalibrationCubesVisible(bool visible)
    {
        if (plantCalibCube != null) plantCalibCube.gameObject.SetActive(visible);
        if (waterCalibCube != null) waterCalibCube.gameObject.SetActive(visible);
        if (sunCalibCube != null)   sunCalibCube.gameObject.SetActive(visible);
    }
}