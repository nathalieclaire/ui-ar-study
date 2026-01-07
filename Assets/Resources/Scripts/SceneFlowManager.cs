using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneFlowManager : MonoBehaviour
{
    public enum Phase { OA, HA }

    [Header("Stations (assign in Inspector)")]
    public GameObject sunStation;
    public GameObject waterStation;

    [Header("Cubes per phase")]
    public CubeTrial[] oaCubes;   // Cube_blue_OA, Cube_green_OA, Cube_grey_OA
    public CubeTrial[] haCubes;   // Cube_blue_HA, Cube_green_HA, Cube_grey_HA

    [Header("Progress")]
    public int totalPlants = 3;

    private int snappedCorrectCount = 0;
    private Phase currentPhase = Phase.OA;

    private CubeTrial active;
    private CubeTrial lastCompletedTrial;

    // ---------------- PHASE CONTROL ----------------

    void Start()
    {
        SetPhase(Phase.OA);   // start in OA condition
    }

    void SetPhase(Phase phase)
    {
        currentPhase = phase;
        snappedCorrectCount = 0;
        active = null;
        lastCompletedTrial = null;

        bool oaOn = (phase == Phase.OA);

        // toggle whole sets
        ToggleCubeSet(oaCubes, oaOn);
        ToggleCubeSet(haCubes, !oaOn);

        Debug.Log($"[SceneFlow] Switched phase → {currentPhase}, OA active = {oaOn}");
    }

    void ToggleCubeSet(CubeTrial[] set, bool on)
    {
        if (set == null) return;

        foreach (var c in set)
        {
            if (c == null) continue;
            c.gameObject.SetActive(on);

            // simple safety: re-enable collider when we turn it on
            var col = c.GetComponent<Collider>();
            if (col != null) col.enabled = on;
        }
    }

    // called by DONE button on the *last OA cube* (its uiDone)
    public void OnOADoneButton()
    {
        Debug.Log("[SceneFlow] OA phase finished → switching to HA.");

        // fade out last OA cube if present
        if (lastCompletedTrial != null)
            StartCoroutine(FadeOutAndDisableCube(lastCompletedTrial));

        // make sure all OA cubes are off
        ToggleCubeSet(oaCubes, false);

        // now enable HA cubes and restart counting
        SetPhase(Phase.HA);
    }

    // ---------------- STATION & UI LOGIC ----------------

    // called by StationCheck when a cube snaps correctly
    public void OnPlantSnappedCorrectly()
    {
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

        // make sure its UI disappears too
        if (trial.uiRoot != null)
            trial.uiRoot.SetActive(false);
    }

    // after correct answer on UI4: go to UI5 or UI6 (uiDone)
    void OnCorrectAnswered()
    {
        if (active == null) return;

        active.uiPage4?.SetActive(false);

        lastCompletedTrial = active;   // remember cube for UI5 / UI6, and for fade-out
        Debug.Log($"[SceneFlow] OnCorrectAnswered in phase {currentPhase}, snapCount={snappedCorrectCount}");

        if (snappedCorrectCount < totalPlants)
        {
            // not the last plant in this phase → show UI5
            active.uiPage5?.SetActive(true);
        }
        else
        {
            // last plant in this phase → show DONE page
            active.uiDone?.SetActive(true);
        }

        active = null;
    }

    // ----------------------------------------------------
    // TRIAL FLOW
    // ----------------------------------------------------

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

        active.anchor.Mount();   // decides OA vs HA (UIAnchorController)
        ShowPage1();
    }

    // note - in Unity editor: Set uiRoot inactive by default.
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

        // re-enable other cubes in this phase
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