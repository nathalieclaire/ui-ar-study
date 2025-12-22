using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneFlowManager : MonoBehaviour
{
    [Header("Stations (assign in Inspector)")]
    public GameObject sunStation;
    public GameObject waterStation;

    [Header("Progress")]
    public int totalPlants = 3;
    private int snappedCorrectCount = 0;

    private CubeTrial active;

    private CubeTrial lastCompletedTrial;

    // ---------- NEW: called by StationCheck when a cube snaps correctly ----------
    public void OnPlantSnappedCorrectly()
    {
        snappedCorrectCount++;
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
    }

    //!! after correct answer on UI4: go to UI5 or UI6 (uiDone)
    void OnCorrectAnswered() //!!
    {
        if (active == null) return;

        active.uiPage4?.SetActive(false); //ide UI4

        lastCompletedTrial = active; // remember cube for UI5 / UI6

        if (snappedCorrectCount < totalPlants)
            active.uiPage5?.SetActive(true);  //how UI5
        else
            active.uiDone?.SetActive(true);   //show UI6 (done)
        active = null;
    }
    // ----------------------------------------------------------------------

    public void StartTrial(CubeTrial trial)
    {
        if (active != null) return; // don't start another trial while another one is still active
        if (trial == null) return;    // safety

        active = trial; // saves what cube is active rn so the manager controls the UI of the corresponding cube

        // disable other cubes (keep visible, just not grabbable)
        foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            if (cube == active.gameObject) continue;

            var col = cube.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        active.anchor.Mount();   // determines OA or HA (function from scipt UIAnchorController)
        ShowPage1(); // shows UI page 1 of active cube
    }
    // note - in Unity editor: Set uiRoot (UI_parent) inactive by default in the scene/prefab, so it doesn’t show before selection.
    public void ShowPage1()
    {
        if (active == null) return; // prevents NullReference crashes if a button fires when no trial is active.

        active.uiRoot.SetActive(true);
        active.uiPage1.SetActive(true);
        active.uiPage2.SetActive(false);
        active.uiPage3.SetActive(false);
        active.uiPage4.SetActive(false);

        if (active.symbol != null) active.symbol.SetActive(false); // hide symbol

        if (sunStation != null) sunStation.SetActive(false);
        if (waterStation != null) waterStation.SetActive(false);

    }

    public void ShowPage2()
    {
        if (active == null) return; // prevents NullReference crashes if a button fires when no trial is active.

        active.uiPage1.SetActive(false);
        active.uiPage2.SetActive(true);

        if (active.symbol != null) active.symbol.SetActive(true); // show symbol
    }

    public void ShowPage3()
    {
        if (active == null) return;

        active.uiPage1.SetActive(false);
        active.uiPage2.SetActive(false);
        if (active.uiPage3 != null) active.uiPage3.SetActive(true);

        // if (active.symbol != null) active.symbol.SetActive(false); // optional: hide symbol after success

        // stations appear now
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

        // re-enable other cubes
        foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            var col = cube.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        lastCompletedTrial = null;
    }

    public void CloseUI()
    {
        if (active == null) return; // prevents NullReference crashes if a button fires when no trial is active.

        active.uiRoot.SetActive(false);

        // re-enable all cubes
        foreach (var cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            var col = cube.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        active = null; // sets cube inactive so no cube is active atm
    }

    // stations use this to check which cube is currently active
    public CubeTrial GetActiveTrial()
    {
        return active;
    }
}