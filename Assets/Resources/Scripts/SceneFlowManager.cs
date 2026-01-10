using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneFlowManager : MonoBehaviour
{
    public enum Phase { Onboarding, OA, HA }

    [Header("Calibration")]
    public AnchorCalibrationManager calibrationManager; // drag in Inspector

    [Header("Stations (assign in Inspector)")]
    public GameObject sunStation;
    public GameObject waterStation;

    [Header("Onboarding")]
    public CubeTrial onboardingCube;                 // Onboarding_cube (CubeTrial)
    public Transform onboardingWorldUIAnchor;        // PlantAnchorTarget/World_onboarding_UI_anchor
    public Transform onboardingCubeUIAnchor;         // Onboarding_cube/Cube_onboarding_UI_anchor

    [Header("Cubes per phase")]
    public CubeTrial[] oaCubes;
    public CubeTrial[] haCubes;

    [Header("Progress (OA/HA only)")]
    public int totalPlants = 3;

    private int snappedCorrectCount = 0;
    private Phase currentPhase = Phase.Onboarding;

    private CubeTrial active;
    private CubeTrial lastCompletedTrial;

    void Start()
    {
        // Start locked behind calibration
        DisableAllTrialContent();

        if (calibrationManager != null)
        {
            calibrationManager.CalibrationFinished += BeginAfterCalibration;
        }
        else
        {
            // If you forgot to assign it, don't block the whole app
            Debug.LogWarning("[SceneFlow] No calibrationManager assigned → starting onboarding anyway.");
            BeginAfterCalibration();
        }
    }

    void OnDestroy()
    {
        if (calibrationManager != null)
            calibrationManager.CalibrationFinished -= BeginAfterCalibration;
    }

    void DisableAllTrialContent()
    {
        if (onboardingCube != null) onboardingCube.gameObject.SetActive(false);
        ToggleCubeSet(oaCubes, false);
        ToggleCubeSet(haCubes, false);

        if (sunStation != null) sunStation.SetActive(false);
        if (waterStation != null) waterStation.SetActive(false);
    }

    void BeginAfterCalibration()
    {
        SetPhase(Phase.Onboarding);
    }

    void SetPhase(Phase phase)
    {
        currentPhase = phase;
        snappedCorrectCount = 0;
        active = null;
        lastCompletedTrial = null;

        // Turn sets on/off
        if (onboardingCube != null)
            onboardingCube.gameObject.SetActive(phase == Phase.Onboarding);

        ToggleCubeSet(oaCubes, phase == Phase.OA);
        ToggleCubeSet(haCubes, phase == Phase.HA);

        // Hide stations at phase start
        if (sunStation != null) sunStation.SetActive(false);
        if (waterStation != null) waterStation.SetActive(false);

        // Onboarding: mount UI to WORLD anchor immediately (so it isn't stuck at 0,0,0)
        if (phase == Phase.Onboarding)
            MountOnboardingUIToWorld();

        Debug.Log($"[SceneFlow] Switched phase → {currentPhase}");
    }

    void ToggleCubeSet(CubeTrial[] set, bool on)
    {
        if (set == null) return;

        foreach (var c in set)
        {
            if (c == null) continue;

            c.gameObject.SetActive(on);

            var col = c.GetComponent<Collider>();
            if (col != null) col.enabled = on;

            if (!on && c.uiRoot != null)
                c.uiRoot.SetActive(false);
        }
    }

    // Hook this to onboarding "DONE" button
    public void OnOnboardingDone()
    {
        Debug.Log("[SceneFlow] Onboarding finished → switching to OA.");
        SetPhase(Phase.OA);
    }

    // Hook this to the DONE button of the last OA cube
    public void OnOADoneButton()
    {
        Debug.Log("[SceneFlow] OA phase finished → switching to HA.");

        if (lastCompletedTrial != null)
            StartCoroutine(FadeOutAndDisableCube(lastCompletedTrial));

        ToggleCubeSet(oaCubes, false);
        SetPhase(Phase.HA);
    }

    // called by StationCheck when a cube snaps correctly
    public void OnPlantSnappedCorrectly()
    {
        // Onboarding: when it snaps, re-parent UI from world anchor to cube anchor
        if (currentPhase == Phase.Onboarding && active == onboardingCube)
        {
            SwitchOnboardingUIToCube();
        }

        snappedCorrectCount++;
        Debug.Log($"[SceneFlow] {currentPhase} snap count = {snappedCorrectCount}/{totalPlants}");
    }

    public void AnswerWrong(Button button)
    {
        if (active == null || button == null) return;
        StartCoroutine(FlashButton(button, false));
    }

    public void AnswerCorrect(Button button)
    {
        if (active == null || button == null) return;
        StartCoroutine(FlashButton(button, true));
    }

    IEnumerator FlashButton(Button button, bool correct)
    {
        Image img = button.GetComponent<Image>();
        if (img == null) yield break;

        Color original = img.color;
        img.color = correct ? Color.green : Color.red;

        yield return new WaitForSeconds(correct ? 1.0f : 0.25f);

        img.color = original;

        if (correct) OnCorrectAnswered();
    }

    IEnumerator FadeOutAndDisableCube(CubeTrial trial)
    {
        if (trial == null) yield break;

        Renderer[] renderers = trial.GetComponentsInChildren<Renderer>();
        float duration = 2f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration);

            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = alpha;
                        mat.color = c;
                    }
                }
            }

            yield return null;
        }

        trial.gameObject.SetActive(false);

        if (trial.uiRoot != null)
            trial.uiRoot.SetActive(false);
    }

    void OnCorrectAnswered()
    {
        if (active == null) return;

        active.uiPage4?.SetActive(false);
        lastCompletedTrial = active;

        if (currentPhase == Phase.Onboarding)
        {
            active.uiDone?.SetActive(true); // no UI5 in onboarding
        }
        else
        {
            if (snappedCorrectCount < totalPlants)
                active.uiPage5?.SetActive(true);
            else
                active.uiDone?.SetActive(true);
        }

        active = null;
    }

    public void StartTrial(CubeTrial trial)
    {
        if (active != null) return;
        if (trial == null) return;

        active = trial;

        foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            if (cube == active.gameObject) continue;

            var col = cube.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        if (active.anchor != null)
        {
            if (currentPhase == Phase.Onboarding)
                MountOnboardingUIToWorld();
            else
                active.anchor.Mount();
        }

        ShowPage1();
    }

    void MountOnboardingUIToWorld()
    {
        if (onboardingCube == null || onboardingCube.anchor == null) return;
        if (onboardingWorldUIAnchor == null) return;

        onboardingCube.anchor.headAnchored = false;
        onboardingCube.anchor.objectAnchor = onboardingWorldUIAnchor;
        onboardingCube.anchor.Mount();
    }

    void SwitchOnboardingUIToCube()
    {
        if (onboardingCube == null || onboardingCube.anchor == null) return;
        if (onboardingCubeUIAnchor == null) return;

        onboardingCube.anchor.headAnchored = false;
        onboardingCube.anchor.objectAnchor = onboardingCubeUIAnchor;
        onboardingCube.anchor.Mount();
    }

    public void ShowPage1()
    {
        if (active == null) return;

        active.uiRoot.SetActive(true);
        active.uiPage1.SetActive(true);
        active.uiPage2.SetActive(false);
        active.uiPage3.SetActive(false);
        active.uiPage4.SetActive(false);

        if (active.symbol != null) active.symbol.SetActive(false);

        if (sunStation != null) sunStation.SetActive(false);
        if (waterStation != null) waterStation.SetActive(false);
    }

    public void ShowPage2()
    {
        if (active == null) return;

        active.uiPage1.SetActive(false);
        active.uiPage2.SetActive(true);

        if (active.symbol != null) active.symbol.SetActive(true);
    }

    public void ShowPage3()
    {
        if (active == null) return;

        active.uiPage1.SetActive(false);
        active.uiPage2.SetActive(false);
        if (active.uiPage3 != null) active.uiPage3.SetActive(true);

        if (sunStation != null) sunStation.SetActive(true);
        if (waterStation != null) waterStation.SetActive(true);
    }

    public void ShowPage4()
    {
        if (active == null) return;

        active.uiPage1.SetActive(false);
        active.uiPage2.SetActive(false);
        if (active.uiPage3 != null) active.uiPage3.SetActive(false);
        if (active.uiPage4 != null) active.uiPage4.SetActive(true);
    }

    public void CloseUI5()
    {
        if (lastCompletedTrial == null) return;

        StartCoroutine(FadeOutAndDisableCube(lastCompletedTrial));

        foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            var col = cube.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        lastCompletedTrial = null;
    }

    public void CloseUI()
    {
        if (active == null) return;

        active.uiRoot.SetActive(false);

        foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            var col = cube.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        active = null;
    }

    public CubeTrial GetActiveTrial() => active;
}