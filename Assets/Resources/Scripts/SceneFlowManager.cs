using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneFlowManager : MonoBehaviour
{
    public enum Phase { Onboarding, OA, HA }

    [Header("Stations (assign in Inspector)")]
    public GameObject sunStation;
    public GameObject waterStation;

    [Header("Onboarding")]
    public CubeTrial onboardingCube;                 // Onboarding_cube (CubeTrial)
    public Transform onboardingWorldUIAnchor;        // PlantAnchorTarget/World_onboarding_UI_anchor
    public Transform onboardingCubeUIAnchor;         // Onboarding_cube/Cube_onboarding_UI_anchor

    [Header("Cubes per phase")]
    public CubeTrial[] oaCubes;   // Cube_blue_OA, Cube_green_OA, Cube_grey_OA
    public CubeTrial[] haCubes;   // Cube_blue_HA, Cube_green_HA, Cube_grey_HA

    [Header("Progress (OA/HA only)")]
    public int totalPlants = 3;

    private int snappedCorrectCount = 0;
    private Phase currentPhase = Phase.Onboarding;

    private CubeTrial active;
    private CubeTrial lastCompletedTrial;

    void Start()
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

            // also ensure UI is hidden when disabling
            if (!on && c.uiRoot != null)
                c.uiRoot.SetActive(false);
        }
    }

    // --- Button hooks ---

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

    // --- Core events ---

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

    // called by UI buttons
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
            // Onboarding: always go straight to DONE (no UI5)
            active.uiDone?.SetActive(true);
        }
        else
        {
            // OA/HA: UI5 until last plant, then DONE
            if (snappedCorrectCount < totalPlants)
                active.uiPage5?.SetActive(true);
            else
                active.uiDone?.SetActive(true);
        }

        active = null;
    }

    // --- Trial flow ---

    public void StartTrial(CubeTrial trial)
    {
        if (active != null) return;
        if (trial == null) return;

        active = trial;

        // disable other cubes (keep visible, just not grabbable)
        foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            if (cube == active.gameObject) continue;

            var col = cube.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        // Mount UI
        if (active.anchor != null)
        {
            if (currentPhase == Phase.Onboarding)
            {
                // ensure it's mounted to WORLD anchor at the start of onboarding interaction
                MountOnboardingUIToWorld();
            }
            else
            {
                active.anchor.Mount(); // OA/HA behavior unchanged
            }
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

    // --- UI pages ---

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

    public CubeTrial GetActiveTrial()
    {
        return active;
    }
}