using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class SceneFlowManager : MonoBehaviour
{
    public enum Phase { Onboarding, OA, HA }

    [Header("Calibration")]
    public AnchorCalibrationManager calibrationManager; // assign in Inspector

    [Header("Stations (assign in Inspector)")]
    public GameObject sunStation;
    public GameObject waterStation;

    [Header("Onboarding")]
    public CubeTrial onboardingCube;
    public Transform onboardingWorldUIAnchor;
    public Transform onboardingCubeUIAnchor;

    [Header("Cubes per phase")]
    public CubeTrial[] oaCubes;
    public CubeTrial[] haCubes;

    [Header("Progress (OA/HA only)")]
    public int totalPlants = 3;

    [Header("UI Fade")]
    public float uiFadeDuration = 0.25f;

    private int snappedCorrectCount = 0;
    private Phase currentPhase = Phase.Onboarding;

    private CubeTrial active;
    private CubeTrial lastCompletedTrial;

    Coroutine activeUIFadeRoutine;

    void Start()
    {
        DisableAllTrialContent();

        if (calibrationManager != null)
            calibrationManager.CalibrationFinished += BeginAfterCalibration;
        else
            Debug.LogWarning("[SceneFlow] No calibrationManager assigned. Calibration won't gate onboarding.");
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

        if (onboardingCube != null)
            onboardingCube.gameObject.SetActive(phase == Phase.Onboarding);

        ToggleCubeSet(oaCubes, phase == Phase.OA);
        ToggleCubeSet(haCubes, phase == Phase.HA);

        if (sunStation != null) sunStation.SetActive(false);
        if (waterStation != null) waterStation.SetActive(false);

        if (phase == Phase.Onboarding)
            MountOnboardingUIToWorld();
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

    // ---------------- fade-out FIRST, then change phase ----------------

    public void OnOnboardingDone()
    {
        // Fade out current UI, then switch phase
        FadeOutUIThen(() =>
        {
            SetPhase(Phase.OA);
        });
    }

    public void OnOADoneButton()
    {
        // Fade out current UI, then do the transition
        FadeOutUIThen(() =>
        {
            if (lastCompletedTrial != null)
                StartCoroutine(FadeOutAndDisableCube(lastCompletedTrial));

            ToggleCubeSet(oaCubes, false);
            SetPhase(Phase.HA);
        });
    }

    // called by StationCheck when a cube snaps correctly
    public void OnPlantSnappedCorrectly()
    {
        if (currentPhase == Phase.Onboarding && active == onboardingCube)
            SwitchOnboardingUIToCube();

        snappedCorrectCount++;
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
        if (trial.uiRoot != null) trial.uiRoot.SetActive(false);
    }

    void OnCorrectAnswered()
    {
        if (active == null) return;

        active.uiPage4?.SetActive(false);
        lastCompletedTrial = active;

        if (currentPhase == Phase.Onboarding)
        {
            active.uiDone?.SetActive(true);
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
        if (active != null || trial == null) return;

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

        FadeInUIRoot();

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
        // Fade out UI first, then do the cube fade + re-enable colliders
        FadeOutUIThen(() =>
        {
            if (lastCompletedTrial != null)
                StartCoroutine(FadeOutAndDisableCube(lastCompletedTrial));

            foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
            {
                var col = cube.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }

            lastCompletedTrial = null;
        });
    }

    public void CloseUI()
    {
        // Fade out UI and then disable it (instead of immediate SetActive(false))
        FadeOutUIThen(() =>
        {
            foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
            {
                var col = cube.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }

            active = null;
        });
    }

    public CubeTrial GetActiveTrial() => active;

    // -------------------- UI FADE HELPERS --------------------

    void FadeInUIRoot()
    {
        if (active == null || active.uiRoot == null) return;

        active.uiRoot.SetActive(true);

        if (activeUIFadeRoutine != null) StopCoroutine(activeUIFadeRoutine);
        activeUIFadeRoutine = StartCoroutine(FadeCanvasGroup(active.uiRoot, 0f, 1f, uiFadeDuration));
    }

    void FadeOutUIThen(Action after)
    {
        // figure out which UI root is currently visible:
        GameObject root = null;
        if (active != null && active.uiRoot != null) root = active.uiRoot;
        else if (lastCompletedTrial != null && lastCompletedTrial.uiRoot != null) root = lastCompletedTrial.uiRoot;

        if (root == null)
        {
            after?.Invoke();
            return;
        }

        if (activeUIFadeRoutine != null) StopCoroutine(activeUIFadeRoutine);
        activeUIFadeRoutine = StartCoroutine(FadeOutAndThen(root, after));
    }

    IEnumerator FadeOutAndThen(GameObject uiRoot, Action after)
    {
        yield return FadeCanvasGroup(uiRoot, 1f, 0f, uiFadeDuration);
        uiRoot.SetActive(false);
        after?.Invoke();
    }

    IEnumerator FadeCanvasGroup(GameObject uiRoot, float from, float to, float duration)
    {
        var cg = uiRoot.GetComponent<CanvasGroup>();
        if (cg == null) cg = uiRoot.AddComponent<CanvasGroup>();

        cg.alpha = from;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        if (duration <= 0f)
        {
            cg.alpha = to;
        }
        else
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            cg.alpha = to;
        }

        bool visible = cg.alpha >= 0.99f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }
}