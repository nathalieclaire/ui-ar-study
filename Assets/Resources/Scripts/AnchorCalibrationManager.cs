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
    public GameObject calibrationUIRoot;

    [Header("Optional: force horizontal rotation only (yaw)")]
    public bool yawOnly = true;

    // store editor defaults so Reset() can restore them
    Vector3 plantCubePos0, waterCubePos0, sunCubePos0;
    Quaternion plantCubeRot0, waterCubeRot0, sunCubeRot0;

    Vector3 plantTargetPos0, waterTargetPos0, sunTargetPos0;
    Quaternion plantTargetRot0, waterTargetRot0, sunTargetRot0;

    public event Action CalibrationFinished;

    void Awake()
    {
        CacheDefaults();
    }

    void Start()
    {
        // Always start in calibration mode when the scene loads
        EnterCalibrationMode();
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

    void EnterCalibrationMode()
    {
        if (calibrationUIRoot != null)
            calibrationUIRoot.SetActive(true);

        // IMPORTANT: we don't auto-apply anything on entry.
        // You manually place the cubes, then press Done.
    }

    // Hook this to the "Reset" button
    public void ResetCalibration()
    {
        // put cubes back to their start pose
        if (plantCalibCube != null) plantCalibCube.SetPositionAndRotation(plantCubePos0, plantCubeRot0);
        if (waterCalibCube != null) waterCalibCube.SetPositionAndRotation(waterCubePos0, waterCubeRot0);
        if (sunCalibCube != null)   sunCalibCube.SetPositionAndRotation(sunCubePos0,   sunCubeRot0);

        // also reset targets back to their editor defaults (optional but usually what you want)
        if (plantAnchorTarget != null) plantAnchorTarget.SetPositionAndRotation(plantTargetPos0, plantTargetRot0);
        if (waterAnchorTarget != null) waterAnchorTarget.SetPositionAndRotation(waterTargetPos0, waterTargetRot0);
        if (sunAnchorTarget != null)   sunAnchorTarget.SetPositionAndRotation(sunTargetPos0,   sunTargetRot0);
    }

    // Hook this to the "Done" button
    public void DoneCalibration()
    {
        ApplyCubeToTarget(plantCalibCube, plantAnchorTarget);
        ApplyCubeToTarget(waterCalibCube, waterAnchorTarget);
        ApplyCubeToTarget(sunCalibCube,   sunAnchorTarget);

        if (calibrationUIRoot != null)
            calibrationUIRoot.SetActive(false);

        CalibrationFinished?.Invoke();
    }

    void ApplyCubeToTarget(Transform cube, Transform target)
    {
        if (cube == null || target == null) return;

        Vector3 pos = cube.position;
        Quaternion rot = cube.rotation;

        if (yawOnly)
        {
            float yaw = rot.eulerAngles.y;
            rot = Quaternion.Euler(0f, yaw, 0f);
        }

        target.SetPositionAndRotation(pos, rot);
    }
}