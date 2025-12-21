using UnityEngine;

public class SceneFlowManager : MonoBehaviour
{
    private CubeTrial active;
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

        Debug.Log(Camera.main);
    }
    // note - in Unity editor: Set uiRoot (UI_parent) inactive by default in the scene/prefab, so it doesn’t show before selection.
    public void ShowPage1()
    {
        if (active == null) return; // prevents NullReference crashes if a button fires when no trial is active.

        active.uiRoot.SetActive(true);
        active.uiPage1.SetActive(true);
        active.uiPage2.SetActive(false);
        active.uiPage3.SetActive(false);

        if (active.symbol != null) active.symbol.SetActive(false); // hide symbol

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
}