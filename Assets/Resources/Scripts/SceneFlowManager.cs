using UnityEngine;

public class SceneFlowManager : MonoBehaviour
{
    private CubeTrial active;
    public void StartTrial(CubeTrial trial)
    {
        if (active != null) return; // don't start another trial while another one is still active
        if (trial == null) return;    // safety

        active = trial; // saves what cube is active rn so the manager controls the UI of the corresponding cube
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
    }

    public void ShowPage2()
    {
        if (active == null) return; // prevents NullReference crashes if a button fires when no trial is active.

        active.uiPage1.SetActive(false);
        active.uiPage2.SetActive(true);
    }

    public void CloseUI()
    {
        if (active == null) return; // prevents NullReference crashes if a button fires when no trial is active.

        active.uiRoot.SetActive(false);
        active = null; // sets cube inactive so no cube is active atm
    }
}