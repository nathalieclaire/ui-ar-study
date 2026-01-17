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

    // track current page so we can crossfade correctly even when active becomes null
    GameObject currentPage;

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
        currentPage = null;

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
        FadeOutUIThen(() => { SetPhase(Phase.OA); });
    }

    public void OnOADoneButton()
    {
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

        lastCompletedTrial = active;

        // crossfade FROM whatever is currently shown TO the next page
        if (currentPhase == Phase.Onboarding)
        {
            CrossFadeTo(active.uiDone);
        }
        else
        {
            if (snappedCorrectCount < totalPlants)
                CrossFadeTo(active.uiPage5);
            else
                CrossFadeTo(active.uiDone);
        }

        active = null;
    }

    public void StartTrial(CubeTrial trial)
    {
        if (active != null || trial == null) return;

        active = trial;
        currentPage = null;

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

    // -------------------- UI PAGES (CROSS-FADE) --------------------

    public void ShowPage1()
    {
        if (active == null) return;

        FadeInUIRoot();

        SetPageActive(active.uiPage1, true);
        SetPageActive(active.uiPage2, false);
        SetPageActive(active.uiPage3, false);
        SetPageActive(active.uiPage4, false);
        SetPageActive(active.uiPage5, false);
        SetPageActive(active.uiDone, false);

        // page1 is the first visible page
        SetCanvasGroupInstant(active.uiPage1, 1f, true);
        currentPage = active.uiPage1;

        if (active.symbol != null) active.symbol.SetActive(false);

        if (sunStation != null) sunStation.SetActive(false);
        if (waterStation != null) waterStation.SetActive(false);
    }

    public void ShowPage2()
    {
        if (active == null) return;

        if (active.symbol != null) active.symbol.SetActive(true);

        CrossFadeTo(active.uiPage2);
    }

    public void ShowPage3()
    {
        if (active == null) return;

        if (sunStation != null) sunStation.SetActive(true);
        if (waterStation != null) waterStation.SetActive(true);

        CrossFadeTo(active.uiPage3);
    }

    public void ShowPage4()
    {
        if (active == null) return;

        CrossFadeTo(active.uiPage4);
    }

    public void CloseUI5()
    {
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
            currentPage = null;
        });
    }

    public void CloseUI()
    {
        FadeOutUIThen(() =>
        {
            foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
            {
                var col = cube.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }

            active = null;
            currentPage = null;
        });
    }

    public CubeTrial GetActiveTrial() => active;

    // -------------------- UI ROOT FADE HELPERS --------------------

    void FadeInUIRoot()
    {
        if (active == null || active.uiRoot == null) return;

        active.uiRoot.SetActive(true);

        if (activeUIFadeRoutine != null) StopCoroutine(activeUIFadeRoutine);
        activeUIFadeRoutine = StartCoroutine(FadeCanvasGroup(active.uiRoot, 0f, 1f, uiFadeDuration));
    }

    void FadeOutUIThen(Action after)
    {
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

    // -------------------- PAGE CROSSFADE --------------------

    void CrossFadeTo(GameObject nextPage)
    {
        if (nextPage == null) return;

        // If we don't have a current page yet, just show it.
        if (currentPage == null)
        {
            nextPage.SetActive(true);
            SetCanvasGroupInstant(nextPage, 1f, true);
            currentPage = nextPage;
            return;
        }

        if (currentPage == nextPage) return;

        CrossFadePages(currentPage, nextPage);
        currentPage = nextPage;
    }

    void CrossFadePages(GameObject fromPage, GameObject toPage)
    {
        if (fromPage == null || toPage == null) return;

        var from = fromPage.GetComponent<CanvasGroup>();
        var to = toPage.GetComponent<CanvasGroup>();

        if (from == null || to == null)
        {
            fromPage.SetActive(false);
            toPage.SetActive(true);
            return;
        }

        fromPage.SetActive(true);
        toPage.SetActive(true);

        to.alpha = 0f;
        to.interactable = false;
        to.blocksRaycasts = false;

        from.alpha = 1f;
        from.interactable = false;
        from.blocksRaycasts = false;

        StartCoroutine(CrossFade(from, to, uiFadeDuration));
    }

    IEnumerator CrossFade(CanvasGroup from, CanvasGroup to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(duration <= 0f ? 1f : t / duration);

            if (from != null) from.alpha = 1f - k;
            if (to != null) to.alpha = k;

            yield return null;
        }

        if (from != null)
        {
            from.alpha = 0f;
            from.interactable = false;
            from.blocksRaycasts = false;
            from.gameObject.SetActive(false);
        }

        if (to != null)
        {
            to.alpha = 1f;
            to.interactable = true;
            to.blocksRaycasts = true;
        }
    }

    void SetPageActive(GameObject page, bool on)
    {
        if (page != null) page.SetActive(on);
    }

    void SetCanvasGroupInstant(GameObject page, float alpha, bool interactable)
    {
        if (page == null) return;

        var cg = page.GetComponent<CanvasGroup>();
        if (cg == null) return;

        cg.alpha = alpha;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }
}